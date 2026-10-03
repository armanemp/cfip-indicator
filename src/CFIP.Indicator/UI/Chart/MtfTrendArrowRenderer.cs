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

            if ((snapshot.HtfTrendDirection == 1 ||
                 snapshot.HtfTrendDirection == -1) &&
                snapshot.HtfTrendDirection == fallbackDirection)
            {
                return snapshot.HtfTrendDirection;
            }

            if ((snapshot.MtfTrendDirection == 1 ||
                 snapshot.MtfTrendDirection == -1) &&
                snapshot.MtfTrendDirection == fallbackDirection)
            {
                return snapshot.MtfTrendDirection;
            }

            return fallbackDirection;
        }

        private void RenderMtfTrendStrengthArrowStack(
            SignalVisualSnapshot snapshot,
            int direction,
            int bar,
            double baseOffset)
        {
            RemoveMtfTrendStrengthArrowStack();

            if (!ShowSignalArrow ||
                snapshot == null ||
                direction == 0 ||
                snapshot.MtfTrendStrengthLevel <= 0)
                return;

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

            double spacing =
                Math.Max(
                    Symbol.PipSize * 0.5,
                    baseOffset *
                    Math.Max(
                        0.35,
                        0.09 /
                        Math.Max(
                            0.02,
                            ArrowOffsetAtr)));

            ChartIconType type =
                direction == 1
                    ? ChartIconType.UpArrow
                    : ChartIconType.DownArrow;

            for (int i = 0;
                 i < count;
                 i++)
            {
                double distance =
                    baseOffset +
                    spacing * i;

                double price =
                    direction == 1
                        ? Bars.LowPrices[bar] - distance
                        : Bars.HighPrices[bar] + distance;

                DrawIcon(
                    P + "MTF_ARROW_" + (i + 1),
                    type,
                    bar,
                    price,
                    SignalArrowColorFor(
                        direction,
                        state));
            }
        }

        private void RemoveMtfTrendStrengthArrowStack()
        {
            Chart.RemoveObject(P + "MTF_ARROW_1");
            Chart.RemoveObject(P + "MTF_ARROW_2");
            Chart.RemoveObject(P + "MTF_ARROW_3");
        }
    }
}
