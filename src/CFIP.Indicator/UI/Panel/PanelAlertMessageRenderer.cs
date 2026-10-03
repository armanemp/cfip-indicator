using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int PanelAlertMessageCapacity = 5;
        private const int PanelAlertMessageVisibleCapacity = 2;
        private const int PanelAlertMessageRowHeight = 18;
        private const int PanelAlertMessageGap = 1;
        private const int PanelAlertMessageMaxCharacters = 132;

        private void CreatePanelAlertMessageRail()
        {
            if (_panelAlertMessageStack != null)
                return;

            _lastRenderedPanelAlertRevision = -1;

            try
                _panelAlertMessageStack =
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        BackgroundColor = Color.FromArgb(0, Color.Black)
                    };

                for (int i = 0; i < PanelAlertMessageVisibleCapacity; i++)
                {
                    TextBlock row =
                        new TextBlock
                        {
                            Text = string.Empty,
                            Width = 0,
                            Height = PanelAlertMessageRowHeight,
                            FontFamily =
                                string.IsNullOrWhiteSpace(PanelFontFamily)
                                    ? "Arial"
                                    : PanelFontFamily,
                            FontSize = Math.Max(10, PanelFontSize - 1),
                            FontWeight = FontWeight.Bold,
                            ForegroundColor = PanelMutedTextColor,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Left,
                            TextWrapping = TextWrapping.NoWrap,
                            TextTrimming = TextTrimming.None,
                            LineHeight =
                                Math.Max(
                                    12,
                                    Math.Min(
                                        16,
                                        PanelFontSize)),
                            Margin =
                                new Thickness(
                                    0,
                                    0,
                                    0,
                                    PanelAlertMessageGap)
                        };

                    row.IsVisible = false;
                    _panelAlertMessageRows.Add(row);
                    _panelAlertMessageStack.AddChild(row);
                }
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP panel alert rail creation failed: {0}",
                    ex.Message);

                _panelAlertMessageStack = null;
                _panelAlertMessageRows.Clear();
            }
        }

        private void RecordPanelAlertDelivery(
            AlertDelivery delivery)
        {
            if (string.IsNullOrWhiteSpace(delivery.Message))
                return;

            try
            {
                if (_panelAlertHistory == null)
                    _panelAlertHistory =
                        new Queue<AlertDelivery>(
                            PanelAlertMessageCapacity);

                _panelAlertHistory.Enqueue(delivery);
                _panelAlertRevision++;

                while (_panelAlertHistory.Count >
                       PanelAlertMessageCapacity)
                {
                    _panelAlertHistory.Dequeue();
                }

                UpdatePanelAlertMessageRail();
                RefreshPanelAlertFooterGeometry();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP panel alert rail update failed: {0}",
                    ex.Message);
            }
        }

        private void RefreshPanelAlertFooterGeometry()
        {
            if (_panel == null ||
                _panelStack == null ||
                _panelScroll == null ||
                _buttonStack == null ||
                _panelAlertMessageStack == null)
                return;

            try
            {
                int padding = Math.Max(0, PanelPadding);
                int border = Math.Max(0, PanelBorderThickness);
                int contentWidth = EffectivePanelContentWidth();

                int buttonHeight = Math.Max(
                    22,
                    Math.Min(40, PanelToggleHeight));

                int toggleHeight = Math.Max(
                    22,
                    Math.Min(40, PanelToggleHeight));

                int toggleWidth = Math.Max(
                    22,
                    Math.Min(40, PanelToggleWidth));

                int buttonGap = Math.Max(0, PanelButtonGap);
                int alertRailHeight = GetPanelAlertMessageRailHeight();

                int footerAreaHeight = ResolvePanelFooterAreaHeight(
                    buttonHeight,
                    toggleHeight,
                    alertRailHeight);

                int configuredMaxHeight = Math.Max(
                    260,
                    Math.Min(1200, PanelMaxHeight));

                int maxHeight = ResolvePanelMaximumHeight(
                    configuredMaxHeight);

                int fixedHeight =
                    PanelHeaderHeight +
                    PanelTrendTimeframeLampRowHeight +
                    PanelTrendTimeframeLampTopSpacing +
                    PanelTrendTimeframeLampBottomSpacing +
                    footerAreaHeight +
                    padding * 2 +
                    border * 2;

                int currentScrollHeight = Math.Max(
                    120,
                    (int)Math.Round(_panelScroll.Height));

                int maximumScrollHeight = Math.Max(
                    120,
                    maxHeight - fixedHeight);

                int scrollHeight = Math.Min(
                    currentScrollHeight,
                    maximumScrollHeight);

                int panelHeight = fixedHeight + scrollHeight;

                _panelScroll.Height = scrollHeight;
                _panelStack.Height = panelHeight;
                _panel.Height = panelHeight;
                _buttonStack.Height = footerAreaHeight;

                _buttonStack.IsVisible =
                    ShowPanelToggleButton ||
                    alertRailHeight > 0;

                ApplyPanelAlertMessageRailLayout(
                    contentWidth,
                    buttonGap,
                    toggleWidth);

                SetPanelAlignment();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP compact alert footer geometry refresh failed: {0}",
                    ex.Message);
            }
        }

        private void UpdatePanelAlertMessageRail()
        {
            if (_panelAlertMessageStack == null)
                return;

            if (_lastRenderedPanelAlertRevision ==
                _panelAlertRevision)
                return;

            try
                List<AlertDelivery> messages =
                    new List<AlertDelivery>(
                        _panelAlertHistory ??
                        new Queue<AlertDelivery>());

                int start =
                    Math.Max(
                        0,
                        messages.Count -
                        PanelAlertMessageVisibleCapacity);

                int rowIndex = 0;

                for (int i = start;
                     i < messages.Count &&
                     rowIndex < _panelAlertMessageRows.Count;
                     i++)
                {
                    AlertDelivery delivery =
                        messages[i];

                    TextBlock row =
                        _panelAlertMessageRows[rowIndex++];

                    row.Text =
                        FormatPanelAlertMessage(
                            delivery);

                    row.ForegroundColor =
                        ResolvePanelAlertMessageColor(
                            delivery);

                    row.FontFamily =
                        string.IsNullOrWhiteSpace(PanelFontFamily)
                            ? "Arial"
                            : PanelFontFamily;

                    row.FontSize =
                        Math.Max(
                            10,
                            PanelFontSize - 1);

                    row.FontWeight =
                        FontWeight.Bold;

                    row.IsVisible = true;
                }

                while (rowIndex < _panelAlertMessageRows.Count)
                {
                    TextBlock row =
                        _panelAlertMessageRows[rowIndex++];

                    row.Text = string.Empty;
                    row.IsVisible = false;
                }

                _panelAlertMessageStack.IsVisible =
                    ShowUnifiedPanel &&
                    !_panelHidden &&
                    _panelAlertHistory != null &&
                    _panelAlertHistory.Count > 0;

                _lastRenderedPanelAlertRevision =
                    _panelAlertRevision;
            }
            catch (Exception ex)
            {
                _lastRenderedPanelAlertRevision = -1;

                Print(
                    "CFIP panel alert rail render failed: {0}",
                    ex.Message);
            }
        }

        private int GetPanelAlertMessageRailHeight()
        {
            if (_panelAlertHistory == null ||
                _panelAlertHistory.Count == 0)
                return 0;

            int rows =
                Math.Min(
                    PanelAlertMessageVisibleCapacity,
                    _panelAlertHistory.Count);

            return
                rows * PanelAlertMessageRowHeight +
                Math.Max(
                    0,
                    rows - 1) *
                PanelAlertMessageGap;
        }

        private void ApplyPanelAlertMessageRailLayout(
            int contentWidth,
            int buttonGap,
            int toggleWidth)
        {
            if (_panelAlertMessageStack == null)
                return;

            int gap = Math.Max(2, buttonGap);
            int availableWidth =
                contentWidth -
                (ShowPanelToggleButton
                    ? toggleWidth + gap
                    : 0);

            _panelAlertMessageStack.Width =
                Math.Max(
                    120,
                    availableWidth);

            int railHeight =
                GetPanelAlertMessageRailHeight();

            _panelAlertMessageStack.Height =
                Math.Max(
                    PanelAlertMessageRowHeight,
                    railHeight);

            _panelAlertMessageStack.HorizontalAlignment =
                HorizontalAlignment.Left;
            _panelAlertMessageStack.VerticalAlignment =
                VerticalAlignment.Center;

            foreach (TextBlock row in
                     _panelAlertMessageRows)
            {
                row.Width =
                    Math.Max(
                        100,
                        availableWidth);
                row.Height =
                    PanelAlertMessageRowHeight;
                row.Margin =
                    new Thickness(
                        0,
                        0,
                        0,
                        PanelAlertMessageGap);
            }

            UpdatePanelAlertMessageRail();
        }

        private int ResolvePanelAlertMessageCharacterLimit()
        {
            int contentWidth =
                EffectivePanelContentWidth();

            int toggleWidth =
                Math.Max(
                    22,
                    Math.Min(
                        40,
                        PanelToggleWidth));

            int gap =
                Math.Max(
                    2,
                    PanelButtonGap);

            int availableWidth =
                contentWidth -
                (ShowPanelToggleButton
                    ? toggleWidth + gap
                    : 0);

            double averageCharacterWidth =
                Math.Max(
                    5.0,
                    Math.Max(
                        10,
                        PanelFontSize - 1) *
                    0.55);

            return Math.Max(
                24,
                Math.Min(
                    PanelAlertMessageMaxCharacters - 2,
                    (int)(
                        Math.Max(
                            120,
                            availableWidth) /
                        averageCharacterWidth)));
        }

        private string FormatPanelAlertMessage(
            AlertDelivery delivery)
        {
            string message =
                (delivery.Message ?? string.Empty).Trim();

            int maxCharacters =
                ResolvePanelAlertMessageCharacterLimit();

            if (message.Length >
                maxCharacters)
            {
                message =
                    message.Substring(
                        0,
                        Math.Max(
                            1,
                            maxCharacters - 1)) +
                    "…";
            }

            return "• " + message;
        }

        private Color ResolvePanelAlertMessageColor(
            AlertDelivery delivery)
        {
            if (delivery.Critical)
                return PanelWarningColor;

            string key =
                delivery.Key ?? string.Empty;

            if (key.StartsWith(
                    "SL|",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "INVALID",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "RESTRICT|",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "REVERSAL|",
                    StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(
                    "PROTECTION",
                    StringComparison.OrdinalIgnoreCase))
            {
                return PanelWarningColor;
            }

            if (delivery.Direction > 0)
                return BuyArrowColor;

            if (delivery.Direction < 0)
                return SellArrowColor;

            return PanelSecondaryTextColor;
        }
    }
}
