using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Enable Economic News Calendar", Group = "28 · NEWS GUARD", DefaultValue = true)]
        public bool EnableEconomicNewsCalendar { get; set; }

        [Parameter("Economic News Data URI", Group = "28 · NEWS GUARD", DefaultValue = "https://nfs.faireconomy.media/ff_calendar_thisweek.json")]
        public string EconomicNewsDataUri { get; set; }

        [Parameter("News Refresh Minutes", Group = "28 · NEWS GUARD", DefaultValue = 60, MinValue = 60, MaxValue = 360)]
        public int NewsRefreshMinutes { get; set; }

        [Parameter("High Impact Minutes Before", Group = "28 · NEWS GUARD", DefaultValue = 30, MinValue = 0, MaxValue = 180)]
        public int HighImpactNewsMinutesBefore { get; set; }

        [Parameter("High Impact Minutes After", Group = "28 · NEWS GUARD", DefaultValue = 20, MinValue = 0, MaxValue = 180)]
        public int HighImpactNewsMinutesAfter { get; set; }

        [Parameter("Block Medium Impact News", Group = "28 · NEWS GUARD", DefaultValue = false)]
        public bool BlockMediumImpactNews { get; set; }

        [Parameter("Medium Impact Minutes Before", Group = "28 · NEWS GUARD", DefaultValue = 15, MinValue = 0, MaxValue = 120)]
        public int MediumImpactNewsMinutesBefore { get; set; }

        [Parameter("Medium Impact Minutes After", Group = "28 · NEWS GUARD", DefaultValue = 10, MinValue = 0, MaxValue = 120)]
        public int MediumImpactNewsMinutesAfter { get; set; }

        [Parameter("Additional News Currencies / Symbol Map", Group = "28 · NEWS GUARD", DefaultValue = "USD;XAU=USD;XAG=USD;US30=USD;US500=USD;SPX500=USD;NAS100=USD;US100=USD;USTEC=USD;DE40=EUR;GER40=EUR;DAX40=EUR;UK100=GBP;FTSE100=GBP;JP225=JPY;HK50=HKD;CN50=CNY;AUS200=AUD;FRA40=EUR;EU50=EUR;BTC=USD;ETH=USD")]
        public string AdditionalNewsCurrencies { get; set; }


        [Parameter("Fail Closed When News Feed Stale", Group = "28 · NEWS GUARD", DefaultValue = true)]
        public bool NewsFailClosedWhenStale { get; set; }

        [Parameter("Maximum News Feed Age Minutes", Group = "28 · NEWS GUARD", DefaultValue = 90, MinValue = 15, MaxValue = 720)]
        public int MaximumNewsFeedAgeMinutes { get; set; }

        [Parameter("Cancel Pending Before High Impact", Group = "28 · NEWS GUARD", DefaultValue = true)]
        public bool CancelPendingBeforeHighImpactNews { get; set; }

        [Parameter("Close Active Before High Impact", Group = "28 · NEWS GUARD", DefaultValue = false)]
        public bool CloseActiveBeforeHighImpactNews { get; set; }

        [Parameter("Show News Risk Status", Group = "28 · NEWS GUARD", DefaultValue = true)]
        public bool ShowNewsRiskStatus { get; set; }
    }
}
