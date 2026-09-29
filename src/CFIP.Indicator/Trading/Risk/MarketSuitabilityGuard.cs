// CFIP Indicator — MarketSuitabilityGuard.cs
// Single-responsibility market risk/suitability module.

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
private bool PassesMarketSuitability(
                                    int closedM5,
                                    int direction,
                                    out string reason,
                                    bool forceRefresh = false)
                                {
                                    int score =
                                        RefreshMarketSuitability(
                                            closedM5,
                                            direction,
                                            forceRefresh);
                        
                                    reason = _marketSuitabilityReason;
                        
                                    if (!EnableMarketSuitabilityGuard)
                                        return true;
                        
                                    bool hardContextBlock =
                                        _marketSuitabilityReason ==
                                            "SYMBOL TRADING DISABLED" ||
                                        _marketSuitabilityReason ==
                                            "MARKET CLOSED" ||
                                        _marketSuitabilityReason ==
                                            "SESSION FILTER" ||
                                        _marketSuitabilityReason ==
                                            "SESSION SUITABILITY" ||
                                        _marketSuitabilityReason ==
                                            "FRIDAY CUTOFF" ||
                                        _marketSuitabilityReason ==
                                            "NEWS BLACKOUT" ||
                                        _marketSuitabilityReason ==
                                            "EVENT SHOCK / VOLATILITY";
                        
                                    if (hardContextBlock &&
                                        HardMarketSuitabilityGate)
                                        return false;
                        
                                    if (score <
                                            Math.Max(
                                                50,
                                                MinimumMarketSuitability) &&
                                        HardMarketSuitabilityGate)
                                        return false;
                        
                                    return true;
                                }
    }
}
