using System;

namespace cAlgo
{
    internal readonly struct ActionabilityThresholdSnapshot
    {
        public int UpstreamEntryLocationQuality { get; }
        public int UpstreamEntryTimingQuality { get; }
        public int FinalMinimumConfidence { get; }
        public int FinalMinimumSmartQuality { get; }
        public int FinalMinimumTimeframeAgreement { get; }
        public int FinalMinimumIndependentEvidence { get; }
        public int FinalMinimumStructuralConfirmations { get; }
        public int FinalMinimumEntryLocationQuality { get; }
        public int FinalMinimumEntryTimingQuality { get; }
        public int FinalMinimumEntryPositionQuality { get; }
        public double FinalMinimumTp1RR { get; }
        public int RecoveryMinimumDeficientQuality { get; }
        public int RecoveryConfidenceFloor { get; }
        public int RecoverySmartQualityFloor { get; }
        public int RecoveryTimeframeAgreementFloor { get; }
        public int RecoveryIndependentEvidenceFloor { get; }
        public int RecoveryStructuralConfirmationsFloor { get; }
        public double RecoveryTp1RRFloor { get; }

        public ActionabilityThresholdSnapshot(
            int upstreamEntryLocationQuality,
            int upstreamEntryTimingQuality,
            int finalMinimumConfidence,
            int finalMinimumSmartQuality,
            int finalMinimumTimeframeAgreement,
            int finalMinimumIndependentEvidence,
            int finalMinimumStructuralConfirmations,
            int finalMinimumEntryLocationQuality,
            int finalMinimumEntryTimingQuality,
            int finalMinimumEntryPositionQuality,
            double finalMinimumTp1RR)
        {
            UpstreamEntryLocationQuality = upstreamEntryLocationQuality;
            UpstreamEntryTimingQuality = upstreamEntryTimingQuality;
            FinalMinimumConfidence = finalMinimumConfidence;
            FinalMinimumSmartQuality = finalMinimumSmartQuality;
            FinalMinimumTimeframeAgreement = finalMinimumTimeframeAgreement;
            FinalMinimumIndependentEvidence = finalMinimumIndependentEvidence;
            FinalMinimumStructuralConfirmations = finalMinimumStructuralConfirmations;
            FinalMinimumEntryLocationQuality = finalMinimumEntryLocationQuality;
            FinalMinimumEntryTimingQuality = finalMinimumEntryTimingQuality;
            FinalMinimumEntryPositionQuality = finalMinimumEntryPositionQuality;
            FinalMinimumTp1RR = finalMinimumTp1RR;

            RecoveryMinimumDeficientQuality =
                Math.Min(
                    finalMinimumEntryLocationQuality,
                    Math.Min(
                        finalMinimumEntryTimingQuality,
                        finalMinimumEntryPositionQuality)) -
                ActionabilityThresholdPolicy.QualityRecoveryDeficitAllowance;

            RecoveryConfidenceFloor =
                Math.Min(
                    ActionabilityThresholdPolicy.FinalConfidenceCap,
                    finalMinimumConfidence +
                    ActionabilityThresholdPolicy.RecoveryConfidenceMargin);

            RecoverySmartQualityFloor =
                Math.Min(
                    ActionabilityThresholdPolicy.RecoverySmartQualityCap,
                    finalMinimumSmartQuality +
                    ActionabilityThresholdPolicy.RecoverySmartQualityMargin);

            RecoveryTimeframeAgreementFloor =
                Math.Min(
                    ActionabilityThresholdPolicy.RecoveryTimeframeAgreementCap,
                    finalMinimumTimeframeAgreement +
                    ActionabilityThresholdPolicy.RecoveryTimeframeAgreementMargin);

            RecoveryIndependentEvidenceFloor =
                Math.Min(
                    ActionabilityThresholdPolicy.FinalEvidenceCap,
                    finalMinimumIndependentEvidence +
                    ActionabilityThresholdPolicy.RecoveryIndependentEvidenceMargin);

            RecoveryStructuralConfirmationsFloor =
                Math.Min(
                    ActionabilityThresholdPolicy.FinalStructureCap,
                    finalMinimumStructuralConfirmations +
                    ActionabilityThresholdPolicy.RecoveryStructuralConfirmationsMargin);

            RecoveryTp1RRFloor =
                finalMinimumTp1RR +
                ActionabilityThresholdPolicy.RecoveryTp1RrMargin;
        }
    }

    internal static class ActionabilityThresholdPolicy
    {
        public const int UpstreamEntryLocationQualityFloor = 64;
        public const int UpstreamEntryTimingQualityFloor = 64;
        public const int PrecisionEntryQualityFloor = 40;
        public const double ExpansionMinimumTp1RRFloor = 2.10;
        public const double RangeMinimumTp1RRFloor = 2.25;
        public const int IndicatorMinimumQuality = 60;
        public const int IndicatorRangeTransitionMinimumQuality = 58;
        public const int IndicatorMaximumConflict = 52;
        public const int IndicatorRangeTransitionMaximumConflict = 55;

