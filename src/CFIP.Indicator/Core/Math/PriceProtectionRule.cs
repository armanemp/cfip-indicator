namespace cAlgo
{
    internal static class PriceProtectionRule
    {
        public static bool ValidateStop(
            int direction,
            double entry,
            double stop,
            double minimumDistance)
        {
            if (!IsFinitePositivePrice(entry) ||
                !IsFinitePositivePrice(stop) ||
                !IsFiniteDistance(minimumDistance) ||
                minimumDistance < 0)
                return false;

            return direction == 1
                ? stop < entry - minimumDistance
                : direction == -1 &&
                  stop > entry + minimumDistance;
        }

        public static bool ValidateTarget(
            int direction,
            double entry,
            double target,
            double minimumDistance)
        {
            if (!IsFinitePositivePrice(entry) ||
                !IsFinitePositivePrice(target) ||
                !IsFiniteDistance(minimumDistance) ||
                minimumDistance < 0)
                return false;

            return direction == 1
                ? target > entry + minimumDistance
                : direction == -1 &&
                  target < entry - minimumDistance;
        }

        private static bool IsFinitePositivePrice(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }

        private static bool IsFiniteDistance(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
