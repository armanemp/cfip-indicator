using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void EnrichScenarioEvidence(
            TradeOpportunityCandidate candidate,
            Frame frame,
            int direction)
        {
            if (candidate == null ||
                frame == null ||
                (direction != 1 &&
                 direction != -1))
                return;

            int independent = 0;
            int location = 0;

            if (direction == 1)
            {
                if (frame.StructureBull) independent++;
                if (frame.MssBull) independent++;
                if (frame.ChochBull) independent++;
                if (frame.DisplacementBull) independent++;
                if (frame.LiquidityBull) independent++;
                if (frame.VolumeBull) independent++;
                if (frame.MacdBull) independent++;
                if (frame.VwapBull) independent++;
                if (frame.WaveTrendBull) independent++;

                location = Math.Max(frame.FvgBullQuality, frame.ObBullQuality);
                if (frame.FvgObBullConfluence)
                    location = Math.Min(100, location + 12);
            }
            else
            {
                if (frame.StructureBear) independent++;
                if (frame.MssBear) independent++;
                if (frame.ChochBear) independent++;
                if (frame.DisplacementBear) independent++;
                if (frame.LiquidityBear) independent++;
                if (frame.VolumeBear) independent++;
                if (frame.MacdBear) independent++;
                if (frame.VwapBear) independent++;
                if (frame.WaveTrendBear) independent++;

                location = Math.Max(frame.FvgBearQuality, frame.ObBearQuality);
                if (frame.FvgObBearConfluence)
                    location = Math.Min(100, location + 12);
            }

            candidate.IndependentEvidenceScore = independent;
            candidate.LocationConfluenceScore = location;
            candidate.WaveTrendQuality = frame.WaveTrendQuality;
        }
    }
}
