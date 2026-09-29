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

            if (enabled &&
                EnableAutoTrading)
            {
                // Quick enable is an explicit operator re-arm request, but it
                // may only clear a previously recovered block. An active
                // EntryBlocked state remains fail-closed.
                _runtimeFaultStateMachine.ObserveAutoTradingSetting(false);
                _runtimeFaultStateMachine.ObserveAutoTradingSetting(true);

                if (!_runtimeFaultStateMachine.CanAutomaticEntryProceed)
                {
                    SetAutoTradingRuntimeState(
                        false,
                        "RUNTIME ENTRY BLOCKED • RECOVERY / EXPLICIT RE-ARM");
                    ApplyRuntimeEntryGate();
                    SetAutoTradingState(
                        "ENTRY_BLOCKED",
                        "RUNTIME FAULT • EXPLICIT RE-ARM REQUIRED");
                    SyncQuickExecutionControls();
                    return;
                }
            }

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
