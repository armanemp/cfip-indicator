using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    internal sealed partial class ParallelOpportunityBuilder
    {
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
                    preview.Tp1,
                    preview.Entry,
                    preview.Stop,
                    direction);

            double tp2RR =
                CalculatePreviewStageRR(
                    preview.Tp2,
                    preview.Entry,
                    preview.Stop,
                    direction);

            double tp3RR =
                CalculatePreviewStageRR(
                    preview.Tp3,
                    preview.Entry,
                    preview.Stop,
                    direction);

            double tp4RR =
                CalculatePreviewStageRR(
                    preview.Tp4,
                    preview.Entry,
                    preview.Stop,
                    direction);

            TradeActionabilityResult actionability =
                EvaluateTradeActionability(
                    closedM5,
                    direction,
                    lane,
                    _decision == null ? "UNKNOWN" : _decision.Regime,
                    execution,
                    preview);

            double candidateAtr =
                Atr(
                    _m5Bars,
                    closedM5);

            double minimumRewardDistanceAtr =
                RegimeAdaptiveRewardFloorRule.ResolveAdaptiveRewardFloor(
                    _decision == null
                        ? "UNKNOWN"
                        : _decision.Regime,
                    MinimumTpSpacingAtr,
                    MinimumSlAtr);

            double rewardDistanceAtr =
                candidateAtr > 0
                    ? Math.Abs(
                        preview.Tp1 -
                        preview.Entry) /
                      candidateAtr
                    : 0;

            PlanRewardRiskQualityResult rewardRisk =
                PlanRewardRiskQualityRule.Evaluate(
                    direction,
                    preview.Entry,
                    preview.Stop,
                    preview.Tp1,
                    candidateAtr,
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
                Risk = preview.Risk,
                RewardDistanceAtr = rewardDistanceAtr,
                MinimumRequiredRewardDistanceAtr =
                    minimumRewardDistanceAtr,
                Tp1RR = tp1RR,
                Tp2RR = tp2RR,
                Tp3RR = tp3RR,
                Tp4RR = tp4RR,
                Entry = preview.Entry,
                IdealEntry = preview.IdealEntry,
                Trigger = preview.Trigger,
                Invalidation = preview.Invalidation,
                Stop = preview.Stop,
                Tp1 = preview.Tp1,
                Tp2 = preview.Tp2,
                Tp3 = preview.Tp3,
                Tp4 = preview.Tp4,
                ZoneLow = preview.ZoneLow,
                ZoneHigh = preview.ZoneHigh,
                ZoneTolerance = preview.ZoneTolerance,
                FutureOrderReady = false,
                FutureOrderDistanceAtr = 0,
                FutureOrderSource = "",
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

    }
}
