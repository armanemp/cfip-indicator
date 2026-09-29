using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Zone BuildManagedFvgZone(
            Bars bars,
            int createdIndex,
            int currentIndex,
            int direction,
            double low,
            double high,
            double gap,
            bool twoBarImbalance,
            double atr)
        {
            if (bars == null ||
                createdIndex < 0 ||
                currentIndex < createdIndex ||
                low >= high)
                return null;

            if (!TryApplyFvgMitigation(
                    bars,
                    createdIndex,
                    currentIndex,
                    direction,
                    low,
                    high,
                    out double managedLow,
                    out double managedHigh))
                return null;

            int quality =
                CalculateFvgQuality(
                    managedLow,
                    managedHigh,
                    low,
                    high,
                    gap,
                    atr,
                    twoBarImbalance);

            Zone z =
                new Zone
                {
                    Low = managedLow,
                    High = managedHigh,
                    Direction = direction,
                    Kind =
                        twoBarImbalance
                            ? "FVG_2BAR"
                            : "FVG",
                    Id =
                        FvgRule.Identity(
                            direction,
                            createdIndex,
                            twoBarImbalance),
                    CreatedIndex =
                        createdIndex,
                    Age =
                        currentIndex -
                        createdIndex,
                    Quality = quality
                };

            return z;
        }

    }
}
