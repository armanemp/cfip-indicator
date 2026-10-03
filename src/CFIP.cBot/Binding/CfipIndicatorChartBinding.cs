using System;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Binding
{
    internal static class CfipIndicatorChartBinding
    {
        public const string DisplayName = "CFIP Smart Indicator";
        public const string TypeName = IndicatorIdentity.TypeName;

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
            int count = 0;

            foreach (ChartIndicator candidate in robot.ChartIndicators)
            {
                if (candidate == null)
                    continue;

                bool instanceNameMatches =
                    string.Equals(
                        candidate.Name,
                        DisplayName,
                        StringComparison.Ordinal);

                string candidateTypeName =
                    candidate.Type == null
                        ? string.Empty
                        : candidate.Type.Name ?? string.Empty;

                string candidateTypeText =
                    candidate.Type == null
                        ? string.Empty
                        : candidate.Type.ToString() ?? string.Empty;

                bool typeNameMatches =
                    string.Equals(
                        candidateTypeName,
                        TypeName,
                        StringComparison.Ordinal) ||
                    string.Equals(
                        candidateTypeText,
                        TypeName,
                        StringComparison.Ordinal) ||
                    candidateTypeText.EndsWith(
                        "." + TypeName,
                        StringComparison.Ordinal);

                if (!instanceNameMatches &&
                    !typeNameMatches)
                    continue;

                match = candidate;
                count++;
            }

            if (count == 0)
            {
                reason = "CFIP SMART INDICATOR NOT ATTACHED TO THIS CHART • chartIndicators=" +
                         robot.ChartIndicators.Count;
                return false;
            }

            if (count > 1)
            {
                reason = "MULTIPLE CFIP SMART INDICATOR INSTANCES";
                return false;
            }

            if (string.IsNullOrWhiteSpace(match.InstanceId))
            {
                reason = "CFIP SMART INDICATOR INSTANCE ID UNAVAILABLE";
                return false;
            }

            indicator = match;
            return true;
        }
    }
}