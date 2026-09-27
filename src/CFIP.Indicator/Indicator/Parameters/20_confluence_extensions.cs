using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Use Volume Expansion", Group = "20 · Confluence Extensions", DefaultValue = false)]
        public bool UseVolumeExpansion { get; set; }

        [Parameter("Volume Expansion Ratio", Group = "20 · Confluence Extensions", DefaultValue = 1.15, MinValue = 1.0, MaxValue = 3.0, Step = 0.05)]
        public double VolumeExpansionRatio { get; set; }

        [Parameter("Use MACD Bias", Group = "20 · Confluence Extensions", DefaultValue = false)]
        public bool UseMacdBias { get; set; }

        [Parameter("MACD Fast Period", Group = "20 · Confluence Extensions", DefaultValue = 12, MinValue = 2, MaxValue = 50)]
        public int MacdFastPeriod { get; set; }

        [Parameter("MACD Slow Period", Group = "20 · Confluence Extensions", DefaultValue = 26, MinValue = 3, MaxValue = 100)]
        public int MacdSlowPeriod { get; set; }

        [Parameter("Use VWAP Bias", Group = "20 · Confluence Extensions", DefaultValue = false)]
        public bool UseVwapBias { get; set; }

        [Parameter("VWAP Lookback Bars", Group = "20 · Confluence Extensions", DefaultValue = 48, MinValue = 10, MaxValue = 200)]
        public int VwapLookbackBars { get; set; }

        [Parameter("Use Healthy Volatility", Group = "20 · Confluence Extensions", DefaultValue = false)]
        public bool UseHealthyVolatility { get; set; }

        [Parameter("Healthy ATR Minimum Ratio", Group = "20 · Confluence Extensions", DefaultValue = 0.85, MinValue = 0.50, MaxValue = 1.50, Step = 0.05)]
        public double HealthyAtrMinimumRatio { get; set; }

        [Parameter("Healthy ATR Maximum Ratio", Group = "20 · Confluence Extensions", DefaultValue = 1.80, MinValue = 1.0, MaxValue = 3.0, Step = 0.05)]
        public double HealthyAtrMaximumRatio { get; set; }
    }
}
