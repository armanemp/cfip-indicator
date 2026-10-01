using System;
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
                decision == null
                    ? 0
                    : decision.Direction);
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
                (direction != 1 &&
                 direction != -1))
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

            double atr =
                geometry.Atr;
            double entry =
                geometry.Entry;
            double stop =
                geometry.Stop;
            double risk =
                geometry.Risk;

            int zoneQuality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        geometry.ExecutionQuality));

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
                        TacticalOpportunityMinimumRR,
                        Math.Max(
                            TacticalOpportunityMinimumRR,
                            MaximumRewardRR),
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
                        NormalizePrice(
                            levels[i].Price);
                }
            }

            if (!IsFinitePositive(bestTarget))
                return new TacticalOpportunityResult(
                    false,
                    OpportunityLane.Tactical,
                    0,
                    0);

            double rr =
                Math.Abs(
                    bestTarget -
                    entry) /
                Math.Max(
                    Symbol.PipSize,
                    risk);

            int quality =
                (int)Math.Round(
                    Math.Min(
                        100,
                        Math.Max(
                            50,
                            directionalScore * 1.6)) * 0.45 +
                    zoneQuality * 0.15 +
                    _m5Frame.WaveTrendQuality * 0.20 +
                    IndependentEvidence(direction) * 5.0 +
                    StructuralConfirmations(direction) * 5.0);

            return TacticalOpportunityRule.Evaluate(
                direction,
                quality,
                _m5Frame.WaveTrendQuality,
                StructuralConfirmations(direction),
                IndependentEvidence(direction),
                direction,
                decision.HtfAnchorDirection,
                decision.HtfAlignment,
                Math.Max(
                    MinimumTimeframeAgreement,
                    SmartMinimumTimeframeAgreement),
                rr,
                TacticalOpportunityMinimumQuality,
                TacticalOpportunityMinimumRR,
                CounterHtfMinimumQuality,
                CounterHtfMinimumRR);
        }
    }
}
