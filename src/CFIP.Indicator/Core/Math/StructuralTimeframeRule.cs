using System;

namespace cAlgo
{
    /// <summary>
    /// Canonical supported higher-timeframe names for structural planning.
    /// Unknown values are invalid; callers must fail closed rather than
    /// silently substituting another timeframe.
    /// </summary>
    internal static class StructuralTimeframeRule
    {
        public static bool IsSupported(string timeframe)
        {
            if (string.IsNullOrWhiteSpace(timeframe))
                return false;

            string value = timeframe.Trim().ToUpperInvariant();

            return
                value == "M1" ||
                value == "M15" ||
                value == "M30" ||
                value == "H1" ||
                value == "H4" ||
                value == "D1" ||
                value == "W1";
        }

        public static bool IsHigherThanM5(string timeframe)
        {
            if (string.IsNullOrWhiteSpace(timeframe))
                return false;

            string value = timeframe.Trim().ToUpperInvariant();

            return
                value == "M15" ||
                value == "M30" ||
                value == "H1" ||
                value == "H4" ||
                value == "D1" ||
                value == "W1";
        }

        public static bool ContainsHigherTimeframeMarker(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;

            foreach (string timeframe in new[]
            {
                "M15", "M30", "H1", "H4", "D1", "W1"
            })
            {
                if (source.IndexOf(
                        "@" + timeframe,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
