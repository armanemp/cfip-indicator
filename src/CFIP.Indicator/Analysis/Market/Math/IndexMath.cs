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
            if (bars == null)
                return -1;

            return ClosedBarReferenceRule.ResolveClosedIndex(
                bars.Count,
                reference,
                index => bars.OpenTimes[index]);
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
        internal static DateTime ClosedBarBoundaryReference(
            Bars bars,
            int closedIndex,
            DateTime fallback)
        {
            if (bars == null ||
                closedIndex < 0 ||
                closedIndex >= bars.Count - 1)
                return fallback;

            DateTime open =
                bars.OpenTimes[closedIndex];

            DateTime nextOpen =
                bars.OpenTimes[closedIndex + 1];

            if (nextOpen <= open)
                return fallback;

            return nextOpen;
        }


    }
}
