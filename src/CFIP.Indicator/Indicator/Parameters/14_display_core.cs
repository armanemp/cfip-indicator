using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Show Level Lines", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowLevelLines { get; set; }

        [Parameter("Full Width Level Lines", Group = "14 · DISPLAY — CORE", DefaultValue = false)]
        public bool FullWidthLevelLines { get; set; }

        [Parameter("Level Line Thickness", Group = "14 · DISPLAY — CORE", DefaultValue = 1, MinValue = 1, MaxValue = 3)]
        public int LevelLineThickness { get; set; }

        [Parameter("Show Entry", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowEntry { get; set; }

        [Parameter("Show Trigger", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowTrigger { get; set; }

        [Parameter("Show SL", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowSL { get; set; }

        [Parameter("Show TP1", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowTP1 { get; set; }

        [Parameter("Show TP2", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowTP2 { get; set; }

        [Parameter("Show TP3", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowTP3 { get; set; }

        [Parameter("Show TP4", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowTP4 { get; set; }

        [Parameter("Show Signal Arrow", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowSignalArrow { get; set; }

        [Parameter("Show Early Watch", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowEarlyWatch { get; set; }

        [Parameter("Show Historical Signals", Group = "14 · DISPLAY — CORE", DefaultValue = false)]
        public bool ShowHistoricalSignals { get; set; }

        [Parameter("Historical Signal Limit", Group = "14 · DISPLAY — CORE", DefaultValue = 10, MinValue = 1, MaxValue = 50)]
        public int HistoricalSignalLimit { get; set; }

        [Parameter("Show Unified Panel", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowUnifiedPanel { get; set; }

        [Parameter("Show Panel Background", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowPanelBackground { get; set; }

        [Parameter("Panel Position", Group = "14 · DISPLAY — CORE", DefaultValue = PanelCorner.BottomLeft)]
        public PanelCorner PanelPosition { get; set; }

        [Parameter("Panel Width", Group = "14 · DISPLAY — CORE", DefaultValue = 430, MinValue = 220, MaxValue = 700)]
        public int PanelWidth { get; set; }

        [Parameter("Panel Font Size", Group = "14 · DISPLAY — CORE", DefaultValue = 11, MinValue = 8, MaxValue = 20)]
        public int PanelFontSize { get; set; }

        [Parameter("Panel Font Family", Group = "14 · DISPLAY — CORE", DefaultValue = "Arial")]
        public string PanelFontFamily { get; set; }

        [Parameter("Panel Bold", Group = "14 · DISPLAY — CORE", DefaultValue = false)]
        public bool PanelBold { get; set; }

        [Parameter("Panel Background", Group = "14 · DISPLAY — CORE", DefaultValue = "Black")]
        public Color PanelBackground { get; set; }

        [Parameter("Panel Background Alpha", Group = "14 · DISPLAY — CORE", DefaultValue = 145, MinValue = 0, MaxValue = 255)]
        public int PanelBackgroundAlpha { get; set; }

        [Parameter("Panel Border", Group = "14 · DISPLAY — CORE", DefaultValue = "#3A4656")]
        public Color PanelBorder { get; set; }

        [Parameter("Panel Border Alpha", Group = "14 · DISPLAY — CORE", DefaultValue = 230, MinValue = 0, MaxValue = 255)]
        public int PanelBorderAlpha { get; set; }

        [Parameter("Panel Border Thickness", Group = "14 · DISPLAY — CORE", DefaultValue = 1, MinValue = 0, MaxValue = 4)]
        public int PanelBorderThickness { get; set; }

        [Parameter("Panel Corner Radius", Group = "14 · DISPLAY — CORE", DefaultValue = 5, MinValue = 0, MaxValue = 20)]
        public int PanelCornerRadius { get; set; }

        [Parameter("Panel Padding", Group = "14 · DISPLAY — CORE", DefaultValue = 9, MinValue = 0, MaxValue = 30)]
        public int PanelPadding { get; set; }

        [Parameter("Panel Margin", Group = "14 · DISPLAY — CORE", DefaultValue = 8, MinValue = 0, MaxValue = 30)]
        public int PanelMargin { get; set; }


        [Parameter("Panel Row Gap", Group = "14 · DISPLAY — CORE", DefaultValue = 1, MinValue = 0, MaxValue = 6)]
        public int PanelRowGap { get; set; }

        [Parameter("Panel Max Height", Group = "14 · DISPLAY — CORE", DefaultValue = 650, MinValue = 260, MaxValue = 1200)]
        public int PanelMaxHeight { get; set; }

        [Parameter("Panel Row Padding", Group = "14 · DISPLAY — CORE", DefaultValue = 3, MinValue = 0, MaxValue = 12)]
        public int PanelRowPadding { get; set; }

        [Parameter("Panel Button Gap", Group = "14 · DISPLAY — CORE", DefaultValue = 4, MinValue = 0, MaxValue = 16)]
        public int PanelButtonGap { get; set; }

        [Parameter("Panel Accent Color", Group = "14 · DISPLAY — CORE", DefaultValue = "#4A90E2")]
        public Color PanelAccentColor { get; set; }

        [Parameter("Panel Section Color", Group = "14 · DISPLAY — CORE", DefaultValue = "#8FA3B8")]
        public Color PanelSectionColor { get; set; }

        [Parameter("Panel Secondary Text Color", Group = "14 · DISPLAY — CORE", DefaultValue = "#C5CBD3")]
        public Color PanelSecondaryTextColor { get; set; }

        [Parameter("Panel Muted Text Color", Group = "14 · DISPLAY — CORE", DefaultValue = "#8A95A5")]
        public Color PanelMutedTextColor { get; set; }

        [Parameter("Panel Warning Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Orange")]
        public Color PanelWarningColor { get; set; }

        [Parameter("Panel Text Color", Group = "14 · DISPLAY — CORE", DefaultValue = "White")]
        public Color PanelTextColor { get; set; }

        [Parameter("Entry Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "White")]
        public Color EntryLineColor { get; set; }

        [Parameter("Trigger Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Orange")]
        public Color TriggerLineColor { get; set; }

        [Parameter("SL Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Red")]
        public Color SlLineColor { get; set; }

        [Parameter("TP1 Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Lime")]
        public Color TpLineColor { get; set; }

        [Parameter("TP2 Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "SpringGreen")]
        public Color Tp2LineColor { get; set; }

        [Parameter("TP3 Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Turquoise")]
        public Color Tp3LineColor { get; set; }

        [Parameter("TP4 Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Gold")]
        public Color Tp4LineColor { get; set; }

        [Parameter("BUY Arrow Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Lime")]
        public Color BuyArrowColor { get; set; }

        [Parameter("SELL Arrow Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Red")]
        public Color SellArrowColor { get; set; }
    }
}
