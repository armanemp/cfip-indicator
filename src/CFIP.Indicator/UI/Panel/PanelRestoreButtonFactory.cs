// CFIP Indicator — PanelRestoreButtonFactory.cs
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
        private void CreatePanelRestoreButton()
                                {
                                    if (_panelRestoreButton != null)
                                        return;

                                    try
                                    {
                                        _panelRestoreButton =
                                            new Button
                                            {
                                                Text = "+",
                                                Width = 26,
                                                Height = 26,
                                                HorizontalAlignment =
                                                    HorizontalAlignment.Left,
                                                VerticalAlignment =
                                                    VerticalAlignment.Bottom,
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
                                                        70,
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
                                                IsVisible = false
                                            };

                                        _panelRestoreButton.Click +=
                                            args => TogglePanel();

                                        Chart.AddControl(
                                            _panelRestoreButton);

                                        SetPanelRestoreAlignment();
                                    }
                                    catch (Exception ex)
                                    {
                                        Print(
                                            "CFIP panel restore button failed: {0}",
                                            ex.Message);

                                        _panelRestoreButton = null;
                                    }
                                }
    }
}
