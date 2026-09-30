// ============================================================================
// CFIP Indicator — StructuralStopRiskRule.cs
// Canonical effective structural-stop risk ceiling semantics.
// ============================================================================

using System;

namespace cAlgo
{
    internal static class StructuralStopRiskRule
    {
        public static double EffectiveMaximumStopRiskAtr(
            double minimumSlAtr,
            double maximumSlAtr,
            double maximumStructuralStopAtr)
        {
            return
                Math.Min(
                    Math.Max(
                        minimumSlAtr,
                        maximumSlAtr),
                    Math.Max(
                        minimumSlAtr,
                        maximumStructuralStopAtr));
        }

        public static bool IsWithinEffectiveMaximumStopRiskAtr(
            double riskAtr,
            double minimumSlAtr,
            double maximumSlAtr,
            double maximumStructuralStopAtr)
        {
            if (double.IsNaN(riskAtr) ||
                double.IsInfinity(riskAtr))
                return false;

            return
                riskAtr <=
                EffectiveMaximumStopRiskAtr(
                    minimumSlAtr,
                    maximumSlAtr,
                    maximumStructuralStopAtr);
        }
    }
}
