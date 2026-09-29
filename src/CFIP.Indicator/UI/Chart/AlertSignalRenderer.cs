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
        private void RenderLatestAlertSignalMarker(
                            int fallbackM5)
                        {
                            if (Bars == null ||
                                Bars.Count < 2 ||
                                _lastVisualAlertDirection == 0)
                                return;

                            int alertM5 =
                                _lastVisualAlertM5 >= 0
                                    ? _lastVisualAlertM5
                                    : fallbackM5;

                            int alertBar =
                                MapM5ToClosedChart(
                                    alertM5,
                                    Math.Max(
                                        0,
                                        Bars.Count - 2));

                            if (alertBar < 0)
                                return;

                            double atr =
                                Atr(
                                    Bars,
                                    Math.Max(
                                        1,
                                        Math.Min(
                                            Bars.Count - 2,
                                            alertBar)));

                            double offset =
                                Math.Max(
                                    Symbol.PipSize * 3,
                                    atr > 0
                                        ? atr * 0.20
                                        : Symbol.PipSize * 5);

                            string state =
                                string.Equals(
                                    _lastVisualAlertKind,
                                    "REACTION",
                                    StringComparison.OrdinalIgnoreCase)
                                    ? "REACTION"
                                    : string.Equals(
                                        _lastVisualAlertKind,
                                        "HIGH",
                                        StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(
                                        _lastVisualAlertKind,
                                        "SMART",
                                        StringComparison.OrdinalIgnoreCase)
                                        ? "CONFIRMED"
                                        : "WATCH";

                            Color color =
                                SignalArrowColorFor(
                                    _lastVisualAlertDirection,
                                    state);

                            // Alerts are event/state annotations, not a second
                            // directional visual authority. The canonical arrow is
                            // rendered by SignalRenderer from ActionableNow.
                            Chart.RemoveObject(
                                P + "ALERT_SIGNAL");

                            if (ShowSignalLabels)
                            {
                                int lineLeft =
                                    GetPlanLineLeftBar();

                                int labelBar =
                                    GetCompactPlanLabelAnchorBar(
                                        lineLeft);

                                int boxRightBar =
                                    GetLabelBoxRightBar(
                                        lineLeft);

                                double labelAtr =
                                    Atr(
                                        Bars,
                                        Math.Max(
                                            1,
                                            Math.Min(
                                                Bars.Count - 2,
                                                labelBar)));

                                double boxHalfHeight =
                                    Math.Max(
                                        Symbol.PipSize * 3,
                                        labelAtr > 0
                                            ? labelAtr * 0.055
                                            : Symbol.PipSize * 4);

                                RenderCompactPlanLabel(
                                    P + "ALERT_SIGNAL_LABEL",
                                    "ALERT " +
                                    (_lastVisualAlertDirection == 1
                                        ? "BUY"
                                        : "SELL") +
                                    " • " +
                                    (_lastVisualAlertKind.Length > 0
                                        ? _lastVisualAlertKind
                                        : "ALERT"),
                                    _lastVisualAlertDirection == 1
                                        ? Bars.LowPrices[alertBar]
                                        : Bars.HighPrices[alertBar],
                                    color,
                                    true,
                                    lineLeft,
                                    labelBar,
                                    boxRightBar,
                                    boxHalfHeight);
                            }
                            else
                            {
                                RemovePlanLabel(
                                    P + "ALERT_SIGNAL_LABEL");
                            }
                        }

    }
}
