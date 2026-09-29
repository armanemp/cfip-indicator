// CFIP Indicator — ExecutionToggleHandlers.cs
// Single-responsibility execution UI module.

using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ApplyAutoTradingQuickToggleClick(
            ToggleButtonEventArgs args)
        {
            if (_executionToggleSyncing)
                return;

            bool enabled =
                !AutoTradingEnabled;

            SetAutoTradingRuntimeState(
                enabled,
                enabled
                    ? "AWAITING EXECUTION"
                    : "DISABLED");

            SetAutoTradingState(
                enabled
                    ? "ARMED"
                    : "OFF",
                enabled
                    ? "QUICK ENABLED"
                    : "QUICK DISABLED");

            SyncQuickExecutionControls();
        }

        private void ApplyAutomaticOrdersQuickToggleClick(
            ToggleButtonEventArgs args)
        {
            if (_executionToggleSyncing)
                return;

            bool enabled =
                !AutomaticOrdersEnabled;

            SetAutomaticOrdersRuntimeState(
                enabled,
                enabled
                    ? "AWAITING ORDER SETUP"
                    : "DISABLED");

            SyncQuickExecutionControls();
        }
    }
}
