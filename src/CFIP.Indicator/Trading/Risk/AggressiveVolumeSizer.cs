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
                                if (stopPips <= 0)
                                    return 0;
                
                                double amount =
                                    Math.Max(
                                        0,
                                        Account.Equity) *
                                    EffectiveAggressiveRiskPercent() /
                                    100.0;
                
                                if (amount <= 0)
                                    return 0;
                
                                double volume =
                                    Symbol.VolumeForFixedRisk(
                                        amount,
                                        stopPips,
                                        RoundingMode.Down);
                
                                return
                                    Symbol.NormalizeVolumeInUnits(
                                        volume,
                                        RoundingMode.Down);
                            }
                            catch
                            {
                                return 0;
                            }
                        }
    }
}
