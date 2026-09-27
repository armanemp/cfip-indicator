// ============================================================================
// CFIP Indicator — BrokerLifecycleEvents.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void OnPositionOpened(
                            PositionOpenedEventArgs args)
                        {
                            if (args == null ||
                                args.Position == null ||
                                !IsManagedPosition(args.Position))
                                return;
                
                            Position position =
                                args.Position;
                
                            int direction =
                                position.TradeType == TradeType.Buy
                                    ? 1
                                    : -1;
                
                            if (_plan != null &&
                                position.SymbolName == SymbolName &&
                                _plan.Direction == direction)
                            {
                                // The broker event is the authoritative fill boundary. If
                                // execution code has not associated the position yet, bind it
                                // here using the managed symbol/side and the actual fill.
                                _plan.PositionId =
                                    position.Id;
                
                                _plan.Entry =
                                    NormalizePrice(
                                        position.EntryPrice);
                
                                _plan.IsLivePosition = true;
                
                                _activeBrokerStop =
                                    position.StopLoss.HasValue
                                        ? NormalizePrice(
                                            position.StopLoss.Value)
                                        : 0;
                
                                _activeBrokerTarget =
                                    position.TakeProfit.HasValue
                                        ? NormalizePrice(
                                            position.TakeProfit.Value)
                                        : 0;
                
                                _brokerProtectionRecoveryRequired =
                                    !position.StopLoss.HasValue ||
                                    !position.TakeProfit.HasValue;
                
                                SetLifecycleState(
                                    _brokerProtectionRecoveryRequired
                                        ? LifecycleState.RecoveryRequired
                                        : LifecycleState.LivePosition,
                                    _brokerProtectionRecoveryRequired
                                        ? "POSITION OPENED • PROTECTION MISSING"
                                        : "POSITION OPENED • LIVE");
                            }
                
                            SendUnifiedAlert(
                                "POSITION-OPEN|" +
                                position.Id,
                                "CFIP POSITION OPENED | #" +
                                position.Id,
                                direction,
                                true);
                        }
        
        private void OnPositionModified(
                            PositionModifiedEventArgs args)
                        {
                            if (args == null ||
                                args.Position == null ||
                                !IsManagedPosition(args.Position))
                                return;
                
                            Position position =
                                args.Position;
                
                            _activeBrokerStop =
                                position.StopLoss.HasValue
                                    ? NormalizePrice(position.StopLoss.Value)
                                    : 0;
                
                            _activeBrokerTarget =
                                position.TakeProfit.HasValue
                                    ? NormalizePrice(position.TakeProfit.Value)
                                    : 0;
                
                            if (_plan != null &&
                                _plan.IsLivePosition &&
                                _plan.PositionId == position.Id &&
                                _lifecycleState !=
                                    LifecycleState.ExitRequested)
                            {
                                _brokerProtectionRecoveryRequired =
                                    !position.StopLoss.HasValue ||
                                    !position.TakeProfit.HasValue;
                
                                SetLifecycleState(
                                    _brokerProtectionRecoveryRequired
                                        ? LifecycleState.RecoveryRequired
                                        : LifecycleState.LivePosition,
                                    _brokerProtectionRecoveryRequired
                                        ? "BROKER POSITION MODIFIED • PROTECTION MISSING"
                                        : "BROKER POSITION MODIFIED");
                            }
                        }
        
        private void OnPendingOrderCreated(
                            PendingOrderCreatedEventArgs args)
                        {
                            if (args == null ||
                                args.PendingOrder == null ||
                                !IsManagedPendingOrder(args.PendingOrder))
                                return;
                
                            SetLifecycleState(
                                LifecycleState.PendingOrder,
                                "PENDING ORDER CREATED #" +
                                args.PendingOrder.Id);
                        }
        
        private void OnPendingOrderModified(
                            PendingOrderModifiedEventArgs args)
                        {
                            if (args == null ||
                                args.PendingOrder == null ||
                                !IsManagedPendingOrder(args.PendingOrder))
                                return;
                
                            SetLifecycleState(
                                LifecycleState.PendingOrder,
                                "PENDING ORDER MODIFIED #" +
                                args.PendingOrder.Id);
                        }
        
        private void OnPendingOrderCancelled(
                            PendingOrderCancelledEventArgs args)
                        {
                            if (args == null ||
                                args.PendingOrder == null ||
                                !IsManagedPendingOrder(args.PendingOrder))
                                return;
                
                            PendingOrder remaining =
                                GetManagedPendingOrder();
                
                            if (remaining == null &&
                                GetManagedPosition() == null &&
                                _plan == null)
                            {
                                SetLifecycleState(
                                    LifecycleState.Closed,
                                    "PENDING ORDER CANCELLED #" +
                                    args.PendingOrder.Id);
                            }
                            else if (remaining == null &&
                                     GetManagedPosition() != null)
                            {
                                SetLifecycleState(
                                    LifecycleState.LivePosition,
                                    "PENDING ORDER CANCELLED • LIVE POSITION EXISTS");
                            }
                        }
        
        private void OnPositionClosed(PositionClosedEventArgs args)
                        {
                            if (args == null ||
                                !IsManagedPosition(args.Position))
                                return;
                
                            _lastExitM5 =
                                Math.Max(
                                    _lastExitM5,
                                    _lastEvaluatedM5);
                
                            int direction =
                                args.Position.TradeType == TradeType.Buy
                                    ? 1
                                    : -1;
                
                            if (!_outcomeRegistered)
                            {
                                bool profitable =
                                    args.Position.NetProfit > 0;
                
                                if (EnableOutcomeTelemetry)
                                {
                                    RegisterOutcome(
                                        direction,
                                        profitable);
                                }
                
                                _outcomeRegistered = true;
                
                                if (profitable)
                                    _wins++;
                                else
                                    _losses++;
                            }
                
                            _brokerProtectionRecoveryRequired = false;
                
                            if (_plan != null &&
                                _plan.IsLivePosition &&
                                _plan.PositionId ==
                                args.Position.Id)
                            {
                                _plan = null;
                                _activeBrokerStop = 0;
                                _activeBrokerTarget = 0;
                                _executionModel = null;
                
                                SetLifecycleState(
                                    LifecycleState.Closed,
                                    args.Position.NetProfit > 0
                                        ? "POSITION CLOSED • PROFIT"
                                        : "POSITION CLOSED • LOSS");
                
                                RemovePlanObjects();
                            }
                        }
        
        private void OnPendingOrderFilled(PendingOrderFilledEventArgs args)
                        {
                            if (args == null ||
                                args.Position == null ||
                                !IsManagedPosition(args.Position))
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
                
                            double entry =
                                args.Position.EntryPrice;
                
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
                                            Symbol.Bid) *
                                        10);
                            }
                
                            double stop =
                                args.Position.StopLoss.HasValue &&
                                IsValidStop(
                                    direction,
                                    entry,
                                    args.Position.StopLoss.Value)
                                    ? NormalizePrice(
                                        args.Position.StopLoss.Value)
                                    : 0;
                
                            double target =
                                args.Position.TakeProfit.HasValue &&
                                IsValidTarget(
                                    direction,
                                    entry,
                                    args.Position.TakeProfit.Value)
                                    ? NormalizePrice(
                                        args.Position.TakeProfit.Value)
                                    : 0;
                
                            bool protectionMissing =
                                !IsFinitePositive(stop) ||
                                !IsFinitePositive(target);
                
                            if (!IsFinitePositive(stop))
                            {
                                string stopSource;
                                int stopQuality;
                
                                stop =
                                    BuildStructuralStop(
                                        closedM5,
                                        direction,
                                        entry,
                                        atr,
                                        out stopSource,
                                        out stopQuality);
                
                                if (!IsValidStop(
                                        direction,
                                        entry,
                                        stop))
                                {
                                    double fallbackRisk =
                                        atr *
                                        Math.Max(
                                            0.10,
                                            FallbackSlAtr);
                
                                    stop =
                                        direction == 1
                                            ? entry - fallbackRisk
                                            : entry + fallbackRisk;
                
                                    stop =
                                        NormalizePrice(stop);
                                }
                            }
                
                            if (!IsFinitePositive(target))
                            {
                                target =
                                    SelectStructuralAutoTarget(
                                        closedM5,
                                        direction,
                                        entry,
                                        stop,
                                        atr,
                                        EffectiveAutoTpStage());
                
                                if (!IsValidTarget(
                                        direction,
                                        entry,
                                        target))
                                {
                                    double risk =
                                        Math.Max(
                                            Symbol.PipSize,
                                            Math.Abs(
                                                entry -
                                                stop));
                
                                    double fallbackDistance =
                                        risk *
                                        Math.Max(
                                            1.0,
                                            MinimumRequiredRR());
                
                                    target =
                                        direction == 1
                                            ? entry + fallbackDistance
                                            : entry - fallbackDistance;
                
                                    target =
                                        NormalizePrice(target);
                                }
                            }
                
                            _plan =
                                CreateManagedPlanFromExecution(
                                    direction,
                                    entry,
                                    stop,
                                    target,
                                    closedM5,
                                    args.Position.VolumeInUnits,
                                    args.PendingOrder.OrderType ==
                                        PendingOrderType.Stop
                                        ? ExecutionMode.ContinuationStop
                                        : ExecutionMode.ReversalLimit);
                
                            _plan.PositionId =
                                args.Position.Id;
                
                            _activeBrokerStop =
                                args.Position.StopLoss.HasValue
                                    ? NormalizePrice(
                                        args.Position.StopLoss.Value)
                                    : 0;
                
                            _activeBrokerTarget =
                                args.Position.TakeProfit.HasValue
                                    ? NormalizePrice(
                                        args.Position.TakeProfit.Value)
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
                
                            EnrichLivePlanTargets(closedM5);
                
                            if (AutoBrokerProtection)
                            {
                                bool protectedOk =
                                    EnsureBrokerProtectionForPosition(
                                        args.Position,
                                        stop,
                                        target,
                                        "PENDING FILL",
                                        direction);
                
                                if (protectedOk)
                                {
                                    _activeBrokerStop = stop;
                                    _activeBrokerTarget = target;
                                    _brokerProtectionRecoveryRequired = false;
                
                                    SetLifecycleState(
                                        LifecycleState.LivePosition,
                                        "PENDING FILL • PROTECTED");
                                }
                            }
                
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
