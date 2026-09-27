// ============================================================================
// CFIP Indicator — NumericGuards.cs
// Platform-neutral numeric invariants.
// ============================================================================

using System;

namespace cAlgo
{
    internal static class NumericGuards
    {
        internal static bool IsFiniteValue(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }

        internal static bool IsFinitePositive(double value)
        {
            return
                IsFiniteValue(value) &&
                value > 0;
        }

        internal static double SafePositive(double value)
        {
            return
                IsFinitePositive(value)
                    ? value
                    : 0;
        }

        internal static double ClampDouble(
            double value,
            double min,
            double max)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
                return min;

            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        internal static double Clamp(
            double value,
            double min,
            double max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        internal static int ClampInt(
            int value,
            int min,
            int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}
