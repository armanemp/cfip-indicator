using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator
{
        public enum LivePositionActionKind
        {
            None = 0,
            ProtectionUpdate = 1,
            PartialClose = 2,
            Close = 3
        }
    
        public sealed class LivePositionAction
        {
            public string BrokerPositionId { get; private set; }
            public LivePositionActionKind Kind { get; private set; }
            public double? StopLoss { get; private set; }
            public double? TakeProfit { get; private set; }
            public double VolumeInUnits { get; private set; }
            public TargetStage TargetStage { get; private set; }
            public string Reason { get; private set; }
            public DateTime CreatedUtc { get; private set; }
    
            public LivePositionAction(
                string brokerPositionId,
                LivePositionActionKind kind,
                double? stopLoss,
                double? takeProfit,
                double volumeInUnits,
                TargetStage targetStage,
                string reason,
                DateTime createdUtc)
            {
                BrokerPositionId = brokerPositionId ?? string.Empty;
                Kind = kind;
                StopLoss = stopLoss;
                TakeProfit = takeProfit;
                VolumeInUnits = Math.Max(0, volumeInUnits);
                TargetStage = targetStage;
                Reason = reason ?? string.Empty;
                CreatedUtc = createdUtc;
            }
        }
    
        internal sealed class LivePlanSnapshot
        {
            public string PlanId;
            public Direction Direction;
            public double EntryPrice;
            public double StructuralStopPrice;
            public double InvalidationPrice;
            public double? Tp1Price;
            public double? Tp2Price;
            public double? Tp3Price;
            public double? Tp4Price;
        }
    
        internal sealed class LivePositionContext
        {
            public string BrokerPositionId;
            public string PlanId;
            public Direction Direction;
            public LivePlanSnapshot PlanSnapshot;
            public double InitialEntryPrice;
            public double InitialRiskPrice;
            public double InitialVolumeInUnits;
            public double LastObservedVolumeInUnits;
            public double InvalidationPrice;
            public TargetStage ActiveTargetStage;
            public double PeakRR;
            public bool Tp1Consumed;
            public bool Tp2Consumed;
            public bool PartialPending;
            public bool ProtectionPending;
            public double? PendingStopLoss;
            public double? PendingTakeProfit;
            public TargetStage PendingProtectionStage;
            public DateTime PendingProtectionRequestedUtc;
            public DateTime LastTargetRepriceReferenceUtc;
            public TargetStage PendingPartialStage;
            public double PendingPartialRequestedVolume;
            public double PendingPartialExpectedRemainingVolume;
            public DateTime PartialAcceptedUtc;
            public int PartialRejectAttempts;
            public DateTime NextPartialRetryUtc;
            public bool ManagementRecoveryRequired;
        }
    
        public sealed class LivePositionManager
        {
            private readonly Dictionary<string, LivePositionContext> _contexts =
                new Dictionary<string, LivePositionContext>(
                    StringComparer.Ordinal);
    
            private readonly List<LivePositionAction> _actions =
                new List<LivePositionAction>();
    
            private readonly HashSet<string> _actionKeys =
                new HashSet<string>(StringComparer.Ordinal);
    
            public int ManagedPositionCount
            {
                get { return _contexts.Count; }
            }
    
            internal IReadOnlyList<LivePositionContext> Contexts
            {
                get
                {
                    return new List<LivePositionContext>(
                        _contexts.Values).AsReadOnly();
                }
            }
    
            public void Register(
                Position position,
                TradePlan plan)
            {
                if (position == null)
                    return;
    
                string id = position.Id.ToString();
    
                LivePositionContext existing;
                if (_contexts.TryGetValue(id, out existing))
                {
                    if (existing.PlanSnapshot == null &&
                        plan != null &&
                        plan.Identity != null &&
                        position.Comment != null &&
                        position.Comment.IndexOf(
                            plan.Identity.PlanId,
                            StringComparison.Ordinal) >= 0 &&
                        plan.Direction ==
                            (position.TradeType == TradeType.Buy
                                ? Direction.Buy
                                : Direction.Sell))
                        existing.PlanSnapshot =
                            BuildPlanSnapshot(plan);
    
                    return;
                }
    
                Direction direction =
                    position.TradeType == TradeType.Buy
                        ? Direction.Buy
                        : Direction.Sell;
    
                LivePlanSnapshot planSnapshot = null;
    
                double risk =
                    position.StopLoss.HasValue
                        ? Math.Abs(
                            position.EntryPrice -
                            position.StopLoss.Value)
                        : 0;
    
                double invalidation = 0;
                string planId = string.Empty;
                TargetStage stage =
                    TargetStage.TP1;
    
                bool belongsToPlan =
                    plan != null &&
                    plan.Identity != null &&
                    position.Comment != null &&
                    position.Comment.IndexOf(
                        plan.Identity.PlanId,
                        StringComparison.Ordinal) >= 0 &&
                    plan.Direction == direction;
    
                if (belongsToPlan)
                {
                    planId = plan.Identity.PlanId;
                    planSnapshot = BuildPlanSnapshot(plan);
    
                    if (plan.StructuralStop != null)
                        risk =
                            Math.Abs(
                                position.EntryPrice -
                                plan.StructuralStop.Price);
    
                    if (plan.Entry != null &&
                        plan.Entry.Invalidation != null)
                        invalidation =
                            plan.Entry.Invalidation.Price;
    
                    stage =
                        FindStageForTarget(
                            plan,
                            position.TakeProfit);
                }
    
                _contexts[id] =
                    new LivePositionContext
                    {
                        BrokerPositionId = id,
                        PlanId = planId,
                        Direction = direction,
                        PlanSnapshot = planSnapshot,
                        InitialEntryPrice = position.EntryPrice,
                        InitialRiskPrice = risk,
                        InitialVolumeInUnits = position.VolumeInUnits,
                        LastObservedVolumeInUnits = position.VolumeInUnits,
                        InvalidationPrice = invalidation,
                        ActiveTargetStage = stage,
                        PeakRR = 0,
                        Tp1Consumed = false,
                        Tp2Consumed = false,
                        PartialPending = false,
                        ProtectionPending = false,
                        PendingStopLoss = null,
                        PendingTakeProfit = null,
                        PendingProtectionStage =
                            stage,
                        PendingProtectionRequestedUtc =
                            DateTime.MinValue,
                        LastTargetRepriceReferenceUtc =
                            DateTime.MinValue,
                        PendingPartialStage = TargetStage.TP1,
                        PendingPartialRequestedVolume = 0,
                        PendingPartialExpectedRemainingVolume = 0,
                        PartialAcceptedUtc = DateTime.MinValue,
                        PartialRejectAttempts = 0,
                        NextPartialRetryUtc = DateTime.MinValue,
                        ManagementRecoveryRequired = false
                    };
            }
    
            public void RegisterFromSnapshot(
                BrokerPositionSnapshot snapshot,
                TradePlan plan)
            {
                if (snapshot == null ||
                    !snapshot.IsOpen ||
                    snapshot.VolumeInUnits <= 0)
                    return;
    
                LivePositionContext existing;
                if (_contexts.TryGetValue(
                        snapshot.BrokerPositionId,
                        out existing))
                    return;
    
                string signalId;
                string parsedPlanId;
                ParseIdentity(
                    snapshot.Comment,
                    out signalId,
                    out parsedPlanId);
    
                string planId = parsedPlanId ?? string.Empty;
                LivePlanSnapshot planSnapshot = null;
                bool belongsToPlan =
                    plan != null &&
                    plan.Identity != null &&
                    snapshot.Comment != null &&
                    snapshot.Comment.IndexOf(
                        plan.Identity.PlanId,
                        StringComparison.Ordinal) >= 0;
    
                if (belongsToPlan)
                {
                    planId = plan.Identity.PlanId;
                    planSnapshot = BuildPlanSnapshot(plan);
                }
    
                double risk =
                    snapshot.StopLoss.HasValue
                        ? Math.Abs(
                            snapshot.EntryPrice -
                            snapshot.StopLoss.Value)
                        : 0;
    
                if (belongsToPlan &&
                    plan.StructuralStop != null)
                {
                    risk =
                        Math.Abs(
                            snapshot.EntryPrice -
                            plan.StructuralStop.Price);
                }
    
                _contexts[snapshot.BrokerPositionId] =
                    new LivePositionContext
                    {
                        BrokerPositionId =
                            snapshot.BrokerPositionId,
                        PlanId = planId,
                        Direction = snapshot.Direction,
                        PlanSnapshot = planSnapshot,
                        InitialEntryPrice =
                            snapshot.EntryPrice,
                        InitialRiskPrice = risk,
                        InitialVolumeInUnits =
                            snapshot.VolumeInUnits,
                        LastObservedVolumeInUnits =
                            snapshot.VolumeInUnits,
                        InvalidationPrice =
                            belongsToPlan &&
                            plan.Entry != null &&
                            plan.Entry.Invalidation != null
                                ? plan.Entry.Invalidation.Price
                                : 0,
                        ActiveTargetStage =
                            belongsToPlan
                                ? FindStageForTarget(
                                    plan,
                                    snapshot.TakeProfit)
                                : TargetStage.TP1,
                        PeakRR = 0,
                        Tp1Consumed = false,
                        Tp2Consumed = false,
                        PartialPending = false,
                        PendingPartialStage =
                            TargetStage.TP1,
                        PendingPartialRequestedVolume = 0,
                        PendingPartialExpectedRemainingVolume = 0,
                        PartialAcceptedUtc = DateTime.MinValue,
                        PartialRejectAttempts = 0,
                        NextPartialRetryUtc = DateTime.MinValue,
                        ManagementRecoveryRequired =
                            !belongsToPlan
                    };
            }
    
            public void HandleClosed(Position position)
            {
                if (position != null)
                    _contexts.Remove(position.Id.ToString());
            }
    
            public void HandleProtectionRequested(
                LivePositionAction action,
                DateTime utc)
            {
                if (action == null ||
                    action.Kind !=
                        LivePositionActionKind.ProtectionUpdate)
                    return;
    
                LivePositionContext context;
                if (!_contexts.TryGetValue(
                        action.BrokerPositionId,
                        out context))
                    return;
    
                context.ProtectionPending = true;
                context.PendingStopLoss = action.StopLoss;
                context.PendingTakeProfit = action.TakeProfit;
                context.PendingProtectionStage = action.TargetStage;
                context.PendingProtectionRequestedUtc = utc;
            }
    
            public void HandlePartialResult(
                LivePositionAction action,
                ExecutionResult result,
                DateTime utc)
            {
                if (action == null ||
                    result == null ||
                    action.Kind !=
                        LivePositionActionKind.PartialClose)
                    return;
    
                LivePositionContext context;
    
                if (!_contexts.TryGetValue(
                        action.BrokerPositionId,
                        out context))
                    return;
    
                if (result.Accepted)
                {
                    context.PartialPending = true;
                    context.PendingPartialStage = action.TargetStage;
                    context.PendingPartialRequestedVolume =
                        action.VolumeInUnits;
                    context.PendingPartialExpectedRemainingVolume =
                        Math.Max(
                            0,
                            context.LastObservedVolumeInUnits -
                            action.VolumeInUnits);
                    context.PartialAcceptedUtc = utc;
                    context.PartialRejectAttempts = 0;
                    context.NextPartialRetryUtc = DateTime.MinValue;
                    context.ManagementRecoveryRequired = false;
                }
                else
                {
                    context.PartialPending = false;
                    context.PartialRejectAttempts++;
                    context.NextPartialRetryUtc =
                        utc +
                        RetryDelay(
                            context.PartialRejectAttempts);
                }
            }
    
            public IReadOnlyList<LivePositionAction> Evaluate(
                IReadOnlyList<BrokerPositionSnapshot> positions,
                RuntimeSnapshot runtime,
                TradePlan currentPlan,
                MarketModel market,
                StructureSnapshot structure,
                ConfigSnapshot configuration)
            {
                _actions.Clear();
                _actionKeys.Clear();
    
                if (positions == null ||
                    runtime == null ||
                    configuration == null)
                    return new List<LivePositionAction>()
                        .AsReadOnly();
    
                for (int i = 0; i < positions.Count; i++)
                {
                    BrokerPositionSnapshot snapshot =
                        positions[i];
    
                    if (snapshot == null ||
                        !snapshot.IsOpen ||
                        snapshot.VolumeInUnits <= 0)
                        continue;
    
                    LivePositionContext context;
    
                    if (!_contexts.TryGetValue(
                            snapshot.BrokerPositionId,
                            out context))
                    {
                        RegisterFromSnapshot(
                            snapshot,
                            currentPlan);
    
                        if (!_contexts.TryGetValue(
                                snapshot.BrokerPositionId,
                                out context))
                            continue;
                    }
    
                    context.LastObservedVolumeInUnits =
                        snapshot.VolumeInUnits;
    
                    TradePlan planForPosition =
                        PlanMatches(
                            context,
                            currentPlan)
                            ? currentPlan
                            : null;
    
                    if (context.ProtectionPending &&
                        ProtectionObserved(
                            context,
                            snapshot))
                    {
                        context.ProtectionPending = false;
    
                        if ((int)context.PendingProtectionStage >
                            (int)context.ActiveTargetStage)
                            context.ActiveTargetStage =
                                context.PendingProtectionStage;
    
                        if (context.PendingTakeProfit.HasValue &&
                            structure != null)
                        {
                            context.LastTargetRepriceReferenceUtc =
                                structure.ReferenceUtc;
                        }
    
                        context.PendingStopLoss = null;
                        context.PendingTakeProfit = null;
                        context.PendingProtectionRequestedUtc =
                            DateTime.MinValue;
                    }
                    else if (context.ProtectionPending &&
                             context.PendingProtectionRequestedUtc !=
                                DateTime.MinValue &&
                             context.PendingProtectionRequestedUtc.AddSeconds(30) <
                                runtime.ServerUtc)
                    {
                        context.ProtectionPending = false;
                        context.PendingStopLoss = null;
                        context.PendingTakeProfit = null;
                        context.PendingProtectionRequestedUtc =
                            DateTime.MinValue;
                    }
    
                    if (planForPosition != null &&
                        snapshot.TakeProfit.HasValue)
                    {
                        TargetStage observedStage =
                            FindStageForTarget(
                                planForPosition,
                                snapshot.TakeProfit);
    
                        if ((int)observedStage >
                            (int)context.ActiveTargetStage)
                            context.ActiveTargetStage =
                                observedStage;
                    }
    
                    double mark =
                        context.Direction ==
                            Direction.Buy
                            ? runtime.Bid
                            : runtime.Ask;
    
                    double currentRR =
                        CalculateRR(
                            context,
                            mark);
    
                    context.PeakRR =
                        Math.Max(
                            context.PeakRR,
                            Math.Max(0, currentRR));
    
                    ConfirmPartialProgress(
                        context,
                        snapshot,
                        runtime.ServerUtc);
    
                    if (!configuration.Get(
                            "EnableLiveExitManagement",
                            true))
                        continue;
    
                    LivePositionAction forcedExit =
                        EvaluateForcedExit(
                            context,
                            snapshot,
                            runtime,
                            market,
                            structure,
                            configuration,
                            currentRR);
    
                    if (forcedExit != null)
                    {
                        Queue(forcedExit);
                        continue;
                    }
    
                    LivePositionAction partial =
                        EvaluatePartialTakeProfit(
                            context,
                            snapshot,
                            runtime,
                            planForPosition,
                            configuration);
    
                    if (partial != null)
                    {
                        Queue(partial);
                        continue;
                    }
    
                    LivePositionAction protection =
                        EvaluateProtection(
                            context,
                            snapshot,
                            runtime,
                            planForPosition,
                            market,
                            structure,
                            configuration,
                            currentRR);
    
                    if (protection != null)
                        Queue(protection);
                }
    
                return new List<LivePositionAction>(
                    _actions).AsReadOnly();
            }
    
            private LivePositionAction EvaluateForcedExit(
                LivePositionContext context,
                BrokerPositionSnapshot snapshot,
                RuntimeSnapshot runtime,
                MarketModel market,
                StructureSnapshot structure,
                ConfigSnapshot configuration,
                double currentRR)
            {
                if (configuration.Get(
                        "EnableEndOfDayAutoClose",
                        true) &&
                    runtime.ServerUtc.Hour >= configuration.Get(
                        "SessionEndUtc",
                        20))
                    return NewClose(
                        context,
                        runtime.ServerUtc,
                        "END_OF_DAY");
    
                if (configuration.Get(
                        "EnableSetupInvalidation",
                        true))
                {
                    bool broken = false;
    
                    if (context.InvalidationPrice > 0)
                        broken =
                            context.Direction ==
                                Direction.Buy
                                ? runtime.Bid <=
                                    context.InvalidationPrice
                                : runtime.Ask >=
                                    context.InvalidationPrice;
    
                    bool adverse =
                        currentRR <= -Math.Max(
                            0,
                            configuration.Get(
                                "InvalidationMaxAdverseR",
                                0.75));
    
                    bool oppositeMtf =
                        market != null &&
                        market.M15 != null &&
                        market.M15.BiasDirection ==
                            Opposite(
                                context.Direction) &&
                        market.M15.BiasStrength >=
                            configuration.Get(
                                "LiveReversalMinimumConfidence",
                                68);
    
                    if ((broken || adverse) &&
                        (!configuration.Get(
                            "RequireMtfFlipForInvalidation",
                            true) ||
                         oppositeMtf))
                        return NewClose(
                            context,
                            runtime.ServerUtc,
                            broken
                                ? "STRUCTURAL_INVALIDATION"
                                : "ADVERSE_R_EXCEEDED");
                }
    
                if (configuration.Get(
                        "EnableReversalProtectionClose",
                        true) ||
                    configuration.Get(
                        "EnableLiveStructuralReversal",
                        true))
                {
                    int evidence =
                        CountOppositeEvidence(
                            context.Direction,
                            structure);
    
                    bool structuralFlip =
                        structure != null &&
                        structure.CurrentStructureDirection ==
                            Opposite(context.Direction) &&
                        structure.StructureQuality >=
                            configuration.Get(
                                "ReversalProtectionMinimumQuality",
                                82);
    
                    bool mtfFlip =
                        market != null &&
                        market.M15 != null &&
                        market.M15.BiasDirection ==
                            Opposite(context.Direction) &&
                        market.M15.BiasStrength >=
                            configuration.Get(
                                "ReversalCloseMinimumMtf",
                                70);
    
                    bool confirmed =
                        evidence >=
                            configuration.Get(
                                "ReversalCloseMinimumEvidence",
                                5) &&
                        (structuralFlip || mtfFlip);
    
                    bool fast =
                        configuration.Get(
                            "EnableFastReversalIntelligence",
                            true) &&
                        evidence >=
                            configuration.Get(
                                "LiveReversalMinimumEvidence",
                                3) &&
                        structure != null &&
                        structure.StructureQuality >=
                            configuration.Get(
                                "FastReversalMinimumQuality",
                                74) &&
                        (!configuration.Get(
                            "AllowFastM5ReversalBeforeM15",
                            true) ||
                         mtfFlip);
    
                    if (confirmed || fast)
                        return NewClose(
                            context,
                            runtime.ServerUtc,
                            confirmed
                                ? "REVERSAL_PROTECTION"
                                : "FAST_REVERSAL");
                }
    
                if (configuration.Get(
                        "EnableProfitExhaustionProtection",
                        true) &&
                    context.PeakRR >=
                        Math.Max(
                            0,
                            configuration.Get(
                                "ExhaustionMinimumPeakRR",
                                1.50)))
                {
                    double retracement =
                        context.PeakRR > 0
                            ? Math.Max(
                                0,
                                (context.PeakRR - currentRR) /
                                context.PeakRR *
                                100.0)
                            : 0;
    
                    int pressure =
                        ExitPressure(
                            context.Direction,
                            runtime,
                            market,
                            structure);
    
                    int evidence =
                        CountOppositeEvidence(
                            context.Direction,
                            structure);
    
                    if (retracement >=
                            configuration.Get(
                                "ExhaustionRetracementPercent",
                                35) &&
                        pressure >=
                            configuration.Get(
                                "ExhaustionPressureThreshold",
                                78) &&
                        evidence >=
                            configuration.Get(
                                "ExhaustionMinimumOppositeEvidence",
                                3))
                        return NewClose(
                            context,
                            runtime.ServerUtc,
                            "PROFIT_EXHAUSTION");
                }
    
                return null;
            }
    
            private LivePositionAction EvaluatePartialTakeProfit(
                LivePositionContext context,
                BrokerPositionSnapshot snapshot,
                RuntimeSnapshot runtime,
                TradePlan plan,
                ConfigSnapshot configuration)
            {
                if (!configuration.Get(
                        "EnablePartialTakeProfit",
                        false) ||
                    context.PartialPending)
                    return null;
    
                TargetStage stage =
                    TargetStage.TP1;
    
                double percent = 0;
    
                double? tp1 =
                    GetManagedTargetPrice(
                        context,
                        plan,
                        TargetStage.TP1);
    
                double? tp2 =
                    GetManagedTargetPrice(
                        context,
                        plan,
                        TargetStage.TP2);
    
                if (!context.Tp1Consumed &&
                    HasReached(
                        context.Direction,
                        runtime,
                        tp1))
                {
                    stage = TargetStage.TP1;
                    percent =
                        configuration.Get(
                            "PartialCloseTp1Percent",
                            33);
                }
                else if (!context.Tp2Consumed &&
                         HasReached(
                             context.Direction,
                             runtime,
                             tp2))
                {
                    stage = TargetStage.TP2;
                    percent =
                        configuration.Get(
                            "PartialCloseTp2Percent",
                            33);
                }
                else
                {
                    return null;
                }
    
                if (runtime.ServerUtc <
                    context.NextPartialRetryUtc)
                    return null;
    
                percent =
                    Math.Max(
                        0,
                        Math.Min(
                            90,
                            percent));
    
                if (percent <= 0)
                    return null;
    
                double requested =
                    NormalizePartialVolume(
                        snapshot.VolumeInUnits * percent / 100.0,
                        snapshot.VolumeInUnits,
                        runtime);
    
                if (requested <= 0)
                    return null;
    
                double remaining =
                    snapshot.VolumeInUnits -
                    requested;
    
                if (remaining <
                    runtime.BrokerConstraints.MinVolumeInUnits)
                    return null;
    
                return new LivePositionAction(
                    context.BrokerPositionId,
                    LivePositionActionKind.PartialClose,
                    null,
                    null,
                    requested,
                    stage,
                    stage == TargetStage.TP1
                        ? "PARTIAL_TP1"
                        : "PARTIAL_TP2",
                    runtime.ServerUtc);
            }
    
            private LivePositionAction EvaluateProtection(
                LivePositionContext context,
                BrokerPositionSnapshot snapshot,
                RuntimeSnapshot runtime,
                TradePlan plan,
                MarketModel market,
                StructureSnapshot structure,
                ConfigSnapshot configuration,
                double currentRR)
            {
                if (context.ProtectionPending)
                    return null;
    
                double? desiredStop =
                    snapshot.StopLoss;
    
                double? desiredTarget =
                    snapshot.TakeProfit;
    
                // A matching live Plan is the authoritative source for restoring
                // protection lost across restart/reconnection.
                if (plan != null &&
                    plan.Identity != null &&
                    string.Equals(
                        context.PlanId,
                        plan.Identity.PlanId,
                        StringComparison.Ordinal))
                {
                    if (!desiredStop.HasValue &&
                        plan.StructuralStop != null)
                        desiredStop = plan.StructuralStop.Price;
    
                    if (!desiredTarget.HasValue)
                        desiredTarget =
                            GetManagedTargetPrice(
                                context,
                                plan,
                                context.ActiveTargetStage);
                }
    
                // Best protective stop wins; no action may loosen SL.
                if (configuration.Get(
                        "MoveSlToBreakEven",
                        true) &&
                    currentRR >=
                        configuration.Get(
                            "BreakEvenTriggerRR",
                            0.90))
                {
                    double extraPips =
                        Math.Max(
                            0,
                            configuration.Get(
                                "BreakEvenBufferPips",
                                0.5));
    
                    if (configuration.Get(
                            "UseSpreadAwareBreakEven",
                            true))
                        extraPips +=
                            runtime.SpreadPips;
    
                    extraPips +=
                        Math.Max(
                            0,
                            configuration.Get(
                                "RiskFreeLockPips",
                                0.5));
    
                    double candidate =
                        context.Direction ==
                            Direction.Buy
                            ? context.InitialEntryPrice +
                              extraPips * runtime.PipSize
                            : context.InitialEntryPrice -
                              extraPips * runtime.PipSize;
    
                    desiredStop =
                        BetterStop(
                            context.Direction,
                            desiredStop,
                            candidate);
                }
    
                double atr =
                    market != null &&
                    market.M5 != null
                        ? market.M5.Atr
                        : 0;
    
                if (atr > 0 &&
                    configuration.Get(
                        "EnableStructuralSlRepricing",
                        true) &&
                    currentRR >=
                        configuration.Get(
                            "SlRepriceStartRR",
                            1.0))
                {
                    double structural =
                        FindStructuralStop(
                            context,
                            structure,
                            atr,
                            configuration);
    
                    desiredStop =
                        BetterStop(
                            context.Direction,
                            desiredStop,
                            structural);
                }
    
                if (atr > 0 &&
                    currentRR >=
                        configuration.Get(
                            "SmartTrailMinimumRR",
                            1.0))
                {
                    double tighten =
                        currentRR >=
                            configuration.Get(
                                "SmartTrailTightenAtRR",
                                1.8)
                            ? 0.20
                            : 0.45;
    
                    double distanceAtr =
                        Math.Max(
                            0.20,
                            configuration.Get(
                                "SlRepriceBreathingAtr",
                                0.85) -
                            tighten);
    
                    double momentum =
                        market != null &&
                        market.M5 != null
                            ? Math.Max(
                                0,
                                market.M5.MomentumAtr)
                            : 0;
    
                    distanceAtr =
                        Math.Max(
                            0.20,
                            distanceAtr -
                            Math.Min(
                                0.30,
                                momentum *
                                configuration.Get(
                                    "SmartTrailMomentumBonusAtr",
                                    0.10)));
    
                    double trail =
                        context.Direction ==
                            Direction.Buy
                            ? runtime.Bid -
                              distanceAtr * atr
                            : runtime.Ask +
                              distanceAtr * atr;
    
                    desiredStop =
                        BetterStop(
                            context.Direction,
                            desiredStop,
                            trail);
                }
    
                if (configuration.Get(
                        "MoveToBreakEvenAfterPartial",
                        true) &&
                    (context.Tp1Consumed ||
                     context.Tp2Consumed))
                {
                    double be =
                        configuration.Get(
                            "BreakEvenBufferPips",
                            0.5) *
                        runtime.PipSize;
    
                    double candidate =
                        context.Direction ==
                            Direction.Buy
                            ? context.InitialEntryPrice + be
                            : context.InitialEntryPrice - be;
    
                    desiredStop =
                        BetterStop(
                            context.Direction,
                            desiredStop,
                            candidate);
                }
    
                if (configuration.Get(
                        "EnableDynamicTpAdvance",
                        true) &&
                    configuration.Get(
                        "UpdateUnhitTargets",
                        true))
                {
                    double? activePrice =
                        GetManagedTargetPrice(
                            context,
                            plan,
                            context.ActiveTargetStage);
    
                    TargetStage nextStage =
                        NextTargetStage(
                            context.ActiveTargetStage);
    
                    double? nextPrice =
                        GetManagedTargetPrice(
                            context,
                            plan,
                            nextStage);
    
                    bool targetRepriceAllowed =
                        !configuration.Get(
                            "StructuralTargetUpdatesOnly",
                            true) ||
                        structure != null &&
                        structure.IsCoherent &&
                        structure.IsPrimaryReady &&
                        structure.ReferenceUtc !=
                            context.LastTargetRepriceReferenceUtc &&
                        HasCurrentDirectionalStructure(
                            context.Direction,
                            structure);
    
                    if (targetRepriceAllowed &&
                        activePrice.HasValue &&
                        nextPrice.HasValue &&
                        currentRR >=
                            configuration.Get(
                                "TargetUpdateTriggerRR",
                                1.20) &&
                        atr > 0 &&
                        Math.Abs(
                            nextPrice.Value -
                            activePrice.Value) >=
                            atr *
                            Math.Max(
                                0.05,
                                configuration.Get(
                                    "TargetUpdateStepAtr",
                                    0.20)) &&
                        IsNearTarget(
                            context,
                            runtime,
                            activePrice.Value,
                            configuration.Get(
                                "TpAdvanceProximityPercent",
                                72)))
                    {
                        desiredTarget =
                            BetterTarget(
                                context.Direction,
                                desiredTarget,
                                nextPrice.Value);
    
                        context.PendingProtectionStage =
                            nextStage;
                    }
                }
    
                // Partial TP requires the broker TP to be no earlier than TP2.
                if (configuration.Get(
                        "EnablePartialTakeProfit",
                        false))
                {
                    double? tp2Price =
                        GetManagedTargetPrice(
                            context,
                            plan,
                            TargetStage.TP2);
    
                    if (tp2Price.HasValue &&
                        (!desiredTarget.HasValue ||
                         IsTargetBehind(
                             context.Direction,
                             desiredTarget.Value,
                             tp2Price.Value)))
                    {
                        desiredTarget =
                            BetterTarget(
                                context.Direction,
                                desiredTarget,
                                tp2Price.Value);
    
                        context.PendingProtectionStage =
                            TargetStage.TP2;
                    }
                }
    
                if (desiredStop.HasValue &&
                    !IsBrokerStopPriceValid(
                        context.Direction,
                        desiredStop.Value,
                        runtime))
                    desiredStop = snapshot.StopLoss;
    
                if (desiredTarget.HasValue &&
                    !IsBrokerTargetPriceValid(
                        context.Direction,
                        desiredTarget.Value,
                        runtime))
                    desiredTarget = snapshot.TakeProfit;
    
                if (NeedsStopUpdate(
                        context.Direction,
                        snapshot.StopLoss,
                        desiredStop,
                        runtime.PipSize) ||
                    NeedsTargetUpdate(
                        context.Direction,
                        snapshot.TakeProfit,
                        desiredTarget,
                        runtime.PipSize))
                {
                    return new LivePositionAction(
                        context.BrokerPositionId,
                        LivePositionActionKind.ProtectionUpdate,
                        desiredStop,
                        desiredTarget,
                        0,
                        context.ActiveTargetStage,
                        "SMART_PROTECTION_AND_TARGET",
                        runtime.ServerUtc);
                }
    
                return null;
            }
    
            private double FindStructuralStop(
                LivePositionContext context,
                StructureSnapshot structure,
                double atr,
                ConfigSnapshot configuration)
            {
                if (structure == null ||
                    structure.Events == null)
                    return 0;
    
                StructureEventRecord best = null;
    
                for (int i = 0; i < structure.Events.Count; i++)
                {
                    StructureEventRecord item =
                        structure.Events[i];
    
                    if (item == null ||
                        item.TimeUtc > structure.ReferenceUtc)
                        continue;
    
                    bool match =
                        context.Direction ==
                            Direction.Buy
                            ? item.Kind ==
                                StructureEventKind.SwingLow
                            : item.Kind ==
                                StructureEventKind.SwingHigh;
    
                    if (!match)
                        continue;
    
                    if (best == null ||
                        item.TimeUtc > best.TimeUtc)
                        best = item;
                }
    
                if (best == null)
                    return 0;
    
                double breathing =
                    Math.Max(
                        0.20,
                        configuration.Get(
                            "SlRepriceBreathingAtr",
                            0.85));
    
                return
                    context.Direction ==
                        Direction.Buy
                        ? best.Price - breathing * atr
                        : best.Price + breathing * atr;
            }
    
            private bool HasCurrentDirectionalStructure(
                Direction direction,
                StructureSnapshot structure)
            {
                if (structure == null ||
                    !structure.IsCoherent ||
                    !structure.IsPrimaryReady ||
                    structure.Events == null)
                    return false;
    
                for (int i = 0; i < structure.Events.Count; i++)
                {
                    StructureEventRecord item =
                        structure.Events[i];
    
                    if (item != null &&
                        item.Direction == direction &&
                        item.TimeUtc <= structure.ReferenceUtc &&
                        (item.Kind ==
                            StructureEventKind.BreakOfStructure ||
                         item.Kind ==
                            StructureEventKind.MarketStructureShift ||
                         item.Kind ==
                            StructureEventKind.Displacement))
                        return true;
                }
    
                return false;
            }
    
            private int CountOppositeEvidence(
                Direction direction,
                StructureSnapshot structure)
            {
                if (structure == null ||
                    structure.Events == null)
                    return 0;
    
                Direction opposite =
                    Opposite(direction);
    
                int count = 0;
    
                for (int i = 0; i < structure.Events.Count; i++)
                {
                    StructureEventRecord item =
                        structure.Events[i];
    
                    if (item == null ||
                        item.Direction != opposite)
                        continue;
    
                    if (item.Kind ==
                            StructureEventKind.BreakOfStructure ||
                        item.Kind ==
                            StructureEventKind.MarketStructureShift ||
                        item.Kind ==
                            StructureEventKind.ChangeOfCharacter ||
                        item.Kind ==
                            StructureEventKind.Displacement ||
                        item.Kind ==
                            StructureEventKind.LiquiditySweep)
                        count++;
                }
    
                return count;
            }
    
            private int ExitPressure(
                Direction direction,
                RuntimeSnapshot runtime,
                MarketModel market,
                StructureSnapshot structure)
            {
                int score = 0;
                Direction opposite =
                    Opposite(direction);
    
                if (market != null &&
                    market.M5 != null &&
                    market.M5.BiasDirection == opposite)
                    score += 25;
    
                if (market != null &&
                    market.M15 != null &&
                    market.M15.BiasDirection == opposite)
                    score += 30;
    
                if (structure != null &&
                    structure.CurrentStructureDirection == opposite)
                    score += 30;
    
                if (market != null &&
                    market.M5 != null)
                {
                    if (direction == Direction.Buy &&
                        market.M5.MomentumAtr < 0)
                        score += 10;
    
                    if (direction == Direction.Sell &&
                        market.M5.MomentumAtr > 0)
                        score += 10;
    
                    if (direction == Direction.Buy &&
                        market.M5.Rsi < 45)
                        score += 5;
    
                    if (direction == Direction.Sell &&
                        market.M5.Rsi > 55)
                        score += 5;
                }
    
                return Math.Max(
                    0,
                    Math.Min(
                        100,
                        score));
            }
    
            private double CalculateRR(
                LivePositionContext context,
                double mark)
            {
                if (context.InitialRiskPrice <= 0)
                    return 0;
    
                double favorable =
                    context.Direction ==
                        Direction.Buy
                        ? mark - context.InitialEntryPrice
                        : context.InitialEntryPrice - mark;
    
                return favorable /
                       context.InitialRiskPrice;
            }
    
            private LivePlanSnapshot BuildPlanSnapshot(
                TradePlan plan)
            {
                if (plan == null ||
                    plan.Identity == null ||
                    plan.TargetLadder == null)
                    return null;
    
                return new LivePlanSnapshot
                {
                    PlanId = plan.Identity.PlanId,
                    Direction = plan.Direction,
                    EntryPrice =
                        plan.ExecutionAnchor != null
                            ? plan.ExecutionAnchor.Price
                            : 0,
                    StructuralStopPrice =
                        plan.StructuralStop != null
                            ? plan.StructuralStop.Price
                            : 0,
                    InvalidationPrice =
                        plan.Entry != null &&
                        plan.Entry.Invalidation != null
                            ? plan.Entry.Invalidation.Price
                            : 0,
                    Tp1Price = StoredPlanTarget(
                        plan,
                        TargetStage.TP1),
                    Tp2Price = StoredPlanTarget(
                        plan,
                        TargetStage.TP2),
                    Tp3Price = StoredPlanTarget(
                        plan,
                        TargetStage.TP3),
                    Tp4Price = StoredPlanTarget(
                        plan,
                        TargetStage.TP4)
                };
            }
    
            private double? StoredPlanTarget(
                TradePlan plan,
                TargetStage stage)
            {
                if (plan == null ||
                    plan.TargetLadder == null)
                    return null;
    
                TargetLevel level =
                    plan.TargetLadder.Find(stage);
    
                return level != null &&
                       level.Level != null
                    ? (double?)level.Level.Price
                    : null;
            }
    
            private double? GetManagedTargetPrice(
                LivePositionContext context,
                TradePlan plan,
                TargetStage stage)
            {
                if (context == null)
                    return null;
    
                if (plan != null &&
                    plan.Identity != null &&
                    context.PlanSnapshot != null &&
                    string.Equals(
                        context.PlanId,
                        plan.Identity.PlanId,
                        StringComparison.Ordinal))
                {
                    double? current =
                        GetTargetPrice(
                            plan,
                            stage);
    
                    if (current.HasValue)
                        return current;
                }
    
                if (context.PlanSnapshot == null)
                    return null;
    
                switch (stage)
                {
                    case TargetStage.TP1:
                        return context.PlanSnapshot.Tp1Price;
                    case TargetStage.TP2:
                        return context.PlanSnapshot.Tp2Price;
                    case TargetStage.TP3:
                        return context.PlanSnapshot.Tp3Price;
                    case TargetStage.TP4:
                        return context.PlanSnapshot.Tp4Price;
                    default:
                        return null;
                }
            }
    
            private TargetStage NextTargetStage(
                TargetStage stage)
            {
                int next = (int)stage + 1;
                return
                    next > (int)TargetStage.TP4
                        ? stage
                        : (TargetStage)next;
            }
    
            private double? GetTargetPrice(
                TradePlan plan,
                TargetStage stage)
            {
                if (plan == null ||
                    plan.TargetLadder == null)
                    return null;
    
                TargetLevel level =
                    plan.TargetLadder.Find(stage);
    
                return level != null &&
                       level.Level != null
                    ? (double?)level.Level.Price
                    : null;
            }
    
            private TargetStage FindStageForTarget(
                TradePlan plan,
                double? targetPrice)
            {
                if (plan == null ||
                    plan.TargetLadder == null ||
                    !targetPrice.HasValue)
                    return TargetStage.TP1;
    
                double best = double.MaxValue;
                TargetStage stage =
                    TargetStage.TP1;
    
                for (int i = 0;
                     i < plan.TargetLadder.Levels.Count;
                     i++)
                {
                    TargetLevel item =
                        plan.TargetLadder.Levels[i];
    
                    if (item == null ||
                        item.Level == null)
                        continue;
    
                    double distance =
                        Math.Abs(
                            item.Level.Price -
                            targetPrice.Value);
    
                    if (distance < best)
                    {
                        best = distance;
                        stage = item.Stage;
                    }
                }
    
                return stage;
            }
    
            private TargetLevel NextTarget(
                TradePlan plan,
                TargetStage stage)
            {
                int next =
                    (int)stage + 1;
    
                return
                    plan == null ||
                    plan.TargetLadder == null ||
                    next > (int)TargetStage.TP4
                        ? null
                        : plan.TargetLadder.Find(
                            (TargetStage)next);
            }
    
            private bool HasReached(
                Direction direction,
                RuntimeSnapshot runtime,
                double? target)
            {
                if (!target.HasValue)
                    return false;
    
                return
                    direction == Direction.Buy
                        ? runtime.Bid >= target.Value
                        : runtime.Ask <= target.Value;
            }
    
            private bool IsNearTarget(
                LivePositionContext context,
                RuntimeSnapshot runtime,
                double target,
                double proximityPercent)
            {
                double fullDistance =
                    context.Direction ==
                        Direction.Buy
                        ? target - context.InitialEntryPrice
                        : context.InitialEntryPrice - target;
    
                double progress =
                    context.Direction ==
                        Direction.Buy
                        ? runtime.Bid - context.InitialEntryPrice
                        : context.InitialEntryPrice - runtime.Ask;
    
                return
                    fullDistance > 0 &&
                    progress > 0 &&
                    progress / fullDistance *
                        100.0 >=
                        Math.Max(
                            50,
                            Math.Min(
                                98,
                                proximityPercent));
            }
    
            private bool IsTargetBehind(
                Direction direction,
                double current,
                double desired)
            {
                return
                    direction == Direction.Buy
                        ? current < desired
                        : current > desired;
            }
    
            private bool NeedsStopUpdate(
                Direction direction,
                double? current,
                double? desired,
                double pipSize)
            {
                if (!desired.HasValue ||
                    pipSize <= 0)
                    return false;
    
                if (!current.HasValue)
                    return true;
    
                double epsilon =
                    pipSize * 0.25;
    
                return
                    direction == Direction.Buy
                        ? desired.Value >
                            current.Value + epsilon
                        : desired.Value <
                            current.Value - epsilon;
            }
    
            private bool NeedsTargetUpdate(
                Direction direction,
                double? current,
                double? desired,
                double pipSize)
            {
                if (!desired.HasValue ||
                    pipSize <= 0)
                    return false;
    
                if (!current.HasValue)
                    return true;
    
                double epsilon =
                    pipSize * 0.25;
    
                return
                    direction == Direction.Buy
                        ? desired.Value >
                            current.Value + epsilon
                        : desired.Value <
                            current.Value - epsilon;
            }
    
            private bool IsBrokerStopPriceValid(
                Direction direction,
                double price,
                RuntimeSnapshot runtime)
            {
                if (runtime == null ||
                    runtime.PipSize <= 0 ||
                    price <= 0)
                    return false;
    
                double minimum =
                    runtime.BrokerConstraints != null
                        ? runtime.BrokerConstraints.MinStopDistancePips *
                          runtime.PipSize
                        : 0;
    
                return
                    direction == Direction.Buy
                        ? price <
                            runtime.Bid - minimum
                        : price >
                            runtime.Ask + minimum;
            }
    
            private bool IsBrokerTargetPriceValid(
                Direction direction,
                double price,
                RuntimeSnapshot runtime)
            {
                if (runtime == null ||
                    runtime.PipSize <= 0 ||
                    price <= 0)
                    return false;
    
                double minimum =
                    runtime.BrokerConstraints != null
                        ? runtime.BrokerConstraints.MinTakeProfitDistancePips *
                          runtime.PipSize
                        : 0;
    
                return
                    direction == Direction.Buy
                        ? price >
                            runtime.Ask + minimum
                        : price <
                            runtime.Bid - minimum;
            }
    
            private double? BetterStop(
                Direction direction,
                double? current,
                double candidate)
            {
                if (candidate <= 0)
                    return current;
    
                if (!current.HasValue)
                    return candidate;
    
                return
                    direction == Direction.Buy
                        ? Math.Max(current.Value, candidate)
                        : Math.Min(current.Value, candidate);
            }
    
            private double? BetterTarget(
                Direction direction,
                double? current,
                double candidate)
            {
                if (candidate <= 0)
                    return current;
    
                if (!current.HasValue)
                    return candidate;
    
                return
                    direction == Direction.Buy
                        ? Math.Max(current.Value, candidate)
                        : Math.Min(current.Value, candidate);
            }
    
            private double NormalizePartialVolume(
                double requested,
                double current,
                RuntimeSnapshot runtime)
            {
                if (requested <= 0 ||
                    current <= 0 ||
                    runtime.BrokerConstraints == null)
                    return 0;
    
                double step =
                    runtime.BrokerConstraints.VolumeStepInUnits;
    
                if (step <= 0)
                    return 0;
    
                double normalized =
                    Math.Floor(
                        requested / step) *
                    step;
    
                if (normalized <
                    runtime.BrokerConstraints.MinVolumeInUnits ||
                    normalized >= current)
                    return 0;
    
                return normalized;
            }
    
            private void ConfirmPartialProgress(
                LivePositionContext context,
                BrokerPositionSnapshot snapshot,
                DateTime utc)
            {
                if (!context.PartialPending ||
                    snapshot == null)
                    return;
    
                if (snapshot.VolumeInUnits <=
                    context.PendingPartialExpectedRemainingVolume +
                    Math.Max(
                        runtimeVolumeTolerance(snapshot),
                        snapshot.VolumeInUnits *
                        0.001))
                {
                    if (context.PendingPartialStage ==
                        TargetStage.TP1)
                        context.Tp1Consumed = true;
    
                    if (context.PendingPartialStage ==
                        TargetStage.TP2)
                        context.Tp2Consumed = true;
    
                    context.PartialPending = false;
                    context.PendingPartialRequestedVolume = 0;
                    context.PendingPartialExpectedRemainingVolume = 0;
                    context.PartialAcceptedUtc = DateTime.MinValue;
                    context.ManagementRecoveryRequired = false;
                    return;
                }
    
                if (context.PartialAcceptedUtc != DateTime.MinValue &&
                    context.PartialAcceptedUtc.AddSeconds(15) < utc)
                {
                    context.PartialPending = false;
                    context.PartialRejectAttempts++;
                    context.NextPartialRetryUtc =
                        utc +
                        RetryDelay(
                            context.PartialRejectAttempts);
                    context.ManagementRecoveryRequired = true;
                    context.PartialAcceptedUtc = DateTime.MinValue;
                }
            }
    
            private double runtimeVolumeTolerance(
                BrokerPositionSnapshot snapshot)
            {
                return
                    snapshot != null
                        ? Math.Max(
                            0,
                            snapshot.VolumeInUnits *
                            0.0001)
                        : 0;
            }
    
            private bool ProtectionObserved(
                LivePositionContext context,
                BrokerPositionSnapshot snapshot)
            {
                if (context == null ||
                    snapshot == null ||
                    !context.ProtectionPending)
                    return false;
    
                bool stopOk =
                    !context.PendingStopLoss.HasValue ||
                    snapshot.StopLoss.HasValue &&
                    Math.Abs(
                        snapshot.StopLoss.Value -
                        context.PendingStopLoss.Value) <=
                        Math.Max(
                            0.00000001,
                            Math.Abs(
                                context.PendingStopLoss.Value) *
                            0.000001);
    
                bool targetOk =
                    !context.PendingTakeProfit.HasValue ||
                    snapshot.TakeProfit.HasValue &&
                    Math.Abs(
                        snapshot.TakeProfit.Value -
                        context.PendingTakeProfit.Value) <=
                        Math.Max(
                            0.00000001,
                            Math.Abs(
                                context.PendingTakeProfit.Value) *
                            0.000001);
    
                return stopOk && targetOk;
            }
    
            private bool PlanMatches(
                LivePositionContext context,
                TradePlan plan)
            {
                if (context == null ||
                    plan == null ||
                    plan.Identity == null ||
                    string.IsNullOrWhiteSpace(context.PlanId))
                    return false;
    
                return
                    plan.Direction == context.Direction &&
                    string.Equals(
                        context.PlanId,
                        plan.Identity.PlanId,
                        StringComparison.Ordinal);
            }
    
            private LivePositionAction NewClose(
                LivePositionContext context,
                DateTime utc,
                string reason)
            {
                return new LivePositionAction(
                    context.BrokerPositionId,
                    LivePositionActionKind.Close,
                    null,
                    null,
                    0,
                    context.ActiveTargetStage,
                    reason,
                    utc);
            }
    
            private void Queue(
                LivePositionAction action)
            {
                if (action == null)
                    return;
    
                string key =
                    action.BrokerPositionId +
                    "|" +
                    action.Kind.ToString() +
                    "|" +
                    action.TargetStage.ToString();
    
                if (_actionKeys.Add(key))
                    _actions.Add(action);
            }
    
            private static Direction Opposite(
                Direction direction)
            {
                return
                    direction == Direction.Buy
                        ? Direction.Sell
                        : direction == Direction.Sell
                            ? Direction.Buy
                            : Direction.Wait;
            }
    
            private static TimeSpan RetryDelay(int attempts)
            {
                int normalized =
                    Math.Max(1, attempts);
    
                int exponent =
                    Math.Min(6, normalized - 1);
    
                int seconds = 2;
    
                for (int i = 0; i < exponent; i++)
                    seconds =
                        Math.Min(
                            60,
                            seconds * 2);
    
                return TimeSpan.FromSeconds(seconds);
            }
        }
    
    
}
