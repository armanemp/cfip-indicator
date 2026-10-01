namespace cAlgo
{
    internal static class VwapBiasRule
    {
        public static bool IsDirectional(
            double close,
            double vwap,
            int direction)
        {
            if (direction != 1 &&
                direction != -1)
                return false;

            if (double.IsNaN(close) ||
                double.IsInfinity(close) ||
                double.IsNaN(vwap) ||
                double.IsInfinity(vwap))
                return false;

            return direction == 1
                ? close > vwap
                : close < vwap;
        }

        public static double AccumulatePriceVolume(
            double current,
            double typical,
            double volume)
        {
            if (double.IsNaN(current) ||
                double.IsInfinity(current) ||
                double.IsNaN(typical) ||
                double.IsInfinity(typical) ||
                double.IsNaN(volume) ||
                double.IsInfinity(volume) ||
                volume <= 0)
                return current;

            return current + typical * volume;
        }

        public static double AccumulateVolume(
            double current,
            double volume)
        {
            if (double.IsNaN(current) ||
                double.IsInfinity(current) ||
                double.IsNaN(volume) ||
                double.IsInfinity(volume) ||
                volume <= 0)
                return current;

            return current + volume;
        }
    }
}
