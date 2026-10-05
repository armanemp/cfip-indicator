// CFIP Indicator — BrokerStateSnapshot.cs
// Single-responsibility lifecycle module.

using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int BrokerStateRefreshIntervalMilliseconds = 1000;

        private bool _brokerStateDirty = true;
        private DateTime _lastBrokerStateSyncUtc =
            DateTime.MinValue;

        private void MarkBrokerStateDirty()
        {
            _brokerStateDirty = true;
            InvalidatePanelExecutionProtectionStateCache();
        }

        private void SynchronizeLiveBrokerState()
        {
            DateTime now =
                Server.TimeInUtc;

            if (!BrokerStateRefreshRule.IsRefreshDue(
                    _brokerStateDirty,
                    _lastBrokerStateSyncUtc,
                    now,
                    BrokerStateRefreshIntervalMilliseconds))
            {
                return;
            }

            // Bound panel freshness even if the terminal does not surface a
            // lifecycle event: a due broker refresh invalidates the snapshot
            // before authoritative broker facts are read again.
            InvalidatePanelExecutionProtectionStateCache();

            double previousBrokerStop =
                _activeBrokerStop;

            double previousBrokerTarget =
                _activeBrokerTarget;

            _activeBrokerStop = 0;
            _activeBrokerTarget = 0;

            if (_plan == null ||
                !_plan.IsLivePosition)
            {
                _brokerStateDirty = false;
                _lastBrokerStateSyncUtc = now;
                return;
            }

            Position position =
                GetManagedLivePositionForPlan();

            if (LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    true,
                    position != null) &&
                _plan != null)
            {
                _brokerProtectionRecoveryRequired = false;
                _executionModel = null;
                _activeBrokerStop = 0;
                _activeBrokerTarget = 0;

                SetLifecycleState(
                    LifecycleState.Closed,
                    "BROKER STATE • BOUND POSITION NOT FOUND");

                _plan = null;
                RemovePlanObjects();

                _brokerStateDirty = false;
                _lastBrokerStateSyncUtc = now;
                return;
            }

            if (position == null)
            {
                _brokerStateDirty = false;
                _lastBrokerStateSyncUtc = now;
                return;
            }

            bool protectionHealthy =
                EvaluateBrokerProtection(
                    position,
                    out bool brokerStopValid,
                    out bool brokerTargetValid);

            int direction =
                position.TradeType == TradeType.Buy
                    ? 1
                    : -1;

            bool protectionStateChanged =
                ApplyBrokerConfirmedProtectionState(
                    position.Id,
                    position.EntryPrice,
                    brokerStopValid
                        ? position.StopLoss
                        : (double?)null,
                    brokerTargetValid
                        ? position.TakeProfit
                        : (double?)null,
                    true);

            if (protectionStateChanged &&
                !double.IsNaN(previousBrokerStop) &&
                !double.IsInfinity(previousBrokerStop) &&
                IsFinitePositive(previousBrokerStop) &&
                IsFinitePositive(_activeBrokerStop) &&
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    direction,
                    previousBrokerStop,
                    _activeBrokerStop) &&
                !SamePrice(previousBrokerStop, _activeBrokerStop))
            {
                MarkBrokerProtectionRegression(
                    "BROKER STOP REGRESSED FROM LAST CONFIRMED STATE");
            }

            if (IsFinitePositive(previousBrokerTarget) &&
                IsFinitePositive(_activeBrokerTarget) &&
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    direction,
                    previousBrokerTarget,
                    _activeBrokerTarget,
                    true))
            {
                MarkBrokerProtectionRegression(
                    "BROKER TARGET REGRESSED FROM LAST CONFIRMED STATE");
            }

            bool protectionRequired =
                CbotCanManage();

            bool protectionMissing =
                !protectionHealthy;

            if (protectionRequired &&
                protectionMissing &&
                _lifecycleState != LifecycleState.ExitRequested)
            {
                _brokerProtectionRecoveryRequired = true;

                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "BROKER STATE • PROTECTION INVALID");
            }

            _brokerStateDirty = false;
            _lastBrokerStateSyncUtc = now;
        }
    }
}
