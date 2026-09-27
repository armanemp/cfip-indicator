using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Enable Sound Alerts", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool EnableSoundAlerts { get; set; }

        [Parameter("Show Popup Alerts", Group = "12 · ALERTS — CORE", DefaultValue = false)]
        public bool ShowPopupAlerts { get; set; }

        [Parameter("Popup Critical Only", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool PopupCriticalOnly { get; set; }

        [Parameter("Popup Duration Seconds", Group = "12 · ALERTS — CORE", DefaultValue = 6, MinValue = 1, MaxValue = 60)]
        public int PopupDurationSeconds { get; set; }

        [Parameter("Popup Font Size", Group = "12 · ALERTS — CORE", DefaultValue = 11, MinValue = 8, MaxValue = 22)]
        public int PopupFontSize { get; set; }

        [Parameter("Show Entry Restriction Popup", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool ShowEntryRestrictionPopup { get; set; }

        [Parameter("Alert On News / Event Guard", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnNewsEventGuard { get; set; }

        [Parameter("Popup Margin", Group = "12 · ALERTS — CORE", DefaultValue = 10, MinValue = 0, MaxValue = 50)]
        public int PopupMargin { get; set; }

        [Parameter("Popup Border Alpha", Group = "12 · ALERTS — CORE", DefaultValue = 235, MinValue = 0, MaxValue = 255)]
        public int PopupBorderAlpha { get; set; }

        [Parameter("Popup Bold", Group = "12 · ALERTS — CORE", DefaultValue = false)]
        public bool PopupBold { get; set; }

        [Parameter("Popup Font Family", Group = "12 · ALERTS — CORE", DefaultValue = "Arial")]
        public string PopupFontFamily { get; set; }

        [Parameter("Alert On Confirmed Signal", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnConfirmedSignal { get; set; }

        [Parameter("Alert On Reaction", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnReaction { get; set; }

        [Parameter("Alert On Early Watch", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnEarlyWatch { get; set; }

        [Parameter("Alert On Level Hit", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnLevelHit { get; set; }

        [Parameter("Alert On Invalidated", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnInvalidated { get; set; }

        [Parameter("Alert Cooldown Seconds", Group = "12 · ALERTS — CORE", DefaultValue = 8, MinValue = 1, MaxValue = 60)]
        public int AlertCooldownSeconds { get; set; }

        [Parameter("Suppress Duplicate Alerts", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool SuppressDuplicateAlerts { get; set; }

        [Parameter("Enable Email Alerts", Group = "12 · ALERTS — CORE", DefaultValue = false)]
        public bool EnableEmailAlerts { get; set; }

        [Parameter("Sender Email", Group = "12 · ALERTS — CORE", DefaultValue = "")]
        public string SenderEmail { get; set; }

        [Parameter("Receiver Email", Group = "12 · ALERTS — CORE", DefaultValue = "")]
        public string ReceiverEmail { get; set; }
    }
}
