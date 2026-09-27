// ============================================================================
// CFIP Indicator — PanelFactory.cs
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
                        
                                        CreateQuickExecutionControls();
                        
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
                        
                                        CreatePanelRows();
                        
                                        _panelStack.AddChild(
                                            _panelHeaderStack);
                        
                                        if (_quickExecutionStack != null)
                                            _panelStack.AddChild(
                                                _quickExecutionStack);
                        
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
        
        private void CreatePanelRows()
                                {
                                    if (_panelRowsStack == null ||
                                        _panelRows.Count == PanelRowCount)
                                        return;
                        
                                    _panelRows.Clear();
                        
                                    for (int i = 0;
                                         i < PanelRowCount;
                                         i++)
                                    {
                                        TextBlock row =
                                            new TextBlock
                                            {
                                                Text = "",
                                                IsVisible = false,
                                                IsHitTestVisible = false,
                                                TextWrapping = TextWrapping.Wrap,
                                                TextTrimming = TextTrimming.None,
                                                TextAlignment = TextAlignment.Left,
                                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                                VerticalAlignment = VerticalAlignment.Top
                                            };
                        
                                        _panelRows.Add(
                                            row);
                        
                                        _panelRowsStack.AddChild(
                                            row);
                                    }
                                }
        
        private void CreatePanelToggleButton()
                                {
                                    if (!ShowPanelToggleButton ||
                                        _panelToggleButton != null)
                                        return;
                        
                                    try
                                    {
                                        _panelToggleButton =
                                            new Button
                                            {
                                                Text = "−",
                                                Width =
                                                    Math.Max(
                                                        22,
                                                        Math.Min(
                                                            40,
                                                            Math.Min(
                                                                PanelToggleWidth,
                                                                PanelToggleHeight))),
                                                Height =
                                                    Math.Max(
                                                        22,
                                                        Math.Min(
                                                            40,
                                                            Math.Min(
                                                                PanelToggleWidth,
                                                                PanelToggleHeight))),
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Left,
                                                VerticalAlignment =
                                                    VerticalAlignment.Center,
                                                HorizontalContentAlignment =
                                                    HorizontalAlignment.Center,
                                                VerticalContentAlignment =
                                                    VerticalAlignment.Center,
                                                ForegroundColor =
                                                    PanelTextColor,
                                                FontSize =
                                                    Math.Max(
                                                        9,
                                                        PanelFontSize),
                                                FontWeight =
                                                    FontWeight.Bold,
                                                BackgroundColor =
                                                    Color.FromArgb(
                                                        100,
                                                        Color.Black),
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
                                                    Math.Min(
                                                        5,
                                                        Math.Max(
                                                            0,
                                                            PanelCornerRadius)),
                                                Margin =
                                                    new Thickness(
                                                        0,
                                                        1,
                                                        8,
                                                        1)
                                            };
                        
                                        _panelToggleButton.Click +=
                                            args => TogglePanel();
                                    }
                                    catch (Exception ex)
                                    {
                                        Print(
                                            "CFIP panel hide button failed: {0}",
                                            ex.Message);
                        
                                        _panelToggleButton = null;
                                    }
                                }
        
        private void CreatePanelRestoreButton()
                                {
                                    if (_panelRestoreButton != null)
                                        return;
                        
                                    try
                                    {
                                        _panelRestoreButton =
                                            new Button
                                            {
                                                Text = "+",
                                                Width = 26,
                                                Height = 26,
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Left,
                                                VerticalAlignment =
                                                    VerticalAlignment.Bottom,
                                                HorizontalContentAlignment =
                                                    HorizontalAlignment.Center,
                                                VerticalContentAlignment =
                                                    VerticalAlignment.Center,
                                                ForegroundColor =
                                                    PanelTextColor,
                                                FontSize =
                                                    Math.Max(
                                                        9,
                                                        PanelFontSize),
                                                FontWeight =
                                                    FontWeight.Bold,
                                                BackgroundColor =
                                                    Color.FromArgb(
                                                        70,
                                                        Color.Black),
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
                                                    Math.Min(
                                                        5,
                                                        Math.Max(
                                                            0,
                                                            PanelCornerRadius)),
                                                IsVisible = false
                                            };
                        
                                        _panelRestoreButton.Click +=
                                            args => TogglePanel();
                        
                                        Chart.AddControl(
                                            _panelRestoreButton);
                        
                                        SetPanelRestoreAlignment();
                                    }
                                    catch (Exception ex)
                                    {
                                        Print(
                                            "CFIP panel restore button failed: {0}",
                                            ex.Message);
                        
                                        _panelRestoreButton = null;
                                    }
                                }
    }
}
