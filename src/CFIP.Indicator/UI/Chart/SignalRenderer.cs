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

            int visualDirection =
                snapshot.AuthoritativeDirection != 0
                    ? snapshot.AuthoritativeDirection
                    : snapshot.DecisionDirection;

            // Direction arrows are live-state guidance. Keep the arrow on
            // the current chart bar so it follows the market rather than remaining
            // pinned to the last closed M5 candle.
            int hostBar =
                Math.Max(
                    0,
                    Math.Min(
                        Bars.Count - 1,
                        chartIndex));

            bool signalPresentationAllowed =
                !snapshot.PendingOrder &&
                !snapshot.LivePosition;

            if (!signalPresentationAllowed)
            {
                RemoveMtfTrendStrengthArrowStack();
                Chart.RemoveObject(P + "REACTION_ARROW");
                return;
            }

            if (!snapshot.ActionableNow)
            {
                RenderNonActionableWatchState(
                    snapshot,
                    visualDirection,
                    hostBar);
                return;
            }

            if (visualDirection == 0)
            {
                RemoveMtfTrendStrengthArrowStack();
                Chart.RemoveObject(P + "REACTION_ARROW");
                return;
            }

            int arrowBar =
                hostBar;

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

            if (ShowSignalArrow)
            {
                RenderMtfTrendStrengthArrowStack(
                    snapshot,
                    ResolveArrowStackDirection(snapshot, visualDirection),
                    arrowBar,
                    offset);
            }
            else
            {
                RemoveMtfTrendStrengthArrowStack();
            }

            Chart.RemoveObject(
                P + "REACTION_ARROW");

            _lastVisualDirection =
                visualDirection;
        }

        private void RenderTriggerRuntimeMarker(
                            SignalVisualSnapshot snapshot)
                        {
                            Chart.RemoveObject(
                                P + "M1_TRIGGER");

                            if (snapshot == null ||
                                !snapshot.TriggerRuntimeReady ||
                                !ShowTrigger ||
                                snapshot.TriggerM1Index < 0 ||
                                (snapshot.DecisionDirection != 1 &&
                                 snapshot.DecisionDirection != -1) ||
                                Bars == null ||
                                Bars.Count < 2)
                                return;

                            int triggerBar =
                                MapM1ToChart(
                                    snapshot.TriggerM1Index,
                                    Bars.Count - 1);

                            triggerBar =
                                Math.Max(
                                    0,
                                    Math.Min(
                                        Bars.Count - 1,
                                        triggerBar));

                            double atr =
                                triggerBar >= 1
                                    ? Atr(
                                        Bars,
                                        triggerBar)
                                    : 0;

                            double offset =
                                Math.Max(
                                    Symbol.PipSize * 1.5,
                                    atr > 0
                                        ? atr * 0.12
                                        : Symbol.PipSize * 2);

                            double price =
                                snapshot.DecisionDirection == 1
                                    ? Bars.LowPrices[triggerBar] - offset
                                    : Bars.HighPrices[triggerBar] + offset;

                            DrawIcon(
                                P + "M1_TRIGGER",
                                (snapshot.DecisionDirection == 1
                                     ? ChartIconType.UpArrow
                                     : ChartIconType.DownArrow),
                                triggerBar,
                                price,
                                SignalArrowColorFor(
                                    snapshot.DecisionDirection,
                                    "STRONG"));
                        }

        private int MapM1ToChart(
                            int m1Index,
                            int alternate)
                        {
                            if (_m1Bars == null ||
                                Bars == null ||
                                m1Index < 0 ||
                                m1Index >= _m1Bars.Count)
                                return Math.Max(
                                    0,
                                    Math.Min(
                                        alternate,
                                        Bars.Count - 1));

                            int mapped =
                                FindContainingBarIndex(
                                    Bars,
                                    _m1Bars.OpenTimes[m1Index]);

                            return mapped >= 0 &&
                                   mapped < Bars.Count
                                ? mapped
                                : Math.Max(
                                    0,
                                    Math.Min(
                                        alternate,
                                        Bars.Count - 1));
                        }

        private string ResolveTriggerArrowState(
                            SignalVisualSnapshot snapshot)
                        {
                            if (snapshot == null)
                                return "WATCH";

                            int required =
                                Math.Max(
                                    1,
                                    snapshot.TriggerRuntimeRequired);

                            int score =
                                Math.Max(
                                    0,
                                    snapshot.TriggerRuntimeScore);

                            if (score >= required + 1)
                                return "STRONG";

                            if (score >= required)
                                return "CONFIRMED";

                            return "WATCH";
                        }

        // Compatibility facade only: the strength/state owner remains MtfTrendStrengthRule.
        private string ResolveSignalArrowState(
            SignalVisualSnapshot snapshot,
            int direction)
        {
            if (snapshot == null || direction == 0)
                return "WATCH";

            return MtfTrendStrengthRule.ResolveMtfTrendStrengthState(
                snapshot.MtfTrendStrengthLevel);
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
                            RemoveMtfTrendStrengthArrowStack();
                
                            Chart.RemoveObject(
                                P + "REACTION_ARROW");
                
                            Chart.RemoveObject(
                                P + "BOS_MARKER");
                
                            Chart.RemoveObject(
                                P + "MSS_MARKER");
                
                            Chart.RemoveObject(
                                P + "SWEEP_MARKER");
                            Chart.RemoveObject(
                                P + "M1_TRIGGER");

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
