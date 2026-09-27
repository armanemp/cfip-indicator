// CFIP Indicator — ProxyExpectedValueCalculator.cs
// Single-responsibility intelligence module.

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
        private double ProxyExpectedValue(
                            int quality,
                            double rr)
                        {
                            double winRate =
                                Clamp(
                                    quality / 100.0,
                                    0.05,
                                    0.95);
                
                            return
                                winRate * rr -
                                (1.0 - winRate);
                        }
    }
}
