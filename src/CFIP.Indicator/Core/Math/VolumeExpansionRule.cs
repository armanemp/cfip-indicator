using System;

namespace cAlgo
{
    internal static class VolumeExpansionRule
    {
        public static bool IsExpanded(
            bool directional,
            bool priceResult,
            double currentVolume,
            double averageVolume,
            double expansionRatio)
        {
            if (!directional ||
                !priceResult ||
                !IsFiniteNonNegative(currentVolume) ||
                !IsFinitePositive(averageVolume))
                return false;

            double ratio =
                Math.Max(
                    1.0,
                    expansionRatio);

            return currentVolume >=
                   averageVolume * ratio;
        }

        private static bool IsFinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value >= 0;
        }
    }
}
