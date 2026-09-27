// ============================================================================
// CFIP Indicator — TimeWindowParser.cs
// Platform-neutral time-window parsing.
// ============================================================================

using System;

namespace cAlgo
{
    internal static class TimeWindowParser
    {
        internal static bool TryParseMinutes(
            string value,
            out int minutes)
        {
            minutes = 0;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            string[] parts =
                value.Trim().Split(':');

            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0], out int hours) ||
                !int.TryParse(parts[1], out int mins))
                return false;

            if (hours < 0 ||
                hours > 23 ||
                mins < 0 ||
                mins > 59)
                return false;

            minutes =
                hours * 60 +
                mins;

            return true;
        }
    }
}
