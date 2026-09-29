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

Current confirmed trigger readiness is M5 closed-bar based. M1 evidence affects score when enabled, but the confirmed trigger function remains `ClosedBarTriggerReady` over M5 data.

Before calling the M1 path a real precision trigger, define one explicit model: closed-bar-only; or controlled intrabar with hysteresis and invalidation.

### G. Track 7 — Parameter truth

`MaximumOpenPositions` currently exposes a range above 1 while the execution-capacity owner supports only exactly one active plan.

Also audit hidden clamps, dead settings and semantically duplicated thresholds.

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

The immediate safety-critical issues found in this audit are addressed in Phase 1.4, and the implementation has passed repository verification gates. The next structural improvement is the Safety Supervisor (Phase 1.5), followed by unified execution retry semantics and the canonical signal visual snapshot.

This document is a continuity record and should be read alongside `ROADMAP.md`, `ARCHITECTURE.md`, `WORKFLOW.md`, `DEVELOPMENT-LOG.md` and `ACCEPTANCE-MATRIX.md`.
## 2026-09-29 Performance and ZIP audit addendum

The uploaded four-indicator archive was reviewed. The production FVG engine remains the authoritative implementation; the archive's useful bounded-work idea was applied without importing its chart-side ownership. WaveTrend was assessed as a correlated momentum composite and intentionally not added as multiple independent decision votes. The economic-calendar implementation was not imported because its blocking external network fetch is inappropriate for the execution core. Volume Profile was empty.

The performance architecture and exact adoption decisions are recorded in docs/PERFORMANCE-ARCHITECTURE-2026-09-29.md.
