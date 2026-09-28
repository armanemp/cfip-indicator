// ============================================================================
// CFIP Indicator — BrokerProtectionExecution.cs
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
                        
                                        int positionDirection =
                                            position.TradeType == TradeType.Buy
                                                ? 1
                                                : -1;

                                        double market =
                                            positionDirection == 1
                                                ? Symbol.Bid
                                                : Symbol.Ask;

                                        bool brokerStopValid =
                                            position.StopLoss.HasValue &&
                                            IsFinitePositive(
                                                position.StopLoss.Value) &&
                                            IsValidManagedStop(
                                                positionDirection,
                                                position.EntryPrice,
                                                market,
                                                position.StopLoss.Value);

                                        bool brokerTargetValid =
                                            position.TakeProfit.HasValue &&
                                            IsFinitePositive(
                                                position.TakeProfit.Value) &&
                                            IsValidTarget(
                                                positionDirection,
                                                position.EntryPrice,
                                                position.TakeProfit.Value);

                                        bool stopConfirmed =
                                            brokerStopValid;

                                        bool targetConfirmed =
                                            !SyncBrokerTakeProfit ||
                                            brokerTargetValid;

                                        bool mutationRequired = false;
                                        bool mutationSucceeded = true;

                                        bool desiredStopValid =
                                            IsValidManagedStop(
                                                positionDirection,
                                                position.EntryPrice,
                                                market,
                                                _plan.Stop);

                                        if (desiredStopValid)
                                        {
                                            double normalizedStop =
                                                NormalizePrice(_plan.Stop);

                                            bool materiallyDifferent =
                                                !brokerStopValid ||
                                                Math.Abs(
                                                    position.StopLoss.Value -
                                                    normalizedStop) >=
                                                Math.Max(
                                                    Symbol.TickSize,
                                                    Symbol.PipSize * 0.25);

                                            if (materiallyDifferent)
                                            {
                                                mutationRequired = true;

                                                bool stopMutationSucceeded =
                                                    TryModifyStopLoss(
                                                        position,
                                                        normalizedStop,
                                                        "LIVE PROTECTION • SL");

                                                mutationSucceeded =
                                                    stopMutationSucceeded &&
                                                    mutationSucceeded;

                                                stopConfirmed =
                                                    stopMutationSucceeded;
                                            }
                                        }

                                        double target = 0;

                                        if (SyncBrokerTakeProfit)
                                        {
                                            target =
                                                AutoTarget(
                                                    _plan,
                                                    EffectiveAutoTpStage());

                                            bool desiredTargetValid =
                                                IsValidTarget(
                                                    positionDirection,
                                                    position.EntryPrice,
                                                    target);

                                            if (desiredTargetValid)
                                            {
                                                bool move = true;

                                                if (PreventBrokerTpBackwardMove &&
                                                    position.TakeProfit.HasValue &&
                                                    brokerTargetValid)
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
                                                    (!brokerTargetValid ||
                                                     Math.Abs(
                                                         position.TakeProfit.HasValue
                                                            ? position.TakeProfit.Value -
                                                              normalizedTarget
                                                            : double.MaxValue) >=
                                                     Math.Max(
                                                         Symbol.TickSize,
                                                         Symbol.PipSize * 0.25));

                                                if (materiallyDifferent)
                                                {
                                                    mutationRequired = true;

                                                    bool targetMutationSucceeded =
                                                        TryModifyTakeProfit(
                                                            position,
                                                            normalizedTarget,
                                                            "LIVE PROTECTION • TP");

                                                    mutationSucceeded =
                                                        targetMutationSucceeded &&
                                                        mutationSucceeded;

                                                    targetConfirmed =
                                                        targetMutationSucceeded;
                                                }
                                            }
                                        }

                                        if (!stopConfirmed ||
                                            !targetConfirmed)
                                        {
                                            _brokerProtectionRecoveryRequired = true;

                                            SetLifecycleState(
                                                LifecycleState.RecoveryRequired,
                                                mutationRequired
                                                    ? "BROKER PROTECTION MUTATION REJECTED"
                                                    : "BROKER PROTECTION MISSING OR INVALID");

                                            if (mutationRequired &&
                                                !mutationSucceeded)
                                            {
                                                SendUnifiedAlert(
                                                    "PROTECTION-SYNC-FAILED|" +
                                                    position.Id,
                                                    "CFIP BROKER PROTECTION SYNC REJECTED | #" +
                                                    position.Id,
                                                    positionDirection,
                                                    true);
                                            }
                                        }
                                        else
                                        {
                                            _brokerProtectionRecoveryRequired = false;

                                            SetLifecycleState(
                                                LifecycleState.LivePosition,
                                                "LIVE POSITION • BROKER STATE SYNCHRONIZED");
                                        }
                        
                                        _lastBrokerModifyUtc =
                                            TimeInUtc;
                        
                                        break;
                                    }
                                }

    }
}
