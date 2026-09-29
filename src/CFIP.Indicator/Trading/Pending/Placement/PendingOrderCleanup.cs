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
                                
                                            MarketRegimeSnapshot activeRegime =
                                                GetActiveM5Regime(
                                                    closedM5);

                                            foreach (PendingOrder order in PendingOrders)
                                            {
                                                if (!IsManagedPendingOrder(order))
                                                    continue;

                                                bool rangeInvalid =
                                                    activeRegime != null &&
                                                    activeRegime.Regime == "COMPRESSION";

                                                if (!rangeInvalid &&
                                                    activeRegime != null &&
                                                    activeRegime.Regime == "RANGE")
                                                {
                                                    bool qualifiedRangePending = false;

                                                    if (order.OrderType ==
                                                        PendingOrderType.Limit &&
                                                        ReversalSetupStrong())
                                                    {
                                                        qualifiedRangePending = true;
                                                    }
                                                    else if (order.OrderType ==
                                                             PendingOrderType.Stop &&
                                                             TrendContinuationStrong())
                                                    {
                                                        qualifiedRangePending =
                                                            EvaluateRangeSignalQuality(
                                                                closedM5,
                                                                _decision.Direction,
                                                                _decision.Confidence,
                                                                _decision.SmartQuality,
                                                                _decision.Edge,
                                                                _decision.IndependentEvidence,
                                                                _decision.StructuralConfirmations).Allowed;
                                                    }

                                                    rangeInvalid =
                                                        !qualifiedRangePending;
                                                }

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
                                
                                                if (!rangeInvalid &&
                                                    !stale &&
                                                    !wrongDirection &&
                                                    !reversalSupersedesStop)
                                                    continue;
                                
                                                if (!TryCancelPendingOrder(
                                                        order,
                                                        rangeInvalid
                                                            ? "RANGE / COMPRESSION NO-TRADE"
                                                            : stale
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
