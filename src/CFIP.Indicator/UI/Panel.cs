using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int PanelRowCount = 64;
                private const int PanelHeaderHeight = 30;
                private const int QuickExecutionRowHeight = 38;
                private const int QuickExecutionButtonHeight = 30;
                private const int QuickExecutionVerticalMargin = 4;
        
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
        
                private void SetPanelRestoreAlignment()
                {
                    if (_panelRestoreButton == null)
                        return;
        
                    switch (PanelPosition)
                    {
                        case PanelCorner.TopLeft:
                            _panelRestoreButton.VerticalAlignment =
                                VerticalAlignment.Top;
                            _panelRestoreButton.HorizontalAlignment =
                                HorizontalAlignment.Left;
                            break;
        
                        case PanelCorner.TopRight:
                            _panelRestoreButton.VerticalAlignment =
                                VerticalAlignment.Top;
                            _panelRestoreButton.HorizontalAlignment =
                                HorizontalAlignment.Right;
                            break;
        
                        case PanelCorner.BottomRight:
                            _panelRestoreButton.VerticalAlignment =
                                VerticalAlignment.Bottom;
                            _panelRestoreButton.HorizontalAlignment =
                                HorizontalAlignment.Right;
                            break;
        
                        default:
                            _panelRestoreButton.VerticalAlignment =
                                VerticalAlignment.Bottom;
                            _panelRestoreButton.HorizontalAlignment =
                                HorizontalAlignment.Left;
                            break;
                    }
        
                    int margin =
                        Math.Max(
                            4,
                            PanelMargin);
        
                    _panelRestoreButton.Margin =
                        new Thickness(
                            margin,
                            margin,
                            margin,
                            margin);
                }
        
                private void TogglePanel()
                {
                    _panelHidden =
                        !_panelHidden;
        
                    if (_panel != null)
                        _panel.IsVisible =
                            !_panelHidden;
        
                    if (_panelRestoreButton != null)
                        _panelRestoreButton.IsVisible =
                            _panelHidden;
                }
        
                
        
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
        
                    int quickExecutionHeight =
                        QuickExecutionRowHeight;
        
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
                        buttons
                            ? buttonContentHeight +
                              buttonMargin * 2
                            : 0;
        
                    int panelHeight =
                        headerHeight +
                        quickExecutionHeight +
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
        
                    _panel.Width =
                        Math.Max(
                            260,
                            PanelWidth);
        
                    _panel.Height =
                        panelHeight;
        
                    _panel.MinWidth = 260;
                    _panel.MaxWidth = 760;
                    _panel.MinHeight = 170;
                    _panel.MaxHeight =
                        Math.Max(
                            220,
                            maxHeight);
        
                    _panel.Padding =
                        padding;
        
                    _panel.Margin =
                        Math.Max(
                            0,
                            PanelMargin);
        
                    _panel.BackgroundColor =
                        Color.FromArgb(
                            backgroundAlpha,
                            PanelBackground);
        
                    _panel.BorderColor =
                        Color.FromArgb(
                            borderAlpha,
                            PanelBorder);
        
                    _panel.BorderThickness =
                        border;
        
                    _panel.CornerRadius =
                        Math.Max(
                            0,
                            PanelCornerRadius);
        
                    _panelHeaderStack.Width =
                        Math.Max(
                            200,
                            contentWidth);
        
                    _panelHeaderStack.Height =
                        headerHeight;
        
                    // The toggle button now lives in the bottom button row (moved per
                    // user request), not the header, so the title reclaims the full
                    // header content width.
                    _panelHeaderTitle.Width =
                        Math.Max(
                            150,
                            contentWidth -
                            8);
        
                    _panelHeaderTitle.FontFamily =
                        string.IsNullOrWhiteSpace(
                            PanelFontFamily)
                            ? "Arial"
                            : PanelFontFamily;
        
                    _panelHeaderTitle.FontSize =
                        Math.Max(
                            9,
                            PanelFontSize);
        
                    _panelHeaderTitle.ForegroundColor =
                        AutoTradingPanelColor();
        
                    _panelHeaderTitle.Text =
                        "CFIP SMART  •  " +
                        AutoTradingPanelLine();
        
                    _panelHeaderTitle.LineHeight =
                        Math.Max(
                            14,
                            PanelFontSize + 2);
        
                    _panelHeaderStack.BackgroundColor =
                        Color.FromArgb(
                            0,
                            Color.Black);
        
                    _panelStack.BackgroundColor =
                        Color.FromArgb(
                            0,
                            Color.Black);
        
                    _panelRowsStack.BackgroundColor =
                        Color.FromArgb(
                            0,
                            Color.Black);
        
                    _panelScroll.BackgroundColor =
                        Color.FromArgb(
                            0,
                            Color.Black);
        
                    _buttonStack.BackgroundColor =
                        Color.FromArgb(
                            0,
                            Color.Black);
        
                    _panelRowsStack.Width =
                        Math.Max(
                            200,
                            contentWidth);
        
                    _panelScroll.Width =
                        Math.Max(
                            200,
                            contentWidth);
        
                    _panelScroll.Height =
                        Math.Max(
                            100,
                            scrollHeight);
        
                    _buttonStack.Width =
                        Math.Max(
                            200,
                            contentWidth);
        
                    _buttonStack.Height =
                        buttonAreaHeight;
        
                    _quickExecutionStack.IsVisible =
                        true;
        
                    if (_quickExecutionStack != null)
                    {
                        int quickGap =
                            Math.Max(
                                2,
                                buttonGap);
        
                        int quickWidth =
                            Math.Max(
                                108,
                                (contentWidth -
                                 quickGap) / 2);
        
                        int quickRightMargin =
                            quickGap / 2;
        
                        int quickLeftMargin =
                            quickGap -
                            quickRightMargin;
        
                        _quickExecutionStack.Width =
                            contentWidth;
                        _quickExecutionStack.Height =
                            QuickExecutionRowHeight;
        
                        if (_autoTradingQuickToggle != null)
                        {
                            _autoTradingQuickToggle.Width =
                                quickWidth;
                            _autoTradingQuickToggle.Height =
                                QuickExecutionButtonHeight;
                            _autoTradingQuickToggle.Margin =
                                new Thickness(
                                    0,
                                    QuickExecutionVerticalMargin,
                                    quickRightMargin,
                                    QuickExecutionVerticalMargin);
                        }
        
                        if (_automaticOrdersQuickToggle != null)
                        {
                            _automaticOrdersQuickToggle.Width =
                                quickWidth;
                            _automaticOrdersQuickToggle.Height =
                                QuickExecutionButtonHeight;
                            _automaticOrdersQuickToggle.Margin =
                                new Thickness(
                                    quickLeftMargin,
                                    QuickExecutionVerticalMargin,
                                    0,
                                    QuickExecutionVerticalMargin);
                        }
                    }
        
                    _buttonStack.IsVisible =
                        buttons;
        
                    for (int i = 0;
                         i < _panelRows.Count;
                         i++)
                    {
                        TextBlock row =
                            _panelRows[i];
        
                        row.Width =
                            Math.Max(
                                190,
                                contentWidth);
        
                        row.FontSize =
                            Math.Max(
                                8,
                                PanelFontSize);
        
                        row.FontFamily =
                            string.IsNullOrWhiteSpace(
                                PanelFontFamily)
                                ? "Arial"
                                : PanelFontFamily;
        
                        row.LineHeight =
                            Math.Max(
                                14,
                                PanelFontSize + 3);
        
                        row.BackgroundColor =
                            Color.FromArgb(
                                0,
                                Color.Black);
                    }
        
                    int toggleSide =
                        Math.Max(
                            22,
                            Math.Min(
                                40,
                                Math.Min(
                                    PanelToggleWidth,
                                    PanelToggleHeight)));
        
                    int halfGap =
                        Math.Max(
                            0,
                            buttonGap / 2);
        
                    int horizontalFootprint =
                        ShowPanelToggleButton
                            ? toggleSide +
                              buttonMargin * 3 +
                              halfGap * 3
                            : buttonMargin * 2 +
                              buttonGap;
        
                    int availableButtonWidth =
                        Math.Max(
                            0,
                            contentWidth -
                            horizontalFootprint);
        
                    int eachButtonWidth =
                        Math.Max(
                            70,
                            Math.Min(
                                Math.Max(
                                    70,
                                    ActionButtonWidth),
                                availableButtonWidth / 2));
        
                    bool showSafetyButtons =
                        ShowTradeActionButtons ||
                        AlwaysShowSafetyButtons;
        
                    if (_closeButton != null)
                    {
                        _closeButton.IsVisible =
                            showSafetyButtons;
        
                        _closeButton.Width =
                            eachButtonWidth;
        
                        _closeButton.Height =
                            Math.Max(
                                26,
                                buttonHeight);
        
                        _closeButton.Text =
                            eachButtonWidth < 120
                                ? "CLOSE"
                                : "CLOSE POSITIONS";
        
                        _closeButton.ForegroundColor =
                            PanelTextColor;
        
                        _closeButton.FontSize =
                            Math.Max(
                                8,
                                PanelFontSize - 1);
        
                        _closeButton.FontWeight =
                            FontWeight.Bold;
        
                        _closeButton.BackgroundColor =
                            Color.FromArgb(
                                ShowPanelBackground
                                    ? 120
                                    : 0,
                                SlLineColor);
        
                        _closeButton.BorderColor =
                            SlLineColor;
        
                        _closeButton.BorderThickness =
                            border;
        
                        _closeButton.CornerRadius =
                            Math.Max(
                                0,
                                PanelCornerRadius);
        
                        _closeButton.Margin =
                            new Thickness(
                                buttonMargin,
                                buttonMargin,
                                buttonGap / 2,
                                buttonMargin);
        
                        _closeButton.HorizontalContentAlignment =
                            HorizontalAlignment.Center;
        
                        _closeButton.VerticalContentAlignment =
                            VerticalAlignment.Center;
                    }
        
                    if (_cancelButton != null)
                    {
                        _cancelButton.IsVisible =
                            showSafetyButtons;
        
                        _cancelButton.Width =
                            eachButtonWidth;
        
                        _cancelButton.Height =
                            Math.Max(
                                26,
                                buttonHeight);
        
                        _cancelButton.Text =
                            eachButtonWidth < 120
                                ? "CANCEL"
                                : "CANCEL ORDERS";
        
                        _cancelButton.ForegroundColor =
                            PanelTextColor;
        
                        _cancelButton.FontSize =
                            Math.Max(
                                8,
                                PanelFontSize - 1);
        
                        _cancelButton.FontWeight =
                            FontWeight.Bold;
        
                        _cancelButton.BackgroundColor =
                            Color.FromArgb(
                                ShowPanelBackground
                                    ? 82
                                    : 0,
                                Color.FromHex("#2A313B"));
        
                        _cancelButton.BorderColor =
                            Color.FromArgb(
                                Math.Max(
                                    60,
                                    Math.Min(
                                        255,
                                        PanelBorderAlpha)),
                                PanelBorder);
        
                        _cancelButton.BorderThickness =
                            border;
        
                        _cancelButton.CornerRadius =
                            Math.Max(
                                0,
                                PanelCornerRadius);
        
                        _cancelButton.Margin =
                            new Thickness(
                                buttonGap / 2,
                                buttonMargin,
                                buttonMargin,
                                buttonMargin);
        
                        _cancelButton.HorizontalContentAlignment =
                            HorizontalAlignment.Center;
        
                        _cancelButton.VerticalContentAlignment =
                            VerticalAlignment.Center;
                    }
        
                    if (_panelToggleButton != null)
                    {
                        _panelToggleButton.IsVisible =
                            ShowPanelToggleButton;
        
                        _panelToggleButton.Width =
                            toggleSide;
        
                        _panelToggleButton.Height =
                            toggleSide;
        
                        _panelToggleButton.ForegroundColor =
                            PanelTextColor;
        
                        _panelToggleButton.FontSize =
                            Math.Max(
                                9,
                                PanelFontSize);
        
                        _panelToggleButton.BackgroundColor =
                            Color.FromArgb(
                                ShowPanelBackground
                                    ? 70
                                    : 0,
                                PanelBackground);
        
                        _panelToggleButton.BorderColor =
                            Color.FromArgb(
                                borderAlpha,
                                PanelBorder);
        
                        _panelToggleButton.BorderThickness =
                            border;
        
                        _panelToggleButton.CornerRadius =
                            Math.Min(
                                5,
                                Math.Max(
                                    0,
                                    PanelCornerRadius));
        
                        _panelToggleButton.VerticalAlignment =
                            VerticalAlignment.Center;
        
                        _panelToggleButton.HorizontalAlignment =
                            HorizontalAlignment.Left;
        
                        // Sits in the bottom button row now, left of Close/Cancel —
                        // match their vertical margin so all three buttons line up.
                        _panelToggleButton.Margin =
                            new Thickness(
                                buttonMargin,
                                buttonMargin,
                                buttonGap / 2,
                                buttonMargin);
                    }
        
                    if (_panelRestoreButton != null)
                    {
                        _panelRestoreButton.IsVisible =
                            _panelHidden;
        
                        _panelRestoreButton.Width = 26;
                        _panelRestoreButton.Height = 26;
                        _panelRestoreButton.ForegroundColor =
                            PanelTextColor;
                        _panelRestoreButton.FontSize =
                            Math.Max(
                                9,
                                PanelFontSize);
                        _panelRestoreButton.BackgroundColor =
                            Color.FromArgb(
                                ShowPanelBackground
                                    ? 70
                                    : 0,
                                Color.Black);
                        _panelRestoreButton.BorderColor =
                            Color.FromArgb(
                                borderAlpha,
                                PanelBorder);
                        _panelRestoreButton.BorderThickness =
                            border;
                        _panelRestoreButton.CornerRadius =
                            Math.Min(
                                5,
                                Math.Max(
                                    0,
                                    PanelCornerRadius));
        
                        SetPanelRestoreAlignment();
                    }
        
                    SetPanelAlignment();
                }
        
                                        private int EstimatePanelScrollHeight(
                    int contentWidth,
                    int maxScrollHeight)
                {
                    int fontSize =
                        Math.Max(
                            8,
                            PanelFontSize);
        
                    int lineHeight =
                        Math.Max(
                            14,
                            fontSize + 3);
        
                    int charsPerLine =
                        Math.Max(
                            24,
                            (int)(
                                Math.Max(
                                    160,
                                    contentWidth) /
                                Math.Max(
                                    4.5,
                                    fontSize * 0.55)));
        
                    int total = 0;
        
                    for (int i = 0;
                         i < _panelRows.Count;
                         i++)
                    {
                        TextBlock row =
                            _panelRows[i];
        
                        if (row == null ||
                            !row.IsVisible)
                            continue;
        
                        int length =
                            string.IsNullOrEmpty(
                                row.Text)
                                ? 1
                                : row.Text.Length;
        
                        int lines =
                            Math.Max(
                                1,
                                (int)Math.Ceiling(
                                    (double)length /
                                    charsPerLine));
        
                        lines =
                            Math.Min(
                                4,
                                lines);
        
                        total +=
                            lines *
                            lineHeight +
                            Math.Max(
                                0,
                                PanelRowPadding) *
                            2 +
                            Math.Max(
                                1,
                                PanelRowGap);
                    }
        
                    return
                        Math.Max(
                            120,
                            Math.Min(
                                Math.Max(
                                    120,
                                    maxScrollHeight),
                                total + 4));
                }
        
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
        
                private string GetAutoTradingPanelState()
                {
                    if (!AutoTradingEnabled)
                        return "OFF";
        
                    string state =
                        string.IsNullOrWhiteSpace(
                            _autoTradingState)
                            ? "ARMED"
                            : _autoTradingState;
        
                    if (string.IsNullOrWhiteSpace(
                            _autoTradingReason))
                        return "ON • " + state;
        
                    return
                        "ON • " +
                        state +
                        " • " +
                        CompactText(
                            _autoTradingReason,
                            72);
                }
        
                private Color GetAutoTradingPanelColor()
                {
                    if (!AutoTradingEnabled)
                        return PanelMutedTextColor;
        
                    if ((EnableAggressiveAutoEntry &&
                         _reaction != null &&
                         _reaction.EntryAllowed) ||
                        (_plan != null &&
                         _decision != null &&
                         _decision.EntryAllowed))
                        return TpLineColor;
        
                    return PanelAccentColor;
                }
        
                private string GetAutoProtectionPanelState()
                {
                    if (!AutoTradingEnabled)
                        return "DISABLED WITH AUTO ENGINE";
        
                    if (!AutoBrokerProtection &&
                        !AutoProtectBrokerPositions)
                        return "OFF";
        
                    string state = "";
        
                    if (AutoBrokerProtection)
                        state = "NEW TRADES";
        
                    if (AutoProtectBrokerPositions)
                        state +=
                            string.IsNullOrEmpty(state)
                                ? "MANAGED POSITIONS"
                                : " + MANAGED POSITIONS";
        
                    return state;
                }
        
                private string PredictionReadinessText()
                {
                    if (_decision == null)
                        return "NO DECISION";
        
                    if (_decision.EntryAllowed)
                        return "ENTRY CONFIRMED";
        
                    if (_reaction != null &&
                        _reaction.EntryAllowed)
                        return "LIVE REACTION READY";
        
                    if (_prediction != null &&
                        _prediction.Direction != 0)
                        return
                            (_prediction.Direction == 1 ? "BUY" : "SELL") +
                            " PREDICTED • CONF " +
                            _prediction.Confidence;
        
                    return
                        "WAIT • " +
                        (string.IsNullOrWhiteSpace(_decision.BlockReason)
                            ? "STRUCTURAL GATE"
                            : _decision.BlockReason);
                }
        
                private Color PredictionReadinessColor()
                {
                    if (_decision != null &&
                        _decision.EntryAllowed)
                        return TpLineColor;
        
                    if (_reaction != null &&
                        _reaction.EntryAllowed)
                        return PanelAccentColor;
        
                    return PanelWarningColor;
                }
        
                private string DailyPivotPanelText()
                {
                    if (_d1Bars == null ||
                        _d1Bars.Count < 3)
                        return "UNAVAILABLE";
        
                    int idx =
                        ClosedIndex(
                            _d1Bars,
                            TimeInUtc);
        
                    if (idx <= 0)
                        return "UNAVAILABLE";
        
                    int prev = idx - 1;
        
                    double h = _d1Bars.HighPrices[prev];
                    double l = _d1Bars.LowPrices[prev];
                    double c = _d1Bars.ClosePrices[prev];
        
                    if (h <= l)
                        return "INVALID";
        
                    double p = (h + l + c) / 3.0;
        
                    double r1 = 2.0 * p - l;
                    double s1 = 2.0 * p - h;
                    double r2 = p + (h - l);
                    double s2 = p - (h - l);
        
                    return
                        "P " + Price(p) +
                        "  R1 " + Price(r1) +
                        "  S1 " + Price(s1) +
                        "  R2 " + Price(r2) +
                        "  S2 " + Price(s2);
                }
        
                private string GetExecutionRelationText(
                    ExecutionModel model,
                    double entry)
                {
                    if (model == null)
                        return "NONE";
        
                    if (model.Mode ==
                        ExecutionMode.WaitingForTrigger)
                        return
                            model.Direction == 1
                                ? "BUY WAIT • ENTRY MUST REACH TRIGGER ABOVE"
                                : "SELL WAIT • ENTRY MUST REACH TRIGGER BELOW";
        
                    if (!IsFinitePositive(entry))
                        return "NOT EXECUTABLE";
        
                    if (model.Mode ==
                        ExecutionMode.BreakoutMarket)
                    {
                        double tolerance =
                            Math.Max(
                                Symbol.TickSize * 2,
                                Math.Max(
                                    Symbol.PipSize * 0.5,
                                    (Symbol.Ask - Symbol.Bid) * 2));
        
                        bool acceptable =
                            model.Direction == 1
                                ? entry >= model.Trigger - tolerance
                                : entry <= model.Trigger + tolerance;
        
                        if (!acceptable)
                            return "BREAKOUT NOT CONFIRMED";
        
                        return IsTriggerReached(
                            model.Direction,
                            entry,
                            model.Trigger)
                            ? "BREAKOUT CONFIRMED"
                            : "BREAKOUT • SLIPPAGE ACCEPTED";
                    }
        
                    if (model.Mode ==
                        ExecutionMode.RetestMarket)
                        return "RETEST INSIDE ZONE";
        
                    return ExecutionModeText(model.Mode);
                }
        
                private void RenderPanelRows(
                    int contentWidth)
                {
                    int slot = 0;
        
                    int authoritativeDirection =
                        GetAuthoritativeDirection();
        
                    _authoritativeDirection =
                        authoritativeDirection;
        
                    _authoritativeState =
                        GetAuthoritativeState(
                            authoritativeDirection);
        
                    string stableState =
                        GetStablePanelState(
                            _authoritativeState);
        
                    int stateDirection =
                        stableState.StartsWith(
                            "BUY",
                            StringComparison.OrdinalIgnoreCase)
                            ? 1
                            : stableState.StartsWith(
                                "SELL",
                                StringComparison.OrdinalIgnoreCase)
                                ? -1
                                : 0;
        
                    AddPanelRow(
                        ref slot,
                        "CFIP SMART   •  " +
                        stableState,
                        PanelDirectionColor(
                            stateDirection),
                        true,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        AutoTradingPanelLine(),
                        AutoTradingPanelColor(),
                        true,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "SMART ACTION  •  " +
                        _authoritativeState,
                        PanelDirectionColor(
                            authoritativeDirection),
                        true,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "SYNC  " +
                        GetSignalSynchronizationText(),
                        GetSignalSynchronizationColor(),
                        true,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        SymbolName +
                        "  •  " +
                        Bars.TimeFrame +
                        "  •  " +
                        TimeInUtc.ToString(
                            "HH:mm:ss") +
                        " UTC",
                        PanelMutedTextColor,
                        false,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        GetSessionPanelText(),
                        GetSessionPanelColor(),
                        true,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "SUITABILITY  " +
                        _marketSuitabilityScore +
                        "/100  •  " +
                        _marketSuitabilityState +
                        "  •  " +
                        CompactText(
                            _marketSuitabilityReason,
                            54),
                        _marketSuitabilityScore >=
                            Math.Max(
                                50,
                                MinimumMarketSuitability)
                            ? TpLineColor
                            : PanelWarningColor,
                        true,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "SMART RISK  " +
                        EffectiveAutoRiskPercent().ToString("F2") +
                        "%  •  " +
                        SuitabilityRiskMultiplier().ToString("F2") +
                        "x BASE",
                        PanelSecondaryTextColor,
                        false,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "AUTO EXEC  " +
                        (AutoTradingEnabled
                            ? "ON"
                            : "OFF") +
                        "  •  " +
                        CompactText(
                            _autoExecutionBlockReason,
                            72),
                        AutoTradingEnabled &&
                        string.Equals(
                            _autoExecutionBlockReason,
                            "READY TO SUBMIT",
                            StringComparison.OrdinalIgnoreCase)
                            ? TpLineColor
                            : PanelSecondaryTextColor,
                        false,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "EXEC MODE  " +
                        (_executionModel == null
                            ? "NONE"
                            : ExecutionModeText(
                                _executionModel.Mode)) +
                        "  •  ENTRY " +
                        (_executionModel != null &&
                         IsFinitePositive(
                             _executionModel.ActualEntry)
                            ? Price(
                                _executionModel.ActualEntry)
                            : "WAIT") +
                        "  •  TRIGGER " +
                        (_executionModel != null
                            ? Price(
                                _executionModel.Trigger)
                            : "-"),
                        _executionModel != null &&
                        _executionModel.Mode ==
                            ExecutionMode.BreakoutMarket
                            ? EntryLineColor
                            : TriggerLineColor,
                        true,
                        contentWidth);
        
                    if (_executionModel != null &&
                        _executionModel.Direction != 0)
                    {
                        AddPanelRow(
                            ref slot,
                            "ENTRY RELATION  " +
                            GetExecutionRelationText(
                                _executionModel,
                                _executionModel.ActualEntry),
                            PanelSecondaryTextColor,
                            false,
                            contentWidth);
                    }
        
                    AddPanelRow(
                        ref slot,
                        "AUTO ORDERS  " +
                        (AutomaticOrdersEnabled
                            ? "ON"
                            : "OFF") +
                        "  •  " +
                        CompactText(
                            _autoOrdersBlockReason,
                            72),
                        AutomaticOrdersEnabled &&
                        string.Equals(
                            _autoOrdersBlockReason,
                            "ORDER PLACED",
                            StringComparison.OrdinalIgnoreCase)
                            ? TpLineColor
                            : PanelSecondaryTextColor,
                        false,
                        contentWidth);
        
                    if (UseDailyPivots)
                    {
                        AddPanelRow(
                            ref slot,
                            "PIVOT  " + DailyPivotPanelText(),
                            PanelAccentColor,
                            false,
                            contentWidth);
                    }
        
                    bool tradingPermission =
                        HasTradingPermission();
        
                    AddPanelRow(
                        ref slot,
                        "PERMISSION  •  " +
                        (tradingPermission
                            ? "TRADING ALLOWED"
                            : "TRADING NOT GRANTED"),
                        tradingPermission
                            ? TpLineColor
                            : PanelWarningColor,
                        true,
                        contentWidth);
        
                    if (ShowSpreadDiagnostics)
                    {
                        double spreadPips =
                            Math.Max(
                                0,
                                (Symbol.Ask - Symbol.Bid) /
                                Math.Max(
                                    Symbol.PipSize,
                                    1e-9));
        
                        double spreadAtrRatio = 0;
        
                        if (_m5Frame != null &&
                            _m5Frame.Atr > 0)
                        {
                            spreadAtrRatio =
                                (Symbol.Ask - Symbol.Bid) /
                                _m5Frame.Atr;
                        }
        
                        AddPanelRow(
                            ref slot,
                            "SPREAD  " +
                            spreadPips.ToString("F1") +
                            " pips  •  ATR " +
                            spreadAtrRatio.ToString("F3"),
                            spreadPips > 0 &&
                            UseSpreadFilter &&
                            _m5Frame != null &&
                            _m5Frame.Atr > 0 &&
                            (Symbol.Ask - Symbol.Bid) /
                            _m5Frame.Atr >
                            MaximumSpreadAtr
                                ? PanelWarningColor
                                : PanelMutedTextColor,
                            false,
                            contentWidth);
                    }
        
                    if (ShowEngineStatus)
                    {
                        AddPanelRow(
                            ref slot,
                            "ENGINE  " +
                            _status,
                            _status.IndexOf(
                                "WAIT",
                                StringComparison.OrdinalIgnoreCase) >= 0
                                ? PanelWarningColor
                                : PanelMutedTextColor,
                            false,
                            contentWidth);
                    }
        
                    if (_decision != null)
                    {
                        int direction =
                            _decision.Direction;
        
                        string decisionState =
                            direction == 1
                                ? "BUY"
                                : direction == -1
                                    ? "SELL"
                                    : "NEUTRAL";
        
                        AddPanelRow(
                            ref slot,
                            "DECISION  •  " +
                            decisionState +
                            "  •  " +
                            (_decision.EntryAllowed
                                ? "READY"
                                : "WATCH / BLOCKED"),
                            PanelDirectionColor(
                                direction),
                            true,
                            contentWidth);
        
                        AddPanelRow(
                            ref slot,
                            "CONF " +
                            _decision.Confidence +
                            "  •  EDGE " +
                            _decision.Edge +
                            "  •  SMART " +
                            _decision.SmartQuality,
                            PanelDirectionColor(
                                direction),
                            true,
                            contentWidth);
        
                        AddPanelRow(
                            ref slot,
                            "MTF " +
                            _decision.TimeframeAgreement +
                            "  •  EVID " +
                            _decision.IndependentEvidence +
                            "  •  STRUCT " +
                            _decision.StructuralConfirmations,
                            PanelSecondaryTextColor,
                            false,
                            contentWidth);
        
                        AddPanelRow(
                            ref slot,
                            "REGIME " +
                            _decision.Regime +
                            "  •  Q" +
                            _decision.RegimeQuality +
                            "  •  RETEST " +
                            _decision.RetestQuality +
                            "  •  SHARE " +
                            _decision.BuyShare +
                            "/" +
                            _decision.SellShare,
                            PanelSecondaryTextColor,
                            false,
                            contentWidth);
        
                        AddPanelRow(
                            ref slot,
                            "CONFLUENCE  " +
                            ConfluenceText(
                                _m5Frame),
                            PanelAccentColor,
                            false,
                            contentWidth);
        
                        AddPanelRow(
                            ref slot,
                            _decision.TriggerReady
                                ? "TRIGGER  CONFIRMED"
                                : "TRIGGER  WAITING",
                            _decision.TriggerReady
                                ? TriggerLineColor
                                : PanelWarningColor,
                            true,
                            contentWidth);
        
                        if (!string.IsNullOrWhiteSpace(
                                _decision.BlockReason))
                        {
                            AddPanelRow(
                                ref slot,
                                "BLOCK  " +
                                _decision.BlockReason,
                                SlLineColor,
                                true,
                                contentWidth);
                        }
        
                        if (_prediction != null &&
                            _prediction.Direction != 0 &&
                            _prediction.Confidence >=
                            Math.Max(
                                MinimumEarlyConfidence,
                                EarlySetupConfidence))
                        {
                            AddPanelRow(
                                ref slot,
                                "EARLY ANALYSIS  •  " +
                                (_prediction.Direction == 1
                                    ? "BUY"
                                    : "SELL") +
                                "  •  CONF " +
                                _prediction.Confidence,
                                PanelDirectionColor(
                                    _prediction.Direction),
                                true,
                                contentWidth);
        
                            AddPanelRow(
                                ref slot,
                                "PREDICTION  " +
                                ExecutionModeText(
                                    _prediction.Mode) +
                                "  •  ENTRY " +
                                Price(_prediction.Entry) +
                                "  •  TRIGGER " +
                                Price(_prediction.Trigger),
                                PanelDirectionColor(
                                    _prediction.Direction),
                                false,
                                contentWidth);
        
                            AddPanelRow(
                                ref slot,
                                "PRED TARGETS  " +
                                Price(_prediction.Target1) +
                                "  /  " +
                                Price(_prediction.Target2) +
                                "  /  " +
                                Price(_prediction.Target3) +
                                "  /  " +
                                Price(_prediction.Target4),
                                PanelSecondaryTextColor,
                                false,
                                contentWidth);
                        }
                    }
        
                    AddPanelRow(
                        ref slot,
                        "READINESS  " +
                        PredictionReadinessText(),
                        PredictionReadinessColor(),
                        true,
                        contentWidth);
        
                    if (_executionModel != null &&
                        _executionModel.Direction != 0)
                    {
                        AddPanelRow(
                            ref slot,
                            "ENTRY MODEL  •  " +
                            ExecutionModeText(
                                _executionModel.Mode) +
                            "  •  " +
                            _executionModel.Source +
                            "  •  Q" +
                            _executionModel.Quality,
                            PanelDirectionColor(
                                _executionModel.Direction),
                            true,
                            contentWidth);
        
                        AddPanelRow(
                            ref slot,
                            "IDEAL ENTRY  " +
                            Price(
                                _executionModel.IdealEntry) +
                            "  •  ZONE " +
                            Price(
                                _executionModel.ZoneLow) +
                            " → " +
                            Price(
                                _executionModel.ZoneHigh),
                            PanelSecondaryTextColor,
                            false,
                            contentWidth);
        
                        AddPanelRow(
                            ref slot,
                            "ENTRY TRIGGER  " +
                            Price(
                                _executionModel.Trigger) +
                            "  •  INVALIDATION " +
                            Price(
                                _executionModel.Invalidation),
                            _executionModel.Ready
                                ? TriggerLineColor
                                : PanelWarningColor,
                            false,
                            contentWidth);
                    }
        
                    if (_plan != null &&
                        ShowTradePlanPanel)
                    {
                        AddPanelRow(
                            ref slot,
                            "TRADE PLAN  •  " +
                            (_plan.Direction == 1
                                ? "BUY ACTIVE"
                                : "SELL ACTIVE"),
                            PanelDirectionColor(
                                _plan.Direction),
                            true,
                            contentWidth);
        
                        if (ShowLevelPricesInUnifiedPanel &&
                            ShowEntry)
                        {
                            AddPanelRow(
                                ref slot,
                                "ENTRY  " +
                                Price(_plan.Entry) +
                                "  •  IDEAL " +
                                Price(_plan.IdealEntry) +
                                "  •  Q" +
                                _plan.EntryQuality +
                                "  •  " +
                                _plan.EntrySource,
                                EntryLineColor,
                                true,
                                contentWidth);
                        }
        
                        if (ShowLevelPricesInUnifiedPanel &&
                            ShowSL)
                        {
                            AddPanelRow(
                                ref slot,
                                "STOP LOSS  " +
                                Price(_plan.Stop) +
                                "  •  " +
                                _plan.StopSource +
                                "  •  Q" +
                                _plan.StopQuality +
                                "  •  RISK " +
                                (_plan.Risk /
                                 Math.Max(
                                     Symbol.PipSize,
                                     1e-9)).ToString("F1") +
                                "p",
                                SlLineColor,
                                true,
                                contentWidth);
                        }
        
                        if (ShowLevelPricesInUnifiedPanel &&
                            ShowTP1)
                        {
                            AddPanelRow(
                                ref slot,
                                "TAKE PROFIT 1  " +
                                Price(_plan.Tp1) +
                                "  •  RR " +
                                _plan.Tp1RR.ToString(
                                    "F2") +
                                "  •  " +
                                _plan.Tp1Source +
                                "  •  Q" +
                                _plan.Tp1Quality +
                                (_tp1Hit != 0
                                    ? "  •  HIT"
                                    : ""),
                                TpLineColor,
                                true,
                                contentWidth);
                        }
        
                        if (ShowLevelPricesInUnifiedPanel &&
                            ShowTP2 &&
                            _plan.Tp2 > 0)
                        {
                            AddPanelRow(
                                ref slot,
                                "TAKE PROFIT 2  " +
                                Price(_plan.Tp2) +
                                "  •  RR " +
                                _plan.Tp2RR.ToString(
                                    "F2") +
                                "  •  " +
                                _plan.Tp2Source +
                                "  •  Q" +
                                _plan.Tp2Quality +
                                (_tp2Hit != 0
                                    ? "  •  HIT"
                                    : ""),
                                Tp2LineColor,
                                true,
                                contentWidth);
                        }
        
                        if (ShowLevelPricesInUnifiedPanel &&
                            ShowTP3 &&
                            _plan.Tp3 > 0)
                        {
                            AddPanelRow(
                                ref slot,
                                "TAKE PROFIT 3  " +
                                Price(_plan.Tp3) +
                                "  •  RR " +
                                _plan.Tp3RR.ToString(
                                    "F2") +
                                "  •  " +
                                _plan.Tp3Source +
                                "  •  Q" +
                                _plan.Tp3Quality +
                                (_tp3Hit != 0
                                    ? "  •  HIT"
                                    : ""),
                                Tp3LineColor,
                                true,
                                contentWidth);
                        }
        
                        if (ShowLevelPricesInUnifiedPanel &&
                            ShowTP4 &&
                            _plan.Tp4 > 0)
                        {
                            AddPanelRow(
                                ref slot,
                                "TAKE PROFIT 4  " +
                                Price(_plan.Tp4) +
                                "  •  RR " +
                                _plan.Tp4RR.ToString(
                                    "F2") +
                                "  •  " +
                                _plan.Tp4Source +
                                "  •  Q" +
                                _plan.Tp4Quality +
                                (_tp4Hit != 0
                                    ? "  •  HIT"
                                    : ""),
                                Tp4LineColor,
                                true,
                                contentWidth);
                        }
        
                        AddPanelRow(
                            ref slot,
                            "REWARD MODEL  •  HTF TARGETS " +
                            _plan.HtfTargetCount +
                            "  •  MAX RR " +
                            Math.Max(
                                0,
                                MaximumRewardRR).ToString("F2"),
                            PanelAccentColor,
                            false,
                            contentWidth);
        
                        double liveRR =
                            _plan.Risk > 0
                                ? (_plan.Direction == 1
                                    ? _lastMarket - _plan.Entry
                                    : _plan.Entry - _lastMarket) /
                                  _plan.Risk
                                : 0;
        
                        int exitPressure =
                            CalculateSmartExitPressure(
                                _lastMarket,
                                liveRR);
        
                        AddPanelRow(
                            ref slot,
                            "LIVE  RR " +
                            liveRR.ToString(
                                "F2") +
                            "  •  TP HIT " +
                            _tp1Hit +
                            "/" +
                            _tp2Hit +
                            "/" +
                            _tp3Hit +
                            "/" +
                            _tp4Hit,
                            liveRR >= 0
                                ? TpLineColor
                                : SlLineColor,
                            true,
                            contentWidth);
        
                         Position managedPosition =
                             GetManagedPosition();
        
                         if (managedPosition != null)
                         {
                             AddPanelRow(
                                 ref slot,
                                 "POSITION  •  " +
                                 (managedPosition.TradeType ==
                                      TradeType.Buy
                                     ? "BUY"
                                     : "SELL") +
                                 "  #" +
                                 managedPosition.Id +
                                 "  •  VOL " +
                                 managedPosition.VolumeInUnits.ToString("F0") +
                                 "  •  P/L " +
                                 managedPosition.NetProfit.ToString("F2") +
                                 "  •  SL " +
                                 (managedPosition.StopLoss.HasValue
                                     ? Price(
                                         managedPosition.StopLoss.Value)
                                     : "-") +
                                 "  •  TP " +
                                 (managedPosition.TakeProfit.HasValue
                                     ? Price(
                                         managedPosition.TakeProfit.Value)
                                     : "-") +
                                 "  •  " +
                                 BrokerTargetStageText(
                                     managedPosition.TakeProfit.HasValue
                                         ? managedPosition.TakeProfit.Value
                                         : 0),
                                 managedPosition.NetProfit >= 0
                                     ? TpLineColor
                                     : SlLineColor,
                                 true,
                                 contentWidth);
                         }
        
                        AddPanelRow(
                            ref slot,
                            "SMART EXIT  " +
                            GetSmartExitMode() +
                            "  •  PRESSURE " +
                            exitPressure,
                            exitPressure >=
                                SmartExitPressureThreshold
                                ? SlLineColor
                                : exitPressure >=
                                  LiveReactionWatchThreshold
                                    ? PanelWarningColor
                                    : TpLineColor,
                            true,
                            contentWidth);
                    }
        
                    if (_reaction != null &&
                        _reaction.Direction != 0)
                    {
                        AddPanelRow(
                            ref slot,
                            "LIVE REACTION  •  " +
                            (_reaction.Direction == 1
                                ? "BUY"
                                : "SELL") +
                            "  •  Q" +
                            _reaction.Confidence +
                            "  •  EVID " +
                            _reaction.IndependentEvidence +
                            (_reaction.EntryAllowed
                                ? "  •  READY"
                                : "  •  WATCH"),
                            PanelDirectionColor(
                                _reaction.Direction),
                            true,
                            contentWidth);
                    }
        
                    AddPanelRow(
                        ref slot,
                        "MTF ALIGNMENT",
                        PanelSectionColor,
                        true,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "M1   " +
                        FrameText(_m1Frame),
                        PanelDirectionColor(
                            FrameDirection(_m1Frame)),
                        false,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "M5   " +
                        FrameText(_m5Frame),
                        PanelDirectionColor(
                            FrameDirection(_m5Frame)),
                        false,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "M15  " +
                        FrameText(_m15Frame),
                        PanelDirectionColor(
                            FrameDirection(_m15Frame)),
                        false,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "M30  " +
                        FrameText(_m30Frame),
                        PanelDirectionColor(
                            FrameDirection(_m30Frame)),
                        false,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "H1   " +
                        FrameText(_h1Frame),
                        PanelDirectionColor(
                            FrameDirection(_h1Frame)),
                        false,
                        contentWidth);
        
                    AddPanelRow(
                        ref slot,
                        "H4   " +
                        FrameText(_h4Frame),
                        PanelDirectionColor(
                            FrameDirection(_h4Frame)),
                        false,
                        contentWidth);
        
                    if (SmartWeeklyContext)
                    {
                        AddPanelRow(
                            ref slot,
                            "D1   " +
                            FrameText(_d1Frame),
                            PanelDirectionColor(
                                FrameDirection(_d1Frame)),
                            false,
                            contentWidth);
        
                        AddPanelRow(
                            ref slot,
                            "W1   " +
                            FrameText(_w1Frame),
                            PanelDirectionColor(
                                FrameDirection(_w1Frame)),
                            false,
                            contentWidth);
                    }
        
                    if (ShowOutcomeDiagnostics)
                    {
                        AddPanelRow(
                            ref slot,
                            "OUTCOME  W" +
                            _wins +
                            "  •  L" +
                            _losses +
                            "  •  CAL " +
                            CalibrationText(),
                            PanelAccentColor,
                            false,
                            contentWidth);
                    }
        
                    AddPanelRow(
                        ref slot,
                        "AUTO TRADING  •  " +
                        GetAutoTradingPanelState(),
                        GetAutoTradingPanelColor(),
                        true,
                        contentWidth);
        
                    PendingOrder managedPending =
                        GetManagedPendingOrder();
        
                    if (managedPending != null)
                    {
                        string pendingType =
                            managedPending.OrderType ==
                                PendingOrderType.Stop
                                ? "STOP"
                                : managedPending.OrderType ==
                                  PendingOrderType.Limit
                                    ? "LIMIT"
                                    : "PENDING";
        
                        AddPanelRow(
                            ref slot,
                            "AUTO ORDER  •  " +
                            pendingType +
                            "  •  ENTRY " +
                            Price(
                                managedPending.TargetPrice) +
                            (managedPending.StopLoss.HasValue
                                ? "  •  SL " +
                                  Price(
                                      managedPending.StopLoss.Value)
                                : "") +
                            (managedPending.TakeProfit.HasValue
                                ? "  •  TP " +
                                  Price(
                                      managedPending.TakeProfit.Value)
                                : ""),
                            TriggerLineColor,
                            true,
                            contentWidth);
                    }
        
                    AddPanelRow(
                        ref slot,
                        "AUTO CONFIG  •  " +
                        (ConfirmedSignalsOnly
                            ? "CONFIRMED ONLY"
                            : "PLAN ELIGIBLE") +
                        "  •  " +
                        (SizingMode ==
                            SizingMode.FixedLots
                            ? "FIXED " +
                              FixedLots.ToString("F2") +
                              " LOT"
                            : "RISK " +
                              RiskPercentEquity.ToString("F2") +
                              "%"),
                        PanelSecondaryTextColor,
                        false,
                        contentWidth);
        
                    if (AutoTradingEnabled &&
                        (AutoBrokerProtection ||
                         AutoProtectBrokerPositions))
                    {
                        AddPanelRow(
                            ref slot,
                            "AUTO PROTECTION  •  " +
                            GetAutoProtectionPanelState(),
                            TpLineColor,
                            false,
                            contentWidth);
                    }
        
                    if (!string.IsNullOrWhiteSpace(
                            _lastAlertMessage))
                    {
                        AddPanelRow(
                            ref slot,
                            "LAST ALERT  •  " +
                            CompactText(
                                _lastAlertMessage,
                                120),
                            _lastAlertCritical
                                ? (_lastAlertDirection == 1
                                    ? BuyArrowColor
                                    : _lastAlertDirection == -1
                                        ? SellArrowColor
                                        : PanelWarningColor)
                                : PanelSecondaryTextColor,
                            false,
                            contentWidth);
                    }
        
                    while (slot < _panelRows.Count)
                    {
                        _panelRows[slot].IsVisible =
                            false;
                        slot++;
                    }
                }
        
                private void SetPanelRow(
                    int index,
                    string text,
                    Color color,
                    bool bold,
                    int width)
                {
                    if (index < 0 ||
                        index >= _panelRows.Count)
                        return;
        
                    TextBlock row =
                        _panelRows[index];
        
                    row.Text =
                        text ?? "";
        
                    row.Width =
                        Math.Max(
                            190,
                            width);
        
                    row.ForegroundColor =
                        color;
        
                    row.TextAlignment =
                        TextAlignment.Left;
        
                    row.TextWrapping =
                        TextWrapping.Wrap;
        
                    row.TextTrimming =
                        TextTrimming.None;
        
                    row.LineHeight =
                        Math.Max(
                            14,
                            PanelFontSize + 3);
        
                    row.FontWeight =
                        bold || PanelBold
                            ? FontWeight.Bold
                            : FontWeight.Normal;
        
                    row.IsVisible =
                        !string.IsNullOrWhiteSpace(
                            text);
                }
        
                private void AddPanelRow(
                    ref int slot,
                    string text,
                    Color color,
                    bool bold,
                    int width)
                {
                    if (slot >= _panelRows.Count)
                        return;
        
                    SetPanelRow(
                        slot,
                        text,
                        color,
                        bold,
                        width);
        
                    slot++;
                }
        
                private Color PanelDirectionColor(
                    int direction)
                {
                    if (direction == 1)
                        return BuyArrowColor;
        
                    if (direction == -1)
                        return SellArrowColor;
        
                    return PanelTextColor;
                }
        
                private int GetAuthoritativeDirection()
                {
                    if (!UseAuthoritativeSignalState)
                        return _decision != null
                            ? _decision.Direction
                            : 0;
        
                    if (_plan != null &&
                        _plan.IsLivePosition &&
                        (_plan.Direction == 1 ||
                         _plan.Direction == -1))
                        return _plan.Direction;
        
                    if (_reaction != null &&
                        _reaction.EntryAllowed &&
                        _reaction.Confidence >=
                        Math.Max(
                            50,
                            LiveReactionThreshold) &&
                        (_reaction.Direction == 1 ||
                         _reaction.Direction == -1))
                        return _reaction.Direction;
        
                    if (_m5Frame != null &&
                        (_m5Frame.Direction == 1 ||
                         _m5Frame.Direction == -1) &&
                        _m5Frame.Quality >=
                        Math.Max(
                            50,
                            LiveReactionThreshold) &&
                        ((_m5Frame.Direction == 1 &&
                          (_m5Frame.MssBull ||
                           _m5Frame.ChochBull)) ||
                         (_m5Frame.Direction == -1 &&
                          (_m5Frame.MssBear ||
                           _m5Frame.ChochBear))))
                        return _m5Frame.Direction;
        
                    if (_decision != null &&
                        (_decision.Direction == 1 ||
                         _decision.Direction == -1))
                        return _decision.Direction;
        
                    if (_prediction != null &&
                        _prediction.Direction != 0 &&
                        _prediction.Confidence >=
                        Math.Max(
                            MinimumEarlyConfidence,
                            EarlySetupConfidence))
                        return _prediction.Direction;
        
                    return 0;
                }
        
                private string GetAuthoritativeState(
                    int direction)
                {
                    if (_plan != null &&
                        _plan.IsLivePosition)
                        return direction == 1
                            ? "BUY ACTIVE"
                            : direction == -1
                                ? "SELL ACTIVE"
                                : "ACTIVE";
        
                    if (_reaction != null &&
                        _reaction.EntryAllowed &&
                        _reaction.Confidence >=
                        Math.Max(
                            LiveReactionThreshold,
                            LiveReactionStrongThreshold))
                        return direction == 1
                            ? "BUY REACTION"
                            : direction == -1
                                ? "SELL REACTION"
                                : "REACTION";
        
                    if (_decision != null &&
                        _decision.EntryAllowed)
                        return direction == 1
                            ? "BUY READY"
                            : direction == -1
                                ? "SELL READY"
                                : "READY";
        
                    if (_prediction != null &&
                        _prediction.Direction != 0)
                        return direction == 1
                            ? "BUY PREDICTION"
                            : direction == -1
                                ? "SELL PREDICTION"
                                : "PREDICTION";
        
                    return direction == 1
                        ? "BUY WATCH"
                        : direction == -1
                            ? "SELL WATCH"
                            : "WAITING";
                }
        
                private string GetSignalSynchronizationText()
                {
                    int direction =
                        GetAuthoritativeDirection();
        
                    bool planAligned =
                        _plan == null ||
                        direction == 0 ||
                        _plan.Direction == direction;
        
                    bool decisionAligned =
                        _decision == null ||
                        _decision.Direction == 0 ||
                        direction == 0 ||
                        _decision.Direction == direction;
        
                    bool reactionAligned =
                        _reaction == null ||
                        _reaction.Direction == 0 ||
                        direction == 0 ||
                        !_reaction.EntryAllowed ||
                        _reaction.Direction == direction;
        
                    bool aligned =
                        planAligned &&
                        decisionAligned &&
                        reactionAligned;
        
                    string visualState =
                        _plan != null
                            ? "PLAN+ARROW+LEVELS"
                            : _decision != null &&
                              _decision.EntryAllowed
                                ? "DECISION+ARROW"
                                : _prediction != null &&
                                  _prediction.Direction != 0
                                    ? "PREDICTION"
                                    : "WAIT";
        
                    return
                        "STATE " +
                        (direction == 1
                            ? "BUY"
                            : direction == -1
                                ? "SELL"
                                : "WAIT") +
                        " | " +
                        (aligned
                            ? "ALIGNED"
                            : "CHECK") +
                        " | " +
                        visualState;
                }
        
                private Color GetSignalSynchronizationColor()
                {
                    string text =
                        GetSignalSynchronizationText();
        
                    return text.IndexOf(
                               "ALIGNED",
                               StringComparison.OrdinalIgnoreCase) >= 0
                        ? TpLineColor
                        : PanelWarningColor;
                }
        
                private int FrameDirection(
                    Frame frame)
                {
                    return frame == null
                        ? 0
                        : frame.Direction;
                }
        
                private void SetPanelAlignment()
                {
                    VerticalAlignment vertical;
                    HorizontalAlignment horizontal;
        
                    switch (PanelPosition)
                    {
                        case PanelCorner.TopLeft:
                            vertical = VerticalAlignment.Top;
                            horizontal = HorizontalAlignment.Left;
                            break;
        
                        case PanelCorner.TopRight:
                            vertical = VerticalAlignment.Top;
                            horizontal = HorizontalAlignment.Right;
                            break;
        
                        case PanelCorner.BottomRight:
                            vertical = VerticalAlignment.Bottom;
                            horizontal = HorizontalAlignment.Right;
                            break;
        
                        default:
                            vertical = VerticalAlignment.Bottom;
                            horizontal = HorizontalAlignment.Left;
                            break;
                    }
        
                    if (_panel != null)
                    {
                        _panel.VerticalAlignment =
                            vertical;
        
                        _panel.HorizontalAlignment =
                            horizontal;
                    }
                }
        
                private string GetStablePanelState(
                    string candidate)
                {
                    if (string.IsNullOrWhiteSpace(
                            candidate))
                        candidate = "WAITING";
        
                    DateTime now =
                        TimeInUtc;
        
                    bool authoritative =
                        _plan != null ||
                        candidate.IndexOf(
                            "ACTIVE",
                            StringComparison.OrdinalIgnoreCase) >= 0;
        
                    if (authoritative ||
                        PanelStateHoldSeconds <= 0 ||
                        string.IsNullOrWhiteSpace(
                            _panelStableHeader))
                    {
                        _panelStableHeader =
                            candidate;
        
                        _panelStableHeaderSinceUtc =
                            now;
        
                        return _panelStableHeader;
                    }
        
                    double heldSeconds =
                        (now -
                         _panelStableHeaderSinceUtc)
                        .TotalSeconds;
        
                    if (!string.Equals(
                            _panelStableHeader,
                            candidate,
                            StringComparison.OrdinalIgnoreCase) &&
                        heldSeconds >=
                        Math.Max(
                            0,
                            PanelStateHoldSeconds))
                    {
                        _panelStableHeader =
                            candidate;
        
                        _panelStableHeaderSinceUtc =
                            now;
                    }
        
                    return _panelStableHeader;
                }
        
                private string ConfluenceText(
                    Frame frame)
                {
                    if (frame == null)
                        return "WAIT";
        
                    List<string> parts =
                        new List<string>();
        
                    if (UseVolumeExpansion)
                        parts.Add(
                            frame.VolumeBull
                                ? "VOL+"
                                : frame.VolumeBear
                                    ? "VOL-"
                                    : "VOL0");
        
                    if (UseMacdBias)
                        parts.Add(
                            frame.MacdBull
                                ? "MACD+"
                                : frame.MacdBear
                                    ? "MACD-"
                                    : "MACD0");
        
                    if (UseVwapBias)
                        parts.Add(
                            frame.VwapBull
                                ? "VWAP+"
                                : frame.VwapBear
                                    ? "VWAP-"
                                    : "VWAP0");
        
                    if (UseHealthyVolatility)
                        parts.Add(
                            frame.VolatilityBull
                                ? "ATR+"
                                : frame.VolatilityBear
                                    ? "ATR-"
                                    : "ATR0");
        
                    return parts.Count == 0
                        ? "OFF"
                        : string.Join(
                            " | ",
                            parts.ToArray());
                }
        
                private string FrameText(
                    Frame frame)
                {
                    if (frame == null)
                        return "WAIT";
        
                    return
                        (frame.Direction == 1
                            ? "BUY"
                            : frame.Direction == -1
                                ? "SELL"
                                : "NEUTRAL") +
                        " | Q" +
                        frame.Quality +
                        " | E" +
                        frame.Evidence;
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
                        catch
                        {
                        }
                    }
        
                    if (_panelRestoreButton != null)
                    {
                        try
                        {
                            Chart.RemoveControl(
                                _panelRestoreButton);
                        }
                        catch
                        {
                        }
                    }
        
                    _panel = null;
                    _panelStack = null;
                    _panelHeaderStack = null;
                    _panelHeaderTitle = null;
                    _panelRowsStack = null;
                    _panelScroll = null;
                    _quickExecutionStack = null;
                    _autoTradingQuickToggle = null;
                    _automaticOrdersQuickToggle = null;
                    _panelRows.Clear();
                    _buttonStack = null;
                    _closeButton = null;
                    _cancelButton = null;
                    _panelToggleButton = null;
                    _panelRestoreButton = null;
                    _panelHidden = false;
                    _lastReactionAlertBar = -1;
                    _panelStableHeader = "";
                    _panelStableHeaderSinceUtc =
                        DateTime.MinValue;
                }
    }
}
