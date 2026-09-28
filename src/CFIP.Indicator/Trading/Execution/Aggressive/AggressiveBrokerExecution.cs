// ============================================================================
// CFIP Indicator — AggressiveTradeExecution.cs
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
        private void ExecuteAggressiveTrade(
                                    int closedM5,
                                    TradeType type,
                                    double entry,
                                    double atr,
                                    double stop,
                                    double target,
                                    double stopPips,
                                    double tpPips,
                                    double volume)
                                {
                                    try
                                    {
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
                                            TryExecuteMarketOrder(
                                                type,
                                                SymbolName,
                                                volume,
                                                NormalizeLabel(),
                                                stopPips,
                                                tpPips,
                                                TradeExecutionMetadata.DefaultExecutionComment,
                                                false,
                                                "AGGRESSIVE MARKET");
                        
                                        if (!BrokerConfirmationPolicy.CanAdoptPosition(
                                                result != null,
                                                result != null &&
                                                result.IsSuccessful,
                                                result != null &&
                                                result.Position != null))
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
                                            else
                                            {
                                                _autoExecutionBlockReason =
                                                    "FILL MISMATCH • POSITION CLOSE REQUESTED";
                                            }

                                            SendUnifiedAlert(
                                                "FILL-MISMATCH|" +
                                                result.Position.Id,
                                                "CFIP AGGRESSIVE FILL OUTSIDE EXECUTION ENVELOPE | #" +
                                                result.Position.Id +
                                                " | " +
                                                aggressiveFillReason,
                                                _reaction.Direction,
                                                true);

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

                                        bool protectionOk = true;

                                        if (AutoBrokerProtection)
                                        {
                                            protectionOk =
                                                EnsureBrokerProtectionForPosition(
                                                    result.Position,
                                                    actualStop,
                                                    actualTarget,
                                                    "AGGRESSIVE ENTRY",
                                                    _reaction.Direction);
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
