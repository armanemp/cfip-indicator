// ============================================================================
// CFIP Indicator — PanelMainRenderer.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
// ============================================================================

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
        private void RenderPanel()
                                {
                                    if (!ShowUnifiedPanel)
                                    {
                                        RemovePanel();
                                        return;
                                    }
                        
                                    if (_panel == null)
                                        CreatePanel();
                        
                                    if (_panel == null ||
                                        _panelStack == null ||
                                        _panelHeaderStack == null ||
                                        _panelHeaderTitle == null ||
                                        _panelRowsStack == null ||
                                        _panelScroll == null ||
                                        _buttonStack == null ||
                                        _panelRows.Count != PanelRowCount)
                                        return;
                        
                                    if (_panelToggleButton == null)
                                        CreatePanelToggleButton();
                        
                                    if (_panelRestoreButton == null)
                                        CreatePanelRestoreButton();
                        
                                    _panel.IsVisible =
                                        !_panelHidden;
                        
                                    int padding =
                                        Math.Max(
                                            0,
                                            PanelPadding);
                        
                                    int border =
                                        Math.Max(
                                            0,
                                            PanelBorderThickness);
                        
                                    int effectivePanelWidth =
                                        Math.Max(
                                            260,
                                            PanelWidth);
                        
                                    int contentWidth =
                                        Math.Max(
                                            200,
                                            effectivePanelWidth -
                                            padding * 2 -
                                            border * 2);
                        
                                    bool showSafetyButtons =
                                        ShowTradeActionButtons ||
                                        AlwaysShowSafetyButtons;
                        
                                    bool showButtonRow =
                                        showSafetyButtons ||
                                        ShowPanelToggleButton;
                        
                                    bool buttons =
                                        showButtonRow;
                        
                                    int buttonHeight =
                                        Math.Max(
                                            26,
                                            ActionButtonHeight);
                        
                                    int buttonGap =
                                        Math.Max(
                                            0,
                                            PanelButtonGap);
                        
                                    int buttonMargin =
                                        Math.Max(
                                            0,
                                            ActionButtonMargin);
                        
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
                                        showButtonRow
                                            ? buttonContentHeight +
                                              buttonMargin * 2
                                            : 0;
                        
                                    int configuredMaxHeight =
                                        Math.Max(
                                            240,
                                            PanelMaxHeight);
                        
                                    int availableChartHeight =
                                        0;
                        
                                    try
                                    {
                                        availableChartHeight =
                                            (int)Math.Round(
                                                Math.Max(
                                                    0,
                                                    Chart.Height -
                                                    Math.Max(
                                                        0,
                                                        PanelMargin) * 2 -
                                                    8));
                                    }
                                    catch
                                    {
                                        availableChartHeight = 0;
                                    }
                        
                                    int maxHeight =
                                        availableChartHeight > 0
                                            ? Math.Max(
                                                180,
                                                Math.Min(
                                                    configuredMaxHeight,
                                                    availableChartHeight))
                                            : configuredMaxHeight;
                        
                                    int quickExecutionHeight =
                                        QuickExecutionRowHeight;
                        
                                    int fixedHeight =
                                        PanelHeaderHeight +
                                        quickExecutionHeight +
                                        buttonAreaHeight +
                                        padding * 2 +
                                        border * 2;
                        
                                    int maximumScrollHeight =
                                        Math.Max(
                                            120,
                                            maxHeight -
                                            fixedHeight);
                        
                                    for (int i = 0;
                                         i < _panelRows.Count;
                                         i++)
                                    {
                                        TextBlock row =
                                            _panelRows[i];
                        
                                        row.Margin =
                                            new Thickness(
                                                Math.Max(
                                                    0,
                                                    PanelRowPadding),
                                                i == 0
                                                    ? 0
                                                    : Math.Max(
                                                        1,
                                                        PanelRowGap),
                                                Math.Max(
                                                    0,
                                                    PanelRowPadding),
                                                Math.Max(
                                                    0,
                                                    PanelRowPadding));
                        
                                        row.IsVisible =
                                            false;
                                    }
                        
                                    RenderPanelRows(
                                        contentWidth);
                        
                                    int scrollHeight =
                                        EstimatePanelScrollHeight(
                                            contentWidth,
                                            maximumScrollHeight);
                        
                                    ApplyPanelVisualSettings(
                                        contentWidth,
                                        scrollHeight,
                                        maxHeight,
                                        buttons,
                                        buttonHeight,
                                        buttonGap);
                        
                                    SyncQuickExecutionControls();
                                }
    }
}
