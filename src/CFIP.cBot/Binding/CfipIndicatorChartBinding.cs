using System;
using System.Collections.Generic;
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

            List<ChartIndicator> candidates =
                new List<ChartIndicator>();

            try
            {
                foreach (ChartIndicator candidate in robot.ChartIndicators.Custom)
                {
                    if (candidate != null &&
                        !candidates.Contains(candidate))
                        candidates.Add(candidate);
                }
            }
            catch
            {
            }

            try
            {
                foreach (ChartIndicator candidate in robot.ChartIndicators)
                {
                    if (candidate != null &&
                        !candidates.Contains(candidate))
                        candidates.Add(candidate);
                }
            }
            catch
            {
            }

            foreach (ChartIndicator candidate in candidates)
            {
                if (candidate == null)
                    continue;

                string candidateName =
                    candidate.Name ?? string.Empty;

                bool instanceNameMatches =
                    string.Equals(
                        candidateName,
                        DisplayName,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        candidateName,
                        TypeName,
                        StringComparison.OrdinalIgnoreCase) ||
                    candidateName.StartsWith(
                        DisplayName + " ",
                        StringComparison.OrdinalIgnoreCase) ||
                    candidateName.StartsWith(
                        TypeName + " ",
                        StringComparison.OrdinalIgnoreCase);

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
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        candidateTypeText,
                        TypeName,
                        StringComparison.OrdinalIgnoreCase) ||
                    candidateTypeText.EndsWith(
                        "." + TypeName,
                        StringComparison.OrdinalIgnoreCase);

                if (!instanceNameMatches &&
                    !typeNameMatches)
                    continue;

                match = candidate;
                count++;
            }

            if (count == 0)
            {
                List<string> diagnostics =
                    new List<string>();

                for (int i = 0;
                     i < candidates.Count && i < 8;
                     i++)
                {
                    ChartIndicator candidate = candidates[i];
                    diagnostics.Add(
                        (candidate.Name ?? "NAME?") +
                        "/" +
                        (candidate.Type == null
                            ? "TYPE?"
                            : candidate.Type.ToString()));
                }

                int customCount = 0;
                try
                {
                    customCount =
                        robot.ChartIndicators.Custom == null
                            ? 0
                            : robot.ChartIndicators.Custom.Count;
                }
                catch
                {
                }

                reason =
                    "CFIP SMART INDICATOR NOT ATTACHED TO THIS CHART" +
                    " • custom=" +
                    customCount +
                    " • total=" +
                    candidates.Count +
                    (diagnostics.Count == 0
                        ? ""
                        : " • seen=" +
                          string.Join(" | ", diagnostics));
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