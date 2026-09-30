// CFIP Indicator — AggressiveVolumeSizer.cs
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
private double CalculateAggressiveVolume(
                            double stopPips)
                        {
                            try
                            {
                                if (!VolumeSizingRule.IsValidStopPips(stopPips))
                                    return 0;
                
                                double amount =
                                    RiskAmountCalculator.Calculate(
                                        Account.Equity,
                                        EffectiveAggressiveRiskPercent());
                
                                if (!NumericGuards.IsFinitePositive(amount))
                                    return 0;
                
                                double volume =
                                    Symbol.VolumeForFixedRisk(
                                        amount,
                                        stopPips,
                                        RoundingMode.Down);
                
                                double normalized =
                                    Symbol.NormalizeVolumeInUnits(
                                        volume,
                                        RoundingMode.Down);

                                if (!VolumeSizingRule.IsValidNormalizedVolume(
                                        normalized,
                                        Symbol.VolumeInUnitsMin,
                                        Symbol.VolumeInUnitsMax))
                                    return 0;

                                return normalized;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP aggressive volume calculation failed: {0}",
                                    ex.Message);

                                return 0;
                            }
                        }
    }
}
