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
            if (!EnableParallelOpportunities ||
                decision == null ||
                _m5Frame == null ||
                _m5Bars == null ||
                closedM5 < 30 ||
                decision.Direction == 0 ||
                _m5Frame.Direction != decision.Direction)
                return new TacticalOpportunityResult(
                    false,
                    OpportunityLane.Tactical,
                    0,
                    0);

            if (!TryBuildExecutionZone(
                    closedM5,
                    decision.Direction,
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
                    decision.Direction,
                    entry,
                    atr,
                    out _,
                    out _);

            if (!IsValidStop(
                    decision.Direction,
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
                    decision.Direction,
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
                        decision.Direction,
                        atr,
                        TacticalOpportunityMinimumRR,
                        Math.Max(
                            TacticalOpportunityMinimumRR,
                            MaximumRewardRR),
                        entry,
                        false,
                        0,
                        out double score))
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
                    _m5Frame.Quality * 0.45 +
                    zoneQuality * 0.15 +
                    _m5Frame.WaveTrendQuality * 0.20 +
                    decision.IndependentEvidence * 5.0 +
                    decision.StructuralConfirmations * 5.0);

            return TacticalOpportunityRule.Evaluate(
                _m5Frame.Direction,
                quality,
                _m5Frame.WaveTrendQuality,
                decision.StructuralConfirmations,
                decision.IndependentEvidence,
                decision.Direction,
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
