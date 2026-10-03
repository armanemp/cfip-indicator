using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int ResolveArrowStackDirection(
            SignalVisualSnapshot snapshot,
            int fallbackDirection)
        {
            if (snapshot == null)
                return fallbackDirection;

            if ((snapshot.MtfTrendDirection == 1 ||
                 snapshot.MtfTrendDirection == -1) &&
                snapshot.MtfTrendDirection == fallbackDirection)
                return snapshot.MtfTrendDirection;

            return fallbackDirection;
        }

        // Canonical owner for every trend-strength arrow state.
        // All watch/confirmed/strong surfaces delegate here.
        private void RenderMtfTrendStrengthArrowStack(
            SignalVisualSnapshot snapshot,
            int direction,
            int bar,
            double baseOffset)
        {
            RemoveMtfTrendStrengthArrowStack();

            if (!ShowSignalArrow ||
                snapshot == null ||
                Bars == null ||
                Bars.Count == 0 ||
                direction == 0 ||
                snapshot.MtfTrendStrengthLevel <= 0)
                return;

            int safeBar =
                Math.Max(
                    0,
                    Math.Min(
                        Bars.Count - 1,
                        bar));

            int level =
                Math.Max(
                    1,
                    Math.Min(
                        9,
                        snapshot.MtfTrendStrengthLevel));

            int count =
                ((level - 1) % 3) + 1;

            string state =
                level <= 3
                    ? "WATCH"
                    : level <= 6
                        ? "CONFIRMED"
                        : "STRONG";

            double atr =
                Atr(
                    Bars,
                    Math.Max(
                        1,
                        Math.Min(
                            Bars.Count - 1,
                            safeBar)));

            // The first arrow is clear of the candle; every additional arrow
            // gets a deterministic gap large enough to prevent glyph overlap.
            double firstClearance =
                Math.Max(
                    Symbol.PipSize * 2.5,
                    baseOffset);

            double spacing =
                Math.Max(
                    Symbol.PipSize * 2.5,
                    Math.Max(
                        atr * 0.08,
                        baseOffset * 0.50));

            ChartIconType type =
                direction == 1
                    ? ChartIconType.UpArrow
                    : ChartIconType.DownArrow;

            for (int i = 0;
                 i < count;
                 i++)
            {
                double distance =
                    firstClearance +
                    spacing * i;

                double price =
                    direction == 1
                        ? Bars.LowPrices[safeBar] - distance
                        : Bars.HighPrices[safeBar] + distance;

                DrawIcon(
                    P + "TREND_ARROW_" + (i + 1),
                    type,
                    safeBar,
                    price,
                    SignalArrowColorFor(
                        direction,
                        state));
            }
        }

        private void RemoveMtfTrendStrengthArrowStack()
        {
            Chart.RemoveObject(P + "TREND_ARROW_1");
            Chart.RemoveObject(P + "TREND_ARROW_2");
            Chart.RemoveObject(P + "TREND_ARROW_3");

            // Legacy namespaces are removed here as a migration safeguard so
            // an older terminal instance cannot leave duplicate arrows behind.
            Chart.RemoveObject(P + "MTF_ARROW_1");
            Chart.RemoveObject(P + "MTF_ARROW_2");
            Chart.RemoveObject(P + "MTF_ARROW_3");
            Chart.RemoveObject(P + "WATCH_ARROW");
            Chart.RemoveObject(P + "WATCH_ARROW_1");
            Chart.RemoveObject(P + "WATCH_ARROW_2");
            Chart.RemoveObject(P + "WATCH_ARROW_3");
            Chart.RemoveObject(P + "ARROW");
        }
    }
}