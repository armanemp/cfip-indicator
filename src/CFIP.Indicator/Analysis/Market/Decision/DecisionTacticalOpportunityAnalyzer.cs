using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TacticalOpportunityResult EvaluateTacticalOpportunity(
            Decision decision,
            int closedM5)
        {
            return EvaluateTacticalOpportunityForDirection(
                decision,
                closedM5,
                decision == null ? 0 : decision.Direction);
        }

        private TacticalOpportunityResult EvaluateTacticalOpportunityForDirection(
            Decision decision,
            int closedM5,
            int direction)
        {
            if (!EnableParallelOpportunities ||
                decision == null ||
                _m5Frame == null ||
                _m5Bars == null ||
                closedM5 < 30 ||
                (direction != 1 && direction != -1))
                return new TacticalOpportunityResult(
                    false,
                    OpportunityLane.Tactical,
                    0,
                    0);

            int directionalScore =
                direction == 1
                    ? _m5Frame.BullScore
                    : _m5Frame.BearScore;

            if (directionalScore < 35)
                return new TacticalOpportunityResult(
                    false,
                    direction == _m5Frame.Direction
                        ? OpportunityLane.Tactical
                        : OpportunityLane.CounterHtfTactical,
                    0,
                    0);

            ParallelScenarioGeometry geometry;

            if (!TryBuildParallelScenarioGeometry(
                    closedM5,
                    direction,
                    out geometry))
                return new TacticalOpportunityResult(
                    false,
                    direction == _m5Frame.Direction
                        ? OpportunityLane.Tactical
                        : OpportunityLane.CounterHtfTactical,
                    0,
                    0);

            double atr = geometry.Atr;
            double entry = geometry.Entry;
            double risk = geometry.Risk;

            double riskAtr =
                risk /
                Math.Max(Symbol.PipSize, atr);

            int zoneQuality =
                Math.Max(
                    0,
                    Math.Min(100, geometry.ExecutionQuality));

            int structuralEvidence =
                StructuralConfirmations(direction);

            int independentEvidence =
                IndependentEvidence(direction);

            int quality =
                (int)Math.Round(
                    Math.Min(
                        100,
                        Math.Max(50, directionalScore * 1.6)) * 0.45 +
                    zoneQuality * 0.15 +
                    _m5Frame.WaveTrendQuality * 0.20 +
                    independentEvidence * 5.0 +
                    structuralEvidence * 5.0);

            bool strongHtfConflict =
                decision.HtfAnchorDirection != 0 &&
                decision.HtfAlignment >=
                Math.Max(
                    60,
                    Math.Max(
                        MinimumTimeframeAgreement,
                        SmartMinimumTimeframeAgreement)) &&
                decision.HtfAnchorDirection != direction;

            OpportunityLane lane =
                strongHtfConflict
                    ? OpportunityLane.CounterHtfTactical
                    : OpportunityLane.Tactical;

            AdaptiveRewardRiskProfile profile =
                AdaptiveRewardRiskProfileRule.Resolve(
                    decision.Regime,
                    lane,
                    riskAtr,
                    decision.Confidence,
                    quality,
                    zoneQuality,
                    Math.Min(100, structuralEvidence * 12),
                    MaximumTargetExtensionAtr);

            double requiredRR =
                profile.RequiredTp1RR;

            double dynamicMaximumRR =
                Math.Min(
                    Math.Max(requiredRR, MaximumRewardRR),
                    Math.Max(
                        requiredRR,
                        MaximumTargetExtensionAtr /
                        Math.Max(0.20, riskAtr)));

            List<Level> levels =
                BuildTargetLevels(
                    closedM5,
                    direction,
                    entry,
                    atr);

            double bestTarget = 0;
            double bestScore = double.MinValue;

            for (int i = 0; i < levels.Count; i++)
            {
                if (!TryScoreTargetCandidate(
                        levels[i],
                        closedM5,
                        entry,
                        risk,
                        direction,
                        atr,
                        requiredRR,
                        dynamicMaximumRR,
                        entry,
                        false,
                        0,
                        out double score,
                        out _))
                    continue;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget =
                        NormalizePrice(levels[i].Price);
                }
            }

            if (!IsFinitePositive(bestTarget))
                return new TacticalOpportunityResult(
                    false,
                    lane,
                    quality,
                    0);

            RiskRewardMathResult rewardRiskGeometry =
                RiskRewardMathRule.EvaluateFromRisk(
                    direction,
                    entry,
                    risk,
                    bestTarget,
                    0,
                    requiredRR,
                    dynamicMaximumRR,
                    Symbol.PipSize);

            if (!rewardRiskGeometry.Valid)
                return new TacticalOpportunityResult(
                    false,
                    lane,
                    quality,
                    0);

            return TacticalOpportunityRule.Evaluate(
                direction,
                quality,
                _m5Frame.WaveTrendQuality,
                structuralEvidence,
                independentEvidence,
                direction,
                decision.HtfAnchorDirection,
                decision.HtfAlignment,
                Math.Max(
                    MinimumTimeframeAgreement,
                    SmartMinimumTimeframeAgreement),
                rewardRiskGeometry.NominalRR,
                riskAtr,
                decision.Regime,
                decision.Confidence);
        }
    }
}
