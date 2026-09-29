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
                input.MinimumEntryLocationQuality)
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • LOCATION");

            if (input.EntryTimingQuality <
                input.MinimumEntryTimingQuality)
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • TIMING");

            if (input.EntryPositionQuality <
                input.MinimumEntryPositionQuality)
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • PRICE POSITION");

            if (!IsFinitePositiveValue(input.Tp1RR) ||
                input.Tp1RR < input.MinimumTp1RR)
                return new ActionableSignalQualityResult(
                    false,
                    "SIGNAL QUALITY • RR");

            return new ActionableSignalQualityResult(
                true,
                string.Empty);
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
