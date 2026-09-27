using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Fast EMA", Group = "06 · Indicators", DefaultValue = 9, MinValue = 2, MaxValue = 50)]
        public int FastEma { get; set; }

        [Parameter("Slow EMA", Group = "06 · Indicators", DefaultValue = 21, MinValue = 3, MaxValue = 100)]
        public int SlowEma { get; set; }

        [Parameter("RSI Period", Group = "06 · Indicators", DefaultValue = 14, MinValue = 2, MaxValue = 50)]
        public int RsiPeriod { get; set; }

        [Parameter("ADX Period", Group = "06 · Indicators", DefaultValue = 14, MinValue = 3, MaxValue = 50)]
        public int AdxPeriod { get; set; }

        [Parameter("ADX Minimum", Group = "06 · Indicators", DefaultValue = 16, MinValue = 5, MaxValue = 50)]
        public double AdxMinimum { get; set; }

        [Parameter("ATR Period", Group = "06 · Indicators", DefaultValue = 14, MinValue = 3, MaxValue = 50)]
        public int AtrPeriod { get; set; }

        [Parameter("Use EMA Slope", Group = "06 · Indicators", DefaultValue = true)]
        public bool UseEmaSlope { get; set; }

        [Parameter("Avoid RSI Exhaustion", Group = "06 · Indicators", DefaultValue = true)]
        public bool AvoidRsiExhaustion { get; set; }
    }
}
