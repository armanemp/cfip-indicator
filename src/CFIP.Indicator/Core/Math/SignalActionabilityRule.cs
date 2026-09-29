using System;

namespace cAlgo
{
    internal readonly struct SignalActionabilityResult
    {
        public bool Allowed { get; }
        public string Reason { get; }

        public SignalActionabilityResult(
            bool allowed,
            string reason)
        {
            Allowed = allowed;
            Reason =
                string.IsNullOrWhiteSpace(reason)
                    ? (allowed ? "ACTIONABLE" : "BLOCKED")
                    : reason;
        }
    }

    internal static class SignalActionabilityRule
    {
        internal static SignalActionabilityResult Evaluate(
            int direction,
            double tp1RR,
            double minimumRR,
            int locationQuality,
            int hardMinimumLocation,
            int divergenceDirection,
            int divergenceQuality,
            bool opposingRegularDivergence,
            int divergenceAgeBars,
            int planAgeBars,
            bool executionReady,
            bool pendingExists,
            double market,
            double entryZoneLow,
            double entryZoneHigh,
            double atr)
        {
            if (direction != 1 &&
                direction != -1)
                return new SignalActionabilityResult(
                    false,
                    "NO DIRECTION");

            if (pendingExists)
                return new SignalActionabilityResult(
                    false,
                    "PENDING ORDER EXISTS");

            if (!executionReady)
                return new SignalActionabilityResult(
                    false,
                    "ENTRY NOT READY");

            if (planAgeBars > 2)
                return new SignalActionabilityResult(
                    false,
                    "SIGNAL LATE");

            if (tp1RR < Math.Max(0.5, minimumRR))
                return new SignalActionabilityResult(
                    false,
                    "RR BELOW MINIMUM");

            if (locationQuality < Math.Max(30, hardMinimumLocation))
                return new SignalActionabilityResult(
                    false,
                    "ENTRY LOCATION WEAK");

            if (opposingRegularDivergence &&
                divergenceDirection == -direction &&
                divergenceQuality >= 72 &&
                divergenceAgeBars <= 18)
                return new SignalActionabilityResult(
                    false,
                    "OPPOSING REGULAR DIVERGENCE");

            if (divergenceDirection == -direction &&
                divergenceQuality >= 86 &&
                divergenceAgeBars <= 8)
                return new SignalActionabilityResult(
                    false,
                    "STRONG OPPOSING DIVERGENCE");

            if (!IsFinitePositive(atr))
                return new SignalActionabilityResult(
                    false,
                    "ATR INVALID");

            double zoneLow =
                Math.Min(
                    entryZoneLow,
                    entryZoneHigh);

            double zoneHigh =
                Math.Max(
                    entryZoneLow,
                    entryZoneHigh);

            if (!IsFinitePositive(zoneLow) ||
                !IsFinitePositive(zoneHigh) ||
                zoneHigh <= zoneLow)
                return new SignalActionabilityResult(
                    false,
                    "ENTRY ZONE INVALID");

            double lateAllowance =
                atr *
                0.30;

            if (direction == 1 &&
                market >
                zoneHigh + lateAllowance)
                return new SignalActionabilityResult(
                    false,
                    "PRICE ABOVE ENTRY ZONE • LATE");

            if (direction == -1 &&
                market <
                zoneLow - lateAllowance)
                return new SignalActionabilityResult(
                    false,
                    "PRICE BELOW ENTRY ZONE • LATE");

            return new SignalActionabilityResult(
                true,
                "ACTIONABLE");
        }

        private static bool IsFinitePositive(
            double value)
        {
            return
                value > 0 &&
                !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}
