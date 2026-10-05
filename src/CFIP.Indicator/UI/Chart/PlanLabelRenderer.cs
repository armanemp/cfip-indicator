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
                        1,
                        Math.Min(
                            Bars.Count - 1,
                            lineLeftBar));

                DrawCompactPlanLabel(
                    name,
                    text,
                    price,
                    color,
                    GetCompactPlanLabelAnchorTime(
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
            DateTime labelTime)
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
                labelTime);
        }

        private void DrawCompactPlanLabel(
            string name,
            string text,
            double price,
            Color semanticColor,
            DateTime labelTime)
        {
            try
            {
                if (Bars == null ||
                    Bars.Count < 2 ||
                    !IsFinitePositive(price))
                    return;

                if (labelTime == DateTime.MinValue)
                    return;

                double labelPrice =
                    NormalizePrice(price);

                Color labelTextColor =
                    ResolveCanonicalPlanLineColor(
                        semanticColor);

                DateTime expectedTime =
                    labelTime;

                ChartText label =
                    Chart.FindObject(name)
                    as ChartText;

                bool recreate =
                    label == null ||
                    label.Time != expectedTime ||
                    label.HorizontalAlignment != HorizontalAlignment.Left ||
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
                            expectedTime,
                            labelPrice,
                            labelTextColor);
                }

                if (label == null)
                    return;

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
                    HorizontalAlignment.Left;
                label.VerticalAlignment =
                    VerticalAlignment.Center;
                label.IsInteractive =
                    false;

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
