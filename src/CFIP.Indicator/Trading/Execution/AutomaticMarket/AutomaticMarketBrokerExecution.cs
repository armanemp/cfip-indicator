using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TradeResult TryExecuteAutomaticMarketCompatibilityPathV97(
            TradeType type,
            double volume,
            double stopPips,
            double targetPips)
        {
            return TryExecuteMarketOrder(
                type,
                SymbolName,
                volume,
                NormalizeLabel(),
                stopPips,
                targetPips,
                TradeExecutionMetadata.DefaultExecutionComment,
                false,
                "AUTOMATIC MARKET FALLBACK");
        }

        private void ExecutePreparedAutomaticMarketTrade(
            int closedM5,
            TradeType type,
            double entry,
            double stopPips,
            double targetPips,
            double target,
            double volume)
        {
            try
            {
                if (!CanRunAutomaticEntry())
                { ApplyRuntimeEntryGate(); return; }

                string submissionReason;
                if (!TryValidateAutomaticMarketSubmission(
                        closedM5, type, entry, target, volume,
                        out submissionReason))
                {
                    _autoExecutionBlockReason = submissionReason;
                    SetAutoTradingState("BLOCKED", submissionReason);
                    return;
                }

                string submissionGateReason;
                SubmissionAttemptIdentity submissionIdentity;
                int direction = _plan == null ? 0 : _plan.Direction;
                string executionScenarioId =
                    ResolvePlanExecutionScenarioId(
                        closedM5);

                _activeExecutionScenarioId =
                    executionScenarioId;

                if (!TryAcquireSubmission(
                        closedM5,
                        direction,
                        ExecutionSubmissionPath.AutomaticMarket,
                        executionScenarioId,
                        out submissionIdentity,
                        out submissionGateReason))
                {
                    _autoExecutionBlockReason = submissionGateReason;
                    SetAutoTradingState("BLOCKED", submissionGateReason);
                    return;
                }

                double marketRangePips =
                    CalculateAutomaticMarketRangePips(closedM5, entry);

                if (marketRangePips <= 0)
                {
                    _autoExecutionBlockReason = "MARKET RANGE UNAVAILABLE";
                    SetAutoTradingState("BLOCKED", _autoExecutionBlockReason);
                    return;
                }

                RelativeTakeProfitProtections serverTakeProfits;
                StopLossBreakEven serverBreakEven;
                bool useServerTakeProfitLadder =
                    TryBuildServerSideTakeProfitLadder(
                        entry, target, volume, out serverTakeProfits, out serverBreakEven);

                TradeResult result;
                try
                {
                    result = useServerTakeProfitLadder
                        ? TryExecuteMarketRangeOrderWithTakeProfitLadder(
                            type, SymbolName, volume, marketRangePips, entry,
                            NormalizeLabel(), stopPips, serverTakeProfits, serverBreakEven,
                            TradeExecutionMetadata.DefaultExecutionComment,
                            false, "AUTOMATIC MARKET RANGE • SERVER TP LADDER")
                        : TryExecuteMarketRangeOrder(
                            type, SymbolName, volume, marketRangePips, entry,
                            NormalizeLabel(), stopPips, targetPips,
                            TradeExecutionMetadata.DefaultExecutionComment,
                            false, "AUTOMATIC MARKET RANGE");
                }
                catch
                {
                    RecordSubmissionFailure(submissionIdentity);
                    throw;
                }

                RecordSubmission(submissionIdentity, result);

                if (result == null)
                {
                    _autoExecutionBlockReason = "NULL TRADE RESULT";
                    SetAutoTradingState("ERROR", "NULL TRADE RESULT");
                    return;
                }

                if (!BrokerConfirmationPolicy.CanAdoptPosition(
                        true, result.IsSuccessful, result.Position != null))
                {
                    _autoExecutionBlockReason =
                        result.Error.HasValue
                            ? result.Error.Value.ToString()
                            : "TRADE REJECTED";
                    SetAutoTradingState("ERROR", _autoExecutionBlockReason);
                    return;
                }

                if (!TryAcceptAutomaticMarketFill(closedM5, result))
                    return;

                AdoptServerSideTakeProfitLadder(result.Position);

                if (!TryResolveAutomaticPostFillTarget(
                        closedM5, result.Position, ref target))
                    return;

                bool protectionOk = true;
                if (AutoBrokerProtection)
                    protectionOk = EnsureBrokerProtectionForPosition(
                        result.Position,
                        _plan.Stop,
                        target,
                        "NEW MARKET ENTRY",
                        _plan.Direction);

                SetAutoTradingState(
                    protectionOk ? "EXECUTED" : "RECOVERY",
                    protectionOk
                        ? "POSITION #" + result.Position.Id
                        : "POSITION #" + result.Position.Id +
                          " • BROKER PROTECTION RECOVERY");

                double confirmedStop = GetActiveBrokerStopPrice();
                double confirmedTarget = GetActiveBrokerTargetPrice();

                SendUnifiedAlert(
                    "AUTO|" + closedM5,
                    "CFIP AUTO " +
                    (_plan.Direction == 1 ? "BUY" : "SELL") +
                    " EXECUTED | #" + result.Position.Id +
                    " | ENTRY " + Price(result.Position.EntryPrice) +
                    " | BROKER SL " +
                    (IsFinitePositive(confirmedStop) ? Price(confirmedStop) : "RECOVERY") +
                    " | BROKER TP " +
                    (IsFinitePositive(confirmedTarget) ? Price(confirmedTarget) : "RECOVERY"),
                    _plan.Direction,
                    true);
            }
            catch (Exception ex)
            {
                SetAutoTradingState("ERROR", ex.Message);
                Print("CFIP auto trade failed: {0}", ex.Message);
            }
        }
    }
}
