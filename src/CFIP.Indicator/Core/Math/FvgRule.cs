using System;

namespace cAlgo
{
    /// <summary>
    /// Deterministic mathematical owner for Fair Value Gap geometry.
    /// The caller supplies only closed-bar values.
    /// </summary>
    internal static class FvgRule
    {
        public static bool TryGetThreeBarGap(
            int direction,
            double olderHigh,
            double olderLow,
            double currentHigh,
            double currentLow,
            out double low,
            out double high,
            out double gap)
        {
            low = high = gap = 0;

            if ((direction != 1 && direction != -1) ||
                !FinitePositive(olderHigh) ||
                !FinitePositive(olderLow) ||
                !FinitePositive(currentHigh) ||
                !FinitePositive(currentLow) ||
                olderLow > olderHigh ||
                currentLow > currentHigh)
                return false;

            if (direction == 1)
            {
                if (currentLow <= olderHigh)
                    return false;

                low = olderHigh;
                high = currentLow;
            }
            else
            {
                if (currentHigh >= olderLow)
                    return false;

                low = currentHigh;
                high = olderLow;
            }

            gap = high - low;
            return IsValidGeometry(low, high, gap);
        }

        public static bool TryGetTwoBarGap(
            int direction,
            double previousHigh,
            double previousLow,
            double currentHigh,
            double currentLow,
            out double low,
            out double high,
            out double gap)
        {
            low = high = gap = 0;

            if ((direction != 1 && direction != -1) ||
                !FinitePositive(previousHigh) ||
                !FinitePositive(previousLow) ||
                !FinitePositive(currentHigh) ||
                !FinitePositive(currentLow) ||
                previousLow > previousHigh ||
                currentLow > currentHigh)
                return false;

            if (direction == 1)
            {
                if (currentLow <= previousHigh)
                    return false;

                low = previousHigh;
                high = currentLow;
            }
            else
            {
                if (currentHigh >= previousLow)
                    return false;

                low = currentHigh;
                high = previousLow;
            }

            gap = high - low;
            return IsValidGeometry(low, high, gap);
        }

        public static bool MeetsMinimumGap(
            double gap,
            double creationAtr,
            double minimumAtr)
        {
            return FinitePositive(gap) &&
                   FinitePositive(creationAtr) &&
                   FinitePositive(minimumAtr) &&
                   gap >= creationAtr * minimumAtr;
        }

        public static bool IsOverlapInclusive(
            double low,
            double high,
            double otherLow,
            double otherHigh)
        {
            return IsValidGeometry(low, high, high - low) &&
                   IsValidGeometry(
                       otherLow,
                       otherHigh,
                       otherHigh - otherLow) &&
                   high >= otherLow &&
                   otherHigh >= low;
        }

        public static bool IsFullyFilled(
            int direction,
            double zoneLow,
            double zoneHigh,
            double fillPrice)
        {
            if (!IsValidGeometry(
                    zoneLow,
                    zoneHigh,
                    zoneHigh - zoneLow) ||
                !FinitePositive(fillPrice))
                return false;

            return direction == 1
                ? fillPrice <= zoneLow
                : direction == -1 &&
                  fillPrice >= zoneHigh;
        }

        public static bool TryApplyPartialMitigation(
            int direction,
            double zoneLow,
            double zoneHigh,
            double fillPrice,
            double tickSize,
            out double managedLow,
            out double managedHigh)
        {
            managedLow = zoneLow;
            managedHigh = zoneHigh;

            if (!IsValidGeometry(
                    zoneLow,
                    zoneHigh,
                    zoneHigh - zoneLow) ||
                !FinitePositive(fillPrice) ||
                !FinitePositive(tickSize) ||
                IsFullyFilled(
                    direction,
                    zoneLow,
                    zoneHigh,
                    fillPrice))
                return false;

            if (direction == 1 &&
                fillPrice < managedHigh)
            {
                managedHigh =
                    Math.Max(
                        managedLow,
                        fillPrice);
            }
            else if (direction == -1 &&
                     fillPrice > managedLow)
            {
                managedLow =
                    Math.Min(
                        managedHigh,
                        fillPrice);
            }

            return managedLow < managedHigh &&
                   managedHigh - managedLow > tickSize;
        }

        public static string Identity(
            int direction,
            int createdIndex,
            bool twoBarImbalance)
        {
            if ((direction != 1 && direction != -1) ||
                createdIndex < 0)
                return string.Empty;

            return direction.ToString() +
                   ":" +
                   createdIndex.ToString() +
                   ":" +
                   (twoBarImbalance ? "2B" : "3B");
        }

        private static bool IsValidGeometry(
            double low,
            double high,
            double gap)
        {
            return FinitePositive(low) &&
                   FinitePositive(high) &&
                   FinitePositive(gap) &&
                   low < high;
        }

        private static bool FinitePositive(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
