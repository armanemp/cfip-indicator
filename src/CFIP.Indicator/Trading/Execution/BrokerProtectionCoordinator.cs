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
                            int direction)
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
                                double normalizedStop =
                                    NormalizePrice(stop);

                                stopOk =
                                    Math.Abs(
                                        position.StopLoss.Value -
                                        normalizedStop) <
                                    Math.Max(
                                        Symbol.TickSize,
                                        Symbol.PipSize * 0.25) ||
                                    TryModifyStopLoss(
                                        position,
                                        normalizedStop,
                                        context + " • SL");
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
                                currentTargetValid;

                            if (!targetOk &&
                                desiredTargetValid)
                            {
                                targetOk =
                                    TryModifyTakeProfit(
                                        position,
                                        NormalizePrice(target),
                                        context + " • TP");
                            }
                            else if (targetOk &&
                                     desiredTargetValid)
                            {
                                double normalizedTarget =
                                    NormalizePrice(target);

                                targetOk =
                                    Math.Abs(
                                        position.TakeProfit.Value -
                                        normalizedTarget) <
                                    Math.Max(
                                        Symbol.TickSize,
                                        Symbol.PipSize * 0.25) ||
                                    TryModifyTakeProfit(
                                        position,
                                        normalizedTarget,
                                        context + " • TP");
                            }

                            bool protectedOk =
                                stopOk &&
                                targetOk;

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

                            return protectedOk;
                        }
    }
}
