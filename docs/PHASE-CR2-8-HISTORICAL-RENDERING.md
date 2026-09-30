# CFIP — CR2.8 Historical Rendering Semantics and Cost — 2026-09-30

## Status

**VERIFIED COMPLETE — PR #93 merged to main.**

- Pull request: #93
- Final verified code head before merge: 18a0d14035b30249bac69f0315c1ad143af33704
- Merge commit: cab5a5e2e9a4fbccaf3ffe10d114c4ff54e6a243

## Scope

This phase implements only Claude finding B9: historical chart rendering cost and presentation semantics.

## Root-cause findings

The previous historical renderer:
- rebuilt its historical object set on each host-bar historical refresh;
- could scan the entire available history back to index 40, with only the number of accepted signals bounded by HistoricalSignalLimit;
- used chart bar index as object identity, which is unstable when chart history is prepended;
- re-ran expensive frame/trigger/ATR calculations for bars already evaluated during prior historical renders.

The review finding is therefore narrowed to the actual host-bar rebuild path rather than an incorrect every-Calculate claim.

## Implemented hardening

### Fixed scan budget
Core/Math/HistoricalRenderingRule.cs defines:
- minimum history: 60 bars;
- first analyzable index: 40;
- maximum historical scan: 500 closed bars.

HistoricalSignalLimit remains a public display parameter and continues to limit qualifying arrows.

### Closed-bar result cache
Historical presentation evaluation is cached by the closed bar DateTime OpenTime. The cached value stores only presentation facts: timestamp, direction, arrow price, and signal-present state.

### Timestamp-based chart identity
Historical chart objects now use CFIP_H_PRESENTATION_<OpenTime.Ticks>. The previous index-based CFIP_H_<index> identity is removed.

### History-change invalidation
The renderer subscribes to cTrader Bars.HistoryLoaded and Bars.Reloaded. It also detects a first-open-time change or history-count shrink and invalidates historical presentation state.

### Stable drawing synchronization
The renderer no longer removes every historical object on each ordinary refresh. Newly required objects are drawn and stale objects outside the bounded presentation window are removed.

### Presentation-only semantics
The historical renderer contains no alert emission, signal-plan creation, broker execution, lifecycle mutation, or outcome registration. Historical arrows are presentation-only; they are not independently verified live signals, replay results, backtest results, or realized outcomes.

## Verification

Source / Architecture: PASS.
Runtime Acceptance: PASS, run 1585, on head 18a0d14035b30249bac69f0315c1ad143af33704.
cTrader Compile: PASS, run 1769, on head 18a0d14035b30249bac69f0315c1ad143af33704.
CR2.8 static audit: PASS.

The static audit enforces the fixed scan budget, closed-bar bounds, timestamp cache, timestamp object identity, history invalidation, host-bar rebuild guard, and presentation-only boundary.

## Routine and performance audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed.

No Decision, signal threshold, OB/FVG/WaveTrend/structure/divergence semantics, Entry/SL/TP geometry, RR floor, Auto Trading, Auto Orders, position capacity, broker mutation ownership, or lifecycle/outcome authority was changed.

Performance/code-cleanliness review confirmed no network/file persistence or broker mutation in the renderer, deterministic timestamp cache identity, bounded scan work, bounded cache window, top-level helper-model architecture, and removal of unused renderer imports.

## Manual target-terminal boundary

CI cannot prove actual target-terminal visual behavior. Manual verification remains required for history prepend/load-more, reconnect/reload, chart-index remapping, perceived responsiveness with historical signals enabled, and confirmation that historical arrows never trigger alerts, plans, orders, or outcome records.

No accuracy, win-rate, RR improvement, or profitability claim is made from this phase.

## Next phase

CR2.9 — Structural stop, divergence and rejection guardrail refinement (B10/B11/B12).

Track 12A local cBot separation remains blocked until CR-FINAL is accepted.
