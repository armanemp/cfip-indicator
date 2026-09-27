using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanLineRenderer.cs
// Single-responsibility chart plan renderer.


namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
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
    }
}
