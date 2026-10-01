namespace cAlgo
{
    internal static class MicroReactionSafetyRule
    {
        internal static bool IsClosedBarSafe(
            int scenarioClosedM5,
            int reactionConfirmedM5,
            int reactionDirection,
            int confirmedDirection,
            int confirmedQuality,
            bool closedBarConfirmed,
            int minimumQuality)
        {
            if (scenarioClosedM5 < 0 ||
                reactionConfirmedM5 < 0 ||
                reactionDirection != 1 &&
                reactionDirection != -1 ||
                confirmedDirection != reactionDirection ||
                !closedBarConfirmed)
                return false;

            return
                reactionConfirmedM5 == scenarioClosedM5 &&
                confirmedQuality >=
                    System.Math.Max(0, minimumQuality);
        }
    }
}
