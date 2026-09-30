// CFIP Indicator — PendingFilledHandler.cs
// Single-responsibility broker-confirmed pending-fill event handler.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void OnPendingOrderFilled(
            PendingOrderFilledEventArgs args)
        {

            if (args == null ||
                args.Position == null ||
                !IsManagedPosition(args.Position))
                return;

                                    MarkBrokerStateDirty();


            if (args.PendingOrder == null ||
                !_lifecycleEventGuard.TryBegin(
                    "PENDING_FILLED",
                    args.PendingOrder.Id))
                return;

            RemoveManagedPendingOrderObjects();

            int direction =
                args.Position.TradeType == TradeType.Buy
                    ? 1
                    : -1;

            int closedM5 =
                Math.Max(
                    1,
                    _lastEvaluatedM5);

            double atr =
                _m5Bars == null
                    ? 0
                    : Atr(
                        _m5Bars,
                        closedM5);

            if (!IsFinitePositive(atr))
            {
                atr =
                    Math.Max(
                        Symbol.PipSize * 20,
                        Math.Abs(
                            Symbol.Ask -
                            Symbol.Bid) * 10);
            }

            Plan priorPlan =
                _plan;

            double stop;
            double target;
            bool protectionMissing;

            _plan =
                BuildPendingFillPlan(
                    args,
                    closedM5,
                    direction,
                    atr,
                    out stop,
                    out target,
                    out protectionMissing);

            if (_plan == null)
            {
                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "PENDING FILL • PLAN BUILD FAILED");
                return;
            }

            _plan.PositionId =
                args.Position.Id;

            AdoptServerSideTakeProfitLadder(
                args.Position);

            bool brokerStopMissing =
                !args.Position.StopLoss.HasValue ||
                !IsFinitePositive(
                    args.Position.StopLoss.Value) ||
                !IsValidManagedStop(
                    direction,
                    args.Position.EntryPrice,
                    Symbol.Bid > 0 && direction == 1
                        ? Symbol.Bid
                        : Symbol.Ask,
                    args.Position.StopLoss.Value);

            bool brokerTargetMissing =
                !_serverSideTakeProfitLadderActive &&
                (!args.Position.TakeProfit.HasValue ||
                 !IsFinitePositive(
                    args.Position.TakeProfit.Value) ||
                 !IsValidTarget(
                    direction,
                    args.Position.EntryPrice,
                    args.Position.TakeProfit.Value));

            protectionMissing =
                brokerStopMissing ||
                brokerTargetMissing;

            if (priorPlan != null &&
                priorPlan.CalibrationEligible)
            {
                CopyPlanCalibrationContext(
                    priorPlan,
                    _plan);
            }

            _plan.IsLivePosition = true;

            ApplyPendingFillProtection(
                args.Position,
                stop,
                target,
                direction,
                protectionMissing);

            EnrichLivePlanTargets(closedM5);

            SendUnifiedAlert(
                "PENDING-FILLED|" +
                args.PendingOrder.Id,
                "CFIP PENDING FILLED | #" +
                args.Position.Id +
                (_brokerProtectionRecoveryRequired
                    ? " | PROTECTION RECOVERY"
                    : ""),
                direction,
                true);
        }
    }
}
