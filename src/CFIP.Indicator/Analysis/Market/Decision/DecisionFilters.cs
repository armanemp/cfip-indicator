using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool PassesDecisionFilters(
            int closedM5,
            DateTime reference,
            Decision decision,
            out string reason)
        {
            reason = string.Empty;

            int adaptiveQualityThreshold;
            int adaptiveShareThreshold;
            int adaptiveEdgeThreshold;

            GetAdaptiveSmartThresholds(
                decision == null
                    ? "UNKNOWN"
                    : decision.Regime,
                out adaptiveQualityThreshold,
                out adaptiveShareThreshold,
                out adaptiveEdgeThreshold);

            DecisionFilterResult thresholdResult =
                new DecisionThresholdFilterEvaluator().Evaluate(
                    decision,
                    new DecisionThresholdFilterInput(
                        decision == null ? 0 : decision.Confidence,
                        decision == null ? 0 : decision.Edge,
                        decision == null ? 0 : decision.SmartQuality,
                        decision == null ? 0 : decision.TimeframeAgreement,
                        decision == null ? 0 : decision.IndependentEvidence,
                        decision == null ? 0 : decision.StructuralConfirmations,
                        MinimumConfidence,
                        adaptiveEdgeThreshold,
                        adaptiveQualityThreshold,
                        RequireHigherTfAgreement,
                        MinimumTimeframeAgreement,
                        MinimumIndependentEvidence,
                        SmartMinimumIndependentEvidence,
                        EnableSmartDecisionEngine,
                        RequireStructuralConfirmation,
                        MinimumStructuralConfirmations));

            if (!thresholdResult.Allowed)
            {
                reason = thresholdResult.Reason;
                return false;
            }

            DecisionFilterResult confirmationResult =
                EvaluateDecisionConfirmationGates(
                    closedM5,
                    decision);

            if (!confirmationResult.Allowed)
            {
                reason = confirmationResult.Reason;
                return false;
            }

            DecisionFilterResult smartResult =
                EvaluateDecisionSmartGates(
                    closedM5,
                    decision,
                    adaptiveQualityThreshold,
                    adaptiveShareThreshold);

            if (!smartResult.Allowed)
            {
                reason = smartResult.Reason;
                return false;
            }

            DecisionFilterResult structureResult =
                EvaluateDecisionStructureGates(
                    reference,
                    closedM5,
                    decision);

            if (!structureResult.Allowed)
            {
                reason = structureResult.Reason;
                return false;
            }

            DecisionFilterResult marketResult =
                EvaluateDecisionMarketGates(
                    closedM5);

            if (!marketResult.Allowed)
            {
                reason = marketResult.Reason;
                return false;
            }

            DecisionFilterResult lifecycleResult =
                EvaluateDecisionLifecycleGates(
                    closedM5,
                    decision);

            if (!lifecycleResult.Allowed)
            {
                reason = lifecycleResult.Reason;
                return false;
            }

            return true;
        }
    }
}
