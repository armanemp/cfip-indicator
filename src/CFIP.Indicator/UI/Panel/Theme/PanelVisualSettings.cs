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
    }
}
