using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Minimum Confidence", Group = "01 · Decision", DefaultValue = 72, MinValue = 50, MaxValue = 99)]
        public int MinimumConfidence { get; set; }

        [Parameter("Minimum Edge", Group = "01 · Decision", DefaultValue = 15, MinValue = 0, MaxValue = 50)]
        public int MinimumEdge { get; set; }

        [Parameter("Minimum Smart Quality", Group = "01 · Decision", DefaultValue = 70, MinValue = 40, MaxValue = 95)]
        public int MinimumSmartQuality { get; set; }

        [Parameter("Minimum Structural Confirmations", Group = "01 · Decision", DefaultValue = 4, MinValue = 1, MaxValue = 8)]
        public int MinimumStructuralConfirmations { get; set; }

        [Parameter("Minimum Independent Evidence", Group = "01 · Decision", DefaultValue = 4, MinValue = 2, MaxValue = 8)]
        public int MinimumIndependentEvidence { get; set; }

        [Parameter("Minimum Timeframe Agreement", Group = "01 · Decision", DefaultValue = 72, MinValue = 50, MaxValue = 100)]
        public int MinimumTimeframeAgreement { get; set; }

        [Parameter("Require Higher TF Agreement", Group = "01 · Decision", DefaultValue = true)]
        public bool RequireHigherTfAgreement { get; set; }

        [Parameter("Require Core Agreement", Group = "01 · Decision", DefaultValue = true)]
        public bool RequireCoreAgreement { get; set; }

        [Parameter("Require Structural Confirmation", Group = "01 · Decision", DefaultValue = true)]
        public bool RequireStructuralConfirmation { get; set; }

        [Parameter("Use Advanced Confluence", Group = "01 · Decision", DefaultValue = true)]
        public bool UseAdvancedConfluence { get; set; }

        [Parameter("Allow Strong Trigger Override", Group = "01 · Decision", DefaultValue = true)]
        public bool AllowStrongTriggerOverride { get; set; }
    }
}
