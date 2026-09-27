// CFIP Indicator — AggressiveRiskPolicy.cs
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
private double EffectiveAggressiveRiskPercent()
                        {
                            double baseRisk =
                                Math.Max(
                                    0.05,
                                    AggressiveRiskPercentEquity);
                
                            if (!UseSmartRiskScaling)
                                return baseRisk;
                
                            return ClampDouble(
                                baseRisk * SuitabilityRiskMultiplier(),
                                0.05,
                                baseRisk);
                        }
    }
}
