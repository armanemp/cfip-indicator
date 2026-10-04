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
        private const double CompactPlanLabelFontSize = 11.0;

        private void DrawPlanLabel(
            string name,
            string text,
            int lineLeftBar,
            double price,
            Color color)
        {
            try
            {
                if (!IsFinitePositive(price) ||
                    Bars == null ||
                    Bars.Count < 2)
                    return;

                int canonicalLineLeftBar =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            lineLeftBar));

                DrawCompactPlanLabel(
                    name,
                    text,
                    price,
                    color,
                    GetCompactPlanLabelAnchorBar(
                        canonicalLineLeftBar));
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

                // The canonical line renderer owns the line geometry.
                // This renderer owns only the native ChartText object.
                // labelBar is the RIGHT edge anchor of the visible text.
                // PlanLabelAnchorCalculator owns the exact one-bar gap.
                int textBar =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            labelBar));

                double labelPrice =
                    NormalizePrice(price);

                Color labelTextColor =
                    ResolveCanonicalPlanLineColor(
                        semanticColor);

                ChartText label =
                    Chart.FindObject(name)
                    as ChartText;

                DateTime expectedTime =
                    Bars.OpenTimes[textBar];

                // X geometry has one source of truth: the bar-index anchor. Recreate
                // the same named ChartText only when its current anchor or visual
                // contract is stale. The replacement is still created exclusively
                // through the bar-index DrawText overload.
                bool recreate =
                    label == null ||
                    (label != null &&
                     label.Time != expectedTime) ||
                    label.HorizontalAlignment != HorizontalAlignment.Right ||
                    label.VerticalAlignment != VerticalAlignment.Center ||
                    label.FontSize != CompactPlanLabelFontSize ||
                    label.IsBold;

                if (recreate)
                {
                    if (label != null)
                        Chart.RemoveObject(name);

                    ChartObject existing =
                        Chart.FindObject(name);

                    if (existing != null)
                        Chart.RemoveObject(name);

                    label =
                        Chart.DrawText(
                            name,
                            text,
                            textBar,
                            labelPrice,
                            labelTextColor);
                }

                if (label == null)
                    return;

                // Do not assign label.Time here. A stale X anchor is corrected by
                // recreating the same named ChartText through the bar-index overload;
                // the Time property is never used as a second movement path.
                label.Text =
                    text;
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
                    HorizontalAlignment.Right;
                label.VerticalAlignment =
                    VerticalAlignment.Center;
                label.IsInteractive =
                    false;

                // Remove every legacy companion shape deterministically.
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

        private void RemovePlanLabel(
            string name)
        {
            Chart.RemoveObject(name);
            Chart.RemoveObject(name + "_BOX");
            Chart.RemoveObject(name + "_ANCHOR");
        }
    }
}
