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
        private void CreatePanelRows()
                                {
                                    if (_panelRowsStack == null)
                                        return;

                                    _panelRows.Clear();
                                }

        private void EnsurePanelRow(int index)
        {
            if (_panelRowsStack == null ||
                index < 0 ||
                index >= PanelRowCount)
                return;

            while (_panelRows.Count <= index)
            {
                TextBlock row =
                    new TextBlock
                    {
                        Text = "",
                        IsVisible = false,
                        IsHitTestVisible = false,
                        TextWrapping = TextWrapping.Wrap,
                        TextTrimming = TextTrimming.None,
                        TextAlignment = TextAlignment.Left,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                _panelRows.Add(row);
                _panelRowsStack.AddChild(row);
            }
        }
    }
}
