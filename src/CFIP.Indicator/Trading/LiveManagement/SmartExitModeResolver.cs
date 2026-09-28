// ============================================================================
// CFIP Indicator — ReversalProtection.cs
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
        private string GetSmartExitMode()
                                        {
                                            if (_plan == null)
                                                return "NO ACTIVE PLAN";
                                
                                            double risk =
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    _plan.Risk);
                                
                                            double currentRR =
                                                _plan.Direction == 1
                                                    ? (_lastMarket - _plan.Entry) / risk
                                                    : (_plan.Entry - _lastMarket) / risk;
                                
                                            int pressure =
                                                CalculateSmartExitPressure(
                                                    _lastMarket,
                                                    currentRR);
                                
                                            if (pressure >=
                                                SmartExitPressureThreshold)
                                                return "PROTECT";
                                
                                            if (pressure >=
                                                LiveReactionWatchThreshold)
                                                return "WATCH";
                                
                                            return "HOLD";
                                        }
    }
}
