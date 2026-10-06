// CFIP Indicator — MacdBiasAnalyzer.cs
// Single-responsibility analysis module.
// This feature is intentionally a two-EMA MACD-line bias. The parameter
// surface currently defines only fast/slow periods; no signal-line crossover is used.

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
        private bool HasMacdBias(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (!UseMacdBias ||
                                bars == null ||
                                index < 35)
                                return false;
                
                            Native set =
                                GetNative(bars);
                
                            if (set == null ||
                                !set.IsInitialized ||
                                set.MacdFast == null ||
                                set.MacdSlow == null ||
                                !NativeIndicatorReadinessRule.IsIndexedWindowReady(
                                    index,
                                    2,
                                    set.MacdFast.Result.Count,
                                    Math.Max(2, MacdFastPeriod)) ||
                                !NativeIndicatorReadinessRule.IsIndexedWindowReady(
                                    index,
                                    2,
                                    set.MacdSlow.Result.Count,
                                    Math.Max(
                                        Math.Max(2, MacdFastPeriod) + 1,
                                        MacdSlowPeriod)))
                                return false;
                
                            double macdLine =
                                set.MacdFast.Result[index] -
                                set.MacdSlow.Result[index];

                            int previousIndex =
                                index - 2;

                            double previousMacdLine =
                                set.MacdFast.Result[previousIndex] -
                                set.MacdSlow.Result[previousIndex];

                            return MacdBiasRule.IsMacdDirectional(
                                direction,
                                macdLine,
                                previousMacdLine);
                        }
    }
}
