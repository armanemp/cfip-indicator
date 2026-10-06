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

            // Compare the current closed-bar ATR with a stable prior baseline,
            // not with one arbitrarily selected historical ATR sample. A single
            // spike/drop must not flip volatility health by itself.
            double baselineAtr =
                AverageAtr(
                    bars,
                    index - 1,
                    20);

            return HealthyVolatilityRule.IsHealthy(
                atr,
                baselineAtr,
                HealthyAtrMinimumRatio,
                HealthyAtrMaximumRatio);
        }
    }
}
