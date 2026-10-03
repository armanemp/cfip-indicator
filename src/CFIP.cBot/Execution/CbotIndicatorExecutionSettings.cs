using System;
using System.Globalization;
using cAlgo.API;

namespace CFIP.cBot.Execution
{
    internal sealed class CbotIndicatorExecutionSettings
    {
        public bool EnableAutoTrading { get; }
        public bool EnableAutomaticOrders { get; }
        public bool UseMarketHoursGuard { get; }
        public int SessionStartUtc { get; }
        public int SessionEndUtc { get; }
        public bool UseSpreadFilter { get; }
        public double MaximumSpreadToStopRiskRatio { get; }
        public bool EnableDailyLossLimit { get; }
        public double MaximumDailyLossPercent { get; }
        public int MaximumOpenPositions { get; }
        public bool OneOrderPerSignal { get; }
        public bool EnableLiveExitManagement { get; }
        public bool EnablePartialTakeProfit { get; }
        public bool AutoBrokerProtection { get; }
        public bool AutoProtectBrokerPositions { get; }
        public bool SyncBrokerTakeProfit { get; }
        public bool ManagedActionsOnly { get; }
        public int BrokerModifyCooldownMs { get; }

        private CbotIndicatorExecutionSettings(
            bool enableAutoTrading,
            bool enableAutomaticOrders,
            bool useMarketHoursGuard,
            int sessionStartUtc,
            int sessionEndUtc,
            bool useSpreadFilter,
            double maximumSpreadToStopRiskRatio,
            bool enableDailyLossLimit,
            double maximumDailyLossPercent,
            int maximumOpenPositions,
            bool oneOrderPerSignal,
            bool enableLiveExitManagement,
            bool enablePartialTakeProfit,
            bool autoBrokerProtection,
            bool autoProtectBrokerPositions,
            bool syncBrokerTakeProfit,
            bool managedActionsOnly,
            int brokerModifyCooldownMs)
        {
            EnableAutoTrading = enableAutoTrading;
            EnableAutomaticOrders = enableAutomaticOrders;
            UseMarketHoursGuard = useMarketHoursGuard;
            SessionStartUtc = ClampHour(sessionStartUtc);
            SessionEndUtc = ClampHour(sessionEndUtc);
            UseSpreadFilter = useSpreadFilter;
            MaximumSpreadToStopRiskRatio = maximumSpreadToStopRiskRatio;
            EnableDailyLossLimit = enableDailyLossLimit;
            MaximumDailyLossPercent = maximumDailyLossPercent;
            MaximumOpenPositions = maximumOpenPositions;
            OneOrderPerSignal = oneOrderPerSignal;
            EnableLiveExitManagement = enableLiveExitManagement;
            EnablePartialTakeProfit = enablePartialTakeProfit;
            AutoBrokerProtection = autoBrokerProtection;
            AutoProtectBrokerPositions = autoProtectBrokerPositions;
            SyncBrokerTakeProfit = syncBrokerTakeProfit;
            ManagedActionsOnly = managedActionsOnly;
            BrokerModifyCooldownMs =
                Math.Max(
                    100,
                    Math.Min(
                        5000,
                        brokerModifyCooldownMs));
        }

