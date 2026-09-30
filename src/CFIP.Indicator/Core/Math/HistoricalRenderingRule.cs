using System;
using System.Globalization;

namespace cAlgo
{
    internal static class HistoricalRenderingRule
    {
        public const int MinimumBarsRequired = 60;
        public const int FirstAnalyzableIndex = 40;
        public const int MaximumScanBars = 500;

        public static int ResolveLastClosedIndex(int barCount)
        {
            if (barCount < 2)
                return -1;

            return barCount - 2;
        }

        public static int ResolveOldestScannedIndex(int barCount)
        {
            int lastClosed =
                ResolveLastClosedIndex(barCount);

            if (lastClosed < FirstAnalyzableIndex)
                return FirstAnalyzableIndex;

            return Math.Max(
                FirstAnalyzableIndex,
                lastClosed -
                MaximumScanBars +
                1);
        }

        public static bool IsClosedAnalyzableIndex(
            int index,
            int barCount)
        {
            return
                barCount >= MinimumBarsRequired &&
                index >= FirstAnalyzableIndex &&
                index < barCount - 1;
        }

        public static string ObjectIdentity(DateTime openTime)
        {
            return
                "PRESENTATION_" +
                openTime.Ticks.ToString(
                    CultureInfo.InvariantCulture);
        }
    }
}
