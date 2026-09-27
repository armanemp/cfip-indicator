using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Enable Market Suitability Guard", Group = "24 · SMART EXECUTION", DefaultValue = true)]
        public bool EnableMarketSuitabilityGuard { get; set; }

        [Parameter("Minimum Market Suitability", Group = "24 · SMART EXECUTION", DefaultValue = 68, MinValue = 50, MaxValue = 95)]
        public int MinimumMarketSuitability { get; set; }

        [Parameter("Hard Market Suitability Gate", Group = "24 · SMART EXECUTION", DefaultValue = true)]
        public bool HardMarketSuitabilityGate { get; set; }

        [Parameter("Require Session Suitability", Group = "24 · SMART EXECUTION", DefaultValue = false)]
        public bool RequireSessionSuitability { get; set; }

        [Parameter("Use Smart Risk Scaling", Group = "24 · SMART EXECUTION", DefaultValue = true)]
        public bool UseSmartRiskScaling { get; set; }

        [Parameter("Minimum Smart Risk Multiplier", Group = "24 · SMART EXECUTION", DefaultValue = 0.55, MinValue = 0.25, MaxValue = 1.0, Step = 0.05)]
        public double MinimumSmartRiskMultiplier { get; set; }

        [Parameter("Full Risk Confidence Threshold", Group = "24 · SMART EXECUTION", DefaultValue = 94, MinValue = 70, MaxValue = 99)]
        public int FullRiskConfidenceThreshold { get; set; }

        [Parameter("Full Risk Suitability Threshold", Group = "24 · SMART EXECUTION", DefaultValue = 88, MinValue = 60, MaxValue = 100)]
        public int FullRiskSuitabilityThreshold { get; set; }

        [Parameter("Penalize Choppy Regime Risk", Group = "24 · SMART EXECUTION", DefaultValue = true)]
        public bool PenalizeChoppyRegimeRisk { get; set; }

        [Parameter("Suitability Recalculation Seconds", Group = "24 · SMART EXECUTION", DefaultValue = 2, MinValue = 1, MaxValue = 10)]
        public int SuitabilityRecalculationSeconds { get; set; }
    }
}
