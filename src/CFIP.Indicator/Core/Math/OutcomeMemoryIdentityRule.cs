using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace cAlgo
{
    internal sealed class OutcomeMemoryParameter
    {
        public OutcomeMemoryParameter(
            string name,
            string group,
            string value)
        {
            Name = name ?? "";
            Group = group ?? "";
            Value = value ?? "";
        }

        public string Name { get; private set; }
        public string Group { get; private set; }
        public string Value { get; private set; }
    }

    internal static class OutcomeMemoryIdentityRule
    {
        internal const string CurrentMemorySchema = "2";
        internal const string CurrentKeyPrefix = "CFIPM2";

        private const string AlertsCoreGroupPrefix = "12 · ALERTS";
        private const string DisplayGroupPrefix = "14 · DISPLAY";

        private static readonly HashSet<string> NonDecisionParameterNames =
            new HashSet<string>(
                StringComparer.Ordinal)
            {
                "EnableEndOfDayAlert",
                "EndOfDayAlertMinutesBefore",

                "EnableAutoTrading",
                "EnableAutomaticOrders",
                "AutoTradingReminder",
                "ShowTradeActionButtons",
                "AlwaysShowSafetyButtons",
                "ActionButtonMargin",
                "ActionButtonWidth",
                "ActionButtonHeight",

                "ShowReactionArrow",
                "ShowHistoricalArrows",

                "UseSemanticAlertSounds",
                "ShowSpreadDiagnostics",

                "AlertOnNewsEvent",
                "AlertOnSessionBlock",
                "AlertOnSpreadBlock",
                "AlertOnFridayBlock",
                "AlertOnRegimeNoTrade",
                "AlertOnCooldownBlock",
                "AlertOnExitPlanUpdate",
                "ShowEngineStatus",
                "PanelStateHoldSeconds",
                "ShowLevelPricesInUnifiedPanel",
                "ShowTradePlanPanel",
                "PredictionColor",
                "StrongBuyArrowColor",
                "StrongSellArrowColor",
                "ConfirmedBuyArrowColor",
                "ConfirmedSellArrowColor",
                "CautionBuyArrowColor",
                "CautionSellArrowColor",
                "BlockedReactionArrowColor",
                "SmartAlertCooldownSeconds",

                "ShowNewsRiskStatus"
            };

        internal static bool IsDecisionOrResultAffecting(
            OutcomeMemoryParameter parameter)
        {
            if (parameter == null)
                return false;

            if (parameter.Group.StartsWith(
                    AlertsCoreGroupPrefix,
                    StringComparison.Ordinal) ||
                parameter.Group.StartsWith(
                    DisplayGroupPrefix,
                    StringComparison.Ordinal))
                return false;

            return !NonDecisionParameterNames.Contains(
                parameter.Name);
        }

        internal static string BuildFingerprint(
            IEnumerable<OutcomeMemoryParameter> parameters,
            bool decisionOrResultOnly)
        {
            List<OutcomeMemoryParameter> selected =
                new List<OutcomeMemoryParameter>();

            if (parameters != null)
            {
                foreach (OutcomeMemoryParameter parameter in parameters)
                {
                    if (parameter == null)
                        continue;

                    if (decisionOrResultOnly &&
                        !IsDecisionOrResultAffecting(parameter))
                        continue;

                    selected.Add(parameter);
                }
            }

            selected.Sort(
                delegate(
                    OutcomeMemoryParameter left,
                    OutcomeMemoryParameter right)
                {
                    int byName =
                        string.CompareOrdinal(
                            left.Name,
                            right.Name);

                    if (byName != 0)
                        return byName;

                    int byGroup =
                        string.CompareOrdinal(
                            left.Group,
                            right.Group);

                    if (byGroup != 0)
                        return byGroup;

                    return string.CompareOrdinal(
                        left.Value,
                        right.Value);
                });

            StringBuilder raw =
                new StringBuilder();

            int parameterCount = 0;

            foreach (OutcomeMemoryParameter parameter in selected)
            {
                raw.Append(parameter.Name);
                raw.Append('=');
                raw.Append(parameter.Value);
                raw.Append(';');
                parameterCount++;
            }

            unchecked
            {
                uint hash = 2166136261;

                hash ^= (uint)parameterCount;
                hash *= 16777619;

                for (int i = 0; i < raw.Length; i++)
                {
                    hash ^= raw[i];
                    hash *= 16777619;
                }

                return hash.ToString(
                    "X8",
                    CultureInfo.InvariantCulture);
            }
        }

        internal static string BuildAccountScopeToken(
            string brokerName,
            int accountNumber,
            string accountType,
            bool isLive)
        {
            string raw =
                (brokerName ?? "UNKNOWN") +
                "|" +
                accountNumber.ToString(
                    CultureInfo.InvariantCulture) +
                "|" +
                (accountType ?? "UNKNOWN") +
                "|" +
                (isLive ? "LIVE" : "DEMO");

            return HashToken(raw, 10);
        }

        internal static string BuildMemoryKey(
            string symbol,
            string timeframe,
            string accountScopeToken,
            string decisionFingerprint)
        {
            return CurrentKeyPrefix +
                HashToken(symbol ?? "UNKNOWN", 8) +
                HashToken(timeframe ?? "UNKNOWN", 6) +
                (accountScopeToken ?? "UNKNOWN") +
                (decisionFingerprint ?? "UNKNOWN");
        }

        internal static string BuildLegacyMemoryKey(
            string symbol,
            string timeframe,
            string migrationFingerprint)
        {
            return "CFIP.OUTCOME." +
                SanitizeArchivePart(symbol) +
                "." +
                SanitizeArchivePart(timeframe) +
                "." +
                (migrationFingerprint ?? "") +
                ".MEM";
        }

        private static string HashToken(
            string value,
            int hexCharacters)
        {
            unchecked
            {
                ulong hash = 14695981039346656037UL;
                string input = value ?? "";

                for (int i = 0; i < input.Length; i++)
                {
                    hash ^= input[i];
                    hash *= 1099511628211UL;
                }

                string hex =
                    hash.ToString(
                        "X16",
                        CultureInfo.InvariantCulture);

                if (hexCharacters <= 0)
                    return "";

                if (hexCharacters >= hex.Length)
                    return hex;

                return hex.Substring(
                    0,
                    hexCharacters);
            }
        }

        private static string SanitizeArchivePart(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "UNKNOWN";

            StringBuilder result =
                new StringBuilder();

            foreach (char ch in value)
            {
                result.Append(
                    char.IsLetterOrDigit(ch)
                        ? ch
                        : '_');
            }

            return result.ToString();
        }
    }
}
