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

        private int GetPredictionLineLeftBar(
                                    int closedM5)
                                {
                                    if (Bars == null ||
                                        Bars.Count < 2)
                                        return 0;

                                    int anchor =
                                        MapM5ToChart(
                                            closedM5,
                                            Bars.Count - 1);

                                    anchor =
                                        Math.Max(
                                            0,
                                            Math.Min(
                                                Bars.Count - 1,
                                                anchor));

                                    return Math.Max(
                                        0,
                                        anchor -
                                        Math.Max(
                                            1,
                                            LineLengthBars));
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

                                        int left =
                                            GetPredictionLineLeftBar(
                                                _m5Bars == null
                                                    ? Bars.Count - 1
                                                    : Math.Max(
                                                        1,
                                                        _m5Bars.Count - 1));
                        
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
                        
                                        DateTime expectedStartTime =
                                            Bars.OpenTimes[left];
                                        DateTime expectedEndTime =
                                            Bars.OpenTimes[right];

                                        ChartTrendLine line =
                                            Chart.FindObject(name)
                                            as ChartTrendLine;

                                        bool recreate =
                                            line == null ||
                                            line.Time1 != expectedStartTime ||
                                            line.Time2 != expectedEndTime;
                        
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
                        
                                        if (recreate)
                                        {
                                            if (line != null)
                                                Chart.RemoveObject(name);

                                            ChartObject existing =
                                                Chart.FindObject(name);

                                            if (existing != null)
                                                Chart.RemoveObject(name);

                                            line =
                                                Chart.DrawTrendLine(
                                                    name,
                                                    expectedStartTime,
                                                    normalized,
                                                    expectedEndTime,
                                                    normalized,
                                                    PredictionLineColor(name),
                                                    1,
                                                    lineStyle);
                                        }
                        
                                        if (line == null)
                                            return;
                        
                                        line.Y1 =
                                            normalized;
                                        line.Y2 =
                                            normalized;
                                        line.Color =
                                            PredictionLineColor(name);
                                        line.Thickness =
                                            1;
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
