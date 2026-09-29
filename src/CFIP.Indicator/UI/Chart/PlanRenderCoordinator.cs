using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanRenderCoordinator.cs
// Single-responsibility chart plan renderer.


namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
                                private void RenderPlan(
            SignalVisualSnapshot snapshot)
                                {
                                    if (_plan == null ||
                                        snapshot == null ||
                                        !snapshot.PlanActive ||
                                        snapshot.PendingOrder)
                                    {
                                        RemovePlanObjects();
                                        return;
                                    }
                        
                                    // A live/confirmed Plan is authoritative for execution visuals.
                                    // Prediction and watch/reaction objects must not survive beside it,
                                    // otherwise identical prices can render as apparently duplicated
                                    // lines/arrows.
                                    RemovePredictionObjects();
                                    ClearWatchObjects();
                        
                                    if (!ShowLevelLines)
                                    {
                                        RemovePlanLine(P + "ENTRY");
                                        RemovePlanLine(P + "IDEAL_ENTRY");
                                        RemovePlanLine(P + "TRIGGER");
                                        RemovePlanLine(P + "SL");
                                        RemovePlanLine(P + "TP1");
                                        RemovePlanLine(P + "TP2");
                                        RemovePlanLine(P + "TP3");
                                        RemovePlanLine(P + "TP4");
                                    }
                                    else
                                    {
                                        DrawPlanLine(
                                            P + "ENTRY",
                                            snapshot.Entry,
                                            EntryLineColor,
                                            ShowEntry);
                        
                                        bool idealDistinct =
                                            snapshot.IdealEntryVisible;
                        
                                        DrawPlanLine(
                                            P + "IDEAL_ENTRY",
                                            snapshot.IdealEntry,
                                            PanelAccentColor,
                                            ShowEntry &&
                                            idealDistinct);
                        
                                        bool triggerDistinct =
                                            snapshot.TriggerVisible;
                        
                                        bool triggerVisualState =
                                            snapshot.TriggerVisible;

                                        DrawPlanLine(
                                            P + "TRIGGER",
                                            snapshot.Trigger,
                                            TriggerLineColor,
                                            ShowTrigger &&
                                            triggerDistinct &&
                                            triggerVisualState);
                        
                                        double displayStop =
                                            snapshot.Stop;
                        
                                        DrawPlanLine(
                                            P + "SL",
                                            displayStop,
                                            SlLineColor,
                                            ShowSL);
                        
                                        bool tp1Distinct =
                                            IsFinitePositive(snapshot.Tp1) &&
                                            !SamePrice(snapshot.Tp1, snapshot.Entry) &&
                                            !SamePrice(snapshot.Tp1, snapshot.Trigger);
                        
                                        DrawPlanLine(
                                            P + "TP1",
                                            snapshot.Tp1,
                                            TpLineColor,
                                            ShowTP1 &&
                                            tp1Distinct);
                        
                                        bool tp2Distinct =
                                            IsFinitePositive(snapshot.Tp2) &&
                                            (!tp1Distinct ||
                                             !SamePrice(snapshot.Tp2, snapshot.Tp1)) &&
                                            !SamePrice(snapshot.Tp2, snapshot.Entry) &&
                                            !SamePrice(snapshot.Tp2, snapshot.Trigger);
                        
                                        DrawPlanLine(
                                            P + "TP2",
                                            snapshot.Tp2,
                                            Tp2LineColor,
                                            ShowTP2 &&
                                            tp2Distinct);
                        
                                        bool tp3Distinct =
                                            IsFinitePositive(snapshot.Tp3) &&
                                            (!tp2Distinct ||
                                             !SamePrice(snapshot.Tp3, snapshot.Tp2)) &&
                                            !SamePrice(snapshot.Tp3, snapshot.Entry) &&
                                            !SamePrice(snapshot.Tp3, snapshot.Trigger);
                        
                                        DrawPlanLine(
                                            P + "TP3",
                                            snapshot.Tp3,
                                            Tp3LineColor,
                                            ShowTP3 &&
                                            tp3Distinct);
                        
                                        bool tp4Distinct =
                                            IsFinitePositive(snapshot.Tp4) &&
                                            (!tp3Distinct ||
                                             !SamePrice(snapshot.Tp4, snapshot.Tp3)) &&
                                            !SamePrice(snapshot.Tp4, snapshot.Entry) &&
                                            !SamePrice(snapshot.Tp4, snapshot.Trigger);
                        
                                        DrawPlanLine(
                                            P + "TP4",
                                            snapshot.Tp4,
                                            Tp4LineColor,
                                            ShowTP4 &&
                                            tp4Distinct);
                        
                                        double activeBrokerTarget =
                                            snapshot.BrokerTarget;
                        
                                        bool activeBrokerTargetDistinct =
                                            snapshot.ActiveBrokerTargetVisible &&
                                            !SamePrice(activeBrokerTarget, snapshot.Entry) &&
                                            !SamePrice(activeBrokerTarget, snapshot.Tp1) &&
                                            !SamePrice(activeBrokerTarget, snapshot.Tp2) &&
                                            !SamePrice(activeBrokerTarget, snapshot.Tp3) &&
                                            !SamePrice(activeBrokerTarget, snapshot.Tp4);
                        
                                        DrawPlanLine(
                                            P + "ACTIVE_TP",
                                            activeBrokerTarget,
                                            PanelAccentColor,
                                            (ShowTP1 ||
                                             ShowTP2 ||
                                             ShowTP3 ||
                                             ShowTP4) &&
                                            activeBrokerTargetDistinct);
                                    }
                        
                                    if (ShowLevelPriceLabels ||
                                        ShowSignalLabels)
                                        RenderPlanLabels(snapshot);
                                    else
                                        RemovePlanLabels();
                        
                                    if (!ShowSignalArrow ||
                                        Bars == null ||
                                        Bars.Count < 2)
                                    {
                                        Chart.RemoveObject(
                                            P + "ARROW");
                                        return;
                                    }
                        
                                    int hostBar =
                                        MapM5ToChart(
                                            snapshot.CreatedM5,
                                            Bars.Count - 1);
                        
                                    double atr =
                                        Atr(
                                            Bars,
                                            Math.Max(
                                                1,
                                                Math.Min(
                                                    Bars.Count - 1,
                                                    hostBar)));
                        
                                    double offset =
                                        Math.Max(
                                            Symbol.PipSize * 2,
                                            atr * 0.18);
                        
                                    double y =
                                        snapshot.Direction == 1
                                            ? Bars.LowPrices[hostBar] -
                                              offset
                                            : Bars.HighPrices[hostBar] +
                                              offset;
                        
                                    DrawIcon(
                                        P + "ARROW",
                                        snapshot.Direction == 1
                                            ? ChartIconType.UpArrow
                                            : ChartIconType.DownArrow,
                                        hostBar,
                                        y,
                                        SignalArrowColorFor(
                                            snapshot.Direction,
                                            snapshot.LivePosition
                                                ? "CONFIRMED"
                                                : snapshot.SmartQuality >=
                                                  SmartStrongSetupQuality
                                                    ? "STRONG"
                                                    : "CONFIRMED"));
                                }
    }
}
