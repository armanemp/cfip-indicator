// ============================================================================
// CFIP Indicator — PanelFactory.cs
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
        private void CreatePanel()
                                {
                                    if (_panel != null)
                                        return;
                        
                                    try
                                    {
                                        int bootstrapWidth =
                                            Math.Max(
                                                260,
                                                Math.Min(
                                                    760,
                                                    PanelWidth));

                                        int bootstrapHeight =
                                            Math.Max(
                                                220,
                                                Math.Min(
                                                    420,
                                                    PanelMaxHeight));

                                        _panelHeaderStack =
                                            new StackPanel
                                            {
                                                Orientation =
                                                    Orientation.Horizontal,
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Stretch,
                                                VerticalAlignment =
                                                    VerticalAlignment.Top,
                                                Height = 30,
                                                BackgroundColor =
                                                    Color.FromArgb(
                                                        0,
                                                        Color.Black)
                                            };
                        
                                        _panelHeaderTitle =
                                            new TextBlock
                                            {
                                                Text = "CFIP SMART",
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Stretch,
                                                VerticalAlignment =
                                                    VerticalAlignment.Center,
                                                TextAlignment =
                                                    TextAlignment.Left,
                                                TextWrapping =
                                                    TextWrapping.NoWrap,
                                                TextTrimming =
                                                    TextTrimming.None,
                                                FontWeight =
                                                    FontWeight.Bold,
                                                BackgroundColor =
                                                    Color.FromArgb(
                                                        0,
                                                        Color.Black)
                                            };
                        
                                        _panelRowsStack =
                                            new StackPanel
                                            {
                                                Orientation =
                                                    Orientation.Vertical,
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Stretch,
                                                VerticalAlignment =
                                                    VerticalAlignment.Top,
                                                BackgroundColor =
                                                    Color.FromArgb(
                                                        0,
                                                        Color.Black)
                                            };
                        
                                        _panelScroll =
                                            new ScrollViewer
                                            {
                                                Width =
                                                    Math.Max(
                                                        240,
                                                        bootstrapWidth - 8),
                                                Height =
                                                    Math.Max(
                                                        120,
                                                        bootstrapHeight - 70),
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Stretch,
                                                VerticalAlignment =
                                                    VerticalAlignment.Top,
                                                HorizontalScrollBarVisibility =
                                                    ScrollBarVisibility.Hidden,
                                                VerticalScrollBarVisibility =
                                                    ScrollBarVisibility.Auto,
                                                BackgroundColor =
                                                    Color.FromArgb(
                                                        0,
                                                        Color.Black)
                                            };
                        
                                        _panelScroll.Content =
                                            _panelRowsStack;
                        
                                        _panelStack =
                                            new StackPanel
                                            {
                                                Orientation =
                                                    Orientation.Vertical,
                                                Width =
                                                    bootstrapWidth,
                                                Height =
                                                    bootstrapHeight,
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Stretch,
                                                VerticalAlignment =
                                                    VerticalAlignment.Top,
                                                BackgroundColor =
                                                    Color.FromArgb(
                                                        0,
                                                        Color.Black)
                                            };
                        
                                        _buttonStack =
                                            new StackPanel
                                            {
                                                Orientation =
                                                    Orientation.Horizontal,
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Stretch,
                                                VerticalAlignment =
                                                    VerticalAlignment.Top,
                                                BackgroundColor =
                                                    Color.FromArgb(
                                                        0,
                                                        Color.Black)
                                            };
                        
                                        // The Indicator panel is analysis/presentation only.
                                        // Broker Close/Cancel actions belong to the cBot surface.
                                        // The hide/show toggle is the only interactive control
                                        // owned by the Indicator panel. Broker actions are cBot-owned.
                                        CreatePanelToggleButton();
                        
                                        if (_panelToggleButton != null)
                                            _buttonStack.AddChild(
                                                _panelToggleButton);
                        
                                        _panelHeaderStack.AddChild(
                                            _panelHeaderTitle);

                                        CreateProcessingHeartbeatLamp();

                                        if (_processingLamp != null)
                                            _panelHeaderStack.AddChild(
                                                _processingLamp);
                        
                                        CreatePanelRows();
                        
                                        _panelStack.AddChild(
                                            _panelHeaderStack);
                        
                                        _panelStack.AddChild(
                                            _panelScroll);
                        
                                        _panelStack.AddChild(
                                            _buttonStack);
                        
                                        _panel =
                                            new Border
                                            {
                                                Child =
                                                    _panelStack,
                                                Width =
                                                    bootstrapWidth,
                                                Height =
                                                    bootstrapHeight,
                                                MinWidth = 260,
                                                MaxWidth = 760,
                                                MinHeight = 170,
                                                MaxHeight = 420,
                                                IsHitTestVisible =
                                                    true,
                                                BackgroundColor =
                                                    Color.FromArgb(
                                                        ShowPanelBackground
                                                            ? Math.Max(
                                                                0,
                                                                Math.Min(
                                                                    255,
                                                                    PanelBackgroundAlpha))
                                                            : 0,
                                                        PanelBackground),
                                                BorderColor =
                                                    Color.FromArgb(
                                                        Math.Max(
                                                            0,
                                                            Math.Min(
                                                                255,
                                                                PanelBorderAlpha)),
                                                        PanelBorder),
                                                BorderThickness =
                                                    Math.Max(
                                                        0,
                                                        PanelBorderThickness),
                                                CornerRadius =
                                                    Math.Max(
                                                        0,
                                                        PanelCornerRadius)
                                            };
                        
                                        SetPanelAlignment();

                                        // Never add an unconstrained panel to the main chart.
                                        // The first layout pass must have a finite geometry or
                                        // cTrader may collapse the chart area while measuring
                                        // the control tree.
                                        Chart.AddControl(
                                            _panel);
                        
                                        CreatePanelRestoreButton();
                                    }
                                    catch (Exception ex)
                                    {
                                        Print(
                                            "CFIP panel creation failed: {0}",
                                            ex.Message);
                        
                                        _panel = null;
                                        _panelStack = null;
                                        _panelHeaderStack = null;
                                        _panelHeaderTitle = null;
                                        _panelRowsStack = null;
                                        _panelScroll = null;
                                        _panelRows.Clear();
                                        _buttonStack = null;
                                        _panelToggleButton = null;
                                        _panelRestoreButton = null;
                                    }
                                }
    }
}
