using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPrepareReversalLimit(
            int closedM5,
            out int direction,
            out double atr,
            out double targetEntry,
            out double stop,
            out double target,
            out double stopPips,
            out double targetPips,
            out double volume,
            out ExecutionIntent pendingIntent)
        {
            direction =
                _reaction.Direction;
            atr = 0;
            targetEntry = 0;
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
                    "PENDING LIMIT • ATR UNAVAILABLE";
                return false;
            }

            ExecutionModel reversalModel =
                null;

            try
            {
                reversalModel =
                    BuildExecutionModel(
                        closedM5,
                        direction);
            }
            catch
            {
                reversalModel = null;
            }

            targetEntry =
                reversalModel != null &&
                reversalModel.Direction == direction &&
                IsFinitePositive(
                    reversalModel.IdealEntry)
                    ? reversalModel.IdealEntry
                    : direction == 1
                        ? Symbol.Bid -
                          atr * 0.25
                        : Symbol.Ask +
                          atr * 0.25;

            targetEntry =
                NormalizePrice(
                    targetEntry);

            if (!IsValidPendingEntry(
                    direction,
                    targetEntry,
                    false))
            {
                _autoOrdersBlockReason =
                    "PENDING LIMIT • INVALID ENTRY";
                return false;
            }

            string source;
            int quality;

            stop =
                BuildStructuralStop(
                    closedM5,
                    direction,
                    targetEntry,
                    atr,
                    out source,
                    out quality);

            if (!IsFinitePositive(stop))
                return false;

            target =
                SelectStructuralAutoTarget(
                    closedM5,
                    direction,
                    targetEntry,
                    stop,
                    atr,
                    EffectiveAutoTpStage());

            if (!IsAutoPlanValid(
                    direction,
                    targetEntry,
                    stop,
                    target))
            {
                _autoOrdersBlockReason =
                    "PENDING LIMIT • INVALID SL/TP";
                return false;
            }

            stopPips =
                Math.Abs(
                    targetEntry -
                    stop) /
                Symbol.PipSize;

            targetPips =
                Math.Abs(
                    target -
                    targetEntry) /
                Symbol.PipSize;

            volume =
                CalculateVolume(
                    EffectiveRiskStopPips(
                        stopPips));

            volume =
                AdjustVolumeForMargin(
                    direction == 1
                        ? TradeType.Buy
                        : TradeType.Sell,
                    volume);

            if (volume <
                Symbol.VolumeInUnitsMin)
            {
                _autoOrdersBlockReason =
                    "PENDING LIMIT • VOLUME BELOW MINIMUM";
                return false;
            }

            pendingIntent =
                BuildExecutionIntent(
                    direction,
                    DecisionPolicyMode.Pending,
                    ExecutionIntentKind.Limit,
                    targetEntry,
                    0,
                    targetEntry,
                    targetEntry,
                    stop,
                    target,
                    volume,
                    closedM5,
                    "REVERSAL LIMIT");

            return true;
        }
    }
}
