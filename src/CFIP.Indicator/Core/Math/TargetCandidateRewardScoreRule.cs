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
            double zoneBonus,
            string timeframe = null)
        {
            if (double.IsNaN(baseScore) ||
                double.IsInfinity(baseScore) ||
                double.IsNaN(rr) ||
                double.IsInfinity(rr) ||
                rr <= 0 ||
                double.IsNaN(requiredRr) ||
                double.IsInfinity(requiredRr) ||
                requiredRr < 0)
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

            if (htf &&
                stage > 0)
            {
                int timeframeRank =
                    ResolveHtfDepthRank(
                        timeframe);

                double htfDepthBonus =
                    Math.Min(
                        6.0,
                        Math.Max(0, htfBonus) *
                        0.05 *
                        timeframeRank *
                        stage) *
                    qualityMultiplier;

                score +=
                    Math.Max(
                        0,
                        htfDepthBonus);
            }

            return score;
        }

        private static int ResolveHtfDepthRank(
            string timeframe)
        {
            if (string.Equals(timeframe, "M15", StringComparison.OrdinalIgnoreCase))
                return 1;
            if (string.Equals(timeframe, "M30", StringComparison.OrdinalIgnoreCase))
                return 2;
            if (string.Equals(timeframe, "H1", StringComparison.OrdinalIgnoreCase))
                return 3;
            if (string.Equals(timeframe, "H4", StringComparison.OrdinalIgnoreCase))
                return 4;
            if (string.Equals(timeframe, "D1", StringComparison.OrdinalIgnoreCase))
                return 5;
            if (string.Equals(timeframe, "W1", StringComparison.OrdinalIgnoreCase))
                return 6;
            return 0;
        }
    }
}