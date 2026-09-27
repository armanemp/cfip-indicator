using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Use FVG", Group = "04 · Zones", DefaultValue = true)]
        public bool UseFvg { get; set; }

        [Parameter("FVG Lookback", Group = "04 · Zones", DefaultValue = 24, MinValue = 5, MaxValue = 80)]
        public int FvgLookback { get; set; }

        [Parameter("Minimum FVG ATR", Group = "04 · Zones", DefaultValue = 0.08, MinValue = 0.01, MaxValue = 1)]
        public double MinimumFvgAtr { get; set; }

        [Parameter("Use Order Block", Group = "04 · Zones", DefaultValue = true)]
        public bool UseOrderBlock { get; set; }

        [Parameter("OB Lookback", Group = "04 · Zones", DefaultValue = 30, MinValue = 5, MaxValue = 100)]
        public int ObLookback { get; set; }

        [Parameter("OB Displacement ATR", Group = "04 · Zones", DefaultValue = 0.60, MinValue = 0.1, MaxValue = 3)]
        public double ObDisplacementAtr { get; set; }

        [Parameter("OB Use Body For Zone", Group = "04 · Zones", DefaultValue = false)]
        public bool ObUseBodyForZone { get; set; }

        [Parameter("OB Break By Wicks", Group = "04 · Zones", DefaultValue = false)]
        public bool ObBreakByWicks { get; set; }

        [Parameter("OB Structure Lookback", Group = "04 · Zones", DefaultValue = 8, MinValue = 3, MaxValue = 40)]
        public int ObStructureLookback { get; set; }

        [Parameter("OB Impulse Bars", Group = "04 · Zones", DefaultValue = 4, MinValue = 2, MaxValue = 8)]
        public int ObImpulseBars { get; set; }

        [Parameter("OB Minimum Quality", Group = "04 · Zones", DefaultValue = 68, MinValue = 50, MaxValue = 100)]
        public int ObMinimumQuality { get; set; }

        [Parameter("Require FVG Retest", Group = "04 · Zones", DefaultValue = true)]
        public bool RequireFvgRetest { get; set; }

        [Parameter("Use 2-Bar Imbalance FVG", Group = "04 · Zones", DefaultValue = true)]
        public bool UseTwoBarImbalanceFvg { get; set; }

        [Parameter("Require OB Displacement", Group = "04 · Zones", DefaultValue = true)]
        public bool RequireObDisplacement { get; set; }

        [Parameter("Zone Proximity ATR", Group = "04 · Zones", DefaultValue = 0.25, MinValue = 0.05, MaxValue = 1)]
        public double ZoneProximityAtr { get; set; }

        [Parameter("Maximum Zone Age", Group = "04 · Zones", DefaultValue = 40, MinValue = 5, MaxValue = 200)]
        public int MaximumZoneAgeBars { get; set; }
    }
}
