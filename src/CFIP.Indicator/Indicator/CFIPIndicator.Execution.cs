// Partial cTrader host orchestration module migrated from v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                        _state.Plan.Direction == CFIPClean89Direction.Buy
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
                        CFIPClean89LifecycleState.SignalDetected,
                        _state.Runtime.ServerUtc,
                        "DECISION_PLAN_READY");
                    _lifecycle.TryTransition(
                        CFIPClean89LifecycleState.PlanReady,
                        _state.Runtime.ServerUtc,
                        "TRADE_PLAN_VALID");
                    _lifecycle.TryTransition(
                        CFIPClean89LifecycleState.ExecutionReady,
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
                            CFIPClean89LifecycleState.Rejected,
                            _state.Runtime.ServerUtc,
                            "BROKER_EXECUTION_REJECTED");
                        return;
                    }
        
                    _submittedExecutionKeys.Add(
                        intent.Identity.IdempotencyKey);
        
                    if (intent.Kind == CFIPClean89ExecutionKind.Market)
                        RegisterBrokerConfirmation(
                            "POSITION",
                            result.BrokerPositionId,
                            _state.Runtime.ServerUtc);
                    else
                        RegisterBrokerConfirmation(
                            "ORDER",
                            result.BrokerOrderId,
                            _state.Runtime.ServerUtc);
        
                    if (intent.Kind != CFIPClean89ExecutionKind.Market &&
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
        
                    if (intent.Kind == CFIPClean89ExecutionKind.Market)
                    {
                        _lifecycle.TryTransition(
                            CFIPClean89LifecycleState.LivePosition,
                            _state.Runtime.ServerUtc,
                            "BROKER_MARKET_ACCEPTED");
                    }
                    else
                    {
                        _lifecycle.TryTransition(
                            CFIPClean89LifecycleState.PendingOrder,
                            _state.Runtime.ServerUtc,
                            "BROKER_PENDING_ACCEPTED");
                    }
        
                    if (result.ReconciliationRequired &&
                        result.BrokerPositionId.Length > 0)
                    {
                        CFIPClean89ExecutionResult protectionResult =
                            _brokerGateway.ModifyProtection(
                                result.BrokerPositionId,
                                intent.StopLoss.Price,
                                intent.EffectiveTarget.Price);
        
                        if (!protectionResult.Accepted)
                        {
                            _state.Execution = protectionResult;
                            _lifecycle.TryTransition(
                                CFIPClean89LifecycleState.RecoveryRequired,
                                _state.Runtime.ServerUtc,
                                "BROKER_PROTECTION_RECOVERY_FAILED");
                        }
                        else
                        {
                            _state.Execution = protectionResult;
                        }
                    }
                }
        
                private bool IsExecutionDuplicate(CFIPClean89TradePlan plan)
                {
                    string label =
                        _configuration.Get(
                            "AutoTradeLabel",
                            "CFIP-SMART-CLEAN89");
                    string signal =
                        plan.Identity.SignalId ?? string.Empty;
        
                    foreach (var key in new[]
                    {
                        "CFIP89|EXEC|" + plan.Identity.PlanId + "|" + CFIPClean89ExecutionKind.Market,
                        "CFIP89|EXEC|" + plan.Identity.PlanId + "|" + CFIPClean89ExecutionKind.Stop,
                        "CFIP89|EXEC|" + plan.Identity.PlanId + "|" + CFIPClean89ExecutionKind.Limit
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
        
                private double CalculateNormalizedVolume(CFIPClean89TradePlan plan)
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
                            CFIPClean89SizingMode.RiskPercentEquity) ==
                        CFIPClean89SizingMode.FixedLots)
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
                                Math.Max(
                                    0.01,
                                    _configuration.Get(
                                        "RiskPercentEquity",
                                        0.50)),
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
                    CFIPClean89TradePlan plan,
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
