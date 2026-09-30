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
        public const double MinimumBodyPips = 0.10;
        public const double MinimumBodyRangeFraction = 0.05;
        public const double WickToBodyRatio = 1.25;
        public const double MinimumWickRangeFraction = 0.20;

        public static bool IsRejection(
            double open,
            double close,
            double high,
            double low,
            int direction,
            double pipSize)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFiniteRejectionInput(open) ||
                !IsFiniteRejectionInput(close) ||
                !IsFiniteRejectionInput(high) ||
                !IsFiniteRejectionInput(low) ||
                !IsFinitePositivePipInput(pipSize) ||
                high < low)
                return false;

            double range =
                Math.Max(
                    pipSize,
                    high - low);

            double body =
                Math.Abs(
                    close - open);

            if (!HasMeaningfulBody(
                    body,
                    range,
                    pipSize))
                return false;

            if (direction == 1)
            {
                double lowerWick =
                    Math.Min(open, close) - low;

                return lowerWick > body * WickToBodyRatio &&
                       lowerWick / range > MinimumWickRangeFraction;
            }

            double upperWick =
                high - Math.Max(open, close);

            return upperWick > body * WickToBodyRatio &&
                   upperWick / range > MinimumWickRangeFraction;
        }

        public static bool IsDoji(
            double open,
            double close,
            double high,
            double low,
            double pipSize)
        {
            if (!IsFiniteRejectionInput(open) ||
                !IsFiniteRejectionInput(close) ||
                !IsFiniteRejectionInput(high) ||
                !IsFiniteRejectionInput(low) ||
                !IsFinitePositivePipInput(pipSize) ||
                high < low)
                return true;

            double range =
                Math.Max(
                    pipSize,
                    high - low);

            return
                !HasMeaningfulBody(
                    Math.Abs(close - open),
                    range,
                    pipSize);
        }

        public static double ResolveMinimumMeaningfulBody(
            double range,
            double pipSize)
        {
            if (!IsFinitePositivePipInput(pipSize) ||
                double.IsNaN(range) ||
                double.IsInfinity(range) ||
                range < 0)
                return double.PositiveInfinity;

            return Math.Max(
                pipSize * MinimumBodyPips,
                range * MinimumBodyRangeFraction);
        }

        private static bool HasMeaningfulBody(
            double body,
            double range,
            double pipSize)
        {
            return
                body >= ResolveMinimumMeaningfulBody(
                    range,
                    pipSize) &&
                !double.IsNaN(body) &&
                !double.IsInfinity(body);
        }

        private static bool IsFiniteRejectionInput(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static bool IsFinitePositivePipInput(double value)
        {
            return IsFiniteRejectionInput(value) && value > 0;
        }
    }
}
