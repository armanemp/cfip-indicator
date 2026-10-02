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
