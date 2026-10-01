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

            // A new position lifecycle starts here even when PositionOpened
            // arrives before or after this pending-fill event.
            _outcomeRegistered = false;

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
                _pendingOrderPlanSnapshot != null
                    ? _pendingOrderPlanSnapshot
                    : _plan;

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

            if (priorPlan != null)
            {
                _plan.Lane = priorPlan.Lane;
                _plan.EntryMode = priorPlan.EntryMode;
                _plan.IdealEntry = priorPlan.IdealEntry;
                _plan.EntryZoneLow = priorPlan.EntryZoneLow;
                _plan.EntryZoneHigh = priorPlan.EntryZoneHigh;
                _plan.EntryZoneTolerance = priorPlan.EntryZoneTolerance;
                _plan.EntryTrigger = priorPlan.EntryTrigger;
                _plan.EntryInvalidation = priorPlan.EntryInvalidation;
                _plan.EntryQuality = priorPlan.EntryQuality;
                _plan.EntrySource = priorPlan.EntrySource;
                _plan.CreatedM5 = priorPlan.CreatedM5;
                _plan.SignalBarOpenTimeUtcTicks =
                    priorPlan.SignalBarOpenTimeUtcTicks;
                _plan.SignalTraceId =
                    priorPlan.SignalTraceId;
            }

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

            bool fillReconciled =
                ReconcileLivePlanToActualFill(
                    args.Position,
                    closedM5,
                    priorPlan);

            if (!fillReconciled)
            {
                _brokerProtectionRecoveryRequired = true;
                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "PENDING FILL • ABSOLUTE EXIT RECONCILIATION FAILED");

                _autoExecutionBlockReason =
                    "PENDING FILL • ABSOLUTE EXIT RECONCILIATION FAILED";

                SendUnifiedAlert(
                    "PENDING-FILL-RECONCILIATION-FAILED|" +
                    args.Position.Id,
                    "CFIP PENDING FILL • EXIT RECONCILIATION FAILED | #" +
                    args.Position.Id,
                    direction,
                    true);

                return;
            }

            EnrichLivePlanTargets(closedM5);

            stop = _plan.Stop;
            target =
                AutoTarget(
                    _plan,
                    EffectiveAutoTpStage());

            if (_serverSideTakeProfitLadderActive &&
                !ReconcilePendingFillServerProtection(
                    args.Position,
                    target))
            {
                _brokerProtectionRecoveryRequired = true;
                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "PENDING FILL • ABSOLUTE TP LADDER RECONCILIATION FAILED");

                _autoExecutionBlockReason =
                    "PENDING FILL • ABSOLUTE TP LADDER RECONCILIATION FAILED";

                SendUnifiedAlert(
                    "PENDING-FILL-LADDER-RECONCILIATION-FAILED|" +
                    args.Position.Id,
                    "CFIP PENDING FILL • TP LADDER RECONCILIATION FAILED | #" +
                    args.Position.Id,
                    direction,
                    true);

                return;
            }

            protectionMissing =
                !args.Position.StopLoss.HasValue ||
                !IsFinitePositive(
                    args.Position.StopLoss.Value) ||
                !IsValidManagedStop(
                    direction,
                    args.Position.EntryPrice,
                    Symbol.Bid > 0 && direction == 1
                        ? Symbol.Bid
                        : Symbol.Ask,
                    args.Position.StopLoss.Value) ||
                (!_serverSideTakeProfitLadderActive &&
                 (!args.Position.TakeProfit.HasValue ||
                  !IsFinitePositive(
                      args.Position.TakeProfit.Value) ||
                  !IsValidTarget(
                      direction,
                      args.Position.EntryPrice,
                      args.Position.TakeProfit.Value)));

            ApplyPendingFillProtection(
                args.Position,
                stop,
                target,
                direction,
                protectionMissing);

            if (!_brokerProtectionRecoveryRequired)
                ClearPendingOrderPlanSnapshot();

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
