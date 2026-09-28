namespace cAlgo
{
    internal static class PriceProtectionRule
    {
        public static bool IsValidStop(
            int direction,
            double entry,
            double stop,
            double minimumDistance)
        {
            if (!IsFinitePositive(entry) ||
                !IsFinitePositive(stop) ||
                !IsFinite(minimumDistance) ||
                minimumDistance < 0)
                return false;

            return direction == 1
                ? stop < entry - minimumDistance
                : direction == -1 &&
                  stop > entry + minimumDistance;
        }

        public static bool IsValidTarget(
            int direction,
            double entry,
            double target,
            double minimumDistance)
        {
            if (!IsFinitePositive(entry) ||
                !IsFinitePositive(target) ||
                !IsFinite(minimumDistance) ||
                minimumDistance < 0)
                return false;

            return direction == 1
                ? target > entry + minimumDistance
                : direction == -1 &&
                  target < entry - minimumDistance;
        }

        private static bool IsFinitePositive(double value)
        {
            return IsFinite(value) && value > 0;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
