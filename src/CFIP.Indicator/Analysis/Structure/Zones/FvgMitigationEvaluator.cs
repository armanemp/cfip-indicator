using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryApplyFvgMitigation(
            Bars bars,
            int createdIndex,
            int currentIndex,
            int direction,
            double low,
            double high,
            out double managedLow,
            out double managedHigh)
        {
            managedLow = low;
            managedHigh = high;

            if (EnableFvgPartialMitigation &&
                UseZoneMitigationGuard &&
                currentIndex > createdIndex)
            {
                int end =
                    Math.Min(
                        currentIndex,
                        createdIndex +
                        Math.Max(
                            1,
                            MaximumZoneAgeBars));

                for (int i =
                         createdIndex + 1;
                     i <= end;
                     i++)
                {
                    double probe =
                        FvgLifecycleRule.ResolveFvgMitigationProbe(
                            direction,
                            bars.OpenPrices[i],
                            bars.ClosePrices[i],
                            bars.LowPrices[i],
                            bars.HighPrices[i],
                            FvgBreakByWicks);

                    double nextLow;
                    double nextHigh;
                    bool fullyFilled;

                    if (!FvgLifecycleRule.TryApplyMitigationStep(
                            direction,
                            managedLow,
                            managedHigh,
                            probe,
                            Symbol.TickSize,
                            FvgInvalidateOnFullFill,
                            out nextLow,
                            out nextHigh,
                            out fullyFilled))
                        return false;

                    if (fullyFilled)
                    {
                        // The lifecycle rule preserves source geometry when
                        // invalidation is explicitly disabled.
                        managedLow = nextLow;
                        managedHigh = nextHigh;
                        break;
                    }

                    managedLow = nextLow;
                    managedHigh = nextHigh;
                }
            }
            else if (FvgInvalidateOnFullFill &&
                     IsZoneFullyMitigated(
                         bars,
                         createdIndex,
                         currentIndex,
                         direction,
                         low,
                         high))
            {
                return false;
            }

            return
                managedLow <
                managedHigh;
        }

        private bool IsZoneFullyMitigated(
            Bars bars,
            int createdIndex,
            int currentIndex,
            int direction,
            double low,
            double high)
        {
            if (!UseZoneMitigationGuard ||
                bars == null ||
                createdIndex < 0 ||
                currentIndex <= createdIndex)
                return false;

            int start =
                Math.Max(
                    0,
                    createdIndex + 1);

            int end =
                Math.Min(
                    bars.Count - 1,
                    Math.Min(
                        currentIndex,
                        createdIndex +
                        Math.Max(
                            1,
                            MaximumZoneAgeBars)));

            for (int i = start;
                 i <= end;
                 i++)
            {
                double fillPrice =
                    FvgLifecycleRule.ResolveFvgMitigationProbe(
                        direction,
                        bars.OpenPrices[i],
                        bars.ClosePrices[i],
                        bars.LowPrices[i],
                        bars.HighPrices[i],
                        FvgBreakByWicks);

                if (FvgRule.IsFullyFilled(
                        direction,
                        low,
                        high,
                        fillPrice))
                    return true;
            }

            return false;
        }
    }
}
