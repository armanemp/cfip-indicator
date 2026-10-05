# Phase 8.3 — FVG Mathematical Audit

Status: implementation verified and ready to merge.

## Scope

Audit and normalize the production Fair Value Gap chain without adding public parameters:
- canonical 3-bar FVG mathematics;
- explicit 2-bar imbalance mathematics;
- gap-size threshold evaluated with ATR at the FVG creation bar;
- overlap and invalid geometry;
- deterministic partial-fill and full-fill semantics;
- bounded age/lifecycle behavior;
- stable FVG identity so one source event cannot become several semantic candidates.

## Decisions

1. A bullish 3-bar FVG is Low[i] > High[i-2]; a bearish 3-bar FVG is High[i] < Low[i-2]. Equality is not a gap.
2. Minimum gap size is evaluated against ATR from the creation bar, not the current bar.
3. Two-bar imbalance remains an explicit extension (FVG_2BAR) and is never silently treated as the 3-bar formula.
4. Zone geometry must remain Low < High.
5. Partial mitigation moves only the affected boundary toward the fill price. Full fill invalidates the zone when safety invalidation is enabled.
6. All lifecycle scans remain bounded by the configured FVG lookback and maximum zone age.
7. No public parameters are added in this phase.

## Implementation closeout

Production `FvgDetectionAnalyzer` and `PredictivePendingZoneCollector` now consume `FvgRule`; historical gap thresholds use creation-bar ATR; current retest is post-creation candle range interaction; mitigation uses one shared full-fill/partial-fill mathematical owner; managed FVGs carry stable source identity. No public parameter count changed.

## Verification

Head `89919e7363d374e2cf3a362ec553b1fdac464919`: Runtime Acceptance PASS; Build PASS; Source/Architecture PASS.

## Acceptance

Automated:
- deterministic 3-bar and 2-bar gap tests;
- creation-ATR threshold test;
- overlap/geometry tests;
- bullish/bearish partial-fill symmetry;
- full-fill invalidation;
- identity stability;
- Runtime, Build and Source/Architecture gates.

Target cTrader replay remains required for empirical signal-quality measurement. No empirical false-signal or win-rate improvement is claimed from CI alone.
