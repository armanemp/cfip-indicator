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
        private const int CompactPlanLineLengthBars = 40;

        private void DrawPlanLine(
            string name,
            double price,
            Color color,
            bool visible,
            int anchorM5)
        {
            if (!visible ||
                !IsFinitePositive(price) ||
                Bars == null ||
                Bars.Count < 2)
            {
                RemovePlanLine(name);
                return;
            }

            double normalized = NormalizePrice(price);

            if (!IsFinitePositive(normalized))
            {
                RemovePlanLine(name);
                return;
            }

            try
            {
                int right =
                    Bars.Count - 1;

                int left;

                if (FullWidthLevelLines)
                {
                    // Explicit compatibility mode: use the entire visible series.
                    // Default remains compact 40-bar presentation.
                    left = 0;
                    right =
                        Math.Max(
                            0,
                            Bars.Count - 1);
                }
                else
                {
                    if (anchorM5 >= 0)
                    {
                        right =
                            MapM5ToChart(
                                anchorM5,
                                right);

                        right =
                            Math.Max(
                                0,
                                Math.Min(
                                    Bars.Count - 1,
                                    right));
                    }

                    left =
                        Math.Max(
                            0,
                            right - CompactPlanLineLengthBars);
                }

                if (right <= left)
                {
                    RemovePlanLine(name);
                    return;
                }

                LineStyle lineStyle =
                    ResolvePlanLineStyle(name);

                int thickness =
                    ResolvePlanLineThickness(name);

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
                            thickness,
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
                    color;
                line.Thickness =
                    thickness;
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
                    "CFIP level render failed [{0}]: {1}",
                    name,
                    ex.Message);
            }
        }

        private LineStyle ResolvePlanLineStyle(
            string name)
        {
            if (name != null &&
                name.EndsWith(
                    "TRIGGER",
                    StringComparison.OrdinalIgnoreCase))
                return LineStyle.Dots;

            if (name != null &&
                name.EndsWith(
                    "SL",
                    StringComparison.OrdinalIgnoreCase))
                return LineStyle.DotsRare;

            if (name != null &&
                name.EndsWith(
                    "TP2",
                    StringComparison.OrdinalIgnoreCase))
                return LineStyle.Lines;

            if (name != null &&
                name.EndsWith(
                    "TP3",
                    StringComparison.OrdinalIgnoreCase))
                return LineStyle.Lines;

            if (name != null &&
                name.EndsWith(
                    "TP4",
                    StringComparison.OrdinalIgnoreCase))
                return LineStyle.LinesDots;

            return LineStyle.Solid;
        }

        private int ResolvePlanLineThickness(
            string name)
        {
            int configured =
                Math.Max(
                    1,
                    LevelLineThickness);

            if (name != null &&
                name.EndsWith(
                    "ENTRY",
                    StringComparison.OrdinalIgnoreCase))
                return Math.Max(
                    2,
                    configured);

            return Math.Min(
                2,
                configured);
        }
    }
}
