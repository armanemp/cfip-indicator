// CFIP Indicator — LiveBiasAnalyzer.cs
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
        private double LiveBias(
                            int chartIndex,
                            int direction)
                        {
                            if (Bars == null ||
                                Bars.Count < 10)
                                return 0;
                
                            int i =
                                Math.Max(
                                    1,
                                    Math.Min(
                                        chartIndex,
                                        Bars.Count - 1));
                
                            double fast =
                                Ema(
                                    Bars,
                                    i,
                                    true);
                
                            double slow =
                                Ema(
                                    Bars,
                                    i,
                                    false);
                
                            if (direction == 1 &&
                                Bars.ClosePrices[i] > fast &&
                                fast > slow)
                                return 5;
                
                            if (direction == -1 &&
                                Bars.ClosePrices[i] < fast &&
                                fast < slow)
                                return 5;
                
                            return 0;
                        }
    }
}
