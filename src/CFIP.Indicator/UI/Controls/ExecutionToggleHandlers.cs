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
            ApplyAutoTradingQuickToggleState(args, true);
        }

        private void OnAutoTradingQuickToggleUnchecked(
            ToggleButtonEventArgs args)
        {
            ApplyAutoTradingQuickToggleState(args, false);
        }

        private void ApplyAutoTradingQuickToggleState(
            ToggleButtonEventArgs args,
            bool enabled)
        {
            if (_executionToggleSyncing ||
                args == null ||
                args.ToggleButton == null)
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
        }

        private void OnAutomaticOrdersQuickToggleChecked(
            ToggleButtonEventArgs args)
        {
            ApplyAutomaticOrdersQuickToggleState(args, true);
        }

        private void OnAutomaticOrdersQuickToggleUnchecked(
            ToggleButtonEventArgs args)
        {
            ApplyAutomaticOrdersQuickToggleState(args, false);
        }

        private void ApplyAutomaticOrdersQuickToggleState(
            ToggleButtonEventArgs args,
            bool enabled)
        {
            if (_executionToggleSyncing ||
                args == null ||
                args.ToggleButton == null)
                return;

            SetAutomaticOrdersRuntimeState(
                enabled,
                enabled
                    ? "AWAITING ORDER SETUP"
                    : "DISABLED");
        }
    }
}