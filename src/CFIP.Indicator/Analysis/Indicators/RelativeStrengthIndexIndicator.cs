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
        private void InitializeRelativeStrengthIndex(Native set, Bars bars)
        {
            set.Rsi = Indicators.RelativeStrengthIndex(bars.ClosePrices, Math.Max(2, RsiPeriod));
        }

private double Rsi(Bars bars, int index)
                {
                    if (bars == null || index < 0 || index >= bars.Count)
                        return 50;
        
                    Native set = GetNative(bars);
        
                    if (set == null ||
                        set.Rsi == null ||
                        index >= set.Rsi.Result.Count)
                        return 50;
        
                    double value = set.Rsi.Result[index];
        
                    return
                        double.IsNaN(value) ||
                        double.IsInfinity(value)
                            ? 50
                            : Clamp(value, 0, 100);
                }
    }
}
