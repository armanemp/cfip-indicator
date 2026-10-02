using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RefreshParallelOpportunityCandidates(
            int closedM5)
        {
            if (_lastOpportunityCandidatesM5 == closedM5)
                return;

            _lastOpportunityCandidatesM5 =
                closedM5;

            _opportunityCandidates.Clear();
            _tradePlanRegistry.Clear();

            if (!EnableParallelOpportunities ||
                _m5Bars == null ||
                _m5Frame == null ||
                closedM5 < 30)
                return;

            AddTimeframeScenarioCandidates(
                closedM5);

            TradeOpportunityCandidate strategic =
                BuildLaneCandidate(
                    closedM5,
                    OpportunityLane.Strategic,
                    _decision == null
                        ? 0
                        : _decision.Direction,
                    _decision == null
                        ? 0
                        : _decision.Confidence);

            if (strategic != null &&
                _decision != null &&
                _decision.TopDownEligible &&
                string.Equals(
                    _decision.TopDownStage,
                    "ENTRY CALIBRATED",
                    StringComparison.OrdinalIgnoreCase) &&
                ShouldPresentOpportunityCandidate(
                    strategic))
                AddOpportunityCandidate(strategic);

            for (int candidateDirection = 1;
                 candidateDirection >= -1;
                 candidateDirection -= 2)
            {
                TacticalOpportunityResult tacticalAssessment =
                    EvaluateTacticalOpportunityForDirection(
                        _decision,
                        closedM5,
                        candidateDirection);

                if (!tacticalAssessment.Allowed)
                    continue;

                TradeOpportunityCandidate tactical =
                    BuildLaneCandidate(
                        closedM5,
                        tacticalAssessment.Lane,
                        candidateDirection,
                        tacticalAssessment.Quality);

                if (tactical != null &&
                    ShouldPresentOpportunityCandidate(
                        tactical))
                    AddOpportunityCandidate(tactical);
            }

            if (_reaction != null &&
                MicroReactionSafetyRule.IsClosedBarSafe(
                    closedM5,
                    _reaction.ReactionConfirmedM5,
                    _reaction.Direction,
                    _reaction.ReactionConfirmedDirection,
                    _reaction.ReactionConfirmedQuality,
                    _reaction.ReactionClosedBarConfirmed,
                    LiveReactionStrongThreshold) &&
                _m5Frame.Direction !=
                _reaction.Direction)
            {
                TradeOpportunityCandidate reaction =
                    BuildLaneCandidate(
                        closedM5,
                        OpportunityLane.MicroReaction,
                        _reaction.Direction,
                        _reaction.ReactionConfirmedQuality);

                if (reaction != null &&
                    ShouldPresentOpportunityCandidate(
                        reaction))
                    AddOpportunityCandidate(reaction);
            }

            TrimOpportunityCandidates();

        }

        private bool ShouldPresentOpportunityCandidate(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return false;

            int minimumQuality;

            if (candidate.IsPrimaryTimeframeSignal)
            {
                // Primary M15/H1 source visibility is intentionally based on
                // the primary-source quality floor. M5 actionability/entry
                // readiness is evaluated separately and must not erase the source setup.
                minimumQuality =
                    Math.Max(
                        60,
                        TacticalOpportunityMinimumQuality - 5);
            }
            else
            {
                minimumQuality =
                    Math.Max(
                        TacticalOpportunityMinimumQuality,
                        Math.Max(
                            MinimumSmartQuality,
                            SmartQualityThreshold));
            }

            if (!candidate.IsPrimaryTimeframeSignal &&
                candidate.Lane ==
                OpportunityLane.Strategic)
            {
                minimumQuality =
                    Math.Max(
                        minimumQuality,
                        MinimumConfidence);
            }
            else if (candidate.Lane ==
                     OpportunityLane.CounterHtfTactical)
            {
                minimumQuality =
                    Math.Max(
                        minimumQuality,
                        CounterHtfMinimumQuality);
            }
            else if (candidate.Lane ==
                     OpportunityLane.MicroReaction)
            {
                minimumQuality =
                    Math.Max(
                        minimumQuality,
                        LiveReactionStrongThreshold);
            }

            // Keep parallel detection available, but do not crowd the chart
            // with candidates that are materially below the active quality floor.
            if (!candidate.IsPrimaryTimeframeSignal &&
                candidate.Lane != OpportunityLane.CounterHtfTactical &&
                candidate.Lane != OpportunityLane.MicroReaction)
                minimumQuality =
                    ActionabilityThresholdPolicy.ApplyParallelCandidateQualityMargin(
                        minimumQuality,
                        true);

            return candidate.Quality >= minimumQuality;
        }

        private TradeOpportunityCandidate BuildLaneCandidate(
            int closedM5,
            OpportunityLane lane,
            int direction,
            int quality,
            string sourceTimeframe = null,
            bool allowPrimaryPresentationFallback = false)
        {
            if (direction != 1 &&
                direction != -1)
                return null;

            ParallelScenarioGeometry geometry;

            if (!TryBuildParallelScenarioGeometry(
                    closedM5,
                    direction,
                    out geometry))
            {
                return allowPrimaryPresentationFallback
                    ? BuildPrimaryPresentationCandidate(
                        closedM5,
                        lane,
                        direction,
                        quality,
                        sourceTimeframe,
                        "PRIMARY PLAN GEOMETRY UNAVAILABLE")
                    : null;
            }

            ExecutionModel execution;

            if (!TryGetParallelExecutionModel(
                    closedM5,
                    direction,
                    out execution))
            {
                return allowPrimaryPresentationFallback
                    ? BuildPrimaryPresentationCandidate(
                        closedM5,
                        lane,
                        direction,
                        quality,
                        sourceTimeframe,
                        "PRIMARY EXECUTION MODEL UNAVAILABLE")
                    : null;
            }

            TradeSetupPreview preview;
            if (!TryGetParallelScenarioPreview(
                    closedM5,
                    execution,
                    lane,
                    geometry,
                    out preview))
            {
                return allowPrimaryPresentationFallback
                    ? BuildPrimaryPresentationCandidate(
                        closedM5,
                        lane,
                        direction,
                        quality,
                        sourceTimeframe,
                        "PRIMARY PLAN PREVIEW UNAVAILABLE")
                    : null;
            }

            if (preview == null ||
                !IsFinitePositive(preview.Stop) ||
                !IsFinitePositive(preview.Tp1) ||
                preview.Risk <= 0)
            {
                return allowPrimaryPresentationFallback
                    ? BuildPrimaryPresentationCandidate(
                        closedM5,
                        lane,
                        direction,
                        quality,
                        sourceTimeframe,
                        "PRIMARY PLAN LEVELS INCOMPLETE")
                    : null;
            }

            double tp1RR =
                CalculatePreviewStageRR(
                    executablePreview.Tp1,
                    executablePreview.Entry,
                    executablePreview.Stop,
                    direction);

            double tp2RR =
                CalculatePreviewStageRR(
                    executablePreview.Tp2,
                    executablePreview.Entry,
                    executablePreview.Stop,
                    direction);

            double tp3RR =
                CalculatePreviewStageRR(
                    executablePreview.Tp3,
                    executablePreview.Entry,
                    executablePreview.Stop,
                    direction);

            double tp4RR =
                CalculatePreviewStageRR(
                    executablePreview.Tp4,
                    executablePreview.Entry,
                    executablePreview.Stop,
                    direction);

            TradeActionabilityResult actionability =
                EvaluateTradeActionability(
                    closedM5,
                    direction,
                    lane,
                    _decision == null ? "UNKNOWN" : _decision.Regime,
                    execution,
                    preview);

            // Once a scenario is actually actionable, the candidate handed to
            // the provider/cBot must carry the exact canonical actual-entry
            // geometry that passed Actionability. Non-actionable/watch
            // candidates intentionally retain the presentation preview.
            TradeSetupPreview executablePreview =
                preview;

            if (actionability.Actionable)
            {
                CanonicalTradePathGeometry canonicalPath;
                string canonicalPathReason;

                if (!TryBuildCanonicalTradePathGeometry(
                        closedM5,
                        direction,
                        execution,
                        lane,
                        out canonicalPath,
                        out canonicalPathReason) ||
                    canonicalPath == null ||
                    !canonicalPath.IsValid)
                    return null;

                executablePreview =
                    canonicalPath.Preview;
            }

            PlanRewardRiskQualityResult rewardRisk =
                PlanRewardRiskQualityRule.Evaluate(
                    direction,
                    executablePreview.Entry,
                    executablePreview.Stop,
                    executablePreview.Tp1,
                    Atr(
                        _m5Bars,
                        closedM5),
                    Math.Max(
                        0,
                        Symbol.Ask - Symbol.Bid),
                    Math.Max(
                        Tp1MinimumRR,
                        MinimumRequiredRRForRegime(
                            _decision == null
                                ? "UNKNOWN"
                                : _decision.Regime)),
                    PreferredStopRiskAtr,
                    StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                        MinimumSlAtr,
                        MaximumSlAtr,
                        MaximumStructuralStopAtr),
                        Math.Max(0, MaximumRewardRR),
                        Symbol.PipSize);

            if (!rewardRisk.Allowed &&
                !allowPrimaryPresentationFallback)
                return null;

            int independentEvidence =
                IndependentEvidence(direction);

            int independentEvidenceGroups =
                IndependentEvidenceGroupCount(direction);

            TradeOpportunityCandidate candidate =
                new TradeOpportunityCandidate
            {
                Id =
                    BuildOpportunityId(
                        lane,
                        direction),
                Lane = lane,
                ExecutionMode = execution.Mode,
                Direction = direction,
                CreatedM5 = closedM5,
                IndependentEvidenceScore =
                    independentEvidence,
                IndependentEvidenceGroupCount =
                    independentEvidenceGroups,
                IndicatorIndependentEvidenceGroupCount =
                    _m5Frame == null ? 0 : _m5Frame.IndicatorIndependentEvidenceGroupCount,
                Quality = Math.Max(
                    0,
                    Math.Min(
                        100,
                        quality > 0
                            ? quality
                            : execution.Quality)),
                Risk = executablePreview.Risk,
                Tp1RR = tp1RR,
                Tp2RR = tp2RR,
                Tp3RR = tp3RR,
                Tp4RR = tp4RR,
                Entry = executablePreview.Entry,
                IdealEntry = executablePreview.IdealEntry,
                Trigger = executablePreview.Trigger,
                Invalidation = executablePreview.Invalidation,
                Stop = executablePreview.Stop,
                Tp1 = executablePreview.Tp1,
                Tp2 = executablePreview.Tp2,
                Tp3 = executablePreview.Tp3,
                Tp4 = executablePreview.Tp4,
                Source = execution.Source,
                Stage =
                    actionability.Actionable
                        ? "READY"
                        : "WATCH • " +
                          actionability.Reason,
                LabelPrefix =
                    LaneText(lane),
                ActionableNow =
                    actionability.Actionable,
                EntryDistanceAtr =
                    actionability.EntryDistanceAtr,
                DivergenceQuality =
                    actionability.DivergenceQuality,
                DivergenceType =
                    actionability.DivergenceType,
                ActionabilityReason =
                    actionability.Reason,
                ScenarioId =
                    string.IsNullOrWhiteSpace(sourceTimeframe)
                        ? BuildCanonicalScenarioId(
                            lane,
                            direction)
                        : "TF-" +
                          sourceTimeframe.Trim() +
                          "-" +
                          (direction == 1
                              ? "BUY"
                              : "SELL"),
                SourceTimeframe =
                    string.IsNullOrWhiteSpace(sourceTimeframe)
                        ? null
                        : sourceTimeframe.Trim(),
                BasePlanTimeframe = "M5"
            };

            double stopPips =
                Math.Abs(
                    candidate.Entry -
                    candidate.Stop) /
                Math.Max(
                    Symbol.PipSize,
                    1e-9);

            candidate.RequestedVolume =
                stopPips > 0
                    ? CalculateVolume(
                        EffectiveRiskStopPips(
                            stopPips))
                    : 0;

            if (string.IsNullOrWhiteSpace(
                    sourceTimeframe))
            {
                EnrichScenarioEvidence(
                    candidate,
                    _m5Frame,
                    direction);
            }

            ScenarioExecutionPolicyResult policy =
                ScenarioExecutionPolicyRule.Evaluate(
                    candidate,
                    _decision,
                    lane,
                    M5OnlyConfirmedTrigger);

            candidate.ExecutionPolicyAllowed =
                policy.ExecutionAuthorized;

            candidate.ExecutionPolicyReason =
                policy.ExecutionReason;

            return candidate;
        }

        private TradeOpportunityCandidate BuildPrimaryPresentationCandidate(
            int closedM5,
            OpportunityLane lane,
            int direction,
            int quality,
            string sourceTimeframe,
            string reason)
        {
            if (!PrimaryTimeframeSignalRule.IsPrimarySource(sourceTimeframe) ||
                (direction != 1 && direction != -1))
                return null;

            string normalizedTimeframe =
                sourceTimeframe.Trim().ToUpperInvariant();

            return new TradeOpportunityCandidate
            {
                Id =
                    "TF-" +
                    normalizedTimeframe +
                    "-" +
                    (direction == 1 ? "BUY" : "SELL"),
                ScenarioId =
                    "TF-" +
                    normalizedTimeframe +
                    "-" +
                    (direction == 1 ? "BUY" : "SELL"),
                SourceTimeframe = normalizedTimeframe,
                BasePlanTimeframe = "M5",
                IsPrimaryTimeframeSignal = true,
                PresentationOnly = true,
                Lane = lane,
                Direction = direction,
                CreatedM5 = closedM5,
                IndependentEvidenceScore =
                    IndependentEvidence(direction),
                IndependentEvidenceGroupCount =
                    IndependentEvidenceGroupCount(direction),
                IndicatorIndependentEvidenceGroupCount =
                    _m5Frame == null
                        ? 0
                        : _m5Frame.IndicatorIndependentEvidenceGroupCount,
                Quality =
                    Math.Max(
                        0,
                        Math.Min(100, quality)),
                Source = "PRIMARY " + normalizedTimeframe,
                Stage =
                    "PRIMARY " +
                    normalizedTimeframe +
                    " • M5 TUNING • WAIT",
                LabelPrefix =
                    "PRIMARY-" +
                    normalizedTimeframe,
                ActionableNow = false,
                ExecutionPolicyAllowed = false,
                ExecutionPolicyReason =
                    "PRESENTATION ONLY • NO EXECUTION PLAN",
                ActionabilityReason =
                    string.IsNullOrWhiteSpace(reason)
                        ? "PRIMARY SOURCE VALID • PLAN PENDING"
                        : reason
            };
        }

        private string BuildCanonicalScenarioId(
            OpportunityLane lane,
            int direction)
        {
            return
                "CANONICAL-" +
                LaneText(lane) +
                "-" +
                (direction == 1
                    ? "BUY"
                    : "SELL");
        }

        private string BuildOpportunityId(
            OpportunityLane lane,
            int direction)
        {
            return
                LaneText(lane) +
                "_" +
                (direction == 1
                    ? "BUY"
                    : "SELL");
        }

        private string LaneText(
            OpportunityLane lane)
        {
            switch (lane)
            {
                case OpportunityLane.Strategic:
                    return "HTF";
                case OpportunityLane.CounterHtfTactical:
                    return "TACTICAL-COUNTER";
                case OpportunityLane.MicroReaction:
                    return "MICRO";
                default:
                    return "TACTICAL";
            }
        }

        private void AddOpportunityCandidate(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return;

            _tradePlanRegistry.UpsertScenario(
                candidate,
                Math.Max(
                    Symbol.PipSize * 2,
                    0));
        }

        private double CalculatePreviewStageRR(
            double target,
            double entry,
            double stop,
            int direction)
        {
            if (!IsFinitePositive(target))
                return 0;

            RiskRewardMathResult geometry =
                RiskRewardMathRule.Evaluate(
                    direction,
                    entry,
                    stop,
                    target,
                    0,
                    0,
                    MaximumRewardRR,
                    Symbol.PipSize);

            return geometry.Valid
                ? geometry.NominalRR
                : 0;
        }

    }
}
