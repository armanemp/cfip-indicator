# CFIP Indicator — Deep Project Audit 2026-09-29

## Scope

This audit reviewed the current `main` architecture, runtime calculation flow, decision pipeline, entry/trigger path, trade-plan construction, market/aggressive/pending execution paths, broker confirmation, lifecycle recovery, live management, chart rendering and runtime contracts.

Repository inventory at the audit baseline:

- 401 production C# files;
- all production modules remain below the 20 KiB source ceiling;
- one decision authority;
- one execution authority;
- explicit broker mutation owners;
- explicit lifecycle/broker-confirmed state ownership;
- closed-bar MTF decision context with live reaction kept separate.

## Executive findings

### Fixed in Phase 1.4

1. Closed-bar analysis faults could return from `Calculate()` before management/protection/reconciliation.
   - Fixed by continuing the management-first live cycle after recoverable preparation/analysis faults.

2. Repeated failure on the same closed bar could repeatedly re-enter expensive analysis.
   - Fixed with per-closed-bar failure key, timestamp, exponential backoff, retry circuit and stale-failure detection.

3. Pre-trade plan SL was not rendered because the live broker SL accessor intentionally returns zero before a live position exists.
   - Fixed: pre-trade visuals use `Plan.Stop`; live visuals use broker-confirmed active SL.

4. Automatic market and aggressive success alerts reported desired plan SL/TP rather than necessarily broker-confirmed values.
   - Fixed: success reporting now uses broker-confirmed active protection and explicitly marks missing protection as `RECOVERY`.

5. Pending Stop/Limit placement reporting used intended levels after submission.
   - Fixed: reporting now reads the broker-confirmed `PendingOrder` fields.

6. Pending setup code had avoidable null-dependency exposure between decision/reaction state and pending preparation.
   - Fixed with explicit dependency guards.

7. Reversal pending preparation silently swallowed execution-model exceptions.
   - Fixed with explicit failure reason.

## Architecture strengths

### Signal and decision

- Confirmed decisions are built from closed-bar MTF context.
- BUY and SELL evidence are collected separately before consensus.
- Decision score, consensus, quality, confidence and filtering are separate owners.
- Trigger readiness is a distinct field and plan creation requires `TriggerReady`.
- Prediction/watch, confirmed decision and active plan are already conceptually separated.

### Planning

`Plan` explicitly carries Entry, IdealEntry, EntryTrigger, EntryInvalidation, Stop, TP1..TP4, quality/source metadata, lifecycle state (`IsLivePosition`, `PositionId`).

This gives the system enough semantic separation to unify visuals and execution without conflating planned and broker-confirmed levels.

### Execution

- Market execution, aggressive execution and pending Stop/Limit each have preparation/validation/mutation/fill owners.
- Broker confirmation is required before adopting execution state.
- Fill mismatch can trigger exit/recovery.
- Broker protection is validated independently and the broker remains authoritative.
- Pending fills are consumed from the broker `PendingOrders.Filled` event into live-plan state.

### Visual synchronization

- Plan rendering removes prediction/watch objects when an active plan exists.
- Pending rendering uses broker pending-order levels.
- Confirmed pending-order reporting is isolated in `PendingOrderConfirmationReporter.cs` so placement owners remain small orchestration boundaries.
- Active SL/TP accessors distinguish broker-confirmed live protection from desired plan values.
- Signal arrows have an authoritative-direction resolver rather than being driven by a separate trade engine.

## Remaining high-value improvements

### A. Phase 1.5 — Safety supervisor

Add a lightweight timer boundary for broker-state reconciliation, protection recovery, pending expiry, EOD supervision and stale-plan detection.

Do not run the full analysis engine at heartbeat frequency.

### B. Track 2.1 — Unified submission retry semantics

Current `SubmissionGate` already applies backoff/circuit behavior, but failure history is maintained per execution gate rather than being partitioned by signal/attempt identity.

Recommended model:

`signal identity → attempt identity → execution path → failure count → next retry → circuit state`

This is needed to make retry behavior traceable and avoid unrelated signals sharing suppression state.

### C. Track 3 — Identity hardening

The current managed identity is label/symbol based. Add one canonical identity object, explicit instance scope, cross-instance duplicate prevention, and exact managed position/pending-plan binding.

### D. Track 4 — Trading-day and EOD semantics

Separate and test broker/session trading day, realized loss, floating loss, combined daily loss, reset point, EOD warning, EOD cancel/close and late-created positions.

### E. Track 5.4 — Canonical signal visual snapshot

Create one immutable `SignalVisualSnapshot` carrying signal stage, direction, prediction confidence, confirmed decision confidence, entry, trigger, invalidation, SL, TP1..TP4, broker-confirmed entry/SL/TP when live, pending order identity and plan identity.

All arrow, trigger, entry, TP and SL renderers should consume that snapshot.

This removes the remaining risk of each renderer interpreting state independently.

### F. Track 6 — Explicit intrabar policy

Resolved by Phase 8.1 — M1 trigger correctness:
- M1 no longer contributes a fixed directional vote to decision consensus.
- When `UseM1Trigger` is enabled, M1 is a confirmation layer evaluated from the canonical closed M1 index.
- The selected M1 bar must be fully closed, belong to the exact selected closed M5 window, match the selected direction, satisfy body/range/close-location constraints and meet the configured trigger score.
- Confirmed TriggerReady becomes `ClosedM5TriggerReady AND M1TriggerReady` when M1 confirmation is enabled; otherwise it remains the canonical M5 trigger.
- The M1 path is deliberately closed-bar based, not an uncontrolled intrabar trigger.

