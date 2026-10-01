using System;

namespace cAlgo
{
    internal static class ChoppinessIndexRule
    {
        public static int ResolveFirstBarIndex(
            int index,
            int period)
        {
            if (index < 0 ||
                period < 1)
                return -1;

            return index - period + 1;
        }

        public static double Evaluate(
            double trueRangeSum,
            double range,
            int period)
        {
            if (!IsChoppinessFinitePositive(trueRangeSum) ||
                !IsChoppinessFinitePositive(range) ||
                period < 2)
                return 100;

            double denominator =
                Math.Log10(period);

            if (!IsChoppinessFinitePositive(denominator))
                return 100;

            double value =
                100.0 *
                Math.Log10(
                    trueRangeSum / range) /
                denominator;

            if (double.IsNaN(value) ||
                double.IsInfinity(value))
                return 100;

            return Math.Max(
                0,
                Math.Min(
                    100,
                    value));
        }

        public static bool HasEnoughHistory(
            int index,
            int period)
        {
            return
                index >= Math.Max(1, period - 1);
        }

        private static bool IsChoppinessFinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}
