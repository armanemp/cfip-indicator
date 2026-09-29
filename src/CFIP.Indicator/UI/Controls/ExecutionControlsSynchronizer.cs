// CFIP Indicator — ExecutionControlsSynchronizer.cs
// Single-responsibility execution UI module.

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void SyncQuickExecutionControls()
        {
            EnsureExecutionRuntimeState();

            SyncExecutionStatus(
                _autoTradingQuickStatus,
                "AUTO TRADE",
                AutoTradingEnabled,
                TpLineColor);

            SyncExecutionStatus(
                _automaticOrdersQuickStatus,
                "AUTO ORDERS",
                AutomaticOrdersEnabled,
                TriggerLineColor);
        }

        private void SyncExecutionStatus(
            Button status,
            string caption,
            bool enabled,
            Color accentColor)
        {
            if (status == null)
                return;

            status.Text =
                caption +
                "  " +
                (enabled ? "ON" : "OFF");

            status.ForegroundColor =
                PanelTextColor;

            status.BackgroundColor =
                enabled
                    ? Color.FromArgb(
                        42,
                        accentColor)
                    : Color.FromArgb(
                        32,
                        Color.Black);

            status.BorderColor =
                enabled
                    ? Color.FromArgb(
                        170,
                        accentColor)
                    : PanelBorder;
        }
    }
}
