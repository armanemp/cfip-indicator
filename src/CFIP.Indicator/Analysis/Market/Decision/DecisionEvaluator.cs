using System;

namespace cAlgo
{
    internal sealed class DecisionEvaluator
    {
        private readonly DecisionScoreCalculator _scoreCalculator =
            new DecisionScoreCalculator();

        private readonly DecisionConsensusCalculator _consensusCalculator =
            new DecisionConsensusCalculator();

        private readonly DecisionQualityCalculator _qualityCalculator =
            new DecisionQualityCalculator();

        private readonly DecisionConfidenceCalculator _confidenceCalculator =
            new DecisionConfidenceCalculator();

        public Decision Evaluate(DecisionInputSnapshot input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            DecisionScoreSnapshot score =
                _scoreCalculator.Calculate(
                    input.ToDecisionScoreInput());

            DecisionConsensusSnapshot consensus =
                _consensusCalculator.Calculate(
                    score.Buy,
                    score.Sell,
                    input.SmartScoreTemperature,
                    input.MinimumSmartDirectionShare);

            DecisionEvidenceSnapshot evidence = input.Evidence;

            // M15 is the canonical decision timeframe. Lower-timeframe evidence
            // may improve entry precision, but a strong, closed M15 direction
            // must not be overturned by a collection of faster-frame votes.
            int m15Direction =
                input.M15Frame == null
                    ? 0
                    : input.M15Frame.Direction;

            bool m15ContextTooWeak =
                consensus.Direction != 0 &&
                m15Direction != 0 &&
                input.M15Frame != null &&
                input.M15Frame.Quality <
                    Math.Max(
                        65,
                        input.MinimumSmartDirectionShare);

            if (m15ContextTooWeak)
            {
                return new Decision
                {
                    BuyShare = consensus.BuyShare,
                    SellShare = consensus.SellShare,
                    Direction = 0,
                    Edge = consensus.Edge,
                    Regime = input.Regime,
                    RegimeQuality = evidence.RegimeQuality,
                    IndicatorConfluenceQuality =
                        input.M5Frame == null
                            ? 0
                            : input.M5Frame.IndicatorConfluenceQuality,
                    IndicatorConflict =
                        input.M5Frame == null
                            ? 0
                            : input.M5Frame.IndicatorConflict,
                    Confidence = consensus.Edge,
                    TriggerReady = false,
                    EntryAllowed = false,
                    BlockReason = "M15 CONTEXT WEAK",
                    Reason =
                        "M15 CONTEXT WEAK | Q=" +
                        input.M15Frame.Quality
                };
            }

            bool strongM15Conflict =
                consensus.Direction != 0 &&
                m15Direction != 0 &&
                m15Direction != consensus.Direction &&
                input.M15Frame != null &&
                input.M15Frame.Quality >=
                    Math.Max(
                        65,
                        input.MinimumSmartDirectionShare);

            if (strongM15Conflict)
            {
                return new Decision
                {
                    BuyShare = consensus.BuyShare,
                    SellShare = consensus.SellShare,
                    Direction = 0,
                    Edge = consensus.Edge,
                    Regime = input.Regime,
                    RegimeQuality = evidence.RegimeQuality,
                    IndicatorConfluenceQuality =
                        input.M5Frame == null
                            ? 0
                            : input.M5Frame.IndicatorConfluenceQuality,
                    IndicatorConflict =
                        input.M5Frame == null
                            ? 0
                            : input.M5Frame.IndicatorConflict,
                    Confidence = consensus.Edge,
                    TriggerReady = false,
                    EntryAllowed = false,
                    BlockReason = "M15 CANONICAL CONFLICT",
                    Reason =
                        "M15 CANONICAL CONFLICT | M15=" +
                        m15Direction +
                        " | CONSENSUS=" +
                        consensus.Direction
                };
            }

            Decision decision =
                new Decision
                {
                    BuyShare = consensus.BuyShare,
                    SellShare = consensus.SellShare,
                    Direction = consensus.Direction,
                    Edge = consensus.Edge,
                    Regime = input.Regime,
                    RegimeQuality = evidence.RegimeQuality,
                    IndicatorConfluenceQuality =
                        input.M5Frame == null
                            ? 0
                            : input.M5Frame.IndicatorConfluenceQuality,
                    IndicatorConflict =
                        input.M5Frame == null
                            ? 0
                            : input.M5Frame.IndicatorConflict
                };

            int strongestShare =
                Math.Max(
                    consensus.BuyShare,
                    consensus.SellShare);

            int selectedTimeframeAgreement =
                consensus.Direction == 0
                    ? 0
                    : consensus.Direction == 1
                        ? evidence.BullTimeframeAgreement
                        : evidence.BearTimeframeAgreement;

            int selectedIndependentEvidence =
                consensus.Direction == 0
                    ? 0
                    : consensus.Direction == 1
                        ? evidence.BullIndependentEvidence
                        : evidence.BearIndependentEvidence;

            int selectedStructuralConfirmations =
                consensus.Direction == 0
                    ? 0
                    : consensus.Direction == 1
                        ? evidence.BullStructuralConfirmations
                        : evidence.BearStructuralConfirmations;

            int selectedRetestQuality =
                consensus.Direction == 0
                    ? 0
                    : consensus.Direction == 1
                        ? evidence.BullRetestQuality
                        : evidence.BearRetestQuality;

            decision.TimeframeAgreement = selectedTimeframeAgreement;
            decision.IndependentEvidence = selectedIndependentEvidence;
            decision.IndependentEvidenceGroupCount =
                consensus.Direction == 0
                    ? 0
                    : consensus.Direction == 1
                        ? evidence.BullIndependentEvidenceGroups
                        : evidence.BearIndependentEvidenceGroups;
            decision.StructuralConfirmations = selectedStructuralConfirmations;
            decision.RetestQuality = selectedRetestQuality;

            decision.SmartQuality =
                _qualityCalculator.Calculate(
                    strongestShare,
                    selectedTimeframeAgreement,
                    selectedIndependentEvidence,
                    selectedStructuralConfirmations,
                    evidence.RegimeQuality,
                    selectedRetestQuality,
                    input.M5Frame == null
                        ? 0
                        : input.M5Frame.IndicatorConfluenceQuality,
                    input.M5Frame == null
                        ? 0
                        : input.M5Frame.IndicatorConflict,
                    decision.IndependentEvidenceGroupCount,
                    input.M15Frame == null
                        ? 0
                        : input.M15Frame.Quality);

            // A trade candidate must be supported by at least three independent
            // evidence families. Multiple correlated indicators inside one family
            // are not enough to manufacture a high-quality signal.
            if (decision.Direction != 0 &&
                decision.IndependentEvidenceGroupCount < 3)
            {
                decision.Direction = 0;
                decision.Confidence = strongestShare;
                decision.TriggerReady = false;
                decision.EntryAllowed = false;
                decision.BlockReason = "EVIDENCE DIVERSITY";
                decision.Reason =
                    "EVIDENCE DIVERSITY | GROUPS=" +
                    decision.IndependentEvidenceGroupCount;
                return decision;
            }

            if (decision.Direction == 0)
            {
                decision.Confidence = strongestShare;
                decision.TriggerReady = false;
                decision.EntryAllowed = false;
                decision.BlockReason = "SMART CONSENSUS";
                decision.Reason =
                    "NEUTRAL | BUY " +
                    consensus.BuyShare +
                    " | SELL " +
                    consensus.SellShare;
                return decision;
            }

            int calibrationAdjustment =
                decision.Direction == 1
                    ? evidence.BullConfidenceAdjustment
                    : evidence.BearConfidenceAdjustment;

            int higherTimeframePenalty =
                decision.Direction == 1
                    ? evidence.BullHigherTimeframePenalty
                    : evidence.BearHigherTimeframePenalty;

            decision.Confidence =
                _confidenceCalculator.Calculate(
                    strongestShare,
                    selectedTimeframeAgreement,
                    decision.SmartQuality,
                    calibrationAdjustment,
                    higherTimeframePenalty);

            bool closedM5TriggerReady =
                decision.Direction == 1
                    ? evidence.BullClosedBarTriggerReady
                    : evidence.BearClosedBarTriggerReady;

            bool m1TriggerReady =
                decision.Direction == 1
                    ? evidence.BullM1TriggerReady
                    : evidence.BearM1TriggerReady;

            decision.TriggerReady =
                closedM5TriggerReady &&
                (!input.UseM1Trigger || m1TriggerReady);

            return decision;
        }
    }
}
