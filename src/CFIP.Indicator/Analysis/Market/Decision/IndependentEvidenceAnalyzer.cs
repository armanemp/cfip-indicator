using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int IndependentEvidence(int direction) =>
            IndependentEvidence(_m5Frame, direction);

        private int IndependentEvidenceGroupCount(int direction) =>
            IndependentEvidenceGroupCount(_m5Frame, direction);

        private int CalculateIndependentEvidenceForFrame(Frame frame, int direction)
        {
            return IndependentEvidenceFusionRule.CalculateScore(BuildIndependentEvidenceInput(frame, direction));
        }

        private int CountIndependentEvidenceGroupsForFrame(Frame frame, int direction)
        {
            return IndependentEvidenceFusionRule.CountGroups(BuildIndependentEvidenceInput(frame, direction));
        }

        private IndependentEvidenceFusionInput BuildIndependentEvidenceInput
        }

        private int CountIndependentEvidenceGroupsForFrame(Frame frame, int direction)
        {
            return IndependentEvidenceFusionRule.CountGroups(BuildIndependentEvidenceInput(frame, direction));
        }

        private IndependentEvidenceFusionInput BuildIndependentEvidenceInput(Frame frame, int direction)
        {
            if (frame == null || (direction != 1 && direction != -1))
                return new IndependentEvidenceFusionInput(
                    false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false);

            bool bull = direction == 1;
            return new IndependentEvidenceFusionInput(
                bull ? frame.StructureBull : frame.StructureBear,
                StructuralEvidenceRule.IsIndependentTransition(
                    bull ? frame.StructureBull : frame.StructureBear,
                    bull ? frame.MssBull : frame.MssBear,
                    bull ? frame.ChochBull : frame.ChochBear),
                bull ? frame.DisplacementBull : frame.DisplacementBear,
                bull ? frame.LiquidityBull : frame.LiquidityBear,
                bull ? frame.FvgBull : frame.FvgBear,
                bull ? frame.ObBull : frame.ObBear,
                bull ? frame.TrendBull : frame.TrendBear,
                bull ? frame.MomentumBull : frame.MomentumBear,
                bull ? frame.MacdBull : frame.MacdBear,
                bull ? frame.VwapBull : frame.VwapBear,
                bull ? frame.VolumeBull : frame.VolumeBear,
                bull ? frame.VolatilityBull : frame.VolatilityBear,
                bull ? frame.RejectionBull : frame.RejectionBear,
                bull ? frame.EqualLow : frame.EqualHigh);
        }
    }
}