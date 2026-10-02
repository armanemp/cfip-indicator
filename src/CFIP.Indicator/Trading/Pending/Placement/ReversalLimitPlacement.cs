using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool PlaceReversalLimit(int closedM5)
        {
            if (!CanRunAutomaticEntry())
            { ApplyRuntimeEntryGate(); return false; }

            int direction; double targetEntry, stop, target, volume; ExecutionIntent pendingIntent;
            double atr = 0;
            if (!TryPrepareReversalLimit(closedM5, out direction, out atr, out targetEntry, out stop, out target, out _, out _, out volume, out pendingIntent))
                return false;

            TradeType type = direction == 1 ? TradeType.Buy : TradeType.Sell;
            double entry = direction == 1 ? Symbol.Bid : Symbol.Ask;
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
            if (!ValidatePendingSubmission(pendingIntent, type, entry, volume, "PENDING LIMIT • ", out reason))
            { _autoOrdersBlockReason = reason; return false; }

            try
            {
                SubmissionAttemptIdentity submissionIdentity; string submissionGateReason;
                string executionScenarioId =
                    "PENDING-LIMIT-" +
                    (direction == 1 ? "BUY" : "SELL");
                _activeExecutionScenarioId =
                    executionScenarioId;
                if (!TryAcquireSubmission(closedM5, direction, ExecutionSubmissionPath.PendingLimit, executionScenarioId, out submissionIdentity, out submissionGateReason))
                { _autoOrdersBlockReason = submissionGateReason; return false; }

                RelativeTakeProfitProtections serverTakeProfits;
                StopLossBreakEven serverBreakEven;
                bool useServerTakeProfitLadder = TryBuildServerSideTakeProfitLadder(
                        pendingIntent.RequestedEntry,
                        pendingIntent.Target,
                        pendingIntent.Volume,
                        out serverTakeProfits,
                        out serverBreakEven);
                TradeResult result;
                try
                {
                    result = useServerTakeProfitLadder
                        ? TryPlaceLimitOrderWithTakeProfitLadder(type, SymbolName, volume, targetEntry, PendingOrderLabel(), pendingIntent.StopPips, serverTakeProfits, serverBreakEven, ProtectionType.Relative, PendingExpiration(), TradeExecutionMetadata.DefaultExecutionComment, false, "REVERSAL LIMIT • SERVER TP LADDER")
                        : TryPlaceLimitOrder(type, SymbolName, volume, targetEntry, PendingOrderLabel(), pendingIntent.StopPips, pendingIntent.TargetPips, ProtectionType.Relative, PendingExpiration(), TradeExecutionMetadata.DefaultExecutionComment, false, "REVERSAL LIMIT");
                }
                catch { RecordSubmissionFailure(submissionIdentity); throw; }

                RecordSubmission(submissionIdentity, result, pendingIntent);
                if (!BrokerConfirmationPolicy.CanAdoptPendingOrder(result != null, result != null && result.IsSuccessful, result != null && result.PendingOrder != null))
                {
                    _autoOrdersBlockReason = result != null && result.Error.HasValue ? result.Error.Value.ToString() : "PENDING LIMIT REJECTED";
                    return false;
                }

                _pendingOrderPlanSnapshot = pendingSnapshot; _lastPendingSignalM5 = closedM5; _plan = null; _executionModel = null; RemovePlanObjects();
                ReportConfirmedPendingOrderPlacement(result.PendingOrder, direction, closedM5, "LIMIT");
                return true;
            }
            catch (Exception ex)
            { _autoOrdersBlockReason = "PENDING LIMIT • " + ex.Message; return false; }
        }
    }
}
