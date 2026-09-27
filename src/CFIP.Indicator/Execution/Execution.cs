using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator
{
        public sealed class ExecutionIntent
        {
            public ExecutionIdentity Identity { get; private set; }
            public TradeIdentity TradeIdentity { get; private set; }
            public Direction Direction { get; private set; }
            public ExecutionKind Kind { get; private set; }
            public EntryMode EntryMode { get; private set; }
    
            // Exact value requested from the broker.
            public PriceLevel RequestedEntry { get; private set; }
    
            // Structural activation threshold, when applicable.
            public PriceLevel Trigger { get; private set; }
    
            // Protection requested at execution time.
            public PriceLevel StopLoss { get; private set; }
    
            // This is the requested active broker target, not the whole ladder.
            public PriceLevel EffectiveTarget { get; private set; }
    
            public double VolumeInUnits { get; private set; }
            public RiskRequest Risk { get; private set; }
            public DecisionPolicyMode PolicyMode { get; private set; }
            public DateTime CreatedUtc { get; private set; }
            public DateTime? ExpiryUtc { get; private set; }
    
            public ExecutionIntent(
                ExecutionIdentity identity,
                TradeIdentity tradeIdentity,
                Direction direction,
                ExecutionKind kind,
                EntryMode entryMode,
                PriceLevel requestedEntry,
                PriceLevel trigger,
                PriceLevel stopLoss,
                PriceLevel effectiveTarget,
                double volumeInUnits,
                RiskRequest risk,
                DecisionPolicyMode policyMode,
                DateTime createdUtc,
                DateTime? expiryUtc)
            {
                Identity =
                    identity ??
                    throw new ArgumentNullException("identity");
                TradeIdentity =
                    tradeIdentity ??
                    throw new ArgumentNullException("tradeIdentity");
                Direction = direction;
                Kind = kind;
                EntryMode = entryMode;
                RequestedEntry = requestedEntry;
                Trigger = trigger;
                StopLoss =
                    stopLoss ??
                    throw new ArgumentNullException("stopLoss");
                EffectiveTarget = effectiveTarget;
                VolumeInUnits =
                    Math.Max(0, volumeInUnits);
                Risk =
                    risk ??
                    throw new ArgumentNullException("risk");
                PolicyMode = policyMode;
                CreatedUtc = createdUtc;
                ExpiryUtc = expiryUtc;
            }
        }
    
        public sealed class ExecutionResult
        {
            public bool Accepted { get; private set; }
            public string BrokerOrderId { get; private set; }
            public string BrokerPositionId { get; private set; }
            public PriceLevel ActualFill { get; private set; }
            public double RequestedVsActualDelta { get; private set; }
            public string BrokerError { get; private set; }
            public ProtectionState ProtectionState { get; private set; }
            public bool ReconciliationRequired { get; private set; }
            public string StatusDetail { get; private set; }
    
            public ExecutionResult(
                bool accepted,
                string brokerOrderId,
                string brokerPositionId,
                PriceLevel actualFill,
                double requestedVsActualDelta,
                string brokerError,
                ProtectionState protectionState,
                bool reconciliationRequired,
                string statusDetail = "")
            {
                Accepted = accepted;
                BrokerOrderId = brokerOrderId ?? string.Empty;
                BrokerPositionId = brokerPositionId ?? string.Empty;
                ActualFill = actualFill;
                RequestedVsActualDelta = requestedVsActualDelta;
                BrokerError = brokerError ?? string.Empty;
                ProtectionState = protectionState;
                ReconciliationRequired = reconciliationRequired;
                StatusDetail = statusDetail ?? string.Empty;
            }
        }
    
        public sealed class ExecutionReadiness
        {
            private readonly ReadOnlyCollection<BlockReason> _blockReasons;
    
            public bool Eligible { get; private set; }
            public ExecutionKind Kind { get; private set; }
            public EntryMode EntryMode { get; private set; }
            public Direction Direction { get; private set; }
            public double RequestedPrice { get; private set; }
            public double VolumeInUnits { get; private set; }
            public double RiskAmount { get; private set; }
            public double EstimatedMargin { get; private set; }
            public IReadOnlyList<BlockReason> BlockReasons { get { return _blockReasons; } }
    
            public ExecutionReadiness(
                bool eligible,
                ExecutionKind kind,
                EntryMode entryMode,
                Direction direction,
                double requestedPrice,
                double volumeInUnits,
                double riskAmount,
                double estimatedMargin,
                IList<BlockReason> blockReasons)
            {
                Eligible = eligible;
                Kind = kind;
                EntryMode = entryMode;
                Direction = direction;
                RequestedPrice = Math.Max(0, requestedPrice);
                VolumeInUnits = Math.Max(0, volumeInUnits);
                RiskAmount = Math.Max(0, riskAmount);
                EstimatedMargin = Math.Max(0, estimatedMargin);
                _blockReasons =
                    new ReadOnlyCollection<BlockReason>(
                        new List<BlockReason>(
                            blockReasons ??
                            new List<BlockReason>()));
            }
        }
    
        public sealed class ExecutionPolicy : IExecutionPolicy
        {
            public ExecutionReadiness Evaluate(
                DecisionSnapshot decision,
                EntrySnapshot entry,
                TradePlan plan,
                RuntimeSnapshot runtime,
                ConfigSnapshot configuration,
                double volumeInUnits,
                double riskAmount,
                double estimatedMargin)
            {
                var blocks = new List<BlockReason>();
    
                if (decision == null || entry == null || plan == null ||
                    runtime == null || configuration == null)
                    return Blocked(blocks, BlockReason.DataIncomplete);
    
                bool market =
                    entry.Mode == EntryMode.RetestMarket ||
                    entry.Mode == EntryMode.BreakoutMarket;
    
                bool pending =
                    entry.Mode == EntryMode.ContinuationStop ||
                    entry.Mode == EntryMode.ReversalLimit;
    
                if (pending &&
                    entry.ExpiresUtc.HasValue &&
                    entry.ExpiresUtc.Value <= runtime.ServerUtc)
                    blocks.Add(BlockReason.EntryInvalid);
    
                if (!decision.DecisionEligible ||
                    decision.Direction == Direction.Wait ||
                    !plan.IsValid)
                    blocks.Add(BlockReason.PolicyBlocked);
    
                if (entry.Decision != decision ||
                    entry.Direction != decision.Direction ||
                    entry.Mode != plan.EntryMode)
                    blocks.Add(BlockReason.EntryInvalid);
    
                if (plan.Identity == null ||
                    string.IsNullOrWhiteSpace(plan.Identity.SignalId) ||
                    string.IsNullOrWhiteSpace(plan.Identity.PlanId))
                    blocks.Add(BlockReason.DataIncomplete);
    
                ExecutionKind kind =
                    market
                        ? ExecutionKind.Market
                        : entry.Mode == EntryMode.ContinuationStop
                            ? ExecutionKind.Stop
                            : entry.Mode == EntryMode.ReversalLimit
                                ? ExecutionKind.Limit
                                : ExecutionKind.None;
    
                if (market)
                {
                    if (!entry.Eligible ||
                        entry.State != EntryTriggerState.Ready)
                        blocks.Add(BlockReason.EntryInvalid);
    
                    if (!configuration.Get("EnableAutoTrading", false))
                        blocks.Add(BlockReason.PolicyBlocked);
                }
                else if (pending)
                {
                    if (entry.State != EntryTriggerState.WaitingBreakout ||
                        entry.Model == null)
                        blocks.Add(BlockReason.EntryInvalid);
    
                    if (!configuration.Get("EnableAutomaticOrders", false))
                        blocks.Add(BlockReason.PolicyBlocked);
                }
                else
                {
                    blocks.Add(BlockReason.EntryInvalid);
                }
    
                if (configuration.Get("ConfirmedSignalsOnly", true) &&
                    decision.PolicyMode != DecisionPolicyMode.Confirmed)
                    blocks.Add(BlockReason.PolicyBlocked);
    
                if (decision.Confidence <
                    configuration.Get("MinimumAutoConfidence", 86))
                    blocks.Add(BlockReason.ConfidenceTooLow);
    
                if (decision.Quality <
                    configuration.Get("MinimumAutoSmartQuality", 80))
                    blocks.Add(BlockReason.EvidenceInsufficient);
    
                if (plan.LevelQuality <
                    configuration.Get("MinimumAutoLevelQuality", 72))
                    blocks.Add(BlockReason.EntryInvalid);
    
                double requestedPrice =
                    ResolveRequestedPrice(entry);
    
                TargetLevel target =
                    plan.TargetLadder.Find(
                        configuration.Get(
                            "AutoTpStage",
                            TargetStage.TP1));
    
                if (target == null)
                    target =
                        plan.TargetLadder.Find(
                            TargetStage.TP1);
    
                if (runtime.PipSize <= 0 ||
                    requestedPrice <= 0 ||
                    target == null)
                {
                    blocks.Add(BlockReason.BrokerConstraintsBlocked);
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
                            kind == ExecutionKind.Stop
                                ? decision.Direction == Direction.Buy
                                    ? requestedPrice > runtime.Ask
                                    : requestedPrice < runtime.Bid
                                : kind == ExecutionKind.Limit
                                    ? decision.Direction == Direction.Buy
                                        ? requestedPrice < runtime.Ask
                                        : requestedPrice > runtime.Bid
                                    : false;
    
                        if (!validPendingSide)
                            blocks.Add(
                                BlockReason.EntryInvalid);
                    }
    
                    if (runtime.BrokerConstraints.MinStopDistancePips > 0 &&
                        stopDistancePips + 0.000001 <
                        runtime.BrokerConstraints.MinStopDistancePips)
                        blocks.Add(
                            BlockReason.BrokerConstraintsBlocked);
    
                    if (runtime.BrokerConstraints.MinTakeProfitDistancePips > 0 &&
                        targetDistancePips + 0.000001 <
                        runtime.BrokerConstraints.MinTakeProfitDistancePips)
                        blocks.Add(
                            BlockReason.BrokerConstraintsBlocked);
                }
    
                if (runtime.BrokerConstraints.MinVolumeInUnits > 0 &&
                    volumeInUnits + 0.000001 <
                    runtime.BrokerConstraints.MinVolumeInUnits)
                    blocks.Add(BlockReason.BrokerConstraintsBlocked);
    
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
                            BlockReason.BrokerConstraintsBlocked);
                }
    
                if (!runtime.SymbolTradingEnabled)
                    blocks.Add(BlockReason.BrokerUnavailable);
    
                if (configuration.Get("UseSessionFilter", false) &&
                    !IsWithinConfiguredSession(
                        runtime.ServerUtc,
                        configuration))
                    blocks.Add(BlockReason.SessionBlocked);
    
                if (configuration.Get("AvoidFridayLateEntry", false) &&
                    runtime.ServerUtc.DayOfWeek == DayOfWeek.Friday &&
                    runtime.ServerUtc.Hour >=
                    configuration.Get("FridayCutoffUtc", 18))
                    blocks.Add(BlockReason.SessionBlocked);
    
                int exposureLimit =
                    Math.Max(1, configuration.Get("MaximumOpenPositions", 1));
    
                int managedExposure =
                    runtime.ManagedPositionCount +
                    runtime.ManagedPendingOrderCount;
    
                if (managedExposure >= exposureLimit)
                    blocks.Add(BlockReason.ExistingExposureBlocked);
    
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
                        blocks.Add(BlockReason.DailyLossBlocked);
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
                        blocks.Add(BlockReason.RiskInvalid);
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
                    blocks.Add(BlockReason.RiskInvalid);
    
                return new ExecutionReadiness(
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
                ConfigSnapshot configuration)
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
    
            private double ResolveRequestedPrice(EntrySnapshot entry)
            {
                if (entry == null || entry.Model == null)
                    return 0;
    
                if (entry.Mode == EntryMode.ContinuationStop &&
                    entry.Model.Trigger != null)
                    return entry.Model.Trigger.Price;
    
                if (entry.Mode == EntryMode.ReversalLimit)
                    return entry.Model.IdealEntry.Price;
    
                return entry.RequestedEntry != null
                    ? entry.RequestedEntry.Price
                    : entry.Model.IdealEntry.Price;
            }
    
            private ExecutionReadiness Blocked(
                IList<BlockReason> blocks,
                BlockReason reason)
            {
                if (!blocks.Contains(reason))
                    blocks.Add(reason);
    
                return new ExecutionReadiness(
                    false,
                    ExecutionKind.None,
                    EntryMode.None,
                    Direction.Wait,
                    0,
                    0,
                    0,
                    0,
                    blocks);
            }
        }
    
        public sealed class ExecutionPlanner : IExecutionPlanner
        {
            public ExecutionIntent CreateIntent(
                TradePlan plan,
                EntrySnapshot entry,
                RuntimeSnapshot runtime,
                ConfigSnapshot configuration,
                ExecutionReadiness readiness)
            {
                if (plan == null || entry == null || runtime == null ||
                    configuration == null || readiness == null || !readiness.Eligible)
                    throw new ArgumentException(
                        "Execution intent requires eligible execution state.");
    
                if (entry.Mode != plan.EntryMode ||
                    entry.Direction != plan.Direction ||
                    readiness.Kind == ExecutionKind.None)
                    throw new ArgumentException(
                        "Execution intent must match the authoritative TradePlan.");
    
                var requested =
                    new PriceLevel(
                        readiness.RequestedPrice,
                        "EXECUTION_REQUESTED_ENTRY",
                        Provenance.Direct(
                            "EXECUTION",
                            readiness.Kind.ToString().ToUpperInvariant()));
    
                TargetStage requestedStage =
                    configuration.Get(
                        "AutoTpStage",
                        TargetStage.TP1);
    
                TargetLevel target =
                    plan.TargetLadder.Find(requestedStage);
    
                if (target == null)
                    throw new ArgumentException(
                        "Requested execution target stage is unavailable.",
                        "AutoTpStage");
    
                string seed =
                    plan.Identity.PlanId +
                    "|" +
                    readiness.Kind.ToString();
    
                return new ExecutionIntent(
                    new ExecutionIdentity(
                        seed,
                        "CFIP|EXEC|" + seed,
                        runtime.ServerUtc),
                    plan.Identity,
                    plan.Direction,
                    readiness.Kind,
                    entry.Mode,
                    requested,
                    entry.Model.Trigger,
                    plan.StructuralStop,
                    target.Level,
                    readiness.VolumeInUnits,
                    new RiskRequest(
                        configuration.Get("RiskPercentEquity", 0.50),
                        readiness.VolumeInUnits,
                        readiness.RiskAmount),
                    entry.Decision.PolicyMode,
                    runtime.ServerUtc,
                    readiness.Kind == ExecutionKind.Market
                        ? null
                        : runtime.ServerUtc +
                          TimeSpan.FromMinutes(
                              Math.Max(
                                  15,
                                  configuration.Get(
                                      "PendingOrderExpiryMinutes",
                                      120))));
            }
        }
    
        public interface IExecutionPolicy
        {
            ExecutionReadiness Evaluate(
                DecisionSnapshot decision,
                EntrySnapshot entry,
                TradePlan plan,
                RuntimeSnapshot runtime,
                ConfigSnapshot configuration,
                double volumeInUnits,
                double riskAmount,
                double estimatedMargin);
        }
    
        public interface IExecutionPlanner
        {
            ExecutionIntent CreateIntent(
                TradePlan plan,
                EntrySnapshot entry,
                RuntimeSnapshot runtime,
                ConfigSnapshot configuration,
                ExecutionReadiness readiness);
        }
    
    
}