        public const int FinalEntryLocationQualityFloor = 70;
        public const int FinalEntryTimingQualityFloor = 75;
        public const int FinalEntryPositionQualityFloor = 70;

        public const int FinalConfidenceMargin = 4;
        public const int FinalSmartQualityMargin = 3;
        public const int FinalTimeframeAgreementMargin = 3;
        public const int FinalIndependentEvidenceMargin = 1;
        public const int FinalStructuralConfirmationsMargin = 1;
        public const int ParallelCandidateQualityMargin = 3;

        public const int FinalConfidenceCap = 99;
        public const int FinalSmartQualityCap = 95;
        public const int FinalTimeframeAgreementCap = 100;
        public const int FinalEvidenceCap = 8;
        public const int FinalStructureCap = 8;

        public const int QualityRecoveryDeficitAllowance = 8;
        public const int RecoveryConfidenceMargin = 5;
        public const int RecoverySmartQualityMargin = 5;
        public const int RecoveryTimeframeAgreementMargin = 3;
        public const int RecoveryIndependentEvidenceMargin = 1;
        public const int RecoveryStructuralConfirmationsMargin = 1;
        public const double RecoveryTp1RrMargin = 0.35;
        public const int RecoverySmartQualityCap = 100;
        public const int RecoveryTimeframeAgreementCap = 100;

        public static int EffectiveUpstreamEntryLocationQuality(
            int configuredMinimumEntryQuality)
        {
            return Math.Max(
                configuredMinimumEntryQuality,
                UpstreamEntryLocationQualityFloor);
        }

        public static int EffectiveUpstreamEntryTimingQuality()
        {
            return UpstreamEntryTimingQualityFloor;
        }

        public static int EffectivePrecisionEntryQualityFloor(
            int configuredMinimumEntryQuality)
        {
            return Math.Max(
                PrecisionEntryQualityFloor,
                configuredMinimumEntryQuality);
        }

        public static int ApplyParallelCandidateQualityMargin(
            int minimumQuality,
            bool applyAdditionalMargin)
        {
            if (!applyAdditionalMargin)
                return minimumQuality;

            return Math.Min(
                FinalSmartQualityCap,
                minimumQuality +
                ParallelCandidateQualityMargin);
        }

        public static ActionabilityThresholdSnapshot ResolveFinal(
            int minimumConfidence,
            int minimumSmartQuality,
            int smartQualityThreshold,
            int minimumTimeframeAgreement,
            int smartMinimumTimeframeAgreement,
            int minimumIndependentEvidence,
            int smartMinimumIndependentEvidence,
            int minimumStructuralConfirmations,
            int minimumEntryLocationQuality,
            int configuredMinimumEntryQuality,
            double minimumTp1RR)
        {
            int finalMinimumConfidence =
                Math.Min(
                    FinalConfidenceCap,
                    Math.Max(
                        50,
                        minimumConfidence +
                        FinalConfidenceMargin));

            int finalMinimumSmartQuality =
                Math.Min(
                    FinalSmartQualityCap,
                    Math.Max(
                        minimumSmartQuality,
                        smartQualityThreshold) +
                    FinalSmartQualityMargin);

            int finalMinimumTimeframeAgreement =
                Math.Min(
                    FinalTimeframeAgreementCap,
                    Math.Max(
                        minimumTimeframeAgreement,
                        smartMinimumTimeframeAgreement) +
                    FinalTimeframeAgreementMargin);

            int finalMinimumIndependentEvidence =
                Math.Min(
                    FinalEvidenceCap,
                    Math.Max(
                        minimumIndependentEvidence,
                        smartMinimumIndependentEvidence) +
                    FinalIndependentEvidenceMargin);

            int finalMinimumStructuralConfirmations =
                Math.Min(
                    FinalStructureCap,
                    minimumStructuralConfirmations +
                    FinalStructuralConfirmationsMargin);

            int finalMinimumEntryLocationQuality =
                Math.Max(
                    FinalEntryLocationQualityFloor,
                    minimumEntryLocationQuality);

            int finalMinimumEntryTimingQuality =
                FinalEntryTimingQualityFloor;

            int finalMinimumEntryPositionQuality =
                FinalEntryPositionQualityFloor;

            return new ActionabilityThresholdSnapshot(
                EffectiveUpstreamEntryLocationQuality(
                    configuredMinimumEntryQuality),
                EffectiveUpstreamEntryTimingQuality(),
                finalMinimumConfidence,
                finalMinimumSmartQuality,
                finalMinimumTimeframeAgreement,
                finalMinimumIndependentEvidence,
                finalMinimumStructuralConfirmations,
                finalMinimumEntryLocationQuality,
                finalMinimumEntryTimingQuality,
                finalMinimumEntryPositionQuality,
                minimumTp1RR);
        }
    }
}
