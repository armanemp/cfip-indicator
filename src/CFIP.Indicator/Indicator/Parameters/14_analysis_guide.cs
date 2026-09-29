using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Show Analysis Guide On Chart", Group = "14 · DISPLAY — ADVANCED", DefaultValue = true)]
        public bool ShowOnChartAnalysisGuide { get; set; }
    }
}
