using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Enable Smart Decision Engine", Group = "17 · Smart Engine", DefaultValue = true)]
        public bool EnableSmartDecisionEngine { get; set; }

        [Parameter("Smart Minimum Timeframe Agreement", Group = "17 · Smart Engine", DefaultValue = 72, MinValue = 50, MaxValue = 95)]
        public int SmartMinimumTimeframeAgreement { get; set; }

        [Parameter("Smart Target Minimum RR", Group = "17 · Smart Engine", DefaultValue = 1.50, MinValue = 0.5, MaxValue = 8, Step = 0.05)]
        public double SmartTargetMinimumRR { get; set; }

        [Parameter("Smart Target Max Candidates", Group = "17 · Smart Engine", DefaultValue = 32, MinValue = 8, MaxValue = 64)]
        public int SmartTargetMaxCandidates { get; set; }

        [Parameter("Smart Regime Quality Floor", Group = "17 · Smart Engine", DefaultValue = 55, MinValue = 30, MaxValue = 90)]
        public int SmartRegimeQualityFloor { get; set; }

        [Parameter("No Trade Minimum Smart Quality", Group = "17 · Smart Engine", DefaultValue = 55, MinValue = 30, MaxValue = 90)]
        public int NoTradeMinimumSmartQuality { get; set; }

        [Parameter("Block Compression Regime", Group = "17 · Smart Engine", DefaultValue = true)]
        public bool BlockCompressionRegime { get; set; }

        [Parameter("Block Weak Range Transition", Group = "17 · Smart Engine", DefaultValue = true)]
        public bool BlockWeakRangeTransition { get; set; }

        [Parameter("Use Strict Regime Quality Gate", Group = "17 · Smart Engine", DefaultValue = true)]
        public bool UseStrictRegimeQualityGate { get; set; }

        [Parameter("Minimum Directional Regime Quality", Group = "17 · Smart Engine", DefaultValue = 62, MinValue = 40, MaxValue = 95)]
        public int MinimumDirectionalRegimeQuality { get; set; }

        [Parameter("Minimum Trend Efficiency", Group = "17 · Smart Engine", DefaultValue = 0.32, MinValue = 0.10, MaxValue = 0.80, Step = 0.01)]
        public double MinimumTrendEfficiency { get; set; }

        [Parameter("Maximum Trend Choppiness", Group = "17 · Smart Engine", DefaultValue = 58, MinValue = 35, MaxValue = 80)]
        public int TrendChoppinessThreshold { get; set; }

        [Parameter("Minimum Trend ADX", Group = "17 · Smart Engine", DefaultValue = 20, MinValue = 10, MaxValue = 50)]
        public int MinimumTrendAdx { get; set; }

        [Parameter("Minimum Trend EMA Spread ATR", Group = "17 · Smart Engine", DefaultValue = 0.30, MinValue = 0.05, MaxValue = 2.0, Step = 0.05)]
        public double MinimumTrendSpreadAtr { get; set; }

        [Parameter("Range Choppiness Threshold", Group = "17 · Smart Engine", DefaultValue = 58, MinValue = 45, MaxValue = 80)]
        public int RangeChoppinessThreshold { get; set; }

        [Parameter("Range Efficiency Threshold", Group = "17 · Smart Engine", DefaultValue = 0.30, MinValue = 0.10, MaxValue = 0.60, Step = 0.01)]
        public double RangeEfficiencyThreshold { get; set; }

        [Parameter("Compression ATR Ratio", Group = "17 · Smart Engine", DefaultValue = 0.78, MinValue = 0.50, MaxValue = 1.0, Step = 0.01)]
        public double CompressionAtrRatio { get; set; }

        [Parameter("Expansion ATR Ratio", Group = "17 · Smart Engine", DefaultValue = 1.30, MinValue = 1.05, MaxValue = 3.0, Step = 0.05)]
        public double ExpansionAtrRatio { get; set; }

        [Parameter("High Volatility ATR Ratio", Group = "17 · Smart Engine", DefaultValue = 1.65, MinValue = 1.20, MaxValue = 4.0, Step = 0.05)]
        public double HighVolatilityAtrRatio { get; set; }

        [Parameter("Choppiness Period", Group = "17 · Smart Engine", DefaultValue = 14, MinValue = 10, MaxValue = 40)]
        public int ChoppinessPeriod { get; set; }

        [Parameter("Regime Lookback Bars", Group = "17 · Smart Engine", DefaultValue = 20, MinValue = 10, MaxValue = 50)]
        public int RegimeLookbackBars { get; set; }

        [Parameter("Regime Return Bars", Group = "17 · Smart Engine", DefaultValue = 5, MinValue = 2, MaxValue = 20)]
        public int RegimeReturnBars { get; set; }

        [Parameter("Minimum Transition ADX", Group = "17 · Smart Engine", DefaultValue = 18, MinValue = 10, MaxValue = 40)]
        public int MinimumTransitionAdx { get; set; }
    }
}
