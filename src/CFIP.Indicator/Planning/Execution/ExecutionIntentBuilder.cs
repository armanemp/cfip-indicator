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
                            string source)
                        {
                            entry = NormalizePrice(entry);
                            stop = NormalizePrice(stop);
                            target = NormalizePrice(target);
                
                            return new ExecutionIntent
                            {
                                Direction = direction,
                                Policy = policy,
                                Kind = kind,
                                RequestedEntry = entry,
                                Trigger = NormalizePrice(trigger),
                                ZoneLow = NormalizePrice(zoneLow),
                                ZoneHigh = NormalizePrice(zoneHigh),
                                Stop = stop,
                                Target = target,
                                StopPips =
                                    IsFinitePositive(entry) &&
                                    IsFinitePositive(stop)
                                        ? Math.Abs(entry - stop) /
                                          Symbol.PipSize
                                        : 0,
                                TargetPips =
                                    IsFinitePositive(entry) &&
                                    IsFinitePositive(target)
                                        ? Math.Abs(target - entry) /
                                          Symbol.PipSize
                                        : 0,
                                Volume = volume,
                                CreatedM5 = closedM5,
                                Source = source
                            };
                        }
    }
}
