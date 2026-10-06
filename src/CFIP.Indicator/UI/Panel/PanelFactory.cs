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
        private StackPanel _panelFooterActions;
        private TextBlock _panelDataStatusText;
        private StackPanel _panelDataStatusBars;
        private readonly List<Border> _panelDataStatusBarControls =
            new List<Border>(5);

        private void CreatePanel()
                                {
                                    if (_panel != null)
                                        return;
                        
                                    try
                                    {
                                        int bootstrapWidth =
                                            Math.Max(
                                                220,
                                                Math.Min(
                                                    700,
                                                    PanelWidth));

                                        int bootstrapHeight =
                                            Math.Max(
                                                180,
                                                Math.Min(
                                                    260,
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

                                        _panelDataStatusBars =
                                            new StackPanel
                                            {
                                                Orientation = Orientation.Horizontal,
                                                HorizontalAlignment = HorizontalAlignment.Center,
                                                VerticalAlignment = VerticalAlignment.Center,
                                                Height = PanelDataStatusRowHeight,
                                                BackgroundColor = Color.FromArgb(0, Color.Black)
                                            };

                                        _panelDataStatusText =
                                            new TextBlock
                                            {
                                                Text = "DATA",
                                                Width = 34,
                                                Height = PanelDataStatusRowHeight,
                                                FontFamily = string.IsNullOrWhiteSpace(PanelFontFamily) ? "Arial" : PanelFontFamily,
                                                FontSize = Math.Max(9, PanelFontSize - 2),
                                                FontWeight = FontWeight.Bold,
                                                ForegroundColor = PanelMutedTextColor,
                                                TextAlignment = TextAlignment.Left,
                                                VerticalAlignment = VerticalAlignment.Center,
                                                TextWrapping = TextWrapping.NoWrap
                                            };
                                        _panelDataStatusBars.AddChild(_panelDataStatusText);

                                        string[] dataLabels = { "M1", "M5", "M15", "H1", "H4" };
                                        for (int i = 0; i < dataLabels.Length; i++)
                                        {
                                            Border bar = new Border
                                            {
                                                Width = 38,
                                                Height = 12,
                                                Margin = new Thickness(2, 0, 2, 0),
                                                CornerRadius = 2,
                                                BorderThickness = 1,
                                                BorderColor = PanelBorder,
                                                BackgroundColor = Color.FromArgb(70, PanelMutedTextColor),
                                                Child = new TextBlock
                                                {
                                                    Text = dataLabels[i],
                                                    FontFamily = string.IsNullOrWhiteSpace(PanelFontFamily) ? "Arial" : PanelFontFamily,
                                                    FontSize = Math.Max(7, PanelFontSize - 4),
                                                    FontWeight = FontWeight.Bold,
                                                    ForegroundColor = PanelTextColor,
                                                    TextAlignment = TextAlignment.Center,
                                                    VerticalAlignment = VerticalAlignment.Center,
                                                    TextWrapping = TextWrapping.NoWrap
                                                }
                                            };
                                            _panelDataStatusBarControls.Add(bar);
                                            _panelDataStatusBars.AddChild(bar);
                                        }

                                        _panelFooterActions =
                                            new StackPanel
                                            {
                                                Orientation =
                                                    Orientation.Horizontal,
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Stretch,
                                                VerticalAlignment =
                                                    VerticalAlignment.Top,
                                                Height = PanelFooterMinHeight -
                                                    PanelDataStatusRowHeight -
                                                    PanelFooterActionGap,
                                                BackgroundColor =
                                                    Color.FromArgb(0, Color.Black)
                                            };
                        
                                        // The Indicator panel is analysis/presentation only.
                                        // Broker Close/Cancel actions belong to the cBot surface.
                                        // The hide/show toggle is the only interactive control
                                        // owned by the Indicator panel. Broker actions are cBot-owned.
                                        CreatePanelToggleButton();
                        
                                        if (_panelToggleButton != null)
                                            _panelFooterActions.AddChild(
                                                _panelToggleButton);

                                        CreatePanelAlertMessageRail();

                                        if (_panelAlertMessageStack != null)
                                            _panelFooterActions.AddChild(
                                                _panelAlertMessageStack);

                                        _buttonStack.AddChild(
                                            _panelDataStatusBars);

                                        _buttonStack.AddChild(
                                            _panelFooterActions);

                        
                                        _panelHeaderStack.AddChild(
                                            _panelHeaderTitle);

                                        CreateProcessingHeartbeatLamp();

                                        if (_processingLamp != null)
                                            _panelHeaderStack.AddChild(
                                                _processingLamp);
                        
                                        CreatePanelRows();
                                        CreatePanelTrendTimeframeLampRow();
                        
                                        _panelStack.AddChild(
                                            _panelHeaderStack);
                        
                                        _panelStack.AddChild(
                                            _panelScroll);
                        
                                        // Fixed MTF trend status row: deliberately outside
                                        // the ScrollViewer so it remains visible while rows scroll.
                                        if (_panelTrendTimeframeLampRow != null)
                                            _panelStack.AddChild(
                                                _panelTrendTimeframeLampRow);
                        
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
                                                MinWidth = 220,
                                                MaxWidth = 700,
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

                                        // The panel is an explicitly sized overlay control. Its
                                        // geometry must remain independent of the chart viewport so
                                        // the control cannot participate in a chart-height feedback loop.
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
                                        RemovePanelTrendTimeframeLampRow();
                                        _panelRows.Clear();
                                        _buttonStack = null;
                                        _panelFooterActions = null;
                                        _panelDataStatusText = null;
                                        _panelDataStatusBars = null;
                                        _panelDataStatusBarControls.Clear();
                                        _panelToggleButton = null;
                                        _panelRestoreButton = null;
                                    }
                                }

    }
}