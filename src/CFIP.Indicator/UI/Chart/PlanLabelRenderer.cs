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
                            color);
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
                    color;
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

        private void RenderCompactPlanLabel(
            string name,
            string text,
            double price,
            Color color,
            bool visible,
            int lineLeft,
            int labelBar,
            int boxRightBar,
            double boxHalfHeight)
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
                lineLeft,
                labelBar,
                boxRightBar,
                boxHalfHeight);
        }

        private void DrawCompactPlanLabel(
            string name,
            string text,
            double price,
            Color color,
            int lineLeft,
            int labelBar,
            int boxRightBar,
            double boxHalfHeight)
        {
            try
            {
                if (Bars == null ||
                    Bars.Count < 2 ||
                    !IsFinitePositive(price))
                    return;

                double labelPrice =
                    NormalizePrice(price);

                int safeLineLeft =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            lineLeft));

                int safeLabelBar =
                    Math.Max(
                        safeLineLeft,
                        Math.Min(
                            Bars.Count - 1,
                            labelBar));

                int safeBoxLeft =
                    Math.Max(
                        safeLineLeft,
                        safeLabelBar - 1);

                int safeBoxRight =
                    Math.Max(
                        safeBoxLeft,
                        Math.Min(
                            Bars.Count - 1,
                            boxRightBar));

                double effectiveHalfHeight =
                    Math.Max(
                        Symbol.PipSize * 1.5,
                        Math.Min(
                            Symbol.PipSize * 4.0,
                            Math.Abs(boxHalfHeight)));

                string boxName =
                    name + "_BOX";

                // Draw/update the box first so the label text remains visually
                // above it regardless of chart-object paint order.
                ChartRectangle box =
                    Chart.FindObject(boxName)
                    as ChartRectangle;

                // Preserve the semantic level color while forcing the
                // rectangle fill itself to be fully opaque.
                Color boxColor = color;
                Color opaqueBoxColor =
                    Color.FromArgb(
                        255,
                        boxColor);

                Color textColor =
                    GetReadableLabelTextColor(
                        boxColor);

                if (box == null)
                {
                    ChartObject existingBox =
                        Chart.FindObject(boxName);

                    if (existingBox != null)
                        Chart.RemoveObject(boxName);

                    box =
                        Chart.DrawRectangle(
                            boxName,
                            safeBoxLeft,
                            price + effectiveHalfHeight,
                            safeBoxRight,
                            price - effectiveHalfHeight,
                            color,
                            1,
                            LineStyle.Solid);
                }

                if (box != null)
                {
                    box.Time1 =
                        Bars.OpenTimes[safeBoxLeft];
                    box.Y1 =
                        NormalizePrice(
                            price + effectiveHalfHeight);
                    box.Time2 =
                        Bars.OpenTimes[safeBoxRight];
                    box.Y2 =
                        NormalizePrice(
                            price - effectiveHalfHeight);
                    box.Color =
                        opaqueBoxColor;
                    box.Thickness =
                        1;
                    box.LineStyle =
                        LineStyle.Solid;
                    box.IsFilled =
                        true;
                    box.IsInteractive =
                        false;
                }

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
                            Bars.OpenTimes[labelBar],
                            labelPrice,
                            textColor);
                }

                if (label == null)
                    return;

                label.Text =
                    text;
                label.Time =
                    Bars.OpenTimes[safeLabelBar];
                label.Y =
                    labelPrice;
                label.Color =
                    textColor;
                label.FontSize =
                    CompactPlanLabelFontSize;
                label.FontFamily =
                    string.IsNullOrWhiteSpace(
                        PanelFontFamily)
                        ? "Arial"
                        : PanelFontFamily;
                label.IsBold =
                    true;
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
                    "CFIP compact plan label failed: {0}",
                    ex.Message);
            }
        }

        private Color GetReadableLabelTextColor(
            Color background)
        {
            double luminance =
                0.299 * background.R +
                0.587 * background.G +
                0.114 * background.B;

            return luminance >= 160
                ? Color.Black
                : Color.White;
        }

        private void RemovePlanLabel(
            string name)
        {
            Chart.RemoveObject(name);
            Chart.RemoveObject(name + "_BOX");
        }
    }
}
