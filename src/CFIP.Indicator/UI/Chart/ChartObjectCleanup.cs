// ============================================================================
// CFIP Indicator — ChartObjectCleanup.cs
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
        private void RemoveAllChartObjects()
                        {
                            RemovePlanObjects();
                            RemoveHistoricalObjects();
                            RemoveManagedPendingOrderObjects();

                            Chart.RemoveObject(
                                P + "ALERT_SIGNAL");
                            RemovePlanLabel(
                                P + "ALERT_SIGNAL_LABEL");
                
                            foreach (string name in _outcomeDrawn)
                                Chart.RemoveObject(name);
                
                            _outcomeDrawn.Clear();
                        }
    }
}
