using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// ============================================================================
// CFIP Indicator — ExecutionControls.cs
// ============================================================================


namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {private void CreateQuickExecutionControls()
                        {
                            if (_quickExecutionStack != null)
                                return;
                
                            _quickExecutionStack =
                                new StackPanel
                                {
                                    Orientation =
                                        Orientation.Horizontal,
                                    HorizontalAlignment =
                                        HorizontalAlignment.Stretch,
                                    VerticalAlignment =
                                        VerticalAlignment.Center,
                                    BackgroundColor =
                                        Color.FromArgb(
                                            0,
                                            Color.Black),
                                    Height =
                                        QuickExecutionRowHeight
                                };
                
                            _autoTradingQuickToggle =
                                CreateExecutionToggle(
                                    AutoTradingEnabled,
                                    "AUTO TRADE",
                                    TpLineColor);
                
                            _automaticOrdersQuickToggle =
                                CreateExecutionToggle(
                                    AutomaticOrdersEnabled,
                                    "AUTO ORDERS",
                                    TriggerLineColor);
                
                            _autoTradingQuickToggle.Click +=
                                OnAutoTradingQuickToggleClicked;
                
                            _automaticOrdersQuickToggle.Click +=
                                OnAutomaticOrdersQuickToggleClicked;
                
                            _quickExecutionStack.AddChild(
                                _autoTradingQuickToggle);
                
                            _quickExecutionStack.AddChild(
                                _automaticOrdersQuickToggle);
                        }
        
        private ToggleButton CreateExecutionToggle(
                            bool isChecked,
                            string caption,
                            Color accentColor)
                        {
                            return
                                new ToggleButton
                                {
                                    Text =
                                        isChecked
                                            ? caption + "  •  ON"
                                            : caption + "  •  OFF",
                                    Width = 170,
                                    Height =
                                        QuickExecutionButtonHeight,
                                    IsChecked =
                                        isChecked,
                                    FontFamily =
                                        string.IsNullOrWhiteSpace(
                                            PanelFontFamily)
                                            ? "Arial"
                                            : PanelFontFamily,
                                    FontSize =
                                        Math.Max(
                                            8,
                                            PanelFontSize - 1),
                                    BackgroundColor =
                                        Color.FromArgb(
                                            105,
                                            isChecked
                                                ? accentColor
                                                : Color.Black),
                                    ForegroundColor =
                                        PanelTextColor,
                                    BorderColor =
                                        isChecked
                                            ? accentColor
                                            : PanelBorder,
                                    BorderThickness = 1,
                                    CornerRadius =
                                        Math.Min(
                                            10,
                                            Math.Max(
                                                5,
                                                PanelCornerRadius))
                                };
                        }
        
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
        
        private string GetSessionPanelText()
                        {
                            DateTime utc =
                                TimeInUtc;
                
                            DateTime local =
                                TimeInUtc +
                                Application.UserTimeOffset;
                
                            int start =
                                ClampInt(
                                    SessionStartUtc,
                                    0,
                                    23);
                
                            int end =
                                ClampInt(
                                    SessionEndUtc,
                                    0,
                                    23);
                
                            int now =
                                utc.Hour * 60 +
                                utc.Minute;
                
                            int startMin =
                                start * 60;
                
                            int endMin =
                                end * 60;
                
                            bool open =
                                startMin <= endMin
                                    ? now >= startMin &&
                                      now < endMin
                                    : now >= startMin ||
                                      now < endMin;
                
                            return
                                "SESSION  " +
                                (open ? "OPEN" : "CLOSED") +
                                "  •  UTC " +
                                utc.ToString("HH:mm") +
                                "  •  LOCAL " +
                                local.ToString("HH:mm") +
                                "  •  " +
                                start.ToString("00") +
                                ":00–" +
                                end.ToString("00") +
                                ":00 UTC";
                        }
        
        private Color GetSessionPanelColor()
                        {
                            DateTime utc =
                                TimeInUtc;
                
                            int now =
                                utc.Hour * 60 +
                                utc.Minute;
                
                            int start =
                                ClampInt(
                                    SessionStartUtc,
                                    0,
                                    23) * 60;
                
                            int end =
                                ClampInt(
                                    SessionEndUtc,
                                    0,
                                    23) * 60;
                
                            bool open =
                                start <= end
                                    ? now >= start &&
                                      now < end
                                    : now >= start ||
                                      now < end;
                
                            return open
                                ? TpLineColor
                                : PanelWarningColor;
                        }
    }
}
