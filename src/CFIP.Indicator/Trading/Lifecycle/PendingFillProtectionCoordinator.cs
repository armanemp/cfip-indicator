// CFIP Indicator — PendingFillProtectionCoordinator.cs
// Single-responsibility broker-protection coordination after a confirmed pending fill.

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ReconcilePendingFillServerProtection(
            Position position,
            double finalTarget)
        {
            if (position == null)
                return false;

            if (!_serverSideTakeProfitLadderActive &&
                !_serverSideTakeProfitLadderOwned)
                return true;

            if (!IsFinitePositive(finalTarget))
                return false;

            RelativeTakeProfitProtections protections;
            StopLossBreakEven stopLossBreakEven;

            if (!TryBuildServerSideTakeProfitLadder(
                    position.EntryPrice,
                    finalTarget,
                    position.VolumeInUnits,
                    out protections,
                    out stopLossBreakEven))
                return false;

            if (!TryModifyTakeProfitLadder(
                    position,
                    protections,
                    "PENDING FILL • ABSOLUTE TP LADDER"))
                return false;

            return AdoptServerSideTakeProfitLadder(position);
        }

        private void ApplyPendingFillProtection(
            Position position,
            double stop,
            double target,
            int direction,
            bool protectionMissing)
        {
            _activeBrokerStop =
                position.StopLoss.HasValue
                    ? NormalizePrice(position.StopLoss.Value)
                    : 0;

            _activeBrokerTarget =
                position.TakeProfit.HasValue
                    ? NormalizePrice(position.TakeProfit.Value)
                    : 0;

            _brokerProtectionRecoveryRequired =
                protectionMissing;

            SetLifecycleState(
                protectionMissing
                    ? LifecycleState.RecoveryRequired
                    : LifecycleState.LivePosition,
                protectionMissing
                    ? "PENDING FILL • BROKER PROTECTION MISSING"
                    : "PENDING FILL • LIVE");

            if (!AutoBrokerProtection)
                return;

            bool protectedOk =
                EnsureBrokerProtectionForPosition(
                    position,
                    stop,
                    target,
                    "PENDING FILL",
                    direction);

            if (!protectedOk)
                return;

            _activeBrokerStop = stop;
            _activeBrokerTarget = target;
            _brokerProtectionRecoveryRequired = false;

            SetLifecycleState(
                LifecycleState.LivePosition,
                "PENDING FILL • PROTECTED");
        }
    }
}
