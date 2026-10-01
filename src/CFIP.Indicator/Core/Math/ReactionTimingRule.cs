namespace cAlgo
{
    internal static class ReactionTimingRule
    {
        internal static bool IsSeparatedObservationAndConfirmation(
            int liveIndex,
            int closedIndex)
        {
            return liveIndex >= 1 &&
                   closedIndex >= 0 &&
                   closedIndex < liveIndex;
        }
    }
}
