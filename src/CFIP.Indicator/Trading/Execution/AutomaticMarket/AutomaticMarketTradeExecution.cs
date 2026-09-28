// ============================================================================
// CFIP Indicator — AutomaticMarketTradeExecution.cs
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
        private void TryAutoTrade(
                                    int closedM5)
                                {
                                    _lastAutoTradeAttemptUtc =
                                        TimeInUtc;
                        
                                    if (!AutoTradingEnabled)
                                    {
                                        _lastAutoPlanAttemptM5 =
                                            -1;
                        
                                        SetAutoTradingState(
                                            "OFF",
                                            "DISABLED");
                                        return;
                                    }

                                    string capacityReason;
                                    if (!ValidateConfiguredPositionCapacity(
                                            out capacityReason))
                                    {
                                        _autoExecutionBlockReason = capacityReason;
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            capacityReason);
                                        return;
                                    }
                        
                                    PendingOrder existingPending =
                                        GetManagedPendingOrder();
                        
                                    if (existingPending != null)
                                    {
                                        _autoExecutionBlockReason =
                                            "PENDING ORDER EXISTS";
                                        SetAutoTradingState(
                                            "ARMED",
                                            "WAITING FOR PENDING ORDER");
                                        return;
                                    }
                        
                                    if (DailyLossLimitHit(
                                            TimeInUtc))
                                    {
                                        _autoExecutionBlockReason =
                                            "DAILY LOSS LIMIT";
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "DAILY LOSS LIMIT REACHED");
                                        return;
                                    }
                        
                                    if (_plan == null)
                                    {
                                        EnsureSignalPlan(
                                            closedM5,
                                            ConfirmedSignalsOnly
                                                ? DecisionPolicyMode.Confirmed
                                                : DecisionPolicyMode.Soft);
                        
                                        if (_plan == null)
                                        {
                                            if (_executionModel != null &&
                                                _executionModel.Mode ==
                                                    ExecutionMode.WaitingForTrigger)
                                            {
                                                _autoExecutionBlockReason =
                                                    "WAITING FOR TRIGGER";
                        
                                                SetAutoTradingState(
                                                    "ARMED",
                                                    "WAITING FOR TRIGGER");
                                            }
                                            else
                                            {
                                                _autoExecutionBlockReason =
                                                    "NO ELIGIBLE PLAN";
                        
                                                SetAutoTradingState(
                                                    "ARMED",
                                                    ConfirmedSignalsOnly
                                                        ? "WAITING FOR CONFIRMED PLAN"
                                                        : "WAITING FOR SMART-ELIGIBLE PLAN");
                                            }
                        
                                            return;
                                        }
                                    }
                        
                                    if (_decision == null ||
                                        _decision.Direction == 0)
                                    {
                                        _autoExecutionBlockReason =
                                            "NO DECISION";
                                        SetAutoTradingState(
                                            "ARMED",
                                            "WAITING FOR DECISION");
                                        return;
                                    }
                        
                                    if (!_decision.EntryAllowed)
                                    {
                                        bool liveGate =
                                            !ConfirmedSignalsOnly &&
                                            IsLiveExecutionGateReason(
                                                _decision.BlockReason);
                        
                                        if (!liveGate)
                                        {
                                            _autoExecutionBlockReason =
                                                string.IsNullOrWhiteSpace(
                                                    _decision.BlockReason)
                                                    ? "WAITING FOR CONFIRMATION"
                                                    : _decision.BlockReason;
                        
                                            SetAutoTradingState(
                                                "ARMED",
                                                string.IsNullOrWhiteSpace(
                                                    _decision.BlockReason)
                                                    ? "WAITING FOR CONFIRMATION"
                                                    : _decision.BlockReason);
                        
                                            return;
                                        }
                                    }
                        
                                    if (OneOrderPerSignal &&
                                        _lastAutoM5 == closedM5)
                                    {
                                        _autoExecutionBlockReason =
                                            "ALREADY TRADED THIS M5";
                                        SetAutoTradingState(
                                            "ARMED",
                                            "ALREADY TRADED THIS M5");
                                        return;
                                    }
                        
                                    if (_decision.Confidence <
                                        MinimumAutoConfidence)
                                    {
                                        _autoExecutionBlockReason =
                                            "CONF " +
                                            _decision.Confidence +
                                            " < " +
                                            MinimumAutoConfidence;
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "CONF " +
                                            _decision.Confidence +
                                            " < " +
                                            MinimumAutoConfidence);
                                        return;
                                    }
                        
                                    if (_decision.SmartQuality <
                                        MinimumAutoSmartQuality)
                                    {
                                        _autoExecutionBlockReason =
                                            "SMART Q " +
                                            _decision.SmartQuality +
                                            " < " +
                                            MinimumAutoSmartQuality;
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "SMART Q " +
                                            _decision.SmartQuality +
                                            " < " +
                                            MinimumAutoSmartQuality);
                                        return;
                                    }
                        
                                    string suitabilityReason;
                        
                                    if (!PassesMarketSuitability(
                                            closedM5,
                                            _plan.Direction,
                                            out suitabilityReason))
                                    {
                                        _autoExecutionBlockReason =
                                            "SUITABILITY • " +
                                            suitabilityReason;
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            suitabilityReason);
                                        return;
                                    }
                        
                                    int levelQuality =
                                        Math.Min(
                                            _plan.StopQuality,
                                            _plan.Tp1Quality);
                        
                                    if (levelQuality <
                                        MinimumAutoLevelQuality)
                                    {
                                        _autoExecutionBlockReason =
                                            "LEVEL Q " +
                                            levelQuality +
                                            " < " +
                                            MinimumAutoLevelQuality;
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "LEVEL Q " +
                                            levelQuality +
                                            " < " +
                                            MinimumAutoLevelQuality);
                                        return;
                                    }
                        
                                    if (ManagedPositionCount() >=
                                        Math.Max(
                                            1,
                                            MaximumOpenPositions))
                                    {
                                        _autoExecutionBlockReason =
                                            "MAX OPEN POSITIONS";
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "MAX OPEN POSITIONS");
                                        return;
                                    }
                        
                                    double plannedEntry = _plan.Entry;
                        
                                    double entry =
                                        NormalizePrice(
                                            _plan.Direction == 1
                                                ? Symbol.Ask
                                                : Symbol.Bid);
                        
                                    double atr =
                                        Atr(
                                            _m5Bars,
                                            closedM5);
                        
                                    if (atr <= 0)
                                    {
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "ATR UNAVAILABLE");
                                        return;
                                    }
                        
                                    if (Math.Abs(
                                            entry -
                                            plannedEntry) >
                                        atr *
                                        MaximumEntryExtensionAtr)
                                    {
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "ENTRY EXTENSION");
                                        return;
                                    }
                        
                                    double stopPips;
                                    double targetPips;
                                    double target;
                        
                                    if (!TryPrepareExecutablePlan(
                                            closedM5,
                                            entry,
                                            out stopPips,
                                            out targetPips,
                                            out target))
                                    {
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "INVALID SMART EXECUTION PLAN");
                                        return;
                                    }
                        
                                    string executableEntryReason;
                        
                                    if (!IsExecutableMarketEntry(
                                            _plan,
                                            entry,
                                            out executableEntryReason))
                                    {
                                        _autoExecutionBlockReason =
                                            executableEntryReason;
                                        SetAutoTradingState(
                                            "ARMED",
                                            executableEntryReason);
                                        return;
                                    }
                        
                                    double effectiveStopPips =
                                        EffectiveRiskStopPips(stopPips);
                        
                                    double volume =
                                        CalculateVolume(
                                            effectiveStopPips);
                        
                                    volume =
                                        AdjustVolumeForMargin(
                                            _plan.Direction == 1
                                                ? TradeType.Buy
                                                : TradeType.Sell,
                                            volume);
                        
                                    if (volume <
                                        Symbol.VolumeInUnitsMin)
                                    {
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "VOLUME BELOW MINIMUM");
                                        return;
                                    }
                        
                                    try
                                    {
                                        TradeType type =
                                            _plan.Direction == 1
                                                ? TradeType.Buy
                                                : TradeType.Sell;
                        
                                        if (!EnsureTradingPermission())
                                        {
                                            _autoExecutionBlockReason =
                                                "TRADING PERMISSION";
                                            SetAutoTradingState(
                                                "BLOCKED",
                                                "TRADING PERMISSION NOT GRANTED");
                                            return;
                                        }
                        
                                        string guardReason;
                        
                                        if (!PassesAutoTradeSafetyGuards(
                                                type,
                                                volume,
                                                out guardReason))
                                        {
                                            _autoExecutionBlockReason =
                                                guardReason;
                        
                                            SetAutoTradingState(
                                                "BLOCKED",
                                                guardReason);
                                            return;
                                        }
                        
                                        ExecutionIntent marketIntent =
                                            BuildExecutionIntent(
                                                _plan.Direction,
                                                ConfirmedSignalsOnly
                                                    ? DecisionPolicyMode.Confirmed
                                                    : DecisionPolicyMode.Soft,
                                                ExecutionIntentKind.Market,
                                                entry,
                                                _plan.EntryTrigger,
                                                _plan.EntryZoneLow,
                                                _plan.EntryZoneHigh,
                                                _plan.Stop,
                                                target,
                                                volume,
                                                closedM5,
                                                "NORMAL MARKET");
                        
                                        string intentReason;
                        
                                        if (!ValidateExecutionIntent(
                                                marketIntent,
                                                entry,
                                                out intentReason))
                                        {
                                            _autoExecutionBlockReason =
                                                intentReason;
                                            SetAutoTradingState(
                                                "BLOCKED",
                                                intentReason);
                                            return;
                                        }
                        
                                        TradeResult result =
                                            TryExecuteMarketOrder(
                                                type,
                                                SymbolName,
                                                volume,
                                                NormalizeLabel(),
                                                stopPips,
                                                targetPips,
                                                TradeExecutionMetadata.DefaultExecutionComment,
                                                false,
                                                "AUTOMATIC MARKET");
                                        if (result == null)
                                        {
                                            _autoExecutionBlockReason =
                                                "NULL TRADE RESULT";
                                            SetAutoTradingState(
                                                "ERROR",
                                                "NULL TRADE RESULT");
                                            return;
                                        }

                                        if (!BrokerConfirmationPolicy.CanAdoptPosition(
                                                true,
                                                result.IsSuccessful,
                                                result.Position != null))
                                        {
                                            _autoExecutionBlockReason =
                                                result.Error.HasValue
                                                    ? result.Error.Value.ToString()
                                                    : "TRADE REJECTED";

                                            SetAutoTradingState(
                                                "ERROR",
                                                _autoExecutionBlockReason);
                                            return;
                                        }
                        
                                        _lastAutoM5 =
                                            closedM5;
                        
                                        _autoExecutionBlockReason =
                                            "EXECUTED";
                        
                                        _plan.OriginalVolume =
                                            result.Position.VolumeInUnits;
                        
                                        _plan.IsLivePosition = true;
                                        _plan.PositionId = result.Position.Id;
                        
                                        SetLifecycleState(
                                            LifecycleState.LivePosition,
                                            "MARKET ENTRY • FILLED");
                        
                                        ReconcileLivePlanToActualFill(
                                            result.Position,
                                            closedM5);
                        
                                        string fillExecutionReason;
                        
                                        if (!IsExecutableFillPrice(
                                                _plan,
                                                result.Position.EntryPrice,
                                                out fillExecutionReason))
                                        {
                                            SetLifecycleState(
                                                LifecycleState.ExitRequested,
                                                "MARKET FILL MISMATCH");
                        
                                            _autoExecutionBlockReason =
                                                "FILL MISMATCH • " +
                                                fillExecutionReason;
                        
                                            SetAutoTradingState(
                                                "ERROR",
                                                fillExecutionReason);
                        
                                            bool closed =
                                                TryClosePosition(
                                                    result.Position,
                                                    "MARKET FILL MISMATCH");
                        
                                            if (!closed)
                                            {
                                                SetLifecycleState(
                                                    LifecycleState.RecoveryRequired,
                                                    "MARKET FILL MISMATCH • CLOSE REJECTED");
                                            }
                        
                                            SendUnifiedAlert(
                                                "FILL-MISMATCH|" +
                                                result.Position.Id,
                                                "CFIP ACCEPTED BROKER FILL OUTSIDE EXECUTION ENVELOPE | #" +
                                                result.Position.Id +
                                                " | " +
                                                fillExecutionReason,
                                                _plan.Direction,
                                                true);
                        
                                            return;
                                        }
                        
                                        double structuralTarget =
                                            AutoTarget(
                                                _plan,
                                                EffectiveAutoTpStage());
                        
                                        if (IsValidTarget(
                                                _plan.Direction,
                                                result.Position.EntryPrice,
                                                structuralTarget))
                                        {
                                            target =
                                                NormalizePrice(
                                                    structuralTarget);
                                        }
                                        else if (result.Position.TakeProfit.HasValue &&
                                                 IsValidTarget(
                                                     _plan.Direction,
                                                     result.Position.EntryPrice,
                                                     result.Position.TakeProfit.Value))
                                        {
                                            target =
                                                NormalizePrice(
                                                    result.Position.TakeProfit.Value);
                                        }
                                        else if (IsValidTarget(
                                                     _plan.Direction,
                                                     result.Position.EntryPrice,
                                                     _plan.Tp1))
                                        {
                                            target =
                                                NormalizePrice(
                                                    _plan.Tp1);
                                        }
                                        else
                                        {
                                            _autoExecutionBlockReason =
                                                "POST-FILL TARGET INVALID";
                        
                                            SetAutoTradingState(
                                                "ERROR",
                                                "POSITION OPENED • NO VALID TARGET");
                        
                                            if (!RequestLivePlanExit(
                                                    closedM5,
                                                    "POST-FILL TARGET INVALID"))
                                            {
                                                SetLifecycleState(
                                                    LifecycleState.RecoveryRequired,
                                                    "POST-FILL TARGET INVALID • CLOSE REJECTED");
                                            }
                        
                                            return;
                                        }
                        
                                        bool protectionOk = true;

                                        if (AutoBrokerProtection)
                                        {
                                            protectionOk =
                                                EnsureBrokerProtectionForPosition(
                                                    result.Position,
                                                    _plan.Stop,
                                                    target,
                                                    "NEW MARKET ENTRY",
                                                    _plan.Direction);
                                        }

                                        SetAutoTradingState(
                                            protectionOk
                                                ? "EXECUTED"
                                                : "RECOVERY",
                                            protectionOk
                                                ? "POSITION #" +
                                                  result.Position.Id
                                                : "POSITION #" +
                                                  result.Position.Id +
                                                  " • BROKER PROTECTION RECOVERY");
                        
                                        SendUnifiedAlert(
                                            "AUTO|" +
                                            closedM5,
                                            "CFIP AUTO " +
                                            (_plan.Direction == 1
                                                ? "BUY"
                                                : "SELL") +
                                            " EXECUTED | #" +
                                            result.Position.Id +
                                            " | ENTRY " +
                                            Price(
                                                result.Position.EntryPrice) +
                                            " | SL " +
                                            Price(
                                                _plan.Stop) +
                                            " | TP " +
                                            Price(
                                                target),
                                            _plan.Direction,
                                            true);
                                    }
                                    catch (Exception ex)
                                    {
                                        SetAutoTradingState(
                                            "ERROR",
                                            ex.Message);
                        
                                        Print(
                                            "CFIP auto trade failed: {0}",
                                            ex.Message);
                                    }
                                }
    }
}
