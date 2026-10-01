using System;

namespace cAlgo
{
    internal static class OssQuoteWindowRule
    {
        internal static int ResolveFirstIndex(
            int closedIndex,
            int windowSize)
        {
            if (closedIndex < 0 ||
                windowSize < 1)
                return -1;

            return Math.Max(
                0,
                closedIndex - windowSize + 1);
        }

        internal static int ResolveWindowCount(
            int closedIndex,
            int windowSize)
        {
            int firstIndex =
                ResolveFirstIndex(
                    closedIndex,
                    windowSize);

            return firstIndex < 0
                ? 0
                : closedIndex - firstIndex + 1;
        }

        internal static bool RequiresRebuild(
            int requestedClosedIndex,
            int expectedFirstIndex,
            int cachedClosedIndex,
            int cachedFirstIndex)
        {
            if (requestedClosedIndex < 0 ||
                expectedFirstIndex < 0)
                return true;

            if (cachedClosedIndex < 0 ||
                cachedFirstIndex < 0)
                return true;

            if (requestedClosedIndex < cachedClosedIndex)
                return true;

            if (expectedFirstIndex < cachedFirstIndex ||
                expectedFirstIndex > cachedFirstIndex + 1)
                return true;

            return requestedClosedIndex >
                   cachedClosedIndex + 1;
        }

        internal static bool IsBoundedCount(
            int count,
            int maximum)
        {
            return
                maximum > 0 &&
                count >= 0 &&
                count <= maximum;
        }
    }
}
