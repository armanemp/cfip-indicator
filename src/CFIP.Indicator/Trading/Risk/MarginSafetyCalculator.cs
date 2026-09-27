// CFIP Indicator — MarginSafetyCalculator.cs
// Single-responsibility risk module.

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
private double AdjustVolumeForMargin(
                            TradeType tradeType,
                            double volume)
                        {
                            if (!IsFinitePositive(volume) ||
                                !UseAutoMarginGuard)
                                return volume;
                
                            try
                            {
                                double freeMargin =
                                    Math.Max(
                                        0,
                                        Account.FreeMargin);
                
                                double usage =
                                    MarginUsagePolicy.CalculateAllowedPercent(
                                        MaxAutoMarginUsagePercent,
                                        MarginBufferPercent);
                
                                double allowed =
                                    freeMargin *
                                    usage /
                                    100.0;
                
                                if (allowed <= 0)
                                    return 0;
                
                                double estimated =
                                    Symbol.GetEstimatedMargin(
                                        tradeType,
                                        volume);
                
                                if (!IsFinitePositive(estimated) ||
                                    estimated <= allowed)
                                    return Symbol.NormalizeVolumeInUnits(
                                        volume,
                                        RoundingMode.Down);
                
                                double reduced =
                                    Symbol.NormalizeVolumeInUnits(
                                        volume *
                                        allowed /
                                        estimated,
                                        RoundingMode.Down);
                
                                while (reduced >= Symbol.VolumeInUnitsMin)
                                {
                                    double check =
                                        Symbol.GetEstimatedMargin(
                                            tradeType,
                                            reduced);
                
                                    if (!IsFinitePositive(check) ||
                                        check <= allowed)
                                        break;
                
                                    reduced =
                                        Symbol.NormalizeVolumeInUnits(
                                            reduced -
                                            Symbol.VolumeInUnitsStep,
                                            RoundingMode.Down);
                                }
                
                                return reduced >= Symbol.VolumeInUnitsMin
                                    ? reduced
                                    : 0;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP margin sizing failed: {0}",
                                    ex.Message);
                                return 0;
                            }
                        }
    }
}
