using System;

namespace cAlgo
{
    internal sealed class DecisionThresholdFilterEvaluator
    {
        public DecisionFilterResult Evaluate(
            Decision decision,
            DecisionThresholdFilterInput input)
        {
            if (decision == null ||
                decision.Direction == 0)
                return new DecisionFilterResult(false, "NO DIRECTION");

            if (input.Confidence < input.MinimumConfidence)
                return new DecisionFilterResult(false, "CONFIDENCE");

            if (input.Edge < input.MinimumEdge)
                return new DecisionFilterResult(false, "EDGE");

            if (input.SmartQuality < input.MinimumSmartQuality)
                return new DecisionFilterResult(false, "SMART QUALITY");

            if (input.RequireHigherTfAgreement &&
                input.TimeframeAgreement < input.MinimumTimeframeAgreement)
                return new DecisionFilterResult(false, "MTF AGREEMENT");

            if (input.IndependentEvidence <
                Math.Max(
                    input.MinimumIndependentEvidence,
                    input.SmartDecisionEnabled
                        ? input.SmartMinimumIndependentEvidence
                        : 0))
                return new DecisionFilterResult(false, "INDEPENDENT EVIDENCE");

            if (input.SmartDecisionEnabled &&
                input.IndependentEvidenceGroups <
                input.MinimumIndependentEvidenceGroups)
                return new DecisionFilterResult(false, "EVIDENCE DIVERSITY");

            if (input.RequireStructuralConfirmation &&
                input.StructuralConfirmations <
                input.MinimumStructuralConfirmations)
                return new DecisionFilterResult(false, "STRUCTURE");

            return new DecisionFilterResult(true, string.Empty);
        }
    }
}