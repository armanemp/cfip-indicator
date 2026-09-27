// CFIP Indicator — MinimumRequiredRiskRewardCalculator.cs
// Single-responsibility planning module.

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
private double MinimumRequiredRR()
                        {
                            if (!AdaptiveStructuralRR ||
                                _decision == null)
                                return Tp1MinimumRR;
                
                            double step =
                                Math.Max(
                                    0.05,
                                    StructuralTpRrStep);
                
                            if (_decision.Regime == "EXPANSION")
                                return Math.Max(
                                    2.10,
                                    Tp1MinimumRR + step);
                
                            if (_decision.Regime == "RANGE")
                                return Math.Max(
                                    1.75,
                                    Tp1MinimumRR -
                                    step * 0.50);
                
                            return Tp1MinimumRR;
                        }
    }
}
