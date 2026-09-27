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
                        private void TryExecuteCurrentCycle()
                {
                    if (_state.Plan == null ||
                        !_state.Plan.IsValid ||
                        _state.Plan.Identity == null ||
                        _state.Decision == null ||
                        _state.Entry == null)
                        return;
        
                    if (_pendingOrderLifecycle != null)
                        _pendingOrderLifecycle.InvalidateSupersededPlan(
                            _state.Plan.Identity.PlanId,
                            _state.Runtime.ServerUtc);
        
                    if (IsExecutionDuplicate(_state.Plan))
                        return;
        
                    double volume =
                        CalculateNormalizedVolume(_state.Plan);
        
                    double riskAmount =
                        CalculateRiskAmount(_state.Plan, volume);
        
                    TradeType tradeType =
                        _state.Plan.Direction == Direction.Buy
                            ? TradeType.Buy
                            : TradeType.Sell;
        
                    double estimatedMargin =
                        volume > 0
                            ? Symbol.GetEstimatedMargin(
                                tradeType,
                                volume)
                            : 0;
        
                    var readiness =
                        _executionPolicy.Evaluate(
                            _state.Decision,
                            _state.Entry,
                            _state.Plan,
                            _state.Runtime,
                            _configuration,
                            volume,
                            riskAmount,
                            estimatedMargin);
        
                    if (!readiness.Eligible)
                        return;
        
                    _lifecycle.TryTransition(
                        LifecycleState.SignalDetected,
                        _state.Runtime.ServerUtc,
                        "DECISION_PLAN_READY");
                    _lifecycle.TryTransition(
                        LifecycleState.PlanReady,
                        _state.Runtime.ServerUtc,
                        "TRADE_PLAN_VALID");
                    _lifecycle.TryTransition(
                        LifecycleState.ExecutionReady,
                        _state.Runtime.ServerUtc,
                        "EXECUTION_POLICY_ACCEPTED");
        
                    var intent =
                        _executionPlanner.CreateIntent(
                            _state.Plan,
                            _state.Entry,
                            _state.Runtime,
                            _configuration,
                            readiness);
        
                    _state.Intent = intent;
        
                    var result =
                        _brokerGateway.Execute(intent);
        
                    _state.Execution = result;
        
                    if (!result.Accepted)
                    {
                        _lifecycle.TryTransition(
                            LifecycleState.Rejected,
                            _state.Runtime.ServerUtc,
                            "BROKER_EXECUTION_REJECTED");
                        return;
                    }
        
                    _submittedExecutionKeys.Add(
                        intent.Identity.IdempotencyKey);
        
                    if (intent.Kind == ExecutionKind.Market)
                        RegisterBrokerConfirmation(
                            "POSITION",
                            result.BrokerPositionId,
                            _state.Runtime.ServerUtc);
                    else
                        RegisterBrokerConfirmation(
                            "ORDER",
                            result.BrokerOrderId,
                            _state.Runtime.ServerUtc);
        
                    if (intent.Kind != ExecutionKind.Market &&
                        result.BrokerOrderId.Length > 0 &&
                        _pendingOrderLifecycle != null)
                    {
                        foreach (var order in PendingOrders)
                        {
                            if (order.Id.ToString() == result.BrokerOrderId)
                            {
                                _pendingOrderLifecycle.RegisterSubmitted(
                                    order,
                                    intent,
                                    _state.Runtime.ServerUtc);
                                break;
                            }
                        }
                    }
        
                    if (intent.Kind == ExecutionKind.Market)
                    {
                        _lifecycle.TryTransition(
                            LifecycleState.LivePosition,
                            _state.Runtime.ServerUtc,
                            "BROKER_MARKET_ACCEPTED");
                    }
                    else
                    {
                        _lifecycle.TryTransition(
                            LifecycleState.PendingOrder,
                            _state.Runtime.ServerUtc,
                            "BROKER_PENDING_ACCEPTED");
                    }
        
                    if (result.ReconciliationRequired &&
                        result.BrokerPositionId.Length > 0)
                    {
                        ExecutionResult protectionResult =
                            _brokerGateway.ModifyProtection(
                                result.BrokerPositionId,
                                intent.StopLoss.Price,
                                intent.EffectiveTarget.Price);
        
                        if (!protectionResult.Accepted)
                        {
                            _state.Execution = protectionResult;
                            _lifecycle.TryTransition(
                                LifecycleState.RecoveryRequired,
                                _state.Runtime.ServerUtc,
                                "BROKER_PROTECTION_RECOVERY_FAILED");
                        }
                        else
                        {
                            _state.Execution = protectionResult;
                        }
                    }
                }
        
                private bool IsExecutionDuplicate(TradePlan plan)
                {
                    string label =
                        _configuration.Get(
                            "AutoTradeLabel",
                            "CFIP-SMART");
                    string signal =
                        plan.Identity.SignalId ?? string.Empty;
        
                    foreach (var key in new[]
                    {
                        "CFIP|EXEC|" + plan.Identity.PlanId + "|" + ExecutionKind.Market,
                        "CFIP|EXEC|" + plan.Identity.PlanId + "|" + ExecutionKind.Stop,
                        "CFIP|EXEC|" + plan.Identity.PlanId + "|" + ExecutionKind.Limit
                    })
                    {
                        if (_submittedExecutionKeys.Contains(key))
                            return true;
                    }
        
                    foreach (var position in Positions)
                    {
                        if (string.Equals(position.Label, label, StringComparison.Ordinal) &&
                            string.Equals(position.SymbolName, SymbolName, StringComparison.Ordinal) &&
                            HasCurrentIdentity(position.Comment) &&
                            (position.Comment ?? string.Empty).IndexOf(
                                signal,
                                StringComparison.Ordinal) >= 0)
                            return true;
                    }
        
                    foreach (var order in PendingOrders)
                    {
                        if (string.Equals(order.Label, label, StringComparison.Ordinal) &&
                            string.Equals(order.SymbolName, SymbolName, StringComparison.Ordinal) &&
                            HasCurrentIdentity(order.Comment) &&
                            (order.Comment ?? string.Empty).IndexOf(
                                signal,
                                StringComparison.Ordinal) >= 0)
                            return true;
                    }
        
                    if (_configuration.Get("OneOrderPerSignal", true))
                    {
                        HistoricalTrade[] trades =
                            History.FindAll(
                                label,
                                SymbolName);
        
                        if (trades != null)
                        {
                            for (int i = 0; i < trades.Length; i++)
                            {
                                if ((trades[i].Comment ?? string.Empty).IndexOf(
                                        signal,
                                        StringComparison.Ordinal) >= 0)
                                    return true;
                            }
                        }
                    }
        
                    return false;
                }
        
                private double CalculateNormalizedVolume(TradePlan plan)
                {
                    if (plan == null || Symbol.PipSize <= 0)
                        return 0;
        
                    double stopPips =
                        Math.Abs(
                            plan.ExecutionAnchor.Price -
                            plan.StructuralStop.Price) /
                        Symbol.PipSize;
        
                    if (stopPips <= 0)
                        return 0;
        
                    double raw;
        
                    if (_configuration.Get(
                            "SizingMode",
                            SizingMode.RiskPercentEquity) ==
                        SizingMode.FixedLots)
                    {
                        raw =
                            Symbol.QuantityToVolumeInUnits(
                                Math.Max(
                                    0.001,
                                    _configuration.Get(
                                        "FixedLots",
                                        0.01)));
                    }
                    else
                    {
                        raw =
                            Symbol.VolumeForProportionalRisk(
                                ProportionalAmountType.Equity,
                                RiskPolicy.ResolveRiskPercent(
                                    _configuration,
                                    _state.Decision == null
                                        ? DecisionPolicyMode.Confirmed
                                        : _state.Decision.PolicyMode,
                                    _state.Suitability),
                                stopPips,
                                RoundingMode.Down);
                    }
        
                    if (raw <= 0)
                        return 0;
        
                    raw =
                        Symbol.NormalizeVolumeInUnits(
                            raw,
                            RoundingMode.Down);
        
                    if (raw < Symbol.VolumeInUnitsMin)
                        return 0;
        
                    return Math.Min(
                        raw,
                        Symbol.NormalizeVolumeInUnits(
                            Symbol.VolumeInUnitsMax,
                            RoundingMode.Down));
                }
        
                private double CalculateRiskAmount(
                    TradePlan plan,
                    double volume)
                {
                    if (plan == null || volume <= 0 || Symbol.PipSize <= 0)
                        return 0;
        
                    double stopPips =
                        Math.Abs(
                            plan.ExecutionAnchor.Price -
                            plan.StructuralStop.Price) /
                        Symbol.PipSize;
        
                    return stopPips > 0
                        ? Symbol.AmountRisked(volume, stopPips)
                        : 0;
                }
        
                private double ConvertMinimumDistanceToPips(double rawDistance)
                {
                    if (rawDistance <= 0 || Symbol.PipSize <= 0)
                        return 0;
        
                    if (Symbol.MinDistanceType ==
                        SymbolMinDistanceType.Pips)
                        return rawDistance;
        
                    double reference =
                        Math.Max(Symbol.Ask, Symbol.Bid);
        
                    return reference > 0
                        ? reference * rawDistance / 100.0 / Symbol.PipSize
                        : 0;
                }
        
        
    }
}
