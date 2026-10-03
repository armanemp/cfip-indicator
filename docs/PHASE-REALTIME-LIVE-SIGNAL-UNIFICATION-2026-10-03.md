# CFIP — Realtime / Live Execution / Signal Truth Unification — 2026-10-03

Status: IMPLEMENTATION COMPLETE — PR #240 open; automated verification pending; target-terminal acceptance still required.

## Objective

Close the remaining execution-chain seams between realtime analysis, current-market actionability, future pending scenarios, cBot execution, simultaneous scenario capacity, live-account routing, chart/popup direction truth, attachment status and sound delivery.

The intended runtime contract is:

Historical context -> MTF analysis -> M15 canonical decision -> M5 trigger/tuning -> M1 optional confirmation -> current quote actionability -> current Market/Aggressive execution OR future Stop/Limit scenario -> ScenarioBatch -> cBot preflight -> account-mode gate -> scenario capacity -> broker mutation -> broker-confirmed facts -> protection/management -> outcome/history.

## Changes

### 1. Explicit Demo / Live account routing

The cBot no longer stops merely because the attached account is live.

Added independent, default-OFF live arms for:
- Market execution
- Pending Stop execution
- Pending Limit execution
- Aggressive execution
- Management/protection execution

The account mode is selected from the actual broker account. Demo controls cannot accidentally arm a live account, and live controls cannot arm a demo account.

The broker mutation owners remain the same existing cBot coordinators. They now receive the real account mode and publish explicit LIVE/DEMO broker comments.

Live remains fail-closed unless the live action is explicitly armed.

### 2. Multi-scenario / simultaneous position capacity

The Indicator's legacy Maximum Open Positions contract remains certified at one active analytical plan. Broker-side simultaneous ScenarioIds are intentionally owned by the cBot, independently of that legacy Indicator capacity.

The cBot computes its effective concurrent-scenario limit directly from Max Concurrent Scenarios, bounded by the cBot's own parameter contract.

The per-ScenarioId single-active-object rule remains intact, so multiple independent scenarios can coexist while duplicate objects for one scenario remain blocked.

### 3. Realtime handoff latency

cBot signal-store reload cadence was reduced from 500 ms to 100 ms.

The cBot processes the published ScenarioBatch on every tick and now also polls the same transport every 100 ms through its cTrader Timer. A revision/scenario-aware handoff guard prevents the timer from redundantly replaying an envelope that was already observed on a market tick. Broker reconciliation and management remain independently controlled.

M15 remains the canonical decision/execution timeframe; M5 remains the trigger/entry-tuning layer and M1 optional.

### 4. Small / stagnant-market opportunity quality

Added a volatility-relative OpportunityMagnitudeRule.

Current and future opportunities are rejected when the planned TP1 movement is too small for the active market regime. The gate is independent of position sizing, so it does not confuse a deliberately small risk amount with a weak setup.

Additional RANGE filtering raises the low-RR floor from the previous 2.00 implementation state to 2.25.

COMPRESSION remains no-trade in the existing range-quality rule and now also receives the magnitude guard on future candidates.

### 5. One canonical direction across arrow and popup

Reaction alerts now consult the same SignalVisualSnapshot authority used by chart rendering.

A reaction alert is suppressed when:
- the canonical current snapshot is already actionable, or
- the canonical authoritative direction conflicts with the reaction direction.

The canonical actionable-entry alert now uses the Decision direction only after verifying that the materialized plan has the same direction. Operator-visible popup direction therefore cannot intentionally diverge from the canonical arrow direction.

### 6. cBot account mode in panel state

CbotExecutionStateSnapshot now exposes ExecutionAccountMode.

The Indicator panel reads this field and presents LIVE or DEMO as part of the cBot status, while keeping the cBot's broker-confirmed lifecycle and protection state authoritative.

Presence wording distinguishes:
- live heartbeat,
- detected cBot presence with unresolved binding,
- actual NOT ATTACHED discovery failure.

## Multi-timeframe analysis / M15 tuning / lower-timeframe entry / all-timeframe risk-reward

The execution contract is explicitly multi-timeframe:

