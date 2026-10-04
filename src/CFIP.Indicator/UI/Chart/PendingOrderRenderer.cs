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

            DateTime labelTime =
                GetCompactPlanLabelAnchorTime();

            string typeText =
                snapshot.PendingOrderType;

            RenderCompactPlanLabel(
                P + "PENDING_ENTRY_LABEL",
                BuildCanonicalLevelLabel(
                    "PENDING " + typeText,
                    "ENTRY",
                    snapshot.PendingEntry,
                    snapshot.PendingEntry,
                    false,
                    false,
                    0,
                    PlanTimeframeTag()),
                snapshot.PendingEntry,
                TriggerLineColor,
                ShowTrigger,
                labelTime);

            RenderCompactPlanLabel(
                P + "PENDING_SL_LABEL",
                BuildCanonicalLevelLabel(
                    "PENDING " + typeText,
                    "SL",
                    snapshot.PendingStop,
                    snapshot.PendingEntry,
                    true,
                    false,
                    0,
                    PlanTimeframeTag()),
                snapshot.PendingStop,
                SlLineColor,
                ShowSL,
                labelBar);

            RenderCompactPlanLabel(
                P + "PENDING_TP_LABEL",
                BuildCanonicalLevelLabel(
                    "PENDING " + typeText,
                    "TP",
                    snapshot.PendingTarget,
                    snapshot.PendingEntry,
                    true,
                    false,
                    0,
                    PlanTimeframeTag()),
                snapshot.PendingTarget,
                TpLineColor,
                ShowTP1,
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
