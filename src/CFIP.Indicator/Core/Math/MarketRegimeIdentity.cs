namespace cAlgo
{
    // Canonical market-regime vocabulary emitted by MarketRegimeClassifier.
    // This is an identity owner only; threshold/decision policy remains elsewhere.
    internal static class MarketRegimeIdentity
    {
        public const string Unknown = "UNKNOWN";
        public const string Trend = "TREND";
        public const string Expansion = "EXPANSION";
        public const string Range = "RANGE";
        public const string Transition = "TRANSITION";
        public const string HighVolatility = "HIGH_VOLATILITY";
        public const string Compression = "COMPRESSION";

        public static string NormalizeMarketRegime(
            string regime)
        {
            if (string.IsNullOrWhiteSpace(regime))
                return Unknown;

            switch (regime.Trim().ToUpperInvariant())
            {
                case Trend:
                    return Trend;
                case Expansion:
                    return Expansion;
                case Range:
                    return Range;
                case Transition:
                    return Transition;
                case HighVolatility:
                    return HighVolatility;
                case Compression:
                    return Compression;
                default:
                    return Unknown;
            }
        }

        public static bool IsKnown(
            string regime)
        {
            return Normalize(regime) != Unknown;
        }
    }
}
