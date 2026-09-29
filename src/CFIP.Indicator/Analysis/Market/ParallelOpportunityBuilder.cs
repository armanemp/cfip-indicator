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
            _tradePlanRegistry.Clear();

            if (!EnableParallelOpportunities ||
                _m5Bars == null ||
                _m5Frame == null ||
                closedM5 < 30)
                return;

            AddTimeframeScenarioCandidates(
                closedM5);

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
                    StringComparison.OrdinalIgnoreCase) &&
                ShouldPresentOpportunityCandidate(
                    strategic))
                AddOpportunityCandidate(strategic);

            for (int candidateDirection = 1;
                 candidateDirection >= -1;
                 candidateDirection -= 2)
            {
                TacticalOpportunityResult tacticalAssessment =
                    EvaluateTacticalOpportunityForDirection(
                        _decision,
                        closedM5,
                        candidateDirection);

                if (!tacticalAssessment.Allowed)
                    continue;

                TradeOpportunityCandidate tactical =
                    BuildLaneCandidate(
                        closedM5,
                        tacticalAssessment.Lane,
                        candidateDirection,
                        tacticalAssessment.Quality);

                if (tactical != null &&
                    ShouldPresentOpportunityCandidate(
                        tactical))
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

                if (reaction != null &&
                    ShouldPresentOpportunityCandidate(
                        reaction))
                    AddOpportunityCandidate(reaction);
            }

            TrimOpportunityCandidates();

            for (int i = 0;
                 i < _opportunityCandidates.Count;
                 i++)
            {
                _tradePlanRegistry.Upsert(
                    _opportunityCandidates[i]);
            }
        }

        private bool ShouldPresentOpportunityCandidate(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return false;

            int minimumQuality =
                Math.Max(
                    TacticalOpportunityMinimumQuality,
                    Math.Max(
                        MinimumSmartQuality,
                        SmartQualityThreshold));

            if (candidate.Lane ==
                OpportunityLane.Strategic)
            {
                minimumQuality =
                    Math.Max(
                        minimumQuality,
                        MinimumConfidence);
            }
            else if (candidate.Lane ==
                     OpportunityLane.CounterHtfTactical)
            {
                minimumQuality =
                    Math.Max(
                        minimumQuality,
                        CounterHtfMinimumQuality);
            }
            else if (candidate.Lane ==
                     OpportunityLane.MicroReaction)
            {
                minimumQuality =
                    Math.Max(
                        minimumQuality,
                        LiveReactionStrongThreshold);
            }

            // Keep parallel detection available, but do not crowd the chart
            // with candidates that are materially below the active quality floor.
            if (candidate.Lane != OpportunityLane.CounterHtfTactical &&
                candidate.Lane != OpportunityLane.MicroReaction)
                minimumQuality =
                    Math.Min(
                        95,
                        minimumQuality + 3);

            return candidate.Quality >= minimumQuality;
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

            TradeActionabilityResult actionability =
                EvaluateTradeActionability(
                    closedM5,
                    direction,
                    lane,
                    _decision == null ? "UNKNOWN" : _decision.Regime,
                    execution,
                    preview);

            PlanRewardRiskQualityResult rewardRisk =
                PlanRewardRiskQualityRule.Evaluate(
                    direction,
                    preview.Entry,
                    preview.Stop,
                    preview.Tp1,
                    Math.Max(
                        Symbol.PipSize,
                        Math.Abs(
                            preview.Entry -
                            preview.Stop)),
                    Math.Max(
                        0,
                        Symbol.Ask - Symbol.Bid),
                    MinimumRequiredRRForRegime(
                        _decision == null
                            ? "UNKNOWN"
                            : _decision.Regime),
                    PreferredStopRiskAtr,
                    Math.Min(
                        Math.Max(
                            MinimumSlAtr,
                            MaximumSlAtr),
                        Math.Max(
                            MinimumSlAtr,
                            MaximumStructuralStopAtr)));

            if (!rewardRisk.Allowed)
                return null;

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
                    actionability.Actionable
                        ? "READY"
                        : "WATCH • " +
                          actionability.Reason,
                LabelPrefix =
                    LaneText(lane),
                ActionableNow =
                    actionability.Actionable,
                EntryDistanceAtr =
                    actionability.EntryDistanceAtr,
                DivergenceQuality =
                    actionability.DivergenceQuality,
                DivergenceType =
                    actionability.DivergenceType,
                ActionabilityReason =
                    actionability.Reason
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

                bool hasSourceTimeframe =
                    !string.IsNullOrWhiteSpace(
                        existing.SourceTimeframe) ||
                    !string.IsNullOrWhiteSpace(
                        candidate.SourceTimeframe);

                if (hasSourceTimeframe &&
                    !string.Equals(
                        existing.SourceTimeframe,
                        candidate.SourceTimeframe,
                        StringComparison.OrdinalIgnoreCase))
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
                    bool existingStrategic =
                        existing.Lane ==
                        OpportunityLane.Strategic;

                    bool candidateStrategic =
                        candidate.Lane ==
                        OpportunityLane.Strategic;

                    if (candidateStrategic &&
                        !existingStrategic)
                    {
                        _opportunityCandidates[i] =
                            candidate;
                    }
                    else if (!existingStrategic &&
                             !candidateStrategic &&
                             candidate.Quality >
                             existing.Quality)
                    {
                        _opportunityCandidates[i] =
                            candidate;
                    }

                    return;
                }
            }

            _opportunityCandidates.Add(candidate);

        }
    }
}
