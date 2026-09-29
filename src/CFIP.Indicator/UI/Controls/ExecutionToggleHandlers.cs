// CFIP Indicator — ExecutionToggleHandlers.cs
// Deliberately empty after the execution-control reliability correction.
//
// AUTO TRADE and AUTO ORDERS are status-only chart indicators.
// Their authoritative enable/disable inputs are the public cTrader settings:
// EnableAutoTrading and EnableAutomaticOrders. The chart surface is non-interactive
// so it cannot drift into a runtime override that differs from the settings.
//
// Keep this module as the documented ownership boundary: UI execution controls
// expose state only; they never mutate execution authority.

using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
    }
}
