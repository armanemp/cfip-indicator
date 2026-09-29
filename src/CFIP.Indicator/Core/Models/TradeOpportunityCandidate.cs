namespace cAlgo
{
    internal sealed class TradeOpportunityCandidate
    {
        public string Id;
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
    }
}
