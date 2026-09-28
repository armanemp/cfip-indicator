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
        private void ApplyPanelRestoreButtonLayout(
                                                    int borderAlpha,
                                                    int border)
                                                {
                                                    if (_panelRestoreButton != null)
                                                    {
                                                        _panelRestoreButton.IsVisible =
                                                            _panelHidden;
                                        
                                                        _panelRestoreButton.Width = 26;
                                                        _panelRestoreButton.Height = 26;
                                                        _panelRestoreButton.ForegroundColor =
                                                            PanelTextColor;
                                                        _panelRestoreButton.FontSize =
                                                            Math.Max(
                                                                9,
                                                                PanelFontSize);
                                                        _panelRestoreButton.BackgroundColor =
                                                            Color.FromArgb(
                                                                ShowPanelBackground
                                                                    ? 70
                                                                    : 0,
                                                                Color.Black);
                                                        _panelRestoreButton.BorderColor =
                                                            Color.FromArgb(
                                                                borderAlpha,
                                                                PanelBorder);
                                                        _panelRestoreButton.BorderThickness =
                                                            border;
                                                        _panelRestoreButton.CornerRadius =
                                                            Math.Min(
                                                                5,
                                                                Math.Max(
                                                                    0,
                                                                    PanelCornerRadius));
                                        
                                                        SetPanelRestoreAlignment();
                                                    }
                                        
                                                }
    }
}
