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
        private void OnAutoTradingQuickToggleChecked(
            ToggleButtonEventArgs args)
        {
            ApplyAutoTradingQuickToggleState(true);
        }

        private void OnAutoTradingQuickToggleUnchecked(
            ToggleButtonEventArgs args)
        {
            ApplyAutoTradingQuickToggleState(false);
        }

        private void ApplyAutoTradingQuickToggleState(
            bool enabled)
        {
            if (_executionToggleSyncing)
                return;

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

        private void OnAutomaticOrdersQuickToggleChecked(
            ToggleButtonEventArgs args)
        {
            ApplyAutomaticOrdersQuickToggleState(true);
        }

        private void OnAutomaticOrdersQuickToggleUnchecked(
            ToggleButtonEventArgs args)
        {
            ApplyAutomaticOrdersQuickToggleState(false);
        }

        private void ApplyAutomaticOrdersQuickToggleState(
            bool enabled)
        {
            if (_executionToggleSyncing)
                return;

            SetAutomaticOrdersRuntimeState(
                enabled,
                enabled
                    ? "AWAITING ORDER SETUP"
                    : "DISABLED");

            SyncQuickExecutionControls();
        }
    }
}
