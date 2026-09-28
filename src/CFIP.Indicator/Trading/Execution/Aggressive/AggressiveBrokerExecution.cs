using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ExecuteAggressiveTrade(
            int closedM5,
            TradeType type,
            double entry,
            double atr,
            double stop,
            double target,
            double stopPips,
            double tpPips,
            double volume)
        {
            try
            {
                if (!CanRunAutomaticEntry())
                {
                    ApplyRuntimeEntryGate();
                    return;
                }

                if (!EnsureTradingPermission())
                {
                    _autoExecutionBlockReason =
                        "TRADING PERMISSION";
                    SetAutoTradingState(
                        "BLOCKED",
                        "TRADING PERMISSION NOT GRANTED");
                    return;
                }

                string guardReason;

                if (!PassesAutoTradeSafetyGuards(
                        type,
                        volume,
                        out guardReason))
                {
                    SetAutoTradingState(
                        "BLOCKED",
                        guardReason);
                    return;
                }

                ExecutionIntent aggressiveIntent =
                    BuildExecutionIntent(
                        _reaction.Direction,
                        DecisionPolicyMode.Aggressive,
                        ExecutionIntentKind.Market,
                        entry,
                        0,
                        0,
                        0,
                        stop,
                        target,
                        volume,
                        closedM5,
                        "AGGRESSIVE MARKET");

                string aggressiveIntentReason;

                if (!ValidateExecutionIntent(
                        aggressiveIntent,
                        entry,
                        out aggressiveIntentReason))
                {
                    SetAutoTradingState(
                        "BLOCKED",
                        aggressiveIntentReason);
                    return;
                }

                string submissionGateReason;

                if (!TryAcquireAggressiveSubmission(
                        closedM5,
                        _reaction == null
                            ? 0
                            : _reaction.Direction,
                        out submissionGateReason))
                {
                    _autoExecutionBlockReason =
                        submissionGateReason;

                    SetAutoTradingState(
                        "BLOCKED",
                        submissionGateReason);
                    return;
                }

                TradeResult result;

                try
                {
                    result =
                        TryExecuteMarketOrder(
                            type,
                            SymbolName,
                            volume,
                            NormalizeLabel(),
                            stopPips,
                            tpPips,
                            TradeExecutionMetadata.DefaultExecutionComment,
                            false,
                            "AGGRESSIVE MARKET");
                }
                catch
                {
                    RecordAggressiveSubmissionFailure();
                    throw;
                }

                RecordAggressiveSubmission(result);

                if (!BrokerConfirmationPolicy.CanAdoptPosition(
                        result != null,
                        result != null &&
                        result.IsSuccessful,
                        result != null &&
                        result.Position != null))
                {
                    _autoExecutionBlockReason =
                        result != null &&
                        result.Error.HasValue
                            ? "AGGRESSIVE • " +
                              result.Error.Value.ToString()
                            : "AGGRESSIVE • TRADE REJECTED";
                    SetAutoTradingState(
                        "ERROR",
                        _autoExecutionBlockReason);
                    return;
                }

                if (!TryProcessAcceptedAggressiveFill(
                        closedM5,
                        entry,
                        atr,
                        stop,
                        target,
                        aggressiveIntent,
                        result,
                        out double actualStop,
                        out double actualTarget))
                    return;

                bool protectionOk = true;

                if (AutoBrokerProtection)
                {
                    protectionOk =
                        EnsureBrokerProtectionForPosition(
                            result.Position,
                            actualStop,
                            actualTarget,
                            "AGGRESSIVE ENTRY",
                            _reaction.Direction);
                }

                SetAutoTradingState(
                    protectionOk
                        ? "EXECUTED"
                        : "RECOVERY",
                    protectionOk
                        ? "POSITION #" +
                          result.Position.Id
                        : "POSITION #" +
                          result.Position.Id +
                          " • BROKER PROTECTION RECOVERY");

                double confirmedStop =
                    GetActiveBrokerStopPrice();

                double confirmedTarget =
                    GetActiveBrokerTargetPrice();

                SendUnifiedAlert(
                    "AUTO-REACTION|" +
                    closedM5,
                    "CFIP AUTO REACTION " +
                    (_reaction.Direction == 1
                        ? "BUY"
                        : "SELL") +
                    " EXECUTED | #" +
                    result.Position.Id +
                    " | ENTRY " +
                    Price(
                        result.Position.EntryPrice) +
                    " | BROKER SL " +
                    (IsFinitePositive(confirmedStop)
                        ? Price(confirmedStop)
                        : "RECOVERY") +
                    " | BROKER TP " +
                    (IsFinitePositive(confirmedTarget)
                        ? Price(confirmedTarget)
                        : "RECOVERY"),
                    _reaction.Direction,
                    true);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP aggressive auto trade failed: {0}",
                    ex.Message);
            }
        }
    }
}
