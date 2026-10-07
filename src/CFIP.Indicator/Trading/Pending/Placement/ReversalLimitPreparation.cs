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
            // Pending reversal orders require a reaction that has already been
            // confirmed on a closed M5 bar. The live/forming reaction direction
            // is never sufficient to manufacture an executable pending intent.
            direction =
                _reaction != null &&
                _reaction.ReactionClosedBarConfirmed
                    ? _reaction.ReactionConfirmedDirection
                    : 0;
            atr = 0;
            targetEntry = 0;
            stop = 0;
            target = 0;
            stopPips = 0;
            targetPips = 0;
            volume = 0;
            pendingIntent = null;

            if (direction == 0)
            {
                _autoOrdersBlockReason =
                    "PREDICTIVE LIMIT • NO REVERSAL DIRECTION";
                return false;
            }

            atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (atr <= 0)
            {
                _autoOrdersBlockReason =
                    "PREDICTIVE LIMIT • ATR UNAVAILABLE";
                return false;
            }

            // A pending Limit is triggered on the executable side that must
            // be reached: Ask for Buy Limit, Bid for Sell Limit.
            double market =
                direction == 1
                    ? Symbol.Ask
                    : Symbol.Bid;

            PredictivePendingCandidate candidate;

            if (!TrySelectPredictivePendingLevel(
                    closedM5,
                    direction,
                    atr,
                    market,
                    out candidate))
            {
                _autoOrdersBlockReason =
                    "PREDICTIVE LIMIT • NO QUALIFIED FUTURE LEVEL";
                return false;
            }

            targetEntry =
                NormalizePrice(
                    candidate.Price);

            if (!IsValidPendingEntry(
                    direction,
                    targetEntry,
                    false))
            {
                _autoOrdersBlockReason =
                    "PREDICTIVE LIMIT • INVALID FUTURE ENTRY";
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
            {
                _autoOrdersBlockReason =
                    "PREDICTIVE LIMIT • INVALID STRUCTURAL SL";
                return false;
            }

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
                    "PREDICTIVE LIMIT • INVALID SL/TP";
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
                    "PREDICTIVE LIMIT • VOLUME BELOW MINIMUM";
                return false;
            }

            _autoOrdersBlockReason =
                "PREDICTIVE " +
                (direction == 1
                    ? "BUY"
                    : "SELL") +
                " LIMIT • " +
                candidate.Source +
                " • SCORE " +
                candidate.Score.ToString("F0") +
                " • DIST " +
                candidate.DistanceAtr.ToString("F2") +
                " ATR";

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
                    "PREDICTIVE REVERSAL LIMIT • " +
                    candidate.Source);

            return true;
        }
    }
}
