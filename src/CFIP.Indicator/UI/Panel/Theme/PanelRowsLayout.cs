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
        private void ApplyPanelRowsLayout(
                                                    int contentWidth)
                                                {
                                                    _buttonStack.IsVisible =
                                                        buttons;
                                        
                                                    for (int i = 0;
                                                         i < _panelRows.Count;
                                                         i++)
                                                    {
                                                        TextBlock row =
                                                            _panelRows[i];
                                        
                                                        row.Width =
                                                            Math.Max(
                                                                190,
                                                                contentWidth);
                                        
                                                        row.FontSize =
                                                            Math.Max(
                                                                8,
                                                                PanelFontSize);
                                        
                                                        row.FontFamily =
                                                            string.IsNullOrWhiteSpace(
                                                                PanelFontFamily)
                                                                ? "Arial"
                                                                : PanelFontFamily;
                                        
                                                        row.LineHeight =
                                                            Math.Max(
                                                }
    }
}
