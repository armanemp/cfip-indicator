using cAlgo.API;

namespace cAlgo
{
    [Indicator(
        IsOverlay = true,
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.Internet)]
    public partial class CFIPIndicator : Indicator
    {
    }
}
