// ============================================================================
// CFIP Indicator — AutomaticMarketTradeExecution.cs
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
        private void TryAutoTrade(
                                    int closedM5)
                                {
                                    _lastAutoTradeAttemptUtc =
                                        TimeInUtc;

                                    TradeType type;
                                    double entry;
                                    double stopPips;
                                    double targetPips;
                                    double target;
                                    double volume;

                                    if (!TryPrepareAutomaticMarketTrade(
                                            closedM5,
                                            out type,
                                            out entry,
                                            out stopPips,
                                            out targetPips,
                                            out target,
                                            out volume))
                                        return;

                                    ExecutePreparedAutomaticMarketTrade(
                                        closedM5,
                                        type,
                                        entry,
                                        stopPips,
                                        targetPips,
                                        target,
                                        volume);
                                }
    }
}
