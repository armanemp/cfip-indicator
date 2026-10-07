namespace cAlgo
{
    internal readonly struct DecisionThresholdFilterInput
    {
        public int Confidence { get; }
        public int Edge { get; }
        public int SmartQuality { get; }
        public int TimeframeAgreement { get; }
        public int IndependentEvidence { get; }
        public int IndependentEvidenceGroups { get; }
        public int StructuralConfirmations { get; }

        public int MinimumConfidence { get; }
        public int MinimumEdge { get; }
        public int MinimumSmartQuality { get; }
        public bool RequireHigherTfAgreement { get; }
        public int MinimumTimeframeAgreement { get; }
        public int MinimumIndependentEvidence { get; }
        public int SmartMinimumIndependentEvidence { get; }
        public bool SmartDecisionEnabled { get; }
        public bool RequireStructuralConfirmation { get; }
        public int MinimumStructuralConfirmations { get; }
        public int MinimumIndependentEvidenceGroups { get; }

        public DecisionThresholdFilterInput(
            int confidence,
            int edge,
            int smartQuality,
            int timeframeAgreement,
            int independentEvidence,
            int structuralConfirmations,
            int minimumConfidence,
            int minimumEdge,
            int minimumSmartQuality,
            bool requireHigherTfAgreement,
            int minimumTimeframeAgreement,
            int minimumIndependentEvidence,
            int smartMinimumIndependentEvidence,
            bool smartDecisionEnabled,
            bool requireStructuralConfirmation,
            int minimumStructuralConfirmations,
            int minimumIndependentEvidenceGroups = 0)
        {
            Confidence = confidence;
            Edge = edge;
            SmartQuality = smartQuality;
            TimeframeAgreement = timeframeAgreement;
            IndependentEvidence = independentEvidence;
            IndependentEvidenceGroups = 0;
            StructuralConfirmations = structuralConfirmations;
            MinimumConfidence = minimumConfidence;
            MinimumEdge = minimumEdge;
            MinimumSmartQuality = minimumSmartQuality;
            RequireHigherTfAgreement = requireHigherTfAgreement;
            MinimumTimeframeAgreement = minimumTimeframeAgreement;
            MinimumIndependentEvidence = minimumIndependentEvidence;
            SmartMinimumIndependentEvidence = smartMinimumIndependentEvidence;
            SmartDecisionEnabled = smartDecisionEnabled;
            RequireStructuralConfirmation = requireStructuralConfirmation;
            MinimumStructuralConfirmations = minimumStructuralConfirmations;
            MinimumIndependentEvidenceGroups = minimumIndependentEvidenceGroups;
        }

        public DecisionThresholdFilterInput WithIndependentEvidenceGroups(
            int independentEvidenceGroups)
        {
            return new DecisionThresholdFilterInput(
                Confidence,
                Edge,
                SmartQuality,
                TimeframeAgreement,
                IndependentEvidence,
                StructuralConfirmations,
                MinimumConfidence,
                MinimumEdge,
                MinimumSmartQuality,
                RequireHigherTfAgreement,
                MinimumTimeframeAgreement,
                MinimumIndependentEvidence,
                SmartMinimumIndependentEvidence,
                SmartDecisionEnabled,
                RequireStructuralConfirmation,
                MinimumStructuralConfirmations,
                MinimumIndependentEvidenceGroups)
            {
                IndependentEvidenceGroups = independentEvidenceGroups
            };
        }
    }
}