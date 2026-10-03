using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ProcessDecisionAlerts(
            int closedM5)
        {
            // Analysis alerts are presentation/decision events, not broker events.
            // They must remain deliverable even when the cBot is absent, stopped,
            // reconnecting, or has not yet published a heartbeat.
            if (_decision == null)
                return;

            ProcessRestrictionAlert(
                closedM5);

            ProcessParallelOpportunityAlerts(
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
            if (_decision == null)
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

            SignalVisualSnapshot watchSnapshot =
                BuildSignalVisualSnapshot(closedM5);

            string watchTrend =
                watchSnapshot != null &&
                watchSnapshot.MtfTrendStrengthLevel > 0 &&
                !string.IsNullOrWhiteSpace(watchSnapshot.MtfTrendStrengthTier)
                    ? " | TREND " +
                      watchSnapshot.MtfTrendStrengthTier.ToUpperInvariant() +
                      " L" +
                      watchSnapshot.MtfTrendStrengthLevel
                    : "";

            if (watchSnapshot != null &&
                watchSnapshot.MtfTrendDirection != 0 &&
                watchSnapshot.MtfTrendDirection != direction)
                return;

            if (SendUnifiedAlert(
                    WatchReactionAlertRule.BuildWatchAlertKey(
                        closedM5,
                        direction),
                    "CFIP " +
                    directionText +
                    " WATCH | CONF " +
                    _decision.Confidence +
                    " | SMART " +
                    _decision.SmartQuality +
                    watchTrend +
                    " | " +
                    _decision.Reason,
                    direction,
                    false))
            {
                _lastEarlyAlertM5 =
                    closedM5;
            }
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

            SignalVisualSnapshot canonicalSnapshot =
                BuildSignalVisualSnapshot(
                    reactionM5);

            // Reaction alerts are informational, but they must never announce a
            // direction that contradicts the same canonical snapshot used by the
            // chart arrow. An already-actionable canonical plan also owns the alert.
            if (canonicalSnapshot != null &&
                canonicalSnapshot.ActionableNow)
                return;

            if (canonicalSnapshot != null &&
                canonicalSnapshot.AuthoritativeDirection != 0 &&
                canonicalSnapshot.AuthoritativeDirection != _reaction.Direction)
                return;

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

            if (SendUnifiedAlert(
                    WatchReactionAlertRule.BuildReactionAlertKey(
                        reactionM5,
                        _reaction.Direction),
                    _reaction.Reason,
                    _reaction.Direction,
                    false))
            {
                _lastReactionAlertBar =
                    reactionM5;
            }
        }

        private void ProcessParallelOpportunityAlerts(
            int closedM5)
        {
            if (!EnableParallelOpportunities ||
                _tradePlanRegistry == null ||
                _m5Bars == null ||
                closedM5 < 0)
                return;

            // Rebuild the scenario snapshot from the freshly closed M5 state so
            // alerting never depends on the previous bar's candidate collection.
            RefreshParallelOpportunityCandidates(
                closedM5);

            var candidates =
                _tradePlanRegistry.SelectScenariosForDisplay(
                    Math.Max(
                        2,
                        MaximumVisibleOpportunities));

            string canonicalScenarioId = string.Empty;

            if (_decision != null)
            {
                canonicalScenarioId =
                    ResolveProviderScenarioId(
                        ResolveProviderSignalId(closedM5),
                        ResolveProviderLane(),
                        _decision.Direction);
            }

            int displayNumber = 0;

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    candidates[i];

                if (candidate == null ||
                    (candidate.Direction != 1 &&
                     candidate.Direction != -1))
                    continue;

                string candidateScenarioId =
                    string.IsNullOrWhiteSpace(candidate.ScenarioId)
                        ? candidate.Id
                        : candidate.ScenarioId;

                if (!string.IsNullOrWhiteSpace(canonicalScenarioId) &&
                    string.Equals(
                        candidateScenarioId,
                        canonicalScenarioId,
                        StringComparison.OrdinalIgnoreCase))
                    continue;

                displayNumber++;

                bool actionable =
                    candidate.ActionableNow &&
                    candidate.ExecutionPolicyAllowed &&
                    !candidate.PresentationOnly;

                bool primaryWatch =
                    candidate.IsPrimaryTimeframeSignal &&
                    !actionable;

                if (!actionable &&
                    !primaryWatch)
                    continue;

                bool enabled =
                    actionable
                        ? AlertOnConfirmedSignal
                        : AlertOnEarlyWatch;

                if (!enabled)
                    continue;

                string timeframe =
                    string.IsNullOrWhiteSpace(
                        candidate.SourceTimeframe)
                        ? "M5"
                        : candidate.SourceTimeframe.Trim().ToUpperInvariant();

                string direction =
                    candidate.Direction == 1
                        ? "BUY"
                        : "SELL";

                string keyPrefix =
                    actionable
                        ? "ACTION|SCENARIO|"
                        : "EARLY|SCENARIO|";

                string key =
                    keyPrefix +
                    candidateScenarioId +
                    "|" +
                    candidate.Direction +
                    "|" +
                    closedM5;

                string message =
                    "CFIP #" +
                    displayNumber +
                    " OPPORTUNITY " +
                    direction +
                    " | " +
                    timeframe +
                    " | SCENARIO " +
                    (string.IsNullOrWhiteSpace(candidate.ScenarioId)
                        ? candidate.Id
                        : candidate.ScenarioId) +
                    " | Q " +
                    Math.Max(
                        0,
                        candidate.Quality);

                if (IsFinitePositive(candidate.Entry))
                    message +=
                        " | ENTRY " +
                        Price(candidate.Entry);

                if (IsFinitePositive(candidate.Stop))
                    message +=
                        " | SL " +
                        Price(candidate.Stop);

                if (IsFinitePositive(candidate.Tp1))
                    message +=
                        " | TP1 " +
                        Price(candidate.Tp1);

                if (candidate.Tp1RR > 0)
                    message +=
                        " | RR " +
                        candidate.Tp1RR.ToString(
                            "F2",
                            System.Globalization.CultureInfo.InvariantCulture) +
                        "R";

                message +=
                    " | " +
                    (candidate.Stage ?? "WATCH");

                SendUnifiedAlert(
                    key,
                    message,
                    candidate.Direction,
                    actionable);
            }
        }

        private void ProcessCanonicalActionableEntryAlert(
            int closedM5)
        {
            // Alerts must consume the same current-quote actionability state as
            // automatic market execution; never rely on a stale M5-only snapshot.
            RefreshLiveDecisionActionability(
                closedM5);

            bool sameBarAlertAlreadySent =
                _lastActionableEntryAlertM5 == closedM5 &&
                _lastActionableEntryAlertDirection == _decision.Direction;

            if (sameBarAlertAlreadySent ||
                !_decision.EntryAllowed ||
                !_decision.ActionableNow ||
                GetManagedPendingOrder() != null ||
                _decision.Direction == 0 ||
                _plan == null ||
                _plan.IsLivePosition ||
                _plan.Direction != _decision.Direction)
                return;

            SignalVisualSnapshot canonicalSnapshot =
                BuildSignalVisualSnapshot(closedM5);

            if (canonicalSnapshot != null &&
                canonicalSnapshot.AuthoritativeDirection != 0 &&
                canonicalSnapshot.AuthoritativeDirection != _decision.Direction)
                return;

            int direction =
                _decision.Direction;

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

            string trendTag =
                canonicalSnapshot != null &&
                canonicalSnapshot.MtfTrendStrengthLevel > 0 &&
                !string.IsNullOrWhiteSpace(canonicalSnapshot.MtfTrendStrengthTier)
                    ? " | TREND " +
                      canonicalSnapshot.MtfTrendStrengthTier.ToUpperInvariant() +
                      " L" +
                      canonicalSnapshot.MtfTrendStrengthLevel
                    : "";

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
                trendTag +
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

            if (SendUnifiedAlert(
                    "ACTION|" +
                    closedM5 +
                    "|" +
                    direction,
                    message,
                    direction,
                    true))
            {
                _lastActionableEntryAlertM5 =
                    closedM5;
                _lastActionableEntryAlertDirection =
                    _decision.Direction;
            }
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

                if (SendUnifiedAlert(
                        "RESTRICT|" +
                        restrictionMessage,
                        "CFIP ENTRY BLOCKED | " +
                        restrictionMessage,
                        _decision.Direction,
                        false))
                {
                    _lastRestrictionMessage =
                        restrictionMessage;

                    _lastRestrictionAlertUtc =
                        TimeInUtc;

                    _lastRestrictionM5 =
                        closedM5;
                }

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
