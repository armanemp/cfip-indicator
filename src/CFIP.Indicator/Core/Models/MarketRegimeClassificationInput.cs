namespace cAlgo
{
    internal sealed class MarketRegimeClassificationInput
    {
        public double AtrRatio { get; set; }
        public double Choppiness { get; set; }
        public double RangeEfficiency { get; set; }
        public double RangeWidthAtr { get; set; }
        public double ReturnAtr { get; set; }
        public double Adx { get; set; }
        public double EmaSpreadAtr { get; set; }
    }
}