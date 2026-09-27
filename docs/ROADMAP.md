# CFIP Indicator — Roadmap

## Completed

- Reference behavior preserved from the complete v73 baseline.
- 513/513 parameters preserved.
- 311/311 reference methods preserved.
- Indicators isolated into dedicated files.
- Structure, zones, liquidity, decision, planning, execution, pending orders, lifecycle, risk, intelligence and UI decomposed into focused modules.
- v73 managed broker label restored.
- Architecture and cTrader compile gates added.

## Phase 2 — domain-boundary extraction

- Replace cross-module mutable field access with explicit domain snapshots/contracts.
- Move broker API calls behind a narrow broker mutation boundary.
- Separate decision state, execution intent, broker snapshot and presentation state.
- Add deterministic fixtures for MTF, FVG, Order Block, liquidity, decision, entry, SL/TP and lifecycle transitions.

## Phase 3 — advanced analytics

- Benchmark OSS indicator backends against cTrader native calculations.
- Adopt only compatible, measured components.
- Add feature-level confidence/provenance so every decision contribution is explainable.
- Expand prediction and outcome calibration without allowing prediction to directly execute trades.

## Phase 4 — trading acceptance

- Compile against the target installed cTrader Automate API.
- Test market execution, pending orders, rejection, slippage, missing protection, partial close, reconnect/restart, reversal, invalidation and end-of-day handling.
- Verify chart/panel state always matches broker state after every mutation.

## Phase 5 — performance and release

- Benchmark calculation latency and memory.
- Remove only proven repeated scans/allocations.
- Produce release build only after source, compile and scenario gates pass.