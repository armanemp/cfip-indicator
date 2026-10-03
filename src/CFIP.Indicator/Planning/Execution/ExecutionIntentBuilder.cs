using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private ExecutionIntent BuildExecutionIntent(
                            int direction,
                            DecisionPolicyMode policy,
                            ExecutionIntentKind kind,
                            double entry,
                            double trigger,
                            double zoneLow,
                            double zoneHigh,
                            double stop,
                            double target,
                            double volume,
                            int closedM5,
                            string source,
                            bool captureProviderIntent = true)
                        {
                            entry = NormalizePrice(entry);
                            stop = NormalizePrice(stop);
                            target = NormalizePrice(target);

                            ExecutionIntentGeometryResult geometry =
                                ExecutionIntentGeometryRule.Evaluate(
                                    direction,
                                    entry,
                                    stop,
                                    target,
                                    Symbol.PipSize);

                            if (!geometry.Valid)
                                return null;

                            ExecutionIntent intent =
                                new ExecutionIntent
                                {
                                    Direction = direction,
                                    Policy = policy,
                                    Kind = kind,
                                    RequestedEntry = entry,
                                    Trigger = NormalizePrice(trigger),
                                    ZoneLow = NormalizePrice(zoneLow),
                                    ZoneHigh = NormalizePrice(zoneHigh),
                                    Stop = geometry.Stop,
                                    Target = geometry.Target,
                                    StopPips = geometry.StopPips,
                                    TargetPips = geometry.TargetPips,
                                    Volume = volume,
                                    CreatedM5 = closedM5,
                                    Source = source
                                };

                            if (captureProviderIntent)
                                CaptureProviderExecutionIntent(intent);

                            return intent;
                        }
    }
}
