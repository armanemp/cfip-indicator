using System;

namespace cAlgo
{
    internal static class ManagedStopProtectionRule
    {
        public static bool Validate(
            int direction,
            double entry,
            double market,
            double stop,
            double minimumDistance)
        {
            if ((direction != 1 && direction != -1) ||
                !IsPositiveFinitePrice(entry) ||
                !IsPositiveFinitePrice(market) ||
                !IsPositiveFinitePrice(stop) ||
                !IsFiniteNonNegative(minimumDistance))
                return false;

            if (direction == 1)
                return stop < market - minimumDistance;

            return stop > market + minimumDistance;
        }

        private static bool IsPositiveFinitePrice(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}