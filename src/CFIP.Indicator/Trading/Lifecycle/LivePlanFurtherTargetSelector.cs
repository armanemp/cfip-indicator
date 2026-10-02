// CFIP Indicator — LivePlanFurtherTargetSelector.cs
// Single-responsibility lifecycle module.

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
        private double FindFurtherLiveTarget(
                            List<Level> selected,
                            int index,
                            double previous,
                            double current,
                            double market,
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

                                if (!farther ||
                                    !IsLiveTargetBrokerSafe(
                                        _plan.Direction,
                                        _plan.Entry,
                                        market,
                                        level.Price,
                                        atr) ||
                                    !LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                                        _plan.Direction,
                                        current,
                                        level.Price,
                                        market,
                                        MinimumLiveTargetDistancePrice(
                                            _plan.Direction,
                                            atr)))
                                    continue;
                
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

                                if (!geometry.Valid)
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
                
                                RiskRewardMathResult previousGeometry =
                                    RiskRewardMathRule.EvaluateFromRisk(
                                        _plan.Direction,
                                        _plan.Entry,
                                        _plan.Risk,
                                        previous,
                                        0,
                                        0,
                                        MaximumRewardRR,
                                        Symbol.PipSize);

                                double baselineRr =
                                    previousGeometry.Valid
                                        ? previousGeometry.NominalRR
                                        : 0;

                                double normalizedDistance =
                                    geometry.Reward /
                                    Math.Max(
                                        Symbol.PipSize,
                                        _plan.Risk);

                                bool htf =
                                    IsHtfTimeframe(
                                        level.Timeframe);

                                bool liquidity =
                                    level.Kind.IndexOf(
                                        "LIQUIDITY",
                                        StringComparison.OrdinalIgnoreCase) >= 0;

                                bool zone =
                                    level.Kind.IndexOf(
                                        "FVG",
                                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    level.Kind.IndexOf(
                                        "ORDER_BLOCK",
                                        StringComparison.OrdinalIgnoreCase) >= 0;

                                double score =
                                    TargetCandidateRewardScoreRule.Calculate(
                                        level.Score,
                                        geometry.NominalRR,
                                        baselineRr,
                                        SmartTargetNearestBias,
                                        normalizedDistance,
                                        htf,
                                        liquidity,
                                        zone,
                                        level.Hits,
                                        1,
                                        HtfRewardBonus,
                                        LiquidityRewardBonus,
                                        ZoneRewardBonus);
                
                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    best = NormalizePrice(level.Price);
                                }
                            }
                
                            return best;
                        }
    }
}
