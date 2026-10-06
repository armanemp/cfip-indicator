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
                    ActionabilityThresholdPolicy.ExpansionMinimumTp1RRFloor,
                    Tp1MinimumRR + step);

            if (string.Equals(
                    regime,
                    "RANGE",
                    StringComparison.OrdinalIgnoreCase))
                // Range setups are edge/reversal trades, so the canonical
                // actionable TP1 floor must never be relaxed below the
                // dedicated range-quality contract.
                return Math.Max(
                    ActionabilityThresholdPolicy.RangeMinimumTp1RRFloor,
                    Tp1MinimumRR);

            return Tp1MinimumRR;
        }
    }
}
