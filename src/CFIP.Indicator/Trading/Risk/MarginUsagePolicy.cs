namespace cAlgo
{
    internal static class MarginUsagePolicy
    {
        public static double CalculateAllowedPercent(
            double maximumUsagePercent,
            double bufferPercent)
        {
            return NumericGuards.Clamp(
                maximumUsagePercent -
                NumericGuards.Clamp(
                    bufferPercent,
                    0,
                    40),
                10,
                100);
        }
    }
}
