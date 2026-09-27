// CFIP Indicator — ExecutionControlsSynchronizer.cs
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
        private void SyncQuickExecutionControls()
                        {
                            EnsureExecutionRuntimeState();
                
                            _executionToggleSyncing = true;
                
                            try
                            {
                                if (_autoTradingQuickToggle != null)
                                {
                                    _autoTradingQuickToggle.IsChecked =
                                        AutoTradingEnabled;
                
                                    bool compactTradeText =
                                        _autoTradingQuickToggle.Width < 120;
                
                                    _autoTradingQuickToggle.Text =
                                        compactTradeText
                                            ? (AutoTradingEnabled ? "TRADE • ON" : "TRADE • OFF")
                                            : (AutoTradingEnabled ? "AUTO TRADE • ON" : "AUTO TRADE • OFF");
                
                                    _autoTradingQuickToggle.BackgroundColor =
                                        Color.FromArgb(
                                            105,
                                            AutoTradingEnabled
                                                ? TpLineColor
                                                : Color.Black);
                
                                    _autoTradingQuickToggle.BorderColor =
                                        AutoTradingEnabled
                                            ? TpLineColor
                                            : PanelBorder;
                
                                    _autoTradingQuickToggle.ForegroundColor =
                                        PanelTextColor;
                                }
                
                                if (_automaticOrdersQuickToggle != null)
                                {
                                    _automaticOrdersQuickToggle.IsChecked =
                                        AutomaticOrdersEnabled;
                
                                    bool compactOrderText =
                                        _automaticOrdersQuickToggle.Width < 120;
                
                                    _automaticOrdersQuickToggle.Text =
                                        compactOrderText
                                            ? (AutomaticOrdersEnabled ? "ORDERS • ON" : "ORDERS • OFF")
                                            : (AutomaticOrdersEnabled ? "AUTO ORDERS • ON" : "AUTO ORDERS • OFF");
                
                                    _automaticOrdersQuickToggle.BackgroundColor =
                                        Color.FromArgb(
                                            105,
                                            AutomaticOrdersEnabled
                                                ? TriggerLineColor
                                                : Color.Black);
                
                                    _automaticOrdersQuickToggle.BorderColor =
                                        AutomaticOrdersEnabled
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
