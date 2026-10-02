using System;

namespace cAlgo
{
    internal sealed class DecisionEvidenceSnapshot
    {
        public int BullTimeframeAgreement { get; }
        public int BearTimeframeAgreement { get; }
        public int BullIndependentEvidence { get; }
        public int BearIndependentEvidence { get; }
        public int BullIndependentEvidenceGroups { get; }
        public int BearIndependentEvidenceGroups { get; }
        public int BullStructuralConfirmations { get; }
        public int BearStructuralConfirmations { get; }
        public int RegimeQuality { get; }
        public int BullRetestQuality { get; }
        public int BearRetestQuality { get; }
        public bool BullClosedBarTriggerReady { get; }
        public bool BearClosedBarTriggerReady { get; }
        public bool BullM1TriggerReady { get; }
        public bool BearM1TriggerReady { get; }
        public int BullConfidenceAdjustment { get; }
        public int BearConfidenceAdjustment { get; }
        public int BullHigherTimeframePenalty { get; }
        public int BearHigherTimeframePenalty { get; }

        public DecisionEvidenceSnapshot(
            int bullTimeframeAgreement,
            int bearTimeframeAgreement,
            int bullIndependentEvidence,
            int bearIndependentEvidence,
            int bullIndependentEvidenceGroups,
            int bearIndependentEvidenceGroups,
            int bullStructuralConfirmations,
            int bearStructuralConfirmations,
            int regimeQuality,
            int bullRetestQuality,
            int bearRetestQuality,
            bool bullClosedBarTriggerReady,
            bool bearClosedBarTriggerReady,
            bool bullM1TriggerReady,
            bool bearM1TriggerReady,
            int bullConfidenceAdjustment,
            int bearConfidenceAdjustment,
            int bullHigherTimeframePenalty,
            int bearHigherTimeframePenalty)
        {
            BullTimeframeAgreement = NumericGuards.ClampInt(bullTimeframeAgreement, 0, 100);
            BearTimeframeAgreement = NumericGuards.ClampInt(bearTimeframeAgreement, 0, 100);
            BullIndependentEvidence = Math.Max(0, bullIndependentEvidence);
            BearIndependentEvidence = Math.Max(0, bearIndependentEvidence);
            BullIndependentEvidenceGroups = NumericGuards.ClampInt(
                bullIndependentEvidenceGroups,
                0,
                4);
            BearIndependentEvidenceGroups = NumericGuards.ClampInt(
                bearIndependentEvidenceGroups,
                0,
                4);
            BullStructuralConfirmations = Math.Max(0, bullStructuralConfirmations);
            BearStructuralConfirmations = Math.Max(0, bearStructuralConfirmations);
            RegimeQuality = NumericGuards.ClampInt(regimeQuality, 0, 100);
            BullRetestQuality = NumericGuards.ClampInt(bullRetestQuality, 0, 100);
            BearRetestQuality = NumericGuards.ClampInt(bearRetestQuality, 0, 100);
            BullClosedBarTriggerReady = bullClosedBarTriggerReady;
            BearClosedBarTriggerReady = bearClosedBarTriggerReady;
            BullM1TriggerReady = bullM1TriggerReady;
            BearM1TriggerReady = bearM1TriggerReady;
            BullConfidenceAdjustment = NumericGuards.ClampInt(bullConfidenceAdjustment, -100, 100);
            BearConfidenceAdjustment = NumericGuards.ClampInt(bearConfidenceAdjustment, -100, 100);
            BullHigherTimeframePenalty = Math.Max(0, bullHigherTimeframePenalty);
            BearHigherTimeframePenalty = Math.Max(0, bearHigherTimeframePenalty);
        }
    }
}
