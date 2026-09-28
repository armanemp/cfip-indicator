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
private void RenderManagedPendingOrder()
                        {
                            PendingOrder pending =
                                GetManagedPendingOrder();

                            if (pending == null ||
                                !IsFinitePositive(
                                    pending.TargetPrice))
                            {
                                RemoveManagedPendingOrderObjects();
                                return;
                            }

                            DateTime now =
                                TimeInUtc;

                            if ((now - _lastPendingRenderUtc).TotalMilliseconds < 120)
                                return;

                            _lastPendingRenderUtc =
                                now;

                            RemoveManagedPendingOrderObjects();
                
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
                                                    _lastEvaluatedM5),
                                                Bars.Count - 1)));
                
                            if (anchorBar < 0)
                                return;
                
                            DrawPlanLine(
                                P + "PENDING_ENTRY",
                                pending.TargetPrice,
                                TriggerLineColor,
                                ShowTrigger);
                
                            if (ShowSL &&
                                pending.StopLoss.HasValue &&
                                IsFinitePositive(
                                    pending.StopLoss.Value))
                            {
                                DrawPlanLine(
                                    P + "PENDING_SL",
                                    pending.StopLoss.Value,
                                    SlLineColor,
                                    ShowSL);
                            }
                
                            if (ShowTP1 &&
                                pending.TakeProfit.HasValue &&
                                IsFinitePositive(
                                    pending.TakeProfit.Value))
                            {
                                DrawPlanLine(
                                    P + "PENDING_TP",
                                    pending.TakeProfit.Value,
                                    TpLineColor,
                                    ShowTP1);
                            }
                
                            if (!ShowLevelPriceLabels &&
                                !ShowSignalLabels)
                                return;
                
                            string typeText =
                                pending.OrderType ==
                                    PendingOrderType.Stop
                                    ? "STOP"
                                    : pending.OrderType ==
                                      PendingOrderType.Limit
                                        ? "LIMIT"
                                        : "PENDING";
                
                            if (ShowTrigger)
                            {
                                DrawPlanLabel(
                                    P + "PENDING_ENTRY_LABEL",
                                    "PENDING " +
                                    typeText +
                                    " " +
                                    Price(
                                        pending.TargetPrice),
                                    anchorBar,
                                    pending.TargetPrice,
                                    TriggerLineColor);
                            }
                
                            if (pending.StopLoss.HasValue &&
                                IsFinitePositive(
                                    pending.StopLoss.Value))
                            {
                                DrawPlanLabel(
                                    P + "PENDING_SL_LABEL",
                                    "SL " +
                                    Price(
                                        pending.StopLoss.Value),
                                    anchorBar,
                                    pending.StopLoss.Value,
                                    SlLineColor);
                            }
                
                            if (pending.TakeProfit.HasValue &&
                                IsFinitePositive(
                                    pending.TakeProfit.Value))
                            {
                                DrawPlanLabel(
                                    P + "PENDING_TP_LABEL",
                                    "TP " +
                                    Price(
                                        pending.TakeProfit.Value),
                                    anchorBar,
                                    pending.TakeProfit.Value,
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
