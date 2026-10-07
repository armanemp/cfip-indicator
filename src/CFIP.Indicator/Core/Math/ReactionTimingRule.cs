using System;

namespace cAlgo
{
    internal static class ReactionTimingRule
    {
        internal const int LiveReactionRefreshIntervalMilliseconds = 100;

        internal static bool IsLiveRefreshDue(
            DateTime nowUtc,
            DateTime lastRefreshUtc,
            int liveIndex,
            int lastRefreshIndex)
        {
            if (liveIndex != lastRefreshIndex ||
                lastRefreshUtc == DateTime.MinValue)
                return true;

            return
                (nowUtc - lastRefreshUtc).TotalMilliseconds >=
                LiveReactionRefreshIntervalMilliseconds;
        }

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
