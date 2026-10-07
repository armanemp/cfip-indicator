namespace cAlgo
{
    internal sealed class OutcomeObservation
    {
        public long PositionId { get; set; }
        public string SignalTraceId { get; set; }
        public int Direction { get; set; }
        public OpportunityLane Lane { get; set; }
        public ExecutionMode EntryMode { get; set; }
        public string Regime { get; set; }
        public int Confidence { get; set; }
        public int ConfidenceBucket { get; set; }
        public int CreatedM5 { get; set; }
        public int ClosedM5 { get; set; }
        public int LifecycleBars { get; set; }
        public double Pips { get; set; }
        public double NetProfit { get; set; }
        public double RealizedR { get; set; }
        public bool Profitable { get; set; }
        public bool CalibrationEligible { get; set; }
        public bool ProtectionRecoveryAtClose { get; set; }
        public bool ServerSideTakeProfitLadderActive { get; set; }
        public long ObservedUtcTicks { get; set; }
    }
}
