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
        private bool PlaceReversalLimit(int closedM5)
                                        {
                                            int direction =
                                                _reaction.Direction;
                                
                                            double atr =
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
                                
                                            double targetEntry =
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
                                
                                            double stop =
                                                BuildStructuralStop(
                                                    closedM5,
                                                    direction,
                                                    targetEntry,
                                                    atr,
                                                    out source,
                                                    out quality);
                                
                                            if (!IsFinitePositive(stop))
                                                return false;
                                
                                            double target =
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
                                
                                            double stopPips =
                                                Math.Abs(
                                                    targetEntry -
                                                    stop) /
                                                Symbol.PipSize;
                                
                                            double targetPips =
                                                Math.Abs(
                                                    target -
                                                    targetEntry) /
                                                Symbol.PipSize;
                                
                                            double volume =
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
                                
                                            ExecutionIntent pendingIntent =
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
                                
                                            string intentReason;
                                
                                            if (!ValidateExecutionIntent(
                                                    pendingIntent,
                                                    direction == 1
                                                        ? Symbol.Bid
                                                        : Symbol.Ask,
                                                    out intentReason))
                                            {
                                                _autoOrdersBlockReason =
                                                    "PENDING LIMIT • " +
                                                    intentReason;
                                                return false;
                                            }
                                
                                            string reason;
                                
                                            if (!PassesAutoTradeSafetyGuards(
                                                    direction == 1
                                                        ? TradeType.Buy
                                                        : TradeType.Sell,
                                                    volume,
                                                    out reason))
                                            {
                                                _autoOrdersBlockReason =
                                                    "PENDING LIMIT • " +
                                                    reason;
                                                return false;
                                            }
                                
                                            try
                                            {
                                                DateTime expiration =
                                                    TimeInUtc.AddMinutes(
                                                        Math.Max(
                                                            15,
                                                            PendingOrderExpiryMinutes));
                                
                                                TradeResult result =
                                                    TryPlaceLimitOrder(
                                                        direction == 1
                                                            ? TradeType.Buy
                                                            : TradeType.Sell,
                                                        SymbolName,
                                                        volume,
                                                        targetEntry,
                                                        PendingOrderLabel(),
                                                        pendingIntent.StopPips,
                                                        pendingIntent.TargetPips,
                                                        ProtectionType.Relative,
                                                        expiration,
                                                        TradeExecutionMetadata.DefaultExecutionComment,
                                                        false,
                                                        "REVERSAL LIMIT");
                                
                                                if (result == null ||
                                                    !result.IsSuccessful ||
                                                    result.PendingOrder == null)
                                                {
                                                    _autoOrdersBlockReason =
                                                        result != null &&
                                                        result.Error.HasValue
                                                            ? result.Error.Value.ToString()
                                                            : "PENDING LIMIT REJECTED";
                                
                                                    return false;
                                                }
                                
                                                _lastPendingSignalM5 =
                                                    closedM5;
                                
                                                _plan = null;
                                                _executionModel = null;
                                                RemovePlanObjects();
                                
                                                _autoOrdersBlockReason =
                                                    "ORDER PLACED • LIMIT " +
                                                    Price(targetEntry);
                                
                                                SendUnifiedAlert(
                                                    "PENDING-LIMIT|" + closedM5,
                                                    "CFIP LIMIT ORDER | " +
                                                    (direction == 1
                                                        ? "BUY"
                                                        : "SELL") +
                                                    " | ENTRY " +
                                                    Price(targetEntry) +
                                                    " | SL " +
                                                    Price(stop) +
                                                    " | TP " +
                                                    Price(target),
                                                    direction,
                                                    true);
                                
                                                return true;
                                            }
                                            catch (Exception ex)
                                            {
                                                _autoOrdersBlockReason =
                                                    "PENDING LIMIT • " +
                                                    ex.Message;
                                
                                                Print(
                                                    "CFIP pending limit failed: {0}",
                                                    ex.Message);
                                
                                                return false;
                                            }
                                        }
    }
}
