// ============================================================================
// CFIP Indicator — TargetSelection.cs
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
    }
}
