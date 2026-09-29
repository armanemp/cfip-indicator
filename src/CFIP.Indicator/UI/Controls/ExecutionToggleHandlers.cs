// CFIP Indicator — ExecutionToggleHandlers.cs
// Single-responsibility execution UI module.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ApplyAutoTradingQuickToggleClick(
            ToggleButtonEventArgs args)
        {
            if (_executionToggleSyncing)
                return;

            // Toggle the canonical runtime flag directly. This deliberately does
            // not trust the control's IsChecked transition, which can be stale
            // during hosted chart control synchronization.
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
