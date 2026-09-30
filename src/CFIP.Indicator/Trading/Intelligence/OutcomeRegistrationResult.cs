namespace cAlgo
{
    internal readonly struct OutcomeRegistrationResult
    {
        public bool Recorded { get; }
        public bool Profitable { get; }
        public double NetProfit { get; }
        public double RealizedR { get; }
        public int HistoricalTradeCount { get; }

        public OutcomeRegistrationResult(
            bool recorded,
            bool profitable,
            double netProfit,
            double realizedR,
            int historicalTradeCount)
        {
            Recorded = recorded;
            Profitable = profitable;
            NetProfit = netProfit;
            RealizedR = realizedR;
            HistoricalTradeCount =
                System.Math.Max(
                    0,
                    historicalTradeCount);
        }

        public static OutcomeRegistrationResult NotRecorded
        {
            get
            {
                return new OutcomeRegistrationResult(
                    false,
                    false,
                    0,
                    0,
                    0);
            }
        }
    }
}
