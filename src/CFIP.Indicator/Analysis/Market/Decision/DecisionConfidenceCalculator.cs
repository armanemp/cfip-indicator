using System;

namespace cAlgo
{
    internal sealed class DecisionConfidenceCalculator
    {
        public int Calculate(
            int strongestShare,
            int timeframeAgreement,
            int smartQuality,
            int calibrationAdjustment,
            int higherTimeframePenalty)
        {
            int baseConfidence =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        strongestShare * 0.45 +
                        timeframeAgreement * 0.25 +
                        smartQuality * 0.30),
                    0,
                    100);

            return NumericGuards.ClampInt(
                baseConfidence +
                calibrationAdjustment -
                Math.Max(0, higherTimeframePenalty),
                0,
                100);
        }
    }
}