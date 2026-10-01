using System;

namespace cAlgo
{
    internal static class PremiumDiscountBiasRule
    {
        public static int Evaluate(
            double close,
            double high,
            double low)
        {
            if (double.IsNaN(close) ||
                double.IsInfinity(close) ||
                double.IsNaN(high) ||
                double.IsInfinity(high) ||
                double.IsNaN(low) ||
                double.IsInfinity(low) ||
                high <= low)
                return 0;

            double midpoint =
                (high + low) * 0.5;

            if (close < midpoint)
                return 1;

            if (close > midpoint)
                return -1;

            return 0;
        }
    }
}
