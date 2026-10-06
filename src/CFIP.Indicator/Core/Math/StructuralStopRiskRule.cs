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
    
        internal const double MicroStopRiskAtrFloor = 0.30;
        internal const double MicroStopM1AtrMultiplier = 1.10;

        public static double ResolveEffectiveMinimumStopRiskAtr(
            double configuredMinimumAtr,
            double riskReferenceAtr,
            double microAtr,
            double spread,
            double pipSize,
            double maximumSpreadToStopRiskRatio,
            bool allowMicroRelaxation)
        {
            double configuredMinimum =
                Math.Max(0.05, configuredMinimumAtr);

            double safeReferenceAtr =
                IsFinitePositive(riskReferenceAtr)
                    ? riskReferenceAtr
                    : 0;

            double spreadReference =
                safeReferenceAtr > 0
                    ? Math.Max(
                        Math.Max(0, pipSize),
                        safeReferenceAtr)
                    : 0;

            double spreadMinimum =
                spreadReference > 0
                    ? Math.Max(0, spread) /
                      spreadReference /
                      Math.Max(0.02, maximumSpreadToStopRiskRatio)
                    : double.PositiveInfinity;

            if (!allowMicroRelaxation ||
                !IsFinitePositive(microAtr) ||
                !IsFinitePositive(safeReferenceAtr))
            {
                return Math.Max(
                    configuredMinimum,
                    spreadMinimum);
            }

            double microMinimum =
                Math.Max(
                    MicroStopRiskAtrFloor,
                    microAtr *
                    MicroStopM1AtrMultiplier /
                    safeReferenceAtr);

            return Math.Max(
                microMinimum,
                spreadMinimum);
        }

        public static bool IsWithinPlanningRiskEnvelope(
            double riskAtr,
            double atr,
            double minimumSlAtr,
            double maximumSlAtr,
            double maximumStructuralStopAtr,
            double spread,
            double pipSize,
            double maximumSpreadToStopRiskRatio)
        {
            if (double.IsNaN(riskAtr) ||
                double.IsInfinity(riskAtr) ||
                riskAtr < 0 ||
                double.IsNaN(atr) ||
                double.IsInfinity(atr) ||
                atr <= 0)
                return false;

            double configuredMinimum =
                Math.Max(
                    0.05,
                    minimumSlAtr);

            double spreadMinimum =
                Math.Max(0, spread) /
                Math.Max(
                    Math.Max(0, pipSize),
                    atr) /
                Math.Max(
                    0.02,
                    maximumSpreadToStopRiskRatio);

            double effectiveMinimum =
                Math.Max(
                    configuredMinimum,
                    spreadMinimum);

            double effectiveMaximum =
                EffectiveMaximumStopRiskAtr(
                    configuredMinimum,
                    maximumSlAtr,
                    maximumStructuralStopAtr);

            return riskAtr >= effectiveMinimum &&
                   riskAtr <= effectiveMaximum;
        }
        private static bool IsFinitePositive(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

    }
}