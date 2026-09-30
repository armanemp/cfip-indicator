using System;

namespace cAlgo
{
    internal static class TargetAgeSemanticsRule
    {
        public static bool IsAllowed(
            string timeframe,
            int sourceAgeBars,
            double sourceAgeMinutes,
            int maximumSetupAgeBars,
            int maximumZoneAgeBars)
        {
            if (string.Equals(
                    timeframe,
                    "M5",
                    StringComparison.OrdinalIgnoreCase))
            {
                return sourceAgeBars <=
                    Math.Max(
                        0,
                        maximumSetupAgeBars);
            }

            if (!StructuralTimeframeRule.IsSupported(timeframe) ||
                double.IsNaN(sourceAgeMinutes) ||
                double.IsInfinity(sourceAgeMinutes) ||
                sourceAgeMinutes < 0)
                return false;

            double maximumMinutes =
                GetMaximumHtfAgeMinutes(
                    timeframe,
                    maximumSetupAgeBars,
                    maximumZoneAgeBars);

            return sourceAgeMinutes <=
                maximumMinutes + 1e-9;
        }

        public static double GetMaximumHtfAgeMinutes(
            string timeframe,
            int maximumSetupAgeBars,
            int maximumZoneAgeBars)
        {
            int equivalentBars =
                Math.Min(
                    Math.Max(0, maximumSetupAgeBars),
                    Math.Max(0, maximumZoneAgeBars));

            return
                equivalentBars *
                TimeframeMinutes(timeframe);
        }

        public static double ElapsedMinutes(
            DateTime sourceOpenTimeUtc,
            DateTime referenceOpenTimeUtc)
        {
            double minutes =
                (referenceOpenTimeUtc -
                 sourceOpenTimeUtc).TotalMinutes;

            return double.IsNaN(minutes) ||
                   double.IsInfinity(minutes)
                    ? 0
                    : Math.Max(0, minutes);
        }

        private static double TimeframeMinutes(
            string timeframe)
        {
            switch ((timeframe ?? string.Empty)
                .Trim()
                .ToUpperInvariant())
            {
                case "M15":
                    return 15;
                case "M30":
                    return 30;
                case "H1":
                    return 60;
                case "H4":
                    return 240;
                case "D1":
                    return 1440;
                case "W1":
                    return 10080;
                default:
                    return 0;
            }
        }
    }
}
