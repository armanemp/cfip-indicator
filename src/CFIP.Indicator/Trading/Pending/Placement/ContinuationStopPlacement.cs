using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool PlaceContinuationStop(int closedM5)
        {
            if (!CanRunAutomaticEntry())
            { ApplyRuntimeEntryGate(); return false; }

            int direction; double trigger, stop, target, volume; ExecutionIntent pendingIntent;
            double atr = 0;
            if (!TryPrepareContinuationStop(closedM5, out direction, out atr, out trigger, out stop, out target, out _, out _, out volume, out pendingIntent))
                return false;

            TradeType type = direction == 1 ? TradeType.Buy : TradeType.Sell;
            double entry = direction == 1 ? Symbol.Ask : Symbol.Bid;
            Plan pendingSnapshot =
                CapturePendingOrderPlanSnapshot(
                    pendingIntent,
                    closedM5,
                    atr);

            if (pendingSnapshot == null)
            {
                _autoOrdersBlockReason =
                    "PENDING • ABSOLUTE PLAN SNAPSHOT UNAVAILABLE";
                return false;
            }

            string reason;
            if (!ValidatePendingSubmission(pendingIntent, type, entry, volume, "PENDING STOP • ", out reason))
            { _autoOrdersBlockReason = reason; return false; }

            try
            {
                SubmissionAttemptIdentity submissionIdentity; string submissionGateReason;
                string executionScenarioId =
                    "PENDING-STOP-" +
                    (direction == 1 ? "BUY" : "SELL");
                _activeExecutionScenarioId =
                    executionScenarioId;
                if (!TryAcquireSubmission(closedM5, direction, ExecutionSubmissionPath.PendingStop, executionScenarioId, out submissionIdentity, out submissionGateReason))
                { _autoOrdersBlockReason = submissionGateReason; return false; }

                RelativeTakeProfitProtections serverTakeProfits;
                StopLossBreakEven serverBreakEven;
                bool useServerTakeProfitLadder =
                    TryBuildServerSideTakeProfitLadder(
                        pendingIntent.RequestedEntry,
                        pendingIntent.Target,
                        pendingIntent.Volume,
                        out serverTakeProfits,
                        out serverBreakEven);
                string label = PendingOrderLabel();
                string comment = TradeExecutionMetadata.DefaultExecutionComment;
                TradeResult result;
                try
                {
                    result = useServerTakeProfitLadder
                        ? TryPlaceStopOrderWithTakeProfitLadder(
                            type, SymbolName, volume, trigger, label,
                            pendingIntent.StopPips, serverTakeProfits, serverBreakEven,
                            ProtectionType.Relative, PendingExpiration(), comment,
                            false, "CONTINUATION STOP • SERVER TP LADDER")
                        : TryPlaceStopOrder(
                            type, SymbolName, volume, trigger, label,
                            pendingIntent.StopPips, pendingIntent.TargetPips,
                            ProtectionType.Relative, PendingExpiration(), comment,
                            false, "CONTINUATION STOP");
                }
                catch { RecordSubmissionFailure(submissionIdentity); throw; }

                RecordSubmission(submissionIdentity, result, pendingIntent);
                if (!BrokerConfirmationPolicy.CanAdoptPendingOrder(result != null, result != null && result.IsSuccessful, result != null && result.PendingOrder != null))
                {
                    _autoOrdersBlockReason = result != null && result.Error.HasValue ? result.Error.Value.ToString() : "PENDING STOP REJECTED";
                    return false;
                }

                _pendingOrderPlanSnapshot = pendingSnapshot; _lastPendingSignalM5 = closedM5; _plan = null; _executionModel = null; RemovePlanObjects();
                ReportConfirmedPendingOrderPlacement(result.PendingOrder, direction, closedM5, "STOP");
                return true;
            }
            catch (Exception ex)
            { _autoOrdersBlockReason = "PENDING STOP • " + ex.Message; return false; }
        }
    }
}
