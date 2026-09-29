using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private SignalVisualSnapshot BuildSignalVisualSnapshot(
            int closedM5)
        {
            SignalVisualSnapshot snapshot =
                new SignalVisualSnapshot
                {
                    ClosedM5 = closedM5,
                    Direction = 0,
                    Stage = "WAIT",
                    CreatedM5 = -1,
                    PositionId = 0,
                    PendingOrderId = 0,
                    PendingOrderType = "",
                    EntryMode = ExecutionMode.WaitingForTrigger,
                    DecisionReason = "",
                    Regime = "UNKNOWN"
                };

            PendingOrder pending =
                GetManagedPendingOrder();

            bool pendingValid =
                pending != null &&
                IsFinitePositive(pending.TargetPrice);

            bool livePlan =
                _plan != null &&
                _plan.IsLivePosition &&
                _plan.PositionId > 0;

            bool decisionReady =
                _decision != null &&
                _decision.EntryAllowed &&
                _decision.Direction != 0;

            bool reactionReady =
                EnableLiveReaction &&
                ShowReactionArrow &&
                _reaction != null &&
                _reaction.EntryAllowed &&
                _reaction.Direction != 0;

            bool predictionReady =
                _prediction != null &&
                _prediction.Direction != 0 &&
                _prediction.Confidence >= Math.Max(
                    MinimumEarlyConfidence,
                    EarlySetupConfidence);

            if (livePlan)
            {
                snapshot.PlanActive = true;
                snapshot.LivePosition = true;
                snapshot.Direction = _plan.Direction;
                snapshot.Stage = "ACTIVE";
                snapshot.EntryMode = _plan.EntryMode;
                snapshot.CreatedM5 = _plan.CreatedM5;
                snapshot.PositionId = _plan.PositionId;
                snapshot.Entry = _plan.Entry;
                snapshot.IdealEntry = _plan.IdealEntry;
                snapshot.Trigger = _plan.EntryTrigger;
                snapshot.Invalidation = _plan.EntryInvalidation;
                snapshot.BrokerStop = GetActiveBrokerStopPrice();
                snapshot.Stop = snapshot.BrokerStop;
                snapshot.Tp1 = _plan.Tp1;
                snapshot.Tp2 = _plan.Tp2;
                snapshot.Tp3 = _plan.Tp3;
                snapshot.Tp4 = _plan.Tp4;
                snapshot.BrokerTarget = GetActiveBrokerTargetPrice();
            }
            else if (pendingValid)
            {
                snapshot.PendingOrder = true;
                snapshot.Direction =
                    pending.TradeType == TradeType.Buy ? 1 : -1;
                snapshot.Stage = "PENDING";
                snapshot.PendingOrderId = pending.Id;
                snapshot.PendingOrderType =
                    pending.OrderType == PendingOrderType.Stop
                        ? "STOP"
                        : pending.OrderType == PendingOrderType.Limit
                            ? "LIMIT"
                            : "PENDING";
                snapshot.PendingEntry = pending.TargetPrice;
                snapshot.PendingStop = pending.StopLoss.HasValue
                    ? pending.StopLoss.Value : 0;
                snapshot.PendingTarget = pending.TakeProfit.HasValue
                    ? pending.TakeProfit.Value : 0;
                return snapshot;
            }
            else if (_plan != null &&
                     (_plan.Direction == 1 || _plan.Direction == -1))
            {
                snapshot.PlanActive = true;
                snapshot.Direction = _plan.Direction;
                snapshot.Stage = "PLAN";
                snapshot.EntryMode = _plan.EntryMode;
                snapshot.CreatedM5 = _plan.CreatedM5;
                snapshot.Entry = _plan.Entry;
                snapshot.IdealEntry = _plan.IdealEntry;
                snapshot.Trigger = _plan.EntryTrigger;
                snapshot.Invalidation = _plan.EntryInvalidation;
                snapshot.Stop = _plan.Stop;
                snapshot.Tp1 = _plan.Tp1;
                snapshot.Tp2 = _plan.Tp2;
                snapshot.Tp3 = _plan.Tp3;
                snapshot.Tp4 = _plan.Tp4;
            }
            else if (decisionReady)
            {
                snapshot.Direction = _decision.Direction;
                snapshot.Stage = "CONFIRMED";
            }
            else if (reactionReady)
            {
                snapshot.Direction = _reaction.Direction;
                snapshot.Stage = "REACTION";
            }
            else if (predictionReady)
            {
                snapshot.Direction = _prediction.Direction;
                snapshot.Stage = "PREDICTION";
            }
            else
            {
                snapshot.Direction =
                    _decision != null ? _decision.Direction : 0;
                snapshot.Stage = snapshot.Direction == 0
                    ? "WAIT" : "WATCH";
            }

            snapshot.DecisionReady = decisionReady;
            snapshot.ReactionReady = reactionReady;
            snapshot.PredictionReady = predictionReady;
            snapshot.SmartQuality =
                _decision == null ? 0 : _decision.SmartQuality;
            snapshot.Confidence =
                _decision == null ? 0 : _decision.Confidence;
            snapshot.DecisionReason =
                _decision == null ? "" : _decision.Reason;
            snapshot.Regime =
                _decision == null || string.IsNullOrWhiteSpace(_decision.Regime)
                    ? "UNKNOWN"
                    : _decision.Regime;

            snapshot.TriggerVisible =
                snapshot.PlanActive &&
                !snapshot.LivePosition &&
                IsFinitePositive(snapshot.Trigger) &&
                (snapshot.EntryMode == ExecutionMode.WaitingForTrigger ||
                 snapshot.EntryMode == ExecutionMode.ContinuationStop);
            snapshot.IdealEntryVisible =
                snapshot.PlanActive &&
                IsFinitePositive(snapshot.IdealEntry) &&
                !SamePrice(snapshot.IdealEntry, snapshot.Entry);
            snapshot.ActiveBrokerTargetVisible =
                snapshot.LivePosition &&
                IsFinitePositive(snapshot.BrokerTarget);

            return snapshot;
        }

    }
}