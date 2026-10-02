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
            out string reason)
        {
            indicator = null;
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

            if (string.IsNullOrWhiteSpace(match.InstanceId))
            {
                reason =
                    "CFIP SMART INDICATOR INSTANCE ID UNAVAILABLE";
                return false;
            }

            indicator = match;
            return true;
        }

        public static bool TryGetManagedExecutionLabel(
            ChartIndicator indicator,
            out string managedExecutionLabel,
            out string reason)
        {
            managedExecutionLabel = "CFIP-SMART";
            reason = "OK";

            if (indicator == null ||
                indicator.Parameters == null)
            {
                reason =
                    "CFIP SMART INDICATOR PARAMETERS UNAVAILABLE";
                return false;
            }

            foreach (AlgoInstanceParameter parameter in indicator.Parameters)
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
                    return true;
                }
            }

            return true;
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
