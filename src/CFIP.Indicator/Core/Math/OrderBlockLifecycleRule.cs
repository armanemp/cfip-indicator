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

        public static bool IsOrderBlockAgeValid(
            int createdIndex,
            int currentIndex,
            int maximumAgeBars)
        {
            return createdIndex >= 0 &&
                   currentIndex >= createdIndex &&
                   maximumAgeBars >= 0 &&
                   currentIndex - createdIndex <= maximumAgeBars;
        }

        public static OrderBlockLifecycleState ClassifyOrderBlockLifecycle(
            bool partiallyMitigated,
            double remainingRatio)
        {
            if (!IsFiniteOrderBlockLifecycleValue(remainingRatio) ||
                remainingRatio <= MinimumRetainedRatio)
                return OrderBlockLifecycleState.Broken;

            return partiallyMitigated
                ? OrderBlockLifecycleState.Mitigated
                : OrderBlockLifecycleState.Fresh;
        }

        public static double ResolveOrderBlockMitigationProbe(
            int direction,
            double open,
            double close,
            double high,
            double low,
            bool breakByWicks)
        {
            if (direction != 1 && direction != -1)
                return double.NaN;

            if (!IsFiniteOrderBlockLifecycleValue(open) ||
                !IsFiniteOrderBlockLifecycleValue(close) ||
                !IsFiniteOrderBlockLifecycleValue(high) ||
                !IsFiniteOrderBlockLifecycleValue(low))
                return double.NaN;

            return direction == 1
                ? (breakByWicks
                    ? low
                    : Math.Min(open, close))
                : (breakByWicks
                    ? high
                    : Math.Max(open, close));
        }

        public static bool IsOrderBlockFullyMitigated(
            int direction,
            double zoneLow,
            double zoneHigh,
            double probe)
        {
            if (!IsValidOrderBlockLifecycleDirection(direction) ||
                !IsValidOrderBlockLifecycleGeometry(zoneLow, zoneHigh) ||
                !IsFiniteOrderBlockLifecycleValue(probe))
                return false;

            return direction == 1
                ? probe <= zoneLow
                : probe >= zoneHigh;
        }

        public static bool TryApplyOrderBlockPartialMitigation(
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

            if (!IsValidOrderBlockLifecycleDirection(direction) ||
                !IsValidOrderBlockLifecycleGeometry(zoneLow, zoneHigh) ||
                !IsFinitePositiveOrderBlockLifecycleValue(probe) ||
                !IsFinitePositiveOrderBlockLifecycleValue(tickSize))
                return false;

            if (IsOrderBlockFullyMitigated(
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
                ClassifyOrderBlockLifecycle(
                    partiallyMitigated,
                    remainingRatio);

            if (lifecycleState ==
                OrderBlockLifecycleState.Broken)
                return false;

            return managedLow < managedHigh &&
                   managedWidth > tickSize;
        }

        private static bool IsValidOrderBlockLifecycleGeometry(
            double low,
            double high)
        {
            return
                IsFiniteOrderBlockLifecycleValue(low) &&
                IsFiniteOrderBlockLifecycleValue(high) &&
                low < high;
        }

        private static bool IsValidOrderBlockLifecycleDirection(int direction)
        {
            return direction == 1 || direction == -1;
        }

        private static bool IsFinitePositiveOrderBlockLifecycleValue(double value)
        {
            return
                IsFiniteOrderBlockLifecycleValue(value) &&
                value > 0;
        }

        private static bool IsFiniteOrderBlockLifecycleValue(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}
