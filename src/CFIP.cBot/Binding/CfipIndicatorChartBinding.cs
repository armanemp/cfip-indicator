using System;
using System.Globalization;
using cAlgo.API;

namespace CFIP.cBot.Binding
{
    internal static class CfipIndicatorChartBinding
    {
        public const string DisplayName = "CFIP Smart Indicator";

        public static bool TryFind(
            Robot robot,
            out ChartIndicator indicator,
            out string fingerprint,
            out string managedExecutionLabel,
            out string reason)
        {
            indicator = null;
            fingerprint = "";
            managedExecutionLabel = "CFIP-SMART";
            reason = "OK";

            if (robot == null)
            {
                reason = "CBOT HOST UNAVAILABLE";
                return false;
            }

            ChartIndicator match = null;
            int matchCount = 0;

            foreach (ChartIndicator candidate in robot.ChartIndicators.Custom)
            {
                if (candidate == null)
                    continue;

                if (!string.Equals(
                        candidate.Name,
                        DisplayName,
                        StringComparison.Ordinal))
                    continue;

                match = candidate;
                matchCount++;
            }

            if (matchCount == 0)
            {
                reason =
                    "CFIP SMART INDICATOR NOT ATTACHED TO THIS CHART";
                return false;
            }

            if (matchCount > 1)
            {
                reason =
                    "MULTIPLE CFIP SMART INDICATOR INSTANCES";
                return false;
            }

            if (match.Parameters == null)
            {
                reason =
                    "CFIP SMART INDICATOR PARAMETERS UNAVAILABLE";
                return false;
            }

            ulong hash =
                14695981039346656037UL;

            foreach (AlgoInstanceParameter parameter in match.Parameters)
            {
                if (parameter == null)
                {
                    reason =
                        "CFIP SMART INDICATOR PARAMETER UNAVAILABLE";
                    return false;
                }

                hash = HashText(
                    hash,
                    parameter.Name ?? "");
                hash = HashText(
                    hash,
                    FormatValue(parameter.Value));
            }

            foreach (AlgoInstanceParameter parameter in match.Parameters)
            {
                if (parameter != null &&
                    string.Equals(
                        parameter.Name,
                        "AutoTradeLabel",
                        StringComparison.Ordinal) &&
                    parameter.Value is string label &&
                    !string.IsNullOrWhiteSpace(label))
                {
                    managedExecutionLabel =
                        label.Trim();
                    break;
                }
            }

            indicator = match;
            fingerprint =
                hash.ToString(
                    "X16",
                    CultureInfo.InvariantCulture);
            return true;
        }

        private static ulong HashText(
            ulong hash,
            string value)
        {
            if (value == null)
                value = "";

            unchecked
            {
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 1099511628211UL;
                }
            }

            return hash;
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return "<null>";

            IFormattable formattable =
                value as IFormattable;

            return formattable != null
                ? formattable.ToString(
                    null,
                    CultureInfo.InvariantCulture)
                : value.ToString();
        }
    }
}
