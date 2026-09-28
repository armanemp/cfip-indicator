using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPanelTradePlanRows(
            ref int slot,
            int contentWidth)
        {
            if (_plan == null ||
                !ShowTradePlanPanel)
                return;

            AddPanelRow(
                ref slot,
                "TRADE PLAN  •  " +
                (_plan.Direction == 1
                    ? "BUY ACTIVE"
                    : "SELL ACTIVE"),
                PanelDirectionColor(
                    _plan.Direction),
                true,
                contentWidth);

            RenderPanelTradePlanLevelRows(
                ref slot,
                contentWidth);

            RenderPanelTradePlanLiveRows(
                ref slot,
                contentWidth);
        }
    }
}
