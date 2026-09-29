using System.Collections.Generic;

namespace cAlgo
{
    internal static class AdaptiveOutcomeRiskPolicy
    {
        public static double Calculate(
            IList<OutcomeObservation> outcomes,
            double suitabilityMultiplier)
        {
            double baseMultiplier =
                NumericGuards.Clamp(
                    suitabilityMultiplier,
                    0.25,
                    1.0);

            if (outcomes == null ||
                outcomes.Count < 8)
                return baseMultiplier;

            int start =
                System.Math.Max(
                    0,
                    outcomes.Count - 12);

            int count = 0;
            int wins = 0;
            int currentLossStreak = 0;
            int maxLossStreak = 0;
            double totalR = 0;

            for (int i = start;
                 i < outcomes.Count;
                 i++)
            {
                OutcomeObservation item = outcomes[i];

                if (item == null)
                    continue;

                count++;

                if (item.Profitable)
                {
                    wins++;
                    currentLossStreak = 0;
                }
                else
                {
                    currentLossStreak++;
                    maxLossStreak =
                        System.Math.Max(
                            maxLossStreak,
                            currentLossStreak);
                }

                if (!double.IsNaN(item.RealizedR) &&
                    !double.IsInfinity(item.RealizedR))
                    totalR += item.RealizedR;
            }

            if (count < 8)
                return baseMultiplier;

            double winRate =
                (double)wins / count;

            double avgR =
                totalR / count;

            double penalty = 0;

            if (winRate < 0.50 &&
                avgR < 0)
                penalty = 0.18;
            else if (winRate < 0.58 &&
                     avgR < -0.10)
                penalty = 0.10;

            if (maxLossStreak >= 4)
                penalty =
                    System.Math.Max(
                        penalty,
                        0.10);

            return NumericGuards.Clamp(
                baseMultiplier * (1.0 - penalty),
                0.25,
                baseMultiplier);
        }
    }
}
