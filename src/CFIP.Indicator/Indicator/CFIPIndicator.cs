using cAlgo.API;

namespace cAlgo
{
    [Indicator(
        IsOverlay = true,
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public partial class CFIPIndicator : Indicator
    {
        [Output(
            "CFIP Runtime",
            IsVisible = false,
            IsLastValueVisibleInTitle = false,
            LineColor = "Transparent",
            PlotType = PlotType.Line,
            Thickness = 1)]
        public IndicatorDataSeries RuntimeOutput { get; set; }
    }
}
