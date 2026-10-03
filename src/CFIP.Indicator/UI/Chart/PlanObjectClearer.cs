using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanObjectClearer.cs
// Single-responsibility chart plan renderer.


namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ClearPlanObjects()
                                {
                                    ClearWatchObjects();
                                    RemovePlanLabels();
                        
                                    RemovePlanLine(
                                        P + "ENTRY");
                        
                                    RemovePlanLine(
                                        P + "IDEAL_ENTRY");
                        
                                    RemovePlanLine(
                                        P + "TRIGGER");
                        
                                    RemovePlanLine(
                                        P + "SL");
                        
                                    RemovePlanLine(
                                        P + "TP1");
                        
                                    RemovePlanLine(
                                        P + "TP2");
                        
                                    RemovePlanLine(
                                        P + "TP3");
                        
                                    RemovePlanLine(
                                        P + "TP4");
                        
                                    RemovePlanLine(
                                        P + "ACTIVE_TP");
                        
                                    RemoveMtfTrendStrengthArrowStack();

                                    Chart.RemoveObject(
                                        P + "ARROW");
                                }
    }
}
