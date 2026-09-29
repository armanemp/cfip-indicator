namespace cAlgo
{
    internal sealed class SignalVisualSnapshot
    {
        public int ClosedM5;
        public int Direction;
        public string Stage;
        public bool DecisionReady;
        public bool ReactionReady;
        public bool PredictionReady;
        public bool PlanActive;
        public bool LivePosition;
        public bool PendingOrder;
        public bool TriggerVisible;
        public int ArrowM5Index;
        public bool IdealEntryVisible;
        public bool ActiveBrokerTargetVisible;

        public ExecutionMode EntryMode;
        public int CreatedM5;
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
        public double PendingEntry;
        public double PendingStop;
        public double PendingTarget;

        public int SmartQuality;
        public int Confidence;
        public string DecisionReason;
        public string ReactionReason;
        public int ReactionConfidence;
        public string Regime;
    }
}