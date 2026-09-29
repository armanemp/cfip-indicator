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
        private void ApplyAutoTradingQuickToggleClick()
        {
            if (_executionToggleSyncing ||
                _autoTradingQuickToggle == null)
                return;

            bool enabled =
                _autoTradingQuickToggle.IsChecked;

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

        private void ApplyAutomaticOrdersQuickToggleClick()
        {
            if (_executionToggleSyncing ||
                _automaticOrdersQuickToggle == null)
                return;

            bool enabled =
                _automaticOrdersQuickToggle.IsChecked;

            SetAutomaticOrdersRuntimeState(
                enabled,
                enabled
                    ? "AWAITING ORDER SETUP"
                    : "DISABLED");

            SyncQuickExecutionControls();
        }
    }
}
