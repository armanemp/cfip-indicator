// ============================================================================
// CFIP Indicator — ExecutionModelBuilder.cs
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
        private void ApplyExecutionMode(
                                    ExecutionModel model,
                                    int direction,
                                    double market,
                                    double low,
                                    double high,
                                    double ideal,
                                    double tolerance,
                                    double triggerBuffer,
                                    double atr,
                                    int quality,
                                    string source)
                                {
                                    if (triggerReached &&
                                        AllowPrecisionBreakoutEntry)
                                    {
                                        model.Mode =
                                            ExecutionMode.BreakoutMarket;
                        
                                        model.ActualEntry =
                                            market;
                        
                                        model.Ready =
                                            qualityReady &&
                                            Math.Abs(
                                                market -
                                                model.Trigger) <=
                                            atr *
                                            Math.Max(
                                                0.10,
                                                MaximumEntryExtensionAtr);
                                    }
                                    else if (continuation)
                                    {
                                        model.Mode =
                                            ExecutionMode.WaitingForTrigger;
                        
                                        model.ActualEntry = 0;
                                        model.Ready = false;
                                    }
                                    else if (retestReady)
                                    {
                                        model.Mode =
                                            ExecutionMode.RetestMarket;
                        
                                        model.ActualEntry =
                                            market;
                        
                                        model.Ready =
                                            qualityReady &&
                                            Math.Abs(
                                                market -
                                                model.IdealEntry) <=
                                            atr *
                                            Math.Max(
                                                0.05,
                                                MaximumEntryDistanceAtr) &&
                                            (retest >= MinimumRetestQuality ||
                                             !RequireRetestQuality);
                                    }
                                    else
                                    {
                                        model.Mode =
                                            ExecutionMode.None;
                        
                                        model.ActualEntry = 0;
                                        model.Ready = false;
                                    }
                        
                                    model.Quality = quality;
                                    model.Source = source;
                        
                                    if (model.Mode ==
                                        ExecutionMode.WaitingForTrigger)
                                        model.Source += "+WAIT";
                                    else if (model.Mode ==
                                             ExecutionMode.RetestMarket)
                                        model.Source += "+RETEST";
                                    else if (model.Mode ==
                                             ExecutionMode.BreakoutMarket)
                                        model.Source += "+EXEC";
                        
                                }
    }
}
