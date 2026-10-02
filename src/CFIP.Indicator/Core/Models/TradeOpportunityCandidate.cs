namespace cAlgo
{
    internal sealed class TradeOpportunityCandidate
    {
        public string Id;
        public string ScenarioId;
        public string SourceTimeframe;
        public string BasePlanTimeframe;
        public int IndependentEvidenceScore;
        public int IndependentEvidenceGroupCount;
        public int IndicatorIndependentEvidenceGroupCount;
        public int LocationConfluenceScore;
        public int WaveTrendQuality;
        public bool ExecutionPolicyAllowed;
        public string ExecutionPolicyReason;
        public bool PrimarySignal;
        public bool PrimarySignalReady;
        public string SignalRole;
        public string LowerTimeframeTuning;
        public int LowerTimeframeTuningAgreement;
        public bool LowerTimeframeConflict;
        public string PrimaryLevelEvidence;
        public int PrimaryLevelEvidenceScore;
        public int PrimaryFvgQuality;
        public int PrimaryObQuality;
        public bool PrimaryObFvgConfluence;
        public OpportunityLane Lane;
        public int Direction;
        public int CreatedM5;
        public int Quality;
        public double Risk;
        public double Tp1RR;
        public double Tp2RR;
        public double Tp3RR;
        public double Tp4RR;

        public double Entry;
        public double IdealEntry;
        public double Trigger;
        public double Invalidation;
        public double Stop;
        public double Tp1;
        public double Tp2;
        public double Tp3;
        public double Tp4;

        public string Source;
        public string Stage;
        public string LabelPrefix;
        public bool ActionableNow;
        public double EntryDistanceAtr;
        public int DivergenceQuality;
        public string DivergenceType;
        public string ActionabilityReason;
    }
}
