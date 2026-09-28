using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double CalculateRangeEfficiency(
            Bars bars,
            int index,
            int period)
        {
            if (bars == null ||
                index < 2)
                return 0;

            int length =
                Math.Max(
                    10,
                    Math.Min(
                        period,
                        index));

            int first =
                Math.Max(
                    1,
                    index - length + 1);

            double path = 0;

            for (int i = first; i <= index; i++)
            {
                path +=
                    Math.Abs(
                        bars.ClosePrices[i] -
                        bars.ClosePrices[i - 1]);
            }

            if (path <= 0)
                return 0;

            double net =
                Math.Abs(
                    bars.ClosePrices[index] -
                    bars.ClosePrices[first - 1]);

            return Math.Max(
                0,
                Math.Min(
                    1,
                    net / path));
        }
    }
}