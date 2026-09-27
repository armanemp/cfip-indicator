using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Use Session Filter", Group = "11 · Filters", DefaultValue = false)]
        public bool UseSessionFilter { get; set; }

        [Parameter("Session Start UTC", Group = "11 · Filters", DefaultValue = 6, MinValue = 0, MaxValue = 23)]
        public int SessionStartUtc { get; set; }

        [Parameter("Session End UTC", Group = "11 · Filters", DefaultValue = 20, MinValue = 0, MaxValue = 23)]
        public int SessionEndUtc { get; set; }

        [Parameter("Enable End Of Day Alert", Group = "11 · Filters", DefaultValue = true)]
        public bool EnableEndOfDayAlert { get; set; }

        [Parameter("End Of Day Alert Minutes Before", Group = "11 · Filters", DefaultValue = 30, MinValue = 5, MaxValue = 180)]
        public int EndOfDayAlertMinutesBefore { get; set; }

        [Parameter("Enable End Of Day Auto Close", Group = "11 · Filters", DefaultValue = true)]
        public bool EnableEndOfDayAutoClose { get; set; }

        [Parameter("Use Spread Filter", Group = "11 · Filters", DefaultValue = true)]
        public bool UseSpreadFilter { get; set; }

        [Parameter("Maximum Spread ATR", Group = "11 · Filters", DefaultValue = 0.15, MinValue = 0.01, MaxValue = 1)]
        public double MaximumSpreadAtr { get; set; }

        [Parameter("Cooldown M5 Bars", Group = "11 · Filters", DefaultValue = 3, MinValue = 0, MaxValue = 50)]
        public int CooldownM5Bars { get; set; }

        [Parameter("Avoid Friday Late Entry", Group = "11 · Filters", DefaultValue = false)]
        public bool AvoidFridayLateEntry { get; set; }

        [Parameter("Friday Cutoff UTC", Group = "11 · Filters", DefaultValue = 18, MinValue = 0, MaxValue = 23)]
        public int FridayCutoffUtc { get; set; }

        [Parameter("Use Volatility Guard", Group = "11 · Filters", DefaultValue = true)]
        public bool UseVolatilityGuard { get; set; }

        [Parameter("Event Shock Range ATR", Group = "11 · Filters", DefaultValue = 2.20, MinValue = 1.2, MaxValue = 5)]
        public double EventShockRangeAtr { get; set; }

        [Parameter("Event Shock ATR Expansion", Group = "11 · Filters", DefaultValue = 1.55, MinValue = 1.1, MaxValue = 3)]
        public double EventShockAtrExpansion { get; set; }

        [Parameter("News Blackout UTC", Group = "11 · Filters", DefaultValue = "")]
        public string NewsBlackoutUtc { get; set; }
    }
}
