using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPanelOverviewRows(
            ref int slot,
            int contentWidth)
        {
            RenderPanelOverviewStateRows(
                ref slot,
                contentWidth);

            RenderPanelOverviewExecutionRows(
                ref slot,
                contentWidth);

            RenderPanelOverviewDiagnosticRows(
                ref slot,
                contentWidth);
        }
    }
}
