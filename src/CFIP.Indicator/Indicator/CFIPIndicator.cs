using cAlgo.API;
using CFIP.Contracts;

namespace cAlgo
{
#pragma warning disable CS0612
    [Indicator(
        IndicatorIdentity.DisplayName,
        IsOverlay = true,
        AutoRescale = false,
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public partial class CFIPIndicator : Indicator
    {
    }
#pragma warning restore CS0612
}