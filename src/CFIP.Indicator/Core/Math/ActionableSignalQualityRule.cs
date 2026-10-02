using System;

namespace cAlgo
{
    internal readonly struct ActionableSignalQualityInput
    {
        public int Confidence { get; }
        public int SmartQuality { get; }
        public int TimeframeAgreement { get; }
        public int IndependentEvidence { get; }
        public int StructuralConfirmations { get; }
        public int EntryLocationQuality { get; }
        public int EntryTimingQuality { get; }
        public int EntryPositionQuality { get; }
        public double Tp1RR { get; }
        public int MinimumConfidence { get; }
        public int MinimumSmartQuality { get; }
        public int MinimumTimeframeAgreement { get; }
        public int MinimumIndependentEvidence { get; }
        public int MinimumStructuralConfirmations { get; }
        public int MinimumEntryLocationQuality { get; }
        public int MinimumEntryTimingQuality { get; }
        public int MinimumEntryPositionQuality { get; }
        public double MinimumTp1RR { get; }

        public ActionableSignalQualityInput(
            int confidence,
            int smartQuality,
            int timeframeAgreement,
            int independentEvidence,
            int structuralConfirmations,
            int entryLocationQuality,
            int entryTimingQuality,
            int entryPositionQuality,
            double tp1RR,
            int minimumConfidence,
            int minimumSmartQuality,
            int minimumTimeframeAgreement,
            int minimumIndependentEvidence,
            int minimumStructuralConfirmations,
            int minimumEntryLocationQuality,
            int minimumEntryTimingQuality,
            int minimumEntryPositionQuality,
            double minimumTp1RR)
        {
            Confidence = confidence;
            SmartQuality = smartQuality;
            TimeframeAgreement = timeframeAgreement;
            IndependentEvidence = independentEvidence;
            StructuralConfirmations = structuralConfirmations;
            EntryLocationQuality = entryLocationQuality;
            EntryTimingQuality = entryTimingQuality;
            EntryPositionQuality = entryPositionQuality;
            Tp1RR = tp1RR;
            MinimumConfidence = minimumConfidence;
            MinimumSmartQuality = minimumSmartQuality;
            MinimumTimeframeAgreement = minimumTimeframeAgreement;
            MinimumIndependentEvidence = minimumIndependentEvidence;
            MinimumStructuralConfirmations = minimumStructuralConfirmations;
            MinimumEntryLocationQuality = minimumEntryLocationQuality;
            MinimumEntryTimingQuality = minimumEntryTimingQuality;
            MinimumEntryPositionQuality = minimumEntryPositionQuality;
            MinimumTp1RR = minimumTp1RR;
        }
    }

    internal readonly struct ActionableSignalQualityResult
    {
        public bool Allowed { get; }
        public string Reason { get; }

        public ActionableSignalQualityResult(
            bool allowed,
            string reason)
        {
            Allowed = allowed;
            Reason = reason ?? string.Empty;
        }
    }

    internal static class ActionableSignalQualityRule
    {
        public static ActionableSignalQualityResult Evaluate(
            ActionableSignalQualityInput input)
        {
            if (input.Confidence < input.MinimumConfidence)
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • CONFIDENCE");

            if (input.SmartQuality < input.MinimumSmartQuality)
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • SMART QUALITY");

            if (input.TimeframeAgreement <
                input.MinimumTimeframeAgreement)
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • MTF");

            if (input.IndependentEvidence <
                input.MinimumIndependentEvidence)
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • INDEPENDENT EVIDENCE");

            if (input.StructuralConfirmations <
                input.MinimumStructuralConfirmations)
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • STRUCTURE");

            if (input.EntryLocationQuality <
                input.MinimumEntryLocationQuality &&
                !AllowsQualityRecovery(input))
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • LOCATION");

            if (input.EntryTimingQuality <
                input.MinimumEntryTimingQuality &&
                !AllowsQualityRecovery(input))
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • TIMING");

            if (input.EntryPositionQuality <
                input.MinimumEntryPositionQuality &&
                !AllowsQualityRecovery(input))
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • PRICE POSITION");

            if (!IsFinitePositiveValue(input.Tp1RR) ||
                !RiskRewardPolicyRule.MeetsMinimum(
                    input.Tp1RR,
                    input.MinimumTp1RR))
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • RR");

            return new ActionableSignalQualityResult(
                true,
                AllowsQualityRecovery(input) &&
                (input.EntryLocationQuality < input.MinimumEntryLocationQuality ||
                 input.EntryTimingQuality < input.MinimumEntryTimingQuality ||
                 input.EntryPositionQuality < input.MinimumEntryPositionQuality)
                    ? "ACTIONABLE • QUALITY RECOVERY"
                    : string.Empty);
        }

        private static bool AllowsQualityRecovery(
            ActionableSignalQualityInput input)
        {
            int deficientDimensions = 0;

            if (input.EntryLocationQuality < input.MinimumEntryLocationQuality)
                deficientDimensions++;
            if (input.EntryTimingQuality < input.MinimumEntryTimingQuality)
                deficientDimensions++;
            if (input.EntryPositionQuality < input.MinimumEntryPositionQuality)
                deficientDimensions++;

            if (deficientDimensions != 1)
                return false;

            int minimumDeficientQuality =
                Math.Min(
                    input.MinimumEntryLocationQuality,
                    Math.Min(
                        input.MinimumEntryTimingQuality,
                        input.MinimumEntryPositionQuality)) -
                ActionabilityThresholdPolicy.QualityRecoveryDeficitAllowance;

            int deficientQuality =
                Math.Min(
                    input.EntryLocationQuality,
                    Math.Min(
                        input.EntryTimingQuality,
                        input.EntryPositionQuality));

            int confidenceFloor =
                Math.Min(
                    ActionabilityThresholdPolicy.FinalConfidenceCap,
                    input.MinimumConfidence +
                    ActionabilityThresholdPolicy.RecoveryConfidenceMargin);
            int smartFloor =
                Math.Min(
                    ActionabilityThresholdPolicy.RecoverySmartQualityCap,
                    input.MinimumSmartQuality +
                    ActionabilityThresholdPolicy.RecoverySmartQualityMargin);
            int timeframeFloor =
                Math.Min(
                    ActionabilityThresholdPolicy.RecoveryTimeframeAgreementCap,
                    input.MinimumTimeframeAgreement +
                    ActionabilityThresholdPolicy.RecoveryTimeframeAgreementMargin);
            int evidenceFloor =
                Math.Min(
                    ActionabilityThresholdPolicy.FinalEvidenceCap,
                    input.MinimumIndependentEvidence +
                    ActionabilityThresholdPolicy.RecoveryIndependentEvidenceMargin);
            int structureFloor =
                Math.Min(
                    ActionabilityThresholdPolicy.FinalStructureCap,
                    input.MinimumStructuralConfirmations +
                    ActionabilityThresholdPolicy.RecoveryStructuralConfirmationsMargin);
            double rrFloor =
                input.MinimumTp1RR +
                ActionabilityThresholdPolicy.RecoveryTp1RrMargin;

            return
                deficientQuality >= minimumDeficientQuality &&
                input.Confidence >= confidenceFloor &&
                input.SmartQuality >= smartFloor &&
                input.TimeframeAgreement >= timeframeFloor &&
                input.IndependentEvidence >= evidenceFloor &&
                input.StructuralConfirmations >= structureFloor &&
                IsFinitePositiveValue(input.Tp1RR) &&
                input.Tp1RR >= rrFloor;
        }

        private static bool IsFinitePositiveValue(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}
