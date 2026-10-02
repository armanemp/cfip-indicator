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

                                        AdoptServerSideTakeProfitLadder(
                                            planPosition);

                                        bool brokerStopValid =
                                            planPosition.StopLoss.HasValue &&
                                            IsExistingManagedStopHealthy(
                                                direction,
                                                planPosition.EntryPrice,
                                                planPosition.StopLoss.Value);

                                        double atr =
                                            _m5Bars == null || _m5Bars.Count < 3
                                                ? 0
                                                : Atr(
                                                    _m5Bars,
                                                    _m5Bars.Count - 2);

                                        double liveTarget =
                                            ResolveLiveProtectionTarget(
                                                planPosition,
                                                AutoTarget(
                                                    _plan,
                                                    EffectiveAutoTpStage()),
                                                atr);

                                        double desiredStop =
                                            IsValidManagedStop(
                                                direction,
                                                planPosition.EntryPrice,
                                                market,
                                                _pendingProtectedStopCandidate) &&
                                            ProtectionProgressionRule.ShouldAdvanceStop(
                                                direction,
                                                _plan.Stop,
                                                _pendingProtectedStopCandidate)
                                                ? NormalizePrice(
                                                    _pendingProtectedStopCandidate)
                                                : NormalizePrice(
                                                    _plan.Stop);

                                        double minimumForwardDistance =
                                            MinimumLiveTargetDistancePrice(
                                                direction,
                                                atr);

                                        bool brokerTargetValid =
                                            planPosition.TakeProfit.HasValue &&
                                            IsLiveTargetBrokerSafe(
                                                direction,
                                                planPosition.EntryPrice,
                                                market,
                                                planPosition.TakeProfit.Value,
                                                atr);

                                        bool stopConfirmed =
                                            brokerStopValid;

                                        bool serverLadderTargetValid =
                                            _serverSideTakeProfitLadderActive &&
                                            IsLiveTargetBrokerSafe(
                                                direction,
                                                planPosition.EntryPrice,
                                                market,
                                                _activeBrokerTarget,
                                                atr);

                                        bool targetConfirmed =
                                            serverLadderTargetValid
                                                ? true
                                                : !SyncBrokerTakeProfit ||
                                                  brokerTargetValid;

                                        bool mutationRequired = false;
                                        bool mutationSucceeded = true;
                                        bool stopMutationSucceeded = false;

                                        if (IsValidManagedStop(
                                                direction,
                                                planPosition.EntryPrice,
                                                market,
                                                desiredStop))
                                        {
                                            double normalizedStop =
                                                NormalizePrice(desiredStop);

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

                                                    stopMutationSucceeded =
                                                        TryModifyStopLoss(
                                                            planPosition,
                                                            normalizedStop,
                                                            "LIVE PROTECTION • SL");

                                                    mutationSucceeded =
                                                        stopMutationSucceeded &&
                                                        mutationSucceeded;

                                                    stopConfirmed =
                                                        stopMutationSucceeded;

                                                    if (stopMutationSucceeded)
                                                    {
                                                        // TradeResult success is the mutation confirmation.
                                                        // Only now may the plan adopt the new protected stop.
                                                        ApplyBrokerConfirmedProtectionState(
                                                            planPosition.Id,
                                                            planPosition.EntryPrice,
                                                            planPosition.StopLoss,
                                                            planPosition.TakeProfit,
                                                            true);
                                                        _pendingProtectedStopCandidate =
                                                            0;
                                                    }
                                                }
                                                else
                                                {
                                                    stopConfirmed = true;
                                                }
                                            }
                                        }

                                        if (stopConfirmed)
                                        {
                                            if (!stopMutationSucceeded &&
                                                brokerStopValid &&
                                                planPosition.StopLoss.HasValue)
                                            {
                                                ApplyBrokerConfirmedProtectionState(
                                                    planPosition.Id,
                                                    planPosition.EntryPrice,
                                                    planPosition.StopLoss,
                                                    planPosition.TakeProfit,
                                                    true);
                                            }
                                        }

                                        if (SyncBrokerTakeProfit &&
                                            !_serverSideTakeProfitLadderActive)
                                        {
                                            double target =
                                                liveTarget;

                                            if (IsValidTarget(
                                                    direction,
                                                    planPosition.EntryPrice,
                                                    target) &&
                                                LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                                                    direction,
                                                    0,
                                                    target,
                                                    market,
                                                    minimumForwardDistance))
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

                                                bool configuredTpProgression =
                                                    !PreventBrokerTpBackwardMove ||
                                                    !brokerTargetValid ||
                                                    ProtectionProgressionRule.ShouldAdvanceTarget(
                                                        direction,
                                                        NormalizePrice(
                                                            planPosition.TakeProfit.Value),
                                                        normalizedTarget,
                                                        true);

                                                if (materiallyDifferent &&
                                                    configuredTpProgression &&
                                                    (!brokerTargetValid ||
                                                     LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                                                         direction,
                                                         NormalizePrice(
                                                             planPosition.TakeProfit.Value),
                                                         normalizedTarget,
                                                         market,
                                                         minimumForwardDistance)))
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
