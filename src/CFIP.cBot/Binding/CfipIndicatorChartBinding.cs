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

            foreach (ChartIndicator candidate in robot.ChartIndicators.Custom)
            {
                if (candidate == null)
                    continue;

                bool instanceNameMatches =
                    string.Equals(
                        candidate.Name,
                        DisplayName,
                        StringComparison.Ordinal);

                bool typeNameMatches =
                    candidate.Type != null &&
                    string.Equals(
                        candidate.Type.Name,
                        TypeName,
                        StringComparison.Ordinal);

                if (!instanceNameMatches &&
                    !typeNameMatches)
                    continue;

                match = candidate;
                count++;
            }

            if (count == 0)
            {
                reason = "CFIP SMART INDICATOR NOT ATTACHED TO THIS CHART";
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