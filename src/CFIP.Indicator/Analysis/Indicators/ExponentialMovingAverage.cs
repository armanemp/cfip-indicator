// ============================================================================
// CFIP Indicator — ExponentialMovingAverage.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void InitializeExponentialMovingAverages(Native set, Bars bars)
        {
            set.Fast = Indicators.ExponentialMovingAverage(bars.ClosePrices, Math.Max(2, FastEma));
            set.Slow = Indicators.ExponentialMovingAverage(bars.ClosePrices, Math.Max(3, SlowEma));
        }

        private double Ema(Bars bars, int index, bool fast)
                        {
                            if (bars == null || index < 0 || index >= bars.Count)
                                return 0;
                
                            Native set = GetNative(bars);
                            if (set == null)
                                return 0;
                
                            ExponentialMovingAverage ema =
                                fast ? set.Fast : set.Slow;
                
                            if (ema == null ||
                                !NativeIndicatorReadinessRule.IsIndexedSeriesReady(
                                    index,
                                    ema.Result.Count,
                                    fast
                                        ? Math.Max(2, FastEma)
                                        : Math.Max(3, SlowEma)))
                                return 0;
                
                            return SafePositive(ema.Result[index]);
                        }
    }
}
