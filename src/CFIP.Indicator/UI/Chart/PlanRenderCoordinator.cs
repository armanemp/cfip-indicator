// CFIP Indicator — PlanRenderCoordinator.cs
Single-responsibility chart plan renderer.

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
                                private void RenderPlan()
                                {
                                    if (_plan == null)
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
                                            _plan.Entry,
                                            EntryLineColor,
                                            ShowEntry);
                        
                                        bool idealDistinct =
                                            IsFinitePositive(_plan.IdealEntry) &&
                                            !SamePrice(
                                                _plan.IdealEntry,
                                                _plan.Entry);
                        
                                        DrawPlanLine(
                                            P + "IDEAL_ENTRY",
                                            _plan.IdealEntry,
                                            PanelAccentColor,
                                            ShowEntry &&
                                            idealDistinct);
                        
                                        bool triggerDistinct =
                                            IsFinitePositive(_plan.EntryTrigger) &&
                                            !SamePrice(
                                                _plan.EntryTrigger,
                                                _plan.Entry) &&
                                            (!idealDistinct ||
                                             !SamePrice(
                                                 _plan.EntryTrigger,
                                                 _plan.IdealEntry));
                        
                                        DrawPlanLine(
                                            P + "TRIGGER",
                                            _plan.EntryTrigger,
                                            TriggerLineColor,
                                            ShowTrigger &&
                                            triggerDistinct &&
                                            !_plan.IsLivePosition);
                        
                                        double displayStop =
                                            GetActiveBrokerStopPrice();
                        
                                        DrawPlanLine(
                                            P + "SL",
                                            displayStop,
                                            SlLineColor,
                                            ShowSL);
                        
                                        bool tp1Distinct =
                                            IsFinitePositive(_plan.Tp1) &&
                                            !SamePrice(_plan.Tp1, _plan.Entry) &&
                                            !SamePrice(_plan.Tp1, _plan.EntryTrigger);
                        
                                        DrawPlanLine(
                                            P + "TP1",
                                            _plan.Tp1,
                                            TpLineColor,
                                            ShowTP1 &&
                                            tp1Distinct);
                        
                                        bool tp2Distinct =
                                            IsFinitePositive(_plan.Tp2) &&
                                            (!tp1Distinct ||
                                             !SamePrice(_plan.Tp2, _plan.Tp1)) &&
                                            !SamePrice(_plan.Tp2, _plan.Entry) &&
                                            !SamePrice(_plan.Tp2, _plan.EntryTrigger);
                        
                                        DrawPlanLine(
                                            P + "TP2",
                                            _plan.Tp2,
                                            Tp2LineColor,
                                            ShowTP2 &&
                                            tp2Distinct);
                        
                                        bool tp3Distinct =
                                            IsFinitePositive(_plan.Tp3) &&
                                            (!tp2Distinct ||
                                             !SamePrice(_plan.Tp3, _plan.Tp2)) &&
                                            !SamePrice(_plan.Tp3, _plan.Entry) &&
                                            !SamePrice(_plan.Tp3, _plan.EntryTrigger);
                        
                                        DrawPlanLine(
                                            P + "TP3",
                                            _plan.Tp3,
                                            Tp3LineColor,
                                            ShowTP3 &&
                                            tp3Distinct);
                        
                                        bool tp4Distinct =
                                            IsFinitePositive(_plan.Tp4) &&
                                            (!tp3Distinct ||
                                             !SamePrice(_plan.Tp4, _plan.Tp3)) &&
                                            !SamePrice(_plan.Tp4, _plan.Entry) &&
                                            !SamePrice(_plan.Tp4, _plan.EntryTrigger);
                        
                                        DrawPlanLine(
                                            P + "TP4",
                                            _plan.Tp4,
                                            Tp4LineColor,
                                            ShowTP4 &&
                                            tp4Distinct);
                        
                                        double activeBrokerTarget =
                                            GetActiveBrokerTargetPrice();
                        
                                        bool activeBrokerTargetDistinct =
                                            _plan.IsLivePosition &&
                                            IsFinitePositive(activeBrokerTarget) &&
                                            !SamePrice(activeBrokerTarget, _plan.Entry) &&
                                            !SamePrice(activeBrokerTarget, _plan.Tp1) &&
                                            !SamePrice(activeBrokerTarget, _plan.Tp2) &&
                                            !SamePrice(activeBrokerTarget, _plan.Tp3) &&
                                            !SamePrice(activeBrokerTarget, _plan.Tp4);
                        
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
                                        RenderPlanLabels();
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
                                            _plan.CreatedM5,
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
                                        _plan.Direction == 1
                                            ? Bars.LowPrices[hostBar] -
                                              offset
                                            : Bars.HighPrices[hostBar] +
                                              offset;
                        
                                    DrawIcon(
                                        P + "ARROW",
                                        _plan.Direction == 1
                                            ? ChartIconType.UpArrow
                                            : ChartIconType.DownArrow,
                                        hostBar,
                                        y,
                                        SignalArrowColorFor(
                                            _plan.Direction,
                                            _plan.IsLivePosition
                                                ? "CONFIRMED"
                                                : _decision != null &&
                                                  _decision.SmartQuality >=
                                                  SmartStrongSetupQuality
                                                    ? "STRONG"
                                                    : "CONFIRMED"));
                                }
    }
}
