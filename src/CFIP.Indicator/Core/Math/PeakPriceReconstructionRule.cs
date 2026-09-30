using System;

namespace cAlgo
{
    internal static class PeakPriceReconstructionRule
    {
        public static bool TryResolve(
            int direction,
            double entry,
            double currentMarket,
            int startIndex,
            int endIndex,
            Func<int, double> highAt,
            Func<int, double> lowAt,
            out double peakPrice)
        {
            peakPrice = 0;

            if ((direction != 1 &&
                 direction != -1) ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(currentMarket) ||
                startIndex < 0 ||
                endIndex < startIndex ||
                highAt == null ||
                lowAt == null)
                return false;

            double peak =
                entry;

            if (direction == 1)
                peak = Math.Max(peak, currentMarket);
            else
                peak = Math.Min(peak, currentMarket);

            for (int index = startIndex;
                 index <= endIndex;
                 index++)
            {
                double extreme =
                    direction == 1
                        ? highAt(index)
                        : lowAt(index);

                if (!NumericGuards.IsFinitePositive(extreme))
                    continue;

                peak =
                    direction == 1
                        ? Math.Max(peak, extreme)
                        : Math.Min(peak, extreme);
            }

            if (!IsFinitePositive(peak))
                return false;

            peakPrice = peak;
            return true;
        }

    }
}
