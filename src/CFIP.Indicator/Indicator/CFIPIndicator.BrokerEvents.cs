using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CFIP.Indicator;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
                        private void PendingOrders_Created(
                    PendingOrderCreatedEventArgs args)
                {
                    if (args == null ||
                        args.PendingOrder == null)
                        return;
        
                    ConfirmBrokerObject(
                        "ORDER",
                        args.PendingOrder.Id.ToString());
        
                    if (_pendingOrderLifecycle != null)
                        _pendingOrderLifecycle.RegisterExisting(
                            args.PendingOrder,
                            TimeInUtc);
                }
        
                private void PendingOrders_Modified(
                    PendingOrderModifiedEventArgs args)
                {
                    if (_pendingOrderLifecycle != null &&
                        args != null &&
                        args.PendingOrder != null)
                        _pendingOrderLifecycle.HandleModified(
                            args.PendingOrder);
                }
        
                private void PendingOrders_Filled(
                    PendingOrderFilledEventArgs args)
                {
                    if (args == null ||
                        args.PendingOrder == null ||
                        args.Position == null)
                        return;
        
                    ConfirmBrokerObject(
                        "ORDER",
                        args.PendingOrder.Id.ToString());
                    ConfirmBrokerObject(
                        "POSITION",
                        args.Position.Id.ToString());
        
                    if (_pendingOrderLifecycle != null)
                        _pendingOrderLifecycle.HandleFilled(
                            args.PendingOrder,
                            args.Position,
                            TimeInUtc);
        
                    if (_positionLifecycle != null)
                        _positionLifecycle.RegisterOpened(
                            args.Position,
                            TimeInUtc,
                            args.PendingOrder.StopLoss,
                            args.PendingOrder.TakeProfit);
        
                    if (_livePositionManager != null)
                        _livePositionManager.Register(
                            args.Position,
                            _state != null ? _state.Plan : null);
        
                    _lifecycle.TryTransition(
                        LifecycleState.LivePosition,
                        TimeInUtc,
                        "PENDING_FILLED_TO_POSITION");
                }
        
                private void PendingOrders_Cancelled(
                    PendingOrderCancelledEventArgs args)
                {
                    if (args == null ||
                        args.PendingOrder == null)
                        return;
        
                    ConfirmBrokerObject(
                        "ORDER",
                        args.PendingOrder.Id.ToString());
        
                    if (_pendingOrderLifecycle != null)
                        _pendingOrderLifecycle.HandleCancelled(
                            args.PendingOrder,
                            TimeInUtc);
        
                    if (_state != null &&
                        _state.Runtime != null)
                        ReconcileBrokerState();
                }
        
                private void Positions_Opened(
                    PositionOpenedEventArgs args)
                {
                    if (args == null ||
                        args.Position == null)
                        return;
        
                    ConfirmBrokerObject(
                        "POSITION",
                        args.Position.Id.ToString());
        
                    if (_positionLifecycle != null)
                        _positionLifecycle.RegisterOpened(
                            args.Position,
                            TimeInUtc,
                            args.Position.StopLoss,
                            args.Position.TakeProfit);
        
                    if (_livePositionManager != null)
                        _livePositionManager.Register(
                            args.Position,
                            _state != null ? _state.Plan : null);
                }
        
                private void Positions_Modified(
                    PositionModifiedEventArgs args)
                {
                    if (_positionLifecycle != null &&
                        args != null &&
                        args.Position != null)
                        _positionLifecycle.HandleModified(
                            args.Position,
                            TimeInUtc);
                }
        
                private void Positions_Closed(
                    PositionClosedEventArgs args)
                {
                    if (args == null ||
                        args.Position == null)
                        return;
        
                    ConfirmBrokerObject(
                        "POSITION",
                        args.Position.Id.ToString());
        
                    if (_positionLifecycle != null)
                        _positionLifecycle.HandleClosed(
                            args.Position,
                            TimeInUtc);
        
                    if (_livePositionManager != null)
                        _livePositionManager.HandleClosed(
                            args.Position);
        
                    if (_state != null &&
                        _state.Runtime != null)
                        ReconcileBrokerState();
                }
        
        
    }
}
