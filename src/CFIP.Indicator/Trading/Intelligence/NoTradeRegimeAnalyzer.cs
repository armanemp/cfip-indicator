// CFIP Indicator — NoTradeRegimeAnalyzer.cs
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
        private bool NoTradeRegimeBlocked(
                            string regime,
                            int quality)
                        {
                            if (quality <
                                NoTradeMinimumSmartQuality)
                                return true;
                
                            if (BlockCompressionRegime &&
                                regime == "COMPRESSION")
                                return true;
                
                            if (BlockWeakRangeTransition &&
                                (regime == "RANGE" ||
                                 regime == "TRANSITION") &&
                                quality <
                                SmartRegimeQualityFloor)
                                return true;
                
                            return false;
                        }
    }
}
