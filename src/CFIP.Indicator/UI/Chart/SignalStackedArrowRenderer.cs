using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const string CanonicalTrendArrowPrefix = "MTF_TREND_ARROW_";

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
            RemoveStackedSignalArrows();

            if (snapshot == null ||
                Bars == null ||
                Bars.Count < 2)
                return;

            int direction =
                snapshot.MtfTrendDirection != 0
                    ? snapshot.MtfTrendDirection
                    : snapshot.AuthoritativeDirection;

            if (direction == 0)
                return;

            int strength =
                NumericGuards.ClampInt(
                    snapshot.MtfTrendStrengthLevel,
                    0,
                    9);

            if (strength <= 0)
                return;

            int arrowCount =
                ((strength - 1) % 3) + 1;

            int barIndex =
                ResolveArrowBarIndex(snapshot);

            if (barIndex < 0 ||
                barIndex >= Bars.Count)
                return;

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

            double atr =
                ResolveArrowAtr();

            double spacing =
                Math.Max(
                    Symbol.PipSize * 2.0,
                    atr > 0
                        ? atr * 0.16
                        : Symbol.PipSize * 4.0);

            double basePrice =
                direction > 0
                    ? Bars.LowPrices[barIndex] - spacing
                    : Bars.HighPrices[barIndex] + spacing;

            ChartIconType iconType =
                direction > 0
                    ? ChartIconType.UpArrow
                    : ChartIconType.DownArrow;

            for (int level = 0;
                 level < arrowCount;
                 level++)
            {
                double price =
                    direction > 0
                        ? basePrice - spacing * level
                        : basePrice + spacing * level;

                DrawCanonicalTrendArrow(
                    CanonicalTrendArrowPrefix + (level + 1),
                    iconType,
                    barIndex,
                    price,
                    arrowColor);
            }
        }

        private int ResolveArrowBarIndex(
            SignalVisualSnapshot snapshot)
        {
            int preferredM5 =
                snapshot.ArrowM5Index;

            if (preferredM5 >= 0 &&
                _m5Bars != null &&
                preferredM5 < _m5Bars.Count)
            {
                int mapped =
                    MapM5ToChart(
                        preferredM5,
                        Bars.Count - 1);

                if (mapped >= 0 &&
                    mapped < Bars.Count)
                    return mapped;
            }

            return Math.Max(
                0,
                Bars.Count - 1);
        }

        private double ResolveArrowAtr()
        {
            if (_m5Frame != null &&
                NumericGuards.IsFinitePositive(
                    _m5Frame.Atr))
                return _m5Frame.Atr;

            if (Bars.Count >= 15)
            {
                int index =
                    Math.Max(
                        1,
                        Bars.Count - 2);

                double range =
                    Math.Abs(
                        Bars.HighPrices[index] -
                        Bars.LowPrices[index]);

                if (NumericGuards.IsFinitePositive(range))
                    return range;
            }

            return 0;
        }

        private void DrawCanonicalTrendArrow(
            string name,
            ChartIconType iconType,
            int barIndex,
            double price,
            Color color)
        {
            try
            {
                if (Bars == null ||
                    Bars.Count < 2 ||
                    barIndex < 0 ||
                    barIndex >= Bars.Count ||
                    !NumericGuards.IsFinitePositive(price))
                    return;

                ChartIcon icon =
                    Chart.DrawIcon(
                        P + name,
                        iconType,
                        barIndex,
                        price,
                        color);

                if (icon != null)
                    icon.IsInteractive = false;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP canonical trend arrow render failed: {0}",
                    ex.Message);
            }
        }

        private void RemoveStackedSignalArrows()
        {
            Chart.RemoveObject(
                P + CanonicalTrendArrowPrefix + "1");
            Chart.RemoveObject(
                P + CanonicalTrendArrowPrefix + "2");
            Chart.RemoveObject(
                P + CanonicalTrendArrowPrefix + "3");

            // Remove obsolete control-era objects as a one-time migration guard.
            Chart.RemoveObject(P + "WATCH_ARROW");
            Chart.RemoveObject(P + "WATCH_ARROW_2");
            Chart.RemoveObject(P + "WATCH_ARROW_3");
            Chart.RemoveObject(P + "ARROW");
        }
    }
}
