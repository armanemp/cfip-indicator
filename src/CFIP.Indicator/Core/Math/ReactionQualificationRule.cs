namespace cAlgo
{
    internal static class ReactionQualificationRule
    {
        public static int ResolveDirection(
            int bullishQuality,
            int bearishQuality)
        {
            if (bullishQuality > bearishQuality)
                return 1;

            if (bearishQuality > bullishQuality)
                return -1;

            return 0;
        }

        public static bool HasPriorCounterMove(
            int direction,
            double earlierClose,
            double previousOpen,
            double previousClose)
        {
            if (!IsFiniteReactionValue(
                    earlierClose) ||
                !IsFiniteReactionValue(
                    previousOpen) ||
                !IsFiniteReactionValue(
                    previousClose))
                return false;

            if (direction == 1)
            {
                return previousClose < previousOpen ||
                       previousClose < earlierClose;
            }

            if (direction == -1)
            {
                return previousClose > previousOpen ||
                       previousClose > earlierClose;
            }

            return false;
        }

        public static bool HasSwingInteraction(
            int direction,
            double currentLow,
            double currentHigh,
            double swingLevel)
        {
            if (!IsFiniteReactionValue(
                    currentLow) ||
                !IsFiniteReactionValue(
                    currentHigh) ||
                !IsFiniteReactionValue(
                    swingLevel))
                return false;

            if (direction == 1)
                return currentLow <= swingLevel;

            if (direction == -1)
                return currentHigh >= swingLevel;

            return false;
        }

        public static bool HasQualifyingContext(
            bool priorCounterMove,
            bool zonePresent,
            bool qualifyingZone,
            bool swingInteraction)
        {
            if (zonePresent)
                return qualifyingZone;

            return priorCounterMove ||
                   swingInteraction;
        }

        public static bool IsClosedBarConfirmed(
            int candidateIndex,
            int closedIndex)
        {
            return candidateIndex >= 0 &&
                   closedIndex >= candidateIndex;
        }

        public static bool IsQualified(
            int direction,
            int quality,
            int evidence,
            bool qualifyingContext,
            int minimumQuality,
            int minimumEvidence,
            bool requireClosedBar,
            bool closedBarConfirmed)
        {
            if (direction == 0 ||
                quality < 0 ||
                evidence < 0 ||
                minimumQuality < 0 ||
                minimumEvidence < 0 ||
                !qualifyingContext)
                return false;

            if (requireClosedBar &&
                !closedBarConfirmed)
                return false;

            return quality >= minimumQuality &&
                   evidence >= minimumEvidence;
        }

        private static bool IsFiniteReactionValue(
            double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
