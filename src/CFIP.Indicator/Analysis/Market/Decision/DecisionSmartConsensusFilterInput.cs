namespace cAlgo
{
    internal readonly struct DecisionSmartConsensusFilterInput
    {
        public bool Enabled { get; }
        public bool RequireConsensus { get; }
        public int StrongestShare { get; }
        public int ConsensusThreshold { get; }
        public int AdaptiveShareThreshold { get; }
        public bool AllowSoftGate { get; }
        public int SmartQuality { get; }
        public int StrongSetupQuality { get; }
        public int Edge { get; }
        public int StrongSetupEdge { get; }
        public int IndependentEvidence { get; }
        public int MinimumIndependentEvidence { get; }
        public int TimeframeAgreement { get; }
        public int MinimumTimeframeAgreement { get; }

        public DecisionSmartConsensusFilterInput(
            bool enabled,
            bool requireConsensus,
            int strongestShare,
            int consensusThreshold,
            int adaptiveShareThreshold,
            bool allowSoftGate,
            int smartQuality,
            int strongSetupQuality,
            int edge,
            int strongSetupEdge,
            int independentEvidence,
            int minimumIndependentEvidence,
            int timeframeAgreement,
            int minimumTimeframeAgreement)
        {
            Enabled = enabled;
            RequireConsensus = requireConsensus;
            StrongestShare = strongestShare;
            ConsensusThreshold = consensusThreshold;
            AdaptiveShareThreshold = adaptiveShareThreshold;
            AllowSoftGate = allowSoftGate;
            SmartQuality = smartQuality;
            StrongSetupQuality = strongSetupQuality;
            Edge = edge;
            StrongSetupEdge = strongSetupEdge;
            IndependentEvidence = independentEvidence;
            MinimumIndependentEvidence = minimumIndependentEvidence;
            TimeframeAgreement = timeframeAgreement;
            MinimumTimeframeAgreement = minimumTimeframeAgreement;
        }
    }
}