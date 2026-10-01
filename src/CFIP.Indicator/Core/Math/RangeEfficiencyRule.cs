using System;

namespace cAlgo
{
    internal static class RangeEfficiencyRule
    {
        public static int ResolveFirstCloseIndex(
            int index,
            int period)
        {
            if (index < 0 ||
                period < 1)
                return -1;

            return index - period;
        }

        public static double Evaluate(
            double netMove,
            double absolutePath)
        {
            if (!IsFiniteNonNegative(netMove) ||
                !IsFiniteNonNegative(absolutePath) ||
                absolutePath <= 0)
                return 0;

            return Math.Max(
                0,
                Math.Min(
                    1,
                    netMove / absolutePath));
        }

        public static bool HasEnoughHistory(
            int index,
            int period)
        {
            return
                index >= Math.Max(1, period);
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value >= 0;
        }
    }
}
