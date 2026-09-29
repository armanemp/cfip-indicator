// CFIP Indicator — BrokerProtectionCoordinator.cs
// Single-responsibility broker protection reconciliation.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double ResolveLiveProtectionTarget(
                            Position position,
                            double requestedTarget,
                            double atr)
                        {
                            if (position == null ||
                                _plan == null)
                                return 0;

                            int direction =
                                position.TradeType == TradeType.Buy
                                    ? 1
                                    : -1;

                            double market =
                                direction == 1
                                    ? Symbol.Bid
                                    : Symbol.Ask;

                            double minimumForwardDistance =
                                MinimumLiveTargetDistancePrice(direction, atr);

                            bool desiredTargetValid =
                                IsFinitePositive(effectiveTarget) &&
                                IsValidTarget(
                                    direction,
                                    position.EntryPrice,
                                    effectiveTarget) &&
                                LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                                    direction,
                                    0,
                                    effectiveTarget,
                                    market,
                                    MinimumLiveTargetDistancePrice(direction, atr);

                            bool serverLadderTargetValid =
                                _serverSideTakeProfitLadderActive &&
                                IsLiveTargetBrokerSafe(
                                    direction,
                                    position.EntryPrice,
                                    market,
                                    _activeBrokerTarget,
                                    atr);

                            bool targetOk =
                                serverLadderTargetValid ||
                                currentTargetValid;

                            if (!_serverSideTakeProfitLadderActive &&
                                !targetOk &&
                                desiredTargetValid)
                            {
                                targetOk =
                                    TryModifyTakeProfit(
                                        position,
                                        NormalizePrice(effectiveTarget),
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
                                    NormalizePrice(effectiveTarget);

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
                                else if (LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                                             direction,
                                             currentTarget,
                                             normalizedTarget,
                                             market,
                                             MinimumLiveTargetDistancePrice(direction, atr))
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
