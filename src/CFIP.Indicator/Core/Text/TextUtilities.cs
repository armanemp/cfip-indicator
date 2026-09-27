// ============================================================================
// CFIP Indicator — TextUtilities.cs
// Platform-neutral text helpers.
// ============================================================================

using System;

namespace cAlgo
{
    internal static class TextUtilities
    {
        internal static string CompactText(
            string value,
            int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            int limit =
                Math.Max(
                    1,
                    maxLength);

            if (value.Length <= limit)
                return value;

            if (limit <= 3)
                return value.Substring(0, limit);

            return
                value.Substring(0, limit - 3) +
                "...";
        }
    }
}
