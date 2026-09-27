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
    }
}
