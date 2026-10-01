
using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool IsAdverseWindowPreZone(
            Bars bars,
            int index,
            double zoneLow,
            double zoneHigh,
            int lookbackBars)
        {
            if (bars == null ||
                index < lookbackBars ||
                index >= bars.Count)
                return false;

            if (double.IsNaN(zoneLow) ||
                double.IsInfinity(zoneLow) ||
                double.IsNaN(zoneHigh) ||
                double.IsInfinity(zoneHigh))
                return false;

            double low =
                Math.Min(
                    zoneLow,
                    zoneHigh);
            double high =
                Math.Max(
                    zoneLow,
                    zoneHigh);

            for (int i = index - lookbackBars;
                 i <= index;
                 i++)
            {
                double barLow = bars.LowPrices[i];
                double barHigh = bars.HighPrices[i];

                if (double.IsNaN(barLow) ||
                    double.IsInfinity(barLow) ||
                    double.IsNaN(barHigh) ||
                    double.IsInfinity(barHigh))
                    return false;

                if (barHigh >= low &&
                    barLow <= high)
                    return false;
            }

            return true;
        }

        private double ResolveAdverseM5Atr(
            Bars bars,
            int index,
            int direction,
            double atr)
        {
            if (bars == null ||
                index < 2 ||
                index >= bars.Count ||
                !IsFinitePositive(atr))
                return 0;

            double move =
                bars.ClosePrices[index] -
                bars.ClosePrices[index - 2];

            return
                direction == 1
                    ? Math.Max(0, -move) /
                      Math.Max(Symbol.PipSize, atr)
                    : Math.Max(0, move) /
                      Math.Max(Symbol.PipSize, atr);
        }

        private double ResolveAdverseM1Atr(
            Bars bars,
            int index,
            int direction)
        {
            if (bars == null ||
                index < 2 ||
                index >= bars.Count)
                return 0;

            double atr = Atr(bars, index);
            if (!IsFinitePositive(atr))
                return 0;

            double move =
                bars.ClosePrices[index] -
                bars.ClosePrices[index - 2];

            return
                direction == 1
                    ? Math.Max(0, -move) /
                      Math.Max(Symbol.PipSize, atr)
                    : Math.Max(0, move) /
                      Math.Max(Symbol.PipSize, atr);
        }

        private static bool IsDirectionConflict(
            int frameDirection,
            int requestedDirection)
        {
            return
                (requestedDirection == 1 &&
                 frameDirection == -1) ||
                (requestedDirection == -1 &&
                 frameDirection == 1);
        }

    }
}
