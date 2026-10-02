// ============================================================================
// CFIP Indicator — PanelConstants.cs
// Single responsibility: fixed panel geometry constants.
// ============================================================================

namespace cAlgo
{
    public partial class CFIPIndicator : cAlgo.API.Indicator
    {
        private const int PanelRowCount = 64;
        private const int PanelHeaderHeight = 30;
        private const int PanelBottomExecutionGap = 64;
    }
}
