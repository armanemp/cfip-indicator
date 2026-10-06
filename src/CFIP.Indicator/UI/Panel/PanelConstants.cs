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

        // Compact footer chrome: the buy/sell pressure rail is a fixed-height,
        // full-width presentation surface above the alert/action area.
        private const int PanelFooterMinHeight = 70;
        private const int PanelFlowPressureRailHeight = 42;
        private const int PanelFlowPressureRowHeight = 20;
        private const int PanelFlowPressureLabelHeight = 10;
        private const int PanelFlowPressureBarHeight = 8;
        private const int PanelFooterActionGap = 8;
        private const int PanelFooterButtonInternalMargin = 2;
        private const int PanelStatusLampFontSize = 20;
        private const int PanelStatusLampWidth = 30;
        private const int PanelStatusLampHeight = 30;
    }
}
