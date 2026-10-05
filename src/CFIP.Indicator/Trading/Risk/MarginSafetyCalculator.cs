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
                            if (!IsFinitePositive(volume))
                                return 0;

                            double normalizedVolume =
                                Symbol.NormalizeVolumeInUnits(
                                    volume,
                                    RoundingMode.Down);

                            if (!IsFinitePositive(normalizedVolume))
                                return 0;

                            if (!UseAutoMarginGuard)
                                return normalizedVolume;
                
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
                                        normalizedVolume);
                
                                if (!IsFinitePositive(estimated) ||
                                    estimated <= allowed)
                                    return Symbol.NormalizeVolumeInUnits(
                                        volume,
                                        RoundingMode.Down);
                
                                double reduced =
                                    Symbol.NormalizeVolumeInUnits(
                                        normalizedVolume *
                                        allowed /
                                        estimated,
                                        RoundingMode.Down);
                
                                double volumeStep =
                                    Symbol.VolumeInUnitsStep;

                                if (!IsFinitePositive(volumeStep))
                                    return 0;

                                const int maxReductionIterations = 10000;
                                int reductionIterations = 0;

                                while (reduced >= Symbol.VolumeInUnitsMin &&
                                       reductionIterations++ < maxReductionIterations)
                                {
                                    double check =
                                        Symbol.GetEstimatedMargin(
                                            tradeType,
                                            reduced);

                                    if (!IsFinitePositive(check) ||
                                        check <= allowed)
                                        break;

                                    double nextReduced =
                                        Symbol.NormalizeVolumeInUnits(
                                            reduced -
                                            volumeStep,
                                            RoundingMode.Down);

                                    if (!IsFinitePositive(nextReduced) ||
                                        nextReduced >= reduced)
                                        return 0;

                                    reduced = nextReduced;
                                }

                                if (reductionIterations >= maxReductionIterations)
                                    return 0;

                                return reduced >= Symbol.VolumeInUnitsMin
                                    ? Symbol.NormalizeVolumeInUnits(
                                        reduced,
                                        RoundingMode.Down)
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
