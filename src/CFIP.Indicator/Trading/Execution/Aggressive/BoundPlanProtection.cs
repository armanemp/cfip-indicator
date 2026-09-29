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
                                            IsFinitePositive(
                                                planPosition.StopLoss.Value) &&
                                            IsValidManagedStop(
                                                direction,
                                                planPosition.EntryPrice,
                                                market,
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

                                        double minimumForwardDistance =
                                            Math.Max(
                                                Symbol.PipSize,
                                                atr > 0
                                                    ? atr *
                                                      Math.Max(
                                                          0.05,
                                                          MinimumTpSpacingAtr)
                                                    : Symbol.TickSize);

                                        bool brokerTargetValid =
                                            planPosition.TakeProfit.HasValue &&
                                            IsFinitePositive(
                                                planPosition.TakeProfit.Value) &&
                                            IsValidTarget(
                                                direction,
                                                planPosition.EntryPrice,
                                                planPosition.TakeProfit.Value) &&
                                            LiveExitGeometryRule.ShouldAdvanceTarget(
                                                direction,
                                                0,
                                                planPosition.TakeProfit.Value,
                                                market,
                                                minimumForwardDistance);

                                        bool stopConfirmed =
                                            brokerStopValid;

                                        bool targetConfirmed =
                                            _serverSideTakeProfitLadderActive
                                                ? true
                                                : !SyncBrokerTakeProfit ||
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

                                        if (SyncBrokerTakeProfit &&
                                            !_serverSideTakeProfitLadderActive)
                                        {
                                            double target =
                                                liveTarget;

                                            if (IsValidTarget(
                                                    direction,
                                                    planPosition.EntryPrice,
                                                    target) &&
                                                LiveExitGeometryRule.ShouldAdvanceTarget(
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
