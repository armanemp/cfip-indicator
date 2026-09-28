using System;

namespace cAlgo
{
    internal sealed class DecisionFrameContributionCalculator
    {
        public DecisionFrameContribution Calculate(
            double bullScore,
            double bearScore,
            int quality,
            double weight)
        {
            if (quality <= 0 ||
                weight <= 0)
            {
                return new DecisionFrameContribution(0, 0, 0);
            }

            double totalScore =
                Math.Max(
                    1.0,
                    Math.Max(0.0, bullScore) +
                    Math.Max(0.0, bearScore));

            double buyShare =
                100.0 *
                Math.Max(0.0, bullScore) /
                totalScore;

            double sellShare =
                100.0 -
                buyShare;

            double qualityFactor =
                Math.Max(
                    0.0,
                    Math.Min(
                        1.0,
                        quality / 100.0));

            double scale =
                Math.Max(
                    0.0,
                    weight / 10.0);

            return new DecisionFrameContribution(
                buyShare * qualityFactor * scale,
                sellShare * qualityFactor * scale,
                0);
        }
    }
}
