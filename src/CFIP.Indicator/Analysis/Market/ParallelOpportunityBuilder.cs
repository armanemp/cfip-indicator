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

            AddFuturePendingOpportunityCandidates(
                closedM5);

            if (_reaction != null &&
                MicroReactionSafetyRule.IsClosedBarSafe(
                    closedM5,
                    _reaction.ReactionConfirmedM5,
                    _reaction.Direction,
                    _reaction.ReactionConfirmedDirection,
                    _reaction.ReactionConfirmedQuality,
                    _reaction.ReactionClosedBarConfirmed,
                    LiveReactionStrongThreshold) &&
                _m5Frame.Direction !=
                _reaction.Direction)
            {
                TradeOpportunityCandidate reaction =
                    BuildLaneCandidate(
                        closedM5,
                        OpportunityLane.MicroReaction,
                        _reaction.Direction,
                        _reaction.ReactionConfirmedQuality);

                if (reaction != null &&
                    ShouldPresentOpportunityCandidate(
                        reaction))
                    AddOpportunityCandidate(reaction);
            }

            TrimOpportunityCandidates();

        }

    }
}
