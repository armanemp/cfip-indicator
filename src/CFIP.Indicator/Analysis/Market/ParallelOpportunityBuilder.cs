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

            int canonicalM15TrendDirection =
                _m15Frame != null
                    ? _m15Frame.TrendBull
                        ? 1
                        : _m15Frame.TrendBear
                            ? -1
                            : 0
                    : 0;

            // In a clear M15 trend, parallel tactical discovery is still allowed,
            // but only on the canonical direction. The opposite direction belongs
            // to the explicit reversal/reaction lifecycle and must not appear as a
            // normal user-facing SELL/BUY signal while the canonical trend persists.
            int[] candidateDirections =
                canonicalM15TrendDirection != 0
                    ? new[] { canonicalM15TrendDirection }
                    : new[] { 1, -1 };

            for (int i = 0; i < candidateDirections.Length; i++)
            {
                int candidateDirection =
                    candidateDirections[i];

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
