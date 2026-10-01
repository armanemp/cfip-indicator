using System;

namespace cAlgo
{
    internal static class DmiBiasRule
    {
        public static double Calculate(
            double diPlus,
            double diMinus)
        {
            if (!IsDmiFiniteNonNegative(diPlus) ||
                !IsDmiFiniteNonNegative(diMinus))
                return 0;

            double total = diPlus + diMinus;

            if (total <= 0)
                return 0;

            return Math.Max(
                -1,
                Math.Min(
                    1,
                    (diPlus - diMinus) / total));
        }

        private static bool IsDmiFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value >= 0;
        }
    }
}
