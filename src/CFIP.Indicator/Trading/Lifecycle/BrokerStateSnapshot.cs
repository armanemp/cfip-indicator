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
        private void SynchronizeLiveBrokerState()
        {
            _activeBrokerStop = 0;
            _activeBrokerTarget = 0;

            if (_plan == null ||
                !_plan.IsLivePosition)
                return;

            Position position =
                GetManagedLivePositionForPlan();

            if (LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    true,
                    position != null) &&
                _plan != null)
            {
                // The broker position is authoritative. If the live plan's bound
                // position no longer exists, clear the stale plan even when the
                // PositionClosed event was missed during reconnect/restart.
                _brokerProtectionRecoveryRequired = false;
                _executionModel = null;
                _activeBrokerStop = 0;
                _activeBrokerTarget = 0;

                SetLifecycleState(
                    LifecycleState.Closed,
                    "BROKER STATE • BOUND POSITION NOT FOUND");

                _plan = null;
                RemovePlanObjects();
                return;
            }

            if (position == null)
                return;

            int direction =
                position.TradeType == TradeType.Buy
                    ? 1
                    : -1;

            double market =
                direction == 1
                    ? Symbol.Bid
                    : Symbol.Ask;

            if (IsFinitePositive(position.EntryPrice))
                _plan.Entry =
                    NormalizePrice(position.EntryPrice);

            bool brokerStopValid =
                position.StopLoss.HasValue &&
                IsFinitePositive(position.StopLoss.Value) &&
                IsValidManagedStop(
                    direction,
                    position.EntryPrice,
                    market,
                    position.StopLoss.Value);

            bool brokerTargetValid =
                position.TakeProfit.HasValue &&
                IsFinitePositive(position.TakeProfit.Value) &&
                IsValidTarget(
                    direction,
                    position.EntryPrice,
                    position.TakeProfit.Value);

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
                !brokerStopValid ||
                (SyncBrokerTakeProfit &&
                 !brokerTargetValid);

            if (protectionRequired &&
                protectionMissing &&
                _lifecycleState != LifecycleState.ExitRequested)
            {
                _brokerProtectionRecoveryRequired = true;

                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "BROKER STATE • PROTECTION INVALID");
            }
        }
    }
}