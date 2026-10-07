using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Range Engine Enabled", Group = "08 · Range Intelligence", DefaultValue = true)]
        public bool RangeEngineEnabled { get; set; }

        [Parameter("Range Lookback M5", Group = "08 · Range Intelligence", DefaultValue = 36, MinValue = 12, MaxValue = 80)]
        public int RangeLookbackM5 { get; set; }

        [Parameter("Range Minimum Touches", Group = "08 · Range Intelligence", DefaultValue = 2, MinValue = 1, MaxValue = 6)]
        public int RangeMinimumTouches { get; set; }

        [Parameter("Range Min Width ATR", Group = "08 · Range Intelligence", DefaultValue = 0.35, MinValue = 0.10, MaxValue = 3.0, Step = 0.05)]
        public double RangeMinWidthAtr { get; set; }

        [Parameter("Range Max Width ATR", Group = "08 · Range Intelligence", DefaultValue = 3.0, MinValue = 0.50, MaxValue = 8.0, Step = 0.10)]
        public double RangeMaxWidthAtr { get; set; }

        [Parameter("Manipulation Sweep ATR", Group = "08 · Range Intelligence", DefaultValue = 0.05, MinValue = 0.01, MaxValue = 0.50, Step = 0.01)]
        public double RangeSweepAtr { get; set; }

        [Parameter("Range Breakout ATR", Group = "08 · Range Intelligence", DefaultValue = 0.10, MinValue = 0.02, MaxValue = 0.50, Step = 0.01)]
        public double RangeBreakoutAtr { get; set; }

        [Parameter("Range Retest ATR", Group = "08 · Range Intelligence", DefaultValue = 0.12, MinValue = 0.02, MaxValue = 0.50, Step = 0.01)]
        public double RangeRetestAtr { get; set; }

        [Parameter("Range Signal Minimum Score", Group = "08 · Range Intelligence", DefaultValue = 72, MinValue = 55, MaxValue = 95)]
        public int RangeSignalMinimumScore { get; set; }

        [Parameter("Show Range Zones", Group = "09 · Range Visuals", DefaultValue = true)]
        public bool ShowRangeZones { get; set; }

        [Parameter("Show Manipulation Marks", Group = "09 · Range Visuals", DefaultValue = true)]
        public bool ShowManipulationMarks { get; set; }

        [Parameter("Show Range Labels", Group = "09 · Range Visuals", DefaultValue = true)]
        public bool ShowRangeLabels { get; set; }
    }
}