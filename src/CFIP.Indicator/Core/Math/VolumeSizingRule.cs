namespace cAlgo
{
    /// <summary>
    /// Platform-neutral volume-input and normalized-volume invariants.
    /// Broker-specific normalization remains in the Indicator until cBot cutover.
    /// </summary>
    internal static class VolumeSizingRule
    {
        internal static bool IsValidStopPips(double stopPips)
        {
            return !double.IsNaN(stopPips) &&
                   !double.IsInfinity(stopPips) &&
                   stopPips > 0;
        }

        internal static bool IsValidRiskInput(double equity, double riskPercent)
        {
            return IsFinitePositive(equity) &&
                   IsFinitePositive(riskPercent);
        }

        internal static bool IsValidNormalizedVolume(
            double volume,
            double minimumVolume,
            double maximumVolume)
        {
            return IsFinitePositive(volume) &&
                   IsFinitePositive(minimumVolume) &&
                   IsFinitePositive(maximumVolume) &&
                   minimumVolume <= maximumVolume &&
                   volume >= minimumVolume &&
                   volume <= maximumVolume;
        }

        private static bool IsFinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}
