// ============================================================================
// CFIP Indicator — UI/Chart/PredictionRenderer.cs
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
        private void RemovePredictionObjects()
                                {
                                    Chart.RemoveObject(
                                        P + "PRED_ZONE");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_ENTRY");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_ENTRY_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_STOP");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_STOP_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TRIGGER");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TRIGGER_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET1");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET1_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET2");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET2_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET3");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET3_LABEL");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET4");
                        
                                    Chart.RemoveObject(
                                        P + "PRED_TARGET4_LABEL");
                                }
    }
    }
}
