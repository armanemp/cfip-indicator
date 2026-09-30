// ============================================================================
// CFIP Indicator — NativeIndicatorReadinessRule.cs
// Platform-neutral native-indicator warm-up and numeric readiness contracts.
// ============================================================================

using System;

namespace cAlgo
{
    internal static class NativeIndicatorReadinessRule
    {
        internal static bool IsIndexedSeriesReady(
            int index,
            int resultCount,
            int warmupPeriod)
        {
            return
                index >=
                    Math.Max(
                        1,
                        warmupPeriod) &&
                index >= 0 &&
                index < resultCount;
        }

        internal static bool IsIndexedWindowReady(
            int index,
            int lookbackBars,
            int resultCount,
            int warmupPeriod)
        {
            int lookback =
                Math.Max(
                    0,
                    lookbackBars);

            return
                IsIndexedSeriesReady(
                    index,
                    resultCount,
                    warmupPeriod) &&
                IsIndexedSeriesReady(
                    index - lookback,
                    resultCount,
                    warmupPeriod);
        }

        internal static bool IsFinitePositive(double value)
        {
            return NumericGuards.IsFinitePositive(value);
        }

        internal static bool IsFiniteBounded(
            double value,
            double minimum,
            double maximum)
        {
            return
                NumericGuards.IsFiniteValue(value) &&
                value >= minimum &&
                value <= maximum;
        }

        internal static bool IsFrameReady(
            int index,
            int atrResultCount,
            int rsiResultCount,
            int adxResultCount,
            int fastEmaResultCount,
            int slowEmaResultCount,
            int atrPeriod,
            int rsiPeriod,
            int adxPeriod,
            int fastEmaPeriod,
            int slowEmaPeriod,
            double atr,
            double rsi,
            double adx,
            double fastEma,
            double slowEma)
        {
            return
                IsIndexedSeriesReady(
                    index,
                    atrResultCount,
                    atrPeriod) &&
                IsIndexedSeriesReady(
                    index,
                    rsiResultCount,
                    rsiPeriod) &&
                IsIndexedSeriesReady(
                    index,
                    adxResultCount,
                    adxPeriod) &&
                IsIndexedSeriesReady(
                    index,
                    fastEmaResultCount,
                    fastEmaPeriod) &&
                IsIndexedSeriesReady(
                    index,
                    slowEmaResultCount,
                    slowEmaPeriod) &&
                IsFinitePositive(atr) &&
                IsFiniteBounded(rsi, 0, 100) &&
                IsFiniteBounded(adx, 0, 100) &&
                IsFinitePositive(fastEma) &&
                IsFinitePositive(slowEma);
        }
    }
}
