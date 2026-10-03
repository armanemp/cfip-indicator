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
        private const int CompactPlanLabelGapBars = 1;

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

                // Prediction lines use their own configured backward span.
                // Convert the prediction anchor into the exact line-left
                // position, then delegate to the same compact tag renderer
                // used by canonical plan/pending/parallel labels.
                int lineLeft =
                    Math.Max(
                        0,
                        anchor -
                        Math.Max(
                            1,
                            LineLengthBars));

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

                // Native cTrader-style presentation:
                // the line owns the geometry; the label is only ChartText.
                // Do not create a second chart shape/rectangle. Rectangle
                // objects are price/time geometry and become visually large
                // or unstable when zoom/scale changes.
                int lineLeftBar =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            labelBar));

                int textBar =
                    Math.Max(
                        0,
                        lineLeftBar - CompactPlanLabelGapBars);

                double labelPrice =
                    NormalizePrice(price);

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
                            Color.White);
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
                    Color.White;
                label.FontSize =
                    CompactPlanLabelFontSize;
                label.FontFamily =
                    string.IsNullOrWhiteSpace(
                        PanelFontFamily)
                        ? "Arial"
                        : PanelFontFamily;
                label.IsBold =
                    false;

                // Right-align against the anchor so the complete text stays
                // on the label side of the line and never crosses its start.
                label.HorizontalAlignment =
                    HorizontalAlignment.Right;
                label.VerticalAlignment =
                    VerticalAlignment.Center;
                label.IsInteractive =
                    false;

                // Remove legacy rectangle objects left by previous versions.
                Chart.RemoveObject(name + "_BOX");
            Chart.RemoveObject(name + "_ANCHOR");

                // A tiny native circle marks the exact line/label junction.
                // It is one chart object, not a filled panel/box.
                string markerName =
                    name + "_ANCHOR";

                ChartIcon marker =
                    Chart.FindObject(markerName)
                    as ChartIcon;

                if (marker == null)
                {
                    ChartObject existingMarker =
                        Chart.FindObject(markerName);

                    if (existingMarker != null)
                        Chart.RemoveObject(markerName);

                    marker =
                        Chart.DrawIcon(
                            markerName,
                            ChartIconType.Circle,
                            Bars.OpenTimes[lineLeftBar],
                            labelPrice,
                            labelColor);
                }

                if (marker != null)
                {
                    marker.Time =
                        Bars.OpenTimes[lineLeftBar];
                    marker.Y =
                        labelPrice;
                    marker.Color =
                        labelColor;
                    marker.IconType =
                        ChartIconType.Circle;
                    marker.IsInteractive =
                        false;
                }
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