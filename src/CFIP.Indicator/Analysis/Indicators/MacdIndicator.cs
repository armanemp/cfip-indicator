using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void InitializeMacd(Native set, Bars bars)
        {
            int macdFastPeriod = Math.Max(2, MacdFastPeriod);
            int macdSlowPeriod = Math.Max(macdFastPeriod + 1, MacdSlowPeriod);
            set.MacdFast = Indicators.ExponentialMovingAverage(bars.ClosePrices, macdFastPeriod);
            set.MacdSlow = Indicators.ExponentialMovingAverage(bars.ClosePrices, macdSlowPeriod);
        }
    }
}
