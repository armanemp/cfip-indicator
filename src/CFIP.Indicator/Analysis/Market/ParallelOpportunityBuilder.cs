using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RefreshParallelOpportunityCandidates(
            int closedM5)
        {
            if (_lastOpportunityCandidatesM5 == closedM5)
                return;

            _lastOpportunityCandidatesM5 =
                closedM5;

            _opportunityCandidates.Clear();

            if (!EnableParallelOpportunities ||
                _m5Bars == null ||
                _m5Frame == null ||
                closedM5 < 30)
                return;

            TradeOpportunityCandidate strategic =
                BuildLaneCandidate(
                    closedM5,
                    OpportunityLane.Strategic,
                    _decision == null
                        ? 0
                        : _decision.Direction,
                    _decision == null
                        ? 0
                        : _decision.Confidence);

            if (strategic != null &&
                _decision != null &&
                _decision.TopDownEligible &&
                string.Equals(
                    _decision.TopDownStage,
                    "ENTRY CALIBRATED",
                    StringComparison.OrdinalIgnoreCase))
                AddOpportunityCandidate(strategic);

            TacticalOpportunityResult tacticalAssessment =
                EvaluateTacticalOpportunity(
                    _decision,
                    closedM5);

            if (tacticalAssessment.Allowed)
            {
                TradeOpportunityCandidate tactical =
                    BuildLaneCandidate(
                        closedM5,
                        tacticalAssessment.Lane,
                        _m5Frame.Direction,
                        tacticalAssessment.Quality);

                if (tactical != null)
                    AddOpportunityCandidate(tactical);
            }

            if (_reaction != null &&
                _reaction.Direction != 0 &&
                _reaction.Confidence >=
                LiveReactionStrongThreshold &&
                _m5Frame.Direction !=
                _reaction.Direction)
            {
                TradeOpportunityCandidate reaction =
                    BuildLaneCandidate(
                        closedM5,
                        OpportunityLane.MicroReaction,
                        _reaction.Direction,
                        _reaction.Confidence);

                if (reaction != null)
                    AddOpportunityCandidate(reaction);
            }
        }

        private TradeOpportunityCandidate BuildLaneCandidate(
            int closedM5,
            OpportunityLane lane,
            int direction,
            int quality)
        {
            if (direction != 1 &&
                direction != -1)
                return null;

            ExecutionModel execution =
                BuildExecutionModel(
                    closedM5,
                    direction);

            if (execution == null)
                return null;

            TradeSetupPreview preview =
                BuildTradeSetupPreview(
                    closedM5,
                    execution,
                    lane);

            if (preview == null ||
                !IsFinitePositive(preview.Stop) ||
                !IsFinitePositive(preview.Tp1) ||
                preview.Risk <= 0)
                return null;

            double tp1RR =
                Math.Abs(
                    preview.Tp1 -
                    preview.Entry) /
                Math.Max(
                    Symbol.PipSize,
                    preview.Risk);

            double tp2RR =
                IsFinitePositive(preview.Tp2)
                    ? Math.Abs(
                        preview.Tp2 -
                        preview.Entry) /
                      Math.Max(
                        Symbol.PipSize,
                        preview.Risk)
                    : 0;

            double tp3RR =
                IsFinitePositive(preview.Tp3)
                    ? Math.Abs(
                        preview.Tp3 -
                        preview.Entry) /
                      Math.Max(
                        Symbol.PipSize,
                        preview.Risk)
                    : 0;

            double tp4RR =
                IsFinitePositive(preview.Tp4)
                    ? Math.Abs(
                        preview.Tp4 -
                        preview.Entry) /
                      Math.Max(
                        Symbol.PipSize,
                        preview.Risk)
                    : 0;

            return new TradeOpportunityCandidate
            {
                Id =
                    BuildOpportunityId(
                        lane,
                        direction),
                Lane = lane,
                Direction = direction,
                CreatedM5 = closedM5,
                Quality = Math.Max(
                    0,
                    Math.Min(
                        100,
                        quality > 0
                            ? quality
                            : execution.Quality)),
                Risk = preview.Risk,
                Tp1RR = tp1RR,
                Tp2RR = tp2RR,
                Tp3RR = tp3RR,
                Tp4RR = tp4RR,
                Entry = preview.Entry,
                IdealEntry = preview.IdealEntry,
                Trigger = preview.Trigger,
                Invalidation = preview.Invalidation,
                Stop = preview.Stop,
                Tp1 = preview.Tp1,
                Tp2 = preview.Tp2,
                Tp3 = preview.Tp3,
                Tp4 = preview.Tp4,
                Source = execution.Source,
                Stage =
                    execution.Ready
                        ? "READY"
                        : "WATCH",
                LabelPrefix =
                    LaneText(lane)
            };
        }

        private string BuildOpportunityId(
            OpportunityLane lane,
            int direction)
        {
            return
                LaneText(lane) +
                "_" +
                (direction == 1
                    ? "BUY"
                    : "SELL");
        }

        private string LaneText(
            OpportunityLane lane)
        {
            switch (lane)
            {
                case OpportunityLane.Strategic:
                    return "HTF";
                case OpportunityLane.CounterHtfTactical:
                    return "TACTICAL-COUNTER";
                case OpportunityLane.MicroReaction:
                    return "MICRO";
                default:
                    return "TACTICAL";
            }
        }

        private void AddOpportunityCandidate(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return;

            for (int i = 0;
                 i < _opportunityCandidates.Count;
                 i++)
            {
                TradeOpportunityCandidate existing =
                    _opportunityCandidates[i];

                if (existing.Direction != candidate.Direction)
                    continue;

                double distance =
                    Math.Abs(
                        existing.Entry -
                        candidate.Entry);

                if (distance <
                    Math.Max(
                        Symbol.PipSize * 2,
                        candidate.Risk * 0.10))
                {
                    if (candidate.Quality >
                        existing.Quality)
                        _opportunityCandidates[i] =
                            candidate;

                    return;
                }
            }

            _opportunityCandidates.Add(candidate);

            while (_opportunityCandidates.Count >
                   Math.Max(
                       1,
                       MaximumVisibleOpportunities))
                _opportunityCandidates.RemoveAt(
                    _opportunityCandidates.Count - 1);
        }
    }
}
