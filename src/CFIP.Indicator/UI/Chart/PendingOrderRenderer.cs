// ============================================================================
// CFIP Indicator — PendingOrderRenderer.cs
// Presentation-only chart rendering for managed pending orders.
// ============================================================================

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderManagedPendingOrder(
            SignalVisualSnapshot snapshot)
        {
            RemoveManagedPendingOrderObjects();

            if (snapshot == null ||
                !snapshot.PendingOrder ||
                !IsFinitePositive(snapshot.PendingEntry))
                return;

            if (!ShowLevelLines)
                return;

            DrawPlanLine(
                P + "PENDING_ENTRY",
                snapshot.PendingEntry,
                TriggerLineColor,
                ShowTrigger);

            if (ShowSL &&
                IsFinitePositive(snapshot.PendingStop))
            {
                DrawPlanLine(
                    P + "PENDING_SL",
                    snapshot.PendingStop,
                    SlLineColor,
                    ShowSL);
            }

            if (ShowTP1 &&
                IsFinitePositive(snapshot.PendingTarget))
            {
                DrawPlanLine(
                    P + "PENDING_TP",
                    snapshot.PendingTarget,
                    TpLineColor,
                    ShowTP1);
            }

            if (!ShowLevelPriceLabels &&
                !ShowSignalLabels)
                return;

            int lineLeft =
                GetPlanLineLeftBar();

            int labelBar =
                GetCompactPlanLabelAnchorBar(
                    lineLeft);

            int boxRightBar =
                GetLabelBoxRightBar(
                    lineLeft);

            string typeText =
                snapshot.PendingOrderType;

            RenderCompactPlanLabel(
                P + "PENDING_ENTRY_LABEL",
                "PENDING " +
                typeText +
                " " +
                Price(snapshot.PendingEntry),
                snapshot.PendingEntry,
                TriggerLineColor,
                ShowTrigger,
                lineLeft,
                labelBar);

            RenderCompactPlanLabel(
                P + "PENDING_SL_LABEL",
                "SL " +
                Price(snapshot.PendingStop),
                snapshot.PendingStop,
                SlLineColor,
                ShowSL,
                lineLeft,
                labelBar);

            RenderCompactPlanLabel(
                P + "PENDING_TP_LABEL",
                "TP " +
                Price(snapshot.PendingTarget),
                snapshot.PendingTarget,
                TpLineColor,
                ShowTP1,
                lineLeft,
                labelBar);
        }

        private void RemoveManagedPendingOrderObjects()
        {
            RemovePlanLine(
                P + "PENDING_ENTRY");
            RemovePlanLine(
                P + "PENDING_SL");
            RemovePlanLine(
                P + "PENDING_TP");

            RemovePlanLabel(
                P + "PENDING_ENTRY_LABEL");
            RemovePlanLabel(
                P + "PENDING_SL_LABEL");
            RemovePlanLabel(
                P + "PENDING_TP_LABEL");
        }
    }
}
