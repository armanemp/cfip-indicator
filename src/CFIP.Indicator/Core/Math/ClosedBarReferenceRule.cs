using System;

namespace cAlgo
{
    internal static class ClosedBarReferenceRule
    {
        internal static int ResolveClosedIndex(
            int count,
            DateTime reference,
            Func<int, DateTime> openTimeAt)
        {
            DateTime normalizedReference =
                CanonicalTimeRule.EnsureUtc(reference);

            if (count < 2 ||
                openTimeAt == null ||
                normalizedReference == DateTime.MinValue)
                return -1;

            if (reference < openTimeAt(0))
                return -1;

            int low = 0;
            int high = count - 1;
            int latestOpenIndex = -1;

            // Resolve the latest bar whose open time is at or before the
            // UTC reference. That bar is still open at the reference when
            // it opened exactly at, or immediately before, the reference.
            // Therefore the preceding bar is the latest fully closed bar.
            while (low <= high)
            {
                int middle = low + ((high - low) / 2);
                DateTime openTime =
                    CanonicalTimeRule.EnsureUtc(
                        openTimeAt(middle));

                if (openTime == DateTime.MinValue)
                    return -1;

                if (openTime <= normalizedReference)
                {
                    latestOpenIndex = middle;
                    low = middle + 1;
                }
                else
                {
                    high = middle - 1;
                }
            }

            int closedIndex =
                latestOpenIndex - 1;

            if (closedIndex < 0)
                return -1;

            return Math.Min(
                count - 2,
                closedIndex);
        }

        internal static bool IsFullyClosed(
            int count,
            int index,
            DateTime reference,
            Func<int, DateTime> openTimeAt)
        {
            DateTime normalizedReference =
                CanonicalTimeRule.EnsureUtc(reference);

            if (count < 2 ||
                openTimeAt == null ||
                index < 0 ||
                index >= count - 1 ||
                normalizedReference == DateTime.MinValue)
                return false;

            DateTime openTime =
                CanonicalTimeRule.EnsureUtc(
                    openTimeAt(index));

            DateTime nextOpenTime =
                CanonicalTimeRule.EnsureUtc(
                    openTimeAt(index + 1));

            return
                openTime != DateTime.MinValue &&
                nextOpenTime != DateTime.MinValue &&
                openTime <
                nextOpenTime &&
                nextOpenTime <= normalizedReference;
        }
    }
}
