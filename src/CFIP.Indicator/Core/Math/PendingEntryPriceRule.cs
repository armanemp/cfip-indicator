using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical conversion from a structural pending trigger to the
    /// executable-side pending price.
    /// </summary>
    internal static class PendingEntryPriceRule
    {
        public static double ForExecutableStop(
            int direction,
            double structuralTrigger,
            double spread,
            double tickSize)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFinitePositivePendingEntry(structuralTrigger))
                return 0;

            double safeSpread =
                Math.Max(
                    0,
                    IsFiniteNonNegativePendingEntry(spread)
                        ? spread
                        : 0);

            double safeTick =
                Math.Max(
                    0,
                    IsFiniteNonNegativePendingEntry(tickSize)
                        ? tickSize
                        : 0);

            double price =
                direction == 1
                    ? structuralTrigger + safeSpread
                    : structuralTrigger - safeSpread;

            if (!IsFinitePositivePendingEntry(price))
                return 0;

            return safeTick > 0
                ? Math.Round(price / safeTick) * safeTick
                : price;
        }

        private static bool IsFinitePositivePendingEntry(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }

        private static bool IsFiniteNonNegativePendingEntry(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value >= 0;
        }
    }
}
