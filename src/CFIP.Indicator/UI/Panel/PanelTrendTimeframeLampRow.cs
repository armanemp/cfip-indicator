using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int PanelTrendTimeframeLampRowHeight = 54;
        private const int PanelTrendTimeframeLampTopSpacing = 3;
        private const int PanelTrendTimeframeLampBottomSpacing = 2;
        private const int PanelTrendTimeframeLampIndicatorHeight = 26;
        private const int PanelTrendTimeframeLampLabelHeight = 17;

        private StackPanel _panelTrendTimeframeLampRow;
        private readonly List<StackPanel> _panelTrendTimeframeLampCells =
            new List<StackPanel>(8);
        private readonly List<TextBlock> _panelTrendTimeframeLampIndicators =
            new List<TextBlock>(8);
        private readonly List<TextBlock> _panelTrendTimeframeLampLabels =
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
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = PanelTrendTimeframeLampRowHeight,
                    Margin =
                        new Thickness(
                            0,
                            PanelTrendTimeframeLampTopSpacing,
                            0,
                            PanelTrendTimeframeLampBottomSpacing),
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            string[] labels =
            {
                "M1", "M5", "M15", "M30",
                "H1", "H4", "D1", "W1"
            };

            for (int i = 0; i < labels.Length; i++)
            {
                StackPanel cell =
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Width = 42,
                        Height = PanelTrendTimeframeLampRowHeight,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        BackgroundColor = Color.FromArgb(0, Color.Black)
                    };

                TextBlock indicator =
                    new TextBlock
                    {
                        Text = "●",
                        Width = 42,
                        Height = PanelTrendTimeframeLampIndicatorHeight,
                        FontFamily = "Arial",
                        FontSize = PanelStatusLampFontSize - 2,
                        FontWeight = FontWeight.Bold,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        ForegroundColor = PanelSecondaryTextColor,
                        BackgroundColor = Color.FromArgb(0, Color.Black)
                    };

                TextBlock label =
                    new TextBlock
                    {
                        Text = labels[i],
                        Width = 42,
                        Height = PanelTrendTimeframeLampLabelHeight,
                        FontFamily = "Arial",
                        FontSize = 10,
                        FontWeight = FontWeight.Bold,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        ForegroundColor = PanelSecondaryTextColor,
                        BackgroundColor = Color.FromArgb(0, Color.Black)
                    };

                cell.AddChild(indicator);
                cell.AddChild(label);

                _panelTrendTimeframeLampCells.Add(cell);
                _panelTrendTimeframeLampIndicators.Add(indicator);
                _panelTrendTimeframeLampLabels.Add(label);
                _panelTrendTimeframeLampRow.AddChild(cell);
            }

            UpdatePanelTrendTimeframeLamps();
        }

        private void UpdatePanelTrendTimeframeLamps()
        {
            if (_panelTrendTimeframeLampIndicators == null ||
                _panelTrendTimeframeLampLabels == null ||
                _panelTrendTimeframeLampIndicators.Count < 8 ||
                _panelTrendTimeframeLampLabels.Count < 8)
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
                    : Math.Max(1, _panelTrendTimeframeLampRow.Width);

            int cellCount =
                Math.Max(
                    1,
                    _panelTrendTimeframeLampCells.Count);

            double cellWidth =
                Math.Max(
                    1,
                    rowWidth / cellCount);

            for (int i = 0;
                 i < _panelTrendTimeframeLampIndicators.Count &&
                 i < _panelTrendTimeframeLampLabels.Count;
                 i++)
            {
                TextBlock indicator =
                    _panelTrendTimeframeLampIndicators[i];

                TextBlock label =
                    _panelTrendTimeframeLampLabels[i];

                StackPanel cell =
                    _panelTrendTimeframeLampCells[i];

                cell.Width = cellWidth;
                indicator.Width = cellWidth;
                label.Width = cellWidth;

                Frame frame =
                    i < frames.Length
                        ? frames[i]
                        : null;

                int direction =
                    frame == null
                        ? 0
                        : FrameDirection(frame);

                int strength =
                    ResolveFrameTrendStrength(
                        frame,
                        direction);

                indicator.Text = "●";
                indicator.FontSize =
                    strength >= 3
                        ? PanelStatusLampFontSize
                        : strength == 2
                            ? PanelStatusLampFontSize - 1
                            : PanelStatusLampFontSize - 2;

                label.Text = labels[i];

                Color lampColor =
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

                indicator.ForegroundColor = lampColor;
                label.ForegroundColor = lampColor;
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
            _panelTrendTimeframeLampIndicators.Clear();
            _panelTrendTimeframeLampLabels.Clear();
            _panelTrendTimeframeLampRow = null;
        }
    }
}