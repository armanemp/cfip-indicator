using cAlgo.API;

namespace cAlgo
{
    [Indicator(
        "CFIP Smart Indicator",
        IsOverlay = true,
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public partial class CFIPIndicator : Indicator
    {
    }
}
