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
        private void OnAutoTradingQuickToggleClicked(
                            ToggleButtonEventArgs args)
                        {
                            if (_executionToggleSyncing ||
                                args == null ||
                                args.ToggleButton == null)
                                return;
                
                            bool enabled =
                                args.ToggleButton.IsChecked;
                
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
                        }

        private void OnAutomaticOrdersQuickToggleClicked(
                            ToggleButtonEventArgs args)
                        {
                            if (_executionToggleSyncing ||
                                args == null ||
                                args.ToggleButton == null)
                                return;
                
                            bool enabled =
                                args.ToggleButton.IsChecked;
                
                            SetAutomaticOrdersRuntimeState(
                                enabled,
                                enabled
                                    ? "AWAITING ORDER SETUP"
                                    : "DISABLED");
                        }
    }
}
