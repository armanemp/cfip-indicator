using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool PlaceContinuationStop(int closedM5)
        {
            if (!TryPrepareContinuationStop(
                    closedM5,
                    out int direction,
                    out _,
                    out double trigger,
                    out double stop,
                    out double target,
                    out _,
                    out _,
                    out double volume,
                    out ExecutionIntent pendingIntent))
                return false;

            TradeType type = direction == 1 ? TradeType.Buy : TradeType.Sell;
            string reason;

            if (!ValidatePendingSubmission(
                    pendingIntent,
                    type,
                    direction == 1 ? Symbol.Ask : Symbol.Bid,
                    volume,
                    "PENDING STOP • ",
                    out reason))
            {
                _autoOrdersBlockReason =
                    reason;
                return false;
            }

            string submissionGateReason;

                if (!TryAcquirePendingSubmission(
                        closedM5,
                        direction,
                        "STOP",
                        out submissionGateReason))
                {
                    _autoOrdersBlockReason =
                        submissionGateReason;
                    return false;
                }

                TradeResult result;

                try
                {
                    result =
                        TryPlaceStopOrder(
                            type,
                            SymbolName,
                            volume,
                            trigger,
                            PendingOrderLabel(),
                            pendingIntent.StopPips,
                            pendingIntent.TargetPips,
                            ProtectionType.Relative,
                            PendingExpiration(),
                            TradeExecutionMetadata.DefaultExecutionComment,
                            false,
                            "CONTINUATION STOP");
                }
            catch
            {
                RecordPendingSubmissionFailure();
                throw;
            }

            RecordPendingSubmission(result);

                if (!BrokerConfirmationPolicy.CanAdoptPendingOrder(
                        result != null,
                        result != null &&
                        result.IsSuccessful,
                        result != null &&
                        result.PendingOrder != null))
                {
                    _autoOrdersBlockReason =
                        result != null &&
                        result.Error.HasValue
                            ? result.Error.Value.ToString()
                            : "PENDING STOP REJECTED";
                    return false;
                }

                _lastPendingSignalM5 =
                    closedM5;

                _plan = null;
                _executionModel = null;
                RemovePlanObjects();

                _autoOrdersBlockReason =
                    "ORDER PLACED • STOP " +
                    Price(trigger);

                SendUnifiedAlert(
                    "PENDING-STOP|" +
                    closedM5,
                    "CFIP STOP ORDER | " +
                    (direction == 1
                        ? "BUY"
                        : "SELL") +
                    " | ENTRY " +
                    Price(trigger) +
                    " | SL " +
                    Price(stop) +
                    " | TP " +
                    Price(target),
                    direction,
                    true);

                return true;
            }
            catch (Exception ex)
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • " +
                    ex.Message;

                Print(
                    "CFIP pending stop failed: {0}",
                    ex.Message);

                return false;            }
        }
    }
}