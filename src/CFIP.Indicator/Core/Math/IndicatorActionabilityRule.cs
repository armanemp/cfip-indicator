using System;

namespace cAlgo
{
    internal readonly struct IndicatorActionabilityResult
    {
        public bool Allowed { get; }
        public string Reason { get; }

        public IndicatorActionabilityResult(
            bool allowed,
            string reason)
        {
            Allowed = allowed;
            Reason =
                string.IsNullOrWhiteSpace(reason)
                    ? string.Empty
                    : reason;
        }

        public static IndicatorActionabilityResult Allow()
        {
            return new IndicatorActionabilityResult(
                true,
                string.Empty);
        }

        public static IndicatorActionabilityResult Block(
            string reason)
        {
            return new IndicatorActionabilityResult(
                false,
                reason);
        }
    }

    internal static class IndicatorActionabilityRule
    {
        public static IndicatorActionabilityResult Evaluate(
            string regime,
            int quality,
            int conflict)
        {
            string normalizedRegime =
                regime ?? "UNKNOWN";

            if (string.Equals(
                    normalizedRegime,
                    "COMPRESSION",
                    StringComparison.OrdinalIgnoreCase))
                return IndicatorActionabilityResult.Block(
                    "INDICATOR FUSION • COMPRESSION");

            int minimumQuality =
                string.Equals(
                    normalizedRegime,
                    "RANGE",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    normalizedRegime,
                    "TRANSITION",
                    StringComparison.OrdinalIgnoreCase)
                    ? 58
                    : 60;

            int maximumConflict =
                string.Equals(
                    normalizedRegime,
                    "RANGE",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    normalizedRegime,
                    "TRANSITION",
                    StringComparison.OrdinalIgnoreCase)
                    ? 55
                    : 52;

            quality =
                NumericGuards.ClampInt(
                    quality,
                    0,
                    100);

            conflict =
                NumericGuards.ClampInt(
                    conflict,
                    0,
                    100);

            if (quality < minimumQuality)
                return IndicatorActionabilityResult.Block(
                    "INDICATOR FUSION • QUALITY");

            if (conflict > maximumConflict)
                return IndicatorActionabilityResult.Block(
                    "INDICATOR FUSION • CONFLICT");

            return IndicatorActionabilityResult.Allow();
        }
    }
}
