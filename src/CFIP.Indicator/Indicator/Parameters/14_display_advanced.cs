using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Line Length Bars", Group = "14 · DISPLAY — ADVANCED", DefaultValue = 40, MinValue = 5, MaxValue = 300)]
        public int LineLengthBars { get; set; }

        [Parameter("Line Forward Bars", Group = "14 · DISPLAY — ADVANCED", DefaultValue = 10, MinValue = 1, MaxValue = 100)]
        public int LineForwardBars { get; set; }

        [Parameter("Show Signal Labels", Group = "14 · DISPLAY — ADVANCED", DefaultValue = true)]
        public bool ShowSignalLabels { get; set; }

        [Parameter("Show Level Price Labels", Group = "14 · DISPLAY — ADVANCED", DefaultValue = true)]
        public bool ShowLevelPriceLabels { get; set; }

        [Parameter("Show Context Event Marker", Group = "14 · DISPLAY — ADVANCED", DefaultValue = true)]
        public bool ShowContextEventMarker { get; set; }

        [Parameter("Show Prediction Objects", Group = "14 · DISPLAY — ADVANCED", DefaultValue = true)]
        public bool ShowPredictionObjects { get; set; }
    }
}
