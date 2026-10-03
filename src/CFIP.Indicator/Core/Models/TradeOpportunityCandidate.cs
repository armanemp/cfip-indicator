namespace cAlgo
{
    internal sealed class TradeOpportunityCandidate
    {
        public string Id;
        public string ScenarioId;
        public string SourceTimeframe;
        public string BasePlanTimeframe;
        public bool IsPrimaryTimeframeSignal;
        public bool PresentationOnly;
        public bool M5TuningAligned;
        public bool M1TuningConfirmed;
        public ExecutionMode ExecutionMode;
        public string PrimarySignalState;
        public int IndependentEvidenceScore;
        public int IndependentEvidenceGroupCount;
        public int IndicatorIndependentEvidenceGroupCount;
        public int LocationConfluenceScore;
        public int SourceFvgQuality;
        public int SourceOrderBlockQuality;
        public bool SourceFvgObConfluence;
        public bool PrimaryLocationConfluence;
        public int PrimaryLocationQuality;
        public int WaveTrendQuality;
        public int VolumeProfileQuality;
        public bool VolumeProfileConfluence;
        public string VolumeProfileLocation;
        public double VolumeProfilePoc;
        public double VolumeProfileValueAreaLow;
        public double VolumeProfileValueAreaHigh;
        public bool ExecutionPolicyAllowed;
        public string ExecutionPolicyReason;
        public OpportunityLane Lane;
        public int Direction;
        public int CreatedM5;
        public int Quality;
        public double Risk;
        public double Tp1RR;
        public double Tp2RR;
        public double Tp3RR;
        public double Tp4RR;
        public double RequestedVolume;

        public double Entry;
        public double IdealEntry;
        public double Trigger;
        public double Invalidation;
        public double Stop;
        public double Tp1;
        public double Tp2;
        public double Tp3;
        public double Tp4;
        public double ZoneLow;
        public double ZoneHigh;
        public double ZoneTolerance;

        // Intrabar opportunity state. Structural geometry is reused; these fields
        // are refreshed from the live quote so scenarios can enter/arm without
        // waiting for the next closed M5 bar.
        public bool FutureOrderReady;
        public double FutureOrderDistanceAtr;
        public string FutureOrderSource;

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
