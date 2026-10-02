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
        private void ApplyPanelSurfaceAndHeaderLayout(
                                                    int contentWidth,
                                                    int panelHeight,
                                                    int padding,
                                                    int border,
                                                    int maxHeight,
                                                    int backgroundAlpha,
                                                    int borderAlpha,
                                                    int headerHeight,
                                                    int scrollHeight,
                                                    int buttonAreaHeight)
                                                {
                                                    _panel.Width =
                                                        Math.Max(
                                                            260,
                                                            PanelWidth);
                                        
                                                    _panel.Height =
                                                        panelHeight;
                                        
                                                    _panel.MinWidth = 260;
                                                    _panel.MaxWidth = 760;
                                                    _panel.MinHeight = 170;
                                                    _panel.MaxHeight =
                                                        Math.Max(
                                                            220,
                                                            maxHeight);
                                        
                                                    _panel.Padding =
                                                        padding;
                                        
                                                    int panelMargin =
                                                        Math.Max(
                                                            0,
                                                            PanelMargin);

                                                    int bottomReservation =
                                                        PanelBottomReservedSpace();

                                                    _panel.Margin =
                                                        bottomReservation > 0
                                                            ? new Thickness(
                                                                panelMargin,
                                                                panelMargin,
                                                                panelMargin,
                                                                panelMargin +
                                                                bottomReservation)
                                                            : new Thickness(
                                                                panelMargin);
                                        
                                                    _panel.BackgroundColor =
                                                        Color.FromArgb(
                                                            backgroundAlpha,
                                                            PanelBackground);
                                        
                                                    _panel.BorderColor =
                                                        Color.FromArgb(
                                                            borderAlpha,
                                                            PanelBorder);
                                        
                                                    _panel.BorderThickness =
                                                        border;
                                        
                                                    _panel.CornerRadius =
                                                        Math.Max(
                                                            0,
                                                            PanelCornerRadius);
                                        
                                                    _panelHeaderStack.Width =
                                                        Math.Max(
                                                            200,
                                                            contentWidth);
                                        
                                                    _panelHeaderStack.Height =
                                                        headerHeight;
                                        
                                                    // The toggle button now lives in the bottom button row (moved per
                                                    // user request), not the header, so the title reclaims the full
                                                    // header content width.
                                                    _panelHeaderTitle.Width =
                                                        Math.Max(
                                                            150,
                                                            contentWidth -
                                                            36);

                                                    if (_processingLamp != null)
                                                    {
                                                        _processingLamp.Width = 24;
                                                        _processingLamp.Height =
                                                            Math.Max(
                                                                20,
                                                                headerHeight - 4);
                                                        _processingLamp.Margin =
                                                            new Thickness(4, 0, 0, 0);
                                                    }
                                        
                                                    _panelHeaderTitle.FontFamily =
                                                        string.IsNullOrWhiteSpace(
                                                            PanelFontFamily)
                                                            ? "Arial"
                                                            : PanelFontFamily;
                                        
                                                    _panelHeaderTitle.FontSize =
                                                        Math.Max(
                                                            9,
                                                            PanelFontSize);
                                        
                                                    _panelHeaderTitle.ForegroundColor =
                                                        AutoTradingPanelColor();
                                        
                                                    _panelHeaderTitle.Text =
                                                        "CFIP SMART  •  " +
                                                        AutoTradingPanelLine();
                                        
                                                    _panelHeaderTitle.LineHeight =
                                                        Math.Max(
                                                            14,
                                                            PanelFontSize + 2);
                                        
                                                    _panelHeaderStack.BackgroundColor =
                                                        Color.FromArgb(
                                                            0,
                                                            Color.Black);
                                        
                                                    _panelStack.BackgroundColor =
                                                        Color.FromArgb(
                                                            0,
                                                            Color.Black);
                                        
                                                    _panelRowsStack.BackgroundColor =
                                                        Color.FromArgb(
                                                            0,
                                                            Color.Black);
                                        
                                                    _panelScroll.BackgroundColor =
                                                        Color.FromArgb(
                                                            0,
                                                            Color.Black);
                                        
                                                    _buttonStack.BackgroundColor =
                                                        Color.FromArgb(
                                                            0,
                                                            Color.Black);
                                        
                                                    _panelRowsStack.Width =
                                                        Math.Max(
                                                            200,
                                                            contentWidth);
                                        
                                                    _panelScroll.Width =
                                                        Math.Max(
                                                            200,
                                                            contentWidth);
                                        
                                                    _panelScroll.Height =
                                                        Math.Max(
                                                            100,
                                                            scrollHeight);
                                        
                                                    _buttonStack.Width =
                                                        Math.Max(
                                                            200,
                                                            contentWidth);
                                        
                                                    _buttonStack.Height =
                                                        buttonAreaHeight;
                                        
                                                }
    }
}
