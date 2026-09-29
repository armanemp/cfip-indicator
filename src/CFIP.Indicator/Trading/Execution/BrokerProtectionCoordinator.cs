// CFIP Indicator — BrokerProtectionCoordinator.cs
// Single-responsibility broker protection reconciliation.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool EnsureBrokerProtectionForPosition(
                            Position position,
                            double stop,
                            double target,
                            string context,
                            int direction,
                            bool manageLifecycleState = true)
                        {
                            if (position == null ||
                                direction != 1 &&
                                direction != -1)
                                return false;

                            double market =
                                direction == 1
                                    ? Symbol.Bid
                                    : Symbol.Ask;

                            if (!IsFinitePositive(market) ||
                                !IsFinitePositive(position.EntryPrice))
                                return false;

                            AdoptServerSideTakeProfitLadder(
                                position);

                            bool currentStopValid =
                                position.StopLoss.HasValue &&
                                IsFinitePositive(
                                    position.StopLoss.Value) &&
                                IsValidManagedStop(
                                    direction,
                                    position.EntryPrice,
                                    market,
                                    position.StopLoss.Value);

                            bool desiredStopValid =
                                IsValidManagedStop(
                                    direction,
                                    position.EntryPrice,
                                    market,
                                    stop);

                            bool stopOk =
                                currentStopValid;

                            if (!stopOk &&
                                desiredStopValid)
                            {
                                stopOk =
                                    TryModifyStopLoss(
                                        position,
                                        NormalizePrice(stop),
                                        context + " • SL");
                            }
                            else if (stopOk &&
                                     desiredStopValid)
                            {
                                double currentStop =
                                    NormalizePrice(
                                        position.StopLoss.Value);

                                double normalizedStop =
                                    NormalizePrice(stop);

                                bool materiallyDifferent =
                                    Math.Abs(
                                        currentStop -
                                        normalizedStop) >=
                                    Math.Max(
                                        Symbol.TickSize,
                                        Symbol.PipSize * 0.25);

                                if (!materiallyDifferent)
                                {
                                    stopOk = true;
                                }
                                else if (ProtectionProgressionRule.ShouldAdvanceStop(
                                             direction,
                                             currentStop,
                                             normalizedStop))
                                {
                                    stopOk =
                                        TryModifyStopLoss(
                                            position,
                                            normalizedStop,
                                            context + " • SL");
                                }
                                else
                                {
                                    // The current broker stop is already more protective.
                                    // Broker state remains authoritative.
                                    stopOk = true;
                                }
                            }

                            bool currentTargetValid =
                                position.TakeProfit.HasValue &&
                                IsFinitePositive(
                                    position.TakeProfit.Value) &&
                                IsValidTarget(
                                    direction,
                                    position.EntryPrice,
                                    position.TakeProfit.Value);

                            bool desiredTargetValid =
                                IsValidTarget(
                                    direction,
                                    position.EntryPrice,
                                    target);

                            bool targetOk =
                                _serverSideTakeProfitLadderActive ||
                                currentTargetValid;

                            if (!_serverSideTakeProfitLadderActive &&
                                !targetOk &&
                                desiredTargetValid)
                            {
                                targetOk =
                                    TryModifyTakeProfit(
                                        position,
                                        NormalizePrice(target),
                                        context + " • TP");
                            }
                            else if (!_serverSideTakeProfitLadderActive &&
                                     targetOk &&
                                     desiredTargetValid)
                            {
                                double currentTarget =
                                    NormalizePrice(
                                        position.TakeProfit.Value);

                                double normalizedTarget =
                                    NormalizePrice(target);

                                bool materiallyDifferent =
                                    Math.Abs(
                                        currentTarget -
                                        normalizedTarget) >=
                                    Math.Max(
                                        Symbol.TickSize,
                                        Symbol.PipSize * 0.25);

                                if (!materiallyDifferent)
                                {
                                    targetOk = true;
                                }
                                else if (ProtectionProgressionRule.ShouldAdvanceTarget(
                                             direction,
                                             currentTarget,
                                             normalizedTarget,
                                             PreventBrokerTpBackwardMove))
                                {
                                    targetOk =
                                        TryModifyTakeProfit(
                                            position,
                                            normalizedTarget,
                                            context + " • TP");
                                }
                                else
                                {
                                    // The current broker target is already at least as
                                    // progressive under the configured policy.
                                    targetOk = true;
                                }
                            }

                            bool protectedOk =
                                stopOk &&
                                targetOk;

                            if (manageLifecycleState)
                            {
                                _brokerProtectionRecoveryRequired =
                                    !protectedOk;

                                if (!protectedOk)
                                {
                                    SetLifecycleState(
                                        LifecycleState.RecoveryRequired,
                                        context +
                                        " • BROKER PROTECTION MISSING OR REJECTED");

                                    SendUnifiedAlert(
                                        "PROTECTION-REJECTED|" +
                                        position.Id,
                                        "CFIP BROKER PROTECTION REJECTED | #" +
                                        position.Id,
                                        direction,
                                        true);
                                }
                            }
                            else if (!protectedOk)
                            {
                                SendUnifiedAlert(
                                    "PROTECTION-REJECTED|" +
                                    position.Id,
                                    "CFIP BROKER PROTECTION REJECTED | #" +
                                    position.Id,
                                    direction,
                                    true);
                            }

                            return protectedOk;
                        }
    }
}
