using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ProcessLiveCalculation(
            int index,
            int closedM5)
        {
            if (_m5Bars != null)
            {
                int liveM5 =
                    _m5Bars.Count - 1;

                DateTime now =
                    TimeInUtc;

                double market =
                    Symbol.Ask > Symbol.Bid
                        ? (Symbol.Ask + Symbol.Bid) * 0.5
                        : Symbol.Ask;

                double liveAtr =
                    _m5Bars.Count > 2
                        ? Atr(
                            _m5Bars,
                            Math.Max(
                                1,
                                liveM5 - 1))
                        : 0;

                bool newM5 =
                    liveM5 != _lastLiveReactionM5;

                bool priceMoved =
                    _lastLiveReactionMarket <= 0 ||
                    Math.Abs(
                        market -
                        _lastLiveReactionMarket) >=
                    Math.Max(
                        Symbol.TickSize * 2,
                        liveAtr * 0.01);

                bool intervalElapsed =
                    (now - _lastLiveReactionCalcUtc).TotalMilliseconds >= 750;

                if (_reaction == null ||
                    newM5 ||
                    (intervalElapsed && priceMoved))
                {
                    _reaction =
                        BuildReaction();

                    _lastLiveReactionCalcUtc =
                        now;

                    _lastLiveReactionM5 =
                        liveM5;

                    _lastLiveReactionMarket =
                        market;
                }
            }

            SynchronizePreTradePlanWithDecision();

            RecoverManagedLivePlan(
                closedM5);

            if (!AutoTradingEnabled &&
                AutoTradingReminder)
                CheckAutoTradingDisabledReminder(
                    closedM5);

            SyncQuickExecutionControls();

            TryEnsureAutomaticPlan(
                closedM5);

            UpdateExecutionModel(
                closedM5);

            SynchronizeLiveBrokerState();

            EvaluateActivePlan(
                closedM5);

            TryAutoTrade(
                closedM5);

            TryAggressiveAutoTrade(
                closedM5);

            TrySmartPendingOrders(
                closedM5);

            ProtectBrokerPositions(
                closedM5);

            MonitorOutcome(
                closedM5);

            CheckEndOfDayAlert(
                TimeInUtc);

            CheckReversalProtection();

            SynchronizeLiveBrokerState();

            RenderCalculationState(
                index,
                closedM5);
        }

        private void TryEnsureAutomaticPlan(
            int closedM5)
        {
            if (!AutoTradingEnabled ||
                _plan != null ||
                _decision == null ||
                _decision.Direction == 0 ||
                _lastAutoPlanAttemptM5 ==
                closedM5)
                return;

            _lastAutoPlanAttemptM5 =
                closedM5;

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
                _lastExecutionModelM5 = -1;
                _lastExecutionModelMarket = 0;
                return;
            }

            int direction =
                _decision.Direction;

            double market =
                direction == 1
                    ? Symbol.Ask
                    : Symbol.Bid;

            DateTime now =
                TimeInUtc;

            bool m5Changed =
                _lastExecutionModelM5 != closedM5;

            bool directionChanged =
                _executionModel == null ||
                _executionModel.Direction != direction;

            bool marketChangedEnough =
                _lastExecutionModelMarket <= 0 ||
                Math.Abs(
                    market -
                    _lastExecutionModelMarket) >=
                Math.Max(
                    Symbol.TickSize * 3,
                    Atr(
                        _m5Bars,
                        closedM5) * 0.02);

            bool intervalElapsed =
                (now - _lastExecutionModelBuildUtc).TotalMilliseconds >= 400;

            if (!m5Changed &&
                !directionChanged &&
                !(intervalElapsed && marketChangedEnough))
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
        }

        private void RenderCalculationState(
            int index,
            int closedM5)
        {
            SynchronizeSignalVisualState();

            if (_plan != null)
                RenderPlan();
            else
                RenderWatchAndReaction(
                    index,
                    closedM5);

            RenderManagedPendingOrder();
            RenderPanel();
        }
    }
}
