// ============================================================================
// CFIP Indicator — PendingOrderPlacement.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        private void TrySmartPendingOrders(int closedM5)
                                {
                                    _lastAutoOrderAttemptUtc =
                                        TimeInUtc;
                        
                                    if (AutomaticOrdersEnabled &&
                                        DailyLossLimitHit(TimeInUtc))
                                    {
                                        _autoOrdersBlockReason =
                                            "DAILY LOSS LIMIT";
                        
                                        CancelAllOrders();
                                        return;
                                    }
                        
                                    if (!AutomaticOrdersEnabled)
                                    {
                                        _autoOrdersBlockReason =
                                            "DISABLED";
                                        return;
                                    }
                        
                                    if (GetManagedPosition() != null)
                                    {
                                        _autoOrdersBlockReason =
                                            "MANAGED POSITION ACTIVE";
                                        return;
                                    }
                        
                                    if (_plan != null &&
                                        !_plan.IsLivePosition)
                                    {
                                        _autoOrdersBlockReason =
                                            "MARKET PLAN ACTIVE";
                                        return;
                                    }
                        
                                    CleanupPendingOrdersIfNeeded(closedM5);
                        
                                    PendingOrder existingPending =
                                        GetManagedPendingOrder();
                        
                                    if (existingPending != null)
                                    {
                                        _autoOrdersBlockReason =
                                            "PENDING ORDER EXISTS";
                                        return;
                                    }
                        
                                    if (ManagedPositionCount() >=
                                        Math.Max(1, MaximumOpenPositions))
                                    {
                                        _autoOrdersBlockReason =
                                            "MAX OPEN POSITIONS";
                                        return;
                                    }
                        
                                    if (ManagedPendingOrderCount() > 0)
                                    {
                                        _autoOrdersBlockReason =
                                            "PENDING ORDER ALREADY EXISTS";
                                        return;
                                    }
                        
                                    if (DailyLossLimitHit(TimeInUtc))
                                    {
                                        _autoOrdersBlockReason =
                                            "DAILY LOSS LIMIT";
                                        return;
                                    }
                        
                                    int pendingDirection =
                                        TrendContinuationStrong()
                                            ? _decision.Direction
                                            : ReversalSetupStrong()
                                                ? _reaction.Direction
                                                : 0;
                        
                                    if (pendingDirection != 0)
                                    {
                                        string pendingSuitabilityReason;
                        
                                        if (!PassesMarketSuitability(
                                                closedM5,
                                                pendingDirection,
                                                out pendingSuitabilityReason))
                                        {
                                            _autoOrdersBlockReason =
                                                "SUITABILITY • " +
                                                pendingSuitabilityReason;
                                            return;
                                        }
                                    }
                        
                                    if (pendingDirection != 0 &&
                                        !EnsureTradingPermission())
                                    {
                                        _autoOrdersBlockReason = "TRADING PERMISSION";
                                        return;
                                    }
                        
                                    if (TrendContinuationStrong() &&
                                        PendingModeAllowsStop())
                                    {
                                        if (PlaceContinuationStop(closedM5))
                                        {
                                            _autoOrdersBlockReason =
                                                "ORDER PLACED";
                                            return;
                                        }
                                    }
                        
                                    if (ReversalSetupStrong() &&
                                        PendingModeAllowsLimit())
                                    {
                                        CheckReversalProtection();
                        
                                        if (PlaceReversalLimit(closedM5))
                                        {
                                            _autoOrdersBlockReason =
                                                "ORDER PLACED";
                                            return;
                                        }
                                    }
                        
                                    if (pendingDirection == 0)
                                    {
                                        _autoOrdersBlockReason =
                                            "NO ELIGIBLE PENDING SETUP";
                                        return;
                                    }
                        
                                    if (string.IsNullOrWhiteSpace(
                                            _autoOrdersBlockReason) ||
                                        _autoOrdersBlockReason ==
                                            "NOT EVALUATED")
                                    {
                                        _autoOrdersBlockReason =
                                            "PENDING EXECUTION BLOCKED";
                                    }
                                }
        
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
                                                "CFIP SMART73",
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
                                            PlaceLimitOrder(
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
                                                "CFIP SMART73",
                                                false);
                        
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
        
        private void CleanupPendingOrdersIfNeeded(
                                    int closedM5)
                                {
                                    if (!PendingAutoCleanup ||
                                        _lastPendingCleanupM5 == closedM5)
                                        return;
                        
                                    _lastPendingCleanupM5 =
                                        closedM5;
                        
                                    foreach (PendingOrder order in PendingOrders)
                                    {
                                        if (!IsManagedPendingOrder(order))
                                            continue;
                        
                                        bool stale =
                                            order.ExpirationTime.HasValue &&
                                            order.ExpirationTime.Value <=
                                            TimeInUtc;
                        
                                        int expectedDirection =
                                            _decision != null
                                                ? _decision.Direction
                                                : 0;
                        
                                        if (order.OrderType ==
                                                PendingOrderType.Limit &&
                                            ReversalSetupStrong())
                                        {
                                            expectedDirection =
                                                _reaction.Direction;
                                        }
                        
                                        bool wrongDirection =
                                            expectedDirection != 0 &&
                                            ((order.TradeType == TradeType.Buy &&
                                              expectedDirection != 1) ||
                                             (order.TradeType == TradeType.Sell &&
                                              expectedDirection != -1));
                        
                                        bool reversalSupersedesStop =
                                            ReversalSetupStrong() &&
                                            order.OrderType == PendingOrderType.Stop;
                        
                                        if (!stale &&
                                            !wrongDirection &&
                                            !reversalSupersedesStop)
                                            continue;
                        
                                        if (!TryCancelPendingOrder(
                                                order,
                                                stale
                                                    ? "STALE PENDING ORDER"
                                                    : wrongDirection
                                                        ? "WRONG DIRECTION PENDING ORDER"
                                                        : "REVERSAL SUPERSEDES STOP"))
                                        {
                                            SetLifecycleState(
                                                LifecycleState.RecoveryRequired,
                                                "PENDING CLEANUP CANCEL REJECTED");
                        
                                            _autoOrdersBlockReason =
                                                "PENDING CLEANUP CANCEL REJECTED";
                                        }
                                    }
                                }
    }
}
