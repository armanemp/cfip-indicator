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
            // Compatibility overload for isolated decision-contract tests and
            // callers that already provide a strong evidence context.
            return Calculate(
                bullScore,
                bearScore,
                quality,
                weight,
                4);
        }

        public DecisionFrameContribution Calculate(
            double bullScore,
            double bearScore,
            int quality,
            double weight,
            int evidence)
        {
            if (quality <= 0 ||
                weight <= 0)
            {
                return new DecisionFrameContribution(0, 0, 0);
            }

            double boundedBull =
                Math.Max(0.0, bullScore);
            double boundedBear =
                Math.Max(0.0, bearScore);

            double totalScore =
                Math.Max(
                    1.0,
                    boundedBull + boundedBear);

            double rawBuyShare =
                100.0 *
                boundedBull /
                totalScore;

            double rawSellShare =
                100.0 -
                rawBuyShare;

            // A frame must earn directional influence through absolute evidence
            // strength. Relative dominance alone is not enough: a weak 36-vs-4
            // frame must not be treated like a genuinely strong 90-vs-10 frame.
            double scoreStrength =
                0.55 +
                0.45 *
                NumericGuards.Clamp(
                    (totalScore - 30.0) / 40.0,
                    0.0,
                    1.0);

            // Evidence is intentionally a bounded modulation, not a hard gate.
            // This preserves strong single-event opportunities while preventing
            // low-coverage frames from dominating the whole MTF consensus.
            double evidenceStrength;

            if (evidence <= 0)
                evidenceStrength = 0.65;
            else if (evidence == 1)
                evidenceStrength = 0.78;
            else if (evidence == 2)
                evidenceStrength = 0.90;
            else
                evidenceStrength = 1.0;

            double directionalStrength =
                NumericGuards.Clamp(
                    scoreStrength * evidenceStrength,
                    0.25,
                    1.0);

            double buyShare =
                50.0 +
                (rawBuyShare - 50.0) *
                directionalStrength;

            double sellShare =
                50.0 +
                (rawSellShare - 50.0) *
                directionalStrength;

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
