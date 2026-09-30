// CFIP Indicator — ManagedIdentityRule.cs
// Platform-neutral ownership rule for broker-managed instance identity.

using System;

namespace cAlgo
{
    internal static class ManagedIdentityRule
    {
        public const string InstanceMarker = "|CFIP-I:";

        public static bool TryBuildLabel(
            string baseLabel,
            string instanceId,
            out string label)
        {
            label = string.Empty;

            if (string.IsNullOrWhiteSpace(baseLabel) ||
                string.IsNullOrWhiteSpace(instanceId))
                return false;

            string normalizedBase = SanitizePart(baseLabel);
            string normalizedInstance = SanitizePart(instanceId);

            if (string.IsNullOrWhiteSpace(normalizedBase) ||
                string.IsNullOrWhiteSpace(normalizedInstance))
                return false;

            label =
                normalizedBase +
                InstanceMarker +
                normalizedInstance;

            return true;
        }

        private static string SanitizePart(string value)
        {
            return value
                .Trim()
                .Replace("|", "/")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }
    }
}
