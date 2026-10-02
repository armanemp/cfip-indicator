using System;

namespace cAlgo
{
    internal static class RiskRewardPolicyRule
    {
        internal const double ExecutionMinimumFloor = 0.10;
        internal const double PlanBaseMinimumFloor = 0.50;

        public static double NormalizeMinimum(
            double configured,
            double hardFloor)
        {
            double safeConfigured =
                IsFiniteNonNegative(configured)
                    ? configured
                    : 0;

            double safeHardFloor =
                IsFiniteNonNegative(hardFloor)
                    ? hardFloor
                    : 0;

            return Math.Max(
                safeHardFloor,
                safeConfigured);
        }

        public static double NormalizeMaximum(
            double configured,
            double minimum)
        {
            double safeMinimum =
                NormalizeMinimum(
                    minimum,
                    0);

            if (!IsFiniteNonNegative(configured) ||
                configured <= 0)
                return safeMinimum;

            return Math.Max(
                safeMinimum,
                configured);
        }



        public static double CalculateAdaptiveMinimum(
            double baseMinimum,
            double riskAtr,
            double preferredStopRiskAtr,
            double cap,
            double multiplier)
        {
            double baseValue =
                NormalizeMinimum(
                    baseMinimum,
                    0);

            double safeRiskAtr =
                IsFiniteNonNegative(riskAtr)
                    ? riskAtr
                    : 0;

            double safePreferred =
                IsFiniteNonNegative(preferredStopRiskAtr)
                    ? preferredStopRiskAtr
                    : 0;

            double safeCap =
                IsFiniteNonNegative(cap)
                    ? cap
                    : 0;

            double safeMultiplier =
                IsFiniteNonNegative(multiplier)
                    ? multiplier
                    : 0;

            double excess =
                Math.Max(
                    0,
                    safeRiskAtr - safePreferred);

            return baseValue +
                Math.Min(
                    safeCap,
                    excess * safeMultiplier);
        }

        public static double CalculateEffectiveMinimum(
            double baseMinimum,
            double baseFactor,
            double absoluteReduction)
        {
            double baseValue =
                NormalizeMinimum(
                    baseMinimum,
                    0);

            double safeFactor =
                IsFiniteNonNegative(baseFactor)
                    ? baseFactor
                    : 0;

            double safeReduction =
                IsFiniteNonNegative(absoluteReduction)
                    ? absoluteReduction
                    : 0;

            return Math.Max(
                baseValue * safeFactor,
                baseValue - safeReduction);
        }

        public static bool MeetsMinimum(
            double nominalRR,
            double minimum)
        {
            if (!IsFiniteNonNegative(nominalRR))
                return false;

            return nominalRR + 1e-12 >=
                NormalizeMinimum(
                    minimum,
                    0);
        }

        public static bool IsWithinMaximum(
            double nominalRR,
            double maximum)
        {
            if (!IsFiniteNonNegative(nominalRR))
                return false;

            if (!IsFiniteNonNegative(maximum) ||
                maximum <= 0)
                return true;

            return nominalRR <=
                NormalizeMaximum(
                    maximum,
                    0) +
                1e-12;
        }

        public static bool IsFiniteNonNegative(
            double value)
        {
            return value >= 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
