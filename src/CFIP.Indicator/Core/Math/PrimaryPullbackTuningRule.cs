namespace cAlgo
{
    /// <summary>
    /// Allows the entry-time M5 frame to remain neutral during a genuine
    /// primary-timeframe pullback. This does not authorize a trade by itself;
    /// actionability still requires the normal execution-zone, RR, trigger and
    /// final quality checks.
    /// </summary>
    internal static class PrimaryPullbackTuningRule
    {
        public static bool AllowsNeutralM5(
            int selectedDirection,
            int m5Direction,
            int m15Direction,
            int h1Direction,
            int m15Quality,
            int h1Quality,
            int minimumPrimaryQuality,
            bool topDownEligible)
        {
            if (!topDownEligible ||
                (selectedDirection != 1 &&
                 selectedDirection != -1) ||
                m5Direction != 0 ||
                m15Direction != selectedDirection ||
                h1Direction != selectedDirection)
                return false;

            int floor =
                NumericGuards.ClampInt(
                    minimumPrimaryQuality,
                    0,
                    100);

            return m15Quality >= floor &&
                   h1Quality >= floor;
        }
    }
}
