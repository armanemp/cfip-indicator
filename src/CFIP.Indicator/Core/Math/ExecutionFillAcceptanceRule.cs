using System;

namespace cAlgo
{
    internal static class ExecutionFillAcceptanceRule
    {
        public static bool IsAcceptable(
            double requestedEntry,
            double actualFill,
            double atr,
            double maximumExtensionAtr)
        {
            if (!IsFinitePositive(requestedEntry) ||
                !IsFinitePositive(actualFill) ||
                !IsFinitePositive(atr) ||
                double.IsNaN(maximumExtensionAtr) ||
                double.IsInfinity(maximumExtensionAtr))
                return false;

            double allowedDistance =
                atr *
                Math.Max(
                    0.10,
                    maximumExtensionAtr);

            return
                !double.IsNaN(allowedDistance) &&
                !double.IsInfinity(allowedDistance) &&
                allowedDistance > 0 &&
                Math.Abs(actualFill - requestedEntry) <=
                allowedDistance;
        }

        private static bool IsFinitePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}