using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanLineRemover.cs
// Single-responsibility chart plan renderer.

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RemovePlanLine(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            Chart.RemoveObject(name);

            // A plan level line and its label are one visual lifecycle.
            // Remove the label through its canonical owner whenever the
            // corresponding line is removed, regardless of which line
            // cleanup path requested the removal.
            if (name == P + "ENTRY")
                RemovePlanLabel(P + "ENTRY_LABEL");
            else if (name == P + "IDEAL_ENTRY")
                RemovePlanLabel(P + "IDEAL_ENTRY_LABEL");
            else if (name == P + "TRIGGER")
                RemovePlanLabel(P + "TRIGGER_LABEL");
            else if (name == P + "SL")
                RemovePlanLabel(P + "SL_LABEL");
            else if (name == P + "TP1")
                RemovePlanLabel(P + "TP1_LABEL");
            else if (name == P + "TP2")
                RemovePlanLabel(P + "TP2_LABEL");
            else if (name == P + "TP3")
                RemovePlanLabel(P + "TP3_LABEL");
            else if (name == P + "TP4")
                RemovePlanLabel(P + "TP4_LABEL");
            else if (name == P + "ACTIVE_TP")
                RemovePlanLabel(P + "ACTIVE_TP_LABEL");
        }
    }
}