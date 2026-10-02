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
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    BackgroundColor = Color.FromArgb(0, Color.Black),
                    Height = QuickExecutionRowHeight
                };

            _autoTradingQuickToggle =
                CreateExecutionToggle(
                    false,
                    "AUTO TRADE",
                    TpLineColor);

            _automaticOrdersQuickToggle =
                CreateExecutionToggle(
                    false,
                    "AUTO ORDERS",
                    TriggerLineColor);

            // Status-only surfaces. Public cTrader parameters remain the
            // authoritative execution settings; the panel never mutates them.

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
                        ExecutionControlPresentationRule.ComposeStatusText(
                            caption,
                            isChecked),
                    Width = 170,
                    Height = QuickExecutionButtonHeight,
                    IsChecked = isChecked,
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
                                PanelCornerRadius)),
                    IsEnabled = false
                };
        }
    }
}
