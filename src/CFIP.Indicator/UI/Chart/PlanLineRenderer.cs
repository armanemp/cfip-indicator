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

                if (recreate)
                {
                    if (line != null)
                        Chart.RemoveObject(name);

                    ChartObject existing =
                        Chart.FindObject(name);

                    if (existing != null)
                        Chart.RemoveObject(name);

                    // One canonical X system: DateTime/OpenTime.
                    line =
                        Chart.DrawTrendLine(
                            name,
                            expectedStartTime,
                            normalized,
                            expectedEndTime,
                            normalized,
                            ResolveCanonicalPlanLineColor(
                                color),
                            thickness,
                            lineStyle);
                }

                if (line == null)
                    return;

                line.Y1 =
                    normalized;
                line.Y2 =
                    normalized;
                line.Color =
                    ResolveCanonicalPlanLineColor(
                        color);
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

        private Color ResolveCanonicalPlanLineColor(
            Color semanticColor)
        {
            // The line renderer is the single owner of chart-level color
            // materialization. Labels consume this exact same result.
            return Color.FromArgb(
                PlanLinePresentationRule.SignalLineAlpha,
                semanticColor);
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

            // Signal/plan lines have a fixed visual contract:
            // exactly 40 chart bars from the latest candle. The legacy
            // FullWidthLevelLines setting is retained elsewhere for
            // compatibility but must not expand signal-level geometry.
            return Math.Max(
                0,
                GetPlanLineRightBar() -
                CompactPlanLineLengthBars);
        }

        private LineStyle ResolvePlanLineStyle(
            string name)
        {
            // All signal/plan level lines intentionally use one solid style.
            return LineStyle.Solid;
        }

        private int ResolvePlanLineThickness(
            string name)
        {
            return
                PlanLinePresentationRule.ResolveThickness(
                    LevelLineThickness);
        }
    }
}
