using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical timeframe-agreement owner.
    /// A frame earns only quality-weighted alignment credit; weak evidence
    /// cannot masquerade as strong MTF agreement.
    /// </summary>
    internal static class TimeframeAgreementRule
    {
        internal static int Calculate(
            int direction,
            int[] directions,
            int[] qualities,
            int[] actualIndices,
            int[] expectedIndices,
            double[] weights,
            bool[] enabled)
        {
            if (direction != 1 && direction != -1)
                return 0;

            int count =
                Math.Min(
                    directions == null ? 0 : directions.Length,
                    Math.Min(
                        qualities == null ? 0 : qualities.Length,
                        Math.Min(
                            actualIndices == null ? 0 : actualIndices.Length,
                            Math.Min(
                                expectedIndices == null ? 0 : expectedIndices.Length,
                                Math.Min(
                                    weights == null ? 0 : weights.Length,
                                    enabled == null ? 0 : enabled.Length)))));

            double totalWeight = 0;
            double alignedWeight = 0;

            for (int i = 0; i < count; i++)
            {
                if (!enabled[i] ||
                    weights[i] <= 0 ||
                    qualities[i] <= 0 ||
                    expectedIndices[i] < 0 ||
                    actualIndices[i] != expectedIndices[i])
                    continue;

                double qualityFactor =
                    Math.Max(
                        0.0,
                        Math.Min(
                            1.0,
                            qualities[i] / 100.0));

                totalWeight += weights[i];

                if (directions[i] == direction)
                    alignedWeight +=
                        weights[i] * qualityFactor;
            }

            return
                totalWeight <= 0
                    ? 0
                    : NumericGuards.ClampInt(
                        (int)Math.Round(
                            100.0 * alignedWeight / totalWeight),
                        0,
                        100);
        }
    }
}
