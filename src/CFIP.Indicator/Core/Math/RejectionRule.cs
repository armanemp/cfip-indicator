using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical closed-bar rejection geometry.
    /// Very small-body candles are treated as doji/indecision rather than
    /// rejection candles, preventing wick-only false rejection evidence.
    /// </summary>
    internal static class RejectionRule
    {
        public static bool IsRejection(
            double open,
            double close,
            double high,
            double low,
            int direction,
            double pipSize)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFiniteCandleValue(open) ||
                !IsFiniteCandleValue(close) ||
                !IsFiniteCandleValue(high) ||
                !IsFiniteCandleValue(low) ||
                !IsFinitePositivePipValue(pipSize) ||
                high < low)
                return false;

            double range =
                Math.Max(
                    pipSize,
                    high - low);

            double body =
                Math.Abs(
                    close - open);

            double minimumBody =
                Math.Max(
                    pipSize * 0.10,
                    range * 0.05);

            if (body < minimumBody)
                return false;

            if (direction == 1)
            {
                double lowerWick =
                    Math.Min(open, close) - low;

                return lowerWick > body * 1.25 &&
                       lowerWick / range > 0.20;
            }

            double upperWick =
                high - Math.Max(open, close);

            return upperWick > body * 1.25 &&
                   upperWick / range > 0.20;
        }

        public static bool IsDoji(
            double open,
            double close,
            double high,
            double low,
            double pipSize)
        {
            if (!IsFiniteCandleValue(open) ||
                !IsFiniteCandleValue(close) ||
                !IsFiniteCandleValue(high) ||
                !IsFiniteCandleValue(low) ||
                !IsFinitePositivePipValue(pipSize) ||
                high < low)
                return true;

            double range =
                Math.Max(
                    pipSize,
                    high - low);

            return
                Math.Abs(close - open) <
                Math.Max(
                    pipSize * 0.10,
                    range * 0.05);
        }

        private static bool IsFiniteCandleValue(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static bool IsFinitePositivePipValue(double value)
        {
            return IsFiniteCandleValue(value) && value > 0;
        }
    }
}
