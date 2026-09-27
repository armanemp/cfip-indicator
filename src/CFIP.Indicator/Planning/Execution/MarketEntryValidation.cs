// ============================================================================
// CFIP Indicator — MarketEntryValidation.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        private bool IsExecutableMarketEntry(
                                    Plan plan,
                                    double market,
                                    out string reason)
                                {
                                    reason = "OK";
                        
                                    if (plan == null)
                                    {
                                        reason = "NO PLAN";
                                        return false;
                                    }
                        
                                    if (!IsFinitePositive(market))
                                    {
                                        reason = "INVALID MARKET";
                                        return false;
                                    }
                        
                                    if (plan.EntryMode ==
                                        ExecutionMode.BreakoutMarket)
                                    {
                                        if (!IsTriggerReached(
                                                plan.Direction,
                                                market,
                                                plan.EntryTrigger))
                                        {
                                            reason = "WAITING FOR TRIGGER";
                                            return false;
                                        }
                        
                                        return true;
                                    }
                        
                                    if (plan.EntryMode ==
                                        ExecutionMode.RetestMarket)
                                    {
                                        double tolerance =
                                            Math.Max(
                                                Symbol.TickSize,
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    Symbol.Ask - Symbol.Bid));
                        
                                        if (market <
                                                plan.EntryZoneLow - tolerance ||
                                            market >
                                                plan.EntryZoneHigh + tolerance)
                                        {
                                            reason = "OUTSIDE RETEST ZONE";
                                            return false;
                                        }
                        
                                        return true;
                                    }
                        
                                    reason =
                                        ExecutionModeText(
                                            plan.EntryMode);
                                    return false;
                                }
        
        private bool IsExecutableFillPrice(
                                    Plan plan,
                                    double fillPrice,
                                    out string reason)
                                {
                                    reason = "OK";
                        
                                    if (plan == null ||
                                        !IsFinitePositive(fillPrice))
                                    {
                                        reason = "INVALID FILL";
                                        return false;
                                    }
                        
                                    if (plan.EntryMode ==
                                        ExecutionMode.BreakoutMarket)
                                    {
                                        if (!IsFinitePositive(
                                                plan.EntryTrigger))
                                        {
                                            reason = "MISSING BREAKOUT TRIGGER";
                                            return false;
                                        }
                        
                                        double tolerance =
                                            Math.Max(
                                                Symbol.TickSize * 2,
                                                Math.Max(
                                                    Symbol.PipSize * 0.5,
                                                    (Symbol.Ask - Symbol.Bid) * 2));
                        
                                        bool acceptable =
                                            plan.Direction == 1
                                                ? fillPrice >=
                                                  plan.EntryTrigger - tolerance
                                                : fillPrice <=
                                                  plan.EntryTrigger + tolerance;
                        
                                        if (!acceptable)
                                        {
                                            reason =
                                                "BROKER FILL FAR FROM TRIGGER";
                                            return false;
                                        }
                        
                                        if (!IsTriggerReached(
                                                plan.Direction,
                                                fillPrice,
                                                plan.EntryTrigger))
                                        {
                                            reason =
                                                "BREAKOUT FILL • SLIPPAGE ACCEPTED";
                                        }
                        
                                        return true;
                                    }
                        
                                    if (plan.EntryMode ==
                                        ExecutionMode.RetestMarket)
                                        return IsExecutableMarketEntry(
                                            plan,
                                            fillPrice,
                                            out reason);
                        
                                    reason =
                                        "INVALID EXECUTION MODE";
                                    return false;
                                }
    }
}
