// ============================================================================
// CFIP Indicator — AggressiveExecution.cs
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
        private void ProtectBrokerPositions(
                            int closedM5)
                        {
                            if (!AutoBrokerProtection &&
                                !AutoProtectBrokerPositions)
                                return;
                
                            if (_plan == null ||
                                !_plan.IsLivePosition ||
                                _lifecycleState ==
                                    LifecycleState.ExitRequested)
                                return;
                
                            if ((TimeInUtc -
                                 _lastBrokerModifyUtc).TotalMilliseconds <
                                Math.Max(
                                    100,
                                    BrokerModifyCooldownMs))
                                return;
                
                            string label =
                                string.IsNullOrWhiteSpace(
                                    ManagedPositionLabel)
                                    ? NormalizeLabel()
                                    : ManagedPositionLabel.Trim();
                
                            foreach (Position position in Positions)
                            {
                                if (position == null ||
                                    position.SymbolName !=
                                    SymbolName)
                                    continue;
                
                                bool byPlanId =
                                    _plan.PositionId > 0 &&
                                    position.Id ==
                                    _plan.PositionId;
                
                                bool byManagedLabel =
                                    AutoProtectBrokerPositions &&
                                    position.Label ==
                                    label;
                
                                if (!byPlanId &&
                                    !byManagedLabel)
                                    continue;
                
                                bool mutationRequired = false;
                                bool mutationSucceeded = true;
                
                                int positionDirection =
                                    position.TradeType == TradeType.Buy
                                        ? 1
                                        : -1;
                
                                if (IsValidStop(
                                        positionDirection,
                                        position.EntryPrice,
                                        _plan.Stop))
                                {
                                    double normalizedStop =
                                        NormalizePrice(_plan.Stop);
                
                                    bool materiallyDifferent =
                                        !position.StopLoss.HasValue ||
                                        Math.Abs(
                                            position.StopLoss.Value -
                                            normalizedStop) >=
                                        Math.Max(
                                            Symbol.TickSize,
                                            Symbol.PipSize * 0.25);
                
                                    if (materiallyDifferent)
                                    {
                                        mutationRequired = true;
                                        mutationSucceeded =
                                            TryModifyStopLoss(
                                                position,
                                                normalizedStop,
                                                "LIVE PROTECTION • SL") &&
                                            mutationSucceeded;
                                    }
                                }
                
                                if (SyncBrokerTakeProfit)
                                {
                                    double target =
                                        AutoTarget(
                                            _plan,
                                            EffectiveAutoTpStage());
                
                                    if (IsValidTarget(
                                            positionDirection,
                                            position.EntryPrice,
                                            target))
                                    {
                                        bool move = true;
                
                                        if (PreventBrokerTpBackwardMove &&
                                            position.TakeProfit.HasValue)
                                        {
                                            double current =
                                                position.TakeProfit.Value;
                
                                            move =
                                                positionDirection == 1
                                                    ? target >= current
                                                    : target <= current;
                                        }
                
                                        double normalizedTarget =
                                            NormalizePrice(target);
                
                                        bool materiallyDifferent =
                                            move &&
                                            (!position.TakeProfit.HasValue ||
                                             Math.Abs(
                                                 position.TakeProfit.Value -
                                                 normalizedTarget) >=
                                             Math.Max(
                                                 Symbol.TickSize,
                                                 Symbol.PipSize * 0.25));
                
                                        if (materiallyDifferent)
                                        {
                                            mutationRequired = true;
                                            mutationSucceeded =
                                                TryModifyTakeProfit(
                                                    position,
                                                    normalizedTarget,
                                                    "LIVE PROTECTION • TP") &&
                                                mutationSucceeded;
                                        }
                                    }
                                }
                
                                if (mutationRequired &&
                                    !mutationSucceeded)
                                {
                                    _brokerProtectionRecoveryRequired = true;
                
                                    SetLifecycleState(
                                        LifecycleState.RecoveryRequired,
                                        "BROKER PROTECTION MUTATION REJECTED");
                
                                    SendUnifiedAlert(
                                        "PROTECTION-SYNC-FAILED|" +
                                        position.Id,
                                        "CFIP BROKER PROTECTION SYNC REJECTED | #" +
                                        position.Id,
                                        positionDirection,
                                        true);
                                }
                                else if (!_brokerProtectionRecoveryRequired)
                                {
                                    SetLifecycleState(
                                        LifecycleState.LivePosition,
                                        "LIVE POSITION • BROKER STATE SYNCHRONIZED");
                                }
                
                                _lastBrokerModifyUtc =
                                    TimeInUtc;
                
                                break;
                            }
                        }
        
        private void TryAggressiveAutoTrade(
                            int closedM5)
                        {
                            if (!AutoTradingEnabled ||
                                !EnableAggressiveAutoEntry ||
                                _lifecycleState ==
                                    LifecycleState.ExitRequested ||
                                _plan != null ||
                                _reaction == null ||
                                !_reaction.EntryAllowed ||
                                _reaction.Direction == 0)
                                return;
                
                            if (GetManagedPendingOrder() != null)
                            {
                                _autoExecutionBlockReason =
                                    "PENDING ORDER EXISTS";
                                return;
                            }
                
                            if (DailyLossLimitHit(
                                    TimeInUtc))
                                return;
                
                            if (OneOrderPerSignal &&
                                _lastAutoM5 ==
                                closedM5)
                                return;
                
                            if (_reaction.Confidence <
                                AggressiveMinimumConfidence ||
                                _reaction.IndependentEvidence <
                                AggressiveMinimumEvidence)
                                return;
                
                            if (AggressiveRequireSmartAgreement &&
                                (_decision == null ||
                                 _decision.Direction !=
                                 _reaction.Direction ||
                                 _decision.SmartQuality <
                                 AggressiveMinimumSmartQuality))
                                return;
                
                            string aggressiveSuitabilityReason;
                
                            if (!PassesMarketSuitability(
                                    closedM5,
                                    _reaction.Direction,
                                    out aggressiveSuitabilityReason))
                            {
                                SetAutoTradingState(
                                    "BLOCKED",
                                    "AGGRESSIVE • " +
                                    aggressiveSuitabilityReason);
                                return;
                            }
                
                            if (ManagedPositionCount() >=
                                Math.Max(
                                    1,
                                    MaximumOpenPositions))
                                return;
                
                            double entry =
                                NormalizePrice(
                                    _reaction.Direction == 1
                                        ? Symbol.Ask
                                        : Symbol.Bid);
                
                            double atr =
                                Atr(
                                    _m5Bars,
                                    closedM5);
                
                            if (atr <= 0)
                            {
                                _autoExecutionBlockReason =
                                    "AGGRESSIVE • ATR UNAVAILABLE";
                                return;
                            }
                
                            string source;
                            int quality;
                
                            double stop =
                                BuildStructuralStop(
                                    closedM5,
                                    _reaction.Direction,
                                    entry,
                                    atr,
                                    out source,
                                    out quality);
                
                            if (!IsFinitePositive(stop))
                            {
                                _autoExecutionBlockReason =
                                    "AGGRESSIVE • INVALID SL";
                                return;
                            }
                
                            double target =
                                SelectStructuralAutoTarget(
                                    closedM5,
                                    _reaction.Direction,
                                    entry,
                                    stop,
                                    atr,
                                    AggressiveTpStage);
                
                            if (!IsAutoPlanValid(
                                    _reaction.Direction,
                                    entry,
                                    stop,
                                    target))
                            {
                                SetAutoTradingState(
                                    "BLOCKED",
                                    "NO VALID STRUCTURAL TARGET");
                                return;
                            }
                
                            double stopPips =
                                Math.Abs(
                                    entry -
                                    stop) /
                                Symbol.PipSize;
                
                            double tpPips =
                                Math.Abs(
                                    target -
                                    entry) /
                                Symbol.PipSize;
                
                            double effectiveStopPips =
                                stopPips;
                
                            if (IncludeSpreadInRiskSizing)
                                effectiveStopPips +=
                                    Math.Max(
                                        0,
                                        (Symbol.Ask - Symbol.Bid) /
                                        Math.Max(
                                            Symbol.PipSize,
                                            1e-9));
                
                            double volume =
                                CalculateAggressiveVolume(
                                    effectiveStopPips);
                
                            volume =
                                AdjustVolumeForMargin(
                                    _reaction.Direction == 1
                                        ? TradeType.Buy
                                        : TradeType.Sell,
                                    volume);
                
                            if (volume <
                                Symbol.VolumeInUnitsMin)
                            {
                                _autoExecutionBlockReason =
                                    "AGGRESSIVE • VOLUME BELOW MINIMUM";
                                return;
                            }
                
                            try
                            {
                                TradeType type =
                                    _reaction.Direction == 1
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
                                    SetAutoTradingState(
                                        "BLOCKED",
                                        guardReason);
                                    return;
                                }
                
                                ExecutionIntent aggressiveIntent =
                                    BuildExecutionIntent(
                                        _reaction.Direction,
                                        DecisionPolicyMode.Aggressive,
                                        ExecutionIntentKind.Market,
                                        entry,
                                        0,
                                        0,
                                        0,
                                        stop,
                                        target,
                                        volume,
                                        closedM5,
                                        "AGGRESSIVE MARKET");
                
                                string aggressiveIntentReason;
                
                                if (!ValidateExecutionIntent(
                                        aggressiveIntent,
                                        entry,
                                        out aggressiveIntentReason))
                                {
                                    SetAutoTradingState(
                                        "BLOCKED",
                                        aggressiveIntentReason);
                                    return;
                                }
                
                                TradeResult result =
                                    ExecuteMarketOrder(
                                        type,
                                        SymbolName,
                                        volume,
                                        NormalizeLabel(),
                                        stopPips,
                                        tpPips,
                                        "CFIP SMART73",
                                        false);
                
                                if (result == null ||
                                    !result.IsSuccessful ||
                                    result.Position == null)
                                {
                                    _autoExecutionBlockReason =
                                        result != null &&
                                        result.Error.HasValue
                                            ? "AGGRESSIVE • " +
                                              result.Error.Value.ToString()
                                            : "AGGRESSIVE • TRADE REJECTED";
                                    SetAutoTradingState(
                                        "ERROR",
                                        _autoExecutionBlockReason);
                                    return;
                                }
                
                                double actualFill =
                                    NormalizePrice(
                                        result.Position.EntryPrice);
                
                                string aggressiveFillReason;
                
                                if (!ValidateActualMarketFill(
                                        aggressiveIntent,
                                        actualFill,
                                        atr,
                                        out aggressiveFillReason))
                                {
                                    SetLifecycleState(
                                        LifecycleState.RecoveryRequired,
                                        "AGGRESSIVE FILL MISMATCH");
                
                                    _plan =
                                        CreateManagedPlanFromExecution(
                                            _reaction.Direction,
                                            actualFill,
                                            stop,
                                            target,
                                            closedM5,
                                            result.Position.VolumeInUnits);
                
                                    _plan.PositionId =
                                        result.Position.Id;
                
                                    ReconcileLivePlanToActualFill(
                                        result.Position,
                                        closedM5);
                
                                    return;
                                }
                
                                string actualStopSource;
                                int actualStopQuality;
                
                                double actualStop =
                                    BuildStructuralStop(
                                        closedM5,
                                        _reaction.Direction,
                                        actualFill,
                                        atr,
                                        out actualStopSource,
                                        out actualStopQuality);
                
                                double actualTarget =
                                    SelectStructuralAutoTarget(
                                        closedM5,
                                        _reaction.Direction,
                                        actualFill,
                                        actualStop,
                                        atr,
                                        AggressiveTpStage);
                
                                if (!IsExecutionPlanConsistent(
                                        _reaction.Direction,
                                        actualFill,
                                        actualStop,
                                        actualTarget))
                                {
                                    SetLifecycleState(
                                        LifecycleState.RecoveryRequired,
                                        "AGGRESSIVE POST-FILL REBUILD FAILED");
                                    return;
                                }
                
                                _lastAutoM5 =
                                    closedM5;
                
                                _autoExecutionBlockReason =
                                    "EXECUTED";
                
                                _plan =
                                    CreateManagedPlanFromExecution(
                                        _reaction.Direction,
                                        result.Position.EntryPrice,
                                        stop,
                                        target,
                                        closedM5,
                                        result.Position.VolumeInUnits);
                
                                _plan.PositionId =
                                    result.Position.Id;
                
                                SetLifecycleState(
                                    LifecycleState.LivePosition,
                                    "AGGRESSIVE ENTRY • FILLED");
                
                                ReconcileLivePlanToActualFill(
                                    result.Position,
                                    closedM5);
                
                                double maximumFillDistance =
                                    Math.Max(
                                        Symbol.TickSize * 2,
                                        Math.Max(
                                            Symbol.PipSize * 0.5,
                                            Math.Max(
                                                (Symbol.Ask - Symbol.Bid) * 2,
                                                atr *
                                                Math.Max(
                                                    0.10,
                                                    MaximumEntryExtensionAtr))));
                
                                if (Math.Abs(
                                        result.Position.EntryPrice -
                                        entry) >
                                    maximumFillDistance)
                                {
                                    SetLifecycleState(
                                        LifecycleState.ExitRequested,
                                        "AGGRESSIVE FILL MISMATCH");
                
                                    bool closed =
                                        TryClosePosition(
                                            result.Position,
                                            "AGGRESSIVE FILL MISMATCH");
                
                                    if (!closed)
                                    {
                                        SetLifecycleState(
                                            LifecycleState.RecoveryRequired,
                                            "AGGRESSIVE FILL MISMATCH • CLOSE REJECTED");
                                    }
                
                                    SendUnifiedAlert(
                                        "FILL-MISMATCH|" +
                                        result.Position.Id,
                                        "CFIP AGGRESSIVE FILL OUTSIDE EXECUTION ENVELOPE | #" +
                                        result.Position.Id,
                                        _reaction.Direction,
                                        true);
                
                                    return;
                                }
                
                                EnrichLivePlanTargets(closedM5);
                
                                if (AutoBrokerProtection)
                                {
                                    EnsureBrokerProtectionForPosition(
                                        result.Position,
                                        actualStop,
                                        actualTarget,
                                        "AGGRESSIVE ENTRY",
                                        _reaction.Direction);
                                }
                
                                SendUnifiedAlert(
                                    "AUTO-REACTION|" +
                                    closedM5,
                                    "CFIP AUTO REACTION " +
                                    (_reaction.Direction == 1
                                        ? "BUY"
                                        : "SELL") +
                                    " EXECUTED | #" +
                                    result.Position.Id +
                                    " | ENTRY " +
                                    Price(
                                        result.Position.EntryPrice) +
                                    " | SL " +
                                    Price(actualStop) +
                                    " | TP " +
                                    Price(actualTarget),
                                    _reaction.Direction,
                                    true);
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP aggressive auto trade failed: {0}",
                                    ex.Message);
                            }
                        }
    }
}
