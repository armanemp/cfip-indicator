using System;

namespace cAlgo
{
    /// <summary>
    /// Filters opportunities whose expected TP1 travel is too small to be
    /// operationally meaningful relative to current M5 volatility.
    /// This is deliberately independent of position sizing: small risk size
    /// must not be mistaken for a small-quality trade.
    /// </summary>
    internal static class OpportunityMagnitudeRule
    {
        public static bool IsMeaningful(
            string regime,
            double targetDistanceAtr)
        {
            if (double.IsNaN(targetDistanceAtr) ||
                double.IsInfinity(targetDistanceAtr) ||
                targetDistanceAtr <= 0)
                return false;

            double minimumAtr =
                string.Equals(regime, "COMPRESSION", StringComparison.OrdinalIgnoreCase)
                    ? 0.70
                    : string.Equals(regime, "RANGE", StringComparison.OrdinalIgnoreCase)
                        ? 0.65
                        : string.Equals(regime, "EXPANSION", StringComparison.OrdinalIgnoreCase)
                            ? 0.30
                            : 0.40;

            return targetDistanceAtr >= minimumAtr;
        }
    }
}
