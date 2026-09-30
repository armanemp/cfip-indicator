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

            if (!TryBuildExecutionZone(
                    closedM5,
                    direction,
                    out double atr,
                    out _,
                    out double low,
                    out double high,
                    out double ideal,
                    out _,
                    out _,
                    out _,
                    out int zoneQuality))
                return new TacticalOpportunityResult(
                    false,
                    OpportunityLane.Tactical,
                    0,
                    0);

            double entry =
                NormalizePrice(
                    IsFinitePositive(ideal)
                        ? ideal
                        : (low + high) * 0.50);

            if (!IsFinitePositive(entry))
                return new TacticalOpportunityResult(
                    false,
                    OpportunityLane.Tactical,
                    0,
                    0);

            double stop =
                BuildStructuralStop(
                    closedM5,
                    direction,
                    entry,
                    atr,
                    out _,
                    out _);

            if (!IsValidStop(
                    direction,
                    entry,
                    stop))
                return new TacticalOpportunityResult(
                    false,
                    OpportunityLane.Tactical,
                    0,
                    0);

            double risk =
                Math.Abs(
                    entry -
                    stop);

            if (!IsFinitePositive(risk))
                return new TacticalOpportunityResult(
                    false,
                    OpportunityLane.Tactical,
                    0,
                    0);

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
                        new List<Level>(),
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
