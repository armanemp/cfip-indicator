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
            out double remainingRatio,
            out OrderBlockLifecycleState lifecycleState)
        {
            managedLow = zoneLow;
            managedHigh = zoneHigh;
            partiallyMitigated = false;
            remainingRatio = 0;
            lifecycleState =
                OrderBlockLifecycleState.Broken;
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
                        OrderBlockLifecycleRule.ResolveMitigationProbe(
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

                    if (!OrderBlockLifecycleRule.TryApplyPartialMitigation(
                            direction,
                            managedLow,
                            managedHigh,
                            probe,
                            Symbol.TickSize,
                            out nextLow,
                            out nextHigh,
                            out changed,
                            out nextRatio,
                            out lifecycleState))
                        return false;

                    managedLow = nextLow;
                    managedHigh = nextHigh;
                    partiallyMitigated =
                        partiallyMitigated ||
                        changed;

                    if (nextRatio <=
                        OrderBlockLifecycleRule.MinimumRetainedRatio)
                    {
                        lifecycleState =
                            OrderBlockLifecycleState.Broken;
                        return false;
                    }
                }
            }

            remainingRatio =
                (managedHigh -
                 managedLow) /
                originalWidth;

            lifecycleState =
                OrderBlockLifecycleRule.Classify(
                    partiallyMitigated,
                    remainingRatio);

            return
                lifecycleState !=
                OrderBlockLifecycleState.Broken &&
                managedHigh > managedLow &&
                remainingRatio >
                OrderBlockLifecycleRule.MinimumRetainedRatio;
        }
    }
}
