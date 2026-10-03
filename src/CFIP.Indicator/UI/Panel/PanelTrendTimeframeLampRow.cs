using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int PanelTrendTimeframeLampRowHeight = 24;

        private StackPanel _panelTrendTimeframeLampRow;
        private readonly List<TextBlock> _panelTrendTimeframeLampCells =
            new List<TextBlock>(8);

        private void CreatePanelTrendTimeframeLampRow()
        {
            if (_panelTrendTimeframeLampRow != null)
                return;

            _panelTrendTimeframeLampRow =
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Height = PanelTrendTimeframeLampRowHeight,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            string[] labels =
            {
                "M1", "M5", "M15", "M30",
                "H1", "H4", "D1", "W1"
            };

            for (int i = 0; i < labels.Length; i++)
            {
                TextBlock cell =
                    new TextBlock
                    {
                        Text = "● " + labels[i],
                        Width = 42,
                        Height = PanelTrendTimeframeLampRowHeight,
                        FontFamily = "Arial",
                        FontSize = 9,
                        FontWeight = FontWeight.Bold,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        ForegroundColor = PanelSecondaryTextColor,
                        BackgroundColor = Color.FromArgb(0, Color.Black)
                    };

                _panelTrendTimeframeLampCells.Add(cell);
                _panelTrendTimeframeLampRow.AddChild(cell);
            }

            UpdatePanelTrendTimeframeLamps();
        }

        private void UpdatePanelTrendTimeframeLamps()
        {
            if (_panelTrendTimeframeLampCells == null ||
                _panelTrendTimeframeLampCells.Count < 8)
                return;

            Frame[] frames =
            {
                _m1Frame, _m5Frame, _m15Frame, _m30Frame,
                _h1Frame, _h4Frame, _d1Frame, _w1Frame
            };

            string[] labels =
            {
                "M1", "M5", "M15", "M30",
                "H1", "H4", "D1", "W1"
            };

            double rowWidth =
                _panelTrendTimeframeLampRow == null
                    ? 336
                    : Math.Max(224, _panelTrendTimeframeLampRow.Width);

            double cellWidth =
                Math.Max(
                    28,
                    rowWidth /
                    Math.Max(1, _panelTrendTimeframeLampCells.Count));

            for (int i = 0; i < _panelTrendTimeframeLampCells.Count; i++)
            {
                TextBlock cell = _panelTrendTimeframeLampCells[i];
                cell.Width = cellWidth;
                Frame frame = i < frames.Length ? frames[i] : null;

                int direction =
                    frame == null
                        ? 0
                        : FrameDirection(frame);

                int strength =
                    ResolveFrameTrendStrength(frame, direction);

                cell.Text =
                    "● " +
                    labels[i] +
                    (strength >= 3 ? " ▲" : strength == 2 ? " •" : "");

                cell.ForegroundColor =
                    direction == 1
                        ? (strength >= 3
                            ? StrongBuyArrowColor
                            : strength == 2
                                ? ConfirmedBuyArrowColor
                                : CautionBuyArrowColor)
                        : direction == -1
                            ? (strength >= 3
                                ? StrongSellArrowColor
                                : strength == 2
                                    ? ConfirmedSellArrowColor
                                    : CautionSellArrowColor)
                            : PanelSecondaryTextColor;
            }
        }

        private int ResolveFrameTrendStrength(
            Frame frame,
            int direction)
        {
            if (frame == null ||
                direction == 0 ||
                !frame.NativeIndicatorsReady)
                return 0;

            int score =
                direction == 1
                    ? frame.BullScore
                    : frame.BearScore;

            double adx =
                double.IsNaN(frame.Adx) ||
                double.IsInfinity(frame.Adx)
                    ? 0
                    : frame.Adx;

            if (score >= 70 || adx >= 25)
                return 3;

            if (score >= 55 || adx >= 20)
                return 2;

            return 1;
        }

        private void RemovePanelTrendTimeframeLampRow()
        {
            _panelTrendTimeframeLampCells.Clear();
            _panelTrendTimeframeLampRow = null;
        }
    }
}
