using System;

namespace cAlgo
{
    internal static class TargetRewardEnvelopeRule
    {
        internal const double HtfMaximumExtensionAtrCap = 24.0;

        public static double MaximumReachableRR(
            double risk,
            double atr,
            double maximumTargetExtensionAtr,
            double maximumRewardRR,
            bool allowHtfExtension = false)
        {
            if (risk <= 0 ||
                atr <= 0 ||
                double.IsNaN(risk) ||
                double.IsInfinity(risk) ||
                double.IsNaN(atr) ||
                double.IsInfinity(atr))
                return 0;

            double effectiveExtensionAtr =
                ResolveMaximumExtensionAtr(
                    risk,
                    atr,
                    maximumTargetExtensionAtr,
                    maximumRewardRR,
                    allowHtfExtension);

            if (effectiveExtensionAtr <= 0)
                return 0;

            double extension =
                atr *
                effectiveExtensionAtr;

            double extensionRR =
                RiskRewardMathRule.NominalRRFromDistance(
                    extension,
                    risk,
                    RiskRewardMathRule.DistanceFloor);

            if (extensionRR <= 0 ||
                double.IsNaN(extensionRR) ||
                double.IsInfinity(extensionRR))
                return 0;

            double safeMaximumRewardRR =
                double.IsNaN(maximumRewardRR) ||
                double.IsInfinity(maximumRewardRR)
                    ? 0
                    : Math.Max(
                        0,
                        maximumRewardRR);

            return Math.Min(
                safeMaximumRewardRR,
                extensionRR);
        }

        public static bool CanReachStage(
            double requiredRR,
            double risk,
            double atr,
            double maximumTargetExtensionAtr,
            double maximumRewardRR,
            bool allowHtfExtension = false)
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
                    maximumRewardRR,
                    allowHtfExtension) +
                1e-12;
        }

        public static double ResolveMaximumExtensionAtr(
            double risk,
            double atr,
            double baseMaximumExtensionAtr,
            double maximumRewardRR,
            bool allowHtfExtension)
        {
            if (risk <= 0 ||
                atr <= 0 ||
                double.IsNaN(risk) ||
                double.IsInfinity(risk) ||
                double.IsNaN(atr) ||
                double.IsInfinity(atr))
                return 0;

            double baseExtension =
                Math.Max(
                    1.0,
                    double.IsNaN(baseMaximumExtensionAtr) ||
                    double.IsInfinity(baseMaximumExtensionAtr)
                        ? 0
                        : baseMaximumExtensionAtr);

            if (!allowHtfExtension)
                return baseExtension;

            double safeMaximumRewardRR =
                double.IsNaN(maximumRewardRR) ||
                double.IsInfinity(maximumRewardRR)
                    ? 0
                    : Math.Max(
                        0,
                        maximumRewardRR);

            double rrBoundExtension =
                safeMaximumRewardRR *
                risk /
                atr;

            return Math.Min(
                HtfMaximumExtensionAtrCap,
                Math.Max(
                    baseExtension,
                    rrBoundExtension));
        }
    }
}
