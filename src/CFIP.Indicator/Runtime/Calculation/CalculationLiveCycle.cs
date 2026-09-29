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
                _setupPreview = null;
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
        }

        private void RenderCalculationState(
            int index,
            int closedM5)
        {
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

                    RenderPredictionObjects(
                        _prediction,
                        closedM5);

                    RenderWatchAndReaction(
                        index,
                        closedM5,
                        _renderSignalVisualSnapshot);
                }

                RenderLatestAlertSignalMarker(
                    closedM5);

                RenderManagedPendingOrder(
                    _renderSignalVisualSnapshot);

                RenderPanel();
            }
            finally
            {
                _renderSignalVisualSnapshot = null;
            }
        }
    }
}
