// ============================================================================
// CFIP Indicator — PanelLayoutManager.cs
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
        private int EffectivePanelWidth()
        {
            return PanelDimensionRule.EffectiveWidth(
                PanelWidth);
        }

        private int EffectivePanelContentWidth()
        {
            return PanelDimensionRule.EffectiveContentWidth(
                PanelWidth,
                PanelPadding,
                PanelBorderThickness);
        }

        private int ResolvePanelFooterContentHeight(
            int buttonHeight,
            int toggleHeight,
            int alertRailHeight)
        {
            if (!ShowPanelToggleButton &&
                alertRailHeight <= 0)
                return 0;

            return Math.Max(
                PanelFooterMinHeight,
                Math.Max(
                    buttonHeight,
                    Math.Max(
                        ShowPanelToggleButton
                            ? toggleHeight
                            : 0,
                        alertRailHeight)));
        }

        private int ResolvePanelFooterAreaHeight(
            int buttonHeight,
            int toggleHeight,
            int alertRailHeight)
        {
            int contentHeight =
                ResolvePanelFooterContentHeight(
                    buttonHeight,
                    toggleHeight,
                    alertRailHeight);

            if (contentHeight <= 0)
                return 0;

            // PanelPadding is already part of the outer panel geometry.
            // Do not charge it again to the footer or the lower panel grows
            // independently of its actual visible content.
            return contentHeight;
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

                                            bool bottomPosition =
                                                PanelPosition ==
                                                    PanelCorner.BottomLeft ||
                                                PanelPosition ==
                                                    PanelCorner.BottomRight;

                                            int bottomMargin =
                                                bottomPosition
                                                    ? Math.Max(
                                                        margin,
                                                        PanelRestoreBottomClearance)
                                                    : margin;

                                            _panelRestoreButton.Margin =
                                                new Thickness(
                                                    margin,
                                                    margin,
                                                    margin,
                                                    bottomMargin);
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
        
        private int ResolvePanelMaximumHeight(
            int configuredMaxHeight)
        {
            int minimumRenderableHeight =
                PanelHeaderHeight +
                PanelTrendTimeframeLampRowHeight +
                PanelTrendTimeframeLampTopSpacing +
                PanelTrendTimeframeLampBottomSpacing +
                PanelFooterMinHeight +
                120 +
                Math.Max(
                    0,
                    PanelPadding) * 2 +
                Math.Max(
                    0,
                    PanelBorderThickness) * 2;

            return Math.Max(
                260,
                Math.Min(
                    1200,
                    Math.Max(
                        configuredMaxHeight,
                        minimumRenderableHeight)));
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
    }
}