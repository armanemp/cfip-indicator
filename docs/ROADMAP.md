# CFIP Indicator — Master Roadmap

Date: 2026-09-27
Repository: armanemp/cfip-indicator
Reference: CFIP-PRO cTrader v89

## Mission
Build a maintainable, testable, modern multi-file cTrader system without losing validated v89 behavior. This is controlled architectural reconstruction, not a blind split of one large file.

## Phase plan

### Phase 0 — Repository and governance
Bootstrap repository, architecture rules, workflow, source-preservation policy and acceptance-gate system.

### Phase 1 — Architecture foundation
Create solution/project structure, namespaces, Core contracts, value objects, dependency boundaries, CTrader adapter boundary and architecture tests.

### Phase 2 — Configuration and parameter parity
Inventory every v89 public parameter; classify ACTIVE, DEPRECATED or REMOVED; centralize configuration and eliminate duplicate meanings.

### Phase 3 — Runtime, time and MTF
RuntimeSnapshot, server UTC, display time, closed-bar reference, timeframe classification, data completeness and no forming-bar leakage.

### Phase 4 — Market model
Canonical MarketFrame, regime, trend, momentum, indicator evidence and provenance.

### Phase 5 — Structure, zones and liquidity
Canonical StructureSnapshot, event ledger, FVG lifecycle, Order Block lifecycle, liquidity objects and BUY/SELL symmetry.

### Phase 6 — Decision
One authoritative DecisionSnapshot containing direction, confidence, evidence, regime, eligibility and typed block reasons.

### Phase 7 — Entry and Trigger
One EntrySnapshot defining IdealEntry, EntryZone, Trigger, RequestedEntry policy and Invalidation.

### Phase 8 — Risk, SL and targets
One TradePlan containing structural SL, TP1-TP4 ladder, effective broker TP, RR, sizing, leverage, exposure and broker constraints.

### Phase 9 — Unified execution
One ExecutionPolicy and ExecutionIntent for market/stop/limit paths, idempotency, duplicate prevention and actual-fill reconciliation.

### Phase 10 — Pending orders
Pending lifecycle, fill handoff, protection handoff, cancellation, circuit breaker integration and multi-position association.

### Phase 11 — Position lifecycle and reconciliation
Position state, reconciliation, close confirmation, retry/backoff, restart/reconnect adoption and ownership guards.

### Phase 12 — Live management
Position-owned plan, BE/risk-free, structural SL repricing, dynamic targets, partial TP confirmation/retry, reversal/exhaustion/invalidation/EOD precedence and protection queue.

### Phase 13 — Outcomes, telemetry and calibration
Immutable outcome records, execution telemetry, calibration, drift and persistence boundary. Outcomes never gain execution authority.

### Phase 14 — Presentation
Chart, terminal/panel, authoritative lines/arrows, settings, localization and alerts. Presentation only renders authoritative state.

### Phase 15 — Cleanup and performance
Remove dead/duplicate code, optimize tick/event paths, allocation and logging, and validate resource use.

### Phase 16 — Verification and release
Full static/unit/scenario/replay tests, real cTrader compile, controlled broker execution, restart/reconnect, order/fill/SL/TP/close verification and release artifact.

## Acceptance rule
A phase is complete only when implementation, tests, relevant runtime gates, documentation, commit record and known limitations are all recorded.

## Product invariants
1. BUY=+1, SELL=-1, WAIT=0.
2. Analysis has no broker side effects.
3. Decision is not execution.
4. TradePlan is not broker state.
5. Broker is authoritative after mutation.
6. Lifecycle follows broker reality.
7. Execution is idempotent.
8. Pending fills are always adopted.
9. Partial TP is consumed only after broker confirmation.
10. SL only moves protectively.
11. Target ladder is authoritative.
12. No manual entry/order controls.
13. Closed-bar analysis cannot use forming-bar data.
14. Every fallback has explicit provenance.
15. Every parameter has a disposition.
16. BUY/SELL behavior is symmetric.
17. Presentation never creates trading authority.

## Versioning
v69-v89 remain frozen historical references in CFIP-PRO. This repository uses semantic implementation versions. Never overwrite a frozen reference artifact.

## Current state
Phase 0: IN PROGRESS.
All later phases: NOT STARTED in this repository.
