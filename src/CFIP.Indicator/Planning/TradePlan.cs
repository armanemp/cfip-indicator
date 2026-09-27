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
        
                private bool IsTriggerReached(
                    int direction,
                    double market,
                    double trigger)
                {
                    if (!IsFinitePositive(market) ||
                        !IsFinitePositive(trigger))
                        return false;
        
                    double tolerance =
                        Math.Max(
                            Symbol.TickSize,
                            Symbol.PipSize * 0.10);
        
                    return direction == 1
                        ? market >= trigger - tolerance
                        : direction == -1 &&
                          market <= trigger + tolerance;
                }
        
                private bool IsContinuationExecutionContext(
                    int direction)
                {
                    if (_decision == null ||
                        _decision.Direction != direction ||
                        _m5Frame == null ||
                        _m15Frame == null)
                        return false;
        
                    bool m5Aligned =
                        _m5Frame.Direction == direction;
        
                    bool m15Compatible =
                        _m15Frame.Direction == direction ||
                        (_m15Frame.Direction == 0 &&
                         AllowM15NeutralPullback);
        
                    bool trendRegime =
                        string.Equals(
                            _decision.Regime,
                            "TREND",
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            _decision.Regime,
                            "EXPANSION",
                            StringComparison.OrdinalIgnoreCase);
        
                    return
                        m5Aligned &&
                        m15Compatible &&
                        (trendRegime ||
                         _decision.StructuralConfirmations >=
                         Math.Max(
                             3,
                             MinimumStructuralConfirmations));
                }
        
                private string ExecutionModeText(
                    ExecutionMode mode)
                {
                    switch (mode)
                    {
                        case ExecutionMode.WaitingForTrigger:
                            return "WAIT TRIGGER";
                        case ExecutionMode.RetestMarket:
                            return "RETEST MARKET";
                        case ExecutionMode.BreakoutMarket:
                            return "BREAKOUT MARKET";
                        case ExecutionMode.ContinuationStop:
                            return "CONTINUATION STOP";
                        case ExecutionMode.ReversalLimit:
                            return "REVERSAL LIMIT";
                        default:
                            return "NONE";
                    }
                }
        
                private bool IsExecutableMarketEntry(
                    Plan plan,
                    double market,
                    out string reason)
                {
                    reason = "OK";
        
                    if (plan == null)
                    {
                        reason = "NO PLAN";
                        return false;
                    }
        
                    if (!IsFinitePositive(market))
                    {
                        reason = "INVALID MARKET";
                        return false;
                    }
        
                    if (plan.EntryMode ==
                        ExecutionMode.BreakoutMarket)
                    {
                        if (!IsTriggerReached(
                                plan.Direction,
                                market,
                                plan.EntryTrigger))
                        {
                            reason = "WAITING FOR TRIGGER";
                            return false;
                        }
        
                        return true;
                    }
        
                    if (plan.EntryMode ==
                        ExecutionMode.RetestMarket)
                    {
                        double tolerance =
                            Math.Max(
                                Symbol.TickSize,
                                Math.Max(
                                    Symbol.PipSize,
                                    Symbol.Ask - Symbol.Bid));
        
                        if (market <
                                plan.EntryZoneLow - tolerance ||
                            market >
                                plan.EntryZoneHigh + tolerance)
                        {
                            reason = "OUTSIDE RETEST ZONE";
                            return false;
                        }
        
                        return true;
                    }
        
                    reason =
                        ExecutionModeText(
                            plan.EntryMode);
                    return false;
                }
        
                private bool IsExecutableFillPrice(
                    Plan plan,
                    double fillPrice,
                    out string reason)
                {
                    reason = "OK";
        
                    if (plan == null ||
                        !IsFinitePositive(fillPrice))
                    {
                        reason = "INVALID FILL";
                        return false;
                    }
        
                    if (plan.EntryMode ==
                        ExecutionMode.BreakoutMarket)
                    {
                        if (!IsFinitePositive(
                                plan.EntryTrigger))
                        {
                            reason = "MISSING BREAKOUT TRIGGER";
                            return false;
                        }
        
                        double tolerance =
                            Math.Max(
                                Symbol.TickSize * 2,
                                Math.Max(
                                    Symbol.PipSize * 0.5,
                                    (Symbol.Ask - Symbol.Bid) * 2));
        
                        bool acceptable =
                            plan.Direction == 1
                                ? fillPrice >=
                                  plan.EntryTrigger - tolerance
                                : fillPrice <=
                                  plan.EntryTrigger + tolerance;
        
                        if (!acceptable)
                        {
                            reason =
                                "BROKER FILL FAR FROM TRIGGER";
                            return false;
                        }
        
                        if (!IsTriggerReached(
                                plan.Direction,
                                fillPrice,
                                plan.EntryTrigger))
                        {
                            reason =
                                "BREAKOUT FILL • SLIPPAGE ACCEPTED";
                        }
        
                        return true;
                    }
        
                    if (plan.EntryMode ==
                        ExecutionMode.RetestMarket)
                        return IsExecutableMarketEntry(
                            plan,
                            fillPrice,
                            out reason);
        
                    reason =
                        "INVALID EXECUTION MODE";
                    return false;
                }
        
                private ExecutionModel BuildExecutionModel(
                    int closedM5,
                    int direction)
                {
                    ExecutionModel model =
                        new ExecutionModel
                        {
                            Direction = direction,
                            Mode = ExecutionMode.None,
                            Source = "NONE"
                        };
        
                    if (_m5Bars == null ||
                        closedM5 < 20 ||
                        (direction != 1 && direction != -1))
                        return model;
        
                    double atr =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    if (atr <= 0)
                        return model;
        
                    double market =
                        NormalizePrice(
                            direction == 1
                                ? Symbol.Ask
                                : Symbol.Bid);
        
                    Zone m5Fvg =
                        FindNearestFvgForExecution(
                            _m5Bars,
                            closedM5,
                            direction,
                            atr,
                            market);
        
                    Zone m5Ob =
                        FindNearestOrderBlockForExecution(
                            _m5Bars,
                            closedM5,
                            direction,
                            atr,
                            market);
        
                    Zone m15Fvg = null;
                    Zone m15Ob = null;
        
                    int m15Index =
                        ClosedIndex(
                            _m15Bars,
                            _m5Bars.OpenTimes[closedM5]);
        
                    double m15Atr =
                        m15Index >= 10
                            ? Atr(_m15Bars, m15Index)
                            : 0;
        
                    if (m15Index >= 10 && m15Atr > 0)
                    {
                        m15Fvg =
                            FindNearestFvgForExecution(
                                _m15Bars,
                                m15Index,
                                direction,
                                m15Atr,
                                market);
        
                        m15Ob =
                            FindNearestOrderBlockForExecution(
                                _m15Bars,
                                m15Index,
                                direction,
                                m15Atr,
                                market);
                    }
        
                    double low = 0;
                    double high = 0;
                    string source = "NONE";
                    int quality = 0;
        
                    if (m5Fvg != null && m5Ob != null)
                    {
                        double overlapLow =
                            Math.Max(m5Fvg.Low, m5Ob.Low);
        
                        double overlapHigh =
                            Math.Min(m5Fvg.High, m5Ob.High);
        
                        if (overlapHigh >= overlapLow)
                        {
                            low = overlapLow;
                            high = overlapHigh;
                            source = "M5 FVG+OB";
                            quality = 92;
                        }
                    }
        
                    if (quality == 0 && m5Fvg != null)
                    {
                        low = m5Fvg.Low;
                        high = m5Fvg.High;
                        source = "M5 FVG";
                        quality = 84;
                    }
        
                    if (quality == 0 && m5Ob != null)
                    {
                        low = m5Ob.Low;
                        high = m5Ob.High;
                        source = "M5 ORDER_BLOCK";
                        quality = 86;
                    }
        
                    if (quality == 0 &&
                        m15Fvg != null &&
                        m15Ob != null)
                    {
                        double overlapLow =
                            Math.Max(m15Fvg.Low, m15Ob.Low);
        
                        double overlapHigh =
                            Math.Min(m15Fvg.High, m15Ob.High);
        
                        if (overlapHigh >= overlapLow)
                        {
                            low = overlapLow;
                            high = overlapHigh;
                            source = "M15 FVG+OB";
                            quality = 86;
                        }
                    }
        
                    if (quality == 0 && m15Fvg != null)
                    {
                        low = m15Fvg.Low;
                        high = m15Fvg.High;
                        source = "M15 FVG";
                        quality = 78;
                    }
        
                    if (quality == 0 && m15Ob != null)
                    {
                        low = m15Ob.Low;
                        high = m15Ob.High;
                        source = "M15 ORDER_BLOCK";
                        quality = 80;
                    }
        
                    if (quality > 0 &&
                        m15Fvg != null &&
                        m15Ob != null)
                    {
                        double overlapLow =
                            Math.Max(
                                low,
                                Math.Max(m15Fvg.Low, m15Ob.Low));
        
                        double overlapHigh =
                            Math.Min(
                                high,
                                Math.Min(m15Fvg.High, m15Ob.High));
        
                        if (overlapHigh > overlapLow)
                        {
                            low = overlapLow;
                            high = overlapHigh;
                            quality += 5;
                            source += "+MTF";
                        }
                    }
        
                    if (quality == 0)
                    {
                        double swing =
                            direction == 1
                                ? FindSwingLowBelow(
                                    _m5Bars,
                                    closedM5,
                                    market)
                                : FindSwingHighAbove(
                                    _m5Bars,
                                    closedM5,
                                    market);
        
                        if (IsFinitePositive(swing))
                        {
                            if (direction == 1)
                            {
                                low = swing;
                                high =
                                    swing +
                                    atr *
                                    Math.Max(
                                        0.10,
                                        ExecutionZoneAtr);
                            }
                            else
                            {
                                low =
                                    swing -
                                    atr *
                                    Math.Max(
                                        0.10,
                                        ExecutionZoneAtr);
                                high = swing;
                            }
        
                            source = "M5 SWING";
                            quality = 70;
                        }
                    }
        
                    if (!IsFinitePositive(low) ||
                        !IsFinitePositive(high) ||
                        high <= low)
                        return model;
        
                    double ideal =
                        low +
                        (high - low) * 0.50;
        
                    int retest =
                        RetestQuality(
                            _m5Bars,
                            closedM5,
                            direction);
        
                    if ((direction == 1 &&
                         PremiumDiscountBias(
                             _m5Bars,
                             closedM5) == 1) ||
                        (direction == -1 &&
                         PremiumDiscountBias(
                             _m5Bars,
                             closedM5) == -1))
                        quality += 5;
        
                    if (_m15Frame != null &&
                        _m15Frame.Direction == direction)
                        quality += 5;
        
                    if (_m30Frame != null &&
                        _m30Frame.Direction == direction)
                        quality += 3;
        
                    if (retest >= MinimumRetestQuality)
                        quality += 5;
        
                    quality =
                        ClampInt(
                            quality,
                            0,
                            100);
        
                    double tolerance =
                        atr *
                        Math.Max(
                            0.02,
                            ExecutionZoneAtr);
        
                    double triggerBuffer =
                        atr *
                        Math.Max(
                            0.01,
                            PrecisionBreakoutBufferAtr);
        
                    model.ZoneLow =
                        NormalizePrice(low);
        
                    model.ZoneHigh =
                        NormalizePrice(high);
        
                    model.IdealEntry =
                        NormalizePrice(ideal);
        
                    model.Trigger =
                        NormalizePrice(
                            direction == 1
                                ? high + triggerBuffer
                                : low - triggerBuffer);
        
                    model.Invalidation =
                        NormalizePrice(
                            direction == 1
                                ? low -
                                  Math.Max(
                                      StopBufferAtr,
                                      0.05) *
                                  atr
                                : high +
                                  Math.Max(
                                      StopBufferAtr,
                                      0.05) *
                                  atr);
        
                    bool inside =
                        market >= low - tolerance &&
                        market <= high + tolerance;
        
                    bool triggerReached =
                        IsTriggerReached(
                            direction,
                            market,
                            model.Trigger);
        
                    bool continuation =
                        IsContinuationExecutionContext(direction);
        
                    bool retestReady =
                        inside &&
                        !triggerReached;
        
                    bool qualityReady =
                        !RequirePrecisionEntry ||
                        quality >=
                        Math.Max(
                            40,
                            MinimumEntryQuality);
        
                    if (triggerReached &&
                        AllowPrecisionBreakoutEntry)
                    {
                        model.Mode =
                            ExecutionMode.BreakoutMarket;
        
                        model.ActualEntry =
                            market;
        
                        model.Ready =
                            qualityReady &&
                            Math.Abs(
                                market -
                                model.Trigger) <=
                            atr *
                            Math.Max(
                                0.10,
                                MaximumEntryExtensionAtr);
                    }
                    else if (continuation)
                    {
                        model.Mode =
                            ExecutionMode.WaitingForTrigger;
        
                        model.ActualEntry = 0;
                        model.Ready = false;
                    }
                    else if (retestReady)
                    {
                        model.Mode =
                            ExecutionMode.RetestMarket;
        
                        model.ActualEntry =
                            market;
        
                        model.Ready =
                            qualityReady &&
                            Math.Abs(
                                market -
                                model.IdealEntry) <=
                            atr *
                            Math.Max(
                                0.05,
                                MaximumEntryDistanceAtr) &&
                            (retest >= MinimumRetestQuality ||
                             !RequireRetestQuality);
                    }
                    else
                    {
                        model.Mode =
                            ExecutionMode.None;
        
                        model.ActualEntry = 0;
                        model.Ready = false;
                    }
        
                    model.Quality = quality;
                    model.Source = source;
        
                    if (model.Mode ==
                        ExecutionMode.WaitingForTrigger)
                        model.Source += "+WAIT";
                    else if (model.Mode ==
                             ExecutionMode.RetestMarket)
                        model.Source += "+RETEST";
                    else if (model.Mode ==
                             ExecutionMode.BreakoutMarket)
                        model.Source += "+EXEC";
        
                    return model;
                }
        
                        private Plan BuildPlan(
                    int closedM5,
                    int direction)
                {
                    if (_m5Bars == null ||
                        closedM5 < 30 ||
                        (direction != 1 && direction != -1))
                        return null;
        
                    double atr =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    if (atr <= 0)
                        return null;
        
                    ExecutionModel execution =
                        BuildExecutionModel(
                            closedM5,
                            direction);
        
                    if (execution == null ||
                        !execution.Ready ||
                        !IsFinitePositive(
                            execution.ActualEntry))
                        return null;
        
                    if (execution.Mode !=
                            ExecutionMode.BreakoutMarket &&
                        execution.Mode !=
                            ExecutionMode.RetestMarket)
                        return null;
        
                    double entry =
                        NormalizePrice(
                            execution.ActualEntry);
        
                    if (!IsFinitePositive(entry))
                        return null;
        
                    if (RequirePrecisionEntry &&
                        execution.Quality <
                        Math.Max(
                            40,
                            MinimumEntryQuality))
                        return null;
        
                    Plan executionProbe =
                        new Plan
                        {
                            Direction = direction,
                            EntryMode = execution.Mode,
                            EntryTrigger = execution.Trigger,
                            EntryZoneLow = execution.ZoneLow,
                            EntryZoneHigh = execution.ZoneHigh
                        };
        
                    string executionReason;
        
                    if (!IsExecutableMarketEntry(
                            executionProbe,
                            entry,
                            out executionReason))
                        return null;
        
                    string stopSource;
                    int stopQuality;
        
                    double stop =
                        BuildStructuralStop(
                            closedM5,
                            direction,
                            entry,
                            atr,
                            out stopSource,
                            out stopQuality);
        
                    if (!IsFinitePositive(stop))
                    {
                        if (RequireStructuralStop)
                            return null;
        
                        stop =
                            direction == 1
                                ? entry -
                                  atr *
                                  FallbackSlAtr
                                : entry +
                                  atr *
                                  FallbackSlAtr;
        
                        stopSource =
                            "ATR FALLBACK";
                        stopQuality = 50;
                    }
        
                    if (AvoidLateEntry &&
                        Math.Abs(
                            entry -
                            _m5Bars.ClosePrices[
                                closedM5]) >
                        atr *
                        MaximumEntryExtensionAtr)
                        return null;
        
                    double risk =
                        Math.Abs(
                            entry -
                            stop);
        
                    double spread =
                        Math.Max(
                            0,
                            Symbol.Ask -
                            Symbol.Bid);
        
                    double minimumRisk =
                        Math.Max(
                            Math.Max(
                                0.05,
                                MinimumSlAtr) *
                            atr,
                            spread *
                            Math.Max(
                                1.0,
                                MaximumSpreadToStopRiskRatio));
        
                    double maximumRisk =
                        Math.Min(
                            Math.Max(
                                MinimumSlAtr,
                                MaximumSlAtr),
                            Math.Max(
                                MinimumSlAtr,
                                MaximumStructuralStopAtr)) *
                        atr;
        
                    if (risk < minimumRisk ||
                        risk > maximumRisk)
                        return null;
        
                    List<Level> candidates =
                        BuildTargetLevels(
                            closedM5,
                            direction,
                            entry,
                            atr);
        
                    List<Level> selected =
                        SelectTargets(
                            candidates,
                            closedM5,
                            entry,
                            risk,
                            direction,
                            atr);
        
                    // FIX (CFIP-BUG-STAGE-ALIGNMENT): `selected` is now a fixed 4-slot
                    // array (possibly containing nulls for stages with no qualifying
                    // level), so count the *filled* slots rather than the list length.
                    int filledTargetSlots =
                        selected.Count(
                            x => x != null);
        
                    if (filledTargetSlots <
                        Math.Max(
                            1,
                            MinimumTargetsForPlan))
                        return null;
        
                    double tp1 =
                        SelectTarget(
                            selected,
                            0,
                            entry,
                            risk,
                            direction,
                            Math.Max(
                                FallbackTp1RR,
                                MinimumRequiredRR()));
        
                    double tp2 =
                        SelectTarget(
                            selected,
                            1,
                            entry,
                            risk,
                            direction,
                            Math.Max(
                                FallbackTp2RR,
                                Tp2MinimumRR));
        
                    double tp3 =
                        SelectTarget(
                            selected,
                            2,
                            entry,
                            risk,
                            direction,
                            Math.Max(
                                FallbackTp3RR,
                                Tp3MinimumRR));
        
                    double tp4 =
                        SelectTarget(
                            selected,
                            3,
                            entry,
                            risk,
                            direction,
                            Math.Max(
                                FallbackTp4RR,
                                Tp4MinimumRR));
        
                    if (!IsValidTarget(
                            direction,
                            entry,
                            tp1))
                        return null;
        
                    if (UseRRFilter &&
                        Math.Abs(
                            tp1 -
                            entry) /
                        Math.Max(
                            Symbol.PipSize,
                            risk) <
                        Math.Max(
                            MinimumTradeRR,
                            MinimumRequiredRR()))
                        return null;
        
                    if (RequireHtfTargets &&
                        !HasAnyHtfTargetLevel(
                            candidates))
                        return null;
        
                    if (RequireHtfRewardForTp2Plus)
                    {
                        if (tp2 > 0 &&
                            !IsHtfSourceForReward(
                                selected,
                                tp2))
                            return null;
        
                        if (tp3 > 0 &&
                            !IsHtfSourceForReward(
                                selected,
                                tp3))
                            return null;
        
                        if (tp4 > 0 &&
                            !IsHtfSourceForReward(
                                selected,
                                tp4))
                            return null;
                    }
        
                    if (RequireHtfRewardForTp1 &&
                        !IsHtfSourceForReward(
                            selected,
                            tp1))
                        return null;
        
                    if (RejectTargetObstacle &&
                        RequireObstacleFreeTp1 &&
                        HasTargetObstacle(
                            _m5Bars,
                            closedM5,
                            direction,
                            entry,
                            tp1,
                            atr))
                        return null;
        
                    Plan p =
                        new Plan
                        {
                            Direction = direction,
                            EntryMode =
                                execution == null
                                    ? ExecutionMode.None
                                    : execution.Mode,
                            Entry = NormalizePrice(entry),
                            IdealEntry =
                                execution == null
                                    ? entry
                                    : NormalizePrice(
                                        execution.IdealEntry),
                            EntryZoneLow =
                                execution == null
                                    ? 0
                                    : NormalizePrice(
                                        execution.ZoneLow),
                            EntryZoneHigh =
                                execution == null
                                    ? 0
                                    : NormalizePrice(
                                        execution.ZoneHigh),
                            EntryTrigger =
                                execution == null
                                    ? 0
                                    : NormalizePrice(
                                        execution.Trigger),
                            EntryInvalidation =
                                execution == null
                                    ? 0
                                    : NormalizePrice(
                                        execution.Invalidation),
                            EntryQuality =
                                execution == null
                                    ? 0
                                    : execution.Quality,
                            EntrySource =
                                execution == null
                                    ? ""
                                    : execution.Source,
                            Stop = NormalizePrice(stop),
                            Tp1 = NormalizePrice(tp1),
                            Tp2 =
                                IsValidTarget(
                                    direction,
                                    entry,
                                    tp2)
                                    ? NormalizePrice(tp2)
                                    : 0,
                            Tp3 =
                                IsValidTarget(
                                    direction,
                                    entry,
                                    tp3)
                                    ? NormalizePrice(tp3)
                                    : 0,
                            Tp4 =
                                IsValidTarget(
                                    direction,
                                    entry,
                                    tp4)
                                    ? NormalizePrice(tp4)
                                    : 0,
                            StopSource = stopSource,
                            StopQuality = stopQuality,
                            CreatedM5 = closedM5
                        };
        
                    p.Risk =
                        Math.Abs(
                            p.Entry -
                            p.Stop);
        
                    p.Tp1RR =
                        p.Tp1 > 0
                            ? Math.Abs(
                                p.Tp1 -
                                p.Entry) /
                              p.Risk
                            : 0;
        
                    p.Tp2RR =
                        p.Tp2 > 0
                            ? Math.Abs(
                                p.Tp2 -
                                p.Entry) /
                              p.Risk
                            : 0;
        
                    p.Tp3RR =
                        p.Tp3 > 0
                            ? Math.Abs(
                                p.Tp3 -
                                p.Entry) /
                              p.Risk
                            : 0;
        
                    p.Tp4RR =
                        p.Tp4 > 0
                            ? Math.Abs(
                                p.Tp4 -
                                p.Entry) /
                              p.Risk
                            : 0;
        
                    ApplyTargetMeta(
                        candidates,
                        p.Tp1,
                        atr,
                        out p.Tp1Source,
                        out p.Tp1Quality);
        
                    ApplyTargetMeta(
                        candidates,
                        p.Tp2,
                        atr,
                        out p.Tp2Source,
                        out p.Tp2Quality);
        
                    ApplyTargetMeta(
                        candidates,
                        p.Tp3,
                        atr,
                        out p.Tp3Source,
                        out p.Tp3Quality);
        
                    ApplyTargetMeta(
                        candidates,
                        p.Tp4,
                        atr,
                        out p.Tp4Source,
                        out p.Tp4Quality);
        
                    p.HtfTargetCount =
                        CountHtfTargetsInPlan(
                            p);
        
                    if (RequirePlanIntegrity &&
                        !ValidatePlanIntegrity(
                            p,
                            direction,
                            entry,
                            atr,
                            true))
                        return null;
        
                    return p;
                }
        
                private bool IsHtfTimeframe(
                    string timeframe)
                {
                    if (string.IsNullOrWhiteSpace(
                            timeframe))
                        return false;
        
                    return
                        timeframe == "M15" ||
                        timeframe == "M30" ||
                        timeframe == "H1" ||
                        timeframe == "H4" ||
                        timeframe == "D1" ||
                        timeframe == "W1";
                }
        
                private bool IsHtfSource(
                    string source)
                {
                    if (string.IsNullOrWhiteSpace(
                            source))
                        return false;
        
                    return
                        source.IndexOf(
                            "@M15",
                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                        source.IndexOf(
                            "@M30",
                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                        source.IndexOf(
                            "@H1",
                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                        source.IndexOf(
                            "@H4",
                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                        source.IndexOf(
                            "@D1",
                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                        source.IndexOf(
                            "@W1",
                            StringComparison.OrdinalIgnoreCase) >= 0;
                }
        
                private bool IsHtfSourceForReward(
                    List<Level> selected,
                    double target)
                {
                    if (selected == null ||
                        !IsFinitePositive(target))
                        return false;
        
                    for (int i = 0;
                         i < selected.Count;
                         i++)
                    {
                        if (selected[i] == null)
                            continue;
        
                        if (Math.Abs(
                                selected[i].Price -
                                target) <=
                            Math.Max(
                                Symbol.PipSize * 2,
                                target * 1e-8) &&
                            IsHtfTimeframe(
                                selected[i].Timeframe))
                            return true;
                    }
        
                    return false;
                }
        
                private int CountHtfTargetsInPlan(
                    Plan plan)
                {
                    if (plan == null)
                        return 0;
        
                    int count = 0;
        
                    if (IsHtfSource(plan.Tp1Source))
                        count++;
        
                    if (IsHtfSource(plan.Tp2Source))
                        count++;
        
                    if (IsHtfSource(plan.Tp3Source))
                        count++;
        
                    if (IsHtfSource(plan.Tp4Source))
                        count++;
        
                    return count;
                }
        
                        private bool ValidatePlanIntegrity(
                    Plan plan,
                    int direction,
                    double referenceEntry,
                    double atr,
                    bool checkSpread)
                {
                    if (plan == null ||
                        (direction != 1 &&
                         direction != -1) ||
                        !IsFinitePositive(referenceEntry) ||
                        atr <= 0)
                        return false;
        
                    if (!IsValidStop(
                            direction,
                            plan.Entry,
                            plan.Stop))
                        return false;
        
                    if (!IsValidTarget(
                            direction,
                            plan.Entry,
                            plan.Tp1))
                        return false;
        
                    if (plan.Risk <= 0)
                        return false;
        
                    if (RequirePrecisionEntry &&
                        plan.EntryQuality <
                        Math.Max(
                            40,
                            MinimumEntryQuality))
                        return false;
        
                    double minimumRR =
                        Math.Max(
                            Tp1MinimumRR,
                            MinimumRequiredRR());
        
                    double maximumRR =
                        Math.Max(
                            minimumRR,
                            MaximumRewardRR);
        
                    double tp1RR =
                        Math.Abs(
                            plan.Tp1 -
                            plan.Entry) /
                        plan.Risk;
        
                    if (tp1RR < minimumRR ||
                        tp1RR > maximumRR)
                        return false;
        
                    if (plan.Tp2 > 0)
                    {
                        double rr =
                            Math.Abs(
                                plan.Tp2 -
                                plan.Entry) /
                            plan.Risk;
        
                        if (rr <
                                Math.Max(
                                    Tp2MinimumRR,
                                    tp1RR +
                                    Math.Max(
                                        0.10,
                                        StructuralTpRrStep)) ||
                            rr > maximumRR)
                            return false;
        
                        if (RequireHtfRewardForTp2Plus &&
                            !IsHtfSource(
                                plan.Tp2Source))
                            return false;
                    }
                    else if (MinimumTargetsForPlan >= 2)
                    {
                        return false;
                    }
        
                    if (plan.Tp3 > 0)
                    {
                        double rr =
                            Math.Abs(
                                plan.Tp3 -
                                plan.Entry) /
                            plan.Risk;
        
                        double previousRR =
                            plan.Tp2 > 0
                                ? plan.Tp2RR
                                : tp1RR;
        
                        if (rr <
                                Math.Max(
                                    Tp3MinimumRR,
                                    previousRR +
                                    Math.Max(
                                        0.10,
                                        StructuralTpRrStep)) ||
                            rr > maximumRR)
                            return false;
        
                        if (RequireHtfRewardForTp2Plus &&
                            !IsHtfSource(
                                plan.Tp3Source))
                            return false;
                    }
        
                    if (plan.Tp4 > 0)
                    {
                        double rr =
                            Math.Abs(
                                plan.Tp4 -
                                plan.Entry) /
                            plan.Risk;
        
                        double previousRR =
                            plan.Tp3 > 0
                                ? plan.Tp3RR
                                : plan.Tp2 > 0
                                    ? plan.Tp2RR
                                    : tp1RR;
        
                        if (rr <
                                Math.Max(
                                    Tp4MinimumRR,
                                    previousRR +
                                    Math.Max(
                                        0.10,
                                        StructuralTpRrStep)) ||
                            rr > maximumRR)
                            return false;
        
                        if (RequireHtfRewardForTp2Plus &&
                            !IsHtfSource(
                                plan.Tp4Source))
                            return false;
                    }
        
                    if (RequireHtfRewardForTp1 &&
                        !IsHtfSource(
                            plan.Tp1Source))
                        return false;
        
                    if (RequireHtfRewardForTp2Plus &&
                        plan.HtfTargetCount <= 0)
                        return false;
        
                    if (checkSpread &&
                        UseSpreadFilter)
                    {
                        double spread =
                            Math.Max(
                                0,
                                Symbol.Ask -
                                Symbol.Bid);
        
                        if (spread > 0 &&
                            plan.Risk > 0 &&
                            spread / plan.Risk >
                            Math.Max(
                                0.02,
                                MaximumSpreadToStopRiskRatio))
                            return false;
                    }
        
                    if (Math.Abs(
                            plan.Entry -
                            referenceEntry) >
                        atr *
                        Math.Max(
                            0.10,
                            MaximumEntryExtensionAtr))
                        return false;
        
                    if (MinimumSmartTargetQualityForTp1 > 0 &&
                        plan.Tp1Quality > 0 &&
                        plan.Tp1Quality <
                        MinimumSmartTargetQualityForTp1)
                        return false;
        
                    if (plan.Tp2 > 0 &&
                        !IsProgressiveTarget(
                            direction,
                            plan.Tp1,
                            plan.Tp2))
                        return false;
        
                    if (plan.Tp3 > 0 &&
                        !IsProgressiveTarget(
                            direction,
                            plan.Tp2 > 0
                                ? plan.Tp2
                                : plan.Tp1,
                            plan.Tp3))
                        return false;
        
                    if (plan.Tp4 > 0 &&
                        !IsProgressiveTarget(
                            direction,
                            plan.Tp3 > 0
                                ? plan.Tp3
                                : plan.Tp2 > 0
                                    ? plan.Tp2
                                    : plan.Tp1,
                            plan.Tp4))
                        return false;
        
                    return true;
                }
        
                private bool IsProgressiveTarget(
                    int direction,
                    double previous,
                    double next)
                {
                    if (!IsFinitePositive(previous) ||
                        !IsFinitePositive(next))
                        return false;
        
                    return direction == 1
                        ? next > previous
                        : direction == -1
                            ? next < previous
                            : false;
                }
        
                private double MinimumRequiredRR()
                {
                    if (!AdaptiveStructuralRR ||
                        _decision == null)
                        return Tp1MinimumRR;
        
                    double step =
                        Math.Max(
                            0.05,
                            StructuralTpRrStep);
        
                    if (_decision.Regime == "EXPANSION")
                        return Math.Max(
                            2.10,
                            Tp1MinimumRR + step);
        
                    if (_decision.Regime == "RANGE")
                        return Math.Max(
                            1.75,
                            Tp1MinimumRR -
                            step * 0.50);
        
                    return Tp1MinimumRR;
                }
        
                        private double BuildStructuralStop(
                    int closedM5,
                    int direction,
                    double entry,
                    double atr,
                    out string source,
                    out int quality)
                {
                    source = "NONE";
                    quality = 0;
        
                    if (_m5Bars == null ||
                        closedM5 < 20 ||
                        atr <= 0 ||
                        !IsFinitePositive(entry))
                        return 0;
        
                    List<Level> candidates =
                        new List<Level>();
        
                    if (UseM5StructureForStop)
                    {
                        double swing =
                            direction == 1
                                ? FindSwingLowBelow(
                                    _m5Bars,
                                    closedM5,
                                    entry)
                                : FindSwingHighAbove(
                                    _m5Bars,
                                    closedM5,
                                    entry);
        
                        AddLevel(
                            candidates,
                            swing,
                            "M5_SWING_STOP",
                            "M5",
                            0,
                            SwingStructureWeight);
                    }
        
                    Zone supportFvg =
                        FindNearestFvg(
                            _m5Bars,
                            closedM5,
                            direction,
                            atr);
        
                    if (supportFvg != null)
                    {
                        AddLevel(
                            candidates,
                            direction == 1
                                ? supportFvg.Low
                                : supportFvg.High,
                            "FVG_STOP",
                            "M5",
                            supportFvg.Age,
                            FvgWeight +
                            SmartStopZoneBonus);
                    }
        
                    Zone supportOb =
                        FindNearestOrderBlock(
                            _m5Bars,
                            closedM5,
                            direction,
                            atr);
        
                    if (supportOb != null)
                    {
                        AddLevel(
                            candidates,
                            direction == 1
                                ? supportOb.Low
                                : supportOb.High,
                            "ORDER_BLOCK_STOP",
                            "M5",
                            supportOb.Age,
                            OrderBlockWeight +
                            SmartStopZoneBonus);
                    }
        
                    if (UseHtfStructureForStop)
                    {
                        Bars[] frames =
                        {
                            _m15Bars,
                            _m30Bars,
                            _h1Bars,
                            _h4Bars,
                            _d1Bars,
                            _w1Bars
                        };
        
                        string[] names =
                        {
                            "M15",
                            "M30",
                            "H1",
                            "H4",
                            "D1",
                            "W1"
                        };
        
                        int[] weights =
                        {
                            M15Weight,
                            M30Weight,
                            H1Weight,
                            H4Weight,
                            D1Weight,
                            W1Weight
                        };
        
                        DateTime reference =
                            _m5Bars.OpenTimes[
                                closedM5];
        
                        for (int i = 0;
                             i < frames.Length;
                             i++)
                        {
                            if (frames[i] == null)
                                continue;
        
                            int idx =
                                ClosedIndex(
                                    frames[i],
                                    reference);
        
                            if (idx < 10)
                                continue;
        
                            double frameAtr =
                                Atr(
                                    frames[i],
                                    idx);
        
                            if (frameAtr <= 0)
                                frameAtr = atr;
        
                            double swing =
                                direction == 1
                                    ? FindSwingLowBelow(
                                        frames[i],
                                        idx,
                                        entry)
                                    : FindSwingHighAbove(
                                        frames[i],
                                        idx,
                                        entry);
        
                            AddLevel(
                                candidates,
                                swing,
                                "HTF_STRUCTURE_STOP",
                                names[i],
                                0,
                                weights[i] +
                                HtfRewardBonus / 2);
        
                            Zone fvg =
                                FindNearestFvg(
                                    frames[i],
                                    idx,
                                    direction,
                                    frameAtr);
        
                            if (fvg != null)
                            {
                                AddLevel(
                                    candidates,
                                    direction == 1
                                        ? fvg.Low
                                        : fvg.High,
                                    "HTF_FVG_STOP",
                                    names[i],
                                    fvg.Age,
                                    FvgWeight +
                                    HtfRewardBonus / 2);
                            }
        
                            Zone ob =
                                FindNearestOrderBlock(
                                    frames[i],
                                    idx,
                                    direction,
                                    frameAtr);
        
                            if (ob != null)
                            {
                                AddLevel(
                                    candidates,
                                    direction == 1
                                        ? ob.Low
                                        : ob.High,
                                    "HTF_ORDER_BLOCK_STOP",
                                    names[i],
                                    ob.Age,
                                    OrderBlockWeight +
                                    HtfRewardBonus / 2);
                            }
                        }
                    }
        
                    if (candidates.Count == 0)
                        return 0;
        
                    double minRiskAtr =
                        Math.Max(
                            0.05,
                            MinimumSlAtr);
        
                    double maxRiskAtr =
                        Math.Min(
                            Math.Max(
                                minRiskAtr,
                                MaximumSlAtr),
                            Math.Max(
                                minRiskAtr,
                                MaximumStructuralStopAtr));
        
                    double spread =
                        Math.Max(
                            0,
                            Symbol.Ask -
                            Symbol.Bid);
        
                    minRiskAtr =
                        Math.Max(
                            minRiskAtr,
                            (spread /
                             Math.Max(
                                 Symbol.PipSize,
                                 atr)) *
                            Math.Max(
                                1.0,
                                MaximumSpreadToStopRiskRatio));
        
                    Level best = null;
                    double bestScore =
                        double.MinValue;
                    int bestQuality = 0;
                    string bestSource = "NONE";
        
                    int minimumQuality =
                        Math.Max(
                            Math.Max(
                                40,
                                MinimumStructuralStopQuality),
                            SmartStopQuality);
        
                    for (int i = 0;
                         i < candidates.Count;
                         i++)
                    {
                        Level candidate =
                            candidates[i];
        
                        double frameAtr =
                            atr;
        
                        if (candidate.Timeframe != "M5")
                        {
                            Bars frame =
                                candidate.Timeframe == "M15"
                                    ? _m15Bars
                                    : candidate.Timeframe == "M30"
                                        ? _m30Bars
                                        : candidate.Timeframe == "H1"
                                            ? _h1Bars
                                            : candidate.Timeframe == "H4"
                                                ? _h4Bars
                                                : candidate.Timeframe == "D1"
                                                    ? _d1Bars
                                                    : _w1Bars;
        
                            int idx =
                                ClosedIndex(
                                    frame,
                                    _m5Bars.OpenTimes[
                                        closedM5]);
        
                            if (idx >= 10)
                            {
                                double localAtr =
                                    Atr(
                                        frame,
                                        idx);
        
                                if (localAtr > 0)
                                    frameAtr =
                                        localAtr;
                            }
                        }
        
                        double buffer =
                            frameAtr *
                            Math.Max(
                                0.02,
                                candidate.Timeframe == "M5"
                                    ? StopBufferAtr
                                    : Math.Max(
                                        StopBufferAtr,
                                        HtfStopBufferAtr));
        
                        double stop =
                            direction == 1
                                ? candidate.Price - buffer
                                : candidate.Price + buffer;
        
                        stop =
                            NormalizePrice(
                                stop);
        
                        if (!IsValidStop(
                                direction,
                                entry,
                                stop))
                            continue;
        
                        double risk =
                            Math.Abs(
                                entry -
                                stop);
        
                        double riskAtr =
                            risk /
                            Math.Max(
                                Symbol.PipSize,
                                atr);
        
                        if (riskAtr < minRiskAtr ||
                            riskAtr > maxRiskAtr)
                            continue;
        
                        double score =
                            candidate.Score;
        
                        if (HasOpposingZonePathObstacle(
                                _m5Bars,
                                closedM5,
                                direction,
                                entry,
                                stop,
                                atr))
                            score -=
                                Math.Min(
                                    18,
                                    StopRiskBalanceWeight);
        
                        if (IsHtfTimeframe(
                                candidate.Timeframe))
                            score +=
                                HtfRewardBonus *
                                0.50;
        
                        if (candidate.Kind.IndexOf(
                                "FVG",
                                StringComparison.OrdinalIgnoreCase) >= 0 ||
                            candidate.Kind.IndexOf(
                                "ORDER_BLOCK",
                                StringComparison.OrdinalIgnoreCase) >= 0)
                            score +=
                                SmartStopZoneBonus;
        
                        if (candidate.Kind.IndexOf(
                                "LIQUIDITY",
                                StringComparison.OrdinalIgnoreCase) >= 0)
                            score +=
                                SmartLiquidityPoolBonus;
        
                        double preferredRisk =
                            Math.Max(
                                0.25,
                                PreferredStopRiskAtr);
        
                        double riskBalance =
                            Math.Max(
                                0,
                                20.0 -
                                Math.Abs(
                                    riskAtr -
                                    preferredRisk) *
                                Math.Max(
                                    6.0,
                                    12.0 *
                                    Math.Max(
                                        0.25,
                                        StopRiskBalanceWeight /
                                        18.0)));
        
                        score +=
                            riskBalance *
                            Math.Max(
                                0,
                                StopRiskBalanceWeight) /
                            20.0;
        
                        if (score > bestScore)
                        {
                            bestScore =
                                score;
                            best =
                                candidate;
                            bestQuality =
                                ClampInt(
                                    (int)Math.Round(
                                        score),
                                    0,
                                    100);
                            bestSource =
                                candidate.Kind +
                                "@" +
                                candidate.Timeframe;
                        }
                    }
        
                    if (best == null ||
                        bestQuality <
                        minimumQuality)
                        return 0;
        
                    double finalFrameAtr =
                        atr;
        
                    if (best.Timeframe != "M5")
                    {
                        Bars frame =
                            best.Timeframe == "M15"
                                ? _m15Bars
                                : best.Timeframe == "M30"
                                    ? _m30Bars
                                    : best.Timeframe == "H1"
                                        ? _h1Bars
                                        : best.Timeframe == "H4"
                                            ? _h4Bars
                                            : best.Timeframe == "D1"
                                                ? _d1Bars
                                                : _w1Bars;
        
                        int idx =
                            ClosedIndex(
                                frame,
                                _m5Bars.OpenTimes[
                                    closedM5]);
        
                        if (idx >= 10)
                        {
                            double localAtr =
                                Atr(
                                    frame,
                                    idx);
        
                            if (localAtr > 0)
                                finalFrameAtr =
                                    localAtr;
                        }
                    }
        
                    double finalBuffer =
                        finalFrameAtr *
                        Math.Max(
                            0.02,
                            best.Timeframe == "M5"
                                ? StopBufferAtr
                                : Math.Max(
                                    StopBufferAtr,
                                    HtfStopBufferAtr));
        
                    double selectedStop =
                        direction == 1
                            ? best.Price - finalBuffer
                            : best.Price + finalBuffer;
        
                    source = bestSource;
                    quality = bestQuality;
        
                    return NormalizePrice(
                        selectedStop);
                }
        
                        private List<Level> BuildTargetLevels(
                    int closedM5,
                    int direction,
                    double entry,
                    double atr)
                {
                    List<Level> levels =
                        new List<Level>();
        
                    if (_m5Bars == null ||
                        atr <= 0 ||
                        !IsFinitePositive(entry))
                        return levels;
        
                    double swing =
                        direction == 1
                            ? FindSwingHighAbove(
                                _m5Bars,
                                closedM5,
                                entry)
                            : FindSwingLowBelow(
                                _m5Bars,
                                closedM5,
                                entry);
        
                    AddLevel(
                        levels,
                        swing,
                        "SWING_TARGET",
                        "M5",
                        0,
                        SwingStructureWeight);
        
                    int opposingDirection =
                        -direction;
        
                    Zone fvg =
                        FindNearestFvg(
                            _m5Bars,
                            closedM5,
                            opposingDirection,
                            atr);
        
                    if (fvg != null)
                    {
                        AddLevel(
                            levels,
                            direction == 1
                                ? fvg.Low
                                : fvg.High,
                            "OPPOSING_FVG",
                            "M5",
                            fvg.Age,
                            FvgWeight +
                            ZoneRewardBonus);
                    }
        
                    Zone ob =
                        FindNearestOrderBlock(
                            _m5Bars,
                            closedM5,
                            opposingDirection,
                            atr);
        
                    if (ob != null)
                    {
                        AddLevel(
                            levels,
                            direction == 1
                                ? ob.Low
                                : ob.High,
                            "OPPOSING_ORDER_BLOCK",
                            "M5",
                            ob.Age,
                            OrderBlockWeight +
                            ZoneRewardBonus);
                    }
        
                    AddSupplyDemandAndLiquidityLevels(
                        levels,
                        closedM5,
                        direction,
                        entry,
                        atr);
        
                    if (UseDailyPivots)
                    {
                        AddDailyPivotLevels(
                            levels,
                            direction,
                            entry,
                            atr,
                            _m5Bars.OpenTimes[closedM5]);
                    }
        
                    if (UseMultiTfLevelMap)
                    {
                        AddHtfTargets(
                            levels,
                            direction,
                            entry,
                            atr,
                            _m5Bars.OpenTimes[
                                closedM5]);
        
                        AddPreviousPeriodLevels(
                            levels,
                            direction,
                            entry,
                            atr,
                            _m5Bars.OpenTimes[
                                closedM5]);
                    }
        
                    AddSmartExtraTargetLevels(
                        levels,
                        closedM5,
                        direction,
                        entry,
                        atr);
        
                    return
                        MergeLevels(
                            levels
                                .Where(
                                    x =>
                                        IsFinitePositive(
                                            x.Price) &&
                                        x.Price != entry)
                                .OrderByDescending(
                                    x =>
                                        x.Score)
                                .Take(
                                    Math.Max(
                                        1,
                                        SmartTargetMaxCandidates))
                                .ToList(),
                            atr);
                }
        
                private void AddSupplyDemandAndLiquidityLevels(
                    List<Level> levels,
                    int closedM5,
                    int direction,
                    double entry,
                    double atr)
                {
                    if (_m5Bars == null)
                        return;
        
                    double swing =
                        direction == 1
                            ? FindSwingHighAbove(
                                _m5Bars,
                                closedM5,
                                entry)
                            : FindSwingLowBelow(
                                _m5Bars,
                                closedM5,
                                entry);
        
                    AddLevel(
                        levels,
                        swing,
                        direction == 1
                            ? "SUPPLY_ZONE"
                            : "DEMAND_ZONE",
                        "M5",
                        0,
                        SupplyDemandWeight);
        
                    if (UseEqualHighLow)
                    {
                        double liquidity =
                            direction == 1
                                ? FindEqualHigh(
                                    _m5Bars,
                                    closedM5,
                                    entry,
                                    atr)
                                : FindEqualLow(
                                    _m5Bars,
                                    closedM5,
                                    entry,
                                    atr);
        
                        AddLevel(
                            levels,
                            liquidity,
                            "LIQUIDITY_POOL",
                            "M5",
                            0,
                            EqualHighLowWeight);
                    }
        
                    if (UseDailyWeeklyLiquidity)
                    {
                        int d1 =
                            ClosedIndex(
                                _d1Bars,
                                _m5Bars.OpenTimes[
                                    closedM5]);
        
                        if (d1 > 0)
                        {
                            AddLevel(
                                levels,
                                direction == 1
                                    ? _d1Bars.HighPrices[d1 - 1]
                                    : _d1Bars.LowPrices[d1 - 1],
                                "LIQUIDITY_POOL",
                                "D1",
                                1,
                                LiquidityPoolWeight);
                        }
                    }
        
                    if (UseSessionLiquidityTargets)
                    {
                        double sessionHigh;
                        double sessionLow;
        
                        GetSessionRange(
                            _m5Bars,
                            closedM5,
                            SessionStartUtc,
                            SessionEndUtc,
                            out sessionHigh,
                            out sessionLow);
        
                        AddLevel(
                            levels,
                            direction == 1
                                ? sessionHigh
                                : sessionLow,
                            "SESSION",
                            "M5",
                            0,
                            Math.Max(
                                SessionWeight,
                                LiquidityTargetMinimumScore));
                    }
                }
        
                private void GetSessionRange(
                    Bars bars,
                    int index,
                    int startHour,
                    int endHour,
                    out double high,
                    out double low)
                {
                    high = 0;
                    low = 0;
        
                    if (bars == null || index < 5)
                        return;
        
                    DateTime anchor =
                        bars.OpenTimes[index];
        
                    DateTime dayStart =
                        new DateTime(
                            anchor.Year,
                            anchor.Month,
                            anchor.Day,
                            0,
                            0,
                            0);
        
                    bool overnight =
                        startHour > endHour;
        
                    DateTime from;
                    DateTime to;
        
                    if (!overnight)
                    {
                        from =
                            dayStart.AddHours(startHour);
        
                        to =
                            dayStart.AddHours(endHour);
                    }
                    else if (anchor.Hour < endHour)
                    {
                        from =
                            dayStart.AddDays(-1)
                                .AddHours(startHour);
        
                        to =
                            dayStart.AddHours(endHour);
                    }
                    else
                    {
                        from =
                            dayStart.AddHours(startHour);
        
                        to =
                            dayStart.AddDays(1)
                                .AddHours(endHour);
                    }
        
                    int first = -1;
                    int last = -1;
        
                    for (int i = index;
                         i >= Math.Max(0, index - 400);
                         i--)
                    {
                        DateTime t = bars.OpenTimes[i];
        
                        if (t < from)
                            break;
        
                        if (t < to)
                        {
                            first = i;
                            if (last < 0)
                                last = i;
                        }
                    }
        
                    if (first < 0 || last < first)
                        return;
        
                    high =
                        Highest(
                            bars,
                            first,
                            last);
        
                    low =
                        Lowest(
                            bars,
                            first,
                            last);
                }
        
                private double FindNextLiquidityAbove(
                    Bars bars,
                    int index,
                    double price)
                {
                    if (bars == null ||
                        index < 10)
                        return 0;
        
                    int first =
                        Math.Max(
                            2,
                            index -
                            LiquidityLookback);
        
                    double best = 0;
        
                    for (int i = first;
                         i <= index - 2;
                         i++)
                    {
                        double high =
                            bars.HighPrices[i];
        
                        if (high <= price)
                            continue;
        
                        if (best <= 0 ||
                            high < best)
                            best = high;
                    }
        
                    return best;
                }
        
                private double FindNextLiquidityBelow(
                    Bars bars,
                    int index,
                    double price)
                {
                    if (bars == null ||
                        index < 10)
                        return 0;
        
                    int first =
                        Math.Max(
                            2,
                            index -
                            LiquidityLookback);
        
                    double best = 0;
        
                    for (int i = first;
                         i <= index - 2;
                         i++)
                    {
                        double low =
                            bars.LowPrices[i];
        
                        if (low >= price)
                            continue;
        
                        if (best <= 0 ||
                            low > best)
                            best = low;
                    }
        
                    return best;
                }
        
                private void AddSmartExtraTargetLevels(
                    List<Level> levels,
                    int closedM5,
                    int direction,
                    double entry,
                    double atr)
                {
                    if (!UseExtendedLiquidityMap ||
                        _m5Bars == null)
                        return;
        
                    double forecast =
                        direction == 1
                            ? FindNextLiquidityAbove(
                                _m5Bars,
                                closedM5,
                                entry)
                            : FindNextLiquidityBelow(
                                _m5Bars,
                                closedM5,
                                entry);
        
                    AddLevel(
                        levels,
                        forecast,
                        "LIQUIDITY_FORECAST",
                        "M5",
                        0,
                        Math.Max(
                            LiquidityPoolWeight,
                            LiquidityTargetMinimumScore));
        
                    if (UseSessionLiquidityTargets)
                    {
                        double high;
                        double low;
        
                        GetSessionRange(
                            _m5Bars,
                            closedM5,
                            SessionStartUtc,
                            SessionEndUtc,
                            out high,
                            out low);
        
                        AddLevel(
                            levels,
                            direction == 1
                                ? high
                                : low,
                            "SESSION_FORECAST",
                            "M5",
                            0,
                            Math.Max(
                                SessionWeight,
                                LiquidityTargetMinimumScore));
                    }
                }
        
                private void AddDailyPivotLevels(
                    List<Level> levels,
                    int direction,
                    double entry,
                    double atr,
                    DateTime reference)
                {
                    if (!UseDailyPivots ||
                        _d1Bars == null ||
                        _d1Bars.Count < 3 ||
                        atr <= 0)
                        return;
        
                    int index =
                        ClosedIndex(
                            _d1Bars,
                            reference);
        
                    if (index <= 0)
                        return;
        
                    int previous = index - 1;
        
                    double high = _d1Bars.HighPrices[previous];
                    double low = _d1Bars.LowPrices[previous];
                    double close = _d1Bars.ClosePrices[previous];
        
                    if (high <= low ||
                        !IsFinitePositive(close))
                        return;
        
                    double pivot = (high + low + close) / 3.0;
                    double range = high - low;
        
                    double r1 = 2.0 * pivot - low;
                    double s1 = 2.0 * pivot - high;
                    double r2 = pivot + range;
                    double s2 = pivot - range;
                    double r3 = high + 2.0 * (pivot - low);
                    double s3 = low - 2.0 * (high - pivot);
        
                    double weight =
                        Math.Max(
                            1,
                            DailyPivotWeight);
        
                    AddLevel(levels, pivot, "PIVOT", "D1", 1, weight);
        
                    if (direction == 1)
                    {
                        AddLevel(levels, r1, "PIVOT_R1", "D1", 1, weight + 4);
                        AddLevel(levels, r2, "PIVOT_R2", "D1", 1, weight + 8);
                        AddLevel(levels, r3, "PIVOT_R3", "D1", 1, weight + 10);
                    }
                    else if (direction == -1)
                    {
                        AddLevel(levels, s1, "PIVOT_S1", "D1", 1, weight + 4);
                        AddLevel(levels, s2, "PIVOT_S2", "D1", 1, weight + 8);
                        AddLevel(levels, s3, "PIVOT_S3", "D1", 1, weight + 10);
                    }
                }
        
                private void AddHtfTargets(
                    List<Level> levels,
                    int direction,
                    double entry,
                    double atr,
                    DateTime reference)
                {
                    Bars[] frames =
                    {
                        _m15Bars,
                        _m30Bars,
                        _h1Bars,
                        _h4Bars,
                        _d1Bars,
                        _w1Bars
                    };
        
                    string[] names =
                    {
                        "M15",
                        "M30",
                        "H1",
                        "H4",
                        "D1",
                        "W1"
                    };
        
                    int[] weights =
                    {
                        M15Weight,
                        M30Weight,
                        H1Weight,
                        H4Weight,
                        D1Weight,
                        W1Weight
                    };
        
                    for (int i = 0;
                         i < frames.Length;
                         i++)
                    {
                        if (frames[i] == null)
                            continue;
        
                        int idx =
                            ClosedIndex(
                                frames[i],
                                reference);
        
                        if (idx < 10)
                            continue;
        
                        double clusterWeight =
                            i < 2
                                ? MtfClusterWeight
                                : HtfStructureWeight;
        
                        double frameAtr =
                            Atr(
                                frames[i],
                                idx);
        
                        if (frameAtr <= 0)
                            frameAtr = atr;
        
                        double swing =
                            direction == 1
                                ? FindSwingHighAbove(
                                    frames[i],
                                    idx,
                                    entry)
                                : FindSwingLowBelow(
                                    frames[i],
                                    idx,
                                    entry);
        
                        AddLevel(
                            levels,
                            swing,
                            "HTF_SWING",
                            names[i],
                            0,
                            weights[i] +
                            clusterWeight / 10.0 +
                            HtfRewardBonus);
        
                        int opposing =
                            -direction;
        
                        Zone fvg =
                            FindNearestFvg(
                                frames[i],
                                idx,
                                opposing,
                                frameAtr);
        
                        if (fvg != null)
                        {
                            AddLevel(
                                levels,
                                direction == 1
                                    ? fvg.Low
                                    : fvg.High,
                                "HTF_FVG",
                                names[i],
                                fvg.Age,
                                FvgWeight +
                                clusterWeight / 10.0 +
                                HtfRewardBonus);
                        }
        
                        Zone ob =
                            FindNearestOrderBlock(
                                frames[i],
                                idx,
                                opposing,
                                frameAtr);
        
                        if (ob != null)
                        {
                            AddLevel(
                                levels,
                                direction == 1
                                    ? ob.Low
                                    : ob.High,
                                "HTF_ORDER_BLOCK",
                                names[i],
                                ob.Age,
                                OrderBlockWeight +
                                clusterWeight / 10.0 +
                                HtfRewardBonus);
                        }
        
                        double liquidity =
                            direction == 1
                                ? FindEqualHigh(
                                    frames[i],
                                    idx,
                                    entry,
                                    frameAtr)
                                : FindEqualLow(
                                    frames[i],
                                    idx,
                                    entry,
                                    frameAtr);
        
                        if (UseHigherTfLiquidityTargets)
                        {
                            AddLevel(
                                levels,
                                liquidity,
                                "HTF_LIQUIDITY",
                                names[i],
                                0,
                                LiquidityPoolWeight +
                                clusterWeight / 10.0 +
                                HtfRewardBonus);
                        }
                    }
                }
        
                private void AddPreviousPeriodLevels(
                    List<Level> levels,
                    int direction,
                    double entry,
                    double atr,
                    DateTime reference)
                {
                    int d1 =
                        ClosedIndex(
                            _d1Bars,
                            reference);
        
                    int w1 =
                        ClosedIndex(
                            _w1Bars,
                            reference);
        
                    if (UseDailyWeeklyLiquidity &&
                        d1 > 0)
                    {
                        AddLevel(
                            levels,
                            direction == 1
                                ? _d1Bars.HighPrices[d1 - 1]
                                : _d1Bars.LowPrices[d1 - 1],
                            "PREVIOUS_DAY",
                            "D1",
                            1,
                            PreviousDayWeight);
                    }
        
                    if (UseDailyWeeklyLiquidity &&
                        w1 > 0)
                    {
                        AddLevel(
                            levels,
                            direction == 1
                                ? _w1Bars.HighPrices[w1 - 1]
                                : _w1Bars.LowPrices[w1 - 1],
                            "PREVIOUS_WEEK",
                            "W1",
                            1,
                            PreviousWeekWeight);
                    }
                }
        
                private void AddLevel(
                    List<Level> levels,
                    double price,
                    string kind,
                    string timeframe,
                    int age,
                    double baseScore)
                {
                    if (!IsFinitePositive(price))
                        return;
        
                    double score =
                        Math.Max(
                            0,
                            baseScore) * 0.60;
        
                    if (age <= 5)
                        score += 15;
        
                    if (timeframe == "H1" ||
                        timeframe == "H4" ||
                        timeframe == "D1" ||
                        timeframe == "W1")
                        score += 8;
        
                    if (kind.IndexOf(
                            "LIQUIDITY",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 5;
        
                    if (kind.IndexOf(
                            "FVG",
                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                        kind.IndexOf(
                            "ORDER_BLOCK",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 4;
        
                    Level level =
                        new Level
                        {
                            Price =
                                NormalizePrice(price),
                            Score =
                                Clamp(
                                    score,
                                    0,
                                    100),
                            Kind = kind,
                            Timeframe = timeframe,
                            Age =
                                Math.Max(
                                    0,
                                    age),
                            Hits = 1
                        };
        
                    levels.Add(
                        level);
                }
        
                private List<Level> MergeLevels(
                    List<Level> input,
                    double atr)
                {
                    List<Level> result =
                        new List<Level>();
        
                    if (input == null ||
                        input.Count == 0)
                        return result;
        
                    double tolerance =
                        Math.Max(
                            Symbol.PipSize * 2,
                            atr *
                            Math.Max(
                                0.02,
                                SmartLevelClusterAtr));
        
                    for (int i = 0;
                         i < input.Count;
                         i++)
                    {
                        Level current =
                            input[i];
        
                        Level match =
                            result.FirstOrDefault(
                                x =>
                                    Math.Abs(
                                        x.Price -
                                        current.Price) <=
                                    tolerance);
        
                        if (match == null)
                        {
                            result.Add(current);
                            continue;
                        }
        
                        int matchHits =
                            Math.Max(
                                1,
                                match.Hits);
        
                        int currentHits =
                            Math.Max(
                                1,
                                current.Hits);
        
                        match.Price =
                            NormalizePrice(
                                (match.Price *
                                 matchHits +
                                 current.Price *
                                 currentHits) /
                                (matchHits +
                                 currentHits));
        
                        match.Score =
                            Clamp(
                                Math.Max(
                                    match.Score,
                                    current.Score) +
                                Math.Min(
                                    20,
                                    Math.Min(
                                        match.Score,
                                        current.Score) *
                                    0.25),
                                0,
                                100);
        
                        match.Hits =
                            matchHits +
                            currentHits;
        
                        match.Age =
                            Math.Min(
                                match.Age,
                                current.Age);
        
                        if (current.Timeframe == "H1" ||
                            current.Timeframe == "H4" ||
                            current.Timeframe == "D1" ||
                            current.Timeframe == "W1")
                            match.Timeframe =
                                current.Timeframe;
        
                        if (match.Kind == "SWING" &&
                            current.Kind != "SWING")
                            match.Kind =
                                current.Kind;
                    }
        
                    return
                        result
                        .OrderByDescending(
                            x => x.Score)
                        .ToList();
                }
        
                        private List<Level> SelectTargets(
                    List<Level> levels,
                    int closedM5,
                    double entry,
                    double risk,
                    int direction,
                    double atr)
                {
                    // FIX (CFIP-BUG-STAGE-ALIGNMENT): this list always has exactly 4 slots
                    // (index 0=TP1 .. 3=TP4), and a slot stays null when no candidate meets
                    // that stage's own RR requirement. Previously the list only contained
                    // the *found* levels appended in order, so if stage 0 (TP1) found
                    // nothing but stage 1 (TP2) did, that TP2-grade level silently became
                    // "selected[0]" and got labelled/priced as TP1 by every caller
                    // (BuildPlan, SelectStructuralAutoTarget, live target refresh). That
                    // meant the reported TP could be at the wrong RR tier without any
                    // indication. Keeping fixed, possibly-null slots makes index == stage
                    // always true, so downstream RR/labels are trustworthy.
                    List<Level> selected =
                        new List<Level>
                        {
                            null, null, null, null
                        };
        
                    if (levels == null ||
                        levels.Count == 0 ||
                        risk <= 0 ||
                        atr <= 0)
                        return selected;
        
                    double rrStep =
                        Math.Max(
                            0.10,
                            StructuralTpRrStep);
        
                    double adaptiveTp1RR =
                        Math.Max(
                            Tp1MinimumRR,
                            MinimumRequiredRR());
        
                    double maximumRR =
                        Math.Max(
                            adaptiveTp1RR,
                            MaximumRewardRR);
        
                    double[] requiredRR =
                    {
                        adaptiveTp1RR,
                        Math.Max(
                            Tp2MinimumRR,
                            adaptiveTp1RR + rrStep),
                        Math.Max(
                            Tp3MinimumRR,
                            Tp2MinimumRR + rrStep),
                        Math.Max(
                            Tp4MinimumRR,
                            Tp3MinimumRR + rrStep)
                    };
        
                    for (int stage = 0;
                         stage < 4;
                         stage++)
                    {
                        if (requiredRR[stage] >
                            maximumRR)
                            continue;
        
                        bool requireHtf =
                            stage == 0
                                ? RequireHtfRewardForTp1
                                : RequireHtfRewardForTp2Plus;
        
                        Level best = null;
                        double bestScore =
                            double.MinValue;
        
                        // Nearest already-assigned earlier stage (skipping any stage
                        // that stayed empty), falling back to entry. This keeps target
                        // spacing meaningful even when a lower stage had no candidate.
                        double previous = entry;
        
                        for (int p = stage - 1; p >= 0; p--)
                        {
                            if (selected[p] != null)
                            {
                                previous = selected[p].Price;
                                break;
                            }
                        }
        
                        for (int i = 0;
                             i < levels.Count;
                             i++)
                        {
                            Level candidate =
                                levels[i];
        
                            if (!IsValidTarget(
                                    direction,
                                    entry,
                                    candidate.Price))
                                continue;
        
                            if (candidate.Age >
                                MaximumSetupAgeBars)
                                continue;
        
                            bool htf =
                                IsHtfTimeframe(
                                    candidate.Timeframe);
        
                            if (requireHtf &&
                                (!htf ||
                                 candidate.Score <
                                 MinimumHtfRewardQuality))
                                continue;
        
                            double distance =
                                Math.Abs(
                                    candidate.Price -
                                    entry);
        
                            double rr =
                                distance /
                                Math.Max(
                                    Symbol.PipSize,
                                    risk);
        
                            double minimumCandidateRR =
                                requiredRR[stage];
        
                            if (htf)
                                minimumCandidateRR =
                                    Math.Max(
                                        minimumCandidateRR,
                                        MinimumHtfTargetRR);
        
                            if (rr < minimumCandidateRR ||
                                rr > maximumRR)
                                continue;
        
                            if (distance >
                                atr *
                                Math.Max(
                                    1.0,
                                    MaximumTargetExtensionAtr))
                                continue;
        
                            double spacing =
                                atr *
                                Math.Max(
                                    0.05,
                                    MinimumTpSpacingAtr);
        
                            if (selected.Any(
                                x =>
                                    x != null &&
                                    Math.Abs(
                                        x.Price -
                                        candidate.Price) <=
                                    spacing * 0.50))
                                continue;
        
                            if (stage > 0)
                            {
                                if (direction == 1 &&
                                    candidate.Price <=
                                    previous +
                                    spacing)
                                    continue;
        
                                if (direction == -1 &&
                                    candidate.Price >=
                                    previous -
                                    spacing)
                                    continue;
                            }
        
                            if (RejectTargetObstacle &&
                                HasTargetObstacle(
                                    _m5Bars,
                                    closedM5,
                                    direction,
                                    entry,
                                    candidate.Price,
                                    atr))
                                continue;
        
                            if (RejectTargetObstacle &&
                                HasOpposingZonePathObstacle(
                                    _m5Bars,
                                    closedM5,
                                    direction,
                                    entry,
                                    candidate.Price,
                                    atr))
                                continue;
        
                            if (stage >= 1 &&
                                RejectTargetObstacle &&
                                HasHigherTfZonePathObstacle(
                                    _m5Bars.OpenTimes[
                                        closedM5],
                                    direction,
                                    entry,
                                    candidate.Price))
                                continue;
        
                            double normalizedDistance =
                                distance /
                                Math.Max(
                                    Symbol.PipSize,
                                    atr);
        
                            double efficiency =
                                Math.Max(
                                    0,
                                    25 -
                                    Math.Abs(
                                        rr -
                                        requiredRR[stage]) *
                                    4);
        
                            double score =
                                candidate.Score +
                                efficiency;
        
                            if (htf)
                                score +=
                                    Math.Max(
                                        0,
                                        HtfRewardBonus);
        
                            if (candidate.Kind.IndexOf(
                                    "LIQUIDITY",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                score +=
                                    Math.Max(
                                        0,
                                        LiquidityRewardBonus);
        
                            if (candidate.Kind.IndexOf(
                                    "FVG",
                                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                                candidate.Kind.IndexOf(
                                    "ORDER_BLOCK",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                score +=
                                    Math.Max(
                                        0,
                                        ZoneRewardBonus);
        
                            score +=
                                Math.Min(
                                    20,
                                    Math.Max(
                                        1,
                                        candidate.Hits) *
                                    2);
        
                            score +=
                                SmartTargetNearestBias /
                                (1.0 +
                                 Math.Max(
                                     0,
                                     normalizedDistance)) *
                                10.0;
        
                            score +=
                                stage *
                                (htf
                                    ? HtfRewardBonus * 0.35
                                    : 2.0);
        
                            if (score > bestScore)
                            {
                                bestScore =
                                    score;
                                best =
                                    candidate;
                            }
                        }
        
                        if (best != null)
                            selected[stage] = best;
                    }
        
                    return selected;
                }
        
                        private double SelectTarget(
                    List<Level> selected,
                    int position,
                    double entry,
                    double risk,
                    int direction,
                    double alternateRR)
                {
                    if (selected != null &&
                        position < selected.Count &&
                        selected[position] != null)
                        return selected[position].Price;
        
                    bool requireHtf =
                        position == 0
                            ? RequireHtfRewardForTp1
                            : RequireHtfRewardForTp2Plus;
        
                    double minimumRR =
                        Math.Max(
                            0.50,
                            alternateRR);
        
                    double maximumRR =
                        Math.Max(
                            minimumRR,
                            MaximumRewardRR);
        
                    if (!AllowSyntheticTargetFallback ||
                        requireHtf ||
                        minimumRR > maximumRR)
                        return 0;
        
                    return NormalizePrice(
                        direction == 1
                            ? entry +
                              risk *
                              minimumRR
                            : entry -
                              risk *
                              minimumRR);
                }
        
                private void ApplyTargetMeta(
                    List<Level> candidates,
                    double target,
                    double atr,
                    out string source,
                    out int quality)
                {
                    source =
                        target > 0
                            ? "RR"
                            : "";
        
                    quality =
                        target > 0
                            ? 55
                            : 0;
        
                    if (target <= 0)
                        return;
        
                    Level best = null;
                    double bestDistance = double.MaxValue;
        
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        double distance =
                            Math.Abs(
                                candidates[i].Price -
                                target);
        
                        if (distance <= atr * 0.15 &&
                            distance < bestDistance)
                        {
                            best =
                                candidates[i];
        
                            bestDistance =
                                distance;
                        }
                    }
        
                    if (best != null)
                    {
                        source =
                            best.Kind +
                            "@" +
                            best.Timeframe;
        
                        quality =
                            ClampInt(
                                (int)Math.Round(
                                    best.Score),
                                0,
                                100);
        
                        if (best.Age <= 5)
                            quality =
                                Math.Min(
                                    100,
                                    quality + 5);
                    }
                }
        
                // ============================================================
    }
}
