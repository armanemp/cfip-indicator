using System;

namespace cAlgo
{
    internal enum OrderBlockLifecycleState
    {
        Fresh,
        Mitigated,
        Broken
    }

    /// <summary>
    /// Canonical lifecycle owner for Order Block source age and mitigation
    /// transitions. Source geometry/qualification remains owned by
    /// OrderBlockRule.
    /// </summary>
    internal static class OrderBlockLifecycleRule
    {
        public const double MinimumRetainedRatio = 0.05;

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

        public static OrderBlockLifecycleState Classify(
            bool partiallyMitigated,
            double remainingRatio)
        {
            if (!IsFinite(remainingRatio) ||
                remainingRatio <= MinimumRetainedRatio)
                return OrderBlockLifecycleState.Broken;

            return partiallyMitigated
                ? OrderBlockLifecycleState.Mitigated
                : OrderBlockLifecycleState.Fresh;
        }

        public static double ResolveMitigationProbe(
            int direction,
            double open,
            double close,
            double high,
            double low,
            bool breakByWicks)
        {
            if (direction != 1 && direction != -1)
                return double.NaN;

            if (!IsFinite(open) ||
                !IsFinite(close) ||
                !IsFinite(high) ||
                !IsFinite(low))
                return double.NaN;

            return direction == 1
                ? (breakByWicks
                    ? low
                    : Math.Min(open, close))
                : (breakByWicks
                    ? high
                    : Math.Max(open, close));
        }

        public static bool IsFullyMitigated(
            int direction,
            double zoneLow,
            double zoneHigh,
            double probe)
        {
            if (!IsValidDirection(direction) ||
                !IsValidGeometry(zoneLow, zoneHigh) ||
                !IsFinite(probe))
                return false;

            return direction == 1
                ? probe <= zoneLow
                : probe >= zoneHigh;
        }

        public static bool TryApplyPartialMitigation(
            int direction,
            double zoneLow,
            double zoneHigh,
            double probe,
            double tickSize,
            out double managedLow,
            out double managedHigh,
            out bool partiallyMitigated,
            out double remainingRatio,
            out OrderBlockLifecycleState lifecycleState)
        {
            managedLow = zoneLow;
            managedHigh = zoneHigh;
            partiallyMitigated = false;
            remainingRatio = 0;
            lifecycleState = OrderBlockLifecycleState.Broken;

            if (!IsValidDirection(direction) ||
                !IsValidGeometry(zoneLow, zoneHigh) ||
                !IsFinitePositive(probe) ||
                !IsFinitePositive(tickSize))
                return false;

            if (IsFullyMitigated(
                    direction,
                    zoneLow,
                    zoneHigh,
                    probe))
                return false;

            if (direction == 1 &&
                probe < managedHigh)
            {
                managedHigh =
                    Math.Max(
                        managedLow,
                        probe);
                partiallyMitigated = true;
            }
            else if (direction == -1 &&
                     probe > managedLow)
            {
                managedLow =
                    Math.Min(
                        managedHigh,
                        probe);
                partiallyMitigated = true;
            }

            double originalWidth =
                zoneHigh -
                zoneLow;

            double managedWidth =
                managedHigh -
                managedLow;

            remainingRatio =
                managedWidth /
                Math.Max(
                    tickSize,
                    originalWidth);

            lifecycleState =
                Classify(
                    partiallyMitigated,
                    remainingRatio);

            if (lifecycleState ==
                OrderBlockLifecycleState.Broken)
                return false;

            return managedLow < managedHigh &&
                   managedWidth > tickSize;
        }

        private static bool IsValidGeometry(
            double low,
            double high)
        {
            return
                IsFinite(low) &&
                IsFinite(high) &&
                low < high;
        }

        private static bool IsValidDirection(int direction)
        {
            return direction == 1 || direction == -1;
        }

        private static bool IsFinitePositive(double value)
        {
            return
                IsFinite(value) &&
                value > 0;
        }

        private static bool IsFinite(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}
