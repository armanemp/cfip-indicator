// CFIP Indicator — MarketSuitabilityRefreshCoordinator.cs
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
private int RefreshMarketSuitability(
                                    int closedM5,
                                    int direction,
                                    bool force)
                                {
                                    DateTime nowUtc = TimeInUtc;
                        
                                    int throttleSeconds =
                                        Math.Max(1, SuitabilityRecalculationSeconds);
                        
                                    bool due =
                                        force ||
                                        closedM5 != _marketSuitabilityM5 ||
                                        _marketSuitabilityDirection != direction ||
                                        (nowUtc - _lastMarketSuitabilityUtc).TotalSeconds >=
                                        throttleSeconds;
                        
                                    if (!due)
                                        return _marketSuitabilityScore;
                        
                                    string reason;
                        
                                    int score =
                                        CalculateMarketSuitability(
                                            closedM5,
                                            direction,
                                            out reason);
                        
                                    _marketSuitabilityM5 = closedM5;
                                    _marketSuitabilityDirection = direction;
                                    _marketSuitabilityScore = score;
                                    _marketSuitabilityReason = reason;
                                    _marketSuitabilityState =
                                        score >= Math.Max(50, MinimumMarketSuitability)
                                            ? "SUITABLE"
                                            : "UNSUITABLE";
                                    _lastMarketSuitabilityUtc = nowUtc;
                        
                                    return score;
                                }
    }
}
