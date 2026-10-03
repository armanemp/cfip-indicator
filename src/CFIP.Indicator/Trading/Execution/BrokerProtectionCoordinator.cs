// CFIP Indicator — BrokerProtectionCoordinator.cs
// Single-responsibility broker protection reconciliation.

using System;
using CFIP.Contracts;
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
                                MinimumLiveTargetDistancePrice(
                                    direction,
                                    atr);

                            if (IsLiveTargetBrokerSafe(
                                    direction,
                                    position.EntryPrice,
                                    market,
                                    requestedTarget,
                                    atr))
                                return NormalizePrice(requestedTarget);

                            return FurthestForwardPlanTarget(
                                market,
                                minimumForwardDistance);
                        }

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

                            double atr =
                                _m5Bars == null
                                    ? 0
                                    : Atr(
                                        _m5Bars,
                                        Math.Max(
                                            1,
                                            _m5Bars.Count - 2));

                            bool currentStopValid =
                                position.StopLoss.HasValue &&
                                IsExistingManagedStopHealthy(
                                    direction,
                                    position.EntryPrice,
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
                                ManagementCommandRequestStatus stopStatus =
                                    RequestModifyStopLoss(
                                        position,
                                        NormalizePrice(stop),
                                        context + " • SL");
                                stopOk =
                                    stopStatus.IsBrokerConfirmed();
                            }
                            else if (currentStopValid &&
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
                                    ManagementCommandRequestStatus stopStatus =
                                        RequestModifyStopLoss(
                                            position,
                                            normalizedStop,
                                            context + " • SL");
                                    stopOk =
                                        stopStatus.IsBrokerConfirmed();
                                }
                                else
                                {
                                    // The current broker stop is already more protective.
                                    // Broker state remains authoritative.
                                    stopOk = true;
                                }
                            }

                            double effectiveTarget =
                                ResolveLiveProtectionTarget(
                                    position,
                                    target,
                                    atr);

                            bool currentTargetValid =
                                position.TakeProfit.HasValue &&
                                IsLiveTargetBrokerSafe(
                                    direction,
                                    position.EntryPrice,
                                    market,
                                    position.TakeProfit.Value,
                                    atr);

                            bool desiredTargetValid =
                                IsLiveTargetBrokerSafe(
                                    direction,
                                    position.EntryPrice,
                                    market,
                                    effectiveTarget,
                                    atr);

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
                                ManagementCommandRequestStatus targetStatus =
                                    RequestModifyTakeProfit(
                                        position,
                                        NormalizePrice(effectiveTarget),
                                        context + " • TP");
                                targetOk =
                                    targetStatus.IsBrokerConfirmed();
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
                                             MinimumLiveTargetDistancePrice(
                                                 direction,
                                                 atr)))
                                {
                                    ManagementCommandRequestStatus targetStatus =
                                        RequestModifyTakeProfit(
                                            position,
                                            normalizedTarget,
                                            context + " • TP");
                                    targetOk =
                                        targetStatus.IsBrokerConfirmed();
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
