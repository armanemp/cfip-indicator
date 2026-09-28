// ============================================================================
// CFIP Indicator — UI/Chart/PredictionRenderer.cs
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
    }
}
