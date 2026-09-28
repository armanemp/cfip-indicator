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
            _reaction =
                BuildReaction();

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
            if (_decision != null &&
                _decision.Direction != 0 &&
                _plan == null)
            {
                _executionModel =
                    BuildExecutionModel(
                        closedM5,
                        _decision.Direction);
            }
            else if (_plan == null)
            {
                _executionModel =
                    null;
            }
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
