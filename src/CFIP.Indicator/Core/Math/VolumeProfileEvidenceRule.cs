using System;

namespace cAlgo
{
    internal readonly struct VolumeProfileEvidenceResult
    {
        public int Quality { get; }
        public bool Confluence { get; }
        public string Location { get; }
        public string Reason { get; }

        public VolumeProfileEvidenceResult(
            int quality,
            bool confluence,
            string location,
            string reason)
        {
            Quality = Math.Max(0, Math.Min(100, quality));
            Confluence = confluence;
            Location = location ?? "UNKNOWN";
            Reason = reason ?? string.Empty;
        }
    }

    internal static class VolumeProfileEvidenceRule
    {
        public static VolumeProfileEvidenceResult Evaluate(
            VolumeProfileSnapshot profile,
            int direction,
            double price,
            double atr)
        {
            if (!profile.IsValid ||
                (direction != 1 && direction != -1) ||
                !IsVolumeProfileEvidenceFinitePositive(price) ||
                !IsVolumeProfileEvidenceFinitePositive(atr) ||
                !IsVolumeProfileEvidenceFinitePositive(profile.BinSize) ||
                !IsVolumeProfileEvidenceFinitePositive(profile.VAH - profile.VAL))
            {
                return new VolumeProfileEvidenceResult(
                    0,
                    false,
                    "UNAVAILABLE",
                    "VOLUME PROFILE UNAVAILABLE");
            }

            double valueWidth =
                profile.VAH - profile.VAL;

            double valueMargin =
                Math.Max(
                    profile.BinSize * 1.5,
                    valueWidth * 0.18);

            double pocMargin =
                Math.Max(
                    profile.BinSize * 1.5,
                    atr * 0.25);

            bool nearPoc =
                Math.Abs(price - profile.POC) <=
                pocMargin;

            bool nearBullValueEdge =
                direction == 1 &&
                price <=
                    profile.VAL + valueMargin;

            bool nearBearValueEdge =
                direction == -1 &&
                price >=
                    profile.VAH - valueMargin;

            bool acceptedAboveValue =
                direction == 1 &&
                price > profile.VAH &&
                price - profile.VAH <= atr * 0.35;

            bool acceptedBelowValue =
                direction == -1 &&
                profile.VAL > price &&
                profile.VAL - price <= atr * 0.35;

            double normalized =
                (price - profile.VAL) /
                valueWidth;

            int quality = 42;
            string location = "VALUE-MID";
            string reason = "INSIDE VALUE AREA";

            if (nearBullValueEdge)
            {
                quality += 28;
                location = "NEAR-VAL";
                reason = "BUY NEAR VALUE-AREA LOW";
            }
            else if (nearBearValueEdge)
            {
                quality += 28;
                location = "NEAR-VAH";
                reason = "SELL NEAR VALUE-AREA HIGH";
            }
            else if (acceptedAboveValue)
            {
                quality += 22;
                location = "ABOVE-VA";
                reason = "BUY ACCEPTING ABOVE VALUE AREA";
            }
            else if (acceptedBelowValue)
            {
                quality += 22;
                location = "BELOW-VA";
                reason = "SELL ACCEPTING BELOW VALUE AREA";
            }
            else if (normalized > 0.30 &&
                     normalized < 0.70)
            {
                quality -= 8;
                location = "VALUE-MID";
                reason = "PRICE IN VALUE-AREA MIDDLE";
            }

            if (nearPoc)
            {
                quality += 10;
                if (location == "VALUE-MID")
                    reason = "PRICE NEAR HIGH-VOLUME POC";
            }

            quality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        quality));

            bool confluence =
                quality >= 68 &&
                (nearBullValueEdge ||
                 nearBearValueEdge ||
                 acceptedAboveValue ||
                 acceptedBelowValue ||
                 nearPoc);

            return new VolumeProfileEvidenceResult(
                quality,
                confluence,
                location,
                reason);
        }

        private static bool IsVolumeProfileEvidenceFinitePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}
