// ============================================================================
// CFIP Indicator — MarketEntryValidation.cs
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
                                        if (!EntryGeometryRule.IsTriggerReached(
                                                plan.Direction,
                                                market,
                                                plan.EntryTrigger,
                                                Symbol.TickSize,
                                                Symbol.PipSize))
                                        {
                                            reason = "WAITING FOR TRIGGER";
                                            return false;
                                        }
                        
                                        return true;
                                    }
                        
                                    if (plan.EntryMode ==
                                        ExecutionMode.RetestMarket)
                                    {
                                        if (!EntryGeometryRule.IsInsideZone(
                                                market,
                                                plan.EntryZoneLow,
                                                plan.EntryZoneHigh,
                                                plan.EntryZoneTolerance))
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
                !IsFinitePositive(fillPrice) ||
                !IsFinitePositive(plan.Entry))
            {
                reason = "INVALID FILL";
                return false;
            }

            double fillAtr =
                Atr(
                    _m5Bars,
                    plan.CreatedM5);

            if (!ExecutionFillAcceptanceRule.IsAcceptable(
                plan.Direction,
                plan.Entry,
                fillPrice,
                fillAtr,
                MaximumEntryExtensionAtr,
                true))
            {
                reason =
                    "BROKER FILL OUTSIDE EXECUTION ENVELOPE";
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
