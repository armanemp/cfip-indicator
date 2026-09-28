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
        private void ApplyPanelActionButtonsLayout(
                                                    int buttonHeight,
                                                    int buttonGap,
                                                    int buttonMargin,
                                                    int eachButtonWidth,
                                                    int toggleSide,
                                                    int border,
                                                    int borderAlpha,
                                                    bool showSafetyButtons)
                                                {
                                                    if (_closeButton != null)
                                                    {
                                                        _closeButton.IsVisible =
                                                            showSafetyButtons;
                                        
                                                        _closeButton.Width =
                                                            eachButtonWidth;
                                        
                                                        _closeButton.Height =
                                                            Math.Max(
                                                                26,
                                                                buttonHeight);
                                        
                                                        _closeButton.Text =
                                                            eachButtonWidth < 120
                                                                ? "CLOSE"
                                                                : "CLOSE POSITIONS";
                                        
                                                        _closeButton.ForegroundColor =
                                                            PanelTextColor;
                                        
                                                        _closeButton.FontSize =
                                                            Math.Max(
                                                                8,
                                                                PanelFontSize - 1);
                                        
                                                        _closeButton.FontWeight =
                                                            FontWeight.Bold;
                                        
                                                        _closeButton.BackgroundColor =
                                                            Color.FromArgb(
                                                                ShowPanelBackground
                                                                    ? 120
                                                                    : 0,
                                                                SlLineColor);
                                        
                                                        _closeButton.BorderColor =
                                                            SlLineColor;
                                        
                                                        _closeButton.BorderThickness =
                                                            border;
                                        
                                                        _closeButton.CornerRadius =
                                                            Math.Max(
                                                                0,
                                                                PanelCornerRadius);
                                        
                                                        _closeButton.Margin =
                                                            new Thickness(
                                                                buttonMargin,
                                                                buttonMargin,
                                                                buttonGap / 2,
                                                                buttonMargin);
                                        
                                                        _closeButton.HorizontalContentAlignment =
                                                            HorizontalAlignment.Center;
                                        
                                                        _closeButton.VerticalContentAlignment =
                                                            VerticalAlignment.Center;
                                                    }
                                        
                                                    if (_cancelButton != null)
                                                    {
                                                        _cancelButton.IsVisible =
                                                            showSafetyButtons;
                                        
                                                        _cancelButton.Width =
                                                            eachButtonWidth;
                                        
                                                        _cancelButton.Height =
                                                            Math.Max(
                                                                26,
                                                                buttonHeight);
                                        
                                                        _cancelButton.Text =
                                                            eachButtonWidth < 120
                                                                ? "CANCEL"
                                                                : "CANCEL ORDERS";
                                        
                                                        _cancelButton.ForegroundColor =
                                                            PanelTextColor;
                                        
                                                        _cancelButton.FontSize =
                                                            Math.Max(
                                                                8,
                                                                PanelFontSize - 1);
                                        
                                                        _cancelButton.FontWeight =
                                                            FontWeight.Bold;
                                        
                                                        _cancelButton.BackgroundColor =
                                                            Color.FromArgb(
                                                                ShowPanelBackground
                                                                    ? 82
                                                                    : 0,
                                                                Color.FromHex("#2A313B"));
                                        
                                                        _cancelButton.BorderColor =
                                                            Color.FromArgb(
                                                                Math.Max(
                                                                    60,
                                                                    Math.Min(
                                                                        255,
                                                                        PanelBorderAlpha)),
                                                                PanelBorder);
                                        
                                                        _cancelButton.BorderThickness =
                                                            border;
                                        
                                                        _cancelButton.CornerRadius =
                                                            Math.Max(
                                                                0,
                                                                PanelCornerRadius);
                                        
                                                        _cancelButton.Margin =
                                                            new Thickness(
                                                                buttonGap / 2,
                                                                buttonMargin,
                                                                buttonMargin,
                                                                buttonMargin);
                                        
                                                        _cancelButton.HorizontalContentAlignment =
                                                            HorizontalAlignment.Center;
                                        
                                                        _cancelButton.VerticalContentAlignment =
                                                            VerticalAlignment.Center;
                                                    }
                                        
                                                    if (_panelToggleButton != null)
                                                    {
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
                                        
                                                        // Sits in the bottom button row now, left of Close/Cancel —
                                                        // match their vertical margin so all three buttons line up.
                                                        _panelToggleButton.Margin =
                                                            new Thickness(
                                                                buttonMargin,
                                                                buttonMargin,
                                                                buttonGap / 2,
                                                                buttonMargin);
                                                    }
                                        
                                                }
    }
}
