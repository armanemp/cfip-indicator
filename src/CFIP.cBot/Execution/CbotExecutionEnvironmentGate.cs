using System;
using cAlgo.API;
using CFIP.Contracts;
using CFIP.cBot.Risk;

namespace CFIP.cBot.Execution
{
    internal sealed class CbotExecutionEnvironmentGate
    {
        public bool Evaluate(
            Robot robot,
            SignalEnvelope envelope,
            double maximumMarginUsagePercent,
            double marginBufferPercent,
            CbotExecutionSettings settings,
            CbotDailyLossGuard dailyLossGuard,
            bool pendingAction,
            int maximumConcurrentScenarios,
            DateTime nowUtc,
            bool allowLiveExecution,
            out string reason)
        {
            reason = "OK";

            if (robot == null ||
                envelope == null ||
                envelope.Identity == null ||
                envelope.Intent == null ||
                settings == null ||
                dailyLossGuard == null)
            {
                reason = "EXECUTION ENVIRONMENT INPUT INVALID";
                return false;
            }

            bool liveAccount =
                robot.Account != null &&
                robot.Account.IsLive;

            // The cBot owns the master execution switches for both demo and live
            // accounts. The Indicator never controls broker execution policy.
            if (!settings.EnableAutoTrading)
            {
                reason = "CBOT AUTO TRADING DISABLED";
                return false;
            }

            if (pendingAction &&
                !settings.EnableAutomaticOrders)
            {
                reason = "CBOT AUTOMATIC ORDERS DISABLED";
                return false;
            }

            if (robot.Symbol == null)
            {
                reason = "SYMBOL UNAVAILABLE";
                return false;
            }

            // Broker-level trading permission is distinct from the cBot's
            // automatic-execution switches. Surface it explicitly so a broker/
            // terminal disabled symbol cannot look like a signal-quality issue.
            if (!robot.Symbol.IsTradingEnabled)
            {
                reason = "SYMBOL TRADING DISABLED";
                return false;
            }

            if (settings.UseMarketHoursGuard)
            {
                if (robot.Symbol.MarketHours == null)
                {
                    reason = "SYMBOL MARKET HOURS UNAVAILABLE";
                    return false;
                }

                if (!robot.Symbol.MarketHours.IsOpened(nowUtc))
                {
                    reason = "SYMBOL MARKET CLOSED";
                    return false;
                }
            }

            if (dailyLossGuard.IsBlocked(
                    robot,
                    settings.EnableDailyLossLimit,
                    settings.MaximumDailyLossPercent,
                    nowUtc,
                    out double lossPercent,
                    out string dailyLossReason))
            {
                reason = dailyLossReason;
                return false;
            }

            if (robot.Account == null)
            {
                reason = "ACCOUNT UNAVAILABLE";
                return false;
            }

            if (robot.Account.IsLive &&
                !allowLiveExecution)
            {
                reason = "LIVE EXECUTION NOT ARMED";
                return false;
            }

            if (envelope.Intent.Action == ExecutionAction.Market &&
                !EffectiveExecutionEnabledForAction(
                    robot,
                    settings,
                    ExecutionAction.Market))
            {
                reason = "CBOT MARKET EXECUTION DISABLED";
                return false;
            }

            if ((envelope.Intent.Action == ExecutionAction.PendingStop ||
                 envelope.Intent.Action == ExecutionAction.PendingLimit) &&
                !settings.EnableAutomaticOrders)
            {
                reason = "CBOT AUTOMATIC ORDERS DISABLED";
                return false;
            }

            if (!string.Equals(
                    envelope.Identity.Symbol,
                    robot.SymbolName,
                    StringComparison.Ordinal))
            {
                reason = "EXECUTION SYMBOL MISMATCH";
                return false;
            }

            if (envelope.Identity.Direction != TradeDirection.Buy &&
                envelope.Identity.Direction != TradeDirection.Sell)
            {
                reason = "EXECUTION DIRECTION INVALID";
                return false;
            }

            if (!FinitePositive(envelope.Intent.RequestedEntry) ||
                !FinitePositive(envelope.Intent.Stop) ||
                !FinitePositive(envelope.Intent.InitialTarget) ||
                !envelope.Intent.RequestedVolume.HasValue ||
                !FinitePositive(envelope.Intent.RequestedVolume.Value))
            {
                reason = "EXECUTION GEOMETRY INVALID";
                return false;
            }

            if (envelope.Identity.Direction == TradeDirection.Buy &&
                (envelope.Intent.Stop >= envelope.Intent.RequestedEntry ||
                 envelope.Intent.InitialTarget <= envelope.Intent.RequestedEntry))
            {
                reason = "BUY EXECUTION GEOMETRY WRONG SIDE";
                return false;
            }

            if (envelope.Identity.Direction == TradeDirection.Sell &&
                (envelope.Intent.Stop <= envelope.Intent.RequestedEntry ||
                 envelope.Intent.InitialTarget >= envelope.Intent.RequestedEntry))
            {
                reason = "SELL EXECUTION GEOMETRY WRONG SIDE";
                return false;
            }

            if (!FinitePositive(robot.Symbol.Bid) ||
                !FinitePositive(robot.Symbol.Ask) ||
                !FinitePositive(robot.Symbol.PipSize))
            {
                reason = "LIVE QUOTE INVALID";
                return false;
            }

            int positions =
                BrokerExecutionSafety.CountManagedPositions(
                    robot,
                    envelope.Intent.ExecutionLabel ?? string.Empty);

            int pending =
                BrokerExecutionSafety.CountManagedPending(
                    robot,
                    envelope.Intent.ExecutionLabel ?? string.Empty);

            if (positions + pending >= 1)
            {
                reason = "SCENARIO CAPACITY BLOCKED • SCENARIO ALREADY ACTIVE";
                return false;
            }

            int managedScenarioObjects =
                BrokerExecutionSafety.CountManagedScenarioObjects(
                    robot,
                    ResolveManagedInstanceRoot(
                        envelope.Intent.ExecutionLabel ?? string.Empty));

            if (maximumConcurrentScenarios < 1 ||
                managedScenarioObjects >= maximumConcurrentScenarios)
            {
                reason =
                    "CONCURRENT SCENARIO CAPACITY BLOCKED • " +
                    Math.Max(1, maximumConcurrentScenarios);
                return false;
            }

            if (!FinitePositive(robot.Account.FreeMargin))
            {
                reason = "FREE MARGIN INVALID";
                return false;
            }

            if (robot.Account.MarginLevel.HasValue &&
                (!FinitePositive(robot.Account.MarginLevel.Value) ||
                 (robot.Account.StopOutLevel > 0 &&
                  robot.Account.MarginLevel.Value <=
                  robot.Account.StopOutLevel)))
            {
                reason = "ACCOUNT MARGIN LEVEL UNSAFE";
                return false;
            }

            if (envelope.Plan == null ||
                !FinitePositive(envelope.Plan.PlanRisk))
            {
                reason = "PLAN RISK UNAVAILABLE";
                return false;
            }

            if (settings.UseSpreadFilter)
            {
                double spread =
                    robot.Symbol.Ask -
                    robot.Symbol.Bid;

                if (!FinitePositive(spread))
                {
                    reason = "SPREAD INVALID";
                    return false;
                }

                double intentSpreadLimit =
                    envelope.Intent.MaxSpreadToStopRiskRatio;

                if (!FinitePositive(intentSpreadLimit) ||
                    !FinitePositive(settings.MaximumSpreadToStopRiskRatio))
                {
                    reason = "SPREAD RISK LIMIT UNAVAILABLE";
                    return false;
                }

                double spreadLimit =
                    Math.Min(
                        settings.MaximumSpreadToStopRiskRatio,
                        intentSpreadLimit);

                if (spread / envelope.Plan.PlanRisk >
                    Math.Max(0.02, spreadLimit))
                {
                    reason = "LIVE SPREAD EXCEEDS PLAN RISK LIMIT";
                    return false;
                }
            }

            if (!FinitePositive(maximumMarginUsagePercent) ||
                !FinitePositive(marginBufferPercent) ||
                marginBufferPercent >=
                maximumMarginUsagePercent)
            {
                reason = "MARGIN SAFETY CONFIGURATION INVALID";
                return false;
            }

            return true;
        }

        private static bool EffectiveExecutionEnabledForAction(
            Robot robot,
            CbotExecutionSettings settings,
            ExecutionAction action)
        {
            // The environment gate is the final cBot-owned policy boundary.
            // Transport/presentation state can never arm an execution mode.
            if (action == ExecutionAction.Market)
                return settings.EnableAutoTrading;

            return action == ExecutionAction.PendingStop ||
                   action == ExecutionAction.PendingLimit
                ? settings.EnableAutomaticOrders
                : settings.EnableAutoTrading;
        }

        private static string ResolveManagedInstanceRoot(
            string executionLabel)
        {
            if (string.IsNullOrWhiteSpace(executionLabel))
                return string.Empty;

            int marker =
                executionLabel.IndexOf(
                    "|CFIP-S:",
                    StringComparison.Ordinal);

            return marker > 0
                ? executionLabel.Substring(0, marker)
                : executionLabel;
        }

        private static bool FinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}
