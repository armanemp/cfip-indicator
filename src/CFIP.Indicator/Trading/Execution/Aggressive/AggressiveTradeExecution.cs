// ============================================================================
// CFIP Indicator — AggressiveTradeExecution.cs
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
        private void TryAggressiveAutoTrade(
                                    int closedM5)
                                {
                                    TradeType type;
                                    double entry;
                                    double atr;
                                    double stop;
                                    double target;
                                    double stopPips;
                                    double tpPips;
                                    double volume;

                                    if (!TryPrepareAggressiveTrade(
                                            closedM5,
                                            out type,
                                            out entry,
                                            out atr,
                                            out stop,
                                            out target,
                                            out stopPips,
                                            out tpPips,
                                            out volume))
                                        return;

                                    ExecuteAggressiveTrade(
                                        closedM5,
                                        type,
                                        entry,
                                        atr,
                                        stop,
                                        target,
                                        stopPips,
                                        tpPips,
                                        volume);
                                }
    }
}
