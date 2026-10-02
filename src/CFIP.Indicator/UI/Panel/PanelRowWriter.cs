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
        private void SetPanelRow(
            int index,
            string text,
            Color color,
            bool bold,
            int width)
        {
            if (index < 0 ||
                index >= PanelRowCount)
                return;

            EnsurePanelRow(index);

            if (index >= _panelRows.Count)
                return;

            TextBlock row =
                _panelRows[index];

            string nextText =
                text ?? "";

            int nextWidth =
                Math.Max(
                    190,
                    width);

            Color nextColor =
                color;

            FontWeight nextWeight =
                bold || PanelBold
                    ? FontWeight.Bold
                    : FontWeight.Normal;

            bool nextVisible =
                !string.IsNullOrWhiteSpace(
                    nextText);

            if (row.Text != nextText)
                row.Text = nextText;

            if (row.Width != nextWidth)
                row.Width = nextWidth;

            if (!Equals(row.ForegroundColor, nextColor))
                row.ForegroundColor = nextColor;

            if (row.TextAlignment != TextAlignment.Left)
                row.TextAlignment = TextAlignment.Left;

            if (row.TextWrapping != TextWrapping.Wrap)
                row.TextWrapping = TextWrapping.Wrap;

            if (row.TextTrimming != TextTrimming.None)
                row.TextTrimming = TextTrimming.None;

            Thickness margin =
                new Thickness(
                    0,
                    Math.Max(
                        0,
                        PanelRowPadding),
                    0,
                    Math.Max(
                        0,
                        PanelRowGap));

            if (row.Margin != margin)
                row.Margin = margin;

            int lineHeight =
                Math.Max(
                    14,
                    PanelFontSize + 3);

            if (row.LineHeight != lineHeight)
                row.LineHeight = lineHeight;

            if (row.FontWeight != nextWeight)
                row.FontWeight = nextWeight;

            if (row.IsVisible != nextVisible)
                row.IsVisible = nextVisible;
        }

        private void AddPanelRow(
            ref int slot,
            string text,
            Color color,
            bool bold,
            int width)
        {
            if (slot >= PanelRowCount)
                return;

            EnsurePanelRow(slot);

            SetPanelRow(
                slot,
                text,
                color,
                bold,
                width);

            slot++;
        }
    }
}
