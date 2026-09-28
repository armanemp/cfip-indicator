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
        private void ProtectBoundPlanPosition(
                                    Position planPosition)
                                {
                                        int direction =
                                            planPosition.TradeType == TradeType.Buy
                                                ? 1
                                                : -1;

                                        double market =
                                            direction == 1
                                                ? Symbol.Bid
                                                : Symbol.Ask;

                                        bool brokerStopValid =
                                            planPosition.StopLoss.HasValue &&
                                            IsFinitePositive(
                                                planPosition.StopLoss.Value) &&
                                            IsValidManagedStop(
                                                direction,
                                                planPosition.EntryPrice,
                                                market,
                                                planPosition.StopLoss.Value);

                                        bool brokerTargetValid =
                                            planPosition.TakeProfit.HasValue &&
                                            IsFinitePositive(
                                                planPosition.TakeProfit.Value) &&
                                            IsValidTarget(
                                                direction,
                                                planPosition.EntryPrice,
                                                planPosition.TakeProfit.Value);

                                        bool stopConfirmed =
                                            brokerStopValid;

                                        bool targetConfirmed =
                                            !SyncBrokerTakeProfit ||
                                            brokerTargetValid;

                                        bool mutationRequired = false;
                                        bool mutationSucceeded = true;

                                        if (IsValidManagedStop(
                                                direction,
                                                planPosition.EntryPrice,
                                                market,
                                                _plan.Stop))
                                        {
                                            double normalizedStop =
                                                NormalizePrice(_plan.Stop);

                                            if (!brokerStopValid ||
                                                Math.Abs(
                                                    planPosition.StopLoss.Value -
                                                    normalizedStop) >=
                                                Math.Max(
                                                    Symbol.TickSize,
                                                    Symbol.PipSize * 0.25))
                                            {
                                                bool shouldAdvance =
                                                    !brokerStopValid ||
                                                    ProtectionProgressionRule.ShouldAdvanceStop(
                                                        direction,
                                                        NormalizePrice(planPosition.StopLoss.Value),
                                                        normalizedStop);

                                                if (shouldAdvance)
                                                {
                                                    mutationRequired = true;

                                                    bool stopMutationSucceeded =
                                                        TryModifyStopLoss(
                                                            planPosition,
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
                                        }

                                        if (SyncBrokerTakeProfit)
                                        {
                                            double target =
                                                AutoTarget(
                                                    _plan,
                                                    EffectiveAutoTpStage());

                                            if (IsValidTarget(
                                                    direction,
                                                    planPosition.EntryPrice,
                                                    target))
                                            {
                                                double normalizedTarget =
                                                    NormalizePrice(target);

                                                bool materiallyDifferent =
                                                    !brokerTargetValid ||
                                                    Math.Abs(
                                                        planPosition.TakeProfit.HasValue
                                                            ? planPosition.TakeProfit.Value -
                                                              normalizedTarget
                                                            : double.MaxValue) >=
                                                    Math.Max(
                                                        Symbol.TickSize,
                                                        Symbol.PipSize * 0.25);

                                                if (materiallyDifferent &&
                                                    (!brokerTargetValid ||
                                                     ProtectionProgressionRule.ShouldAdvanceTarget(
                                                         direction,
                                                         NormalizePrice(
                                                             planPosition.TakeProfit.Value),
                                                         normalizedTarget,
                                                         PreventBrokerTpBackwardMove)))
                                                {
                                                    mutationRequired = true;

                                                    bool targetMutationSucceeded =
                                                        TryModifyTakeProfit(
                                                            planPosition,
                                                            normalizedTarget,
                                                            "LIVE PROTECTION • TP");

                                                    mutationSucceeded =
                                                        targetMutationSucceeded &&
                                                        mutationSucceeded;

                                                    targetConfirmed =
                                                        targetMutationSucceeded;
                                                }
                                                else if (materiallyDifferent)
                                                {
                                                    targetConfirmed = true;
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
                                                    planPosition.Id,
                                                    "CFIP BROKER PROTECTION SYNC REJECTED | #" +
                                                    planPosition.Id,
                                                    direction,
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
                                        return;
                                }
    }
}
