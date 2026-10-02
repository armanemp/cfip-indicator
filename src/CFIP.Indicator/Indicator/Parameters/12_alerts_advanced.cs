using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Alert On Live Reaction", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool AlertOnLiveReaction { get; set; }

        [Parameter("Alert On Smart Decision", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool AlertOnSmartDecision { get; set; }

        [Parameter("Enable Level Hit Alerts", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool EnableLevelHitAlerts { get; set; }

        [Parameter("Alert On TP1", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool AlertOnTp1 { get; set; }

        [Parameter("Alert On TP2", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool AlertOnTp2 { get; set; }

        [Parameter("Alert On TP3", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool AlertOnTp3 { get; set; }

        [Parameter("Alert On TP4", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool AlertOnTp4 { get; set; }

        [Parameter("Alert On SL", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool AlertOnSl { get; set; }

        [Parameter("Alert On False Signal Risk", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool AlertOnFalseSignalRisk { get; set; }

        [Parameter("Alert On Entry Restriction", Group = "12 · ALERTS — ADVANCED", DefaultValue = false)]
        public bool AlertOnEntryRestriction { get; set; }

        [Parameter("Alert On High Confidence Entry", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool AlertOnHighConfidenceEntry { get; set; }

        [Parameter("High Confidence Threshold", Group = "12 · ALERTS — ADVANCED", DefaultValue = 82, MinValue = 70, MaxValue = 99)]
        public int HighConfidenceThreshold { get; set; }

        [Parameter("Alert Sound Type", Group = "12 · ALERTS — ADVANCED", DefaultValue = SoundType.PositiveNotification)]
        public cAlgo.API.SoundType AlertSoundType { get; set; }

        [Parameter("Sound File Path", Group = "12 · ALERTS — ADVANCED", DefaultValue = "")]
        public string SoundFilePath { get; set; }

        [Parameter("Popup Position", Group = "12 · ALERTS — ADVANCED", DefaultValue = PanelCorner.BottomRight)]
        public PanelCorner PopupPosition { get; set; }

        [Parameter("Popup Width", Group = "12 · ALERTS — ADVANCED", DefaultValue = 400, MinValue = 220, MaxValue = 700)]
        public int PopupWidth { get; set; }

        [Parameter("Keep Popup Until Next Alert", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool KeepPopupUntilNextAlert { get; set; }

        [Parameter("Show Popup Close Button", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
        public bool ShowPopupCloseButton { get; set; }

        [Parameter("Popup Background", Group = "12 · ALERTS — ADVANCED", DefaultValue = "Black")]
        public Color PopupBackgroundColor { get; set; }

        [Parameter("Popup Background Alpha", Group = "12 · ALERTS — ADVANCED", DefaultValue = 235, MinValue = 0, MaxValue = 255)]
        public int PopupBackgroundAlpha { get; set; }

        [Parameter("Popup Border", Group = "12 · ALERTS — ADVANCED", DefaultValue = "#3A4656")]
        public Color PopupBorderColor { get; set; }

        [Parameter("Popup Border Thickness", Group = "12 · ALERTS — ADVANCED", DefaultValue = 1, MinValue = 0, MaxValue = 4)]
        public int PopupBorderThickness { get; set; }

        [Parameter("Popup Corner Radius", Group = "12 · ALERTS — ADVANCED", DefaultValue = 10, MinValue = 0, MaxValue = 20)]
        public int PopupCornerRadius { get; set; }

        [Parameter("Popup Padding", Group = "12 · ALERTS — ADVANCED", DefaultValue = 10, MinValue = 0, MaxValue = 30)]
        public int PopupPadding { get; set; }

        [Parameter("Popup Text Color", Group = "12 · ALERTS — ADVANCED", DefaultValue = "White")]
        public Color PopupTextColor { get; set; }
    }
}
