using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Aggressive Minimum Confidence", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 88, MinValue = 50, MaxValue = 99)]
        public int AggressiveMinimumConfidence { get; set; }

        [Parameter("Aggressive Minimum Evidence", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 4, MinValue = 1, MaxValue = 8)]
        public int AggressiveMinimumEvidence { get; set; }

        [Parameter("Aggressive Minimum Smart Quality", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 78, MinValue = 50, MaxValue = 95)]
        public int AggressiveMinimumSmartQuality { get; set; }

        [Parameter("Aggressive Risk % Equity", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 0.25, MinValue = 0.05, MaxValue = 5)]
        public double AggressiveRiskPercentEquity { get; set; }

        [Parameter("Aggressive TP Stage", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = TargetStage.TP1)]
        public TargetStage AggressiveTpStage { get; set; }

        [Parameter("Aggressive Require Smart Agreement", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = true)]
        public bool AggressiveRequireSmartAgreement { get; set; }
    }
}
