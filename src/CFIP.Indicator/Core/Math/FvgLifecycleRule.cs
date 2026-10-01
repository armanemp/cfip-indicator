using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical lifecycle owner for one closed-bar FVG mitigation step and
    /// bounded source-age semantics. Geometry remains owned by FvgRule.
    /// </summary>
    internal static class FvgLifecycleRule
    {
        public static bool IsAgeValid(
            int createdIndex,
            int currentIndex,
            int maximumAgeBars)
        {
            return createdIndex >= 0 &&
                   currentIndex >= createdIndex &&
                   maximumAgeBars >= 0 &&
                   currentIndex - createdIndex <= maximumAgeBars;
        }

        public static double ResolveFvgMitigationProbe(
            int direction,
            double open,
            double close,
            double low,
            double high,
            bool breakByWicks)
        {
            if (direction != 1 && direction != -1)
                return double.NaN;

            if (breakByWicks)
                return direction == 1
                    ? low
                    : high;

            return direction == 1
                ? Math.Min(open, close)
                : Math.Max(open, close);
        }

        public static bool TryApplyMitigationStep(
            int direction,
            double zoneLow,
            double zoneHigh,
            double probe,
            double tickSize,
            bool invalidateOnFullFill,
            out double managedLow,
            out double managedHigh,
            out bool fullyFilled)
        {
            managedLow = zoneLow;
            managedHigh = zoneHigh;
            fullyFilled = false;

            if ((direction != 1 && direction != -1) ||
                !IsFinitePositiveFvgLifecycleValue(zoneLow) ||
                !IsFinitePositiveFvgLifecycleValue(zoneHigh) ||
                zoneLow >= zoneHigh ||
                !IsFinitePositiveFvgLifecycleValue(probe) ||
                !IsFinitePositiveFvgLifecycleValue(tickSize))
                return false;

            fullyFilled =
                FvgRule.IsFullyFilled(
                    direction,
                    zoneLow,
                    zoneHigh,
                    probe);

            if (fullyFilled)
            {
                // When safety invalidation is disabled, retain the source
                // geometry as a historically valid zone instead of collapsing
                // it to zero width and silently dropping it.
                return !invalidateOnFullFill;
            }

            if (!FvgRule.TryApplyPartialMitigation(
                    direction,
                    zoneLow,
                    zoneHigh,
                    probe,
                    tickSize,
                    out managedLow,
                    out managedHigh))
            {
                // No partial movement can be applied (including a zone that
                // is already too narrow). The caller treats this as inactive.
                return false;
            }

            return managedLow < managedHigh &&
                   managedHigh - managedLow > tickSize;
        }

        private static bool IsFinitePositiveFvgLifecycleValue(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
