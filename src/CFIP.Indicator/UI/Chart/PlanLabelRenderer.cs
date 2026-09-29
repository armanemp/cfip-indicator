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
        private const double CompactPlanLabelFontSize = 9.0;
        private const double CompactPlanLabelAtrHeight = 0.055;

        private void RenderPlanLabel(
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

            DrawPlanLabel(
                name,
                text,
                price,
                color,
                lineLeft,
                labelBar,
                boxRightBar,
                boxHalfHeight);
        }

        private void DrawPlanLabel(
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
                if (!IsFinitePositive(price) ||
                    Bars == null ||
                    Bars.Count < 2)
                {
                    RemovePlanLabel(name);
                    return;
                }

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
                            Bars.OpenTimes[labelBar],
                            labelPrice,
                            color);
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
                    color;
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

                string boxName =
                    name + "_BOX";

                ChartRectangle box =
                    Chart.FindObject(boxName)
                    as ChartRectangle;

                Color boxColor =
                    Color.FromArgb(
                        175,
                        color);

                if (box == null)
                {
                    ChartObject existingBox =
                        Chart.FindObject(boxName);

                    if (existingBox != null)
                        Chart.RemoveObject(boxName);

                    box =
                        Chart.DrawRectangle(
                            boxName,
                            lineLeft,
                            price + boxHalfHeight,
                            boxRightBar,
                            price - boxHalfHeight,
                            boxColor,
                            1,
                            LineStyle.Solid);
                }

                if (box != null)
                {
                    box.Time1 =
                        Bars.OpenTimes[lineLeft];
                    box.Y1 =
                        price + boxHalfHeight;
                    box.Time2 =
                        Bars.OpenTimes[boxRightBar];
                    box.Y2 =
                        price - boxHalfHeight;
                    box.Color =
                        boxColor;
                    box.Thickness =
                        1;
                    box.LineStyle =
                        LineStyle.Solid;
                    box.IsFilled =
                        false;
                    box.IsInteractive =
                        false;
                }
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP plan label failed: {0}",
                    ex.Message);
            }
        }

        private void RemovePlanLabel(
            string name)
        {
            Chart.RemoveObject(name);
            Chart.RemoveObject(name + "_BOX");
        }
    }
}
