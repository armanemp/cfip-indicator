# CFIP Indicator — CI-01 Primitive Indicator Integrity

Date: 2026-10-01

Status: **IMPLEMENTED — awaiting final repository gate verification**

## Scope

CI-01 establishes mathematical and readiness contracts for the production primitive indicator layer and the hand-written market primitives that feed regime/context decisions.

The cTrader-native ATR, ADX/DMI, EMA and RSI implementations remain platform indicator authorities. The production code does not reimplement their internal smoothing algorithms.

The documented cTrader API confirms that ATR is exposed as AverageTrueRange, DirectionalMovementSystem exposes ADX/+DI/-DI and supports a moving-average type, EMA is exposed as ExponentialMovingAverage, RSI is a Wilder oscillator bounded to 0..100, and MACD CrossOver separately defines MACD line, signal line and histogram. citeturn387692search5turn884220search3turn884220search1turn387692search0turn873802search1

The current CFIP MACD feature intentionally uses only the fast/slow EMA difference because the existing public parameter surface contains only fast and slow periods. It is therefore named and tested as MACD-line bias, not as a MACD signal-line crossover or histogram implementation.

## Corrections completed

### DMI

DmiBias previously checked series bounds and finite values but did not enforce the same configured warm-up boundary used by ATR/ADX/EMA/RSI. It now consumes NativeIndicatorReadinessRule for both +DI and -DI and delegates the normalized formula to DmiBiasRule.

### Range Efficiency

The previous implementation mixed interval counts: the numerator used one closed-to-closed span while the denominator summed a different number of close changes. It also silently shortened the configured period near the beginning of the available series.

The canonical implementation now uses exactly N close-change intervals:

- start close = index - period;
- path = absolute close changes from start + 1 through index;
- net move = absolute difference between close[index] and close[start].

Insufficient history returns an unavailable value rather than silently changing the configured period.

### Choppiness

The previous implementation silently shortened a requested period when the current index was near the beginning of the series. It now requires the complete configured N-bar window before calculation and delegates the logarithmic CHOP formula to ChoppinessIndexRule.

### VWAP

The previous implementation gave every zero-volume bar an artificial unit weight and its window arithmetic could produce one extra sample when the configured lookback was above the minimum.

The canonical implementation now uses exactly N bars after minimum-lookback normalization, actual non-negative tick volume, zero weight for zero-volume samples, and rejects the result when total volume is zero. Directional comparison and accumulation arithmetic are delegated to VwapBiasRule.

### Volume Expansion

Current-bar range no longer uses a pip-size floor. A bar's geometric range is its actual High - Low; a non-positive range is unavailable rather than being silently replaced by instrument scale.

The average volume/range window remains the existing 20 preceding bars and the current bar remains excluded from the baseline.

### MACD naming/semantics

The implementation now uses explicit macdLine and previousMacdLine names. No new signal-line parameter or threshold was introduced. This avoids presenting the two-EMA difference as a histogram or crossover engine.

## Non-changes

CI-01 intentionally does not tune scoring weights, tune thresholds, add a MACD signal-period parameter, replace cTrader native indicator mathematics with a second calculator, alter FVG/OB/structure mathematics, alter divergence/WaveTrend mathematics, or alter decision thresholds/execution geometry.

Those concerns remain in their specified CI phases.

## Deterministic contract coverage

Runtime contracts cover DMI formula and symmetry, MACD-line bias directionality, exact RangeEfficiency window and ratio, exact Choppiness window and logarithmic formula, VWAP directional symmetry and zero-volume behavior, and Volume Expansion prerequisites and configured ratio.

Static CI-01 audit verifies that corrected consumers are bound to their canonical math owners and that period shortening is not reintroduced.

## Next phase

**CI-02 — OSS numerical parity / warm-up / cache audit**

The next phase must start from the merged CI-01 main commit and continue the same one-owner/no-duplicate rule. Do not resume Prompt 8 refinement before CI-FINAL.
