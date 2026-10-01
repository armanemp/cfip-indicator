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

            if (_autoTradingQuickToggle != null)
            {
                _autoTradingQuickToggle.IsChecked = AutoTradingEnabled;
                _autoTradingQuickToggle.Text =
                    ExecutionControlPresentationRule.ComposeStatusText(
                        "AUTO TRADE",
                        AutoTradingEnabled);
                _autoTradingQuickToggle.IsEnabled =
                    ExecutionControlPresentationRule.IsInteractive;
                _autoTradingQuickToggle.BackgroundColor =
                    Color.FromArgb(
                        105,
                        AutoTradingEnabled ? TpLineColor : Color.Black);
                _autoTradingQuickToggle.BorderColor =
                    AutoTradingEnabled ? TpLineColor : PanelBorder;
                _autoTradingQuickToggle.ForegroundColor = PanelTextColor;
            }

            if (_automaticOrdersQuickToggle != null)
            {
                _automaticOrdersQuickToggle.IsChecked = AutomaticOrdersEnabled;
                _automaticOrdersQuickToggle.Text =
                    ExecutionControlPresentationRule.ComposeStatusText(
                        "AUTO ORDERS",
                        AutomaticOrdersEnabled);
                _automaticOrdersQuickToggle.IsEnabled =
                    ExecutionControlPresentationRule.IsInteractive;
                _automaticOrdersQuickToggle.BackgroundColor =
                    Color.FromArgb(
                        105,
                        AutomaticOrdersEnabled ? TriggerLineColor : Color.Black);
                _automaticOrdersQuickToggle.BorderColor =
                    AutomaticOrdersEnabled ? TriggerLineColor : PanelBorder;
                _automaticOrdersQuickToggle.ForegroundColor = PanelTextColor;
            }
        }
    }
}
