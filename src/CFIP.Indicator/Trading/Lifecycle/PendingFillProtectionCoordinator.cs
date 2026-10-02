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

            RelativeTakeProfitProtections unusedProtections;
            StopLossBreakEven unusedBreakEven;

            if (!TryBuildServerSideTakeProfitLadder(
                    position.EntryPrice,
                    finalTarget,
                    position.VolumeInUnits,
                    out unusedProtections,
                    out unusedBreakEven))
                return false;

            double firstVolume =
                Symbol.NormalizeVolumeInUnits(
                    position.VolumeInUnits *
                    PartialCloseTp1Percent /
                    100.0,
                    RoundingMode.Down);

            double secondVolume =
                Symbol.NormalizeVolumeInUnits(
                    position.VolumeInUnits *
                    PartialCloseTp2Percent /
                    100.0,
                    RoundingMode.Down);

            double firstPips =
                Math.Abs(
                    _plan.Tp1 -
                    position.EntryPrice) /
                Math.Max(Symbol.PipSize, 1e-9);

            double secondPips =
                Math.Abs(
                    _plan.Tp2 -
                    position.EntryPrice) /
                Math.Max(Symbol.PipSize, 1e-9);

            double finalPips =
                Math.Abs(
                    finalTarget -
                    position.EntryPrice) /
                Math.Max(Symbol.PipSize, 1e-9);

            if (!TryModifyTakeProfitLadder(
                    position,
                    firstVolume,
                    firstPips,
                    secondVolume,
                    secondPips,
                    finalPips,
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
