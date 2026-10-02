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
        private void RenderPanelRows(
            int contentWidth)
        {
            int slot = 0;

            RenderPanelOverviewRows(
                ref slot,
                contentWidth);

            RenderPanelDecisionRows(
                ref slot,
                contentWidth);

            RenderPanelExecutionRows(
                ref slot,
                contentWidth);

            RenderPanelTradePlanRows(
                ref slot,
                contentWidth);

            RenderPanelContextRows(
                ref slot,
                contentWidth);

                                                while (slot < _panelRows.Count)
                                                {
                                                    _panelRows[slot].IsVisible =
                                                        false;
                                                    slot++;
                                                }
                                            
        }
    }
