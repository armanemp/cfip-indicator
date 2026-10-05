# CFIP Indicator — Performance Architecture 2026-09-29

## Objectives

Startup must minimize blocking market-data acquisition without compromising data readiness. Live execution must avoid recomputing stable closed-bar values and must keep broker safety independent from chart repaint frequency.

## Implemented in Phase 1.5

### Asynchronous startup

cTrader exposes `MarketData.GetBarsAsync(...)`. The project now requests the required M1/M5/M15/M30/H1/H4 series and optional D1/W1 series asynchronously, tracks outstanding loads, and finalizes native indicator registration only after the required data is ready.

This replaces the old 18-step serial timer acquisition/registration sequence with a small polling/finalization boundary. The timer is still intentionally retained for finalization and fault timeout so initialization remains bounded and observable.

### Closed MTF context cache

Stable MTF closed indices are cached until one of the participating Bars collections changes size or identity. This removes repeated `GetIndexByTime` work on every live tick while preserving recalculation when a timeframe advances.

### Closed M1 frame reuse

The preparation stage already computes the current closed M1 frame for panel/trigger consumers. Closed-bar processing now reuses it when the `(Bars,index)` identity is unchanged instead of recomputing the full M1 frame.

### M5 regime core cache

The current M5 regime snapshot previously evaluated the previous and before-previous regime cores again on each new bar. A bounded three-entry cache now retains those recent cores, so a normal new-bar transition only needs the new core calculation while stability checks reuse recent values.

### Zone scan bounds

FVG and Order Block candidate scans now cap their search window by the effective `MaximumZoneAgeBars`. Their mitigation walks are also bounded by the same maximum age. Older candidates were already rejected by the quality/age contract, so this optimization preserves selection semantics while reducing unnecessary history traversal.

### Protection hot path

Active-plan protection now reuses the already computed closed M5 ATR from the current frame when available and otherwise computes it once for the entire protection calculation. This removes repeated native-series lookups within one protection pass.

### Safety supervisor

The runtime timer now has a dedicated lightweight safety supervisor that can run broker reconciliation, live-plan recovery, broker protection and EOD supervision without running the full analysis engine.

Safety supervision is skipped while the main calculation cycle is busy, so the same indicator state is not concurrently mutated.

## Complexity intent

Targeted changes reduce repeated per-tick work in these areas:

`tick → MTF index mapping → cached`
`new M5 bar → recent regime cores → cached`
`zone search → effective age bounded`
`active protection → single ATR lookup/reuse`
`startup → async data acquisition instead of serialized timer stages`

These are structural performance improvements, not score-threshold tuning.

## ZIP indicator audit

The uploaded archive `indicators.zip` contained four files:

- `fvg.txt` — a chart-oriented SIBI/BISI/FVG-style implementation;
- `wavetrend.txt` — RSI/MFI/RMI composite oscillator with smoothing and signal line;
- `economic.txt` — external economic-calendar chart overlay using network access;
- `volume-profile.txt` — empty.

### FVG

The repository's existing FVG engine is retained. It already owns standard 3-candle FVG detection plus optional 2-bar imbalance, partial/full mitigation, age and quality, and it is separated from chart rendering. The uploaded implementation creates chart objects directly inside detection and uses independent history structures, so it is not a cleaner source of truth for the production engine.

Useful optimization ideas from the uploaded FVG implementation were adopted conceptually: avoid broad array copies and bound work to the active zone lifetime. The production implementation now enforces those bounds without importing its chart-side architecture.

### WaveTrend

The WaveTrend implementation was not added as another independent score vote. Its RSI/MFI/RMI ingredients overlap substantially with the project's existing momentum/RSI/MFI-style evidence and would increase correlated evidence and per-bar compute cost if simply added to the decision score.

The correct future use is as one composite momentum feature or as an early-prediction feature, not three or more independent votes. That requires an explicit owner and calibration before it can influence confirmed execution.

### Economic calendar

The uploaded implementation is useful as a UI/calendar concept, but its direct network fetch in an indicator would make startup and runtime availability dependent on external connectivity. The project therefore keeps news suitability separate from chart rendering and does not import the blocking `WebClient` pattern.

### Volume Profile

The file is empty and contributes no usable implementation.

## Design rule

Do not add a new indicator merely because it produces another number. A new feature must either:

- add genuinely distinct information;
- replace a weaker/correlated feature;
- or improve an existing feature's accuracy/robustness without creating a second decision authority.

This rule is now part of the project continuity record.

## Phase 2.1 execution-retry addendum

Submission retry is now owned by one keyed gate partitioned by signal identity, attempt identity and execution path. This prevents cross-signal contamination of retry suppression while keeping one consistent backoff/circuit policy.
