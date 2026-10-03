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

            UpsertPlanLabel(
                name,
                text,
                safeBar,
                price,
                color);
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
            if (!IsFinitePositive(price) ||
                Bars == null ||
                Bars.Count < 2)
                return;

            int safeLabelBar =
                Math.Max(
                    0,
                    Math.Min(
                        Bars.Count - 1,
                        labelBar));

            UpsertPlanLabel(
                name,
                text,
                safeLabelBar,
                price,
                semanticColor);
        }

        private void UpsertPlanLabel(
            string name,
            string text,
            int labelBar,
            double price,
            Color semanticColor)
        {
            try
            {
                if (Bars == null ||
                    Bars.Count < 2 ||
                    !IsFinitePositive(price))
                    return;

                double labelPrice =
                    NormalizePrice(price);

                if (!IsFinitePositive(labelPrice))
                    return;

                Color labelTextColor =
                    GetReadableLabelTextColor(
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
                            Bars.OpenTimes[labelBar],
                            labelPrice,
                            labelTextColor);
                }

                if (label == null)
                    return;

                label.Text =
                    text;
                label.Time =
                    Bars.OpenTimes[labelBar];
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
                    true;

                // cTrader ChartText alignment is relative to the object's
                // anchor point. The anchor is deliberately placed before the
                // 40-bar line start; Right alignment makes the visible text
                // extend further left, guaranteeing the annotation stays on
                // the left side rather than growing back toward the line.
                label.HorizontalAlignment =
                    HorizontalAlignment.Right;
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

        private Color GetReadableLabelTextColor(
            Color semanticColor)
        {
            // Compact level labels have no background; the current
            // presentation contract uses white text for every signal level.
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
