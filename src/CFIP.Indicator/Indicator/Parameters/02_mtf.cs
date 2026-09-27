using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("M1 Trigger", Group = "02 · MTF", DefaultValue = false)]
        public bool UseM1Trigger { get; set; }

        [Parameter("M5 Confirmation", Group = "02 · MTF", DefaultValue = true)]
        public bool UseM5Confirmation { get; set; }

        [Parameter("M5 Weight", Group = "02 · MTF", DefaultValue = 7, MinValue = 1, MaxValue = 20)]
        public int M5Weight { get; set; }

        [Parameter("M15 Weight", Group = "02 · MTF", DefaultValue = 8, MinValue = 1, MaxValue = 20)]
        public int M15Weight { get; set; }

        [Parameter("M30 Weight", Group = "02 · MTF", DefaultValue = 5, MinValue = 0, MaxValue = 20)]
        public int M30Weight { get; set; }

        [Parameter("H1 Weight", Group = "02 · MTF", DefaultValue = 3, MinValue = 0, MaxValue = 20)]
        public int H1Weight { get; set; }

        [Parameter("H4 Weight", Group = "02 · MTF", DefaultValue = 2, MinValue = 0, MaxValue = 20)]
        public int H4Weight { get; set; }

        [Parameter("D1 Weight", Group = "02 · MTF", DefaultValue = 2, MinValue = 0, MaxValue = 20)]
        public int D1Weight { get; set; }

        [Parameter("W1 Weight", Group = "02 · MTF", DefaultValue = 3, MinValue = 0, MaxValue = 20)]
        public int W1Weight { get; set; }
    }
}
