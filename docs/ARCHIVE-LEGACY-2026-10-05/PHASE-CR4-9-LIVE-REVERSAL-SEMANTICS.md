# CR4.9 / D9 — Live reversal action and alert semantics

## Scope

Harden live reversal semantics without changing public parameters, default
trading thresholds, RR policy or execution policy.

## Finding reconciliation

The pre-existing live reversal path mixed four different meanings:

1. reversal evidence detected;
2. position intentionally retained because a protective close is disabled or
   not profitable;
3. close request accepted/submitted;
4. broker-confirmed position closure.

D9 separates these meanings while preserving the existing broker-confirmed
lifecycle/outcome authority.

## Implementation

- Added `LiveReversalDecisionRule` as the Core owner for opposite direction,
  directional reversal confidence and reversal action classification.
- Reaction confidence is accepted only when `_reaction.Direction` is the
  opposite of the live position.
- M5 frame quality contributes to reversal confidence only when the frame's
  direction is the opposite direction.
- Added `LiveReversalEpisodeState` so the `REVERSAL` detection alert is emitted
  once per qualifying position/direction episode instead of once per M5 bar.
- A confirmed position close resets the reversal episode state.
- A retained position is presented as `WAIT` with the explicit reason
  `REVERSAL DETECTED • POSITION RETAINED`; it is not presented as a global
  `BLOCKED` Auto Trading state.
- An accepted close submission is presented as `EXIT_REQUESTED`, not
  `EXECUTED`; actual closure remains owned by `PositionClosed`.
- A missing managed position during live reversal no longer clears `_plan` or
  synthesizes a closed outcome. It marks broker state dirty and enters a
  reconciliation/recovery boundary so the normal broker-state and
  `PositionClosed`/outcome path remains authoritative.
- Existing public reversal parameters keep their current names, types,
  defaults and ranges; their bounds are given explicit named Core ownership.

## Deterministic evidence

`VerifyLiveReversalD9()` covers:

- BUY/SELL opposite-direction mapping;
- same-direction evidence rejection;
- directional reaction confidence;
- directional frame-quality fallback;
- profitable versus retained reversal action;
- accepted-exit wait-for-confirmation semantics;
- missing-position reconciliation semantics;
- public live-reversal confidence and structural-score bounds.

## Safety boundary

- No public parameter name/type/DefaultValue changed.
- No default RR, confidence, SL, target or execution threshold was tuned.
- No second decision or broker-execution authority was introduced.
- No synthetic outcome is recorded by the reversal detector.
- Broker confirmation remains authoritative for final closure.

## Verification boundary

Repository CI must validate:

- accumulated Source/Architecture checks including `audit_phase_4_9.py`;
- Runtime Acceptance Contracts;
- cTrader Compile.

Target-terminal cTrader timing, broker close acknowledgement ordering,
restart/reconnect behavior, chart/panel responsiveness and empirical
signal-quality/profitability remain manual acceptance work.

## Performance and cleanliness audit

- Reversal qualification remains bounded and performs no new per-bar I/O.
- Alert episode state is O(1) and avoids unbounded per-bar reversal keys.
- The new Core rule is pure and allocation-free.
- Existing project-wide routine and optimization audits remain mandatory.

## Next phase

**CR4.10 / D10 — Native-indicator defensive safety and registry performance.**
