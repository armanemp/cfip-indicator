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
                _autoTradingQuickStatusText,
                _autoTradingQuickSwitchTrack,
                _autoTradingQuickSwitchThumb,
                "AUTO TRADE",
                AutoTradingEnabled,
                TpLineColor);

            SyncExecutionStatus(
                _automaticOrdersQuickStatus,
                _automaticOrdersQuickStatusText,
                _automaticOrdersQuickSwitchTrack,
                _automaticOrdersQuickSwitchThumb,
                "AUTO ORDERS",
                AutomaticOrdersEnabled,
                TriggerLineColor);
        }

        private void SyncExecutionStatus(
            Border status,
            TextBlock statusText,
            Border switchTrack,
            Border switchThumb,
            string caption,
            bool enabled,
            Color accentColor)
        {
            if (status == null ||
                statusText == null ||
                switchTrack == null ||
                switchThumb == null)
                return;

            statusText.Text =
                caption +
                "  " +
                (enabled ? "ON" : "OFF");

            statusText.ForegroundColor =
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

            switchTrack.BackgroundColor =
                enabled
                    ? Color.FromArgb(
                        180,
                        accentColor)
                    : Color.FromArgb(
                        110,
                        Color.Black);

            switchTrack.BorderColor =
                enabled
                    ? accentColor
                    : PanelBorder;

            switchThumb.HorizontalAlignment =
                enabled
                    ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left;
        }
    }
}
