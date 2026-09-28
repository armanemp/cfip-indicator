// ============================================================================
// CFIP Indicator — PriceProtectionValidation.cs
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
        // ============================================================
                        
                                private double MinimumTakeProfitDistancePrice()
                                {
                                    try
                                    {
                                        double distance =
                                            Math.Max(
                                                0,
                                                Symbol.MinTakeProfitDistance);
                        
                                        if (distance <= 0)
                                            return
                                                Math.Max(
                                                    Symbol.TickSize,
                                                    Symbol.PipSize);
                        
                                        if (Symbol.MinDistanceType ==
                                            SymbolMinDistanceType.Pips)
                                            return
                                                distance *
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    Symbol.TickSize);
                        
                                        return
                                            Symbol.Bid *
                                            distance /
                                            100.0;
                                    }
                                    catch
                                    {
                                        return
                                            Math.Max(
                                                Symbol.TickSize,
                                                Symbol.PipSize);
                                    }
                                }
        
        private bool IsValidTarget(
                                    int direction,
                                    double entry,
                                    double target)
                                {
                                    if (!IsFinitePositive(entry) ||
                                        !IsFinitePositive(target))
                                        return false;
                        
                                    double minimumDistance =
                                        Math.Max(
                                            Symbol.TickSize,
                                            MinimumTakeProfitDistancePrice());
                        
                                    return PriceProtectionRule.IsValidTarget(
                                        direction,
                                        entry,
                                        target,
                                        minimumDistance);
                                }
        
        private double MinimumProtectionDistancePrice()
                                {
                                    try
                                    {
                                        double distance =
                                            Math.Max(
                                                0,
                                                Symbol.MinStopLossDistance);
                        
                                        if (distance <= 0)
                                            return
                                                Math.Max(
                                                    Symbol.TickSize,
                                                    Symbol.PipSize);
                        
                                        if (Symbol.MinDistanceType ==
                                            SymbolMinDistanceType.Pips)
                                            return
                                                distance *
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    Symbol.TickSize);
                        
                                        return
                                            Symbol.Bid *
                                            distance /
                                            100.0;
                                    }
                                    catch
                                    {
                                        return
                                            Math.Max(
                                                Symbol.TickSize,
                                                Symbol.PipSize);
                                    }
                                }
        
        private bool IsValidStop(
                                    int direction,
                                    double entry,
                                    double stop)
                                {
                                    if (!IsFinitePositive(entry) ||
                                        !IsFinitePositive(stop))
                                        return false;
                        
                                    double minimumDistance =
                                        Math.Max(
                                            Symbol.TickSize,
                                            MinimumProtectionDistancePrice());
                        
                                    return PriceProtectionRule.IsValidStop(
                                        direction,
                                        entry,
                                        stop,
                                        minimumDistance);
                                }
    }
}
