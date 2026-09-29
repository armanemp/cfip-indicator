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
                        direction == 1
                            ? (ObBreakByWicks
                                ? bars.LowPrices[j]
                                : Math.Min(
                                    bars.OpenPrices[j],
                                    bars.ClosePrices[j]))
                            : (ObBreakByWicks
                                ? bars.HighPrices[j]
                                : Math.Max(
                                    bars.OpenPrices[j],
                                    bars.ClosePrices[j]));

                    if (direction == 1)
                    {
                        if (probe <=
                            zoneLow)
                            return false;

                        if (probe <
                            managedHigh)
                        {
                            managedHigh =
                                Math.Max(
                                    managedLow,
                                    probe);
                            partiallyMitigated = true;
                        }
                    }
                    else
                    {
                        if (probe >=
                            zoneHigh)
                            return false;

                        if (probe >
                            managedLow)
                        {
                            managedLow =
                                Math.Min(
                                    managedHigh,
                                    probe);
                            partiallyMitigated = true;
                        }
                    }

                    if (managedHigh -
                        managedLow <=
                        Symbol.TickSize)
                        return false;
                }
            }

            remainingRatio =
                (managedHigh -
                 managedLow) /
                originalWidth;

            return remainingRatio > 0.05;
        }
    }
}
