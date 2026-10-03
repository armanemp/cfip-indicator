// ============================================================================
// CFIP Indicator — PanelMainRenderer.cs
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
            bool ownsVisualSnapshot = false;
            if (_panelRenderBusy)
                return;

            _panelRenderBusy = true;

            try
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
                                                        _buttonStack == null)
                                                        return;

                                                    if (_panelAlertMessageStack == null)
                                                        CreatePanelAlertMessageRail();

                                                    UpdatePanelAlertMessageRail();

                
                                                    DateTime now =
                                                        Server.TimeInUtc;

                                                    UpdateProcessingHeartbeatLamp();
                                                    UpdatePanelHeaderLiveState();
                                                    UpdatePanelTrendTimeframeLamps();
                                        
                                                    ownsVisualSnapshot =
                                                        _renderSignalVisualSnapshot == null;

                                                    if (ownsVisualSnapshot)
                                                        _renderSignalVisualSnapshot =
                                                            BuildSignalVisualSnapshot(
                                                                Math.Max(
                                                                    1,
                                                                    _lastEvaluatedM5));

                                                    if (!ShouldRenderFullPanel(
                                                            _renderSignalVisualSnapshot))
                                                        return;

                                                    if (_panelToggleButton == null)
                                                        CreatePanelToggleButton();
                                        
                                                    if (_panelRestoreButton == null)
                                                        CreatePanelRestoreButton();
                                        
                                                    _panel.IsVisible =
                                                        !_panelHidden;

                                                    if (_panelHidden)
                                                        return;
                                        
                                                    int padding =
                                                        Math.Max(
                                                            0,
                                                            PanelPadding);
                                        
                                                    int border =
                                                        Math.Max(
                                                            0,
                                                            PanelBorderThickness);
                                        
                                                    int effectivePanelWidth =
                                                        EffectivePanelWidth();
                                        
                                                    int contentWidth =
                                                        EffectivePanelContentWidth();
                                        
                                                    bool buttons =
                                                        ShowPanelToggleButton;
                                        
                                                    int buttonHeight =
                                                        Math.Max(
                                                            22,
                                                            Math.Min(
                                                                40,
                                                                PanelToggleHeight));

                                                    int toggleHeight =
                                                        Math.Max(
                                                            22,
                                                            Math.Min(
                                                                40,
                                                                PanelToggleHeight));
                                        
                                                    int buttonGap =
                                                        Math.Max(
                                                            0,
                                                            PanelButtonGap);
                                        
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
                                        
                                                    int configuredMaxHeight =
                                                        Math.Max(
                                                            260,
                                                            Math.Min(
                                                                1200,
                                                                PanelMaxHeight));
                                        
                                                    int maxHeight =
                                                        ResolvePanelMaximumHeight(
                                                            configuredMaxHeight);
                                        
                                                    int fixedHeight =
                                                        PanelHeaderHeight +
                                                        PanelTrendTimeframeLampRowHeight +
                                                        PanelTrendTimeframeLampTopSpacing +
                                                        PanelTrendTimeframeLampBottomSpacing +
                                                        buttonAreaHeight +
                                                        padding * 2 +
                                                        border * 2;
                                        
                                                    int maximumScrollHeight =
                                                        Math.Max(
                                                            120,
                                                            maxHeight -
                                                            fixedHeight);
                                        
                                                    RenderPanelRows(
                                                        contentWidth);

                                                    _lastPanelContentRefreshUtc =
                                                        now;
                                        
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
                                        
                                                    _lastPanelRenderUtc =
                                                        now;
                                                
            }
            finally
            {
                if (ownsVisualSnapshot)
                    _renderSignalVisualSnapshot = null;

                _panelRenderBusy = false;
            }
}
    }
}
