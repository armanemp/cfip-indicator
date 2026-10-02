using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ApplyPanelActionButtonsLayout(
            int buttonMargin,
            int toggleSide,
            int border,
            int borderAlpha)
        {
            if (_panelToggleButton == null)
                return;

            _panelToggleButton.IsVisible =
                ShowPanelToggleButton;

            _panelToggleButton.Width =
                toggleSide;

            _panelToggleButton.Height =
                toggleSide;

            _panelToggleButton.ForegroundColor =
                PanelTextColor;

            _panelToggleButton.FontSize =
                Math.Max(
                    9,
                    PanelFontSize);

            _panelToggleButton.BackgroundColor =
                Color.FromArgb(
                    ShowPanelBackground
                        ? 70
                        : 0,
                    PanelBackground);

            _panelToggleButton.BorderColor =
                Color.FromArgb(
                    borderAlpha,
                    PanelBorder);

            _panelToggleButton.BorderThickness =
                border;

            _panelToggleButton.CornerRadius =
                Math.Min(
                    5,
                    Math.Max(
                        0,
                        PanelCornerRadius));

            _panelToggleButton.VerticalAlignment =
                VerticalAlignment.Center;

            _panelToggleButton.HorizontalAlignment =
                HorizontalAlignment.Left;

            _panelToggleButton.Margin =
                new Thickness(
                    buttonMargin,
                    buttonMargin,
                    0,
                    buttonMargin);
        }
    }
}
