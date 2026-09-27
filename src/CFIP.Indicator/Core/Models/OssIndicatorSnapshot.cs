namespace cAlgo
{
    internal sealed class OssIndicatorSnapshot
    {
        public int BullVotes { get; set; }
        public int BearVotes { get; set; }
        public int IndicatorCount { get; set; }
        public double Rsi { get; set; }
        public double MacdHistogram { get; set; }
        public double BollingerPercentB { get; set; }
        public double Mfi { get; set; }
        public double StochK { get; set; }
        public double StochD { get; set; }
        public double SuperTrend { get; set; }
    }
}
