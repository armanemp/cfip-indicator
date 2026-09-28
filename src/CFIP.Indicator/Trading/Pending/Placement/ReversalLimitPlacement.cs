using System;
using cAlgo.API;
namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool PlaceReversalLimit(
            int closedM5)
        {
            if (!TryPrepareReversalLimit(closedM5, out int direction, out _, out double targetEntry, out double stop, out double target, out _, out _, out double volume, out ExecutionIntent pendingIntent))
                return false;
            string reason;
            if (!ValidatePendingSubmission(
                    pendingIntent,
                    direction == 1
                        ? TradeType.Buy
                        : TradeType.Sell,
                    direction == 1
                        ? Symbol.Bid
                        : Symbol.Ask,
                    volume,
                    "PENDING LIMIT • ",
                    out reason))
            {
                _autoOrdersBlockReason = reason;
                return false;
            }
            try
            {
                string submissionKey =
                    "LIMIT|" +
                    closedM5 +
                    "|" +
                    direction;
                string submissionGateReason;
                if (!_pendingSubmissionGate.TryAcquire(
                        Server.TimeInUtc,
                        submissionKey,
                        out submissionGateReason))
                {
                    _autoOrdersBlockReason = submissionGateReason;
                    return false;
                }
                TradeResult result;
                try
                {
                    result =
                        TryPlaceLimitOrder(
                            direction == 1
                                ? TradeType.Buy
                                : TradeType.Sell,
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
                    _pendingSubmissionGate.Record(
                        Server.TimeInUtc,
                        false);
                    throw;
                }
                _pendingSubmissionGate.Record(
                    Server.TimeInUtc,
                    result != null &&
                    result.IsSuccessful &&
                    result.PendingOrder != null);
                if (!BrokerConfirmationPolicy.CanAdoptPendingOrder(result != null, result?.IsSuccessful == true, result?.PendingOrder != null))
                {
                    _autoOrdersBlockReason =
                        result != null &&
                        result.Error.HasValue
                            ? result.Error.Value.ToString()
                            : "PENDING LIMIT REJECTED";
                    return false;
                }
                _lastPendingSignalM5 = closedM5;
                _plan = null;
                _executionModel = null;
                RemovePlanObjects();
                _autoOrdersBlockReason = "ORDER PLACED • LIMIT " + Price(targetEntry);
                SendUnifiedAlert(
                    "PENDING-LIMIT|" + closedM5,
                    "CFIP LIMIT | " + (direction == 1 ? "BUY" : "SELL") +
                    " | ENTRY " + Price(targetEntry) + " | SL " + Price(stop) + " | TP " + Price(target),
                    direction,
                    true);
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
