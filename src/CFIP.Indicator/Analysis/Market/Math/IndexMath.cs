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
                probe = bars.Count - 1;

            probe =
                Math.Max(
                    0,
                    Math.Min(
                        probe,
                        bars.Count - 1));

            for (int i = probe;
                 i >= 0;
                 i--)
            {
                TimeSpan span;

                if (i + 1 < bars.Count)
                    span =
                        bars.OpenTimes[i + 1] -
                        bars.OpenTimes[i];
                else if (i > 0)
                    span =
                        bars.OpenTimes[i] -
                        bars.OpenTimes[i - 1];
                else
                    span =
                        TimeSpan.FromMinutes(1);

                if (span <= TimeSpan.Zero)
                    span =
                        TimeSpan.FromMinutes(1);

                if (bars.OpenTimes[i] + span <= reference)
                    return i;
            }

            return -1;
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
