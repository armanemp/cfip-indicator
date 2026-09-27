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
            int regimeQuality)
        {
            return NumericGuards.ClampInt(
                (int)Math.Round(
                    strongestShare * 0.28 +
                    timeframeAgreement * 0.23 +
                    Math.Min(100, independentEvidence * 10) * 0.20 +
                    Math.Min(100, structuralConfirmations * 16) * 0.17 +
                    regimeQuality * 0.12),
                0,
                100);
        }
    }
}