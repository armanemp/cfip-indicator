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
            if (snapshot == null ||
                Bars == null ||
                Bars.Count < 2)
            {
                RemoveStackedSignalArrows();
                return;
            }

            int safeBar =
                Math.Max(
                    0,
                    Math.Min(
                        Bars.Count - 1,
                        bar));

            double atr =
                Atr(
                    Bars,
                    Math.Max(
                        1,
                        Math.Min(
                            Bars.Count - 1,
                            safeBar)));

            double offset =
                Math.Max(
                    Symbol.PipSize * Math.Max(0.5, MinimumArrowOffsetPips),
                    atr * Math.Max(0.02, ArrowOffsetAtr));

            if (!ShowSignalArrow)
            {
                RemoveStackedSignalArrows();
                return;
            }

            RenderStackedSignalArrows(
                snapshot,
                safeBar,
                offset);
        }

        private void RenderStackedSignalArrows(
            SignalVisualSnapshot snapshot,
            int bar,
            double offset)
        {
            if (snapshot == null ||
                Bars == null ||
                Bars.Count == 0)
            {
                RemoveStackedSignalArrows();
                return;
            }

            int direction = snapshot.MtfTrendDirection;

            // A canonical actionable/active plan owns the trade direction.
            // Never display a contradictory MTF arrow above a live decision.
            int decisionDirection =
                snapshot.PlanDirection != 0
                    ? snapshot.PlanDirection
                    : snapshot.DecisionDirection;

            bool decisionOwnsDirection =
                snapshot.PlanActive ||
                snapshot.ActionableNow ||
                snapshot.DecisionEntryAllowed;

            if (decisionOwnsDirection &&
                decisionDirection != 0 &&
                direction != 0 &&
                decisionDirection != direction)
            {
                RemoveStackedSignalArrows();
                return;
            }

            if (direction == 0)
            {
                RemoveStackedSignalArrows();
                return;
            }

            // One canonical strength owner: the snapshot already contains the
            // 9-level MTF trend result calculated for the authoritative direction.
            int strength =
                NumericGuards.ClampInt(
                    snapshot.MtfTrendStrengthLevel,
                    0,
                    9);

            if (strength <= 0 ||
                snapshot.MtfTrendDirection != direction)
            {
                RemoveStackedSignalArrows();
                return;
            }

            int arrowCount =
                PanelNineLevelPresentationRule.ArrowCountForLevel(
                    strength);

            Color arrowColor =
                PanelNineLevelPresentationRule.ResolveColor(
                    direction,
                    strength,
                    StrongBuyArrowColor,
                    StrongSellArrowColor,
                    ConfirmedBuyArrowColor,
                    ConfirmedSellArrowColor,
                    CautionBuyArrowColor,
                    CautionSellArrowColor,
                    BlockedReactionArrowColor);

            // Keep every glyph outside the candle body and ensure the visible
            // arrow glyphs have a real vertical clearance from each other.
            double separation =
                Math.Max(
                    Symbol.PipSize * 3,
                    Math.Max(
                        offset * 1.5,
                        Symbol.TickSize * 8));

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
