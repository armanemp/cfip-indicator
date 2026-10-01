using System;

namespace cAlgo
{
    internal static class OssIndicatorWarmupPolicy
    {
        // Fixed, non-public safety/performance boundary for the path-dependent
        // Skender v2 adapters. The current public parameter envelope fits inside
        // this window while keeping the historical prefix bounded.
        internal const int StableQuoteWindowSize = 768;

        internal const int RsiConvergenceMargin = 100;
        internal const int MacdConvergenceMargin = 250;
        internal const int SuperTrendConvergenceMargin = 250;
        internal const int ParabolicSarConvergenceBars = 100;

        internal static int RecommendedStableBars(
            int rsiPeriod,
            int macdSlowPeriod,
            int macdSignalPeriod,
            int superTrendPeriod)
        {
            int safeRsi = Math.Max(2, rsiPeriod);
            int safeMacdSlow = Math.Max(2, macdSlowPeriod);
            int safeMacdSignal = Math.Max(1, macdSignalPeriod);
            int safeSuperTrend = Math.Max(2, superTrendPeriod);

            int rsiBars = safeRsi * 10;
            int macdBars =
                safeMacdSlow +
                safeMacdSignal +
                MacdConvergenceMargin;
            int superTrendBars =
                safeSuperTrend +
                SuperTrendConvergenceMargin;

            return Math.Max(
                Math.Max(rsiBars, macdBars),
                Math.Max(
                    superTrendBars,
                    ParabolicSarConvergenceBars));
        }

        internal static bool FitsCurrentPublicParameterEnvelope(
            int rsiPeriod,
            int macdSlowPeriod,
            int macdSignalPeriod,
            int superTrendPeriod)
        {
            return RecommendedStableBars(
                       rsiPeriod,
                       macdSlowPeriod,
                       macdSignalPeriod,
                       superTrendPeriod) <=
                   StableQuoteWindowSize;
        }
    }
}