The empirical effect on false-signal rate remains unverified until deterministic replay or historical outcome validation is completed.

### G. Track 7 — Parameter truth

Resolved in Phase 7.2 and Phase 7.4:
- dead/unused public settings were audited;
- semantic aliases were audited;
- `MaximumOpenPositions` is retained only as a compatibility setting constrained to 1 because the current execution architecture supports one active managed plan only;
- automatic market, aggressive and predictive-pending execution now share one semantic single-plan capacity guard; `BlockNewSignalWhileActive` was removed because the single-plan boundary is mandatory;

Remaining parameter work is limited to future changes where the underlying capability is actually expanded; no numeric setting should advertise multi-plan behavior until a real multi-plan architecture exists.

### H. Track 8 — Mathematical correctness

Perform deterministic audits for M1 trigger semantics, swing equality/plateaus, FVG definition/mitigation, Order Block definition/mitigation/confluence, BUY/SELL mirror symmetry and target obstacle logic.

### I. Track 9 — Decision intelligence

Current confidence is a deterministic composite score. It should not be presented as calibrated probability.

Next refinement: control correlated evidence duplication; empirical calibration from outcome data; regime-conditioned relevance; explicit no-trade explanations.

### J. Track 10–12 — Risk, telemetry and exit reliability

Consolidate margin logic, sizing semantics, daily-loss semantics, alert identity, outcome idempotency, partial-close retry policy and break-even confirmation/retry.

### K. Track 13+ — Target terminal validation

Repository CI proves source contracts and compile compatibility, but target-terminal validation remains necessary for actual cTrader API behavior, permission flow, live broker protection, fill semantics, startup/unload, event timing and reconnect/reload behavior.

## Signal → trade canonical flow

`Market data` → `Closed MTF context` → `Evidence` → `Decision` → `Trigger` → `Trade Plan` → `Execution Intent` → `Risk/Eligibility` → `Broker Mutation` → `Broker Confirmation` → `Live Plan` → `Visual Snapshot` → `Live Protection/Exit` → `Outcome`

The important rule is that the same plan/intent identity should drive both the chart and the broker path; visual presentation must never independently recreate trade parameters.

## Phase discipline

The following areas are intentionally not silently folded into Phase 1.4:

- full M1 trigger redesign;
- confidence calibration;
- multi-position support;
- cross-instance identity;
- daily-loss/trading-day redesign;
- adaptive learning;
- large-scale strategy-weight changes.

Those need their dedicated tests and acceptance criteria before production behavior is changed.

## Audit conclusion

The current architecture is strong enough to continue refinement without rebuilding the core.

The immediate safety-critical issues found in this audit are addressed in Phase 1.4 and Phase 1.5. The canonical visual-state contract is now implemented in the current Track 5.4 work. Remaining major areas are unified submission retry semantics, identity/session semantics, explicit intrabar policy, analytical correctness and empirical decision calibration.

This document is a continuity record and should be read alongside `ROADMAP.md`, `ARCHITECTURE.md`, `WORKFLOW.md`, `DEVELOPMENT-LOG.md` and `ACCEPTANCE-MATRIX.md`.
## 2026-09-29 Performance and ZIP audit addendum

The uploaded four-indicator archive was reviewed. The production FVG engine remains the authoritative implementation; the archive's useful bounded-work idea was applied without importing its chart-side ownership. WaveTrend was assessed as a correlated momentum composite and intentionally not added as multiple independent decision votes. The economic-calendar implementation was not imported because its blocking external network fetch is inappropriate for the execution core. Volume Profile was empty.

The performance architecture and exact adoption decisions are recorded in docs/PERFORMANCE-ARCHITECTURE-2026-09-29.md.


## 2026-09-29 Track 5.4 certification

Canonical SignalVisualSnapshot is fully implemented. The branch passed Source / Architecture, Runtime Acceptance Contracts and cTrader Compile after the final cleanup of direct renderer state reads. Live Plan, Pending, pre-trade Plan, Confirmed, Reaction and Prediction states remain explicitly ordered and broker-confirmed live levels remain distinct from intended plan levels.

## 2026-09-29 Phase 9.17 corrective exit/risk calculation audit

A second-pass review was performed after the initial Phase 9.17 implementation, focused on hidden
numeric inconsistencies and state desynchronization rather than feature expansion.

### Findings closed

- TP forward distance is now centralized and combines broker minimum TP distance with ATR/pip/tick spacing.
- Percentage broker-distance conversion uses the executable-side quote for the trade direction.
- Server-side ladder adoption reads the broker-owned LastTakeProfit.Price instead of rebuilding internal state from plan levels.
- Live TP mutation paths reject already-passed or wrong-side targets and require monotonic progression.
- Fill reconciliation now distinguishes the existing SL from the structural candidate before applying stop ratcheting.
- Recovery fallback target construction is capped by MaximumRewardRR and revalidated against live quote geometry.
- RR progression for later targets is derived from current prices rather than cached RR fields.
- NaN/Infinity is fail-closed in live exit geometry, plan RR validation and Smart Break-Even.
- A server-side ladder object without a valid live broker target no longer counts as synchronized protection.

### Verification boundary

Automated verification passed for cTrader compile/build, Decision/Planning/Execution contracts,
runtime acceptance and the accumulated source/architecture audits including the dedicated exit-geometry audit.
Target-terminal replay is still required for actual cTrader runtime timing, server-side advanced-protection
behavior, slippage and empirical confirmation of the reported TP rollback.