1. M1, M5, M15, M30, H1, H4, D1 and W1 are analyzed from one aligned closed-bar market context. M1 is deliberately a low-weight/optional confirmation input so it cannot dominate the decision.
2. The directional signal is produced from the combined timeframe evidence. M15 is the canonical tuning/reference layer used to validate the final directional setup; it is not the sole analytical source.
3. M5 provides the primary lower-timeframe entry geometry and live entry precision. M1 may refine the micro-entry and now also contributes bounded micro-structure/target context when enabled.
4. Stop selection evaluates lower and higher timeframe structural candidates. The current stop pipeline now accepts M1 micro structure plus M5, M15, M30, H1, H4, D1 and W1 structural candidates, then applies the existing risk envelope and reward-path checks.
5. Target construction already evaluates M5 plus M15, M30, H1, H4, D1 and W1 reward levels; M1 micro swing/FVG/OB targets are now added as bounded candidates. Minimum RR, obstacle checks and normalization remain mandatory.
6. The final Entry/SL/TP selection is therefore a constrained optimization problem over multi-timeframe evidence: reject invalid/wrong-side/high-risk candidates first, then prefer valid geometry with stronger structural support and attainable reward path. No timeframe may bypass the common safety gates.

This corrects the earlier overly narrow interpretation that M15 was the only analysis layer. M15 is the final tuning/reference layer inside a simultaneous multi-timeframe engine.

## New realtime intelligence/display hardening — 2026-10-03 follow-up

### 7. All-timeframe early prediction fusion
The early-prediction score now has a dedicated MTF fusion owner. M5/M15/M30/H1/H4/D1/W1
are fused with their configured weights and per-frame quality; M1 remains deliberately
excluded from forecast authority and stays a precision/confirmation layer.

The forecast path remains separate from the execution authority: the canonical M15 decision
and M5/M1 actionability gates still decide whether a current Market execution is allowed.

### 8. Nine-level smart HTF trend arrows
The chart now derives a presentation-only MTF trend-strength level from the active M1/M5/M15/M30/H1/H4/D1/W1
frames plus the live bid/ask midpoint. H1+ has explicit higher-timeframe authority.

Levels are grouped as:
- 1-3: weak color, 1/2/3 stacked arrows;
- 4-6: medium color, 1/2/3 stacked arrows;
- 7-9: strong color, 1/2/3 stacked arrows.

The stack is bounded to three visible arrows; level intensity is encoded by color tier plus
arrow count. It cannot authorize a trade by itself.

### 9. cBot binding hardening
Same-chart binding now preserves a previously known Indicator InstanceId when multiple CFIP
instances exist, instead of losing the known-good attachment. When binding still fails, the cBot
logs every custom chart indicator name/type/instance for deterministic diagnosis rather than only
reporting a generic NOT ATTACHED state.

### 10. Stagnant-market trade-size/magnitude discipline
The volatility-relative TP1 magnitude floor is now more defensive:
COMPRESSION 0.85 ATR, RANGE 0.75 ATR, TRANSITION 0.55 ATR, TREND/default 0.50 ATR,
HIGH_VOLATILITY 0.45 ATR and EXPANSION 0.40 ATR. This filters tiny operationally insignificant
setups without changing position sizing itself.

## Safety invariants retained

- Indicator remains broker-mutation-free.
- M15/M5/M1 role separation remains unchanged.
- Same ScenarioId remains idempotent.
- Broker-side geometry, volume and margin validation remain cBot-owned.
- Daily-loss, market-hours and spread protection remain active.
- Live execution is default OFF.
- Demo and Live account flags are mutually account-scoped.
- Future pending scenarios are not presented as current market arrows.
- Current-market execution and future-pending execution remain separate actions.
- No risk percentage was increased automatically.
- Position sizing remains risk-based and broker-normalized.

## Verification required

Automated Source/Architecture and Runtime Acceptance must pass.

Target terminal acceptance still needs to verify on an actual cTrader installation:
- live-account start with all live arms OFF shows LIVE / DISARMED rather than a demo-only stop;
- explicit live Market / Pending Stop / Pending Limit / Management arms mutate only when their corresponding controls are enabled;
- two or more distinct ScenarioIds can coexist up to the effective cap;
- current actionable scenario can execute while future scenarios remain pending;
- a future order can fill and transition into broker-confirmed active state;
- chart arrow, panel popup and alert direction remain identical;
- alert sound is actually audible in the user's cTrader/OS audio environment;
- same-chart cBot is detected without false NOT ATTACHED state;
- 100 ms signal reload does not create UI overload or excess CPU on the target machine;
- reconnect/restart preserves idempotency and broker truth.

## Operator action after merge

`git pull --ff-only`
