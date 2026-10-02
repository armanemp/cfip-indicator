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
        private int _pendingInvalidationLastM5 = -1;
        private int _pendingInvalidationStreak;
        private string _pendingInvalidationKey = string.Empty;

        private PendingArbiterResult ResolvePendingDecision(
            int closedM5)
        {
            bool continuationEligible =
                _decision != null &&
                TrendContinuationStrong() &&
                PendingModeAllowsStop();

            bool reversalEligible =
                _reaction != null &&
                ReversalSetupStrong() &&
                PendingModeAllowsLimit();

            string continuationBlock = string.Empty;
            string reversalBlock = string.Empty;

            if (continuationEligible)
            {
                RangeSignalQualityResult rangeQuality =
                    EvaluateRangeSignalQuality(
                        closedM5,
                        _decision.Direction,
                        _decision.Confidence,
                        _decision.SmartQuality,
                        _decision.Edge,
                        _decision.IndependentEvidence,
                        _decision.StructuralConfirmations);

                if (!rangeQuality.Allowed)
                {
                    continuationEligible = false;
                    continuationBlock =
                        rangeQuality.Reason;
                }
                else
                {
                    string suitabilityReason;

                    if (!PassesMarketSuitability(
                            closedM5,
                            _decision.Direction,
                            out suitabilityReason,
                            true))
                    {
                        continuationEligible = false;
                        continuationBlock =
                            "SUITABILITY • " +
                            suitabilityReason;
                    }
                }
            }

            if (reversalEligible)
            {
                RangeSignalQualityResult rangeQuality =
                    EvaluateRangeSignalQuality(
                        closedM5,
                        _reaction.Direction,
                        _reaction.ReactionConfirmedQuality,
                        _reaction.ReactionConfirmedQuality,
                        _decision == null
                            ? 0
                            : _decision.Edge,
                        _reaction.ReactionConfirmedEvidence,
                        StructuralConfirmations(
                            _reaction.Direction));

                if (!rangeQuality.Allowed)
                {
                    reversalEligible = false;
                    reversalBlock =
                        rangeQuality.Reason;
                }
                else
                {
                    string suitabilityReason;

                    if (!PassesMarketSuitability(
                            closedM5,
                            _reaction.Direction,
                            out suitabilityReason,
                            true))
                    {
                        reversalEligible = false;
                        reversalBlock =
                            "SUITABILITY • " +
                            suitabilityReason;
                    }
                }
            }

            int continuationScore =
                continuationEligible
                    ? PendingDecisionArbiterRule.ScoreCandidate(
                        _decision.Confidence,
                        _decision.SmartQuality,
                        _decision.TimeframeAgreement,
                        _decision.IndependentEvidence,
                        _decision.StructuralConfirmations)
                    : 0;

            int reversalScore =
                reversalEligible
                    ? PendingDecisionArbiterRule.ScoreCandidate(
                        _reaction.ReactionConfirmedQuality,
                        _reaction.ReactionConfirmedQuality,
                        _reaction.ReactionConfirmedEvidence * 10,
                        _reaction.ReactionConfirmedEvidence,
                        StructuralConfirmations(
                            _reaction.Direction))
                    : 0;

            PendingArbiterResult result =
                PendingDecisionArbiterRule.SelectWinner(
                    continuationEligible,
                    _decision == null
                        ? 0
                        : _decision.Direction,
                    continuationScore,
                    reversalEligible,
                    _reaction == null
                        ? 0
                        : _reaction.Direction,
                    reversalScore,
                    PendingOrderMode);

            if (!result.HasChoice)
            {
                if (!string.IsNullOrWhiteSpace(
                        continuationBlock))
                {
                    _autoOrdersBlockReason =
                        "CONTINUATION • " +
                        continuationBlock;
                }
                else if (!string.IsNullOrWhiteSpace(
                             reversalBlock))
                {
                    _autoOrdersBlockReason =
                        "REVERSAL • " +
                        reversalBlock;
                }
                else
                {
                    _autoOrdersBlockReason =
                        result.Reason;
                }
            }

            return result;
        }

        private void RefreshPendingExecutionIntent(
            int closedM5)
        {
            _lastAutoOrderAttemptUtc =
                TimeInUtc;

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

            PendingOrder existingPending =
                GetManagedPendingOrder();

            if (existingPending != null ||
                ManagedPendingOrderCount() > 0)
            {
                _autoOrdersBlockReason =
                    "PENDING ORDER EXISTS";
                return;
            }

            PendingArbiterResult arbiter =
                ResolvePendingDecision(
                    closedM5);

            if (!arbiter.HasChoice)
            {
                _autoOrdersBlockReason =
                    string.IsNullOrWhiteSpace(
                        _autoOrdersBlockReason)
                        ? "NO ELIGIBLE PENDING SETUP"
                        : _autoOrdersBlockReason;
                return;
            }

            _autoOrdersBlockReason =
                "PENDING • " +
                arbiter.Reason +
                " • C " +
                arbiter.ContinuationScore +
                " / R " +
                arbiter.ReversalScore;

            // Pending Stop is now an analysis-owned execution intent. The cBot
            // performs the broker mutation. Reversal Limit remains on the staged
            // P4D migration path until its owner is moved.
            if (arbiter.Choice ==
                PendingArbiterChoice.ContinuationStop)
            {
                if (PrepareContinuationStopForCbot(
                        closedM5))
                {
                    _autoOrdersBlockReason =
                        "PENDING STOP • READY FOR CBOT";
                }

                return;
            }

            if (arbiter.Choice ==
                PendingArbiterChoice.ReversalLimit)
            {
                if (PrepareReversalLimitForCbot(
                        closedM5))
                {
                    _autoOrdersBlockReason =
                        "PENDING LIMIT • READY FOR CBOT";
                }

                return;
            }

            _autoOrdersBlockReason =
                "PENDING EXECUTION BLOCKED";
        }

        private bool ObservePendingInvalidation(
            int closedM5,
            string key,
            bool invalidated)
        {
            if (!invalidated)
            {
                _pendingInvalidationLastM5 =
                    -1;
                _pendingInvalidationStreak =
                    0;
                _pendingInvalidationKey =
                    string.Empty;
                return false;
            }

            if (!string.Equals(
                    _pendingInvalidationKey,
                    key,
                    StringComparison.Ordinal))
            {
                _pendingInvalidationKey =
                    key;
                _pendingInvalidationStreak = 0;
                _pendingInvalidationLastM5 =
                    -1;
            }

            if (_pendingInvalidationLastM5 == closedM5)
                return
                    PendingDecisionArbiterRule
                        .ShouldCancelAfterHysteresis(
                            true,
                            _pendingInvalidationStreak,
                            2);

            if (_pendingInvalidationLastM5 >= 0 &&
                closedM5 !=
                _pendingInvalidationLastM5 + 1)
            {
                _pendingInvalidationStreak =
                    0;
            }

            _pendingInvalidationStreak++;
            _pendingInvalidationLastM5 =
                closedM5;

            return
                PendingDecisionArbiterRule
                    .ShouldCancelAfterHysteresis(
                        true,
                        _pendingInvalidationStreak,
                        2);
        }
    }
}
