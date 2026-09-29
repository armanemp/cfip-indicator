using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int ResolveCanonicalVisualDirection(
            int pendingDirection,
            bool livePlan,
            bool decisionReady,
            bool reactionReady,
            bool predictionReady)
        {
            if (!UseAuthoritativeSignalState)
            {
                return decisionReady &&
                       _decision != null
                    ? _decision.Direction
                    : 0;
            }

            if (livePlan &&
                _plan != null)
                return _plan.Direction;

            if (pendingDirection != 0)
                return pendingDirection;

            if (_plan != null &&
                (_plan.Direction == 1 ||
                 _plan.Direction == -1))
                return _plan.Direction;

            if (decisionReady &&
                _decision != null)
                return _decision.Direction;

            if (reactionReady &&
                _reaction != null)
                return _reaction.Direction;

            if (_m5Frame != null &&
                (_m5Frame.Direction == 1 ||
                 _m5Frame.Direction == -1) &&
                _m5Frame.Quality >=
                    Math.Max(
                        50,
                        LiveReactionStrongThreshold) &&
                ((_m5Frame.Direction == 1 &&
                  (_m5Frame.MssBull ||
                   _m5Frame.ChochBull)) ||
                 (_m5Frame.Direction == -1 &&
                  (_m5Frame.MssBear ||
                   _m5Frame.ChochBear))))
                return _m5Frame.Direction;

            if (predictionReady &&
                _prediction != null)
                return _prediction.Direction;

            return 0;
        }

        private SignalVisualSnapshot BuildSignalVisualSnapshot(
            int closedM5)
        {
            SignalVisualSnapshot snapshot =
                new SignalVisualSnapshot
                {
                    ClosedM5 = closedM5,
                    AuthoritativeDirection = 0,
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

            snapshot.PlanDirection =
                _plan == null ? 0 : _plan.Direction;
            snapshot.PendingDirection =
                pendingValid
                    ? (pending.TradeType == TradeType.Buy ? 1 : -1)
                    : 0;
            snapshot.DecisionDirection =
                _decision == null ? 0 : _decision.Direction;
            snapshot.ReactionDirection =
                _reaction == null ? 0 : _reaction.Direction;

            bool predictionReady =
                _prediction != null &&
                _prediction.Direction != 0 &&
                _prediction.Confidence >= Math.Max(
                    MinimumEarlyConfidence,
                    EarlySetupConfidence);

            int visualDirection =
                ResolveCanonicalVisualDirection(
                    pendingValid
                        ? (pending.TradeType == TradeType.Buy ? 1 : -1)
                        : 0,
                    livePlan,
                    decisionReady,
                    reactionReady,
                    predictionReady);

            if (livePlan)
            {
                snapshot.PlanActive = true;
                snapshot.LivePosition = true;
                snapshot.AuthoritativeDirection = _plan.Direction;
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
            }
            else if (_plan != null &&
                     (_plan.Direction == 1 || _plan.Direction == -1))
            {
                snapshot.PlanActive = true;
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
                snapshot.Stage = "CONFIRMED";
            }
            else if (reactionReady)
            {
                snapshot.Stage = "REACTION";
            }
            else if (predictionReady)
            {
                snapshot.Stage = "PREDICTION";
            }
            else
            {
                snapshot.Stage =
                    visualDirection == 0
                        ? "WAIT"
                        : "WATCH";
            }

            snapshot.AuthoritativeDirection =
                visualDirection;

            snapshot.DecisionReady = decisionReady;
            snapshot.ReactionReady = reactionReady;
            snapshot.PredictionReady = predictionReady;
            snapshot.SmartQuality =
                _decision == null ? 0 : _decision.SmartQuality;
            snapshot.Confidence =
                _decision == null ? 0 : _decision.Confidence;
            snapshot.DecisionReason =
                _decision == null ? "" : _decision.Reason;
            snapshot.ReactionReason =
                _reaction == null ? "" : _reaction.Reason;
            snapshot.ReactionConfidence =
                _reaction == null ? 0 : _reaction.Confidence;
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
            snapshot.ArrowM5Index =
                reactionReady &&
                snapshot.AuthoritativeDirection == _reaction.Direction &&
                _m5Bars != null
                    ? _m5Bars.Count - 1
                    : closedM5;

            return snapshot;
        }

    }
}