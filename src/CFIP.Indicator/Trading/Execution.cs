using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        // ============================================================
        
                private bool HasTradingPermission()
                {
                    try
                    {
                        return Permissions.TradingPermission.IsAllowed;
                    }
                    catch
                    {
                        return false;
                    }
                }
        
                private bool EnsureTradingPermission()
                {
                    if (HasTradingPermission())
                    {
                        _lastTradingPermissionRequestUtc =
                            DateTime.MinValue;
                        return true;
                    }
        
                    DateTime nowUtc =
                        TimeInUtc;
        
                    if ((nowUtc -
                         _lastTradingPermissionRequestUtc).TotalSeconds < 3)
                        return false;
        
                    _lastTradingPermissionRequestUtc =
                        nowUtc;
        
                    try
                    {
                        bool granted =
                            Permissions.TradingPermission.Request();
        
                        return
                            granted &&
                            HasTradingPermission();
                    }
                    catch (Exception ex)
                    {
                        Print(
                            "CFIP TradingPermission request failed: {0}",
                            ex.Message);
                        return false;
                    }
                }
        
                private bool IsManagedPosition(Position position)
                {
                    if (position == null ||
                        position.SymbolName != SymbolName)
                        return false;
        
                    if (!ManagedActionsOnly)
                        return true;
        
                    string managedLabel =
                        string.IsNullOrWhiteSpace(
                            ManagedPositionLabel)
                            ? NormalizeLabel()
                            : ManagedPositionLabel.Trim();
        
                    return string.Equals(
                        position.Label,
                        managedLabel,
                        StringComparison.Ordinal);
                }
        
                private bool IsManagedPendingOrder(PendingOrder order)
                {
                    return
                        order != null &&
                        order.SymbolName == SymbolName &&
                        string.Equals(
                            order.Label,
                            PendingOrderLabel(),
                            StringComparison.Ordinal);
                }
        
                private string PendingOrderLabel()
                {
                    return NormalizeLabel() + "-PENDING";
                }
        
                private int ManagedPendingOrderCount()
                {
                    int count=0;
        
                    foreach (PendingOrder order in PendingOrders)
                    {
                        if (IsManagedPendingOrder(order))
                            count++;
                    }
        
                    return count;
                }
        
                private void EnrichLivePlanTargets(int closedM5)
                {
                    if (_plan == null ||
                        !_plan.IsLivePosition ||
                        _m5Bars == null)
                        return;
        
                    int index =
                        Math.Max(
                            1,
                            Math.Min(
                                closedM5,
                                _m5Bars.Count - 2));
        
                    double atr =
                        Atr(
                            _m5Bars,
                            index);
        
                    if (atr <= 0)
                        return;
        
                    List<Level> levels =
                        BuildTargetLevels(
                            index,
                            _plan.Direction,
                            _plan.Entry,
                            atr);
        
                    List<Level> selected =
                        SelectTargets(
                            levels,
                            index,
                            _plan.Entry,
                            Math.Max(
                                Symbol.PipSize,
                                _plan.Risk),
                            _plan.Direction,
                            atr);
        
                    double baseTarget = _plan.Tp1;
        
                    _plan.Tp2 =
                        FindFurtherLiveTarget(
                            selected,
                            index,
                            baseTarget,
                            atr);
        
                    double base2 =
                        _plan.Tp2 > 0
                            ? _plan.Tp2
                            : baseTarget;
        
                    _plan.Tp3 =
                        FindFurtherLiveTarget(
                            selected,
                            index,
                            base2,
                            atr);
        
                    double base3 =
                        _plan.Tp3 > 0
                            ? _plan.Tp3
                            : base2;
        
                    _plan.Tp4 =
                        FindFurtherLiveTarget(
                            selected,
                            index,
                            base3,
                            atr);
        
                    ApplyTargetMeta(
                        levels,
                        _plan.Tp2,
                        atr,
                        out _plan.Tp2Source,
                        out _plan.Tp2Quality);
        
                    ApplyTargetMeta(
                        levels,
                        _plan.Tp3,
                        atr,
                        out _plan.Tp3Source,
                        out _plan.Tp3Quality);
        
                    ApplyTargetMeta(
                        levels,
                        _plan.Tp4,
                        atr,
                        out _plan.Tp4Source,
                        out _plan.Tp4Quality);
        
                    _plan.HtfTargetCount =
                        CountHtfTargetsInPlan(
                            _plan);
        
                    RecalculatePlanRR();
                }
        
                private double FindFurtherLiveTarget(
                    List<Level> selected,
                    int index,
                    double previous,
                    double atr)
                {
                    if (selected == null ||
                        !IsFinitePositive(previous) ||
                        _plan == null)
                        return 0;
        
                    double best = 0;
                    double bestScore = double.MinValue;
        
                    foreach (Level level in selected)
                    {
                        if (level == null ||
                            !IsFinitePositive(level.Price) ||
                            level.Score < SmartTargetQuality)
                            continue;
        
                        bool farther =
                            _plan.Direction == 1
                                ? level.Price > previous + Symbol.PipSize
                                : level.Price < previous - Symbol.PipSize;
        
                        if (!farther)
                            continue;
        
                        double rr =
                            Math.Abs(
                                level.Price -
                                _plan.Entry) /
                            Math.Max(
                                Symbol.PipSize,
                                _plan.Risk);
        
                        if (rr >
                            Math.Max(
                                0,
                                MaximumRewardRR))
                            continue;
        
                        if (RejectTargetObstacle &&
                            HasTargetObstacle(
                                _m5Bars,
                                index,
                                _plan.Direction,
                                _plan.Entry,
                                level.Price,
                                atr))
                            continue;
        
                        double score =
                            level.Score +
                            (IsHtfTimeframe(level.Timeframe)
                                ? HtfRewardBonus
                                : 0);
        
                        if (score > bestScore)
                        {
                            bestScore = score;
                            best = NormalizePrice(level.Price);
                        }
                    }
        
                    return best;
                }
        
                private Plan CreateManagedPlanFromExecution(
                    int direction,
                    double entry,
                    double stop,
                    double target,
                    int createdM5,
                    double volume,
                    ExecutionMode entryMode =
                        ExecutionMode.BreakoutMarket)
                {
                    double risk =
                        Math.Abs(
                            entry -
                            stop);
        
                    return new Plan
                    {
                        Direction = direction,
                        EntryMode = entryMode,
                        Entry = NormalizePrice(entry),
                        IdealEntry = NormalizePrice(entry),
                        Stop = NormalizePrice(stop),
                        Tp1 = NormalizePrice(target),
                        Tp2 = 0,
                        Tp3 = 0,
                        Tp4 = 0,
                        Risk = Math.Max(
                            Symbol.PipSize,
                            risk),
                        Tp1RR =
                            risk > 0
                                ? Math.Abs(
                                    target - entry) /
                                  risk
                                : 0,
                        StopSource = "LIVE / STRUCTURAL",
                        StopQuality = 100,
                        Tp1Source = "LIVE / ADAPTIVE",
                        Tp1Quality = 100,
                        CreatedM5 = createdM5,
                        OriginalVolume = volume,
                        IsLivePosition = true
                    };
                }
        
                        private void RecoverManagedLivePlan(int closedM5)
                {
                    if (_plan != null &&
                        _plan.IsLivePosition)
                        return;
        
                    foreach (Position position in Positions)
                    {
                        if (!IsManagedPosition(position))
                            continue;
        
                        int direction =
                            position.TradeType == TradeType.Buy
                                ? 1
                                : -1;
        
                        double entry =
                            position.EntryPrice;
        
                        double atr =
                            _m5Bars == null
                                ? 0
                                : Atr(
                                    _m5Bars,
                                    Math.Max(
                                        1,
                                        closedM5));
        
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
                            position.StopLoss.HasValue &&
                            IsValidStop(
                                direction,
                                entry,
                                position.StopLoss.Value)
                                ? NormalizePrice(
                                    position.StopLoss.Value)
                                : 0;
        
                        double target =
                            position.TakeProfit.HasValue &&
                            IsValidTarget(
                                direction,
                                entry,
                                position.TakeProfit.Value)
                                ? NormalizePrice(
                                    position.TakeProfit.Value)
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
                                    Math.Max(1, closedM5),
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
                                    Math.Max(1, closedM5),
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
        
                                double distance =
                                    risk *
                                    Math.Max(
                                        1.0,
                                        MinimumRequiredRR());
        
                                target =
                                    direction == 1
                                        ? entry + distance
                                        : entry - distance;
        
                                target =
                                    NormalizePrice(target);
                            }
                        }
        
                        if (!IsExecutionPlanConsistent(
                                direction,
                                entry,
                                stop,
                                target))
                        {
                            _brokerProtectionRecoveryRequired = true;
        
                            SetLifecycleState(
                                LifecycleState.RecoveryRequired,
                                "STARTUP RECOVERY FAILED");
                            continue;
                        }
        
                        _plan =
                            CreateManagedPlanFromExecution(
                                direction,
                                entry,
                                stop,
                                target,
                                Math.Max(1, closedM5),
                                position.VolumeInUnits);
        
                        _plan.PositionId =
                            position.Id;
        
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
                            protectionMissing;
        
                        SetLifecycleState(
                            protectionMissing
                                ? LifecycleState.RecoveryRequired
                                : LifecycleState.LivePosition,
                            protectionMissing
                                ? "STARTUP RECOVERY • BROKER PROTECTION MISSING"
                                : "STARTUP RECOVERY • LIVE");
        
                        EnrichLivePlanTargets(closedM5);
        
                        if (AutoProtectBrokerPositions ||
                            AutoBrokerProtection)
                        {
                            bool protectionOk =
                                EnsureBrokerProtectionForPosition(
                                    position,
                                    stop,
                                    target,
                                    "STARTUP RECOVERY",
                                    direction);
        
                            if (protectionOk)
                            {
                                _activeBrokerStop = stop;
                                _activeBrokerTarget = target;
                                _brokerProtectionRecoveryRequired = false;
        
                                SetLifecycleState(
                                    LifecycleState.LivePosition,
                                    "STARTUP RECOVERY • PROTECTED");
                            }
                        }
        
                        _lastMarket =
                            direction == 1
                                ? Symbol.Bid
                                : Symbol.Ask;
        
                        _peakPrice =
                            _lastMarket;
        
                        break;
                    }
                }
        
                private void CheckAutoTradingDisabledReminder(int closedM5)
                {
                    if (_decision == null ||
                        _decision.Direction == 0 ||
                        !_decision.EntryAllowed ||
                        _lastAutoTradingReminderM5 == closedM5)
                        return;
        
                    int confidenceFloor =
                        Math.Max(
                            MinimumAutoConfidence,
                            HighConfidenceThreshold);
        
                    if (_decision.Confidence < confidenceFloor ||
                        _decision.SmartQuality < MinimumAutoSmartQuality ||
                        _decision.IndependentEvidence < MinimumIndependentEvidence)
                        return;
        
                    _lastAutoTradingReminderM5 =
                        closedM5;
        
                    SetAutoTradingState(
                        "OFF",
                        "HIGH-CONFIDENCE SIGNAL READY");
        
                    ShowPopup(
                        "CFIP SMART\n" +
                        (_decision.Direction == 1 ? "BUY" : "SELL") +
                        " setup is confirmed while Auto Trading is OFF.\n" +
                        "Review ENTRY / SL / TP before taking any manual action.");
        
                    SendUnifiedAlert(
                        "AUTOOFF|" + closedM5,
                        "CFIP AUTO TRADING OFF | " +
                        (_decision.Direction == 1 ? "BUY" : "SELL") +
                        " SIGNAL READY | CONF " +
                        _decision.Confidence +
                        " | SMART " +
                        _decision.SmartQuality,
                        _decision.Direction,
                        true);
                }
        
                private bool TrendContinuationStrong()
                {
                    if (_decision == null ||
                        _m5Frame == null ||
                        _m15Frame == null ||
                        _decision.Direction == 0)
                        return false;
        
                    return
                        _m5Frame.Direction == _decision.Direction &&
                        _m15Frame.Direction == _decision.Direction &&
                        _decision.StructuralConfirmations >=
                        Math.Max(
                            3,
                            MinimumStructuralConfirmations) &&
                        _decision.Confidence >=
                        PendingMinimumConfidence &&
                        _decision.SmartQuality >=
                        PendingMinimumSmartQuality &&
                        _decision.TimeframeAgreement >=
                        PendingMinimumTrendQuality;
                }
        
                private bool ReversalSetupStrong()
                {
                    if (_reaction == null ||
                        _decision == null ||
                        _reaction.Direction == 0 ||
                        _decision.Direction == 0)
                        return false;
        
                    return
                        _reaction.Direction != _decision.Direction &&
                        _reaction.EntryAllowed &&
                        _reaction.Confidence >=
                        Math.Max(
                            PendingMinimumConfidence,
                            ReversalProtectionMinimumQuality) &&
                        _reaction.IndependentEvidence >=
                        Math.Max(
                            2,
                            ReversalCloseMinimumEvidence);
                }
        
                private bool PendingModeAllowsStop()
                {
                    return
                        PendingOrderMode == PendingOrderMode.Adaptive ||
                        PendingOrderMode == PendingOrderMode.ContinuationStop ||
                        PendingOrderMode == PendingOrderMode.Both;
                }
        
                private bool PendingModeAllowsLimit()
                {
                    return
                        PendingOrderMode == PendingOrderMode.Adaptive ||
                        PendingOrderMode == PendingOrderMode.ReversalLimit ||
                        PendingOrderMode == PendingOrderMode.Both;
                }
        
                private void TrySmartPendingOrders(int closedM5)
                {
                    _lastAutoOrderAttemptUtc =
                        TimeInUtc;
        
                    if (AutomaticOrdersEnabled &&
                        DailyLossLimitHit(TimeInUtc))
                    {
                        _autoOrdersBlockReason =
                            "DAILY LOSS LIMIT";
        
                        CancelAllOrders();
                        return;
                    }
        
                    if (!AutomaticOrdersEnabled)
                    {
                        _autoOrdersBlockReason =
                            "DISABLED";
                        return;
                    }
        
                    if (GetManagedPosition() != null)
                    {
                        _autoOrdersBlockReason =
                            "MANAGED POSITION ACTIVE";
                        return;
                    }
        
                    if (_plan != null &&
                        !_plan.IsLivePosition)
                    {
                        _autoOrdersBlockReason =
                            "MARKET PLAN ACTIVE";
                        return;
                    }
        
                    CleanupPendingOrdersIfNeeded(closedM5);
        
                    PendingOrder existingPending =
                        GetManagedPendingOrder();
        
                    if (existingPending != null)
                    {
                        _autoOrdersBlockReason =
                            "PENDING ORDER EXISTS";
                        return;
                    }
        
                    if (ManagedPositionCount() >=
                        Math.Max(1, MaximumOpenPositions))
                    {
                        _autoOrdersBlockReason =
                            "MAX OPEN POSITIONS";
                        return;
                    }
        
                    if (ManagedPendingOrderCount() > 0)
                    {
                        _autoOrdersBlockReason =
                            "PENDING ORDER ALREADY EXISTS";
                        return;
                    }
        
                    if (DailyLossLimitHit(TimeInUtc))
                    {
                        _autoOrdersBlockReason =
                            "DAILY LOSS LIMIT";
                        return;
                    }
        
                    int pendingDirection =
                        TrendContinuationStrong()
                            ? _decision.Direction
                            : ReversalSetupStrong()
                                ? _reaction.Direction
                                : 0;
        
                    if (pendingDirection != 0)
                    {
                        string pendingSuitabilityReason;
        
                        if (!PassesMarketSuitability(
                                closedM5,
                                pendingDirection,
                                out pendingSuitabilityReason))
                        {
                            _autoOrdersBlockReason =
                                "SUITABILITY • " +
                                pendingSuitabilityReason;
                            return;
                        }
                    }
        
                    if (pendingDirection != 0 &&
                        !EnsureTradingPermission())
                    {
                        _autoOrdersBlockReason = "TRADING PERMISSION";
                        return;
                    }
        
                    if (TrendContinuationStrong() &&
                        PendingModeAllowsStop())
                    {
                        if (PlaceContinuationStop(closedM5))
                        {
                            _autoOrdersBlockReason =
                                "ORDER PLACED";
                            return;
                        }
                    }
        
                    if (ReversalSetupStrong() &&
                        PendingModeAllowsLimit())
                    {
                        CheckReversalProtection();
        
                        if (PlaceReversalLimit(closedM5))
                        {
                            _autoOrdersBlockReason =
                                "ORDER PLACED";
                            return;
                        }
                    }
        
                    if (pendingDirection == 0)
                    {
                        _autoOrdersBlockReason =
                            "NO ELIGIBLE PENDING SETUP";
                        return;
                    }
        
                    if (string.IsNullOrWhiteSpace(
                            _autoOrdersBlockReason) ||
                        _autoOrdersBlockReason ==
                            "NOT EVALUATED")
                    {
                        _autoOrdersBlockReason =
                            "PENDING EXECUTION BLOCKED";
                    }
                }
        
                private double EffectiveRiskStopPips(double stopPips)
                {
                    double value =
                        Math.Max(
                            0,
                            stopPips);
        
                    if (IncludeSpreadInRiskSizing)
                        value +=
                            Math.Max(
                                0,
                                (Symbol.Ask - Symbol.Bid) /
                                Math.Max(
                                    Symbol.PipSize,
                                    1e-9));
        
                    return value;
                }
        
                private bool IsValidPendingEntry(
                    int direction,
                    double price,
                    bool stopOrder)
                {
                    if (!IsFinitePositive(price))
                        return false;
        
                    double tolerance =
                        Math.Max(
                            Symbol.TickSize,
                            Symbol.PipSize * 0.10);
        
                    if (direction == 1)
                        return stopOrder
                            ? price > Symbol.Ask + tolerance
                            : price < Symbol.Ask - tolerance;
        
                    if (direction == -1)
                        return stopOrder
                            ? price < Symbol.Bid - tolerance
                            : price > Symbol.Bid + tolerance;
        
                    return false;
                }
        
                private bool PlaceContinuationStop(int closedM5)
                {
                    int direction =
                        _decision.Direction;
        
                    double atr =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    if (atr <= 0)
                    {
                        _autoOrdersBlockReason =
                            "PENDING STOP • ATR UNAVAILABLE";
                        return false;
                    }
        
                    if (_executionModel == null ||
                        _executionModel.Direction != direction ||
                        _executionModel.Mode !=
                            ExecutionMode.WaitingForTrigger)
                    {
                        _autoOrdersBlockReason =
                            "CONTINUATION STOP NOT ARMED";
                        return false;
                    }
        
                    double trigger =
                        _executionModel != null &&
                        IsFinitePositive(
                            _executionModel.Trigger)
                            ? _executionModel.Trigger
                            : direction == 1
                                ? Highest(
                                      _m5Bars,
                                      Math.Max(
                                          1,
                                          closedM5 - 6),
                                      closedM5 - 1) +
                                  atr *
                                  Math.Max(
                                      0.02,
                                      PendingEntryBufferAtr)
                                : Lowest(
                                      _m5Bars,
                                      Math.Max(
                                          1,
                                          closedM5 - 6),
                                      closedM5 - 1) -
                                  atr *
                                  Math.Max(
                                      0.02,
                                      PendingEntryBufferAtr);
        
                    trigger =
                        NormalizePrice(
                            trigger);
        
                    if (!IsValidPendingEntry(
                            direction,
                            trigger,
                            true))
                    {
                        _autoOrdersBlockReason =
                            "PENDING STOP • INVALID TRIGGER";
                        return false;
                    }
        
                    string source;
                    int quality;
        
                    double stop =
                        BuildStructuralStop(
                            closedM5,
                            direction,
                            trigger,
                            atr,
                            out source,
                            out quality);
        
                    if (!IsFinitePositive(stop))
                    {
                        _autoOrdersBlockReason =
                            "PENDING STOP • INVALID SL";
                        return false;
                    }
        
                    double target =
                        SelectStructuralAutoTarget(
                            closedM5,
                            direction,
                            trigger,
                            stop,
                            atr,
                            EffectiveAutoTpStage());
        
                    if (!IsAutoPlanValid(
                            direction,
                            trigger,
                            stop,
                            target))
                    {
                        _autoOrdersBlockReason =
                            "PENDING STOP • INVALID SL/TP";
                        return false;
                    }
        
                    double stopPips =
                        Math.Abs(
                            trigger -
                            stop) /
                        Symbol.PipSize;
        
                    double targetPips =
                        Math.Abs(
                            target -
                            trigger) /
                        Symbol.PipSize;
        
                    double volume =
                        CalculateVolume(
                            EffectiveRiskStopPips(
                                stopPips));
        
                    volume =
                        AdjustVolumeForMargin(
                            direction == 1
                                ? TradeType.Buy
                                : TradeType.Sell,
                            volume);
        
                    if (volume <
                        Symbol.VolumeInUnitsMin)
                    {
                        _autoOrdersBlockReason =
                            "PENDING STOP • VOLUME BELOW MINIMUM";
                        return false;
                    }
        
                    ExecutionIntent pendingIntent =
                        BuildExecutionIntent(
                            direction,
                            DecisionPolicyMode.Pending,
                            ExecutionIntentKind.Stop,
                            trigger,
                            trigger,
                            0,
                            0,
                            stop,
                            target,
                            volume,
                            closedM5,
                            "CONTINUATION STOP");
        
                    string intentReason;
        
                    if (!ValidateExecutionIntent(
                            pendingIntent,
                            direction == 1
                                ? Symbol.Ask
                                : Symbol.Bid,
                            out intentReason))
                    {
                        _autoOrdersBlockReason =
                            "PENDING STOP • " +
                            intentReason;
                        return false;
                    }
        
                    string reason;
        
                    if (!PassesAutoTradeSafetyGuards(
                            direction == 1
                                ? TradeType.Buy
                                : TradeType.Sell,
                            volume,
                            out reason))
                    {
                        _autoOrdersBlockReason =
                            "PENDING STOP • " +
                            reason;
                        return false;
                    }
        
                    try
                    {
                        DateTime expiration =
                            TimeInUtc.AddMinutes(
                                Math.Max(
                                    15,
                                    PendingOrderExpiryMinutes));
        
                        TradeResult result =
                            PlaceStopOrder(
                                direction == 1
                                    ? TradeType.Buy
                                    : TradeType.Sell,
                                SymbolName,
                                volume,
                                trigger,
                                PendingOrderLabel(),
                                pendingIntent.StopPips,
                                pendingIntent.TargetPips,
                                ProtectionType.Relative,
                                expiration,
                                "CFIP SMART73",
                                false);
        
                        if (result == null ||
                            !result.IsSuccessful ||
                            result.PendingOrder == null)
                        {
                            _autoOrdersBlockReason =
                                result != null &&
                                result.Error.HasValue
                                    ? result.Error.Value.ToString()
                                    : "PENDING STOP REJECTED";
        
                            return false;
                        }
        
                        _lastPendingSignalM5 =
                            closedM5;
        
                        _plan = null;
                        _executionModel = null;
                        RemovePlanObjects();
        
                        _autoOrdersBlockReason =
                            "ORDER PLACED • STOP " + Price(trigger);
        
                        SendUnifiedAlert(
                            "PENDING-STOP|" + closedM5,
                            "CFIP STOP ORDER | " +
                            (direction == 1
                                ? "BUY"
                                : "SELL") +
                            " | ENTRY " +
                            Price(trigger) +
                            " | SL " +
                            Price(stop) +
                            " | TP " +
                            Price(target),
                            direction,
                            true);
        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _autoOrdersBlockReason =
                            "PENDING STOP • " +
                            ex.Message;
        
                        Print(
                            "CFIP pending stop failed: {0}",
                            ex.Message);
        
                        return false;
                    }
                }
        
                private bool PlaceReversalLimit(int closedM5)
                {
                    int direction =
                        _reaction.Direction;
        
                    double atr =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    if (atr <= 0)
                    {
                        _autoOrdersBlockReason =
                            "PENDING LIMIT • ATR UNAVAILABLE";
                        return false;
                    }
        
                    ExecutionModel reversalModel =
                        null;
        
                    try
                    {
                        reversalModel =
                            BuildExecutionModel(
                                closedM5,
                                direction);
                    }
                    catch
                    {
                        reversalModel = null;
                    }
        
                    double targetEntry =
                        reversalModel != null &&
                        reversalModel.Direction == direction &&
                        IsFinitePositive(
                            reversalModel.IdealEntry)
                            ? reversalModel.IdealEntry
                            : direction == 1
                                ? Symbol.Bid -
                                  atr * 0.25
                                : Symbol.Ask +
                                  atr * 0.25;
        
                    targetEntry =
                        NormalizePrice(
                            targetEntry);
        
                    if (!IsValidPendingEntry(
                            direction,
                            targetEntry,
                            false))
                    {
                        _autoOrdersBlockReason =
                            "PENDING LIMIT • INVALID ENTRY";
                        return false;
                    }
        
                    string source;
                    int quality;
        
                    double stop =
                        BuildStructuralStop(
                            closedM5,
                            direction,
                            targetEntry,
                            atr,
                            out source,
                            out quality);
        
                    if (!IsFinitePositive(stop))
                        return false;
        
                    double target =
                        SelectStructuralAutoTarget(
                            closedM5,
                            direction,
                            targetEntry,
                            stop,
                            atr,
                            EffectiveAutoTpStage());
        
                    if (!IsAutoPlanValid(
                            direction,
                            targetEntry,
                            stop,
                            target))
                    {
                        _autoOrdersBlockReason =
                            "PENDING LIMIT • INVALID SL/TP";
                        return false;
                    }
        
                    double stopPips =
                        Math.Abs(
                            targetEntry -
                            stop) /
                        Symbol.PipSize;
        
                    double targetPips =
                        Math.Abs(
                            target -
                            targetEntry) /
                        Symbol.PipSize;
        
                    double volume =
                        CalculateVolume(
                            EffectiveRiskStopPips(
                                stopPips));
        
                    volume =
                        AdjustVolumeForMargin(
                            direction == 1
                                ? TradeType.Buy
                                : TradeType.Sell,
                            volume);
        
                    if (volume <
                        Symbol.VolumeInUnitsMin)
                    {
                        _autoOrdersBlockReason =
                            "PENDING LIMIT • VOLUME BELOW MINIMUM";
                        return false;
                    }
        
                    ExecutionIntent pendingIntent =
                        BuildExecutionIntent(
                            direction,
                            DecisionPolicyMode.Pending,
                            ExecutionIntentKind.Limit,
                            targetEntry,
                            0,
                            targetEntry,
                            targetEntry,
                            stop,
                            target,
                            volume,
                            closedM5,
                            "REVERSAL LIMIT");
        
                    string intentReason;
        
                    if (!ValidateExecutionIntent(
                            pendingIntent,
                            direction == 1
                                ? Symbol.Bid
                                : Symbol.Ask,
                            out intentReason))
                    {
                        _autoOrdersBlockReason =
                            "PENDING LIMIT • " +
                            intentReason;
                        return false;
                    }
        
                    string reason;
        
                    if (!PassesAutoTradeSafetyGuards(
                            direction == 1
                                ? TradeType.Buy
                                : TradeType.Sell,
                            volume,
                            out reason))
                    {
                        _autoOrdersBlockReason =
                            "PENDING LIMIT • " +
                            reason;
                        return false;
                    }
        
                    try
                    {
                        DateTime expiration =
                            TimeInUtc.AddMinutes(
                                Math.Max(
                                    15,
                                    PendingOrderExpiryMinutes));
        
                        TradeResult result =
                            PlaceLimitOrder(
                                direction == 1
                                    ? TradeType.Buy
                                    : TradeType.Sell,
                                SymbolName,
                                volume,
                                targetEntry,
                                PendingOrderLabel(),
                                pendingIntent.StopPips,
                                pendingIntent.TargetPips,
                                ProtectionType.Relative,
                                expiration,
                                "CFIP SMART73",
                                false);
        
                        if (result == null ||
                            !result.IsSuccessful ||
                            result.PendingOrder == null)
                        {
                            _autoOrdersBlockReason =
                                result != null &&
                                result.Error.HasValue
                                    ? result.Error.Value.ToString()
                                    : "PENDING LIMIT REJECTED";
        
                            return false;
                        }
        
                        _lastPendingSignalM5 =
                            closedM5;
        
                        _plan = null;
                        _executionModel = null;
                        RemovePlanObjects();
        
                        _autoOrdersBlockReason =
                            "ORDER PLACED • LIMIT " +
                            Price(targetEntry);
        
                        SendUnifiedAlert(
                            "PENDING-LIMIT|" + closedM5,
                            "CFIP LIMIT ORDER | " +
                            (direction == 1
                                ? "BUY"
                                : "SELL") +
                            " | ENTRY " +
                            Price(targetEntry) +
                            " | SL " +
                            Price(stop) +
                            " | TP " +
                            Price(target),
                            direction,
                            true);
        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _autoOrdersBlockReason =
                            "PENDING LIMIT • " +
                            ex.Message;
        
                        Print(
                            "CFIP pending limit failed: {0}",
                            ex.Message);
        
                        return false;
                    }
                }
        
                        private void CleanupPendingOrdersIfNeeded(
                    int closedM5)
                {
                    if (!PendingAutoCleanup ||
                        _lastPendingCleanupM5 == closedM5)
                        return;
        
                    _lastPendingCleanupM5 =
                        closedM5;
        
                    foreach (PendingOrder order in PendingOrders)
                    {
                        if (!IsManagedPendingOrder(order))
                            continue;
        
                        bool stale =
                            order.ExpirationTime.HasValue &&
                            order.ExpirationTime.Value <=
                            TimeInUtc;
        
                        int expectedDirection =
                            _decision != null
                                ? _decision.Direction
                                : 0;
        
                        if (order.OrderType ==
                                PendingOrderType.Limit &&
                            ReversalSetupStrong())
                        {
                            expectedDirection =
                                _reaction.Direction;
                        }
        
                        bool wrongDirection =
                            expectedDirection != 0 &&
                            ((order.TradeType == TradeType.Buy &&
                              expectedDirection != 1) ||
                             (order.TradeType == TradeType.Sell &&
                              expectedDirection != -1));
        
                        bool reversalSupersedesStop =
                            ReversalSetupStrong() &&
                            order.OrderType == PendingOrderType.Stop;
        
                        if (!stale &&
                            !wrongDirection &&
                            !reversalSupersedesStop)
                            continue;
        
                        if (!TryCancelPendingOrder(
                                order,
                                stale
                                    ? "STALE PENDING ORDER"
                                    : wrongDirection
                                        ? "WRONG DIRECTION PENDING ORDER"
                                        : "REVERSAL SUPERSEDES STOP"))
                        {
                            SetLifecycleState(
                                LifecycleState.RecoveryRequired,
                                "PENDING CLEANUP CANCEL REJECTED");
        
                            _autoOrdersBlockReason =
                                "PENDING CLEANUP CANCEL REJECTED";
                        }
                    }
                }
        
                private Position GetManagedPosition()
                {
                    foreach (Position position in Positions)
                    {
                        if (IsManagedPosition(position) &&
                            position.SymbolName == SymbolName)
                            return position;
                    }
        
                    return null;
                }
        
                private PendingOrder GetManagedPendingOrder()
                {
                    foreach (PendingOrder order in PendingOrders)
                    {
                        if (IsManagedPendingOrder(order) &&
                            order.SymbolName == SymbolName)
                            return order;
                    }
        
                    return null;
                }
        
                private void RenderManagedPendingOrder()
                {
                    RemoveManagedPendingOrderObjects();
        
                    PendingOrder pending =
                        GetManagedPendingOrder();
        
                    if (pending == null ||
                        !IsFinitePositive(
                            pending.TargetPrice))
                        return;
        
                    if (!ShowLevelLines)
                        return;
        
                    int anchorBar =
                        Bars == null ||
                        Bars.Count < 2
                            ? -1
                            : Math.Max(
                                0,
                                Math.Min(
                                    Bars.Count - 1,
                                    MapM5ToChart(
                                        Math.Max(
                                            1,
                                            _lastEvaluatedM5),
                                        Bars.Count - 1)));
        
                    if (anchorBar < 0)
                        return;
        
                    DrawPlanLine(
                        P + "PENDING_ENTRY",
                        pending.TargetPrice,
                        TriggerLineColor,
                        ShowTrigger);
        
                    if (ShowSL &&
                        pending.StopLoss.HasValue &&
                        IsFinitePositive(
                            pending.StopLoss.Value))
                    {
                        DrawPlanLine(
                            P + "PENDING_SL",
                            pending.StopLoss.Value,
                            SlLineColor,
                            ShowSL);
                    }
        
                    if (ShowTP1 &&
                        pending.TakeProfit.HasValue &&
                        IsFinitePositive(
                            pending.TakeProfit.Value))
                    {
                        DrawPlanLine(
                            P + "PENDING_TP",
                            pending.TakeProfit.Value,
                            TpLineColor,
                            ShowTP1);
                    }
        
                    if (!ShowLevelPriceLabels &&
                        !ShowSignalLabels)
                        return;
        
                    string typeText =
                        pending.OrderType ==
                            PendingOrderType.Stop
                            ? "STOP"
                            : pending.OrderType ==
                              PendingOrderType.Limit
                                ? "LIMIT"
                                : "PENDING";
        
                    if (ShowTrigger)
                    {
                        DrawPlanLabel(
                            P + "PENDING_ENTRY_LABEL",
                            "PENDING " +
                            typeText +
                            " " +
                            Price(
                                pending.TargetPrice),
                            anchorBar,
                            pending.TargetPrice,
                            TriggerLineColor);
                    }
        
                    if (pending.StopLoss.HasValue &&
                        IsFinitePositive(
                            pending.StopLoss.Value))
                    {
                        DrawPlanLabel(
                            P + "PENDING_SL_LABEL",
                            "SL " +
                            Price(
                                pending.StopLoss.Value),
                            anchorBar,
                            pending.StopLoss.Value,
                            SlLineColor);
                    }
        
                    if (pending.TakeProfit.HasValue &&
                        IsFinitePositive(
                            pending.TakeProfit.Value))
                    {
                        DrawPlanLabel(
                            P + "PENDING_TP_LABEL",
                            "TP " +
                            Price(
                                pending.TakeProfit.Value),
                            anchorBar,
                            pending.TakeProfit.Value,
                            TpLineColor);
                    }
                }
        
                private void RemoveManagedPendingOrderObjects()
                {
                    RemovePlanLine(
                        P + "PENDING_ENTRY");
                    RemovePlanLine(
                        P + "PENDING_SL");
                    RemovePlanLine(
                        P + "PENDING_TP");
        
                    Chart.RemoveObject(
                        P + "PENDING_ENTRY_LABEL");
                    Chart.RemoveObject(
                        P + "PENDING_SL_LABEL");
                    Chart.RemoveObject(
                        P + "PENDING_TP_LABEL");
                }
        
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
        
                private void CreateQuickExecutionControls()
                {
                    if (_quickExecutionStack != null)
                        return;
        
                    _quickExecutionStack =
                        new StackPanel
                        {
                            Orientation =
                                Orientation.Horizontal,
                            HorizontalAlignment =
                                HorizontalAlignment.Stretch,
                            VerticalAlignment =
                                VerticalAlignment.Center,
                            BackgroundColor =
                                Color.FromArgb(
                                    0,
                                    Color.Black),
                            Height =
                                QuickExecutionRowHeight
                        };
        
                    _autoTradingQuickToggle =
                        CreateExecutionToggle(
                            AutoTradingEnabled,
                            "AUTO TRADE",
                            TpLineColor);
        
                    _automaticOrdersQuickToggle =
                        CreateExecutionToggle(
                            AutomaticOrdersEnabled,
                            "AUTO ORDERS",
                            TriggerLineColor);
        
                    _autoTradingQuickToggle.Click +=
                        OnAutoTradingQuickToggleClicked;
        
                    _automaticOrdersQuickToggle.Click +=
                        OnAutomaticOrdersQuickToggleClicked;
        
                    _quickExecutionStack.AddChild(
                        _autoTradingQuickToggle);
        
                    _quickExecutionStack.AddChild(
                        _automaticOrdersQuickToggle);
                }
        
                private ToggleButton CreateExecutionToggle(
                    bool isChecked,
                    string caption,
                    Color accentColor)
                {
                    return
                        new ToggleButton
                        {
                            Text =
                                isChecked
                                    ? caption + "  •  ON"
                                    : caption + "  •  OFF",
                            Width = 170,
                            Height =
                                QuickExecutionButtonHeight,
                            IsChecked =
                                isChecked,
                            FontFamily =
                                string.IsNullOrWhiteSpace(
                                    PanelFontFamily)
                                    ? "Arial"
                                    : PanelFontFamily,
                            FontSize =
                                Math.Max(
                                    8,
                                    PanelFontSize - 1),
                            BackgroundColor =
                                Color.FromArgb(
                                    105,
                                    isChecked
                                        ? accentColor
                                        : Color.Black),
                            ForegroundColor =
                                PanelTextColor,
                            BorderColor =
                                isChecked
                                    ? accentColor
                                    : PanelBorder,
                            BorderThickness = 1,
                            CornerRadius =
                                Math.Min(
                                    10,
                                    Math.Max(
                                        5,
                                        PanelCornerRadius))
                        };
                }
        
                private void OnAutoTradingQuickToggleClicked(
                    ToggleButtonEventArgs args)
                {
                    if (_executionToggleSyncing ||
                        args == null ||
                        args.ToggleButton == null)
                        return;
        
                    bool enabled =
                        args.ToggleButton.IsChecked;
        
                    SetAutoTradingRuntimeState(
                        enabled,
                        enabled
                            ? "AWAITING EXECUTION"
                            : "DISABLED");
        
                    SetAutoTradingState(
                        enabled
                            ? "ARMED"
                            : "OFF",
                        enabled
                            ? "QUICK ENABLED"
                            : "QUICK DISABLED");
                }
        
                private void OnAutomaticOrdersQuickToggleClicked(
                    ToggleButtonEventArgs args)
                {
                    if (_executionToggleSyncing ||
                        args == null ||
                        args.ToggleButton == null)
                        return;
        
                    bool enabled =
                        args.ToggleButton.IsChecked;
        
                    SetAutomaticOrdersRuntimeState(
                        enabled,
                        enabled
                            ? "AWAITING ORDER SETUP"
                            : "DISABLED");
                }
        
                private void SyncQuickExecutionControls()
                {
                    EnsureExecutionRuntimeState();
        
                    _executionToggleSyncing = true;
        
                    try
                    {
                        if (_autoTradingQuickToggle != null)
                        {
                            _autoTradingQuickToggle.IsChecked =
                                AutoTradingEnabled;
        
                            bool compactTradeText =
                                _autoTradingQuickToggle.Width < 120;
        
                            _autoTradingQuickToggle.Text =
                                compactTradeText
                                    ? (AutoTradingEnabled ? "TRADE • ON" : "TRADE • OFF")
                                    : (AutoTradingEnabled ? "AUTO TRADE • ON" : "AUTO TRADE • OFF");
        
                            _autoTradingQuickToggle.BackgroundColor =
                                Color.FromArgb(
                                    105,
                                    AutoTradingEnabled
                                        ? TpLineColor
                                        : Color.Black);
        
                            _autoTradingQuickToggle.BorderColor =
                                AutoTradingEnabled
                                    ? TpLineColor
                                    : PanelBorder;
        
                            _autoTradingQuickToggle.ForegroundColor =
                                PanelTextColor;
                        }
        
                        if (_automaticOrdersQuickToggle != null)
                        {
                            _automaticOrdersQuickToggle.IsChecked =
                                AutomaticOrdersEnabled;
        
                            bool compactOrderText =
                                _automaticOrdersQuickToggle.Width < 120;
        
                            _automaticOrdersQuickToggle.Text =
                                compactOrderText
                                    ? (AutomaticOrdersEnabled ? "ORDERS • ON" : "ORDERS • OFF")
                                    : (AutomaticOrdersEnabled ? "AUTO ORDERS • ON" : "AUTO ORDERS • OFF");
        
                            _automaticOrdersQuickToggle.BackgroundColor =
                                Color.FromArgb(
                                    105,
                                    AutomaticOrdersEnabled
                                        ? TriggerLineColor
                                        : Color.Black);
        
                            _automaticOrdersQuickToggle.BorderColor =
                                AutomaticOrdersEnabled
                                    ? TriggerLineColor
                                    : PanelBorder;
        
                            _automaticOrdersQuickToggle.ForegroundColor =
                                PanelTextColor;
                        }
                    }
                    finally
                    {
                        _executionToggleSyncing = false;
                    }
                }
        
                private string GetSessionPanelText()
                {
                    DateTime utc =
                        TimeInUtc;
        
                    DateTime local =
                        TimeInUtc +
                        Application.UserTimeOffset;
        
                    int start =
                        ClampInt(
                            SessionStartUtc,
                            0,
                            23);
        
                    int end =
                        ClampInt(
                            SessionEndUtc,
                            0,
                            23);
        
                    int now =
                        utc.Hour * 60 +
                        utc.Minute;
        
                    int startMin =
                        start * 60;
        
                    int endMin =
                        end * 60;
        
                    bool open =
                        startMin <= endMin
                            ? now >= startMin &&
                              now < endMin
                            : now >= startMin ||
                              now < endMin;
        
                    return
                        "SESSION  " +
                        (open ? "OPEN" : "CLOSED") +
                        "  •  UTC " +
                        utc.ToString("HH:mm") +
                        "  •  LOCAL " +
                        local.ToString("HH:mm") +
                        "  •  " +
                        start.ToString("00") +
                        ":00–" +
                        end.ToString("00") +
                        ":00 UTC";
                }
        
                private Color GetSessionPanelColor()
                {
                    DateTime utc =
                        TimeInUtc;
        
                    int now =
                        utc.Hour * 60 +
                        utc.Minute;
        
                    int start =
                        ClampInt(
                            SessionStartUtc,
                            0,
                            23) * 60;
        
                    int end =
                        ClampInt(
                            SessionEndUtc,
                            0,
                            23) * 60;
        
                    bool open =
                        start <= end
                            ? now >= start &&
                              now < end
                            : now >= start ||
                              now < end;
        
                    return open
                        ? TpLineColor
                        : PanelWarningColor;
                }
        
                // ============================================================
    }
}
