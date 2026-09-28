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

                                    Position planPosition = null;

                                    if (_plan.PositionId > 0)
                                    {
                                        foreach (Position position in Positions)
                                        {
                                            if (position != null &&
                                                position.SymbolName == SymbolName &&
                                                position.Id == _plan.PositionId &&
                                                IsManagedPosition(position))
                                            {
                                                planPosition = position;
                                                break;
                                            }
                                        }
                                    }

                                    // The active plan may manage only its bound broker position.
                                    // Never apply its stop/target to another position just because
                                    // that position happens to share the managed label.
                                    if (planPosition != null)
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

                                    if (!AutoProtectBrokerPositions)
                                        return;

                                    string label =
                                        string.IsNullOrWhiteSpace(
                                            ManagedPositionLabel)
                                            ? NormalizeLabel()
                                            : ManagedPositionLabel.Trim();

                                    foreach (Position position in Positions)
                                    {
                                        if (position == null ||
                                            position.SymbolName != SymbolName ||
                                            position.Label != label)
                                            continue;

                                        int direction =
                                            position.TradeType == TradeType.Buy
                                                ? 1
                                                : -1;

                                        double atr =
                                            _m5Bars == null
                                                ? 0
                                                : Atr(
                                                    _m5Bars,
                                                    Math.Max(
                                                        1,
                                                        closedM5));

                                        if (!IsFinitePositive(atr))
                                            atr =
                                                Math.Max(
                                                    Symbol.PipSize * 20,
                                                    Math.Abs(
                                                        Symbol.Ask -
                                                        Symbol.Bid) * 10);

                                        string stopSource;
                                        int stopQuality;

                                        double stop =
                                            position.StopLoss.HasValue &&
                                            IsValidManagedStop(
                                                direction,
                                                position.EntryPrice,
                                                direction == 1
                                                    ? Symbol.Bid
                                                    : Symbol.Ask,
                                                position.StopLoss.Value)
                                                ? position.StopLoss.Value
                                                : BuildStructuralStop(
                                                    Math.Max(1, closedM5),
                                                    direction,
                                                    position.EntryPrice,
                                                    atr,
                                                    out stopSource,
                                                    out stopQuality);

                                        if (!IsValidStop(
                                                direction,
                                                position.EntryPrice,
                                                stop))
                                        {
                                            double fallbackRisk =
                                                atr *
                                                Math.Max(
                                                    0.10,
                                                    FallbackSlAtr);

                                            stop =
                                                direction == 1
                                                    ? position.EntryPrice - fallbackRisk
                                                    : position.EntryPrice + fallbackRisk;

                                            stop =
                                                NormalizePrice(stop);
                                        }

                                        if (!IsValidStop(
                                                direction,
                                                position.EntryPrice,
                                                stop))
                                            continue;

                                        double target =
                                            position.TakeProfit.HasValue &&
                                            IsValidTarget(
                                                direction,
                                                position.EntryPrice,
                                                position.TakeProfit.Value)
                                                ? position.TakeProfit.Value
                                                : SelectStructuralAutoTarget(
                                                    Math.Max(1, closedM5),
                                                    direction,
                                                    position.EntryPrice,
                                                    stop,
                                                    atr,
                                                    EffectiveAutoTpStage());

                                        if (!IsValidTarget(
                                                direction,
                                                position.EntryPrice,
                                                target))
                                        {
                                            double risk =
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    Math.Abs(
                                                        position.EntryPrice -
                                                        stop));

                                            target =
                                                direction == 1
                                                    ? position.EntryPrice +
                                                      risk *
                                                      Math.Max(
                                                          1,
                                                          MinimumRequiredRR())
                                                    : position.EntryPrice -
                                                      risk *
                                                      Math.Max(
                                                          1,
                                                          MinimumRequiredRR());
                                        }

                                        bool protectedOk =
                                            EnsureBrokerProtectionForPosition(
                                                position,
                                                stop,
                                                target,
                                                "ORPHAN MANAGED POSITION",
                                                direction,
                                                false);

                                        _lastBrokerModifyUtc =
                                            TimeInUtc;

                                        if (!protectedOk)
                                            return;
                                    }
                                }
    }
}
