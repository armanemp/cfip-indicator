namespace cAlgo
{
    internal static class PanelFrameDirectionRule
    {
        internal static int ResolveDisplayDirection(
            int resolvedDirection,
            int bullScore,
            int bearScore,
            bool trendBull,
            bool trendBear)
        {
            if (resolvedDirection == 1 || resolvedDirection == -1)
                return resolvedDirection;

            if (trendBull && bullScore >= bearScore)
                return 1;

            if (trendBear && bearScore >= bullScore)
                return -1;

            if (bullScore > bearScore)
                return 1;

            if (bearScore > bullScore)
                return -1;

            return 0;
        }

        internal static string ResolveLabel(
            int resolvedDirection,
            int displayDirection)
        {
            if (resolvedDirection == 1)
                return "BUY";

            if (resolvedDirection == -1)
                return "SELL";

            if (displayDirection == 1)
                return "BULL BIAS";

            if (displayDirection == -1)
                return "BEAR BIAS";

            return "NEUTRAL";
        }
    }
}
