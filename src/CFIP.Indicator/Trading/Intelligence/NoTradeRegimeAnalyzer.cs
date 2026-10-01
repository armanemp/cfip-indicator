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
                                regime == MarketRegimeIdentity.Compression)
                                return true;
                
                            if (BlockWeakRangeTransition &&
                                (regime == MarketRegimeIdentity.Range ||
                                 regime == MarketRegimeIdentity.Transition) &&
                                quality <
                                SmartRegimeQualityFloor)
                                return true;
                
                            return false;
                        }
    }
}
