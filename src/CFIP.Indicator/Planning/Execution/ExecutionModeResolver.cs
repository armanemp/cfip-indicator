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
                                    int closedM5,
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
                                    int retest =
                                        RetestQuality(
                                            _m5Bars,
                                            closedM5,
                                            direction);

                                    EntryGeometrySnapshot geometry =
                                        EntryGeometryRule.Evaluate(
                                            direction,
                                            ExecutionMode.None,
                                            market,
                                            low,
                                            high,
                                            tolerance,
                                            ideal,
                                            model.Trigger,
                                            0,
                                            atr,
                                            Symbol.TickSize,
                                            Symbol.PipSize,
                                            AllowPrecisionBreakoutEntry,
                                            IsContinuationExecutionContext(direction),
                                            MaximumEntryExtensionAtr,
                                            MaximumEntryDistanceAtr);

                                    bool qualityReady =
                                        !RequirePrecisionEntry ||
                                        quality >=
                                        Math.Max(
                                            EntryActionabilityPolicy.ExecutionZoneQualityFloor,
                                            MinimumEntryQuality);

                                    model.ZoneTolerance =
                                        geometry.ZoneTolerance;
                                    model.Mode =
                                        geometry.Mode;
                                    model.ActualEntry =
                                        geometry.ActualEntry;
                                    model.IsLate =
                                        geometry.IsLate;
                                    model.Ready =
                                        qualityReady &&
                                        (!AvoidLateEntry ||
                                         !geometry.IsLate) &&
                                        ((geometry.Mode ==
                                          ExecutionMode.BreakoutMarket) ||
                                         (geometry.Mode ==
                                          ExecutionMode.RetestMarket &&
                                          (retest >= MinimumRetestQuality ||
                                           !RequireRetestQuality)));
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
