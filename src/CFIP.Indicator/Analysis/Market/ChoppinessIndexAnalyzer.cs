using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double CalculateChoppinessIndex(
            Bars bars,
            int index,
            int period)
        {
            if (bars == null ||
                index < 2)
                return 100;

            int length =
                Math.Max(
                    10,
                    Math.Min(
                        period,
                        index + 1));

            int first =
                index - length + 1;

            double trueRangeSum = 0;
            double highest = double.MinValue;
            double lowest = double.MaxValue;

            for (int i = first; i <= index; i++)
            {
                double high = bars.HighPrices[i];
                double low = bars.LowPrices[i];

                if (high > highest)
                    highest = high;

                if (low < lowest)
                    lowest = low;

                double previousClose =
                    i > 0
                        ? bars.ClosePrices[i - 1]
                        : bars.ClosePrices[i];

                double trueRange =
                    Math.Max(
                        high - low,
                        Math.Max(
                            Math.Abs(high - previousClose),
                            Math.Abs(low - previousClose)));

                trueRangeSum +=
                    Math.Max(
                        0,
                        trueRange);
            }

            double range =
                highest -
                lowest;

            if (range <= 0 ||
                trueRangeSum <= 0)
                return 100;

            double value =
                100.0 *
                Math.Log10(
                    trueRangeSum /
                    range) /
                Math.Log10(
                    Math.Max(
                        2,
                        length));

            return Math.Max(
                0,
                Math.Min(
                    100,
                    value));
        }

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