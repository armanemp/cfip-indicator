using System;

namespace cAlgo
{
    internal static class TargetRewardEnvelopeRule
    {
        public static double MaximumReachableRR(
            double risk,
            double atr,
            double maximumTargetExtensionAtr,
            double maximumRewardRR)
        {
            if (risk <= 0 ||
                atr <= 0 ||
                double.IsNaN(risk) ||
                double.IsInfinity(risk) ||
                double.IsNaN(atr) ||
                double.IsInfinity(atr))
                return 0;

            double extension =
                atr *
                Math.Max(
                    1.0,
                    maximumTargetExtensionAtr);

            double extensionRR =
                RiskRewardMathRule.NominalRRFromDistance(
                    extension,
                    risk,
                    RiskRewardMathRule.DistanceFloor);

            if (extensionRR <= 0 ||
                double.IsNaN(extensionRR) ||
                double.IsInfinity(extensionRR))
                return 0;

            return Math.Min(
                Math.Max(0, maximumRewardRR),
                extensionRR);
        }

        public static bool CanReachStage(
            double requiredRR,
            double risk,
            double atr,
            double maximumTargetExtensionAtr,
            double maximumRewardRR)
        {
            if (requiredRR <= 0 ||
                double.IsNaN(requiredRR) ||
                double.IsInfinity(requiredRR))
                return false;

            return
                requiredRR <=
                MaximumReachableRR(
                    risk,
                    atr,
                    maximumTargetExtensionAtr,
                    maximumRewardRR) +
                1e-12;
        }
    }
}
