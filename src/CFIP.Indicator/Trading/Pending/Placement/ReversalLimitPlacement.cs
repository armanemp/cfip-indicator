using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool PlaceReversalLimit(int closedM5)
        {
            if (!CanRunAutomaticEntry())
            {
                ApplyRuntimeEntryGate();
                return false;
            }

            int direction;
            double targetEntry, stop, target, volume;
            ExecutionIntent pendingIntent;

            if (!TryPrepareReversalLimit(
                    closedM5,
                    out direction,
                    out _,
                    out targetEntry,
                    out stop,
                    out target,
                    out _,
                    out _,
                    out volume,
                    out pendingIntent))
                return false;

            TradeType tradeType = direction == 1 ? TradeType.Buy : TradeType.Sell;
            double entryPrice = direction == 1 ? Symbol.Bid : Symbol.Ask;
            string reason;

            if (!ValidatePendingSubmission(
                    pendingIntent,
                    tradeType,
                    entryPrice,
                    volume,
                    "PENDING LIMIT • ",
                    out reason))
            {
                _autoOrdersBlockReason = reason;
                return false;
            }

            try
            {
                SubmissionAttemptIdentity submissionIdentity;
                string submissionGateReason;

                if (!TryAcquireSubmission(
                        closedM5,
                        direction,
                        ExecutionSubmissionPath.PendingLimit,
                        out submissionIdentity,
                        out submissionGateReason))
                {
                    _autoOrdersBlockReason = submissionGateReason;
                    return false;
                }

                RelativeTakeProfitProtections serverTakeProfits;
                bool useServerTakeProfitLadder =
                    TryBuildServerSideTakeProfitLadder(
                        targetEntry,
                        target,
                        volume,
                        out serverTakeProfits);

                TradeResult result;

                try
                {
                    result = useServerTakeProfitLadder
                        ? TryPlaceLimitOrderWithTakeProfitLadder(
                            tradeType,
                            SymbolName,
                            volume,
                            targetEntry,
                            PendingOrderLabel(),
                            pendingIntent.StopPips,
                            serverTakeProfits,
                            ProtectionType.Relative,
                            PendingExpiration(),
                            TradeExecutionMetadata.DefaultExecutionComment,
                            false,
                            "REVERSAL LIMIT • SERVER TP LADDER")
                        : TryPlaceLimitOrder(
                            tradeType,
                            SymbolName,
                            volume,
                            targetEntry,
                            PendingOrderLabel(),
                            pendingIntent.StopPips,
                            pendingIntent.TargetPips,
                            ProtectionType.Relative,
                            PendingExpiration(),
                            TradeExecutionMetadata.DefaultExecutionComment,
                            false,
                            "REVERSAL LIMIT");
                }
                catch
                {
                    RecordSubmissionFailure(submissionIdentity);
                    throw;
                }

                RecordSubmission(submissionIdentity, result);

                if (!BrokerConfirmationPolicy.CanAdoptPendingOrder(
                        result != null,
                        result != null && result.IsSuccessful,
                        result != null && result.PendingOrder != null))
                {
                    _autoOrdersBlockReason =
                        result != null && result.Error.HasValue
                            ? result.Error.Value.ToString()
                            : "PENDING LIMIT REJECTED";
                    return false;
                }

                _lastPendingSignalM5 = closedM5;
                _plan = null;
                _executionModel = null;
                RemovePlanObjects();

                ReportConfirmedPendingOrderPlacement(
                    result.PendingOrder,
                    direction,
                    closedM5,
                    "LIMIT");

                return true;
            }
            catch (Exception ex)
            {
                _autoOrdersBlockReason = "PENDING LIMIT • " + ex.Message;
                return false;
            }
        }
    }
}
