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
        private const double CompactPlanLabelHeight = 20.0;
        private const double CompactPlanLabelMinWidth = 82.0;
        private const double CompactPlanLabelMaxWidth = 180.0;

        private readonly Dictionary<string, Border> _planLabelControls =
            new Dictionary<string, Border>(StringComparer.Ordinal);

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
                    !IsFinitePositive(price) ||
                    labelTime == DateTime.MinValue)
                    return;

                string safeText =
                    string.IsNullOrWhiteSpace(text)
                        ? "--"
                        : text.Trim();

                double labelPrice =
                    NormalizePrice(price);

                Color labelTextColor =
                    ResolveCanonicalPlanLineColor(
                        semanticColor);

                Border box;
                if (!_planLabelControls.TryGetValue(
                        name,
                        out box) ||
                    box == null)
                {
                    Chart.RemoveObject(name);

                    box =
                        new Border
                        {
                            Height = CompactPlanLabelHeight,
                            BackgroundColor =
                                Color.FromArgb(
                                    55,
                                    labelTextColor),
                            BorderColor =
                                labelTextColor,
                            BorderThickness =
                                new Thickness(1),
                            CornerRadius =
                                new CornerRadius(3),
                            Padding =
                                new Thickness(
                                    5,
                                    1,
                                    5,
                                    1),
                            HorizontalAlignment =
                                HorizontalAlignment.Right,
                            VerticalAlignment =
                                VerticalAlignment.Center,
                            IsHitTestVisible = false,
                            Child =
                                new TextBlock
                                {
                                    Height =
                                        CompactPlanLabelHeight - 2,
                                    FontFamily =
                                        string.IsNullOrWhiteSpace(
                                            PanelFontFamily)
                                            ? "Arial"
                                            : PanelFontFamily,
                                    FontSize =
                                        CompactPlanLabelFontSize,
                                    FontWeight =
                                        FontWeight.Normal,
                                    ForegroundColor =
                                        labelTextColor,
                                    TextAlignment =
                                        TextAlignment.Left,
                                    VerticalAlignment =
                                        VerticalAlignment.Center,
                                    HorizontalAlignment =
                                        HorizontalAlignment.Left,
                                    TextWrapping =
                                        TextWrapping.NoWrap,
                                    TextTrimming =
                                        TextTrimming.CharacterEllipsis,
                                    IsHitTestVisible = false,
                                    BackgroundColor =
                                        Color.FromArgb(
                                            0,
                                            Color.Black)
                                }
                        };

                    _planLabelControls[name] = box;
                    Chart.AddControl(
                        box,
                        labelTime,
                        labelPrice);
                }
                else
                {
                    Chart.MoveControl(
                        box,
                        labelTime,
                        labelPrice);
                }

                TextBlock label =
                    box.Child as TextBlock;

                if (label == null)
                    return;

                double desiredWidth =
                    Math.Max(
                        CompactPlanLabelMinWidth,
                        Math.Min(
                            CompactPlanLabelMaxWidth,
                            16.0 +
                            safeText.Length * 7.0));

                box.Width =
                    desiredWidth;
                box.Height =
                    CompactPlanLabelHeight;
                box.BackgroundColor =
                    Color.FromArgb(
                        55,
                        labelTextColor);
                box.BorderColor =
                    labelTextColor;
                box.BorderThickness =
                    new Thickness(1);
                box.CornerRadius =
                    new CornerRadius(3);
                box.IsHitTestVisible =
                    false;
                box.HorizontalAlignment =
                    HorizontalAlignment.Right;
                box.VerticalAlignment =
                    VerticalAlignment.Center;

                label.Text =
                    safeText;
                label.Width =
                    desiredWidth - 10.0;
                label.Height =
                    CompactPlanLabelHeight - 2.0;
                label.ForegroundColor =
                    labelTextColor;
                label.FontSize =
                    CompactPlanLabelFontSize;
                label.FontFamily =
                    string.IsNullOrWhiteSpace(
                        PanelFontFamily)
                        ? "Arial"
                        : PanelFontFamily;
                label.FontWeight =
                    FontWeight.Normal;
                label.TextWrapping =
                    TextWrapping.NoWrap;
                label.TextTrimming =
                    TextTrimming.Ellipsis;
                label.TextAlignment =
                    TextAlignment.Left;
                label.VerticalAlignment =
                    VerticalAlignment.Center;
                label.HorizontalAlignment =
                    HorizontalAlignment.Left;
                label.BackgroundColor =
                    Color.FromArgb(
                        0,
                        Color.Black);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP boxed plan label failed: {0}",
                    ex.Message);
            }
        }

        private void RemovePlanLabel(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            Border box;
            if (_planLabelControls.TryGetValue(
                    name,
                    out box))
            {
                try
                {
                    if (box != null)
                        Chart.RemoveControl(box);
                }
                catch (Exception ex)
                {
                    Print(
                        "CFIP plan label control removal failed: {0}",
                        ex.Message);
                }

                _planLabelControls.Remove(name);
            }

            Chart.RemoveObject(name);
            Chart.RemoveObject(name + "_BOX");
            Chart.RemoveObject(name + "_ANCHOR");
        }

        private void RemoveAllPlanLabelControls()
        {
            if (_planLabelControls.Count == 0)
                return;

            List<Border> controls =
                new List<Border>(
                    _planLabelControls.Values);

            for (int i = 0; i < controls.Count; i++)
            {
                Border box = controls[i];
                if (box == null)
                    continue;

                try
                {
                    Chart.RemoveControl(box);
                }
                catch (Exception ex)
                {
                    Print(
                        "CFIP plan label bulk removal failed: {0}",
                        ex.Message);
                }
            }

            _planLabelControls.Clear();
        }
    }
}
