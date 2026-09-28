using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double FindImprovedLiveTarget(
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
                Math.Max(
                    0.05,
                    TargetUpdateStepAtr);

            double spacing =
                atr *
                Math.Max(
                    0.05,
                    MinimumTpSpacingAtr);

            double best = current;
            double bestScore = double.MinValue;

            for (int i = 0;
                 i < levels.Count;
                 i++)
            {
                Level level = levels[i];

                if (!IsEligibleLiveTarget(
                        level,
                        requireHtf))
                    continue;

                if (!IsImprovedLiveTarget(
                        level.Price,
                        current,
                        previousTarget,
                        nextTarget,
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
                    best = level.Price;
                }
            }

            return best;
        }

        private bool IsEligibleLiveTarget(
            Level level,
            bool requireHtf)
        {
            if (level == null ||
                level.Score < SmartTargetQuality)
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

            double distance =
                Math.Abs(
                    level.Price -
                    _plan.Entry);

            double rr =
                distance /
                Math.Max(
                    Symbol.PipSize,
                    _plan.Risk);

            return
                rr <=
                Math.Max(
                    0,
                    MaximumRewardRR);
        }

        private bool IsImprovedLiveTarget(
            double price,
            double current,
            double previousTarget,
            double nextTarget,
            double step,
            double spacing)
        {
            bool improves =
                _plan.Direction == 1
                    ? price > current + step
                    : price < current - step;

            if (!improves)
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
