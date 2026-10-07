// ============================================================================
// CFIP Indicator — PanelVisibility.cs
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
        private void TogglePanel()
                                {
                                    _panelHidden =
                                        !_panelHidden;

                                    if (!_panelHidden)
                                        _lastPanelContentRefreshUtc =
                                            DateTime.MinValue;
                        
                                    if (_panel != null)
                                        _panel.IsVisible =
                                            !_panelHidden;

                                    // The MTF lamp rail is hosted directly by the panel stack,
                                    // while the footer/action surface is hosted by _buttonStack.
                                    // Hiding the panel makes _buttonStack invisible through the
                                    // normal layout pass. Restoring only _panel.IsVisible would
                                    // therefore expose the lamp rail while leaving the footer
                                    // invisible until another full render occurred.
                                    if (_buttonStack != null)
                                        _buttonStack.IsVisible =
                                            ShowUnifiedPanel &&
                                            !_panelHidden;

                                    if (_panelRestoreButton != null)
                                        _panelRestoreButton.IsVisible =
                                            _panelHidden;

                                    if (!_panelHidden)
                                    {
                                        _lastPanelRenderUtc =
                                            DateTime.MinValue;
                                        RenderPanel();
                                    }

                                }
        
        private void RemovePanel()
                                {
                                    if (_panel != null)
                                    {
                                        try
                                        {
                                            Chart.RemoveControl(
                                                _panel);
                                        }
                                        catch (Exception ex)
                                        {
                                            Print(
                                                "CFIP panel removal failed: {0}",
                                                ex.ToString());
                                        }
                                    }
                        
                                    if (_panelRestoreButton != null)
                                    {
                                        try
                                        {
                                            Chart.RemoveControl(
                                                _panelRestoreButton);
                                        }
                                        catch (Exception ex)
                                        {
                                            Print(
                                                "CFIP panel restore-button removal failed: {0}",
                                                ex.ToString());
                                        }
                                    }
                        
                                    _panel = null;
                                    _lastPanelHeaderLiveKey = "";
                                    _panelStack = null;
                                    _panelHeaderStack = null;
                                    _panelHeaderTitle = null;
                                    _processingLamp = null;
                                    _processingLampPulseIndex = 0;
                                    _panelRowsStack = null;
                                    _panelScroll = null;
                                    _quickExecutionStack = null;
                                    _autoTradingQuickToggle = null;
                                    _automaticOrdersQuickToggle = null;
                                    _panelRows.Clear();
                                    _buttonStack = null;
                                    _panelToggleButton = null;
                                    _panelRestoreButton = null;
                                    _panelHidden = false;
                                    _lastPanelContentRefreshUtc =
                                        DateTime.MinValue;
                                    _panelStableHeader = "";
                                    _panelStableHeaderSinceUtc =
                                        DateTime.MinValue;
                                }
    }
}
