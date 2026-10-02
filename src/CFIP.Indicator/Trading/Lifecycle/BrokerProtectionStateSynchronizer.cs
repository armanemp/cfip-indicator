// CFIP Indicator — BrokerProtectionStateSynchronizer.cs
// Canonical broker-confirmed protection state adoption.
//
// Broker-confirmed values update the observed broker state immediately.
// The analytical plan stop can only advance protectively; a regression
// becomes a recovery condition instead of rewriting the plan backward.
// The future TP1..TP4 ladder remains separate from the active broker target.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ApplyBrokerConfirmedProtectionState(
            long? positionId,
            double? confirmedEntry,
            double? confirmedStop,
            double? confirmedTarget,
            bool enforceMonotonic)
        {
            if (_plan == null ||
                !_plan.IsLivePosition)
                return false;

            if (positionId.HasValue &&
                _plan.PositionId > 0 &&
                _plan.PositionId != positionId.Value)
                return false;

            bool changed = false;
            int direction = _plan.Direction;

            double previousBrokerStop =
                _activeBrokerStop;

            double previousBrokerTarget =
                _activeBrokerTarget;

            double market =
                direction == 1
                    ? Symbol.Bid
                    : Symbol.Ask;

            if (confirmedEntry.HasValue &&
                IsFinitePositive(confirmedEntry.Value))
            {
                double entry =
                    NormalizePrice(confirmedEntry.Value);

                if (IsFinitePositive(entry))
                {
                    _plan.Entry = entry;
                    changed = true;
                }
            }

            double entryPrice = _plan.Entry;

            if (confirmedStop.HasValue &&
                IsFinitePositive(confirmedStop.Value))
            {
                double brokerStop =
                    NormalizePrice(confirmedStop.Value);

                _activeBrokerStop = brokerStop;

                bool stopHealthy =
                    IsFinitePositive(entryPrice) &&
                    IsFinitePositive(market) &&
                    IsValidManagedStop(
                        direction,
                        entryPrice,
                        market,
                        brokerStop);

                bool brokerStopRegressed =
                    IsFinitePositive(previousBrokerStop) &&
                    !SamePrice(
                        previousBrokerStop,
                        brokerStop) &&
                    !ProtectionProgressionRule.ShouldAdvanceStop(
                        direction,
                        previousBrokerStop,
                        brokerStop);

                if (brokerStopRegressed)
                {
                    if (enforceMonotonic)
                        MarkBrokerProtectionRegression(
                            "BROKER CONFIRMED STOP REGRESSED");
                }
                else if (!stopHealthy)
                {
                    if (enforceMonotonic)
                        MarkBrokerProtectionRegression(
                            "BROKER CONFIRMED STOP INVALID");
                }
                else if (!brokerStopRegressed &&
                         !IsFinitePositive(_plan.Stop))
                {
                    _plan.Stop = brokerStop;
                    RecalculatePlanRR();
                    changed = true;
                }
                else if (SamePrice(
                             _plan.Stop,
                             brokerStop))
                {
                    changed = true;
                }
                else if (!brokerStopRegressed &&
                         ProtectionProgressionRule.ShouldAdvanceStop(
                             direction,
                             _plan.Stop,
                             brokerStop))
                {
                    _plan.Stop = brokerStop;
                    RecalculatePlanRR();
                    changed = true;
                }
                else if (!brokerStopRegressed &&
                         enforceMonotonic)
                {
                    MarkBrokerProtectionRegression(
                        "BROKER CONFIRMED STOP REGRESSED");
                }
            }
            else
            {
                _activeBrokerStop = 0;
            }

            if (confirmedTarget.HasValue &&
                IsFinitePositive(confirmedTarget.Value))
            {
                double brokerTarget =
                    NormalizePrice(confirmedTarget.Value);

                _activeBrokerTarget = brokerTarget;

                bool targetHealthy =
                    IsFinitePositive(entryPrice) &&
                    TargetProgressionRule.IsValid(
                        direction,
                        entryPrice,
                        brokerTarget);

                bool brokerTargetRegressed =
                    IsFinitePositive(previousBrokerTarget) &&
                    !SamePrice(
                        previousBrokerTarget,
                        brokerTarget) &&
                    !ProtectionProgressionRule.ShouldAdvanceTarget(
                        direction,
                        previousBrokerTarget,
                        brokerTarget,
                        true);

                if ((brokerTargetRegressed ||
                     !targetHealthy) &&
                    enforceMonotonic)
                {
                    MarkBrokerProtectionRegression(
                        "BROKER CONFIRMED TARGET INVALID");
                }
            }
            else
            {
                _activeBrokerTarget = 0;
            }

            return changed;
        }

        private void MarkBrokerProtectionRegression(
            string reason)
        {
            _brokerProtectionRecoveryRequired = true;

            if (_lifecycleState != LifecycleState.ExitRequested)
            {
                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    string.IsNullOrWhiteSpace(reason)
                        ? "BROKER PROTECTION REGRESSED"
                        : reason);
            }
        }
    }
}
