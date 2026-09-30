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

            if (IsFinitePositive(position.EntryPrice))
                _plan.Entry =
                    NormalizePrice(position.EntryPrice);

            if (brokerStopValid)
            {
                _activeBrokerStop =
                    NormalizePrice(position.StopLoss.Value);
            }

            if (brokerTargetValid)
            {
                _activeBrokerTarget =
                    NormalizePrice(position.TakeProfit.Value);
            }

            bool protectionRequired =
                AutoBrokerProtection ||
                AutoProtectBrokerPositions;

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
