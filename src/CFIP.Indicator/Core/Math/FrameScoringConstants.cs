using System;

namespace cAlgo
{
    internal static class FrameScoringConstants
    {
        // Canonical structural-event contributions.
        internal const int StructureContribution = 16;
        internal const int MssContribution = 12;
        internal const int ChochContribution = 9;

        // Independent market-event contributions.
        internal const int DisplacementContribution = 10;
        internal const int LiquidityContribution = 10;
        internal const int EqualLevelContribution = 5;

        // Direction resolution.
        internal const int DirectionMinimumScore = 35;
        internal const int DirectionMinimumLead = 8;

        // Conflict penalty applied after indicator fusion.
        internal const int ConflictPenaltyThreshold = 45;
        internal const int ConflictPenaltyBaseline = 40;
        internal const int ConflictPenaltyCap = 6;
        internal const int ConflictPenaltyDivisor = 10;

        // RSI exhaustion defensive penalty.
        internal const double RsiBullExhaustionThreshold = 75.0;
        internal const double RsiBearExhaustionThreshold = 25.0;
        internal const int RsiExhaustionPenalty = 5;

        // Regime contribution.
        internal const double ChoppyRegimeBase = 8.0;
        internal const double ChoppyRegimeSlope = 0.30;
        internal const double NonChoppyRegimeCap = 14.0;
        internal const double NonChoppyAdxSlope = 0.35;
        internal const double RangeEfficiencyWeight = 8.0;
        internal const double EmaSpreadCap = 4.0;
        internal const double EmaSpreadWeight = 2.0;

        // Frame quality composition.
        internal const double StrongestQualityWeight = 0.40;
        internal const double AdxQualityScale = 1.45;
        internal const double AdxQualityWeight = 0.13;
        internal const double EvidenceQualityScale = 5.0;
        internal const double EvidenceQualityCap = 100.0;
        internal const double EvidenceQualityWeight = 0.20;
        internal const double RegimeQualityWeight = 0.15;
        internal const double IndicatorQualityWeight = 0.12;
        internal const int QualityConflictPenaltyCap = 10;
        internal const int QualityConflictPenaltyBaseline = 35;
        internal const int QualityConflictPenaltyDivisor = 6;

        internal const double DirectionalTotalMinimum = 1.0;
        internal const double PercentageScale = 100.0;
        internal const int QualityMinimum = 0;
        internal const int QualityMaximum = 100;
    }
}
