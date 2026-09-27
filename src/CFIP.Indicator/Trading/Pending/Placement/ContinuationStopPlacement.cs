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
        private bool PlaceContinuationStop(int closedM5)
                                        {
                                            int direction =
                                                _decision.Direction;
                                
                                            double atr =
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
                                
                                            double trigger =
                                                _executionModel != null &&
                                                IsFinitePositive(
                                                    _executionModel.Trigger)
                                                    ? _executionModel.Trigger
                                                    : direction == 1
                                                        ? Highest(
                                                              _m5Bars,
                                                              Math.Max(
                                                                  1,
                                                                  closedM5 - 6),
                                                              closedM5 - 1) +
                                                          atr *
                                                          Math.Max(
                                                              0.02,
                                                              PendingEntryBufferAtr)
                                                        : Lowest(
                                                              _m5Bars,
                                                              Math.Max(
                                                                  1,
                                                                  closedM5 - 6),
                                                              closedM5 - 1) -
                                                          atr *
                                                          Math.Max(
                                                              0.02,
                                                              PendingEntryBufferAtr);
                                
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
                                
                                            double stop =
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
                                
                                            double target =
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
                                
                                            double stopPips =
                                                Math.Abs(
                                                    trigger -
                                                    stop) /
                                                Symbol.PipSize;
                                
                                            double targetPips =
                                                Math.Abs(
                                                    target -
                                                    trigger) /
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
                                                    "PENDING STOP • VOLUME BELOW MINIMUM";
                                                return false;
                                            }
                                
                                            ExecutionIntent pendingIntent =
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
                                
                                            string intentReason;
                                
                                            if (!ValidateExecutionIntent(
                                                    pendingIntent,
                                                    direction == 1
                                                        ? Symbol.Ask
                                                        : Symbol.Bid,
                                                    out intentReason))
                                            {
                                                _autoOrdersBlockReason =
                                                    "PENDING STOP • " +
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
                                                    "PENDING STOP • " +
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
                                                    PlaceStopOrder(
                                                        direction == 1
                                                            ? TradeType.Buy
                                                            : TradeType.Sell,
                                                        SymbolName,
                                                        volume,
                                                        trigger,
                                                        PendingOrderLabel(),
                                                        pendingIntent.StopPips,
                                                        pendingIntent.TargetPips,
                                                        ProtectionType.Relative,
                                                        expiration,
                                                        TradeExecutionMetadata.DefaultExecutionComment,
                                                        false);
                                
                                                if (result == null ||
                                                    !result.IsSuccessful ||
                                                    result.PendingOrder == null)
                                                {
                                                    _autoOrdersBlockReason =
                                                        result != null &&
                                                        result.Error.HasValue
                                                            ? result.Error.Value.ToString()
                                                            : "PENDING STOP REJECTED";
                                
                                                    return false;
                                                }
                                
                                                _lastPendingSignalM5 =
                                                    closedM5;
                                
                                                _plan = null;
                                                _executionModel = null;
                                                RemovePlanObjects();
                                
                                                _autoOrdersBlockReason =
                                                    "ORDER PLACED • STOP " + Price(trigger);
                                
                                                SendUnifiedAlert(
                                                    "PENDING-STOP|" + closedM5,
                                                    "CFIP STOP ORDER | " +
                                                    (direction == 1
                                                        ? "BUY"
                                                        : "SELL") +
                                                    " | ENTRY " +
                                                    Price(trigger) +
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
                                                    "PENDING STOP • " +
                                                    ex.Message;
                                
                                                Print(
                                                    "CFIP pending stop failed: {0}",
                                                    ex.Message);
                                
                                                return false;
                                            }
                                        }
    }
}
