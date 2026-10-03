using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderStackedSignalArrows(
            SignalVisualSnapshot snapshot,
            int direction,
            int bar,
            double offset,
            string fallbackState)
        {
            if (snapshot == null ||
                direction == 0 ||
                Bars == null ||
                Bars.Count == 0)
            {
                RemoveStackedSignalArrows();
                return;
            }

            int strength =
                HtfTrendArrowStrengthRule.ResolveStrength(
                    _h1Frame,
                    _h4Frame,
                    _d1Frame,
                    _w1Frame,
                    direction);

            // Preserve the previous one-arrow behavior when the
            // higher-timeframe stack has no usable directional evidence.
            if (strength <= 0)
            {
                strength =
                    fallbackState == "STRONG"
                        ? 7
                        : fallbackState == "CONFIRMED"
                            ? 4
                            : 1;
            }

            strength =
                Math.Max(
                    1,
                    Math.Min(
                        9,
                        strength));

            int arrowCount =
                ((strength - 1) % 3) + 1;

            int tier =
                (strength - 1) / 3;

            Color arrowColor =
                tier == 0
                    ? (direction == 1
                        ? CautionBuyArrowColor
                        : CautionSellArrowColor)
                    : tier == 1
                        ? (direction == 1
                            ? ConfirmedBuyArrowColor
                            : ConfirmedSellArrowColor)
                        : (direction == 1
                            ? StrongBuyArrowColor
                            : StrongSellArrowColor);

            double separation =
                Math.Max(
                    Symbol.PipSize * 0.75,
                    offset * 0.55);

            for (int i = 0;
                 i < 3;
                 i++)
            {
                string name =
                    i == 0
                        ? P + "WATCH_ARROW"
                        : P + "WATCH_ARROW_" +
                          (i + 1);

                if (i >= arrowCount)
                {
                    Chart.RemoveObject(name);
                    continue;
                }

                double price =
                    direction == 1
                        ? Bars.LowPrices[bar] -
                          offset -
                          separation * i
                        : Bars.HighPrices[bar] +
                          offset +
                          separation * i;

                DrawIcon(
                    name,
                    direction == 1
                        ? ChartIconType.UpArrow
                        : ChartIconType.DownArrow,
                    bar,
                    price,
                    arrowColor);
            }
        }

        private void RemoveStackedSignalArrows()
        {
            Chart.RemoveObject(
                P + "WATCH_ARROW");
            Chart.RemoveObject(
                P + "WATCH_ARROW_2");
            Chart.RemoveObject(
                P + "WATCH_ARROW_3");

            // Remove the pre-canonical active-plan marker as well. All
            // directional signal states now share one arrow renderer/lifecycle.
            Chart.RemoveObject(
                P + "ARROW");
        }
    }
}
