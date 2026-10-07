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
            int indicatorConfluenceQuality = 0,
            int indicatorConflict = 0,
            int independentEvidenceGroupCount = 0,
            int canonicalContextQuality = 0)
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
                NumericGuards.ClampInt(
                    retestQuality,
                    0,
                    100);

            int effectiveCanonicalContext =
                NumericGuards.ClampInt(
                    canonicalContextQuality,
                    0,
                    100);

            int diversityBonus =
                IndependentEvidenceDiversityRule.QualityBonus(
                    independentEvidenceGroupCount);

            // M15 is the canonical execution/decision context. It is therefore
            // a first-class quality dimension, not merely a late veto. This
            // prevents a lower-timeframe vote stack from making a marginal
            // M15 setup look executable.
            if (indicatorConfluenceQuality <= 0 &&
                indicatorConflict <= 0)
            {
                int baselineQuality =
                    (int)Math.Round(
                        NumericGuards.ClampInt(strongestShare, 0, 100) * 0.22 +
                        NumericGuards.ClampInt(timeframeAgreement, 0, 100) * 0.18 +
                        normalizedIndependentEvidence * 0.18 +
                        normalizedStructural * 0.14 +
                        NumericGuards.ClampInt(regimeQuality, 0, 100) * 0.08 +
                        effectiveRetestQuality * 0.05 +
                        effectiveCanonicalContext * 0.15 +
                        diversityBonus);

                return NumericGuards.ClampInt(
                    baselineQuality,
                    0,
                    100);
            }

            return NumericGuards.ClampInt(
                (int)Math.Round(
                    NumericGuards.ClampInt(strongestShare, 0, 100) * 0.20 +
                    NumericGuards.ClampInt(timeframeAgreement, 0, 100) * 0.16 +
                    normalizedIndependentEvidence * 0.16 +
                    normalizedStructural * 0.13 +
                    NumericGuards.ClampInt(regimeQuality, 0, 100) * 0.08 +
                    effectiveRetestQuality * 0.05 +
                    effectiveCanonicalContext * 0.10 +
                    NumericGuards.ClampInt(indicatorConfluenceQuality, 0, 100) * 0.12 +
                    diversityBonus) -
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
