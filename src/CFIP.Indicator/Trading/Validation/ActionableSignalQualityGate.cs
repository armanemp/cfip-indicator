using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private ActionableSignalQualityResult EvaluateFinalActionableSignalQuality(
            Decision decision,
            TradeActionabilityResult actionability)
        {
            if (decision == null ||
                !actionability.Actionable)
                return new ActionableSignalQualityResult(
                    true,
                    string.Empty);

            double minimumTp1RR =
                MinimumRequiredRRForRegime(
                        decision.Regime);

            ActionabilityThresholdSnapshot thresholds =
                ActionabilityThresholdPolicy.ResolveFinal(
                    MinimumConfidence,
                    MinimumSmartQuality,
                    SmartQualityThreshold,
                    MinimumTimeframeAgreement,
                    SmartMinimumTimeframeAgreement,
                    MinimumIndependentEvidence,
                    SmartMinimumIndependentEvidence,
                    MinimumStructuralConfirmations,
                    MinimumEntryLocationQuality,
                    MinimumEntryQuality,
                    minimumTp1RR);

            return ActionableSignalQualityRule.Evaluate(
                new ActionableSignalQualityInput(
                    decision.Confidence,
                    decision.SmartQuality,
                    decision.TimeframeAgreement,
                    decision.IndependentEvidence,
                    decision.StructuralConfirmations,
                    actionability.LocationQuality,
                    actionability.TimingQuality,
                    actionability.PricePositionQuality,
                    actionability.Tp1RR,
                    thresholds.FinalMinimumConfidence,
                    thresholds.FinalMinimumSmartQuality,
                    thresholds.FinalMinimumTimeframeAgreement,
                    thresholds.FinalMinimumIndependentEvidence,
                    thresholds.FinalMinimumStructuralConfirmations,
                    thresholds.FinalMinimumEntryLocationQuality,
                    thresholds.FinalMinimumEntryTimingQuality,
                    thresholds.FinalMinimumEntryPositionQuality,
                    thresholds.FinalMinimumTp1RR));
        }
    }
}
