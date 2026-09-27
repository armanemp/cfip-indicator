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
        private void CleanupPendingOrdersIfNeeded(
                                            int closedM5)
                                        {
                                            if (!PendingAutoCleanup ||
                                                _lastPendingCleanupM5 == closedM5)
                                                return;
                                
                                            _lastPendingCleanupM5 =
                                                closedM5;
                                
                                            foreach (PendingOrder order in PendingOrders)
                                            {
                                                if (!IsManagedPendingOrder(order))
                                                    continue;
                                
                                                bool stale =
                                                    order.ExpirationTime.HasValue &&
                                                    order.ExpirationTime.Value <=
                                                    TimeInUtc;
                                
                                                int expectedDirection =
                                                    _decision != null
                                                        ? _decision.Direction
                                                        : 0;
                                
                                                if (order.OrderType ==
                                                        PendingOrderType.Limit &&
                                                    ReversalSetupStrong())
                                                {
                                                    expectedDirection =
                                                        _reaction.Direction;
                                                }
                                
                                                bool wrongDirection =
                                                    expectedDirection != 0 &&
                                                    ((order.TradeType == TradeType.Buy &&
                                                      expectedDirection != 1) ||
                                                     (order.TradeType == TradeType.Sell &&
                                                      expectedDirection != -1));
                                
                                                bool reversalSupersedesStop =
                                                    ReversalSetupStrong() &&
                                                    order.OrderType == PendingOrderType.Stop;
                                
                                                if (!stale &&
                                                    !wrongDirection &&
                                                    !reversalSupersedesStop)
                                                    continue;
                                
                                                if (!TryCancelPendingOrder(
                                                        order,
                                                        stale
                                                            ? "STALE PENDING ORDER"
                                                            : wrongDirection
                                                                ? "WRONG DIRECTION PENDING ORDER"
                                                                : "REVERSAL SUPERSEDES STOP"))
                                                {
                                                    SetLifecycleState(
                                                        LifecycleState.RecoveryRequired,
                                                        "PENDING CLEANUP CANCEL REJECTED");
                                
                                                    _autoOrdersBlockReason =
                                                        "PENDING CLEANUP CANCEL REJECTED";
                                                }
                                            }
                                        }
    }
}
