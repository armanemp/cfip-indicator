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
        private ExecutionModel BuildExecutionModel(
                                    int closedM5,
                                    int direction)
                                {
                                    ExecutionModel model =
                                        new ExecutionModel
                                        {
                                            Direction = direction,
                                            Mode = ExecutionMode.None,
                                            Source = "NONE"
                                        };

                                    double atr;
                                    double market;
                                    double low;
                                    double high;
                                    double ideal;
                                    double tolerance;
                                    double triggerBuffer;
                                    string source;
                                    int quality;

                                    if (!TryBuildExecutionZone(
                                            closedM5,
                                            direction,
                                            out atr,
                                            out market,
                                            out low,
                                            out high,
                                            out ideal,
                                            out tolerance,
                                            out triggerBuffer,
                                            out source,
                                            out quality))
                                        return model;

                                    model.ZoneLow =
                                        NormalizePrice(low);
                                    model.ZoneHigh =
                                        NormalizePrice(high);
                                    model.ZoneTolerance =
                                        Math.Max(0, tolerance);
                                    model.IdealEntry =
                                        NormalizePrice(ideal);
                                    model.Trigger =
                                        NormalizePrice(
                                            direction == 1
                                                ? high + triggerBuffer
                                                : low - triggerBuffer);
                                    model.Invalidation =
                                        NormalizePrice(
                                            direction == 1
                                                ? low -
                                                  Math.Max(
                                                      StopBufferAtr,
                                                      0.05) *
                                                  atr
                                                : high +
                                                  Math.Max(
                                                      StopBufferAtr,
                                                      0.05) *
                                                  atr);

                                    ApplyExecutionMode(
                                        model,
                                        closedM5,
                                        direction,
                                        market,
                                        low,
                                        high,
                                        ideal,
                                        tolerance,
                                        triggerBuffer,
                                        atr,
                                        quality,
                                        source);

                                    return model;
                                }
    }
}
