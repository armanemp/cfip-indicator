using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Liquidity Lookback", Group = "05 · Liquidity", DefaultValue = 40, MinValue = 10, MaxValue = 150)]
        public int LiquidityLookback { get; set; }

        [Parameter("Use Equal High / Low", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UseEqualHighLow { get; set; }

        [Parameter("Equal Level Tolerance ATR", Group = "05 · Liquidity", DefaultValue = 0.12, MinValue = 0.02, MaxValue = 0.5)]
        public double EqualLevelToleranceAtr { get; set; }

        [Parameter("Use Liquidity Sweep", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UseLiquiditySweep { get; set; }

        [Parameter("Liquidity Sweep Minimum Depth ATR", Group = "05 · Liquidity", DefaultValue = 0.05, MinValue = 0, MaxValue = 1.0, Step = 0.01)]
        public double LiquiditySweepMinimumDepthAtr { get; set; }

        [Parameter("Use Displacement", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UseDisplacement { get; set; }

        [Parameter("Displacement ATR", Group = "05 · Liquidity", DefaultValue = 0.80, MinValue = 0.2, MaxValue = 3)]
        public double DisplacementAtr { get; set; }

        [Parameter("Use Premium / Discount", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UsePremiumDiscount { get; set; }

        [Parameter("Use Daily Weekly Liquidity", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UseDailyWeeklyLiquidity { get; set; }

        [Parameter("Use Daily Pivots", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UseDailyPivots { get; set; }

        [Parameter("Daily Pivot Weight", Group = "05 · Liquidity", DefaultValue = 18, MinValue = 1, MaxValue = 50)]
        public int DailyPivotWeight { get; set; }
    }
}
