using System;

namespace cAlgo
{
    internal sealed class DecisionQualityCalculator
    {
        public int Calculate(
            int strongestShare,
            DecisionEvidenceSnapshot evidence)
        {
            if (evidence == null)
                throw new ArgumentNullException(nameof(evidence));

            return NumericGuards.ClampInt(
                (int)Math.Round(
                    strongestShare * 0.28 +
                    evidence.TimeframeAgreement * 0.23 +
                    Math.Min(100, evidence.IndependentEvidence * 10) * 0.20 +
                    Math.Min(100, evidence.StructuralConfirmations * 16) * 0.17 +
                    evidence.RegimeQuality * 0.12),
                0,
                100);
        }
    }
}