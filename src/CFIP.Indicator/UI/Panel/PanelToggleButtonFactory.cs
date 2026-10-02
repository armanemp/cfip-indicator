// ============================================================================
// CFIP Indicator — PanelFactory.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void CreatePanelToggleButton()
                                {
                                    if (!ShowPanelToggleButton ||
                                        _panelToggleButton != null)
                                        return;
                        
                                    try
                                    {
                                        _panelToggleButton =
                                            new Button
                                            {
                                                Text = "−",
                                                Width =
                                                    Math.Max(
                                                        22,
                                                        Math.Min(
                                                            40,
                                                            PanelToggleWidth),
                                                Height =
                                                    Math.Max(
                                                        22,
                                                        Math.Min(
                                                            40,
                                                            PanelToggleWidth),
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Left,
                                                VerticalAlignment =
                                                    VerticalAlignment.Center,
                                                HorizontalContentAlignment =
                                                    HorizontalAlignment.Center,
                                                VerticalContentAlignment =
                                                    VerticalAlignment.Center,
                                                ForegroundColor =
                                                    PanelTextColor,
                                                FontSize =
                                                    Math.Max(
                                                        9,
                                                        PanelFontSize),
                                                FontWeight =
                                                    FontWeight.Bold,
                                                BackgroundColor =
                                                    Color.FromArgb(
                                                        100,
                                                        Color.Black),
                                                BorderColor =
                                                    Color.FromArgb(
                                                        Math.Max(
                                                            0,
                                                            Math.Min(
                                                                255,
                                                                PanelBorderAlpha)),
                                                        PanelBorder),
                                                BorderThickness =
                                                    Math.Max(
                                                        0,
                                                        PanelBorderThickness),
                                                CornerRadius =
                                                    Math.Min(
                                                        5,
                                                        Math.Max(
                                                            0,
                                                            PanelCornerRadius)),
                                                Margin =
                                                    new Thickness(
                                                        0,
                                                        1,
                                                        8,
                                                        1)
                                            };
                        
                                        _panelToggleButton.Click +=
                                            args => TogglePanel();
                                    }
                                    catch (Exception ex)
                                    {
                                        Print(
                                            "CFIP panel hide button failed: {0}",
                                            ex.Message);
                        
                                        _panelToggleButton = null;
                                    }
                                }
    }
}
