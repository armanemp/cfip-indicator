using System;

namespace cAlgo
{
    internal static class LiveM5BiasRule
    {
        public static double Evaluate(
            double close,
            double fast,
            double slow,
            int direction)
        {
            if (direction != 1 &&
                direction != -1)
                return 0;

            if (double.IsNaN(close) ||
                double.IsInfinity(close) ||
                double.IsNaN(fast) ||
                double.IsInfinity(fast) ||
                double.IsNaN(slow) ||
                double.IsInfinity(slow))
                return 0;

            if (direction == 1 &&
                close > fast &&
                fast > slow)
                return 5;

            if (direction == -1 &&
                close < fast &&
                fast < slow)
                return 5;

            return 0;
        }
    }
}
