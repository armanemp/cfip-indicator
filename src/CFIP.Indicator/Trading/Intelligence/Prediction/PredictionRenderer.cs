// ============================================================================
// CFIP Indicator — PredictionRenderer.cs
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
        private void RenderPredictionObjects(
                                    Prediction prediction,
                                    int closedM5)
                                {
                                    RemovePredictionObjects();
                        
                                    if (!ShowPredictionObjects ||
                                        !EnableEarlyPrediction ||
                                        prediction == null ||
                                        prediction.Direction == 0 ||
                                        prediction.Confidence <
                                        Math.Max(
                                            MinimumEarlyConfidence,
                                            EarlySetupConfidence))
                                        return;
                        
                                    int start =
                                        MapM5ToChart(
                                            Math.Max(
                                                0,
                                                closedM5 -
                                                Math.Max(
                                                    2,
                                                    PredictionLookaheadBars / 2)),
                                            Bars.Count - 1);
                        
                                    int end =
                                        MapM5ToChart(
                                            Math.Min(
                                                _m5Bars.Count - 1,
                                                closedM5 +
                                                Math.Max(
                                                    2,
                                                    PredictionLookaheadBars)),
                                            Bars.Count - 1);
                        
                                    if (end <= start)
                                        end =
                                            Math.Min(
                                                Bars.Count - 1,
                                                start + 4);
                        
                                    if (ShowPredictionZone &&
                                        prediction.ZoneHigh >
                                        prediction.ZoneLow)
                                    {
                                        ChartRectangle zone =
                                            Chart.DrawRectangle(
                                                P + "PRED_ZONE",
                                                start,
                                                prediction.ZoneHigh,
                                                end,
                                                prediction.ZoneLow,
                                                PredictionColor,
                                                1,
                                                LineStyle.Solid);
                        
                                        zone.IsFilled = true;
                        
                                        zone.Color =
                                            Color.FromArgb(
                                                35,
                                                PredictionColor);
                        
                                        zone.IsInteractive = false;
                                    }
                        
                                    if (prediction.Entry > 0)
                                        DrawPredictionLine(
                                            P + "PRED_ENTRY",
                                            prediction.Entry);
                        
                                    if (prediction.StopLoss > 0)
                                        DrawPredictionLine(
                                            P + "PRED_STOP",
                                            prediction.StopLoss);
                        
                                    if (prediction.Trigger > 0)
                                        DrawPredictionLine(
                                            P + "PRED_TRIGGER",
                                            prediction.Trigger);
                        
                                    if (ShowPredictionTargets)
                                    {
                                        if (prediction.Target1 > 0)
                                            DrawPredictionLine(
                                                P + "PRED_TARGET1",
                                                prediction.Target1);
                        
                                        if (prediction.Target2 > 0)
                                            DrawPredictionLine(
                                                P + "PRED_TARGET2",
                                                prediction.Target2);
                        
                                        if (prediction.Target3 > 0)
                                            DrawPredictionLine(
                                                P + "PRED_TARGET3",
                                                prediction.Target3);
                        
                                        if (prediction.Target4 > 0)
                                            DrawPredictionLine(
                                                P + "PRED_TARGET4",
                                                prediction.Target4);
                                    }
                        
                                    RenderPredictionLabels(
                                        prediction,
                                        closedM5);
                                }
        
        private void RenderPredictionLabels(
                                    Prediction prediction,
                                    int closedM5)
                                {
                                    if (prediction == null ||
                                        Bars == null ||
                                        Bars.Count < 2)
                                        return;
                        
                                    int bar =
                                        MapM5ToChart(
                                            closedM5,
                                            Bars.Count - 1);
                        
                                    bar =
                                        Math.Max(
                                            0,
                                            Math.Min(
                                                Bars.Count - 1,
                                                bar));
                        
                                    DrawPlanLabel(
                                        P + "PRED_ENTRY_LABEL",
                                        "ENTRY " +
                                        Price(prediction.Entry),
                                        bar,
                                        prediction.Entry,
                                        EntryLineColor);
                        
                                    if (prediction.StopLoss > 0)
                                        DrawPlanLabel(
                                            P + "PRED_STOP_LABEL",
                                            "SL " +
                                            Price(prediction.StopLoss),
                                            bar,
                                            prediction.StopLoss,
                                            SlLineColor);
                        
                                    if (prediction.Trigger > 0 &&
                                        !SamePrice(
                                            prediction.Trigger,
                                            prediction.Entry))
                                        DrawPlanLabel(
                                            P + "PRED_TRIGGER_LABEL",
                                            "TRIGGER " +
                                            Price(prediction.Trigger),
                                            bar,
                                            prediction.Trigger,
                                            TriggerLineColor);
                        
                                    if (ShowPredictionTargets)
                                    {
                                        if (prediction.Target1 > 0 &&
                                            !SamePrice(
                                                prediction.Target1,
                                                prediction.Entry))
                                            DrawPlanLabel(
                                                P + "PRED_TARGET1_LABEL",
                                                "TP1 " +
                                                Price(prediction.Target1),
                                                bar,
                                                prediction.Target1,
                                                TpLineColor);
                        
                                        if (prediction.Target2 > 0 &&
                                            !SamePrice(
                                                prediction.Target2,
                                                prediction.Target1))
                                            DrawPlanLabel(
                                                P + "PRED_TARGET2_LABEL",
                                                "TP2 " +
                                                Price(prediction.Target2),
                                                bar,
                                                prediction.Target2,
                                                Tp2LineColor);
                        
                                        if (prediction.Target3 > 0 &&
                                            !SamePrice(
                                                prediction.Target3,
                                                prediction.Target2))
                                            DrawPlanLabel(
                                                P + "PRED_TARGET3_LABEL",
                                                "TP3 " +
                                                Price(prediction.Target3),
                                                bar,
                                                prediction.Target3,
                                                Tp3LineColor);
                        
                                        if (prediction.Target4 > 0 &&
                                            !SamePrice(
                                                prediction.Target4,
                                                prediction.Target3))
                                            DrawPlanLabel(
                                                P + "PRED_TARGET4_LABEL",
                                                "TP4 " +
                                                Price(prediction.Target4),
                                                bar,
                                                prediction.Target4,
                                                Tp4LineColor);
                                    }
                                }
        
        private Color PredictionLineColor(
                                    string name)
                                {
                                    if (name.IndexOf(
                                            "PRED_TRIGGER",
                                            StringComparison.OrdinalIgnoreCase) >= 0)
                                        return TriggerLineColor;
                        
                                    if (name.IndexOf(
                                            "PRED_STOP",
                                            StringComparison.OrdinalIgnoreCase) >= 0)
                                        return SlLineColor;
                        
                                    if (name.IndexOf(
                                            "PRED_TARGET1",
                                            StringComparison.OrdinalIgnoreCase) >= 0)
                                        return TpLineColor;
                        
                                    if (name.IndexOf(
                                            "PRED_TARGET2",
                                            StringComparison.OrdinalIgnoreCase) >= 0)
                                        return Tp2LineColor;
                        
                                    if (name.IndexOf(
                                            "PRED_TARGET3",
                                            StringComparison.OrdinalIgnoreCase) >= 0)
                                        return Tp3LineColor;
                        
                                    if (name.IndexOf(
                                            "PRED_TARGET4",
                                            StringComparison.OrdinalIgnoreCase) >= 0)
                                        return Tp4LineColor;
                        
                                    if (name.IndexOf(
                                            "PRED_ENTRY",
                                            StringComparison.OrdinalIgnoreCase) >= 0)
                                        return EntryLineColor;
                        
                                    return PredictionColor;
                                }
        
        private void DrawPredictionLine(
                                    string name,
                                    double price)
                                {
                                    if (!IsFinitePositive(price) ||
                                        Bars == null ||
                                        Bars.Count < 2)
                                    {
                                        Chart.RemoveObject(name);
                                        return;
                                    }
                        
                                    try
                                    {
                                        int anchor =
                                            MapM5ToChart(
                                                _m5Bars == null
                                                    ? Bars.Count - 1
                                                    : Math.Max(
                                                        1,
                                                        _m5Bars.Count - 1),
                                                Bars.Count - 1);
                        
                                        anchor =
                                            Math.Max(
                                                0,
                                                Math.Min(
                                                    Bars.Count - 1,
                                                    anchor));
                        
                                        int left =
                                            Math.Max(
                                                0,
                                                anchor -
                                                Math.Max(
                                                    1,
                                                    LineLengthBars));
                        
                                        int right =
                                            Math.Min(
                                                Bars.Count - 1,
                                                anchor +
                                                Math.Max(
                                                    1,
                                                    Math.Max(
                                                        LineForwardBars,
                                                        PredictionLookaheadBars)));
                        
                                        if (right <= left)
                                        {
                                            Chart.RemoveObject(name);
                                            return;
                                        }
                        
                                        double normalized =
                                            NormalizePrice(price);
                        
                                        if (!IsFinitePositive(normalized))
                                        {
                                            Chart.RemoveObject(name);
                                            return;
                                        }
                        
                                        ChartTrendLine line =
                                            Chart.FindObject(name)
                                            as ChartTrendLine;
                        
                                        LineStyle lineStyle =
                                            LineStyle.Solid;
                        
                                        if (name.IndexOf(
                                                "PRED_TRIGGER",
                                                StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            lineStyle =
                                                LineStyle.Solid;
                                        }
                                        else if (name.IndexOf(
                                                    "PRED_TARGET",
                                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            lineStyle =
                                                LineStyle.Solid;
                                        }
                        
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
                                                    PredictionLineColor(name),
                                                    Math.Max(
                                                        1,
                                                        LevelLineThickness),
                                                    lineStyle);
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
                                            PredictionLineColor(name);
                                        line.Thickness =
                                            Math.Max(
                                                1,
                                                LevelLineThickness);
                                        line.LineStyle =
                                            lineStyle;
                                        line.ExtendToInfinity =
                                            false;
                                        line.IsInteractive =
                                            false;
                                    }
                                    catch (Exception ex)
                                    {
                                        Print(
                                            "CFIP prediction render failed: {0}",
                                            ex.Message);
                                    }
                                }
        
        private void RemovePredictionObjects()
                                {
                                    Chart.RemoveObject(
                                        P + "PRED_ZONE");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_ENTRY");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_ENTRY_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_STOP");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_STOP_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TRIGGER");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TRIGGER_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET1");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET1_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET2");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET2_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET3");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET3_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET4");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET4_LABEL");
                                }
    }
}
