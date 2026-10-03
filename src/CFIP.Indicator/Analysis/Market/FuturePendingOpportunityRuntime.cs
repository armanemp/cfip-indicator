using System.Collections.Generic;
using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void AddFuturePendingOpportunityCandidates(int closedM5)
        {
            if (_decision == null ||
                closedM5 < 30 ||
                _m5Bars == null ||
                _m5Frame == null)
                return;

            if (PendingModeAllowsStop() &&
                TrendContinuationStrong())
            {
                TradeOpportunityCandidate continuation;
                if (TryBuildFuturePendingCandidate(
                        closedM5,
                        ExecutionMode.ContinuationStop,
                        out continuation) &&
                    continuation != null)
                {
                    AddOpportunityCandidate(continuation);
                }
            }

            if (PendingModeAllowsLimit() &&
                _reaction != null &&
                ReversalSetupStrong())
            {
                TradeOpportunityCandidate reversal;
                if (TryBuildFuturePendingCandidate(
                        closedM5,
                        ExecutionMode.ReversalLimit,
                        out reversal) &&
                    reversal != null)
                {
                    AddOpportunityCandidate(reversal);
                }
            }
        }

        private bool TryBuildFuturePendingCandidate(
            int closedM5,
            ExecutionMode mode,
            out TradeOpportunityCandidate candidate)
        {
            candidate = null;

            if (mode != ExecutionMode.ContinuationStop &&
                mode != ExecutionMode.ReversalLimit)
                return false;

            int direction;
            double atr;
            double entry;
            double stop;
            double target;
            double stopPips;
            double targetPips;
            double volume;
            ExecutionIntent intent;

            bool prepared;

            // Future-order discovery reuses the exact analytical preparation
            // geometry, but it must not replace the canonical live provider
            // intent that represents the current market-entry path.
            _suppressProviderIntentCapture = true;
            try
            {
                prepared =
                    mode == ExecutionMode.ContinuationStop
                        ? TryPrepareContinuationStop(
                            closedM5,
                            out direction,
                            out atr,
                            out entry,
                            out stop,
                            out target,
                            out stopPips,
                            out targetPips,
                            out volume,
                            out intent)
                        : TryPrepareReversalLimit(
                            closedM5,
                            out direction,
                            out atr,
                            out entry,
                            out stop,
                            out target,
                            out stopPips,
                            out targetPips,
                            out volume,
                            out intent);
            }
            finally
            {
                _suppressProviderIntentCapture = false;
            }

            if (!prepared ||
                intent == null ||
                direction == 0 ||
                !IsFinitePositive(atr) ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(stop) ||
                !IsFinitePositive(target) ||
                !IsFinitePositive(volume))
                return false;

            if (!IsValidPendingEntry(
                    direction,
                    entry,
                    mode == ExecutionMode.ContinuationStop))
                return false;

            double distanceAtr =
                Math.Abs(
                    entry -
                    (direction == 1
                        ? Symbol.Ask
                        : Symbol.Bid)) /
                Math.Max(
                    atr,
                    1e-9);

            if (!IsFinitePositive(distanceAtr))
                return false;

            RiskRewardMathResult rr =
                RiskRewardMathRule.Evaluate(
                    direction,
                    entry,
                    stop,
                    target,
                    0,
                    0,
                    MaximumRewardRR,
                    Symbol.PipSize);

            if (!rr.Valid ||
                rr.NominalRR < Math.Max(
                    Tp1MinimumRR,
                    MinimumRequiredRRForRegime(
                        _decision == null
                            ? "UNKNOWN"
                            : _decision.Regime)))
                return false;

            int baseQuality =
                mode == ExecutionMode.ContinuationStop
                    ? Math.Max(
                        _decision.Confidence,
                        _decision.SmartQuality)
                    : Math.Max(
                        _reaction == null
                            ? 0
                            : _reaction.ReactionConfirmedQuality,
                        _decision.SmartQuality);

            int score =
                PendingDecisionArbiterRule.ScoreCandidate(
                    baseQuality,
                    mode == ExecutionMode.ContinuationStop
                        ? _decision.SmartQuality
                        : (_reaction == null
                            ? 0
                            : _reaction.ReactionConfirmedQuality),
                    mode == ExecutionMode.ContinuationStop
                        ? _decision.TimeframeAgreement
                        : (_reaction == null
                            ? 0
                            : _reaction.ReactionConfirmedEvidence * 10),
                    mode == ExecutionMode.ContinuationStop
                        ? _decision.IndependentEvidence
                        : (_reaction == null
                            ? 0
                            : _reaction.ReactionConfirmedEvidence),
                    mode == ExecutionMode.ContinuationStop
                        ? _decision.StructuralConfirmations
                        : StructuralConfirmations(direction));

            string source =
                intent.Source ?? (
                    mode == ExecutionMode.ContinuationStop
                        ? "FUTURE CONTINUATION STOP"
                        : "FUTURE REVERSAL LIMIT");

            candidate =
                new TradeOpportunityCandidate
                {
                    Id =
                        "FUTURE-" +
                        mode.ToString().ToUpperInvariant() +
                        "-" +
                        (direction == 1 ? "BUY" : "SELL"),
                    ScenarioId =
                        "FUTURE-" +
                        mode.ToString().ToUpperInvariant() +
                        "-" +
                        (direction == 1 ? "BUY" : "SELL"),
                    SourceTimeframe = "M5",
                    BasePlanTimeframe = "M5",
                    IsPrimaryTimeframeSignal = false,
                    PresentationOnly = false,
                    M5TuningAligned = true,
                    M1TuningConfirmed =
                        _m1Frame != null &&
                        ((direction == 1 && _m1Frame.Direction == 1) ||
                         (direction == -1 && _m1Frame.Direction == -1)),
                    ExecutionMode = mode,
                    PrimarySignalState = "FUTURE",
                    IndependentEvidenceScore =
                        mode == ExecutionMode.ContinuationStop
                            ? _decision.IndependentEvidence
                            : (_reaction == null
                                ? 0
                                : _reaction.ReactionConfirmedEvidence),
                    IndependentEvidenceGroupCount =
                        mode == ExecutionMode.ContinuationStop
                            ? IndependentEvidenceGroupCount(direction)
                            : 1,
                    IndicatorIndependentEvidenceGroupCount =
                        _m5Frame.IndicatorIndependentEvidenceGroupCount,
                    Quality =
                        Math.Max(
                            0,
                            Math.Min(
                                100,
                                Math.Max(
                                    baseQuality,
                                    score))),
                    Risk =
                        Math.Abs(
                            entry -
                            stop),
                    Tp1RR = rr.NominalRR,
                    Tp2RR = 0,
                    Tp3RR = 0,
                    Tp4RR = 0,
                    RequestedVolume = volume,
                    Entry = entry,
                    IdealEntry = entry,
                    Trigger = intent.Trigger,
                    Invalidation = stop,
                    Stop = stop,
                    Tp1 = target,
                    Tp2 = target,
                    Tp3 = target,
                    Tp4 = target,
                    ZoneLow = intent.ZoneLow,
                    ZoneHigh = intent.ZoneHigh,
                    ZoneTolerance =
                        Math.Max(
                            Symbol.TickSize,
                            atr * 0.05),
                    FutureOrderReady = true,
                    FutureOrderDistanceAtr = distanceAtr,
                    FutureOrderSource = source,
                    Source = source,
                    Stage = "FUTURE ORDER • ARMED",
                    LabelPrefix =
                        mode == ExecutionMode.ContinuationStop
                            ? "FUTURE-STOP"
                            : "FUTURE-LIMIT",
                    ActionableNow = false,
                    EntryDistanceAtr = distanceAtr,
                    ActionabilityReason =
                        "FUTURE LEVEL ARMED • WAITING FOR PRICE",
                    ExecutionPolicyAllowed = false,
                    ExecutionPolicyReason = "PENDING POLICY EVALUATION"
                };

            ScenarioExecutionPolicyResult policy =
                ScenarioExecutionPolicyRule.Evaluate(
                    candidate,
                    _decision,
                    candidate.Lane,
                    M5OnlyConfirmedTrigger);

            candidate.ExecutionPolicyAllowed =
                policy.ExecutionAuthorized;
            candidate.ExecutionPolicyReason =
                policy.ExecutionReason;

            return policy.ExecutionAuthorized;
        }

        private void RefreshLiveParallelOpportunityStates(
            int closedM5)
        {
            DateTime now = Server.TimeInUtc;
            bool newM5 =
                _lastLiveOpportunityRefreshM5 != closedM5;

            if (!newM5 &&
                (now - _lastLiveOpportunityRefreshUtc).TotalMilliseconds < 200)
                return;

            _lastLiveOpportunityRefreshM5 = closedM5;
            _lastLiveOpportunityRefreshUtc = now;

            IReadOnlyList<TradeOpportunityCandidate> executionCandidates =
                _tradePlanRegistry == null
                    ? null
                    : _tradePlanRegistry.Snapshot();

            if (executionCandidates == null ||
                executionCandidates.Count == 0)
                return;

            for (int i = 0;
                 i < executionCandidates.Count;
                 i++)
            {
                TradeOpportunityCandidate candidate =
                    executionCandidates[i];

                if (candidate == null ||
                    candidate.CreatedM5 != closedM5 ||
                    candidate.PresentationOnly)
                    continue;

                if (candidate.FutureOrderReady)
                {
                    TradeOpportunityCandidate refreshed;
                    if (TryBuildFuturePendingCandidate(
                            closedM5,
                            candidate.ExecutionMode,
                            out refreshed) &&
                        refreshed != null)
                    {
                        candidate.Entry = refreshed.Entry;
                        candidate.IdealEntry = refreshed.IdealEntry;
                        candidate.Trigger = refreshed.Trigger;
                        candidate.Invalidation = refreshed.Invalidation;
                        candidate.Stop = refreshed.Stop;
                        candidate.Tp1 = refreshed.Tp1;
                        candidate.Tp2 = refreshed.Tp2;
                        candidate.Tp3 = refreshed.Tp3;
                        candidate.Tp4 = refreshed.Tp4;
                        candidate.Risk = refreshed.Risk;
                        candidate.Tp1RR = refreshed.Tp1RR;
                        candidate.RequestedVolume = refreshed.RequestedVolume;
                        candidate.ZoneLow = refreshed.ZoneLow;
                        candidate.ZoneHigh = refreshed.ZoneHigh;
                        candidate.ZoneTolerance = refreshed.ZoneTolerance;
                        candidate.FutureOrderDistanceAtr =
                            refreshed.FutureOrderDistanceAtr;
                        candidate.FutureOrderSource =
                            refreshed.FutureOrderSource;
                        candidate.Source = refreshed.Source;
                        candidate.Stage = refreshed.Stage;
                        candidate.Quality = refreshed.Quality;
                        candidate.ExecutionPolicyAllowed = true;
                        candidate.ExecutionPolicyReason =
                            "FUTURE PENDING SCENARIO CANDIDATE";
                        candidate.ActionableNow = false;
                        candidate.ActionabilityReason =
                            "FUTURE LEVEL ARMED • WAITING FOR PRICE";
                    }
                    else
                    {
                        candidate.FutureOrderReady = false;
                        candidate.ExecutionPolicyAllowed = false;
                        candidate.ExecutionPolicyReason =
                            "FUTURE LEVEL NO LONGER VALID";
                        candidate.Stage =
                            "FUTURE ORDER • DISARMED";
                        candidate.ActionabilityReason =
                            "FUTURE LEVEL INVALIDATED";
                    }

                    continue;
                }

                ExecutionModel execution =
                    new ExecutionModel
                    {
                        Direction = candidate.Direction,
                        Mode = candidate.ExecutionMode,
                        IdealEntry = candidate.IdealEntry,
                        ActualEntry = candidate.Entry,
                        ZoneLow = candidate.ZoneLow,
                        ZoneHigh = candidate.ZoneHigh,
                        ZoneTolerance = candidate.ZoneTolerance,
                        Trigger = candidate.Trigger,
                        Invalidation = candidate.Invalidation,
                        Quality = candidate.Quality,
                        Ready = true,
                        Source = candidate.Source
                    };

                TradeSetupPreview preview =
                    new TradeSetupPreview
                    {
                        Direction = candidate.Direction,
                        EntryMode = candidate.ExecutionMode,
                        CreatedM5 = candidate.CreatedM5,
                        Entry = candidate.Entry,
                        IdealEntry = candidate.IdealEntry,
                        ZoneLow = candidate.ZoneLow,
                        ZoneHigh = candidate.ZoneHigh,
                        ZoneTolerance = candidate.ZoneTolerance,
                        Trigger = candidate.Trigger,
                        Invalidation = candidate.Invalidation,
                        Stop = candidate.Stop,
                        Tp1 = candidate.Tp1,
                        Tp2 = candidate.Tp2,
                        Tp3 = candidate.Tp3,
                        Tp4 = candidate.Tp4,
                        Risk = candidate.Risk
                    };

                TradeActionabilityResult actionability =
                    EvaluateTradeActionability(
                        closedM5,
                        candidate.Direction,
                        candidate.Lane,
                        _decision == null
                            ? "UNKNOWN"
                            : _decision.Regime,
                        execution,
                        preview);

                candidate.ActionableNow =
                    actionability.Actionable;
                candidate.EntryDistanceAtr =
                    actionability.EntryDistanceAtr;
                candidate.DivergenceQuality =
                    actionability.DivergenceQuality;
                candidate.DivergenceType =
                    actionability.DivergenceType;
                candidate.ActionabilityReason =
                    actionability.Reason;
                candidate.Stage =
                    actionability.Actionable
                        ? "READY • REALTIME"
                        : "WATCH • " +
                          actionability.Reason;

                ScenarioExecutionPolicyResult policy =
                    ScenarioExecutionPolicyRule.Evaluate(
                        candidate,
                        _decision,
                        candidate.Lane,
                        M5OnlyConfirmedTrigger);

                candidate.ExecutionPolicyAllowed =
                    policy.ExecutionAuthorized;
                candidate.ExecutionPolicyReason =
                    policy.ExecutionReason;
            }

            TrimOpportunityCandidates();
        }
    }
}
