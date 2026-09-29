// CFIP Indicator — ExecutionControlsFactory.cs
// Single-responsibility execution UI module.

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void CreateQuickExecutionControls()
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

            _autoTradingQuickStatus =
                CreateExecutionStatus(
                    "AUTO TRADE",
                    AutoTradingEnabled,
                    TpLineColor,
                    out _autoTradingQuickStatusText,
                    out _autoTradingQuickSwitchTrack,
                    out _autoTradingQuickSwitchThumb);

            _automaticOrdersQuickStatus =
                CreateExecutionStatus(
                    "AUTO ORDERS",
                    AutomaticOrdersEnabled,
                    TriggerLineColor,
                    out _automaticOrdersQuickStatusText,
                    out _automaticOrdersQuickSwitchTrack,
                    out _automaticOrdersQuickSwitchThumb);

            _quickExecutionStack.AddChild(
                _autoTradingQuickStatus);

            _quickExecutionStack.AddChild(
                _automaticOrdersQuickStatus);
        }

        private Border CreateExecutionStatus(
            string caption,
            bool enabled,
            Color accentColor,
            out TextBlock statusText,
            out Border switchTrack,
            out Border switchThumb)
        {
            statusText =
                new TextBlock
                {
                    Text =
                        caption +
                        "  " +
                        (enabled ? "ON" : "OFF"),
                    Width = 122,
                    HorizontalAlignment =
                        HorizontalAlignment.Left,
                    VerticalAlignment =
                        VerticalAlignment.Center,
                    TextAlignment =
                        TextAlignment.Left,
                    TextWrapping =
                        TextWrapping.NoWrap,
                    FontFamily =
                        string.IsNullOrWhiteSpace(
                            PanelFontFamily)
                            ? "Arial"
                            : PanelFontFamily,
                    FontSize =
                        Math.Max(
                            8,
                            PanelFontSize - 1),
                    FontWeight =
                        FontWeight.Bold,
                    ForegroundColor =
                        PanelTextColor,
                    BackgroundColor =
                        Color.FromArgb(
                            0,
                            Color.Black)
                };

            switchThumb =
                new Border
                {
                    Width = 12,
                    Height = 12,
                    CornerRadius = 6,
                    BackgroundColor =
                        Color.FromArgb(
                            240,
                            Color.White),
                    BorderColor =
                        Color.FromArgb(
                            220,
                            Color.White),
                    BorderThickness = 1,
                    HorizontalAlignment =
                        enabled
                            ? HorizontalAlignment.Right
                            : HorizontalAlignment.Left,
                    VerticalAlignment =
                        VerticalAlignment.Center,
                    IsHitTestVisible = false
                };

            switchTrack =
                new Border
                {
                    Child = switchThumb,
                    Width = 34,
                    Height = 18,
                    Padding = 2,
                    CornerRadius = 9,
                    BackgroundColor =
                        enabled
                            ? Color.FromArgb(
                                180,
                                accentColor)
                            : Color.FromArgb(
                                110,
                                Color.Black),
                    BorderColor =
                        enabled
                            ? accentColor
                            : PanelBorder,
                    BorderThickness = 1,
                    HorizontalAlignment =
                        HorizontalAlignment.Right,
                    VerticalAlignment =
                        VerticalAlignment.Center,
                    IsHitTestVisible = false
                };

            StackPanel row =
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
                            Color.Black)
                };

            row.AddChild(statusText);
            row.AddChild(switchTrack);

            return
                new Border
                {
                    Child = row,
                    Width = 170,
                    Height = QuickExecutionButtonHeight,
                    Padding = 5,
                    Margin = 2,
                    BackgroundColor =
                        enabled
                            ? Color.FromArgb(
                                42,
                                accentColor)
                            : Color.FromArgb(
                                32,
                                Color.Black),
                    BorderColor =
                        enabled
                            ? Color.FromArgb(
                                170,
                                accentColor)
                            : PanelBorder,
                    BorderThickness = 1,
                    CornerRadius =
                        Math.Min(
                            10,
                            Math.Max(
                                6,
                                PanelCornerRadius)),
                    IsHitTestVisible = false
                };
        }
    }
}
