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
        private void RenderLatestAlertSignalMarker(
            int fallbackM5)
        {
            // Canonical signal arrows come from SignalRenderer/SignalVisualSnapshot.
            // This legacy alert-mirror surface must never create a second arrow or
            // an "ALERT BUY/SELL" label beside signal levels.
            Chart.RemoveObject(P + "ALERT_SIGNAL");
            RemovePlanLabel(P + "ALERT_SIGNAL_LABEL");
        }
    }
}
