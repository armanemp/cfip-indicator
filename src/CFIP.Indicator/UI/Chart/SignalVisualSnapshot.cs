namespace cAlgo
{
    internal sealed class SignalVisualSnapshot
    {
        public int ClosedM5;
        public int AuthoritativeDirection;
        public int PlanDirection;
        public int PendingDirection;
        public int DecisionDirection;
        public int HtfAnchorDirection;
        public int HtfAlignment;
        public int MidframeDirection;
        public int MidframeAlignment;
        public int EntryFrameAlignment;
        public bool TopDownEligible;
        public string TopDownStage;
        public int ReactionDirection;
        public string Stage;
        public bool DecisionReady;
        public bool DecisionEntryAllowed;
        public bool ActionableNow;
        public bool ReactionReady;
        public bool ReactionIntrabar;
        public int ReactionM5Index;
        public bool PredictionReady;
        public bool PlanActive;
        public bool LivePosition;
        public bool PendingOrder;
        public bool SetupPreviewActive;
        public bool TriggerVisible;
        public bool TriggerRuntimeReady;
        public int TriggerM1Index;
        public int TriggerRuntimeScore;
        public int TriggerRuntimeRequired;
        public string TriggerRuntimeReason;
        public int ArrowM5Index;
        public bool IdealEntryVisible;
        public bool ActiveBrokerTargetVisible;

        public ExecutionMode EntryMode;
        public ExecutionMode SetupEntryMode;
        public int CreatedM5;
        public int SetupCreatedM5;
        public long PositionId;
        public long PendingOrderId;
        public string PendingOrderType;

        public double Entry;
        public double IdealEntry;
        public double Trigger;
        public double Invalidation;
        public double Stop;
        public double BrokerStop;
        public double Tp1;
        public double Tp2;
        public double Tp3;
        public double Tp4;
        public double BrokerTarget;

        public double SetupEntry;
        public double SetupIdealEntry;
        public double SetupTrigger;
        public double SetupInvalidation;
        public double SetupStop;
        public double SetupTp1;
        public double SetupTp2;
        public double SetupTp3;
        public double SetupTp4;
        public double SetupRisk;

        public double PendingEntry;
        public double PendingStop;
        public double PendingTarget;

        public int SmartQuality;
        public int TimeframeAgreement;
        public int IndependentEvidence;
        public int StructuralConfirmations;
        public int Confidence;
        public int EntryLocationQuality;
        public int EntryTimingQuality;
        public int EntryPositionQuality;
        public double EntryDistanceAtr;
        public double ActionableTp1RR;
        public int DivergenceDirection;
        public int DivergenceQuality;
        public string DivergenceType;
        public string ActionabilityReason;
        public int BaseConfidence;
        public int CalibratedConfidence;
        public int EmpiricalCalibrationAdjustment;
        public int EmpiricalCalibrationSamples;
        public int EmpiricalCalibrationWins;
        public int EmpiricalCalibrationBucket;
        public double EmpiricalCalibrationObservedWinRate;
        public string EmpiricalCalibrationSource;
        public string DecisionReason;
        public string ReactionReason;
        public int ReactionConfidence;
        public string Regime;
    }
}