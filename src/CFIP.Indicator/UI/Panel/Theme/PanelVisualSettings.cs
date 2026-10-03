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
        private void ApplyPanelVisualSettings(
                                                    int contentWidth,
                                                    int scrollHeight,
                                                    int maxHeight,
                                                    bool buttons,
                                                    int buttonHeight,
                                                    int buttonGap)
                                                {
                                                    if (_panel == null ||
                                                        _panelStack == null ||
                                                        _panelHeaderStack == null ||
                                                        _panelHeaderTitle == null ||
                                                        _panelScroll == null ||
                                                        _buttonStack == null)
                                                        return;
                                        
                                                    int padding =
                                                        Math.Max(
                                                            0,
                                                            PanelPadding);
                                        
                                                    int border =
                                                        Math.Max(
                                                            0,
                                                            PanelBorderThickness);
                                        
                                                    int headerHeight =
                                                        PanelHeaderHeight;
                                        
                                                    int buttonMargin =
                                                        PanelFooterButtonInternalMargin;
                                        
                                                    int toggleHeight =
                                                        Math.Max(
                                                            22,
                                                            Math.Min(
                                                                40,
                                                                PanelToggleHeight));
                                        
                                                    int toggleWidth =
                                                        Math.Max(
                                                            22,
                                                            Math.Min(
                                                                40,
                                                                PanelToggleWidth));
                                        
                                                    // Reserve an explicit footer minimum before calculating
                                                    // the ScrollViewer budget so the bottom controls cannot
                                                    // be pushed below the panel edge.
                                                    int alertRailHeight =
                                                        GetPanelAlertMessageRailHeight();

                                                    int buttonAreaHeight =
                                                        ResolvePanelFooterAreaHeight(
                                                            buttonHeight,
                                                            toggleHeight,
                                                            alertRailHeight);
                                        
                                                    int panelHeight =
                                                        headerHeight +
                                                        PanelTrendTimeframeLampRowHeight +
                                                        PanelTrendTimeframeLampTopSpacing +
                                                        PanelTrendTimeframeLampBottomSpacing +
                                                        scrollHeight +
                                                        buttonAreaHeight +
                                                        padding * 2 +
                                                        border * 2;
                                        
                                                    panelHeight =
                                                        Math.Max(
                                                            170,
                                                            Math.Min(
                                                                panelHeight,
                                                                Math.Max(
                                                                    200,
                                                                    maxHeight)));
                                        
                                                    int backgroundAlpha =
                                                        ShowPanelBackground
                                                            ? Math.Max(
                                                                0,
                                                                Math.Min(
                                                                    255,
                                                                    PanelBackgroundAlpha))
                                                            : 0;
                                        
                                                    int borderAlpha =
                                                        Math.Max(
                                                            0,
                                                            Math.Min(
                                                                255,
                                                                PanelBorderAlpha));


                                                    _panelStack.Height =
                                                        panelHeight;

                                                    _panelStack.VerticalAlignment =
                                                        VerticalAlignment.Top;

                                                    ApplyPanelSurfaceAndHeaderLayout(
                                                        contentWidth,
                                                        panelHeight,
                                                        padding,
                                                        border,
                                                        maxHeight,
                                                        backgroundAlpha,
                                                        borderAlpha,
                                                        headerHeight,
                                                        scrollHeight,
                                                        buttonAreaHeight);

                                                    ApplyPanelRowsLayout(
                                                        contentWidth);

                                                    _buttonStack.IsVisible =
                                                        ShowPanelToggleButton ||
                                                        GetPanelAlertMessageRailHeight() > 0;

                                                    ApplyPanelActionButtonsLayout(
                                                        buttonMargin,
                                                        toggleWidth,
                                                        toggleHeight,
                                                        border,
                                                        borderAlpha,
                                                        buttonGap);

                                                    ApplyPanelAlertMessageRailLayout(
                                                        contentWidth,
                                                        buttonGap,
                                                        toggleWidth);

                                                    ApplyPanelRestoreButtonLayout(
                                                        borderAlpha,
                                                        border);

                                                    SetPanelAlignment();
                                                }
    }
}
