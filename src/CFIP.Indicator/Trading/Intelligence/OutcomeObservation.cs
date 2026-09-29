namespace cAlgo
{
    internal sealed class OutcomeObservation
    {
        public long PositionId;
        public int Direction;
        public OpportunityLane Lane;
        public ExecutionMode EntryMode;
        public string Regime;
        public int Confidence;
        public int ConfidenceBucket;
        public int CreatedM5;
        public int ClosedM5;
        public int LifecycleBars;
        public double Pips;
        public double NetProfit;
        public double RealizedR;
        public bool Profitable;
        public bool CalibrationEligible;
        public bool ProtectionRecoveryAtClose;
        public bool ServerSideTakeProfitLadderActive;
        public long ObservedUtcTicks;
    }
}
