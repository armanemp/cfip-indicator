using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PopupRenderer.cs
// Single-responsibility popup renderer.


namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
                        private void ShowPopup(
                            string message,
                            bool critical)
                        {
                            if (!ShowPopupAlerts)
                                return;
                
                            if (_popup == null)
                            {
                                try
                                {
                                    _popupText =
                                        new TextBlock
                                        {
                                            Text = "",
                                            TextWrapping = TextWrapping.Wrap,
                                            TextAlignment = TextAlignment.Left,
                                            HorizontalAlignment = HorizontalAlignment.Left,
                                            VerticalAlignment = VerticalAlignment.Top,
                                            Margin = 0
                                        };
                
                                    StackPanel popupStack =
                                        new StackPanel
                                        {
                                            Orientation =
                                                Orientation.Vertical
                                        };
                
                                    popupStack.AddChild(
                                        _popupText);
                
                                    _popupCloseButton =
                                        new Button();
                
                                    _popupCloseButton.Text =
                                        "CLOSE";
                
                                    _popupCloseButton.Width = 70;
                                    _popupCloseButton.Height = 22;
                                    _popupCloseButton.HorizontalAlignment =
                                        HorizontalAlignment.Right;
                
                                    _popupCloseButton.Click +=
                                        args => RemovePopup();
                
                                    popupStack.AddChild(
                                        _popupCloseButton);
                
                                    _popup =
                                        new Border
                                        {
                                            Child = popupStack,
                                            IsHitTestVisible = true
                                        };
                
                                    Chart.AddControl(
                                        _popup);
                                }
                                catch (Exception ex)
                                {
                                    Print(
                                        "CFIP popup creation failed: {0}",
                                        ex.Message);
                
                                    _popup = null;
                                    _popupText = null;
                                    return;
                                }
                            }
                
                            if (_popupText == null)
                                return;
                
                            _popupText.Text =
                                message;
                
                            _popupText.FontSize =
                                Math.Max(
                                    8,
                                    Math.Min(
                                        22,
                                        PopupFontSize));
                
                            _popupText.FontWeight =
                                PopupBold
                                    ? FontWeight.Bold
                                    : FontWeight.Normal;
                
                            _popupText.FontFamily =
                                string.IsNullOrWhiteSpace(
                                    PopupFontFamily)
                                    ? "Arial"
                                    : PopupFontFamily;
                
                            _popupText.ForegroundColor =
                                PopupTextColor;
                
                            _popup.Width =
                                Math.Max(
                                    220,
                                    PopupWidth);
                
                            _popup.Padding =
                                Math.Max(
                                    0,
                                    PopupPadding);
                
                            _popup.Margin =
                                Math.Max(
                                    0,
                                    PopupMargin);
                
                            _popup.BackgroundColor =
                                Color.FromArgb(
                                    Math.Max(
                                        0,
                                        Math.Min(
                                            255,
                                            PopupBackgroundAlpha)),
                                    PopupBackgroundColor);
                
                            _popup.BorderColor =
                                Color.FromArgb(
                                    Math.Max(
                                        0,
                                        Math.Min(
                                            255,
                                            PopupBorderAlpha)),
                                    PopupBorderColor);
                
                            _popup.BorderThickness =
                                Math.Max(
                                    0,
                                    PopupBorderThickness);
                
                            _popup.CornerRadius =
                                Math.Max(
                                    0,
                                    PopupCornerRadius);
                
                            switch (PopupPosition)
                            {
                                case PanelCorner.TopLeft:
                                    _popup.VerticalAlignment =
                                        VerticalAlignment.Top;
                                    _popup.HorizontalAlignment =
                                        HorizontalAlignment.Left;
                                    break;
                
                                case PanelCorner.BottomLeft:
                                    _popup.VerticalAlignment =
                                        VerticalAlignment.Bottom;
                                    _popup.HorizontalAlignment =
                                        HorizontalAlignment.Left;
                                    break;
                
                                case PanelCorner.BottomRight:
                                    _popup.VerticalAlignment =
                                        VerticalAlignment.Bottom;
                                    _popup.HorizontalAlignment =
                                        HorizontalAlignment.Right;
                                    break;
                
                                default:
                                    _popup.VerticalAlignment =
                                        VerticalAlignment.Top;
                                    _popup.HorizontalAlignment =
                                        HorizontalAlignment.Right;
                                    break;
                            }
                
                            if (_popupCloseButton != null)
                                _popupCloseButton.IsVisible =
                                    ShowPopupCloseButton;
                
                            _popupCritical =
                                critical;

                            _popupUntilUtc =
                                KeepPopupUntilNextAlert
                                    ? DateTime.MaxValue
                                    : TimeInUtc.AddSeconds(
                                        Math.Max(
                                            1,
                                            PopupDurationSeconds));
                        }
    }
}
