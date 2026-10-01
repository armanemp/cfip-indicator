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
                    MarketRegimeIdentity.Compression,
                    StringComparison.OrdinalIgnoreCase))
                return IndicatorActionabilityResult.Block(
                    "INDICATOR FUSION • COMPRESSION");

            int minimumQuality =
                string.Equals(
                    normalizedRegime,
                    MarketRegimeIdentity.Range,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    normalizedRegime,
                    MarketRegimeIdentity.Transition,
                    StringComparison.OrdinalIgnoreCase)
                    ? ActionabilityThresholdPolicy.IndicatorRangeTransitionMinimumQuality
                    : ActionabilityThresholdPolicy.IndicatorMinimumQuality;

            int maximumConflict =
                string.Equals(
                    normalizedRegime,
                    "RANGE",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    normalizedRegime,
                    "TRANSITION",
                    StringComparison.OrdinalIgnoreCase)
                    ? ActionabilityThresholdPolicy.IndicatorRangeTransitionMaximumConflict
                    : ActionabilityThresholdPolicy.IndicatorMaximumConflict;

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
