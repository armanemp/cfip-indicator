// ============================================================================
// CFIP Indicator — AverageTrueRange.cs
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
        private void InitializeAverageTrueRange(Native set, Bars bars)
        {
            set.Atr = Indicators.AverageTrueRange(bars, Math.Max(2, AtrPeriod), MovingAverageType.WilderSmoothing);
        }

        private double Atr(Bars bars, int index)
                        {
                            if (bars == null || index < 0 || index >= bars.Count)
                                return double.NaN;
                
                            Native set = GetNative(bars);
                
                            if (set == null ||
                                !set.IsInitialized ||
                                set.Atr == null ||
                                !NativeIndicatorReadinessRule.IsIndexedSeriesReady(
                                    index,
                                    set.Atr.Result.Count,
                                    Math.Max(2, AtrPeriod)))
                                return 0;
                
                            double value = set.Atr.Result[index];

                            return NativeIndicatorReadinessRule.IsFinitePositiveNative(value)
                                ? value
                                : double.NaN;
                        }
    }
}
