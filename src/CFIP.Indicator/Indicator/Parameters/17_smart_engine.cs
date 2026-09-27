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
    }
}
