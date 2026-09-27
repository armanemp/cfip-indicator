// ============================================================================
// CFIP Indicator — ExponentialMovingAverage.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        private double Ema(Bars bars, int index, bool fast)
                        {
                            if (bars == null || index < 0 || index >= bars.Count)
                                return 0;
                
                            Native set = GetNative(bars);
                            if (set == null)
                                return 0;
                
                            ExponentialMovingAverage ema =
                                fast ? set.Fast : set.Slow;
                
                            if (ema == null || index >= ema.Result.Count)
                                return 0;
                
                            return SafePositive(ema.Result[index]);
                        }
    }
}
