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
                for (int i =
                         createdIndex + 1;
                     i <= currentIndex;
                     i++)
                {
                    double breaker =
                        direction == 1
                            ? (FvgBreakByWicks
                                ? bars.LowPrices[i]
                                : Math.Min(
                                    bars.OpenPrices[i],
                                    bars.ClosePrices[i]))
                            : (FvgBreakByWicks
                                ? bars.HighPrices[i]
                                : Math.Max(
                                    bars.OpenPrices[i],
                                    bars.ClosePrices[i]));

                    if (direction == 1)
                    {
                        if (breaker <= managedLow)
                        {
                            if (FvgInvalidateOnFullFill)
                                return false;

                            managedHigh =
                                managedLow;
                            break;
                        }

                        if (breaker < managedHigh)
                            managedHigh =
                                Math.Max(
                                    managedLow,
                                    breaker);
                    }
                    else
                    {
                        if (breaker >= managedHigh)
                        {
                            if (FvgInvalidateOnFullFill)
                                return false;

                            managedLow =
                                managedHigh;
                            break;
                        }

                        if (breaker > managedLow)
                            managedLow =
                                Math.Min(
                                    managedHigh,
                                    breaker);
                    }

                    if (managedHigh -
                        managedLow <=
                        Symbol.TickSize)
                    {
                        if (FvgInvalidateOnFullFill)
                            return false;

                        managedHigh =
                            managedLow;
                        break;
                    }
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
                    currentIndex);

            for (int i = start;
                 i <= end;
                 i++)
            {
                if (direction == 1)
                {
                    if (bars.LowPrices[i] <= low)
                        return true;
                }
                else if (direction == -1)
                {
                    if (bars.HighPrices[i] >= high)
                        return true;
                }
            }

            return false;
        }
    }
}
