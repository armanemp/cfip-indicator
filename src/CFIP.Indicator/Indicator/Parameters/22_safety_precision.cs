using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Use M5 Structure For Initial Stop", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool UseM5StructureForStop { get; set; }

        [Parameter("Use Zone Mitigation Guard", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool UseZoneMitigationGuard { get; set; }

        [Parameter("Require Order Block Retest", Group = "22 · Safety & Precision", DefaultValue = false)]
        public bool RequireObRetest { get; set; }

        [Parameter("FVG Invalidate On Full Fill (Safety-Enforced)", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool FvgInvalidateOnFullFill { get; set; }

        [Parameter("FVG Partial Mitigation", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool EnableFvgPartialMitigation { get; set; }

        [Parameter("FVG Break By Wicks", Group = "22 · Safety & Precision", DefaultValue = false)]
        public bool FvgBreakByWicks { get; set; }

        [Parameter("Use Semantic Alert Sounds", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool UseSemanticAlertSounds { get; set; }

        [Parameter("Show Spread Diagnostics", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool ShowSpreadDiagnostics { get; set; }
    }
}
