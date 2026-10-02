using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical target reward scoring. Minimum RR is a validity floor; reward
    /// expansion above that floor is preferred only in proportion to source quality.
    /// </summary>
    internal static class TargetCandidateRewardScoreRule
    {
        public static double Calculate(
            double baseScore,
            double rr,
            double requiredRr,
            double nearestBias,
            double normalizedDistance,
            bool htf,
            bool liquidity,
            bool zone,
            int hits,
            int stage,
            double htfBonus,
            double liquidityBonus,
            double zoneBonus)
        {
            if (!IsFinite(baseScore) ||
                !IsFinitePositive(rr) ||
                !IsFiniteNonNegative(requiredRr))
                return double.MinValue;

            double rewardExpansionRr =
                Math.Max(
                    0,
                    rr - requiredRr);

            double qualityMultiplier =
                Math.Max(
                    0.35,
                    Math.Min(
                        1.0,
                        Math.Max(0, baseScore) / 100.0));

            double rewardExpansionBonus =
                Math.Min(
                    20.0,
                    rewardExpansionRr * 4.0) *
                qualityMultiplier;

            double nearestPracticalityBonus =
                Math.Max(0, nearestBias) /
                (1.0 +
                 Math.Max(
                     0,
                     normalizedDistance)) *
                3.0;

            double score =
                Math.Max(
                    0,
                    baseScore) +
                rewardExpansionBonus +
                nearestPracticalityBonus;

            if (htf)
                score +=
                    Math.Max(
                        0,
                        htfBonus);

            if (liquidity)
                score +=
                    Math.Max(
                        0,
                        liquidityBonus);

            if (zone)
                score +=
                    Math.Max(
                        0,
                        zoneBonus);

            score +=
                Math.Min(
                    20,
                    Math.Max(
                        1,
                        hits) *
                    2);

            score +=
                Math.Max(
                    0,
                    stage) *
                (htf
                    ? Math.Max(0, htfBonus) * 0.35
                    : 2.0);

            return score;
        }

        private static bool IsFinite(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }

        private static bool IsFinitePositive(double value)
        {
            return
                IsFinite(value) &&
                value > 0;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return
                IsFinite(value) &&
                value >= 0;
        }
    }
}
