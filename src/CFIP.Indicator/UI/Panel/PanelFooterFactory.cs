// ============================================================================
// CFIP Indicator — PanelFooterFactory.cs
// ============================================================================

using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private StackPanel _panelFooterActions;
        private TextBlock _panelDataStatusText;
        private StackPanel _panelDataStatusBars;
        private readonly List<Border> _panelDataStatusBarControls =
            new List<Border>(5);

        private void CreatePanelFooter()
        {
            _panelDataStatusBars =
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Height = PanelDataStatusRowHeight,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            _panelDataStatusText =
                new TextBlock
                {
                    Text = "DATA",
                    Width = 34,
                    Height = PanelDataStatusRowHeight,
                    FontFamily = string.IsNullOrWhiteSpace(PanelFontFamily) ? "Arial" : PanelFontFamily,
                    FontSize = Math.Max(9, PanelFontSize - 2),
                    FontWeight = FontWeight.Bold,
                    ForegroundColor = PanelMutedTextColor,
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.NoWrap
                };
            _panelDataStatusBars.AddChild(_panelDataStatusText);

            string[] dataLabels = { "M1", "M5", "M15", "H1", "H4" };
            for (int i = 0; i < dataLabels.Length; i++)
            {
                Border bar = new Border
                {
                    Width = 38,
                    Height = 12,
                    Margin = new Thickness(2, 0, 2, 0),
                    CornerRadius = 2,
                    BorderThickness = 1,
                    BorderColor = PanelBorder,
                    BackgroundColor = Color.FromArgb(70, PanelMutedTextColor),
                    Child = new TextBlock
                    {
                        Text = dataLabels[i],
                        FontFamily = string.IsNullOrWhiteSpace(PanelFontFamily) ? "Arial" : PanelFontFamily,
                        FontSize = Math.Max(7, PanelFontSize - 4),
                        FontWeight = FontWeight.Bold,
                        ForegroundColor = PanelTextColor,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.NoWrap
                    }
                };
                _panelDataStatusBarControls.Add(bar);
                _panelDataStatusBars.AddChild(bar);
            }

            _panelFooterActions =
                new StackPanel
                {
                    Orientation =
                        Orientation.Horizontal,
                    HorizontalAlignment =
                        HorizontalAlignment.Stretch,
                    VerticalAlignment =
                        VerticalAlignment.Top,
                    Height = PanelFooterMinHeight -
                        PanelDataStatusRowHeight -
                        PanelFooterActionGap,
                    BackgroundColor =
                        Color.FromArgb(0, Color.Black)
                };

            // The Indicator panel is analysis/presentation only.
            // Broker Close/Cancel actions belong to the cBot surface.
            // The hide/show toggle is the only interactive control
            // owned by the Indicator panel. Broker actions are cBot-owned.
            CreatePanelToggleButton();

            if (_panelToggleButton != null)
                _panelFooterActions.AddChild(
                    _panelToggleButton);

            CreatePanelAlertMessageRail();

            if (_panelAlertMessageStack != null)
                _panelFooterActions.AddChild(
                    _panelAlertMessageStack);

            _buttonStack.AddChild(
                _panelDataStatusBars);

            _buttonStack.AddChild(
                _panelFooterActions);
        }
    }
}
