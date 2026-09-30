using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical owner for the effective structural-stop risk ceiling.
    ///
    /// MaximumSlAtr remains the broad configured stop-risk ceiling.
    /// MaximumStructuralStopAtr remains the additional structural-stop ceiling.
    /// The effective ceiling is the tighter normalized cap. The minimum SL
    /// floor is preserved exactly as in the previous inline formula.
    /// </summary>
    internal static class StructuralStopRiskRule
    {
        public static double EffectiveMaximumStopRiskAtr(
            double minimumSlAtr,
            double maximumSlAtr,
            double maximumStructuralStopAtr)
        {
            double minimum =
                Math.Max(
                    0.05,
                    minimumSlAtr);

            return Math.Min(
                Math.Max(
                    minimum,
                    maximumSlAtr),
                Math.Max(
                    minimum,
                    maximumStructuralStopAtr));
        }
    }
}