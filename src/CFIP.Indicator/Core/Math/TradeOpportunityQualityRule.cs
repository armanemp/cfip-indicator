using System;

namespace cAlgo
{
    internal static class TradeOpportunityQualityRule
    {
        internal static int CalculateRankBonus(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return 0;

            int bonus = 0;

            int evidenceGroups =
                Math.Max(
                    0,
                    candidate.IndependentEvidenceGroupCount);

            if (evidenceGroups >= 3)
                bonus += 8;
            else if (evidenceGroups >= 2)
                bonus += 5;
            else if (evidenceGroups >= 1)
                bonus += 2;

            if (candidate.IndicatorIndependentEvidenceGroupCount >= 2)
                bonus += 2;

            if (candidate.SourceFvgObConfluence)
                bonus += 5;

            if (candidate.PrimaryLocationConfluence)
                bonus += 4;

            if (candidate.PrimaryLocationQuality >= 80)
                bonus += 3;
            else if (candidate.PrimaryLocationQuality >= 70)
                bonus += 2;

            if (candidate.WaveTrendQuality >= 80)
                bonus += 3;
            else if (candidate.WaveTrendQuality >= 70)
                bonus += 2;

            if (candidate.Tp1RR >= 4.0)
                bonus += 6;
            else if (candidate.Tp1RR >= 3.0)
                bonus += 4;
            else if (candidate.Tp1RR >= 2.0)
                bonus += 2;

            if (candidate.RewardDistanceAtr >= 2.0)
                bonus += 4;
            else if (candidate.RewardDistanceAtr >= 1.5)
                bonus += 2;

            if (IsFiniteNonNegativeQuality(candidate.EntryDistanceAtr))
            {
                if (candidate.EntryDistanceAtr <= 0.50)
                    bonus += 4;
                else if (candidate.EntryDistanceAtr <= 1.00)
                    bonus += 2;
                else if (candidate.EntryDistanceAtr > 1.50)
                    bonus -= 4;
            }

            if (!IsFinitePositiveQuality(candidate.Risk))
                bonus -= 8;

            if (!IsFinitePositiveQuality(candidate.Stop) ||
                !IsFinitePositiveQuality(candidate.Tp1))
                bonus -= 8;

            return Math.Max(
                -15,
                Math.Min(
                    30,
                    bonus));
        }

        internal static double CalculateExecutionPriorityScore(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return double.MinValue;

            double priority =
                Math.Max(0, Math.Min(100, candidate.Quality)) * 10.0;

            priority +=
                CalculateRankBonus(candidate) * 4.0;

            int forecast =
                Math.Max(
                    -100,
                    Math.Min(
                        100,
                        candidate.ForecastAlignmentScore));

            priority += forecast * 2.0;

            int historical =
                Math.Max(
                    -20,
                    Math.Min(
                        20,
                        candidate.HistoricalSupportScore));

            if (candidate.HistoricalCalibrationSamples >= 3)
                priority += historical * 5.0;

            double rewardDistance =
                IsFinitePositiveQuality(candidate.RewardDistanceAtr)
                    ? Math.Min(3.0, candidate.RewardDistanceAtr)
                    : 0;

            priority += rewardDistance * 15.0;

            if (candidate.ActionableNow)
                priority += 80.0;
            else if (candidate.FutureOrderReady)
                priority += 20.0;

            return priority;
        }

        private static bool IsFinitePositiveQuality(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteNonNegativeQuality(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}