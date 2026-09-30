using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical deterministic Order Block quality scoring owner.
    /// Each evidence component is scored independently so the caller cannot
    /// accidentally merge or double-count structural evidence.
    /// </summary>
    internal static class OrderBlockQualityRule
    {
        private const int BaseQuality = 54;
        private const int MaxAgePenalty = 10;

        public static int Calculate(
            double bodyRatio,
            double impulseRatio,
            double remainingRatio,
            int ageBars,
            bool displacement,
            bool structureBreak,
            bool liquiditySweep,
            bool fvgConfluence,
            bool partiallyMitigated)
        {
            if (!Finite(bodyRatio) ||
                !Finite(impulseRatio) ||
                !Finite(remainingRatio) ||
                ageBars < 0)
                return 0;

            double boundedRemaining =
                Math.Min(
                    1.0,
                    Math.Max(
                        0,
                        remainingRatio));

            int quality = BaseQuality;

            if (displacement)
                quality += 14;

            if (structureBreak)
                quality += 13;

            if (liquiditySweep)
                quality += 8;

            if (fvgConfluence)
                quality += 8;

            quality +=
                (int)Math.Round(
                    8 *
                    boundedRemaining);

            if (bodyRatio <= 0.25)
                quality -= 4;
            else if (bodyRatio >= 0.65)
                quality += 3;

            if (impulseRatio >= 1.50)
                quality += 4;
            else if (impulseRatio >= 1.00)
                quality += 2;

            quality -=
                Math.Min(
                    MaxAgePenalty,
                    ageBars / 6);

            if (partiallyMitigated)
            {
                quality -=
                    (int)Math.Round(
                        10 *
                        (1.0 -
                         boundedRemaining));
            }

            return Clamp(
                quality,
                0,
                100);
        }

        private static int Clamp(
            int value,
            int min,
            int max)
        {
            return
                Math.Max(
                    min,
                    Math.Min(
                        max,
                        value));
        }

        private static bool Finite(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}
