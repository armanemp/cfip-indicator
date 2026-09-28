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
                        
                                        bool mutationRequired = false;
                                        bool mutationSucceeded = true;
                                        bool stopConfirmed = false;
                                        bool targetConfirmed =
                                            !SyncBrokerTakeProfit;

                                        int positionDirection =
                                            position.TradeType == TradeType.Buy
                                                ? 1
                                                : -1;

                                        double market =
                                            positionDirection == 1
                                                ? Symbol.Bid
                                                : Symbol.Ask;

                                        if (IsValidManagedStop(
                                                positionDirection,
                                                position.EntryPrice,
                                                market,
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
                                            else
                                            {
                                                stopConfirmed = true;
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
                                                else
                                                {
                                                    targetConfirmed = true;
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
                                        else if (stopConfirmed &&
                                                 targetConfirmed)
                                        {
                                            _brokerProtectionRecoveryRequired =
                                                false;

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
