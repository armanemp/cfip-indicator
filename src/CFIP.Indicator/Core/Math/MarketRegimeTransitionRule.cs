namespace cAlgo
{
    internal static class MarketRegimeTransitionRule
    {
        public const string Unknown = "UNKNOWN";
        public const string Initial = "INITIAL";
        public const string Stable = "STABLE";
        public const string Changed = "CHANGED";

        public static string ClassifyTransition(
            string previousRegime,
            string currentRegime)
        {
            string previous =
                MarketRegimeIdentity.NormalizeMarketRegime(
                    previousRegime);

            string current =
                MarketRegimeIdentity.NormalizeMarketRegime(
                    currentRegime);

            if (current == MarketRegimeIdentity.Unknown)
                return Unknown;

            if (previous == MarketRegimeIdentity.Unknown)
                return Initial;

            return previous == current
                ? Stable
                : Changed;
        }

        public static bool IsChanged(
            string previousRegime,
            string currentRegime)
        {
            return ClassifyTransition(
                previousRegime,
                currentRegime) == Changed;
        }
    }
}
