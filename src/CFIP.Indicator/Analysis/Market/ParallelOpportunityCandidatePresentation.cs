using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ShouldPresentOpportunityCandidate(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return false;
        
            int minimumQuality;
        
            if (candidate.IsPrimaryTimeframeSignal)
            {
                // Primary M15/H1 source visibility is intentionally based on
                // the primary-source quality floor. M5 actionability/entry
                // readiness is evaluated separately and must not erase the source setup.
                minimumQuality =
                    Math.Max(
                        60,
                        TacticalOpportunityMinimumQuality - 5);
            }
            else
            {
                minimumQuality =
                    Math.Max(
                        TacticalOpportunityMinimumQuality,
                        Math.Max(
                            MinimumSmartQuality,
                            SmartQualityThreshold));
            }
        
            if (!candidate.IsPrimaryTimeframeSignal &&
                candidate.Lane ==
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
            if (!candidate.IsPrimaryTimeframeSignal &&
                candidate.Lane != OpportunityLane.CounterHtfTactical &&
                candidate.Lane != OpportunityLane.MicroReaction)
                minimumQuality =
                    ActionabilityThresholdPolicy.ApplyParallelCandidateQualityMargin(
                        minimumQuality,
                        true);
        
            if (candidate.PresentationOnly)
                return candidate.Quality >= minimumQuality;
        
            // A trade-facing opportunity must carry a concrete, regime-appropriate
            // reward distance. Zero/unknown reward is not a presentable opportunity.
            if (!IsFinitePositive(candidate.RewardDistanceAtr) ||
                !IsFinitePositive(candidate.MinimumRequiredRewardDistanceAtr) ||
                candidate.RewardDistanceAtr <
                    candidate.MinimumRequiredRewardDistanceAtr)
                return false;
        
            if (candidate.Tp1RR <
                Math.Max(
                    2.0,
                    Tp1MinimumRR))
                return false;
        
            return candidate.Quality >= minimumQuality;
        }
        
        
    }
}
