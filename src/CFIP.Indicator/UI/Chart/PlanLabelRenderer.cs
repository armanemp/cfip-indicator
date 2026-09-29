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
            bool visible)
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
                color);
        }

        private void DrawPlanLabel(
            string name,
            string text,
            double price,
            Color color)
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

                int labelBar =
                    GetLabelAnchorBar(
                        name,
                        GetCompactPlanLineLeftBar());

                int boxRightBar =
                    GetLabelBoxRightBar();

                double atr =
                    Bars.Count >= 3
                        ? Atr(
                            Bars,
                            Math.Max(
                                1,
                                Math.Min(
                                    Bars.Count - 2,
                                    labelBar)))
                        : 0;

                double boxHalfHeight =
                    Math.Max(
                        Symbol.PipSize * 3,
                        atr > 0
                            ? atr * CompactPlanLabelAtrHeight
                            : Symbol.PipSize * 4);

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

                if (box == null)
                {
                    ChartObject existingBox =
                        Chart.FindObject(boxName);

                    if (existingBox != null)
                        Chart.RemoveObject(boxName);

                    box =
                        Chart.DrawRectangle(
                            boxName,
                            GetCompactPlanLineLeftBar(),
                            price + boxHalfHeight,
                            boxRightBar,
                            price - boxHalfHeight,
                            Color.FromArgb(
                                175,
                                color),
                            1,
                            LineStyle.Solid);
                }

                if (box != null)
                {
                    box.Time1 =
                        Bars.OpenTimes[
                            GetCompactPlanLineLeftBar()];
                    box.Y1 =
                        price + boxHalfHeight;
                    box.Time2 =
                        Bars.OpenTimes[boxRightBar];
                    box.Y2 =
                        price - boxHalfHeight;
                    box.Color =
                        Color.FromArgb(
                            175,
                            color);
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
