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
                    TpLineColor);

            _automaticOrdersQuickStatus =
                CreateExecutionStatus(
                    "AUTO ORDERS",
                    AutomaticOrdersEnabled,
                    TriggerLineColor);

            WireExecutionToggleHandlers();

            _quickExecutionStack.AddChild(
                _autoTradingQuickStatus);

            _quickExecutionStack.AddChild(
                _automaticOrdersQuickStatus);
        }

        private Button CreateExecutionStatus(
            string caption,
            bool enabled,
            Color accentColor)
        {
            return
                new Button
                {
                    Text =
                        caption +
                        "  " +
                        (enabled ? "ON" : "OFF"),
                    Width = 170,
                    Height = QuickExecutionButtonHeight,
                    Margin = 2,
                    HorizontalAlignment =
                        HorizontalAlignment.Stretch,
                    VerticalAlignment =
                        VerticalAlignment.Center,
                    HorizontalContentAlignment =
                        HorizontalAlignment.Center,
                    VerticalContentAlignment =
                        VerticalAlignment.Center,
                    ForegroundColor =
                        PanelTextColor,
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
                    BorderThickness =
                        Math.Max(
                            1,
                            PanelBorderThickness),
                    CornerRadius =
                        Math.Min(
                            10,
                            Math.Max(
                                6,
                                PanelCornerRadius))
                };
        }
    }
}
