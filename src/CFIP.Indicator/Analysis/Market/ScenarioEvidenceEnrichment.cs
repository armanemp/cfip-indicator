using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void EnrichScenarioEvidence(
            TradeOpportunityCandidate candidate, Frame frame, int direction)
        {
            if (candidate == null || frame == null || (direction != 1 && direction != -1))
                return;

            candidate.IndependentEvidenceScore = CalculateIndependentEvidenceForFrame(frame, direction);
            candidate.IndependentEvidenceGroupCount = CountIndependentEvidenceGroupsForFrame(frame, direction);

            LocationEvidenceScore location = direction == 1
                ? LocationEvidenceRule.Evaluate(frame.FvgBull, frame.FvgBullQuality, frame.ObBull, frame.ObBullQuality, frame.FvgObBullConfluence)
                : LocationEvidenceRule.Evaluate(frame.FvgBear, frame.FvgBearQuality, frame.ObBear, frame.ObBearQuality, frame.FvgObBearConfluence);

            candidate.LocationConfluenceScore = location.Score;
            candidate.SourceFvgQuality =
                direction == 1
                    ? Math.Max(0, frame.FvgBullQuality)
                    : Math.Max(0, frame.FvgBearQuality);
            candidate.SourceOrderBlockQuality =
                direction == 1
                    ? Math.Max(0, frame.ObBullQuality)
                    : Math.Max(0, frame.ObBearQuality);
            candidate.SourceFvgObConfluence =
                direction == 1
                    ? frame.FvgObBullConfluence
                    : frame.FvgObBearConfluence;
            candidate.PrimaryLocationConfluence =
                location.Confluence;
            candidate.PrimaryLocationQuality =
                location.Score;
            candidate.WaveTrendQuality = frame.WaveTrendQuality;
            candidate.IndicatorIndependentEvidenceGroupCount = frame.IndicatorIndependentEvidenceGroupCount;
        }
    }
}