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
            RefreshCbotExecutionStateIfDue();

            if (_executionToggleSyncing)
                return;

            _executionToggleSyncing = true;

            try
            {
                if (_autoTradingQuickToggle != null)
                {
                    bool autoTrading =
                        _cBotExecutionState != null &&
                        IsCbotExecutionStateFresh() &&
                        _cBotExecutionState.EffectiveAutoTradingEnabled;

                    _autoTradingQuickToggle.IsChecked =
                        autoTrading;

                    _autoTradingQuickToggle.Text =
                        ExecutionControlPresentationRule.ComposeStatusText(
                            "AUTO TRADE",
                            autoTrading);

                    _autoTradingQuickToggle.IsEnabled =
                        ExecutionControlPresentationRule.IsInteractive;

                    _autoTradingQuickToggle.BackgroundColor =
                        Color.FromArgb(
                            105,
                            autoTrading
                                ? TpLineColor
                                : Color.Black);

                    _autoTradingQuickToggle.BorderColor =
                        autoTrading
                            ? TpLineColor
                            : PanelBorder;

                    _autoTradingQuickToggle.ForegroundColor =
                        PanelTextColor;
                }

                if (_automaticOrdersQuickToggle != null)
                {
                    bool autoOrders =
                        _cBotExecutionState != null &&
                        IsCbotExecutionStateFresh() &&
                        _cBotExecutionState.EffectiveAutomaticOrdersEnabled;

                    _automaticOrdersQuickToggle.IsChecked =
                        autoOrders;

                    _automaticOrdersQuickToggle.Text =
                        ExecutionControlPresentationRule.ComposeStatusText(
                            "AUTO ORDERS",
                            autoOrders);

                    _automaticOrdersQuickToggle.IsEnabled =
                        ExecutionControlPresentationRule.IsInteractive;

                    _automaticOrdersQuickToggle.BackgroundColor =
                        Color.FromArgb(
                            105,
                            autoOrders
                                ? TriggerLineColor
                                : Color.Black);

                    _automaticOrdersQuickToggle.BorderColor =
                        autoOrders
                            ? TriggerLineColor
                            : PanelBorder;

                    _automaticOrdersQuickToggle.ForegroundColor =
                        PanelTextColor;
                }
            }
            finally
            {
                _executionToggleSyncing = false;
            }
        }
    }
}
