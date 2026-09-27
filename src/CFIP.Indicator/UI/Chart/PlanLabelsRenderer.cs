// ============================================================================
// CFIP Indicator — PlanLabelsRenderer.cs
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
    }
}
