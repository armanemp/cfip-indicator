using System;

namespace cAlgo
{
    internal sealed class DecisionQualityCalculator
    {
        public int Calculate(
            int strongestShare,
            int timeframeAgreement,
            int independentEvidence,
            int structuralConfirmations,
            int regimeQuality,
            int retestQuality,
            int indicatorConfluenceQuality,
            int indicatorConflict)
        {
            double normalizedIndependentEvidence =
                Math.Min(
                    100.0,
                    Math.Max(
                        0.0,
                        independentEvidence * 12.5));

            double normalizedStructural =
                Math.Min(
                    100.0,
                    Math.Max(
                        0.0,
                        structuralConfirmations * 16.0));

            int effectiveRetestQuality =
                retestQuality <= 0
                    ? 50
                    : NumericGuards.ClampInt(
                        retestQuality,
                        0,
                        100);

            return NumericGuards.ClampInt(
                (int)Math.Round(
                    NumericGuards.ClampInt(strongestShare, 0, 100) * 0.22 +
                    NumericGuards.ClampInt(timeframeAgreement, 0, 100) * 0.18 +
                    normalizedIndependentEvidence * 0.18 +
                    normalizedStructural * 0.15 +
                    NumericGuards.ClampInt(regimeQuality, 0, 100) * 0.10 +
                    effectiveRetestQuality * 0.07 +
                    NumericGuards.ClampInt(indicatorConfluenceQuality, 0, 100) * 0.10) -
                Math.Min(
                    12,
                    Math.Max(
                        0,
                        indicatorConflict - 35) / 5),
                0,
                100);
        }
    }
}
