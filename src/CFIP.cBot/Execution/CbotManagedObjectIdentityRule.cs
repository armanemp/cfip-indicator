using System;

namespace CFIP.cBot.Execution
{
    internal static class CbotManagedObjectIdentityRule
    {
        public const string InstanceMarker = "|CFIP-I:";

        public static bool MatchesManagedLabel(
            string actual,
            string expected)
        {
            if (string.IsNullOrWhiteSpace(actual) ||
                string.IsNullOrWhiteSpace(expected))
                return false;

            if (string.Equals(
                    actual,
                    expected,
                    StringComparison.Ordinal))
                return true;

            int markerIndex =
                expected.IndexOf(
                    InstanceMarker,
                    StringComparison.Ordinal);

            if (markerIndex < 0)
                return false;

            string scopedSuffix =
                expected.Substring(markerIndex);

            return
                !string.IsNullOrWhiteSpace(scopedSuffix) &&
                actual.EndsWith(
                    scopedSuffix,
                    StringComparison.Ordinal);
        }

        public static bool MatchesManagedPendingLabel(
            string actual,
            string expected)
        {
            return MatchesManagedLabel(
                actual,
                string.IsNullOrWhiteSpace(expected)
                    ? string.Empty
                    : expected + "-PENDING");
        }

        public static bool MatchesInstanceScope(
            string actual,
            string scopedSuffix)
        {
            return
                !string.IsNullOrWhiteSpace(actual) &&
                !string.IsNullOrWhiteSpace(scopedSuffix) &&
                actual.EndsWith(
                    scopedSuffix,
                    StringComparison.Ordinal);
        }
    }
}