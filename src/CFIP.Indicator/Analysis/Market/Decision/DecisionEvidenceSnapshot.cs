namespace cAlgo
{
    internal sealed class DecisionEvidenceSnapshot
    {
        public int TimeframeAgreement { get; }
        public int IndependentEvidence { get; }
        public int StructuralConfirmations { get; }
        public int RegimeQuality { get; }
        public int RetestQuality { get; }
        public bool ClosedBarTriggerReady { get; }
        public int BullConfidenceAdjustment { get; }
        public int BearConfidenceAdjustment { get; }
        public int HigherTimeframePenalty { get; }

        public DecisionEvidenceSnapshot(
            int timeframeAgreement,
            int independentEvidence,
            int structuralConfirmations,
            int regimeQuality,
            int retestQuality,
            bool closedBarTriggerReady,
            int bullConfidenceAdjustment,
            int bearConfidenceAdjustment,
            int higherTimeframePenalty)
        {
            TimeframeAgreement = NumericGuards.ClampInt(timeframeAgreement, 0, 100);
            IndependentEvidence = Math.Max(0, independentEvidence);
            StructuralConfirmations = Math.Max(0, structuralConfirmations);
            RegimeQuality = NumericGuards.ClampInt(regimeQuality, 0, 100);
            RetestQuality = NumericGuards.ClampInt(retestQuality, 0, 100);
            ClosedBarTriggerReady = closedBarTriggerReady;
            BullConfidenceAdjustment = NumericGuards.ClampInt(bullConfidenceAdjustment, -100, 100);
            BearConfidenceAdjustment = NumericGuards.ClampInt(bearConfidenceAdjustment, -100, 100);
            HigherTimeframePenalty = Math.Max(0, higherTimeframePenalty);
        }
    }
}