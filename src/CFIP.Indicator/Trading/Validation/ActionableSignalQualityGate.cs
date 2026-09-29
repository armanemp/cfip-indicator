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
                actionability == null ||
                !actionability.Actionable)
                return new ActionableSignalQualityResult(
                    true,
                    string.Empty);

            int minimumConfidence =
                Math.Min(
                    99,
                    Math.Max(
                        50,
                        MinimumConfidence + 4));

            int minimumSmartQuality =
                Math.Min(
                    95,
                    Math.Max(
                        MinimumSmartQuality,
                        SmartQualityThreshold) + 3);

            int minimumTimeframeAgreement =
                Math.Min(
                    100,
                    Math.Max(
                        MinimumTimeframeAgreement,
                        SmartMinimumTimeframeAgreement) + 3);

            int minimumIndependentEvidence =
                Math.Min(
                    8,
                    Math.Max(
                        MinimumIndependentEvidence,
                        SmartMinimumIndependentEvidence) + 1);

            int minimumStructuralConfirmations =
                Math.Min(
                    8,
                    MinimumStructuralConfirmations + 1);

            int minimumEntryLocationQuality =
                Math.Max(
                    70,
                    MinimumEntryLocationQuality);

            int minimumEntryTimingQuality = 75;
            int minimumEntryPositionQuality = 70;

            double minimumTp1RR =
                Math.Max(
                    Tp1MinimumRR,
                    MinimumRequiredRRForRegime(
                        decision.Regime));

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
                    minimumConfidence,
                    minimumSmartQuality,
                    minimumTimeframeAgreement,
                    minimumIndependentEvidence,
                    minimumStructuralConfirmations,
                    minimumEntryLocationQuality,
                    minimumEntryTimingQuality,
                    minimumEntryPositionQuality,
                    minimumTp1RR));
        }
    }
}
