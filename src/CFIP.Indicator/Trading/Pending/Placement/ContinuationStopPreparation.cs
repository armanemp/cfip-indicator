using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPrepareContinuationStop(
            int closedM5,
            out int direction,
            out double atr,
            out double trigger,
            out double stop,
            out double target,
            out double stopPips,
            out double targetPips,
            out double volume,
            out ExecutionIntent pendingIntent)
        {
            direction =
                _decision.Direction;
            atr = 0;
            trigger = 0;
            stop = 0;
            target = 0;
            stopPips = 0;
            targetPips = 0;
            volume = 0;
            pendingIntent = null;

            atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (atr <= 0)
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • ATR UNAVAILABLE";
                return false;
            }

            if (_executionModel == null ||
                _executionModel.Direction != direction ||
                _executionModel.Mode !=
                    ExecutionMode.WaitingForTrigger)
            {
                _autoOrdersBlockReason =
                    "CONTINUATION STOP NOT ARMED";
                return false;
            }

            if (!IsFinitePositive(
                    _executionModel.Trigger) &&
                closedM5 < 1)
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • INSUFFICIENT LOOKBACK";
                return false;
            }

            int fallbackStart =
                Math.Max(
                    0,
                    closedM5 - 6);

            int fallbackEnd =
                closedM5 - 1;

            if (fallbackEnd < fallbackStart &&
                !IsFinitePositive(
                    _executionModel.Trigger))
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • INVALID LOOKBACK";
                return false;
            }

            trigger =
                IsFinitePositive(
                    _executionModel.Trigger)
                    ? _executionModel.Trigger
                    : direction == 1
                        ? Highest(
                              _m5Bars,
                              fallbackStart,
                              fallbackEnd) +
                          atr *
                          Math.Max(
                              0.02,
                              PendingEntryBufferAtr)
                        : Lowest(
                              _m5Bars,
                              fallbackStart,
                              fallbackEnd) -
                          atr *
                          Math.Max(
                              0.02,
                              PendingEntryBufferAtr);

            double spread =
                Math.Max(
                    0,
                    Symbol.Ask - Symbol.Bid);

            trigger =
                PendingEntryPriceRule.ForExecutableStop(
                    direction,
                    trigger,
                    spread,
                    Symbol.TickSize);

            trigger =
                NormalizePrice(
                    trigger);

            if (!IsValidPendingEntry(
                    direction,
                    trigger,
                    true))
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • INVALID TRIGGER";
                return false;
            }

            string source;
            int quality;

            stop =
                BuildStructuralStop(
                    closedM5,
                    direction,
                    trigger,
                    atr,
                    out source,
                    out quality);

            if (!IsFinitePositive(stop))
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • INVALID SL";
                return false;
            }

            target =
                SelectStructuralAutoTarget(
                    closedM5,
                    direction,
                    trigger,
                    stop,
                    atr,
                    EffectiveAutoTpStage());

            if (!IsAutoPlanValid(
                    direction,
                    trigger,
                    stop,
                    target))
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • INVALID SL/TP";
                return false;
            }

            stopPips =
                Math.Abs(
                    trigger -
                    stop) /
                Symbol.PipSize;

            targetPips =
                Math.Abs(
                    target -
                    trigger) /
                Symbol.PipSize;

            volume =
                CalculateVolume(
                    EffectiveRiskStopPips(
                        stopPips));

            if (volume <
                Symbol.VolumeInUnitsMin)
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • VOLUME BELOW MINIMUM";
                return false;
            }

            pendingIntent =
                BuildExecutionIntent(
                    direction,
                    DecisionPolicyMode.Pending,
                    ExecutionIntentKind.Stop,
                    trigger,
                    trigger,
                    0,
                    0,
                    stop,
                    target,
                    volume,
                    closedM5,
                    "CONTINUATION STOP");

            return true;
        }
    }
}
