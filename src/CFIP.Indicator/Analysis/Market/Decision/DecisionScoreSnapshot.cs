namespace cAlgo
{
    internal readonly struct DecisionScoreSnapshot
    {
        public double Buy { get; }
        public double Sell { get; }

        public DecisionScoreSnapshot(double buy, double sell)
        {
            Buy = buy;
            Sell = sell;
        }
    }
}