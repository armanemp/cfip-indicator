using System;

namespace CFIP.cBot.Execution
{
    public static class ManagedExecutionLabelRule
    {
        public static bool Matches(
            string label,
            string baseLabel)
        {
            if (string.IsNullOrWhiteSpace(label) ||
                string.IsNullOrWhiteSpace(baseLabel))
                return false;

            return string.Equals(
                       label,
                       baseLabel,
                       StringComparison.Ordinal) ||
                   label.StartsWith(
                       baseLabel + "|",
                       StringComparison.Ordinal) ||
                   label.StartsWith(
                       baseLabel + "-",
                       StringComparison.Ordinal);
        }
    }
}
