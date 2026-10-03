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
                                                            220,
                                                            Math.Min(
                                                                700,
                                                                PanelWidth));
                                        
                                                    _panel.Height =
                                                        panelHeight;
                                        
                                                    _panel.MinWidth = 220;
                                                    _panel.MaxWidth = 700;
                                                    _panel.MinHeight = 170;
                                                    _panel.MaxHeight =
                                                        Math.Max(
                                                            220,
                                                            maxHeight);
                                        
                                                    _panel.Padding =
                                                        padding;
                                        
                                                    int baseMargin =
                                                        Math.Max(
                                                            0,
                                                            PanelMargin);

                                                    bool bottomPosition =
                                                        PanelPosition ==
                                                            PanelCorner.BottomLeft ||
                                                        PanelPosition ==
                                                            PanelCorner.BottomRight;

                                                    int bottomMargin =
                                                        bottomPosition
                                                            ? Math.Max(
                                                                baseMargin,
                                                                PanelBottomClearance)
                                                            : baseMargin;

                                                    _panel.Margin =
                                                        new Thickness(
                                                            baseMargin,
                                                            baseMargin,
                                                            baseMargin,
                                                            bottomMargin);
                                        
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
                                                        GetCanonicalSignalPanelStatusColor();

                                                    // Header content is owned by the lightweight live-header
                                                    // presentation service so geometry updates never freeze its
                                                    // realtime state.
                                                    UpdatePanelHeaderLiveState();
                                        
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

                                                    if (_panelTrendTimeframeLampRow != null)
                                                    {
                                                        _panelTrendTimeframeLampRow.Width =
                                                            Math.Max(
                                                                200,
                                                                contentWidth);
                                                        _panelTrendTimeframeLampRow.Height =
                                                            PanelTrendTimeframeLampRowHeight;
                                                    }
                                        
                                                    _buttonStack.Width =
                                                        Math.Max(
                                                            200,
                                                            contentWidth);
                                        
                                                    _buttonStack.Height =
                                                        buttonAreaHeight;
                                        
                                                }
    }
}
