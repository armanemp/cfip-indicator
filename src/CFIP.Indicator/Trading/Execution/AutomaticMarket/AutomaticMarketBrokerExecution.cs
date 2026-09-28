using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
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
                {
                    ApplyRuntimeEntryGate();
                    return;
                }

                string submissionReason;
                if (!TryValidateAutomaticMarketSubmission(
                        closedM5,
                        type,
                        entry,
                        target,
                        volume,
                        out submissionReason))
                {
                    _autoExecutionBlockReason =
                        submissionReason;

                    SetAutoTradingState(
                        "BLOCKED",
                        submissionReason);
                    return;
                }

                string submissionGateReason;

                if (!TryAcquireNormalSubmission(
                        closedM5,
                        _plan == null
                            ? 0
                            : _plan.Direction,
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
                            targetPips,
                            TradeExecutionMetadata.DefaultExecutionComment,
                            false,
                            "AUTOMATIC MARKET");
                }
                catch
                {
                    RecordNormalSubmissionFailure();
                    throw;
                }

                RecordNormalSubmission(result);

                if (result == null)
                {
                    _autoExecutionBlockReason =
                        "NULL TRADE RESULT";
                    SetAutoTradingState(
                        "ERROR",
                        "NULL TRADE RESULT");
                    return;
                }

                if (!BrokerConfirmationPolicy.CanAdoptPosition(
                        true,
                        result.IsSuccessful,
                        result.Position != null))
                {
                    _autoExecutionBlockReason =
                        result.Error.HasValue
                            ? result.Error.Value.ToString()
                            : "TRADE REJECTED";

                    SetAutoTradingState(
                        "ERROR",
                        _autoExecutionBlockReason);
                    return;
                }

                if (!TryAcceptAutomaticMarketFill(
                        closedM5,
                        result))
                    return;

                if (!TryResolveAutomaticPostFillTarget(
                        closedM5,
                        result.Position,
                        ref target))
                    return;

                bool protectionOk = true;

                if (AutoBrokerProtection)
                {
                    protectionOk =
                        EnsureBrokerProtectionForPosition(
                            result.Position,
                            _plan.Stop,
                            target,
                            "NEW MARKET ENTRY",
                            _plan.Direction);
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

                SendUnifiedAlert(
                    "AUTO|" +
                    closedM5,
                    "CFIP AUTO " +
                    (_plan.Direction == 1
                        ? "BUY"
                        : "SELL") +
                    " EXECUTED | #" +
                    result.Position.Id +
                    " | ENTRY " +
                    Price(
                        result.Position.EntryPrice) +
                    " | SL " +
                    Price(
                        _plan.Stop) +
                    " | TP " +
                    Price(
                        target),
                    _plan.Direction,
                    true);
            }
            catch (Exception ex)
            {
                SetAutoTradingState(
                    "ERROR",
                    ex.Message);

                Print(
                    "CFIP auto trade failed: {0}",
                    ex.Message);
            }
        }
    }
}
