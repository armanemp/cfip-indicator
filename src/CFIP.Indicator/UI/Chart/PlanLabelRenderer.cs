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
        // Canonical presentation contract: native text, line semantic color, exact 2-pip clearance, no marker/box.
        private const double CompactPlanLabelFontSize = 8.5;
        private const double CompactPlanLabelGapPips = 2.0;

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

                int anchor =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            bar));

                int lineLeft =
                    GetPlanLineLeftBar();

                DrawCompactPlanLabel(
                    name,
                    text,
                    price,
                    color,
                    lineLeft);
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

                // The line owns geometry. The label is only native ChartText.
                // No rectangle, panel or marker is created for the label.
                int lineLeftBar =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            labelBar));

                int textBar =
                    lineLeftBar;

                double labelPrice =
                    NormalizePrice(
                        price +
                        (CompactPlanLabelGapPips * Symbol.PipSize));

                Color labelColor =
                    PlanLinePresentationRule.ResolveColor(
                        semanticColor);

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
                            Bars.OpenTimes[textBar],
                            labelPrice,
                            labelColor);
                }

                if (label == null)
                    return;

                label.Text =
                    text;
                label.Time =
                    Bars.OpenTimes[textBar];
                label.Y =
                    labelPrice;
                label.Color =
                    GetReadableLabelTextColor(labelColor);
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
                    HorizontalAlignment.Right;
                label.VerticalAlignment =
                    VerticalAlignment.Center;
                label.IsInteractive =
                    false;

                // Clean up every legacy label shape deterministically.
                Chart.RemoveObject(name + "_BOX");
                Chart.RemoveObject(name + "_ANCHOR");
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP native plan label failed: {0}",
                    ex.Message);
            }
        }

        private Color GetReadableLabelTextColor(
            Color semanticColor)
        {
            // Label text intentionally matches the canonical line color.
            return PlanLinePresentationRule.ResolveColor(
                semanticColor);
        }

        private void RemovePlanLabel(
            string name)
        {
            Chart.RemoveObject(name);
            Chart.RemoveObject(name + "_BOX");
            Chart.RemoveObject(name + "_ANCHOR");
        }
    }
}