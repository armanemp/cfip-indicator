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
    }
}
