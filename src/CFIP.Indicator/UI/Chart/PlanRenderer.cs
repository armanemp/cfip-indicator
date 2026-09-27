// ============================================================================
// CFIP Indicator — PlanRenderer.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        // ============================================================
                
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
        
        private void RenderPlanLabels()
                        {
                            if (_plan == null ||
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
                                            _plan.CreatedM5,
                                            Bars.Count - 1)));
                
                            if (ShowEntry)
                            {
                                DrawPlanLabel(
                                    P + "ENTRY_LABEL",
                                    "ENTRY " +
                                    Price(
                                        _plan.Entry),
                                    bar,
                                    _plan.Entry,
                                    EntryLineColor);
                            }
                
                            bool idealDistinct =
                                IsFinitePositive(_plan.IdealEntry) &&
                                !SamePrice(
                                    _plan.IdealEntry,
                                    _plan.Entry);
                
                            if (ShowEntry &&
                                idealDistinct)
                            {
                                DrawPlanLabel(
                                    P + "IDEAL_ENTRY_LABEL",
                                    (_plan.EntryMode ==
                                        ExecutionMode.BreakoutMarket
                                        ? "ZONE MID "
                                        : "IDEAL ") +
                                    Price(
                                        _plan.IdealEntry),
                                    bar,
                                    _plan.IdealEntry,
                                    PanelAccentColor);
                            }
                
                            bool triggerDistinct =
                                IsFinitePositive(_plan.EntryTrigger) &&
                                !SamePrice(
                                    _plan.EntryTrigger,
                                    _plan.Entry) &&
                                (!idealDistinct ||
                                 !SamePrice(
                                     _plan.EntryTrigger,
                                     _plan.IdealEntry));
                
                            if (ShowTrigger &&
                                triggerDistinct &&
                                !_plan.IsLivePosition)
                            {
                                DrawPlanLabel(
                                    P + "TRIGGER_LABEL",
                                    "TRIGGER " +
                                    Price(
                                        _plan.EntryTrigger),
                                    bar,
                                    _plan.EntryTrigger,
                                    TriggerLineColor);
                            }
                
                            double displayStop =
                                GetActiveBrokerStopPrice();
                
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
                                IsFinitePositive(_plan.Tp1) &&
                                !SamePrice(_plan.Tp1, _plan.Entry) &&
                                !SamePrice(_plan.Tp1, _plan.EntryTrigger);
                
                            if (ShowTP1 &&
                                tp1Distinct)
                            {
                                DrawPlanLabel(
                                    P + "TP1_LABEL",
                                    "TP1 " +
                                    Price(
                                        _plan.Tp1),
                                    bar,
                                    _plan.Tp1,
                                    TpLineColor);
                            }
                
                            bool tp2Distinct =
                                IsFinitePositive(_plan.Tp2) &&
                                (!tp1Distinct ||
                                 !SamePrice(_plan.Tp2, _plan.Tp1)) &&
                                !SamePrice(_plan.Tp2, _plan.Entry) &&
                                !SamePrice(_plan.Tp2, _plan.EntryTrigger);
                
                            if (ShowTP2 &&
                                tp2Distinct)
                            {
                                DrawPlanLabel(
                                    P + "TP2_LABEL",
                                    "TP2 " +
                                    Price(
                                        _plan.Tp2),
                                    bar,
                                    _plan.Tp2,
                                    Tp2LineColor);
                            }
                
                            bool tp3Distinct =
                                IsFinitePositive(_plan.Tp3) &&
                                (!tp2Distinct ||
                                 !SamePrice(_plan.Tp3, _plan.Tp2)) &&
                                !SamePrice(_plan.Tp3, _plan.Entry) &&
                                !SamePrice(_plan.Tp3, _plan.EntryTrigger);
                
                            if (ShowTP3 &&
                                tp3Distinct)
                            {
                                DrawPlanLabel(
                                    P + "TP3_LABEL",
                                    "TP3 " +
                                    Price(
                                        _plan.Tp3),
                                    bar,
                                    _plan.Tp3,
                                    Tp3LineColor);
                            }
                
                            bool tp4Distinct =
                                IsFinitePositive(_plan.Tp4) &&
                                (!tp3Distinct ||
                                 !SamePrice(_plan.Tp4, _plan.Tp3)) &&
                                !SamePrice(_plan.Tp4, _plan.Entry) &&
                                !SamePrice(_plan.Tp4, _plan.EntryTrigger);
                
                            if (ShowTP4 &&
                                tp4Distinct)
                            {
                                DrawPlanLabel(
                                    P + "TP4_LABEL",
                                    "TP4 " +
                                    Price(
                                        _plan.Tp4),
                                    bar,
                                    _plan.Tp4,
                                    Tp4LineColor);
                            }
                
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
        
        private int GetLabelAnchorBar(
                            string name,
                            int referenceBar)
                        {
                            if (Bars == null ||
                                Bars.Count < 2)
                                return 0;
                
                            bool prediction =
                                name != null &&
                                name.IndexOf(
                                    "PRED_",
                                    StringComparison.OrdinalIgnoreCase) >= 0;
                
                            int left;
                
                            if (!prediction &&
                                FullWidthLevelLines)
                            {
                                try
                                {
                                    left =
                                        Chart.FirstVisibleBarIndex;
                                }
                                catch
                                {
                                    left =
                                        referenceBar -
                                        Math.Max(
                                            1,
                                            LineLengthBars);
                                }
                            }
                            else
                            {
                                left =
                                    referenceBar -
                                    Math.Max(
                                        1,
                                        LineLengthBars);
                            }
                
                            left =
                                Math.Max(
                                    0,
                                    Math.Min(
                                        Bars.Count - 1,
                                        left));
                
                            int offset =
                                Math.Max(
                                    1,
                                    LabelLeftOffsetBars);
                
                            return
                                Math.Max(
                                    0,
                                    Math.Min(
                                        Bars.Count - 1,
                                        left + offset));
                        }
        
        private void DrawPlanLabel(
                            string name,
                            string text,
                            int bar,
                            double price,
                            Color color)
                        {
                            try
                            {
                                if (!IsFinitePositive(price) ||
                                    Bars == null ||
                                    Bars.Count < 2)
                                    return;
                
                                int labelBar =
                                    GetLabelAnchorBar(
                                        name,
                                        Math.Max(
                                            0,
                                            Math.Min(
                                                Bars.Count - 1,
                                                bar)));
                
                                double atr =
                                    Bars.Count >= 3
                                        ? Atr(
                                            Bars,
                                            Math.Max(
                                                1,
                                                Math.Min(
                                                    Bars.Count - 2,
                                                    labelBar)))
                                        : 0;
                
                                double labelOffset =
                                    Math.Max(
                                        Symbol.PipSize * 2,
                                        atr > 0
                                            ? atr * 0.06
                                            : Symbol.PipSize * 3);
                
                                double labelPrice =
                                    NormalizePrice(
                                        price +
                                        labelOffset);
                
                                ChartText label =
                                    Chart.DrawText(
                                        name,
                                        text,
                                        Bars.OpenTimes[labelBar],
                                        labelPrice,
                                        color);
                
                                label.FontSize =
                                    Math.Max(
                                        8,
                                        PanelFontSize);
                
                                label.FontFamily =
                                    string.IsNullOrWhiteSpace(
                                        PanelFontFamily)
                                        ? "Arial"
                                        : PanelFontFamily;
                
                                label.IsBold =
                                    PanelBold;
                
                                label.HorizontalAlignment =
                                    HorizontalAlignment.Right;
                
                                label.VerticalAlignment =
                                    VerticalAlignment.Bottom;
                
                                label.IsInteractive =
                                    false;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP plan label failed: {0}",
                                    ex.Message);
                            }
                        }
        
        private void RemovePlanLabels()
                        {
                            Chart.RemoveObject(
                                P + "ENTRY_LABEL");
                            Chart.RemoveObject(
                                P + "IDEAL_ENTRY_LABEL");
                            Chart.RemoveObject(
                                P + "TRIGGER_LABEL");
                            Chart.RemoveObject(
                                P + "SL_LABEL");
                            Chart.RemoveObject(
                                P + "TP1_LABEL");
                            Chart.RemoveObject(
                                P + "TP2_LABEL");
                            Chart.RemoveObject(
                                P + "TP3_LABEL");
                            Chart.RemoveObject(
                                P + "TP4_LABEL");
                            Chart.RemoveObject(
                                P + "ACTIVE_TP_LABEL");
                        }
        
        private void DrawPlanLine(
                            string name,
                            double price,
                            Color color,
                            bool visible)
                        {
                            if (!visible ||
                                !IsFinitePositive(price) ||
                                Bars == null ||
                                Bars.Count < 2)
                            {
                                RemovePlanLine(name);
                                return;
                            }
                
                            double normalized =
                                NormalizePrice(price);
                
                            if (!IsFinitePositive(normalized))
                            {
                                RemovePlanLine(name);
                                return;
                            }
                
                            try
                            {
                                int anchor =
                                    _plan != null
                                        ? MapM5ToChart(
                                            _plan.CreatedM5,
                                            Bars.Count - 1)
                                        : Bars.Count - 1;
                
                                anchor =
                                    Math.Max(
                                        0,
                                        Math.Min(
                                            Bars.Count - 1,
                                            anchor));
                
                                int left;
                                int right;
                
                                if (FullWidthLevelLines)
                                {
                                    left =
                                        Math.Max(
                                            0,
                                            Math.Min(
                                                Bars.Count - 1,
                                                Chart.FirstVisibleBarIndex));
                
                                    right =
                                        Math.Max(
                                            left,
                                            Math.Min(
                                                Bars.Count - 1,
                                                Chart.LastVisibleBarIndex));
                
                                    if (right <= left)
                                    {
                                        left = 0;
                                        right = Bars.Count - 1;
                                    }
                                }
                                else
                                {
                                    left =
                                        Math.Max(
                                            0,
                                            anchor -
                                            Math.Max(
                                                1,
                                                LineLengthBars));
                
                                    right =
                                        Math.Min(
                                            Bars.Count - 1,
                                            anchor +
                                            Math.Max(
                                                1,
                                                LineForwardBars));
                                }
                
                                if (right <= left)
                                {
                                    RemovePlanLine(name);
                                    return;
                                }
                
                                ChartTrendLine line =
                                    Chart.FindObject(name)
                                    as ChartTrendLine;
                
                                if (line == null)
                                {
                                    ChartObject existing =
                                        Chart.FindObject(name);
                
                                    if (existing != null)
                                        Chart.RemoveObject(name);
                
                                    line =
                                        Chart.DrawTrendLine(
                                            name,
                                            left,
                                            normalized,
                                            right,
                                            normalized,
                                            color,
                                            Math.Max(
                                                1,
                                                LevelLineThickness),
                                            LineStyle.Solid);
                                }
                
                                if (line == null)
                                    return;
                
                                line.Time1 =
                                    Bars.OpenTimes[left];
                                line.Y1 =
                                    normalized;
                                line.Time2 =
                                    Bars.OpenTimes[right];
                                line.Y2 =
                                    normalized;
                                line.Color =
                                    color;
                                line.Thickness =
                                    Math.Max(
                                        1,
                                        LevelLineThickness);
                                line.LineStyle =
                                    LineStyle.Solid;
                                line.ExtendToInfinity =
                                    false;
                                line.IsInteractive =
                                    false;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP level render failed [{0}]: {1}",
                                    name,
                                    ex.Message);
                            }
                        }
        
        private void ClearPlanObjects()
                        {
                            ClearWatchObjects();
                            RemovePlanLabels();
                
                            RemovePlanLine(
                                P + "ENTRY");
                
                            RemovePlanLine(
                                P + "IDEAL_ENTRY");
                
                            RemovePlanLine(
                                P + "TRIGGER");
                
                            RemovePlanLine(
                                P + "SL");
                
                            RemovePlanLine(
                                P + "TP1");
                
                            RemovePlanLine(
                                P + "TP2");
                
                            RemovePlanLine(
                                P + "TP3");
                
                            RemovePlanLine(
                                P + "TP4");
                
                            RemovePlanLine(
                                P + "ACTIVE_TP");
                
                            Chart.RemoveObject(
                                P + "ARROW");
                        }
        
        private void RemovePlanLine(
                            string name)
                        {
                            Chart.RemoveObject(name);
                        }
        
        private void RemovePlanObjects()
                        {
                            ClearPlanObjects();
                            RemovePredictionObjects();
                        }
    }
}
