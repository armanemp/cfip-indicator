using System;

namespace cAlgo
{
    internal static class EntryGeometryRule
    {
        public static EntryGeometrySnapshot Evaluate(
            int direction,
            ExecutionMode preferredMode,
            double market,
            double zoneLow,
            double zoneHigh,
            double zoneTolerance,
            double idealEntry,
            double trigger,
            double fallbackEntry,
            double atr,
            double tickSize,
            double pipSize,
            bool allowBreakout,
            bool continuation,
            double maximumEntryExtensionAtr,
            double maximumEntryDistanceAtr)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFinitePositive(market) ||
                !IsFinitePositive(atr) ||
                !IsFinitePositive(zoneLow) ||
                !IsFinitePositive(zoneHigh) ||
                zoneHigh < zoneLow)
                return EntryGeometrySnapshot.Invalid("INVALID ENTRY GEOMETRY");

            double safeTolerance =
                Math.Max(
                    0,
                    IsFiniteNonNegative(zoneTolerance)
                        ? zoneTolerance
                        : 0);

            double safeIdeal =
                IsFinitePositive(idealEntry)
                    ? idealEntry
                    : 0;

            double safeTrigger =
                IsFinitePositive(trigger)
                    ? trigger
                    : 0;

            double safeFallback =
                IsFinitePositive(fallbackEntry)
                    ? fallbackEntry
                    : safeIdeal;

            double triggerTolerance =
                Math.Max(
                    0,
                    ResolveTriggerTolerance(
                        tickSize,
                        pipSize));

            bool triggerReached =
                safeTrigger > 0 &&
                (direction == 1
                    ? market >= safeTrigger - triggerTolerance
                    : market <= safeTrigger + triggerTolerance);

            bool insideZone =
                market >= zoneLow - safeTolerance &&
                market <= zoneHigh + safeTolerance;

            ExecutionMode mode;

            if (triggerReached &&
                allowBreakout)
            {
                mode = ExecutionMode.BreakoutMarket;
            }
            else if (continuation)
            {
                mode = ExecutionMode.WaitingForTrigger;
            }
            else if (insideZone)
            {
                mode = ExecutionMode.RetestMarket;
            }
            else
            {
                mode = ExecutionMode.None;
            }

            // A caller may supply the already-resolved mode when it is
            // reconstructing a materialized plan. Do not allow that mode to
            // invent a live state that contradicts the canonical quote geometry.
            if (preferredMode == ExecutionMode.BreakoutMarket &&
                mode != ExecutionMode.BreakoutMarket)
                mode = ExecutionMode.None;
            else if (preferredMode == ExecutionMode.RetestMarket &&
                     mode == ExecutionMode.None &&
                     insideZone)
                mode = ExecutionMode.RetestMarket;

            double anchor =
                ResolveAnchor(
                    mode,
                    safeTrigger,
                    safeIdeal,
                    safeFallback);

            double entryDistanceAtr =
                IsFinitePositive(anchor)
                    ? Math.Abs(market - anchor) / atr
                    : 0;

            double distanceFromZone =
                insideZone
                    ? 0
                    : market < zoneLow
                        ? zoneLow - market
                        : market - zoneHigh;

            double zoneDistanceAtr =
                Math.Max(
                    0,
                    distanceFromZone / atr);

            double actualEntry =
                mode == ExecutionMode.WaitingForTrigger
                    ? safeFallback
                    : market;

            double triggerExtensionAtr =
                mode == ExecutionMode.BreakoutMarket &&
                safeTrigger > 0 &&
                ((direction == 1 && market > safeTrigger) ||
                 (direction == -1 && market < safeTrigger))
                    ? Math.Abs(market - safeTrigger) / atr
                    : 0;

            bool late =
                IsLate(
                    mode,
                    triggerExtensionAtr,
                    entryDistanceAtr,
                    maximumEntryExtensionAtr,
                    maximumEntryDistanceAtr);

            string reason =
                mode == ExecutionMode.BreakoutMarket
                    ? (late ? "BREAKOUT LATE" : "BREAKOUT")
                    : mode == ExecutionMode.RetestMarket
                        ? (late ? "RETEST LATE" : "RETEST")
                        : mode == ExecutionMode.WaitingForTrigger
                            ? "WAITING FOR TRIGGER"
                            : "OUTSIDE EXECUTION WINDOW";

            return new EntryGeometrySnapshot(
                direction,
                mode,
                true,
                insideZone,
                triggerReached,
                late,
                market,
                zoneLow,
                zoneHigh,
                safeTolerance,
                safeIdeal,
                safeTrigger,
                anchor,
                actualEntry,
                entryDistanceAtr,
                zoneDistanceAtr,
                triggerExtensionAtr,
                reason);
        }

        public static double ResolveAnchor(
            ExecutionMode mode,
            double trigger,
            double ideal,
            double fallback)
        {
            double anchor =
                mode == ExecutionMode.BreakoutMarket
                    ? trigger
                    : ideal;

            return IsFinitePositive(anchor)
                ? anchor
                : fallback;
        }

        public static bool IsLate(
            ExecutionMode mode,
            double triggerExtensionAtr,
            double entryDistanceAtr,
            double maximumExtensionAtr,
            double maximumDistanceAtr)
        {
            if (mode == ExecutionMode.None ||
                mode == ExecutionMode.WaitingForTrigger)
                return false;

            if (mode == ExecutionMode.BreakoutMarket)
                return triggerExtensionAtr >
                    Math.Max(
                        0.10,
                        IsFiniteNonNegative(maximumExtensionAtr)
                            ? maximumExtensionAtr
                            : 0);

            return entryDistanceAtr >
                Math.Max(
                    0.05,
                    IsFiniteNonNegative(maximumDistanceAtr)
                        ? maximumDistanceAtr
                        : 0);
        }

        public static bool IsInsideZone(
            double market,
            double zoneLow,
            double zoneHigh,
            double zoneTolerance)
        {
            if (!IsFinitePositive(market) ||
                !IsFinitePositive(zoneLow) ||
                !IsFinitePositive(zoneHigh) ||
                zoneHigh < zoneLow)
                return false;

            double tolerance =
                IsFiniteNonNegative(zoneTolerance)
                    ? zoneTolerance
                    : 0;

            return
                market >= zoneLow - tolerance &&
                market <= zoneHigh + tolerance;
        }

        public static bool IsTriggerReached(
            int direction,
            double market,
            double trigger,
            double tickSize,
            double pipSize)
        {
            if ((direction != 1 && direction != -1) ||
                !IsFinitePositive(market) ||
                !IsFinitePositive(trigger))
                return false;

            double tolerance =
                ResolveTriggerTolerance(
                    tickSize,
                    pipSize);

            return direction == 1
                ? market >= trigger - tolerance
                : market <= trigger + tolerance;
        }

        private static double ResolveTriggerTolerance(
            double tickSize,
            double pipSize)
        {
            double safeTick =
                IsFiniteNonNegative(tickSize)
                    ? tickSize
                    : 0;
            double safePip =
                IsFiniteNonNegative(pipSize)
                    ? pipSize
                    : 0;

            return Math.Max(
                safeTick,
                safePip * 0.10);
        }

        private static bool IsFinitePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}
