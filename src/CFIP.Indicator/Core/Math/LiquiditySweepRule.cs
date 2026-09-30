using System;

namespace cAlgo
{
    /// <summary>
    /// Platform-neutral liquidity-level validity rules.
    /// A level is active when no prior closed bar has invalidated it from the
    /// protected side after the structural swing was confirmed.
    /// </summary>
    internal static class LiquiditySweepRule
    {
        public static bool IsActiveUnbrokenLevel(
            int direction,
            int confirmationIndex,
            int currentIndex,
            double level,
            double tolerance,
            Func<int, double> closeAt)
        {
            if ((direction != 1 && direction != -1) ||
                confirmationIndex < 0 ||
                currentIndex <= confirmationIndex ||
                !FinitePositive(level) ||
                double.IsNaN(tolerance) ||
                double.IsInfinity(tolerance) ||
                tolerance < 0 ||
                closeAt == null)
                return false;

            for (int i = confirmationIndex + 1;
                 i < currentIndex;
                 i++)
            {
                double close = closeAt(i);

                if (!FinitePositive(close))
                    return false;

                bool invalidated =
                    direction == 1
                        ? close < level - tolerance
                        : close > level + tolerance;

                if (invalidated)
                    return false;
            }

            return true;
        }

        private static bool FinitePositive(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
