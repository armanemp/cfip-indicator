using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Enable Parallel Opportunities", Group = "27 · PARALLEL OPPORTUNITIES", DefaultValue = true)]
        public bool EnableParallelOpportunities { get; set; }

        [Parameter("Maximum Visible Opportunities", Group = "27 · PARALLEL OPPORTUNITIES", DefaultValue = 6, MinValue = 2, MaxValue = 8)]
        public int MaximumVisibleOpportunities { get; set; }

        [Parameter("Tactical Opportunity Minimum Quality", Group = "27 · PARALLEL OPPORTUNITIES", DefaultValue = 70, MinValue = 50, MaxValue = 95)]
        public int TacticalOpportunityMinimumQuality { get; set; }

        [Parameter("Tactical Opportunity Minimum RR", Group = "27 · PARALLEL OPPORTUNITIES", DefaultValue = 1.75, MinValue = 1.0, MaxValue = 6.0, Step = 0.05)]
        public double TacticalOpportunityMinimumRR { get; set; }

        [Parameter("Counter HTF Minimum Quality", Group = "27 · PARALLEL OPPORTUNITIES", DefaultValue = 82, MinValue = 60, MaxValue = 99)]
        public int CounterHtfMinimumQuality { get; set; }

        [Parameter("Counter HTF Minimum RR", Group = "27 · PARALLEL OPPORTUNITIES", DefaultValue = 2.20, MinValue = 1.25, MaxValue = 8.0, Step = 0.05)]
        public double CounterHtfMinimumRR { get; set; }

        [Parameter("Show Tactical Opportunity Labels", Group = "27 · PARALLEL OPPORTUNITIES", DefaultValue = true)]
        public bool ShowTacticalOpportunityLabels { get; set; }
    }
}
