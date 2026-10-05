using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int PanelDataFlowHeight = 96;
        private const int PanelDataFlowTrackHeight = 8;
        private const int PanelDataFlowLookback = 48;

        private Border _panelDataFlowCard;
        private TextBlock _panelDataFlowTitle;
        private TextBlock _panelDataFlowMeta;
        private TextBlock _panelDataFlowBuyValue;
        private TextBlock _panelDataFlowSellValue;
        private TextBlock _panelDataFlowDeltaValue;
        private TextBlock _panelDataFlowLoadValue;
        private Border _panelDataFlowBuyTrack;
        private Border _panelDataFlowSellTrack;
        private Border _panelDataFlowBuyFill;
        private Border _panelDataFlowSellFill;

        private void CreatePanelDataFlowCard()
        {
            if (_panelDataFlowCard != null)
                return;

            StackPanel content =
                new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            _panelDataFlowTitle =
                new TextBlock
                {
                    Text = "LIVE DATA FLOW",
                    FontFamily = "Arial",
                    FontSize = Math.Max(9, PanelFontSize - 1),
                    FontWeight = FontWeight.Bold,
                    ForegroundColor = PanelTextColor,
                    TextAlignment = TextAlignment.Left,
                    Height = 17,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            _panelDataFlowMeta =
                new TextBlock
                {
                    Text = "M5 • EST. BUY / SELL PRESSURE • TICKS IN",
                    FontFamily = "Arial",
                    FontSize = 8,
                    FontWeight = FontWeight.Normal,
                    ForegroundColor = PanelMutedTextColor,
                    TextAlignment = TextAlignment.Left,
                    Height = 14,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            _panelDataFlowBuyValue =
                CreatePanelDataFlowValueText(PanelSecondaryTextColor);

            _panelDataFlowSellValue =
                CreatePanelDataFlowValueText(PanelSecondaryTextColor);

            _panelDataFlowDeltaValue =
                CreatePanelDataFlowValueText(PanelTextColor);

            _panelDataFlowLoadValue =
                CreatePanelDataFlowValueText(PanelAccentColor);

            _panelDataFlowBuyTrack =
                CreatePanelDataFlowTrack();

            _panelDataFlowSellTrack =
                CreatePanelDataFlowTrack();

            _panelDataFlowBuyFill =
                new Border
                {
                    Height = PanelDataFlowTrackHeight,
                    Width = 2,
                    BackgroundColor = Color.FromArgb(70, Color.Lime),
                    CornerRadius = 4
                };

            _panelDataFlowSellFill =
                new Border
                {
                    Height = PanelDataFlowTrackHeight,
                    Width = 2,
                    BackgroundColor = Color.FromArgb(70, Color.Red),
                    CornerRadius = 4
                };

            _panelDataFlowBuyTrack.Child = _panelDataFlowBuyFill;
            _panelDataFlowSellTrack.Child = _panelDataFlowSellFill;

            content.AddChild(_panelDataFlowTitle);
            content.AddChild(_panelDataFlowMeta);
            content.AddChild(
                CreatePanelDataFlowMetricRow(
                    "BUY",
                    _panelDataFlowBuyValue,
                    _panelDataFlowBuyTrack));
            content.AddChild(
                CreatePanelDataFlowMetricRow(
                    "SELL",
                    _panelDataFlowSellValue,
                    _panelDataFlowSellTrack));
            content.AddChild(
                CreatePanelDataFlowSummaryRow());

            _panelDataFlowCard =
                new Border
                {
                    Height = PanelDataFlowHeight,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 3, 0, 5),
                    Padding = new Thickness(8, 6, 8, 5),
                    BackgroundColor =
                        Color.FromArgb(
                            Math.Min(
                                220,
                                Math.Max(
                                    40,
                                    PanelBackgroundAlpha + 25)),
                            PanelBackground),
                    BorderColor =
                        Color.FromArgb(
                            Math.Min(
                                255,
                                Math.Max(
                                    80,
                                    PanelBorderAlpha)),
                            PanelBorder),
                    BorderThickness = 1,
                    CornerRadius = Math.Max(4, PanelCornerRadius),
                    Child = content
                };

            _panelRowsStack.AddChild(_panelDataFlowCard);
            UpdatePanelDataFlowCard();
        }

        private TextBlock CreatePanelDataFlowValueText(Color color)
        {
            return
                new TextBlock
                {
                    Text = "—",
                    Width = 78,
                    FontFamily = "Arial",
                    FontSize = 8,
                    FontWeight = FontWeight.Bold,
                    ForegroundColor = color,
                    TextAlignment = TextAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };
        }

        private Border CreatePanelDataFlowTrack()
        {
            return
                new Border
                {
                    Height = PanelDataFlowTrackHeight,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    BackgroundColor = Color.FromArgb(42, PanelSecondaryTextColor),
                    CornerRadius = 4
                };
        }

        private StackPanel CreatePanelDataFlowMetricRow(
            string label,
            TextBlock value,
            Border track)
        {
            StackPanel row =
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            TextBlock name =
                new TextBlock
                {
                    Text = label,
                    Width = 38,
                    Height = 18,
                    FontFamily = "Arial",
                    FontSize = 8,
                    FontWeight = FontWeight.Bold,
                    ForegroundColor =
                        string.Equals(
                            label,
                            "BUY",
                            StringComparison.Ordinal)
                            ? Color.Lime
                            : Color.Red,
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            row.AddChild(name);
            row.AddChild(track);
            row.AddChild(value);

            return row;
        }

        private StackPanel CreatePanelDataFlowSummaryRow()
        {
            StackPanel row =
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Height = 18,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            TextBlock deltaLabel =
                new TextBlock
                {
                    Text = "DELTA",
                    Width = 38,
                    FontFamily = "Arial",
                    FontSize = 8,
                    FontWeight = FontWeight.Bold,
                    ForegroundColor = PanelMutedTextColor,
                    VerticalAlignment = VerticalAlignment.Center,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            TextBlock loadLabel =
                new TextBlock
                {
                    Text = "DATA LOAD",
                    Width = 58,
                    FontFamily = "Arial",
                    FontSize = 8,
                    FontWeight = FontWeight.Bold,
                    ForegroundColor = PanelMutedTextColor,
                    VerticalAlignment = VerticalAlignment.Center,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            row.AddChild(deltaLabel);
            row.AddChild(_panelDataFlowDeltaValue);
            row.AddChild(loadLabel);
            row.AddChild(_panelDataFlowLoadValue);

            return row;
        }

        private void UpdatePanelDataFlowCard()
        {
            if (_panelDataFlowCard == null ||
                _m5Bars == null)
                return;

            try
            {
                int m5Index = _m5Bars.Count - 1;
                if (m5Index < 0)
                    return;

                double m5Ticks = SafeTickVolume(_m5Bars, m5Index);
                double m5Max = RollingTickVolumeMaximum(
                    _m5Bars,
                    m5Index,
                    PanelDataFlowLookback);

                double buyRatio = EstimateBuyPressure(
                    _m5Bars,
                    m5Index);

                double sellRatio = 1.0 - buyRatio;
                double deltaRatio = buyRatio - sellRatio;
                double loadRatio =
                    m5Max > 0
                        ? Clamp01(m5Ticks / m5Max)
                        : 0;

                double buyTicks = m5Ticks * buyRatio;
                double sellTicks = m5Ticks * sellRatio;

                _panelDataFlowTitle.Text =
                    "LIVE DATA FLOW  •  M5";

                _panelDataFlowMeta.Text =
                    "EST. BUY / SELL PRESSURE  •  " +
                    FormatCompactVolume(m5Ticks) +
                    " TICKS IN";

                _panelDataFlowBuyValue.Text =
                    FormatCompactVolume(buyTicks) +
                    "  " +
                    (buyRatio * 100).ToString(
                        "F0",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "%";

                _panelDataFlowSellValue.Text =
                    FormatCompactVolume(sellTicks) +
                    "  " +
                    (sellRatio * 100).ToString(
                        "F0",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "%";

                _panelDataFlowDeltaValue.Text =
                    (deltaRatio >= 0 ? "+" : "") +
                    (deltaRatio * 100).ToString(
                        "F0",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "%";

                _panelDataFlowLoadValue.Text =
                    (loadRatio * 100).ToString(
                        "F0",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "%";

                int trackWidth =
                    Math.Max(
                        120,
                        EffectivePanelContentWidth() - 120);

                _panelDataFlowBuyTrack.Width = trackWidth;
                _panelDataFlowSellTrack.Width = trackWidth;

                _panelDataFlowBuyFill.Width =
                    Math.Max(
                        3,
                        trackWidth * buyRatio);

                _panelDataFlowSellFill.Width =
                    Math.Max(
                        3,
                        trackWidth * sellRatio);

                _panelDataFlowBuyFill.BackgroundColor =
                    Color.FromArgb(
                        FlowAlpha(buyRatio),
                        Color.Lime);

                _panelDataFlowSellFill.BackgroundColor =
                    Color.FromArgb(
                        FlowAlpha(sellRatio),
                        Color.Red);

                _panelDataFlowDeltaValue.ForegroundColor =
                    deltaRatio >= 0
                        ? Color.Lime
                        : Color.Red;

                _panelDataFlowLoadValue.ForegroundColor =
                    ResolveFlowLoadColor(loadRatio);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP panel data-flow refresh failed: {0}",
                    ex.Message);
            }
        }

        private double EstimateBuyPressure(
            Bars bars,
            int index)
        {
            double high = bars.HighPrices[index];
            double low = bars.LowPrices[index];
            double close = bars.ClosePrices[index];

            double range = high - low;
            if (!IsFinitePositive(range))
                return 0.5;

            return Clamp01(
                (close - low) / range);
        }

        private double RollingTickVolumeMaximum(
            Bars bars,
            int index,
            int lookback)
        {
            int start =
                Math.Max(
                    0,
                    index - Math.Max(1, lookback) + 1);

            double maximum = 0;
            for (int i = start; i <= index; i++)
                maximum = Math.Max(
                    maximum,
                    SafeTickVolume(bars, i));

            return maximum;
        }

        private double SafeTickVolume(
            Bars bars,
            int index)
        {
            if (bars == null ||
                index < 0 ||
                index >= bars.Count)
                return 0;

            double value = bars.TickVolumes[index];
            return double.IsNaN(value) ||
                   double.IsInfinity(value) ||
                   value < 0
                ? 0
                : value;
        }

        private double Clamp01(double value)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
                return 0;

            return Math.Max(
                0,
                Math.Min(
                    1,
                    value));
        }

        private int FlowAlpha(double ratio)
        {
            return
                65 +
                (int)Math.Round(
                    Clamp01(ratio) * 190);
        }

        private Color ResolveFlowLoadColor(double ratio)
        {
            if (ratio >= 0.80)
                return Color.Lime;

            if (ratio >= 0.50)
                return Color.Gold;

            return PanelMutedTextColor;
        }

        private string FormatCompactVolume(double value)
        {
            if (value >= 1000000)
                return (value / 1000000.0).ToString(
                    "0.0",
                    System.Globalization.CultureInfo.InvariantCulture) + "M";

            if (value >= 1000)
                return (value / 1000.0).ToString(
                    "0.0",
                    System.Globalization.CultureInfo.InvariantCulture) + "K";

            return value.ToString(
                "0",
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private void RemovePanelDataFlowCard()
        {
            _panelDataFlowCard = null;
            _panelDataFlowTitle = null;
            _panelDataFlowMeta = null;
            _panelDataFlowBuyValue = null;
            _panelDataFlowSellValue = null;
            _panelDataFlowDeltaValue = null;
            _panelDataFlowLoadValue = null;
            _panelDataFlowBuyTrack = null;
            _panelDataFlowSellTrack = null;
            _panelDataFlowBuyFill = null;
            _panelDataFlowSellFill = null;
        }
    }
}
