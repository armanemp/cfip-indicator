using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Show Panel Toggle Button", Group = "14 · DISPLAY — PANEL", DefaultValue = true)]
        public bool ShowPanelToggleButton { get; set; }

        [Parameter("Panel Toggle Width", Group = "14 · DISPLAY — PANEL", DefaultValue = 26, MinValue = 22, MaxValue = 40)]
        public int PanelToggleWidth { get; set; }

        [Parameter("Panel Toggle Height", Group = "14 · DISPLAY — PANEL", DefaultValue = 26, MinValue = 22, MaxValue = 40)]
        public int PanelToggleHeight { get; set; }
    }
}
