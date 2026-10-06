using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderCanonicalMtfTrendArrows(
            SignalVisualSnapshot snapshot,
            int bar)
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

            int mtfDirection =
                snapshot.MtfTrendDirection;

            bool decisionOwnsDirection =
                snapshot.PlanActive ||
                snapshot.ActionableNow ||
                snapshot.DecisionEntryAllowed ||
                snapshot.DecisionDirection != 0 ||
                snapshot.PlanDirection != 0;

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

            if (decisionOwnsDirection &&
                mtfDirection != 0 &&
                mtfDirection != direction)
            {
                strength = Math.Min(3, strength);
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

            // Directional arrows are presentation-only and now live in one
            // fixed chart-control box. No directional glyph is drawn under
            // candles, so there is exactly one visible arrow owner.
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
                        Height = 28,
                        FontFamily = "Arial",
                        FontSize = 22,
                        FontWeight = FontWeight.Normal,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        ForegroundColor = PanelTextColor,
                        TextWrapping = TextWrapping.NoWrap,
                        Margin = new Thickness(1, 0, 1, 0)
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
                    Child = _signalArrowBoxStack
                };

            Chart.AddControl(_signalArrowBox);
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
