using System;

namespace cAlgo
{
    internal readonly struct PendingFillExitResolution
    {
        public bool Allowed { get; }
        public double Price { get; }
        public string Source { get; }

        public PendingFillExitResolution(
            bool allowed,
            double price,
            string source)
        {
            Allowed = allowed;
            Price = price;
            Source = source ?? "";
        }
    }

    internal static class PendingFillExitResolutionRule
    {
        public static PendingFillExitResolution ResolveProtectiveStop(
            int direction,
            double actualEntry,
            double market,
            double plannedStop,
            double brokerStop,
            double minimumDistance)
        {
            bool plannedValid =
                IsEntrySideStop(direction, actualEntry, plannedStop) &&
                LiveExitGeometryRule.IsProtectiveStop(
                    direction,
                    actualEntry,
                    market,
                    plannedStop,
                    minimumDistance);

            bool brokerValid =
                IsEntrySideStop(direction, actualEntry, brokerStop) &&
                LiveExitGeometryRule.IsProtectiveStop(
                    direction,
                    actualEntry,
                    market,
                    brokerStop,
                    minimumDistance);

            if (!plannedValid && !brokerValid)
                return new PendingFillExitResolution(
                    false,
                    0,
                    "NO SAFE STOP");

            if (plannedValid && !brokerValid)
                return new PendingFillExitResolution(
                    true,
                    plannedStop,
                    "ABSOLUTE PLAN STOP");

            if (!plannedValid && brokerValid)
                return new PendingFillExitResolution(
                    true,
                    brokerStop,
                    "BROKER CONFIRMED STOP");

            bool brokerMoreProtective =
                direction == 1
                    ? brokerStop > plannedStop
                    : brokerStop < plannedStop;

            return new PendingFillExitResolution(
                true,
                brokerMoreProtective
                    ? brokerStop
                    : plannedStop,
                brokerMoreProtective
                    ? "BROKER MORE PROTECTIVE"
                    : "ABSOLUTE PLAN STOP");
        }

        public static PendingFillExitResolution ResolveProgressiveTarget(
            int direction,
            double actualEntry,
            double market,
            double plannedTarget,
            double brokerTarget,
            double minimumForwardDistance)
        {
            bool plannedValid =
                LiveExitGeometryRule.ValidateLiveTarget(
                    direction,
                    actualEntry,
                    market,
                    plannedTarget,
                    1.0,
                    minimumForwardDistance).Allowed;

            bool brokerValid =
                LiveExitGeometryRule.ValidateLiveTarget(
                    direction,
                    actualEntry,
                    market,
                    brokerTarget,
                    1.0,
                    minimumForwardDistance).Allowed;

            if (!plannedValid && !brokerValid)
                return new PendingFillExitResolution(
                    false,
                    0,
                    "NO SAFE TARGET");

            if (plannedValid && !brokerValid)
                return new PendingFillExitResolution(
                    true,
                    plannedTarget,
                    "ABSOLUTE PLAN TARGET");

            if (!plannedValid && brokerValid)
                return new PendingFillExitResolution(
                    true,
                    brokerTarget,
                    "BROKER CONFIRMED TARGET");

            bool brokerMoreProgressive =
                direction == 1
                    ? brokerTarget > plannedTarget
                    : brokerTarget < plannedTarget;

            return new PendingFillExitResolution(
                true,
                brokerMoreProgressive
                    ? brokerTarget
                    : plannedTarget,
                brokerMoreProgressive
                    ? "BROKER MORE PROGRESSIVE"
                    : "ABSOLUTE PLAN TARGET");
        }

        private static bool IsEntrySideStop(
            int direction,
            double entry,
            double stop)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFinitePricePositive(entry) ||
                !IsFinitePricePositive(stop))
                return false;

            return direction == 1
                ? stop < entry
                : stop > entry;
        }

        private static bool IsFinitePricePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}
