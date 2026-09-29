using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanLabelRenderCoordinator.cs
// Single-responsibility plan-label renderer.


namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPlanLabels(
            SignalVisualSnapshot snapshot)
                                {
                                    if (_plan == null ||
                                        snapshot == null ||
                                        !snapshot.PlanActive ||
                                        snapshot.PendingOrder ||
                                        (!ShowLevelPriceLabels &&
                                         !ShowSignalLabels) ||
                                        Bars == null ||
                                        Bars.Count < 2)
                                        return;

                                    RemovePlanLabels();
                        
                                    int bar =
                                        Math.Max(
                                            0,
                                            Math.Min(
                                                Bars.Count - 1,
                                                MapM5ToChart(
                                                    snapshot.CreatedM5,
                                                    Bars.Count - 1)));
                        
                                    if (ShowEntry)
                                    {
                                        DrawPlanLabel(
                                            P + "ENTRY_LABEL",
                                            "ENTRY " +
                                            Price(
                                                snapshot.Entry),
                                            bar,
                                            snapshot.Entry,
                                            EntryLineColor);
                                    }
                        
                                    bool idealDistinct =
                                        snapshot.IdealEntryVisible;
                        
                                    if (ShowEntry &&
                                        idealDistinct)
                                    {
                                        DrawPlanLabel(
                                            P + "IDEAL_ENTRY_LABEL",
                                            (snapshot.EntryMode ==
                                                ExecutionMode.BreakoutMarket
                                                ? "ZONE MID "
                                                : "IDEAL ") +
                                            Price(
                                                snapshot.IdealEntry),
                                            bar,
                                            snapshot.IdealEntry,
                                            PanelAccentColor);
                                    }
                        
                                    bool triggerDistinct =
                                        IsFinitePositive(snapshot.EntryTrigger) &&
                                        !SamePrice(
                                            snapshot.EntryTrigger,
                                            snapshot.Entry) &&
                                        (!idealDistinct ||
                                         !SamePrice(
                                             snapshot.EntryTrigger,
                                             snapshot.IdealEntry));
                        
                                    if (ShowTrigger &&
                                        triggerDistinct &&
                                        !snapshot.LivePosition)
                                    {
                                        DrawPlanLabel(
                                            P + "TRIGGER_LABEL",
                                            "TRIGGER " +
                                            Price(
                                                snapshot.EntryTrigger),
                                            bar,
                                            snapshot.EntryTrigger,
                                            TriggerLineColor);
                                    }
                        
                                    double displayStop =
                                        snapshot.Stop;
                        
                                    if (ShowSL &&
                                        IsFinitePositive(displayStop))
                                    {
                                        DrawPlanLabel(
                                            P + "SL_LABEL",
                                            "SL " +
                                            Price(displayStop),
                                            bar,
                                            displayStop,
                                            SlLineColor);
                                    }
                        
                                    bool tp1Distinct =
                                        IsFinitePositive(snapshot.Tp1) &&
                                        !SamePrice(snapshot.Tp1, snapshot.Entry) &&
                                        !SamePrice(snapshot.Tp1, snapshot.EntryTrigger);
                        
                                    if (ShowTP1 &&
                                        tp1Distinct)
                                    {
                                        DrawPlanLabel(
                                            P + "TP1_LABEL",
                                            "TP1 " +
                                            Price(
                                                snapshot.Tp1),
                                            bar,
                                            snapshot.Tp1,
                                            TpLineColor);
                                    }
                        
                                    bool tp2Distinct =
                                        IsFinitePositive(snapshot.Tp2) &&
                                        (!tp1Distinct ||
                                         !SamePrice(snapshot.Tp2, snapshot.Tp1)) &&
                                        !SamePrice(snapshot.Tp2, snapshot.Entry) &&
                                        !SamePrice(snapshot.Tp2, snapshot.EntryTrigger);
                        
                                    if (ShowTP2 &&
                                        tp2Distinct)
                                    {
                                        DrawPlanLabel(
                                            P + "TP2_LABEL",
                                            "TP2 " +
                                            Price(
                                                snapshot.Tp2),
                                            bar,
                                            snapshot.Tp2,
                                            Tp2LineColor);
                                    }
                        
                                    bool tp3Distinct =
                                        IsFinitePositive(snapshot.Tp3) &&
                                        (!tp2Distinct ||
                                         !SamePrice(snapshot.Tp3, snapshot.Tp2)) &&
                                        !SamePrice(snapshot.Tp3, snapshot.Entry) &&
                                        !SamePrice(snapshot.Tp3, snapshot.EntryTrigger);
                        
                                    if (ShowTP3 &&
                                        tp3Distinct)
                                    {
                                        DrawPlanLabel(
                                            P + "TP3_LABEL",
                                            "TP3 " +
                                            Price(
                                                snapshot.Tp3),
                                            bar,
                                            snapshot.Tp3,
                                            Tp3LineColor);
                                    }
                        
                                    bool tp4Distinct =
                                        IsFinitePositive(snapshot.Tp4) &&
                                        (!tp3Distinct ||
                                         !SamePrice(snapshot.Tp4, snapshot.Tp3)) &&
                                        !SamePrice(snapshot.Tp4, snapshot.Entry) &&
                                        !SamePrice(snapshot.Tp4, snapshot.EntryTrigger);
                        
                                    if (ShowTP4 &&
                                        tp4Distinct)
                                    {
                                        DrawPlanLabel(
                                            P + "TP4_LABEL",
                                            "TP4 " +
                                            Price(
                                                snapshot.Tp4),
                                            bar,
                                            snapshot.Tp4,
                                            Tp4LineColor);
                                    }
                        
                                    double activeBrokerTarget =
                                        snapshot.BrokerTarget;
                        
                                    bool activeBrokerTargetDistinct =
                                        snapshot.ActiveBrokerTargetVisible &&
                                        !SamePrice(activeBrokerTarget, snapshot.Entry) &&
                                        !SamePrice(activeBrokerTarget, snapshot.Tp1) &&
                                        !SamePrice(activeBrokerTarget, snapshot.Tp2) &&
                                        !SamePrice(activeBrokerTarget, snapshot.Tp3) &&
                                        !SamePrice(activeBrokerTarget, snapshot.Tp4);
                        
                                    if ((ShowTP1 ||
                                         ShowTP2 ||
                                         ShowTP3 ||
                                         ShowTP4) &&
                                        activeBrokerTargetDistinct)
                                    {
                                        DrawPlanLabel(
                                            P + "ACTIVE_TP_LABEL",
                                            "ACTIVE TP " +
                                            Price(activeBrokerTarget),
                                            bar,
                                            activeBrokerTarget,
                                            PanelAccentColor);
                                    }
                                }
    }
}
