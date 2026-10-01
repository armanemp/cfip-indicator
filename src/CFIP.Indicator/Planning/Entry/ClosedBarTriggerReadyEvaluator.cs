// CFIP Indicator — ClosedBarTriggerReadyEvaluator.cs
// Single-responsibility entry/trigger module.

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
private bool ClosedBarTriggerReady(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (bars == null ||
                                index < 20 ||
                                index >= bars.Count ||
                                (direction != 1 &&
                                 direction != -1))
                                return false;
                
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            if (atr <= 0)
                                return false;
                
                            double range =
                                bars.HighPrices[index] -
                                bars.LowPrices[index];
                
                            double body =
                                Math.Abs(
                                    bars.ClosePrices[index] -
                                    bars.OpenPrices[index]);
                
                            if (range <= 0 ||
                                body <
                                atr *
                                MinimumTriggerBodyAtr ||
                                range >
                                atr *
                                MaximumTriggerRangeAtr)
                                return false;
                
                            double location =
                                direction == 1
                                    ? (bars.ClosePrices[index] -
                                       bars.LowPrices[index]) /
                                      range
                                    : (bars.HighPrices[index] -
                                       bars.ClosePrices[index]) /
                                      range;
                
                            if (location <
                                MinimumCloseLocation)
                                return false;
                
                            int trigger =
                                direction == 1
                                    ? BullTriggerScore(
                                        bars,
                                        index)
                                    : BearTriggerScore(
                                        bars,
                                        index);
                
                            int requiredTrigger =
                                TriggerThresholdRule.ResolveRequiredScore(
                                    UsePrecisionExecutionModel,
                                    LiveTriggerScore,
                                    PrecisionTriggerScore);
                
                            double breakLevel =
                                direction == 1
                                    ? Highest(
                                        bars,
                                        Math.Max(
                                            0,
                                            index - 6),
                                        index - 1)
                                    : Lowest(
                                        bars,
                                        Math.Max(
                                            0,
                                            index - 6),
                                        index - 1);
                
                            double buffer =
                                atr *
                                Math.Max(
                                    0,
                                    EntryBufferAtr);
                
                            bool breakReady =
                                direction == 1
                                    ? bars.ClosePrices[index] >
                                      breakLevel +
                                      buffer
                                    : bars.ClosePrices[index] <
                                      breakLevel -
                                      buffer;
                
                            if (RequireFreshM5Trigger &&
                                FreshTriggerEvidence(
                                    bars,
                                    index,
                                    direction) <
                                MinimumFreshTriggerEvidence)
                            {
                                bool overrideOk =
                                    AllowDirectDisplacementOverride &&
                                    UseDisplacement &&
                                    trigger >=
                                    ClampInt(
                                        DirectDisplacementOverrideScore,
                                        1,
                                        6) &&
                                    (direction == 1
                                        ? BullDisplacement(
                                            bars,
                                            index,
                                            atr)
                                        : BearDisplacement(
                                            bars,
                                            index,
                                            atr));
                
                                if (!overrideOk)
                                    return false;
                            }
                
                            return
                                TriggerThresholdRule.IsScoreReady(
                                    trigger,
                                    requiredTrigger) &&
                                breakReady;
                        }
    }
}
