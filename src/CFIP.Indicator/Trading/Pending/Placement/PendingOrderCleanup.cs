using System;
using CFIP.Contracts;
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
        private void CleanupPendingOrdersIfNeeded(
            int closedM5)
        {
            if (!PendingAutoCleanup ||
                _lastPendingCleanupM5 == closedM5)
                return;

            _lastPendingCleanupM5 =
                closedM5;

            PendingArbiterResult arbiter =
                ResolvePendingDecision(
                    closedM5);

            foreach (PendingOrder order in PendingOrders)
            {
                if (!IsManagedPendingOrder(order))
                    continue;

                bool stale =
                    order.ExpirationTime.HasValue &&
                    order.ExpirationTime.Value <=
                    TimeInUtc;

                if (stale)
                {
                    ResetPendingInvalidationHysteresis();

                    ManagementCommandRequestStatus cancelStatus =
                        TryCancelPendingOrder(
                            order,
                            "STALE PENDING ORDER");
                    if (!cancelStatus.IsAccepted())
                    {
                        SetLifecycleState(
                            LifecycleState.RecoveryRequired,
                            "PENDING CLEANUP CANCEL REJECTED");

                        _autoOrdersBlockReason =
                            "PENDING CLEANUP CANCEL REJECTED";
                    }

                    continue;
                }

                bool hasWinner =
                    arbiter.HasChoice;

                bool sameChoice =
                    hasWinner &&
                    PendingDecisionArbiterRule.IsSameChoice(
                        arbiter.Choice,
                        order.OrderType ==
                            PendingOrderType.Stop);

                bool sameDirection =
                    hasWinner &&
                    arbiter.Direction != 0 &&
                    ((order.TradeType ==
                        TradeType.Buy &&
                      arbiter.Direction == 1) ||
                     (order.TradeType ==
                        TradeType.Sell &&
                      arbiter.Direction == -1));

                bool invalidated =
                    !hasWinner ||
                    !sameChoice ||
                    !sameDirection;

                if (!invalidated)
                {
                    ResetPendingInvalidationHysteresis();
                    continue;
                }

                string invalidationKey =
                    order.OrderType +
                    "|" +
                    order.TradeType;

                bool shouldCancel =
                    ObservePendingInvalidation(
                        closedM5,
                        invalidationKey,
                        true);

                if (!shouldCancel)
                {
                    _autoOrdersBlockReason =
                        !hasWinner
                            ? "PENDING WAITING FOR STABLE SETUP"
                            : !sameDirection
                                ? "PENDING DIRECTION CHANGED • HYSTERESIS"
                                : "PENDING POLICY CHANGED • HYSTERESIS";
                    continue;
                }

                string reason =
                    !hasWinner
                        ? "NO ELIGIBLE PENDING SETUP"
                        : !sameDirection
                            ? "WRONG DIRECTION PENDING ORDER"
                            : "PENDING POLICY SUPERSEDED";

                if (!TryCancelPendingOrder(
                        order,
                        reason))
                {
                    SetLifecycleState(
                        LifecycleState.RecoveryRequired,
                        "PENDING CLEANUP CANCEL REJECTED");

                    _autoOrdersBlockReason =
                        "PENDING CLEANUP CANCEL REJECTED";
                }
                else
                {
                    ResetPendingInvalidationHysteresis();
                }
            }
        }

        private void ResetPendingInvalidationHysteresis()
        {
            _pendingInvalidationLastM5 =
                -1;
            _pendingInvalidationStreak =
                0;
            _pendingInvalidationKey =
                string.Empty;
        }
    }
}
