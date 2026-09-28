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
        private void TrySmartPendingOrders(int closedM5)
                                        {
                                            _lastAutoOrderAttemptUtc =
                                                TimeInUtc;
                                
                                            if (AutomaticOrdersEnabled &&
                                                DailyLossLimitHit(TimeInUtc))
                                            {
                                                _autoOrdersBlockReason =
                                                    "DAILY LOSS LIMIT";
                                
                                                CancelAllOrders();
                                                return;
                                            }
                                
                                            if (!AutomaticOrdersEnabled)
                                            {
                                                _autoOrdersBlockReason =
                                                    "DISABLED";
                                                return;
                                            }

                                            string capacityReason;
                                            if (!ValidateConfiguredPositionCapacity(
                                                    out capacityReason))
                                            {
                                                _autoOrdersBlockReason =
                                                    capacityReason;
                                                return;
                                            }
                                
                                            if (GetManagedPosition() != null)
                                            {
                                                _autoOrdersBlockReason =
                                                    "MANAGED POSITION ACTIVE";
                                                return;
                                            }
                                
                                            if (_plan != null &&
                                                !_plan.IsLivePosition)
                                            {
                                                _autoOrdersBlockReason =
                                                    "MARKET PLAN ACTIVE";
                                                return;
                                            }
                                
                                            CleanupPendingOrdersIfNeeded(closedM5);
                                
                                            PendingOrder existingPending =
                                                GetManagedPendingOrder();
                                
                                            if (existingPending != null)
                                            {
                                                _autoOrdersBlockReason =
                                                    "PENDING ORDER EXISTS";
                                                return;
                                            }
                                
                                            if (ManagedPositionCount() >=
                                                Math.Max(1, MaximumOpenPositions))
                                            {
                                                _autoOrdersBlockReason =
                                                    "MAX OPEN POSITIONS";
                                                return;
                                            }
                                
                                            if (ManagedPendingOrderCount() > 0)
                                            {
                                                _autoOrdersBlockReason =
                                                    "PENDING ORDER ALREADY EXISTS";
                                                return;
                                            }
                                
                                            if (DailyLossLimitHit(TimeInUtc))
                                            {
                                                _autoOrdersBlockReason =
                                                    "DAILY LOSS LIMIT";
                                                return;
                                            }
                                
                                            int pendingDirection =
                                                TrendContinuationStrong()
                                                    ? _decision.Direction
                                                    : ReversalSetupStrong()
                                                        ? _reaction.Direction
                                                        : 0;
                                
                                            if (pendingDirection != 0)
                                            {
                                                string pendingSuitabilityReason;
                                
                                                if (!PassesMarketSuitability(
                                                        closedM5,
                                                        pendingDirection,
                                                        out pendingSuitabilityReason))
                                                {
                                                    _autoOrdersBlockReason =
                                                        "SUITABILITY • " +
                                                        pendingSuitabilityReason;
                                                    return;
                                                }
                                            }
                                
                                            if (pendingDirection != 0 &&
                                                !EnsureTradingPermission())
                                            {
                                                _autoOrdersBlockReason = "TRADING PERMISSION";
                                                return;
                                            }
                                
                                            if (continuationStrong &&
                                                PendingModeAllowsStop())
                                            {
                                                if (PlaceContinuationStop(closedM5))
                                                {
                                                    _autoOrdersBlockReason =
                                                        "ORDER PLACED";
                                                    return;
                                                }
                                            }
                                
                                            if (reversalStrong &&
                                                PendingModeAllowsLimit())
                                            {
                                                CheckReversalProtection();
                                
                                                if (PlaceReversalLimit(closedM5))
                                                {
                                                    _autoOrdersBlockReason =
                                                        "ORDER PLACED";
                                                    return;
                                                }
                                            }
                                
                                            if (pendingDirection == 0)
                                            {
                                                _autoOrdersBlockReason =
                                                    "NO ELIGIBLE PENDING SETUP";
                                                return;
                                            }
                                
                                            if (string.IsNullOrWhiteSpace(
                                                    _autoOrdersBlockReason) ||
                                                _autoOrdersBlockReason ==
                                                    "NOT EVALUATED")
                                            {
                                                _autoOrdersBlockReason =
                                                    "PENDING EXECUTION BLOCKED";
                                            }
                                        }
    }
}
