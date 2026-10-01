
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
    }
}
