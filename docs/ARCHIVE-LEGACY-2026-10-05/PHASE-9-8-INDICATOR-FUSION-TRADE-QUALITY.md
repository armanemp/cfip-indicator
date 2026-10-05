# Phase 9.8 — Indicator Fusion & Trade Quality

Date: 2026-09-29

## Status

VERIFIED COMPLETE on `ae7abf3f169d0743e56be85f59f0d9ffa5081069`.

Final pre-merge gates on the implementation head:
- Runtime Acceptance / Decision Contracts: PASS
- cTrader Compile/Build: PASS
- Source/Architecture and project audits: PASS

Merge closeout: PR #50 merged into `main` as `37cfd761bbb439d6e154315995664721b6c31740`.

Operator pull requirement: local `main` must be pulled before the next continuation.

## User-requested direction

This phase prioritizes deeper signal quality, indicator-engine coherence and the automatic trading path. It also removes compact level-label backgrounds entirely and renders level-label text in white.

## Indicator fusion

The previous frame-scoring path could stack several correlated indicator votes independently (for example RSI, EMA slope, DMI, momentum, MACD, WaveTrend and OSS confluence).

Phase 9.8 introduces one deterministic `IndicatorEvidenceFusionRule`:

- separates trend, momentum and context evidence;
- applies regime-aware weights;
- caps the contribution of OSS indicator consensus instead of treating every agreeing indicator as an independent edge source;
- measures directional conflict instead of silently converting contradictory indicators into stronger confidence;
- treats strong divergence as a reduction to the affected directional momentum evidence;
- produces both indicator-confluence quality and indicator-conflict metrics.

The same fused result is stored on the market `Frame` and then propagated into the canonical `Decision`.

## Smart Quality

`DecisionQualityCalculator` now consumes M5 indicator-confluence quality and indicator conflict.

When fusion data is available:

- Smart Quality includes a bounded indicator-quality component;
- significant indicator conflict receives a deterministic penalty.

When fusion data is unavailable, the previous Smart Quality formula is preserved for compatibility with existing contracts and early/no-data states.

## Decision score de-duplication

The adaptive regime section of `DecisionScoreCalculator` no longer re-adds raw displacement/liquidity/FVG/OB/volume votes that were already included in frame scoring.

Instead it consumes the canonical M5 fusion quality/conflict and applies only a small bounded regime-quality adjustment.

This removes a major source of double counting.

## Automatic trading

Automatic market execution now additionally requires:

- M5 indicator fusion quality of at least 60;
- M5 indicator conflict no greater than 52.

Automatic pending execution uses a slightly wider preparation floor:

- M5 indicator fusion quality of at least 58;
- M5 indicator conflict no greater than 55.

These are execution-specific safety floors on top of existing canonical ActionableNow, confidence, Smart Quality, structural level and suitability gates.

The Phase 9.7 bounded Market Range execution, fresh suitability refresh, spread-to-stop-risk validation, single-capacity rule and broker-confirmation lifecycle remain unchanged.

cTrader's current Algo documentation exposes Market Range execution with an explicit slippage range and also exposes server-side protection APIs. This phase retains the existing basic protected order call; migration to the newer advanced server-side multi-target protection API is kept as a separate hardening phase so it can be verified against the project's cTrader API reference rather than introduced speculatively.

## Execution-zone continuity

The existing execution-zone selector remains canonical:

- M5 FVG/OB confluence;
- M5 single-zone candidates;
- M15 FVG/OB candidates;
- MTF zone overlap;
- structural swing fallback.

No second entry-zone engine was introduced.

## Chart labels

Compact level-label rectangles are no longer drawn.

All compact level-label text is forced to `Color.White`.

Legacy `*_BOX` objects are removed on redraw so old background rectangles do not persist after upgrading.

## Verification boundary

Required automated gates:

- Runtime Acceptance / Decision Contracts;
- cTrader Compile/Build;
- Source/Architecture and all project audits.

Target cTrader replay remains required for empirical validation of false-signal frequency, regime classification, indicator fusion behavior, entry timing, slippage, realized RR and actual chart readability.

No profitability, win-rate or expectancy improvement is claimed from static/contract verification alone.

## Research basis

The implementation uses current cTrader Algo capabilities for multi-timeframe bar retrieval, Market Range execution, volume normalization and broker/server-side protections. Relevant official references:

- cTrader MarketData.GetBars supports retrieving bars for other timeframes;
- cTrader Market Range execution accepts an explicit slippage range;
- Symbol.NormalizeVolumeInUnits and VolumeForFixedRisk are the supported volume-normalization/risk-sizing primitives;
- current cTrader Algo documentation describes server-side protection and newer advanced protection APIs.

The phase does not introduce asynchronous execution merely for speed; correctness, bounded price acceptance and confirmed broker state remain the authority.


Final merge baseline: `37cfd761bbb439d6e154315995664721b6c31740`.