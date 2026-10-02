using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Level FindImprovedLiveTarget(
            List<Level> levels,
            int closedM5,
            double current,
            double previousTarget,
            double nextTarget,
            double market,
            double atr,
            double peakRR,
            bool requireHtf)
        {
            double step =
                atr *
                TargetUpdateStepAtr;

            double spacing =
                MinimumLiveTargetDistancePrice(
                    _plan.Direction,
                    atr);

            Level best = null;
            double bestScore = double.MinValue;

            for (int i = 0;
                 i < levels.Count;
                 i++)
            {
                Level level = levels[i];

                if (!IsEligibleLiveTarget(
                        level,
                        requireHtf,
                        market,
                        spacing))
                    continue;

                if (!IsImprovedLiveTarget(
                        level.Price,
                        current,
                        previousTarget,
                        nextTarget,
                        market,
                        step,
                        spacing))
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
                    CalculateLiveTargetScore(
                        level);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = level;
                }
            }

            return best;
        }

        private bool IsEligibleLiveTarget(
            Level level,
            bool requireHtf,
            double market,
            double minimumForwardDistance)
        {
            if (level == null ||
                !IsFinitePositive(level.Price) ||
                !IsFinitePositive(_plan.Entry) ||
                !IsFinitePositive(_plan.Risk) ||
                level.Score < SmartTargetQuality)
                return false;

            if (!LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    _plan.Direction,
                    0,
                    level.Price,
                    market,
                    minimumForwardDistance))
                return false;

            bool htf =
                IsHtfTimeframe(
                    level.Timeframe);

            if (requireHtf &&
                !htf)
                return false;

            if (htf &&
                !UseHigherTfLiquidityTargets &&
                level.Kind.IndexOf(
                    "LIQUIDITY",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            RiskRewardMathResult geometry =
                RiskRewardMathRule.EvaluateFromRisk(
                    _plan.Direction,
                    _plan.Entry,
                    _plan.Risk,
                    level.Price,
                    0,
                    0,
                    MaximumRewardRR,
                    Symbol.PipSize);

            double rr =
                geometry.NominalRR;

            return
                geometry.Valid &&
                IsFinitePositive(rr);
        }

        private bool IsImprovedLiveTarget(
            double price,
            double current,
            double previousTarget,
            double nextTarget,
            double market,
            double step,
            double spacing)
        {
            if (!LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    _plan.Direction,
                    current,
                    price,
                    market,
                    Math.Max(spacing, step)))
                return false;

            bool keepsPreviousSpacing =
                _plan.Direction == 1
                    ? price > previousTarget + spacing
                    : price < previousTarget - spacing;

            if (!keepsPreviousSpacing)
                return false;

            return
                nextTarget <= 0 ||
                (_plan.Direction == 1
                    ? price < nextTarget - spacing
                    : price > nextTarget + spacing);
        }

        private double CalculateLiveTargetScore(
            Level level)
        {
            bool htf =
                IsHtfTimeframe(
                    level.Timeframe);

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

            return score +
                Math.Min(
                    20,
                    Math.Max(
                        1,
                        level.Hits) *
                    2);
        }
    }
}
