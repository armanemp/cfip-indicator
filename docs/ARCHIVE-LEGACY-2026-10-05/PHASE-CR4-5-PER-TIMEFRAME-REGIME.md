# CR4.5 / D5 — Per-timeframe regime semantics

## Scope

This phase resolves the verified gap where `MarketRegimeClassifier` could already
classify non-M5 bars, but `MarketFrameScoringService.ResolveFrameRegime` discarded
that result and forced every non-M5 frame to `UNKNOWN`.

## Findings

- M5 already has a dedicated cached core-regime path and remains unchanged.
- M15/M30/H1/H4/D1/W1 were already passed through the same `AnalyzeMarketRegimeCore`
  classifier when other code requested regime information, but frame scoring did not
  consume those results.
- `IndicatorEvidenceFusionRule` already treats `UNKNOWN` as neutral via unit weights;
  this is now explicit and normalized through one resolution rule.

## Implementation

### Per-timeframe regime ownership

`AnalyzeMarketRegime` now uses:
- the existing dedicated M5 cache/semantics for M5;
- a bounded `MarketRegimeFrameCache` for every non-M5 `Bars` series.

`Frame` now records `Regime`, `RegimeQuality` and `RegimeStability`. The frame evidence
builder obtains these values from the same regime analyzer for every timeframe.

`MarketFrameScoringService.ResolveFrameRegime` consumes the frame's own normalized
regime instead of treating non-M5 frames as `UNKNOWN`.

### Neutral UNKNOWN semantics

`FrameRegimeResolutionRule` is the canonical normalizer. Unknown or invalid labels
normalize to `UNKNOWN`.

`IndicatorEvidenceFusionRule` keeps `UNKNOWN` neutral: trend, momentum and context
regime weights stay at 1.0, with no directional preference.

### Cache discipline

The non-M5 regime cache is bounded to eight entries, covering the active MTF set.
Each entry is keyed by `Bars` identity and exact closed-bar index and is invalidated
when the series or stored bar fingerprints no longer match.

## Verification

- deterministic runtime contracts cover known/unknown normalization, direction
  independence, BUY/SELL symmetry and explicit UNKNOWN neutrality;
- dedicated `tools/audit_phase_4_5.py` is wired into Source/Architecture CI;
- all accumulated routine audits remain mandatory;
- cTrader compile and runtime acceptance remain required.

## Safety boundary

No public parameter name/type/DefaultValue changed.
No regime threshold or scoring weight value was tuned.
No execution authority, position capacity, RR floor or confidence threshold changed.
Target-terminal timing and empirical signal-quality validation remain manual acceptance boundaries.
