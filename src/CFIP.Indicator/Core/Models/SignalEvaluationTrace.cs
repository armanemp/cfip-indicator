namespace cAlgo
{
    internal sealed class SignalEvaluationTrace
    {
        public string SignalTraceId;
        public long BarOpenTimeUtcTicks;
        public long ObservedUtcTicks;
        public int ClosedM5;

        public double Open;
        public double High;
        public double Low;
        public double Close;

        public int Direction;
        public int BuyShare;
        public int SellShare;
        public int Edge;
        public int BaseConfidence;
        public int Confidence;
        public int SmartQuality;

        public int HtfAnchorDirection;
        public int HtfAlignment;
        public int MidframeDirection;
        public int MidframeAlignment;
        public int EntryFrameAlignment;
        public int TopDownEligible;
        public string TopDownStage;
        public OpportunityLane Lane;

        public int M5BullScore;
        public int M5BearScore;
        public int M5Evidence;
        public int M5Quality;
        public int FvgBullQuality;
        public int FvgBearQuality;
        public int ObBullQuality;
        public int ObBearQuality;
        public int FvgObBullConfluence;
        public int FvgObBearConfluence;
        public int LocationEvidenceBull;
        public int LocationEvidenceBear;

        public int IndicatorConfluenceQuality;
        public int IndicatorConflict;
        public int WaveTrendDirection;
        public int WaveTrendQuality;
        public int DivergenceDirection;
        public int DivergenceQuality;

        public int EntryAllowed;
        public int TriggerReady;
        public int ActionableNow;
        public int EntryLocationQuality;
        public int EntryTimingQuality;
        public int EntryPositionQuality;
        public double EntryDistanceAtr;
        public double ActionableTp1RR;
        public double PlanRiskAtr;
        public long GeometryBarOpenTimeUtcTicks;
        public string GeometrySource;
        public double EffectiveTp1RR;
        public double RequiredTp1RR;

        public ExecutionMode EntryMode;
        public double Entry;
        public double IdealEntry;
        public double Stop;
        public double Tp1;
        public double Tp2;
        public double Tp3;
        public double Tp4;

        public string TraceGate;
        public string BlockReason;
        public string ActionabilityReason;
        public string DecisionReason;
    }
}
