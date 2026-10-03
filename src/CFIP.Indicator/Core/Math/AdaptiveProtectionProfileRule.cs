using System;

namespace cAlgo
{
    internal readonly struct AdaptiveProtectionProfile
    {
        public double BreakEvenRewardAtr { get; }
        public double TrailStartRewardAtr { get; }
        public double TightenRewardAtr { get; }
        public double BreathingAtr { get; }

        public AdaptiveProtectionProfile(
            double breakEvenRewardAtr,
            double trailStartRewardAtr,
            double tightenRewardAtr,
            double breathingAtr)
        {
            BreakEvenRewardAtr = breakEvenRewardAtr;
            TrailStartRewardAtr = trailStartRewardAtr;
            TightenRewardAtr = tightenRewardAtr;
            BreathingAtr = breathingAtr;
        }
    }

    internal static class AdaptiveProtectionProfileRule
    {
        public static AdaptiveProtectionProfile Resolve(
            string regime,
            double riskAtr,
            int confidence,
            int smartQuality,
            int exitPressure,
            bool momentumAligned,
            double configuredBreathingAtr)
        {
            double risk =
                Clamp(riskAtr, 0.20, 3.50);

            double quality =
                Clamp(
                    confidence * 0.55 +
                    smartQuality * 0.45,
                    0,
                    100);

            double breakEven =
                Clamp(
                    0.45 +
                    risk * 0.12 +
                    Math.Max(0, 70 - quality) * 0.002,
                    0.35,
                    1.00);

            double trailStart =
                Clamp(
                    0.70 +
                    risk * 0.20 +
                    Math.Max(0, 72 - quality) * 0.003,
                    0.60,
                    1.50);

            double tighten =
                trailStart +
                0.30 +
                Math.Max(
                    0,
                    60 - Math.Max(0, exitPressure)) *
                0.002;

            string normalized =
                (regime ?? string.Empty).Trim().ToUpperInvariant();

            if (normalized == MarketRegimeIdentity.Expansion)
                tighten += 0.15;
            else if (normalized == MarketRegimeIdentity.Range ||
                     normalized == MarketRegimeIdentity.Compression)
                trailStart += 0.10;

            if (momentumAligned)
                tighten += 0.10;

            double breathing =
                Clamp(
                    Math.Max(0.20, configuredBreathingAtr) +
                    risk * 0.10 +
                    (quality >= 82 ? 0.08 : 0) -
                    (exitPressure >= 80 ? 0.18 : 0),
                    0.35,
                    1.50);

            return new AdaptiveProtectionProfile(
                breakEven,
                trailStart,
                tighten,
                breathing);
        }

        private static double Clamp(
            double value,
            double minimum,
            double maximum)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
                return minimum;

            return Math.Max(
                minimum,
                Math.Min(
                    maximum,
                    value));
        }
    }
}
