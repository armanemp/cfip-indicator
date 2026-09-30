// CFIP Indicator — VolumeSizer.cs
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
private double CalculateVolume(
                            double stopPips)
                        {
                            try
                            {
                                double volume;
                
                                if (SizingMode ==
                                    SizingMode.FixedLots)
                                {
                                    volume =
                                        Symbol.QuantityToVolumeInUnits(
                                            Math.Max(
                                                0.001,
                                                FixedLots));
                                }
                                else
                                {
                                    if (!VolumeSizingRule.IsValidStopPips(stopPips))
                                        return 0;

                                    double riskAmount =
                                        RiskAmountCalculator.Calculate(
                                            Account.Equity,
                                            EffectiveAutoRiskPercent());
                
                                    if (!NumericGuards.IsFinitePositive(riskAmount))
                                        return 0;
                
                                    volume =
                                        Symbol.VolumeForFixedRisk(
                                            riskAmount,
                                            stopPips,
                                            RoundingMode.Down);
                                }
                
                                if (!IsFinitePositive(
                                        volume))
                                    return 0;
                
                                volume =
                                    Symbol.NormalizeVolumeInUnits(
                                        volume,
                                        RoundingMode.Down);
                
                                if (!VolumeSizingRule.IsValidNormalizedVolume(
                                        volume,
                                        Symbol.VolumeInUnitsMin,
                                        Symbol.VolumeInUnitsMax))
                                    return 0;

                                return volume;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP volume calculation failed: {0}",
                                    ex.Message);
                
                                return 0;
                            }
                        }
    }
}
