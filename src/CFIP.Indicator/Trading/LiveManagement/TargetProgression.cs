// ============================================================================
// CFIP Indicator — TargetProgression.cs
// ============================================================================

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
        private void UpdateUnhitTargetsLive(
                                    int closedM5,
                                    double market)
                                {
                                    if (_plan == null)
                                        return;
                        
                                    if (StructuralTargetUpdatesOnly &&
                                        _lastTargetRepriceM5 ==
                                        closedM5)
                                        return;
                        
                                    double atr =
                                        Atr(
                                            _m5Bars,
                                            closedM5);
                        
                                    if (atr <= 0)
                                        return;
                        
                                    List<Level> levels =
                                        BuildTargetLevels(
                                            closedM5,
                                            _plan.Direction,
                                            _plan.Entry,
                                            atr);
                        
                                    double step =
                                        atr *
                                        Math.Max(
                                            0.05,
                                            TargetUpdateStepAtr);
                        
                                    double spacing =
                                        atr *
                                        Math.Max(
                                            0.05,
                                            MinimumTpSpacingAtr);
                        
                                    bool changed = false;
                        
                                    for (int stage = 0;
                                         stage < 4;
                                         stage++)
                                    {
                                        int hit =
                                            stage == 0
                                                ? _tp1Hit
                                                : stage == 1
                                                    ? _tp2Hit
                                                    : stage == 2
                                                        ? _tp3Hit
                                                        : _tp4Hit;
                        
                                        if (hit != 0)
                                            continue;
                        
                                        double current =
                                            stage == 0
                                                ? _plan.Tp1
                                                : stage == 1
                                                    ? _plan.Tp2
                                                    : stage == 2
                                                        ? _plan.Tp3
                                                        : _plan.Tp4;
                        
                                        if (!IsFinitePositive(current))
                                            continue;
                        
                                        bool requireHtf =
                                            stage == 0
                                                ? RequireHtfRewardForTp1
                                                : RequireHtfRewardForTp2Plus;
                        
                                        double previousTarget =
                                            stage == 0
                                                ? _plan.Entry
                                                : stage == 1
                                                    ? _plan.Tp1
                                                    : stage == 2
                                                        ? _plan.Tp2
                                                        : _plan.Tp3;
                        
                                        double nextTarget =
                                            stage == 0
                                                ? _plan.Tp2
                                                : stage == 1
                                                    ? _plan.Tp3
                                                    : stage == 2
                                                        ? _plan.Tp4
                                                        : 0;
                        
                                        double best =
                                            current;
                        
                                        double bestScore =
                                            double.MinValue;
                        
                                        for (int i = 0;
                                             i < levels.Count;
                                             i++)
                                        {
                                            Level level =
                                                levels[i];
                        
                                            if (level.Score <
                                                SmartTargetQuality)
                                                continue;
                        
                                            bool htf =
                                                IsHtfTimeframe(
                                                    level.Timeframe);
                        
                                            if (requireHtf &&
                                                !htf)
                                                continue;
                        
                                            if (htf &&
                                                !UseHigherTfLiquidityTargets &&
                                                level.Kind.IndexOf(
                                                    "LIQUIDITY",
                                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                                continue;
                        
                                            double distance =
                                                Math.Abs(
                                                    level.Price -
                                                    _plan.Entry);
                        
                                            double rr =
                                                distance /
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    _plan.Risk);
                        
                                            if (rr >
                                                Math.Max(
                                                    0,
                                                    MaximumRewardRR))
                                                continue;
                        
                                            bool improves =
                                                _plan.Direction == 1
                                                    ? level.Price >
                                                      best + step
                                                    : level.Price <
                                                      best - step;
                        
                                            if (!improves)
                                                continue;
                        
                                            bool keepsPreviousSpacing =
                                                _plan.Direction == 1
                                                    ? level.Price >
                                                      previousTarget +
                                                      spacing
                                                    : level.Price <
                                                      previousTarget -
                                                      spacing;
                        
                                            if (!keepsPreviousSpacing)
                                                continue;
                        
                                            bool keepsNextSpacing =
                                                nextTarget <= 0 ||
                                                (_plan.Direction == 1
                                                    ? level.Price <
                                                      nextTarget -
                                                      spacing
                                                    : level.Price >
                                                      nextTarget +
                                                      spacing);
                        
                                            if (!keepsNextSpacing)
                                                continue;
                        
                                            if (RejectTargetObstacle &&
                                                HasTargetObstacle(
                                                    _m5Bars,
                                                    closedM5,
                                                    _plan.Direction,
                                                    _plan.Entry,
                                                    level.Price,
                                                    atr))
                                                continue;
                        
                                            double score =
                                                level.Score;
                        
                                            if (htf)
                                                score +=
                                                    HtfRewardBonus;
                        
                                            if (level.Kind.IndexOf(
                                                    "LIQUIDITY",
                                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                                score +=
                                                    LiquidityRewardBonus;
                        
                                            if (level.Kind.IndexOf(
                                                    "FVG",
                                                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                level.Kind.IndexOf(
                                                    "ORDER_BLOCK",
                                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                                score +=
                                                    ZoneRewardBonus;
                        
                                            score +=
                                                Math.Min(
                                                    20,
                                                    Math.Max(
                                                        1,
                                                        level.Hits) * 2);
                        
                                            if (score > bestScore)
                                            {
                                                bestScore =
                                                    score;
                                                best =
                                                    level.Price;
                                            }
                                        }
                        
                                        if (Math.Abs(
                                                best -
                                                current) <
                                            Symbol.PipSize)
                                            continue;
                        
                                        if (stage == 0)
                                            _plan.Tp1 =
                                                NormalizePrice(best);
                                        else if (stage == 1)
                                            _plan.Tp2 =
                                                NormalizePrice(best);
                                        else if (stage == 2)
                                            _plan.Tp3 =
                                                NormalizePrice(best);
                                        else
                                            _plan.Tp4 =
                                                NormalizePrice(best);
                        
                                        changed = true;
                                    }
                        
                                    _lastTargetRepriceM5 =
                                        closedM5;
                        
                                    if (changed)
                                    {
                                        ApplyTargetMeta(
                                            levels,
                                            _plan.Tp1,
                                            atr,
                                            out _plan.Tp1Source,
                                            out _plan.Tp1Quality);
                        
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
                                    }
                        
                                    RecalculatePlanRR();
                                }
        
        private void RecalculatePlanRR()
                                {
                                    if (_plan == null ||
                                        _plan.Risk <= 0)
                                        return;
                        
                                    _plan.Tp1RR =
                                        _plan.Tp1 > 0
                                            ? Math.Abs(
                                                _plan.Tp1 -
                                                _plan.Entry) /
                                              _plan.Risk
                                            : 0;
                        
                                    _plan.Tp2RR =
                                        _plan.Tp2 > 0
                                            ? Math.Abs(
                                                _plan.Tp2 -
                                                _plan.Entry) /
                                              _plan.Risk
                                            : 0;
                        
                                    _plan.Tp3RR =
                                        _plan.Tp3 > 0
                                            ? Math.Abs(
                                                _plan.Tp3 -
                                                _plan.Entry) /
                                              _plan.Risk
                                            : 0;
                        
                                    _plan.Tp4RR =
                                        _plan.Tp4 > 0
                                            ? Math.Abs(
                                                _plan.Tp4 -
                                                _plan.Entry) /
                                              _plan.Risk
                                            : 0;
                                }
    }
}
