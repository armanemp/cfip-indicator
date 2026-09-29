using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanLabelRemover.cs
// Single-responsibility plan-label renderer.

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RemovePlanLabels()
        {
            RemovePlanLabel(P + "ENTRY_LABEL");
            RemovePlanLabel(P + "IDEAL_ENTRY_LABEL");
            RemovePlanLabel(P + "TRIGGER_LABEL");
            RemovePlanLabel(P + "SL_LABEL");
            RemovePlanLabel(P + "TP1_LABEL");
            RemovePlanLabel(P + "TP2_LABEL");
            RemovePlanLabel(P + "TP3_LABEL");
            RemovePlanLabel(P + "TP4_LABEL");
            RemovePlanLabel(P + "ACTIVE_TP_LABEL");
        }
    }
}
