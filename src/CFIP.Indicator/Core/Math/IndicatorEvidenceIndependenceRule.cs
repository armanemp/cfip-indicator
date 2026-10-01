using System;

namespace cAlgo
{
    /// <summary>
    /// Diagnostic/provenance owner for correlated indicator measurements.
    /// Trend, momentum and context measurements form three indicator groups.
    /// Divergence remains a modifier and aggregate OSS consensus is not an
    /// additional independent group. This does not change score or thresholds.
    /// </summary>
    internal static class IndicatorEvidenceIndependenceRule
    {
        internal static int CountIndicatorGroups(IndicatorEvidenceFusionInput input)
        {
            int groups = 0;
            bool trend = input.TrendBull || input.TrendBear ||
                          IsDirectionalDmi(input) ||
                          Math.Abs(input.EmaSlopeAtr) >= 0.08;
            if (trend) groups++;

            bool momentum = input.MomentumBull || input.MomentumBear ||
                            (input.UseMacd && (input.MacdBull || input.MacdBear)) ||
                            input.Rsi >= 55 || input.Rsi <= 45 ||
                            (WaveTrendEvidenceRule.MeetsMinimumQuality(
                                 input.WaveTrendQuality,
                                 input.MinimumWaveTrendQuality) &&
                             (input.WaveTrendDirection != 0 ||
                              input.WaveTrendBullCross || input.WaveTrendBearCross ||
                              input.WaveTrendOversold || input.WaveTrendOverbought));
            if (momentum) groups++;

            bool context = (input.UseVwap && (input.VwapBull || input.VwapBear)) ||
                           (input.UseVolume && (input.VolumeBull || input.VolumeBear)) ||
                           (input.UseHealthyVolatility &&
                            (input.VolatilityBull || input.VolatilityBear));
            if (context) groups++;

            return groups;
        }

        private static bool IsDirectionalDmi(IndicatorEvidenceFusionInput input)
        {
            return input.Adx >= input.AdxMinimum && input.DmiBias != 0;
        }
    }
}