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
        private bool TryPrepareAutomaticMarketTrade(
                                    int closedM5,
                                    out TradeType type,
                                    out double entry,
                                    out double stopPips,
                                    out double targetPips,
                                    out double target,
                                    out double volume)
                                {
                                    type = TradeType.Buy;
                                    entry = 0;
                                    stopPips = 0;
                                    targetPips = 0;
                                    target = 0;
                                    volume = 0;

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

                                    type =
                                        _plan.Direction == 1
                                            ? TradeType.Buy
                                            : TradeType.Sell;

                                    return true;
                                }
    }
}
