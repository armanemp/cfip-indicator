using System;

namespace cAlgo
{
    internal sealed class DecisionFrameContributionCalculator
    {
        public DecisionFrameContribution Calculate(
            Frame frame,
            double weight)
        {
            if (frame == null ||
                frame.Quality <= 0 ||
                weight <= 0)
            {
                return new DecisionFrameContribution(
                    0,
                    0,
                    0);
            }

            double totalScore =
                Math.Max(
                    1.0,
                    frame.BullScore + frame.BearScore);

            double buyShare =
                100.0 *
                Math.Max(0, frame.BullScore) /
                totalScore;

            double sellShare =
                100.0 -
                buyShare;

            double qualityFactor =
                Math.Max(
                    0.0,
                    Math.Min(
                        1.0,
                        frame.Quality / 100.0));

            double scale =
                Math.Max(
                    0.0,
                    weight / 10.0);

            return new DecisionFrameContribution(
                buyShare * qualityFactor * scale,
                sellShare * qualityFactor * scale,
                frame.Direction == 0 ? 0 : 1);
        }
    }
}
