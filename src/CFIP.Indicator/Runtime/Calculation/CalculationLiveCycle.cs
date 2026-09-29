using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void UpdateLiveReaction()
        {
            if (_m5Bars == null ||
                _m5Bars.Count < 10)
                return;

            int liveM5 =
                _m5Bars.Count - 1;

            DateTime now =
                Server.TimeInUtc;

            double market =
                Symbol.Ask > 0 &&
                Symbol.Bid > 0
                    ? (Symbol.Ask + Symbol.Bid) * 0.5
                    : Symbol.Ask;

            double atr =
                liveM5 > 1
                    ? Atr(
                        _m5Bars,
                        liveM5 - 1)
                    : 0;

            bool newM5 =
                liveM5 != _lastReactionM5;

            bool priceMoved =
                _lastReactionMarket <= 0 ||
                Math.Abs(
                    market -
                    _lastReactionMarket) >=
                Math.Max(
                    Symbol.TickSize * 2,
                    atr * 0.01);

            bool intervalElapsed =
                (now -
                 _lastReactionCalcUtc)
                .TotalMilliseconds >= 750;

            if (_reaction == null ||
                newM5 ||
                (intervalElapsed &&
                 priceMoved))
            {
                _reaction =
                    BuildReaction();

                _lastReactionCalcUtc =
                    now;

                _lastReactionM5 =
                    liveM5;

                _lastReactionMarket =
                    market;
            }
        }

        private void TryEnsureAutomaticPlan(
            int closedM5)
        {
            // A confirmed predictive pending order is its own executable plan. Do not
            // recreate a chart/market plan behind it in the same or a later cycle.
            if (GetManagedPendingOrder() != null)
            {
                _lastAutoPlanAttemptM5 =
                    closedM5;
                return;
            }

            RefreshLiveDecisionActionability(
                closedM5);

            bool triggerRevisionChanged =
                _lastAutoPlanTriggerM1 !=
                _triggerRuntime.ConfirmedM1;

            if (!AutoTradingEnabled ||
                _plan != null ||
                _decision == null ||
                _decision.Direction == 0 ||
                (_lastAutoPlanAttemptM5 == closedM5 &&
                 !triggerRevisionChanged))
                return;

            _lastAutoPlanAttemptM5 =
                closedM5;

            _lastAutoPlanTriggerM1 =
                _triggerRuntime.ConfirmedM1;

            EnsureSignalPlan(
                closedM5,
                ConfirmedSignalsOnly
                    ? DecisionPolicyMode.Confirmed
                    : DecisionPolicyMode.Soft);
        }

        private void UpdateExecutionModel(
            int closedM5)
        {
            if (_decision == null ||
                _decision.Direction == 0 ||
                _plan != null)
            {
                _executionModel = null;
                _setupPreview = null;
                _lastExecutionModelM5 = -1;
                _lastExecutionModelMarket = 0;

                RefreshParallelOpportunityCandidates(
                    closedM5);
                return;
            }

            int direction =
                _decision.Direction;

            double market =
                direction == 1
                    ? Symbol.Ask
                    : Symbol.Bid;

            DateTime now =
                Server.TimeInUtc;

            double atr =
                _m5Bars != null &&
                closedM5 >= 1
                    ? Atr(
                        _m5Bars,
                        closedM5)
                    : 0;

            bool m5Changed =
                _lastExecutionModelM5 != closedM5;

            bool directionChanged =
                _executionModel == null ||
                _executionModel.Direction != direction;

            // Execution-model geometry is structural. Do not rebuild it merely because
            // the quote moved; otherwise Entry/SL/TP previews become a disguised market
            // follower. Rebuild on a newly closed M5 bar or a direction change only.
            if (!m5Changed &&
                !directionChanged)
                return;

            _executionModel =
                BuildExecutionModel(
                    closedM5,
                    direction);

            _lastExecutionModelBuildUtc =
                now;

            _lastExecutionModelM5 =
                closedM5;

            _lastExecutionModelMarket =
                market;

            _setupPreview =
                BuildTradeSetupPreview(
                    closedM5,
                    _executionModel);

            RefreshParallelOpportunityCandidates(
                closedM5);
        }

        private void RefreshLiveDecisionActionability(
            int closedM5)
        {
            if (_decision == null)
                return;

            if (_decision.Direction == 0)
            {
                ResetLiveActionability(
                    "NO DIRECTION");
                return;
            }

            if (!_decision.EntryAllowed)
            {
                ResetLiveActionability(
                    string.IsNullOrWhiteSpace(
                        _decision.BlockReason)
                        ? "DECISION FILTER"
                        : _decision.BlockReason);
                return;
            }

            if (_executionModel == null ||
                _setupPreview == null)
            {
                ResetLiveActionability(
                    "EXECUTION MODEL UNAVAILABLE");
                return;
            }

            OpportunityLane lane =
                _decision.TopDownEligible &&
                string.Equals(
                    _decision.TopDownStage,
                    "ENTRY CALIBRATED",
                    StringComparison.OrdinalIgnoreCase)
                    ? OpportunityLane.Strategic
                    : _decision.TacticalOpportunityLane;

            TradeActionabilityResult result =
                EvaluateTradeActionability(
                    closedM5,
                    _decision.Direction,
                    lane,
                    _decision.Regime,
                    _executionModel,
                    _setupPreview);

            bool actionable =
                result.Actionable;

            string reason =
                result.Reason;

            // Once a pre-trade plan exists, actionability must also agree with
            // the plan's actual execution mode and direction. A pending stop/limit
            // plan is not a current market-entry action and must not produce an
            // ACTION BUY/SELL arrow or alert.
            if (actionable &&
                _plan != null &&
                !_plan.IsLivePosition)
            {
                if (_plan.Direction !=
                    _decision.Direction)
                {
                    actionable = false;
                    reason = "PLAN / DECISION DIRECTION MISMATCH";
                }
                else if (_plan.EntryMode !=
                         _executionModel.Mode)
                {
                    actionable = false;
                    reason = "PLAN / EXECUTION MODE MISMATCH";
                }
                else if (_plan.EntryMode !=
                             ExecutionMode.RetestMarket &&
                         _plan.EntryMode !=
                             ExecutionMode.BreakoutMarket)
                {
                    actionable = false;
                    reason = "PENDING ENTRY PLAN • WAITING";
                }
            }

            _decision.ActionableNow =
                actionable;
            _decision.EntryLocationQuality =
                result.LocationQuality;
            _decision.EntryTimingQuality =
                result.TimingQuality;
            _decision.EntryPositionQuality =
                result.PricePositionQuality;
            _decision.EntryDistanceAtr =
                result.EntryDistanceAtr;
            _decision.ActionableTp1RR =
                result.Tp1RR;
            _decision.DivergenceQuality =
                result.DivergenceQuality;
            _decision.DivergenceDirection =
                result.DivergenceDirection;
            _decision.DivergenceType =
                result.DivergenceType;
            _decision.ActionabilityReason =
                reason;
        }

        private void ResetLiveActionability(
            string reason)
        {
            _decision.ActionableNow = false;
            _decision.EntryLocationQuality = 0;
            _decision.EntryTimingQuality = 0;
            _decision.EntryPositionQuality = 0;
            _decision.EntryDistanceAtr = 0;
            _decision.ActionableTp1RR = 0;
            _decision.DivergenceQuality = 0;
            _decision.DivergenceDirection = 0;
            _decision.DivergenceType = "NONE";
            _decision.ActionabilityReason =
                string.IsNullOrWhiteSpace(reason)
                    ? "NOT ACTIONABLE"
                    : reason;
        }

        private void RenderCalculationState(
            int index,
            int closedM5)
        {
            // Keep chart actionability on the same current-quote state as execution.
            RefreshLiveDecisionActionability(
                closedM5);

            _renderSignalVisualSnapshot =
                BuildSignalVisualSnapshot(
                    closedM5);

            try
            {
                if (_renderSignalVisualSnapshot != null &&
                    _renderSignalVisualSnapshot.PlanActive &&
                    !_renderSignalVisualSnapshot.PendingOrder)
                {
                    RenderPlan(
                        _renderSignalVisualSnapshot);
                }
                else
                {
                    RenderSetupPreview(
                        _renderSignalVisualSnapshot);

                    if (_renderSignalVisualSnapshot == null ||
                        (!_renderSignalVisualSnapshot.PlanActive &&
                         !_renderSignalVisualSnapshot.PendingOrder &&
                         !_renderSignalVisualSnapshot.SetupPreviewActive))
                    {
                        RenderPredictionObjects(
                            _prediction,
                            closedM5);
                    }
                    else
                    {
                        RemovePredictionObjects();
                    }

                    RenderWatchAndReaction(
                        index,
                        closedM5,
                        _renderSignalVisualSnapshot);
                }

                RenderLatestAlertSignalMarker(
                    closedM5);

                RenderManagedPendingOrder(
                    _renderSignalVisualSnapshot);

                RenderParallelOpportunityCandidates(
                    closedM5);

                RenderPanel();
            }
            finally
            {
                _renderSignalVisualSnapshot = null;
            }
        }
    }
}
