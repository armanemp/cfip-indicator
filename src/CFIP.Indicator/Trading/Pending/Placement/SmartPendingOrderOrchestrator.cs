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

                                            // Refresh quote-sensitive actionability immediately
                                            // before pending evaluation. Pending eligibility itself
                                            // remains independent of ActionableNow because a future
                                            // Stop/Limit entry is intentionally not a market-entry state.
                                            RefreshLiveDecisionActionability(
                                                closedM5);

                                            string capacityReason;
                                            if (!ValidateSingleExecutionCapacity(
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
                                
                                
                                            if (ManagedPendingOrderCount() > 0)
                                            {
                                                _autoOrdersBlockReason =
                                                    "PENDING ORDER ALREADY EXISTS";
                                                return;
                                            }

                                            bool continuationStrong =
                                                _decision != null &&
                                                TrendContinuationStrong();

                                            bool reversalStrong =
                                                _reaction != null &&
                                                ReversalSetupStrong();

                                            int pendingDirection =
                                                continuationStrong
                                                    ? _decision.Direction
                                                    : reversalStrong
                                                        ? _reaction.Direction
                                                        : 0;
                                
                                            if (pendingDirection != 0)
                                            {
                                                RangeSignalQualityResult rangeQuality =
                                                    EvaluateRangeSignalQuality(
                                                        closedM5,
                                                        pendingDirection,
                                                        continuationStrong && _decision != null
                                                            ? _decision.Confidence
                                                            : _reaction == null
                                                                ? 0
                                                                : _reaction.Confidence,
                                                        continuationStrong && _decision != null
                                                            ? _decision.SmartQuality
                                                            : _reaction == null
                                                                ? 0
                                                                : _reaction.SmartQuality,
                                                        _decision == null
                                                            ? 0
                                                            : _decision.Edge,
                                                        continuationStrong && _decision != null
                                                            ? _decision.IndependentEvidence
                                                            : _reaction == null
                                                                ? 0
                                                                : _reaction.IndependentEvidence,
                                                        continuationStrong && _decision != null
                                                            ? _decision.StructuralConfirmations
                                                            : StructuralConfirmations(
                                                                pendingDirection));

                                                if (!rangeQuality.Allowed)
                                                {
                                                    _autoOrdersBlockReason =
                                                        rangeQuality.Reason;
                                                    return;
                                                }

                                                string pendingSuitabilityReason;
                                
                                                if (!PassesMarketSuitability(
                                                        closedM5,
                                                        pendingDirection,
                                                        out pendingSuitabilityReason,
                                                        true))
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
