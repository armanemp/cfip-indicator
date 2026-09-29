using System;

namespace cAlgo
{
    internal readonly struct LocationEvidenceScore
    {
        public int Score { get; }
        public int Evidence { get; }
        public bool Confluence { get; }

        public LocationEvidenceScore(
            int score,
            int evidence,
            bool confluence)
        {
            Score = NumericGuards.ClampInt(score, 0, 40);
            Evidence = Math.Max(0, evidence);
            Confluence = confluence;
        }
    }

    internal static class LocationEvidenceRule
    {
        public static LocationEvidenceScore Evaluate(
            bool fvg,
            int fvgQuality,
            bool orderBlock,
            int orderBlockQuality,
            bool confluence)
        {
            if (!fvg && !orderBlock)
                return new LocationEvidenceScore(0, 0, false);

            int boundedFvgQuality =
                NumericGuards.ClampInt(
                    fvgQuality,
                    0,
                    100);

            int boundedOrderBlockQuality =
                NumericGuards.ClampInt(
                    orderBlockQuality,
                    0,
                    100);

            int score = 0;

            if (fvg)
            {
                score +=
                    6 +
                    Math.Min(
                        4,
                        boundedFvgQuality / 25);
            }

            if (orderBlock)
            {
                score +=
                    7 +
                    Math.Min(
                        5,
                        boundedOrderBlockQuality / 20);
            }

            if (confluence)
            {
                int minimumQuality =
                    Math.Min(
                        boundedFvgQuality,
                        boundedOrderBlockQuality);

                // OB+FVG is the strongest single location-quality feature.
                // Its synergy is intentionally bounded so it cannot replace
                // structure, liquidity, reward-path or risk validation.
                int synergy =
                    minimumQuality >= 85
                        ? 18
                        : minimumQuality >= 75
                            ? 16
                            : minimumQuality >= 60
                                ? 12
                                : 8;

                score += synergy;
            }

            return new LocationEvidenceScore(
                score,
                1,
                confluence);
        }
    }
}
