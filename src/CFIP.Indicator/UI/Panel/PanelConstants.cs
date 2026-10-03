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
        private const int QuickExecutionRowHeight = 38;
        private const int QuickExecutionButtonHeight = 30;
        private const int QuickExecutionVerticalMargin = 4;
        private const int PanelBottomClearance = 50;
        private const int PanelRestoreBottomClearance = 50;

        private const int PanelFooterMinHeight = 40;
        private const int PanelStatusLampFontSize = 20;
        private const int PanelStatusLampWidth = 30;
        private const int PanelStatusLampHeight = 30;
    }
}
