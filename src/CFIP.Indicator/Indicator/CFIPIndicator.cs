using cAlgo.API;

namespace cAlgo
{
    [Indicator(
        "CFIPIndicator",
        IsOverlay = true,
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public partial class CFIPIndicator : Indicator
    {
    }
}
