using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Enable Exact WaveTrend Evidence", Group = "26 · WAVETREND", DefaultValue = true)]
        public bool UseWaveTrendEvidence { get; set; }

        [Parameter("WaveTrend Length", Group = "26 · WAVETREND", DefaultValue = 10, MinValue = 2, MaxValue = 50)]
        public int WaveTrendLength { get; set; }

        [Parameter("WaveTrend Momentum Length", Group = "26 · WAVETREND", DefaultValue = 5, MinValue = 1, MaxValue = 20)]
        public int WaveTrendMomentumLength { get; set; }

        [Parameter("WaveTrend Smoothing Length", Group = "26 · WAVETREND", DefaultValue = 4, MinValue = 1, MaxValue = 20)]
        public int WaveTrendSmoothingLength { get; set; }

        [Parameter("WaveTrend Signal Length", Group = "26 · WAVETREND", DefaultValue = 5, MinValue = 1, MaxValue = 20)]
        public int WaveTrendSignalLength { get; set; }

        [Parameter("WaveTrend Smooth MA", Group = "26 · WAVETREND", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType WaveTrendSmoothMA { get; set; }

        [Parameter("WaveTrend Signal MA", Group = "26 · WAVETREND", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType WaveTrendSignalMA { get; set; }

        [Parameter("WaveTrend OS1", Group = "26 · WAVETREND", DefaultValue = -30)]
        public int WaveTrendOs1 { get; set; }

        [Parameter("WaveTrend OS2", Group = "26 · WAVETREND", DefaultValue = -40)]
        public int WaveTrendOs2 { get; set; }

        [Parameter("WaveTrend OB1", Group = "26 · WAVETREND", DefaultValue = 30)]
        public int WaveTrendOb1 { get; set; }

        [Parameter("WaveTrend OB2", Group = "26 · WAVETREND", DefaultValue = 40)]
        public int WaveTrendOb2 { get; set; }

        [Parameter("WaveTrend Evidence Weight", Group = "26 · WAVETREND", DefaultValue = 6, MinValue = 1, MaxValue = 15)]
        public int WaveTrendEvidenceWeight { get; set; }

        [Parameter("Minimum WaveTrend Quality", Group = "26 · WAVETREND", DefaultValue = 58, MinValue = 30, MaxValue = 95)]
        public int MinimumWaveTrendQuality { get; set; }
    }
}
