// CFIP Indicator — PlanBuilder.cs
// Single-responsibility planning/risk module.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
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
    }
}
