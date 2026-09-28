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
        private void ExecutePreparedAutomaticMarketTrade(
                                    int closedM5,
                                    TradeType type,
                                    double entry,
                                    double stopPips,
                                    double targetPips,
                                    double target,
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
