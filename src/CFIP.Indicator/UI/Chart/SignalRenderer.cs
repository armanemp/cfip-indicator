// ============================================================================
// CFIP Indicator — SignalRenderer.cs
// ============================================================================

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
        private void RenderWatchAndReaction(
            int chartIndex,
            int closedM5,
            SignalVisualSnapshot snapshot)
                        {
                            ClearWatchObjects();
                
                            if (Bars == null ||
                                Bars.Count < 2 ||
                                snapshot == null)
                                return;

                            bool decisionReady =
                                snapshot.DecisionReady;

                            bool reactionReady =
                                snapshot.ReactionReady;

                            int visualDirection =
                                snapshot.AuthoritativeDirection;

                            if (snapshot.PendingOrder)
                            {
                                Chart.RemoveObject(P + "WATCH_ARROW");
                                Chart.RemoveObject(P + "REACTION_ARROW");
                                return;
                            }
                
                            if (visualDirection == 0)
                            {
                                Chart.RemoveObject(P + "WATCH_ARROW");
                                Chart.RemoveObject(P + "REACTION_ARROW");
                                return;
                            }
                
                            int hostBar =
                                MapM5ToChart(
                                    closedM5,
                                    chartIndex);
                
                            hostBar =
                                Math.Max(
                                    0,
                                    Math.Min(
                                        Bars.Count - 1,
                                        hostBar));
                
                            int reactionBar =
                                MapM5ToChart(
                                    snapshot.ArrowM5Index,
                                    chartIndex);
                
                            reactionBar =
                                Math.Max(
                                    0,
                                    Math.Min(
                                        Bars.Count - 1,
                                        reactionBar));
                
                            int arrowBar =
                                reactionReady &&
                                visualDirection == snapshot.ReactionDirection
                                    ? reactionBar
                                    : hostBar;
                
                            double atr =
                                Atr(
                                    Bars,
                                    Math.Max(
                                        1,
                                        Math.Min(
                                            Bars.Count - 1,
                                            arrowBar)));
                
                            double offset =
                                Math.Max(
                                    Symbol.PipSize *
                                    Math.Max(
                                        0.5,
                                        MinimumArrowOffsetPips),
                                    atr *
                                    Math.Max(
                                        0.02,
                                        ArrowOffsetAtr));
                
                            string arrowState =
                                reactionReady &&
                                visualDirection == snapshot.ReactionDirection
                                    ? (snapshot.ReactionConfidence >=
                                       Math.Max(
                                           LiveReactionThreshold,
                                           LiveReactionStrongThreshold)
                                        ? "STRONG"
                                        : "REACTION")
                                    : decisionReady
                                        ? "CONFIRMED"
                                        : "WATCH";
                
                            if (ShowSignalArrow)
                            {
                                DrawIcon(
                                    P + "WATCH_ARROW",
                                    visualDirection == 1
                                        ? ChartIconType.UpArrow
                                        : ChartIconType.DownArrow,
                                    arrowBar,
                                    visualDirection == 1
                                        ? Bars.LowPrices[arrowBar] - offset
                                        : Bars.HighPrices[arrowBar] + offset,
                                    SignalArrowColorFor(
                                        visualDirection,
                                        arrowState));
                            }
                            else
                            {
                                Chart.RemoveObject(
                                    P + "WATCH_ARROW");
                            }
                
                            Chart.RemoveObject(
                                P + "REACTION_ARROW");
                
                            if (snapshot.DecisionDirection != 0 &&
                                !decisionReady &&
                                ShowEarlyWatch &&
                                AlertOnEarlyWatch &&
                                snapshot.Confidence >=
                                Math.Max(
                                    60,
                                    MinimumConfidence - 8) &&
                                snapshot.Confidence <
                                MinimumConfidence &&
                                _lastEarlyAlertM5 !=
                                closedM5)
                            {
                                SendUnifiedAlert(
                                    "WATCH|" +
                                    closedM5 +
                                    "|" +
                                    snapshot.DecisionDirection,
                                    "CFIP " +
                                    (snapshot.DecisionDirection == 1
                                        ? "BUY"
                                        : "SELL") +
                                    " WATCH | CONF " +
                                    snapshot.Confidence +
                                    " | SMART " +
                                    snapshot.SmartQuality +
                                    " | " +
                                    snapshot.DecisionReason,
                                    snapshot.DecisionDirection,
                                    false);
                
                                _lastEarlyAlertM5 =
                                    closedM5;
                            }
                
                            if (reactionReady &&
                                AlertOnReaction &&
                                AlertOnLiveReaction &&
                                _lastReactionAlertBar !=
                                snapshot.ArrowM5Index)
                            {
                                SendUnifiedAlert(
                                    "REACTION|" +
                                    snapshot.ArrowM5Index,
                                    snapshot.ReactionReason,
                                    snapshot.ReactionDirection,
                                    false);
                
                                _lastReactionAlertBar =
                                    snapshot.ArrowM5Index;
                            }
                
                            _lastVisualDirection =
                                visualDirection;
                        }
        
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

                            if (ShowSignalArrow)
                            {
                                DrawIcon(
                                    P + "ALERT_SIGNAL",
                                    _lastVisualAlertDirection == 1
                                        ? ChartIconType.UpArrow
                                        : ChartIconType.DownArrow,
                                    alertBar,
                                    _lastVisualAlertDirection == 1
                                        ? Bars.LowPrices[alertBar] - offset
                                        : Bars.HighPrices[alertBar] + offset,
                                    color);
                            }
                            else
                            {
                                Chart.RemoveObject(
                                    P + "ALERT_SIGNAL");
                            }

                            if (ShowSignalLabels)
                            {
                                int lineLeft =
                                    GetCompactPlanLineLeftBar();

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
                                    "SIGNAL " +
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

        private Color SignalArrowColorFor(
                            int direction,
                            string state)
                        {
                            if (state == "REACTION")
                                return BlockedReactionArrowColor;
                
                            if (state == "WATCH")
                                return direction == 1
                                    ? CautionBuyArrowColor
                                    : CautionSellArrowColor;
                
                            if (state == "CONFIRMED")
                                return direction == 1
                                    ? ConfirmedBuyArrowColor
                                    : ConfirmedSellArrowColor;
                
                            return direction == 1
                                ? StrongBuyArrowColor
                                : StrongSellArrowColor;
                        }
        
        private void DrawIcon(
                            string name,
                            ChartIconType type,
                            int bar,
                            double price,
                            Color color)
                        {
                            try
                            {
                                if (Bars == null ||
                                    Bars.Count == 0 ||
                                    !IsFinitePositive(price))
                                    return;
                
                                int safeBar =
                                    Math.Max(
                                        0,
                                        Math.Min(
                                            bar,
                                            Bars.Count - 1));
                
                                Chart.DrawIcon(
                                    name,
                                    type,
                                    safeBar,
                                    price,
                                    color);
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP icon render failed: {0}",
                                    ex.Message);
                            }
                        }
        
        private int ClosedChartIndex(
                            DateTime reference,
                            int fallback)
                        {
                            if (Bars == null ||
                                Bars.Count < 2 ||
                                reference == DateTime.MinValue)
                                return -1;
                
                            int closed =
                                ClosedIndex(
                                    Bars,
                                    reference);
                
                            if (closed >= 0 &&
                                closed < Bars.Count - 1)
                                return closed;
                
                            return Math.Max(
                                0,
                                Math.Min(
                                    Bars.Count - 2,
                                    fallback));
                        }
        
        private int MapM5ToClosedChart(
                            int m5Index,
                            int alternate)
                        {
                            if (_m5Bars == null ||
                                Bars == null ||
                                m5Index < 0 ||
                                m5Index >= _m5Bars.Count)
                                return Math.Max(
                                    0,
                                    Math.Min(
                                        Math.Max(
                                            0,
                                            Bars.Count - 2),
                                        alternate));
                
                            return ClosedChartIndex(
                                _m5Bars.OpenTimes[m5Index],
                                alternate);
                        }
        
        private int MapM5ToChart(
                            int m5Index,
                            int alternate)
                        {
                            if (_m5Bars == null ||
                                Bars == null ||
                                m5Index < 0 ||
                                m5Index >= _m5Bars.Count)
                                return Math.Max(
                                    0,
                                    Math.Min(
                                        alternate,
                                        Bars.Count - 1));
                
                            int mapped =
                                Bars.OpenTimes.GetIndexByTime(
                                    _m5Bars.OpenTimes[m5Index]);
                
                            return
                                mapped >= 0 &&
                                mapped < Bars.Count
                                    ? mapped
                                    : Math.Max(
                                        0,
                                        Math.Min(
                                            alternate,
                                            Bars.Count - 1));
                        }
        
        private void ClearWatchObjects()
                        {
                            Chart.RemoveObject(
                                P + "WATCH_ARROW");
                
                            Chart.RemoveObject(
                                P + "REACTION_ARROW");
                
                            Chart.RemoveObject(
                                P + "BOS_MARKER");
                
                            Chart.RemoveObject(
                                P + "MSS_MARKER");
                
                            Chart.RemoveObject(
                                P + "SWEEP_MARKER");
                        }
        
        private bool SamePrice(
                            double left,
                            double right)
                        {
                            if (!IsFinitePositive(left) ||
                                !IsFinitePositive(right))
                                return false;
                
                            double tolerance =
                                Math.Max(
                                    Symbol.TickSize,
                                    Symbol.PipSize * 0.05);
                
                            return Math.Abs(left - right) <=
                                   Math.Max(
                                       tolerance,
                                       Symbol.TickSize * 0.5);
                        }
    }
}
