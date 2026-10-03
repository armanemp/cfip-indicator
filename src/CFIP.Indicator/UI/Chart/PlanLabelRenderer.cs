using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanLabelRenderer.cs
// Single-responsibility plan-label renderer.

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const double CompactPlanLabelFontSize = 8.5;
        private const int CompactPlanLabelWidthBars = 7;
        private const int CompactPlanLabelGapBars = 2;
        private const double CompactPlanLabelHeightRangeFactor = 0.12;
        private const double CompactPlanLabelMinimumHeightPips = 4.0;

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

                int safeBar =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            bar));

                double labelPrice =
                    NormalizePrice(price);

                Color labelTextColor =
                    GetReadableLabelTextColor(color);

                ChartText label =
                    Chart.FindObject(name)
                    as ChartText;

                if (label == null)
                {
                    ChartObject existing =
                        Chart.FindObject(name);

                    if (existing != null)
                        Chart.RemoveObject(name);

                    label =
                        Chart.DrawText(
                            name,
                            text,
                            Bars.OpenTimes[safeBar],
                            labelPrice,
                            labelTextColor);
                }

                if (label == null)
                    return;

                label.Text =
                    text;
                label.Time =
                    Bars.OpenTimes[safeBar];
                label.Y =
                    labelPrice;
                label.Color =
                    labelTextColor;
                label.FontSize =
                    CompactPlanLabelFontSize;
                label.FontFamily =
                    string.IsNullOrWhiteSpace(
                        PanelFontFamily)
                        ? "Arial"
                        : PanelFontFamily;
                label.IsBold =
                    false;
                label.HorizontalAlignment =
                    HorizontalAlignment.Left;
                label.VerticalAlignment =
                    VerticalAlignment.Center;
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

        private void RenderCompactPlanLabel(
            string name,
            string text,
            double price,
            Color color,
            bool visible,
            int labelBar)
        {
            if (!visible ||
                !IsFinitePositive(price) ||
                Bars == null ||
                Bars.Count < 2)
            {
                RemovePlanLabel(name);
                return;
            }

            DrawCompactPlanLabel(
                name,
                text,
                price,
                color,
                labelBar);
        }

        private void DrawCompactPlanLabel(
            string name,
            string text,
            double price,
            Color semanticColor,
            int labelBar)
        {
            try
            {
                if (Bars == null ||
                    Bars.Count < 2 ||
                    !IsFinitePositive(price))
                    return;

                // The label belongs OUTSIDE the line geometry.
                // The line starts at GetPlanLineLeftBar(); the tag is placed
                // immediately to its left with a fixed chart-bar gap.
                int lineLeftBar =
                    GetPlanLineLeftBar();

                int tagRightBar =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            lineLeftBar -
                            CompactPlanLabelGapBars));

                int tagLeftBar =
                    Math.Max(
                        0,
                        tagRightBar -
                        CompactPlanLabelWidthBars +
                        1);

                if (tagRightBar <= tagLeftBar)
                {
                    RemovePlanLabel(name);
                    return;
                }

                double labelPrice =
                    NormalizePrice(price);

                double rangeSum = 0.0;
                int sampleCount = 0;
                int firstSample =
                    Math.Max(
                        0,
                        Bars.Count - 9);

                for (int i = firstSample;
                     i < Bars.Count;
                     i++)
                {
                    double high = Bars.HighPrices[i];
                    double low = Bars.LowPrices[i];

                    if (!double.IsNaN(high) &&
                        !double.IsInfinity(high) &&
                        !double.IsNaN(low) &&
                        !double.IsInfinity(low) &&
                        high >= low)
                    {
                        rangeSum += high - low;
                        sampleCount++;
                    }
                }

                double averageRange =
                    sampleCount > 0
                        ? rangeSum / sampleCount
                        : Symbol.PipSize *
                          CompactPlanLabelMinimumHeightPips;

                double minimumHeight =
                    Symbol.PipSize *
                    CompactPlanLabelMinimumHeightPips;

                double halfHeight =
                    Math.Max(
                        minimumHeight,
                        averageRange *
                        CompactPlanLabelHeightRangeFactor);

                Color labelColor =
                    PlanLinePresentationRule.ResolveColor(
                        semanticColor);

                string boxName =
                    name + "_BOX";

                ChartRectangle box =
                    Chart.FindObject(boxName)
                    as ChartRectangle;

                if (box == null)
                {
                    ChartObject existing =
                        Chart.FindObject(boxName);

                    if (existing != null)
                        Chart.RemoveObject(boxName);

                    box =
                        Chart.DrawRectangle(
                            boxName,
                            tagLeftBar,
                            labelPrice + halfHeight,
                            tagRightBar,
                            labelPrice - halfHeight,
                            labelColor,
                            1,
                            LineStyle.Solid);
                }

                if (box == null)
                    return;

                box.Time1 =
                    Bars.OpenTimes[tagLeftBar];
                box.Y1 =
                    labelPrice + halfHeight;
                box.Time2 =
                    Bars.OpenTimes[tagRightBar];
                box.Y2 =
                    labelPrice - halfHeight;
                box.Color =
                    labelColor;
                box.Thickness =
                    1;
                box.LineStyle =
                    LineStyle.Solid;
                box.IsFilled =
                    true;
                box.IsInteractive =
                    false;

                ChartText label =
                    Chart.FindObject(name)
                    as ChartText;

                int textBar =
                    tagLeftBar +
                    ((tagRightBar - tagLeftBar) / 2);

                if (label == null)
                {
                    ChartObject existing =
                        Chart.FindObject(name);

                    if (existing != null)
                        Chart.RemoveObject(name);

                    label =
                        Chart.DrawText(
                            name,
                            text,
                            Bars.OpenTimes[textBar],
                            labelPrice,
                            Color.White);
                }

                if (label == null)
                    return;

                label.Text = text;
                label.Time = Bars.OpenTimes[textBar];
                label.Y = labelPrice;
                label.Color = Color.White;
                label.FontSize = CompactPlanLabelFontSize;
                label.FontFamily =
                    string.IsNullOrWhiteSpace(PanelFontFamily)
                        ? "Arial"
                        : PanelFontFamily;
                label.IsBold = false;
                label.HorizontalAlignment =
                    HorizontalAlignment.Center;
                label.VerticalAlignment =
                    VerticalAlignment.Center;
                label.IsInteractive = false;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP compact plan label failed: {0}",
                    ex.Message);
            }
        }

        private Color GetReadableLabelTextColor(
            Color semanticColor)
        {
            // Text is always white; its canonical filled background is rendered
            // by DrawCompactPlanLabel using the exact line semantic color.
            return Color.White;
        }

        private void RemovePlanLabel(
            string name)
        {
            Chart.RemoveObject(name);
            Chart.RemoveObject(name + "_BOX");
        }
    }
}