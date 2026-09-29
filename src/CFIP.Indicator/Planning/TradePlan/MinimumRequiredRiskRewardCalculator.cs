// CFIP Indicator — MinimumRequiredRiskRewardCalculator.cs
// Single-responsibility planning module.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double MinimumRequiredRR()
        {
            return MinimumRequiredRRForRegime(
                _decision == null
                    ? "UNKNOWN"
                    : _decision.Regime);
        }

        private double MinimumRequiredRRForRegime(
            string regime)
        {
            if (!AdaptiveStructuralRR)
                return Tp1MinimumRR;

            double step =
                Math.Max(
                    0.05,
                    StructuralTpRrStep);

            if (string.Equals(
                    regime,
                    "EXPANSION",
                    StringComparison.OrdinalIgnoreCase))
                return Math.Max(
                    2.10,
                    Tp1MinimumRR + step);

            if (string.Equals(
                    regime,
                    "RANGE",
                    StringComparison.OrdinalIgnoreCase))
                return Math.Max(
                    1.75,
                    Tp1MinimumRR -
                    step * 0.50);

            return Tp1MinimumRR;
        }
    }
}
