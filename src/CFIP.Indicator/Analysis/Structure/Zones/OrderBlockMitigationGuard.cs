using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryApplyOrderBlockMitigation(
            Bars bars,
            int createdIndex,
            int currentIndex,
            int direction,
            double zoneLow,
            double zoneHigh,
            out double managedLow,
            out double managedHigh,
            out bool partiallyMitigated,
            out double remainingRatio)
        {
            managedLow = zoneLow;
            managedHigh = zoneHigh;
            partiallyMitigated = false;
            remainingRatio = 0;
            double originalWidth =
                Math.Max(
                    Symbol.TickSize,
                    zoneHigh -
                    zoneLow);

            if (UseZoneMitigationGuard)
            {
                int end =
                    Math.Min(
                        currentIndex,
                        createdIndex +
                        Math.Max(
                            1,
                            MaximumZoneAgeBars));

                for (int j =
                         createdIndex + 1;
                     j <= end;
                     j++)
                {
                    double probe =
                        OrderBlockRule.GetMitigationProbe(
                            direction,
                            bars.OpenPrices[j],
                            bars.ClosePrices[j],
                            bars.HighPrices[j],
                            bars.LowPrices[j],
                            ObBreakByWicks);

                    double nextLow;
                    double nextHigh;
                    bool changed;
                    double nextRatio;

                    if (!OrderBlockRule.TryApplyPartialMitigation(
                            direction,
                            managedLow,
                            managedHigh,
                            probe,
                            Symbol.TickSize,
                            out nextLow,
                            out nextHigh,
                            out changed,
                            out nextRatio))
                        return false;

                    managedLow = nextLow;
                    managedHigh = nextHigh;
                    partiallyMitigated =
                        partiallyMitigated ||
                        changed;

                    if (nextRatio <= 0.05)
                        return false;

            }

            remainingRatio =
                (managedHigh -
                 managedLow) /
                originalWidth;

            return
                managedHigh > managedLow &&
                remainingRatio > 0.05;
        }
    }
}
