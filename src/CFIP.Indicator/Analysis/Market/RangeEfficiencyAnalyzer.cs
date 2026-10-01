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
                index < 0 ||
                index >= bars.Count)
                return 0;

            int length =
                Math.Max(
                    10,
                    period);

            if (!RangeEfficiencyRule.HasRangeEnoughHistory(
                    index,
                    length))
                return 0;

            int first =
                RangeEfficiencyRule.ResolveFirstCloseIndex(
                    index,
                    length);

            double path = 0;

            for (int i = first + 1;
                 i <= index;
                 i++)
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
                    bars.ClosePrices[first]);

            return RangeEfficiencyRule.Evaluate(
                net,
                path);
        }
    }
}