# CBOT Position Truth / Restart Idempotency Hardening — 2026-10-03

Status: IMPLEMENTATION COMPLETE — automated verification PASS; target-terminal acceptance pending.

## Purpose

This phase closes the remaining cBot correctness seam in the execution chain:

Indicator analysis -> M15 decision -> M5 trigger/entry precision -> Entry/SL/TP -> SignalEnvelope/ScenarioBatch -> cBot preflight -> per-ScenarioId broker truth -> execution -> broker confirmation -> management/protection.

The scope is limited to execution correctness. No analytical quality threshold is changed.

## Findings closed

### 1. Aggregate reconciliation was conflating valid scenarios with ambiguity

CBOT-6M legitimately allows several independent ScenarioIds to coexist. The previous aggregate reconciliation path treated more than one managed broker object as a recovery condition even when the objects belonged to different scenarios.

The canonical correction is:

- a non-empty execution label is reconciled as one exact scenario;
- an empty preferred label is reconciled as an aggregate instance view;
- duplicate broker objects for the same logical scenario remain a recovery condition;
- a position plus pending order remains a recovery condition only when both belong to the same scenario;
- independent scenario objects reconcile as normal multi-scenario state.

This keeps the safety rule while removing false global ambiguity.

### 2. Scenario processing now adopts per-scenario broker truth

CFIPExecutionBot.ProcessSignalEnvelope explicitly refreshes reconciliation from ReconcileScenarioState(envelope, ...).

The global label-selected reconciliation remains useful for startup/heartbeat aggregate state, but it is no longer the authority used to judge an individual scenario before execution.

### 3. Aggregate broker state is visible to the status publisher

When the active scenario label is empty, the state publisher discovers managed positions/pending orders by the Indicator-instance scope. This prevents the cBot from publishing zero managed objects merely because no single scenario has been selected yet.

### 4. Management no longer inherits unrelated scenario recovery

The cBot no longer blocks the management command processor merely because reconciliation for another or aggregate scenario is in recovery. Management commands continue to resolve their own exact identity and broker object before mutation.

### 5. Stable execution identity is validated at the cBot boundary

Preflight now rejects incompatible contract versions, missing Plan/Intent, mismatched Intent identity, incomplete Signal/Scenario/Plan/Idempotency identity and ExecutionAction.None.

### 6. Idempotency survives restart

CbotExecutionIdempotencyStore persists execution-attempt state in Device-scoped LocalStorage, scoped by Indicator InstanceId.

- confirmed idempotency keys remain suppressed for the retention window after cBot restart;
- failed submission attempts are retryable after a short bounded cooldown;
- the existing broker-object capacity/reconciliation checks remain a second protection against duplicate execution.

## Safety

- Live accounts remain blocked.
- M15 remains the canonical trade-decision/execution reference.
- M5 remains trigger/tuning/entry precision.
- M1 remains optional confirmation.
- H1 remains context/reward and is not independently auto-executed.
- Existing RR, quality, risk, margin, spread, market-hours and daily-loss gates are not lowered.
- Indicator broker-mutation authority is unchanged: the cBot remains the sole broker mutation owner.
- Concurrent capacity remains bounded by Max Concurrent Scenarios.

## Verification feedback incorporated

The first cTrader compile gate exposed two integration defects in the initial implementation: one missing Market coordinator argument and one inaccessible Contracts hash helper. Both were corrected on this branch before final verification. A serialization-escape issue in the new Device store was also corrected before the current compile cycle.

## Verification status

The automated Source/Architecture audit, Runtime Acceptance and cTrader Compile/Build passed for the implementation merged through PR #227. A follow-up per-scenario recovery-truth regression was then added on `main` and is being re-run through the same verification path.

## Verification required

Automated:
- Source/Architecture accumulated audit;
- Runtime Acceptance;
- cTrader Compile/Build;
- dedicated position-truth audit.

Target terminal/manual:
- attach/restart/rebind;
- two distinct ScenarioIds active together;
- same-scenario duplicate replay;
- restart with a confirmed execution key;
- rejection retry after cooldown;
- independent protection/reconciliation of multiple broker objects;
- management of one scenario while another scenario is in recovery;
- panel aggregate broker counts at startup.

No profitability claim is implied by this phase.

Operator action after merge: git pull --ff-only on local main.
