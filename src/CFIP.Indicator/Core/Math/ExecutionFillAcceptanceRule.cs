using System;

namespace cAlgo
{
    internal static class ExecutionFillAcceptanceRule
    {
        public static bool IsAcceptable(
            int direction,
            double requestedEntry,
            double actualFill,
            double atr,
            double maxAdverseExtensionAtr,
            bool allowFavorable)
        {
            if ((direction != 1 && direction != -1) ||
                !IsPositiveFiniteValue(requestedEntry) ||
                !IsPositiveFiniteValue(actualFill) ||
                !IsPositiveFiniteValue(atr) ||
                double.IsNaN(maxAdverseExtensionAtr) ||
                double.IsInfinity(maxAdverseExtensionAtr) ||
                maxAdverseExtensionAtr < 0)
                return false;

            bool favorable =
                direction == 1
                    ? actualFill < requestedEntry
                    : actualFill > requestedEntry;

            if (allowFavorable && favorable)
                return true;

            double allowedAdverseDistance =
                atr *
                Math.Max(
                    0.10,
                    maxAdverseExtensionAtr);

            if (!IsPositiveFiniteValue(allowedAdverseDistance))
                return false;

            double adverseDistance =
                direction == 1
                    ? actualFill - requestedEntry
                    : requestedEntry - actualFill;

            if (adverseDistance < 0)
                return allowFavorable;

            return adverseDistance <= allowedAdverseDistance;
        }

        private static bool IsPositiveFiniteValue(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}
