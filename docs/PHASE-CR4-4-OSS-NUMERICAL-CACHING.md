# CR4.4 / D4 — Skender numerical stability and incremental caching

## Scope

This phase hardens the production OSS indicator boundary without changing public
parameter names, types, defaults, trading thresholds, RR floors or decision /
execution authority.

## Findings reconciled

### Path dependence

Stable-prefix history is required for the recursive/path-dependent adapters:
- RSI (Wilder-style recursive smoothing);
- MACD (recursive EMA components);
- SuperTrend;
- Parabolic SAR.

The bounded rolling cache remains appropriate for fixed-window adapters:
- Bollinger Bands;
- MFI;
- Stochastic;
- Aroon;
- CCI.

OBV remains callable for diagnostic/research visibility, but its one-bar bias is
not treated as independent confluence evidence.

## Implementation

### Authoritative constants

`OssIndicatorParameters` is now the single owner of fixed OSS periods/factors,
the MACD signal period, adapter warm-up requirements, the bounded rolling
quote-window size, and the existing configured-period safety clamps.

The existing public `RsiPeriod`, `MacdFastPeriod` and `MacdSlowPeriod` semantics
remain unchanged.

### Incremental quote cache

`OssQuoteSeriesCache` now maintains:
- a stable prefix that starts at bar 0 and only appends new bars;
- a bounded 161-bar rolling window that advances by append/remove;
- first-bar and cached stable-last-bar fingerprints for conservative invalidation.

History replacement, reconnect, or cached-prefix mutation causes a full cache reset. cTrader HistoryLoaded / Reloaded events mark the cache invalid before the next calculation.
Consecutive new bars do not.

Backward index requests rebuild only the bounded rolling view or copy the already
materialized stable prefix to the requested end index.

### OBV boundary

OBV remains in `OssIndicatorSnapshot.ObvBias` for diagnostics/research, but it is
no longer counted in `BullVotes`, `BearVotes` or `IndicatorCount`. The extended
OSS feature remains default-off, so default CFIP decision behavior is unchanged.

## Verification additions

- `tools/audit_phase_4_4.py` — static acceptance gate;
- runtime contracts for the centralized OSS constants, warm-up and clamp semantics;
- isolated benchmark comparing per-bar quote-window rebuild with incremental
  append/remove materialization;
- benchmark report section for the incremental cache measurement.

The existing Skender 2.7.3 / FacioQuo 3.0.1 numerical parity benchmark remains
the research/package-comparison boundary and FacioQuo remains research-only.

## Safety boundary

No public parameter name/type/DefaultValue changed.
No default trading threshold, RR, confidence, position capacity or execution policy changed.
No future-outcome data enters live decision logic.
Target-terminal cTrader runtime timing, memory profile and empirical signal-quality
validation remain manual acceptance boundaries.
