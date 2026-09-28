// ============================================================================
// CFIP Indicator — IndexMath.cs
// Market-series index/range helpers.
// ============================================================================

using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal static class IndexMath
    {
        internal static int ClosedIndex(
            Bars bars,
            DateTime reference)
        {
            if (bars == null ||
                bars.Count < 2 ||
                reference < bars.OpenTimes[0])
                return -1;

            int probe =
                bars.OpenTimes.GetIndexByTime(
                    reference);

            if (probe < 0)
            {
                int left = 0;
                int right =
                    bars.Count - 1;

                // Find the last bar whose open time is <= reference.
                while (left <= right)
                {
                    int mid =
                        left +
                        ((right - left) / 2);

                    if (bars.OpenTimes[mid] <=
                        reference)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }

                probe = right;
            }

            // The bar containing reference is still forming. The previous
            // bar is therefore the latest fully closed bar. This uses ordering
            // rather than inferred bar duration, so weekend/holiday gaps cannot
            // make the resolver skip an otherwise closed candle.
            return Math.Max(
                -1,
                Math.Min(
                    bars.Count - 2,
                    probe - (bars.OpenTimes[probe] == reference ? 1 : 0)));
        }

        internal static double Highest(
            Bars bars,
            int start,
            int end)
        {
            if (bars == null ||
                bars.Count == 0)
                return 0;

            start =
                Math.Max(
                    0,
                    start);

            end =
                Math.Min(
                    bars.Count - 1,
                    end);

            if (end < start)
                return
                    bars.HighPrices[
                        Math.Max(
                            0,
                            Math.Min(
                                bars.Count - 1,
                                start))];

            double value =
                double.MinValue;

            for (int i = start;
                 i <= end;
                 i++)
                value =
                    Math.Max(
                        value,
                        bars.HighPrices[i]);

            return
                value == double.MinValue
                    ? 0
                    : value;
        }

        internal static double Lowest(
            Bars bars,
            int start,
            int end)
        {
            if (bars == null ||
                bars.Count == 0)
                return 0;

            start =
                Math.Max(
                    0,
                    start);

            end =
                Math.Min(
                    bars.Count - 1,
                    end);

            if (end < start)
                return
                    bars.LowPrices[
                        Math.Max(
                            0,
                            Math.Min(
                                bars.Count - 1,
                                start))];

            double value =
                double.MaxValue;

            for (int i = start;
                 i <= end;
                 i++)
                value =
                    Math.Min(
                        value,
                        bars.LowPrices[i]);

            return
                value == double.MaxValue
                    ? 0
                    : value;
        }
    }
}
