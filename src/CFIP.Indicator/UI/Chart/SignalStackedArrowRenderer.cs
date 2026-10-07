using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Border _signalArrowBox;
        private StackPanel _signalArrowBoxStack;
        private readonly System.Collections.Generic.List<TextBlock> _signalArrowBoxArrows =
            new System.Collections.Generic.List<TextBlock>(3);

        private void RenderCanonicalMtfTrendArrows(
            SignalVisualSnapshot snapshot)
        {
            if (!ShowSignalArrow ||
                snapshot == null ||
                Bars == null ||
                Bars.Count < 2)
            {
                RemoveStackedSignalArrows();
                return;
            }

            RenderStackedSignalArrows(snapshot);
        }

        private void RenderStackedSignalArrows(
            SignalVisualSnapshot snapshot)
        {
            if (snapshot == null)
            {
                RemoveStackedSignalArrows();
                return;
            }

            int direction =
                snapshot.AuthoritativeDirection != 0
                    ? snapshot.AuthoritativeDirection
                    : snapshot.MtfTrendDirection;

            if (direction == 0)
            {
                RemoveStackedSignalArrows();
                return;
            }

            int strength =
                NumericGuards.ClampInt(
                    snapshot.MtfTrendStrengthLevel,
                    0,
                    9);

            if (strength <= 0)
            {
                RemoveStackedSignalArrows();
                return;
            }

            int arrowCount =
                ((strength - 1) % 3) + 1;

            Color arrowColor =
                SignalPresentationColorRule.Resolve(
                    direction,
                    strength,
                    StrongBuyArrowColor,
                    StrongSellArrowColor,
                    ConfirmedBuyArrowColor,
                    ConfirmedSellArrowColor,
                    CautionBuyArrowColor,
                    CautionSellArrowColor,
                    BlockedReactionArrowColor);

            // Direction is owned by the canonical signal snapshot so a
            // qualifying live M5 reaction can surface without waiting for a
            // new closed M5 bar. Strength remains owned by the canonical MTF
            // trend-strength ladder.
            UpdateSignalArrowBox(
                direction,
                arrowCount,
                arrowColor);
        }


        private void EnsureSignalArrowBox()
        {
            if (_signalArrowBox != null)
                return;

            _signalArrowBoxStack =
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            _signalArrowBoxArrows.Clear();

            for (int i = 0; i < 3; i++)
            {
                TextBlock arrow =
                    new TextBlock
                    {
                        Text = string.Empty,
                        Width = 18,
                        Height = 36,
                        FontFamily = "Segoe UI Symbol",
                        FontSize = 28,
                        FontWeight = FontWeight.ExtraBold,
                        LineHeight = 32,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        ForegroundColor = PanelTextColor,
                        TextWrapping = TextWrapping.NoWrap,
                        Margin = new Thickness(0, 0, 0, 0)
                    };

                _signalArrowBoxArrows.Add(arrow);
                _signalArrowBoxStack.AddChild(arrow);
            }

            _signalArrowBox =
                new Border
                {
                    Width = 66,
                    Height = 66,
                    Padding = 2,
                    BackgroundColor =
                        Color.FromArgb(
                            165,
                            PanelBackground),
                    BorderColor = PanelBorder,
                    BorderThickness = 1,
                    CornerRadius = Math.Max(3, PanelCornerRadius),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(
                        Math.Max(8, PanelMargin),
                        Math.Max(8, PanelMargin),
                        Math.Max(8, PanelMargin),
                        Math.Max(8, PanelMargin)),
                    IsHitTestVisible = false,
                    Child = _signalArrowBoxStack,
                    IsVisible = false
                };

            Chart.AddControl(_signalArrowBox);
        }

        private void BringSignalArrowBoxToFront()
        {
            if (_signalArrowBox == null)
                return;

            try
            {
                Chart.RemoveControl(_signalArrowBox);
                Chart.AddControl(_signalArrowBox);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP signal arrow overlay reorder failed: {0}",
                    ex.Message);
            }
        }

        private void UpdateSignalArrowBox(
            int direction,
            int arrowCount,
            Color arrowColor)
        {
            EnsureSignalArrowBox();

            string glyph =
                direction > 0
                    ? "↑"
                    : "↓";

            for (int i = 0; i < _signalArrowBoxArrows.Count; i++)
            {
                TextBlock arrow =
                    _signalArrowBoxArrows[i];

                bool visible = i < arrowCount;

                arrow.Text =
                    visible
                        ? glyph
                        : string.Empty;

                arrow.ForegroundColor =
                    arrowColor;

                arrow.IsVisible =
                    visible;
            }

            _signalArrowBox.IsVisible =
                ShowSignalArrow &&
                direction != 0 &&
                arrowCount > 0;
        }

        private void RemoveStackedSignalArrows()
        {
            Chart.RemoveObject(
                P + "WATCH_ARROW");
            Chart.RemoveObject(
                P + "WATCH_ARROW_2");
            Chart.RemoveObject(
                P + "WATCH_ARROW_3");
            Chart.RemoveObject(
                P + "ARROW");

            if (_signalArrowBox != null)
                _signalArrowBox.IsVisible = false;

            for (int i = 0; i < _signalArrowBoxArrows.Count; i++)
            {
                _signalArrowBoxArrows[i].Text = string.Empty;
                _signalArrowBoxArrows[i].IsVisible = false;
            }
        }
    }
}
