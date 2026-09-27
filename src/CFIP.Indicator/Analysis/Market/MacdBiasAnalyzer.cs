// CFIP Indicator — MacdBiasAnalyzer.cs
// Single-responsibility analysis module.

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
                                set.MacdFast == null ||
                                set.MacdSlow == null ||
                                index >= set.MacdFast.Result.Count ||
                                index >= set.MacdSlow.Result.Count)
                                return false;
                
                            double histogram =
                                set.MacdFast.Result[index] -
                                set.MacdSlow.Result[index];
                
                            int previousIndex =
                                Math.Max(
                                    0,
                                    index - 2);
                
                            double previous =
                                set.MacdFast.Result[previousIndex] -
                                set.MacdSlow.Result[previousIndex];
                
                            if (double.IsNaN(histogram) ||
                                double.IsInfinity(histogram) ||
                                double.IsNaN(previous) ||
                                double.IsInfinity(previous))
                                return false;
                
                            return
                                direction == 1
                                    ? histogram > 0 &&
                                      histogram >= previous
                                    : histogram < 0 &&
                                      histogram <= previous;
                        }
    }
}
