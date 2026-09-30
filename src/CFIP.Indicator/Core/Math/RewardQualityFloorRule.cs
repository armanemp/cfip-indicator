using System;

namespace cAlgo
{
    // Deterministic reward-quality heuristic used by the legacy smart gate.
    //
    // This is deliberately NOT named or presented as expected value:
    // SmartQuality is an analytical quality score, not a calibrated probability.
    // The calculation preserves the legacy monotonic quality/RR behavior while
    // making the semantic boundary explicit for future outcome-based calibration.
    internal static class RewardQualityFloorRule
    {
        public static double Calculate(
            int quality,
            double rewardRisk)
        {
            double boundedQuality =
                NumericGuards.ClampDouble(
                    quality / 100.0,
                    0.05,
                    0.95);

            double safeRewardRisk =
                NumericGuards.IsFiniteValue(rewardRisk)
                    ? Math.Max(0, rewardRisk)
                    : 0;

            return
                boundedQuality * safeRewardRisk -
                (1.0 - boundedQuality);
        }
    }
}
