using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanLabelRemover.cs
Single-responsibility plan-label renderer.


namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RemovePlanLabels()
                                {
                                    Chart.RemoveObject(
                                        P + "ENTRY_LABEL");
                                    Chart.RemoveObject(
                                        P + "IDEAL_ENTRY_LABEL");
                                    Chart.RemoveObject(
                                        P + "TRIGGER_LABEL");
                                    Chart.RemoveObject(
                                        P + "SL_LABEL");
                                    Chart.RemoveObject(
                                        P + "TP1_LABEL");
                                    Chart.RemoveObject(
                                        P + "TP2_LABEL");
                                    Chart.RemoveObject(
                                        P + "TP3_LABEL");
                                    Chart.RemoveObject(
                                        P + "TP4_LABEL");
                                    Chart.RemoveObject(
                                        P + "ACTIVE_TP_LABEL");
                                }
    }
}
