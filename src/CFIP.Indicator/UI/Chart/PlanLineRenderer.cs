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

            double normalized = NormalizePrice(price);

            if (!IsFinitePositive(normalized))
            {
                RemovePlanLine(name);
                return;
            }

            try
            {
                int right =
                    GetPlanLineRightBar();

                int left =
                    GetPlanLineLeftBar();

                // Plan levels are chart geometry, not M5-event markers.
                // The right edge must always reach the latest chart candle.
                // This prevents M1/M15/H1/etc. charts from ending the line at
                // the exact M5 open-time mapping and appearing visually truncated.

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

        private int GetPlanLineRightBar()
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            return Bars.Count - 1;
        }

        private int GetPlanLineLeftBar()
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            if (FullWidthLevelLines)
                return 0;

            return Math.Max(
                0,
                GetPlanLineRightBar() -
                CompactPlanLineLengthBars);
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
