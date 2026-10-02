using cAlgo.API;

namespace cAlgo
{
#pragma warning disable CS0612
    [Indicator(
        "CFIP Smart Indicator",
        IsOverlay = true,
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public partial class CFIPIndicator : Indicator
    {
    }
#pragma warning restore CS0612
}
