namespace cAlgo
{
    internal sealed class MarketRegimeSnapshot
    {
        public string Regime { get; set; }
        public int Quality { get; set; }
        public int Stability { get; set; }
        public string PreviousRegime { get; set; }
        public string RegimeTransition { get; set; }
        public int Direction { get; set; }
        public double Choppiness { get; set; }
        public double AtrRatio { get; set; }
        public double EmaSpreadAtr { get; set; }
        public double EmaSlopeAtr { get; set; }
        public double RangeEfficiency { get; set; }
        public double RangeWidthAtr { get; set; }
        public double Adx { get; set; }
        public double DmiBias { get; set; }
        public double ReturnAtr { get; set; }
        public string Reason { get; set; }
    }
}