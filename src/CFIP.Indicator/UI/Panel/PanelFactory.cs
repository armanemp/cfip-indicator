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
                        
                                        _closeButton =
                                            new Button();
                        
                                        _cancelButton =
                                            new Button();
                        
                                        _closeButton.Click +=
                                            args => CloseAllPositions();
                        
                                        _cancelButton.Click +=
                                            args => CancelAllOrders();
                        
                                        // Moved (per user request): the hide/show toggle now lives at
                                        // the bottom of the box, to the left of Close/Cancel, instead
                                        // of the header. It must be added to _buttonStack BEFORE the
                                        // other two buttons so it renders left-most in this
                                        // horizontal StackPanel.
                                        CreatePanelToggleButton();
                        
                                        if (_panelToggleButton != null)
                                            _buttonStack.AddChild(
                                                _panelToggleButton);
                        
                                        _buttonStack.AddChild(
                                            _closeButton);
                        
                                        _buttonStack.AddChild(
                                            _cancelButton);
                        
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
                                        _closeButton = null;
                                        _cancelButton = null;
                                        _panelToggleButton = null;
                                        _panelRestoreButton = null;
                                    }
                                }
    }
}