        public static bool TryRead(
            ChartIndicator indicator,
            out CbotIndicatorExecutionSettings settings,
            out string reason)
        {
            settings = null;
            reason = "OK";

            if (indicator == null)
            {
                reason = "INDICATOR SETTINGS INSTANCE UNAVAILABLE";
                return false;
            }

            if (!TryGetBool(
                    indicator,
                    "EnableAutoTrading",
                    out bool enableAutoTrading) ||
                !TryGetBool(
                    indicator,
                    "EnableAutomaticOrders",
                    out bool enableAutomaticOrders) ||
                !TryGetBool(
                    indicator,
                    "UseMarketHoursGuard",
                    out bool useMarketHoursGuard) ||
                !TryGetInt(
                    indicator,
                    "SessionStartUtc",
                    out int sessionStartUtc) ||
                !TryGetInt(
                    indicator,
                    "SessionEndUtc",
                    out int sessionEndUtc) ||
                !TryGetBool(
                    indicator,
                    "UseSpreadFilter",
                    out bool useSpreadFilter) ||
                !TryGetDouble(
                    indicator,
                    "MaximumSpreadToStopRiskRatio",
                    out double maximumSpreadToStopRiskRatio) ||
                !TryGetBool(
                    indicator,
                    "EnableDailyLossLimit",
                    out bool enableDailyLossLimit) ||
                !TryGetDouble(
                    indicator,
                    "MaximumDailyLossPercent",
                    out double maximumDailyLossPercent) ||
                !TryGetInt(
                    indicator,
                    "MaximumOpenPositions",
                    out int maximumOpenPositions) ||
                !TryGetBool(
                    indicator,
                    "OneOrderPerSignal",
                    out bool oneOrderPerSignal) ||
                !TryGetBool(
                    indicator,
                    "EnableLiveExitManagement",
                    out bool enableLiveExitManagement) ||
                !TryGetBool(
                    indicator,
                    "EnablePartialTakeProfit",
                    out bool enablePartialTakeProfit) ||
                !TryGetBool(
                    indicator,
                    "AutoBrokerProtection",
                    out bool autoBrokerProtection) ||
                !TryGetBool(
                    indicator,
                    "AutoProtectBrokerPositions",
                    out bool autoProtectBrokerPositions) ||
                !TryGetBool(
                    indicator,
                    "SyncBrokerTakeProfit",
                    out bool syncBrokerTakeProfit) ||
                !TryGetBool(
                    indicator,
                    "ManagedActionsOnly",
                    out bool managedActionsOnly) ||
                !TryGetInt(
                    indicator,
                    "BrokerModifyCooldownMs",
                    out int brokerModifyCooldownMs))
            {
                reason = "INDICATOR EXECUTION SETTINGS INCOMPLETE";
                return false;
            }
            if (maximumOpenPositions != 1)
            {
                reason = "UNSUPPORTED NON-SINGLE-PLAN CAPACITY";
                return false;
            }

            if (!FinitePositive(maximumSpreadToStopRiskRatio) ||
                maximumDailyLossPercent < 0 ||
                double.IsNaN(maximumDailyLossPercent) ||
                double.IsInfinity(maximumDailyLossPercent))
            {
                reason = "INDICATOR EXECUTION SETTINGS INVALID";
                return false;
            }

            settings =
                new CbotIndicatorExecutionSettings(
                    enableAutoTrading,
                    enableAutomaticOrders,
                    useMarketHoursGuard,
                    sessionStartUtc,
                    sessionEndUtc,
                    useSpreadFilter,
                    maximumSpreadToStopRiskRatio,
                    enableDailyLossLimit,
                    maximumDailyLossPercent,
                    maximumOpenPositions,
                    oneOrderPerSignal,
                    enableLiveExitManagement,
                    enablePartialTakeProfit,
                    autoBrokerProtection,
                    autoProtectBrokerPositions,
                    syncBrokerTakeProfit,
                    managedActionsOnly,
                    brokerModifyCooldownMs);

            return true;
        }

        private static bool TryGet(
            ChartIndicator indicator,
            string name,
            out object value)
        {
            value = null;

            try
            {
                foreach (AlgoInstanceParameter parameter in indicator.Parameters)
                {
                    if (parameter == null ||
                        !string.Equals(
                            parameter.Name,
                            name,
                            StringComparison.Ordinal))
                        continue;

                    value = parameter.Value;
                    return value != null;
                }
            }
            catch
            {
                value = null;
            }

            return false;
        }

        private static bool TryGetBool(
            ChartIndicator indicator,
            string name,
            out bool value)
        {
            value = false;

            if (!TryGet(indicator, name, out object raw))
                return false;

            try
            {
                if (raw is bool boolean)
                {
                    value = boolean;
                    return true;
                }

                return bool.TryParse(
                    Convert.ToString(
                        raw,
                        CultureInfo.InvariantCulture),
                    out value);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetInt(
            ChartIndicator indicator,
            string name,
            out int value)
        {
            value = 0;

            if (!TryGet(indicator, name, out object raw))
                return false;

            try
            {
                value =
                    Convert.ToInt32(
                        raw,
                        CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return int.TryParse(
                    Convert.ToString(
                        raw,
                        CultureInfo.InvariantCulture),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value);
            }
        }

        private static bool TryGetDouble(
            ChartIndicator indicator,
            string name,
            out double value)
        {
            value = 0;

            if (!TryGet(indicator, name, out object raw))
                return false;

            try
            {
                value =
                    Convert.ToDouble(
                        raw,
                        CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return double.TryParse(
                    Convert.ToString(
                        raw,
                        CultureInfo.InvariantCulture),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out value);
            }
        }

        private static bool FinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }

        private static int ClampHour(int hour)
        {
            return Math.Max(0, Math.Min(23, hour));
        }
    }
}
