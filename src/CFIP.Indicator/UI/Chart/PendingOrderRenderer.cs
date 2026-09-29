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
                
                            int anchorBar =
                                Bars == null ||
                                Bars.Count < 2
                                    ? -1
                                    : Math.Max(
                                        0,
                                        Math.Min(
                                            Bars.Count - 1,
                                            MapM5ToChart(
                                                Math.Max(
                                                    1,
                                                    snapshot.ClosedM5),
                                                Bars.Count - 1)));
                
                            if (anchorBar < 0)
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
                
                            string typeText =
                                snapshot.PendingOrderType;
                
                            if (ShowTrigger)
                            {
                                DrawPlanLabel(
                                    P + "PENDING_ENTRY_LABEL",
                                    "PENDING " +
                                    typeText +
                                    " " +
                                    Price(
                                        snapshot.PendingEntry),
                                    anchorBar,
                                    snapshot.PendingEntry,
                                    TriggerLineColor);
                            }
                
                            if (IsFinitePositive(snapshot.PendingStop))
                            {
                                DrawPlanLabel(
                                    P + "PENDING_SL_LABEL",
                                    "SL " +
                                    Price(
                                        snapshot.PendingStop),
                                    anchorBar,
                                    snapshot.PendingStop,
                                    SlLineColor);
                            }
                
                            if (IsFinitePositive(snapshot.PendingTarget))
                            {
                                DrawPlanLabel(
                                    P + "PENDING_TP_LABEL",
                                    "TP " +
                                    Price(
                                        snapshot.PendingTarget),
                                    anchorBar,
                                    snapshot.PendingTarget,
                                    TpLineColor);
                            }
                        }

private void RemoveManagedPendingOrderObjects()
                        {
                            RemovePlanLine(
                                P + "PENDING_ENTRY");
                            RemovePlanLine(
                                P + "PENDING_SL");
                            RemovePlanLine(
                                P + "PENDING_TP");
                
                            Chart.RemoveObject(
                                P + "PENDING_ENTRY_LABEL");
                            Chart.RemoveObject(
                                P + "PENDING_SL_LABEL");
                            Chart.RemoveObject(
                                P + "PENDING_TP_LABEL");
                        }
    }
}
