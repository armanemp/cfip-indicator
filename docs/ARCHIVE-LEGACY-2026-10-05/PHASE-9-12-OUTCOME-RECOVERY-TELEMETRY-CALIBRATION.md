# Phase 9.12 — Broker Outcome / Recovery Telemetry & Recent Lifecycle Calibration

Date: 2026-09-29

## Status

VERIFIED COMPLETE on `phase/9-12-outcome-recovery-calibration`.

## Why this phase was required

Phase 9.11 exposed broker submission telemetry through a latest-only state and closed signal visuals through a bounded lifecycle. The remaining weakness was that the runtime could not retain a bounded history of execution outcomes/recovery transitions for diagnosis, and empirical calibration could continue to be dominated by older in-memory outcomes.

This phase keeps the existing decision authority intact while making the observation layer more useful and deterministic.

## Implementation

### Bounded outcome history

Added `OutcomeObservation` and a bounded runtime history of 128 managed outcomes.

Each recorded managed broker close retains:
- position id and direction;
- opportunity lane and execution mode;
- calibration regime/confidence/bucket when the original plan is calibration-eligible;
- created/closed M5 and lifecycle duration;
- Pips, NetProfit and realized R relative to the plan risk;
- profitable outcome flag;
- whether broker protection recovery was still required at close;
- whether the server-side TP ladder was active.

The close event remains the authoritative outcome source. Duplicate position-close notifications are protected by the existing lifecycle idempotency guard and an additional position-id history check.

Recovery-only plans remain non-calibratable, preserving the Phase 9.3 rule that reconstructed runtime state cannot contaminate empirical signal context.

### Recent-first empirical calibration

Added a deterministic recent-context calibration path over the latest 128 eligible observations.

Selection remains hierarchical:
1. exact direction + lane + regime + confidence bucket;
2. lane + regime;
3. direction-only;
4. no calibration below the existing sample gates.

When a recent context has enough observations, it is preferred and explicitly reported with a `-RECENT` source suffix. When recent history is too sparse, the existing in-memory contextual dictionaries remain the fallback.

The smoothing prior and maximum confidence adjustment remain unchanged. No new public parameter was introduced.

### Execution and recovery telemetry history

Submission events are now retained in a bounded 64-record history instead of being represented only by the latest state.

The history records:
- automatic market / aggressive market / pending path;
- M5 attempt index;
- confirmed / unconfirmed / rejected / null-result / failed state;
- broker or submission reason;
- UTC timestamp.

Lifecycle transitions into `RecoveryRequired` and transitions out of recovery into `LivePosition`, `Closed` or `PlanReady` are also recorded through the same telemetry history with the `RECOVERY` path.

The existing latest telemetry fields remain intact for compatibility; the new history does not create a second execution authority.

### Panel diagnostics

The panel now exposes compact recent-history diagnostics:
- recent outcome count, observed win rate, average realized R and average lifecycle in M5;
- broker trace count, latest state/path, error count and recovery count.

These are diagnostic observations, not probability forecasts or trading commands.

## Verification contracts

Added deterministic Decision Contract coverage for:
- recent exact-context calibration;
- exclusion of older outcomes when the recent window is sufficient;
- adjustment response to the newest outcome;
- repeatability of the recent calibration result.

Extended the accumulated phase audit to require:
- bounded outcome history;
- bounded execution telemetry history;
- central broker-close outcome recording;
- recent calibration consumption;
- recovery transition telemetry;
- unchanged public parameter count and shared auto-trade/protection invariants.

## Safety / authority boundary

No new public parameter.
No second decision authority.
No second execution authority.
No new broker mutation path.
Broker-confirmed close remains authoritative for realized outcome registration.
Recovery telemetry is observational and does not mutate broker state.

## Empirical boundary

The history is still runtime in-memory. Restarting the indicator clears it. Target cTrader replay remains required to validate actual chart timing, realized R distribution, broker event ordering, and whether recent calibration materially improves signal quality.

## Next phase

Phase 9.13 — target-terminal lifecycle replay and outcome calibration validation.


## Final verification

- Runtime Acceptance #1096: PASS
- cTrader Compile/Build #1280: PASS
- Source/Architecture + accumulated audits #1287: PASS
- Decision Contracts within cTrader Compile/Build: PASS
- Public parameter count: 552
- Final verified branch head: `626618e7100b2e2cecf8a172d67ed49aee43345b`

The phase is complete for source/contract/build verification. Target-terminal replay remains the empirical acceptance boundary.

## Merge closeout

Merged PR #54 into `main` as `033bad1555fc7e2e1780c126d501020ddb7c7737`.
