using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private string ResolveFrameRegime(
            Frame frame)
        {
            if (frame == null)
                return FrameRegimeResolutionRule.Unknown;

            return FrameRegimeResolutionRule.NormalizeFrameRegimeValue(
                frame.Regime);
        }

        private Frame ScoreMarketFrame(
            Frame f)
        {
            if (f == null ||
                f.Bars == null ||
                f.Index < 0 ||
                f.Index >= f.Bars.Count ||
                !f.NativeIndicatorsReady)
                return f;

            Bars bars = f.Bars;
            int index = f.Index;

            int bull = 0;
            int bear = 0;
            int evidence = 0;

            // Structure, MSS and CHOCH can describe the same causal break.
            // Count one structural event only, preserving the strongest applicable
            // label instead of stacking correlated points.
            if (StructuralEvidenceRule.HasCanonicalStructuralEvent(
                    f.StructureBull,
                    f.MssBull,
                    f.ChochBull))
            {
                if (f.StructureBull)
                    AddScore(true, FrameScoringConstants.StructureContribution, ref bull, ref evidence);
                else if (f.MssBull)
                    AddScore(true, FrameScoringConstants.MssContribution, ref bull, ref evidence);
                else
                    AddScore(true, FrameScoringConstants.ChochContribution, ref bull, ref evidence);
            }

            if (StructuralEvidenceRule.HasCanonicalStructuralEvent(
                    f.StructureBear,
                    f.MssBear,
                    f.ChochBear))
            {
                if (f.StructureBear)
                    AddScore(true, FrameScoringConstants.StructureContribution, ref bear, ref evidence);
                else if (f.MssBear)
                    AddScore(true, FrameScoringConstants.MssContribution, ref bear, ref evidence);
                else
                    AddScore(true, FrameScoringConstants.ChochContribution, ref bear, ref evidence);
            }
            AddScore(f.DisplacementBull, FrameScoringConstants.DisplacementContribution, ref bull, ref evidence);
            AddScore(f.DisplacementBear, FrameScoringConstants.DisplacementContribution, ref bear, ref evidence);
            AddScore(f.LiquidityBull, FrameScoringConstants.LiquidityContribution, ref bull, ref evidence);
            AddScore(f.LiquidityBear, FrameScoringConstants.LiquidityContribution, ref bear, ref evidence);
            LocationEvidenceScore bullLocation =
                LocationEvidenceRule.Evaluate(
                    f.FvgBull,
                    f.FvgBullQuality,
                    f.ObBull,
                    f.ObBullQuality,
                    f.FvgObBullConfluence);

            LocationEvidenceScore bearLocation =
                LocationEvidenceRule.Evaluate(
                    f.FvgBear,
                    f.FvgBearQuality,
                    f.ObBear,
                    f.ObBearQuality,
                    f.FvgObBearConfluence);

            f.LocationEvidenceBull =
                bullLocation.Score;
            f.LocationEvidenceBear =
                bearLocation.Score;

            f.LocationEvidenceBullCount =
                bullLocation.Evidence;
            f.LocationEvidenceBearCount =
                bearLocation.Evidence;

            bull +=
                bullLocation.Score;
            bear +=
                bearLocation.Score;

            evidence +=
                bullLocation.Evidence +
                bearLocation.Evidence;

            AddScore(
                f.EqualLow,
                FrameScoringConstants.EqualLevelContribution,
                ref bull,
                ref evidence);

            AddScore(
                f.EqualHigh,
                FrameScoringConstants.EqualLevelContribution,
                ref bear,
                ref evidence);

            IndicatorEvidenceFusionInput indicatorFusionInput =
                new IndicatorEvidenceFusionInput(
                    ResolveFrameRegime(f), f.TrendBull, f.TrendBear,
                    f.MomentumBull, f.MomentumBear, f.MacdBull, f.MacdBear,
                    f.VwapBull, f.VwapBear, f.VolumeBull, f.VolumeBear,
                    f.VolatilityBull, f.VolatilityBear,
                    UseVolumeExpansionEvidence, UseMacdEvidence,
                    UseVwapEvidence, UseHealthyVolatilityEvidence,
                    WaveTrendEvidenceWeight, f.Adx, AdxMinimum, f.Rsi,
                    DmiBias(bars, index), UseEmaSlope ? f.EmaSlopeAtr : 0,
                    f.WaveTrendDirection, f.WaveTrendQuality,
                    MinimumWaveTrendQuality,
                    f.DivergenceDirection, f.DivergenceQuality,
                    f.WaveTrendBullCross, f.WaveTrendBearCross,
                    f.WaveTrendOversold, f.WaveTrendOverbought,
                    f.OssBullVotes, f.OssBearVotes, f.OssIndicatorCount,
                    OssConfluenceWeight, MinimumOssIndicatorAgreement);

            IndicatorEvidenceFusionResult indicatorFusion =
                IndicatorEvidenceFusionRule.Evaluate(indicatorFusionInput);

            f.IndicatorIndependentEvidenceGroupCount =
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(indicatorFusionInput);

            f.IndicatorConfluenceQuality = indicatorFusion.Quality;
            f.IndicatorConflict = indicatorFusion.Conflict;
            if (indicatorFusion.Conflict >= FrameScoringConstants.ConflictPenaltyThreshold)
            {
                if (bull >= bear)
                    bull =
                        Math.Max(
                            0,
                            bull -
                            Math.Min(
                                FrameScoringConstants.ConflictPenaltyCap,
                                (indicatorFusion.Conflict - FrameScoringConstants.ConflictPenaltyBaseline) /
                                FrameScoringConstants.ConflictPenaltyDivisor));
                else
                    bear =
                        Math.Max(
                            0,
                            bear -
                            Math.Min(
                                FrameScoringConstants.ConflictPenaltyCap,
                                (indicatorFusion.Conflict - FrameScoringConstants.ConflictPenaltyBaseline) /
                                FrameScoringConstants.ConflictPenaltyDivisor));
            }

            if (AvoidRsiExhaustion)
            {
                if (f.Rsi >= FrameScoringConstants.RsiBullExhaustionThreshold)
                    bull = Math.Max(
                        0,
                        bull - FrameScoringConstants.RsiExhaustionPenalty);

                if (f.Rsi <= FrameScoringConstants.RsiBearExhaustionThreshold)
                    bear = Math.Max(
                        0,
                        bear - FrameScoringConstants.RsiExhaustionPenalty);
            }

            f.BullScore = bull;
            f.BearScore = bear;
            f.Evidence = evidence;

            if (bull >= FrameScoringConstants.DirectionMinimumScore &&
                bull >= bear + FrameScoringConstants.DirectionMinimumLead)
                f.Direction = 1;
            else if (bear >= FrameScoringConstants.DirectionMinimumScore &&
                     bear >= bull + FrameScoringConstants.DirectionMinimumLead)
                f.Direction = -1;

            double total =
                Math.Max(
                    FrameScoringConstants.DirectionalTotalMinimum,
                    bull + bear);

            double strongest =
                FrameScoringConstants.PercentageScale *
                Math.Max(
                    bull,
                    bear) /
                total;

            double regimeContribution;

            if (f.Choppy)
            {
                regimeContribution =
                    Math.Max(
                        0,
                        FrameScoringConstants.ChoppyRegimeBase -
                        Math.Max(
                            0,
                            f.Choppiness -
                            RangeChoppinessThreshold) *
                        FrameScoringConstants.ChoppyRegimeSlope);
            }
            else
            {
                regimeContribution =
                    Math.Min(
                        FrameScoringConstants.NonChoppyRegimeCap,
                        Math.Max(
                            0,
                            f.Adx -
                            MinimumTransitionAdx) *
                        FrameScoringConstants.NonChoppyAdxSlope +
                        f.RangeEfficiency * FrameScoringConstants.RangeEfficiencyWeight +
                        Math.Min(
                            FrameScoringConstants.EmaSpreadCap,
                            f.EmaSpreadAtr * FrameScoringConstants.EmaSpreadWeight));
            }

            f.Quality =
                ClampInt(
                    (int)Math.Round(
                        strongest * FrameScoringConstants.StrongestQualityWeight +
                        Math.Min(
                            FrameScoringConstants.EvidenceQualityCap,
                            f.Adx * FrameScoringConstants.AdxQualityScale) *
                        FrameScoringConstants.AdxQualityWeight +
                        Math.Min(
                            FrameScoringConstants.EvidenceQualityCap,
                            evidence * FrameScoringConstants.EvidenceQualityScale) *
                        FrameScoringConstants.EvidenceQualityWeight +
                        regimeContribution * FrameScoringConstants.RegimeQualityWeight +
                        f.IndicatorConfluenceQuality * FrameScoringConstants.IndicatorQualityWeight) -
                    Math.Min(
                        FrameScoringConstants.QualityConflictPenaltyCap,
                        Math.Max(
                            0,
                            f.IndicatorConflict - FrameScoringConstants.QualityConflictPenaltyBaseline) /
                        FrameScoringConstants.QualityConflictPenaltyDivisor),
                    FrameScoringConstants.QualityMinimum,
                    FrameScoringConstants.QualityMaximum);

            return f;
        }
    }
}
