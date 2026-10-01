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
                index < 0 ||
                index >= bars.Count)
                return 100;

            int length =
                Math.Max(
                    10,
                    period);

            if (!ChoppinessIndexRule.HasChoppinessEnoughHistory(
                    index,
                    length))
                return 100;

            int first =
                ChoppinessIndexRule.ResolveFirstBarIndex(
                    index,
                    length);

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

            return ChoppinessIndexRule.Evaluate(
                trueRangeSum,
                range,
                length);
        }

    }
}
