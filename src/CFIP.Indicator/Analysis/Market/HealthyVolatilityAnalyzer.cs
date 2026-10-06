using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool HasHealthyVolatility(
            Bars bars,
            int index)
        {
            if (!UseHealthyVolatility ||
                bars == null ||
                index < 30 ||
                index >= bars.Count)
                return false;

            double atr = Atr(bars, index);
            double oldAtr =
                Atr(
                    bars,
                    Math.Max(
                        5,
                        index - 10));

            if (!NumericGuards.IsFinitePositive(atr) ||
                !NumericGuards.IsFinitePositive(oldAtr))
                return false;

            return HealthyVolatilityRule.IsHealthy(
                atr,
                oldAtr,
                HealthyAtrMinimumRatio,
                HealthyAtrMaximumRatio);
        }
    }
}
