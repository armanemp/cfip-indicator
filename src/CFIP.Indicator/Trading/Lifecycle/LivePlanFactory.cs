// CFIP Indicator — LivePlanFactory.cs
// Single-responsibility lifecycle module.

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
        private Plan CreateManagedPlanFromExecution(
                            int direction,
                            double entry,
                            double stop,
                            double target,
                            int createdM5,
                            double volume,
                            ExecutionMode entryMode =
                                ExecutionMode.BreakoutMarket,
                            bool bindSignalTrace = false)
                        {
                            RiskRewardMathResult geometry =
                                RiskRewardMathRule.Evaluate(
                                    direction,
                                    entry,
                                    stop,
                                    target,
                                    0,
                                    0,
                                    MaximumRewardRR,
                                    Symbol.PipSize);

                            if (!geometry.Valid)
                                return null;

                            Plan plan =
                                new Plan
                                {
                                    Direction = direction,
                                    EntryMode = entryMode,
                                    Entry = NormalizePrice(entry),
                                    IdealEntry = NormalizePrice(entry),
                                    Stop = NormalizePrice(stop),
                                    Tp1 = NormalizePrice(target),
                                    Tp2 = 0,
                                    Tp3 = 0,
                                    Tp4 = 0,
                                    Risk = geometry.Risk,
                                    Tp1RR = geometry.NominalRR,
                                    StopSource = "LIVE / STRUCTURAL",
                                    StopQuality = 100,
                                    Tp1Source = "LIVE / ADAPTIVE",
                                    Tp1Quality = 100,
                                    CreatedM5 = createdM5,
                                    OriginalVolume = volume,
                                    CalibrationEligible = false,
                                    IsLivePosition = true
                                };

                            if (bindSignalTrace)
                            {
                                plan.SignalBarOpenTimeUtcTicks =
                                    GetSignalBarOpenTimeUtcTicks(
                                        createdM5);
                                plan.SignalTraceId =
                                    BuildSignalTraceId(
                                        createdM5);
                            }

                            return plan;
                        }
    }
}
