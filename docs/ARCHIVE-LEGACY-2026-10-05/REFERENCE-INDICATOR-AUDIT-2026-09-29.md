# Reference Indicator Audit — 2026-09-29

## Source files inspected

The user-provided Library archives contain:
- `fvg.txt`
- `wavetrend.txt`
- `wave-trend.txt`

The two WaveTrend files contain the same `CUSTOMWAVETREND` implementation.

## FVG reference findings

The reference FVG indicator implements three distinct zone families:

1. Opening gap: absolute distance between current open and previous close, gated by a pip-size threshold.
2. Two-bar imbalance zone: current bar versus previous bar high/low.
3. Three-bar imbalance zone: current bar versus the bar two positions back high/low.

For the 2-bar and 3-bar structural zones, the mathematical geometry matches the canonical formulas now centralized in `FvgRule`.

Its lifecycle has an important behavior worth preserving:
- zone invalidation can be wick-based or body-based;
- partial fills shrink the active rectangle boundary rather than instantly deleting the zone;
- history rectangles can be retained or removed independently from active state.

Phase 8.3 already centralized the 2-bar/3-bar geometry, creation-bar ATR thresholding, retest semantics and mitigation. The reference indicator's opening-gap family is deliberately kept separate from structural FVG identity; it should not be silently relabeled as a 3-bar FVG.

## WaveTrend reference findings

`CUSTOMWAVETREND` is a custom composite oscillator:

- RSI uses `Bars.TypicalPrices` with default period 10.
- MFI uses the same default period 10.
- RMI is computed from close-to-close momentum with default momentum length 5, then smoothed with two EMAs of period 10.
- The three 0..100 components are averaged.
- The combined series is smoothed with default Exponential MA length 4.
- Signal is a Simple MA length 5 over the smoothed combined series.
- Displayed WAVE and SIGNAL are shifted by -50.
- Histogram is WAVE minus SIGNAL; adaptive mode optionally colors by histogram slope.

Important edge cases in the source:
- momentum length 0 is allowed by the parameter but makes the RMI numerator/denominator degenerate;
- division by a zero downside EMA is not explicitly guarded.

## Integration decision

The reference WaveTrend should not become a standalone trade trigger. The safe architecture is a closed-bar adapter exposing:
- wave value;
- signal value;
- wave/signal spread;
- zero-line state;
- slope/histogram direction;
- overbought/oversold state.

Those states can then act as confluence evidence alongside structure, liquidity, FVG and Order Block evidence. Any adapter must reproduce the reference semantics first in deterministic tests before its evidence can affect the trade decision.

No new public parameters are added by this audit.


## Phase 11 reassessment — user-provided indicator archives

The additional Library archives were re-inspected:

- `indicator.zip` contains `wave-trend.txt` plus an empty TPO profile placeholder.
- `indicators.zip` contains `economic.txt`, `wavetrend.txt`, `fvg.txt` and an empty `volume-profile.txt`.
- `wavetrend.txt` and `wave-trend.txt` are the same WaveTrend implementation.

WaveTrend:
- CFIP already contains a deterministic internal WaveTrend engine with closed-bar snapshots and evidence mapping.
- The internal implementation includes finite-value guards and a clearer adapter boundary, so the reference file is retained as a parity reference rather than duplicated.

FVG:
- CFIP already implements 2-bar and 3-bar FVG geometry through FvgRule.
- CFIP already implements partial mitigation, wick/body break behavior and full-fill invalidation.
- The reference FVG indicator's opening-gap family is a separate concept and is not promoted to structural FVG evidence because it overlaps with existing displacement/volatility context and could create correlated double-counting.
- No replacement of the canonical FVG engine was warranted.

Economic:
- The supplied economic indicator is materially useful because it uses a weekly XML economic-calendar feed and symbol-currency matching.
- The older reference implementation only displays scheduled events and does not block trading itself.
- Phase 11 promotes the useful calendar parsing concept into CFIP's canonical risk layer: cached feed, relevance filtering, impact-aware blackout, stale-feed handling, pending cancellation and runtime telemetry.
