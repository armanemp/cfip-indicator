# CFIP — Realtime / Live Execution / Signal Truth Unification — 2026-10-03

Status: IMPLEMENTATION COMPLETE — automated verification pending; target-terminal acceptance still required.

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

The Indicator maximum-open-position parameter is now bounded to 1..10 and defaults to 3.

The cBot computes an effective concurrent-scenario limit as:

min(Cbot Max Concurrent Scenarios, Indicator Maximum Open Positions)

The per-ScenarioId single-active-object rule remains intact, so multiple independent scenarios can coexist while duplicate objects for one scenario remain blocked.

### 3. Realtime handoff latency

cBot signal-store reload cadence was reduced from 500 ms to 100 ms.

The cBot still processes the published ScenarioBatch on every tick and keeps broker reconciliation independently throttled.

M15 remains the canonical decision/execution timeframe; M5 remains the trigger/entry-tuning layer and M1 optional.

### 4. Small / stagnant-market opportunity quality

Added a volatility-relative OpportunityMagnitudeRule.

Current and future opportunities are rejected when the planned TP1 movement is too small for the active market regime. The gate is independent of position sizing, so it does not confuse a deliberately small risk amount with a weak setup.

Additional RANGE filtering raises the low-RR floor from the previous 1.80 structural check to 2.00.

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
