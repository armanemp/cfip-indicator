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
        
                private double MinimumTakeProfitDistancePrice()
                {
                    try
                    {
                        double distance =
                            Math.Max(
                                0,
                                Symbol.MinTakeProfitDistance);
        
                        if (distance <= 0)
                            return
                                Math.Max(
                                    Symbol.TickSize,
                                    Symbol.PipSize);
        
                        if (Symbol.MinDistanceType ==
                            SymbolMinDistanceType.Pips)
                            return
                                distance *
                                Math.Max(
                                    Symbol.PipSize,
                                    Symbol.TickSize);
        
                        return
                            Symbol.Bid *
                            distance /
                            100.0;
                    }
                    catch
                    {
                        return
                            Math.Max(
                                Symbol.TickSize,
                                Symbol.PipSize);
                    }
                }
        
                private bool IsValidTarget(
                    int direction,
                    double entry,
                    double target)
                {
                    if (!IsFinitePositive(entry) ||
                        !IsFinitePositive(target))
                        return false;
        
                    double minimumDistance =
                        Math.Max(
                            Symbol.TickSize,
                            MinimumTakeProfitDistancePrice());
        
                    return direction == 1
                        ? target > entry + minimumDistance
                        : direction == -1 &&
                          target < entry - minimumDistance;
                }
        
                private double MinimumProtectionDistancePrice()
                {
                    try
                    {
                        double distance =
                            Math.Max(
                                0,
                                Symbol.MinStopLossDistance);
        
                        if (distance <= 0)
                            return
                                Math.Max(
                                    Symbol.TickSize,
                                    Symbol.PipSize);
        
                        if (Symbol.MinDistanceType ==
                            SymbolMinDistanceType.Pips)
                            return
                                distance *
                                Math.Max(
                                    Symbol.PipSize,
                                    Symbol.TickSize);
        
                        return
                            Symbol.Bid *
                            distance /
                            100.0;
                    }
                    catch
                    {
                        return
                            Math.Max(
                                Symbol.TickSize,
                                Symbol.PipSize);
                    }
                }
        
                private bool IsValidStop(
                    int direction,
                    double entry,
                    double stop)
                {
                    if (!IsFinitePositive(entry) ||
                        !IsFinitePositive(stop))
                        return false;
        
                    double minimumDistance =
                        Math.Max(
                            Symbol.TickSize,
                            MinimumProtectionDistancePrice());
        
                    return direction == 1
                        ? stop < entry - minimumDistance
                        : direction == -1 &&
                          stop > entry + minimumDistance;
                }
        
                private bool HasOpposingZonePathObstacle(
                    Bars bars,
                    int index,
                    int direction,
                    double entry,
                    double target,
                    double atr)
                {
                    if (bars == null ||
                        index < 8 ||
                        atr <= 0 ||
                        !IsFinitePositive(entry) ||
                        !IsFinitePositive(target))
                        return false;
        
                    int first =
                        Math.Max(
                            2,
                            index -
                            Math.Max(
                                5,
                                TargetObstacleLookbackBars));
        
                    double clearance =
                        atr *
                        Math.Max(
                            TargetClearanceAtr,
                            TargetObstacleBufferAtr);
        
                    for (int i = first;
                         i <= index - 1;
                         i++)
                    {
                        if (direction == 1)
                        {
                            double gap =
                                bars.LowPrices[i] -
                                bars.HighPrices[i - 2];
        
                            if (gap >=
                                atr *
                                MinimumFvgAtr)
                            {
                                double low =
                                    bars.HighPrices[i - 2];
        
                                double high =
                                    bars.LowPrices[i];
        
                                if (ZoneBlocksRewardPath(
                                        low,
                                        high,
                                        entry,
                                        target,
                                        clearance))
                                    return true;
                            }
                        }
                        else
                        {
                            double gap =
                                bars.LowPrices[i - 2] -
                                bars.HighPrices[i];
        
                            if (gap >=
                                atr *
                                MinimumFvgAtr)
                            {
                                double low =
                                    bars.HighPrices[i];
        
                                double high =
                                    bars.LowPrices[i - 2];
        
                                if (ZoneBlocksRewardPath(
                                        low,
                                        high,
                                        entry,
                                        target,
                                        clearance))
                                    return true;
                            }
                        }
        
                        if (UseOrderBlock)
                        {
                            Zone oppositeOb =
                                BuildOrderBlockCandidate(
                                    bars,
                                    i,
                                    index,
                                    -direction,
                                    atr);
        
                            if (oppositeOb != null &&
                                ZoneBlocksRewardPath(
                                    oppositeOb.Low,
                                    oppositeOb.High,
                                    entry,
                                    target,
                                    clearance))
                                return true;
                        }
                    }
        
                    return false;
                }
        
                private bool HasHigherTfZonePathObstacle(
                    DateTime reference,
                    int direction,
                    double entry,
                    double target)
                {
                    Bars[] frames =
                    {
                        _m15Bars,
                        _m30Bars,
                        _h1Bars,
                        _h4Bars
                    };
        
                    for (int i = 0;
                         i < frames.Length;
                         i++)
                    {
                        Bars frame =
                            frames[i];
        
                        if (frame == null)
                            continue;
        
                        int index =
                            ClosedIndex(
                                frame,
                                reference);
        
                        if (index < 8)
                            continue;
        
                        double frameAtr =
                            Atr(
                                frame,
                                index);
        
                        if (frameAtr <= 0)
                            continue;
        
                        if (HasOpposingZonePathObstacle(
                                frame,
                                index,
                                direction,
                                entry,
                                target,
                                frameAtr))
                            return true;
                    }
        
                    return false;
                }
        
                private bool ZoneBlocksRewardPath(
                    double low,
                    double high,
                    double entry,
                    double target,
                    double clearance)
                {
                    if (low >= high)
                        return false;
        
                    double pathLow =
                        Math.Min(
                            entry,
                            target);
        
                    double pathHigh =
                        Math.Max(
                            entry,
                            target);
        
                    if (high <=
                        pathLow +
                        clearance ||
                        low >=
                        pathHigh -
                        clearance)
                        return false;
        
                    bool containsTarget =
                        target >=
                            low - clearance &&
                        target <=
                            high + clearance;
        
                    if (containsTarget)
                        return false;
        
                    bool containsEntry =
                        entry >=
                            low - clearance &&
                        entry <=
                            high + clearance;
        
                    if (containsEntry)
                        return false;
        
                    return
                        high >
                        pathLow + clearance &&
                        low <
                        pathHigh - clearance;
                }
        
                private bool HasTargetObstacle(
                    Bars bars,
                    int index,
                    int direction,
                    double entry,
                    double target,
                    double atr)
                {
                    if (bars == null ||
                        index < 8 ||
                        atr <= 0 ||
                        !IsFinitePositive(entry) ||
                        !IsFinitePositive(target))
                        return false;
        
                    double clearance =
                        atr *
                        Math.Max(
                            TargetClearanceAtr,
                            TargetObstacleBufferAtr);
        
                    int strength =
                        Math.Max(
                            1,
                            Math.Min(
                                SwingStrength,
                                3));
        
                    int start =
                        Math.Max(
                            strength + 1,
                            index -
                            Math.Max(
                                5,
                                TargetObstacleLookbackBars));
        
                    int last =
                        Math.Max(
                            start,
                            index -
                            strength -
                            1);
        
                    for (int i = start;
                         i <= last;
                         i++)
                    {
                        bool swing = true;
        
                        for (int j = 1;
                             j <= strength;
                             j++)
                        {
                            if (direction == 1)
                            {
                                if (bars.HighPrices[i] <=
                                    bars.HighPrices[i - j] ||
                                    bars.HighPrices[i] <=
                                    bars.HighPrices[i + j])
                                {
                                    swing = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (bars.LowPrices[i] >=
                                    bars.LowPrices[i - j] ||
                                    bars.LowPrices[i] >=
                                    bars.LowPrices[i + j])
                                {
                                    swing = false;
                                    break;
                                }
                            }
                        }
        
                        if (!swing)
                            continue;
        
                        if (direction == 1)
                        {
                            double level =
                                bars.HighPrices[i];
        
                            if (level > entry &&
                                level < target - clearance)
                                return true;
                        }
                        else
                        {
                            double level =
                                bars.LowPrices[i];
        
                            if (level < entry &&
                                level > target + clearance)
                                return true;
                        }
                    }
        
                    if (UseEqualHighLow)
                    {
                        double liquidityLevel =
                            direction == 1
                                ? FindEqualHigh(
                                    bars,
                                    index,
                                    entry,
                                    atr)
                                : FindEqualLow(
                                    bars,
                                    index,
                                    entry,
                                    atr);
        
                        if (direction == 1 &&
                            liquidityLevel > entry &&
                            liquidityLevel <
                            target - clearance)
                            return true;
        
                        if (direction == -1 &&
                            liquidityLevel < entry &&
                            liquidityLevel >
                            target + clearance)
                            return true;
                    }
        
                    return false;
                }
        
                        private void EnsureSignalPlan(
                    int closedM5,
                    DecisionPolicyMode policy)
                {
                    if (_plan != null ||
                        _decision == null ||
                        _decision.Direction == 0)
                        return;
        
                    if (!ShouldCreatePlan(
                            closedM5,
                            policy))
                        return;
        
                    Plan plan =
                        BuildPlan(
                            closedM5,
                            _decision.Direction);
        
                    if (plan == null)
                    {
                        if (AutoTradingEnabled)
                        {
                            SetAutoTradingState(
                                "BLOCKED",
                                "PLAN BUILD / STRUCTURAL REWARD GATE");
                        }
        
                        return;
                    }
        
                    _lastAutoPlanAttemptM5 =
                        closedM5;
        
                    ActivatePlan(plan);
                }
        
                private bool IsHardDecisionBlockReason(
                    string reason)
                {
                    if (string.IsNullOrWhiteSpace(reason))
                        return false;
        
                    switch (reason.Trim().ToUpperInvariant())
                    {
                        case "SESSION":
                        case "FRIDAY":
                        case "SPREAD":
                        case "VOLATILITY GUARD":
                        case "REGIME NO-TRADE":
                        case "NEWS":
                        case "COOLDOWN":
                        case "DIRECTION FLIP":
                        case "CHOP":
                        case "M1 MISALIGNMENT":
                            return true;
        
                        default:
                            return false;
                    }
                }
        
                private bool IsLiveExecutionGateReason(
                    string reason)
                {
                    if (string.IsNullOrWhiteSpace(reason))
                        return false;
        
                    switch (reason.Trim().ToUpperInvariant())
                    {
                        case "TRIGGER":
                        case "M5 TRIGGER":
                        case "ENTRY LOCATION":
                            return true;
        
                        default:
                            return false;
                    }
                }
        
                        private bool ShouldCreatePlan(
                    int closedM5,
                    DecisionPolicyMode policy)
                {
                    if (BlockNewSignalWhileActive &&
                        _plan != null)
                        return false;
        
                    if (_decision == null ||
                        _decision.Direction == 0)
                        return false;
        
                    if (policy ==
                            DecisionPolicyMode.Confirmed)
                    {
                        if (!_decision.EntryAllowed)
                            return false;
                    }
                    else
                    {
                        if (!_decision.EntryAllowed)
                        {
                            bool softEligible =
                                !IsHardDecisionBlockReason(
                                    _decision.BlockReason) &&
                                _decision.Confidence >=
                                    MinimumAutoConfidence &&
                                _decision.SmartQuality >=
                                    MinimumAutoSmartQuality;
        
                            if (!softEligible)
                                return false;
                        }
                    }
        
                    if (BlockSameBarReentryAfterExit &&
                        _lastExitM5 == closedM5)
                        return false;
        
                    if (_lastSignalM5 >= 0 &&
                        closedM5 - _lastSignalM5 <
                        Math.Max(
                            CooldownBars,
                            Math.Max(
                                CooldownM5Bars,
                                ExitReentryCooldownM5)))
                        return false;
        
                    return _lastSignalM5 != closedM5;
                }
        
                private bool HasAnyHtfTargetLevel(
                    List<Level> candidates)
                {
                    if (candidates == null)
                        return false;
        
                    for (int i = 0;
                         i < candidates.Count;
                         i++)
                    {
                        string tf =
                            candidates[i].Timeframe;
        
                        if (tf == "H1" ||
                            tf == "H4" ||
                            tf == "D1" ||
                            tf == "W1")
                            return true;
                    }
        
                    return false;
                }
        
                private void GetAdaptiveSmartThresholds(
                    string regime,
                    out int qualityThreshold,
                    out int shareThreshold,
                    out int edgeThreshold)
                {
                    qualityThreshold =
                        Math.Max(
                            40,
                            Math.Min(
                                95,
                                MinimumSmartQuality));
        
                    shareThreshold =
                        Math.Max(
                            50,
                            Math.Min(
                                90,
                                MinimumSmartDirectionShare));
        
                    edgeThreshold =
                        Math.Max(
                            4,
                            Math.Min(
                                30,
                                MinimumEdge));
        
                    if (!AdaptiveSmartThresholds)
                        return;
        
                    int b =
                        Math.Max(
                            0,
                            SmartRegimeBuffer);
        
                    switch (
                        regime ??
                        "UNKNOWN")
                    {
                        case "TREND":
                        case "EXPANSION":
                            qualityThreshold -= b;
                            shareThreshold -= Math.Max(1, b / 3);
                            edgeThreshold -= Math.Max(1, b / 3);
                            break;
        
                        case "REVERSAL":
                            qualityThreshold -= Math.Max(1, b / 2);
                            break;
        
                        case "RANGE":
                            qualityThreshold += Math.Max(1, b / 2);
                            shareThreshold += Math.Max(1, b / 3);
                            edgeThreshold += Math.Max(1, b / 3);
                            break;
        
                        case "COMPRESSION":
                            qualityThreshold += b;
                            shareThreshold += Math.Max(1, b / 2);
                            edgeThreshold += Math.Max(1, b / 2);
                            break;
                    }
        
                    qualityThreshold =
                        Math.Max(
                            40,
                            Math.Min(
                                95,
                                qualityThreshold));
        
                    shareThreshold =
                        Math.Max(
                            50,
                            Math.Min(
                                90,
                                shareThreshold));
        
                    edgeThreshold =
                        Math.Max(
                            4,
                            Math.Min(
                                30,
                                edgeThreshold));
                }
        
                private int SmartMinimumConsensusFloor()
                {
                    return Math.Max(
                        40,
                        SmartConsensusThreshold - 12);
                }
        
                // ============================================================
    }
}
