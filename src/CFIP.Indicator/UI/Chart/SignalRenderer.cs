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
                            int closedM5)
                        {
                            if (_lastSignalMarkerClearM5 != closedM5)
                            {
                                Chart.RemoveObject(P + "BOS_MARKER");
                                Chart.RemoveObject(P + "MSS_MARKER");
                                Chart.RemoveObject(P + "SWEEP_MARKER");
                                _lastSignalMarkerClearM5 =
                                    closedM5;
                            }
                
                            if (Bars == null ||
                                Bars.Count < 2)
                                return;
                
                            PendingOrder pendingAuthority =
                                GetManagedPendingOrder();
                
                            if (pendingAuthority != null)
                            {
                                Chart.RemoveObject(P + "WATCH_ARROW");
                                Chart.RemoveObject(P + "REACTION_ARROW");
                                return;
                            }
                
                            bool decisionReady =
                                _decision != null &&
                                _decision.EntryAllowed &&
                                _decision.Direction != 0;
                
                            bool reactionReady =
                                EnableLiveReaction &&
                                ShowReactionArrow &&
                                _reaction != null &&
                                _reaction.EntryAllowed &&
                                _reaction.Direction != 0;
                
                            int visualDirection =
                                GetAuthoritativeDirection();
                
                            if (visualDirection == 0)
                            {
                                Chart.RemoveObject(P + "WATCH_ARROW");
                                RemovePlanLine(P + "WATCH_TRIGGER");
                                Chart.RemoveObject(P + "REACTION_ARROW");

                                _lastSignalRenderVisible = false;
                                _lastSignalRenderBar = -1;
                                _lastSignalRenderDirection = 0;
                                _lastSignalRenderState = "";

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
                                    _m5Bars.Count - 1,
                                    chartIndex);
                
                            reactionBar =
                                Math.Max(
                                    0,
                                    Math.Min(
                                        Bars.Count - 1,
                                        reactionBar));
                
                            int arrowBar =
                                reactionReady &&
                                visualDirection == _reaction.Direction
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
                                visualDirection == _reaction.Direction
                                    ? (_reaction.Confidence >=
                                       Math.Max(
                                           LiveReactionThreshold,
                                           LiveReactionStrongThreshold)
                                        ? "STRONG"
                                        : "REACTION")
                                    : decisionReady
                                        ? "CONFIRMED"
                                        : "WATCH";
                
                            bool arrowChanged =
                                !_lastSignalRenderVisible ||
                                _lastSignalRenderBar != arrowBar ||
                                _lastSignalRenderDirection != visualDirection ||
                                !string.Equals(
                                    _lastSignalRenderState,
                                    arrowState,
                                    StringComparison.Ordinal) ||
                                Chart.FindObject(
                                    P + "WATCH_ARROW") == null;

                            if (ShowSignalArrow)
                            {
                                if (arrowChanged)
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
                            }
                            else
                            {
                                Chart.RemoveObject(
                                    P + "WATCH_ARROW");
                            }

                            _lastSignalRenderVisible =
                                ShowSignalArrow;

                            _lastSignalRenderBar =
                                arrowBar;

                            _lastSignalRenderDirection =
                                visualDirection;

                            _lastSignalRenderState =
                                arrowState;
                
                            if (_executionModel != null &&
                                _executionModel.Direction == visualDirection &&
                                IsFinitePositive(
                                    _executionModel.Trigger) &&
                                ShowTrigger)
                            {
                                DrawPlanLine(
                                    P + "WATCH_TRIGGER",
                                    _executionModel.Trigger,
                                    TriggerLineColor,
                                    true);
                            }
                            else
                            {
                                RemovePlanLine(
                                    P + "WATCH_TRIGGER");
                            }

                            Chart.RemoveObject(
                                P + "REACTION_ARROW");
                
                            if (_decision != null &&
                                !decisionReady &&
                                ShowEarlyWatch &&
                                AlertOnEarlyWatch &&
                                _decision.Confidence >=
                                Math.Max(
                                    60,
                                    MinimumConfidence - 8) &&
                                _decision.Confidence <
                                MinimumConfidence &&
                                _lastEarlyAlertM5 !=
                                closedM5)
                            {
                                SendUnifiedAlert(
                                    "WATCH|" +
                                    closedM5 +
                                    "|" +
                                    _decision.Direction,
                                    "CFIP " +
                                    (_decision.Direction == 1
                                        ? "BUY"
                                        : "SELL") +
                                    " WATCH | CONF " +
                                    _decision.Confidence +
                                    " | SMART " +
                                    _decision.SmartQuality +
                                    " | " +
                                    _decision.Reason,
                                    _decision.Direction,
                                    false);
                
                                _lastEarlyAlertM5 =
                                    closedM5;
                            }
                
                            if (reactionReady &&
                                AlertOnReaction &&
                                AlertOnLiveReaction &&
                                _lastReactionAlertBar !=
                                _m5Bars.Count - 1)
                            {
                                SendUnifiedAlert(
                                    "REACTION|" +
                                    _m5Bars.Count,
                                    _reaction.Reason,
                                    _reaction.Direction,
                                    false);
                
                                _lastReactionAlertBar =
                                    _m5Bars.Count - 1;
                            }
                
                            _lastVisualDirection =
                                visualDirection;
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
