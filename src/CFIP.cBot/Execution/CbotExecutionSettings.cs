using System;

namespace CFIP.cBot.Execution
{
    /// <summary>
    /// Canonical cBot-owned execution policy.
    /// It is constructed only from cBot parameters; the Indicator is never
    /// inspected for execution configuration.
    /// </summary>
    internal sealed class CbotExecutionSettings
    {
        public bool EnableAutoTrading { get; }
        public bool EnableAutomaticOrders { get; }
        public bool UseMarketHoursGuard { get; }
        public bool UseSpreadFilter { get; }
        public double MaximumSpreadToStopRiskRatio { get; }
        public bool EnableDailyLossLimit { get; }
        public double MaximumDailyLossPercent { get; }
        public bool EnableLiveExitManagement { get; }
        public bool EnablePartialTakeProfit { get; }
        public bool AutoBrokerProtection { get; }
        public bool AutoProtectBrokerPositions { get; }
        public bool SyncBrokerTakeProfit { get; }
        public bool ManagedActionsOnly { get; }
        public int BrokerModifyCooldownMs { get; }
        public int PendingOrderExpiryMinutes { get; }
        public string AutoTradeLabel { get; }
        public int MaxConcurrentScenarios { get; }
        public double MaxExecutionMarginUsagePercent { get; }
        public double ExecutionMarginBufferPercent { get; }

        private CbotExecutionSettings(
            bool enableAutoTrading, bool enableAutomaticOrders,
            bool useMarketHoursGuard,
            bool useSpreadFilter, double maximumSpreadToStopRiskRatio,
            bool enableDailyLossLimit, double maximumDailyLossPercent,
            bool enableLiveExitManagement, bool enablePartialTakeProfit,
            bool autoBrokerProtection, bool autoProtectBrokerPositions,
            bool syncBrokerTakeProfit, bool managedActionsOnly,
            int brokerModifyCooldownMs, int pendingOrderExpiryMinutes,
            string autoTradeLabel,
            int maxConcurrentScenarios,
            double maxExecutionMarginUsagePercent,
            double executionMarginBufferPercent)
        {
            EnableAutoTrading = enableAutoTrading;
            EnableAutomaticOrders = enableAutomaticOrders;
            UseMarketHoursGuard = useMarketHoursGuard;
            UseSpreadFilter = useSpreadFilter;
            MaximumSpreadToStopRiskRatio = NormalizePositive(maximumSpreadToStopRiskRatio, 0.18);
            EnableDailyLossLimit = enableDailyLossLimit;
            MaximumDailyLossPercent = Math.Max(0, double.IsNaN(maximumDailyLossPercent) || double.IsInfinity(maximumDailyLossPercent) ? 3.0 : maximumDailyLossPercent);
            EnableLiveExitManagement = enableLiveExitManagement;
            EnablePartialTakeProfit = enablePartialTakeProfit;
            AutoBrokerProtection = autoBrokerProtection;
            AutoProtectBrokerPositions = autoProtectBrokerPositions;
            SyncBrokerTakeProfit = syncBrokerTakeProfit;
            ManagedActionsOnly = managedActionsOnly;
            BrokerModifyCooldownMs = Math.Max(100, Math.Min(5000, brokerModifyCooldownMs));
            PendingOrderExpiryMinutes = Math.Max(15, Math.Min(1440, pendingOrderExpiryMinutes));
            AutoTradeLabel = string.IsNullOrWhiteSpace(autoTradeLabel) ? "CFIP-SMART" : autoTradeLabel.Trim();
            MaxConcurrentScenarios =
                Math.Max(
                    1,
                    Math.Min(
                        10,
                        maxConcurrentScenarios));
            MaxExecutionMarginUsagePercent =
                Math.Max(
                    10,
                    Math.Min(
                        100,
                        NormalizePositive(
                            maxExecutionMarginUsagePercent,
                            80)));
            ExecutionMarginBufferPercent =
                Math.Max(
                    0,
                    Math.Min(
                        40,
                        NormalizeNonNegative(
                            executionMarginBufferPercent,
                            10)));
        }

        public static CbotExecutionSettings Create(CFIP.cBot.CFIPExecutionBot robot)
        {
            if (robot == null)
                return null;

            return new CbotExecutionSettings(
                robot.EnableAutoTrading, robot.EnableAutomaticOrders,
                robot.UseMarketHoursGuard,
                robot.UseSpreadFilter, robot.MaximumSpreadToStopRiskRatio,
                robot.EnableDailyLossLimit, robot.MaximumDailyLossPercent,
                robot.EnableLiveExitManagement, robot.EnablePartialTakeProfit,
                robot.AutoBrokerProtection, robot.AutoProtectBrokerPositions,
                robot.SyncBrokerTakeProfit, robot.ManagedActionsOnly,
                robot.BrokerModifyCooldownMs, robot.PendingOrderExpiryMinutes,
                robot.AutoTradeLabel,
                robot.MaxConcurrentScenarios,
                robot.MaxExecutionMarginUsagePercent,
                robot.ExecutionMarginBufferPercent);
        }

        private static double NormalizePositive(double value, double fallback)
        {
            return double.IsNaN(value) || double.IsInfinity(value) || value <= 0
                ? fallback
                : value;
        }

        private static double NormalizeNonNegative(double value, double fallback)
        {
            return double.IsNaN(value) || double.IsInfinity(value) || value < 0
                ? fallback
                : value;
        }
    }
}