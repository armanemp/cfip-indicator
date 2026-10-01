using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ProcessDecisionAlerts(
            int closedM5)
        {
            // Decision/entry alerts must never consume a broker lifecycle snapshot
            // that was not reconciled at the start of the same calculation cycle.
            if (_decision == null ||
                !_brokerStateReconciledThisCycle)
                return;

            ProcessRestrictionAlert(
                closedM5);

            ProcessCanonicalActionableEntryAlert(
                closedM5);
        }

        private void ProcessDecisionOwnedWatchReactionAlerts(
            int closedM5)
        {
            // WATCH/REACTION alerts are analysis/alert state, not chart state.
            // They must be evaluated even when chart presentation is disabled,
            // skipped or optimized away.
            if (_decision == null ||
                !_brokerStateReconciledThisCycle)
                return;

            RefreshLiveDecisionActionability(
                closedM5);

            ProcessDecisionOwnedWatchAlert(
                closedM5);

            ProcessDecisionOwnedReactionAlert();
        }

        private void ProcessDecisionOwnedWatchAlert(
            int closedM5)
        {
            bool hasPlan =
                _plan != null;

            bool hasPendingOrder =
                GetManagedPendingOrder() != null;

            bool hasLivePosition =
                _plan != null &&
                _plan.IsLivePosition;

            if (_lastEarlyAlertM5 ==
                    closedM5 ||
                !WatchReactionAlertRule.IsWatchAlertEligible(
                    AlertOnEarlyWatch,
                    _decision.EntryAllowed,
                    _decision.ActionableNow,
                    hasPlan,
                    hasPendingOrder,
                    hasLivePosition,
                    _decision.Direction,
                    _decision.Confidence,
                    _decision.SmartQuality,
                    _decision.TimeframeAgreement,
                    _decision.IndependentEvidence,
                    _decision.StructuralConfirmations,
                    MinimumConfidence,
                    MinimumSmartQuality,
                    SmartQualityThreshold,
                    MinimumTimeframeAgreement,
                    SmartMinimumTimeframeAgreement,
                    MinimumIndependentEvidence,
                    MinimumStructuralConfirmations))
                return;

            int direction =
                _decision.Direction;

            string directionText =
                direction == 1
                    ? "BUY"
                    : "SELL";

            SendUnifiedAlert(
                WatchReactionAlertRule.BuildWatchAlertKey(
                    closedM5,
                    direction),
                "CFIP " +
                directionText +
                " WATCH | CONF " +
                _decision.Confidence +
                " | SMART " +
                _decision.SmartQuality +
                " | " +
                _decision.Reason,
                direction,
                false);

            _lastEarlyAlertM5 =
                closedM5;
        }

        private void ProcessDecisionOwnedReactionAlert()
        {
            if (_reaction == null ||
                _m5Bars == null ||
                _m5Bars.Count < 10)
                return;

            int reactionM5 =
                _m5Bars.Count - 1;

            bool hasPlan =
                _plan != null;

            bool hasPendingOrder =
                GetManagedPendingOrder() != null;

            bool hasLivePosition =
                _plan != null &&
                _plan.IsLivePosition;

            bool rangeAllowed =
                IsRangeSignalVisualAllowed(
                    reactionM5,
                    _reaction.Direction,
                    _reaction.Confidence,
                    _reaction.SmartQuality,
                    _reaction.Edge,
                    _reaction.IndependentEvidence,
                    StructuralConfirmations(
                        _reaction.Direction));

            if (_lastReactionAlertBar ==
                    reactionM5 ||
                !WatchReactionAlertRule.IsReactionAlertEligible(
                    AlertOnReaction,
                    AlertOnLiveReaction,
                    _reaction.EntryAllowed,
                    hasPlan,
                    hasPendingOrder,
                    hasLivePosition,
                    rangeAllowed,
                    _reaction.Direction,
                    _reaction.Confidence,
                    _reaction.IndependentEvidence,
                    LiveReactionStrongThreshold,
                    MinimumLiveReactionEvidence))
                return;

            SendUnifiedAlert(
                WatchReactionAlertRule.BuildReactionAlertKey(
                    reactionM5,
                    _reaction.Direction),
                _reaction.Reason,
                _reaction.Direction,
                false);

            _lastReactionAlertBar =
                reactionM5;
        }

        private void ProcessCanonicalActionableEntryAlert(
            int closedM5)
        {
            // Alerts must consume the same current-quote actionability state as
            // automatic market execution; never rely on a stale M5-only snapshot.
            RefreshLiveDecisionActionability(
                closedM5);

            if (_lastActionableEntryAlertM5 ==
                closedM5 &&
                _lastActionableEntryAlertDirection ==
                    _decision.Direction ||
                !_decision.EntryAllowed ||
                !_decision.ActionableNow ||
                GetManagedPendingOrder() != null ||
                _decision.Direction == 0 ||
                _plan == null ||
                _plan.IsLivePosition)
                return;

            int direction =
                _plan.Direction;

            string source = "CONFIRMED";

            if (AlertOnSmartDecision &&
                _decision.SmartQuality >=
                    SmartStrongSetupQuality &&
                _decision.Edge >=
                    SmartStrongSetupEdge)
            {
                source = "SMART";
            }
            else if (AlertOnHighConfidenceEntry &&
                     _decision.Confidence >=
                     HighConfidenceThreshold)
            {
                source = "HIGH";
            }
            else if (!AlertOnConfirmedSignal)
            {
                return;
            }

            string directionText =
                direction == 1
                    ? "BUY"
                    : "SELL";

            string message =
                "CFIP ACTIONABLE " +
                directionText +
                " | " +
                source +
                " | ENTRY " +
                Price(_plan.Entry) +
                " | IDEAL " +
                Price(_plan.IdealEntry) +
                " | TRIGGER " +
                Price(_plan.EntryTrigger) +
                " | SL " +
                Price(_plan.Stop) +
                " | TP1 " +
                Price(_plan.Tp1) +
                " | RR " +
                _plan.Tp1RR.ToString("F2") +
                " | CONF " +
                _decision.Confidence +
                " | LOC " +
                _decision.EntryLocationQuality +
                " | TIMING " +
                _decision.EntryTimingQuality +
                " | POS " +
                _decision.EntryPositionQuality +
                " | DIV " +
                (_decision.DivergenceType ?? "NONE") +
                " Q" +
                _decision.DivergenceQuality +
                " | " +
                (_decision.ActionabilityReason ?? "ACTIONABLE");

            SendUnifiedAlert(
                "ACTION|" +
                closedM5 +
                "|" +
                direction,
                message,
                direction,
                true);

            _lastActionableEntryAlertM5 =
                closedM5;
            _lastActionableEntryAlertDirection =
                _decision.Direction;
        }

        private void ProcessRestrictionAlert(
            int closedM5)
        {
            bool actionabilityBlocked =
                _decision.EntryAllowed &&
                !_decision.ActionableNow &&
                !string.IsNullOrWhiteSpace(
                    _decision.ActionabilityReason);

            string effectiveRestriction =
                actionabilityBlocked
                    ? "ACTIONABILITY • " +
                      _decision.ActionabilityReason
                    : _decision.BlockReason;

            if ((!_decision.EntryAllowed ||
                 actionabilityBlocked) &&
                RestrictionAlertEnabled(
                    effectiveRestriction) &&
                !string.IsNullOrWhiteSpace(
                    effectiveRestriction))
            {
                string restrictionMessage =
                    effectiveRestriction.Trim();

                bool restrictionChanged =
                    !string.Equals(
                        _lastRestrictionMessage,
                        restrictionMessage,
                        StringComparison.OrdinalIgnoreCase);

                if (!restrictionChanged)
                    return;

                SendUnifiedAlert(
                    "RESTRICT|" +
                    restrictionMessage,
                    "CFIP ENTRY BLOCKED | " +
                    restrictionMessage,
                    _decision.Direction,
                    false);

                _lastRestrictionMessage =
                    restrictionMessage;

                _lastRestrictionAlertUtc =
                    TimeInUtc;

                _lastRestrictionM5 =
                    closedM5;

                return;
            }

            if (_decision.EntryAllowed &&
                _decision.ActionableNow)
            {
                _lastRestrictionMessage =
                    "";
                _lastRestrictionAlertUtc =
                    DateTime.MinValue;
                _lastRestrictionM5 =
                    -1;
            }
        }
    }
}
