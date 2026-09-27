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
                        
                                        int labelBar =
                                            GetLabelAnchorBar(
                                                name,
                                                Math.Max(
                                                    0,
                                                    Math.Min(
                                                        Bars.Count - 1,
                                                        bar)));
                        
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
                        
                                        double labelOffset =
                                            Math.Max(
                                                Symbol.PipSize * 2,
                                                atr > 0
                                                    ? atr * 0.06
                                                    : Symbol.PipSize * 3);
                        
                                        double labelPrice =
                                            NormalizePrice(
                                                price +
                                                labelOffset);
                        
                                        ChartText label =
                                            Chart.DrawText(
                                                name,
                                                text,
                                                Bars.OpenTimes[labelBar],
                                                labelPrice,
                                                color);
                        
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
    }
}
