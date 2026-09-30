using System;

namespace cAlgo
{
    internal static class FrameRegimeResolutionRule
    {
        public const string Unknown = "UNKNOWN";

        public static string ResolveFrameSnapshotRegime(
            MarketRegimeSnapshot snapshot)
        {
            return NormalizeFrameRegimeValue(
                snapshot == null
                    ? null
                    : snapshot.Regime);
        }

        public static string NormalizeFrameRegimeValue(
            string regime)
        {
            if (string.IsNullOrWhiteSpace(regime))
                return Unknown;

            switch (regime.Trim().ToUpperInvariant())
            {
                case "TREND":
                    return "TREND";
                case "EXPANSION":
                    return "EXPANSION";
                case "RANGE":
                    return "RANGE";
                case "TRANSITION":
                    return "TRANSITION";
                case "HIGH_VOLATILITY":
                    return "HIGH_VOLATILITY";
                case "COMPRESSION":
                    return "COMPRESSION";
                default:
                    return Unknown;
            }
        }

        public static bool IsNeutral(
            string regime)
        {
            return NormalizeFrameRegimeValue(regime) == Unknown;
        }
    }
}
