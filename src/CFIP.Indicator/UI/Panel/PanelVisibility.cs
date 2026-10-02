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
                        
                                    if (_panelRestoreButton != null)
                                        _panelRestoreButton.IsVisible =
                                            _panelHidden;

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
                                    _panelStack = null;
                                    _panelHeaderStack = null;
                                    _panelHeaderTitle = null;
                                    _processingLamp = null;
                                    _processingLampPulseIndex = 0;
                                    _panelRowsStack = null;
                                    _panelScroll = null;
                                    _panelRows.Clear();
                                    _buttonStack = null;
                                    _closeButton = null;
                                    _cancelButton = null;
                                    _panelToggleButton = null;
                                    _panelRestoreButton = null;
                                    _panelHidden = false;
                                    _lastPanelContentRefreshUtc =
                                        DateTime.MinValue;
                                    _lastReactionAlertBar = -1;
                                    _panelStableHeader = "";
                                    _panelStableHeaderSinceUtc =
                                        DateTime.MinValue;
                                }
    }
}
