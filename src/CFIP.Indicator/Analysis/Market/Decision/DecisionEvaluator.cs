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
                _scoreCalculator.Calculate(input);

            DecisionConsensusSnapshot consensus =
                _consensusCalculator.Calculate(
                    score.Buy,
                    score.Sell,
                    input.SmartScoreTemperature,
                    input.MinimumSmartDirectionShare);

            DecisionEvidenceSnapshot evidence = input.Evidence;

            Decision decision =
                new Decision
                {
                    BuyShare = consensus.BuyShare,
                    SellShare = consensus.SellShare,
                    Direction = consensus.Direction,
                    Edge = consensus.Edge,
                    Regime = input.Regime,
                    RegimeQuality = evidence.RegimeQuality
                };

            int strongestShare =
                Math.Max(
                    consensus.BuyShare,
                    consensus.SellShare);

            int selectedTimeframeAgreement =
                consensus.Direction == 1
                    ? evidence.BullTimeframeAgreement
                    : evidence.BearTimeframeAgreement;

            int selectedIndependentEvidence =
                consensus.Direction == 1
                    ? evidence.BullIndependentEvidence
                    : evidence.BearIndependentEvidence;

            int selectedStructuralConfirmations =
                consensus.Direction == 1
                    ? evidence.BullStructuralConfirmations
                    : evidence.BearStructuralConfirmations;

            int selectedRetestQuality =
                consensus.Direction == 1
                    ? evidence.BullRetestQuality
                    : evidence.BearRetestQuality;

            decision.TimeframeAgreement = selectedTimeframeAgreement;
            decision.IndependentEvidence = selectedIndependentEvidence;
            decision.StructuralConfirmations = selectedStructuralConfirmations;
            decision.RetestQuality = selectedRetestQuality;

            decision.SmartQuality =
                _qualityCalculator.Calculate(
                    strongestShare,
                    selectedTimeframeAgreement,
                    selectedIndependentEvidence,
                    selectedStructuralConfirmations,
                    evidence.RegimeQuality);

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

            decision.TriggerReady =
                decision.Direction == 1
                    ? evidence.BullClosedBarTriggerReady
                    : evidence.BearClosedBarTriggerReady;

            return decision;
        }
    }
}
