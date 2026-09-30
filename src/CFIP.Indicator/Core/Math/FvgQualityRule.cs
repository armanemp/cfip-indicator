using System;

namespace cAlgo
{
    /// <summary>
    /// Deterministic FVG quality model. It separates gap size, displacement,
    /// freshness, remaining geometry and contextual alignment instead of
    /// assigning a uniformly high base score to every candidate.
    /// </summary>
    internal static class FvgQualityRule
    {
        internal static int Calculate(
            double gapAtrRatio,
            double remainingRatio,
            int ageBars,
            int maximumAgeBars,
            double displacementAtrRatio,
            bool structuralAlignment,
            bool higherTimeframeAlignment,
            bool twoBarImbalance)
        {
            double gapScore =
                Normalize(gapAtrRatio, 0, 0.50) * 30.0;

            double displacementScore =
                Normalize(displacementAtrRatio, 0, 1.50) * 20.0;

            double freshnessScore =
                Normalize(
                    maximumAgeBars - Math.Max(0, ageBars),
                    0,
                    Math.Max(1, maximumAgeBars)) * 15.0;

            double remainingScore =
                Normalize(remainingRatio, 0, 1) * 15.0;

            double structureScore =
                structuralAlignment ? 10.0 : 0.0;

            double higherTimeframeScore =
                higherTimeframeAlignment ? 10.0 : 0.0;

            double quality =
                gapScore +
                displacementScore +
                freshnessScore +
                remainingScore +
                structureScore +
                higherTimeframeScore;

            if (twoBarImbalance)
                quality -= 3.0;

            if (double.IsNaN(quality) ||
                double.IsInfinity(quality))
                return 0;

            return Math.Max(
                0,
                Math.Min(
                    100,
                    (int)Math.Round(quality)));
        }

        private static double Normalize(
            double value,
            double minimum,
            double maximum)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
                return 0;

            if (maximum <= minimum)
                return 0;

            return Math.Max(
                0,
                Math.Min(
                    1,
                    (value - minimum) /
                    (maximum - minimum)));
        }
    }
}
