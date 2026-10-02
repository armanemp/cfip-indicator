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
                                                        Math.Max(
                                                            0,
                                                            PanelPadding);
                                        
                                                    int toggleSideForLayout =
                                                        Math.Max(
                                                            22,
                                                            Math.Min(
                                                                40,
                                                                Math.Min(
                                                                    PanelToggleWidth,
                                                                    PanelToggleHeight)));
                                        
                                                    int buttonContentHeight =
                                                        Math.Max(
                                                            buttonHeight,
                                                            ShowPanelToggleButton
                                                                ? toggleSideForLayout
                                                                : 0);
                                        
                                                    int buttonAreaHeight =
                                                        buttons
                                                            ? buttonContentHeight +
                                                              buttonMargin * 2
                                                            : 0;
                                        
                                                    int panelHeight =
                                                        headerHeight +
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

                                                    int toggleSide =
                                                        Math.Max(
                                                            22,
                                                            Math.Min(
                                                                40,
                                                                Math.Min(
                                                                    PanelToggleWidth,
                                                                    PanelToggleHeight)));

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
                                                        buttons;

                                                    ApplyPanelActionButtonsLayout(
                                                        buttonMargin,
                                                        toggleSide,
                                                        border,
                                                        borderAlpha);

                                                    ApplyPanelRestoreButtonLayout(
                                                        borderAlpha,
                                                        border);

                                                    SetPanelAlignment();
                                                }
    }
}
