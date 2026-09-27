// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89ExecutionPolicy : ICFIPClean89ExecutionPolicy
        {
            public CFIPClean89ExecutionReadiness Evaluate(
                CFIPClean89DecisionSnapshot decision,
                CFIPClean89EntrySnapshot entry,
                CFIPClean89TradePlan plan,
                CFIPClean89RuntimeSnapshot runtime,
                CFIPClean89ConfigSnapshot configuration,
                double volumeInUnits,
                double riskAmount,
                double estimatedMargin)
            {
                var blocks = new List<CFIPClean89BlockReason>();
    
                if (decision == null || entry == null || plan == null ||
                    runtime == null || configuration == null)
                    return Blocked(blocks, CFIPClean89BlockReason.DataIncomplete);
    
                bool market =
                    entry.Mode == CFIPClean89EntryMode.RetestMarket ||
                    entry.Mode == CFIPClean89EntryMode.BreakoutMarket;
    
                bool pending =
                    entry.Mode == CFIPClean89EntryMode.ContinuationStop ||
                    entry.Mode == CFIPClean89EntryMode.ReversalLimit;
    
                if (pending &&
                    entry.ExpiresUtc.HasValue &&
                    entry.ExpiresUtc.Value <= runtime.ServerUtc)
                    blocks.Add(CFIPClean89BlockReason.EntryInvalid);
    
                if (!decision.DecisionEligible ||
                    decision.Direction == CFIPClean89Direction.Wait ||
                    !plan.IsValid)
                    blocks.Add(CFIPClean89BlockReason.PolicyBlocked);
    
                if (entry.Decision != decision ||
                    entry.Direction != decision.Direction ||
                    entry.Mode != plan.EntryMode)
                    blocks.Add(CFIPClean89BlockReason.EntryInvalid);
    
                if (plan.Identity == null ||
                    string.IsNullOrWhiteSpace(plan.Identity.SignalId) ||
                    string.IsNullOrWhiteSpace(plan.Identity.PlanId))
                    blocks.Add(CFIPClean89BlockReason.DataIncomplete);
    
                CFIPClean89ExecutionKind kind =
                    market
                        ? CFIPClean89ExecutionKind.Market
                        : entry.Mode == CFIPClean89EntryMode.ContinuationStop
                            ? CFIPClean89ExecutionKind.Stop
                            : entry.Mode == CFIPClean89EntryMode.ReversalLimit
                                ? CFIPClean89ExecutionKind.Limit
                                : CFIPClean89ExecutionKind.None;
    
                if (market)
                {
                    if (!entry.Eligible ||
                        entry.State != CFIPClean89EntryTriggerState.Ready)
                        blocks.Add(CFIPClean89BlockReason.EntryInvalid);
    
                    if (!configuration.Get("EnableAutoTrading", false))
                        blocks.Add(CFIPClean89BlockReason.PolicyBlocked);
                }
                else if (pending)
                {
                    if (entry.State != CFIPClean89EntryTriggerState.WaitingBreakout ||
                        entry.Model == null)
                        blocks.Add(CFIPClean89BlockReason.EntryInvalid);
    
                    if (!configuration.Get("EnableAutomaticOrders", false))
                        blocks.Add(CFIPClean89BlockReason.PolicyBlocked);
                }
                else
                {
                    blocks.Add(CFIPClean89BlockReason.EntryInvalid);
                }
    
                if (configuration.Get("ConfirmedSignalsOnly", true) &&
                    decision.PolicyMode != CFIPClean89DecisionPolicyMode.Confirmed)
                    blocks.Add(CFIPClean89BlockReason.PolicyBlocked);
    
                if (decision.Confidence <
                    configuration.Get("MinimumAutoConfidence", 86))
                    blocks.Add(CFIPClean89BlockReason.ConfidenceTooLow);
    
                if (decision.Quality <
                    configuration.Get("MinimumAutoSmartQuality", 80))
                    blocks.Add(CFIPClean89BlockReason.EvidenceInsufficient);
    
                if (plan.LevelQuality <
                    configuration.Get("MinimumAutoLevelQuality", 72))
                    blocks.Add(CFIPClean89BlockReason.EntryInvalid);
    
                double requestedPrice =
                    ResolveRequestedPrice(entry);
    
                CFIPClean89TargetLevel target =
                    plan.TargetLadder.Find(
                        configuration.Get(
                            "AutoTpStage",
                            CFIPClean89TargetStage.TP1));
    
                if (target == null)
                    target =
                        plan.TargetLadder.Find(
                            CFIPClean89TargetStage.TP1);
    
                if (runtime.PipSize <= 0 ||
                    requestedPrice <= 0 ||
                    target == null)
                {
                    blocks.Add(CFIPClean89BlockReason.BrokerConstraintsBlocked);
                }
                else
                {
                    double stopDistancePips =
                        Math.Abs(
                            requestedPrice -
                            plan.StructuralStop.Price) /
                        runtime.PipSize;
                    double targetDistancePips =
                        Math.Abs(
                            target.Level.Price -
                            requestedPrice) /
                        runtime.PipSize;
    
                    if (pending)
                    {
                        bool validPendingSide =
                            kind == CFIPClean89ExecutionKind.Stop
                                ? decision.Direction == CFIPClean89Direction.Buy
                                    ? requestedPrice > runtime.Ask
                                    : requestedPrice < runtime.Bid
                                : kind == CFIPClean89ExecutionKind.Limit
                                    ? decision.Direction == CFIPClean89Direction.Buy
                                        ? requestedPrice < runtime.Ask
                                        : requestedPrice > runtime.Bid
                                    : false;
    
                        if (!validPendingSide)
                            blocks.Add(
                                CFIPClean89BlockReason.EntryInvalid);
                    }
    
                    if (runtime.BrokerConstraints.MinStopDistancePips > 0 &&
                        stopDistancePips + 0.000001 <
                        runtime.BrokerConstraints.MinStopDistancePips)
                        blocks.Add(
                            CFIPClean89BlockReason.BrokerConstraintsBlocked);
    
                    if (runtime.BrokerConstraints.MinTakeProfitDistancePips > 0 &&
                        targetDistancePips + 0.000001 <
                        runtime.BrokerConstraints.MinTakeProfitDistancePips)
                        blocks.Add(
                            CFIPClean89BlockReason.BrokerConstraintsBlocked);
                }
    
                if (runtime.BrokerConstraints.MinVolumeInUnits > 0 &&
                    volumeInUnits + 0.000001 <
                    runtime.BrokerConstraints.MinVolumeInUnits)
                    blocks.Add(CFIPClean89BlockReason.BrokerConstraintsBlocked);
    
                if (runtime.BrokerConstraints.VolumeStepInUnits > 0 &&
                    volumeInUnits > 0)
                {
                    double steps =
                        volumeInUnits /
                        runtime.BrokerConstraints.VolumeStepInUnits;
                    double nearest =
                        Math.Round(steps);
    
                    if (Math.Abs(steps - nearest) > 0.000001)
                        blocks.Add(
                            CFIPClean89BlockReason.BrokerConstraintsBlocked);
                }
    
                if (!runtime.SymbolTradingEnabled)
                    blocks.Add(CFIPClean89BlockReason.BrokerUnavailable);
    
                if (configuration.Get("UseSessionFilter", false) &&
                    !IsWithinConfiguredSession(
                        runtime.ServerUtc,
                        configuration))
                    blocks.Add(CFIPClean89BlockReason.SessionBlocked);
    
                if (configuration.Get("AvoidFridayLateEntry", false) &&
                    runtime.ServerUtc.DayOfWeek == DayOfWeek.Friday &&
                    runtime.ServerUtc.Hour >=
                    configuration.Get("FridayCutoffUtc", 18))
                    blocks.Add(CFIPClean89BlockReason.SessionBlocked);
    
                int exposureLimit =
                    Math.Max(1, configuration.Get("MaximumOpenPositions", 1));
    
                int managedExposure =
                    runtime.ManagedPositionCount +
                    runtime.ManagedPendingOrderCount;
    
                if (managedExposure >= exposureLimit)
                    blocks.Add(CFIPClean89BlockReason.ExistingExposureBlocked);
    
                if (configuration.Get("EnableDailyLossLimit", true))
                {
                    double baseline =
                        runtime.Balance -
                        runtime.DailyRealizedNetProfit;
    
                    double limit =
                        Math.Max(0, baseline) *
                        Math.Max(
                            0,
                            configuration.Get(
                                "MaximumDailyLossPercent",
                                3.0)) /
                        100.0;
    
                    if (runtime.DailyRealizedNetProfit < -limit)
                        blocks.Add(CFIPClean89BlockReason.DailyLossBlocked);
                }
    
                if (configuration.Get("UseAutoMarginGuard", true))
                {
                    double maxUsage =
                        Math.Max(
                            0,
                            configuration.Get(
                                "MaxAutoMarginUsagePercent",
                                80));
                    double buffer =
                        Math.Max(
                            0,
                            configuration.Get(
                                "MarginBufferPercent",
                                10));
    
                    double allowedMargin =
                        runtime.Equity *
                        Math.Max(0, maxUsage - buffer) /
                        100.0;
    
                    double projectedMargin =
                        runtime.Margin +
                        Math.Max(0, estimatedMargin);
    
                    if (runtime.Equity <= 0 ||
                        runtime.FreeMargin <= 0 ||
                        estimatedMargin < 0 ||
                        estimatedMargin > runtime.FreeMargin ||
                        projectedMargin > allowedMargin)
                        blocks.Add(CFIPClean89BlockReason.RiskInvalid);
                }
    
                double riskPercent =
                    Math.Max(
                        0,
                        configuration.Get(
                            "RiskPercentEquity",
                            0.50));
                double riskBudget =
                    runtime.Equity * riskPercent / 100.0;
    
                if (volumeInUnits <= 0 ||
                    riskAmount <= 0 ||
                    (riskBudget > 0 &&
                     riskAmount > riskBudget * 1.01) ||
                    estimatedMargin < 0)
                    blocks.Add(CFIPClean89BlockReason.RiskInvalid);
    
                return new CFIPClean89ExecutionReadiness(
                    blocks.Count == 0,
                    kind,
                    entry.Mode,
                    decision.Direction,
                    ResolveRequestedPrice(entry),
                    volumeInUnits,
                    riskAmount,
                    estimatedMargin,
                    blocks);
            }
    
            private bool IsWithinConfiguredSession(
                DateTime utc,
                CFIPClean89ConfigSnapshot configuration)
            {
                int start =
                    Math.Max(
                        0,
                        Math.Min(
                            23,
                            configuration.Get(
                                "SessionStartUtc",
                                6)));
                int end =
                    Math.Max(
                        0,
                        Math.Min(
                            23,
                            configuration.Get(
                                "SessionEndUtc",
                                20)));
    
                int hour = utc.Hour;
    
                if (start == end)
                    return true;
    
                return start < end
                    ? hour >= start && hour < end
                    : hour >= start || hour < end;
            }
    
            private double ResolveRequestedPrice(CFIPClean89EntrySnapshot entry)
            {
                if (entry == null || entry.Model == null)
                    return 0;
    
                if (entry.Mode == CFIPClean89EntryMode.ContinuationStop &&
                    entry.Model.Trigger != null)
                    return entry.Model.Trigger.Price;
    
                if (entry.Mode == CFIPClean89EntryMode.ReversalLimit)
                    return entry.Model.IdealEntry.Price;
    
                return entry.RequestedEntry != null
                    ? entry.RequestedEntry.Price
                    : entry.Model.IdealEntry.Price;
            }
    
            private CFIPClean89ExecutionReadiness Blocked(
                IList<CFIPClean89BlockReason> blocks,
                CFIPClean89BlockReason reason)
            {
                if (!blocks.Contains(reason))
                    blocks.Add(reason);
    
                return new CFIPClean89ExecutionReadiness(
                    false,
                    CFIPClean89ExecutionKind.None,
                    CFIPClean89EntryMode.None,
                    CFIPClean89Direction.Wait,
                    0,
                    0,
                    0,
                    0,
                    blocks);
            }
        }
}
