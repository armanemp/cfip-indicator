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
                        private void ProcessLivePositionManagement()
                {
                    if (_livePositionManager == null ||
                        _positionLifecycle == null ||
                        _state == null ||
                        _state.Runtime == null ||
                        _state.Broker == null ||
                        _configuration == null ||
                        !Server.IsConnected)
                        return;
        
                    IReadOnlyList<LivePositionAction> actions =
                        _livePositionManager.Evaluate(
                            _state.Broker.Positions,
                            _state.Runtime,
                            _state.Plan,
                            _state.Market,
                            _state.Structure,
                            _configuration);
        
                    for (int i = 0; i < actions.Count; i++)
                    {
                        LivePositionAction action =
                            actions[i];
        
                        if (action == null)
                            continue;
        
                        if (action.Kind ==
                            LivePositionActionKind.Close)
                        {
                            _positionLifecycle.RequestClose(
                                action.BrokerPositionId,
                                _state.Runtime.ServerUtc,
                                action.Reason);
                            continue;
                        }
        
                        if (action.Kind ==
                            LivePositionActionKind.ProtectionUpdate)
                        {
                            bool protectionQueued =
                                _positionLifecycle.RequestProtectionMutation(
                                    action.BrokerPositionId,
                                    action.StopLoss,
                                    action.TakeProfit,
                                    _state.Runtime.ServerUtc,
                                    action.Reason);
        
                            if (protectionQueued)
                                _livePositionManager.HandleProtectionRequested(
                                    action,
                                    _state.Runtime.ServerUtc);
        
                            continue;
                        }
        
                        if (action.Kind ==
                            LivePositionActionKind.PartialClose)
                        {
                            ExecutionResult result =
                                _brokerGateway.PartialClosePosition(
                                    action.BrokerPositionId,
                                    action.VolumeInUnits);
        
                            _livePositionManager.HandlePartialResult(
                                action,
                                result,
                                _state.Runtime.ServerUtc);
        
                            if (!result.Accepted)
                            {
                                _lifecycle.TryTransition(
                                    LifecycleState.RecoveryRequired,
                                    _state.Runtime.ServerUtc,
                                    "PARTIAL_CLOSE_FAILED_" +
                                    action.Reason);
                            }
                        }
                    }
                }
        
                private void ProcessPositionLifecycle()
                {
                    if (_positionLifecycle == null ||
                        _state == null ||
                        _state.Runtime == null ||
                        !Server.IsConnected)
                        return;
        
                    _positionLifecycle.ReconcileBrokerState(
                        _state.Broker != null
                            ? _state.Broker.Positions
                            : null,
                        _state.Runtime.ServerUtc);
        
                    // Pending -> Position protection handoff survives a restart or
                    // missed Filled event by transferring expected protection to
                    // every broker Position associated with the pending PlanId.
                    IReadOnlyList<PendingOrderRecord> pendingRecords =
                        _pendingOrderLifecycle != null
                            ? _pendingOrderLifecycle.Records
                            : null;
        
                    if (pendingRecords != null)
                    {
                        for (int i = 0; i < pendingRecords.Count; i++)
                        {
                            PendingOrderRecord pending =
                                pendingRecords[i];
        
                            if (pending == null ||
                                pending.State !=
                                PendingOrderLifecycleState.Filled)
                                continue;
        
                            IReadOnlyList<string> positionIds =
                                pending.BrokerPositionIds;
        
                            for (int j = 0;
                                 j < positionIds.Count;
                                 j++)
                            {
                                _positionLifecycle.RegisterExpectedProtection(
                                    positionIds[j],
                                    pending.ExpectedStopLoss,
                                    pending.ExpectedTakeProfit,
                                    _state.Runtime.ServerUtc);
                            }
                        }
                    }
        
                    _positionLifecycle.EvaluateRetryActions(
                        _state.Runtime.ServerUtc);
        
                    IReadOnlyList<PositionAction> actions =
                        _positionLifecycle.DrainActions();
        
                    for (int i = 0; i < actions.Count; i++)
                    {
                        PositionAction action = actions[i];
        
                        if (action.Kind !=
                                PositionActionKind.RestoreProtection &&
                            action.Kind !=
                                PositionActionKind.Close)
                            continue;
        
                        ExecutionResult result =
                            action.Kind ==
                                PositionActionKind.RestoreProtection
                                ? _brokerGateway.ModifyProtection(
                                    action.BrokerPositionId,
                                    action.StopLoss,
                                    action.TakeProfit)
                                : _brokerGateway.ClosePosition(
                                    action.BrokerPositionId);
        
                        _positionLifecycle.HandleActionResult(
                            action,
                            result,
                            _state.Runtime.ServerUtc);
        
                        if (!result.Accepted)
                        {
                            _lifecycle.TryTransition(
                                LifecycleState.RecoveryRequired,
                                _state.Runtime.ServerUtc,
                                action.Kind ==
                                    PositionActionKind.Close
                                    ? "POSITION_CLOSE_RETRY_" + action.Reason
                                    : "POSITION_PROTECTION_RECOVERY_FAILED_" +
                                      action.Reason);
                        }
                    }
                }
        
                private void ProcessPendingOrderLifecycle()
                {
                    if (_pendingOrderLifecycle == null ||
                        _state == null ||
                        _state.Runtime == null ||
                        !Server.IsConnected)
                        return;
        
                    _pendingOrderLifecycle.Evaluate(
                        _state.Runtime.ServerUtc,
                        IsDailyLossLimitBreached());
        
                    IReadOnlyList<PendingOrderAction> actions =
                        _pendingOrderLifecycle.DrainActions();
        
                    for (int i = 0; i < actions.Count; i++)
                    {
                        PendingOrderAction action =
                            actions[i];
        
                        ExecutionResult result;
        
                        if (action.Kind ==
                            PendingOrderActionKind.Cancel)
                        {
                            result =
                                _brokerGateway.CancelPendingOrder(
                                    action.BrokerOrderId);
                        }
                        else if (action.Kind ==
                                 PendingOrderActionKind.RestoreProtection)
                        {
                            result =
                                _brokerGateway.ModifyPendingProtection(
                                    action.BrokerOrderId,
                                    action.StopLoss,
                                    action.TakeProfit);
                        }
                        else
                        {
                            continue;
                        }
        
                        _pendingOrderLifecycle.HandleActionResult(
                            action,
                            result,
                            _state.Runtime.ServerUtc);
        
                        if (!result.Accepted)
                        {
                            _lifecycle.TryTransition(
                                LifecycleState.RecoveryRequired,
                                _state.Runtime.ServerUtc,
                                "PENDING_ACTION_FAILED_" +
                                action.Reason);
                        }
                    }
                }
        
                private void RegisterBrokerConfirmation(
                    string kind,
                    string brokerId,
                    DateTime utc)
                {
                    if (!string.IsNullOrWhiteSpace(brokerId))
                        _awaitingBrokerConfirmations[kind + ":" + brokerId] =
                            utc + BrokerConfirmationGrace;
                }
        
                private void ConfirmBrokerObject(
                    string kind,
                    string brokerId)
                {
                    if (!string.IsNullOrWhiteSpace(brokerId))
                        _awaitingBrokerConfirmations.Remove(
                            kind + ":" + brokerId);
                }
        
                private bool HasPendingBrokerConfirmation(DateTime utc)
                {
                    var expired = new List<string>();
                    bool pending = false;
        
                    foreach (var pair in _awaitingBrokerConfirmations)
                    {
                        if (utc >= pair.Value)
                            expired.Add(pair.Key);
                        else
                            pending = true;
                    }
        
                    for (int i = 0; i < expired.Count; i++)
                        _awaitingBrokerConfirmations.Remove(expired[i]);
        
                    if (expired.Count > 0 &&
                        _lifecycle != null)
                        _lifecycle.TryTransition(
                            LifecycleState.RecoveryRequired,
                            utc,
                            "BROKER_CONFIRMATION_TIMEOUT");
        
                    return pending;
                }
        
                private void ReconcileBrokerState()
                {
                    if (_brokerStateReader == null ||
                        _state == null ||
                        _state.Runtime == null ||
                        _configuration == null ||
                        !Server.IsConnected)
                        return;
        
                    bool awaitingConfirmation =
                        HasPendingBrokerConfirmation(
                            _state.Runtime.ServerUtc);
        
                    _state.Broker =
                        _brokerStateReader.ReadManagedState(
                            SymbolName,
                            _configuration.StrategyId);
        
                    foreach (var position in _state.Broker.Positions)
                        ConfirmBrokerObject(
                            "POSITION",
                            position.BrokerPositionId);
        
                    foreach (var order in _state.Broker.PendingOrders)
                        ConfirmBrokerObject(
                            "ORDER",
                            order.BrokerOrderId);
        
                    if (_pendingOrderLifecycle != null)
                        _pendingOrderLifecycle.ReconcileBrokerState(
                            _state.Broker.PendingOrders,
                            _state.Broker.Positions,
                            _state.Runtime.ServerUtc);
        
                    if (_state.Broker.Positions.Count > 0)
                    {
                        _lifecycle.TryTransition(
                            LifecycleState.LivePosition,
                            _state.Runtime.ServerUtc,
                            "BROKER_POSITION_PRESENT");
                    }
                    else if (_state.Broker.PendingOrders.Count > 0)
                    {
                        _lifecycle.TryTransition(
                            LifecycleState.PendingOrder,
                            _state.Runtime.ServerUtc,
                            "BROKER_PENDING_PRESENT");
                    }
                    else if (!awaitingConfirmation &&
                             (_lifecycle.State ==
                              LifecycleState.LivePosition ||
                              _lifecycle.State ==
                              LifecycleState.PendingOrder ||
                              _lifecycle.State ==
                              LifecycleState.ExitRequested))
                    {
                        _lifecycle.TryTransition(
                            LifecycleState.Closed,
                            _state.Runtime.ServerUtc,
                            "BROKER_OBJECTS_ABSENT");
                    }
                }
        
        
    }
}
