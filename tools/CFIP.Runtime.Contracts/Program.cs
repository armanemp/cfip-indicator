using System;
using System.Collections.Generic;
using System.IO;

namespace cAlgo
{
    internal static class Program
    {
        private static void Main()
        {
            VerifyM1TriggerSemantics();
            VerifyCi10TriggerLifecycle();
            VerifyCi16DeterministicReplay();
            VerifySwingPlateauSemantics();
            VerifyFvgMathematics();
            VerifyFvgLifecycleSemantics();
            VerifyFvgQualitySemantics();
            VerifyExecutionThresholdSemantics();
            VerifyVolumeSizingSemantics();
            VerifyOrderBlockMathematics();
            VerifyOrderBlockQualitySemantics();
            VerifyZoneConfluenceSymmetry();
            VerifyTopDownCalibration();
            VerifyProtectionProgressionSemantics();
            IntelligentProtectionContracts.Run();
            VerifyWaveTrendMathematics();
            VerifyHistoricalRenderingSemantics();
            VerifyStructuralStopScoringSemantics();
            VerifyDivergenceThresholdSemantics();
            VerifyRejectionThresholdSemantics();
            VerifyLiveInvalidationSemantics();
            VerifyFalseSignalAdverseRSemantics();
            VerifyRewardQualityFloorSemantics();
            VerifyEarlyPredictionScoreSemantics();
            VerifyWaveTrendEvidence();
            VerifyParallelOpportunityRule();
            VerifyParallelScenarioSelectionSemantics();
            VerifyMicroReactionClosedBarSemantics();
            VerifyMtfContextIntegrity();
            VerifyCi07MarketStateSemantics();
            VerifyCi08DivergenceWaveTrendReactionEarlySignal();
            VerifyCanonicalMarketContext();
            VerifySessionWindowSemantics();
            VerifyCalculationReadinessSemantics();
            VerifyNativeIndicatorReadinessSemantics();
            VerifyPrimitiveIndicatorMathematics();
            VerifyOssQuoteProjectionSemantics();
            VerifyOssQuoteWindowSemantics();
            VerifyOssWarmupPolicy();
            VerifyStructuralStopRiskCeilingSemantics();
            VerifyStructuralStopGeometryCi12();
            VerifyLiquidityTargetCandidateSemantics();
            VerifyIndependentEvidenceGroupSemantics();
            VerifyCi03IndicatorEvidenceIndependence();
            VerifyPendingFillExitResolutionSemantics();
            VerifyBrokerStateRefreshSemantics();
            VerifyBufferedArchivePersistence();
            VerifyDailyLossSemantics();
            VerifyDailyLossBaselineSemantics();
            VerifyEconomicNewsFeedStateSemantics();
            VerifyEconomicNewsCurrencyMappingSemantics();
            VerifyClosedBarReferenceContract();
            VerifyMarketExecutionAcceptance();
            VerifyPendingOrderAcceptance();
            VerifyRejectedMutationHandling();
            VerifyFillEnvelopeSymmetry();
            VerifyInitialProtectionDirectionality();
            VerifyManagedBreakEvenDirectionality();
            VerifyProtectionProgression();
            VerifyTargetProgression();
            VerifyExecutionCapacity();
            VerifyStaleLivePlanRecovery();
            VerifyLifecycleFlows();
            VerifyLifecycleIdempotency();
            VerifyRuntimeStageIsolation();
            VerifyRuntimeFaultStateMachine();
            VerifyRuntimeExplicitRearmSemantics();
            VerifyAlertDeliveryQueueSemantics();
            VerifyClosedBarRetryPolicy();
            VerifyUnifiedSubmissionGate();
            VerifyVisualAndExecutionControls();
            VerifyResponsivePanelRuntime();
            VerifyPanelLiveContentRefresh();
            VerifyDirectionalExecutionFillAcceptance();
            VerifyAggressiveEntryPolicy();
            VerifyTradePlanRegistry();
            VerifyScenarioExecutionPolicy();
            VerifyActionabilityAndDivergenceState();
            VerifyManagedIdentitySemantics();
            VerifyReversalProfitThresholdSemantics();
            VerifyStructuralEventSemantics();
            VerifyLiquiditySweepSemantics();
            VerifyRejectionSemantics();
            VerifyDivergenceConflictSemantics();
            VerifyStructuralTimeframeSemantics();
            VerifyReactionQualificationSemantics();
            VerifyIndicatorExecutionQualitySemantics();
            VerifyPendingDecisionArbiterSemantics();
            VerifyLifecycleOutcomeSemantics();
            VerifyPartialTakeProfitRetrySemantics();
            VerifyServerPartialTakeProfitEvidenceSemantics();
            VerifyPeakPriceReconstructionSemantics();
            VerifySmartBreakEvenSemantics();
            VerifyTargetProgressionMonotonicity();
            VerifyOutcomeMemoryIdentitySemantics();
            VerifyPersistenceHealthSemantics();
            VerifySignalTraceLineageSemantics();
            VerifyFrameRegimeSemantics();
            VerifySmartThresholdRegimeSemantics();
            VerifyFrameScoringConstants();
            VerifyOssIndicatorParameters();
            VerifyDirectionalBiasTimeframeSemantics();
            VerifyWatchReactionAlertSemantics();
            VerifyOpposingZonePathF1();
            VerifyAggressiveRiskAndFillSemantics();
            VerifyActionabilityThresholdTransparency();
            VerifyEntryActionabilityF6();
            VerifyIndependentTimeframeScenarioSemanticsF7();
            VerifyMtfPrimaryTimeframeSignals();
            VerifyMtfPrimaryLocationEvidence();
            VerifyMtfPrimaryProviderIdentity();
            VerifyPanelFrameDirectionPresentation();
            VerifyTargetObstacleTelemetryF8();
            VerifyOrphanManagedProtectionF3();
            VerifyEntryTrapRiskG2();
            VerifyPlanLineThicknessG3();
            VerifyTargetObstacleCacheKeyHashSemantics();
            VerifyBrokerProtectionG1();
            VerifyExecutionProtectionPanelStateFreshnessG5();
            VerifyExecutionPanelPresentationIdentityG6A();
            VerifyExecutionControlPresentationG6B();
            VerifyExecutionProtectionPanelStateG4();

            Console.WriteLine("Runtime acceptance contracts OK");
        }




        private static void VerifyOrphanManagedProtectionF3()
        {
            Assert(
                !OrphanManagedProtectionRule.CanReportSuccess(
                    1,
                    false,
                    true),
                "F3 BUY invalid computed stop can never report protection success");

            Assert(
                !OrphanManagedProtectionRule.CanReportSuccess(
                    -1,
                    false,
                    true),
                "F3 SELL invalid computed stop can never report protection success");

            Assert(
                !OrphanManagedProtectionRule.CanReportSuccess(
                    1,
                    true,
                    false) &&
                !OrphanManagedProtectionRule.CanReportSuccess(
                    -1,
                    true,
                    false),
                "F3 broker protection rejection can never report success for BUY or SELL");

            Assert(
                !OrphanManagedProtectionRule.CanReportSuccess(
                    1,
                    false,
                    false) &&
                !OrphanManagedProtectionRule.CanReportSuccess(
                    -1,
                    false,
                    false),
                "F3 all-invalid protection state is fail-closed symmetrically");

            Assert(
                OrphanManagedProtectionRule.CanReportSuccess(
                    1,
                    true,
                    true) &&
                OrphanManagedProtectionRule.CanReportSuccess(
                    -1,
                    true,
                    true),
                "F3 only valid stop plus broker confirmation may report success for BUY and SELL");

            Assert(
                !OrphanManagedProtectionRule.CanReportSuccess(
                    0,
                    true,
                    true),
                "F3 invalid direction is fail-closed");
        }


        private static void VerifyTargetObstacleTelemetryF8()
        {
            TargetObstacleTelemetryAccumulator telemetry =
                new TargetObstacleTelemetryAccumulator();

            telemetry.Observe(
                0.80,
                0.40,
                4.0);

            telemetry.Observe(
                3.20,
                1.20,
                4.0);

            Assert(
                telemetry.Count == 2 &&
                telemetry.MinTargetDistanceAtr == 0.80 &&
                telemetry.MaxTargetDistanceAtr == 3.20 &&
                telemetry.MinObstacleDistanceAtr == 0.40 &&
                telemetry.MaxObstacleDistanceAtr == 1.20 &&
                telemetry.MinTargetExtensionPercent == 20.0 &&
                telemetry.MaxTargetExtensionPercent == 80.0,
                "target obstacle telemetry aggregates target/obstacle distance in ATR and existing extension-envelope percentage");

            Assert(
                telemetry.FormatSummary() ==
                    "TARGET_ATR_MIN=0.80 • TARGET_ATR_MAX=3.20 • EXT_PCT_MIN=20.0 • EXT_PCT_MAX=80.0 • OBSTACLE_ATR_MIN=0.40 • OBSTACLE_ATR_MAX=1.20",
                "target obstacle telemetry summary is deterministic and culture-neutral");

            telemetry.Observe(
                double.NaN,
                0.50,
                4.0);

            Assert(
                telemetry.Count == 2,
                "invalid obstacle observation does not contaminate bounded telemetry");

            TargetObstacleTelemetryAccumulator missingObstacle =
                new TargetObstacleTelemetryAccumulator();

            missingObstacle.Observe(
                2.0,
                -1,
                4.0);

            Assert(
                missingObstacle.Count == 1 &&
                missingObstacle.MinTargetDistanceAtr == 2.0 &&
                double.IsPositiveInfinity(
                    missingObstacle.MinObstacleDistanceAtr) &&
                missingObstacle.FormatSummary() ==
                    "TARGET_ATR_MIN=2.00 • TARGET_ATR_MAX=2.00 • EXT_PCT_MIN=50.0 • EXT_PCT_MAX=50.0",
                "zone-only obstacle telemetry retains target-distance evidence without inventing obstacle distance");

            Assert(
                TargetCandidateRejectionReasons.M5Obstacle ==
                    "OBSTACLE_SWING" &&
                TargetCandidateRejectionReasons.EqualHighLowObstacle ==
                    "OBSTACLE_EQ" &&
                TargetCandidateRejectionReasons.OpposingZoneObstacle ==
                    "OBSTACLE_OPPOSING_ZONE" &&
                TargetCandidateRejectionReasons.HtfZoneObstacle ==
                    "OBSTACLE_HTF_ZONE",
                "target obstacle rejection taxonomy preserves swing, equality, opposing-zone and HTF-zone categories");
        }

        private static void VerifyStructuralStopGeometryCi12()
        {
            Assert(
                StructuralStopGeometryRule.ResolveBufferAtr(
                    "M5",
                    0.10,
                    0.15) == 0.10 &&
                StructuralStopGeometryRule.ResolveBufferAtr(
                    "H1",
                    0.10,
                    0.15) == 0.15,
                "structural stop buffer uses M5 and HTF semantics without drift");

            StructuralStopGeometrySnapshot buy =
                StructuralStopGeometryRule.Evaluate(
                    1,
                    100,
                    100,
                    2,
                    2,
                    0.01,
                    "M5",
                    0.10,
                    0.15,
                    0.01,
                    2);

            StructuralStopGeometrySnapshot sell =
                StructuralStopGeometryRule.Evaluate(
                    -1,
                    100,
                    100,
                    2,
                    2,
                    0.01,
                    "M5",
                    0.10,
                    0.15,
                    0.01,
                    2);

            Assert(
                buy.IsValid &&
                sell.IsValid &&
                buy.Stop == 99.80 &&
                sell.Stop == 100.20 &&
                buy.RiskAtr == sell.RiskAtr,
                "BUY/SELL structural stop geometry is mirrored and normalized deterministically");

            StructuralStopGeometrySnapshot roundedBuy =
                StructuralStopGeometryRule.Evaluate(
                    1,
                    100,
                    100,
                    2,
                    2,
                    0.01,
                    "M5",
                    0.10,
                    0.15,
                    0.25,
                    2);

            StructuralStopGeometrySnapshot roundedSell =
                StructuralStopGeometryRule.Evaluate(
                    -1,
                    100,
                    100,
                    2,
                    2,
                    0.01,
                    "M5",
                    0.10,
                    0.15,
                    0.25,
                    2);

            Assert(
                roundedBuy.Stop == 99.75 &&
                roundedSell.Stop == 100.25 &&
                Math.Abs(
                    roundedBuy.Risk -
                    roundedSell.Risk) < 1e-9,
                "tick-size normalization preserves mirrored structural risk");

            Assert(
                !StructuralStopGeometryRule.Evaluate(
                    1,
                    100,
                    101,
                    2,
                    2,
                    0.01,
                    "M5",
                    0.10,
                    0.15,
                    0.01,
                    2).IsValid &&
                !StructuralStopGeometryRule.Evaluate(
                    -1,
                    100,
                    99,
                    2,
                    2,
                    0.01,
                    "M5",
                    0.10,
                    0.15,
                    0.01,
                    2).IsValid,
                "structural stop cannot cross to the non-protective side");

            StructuralStopGeometrySnapshot fallbackBuy =
                StructuralStopGeometryRule.EvaluateFallback(
                    1,
                    100,
                    2,
                    1,
                    0.01,
                    2);

            StructuralStopGeometrySnapshot fallbackSell =
                StructuralStopGeometryRule.EvaluateFallback(
                    -1,
                    100,
                    2,
                    1,
                    0.01,
                    2);

            Assert(
                fallbackBuy.IsValid &&
                fallbackSell.IsValid &&
                fallbackBuy.Stop == 98 &&
                fallbackSell.Stop == 102 &&
                fallbackBuy.RiskAtr == fallbackSell.RiskAtr &&
                fallbackBuy.Reason == "FALLBACK" &&
                fallbackSell.Reason == "FALLBACK",
                "fallback structural stop geometry is canonical and symmetric");

            Assert(
                StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(
                    1.80,
                    1.0,
                    0.55,
                    1.80,
                    2.25,
                    0.20,
                    0.01,
                    0.50) &&
                !StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(
                    1.81,
                    1.0,
                    0.55,
                    1.80,
                    2.25,
                    0.20,
                    0.01,
                    0.50),
                "planning stop risk remains bounded by the configured maximum");

            Assert(
                !StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(
                    1.80,
                    1.0,
                    0.55,
                    1.80,
                    2.25,
                    2.0,
                    0.01,
                    1.0),
                "spread pressure cannot raise the configured maximum stop-risk ceiling");

            Console.WriteLine(
                "CI-12 structural stop geometry and risk contracts PASS");
        }


        private static void VerifyStructuralStopRiskCeilingSemantics()
        {
            Assert(
                StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                    0.55,
                    1.80,
                    2.40) == 1.80,
                "effective stop ceiling uses the broad MaximumSlAtr when it is tighter");

            Assert(
                StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                    0.55,
                    2.40,
                    1.80) == 1.80,
                "effective stop ceiling uses the structural MaximumStructuralStopAtr when it is tighter");

            Assert(
                StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                    0.55,
                    0.30,
                    1.80) == 0.55 &&
                StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                    0.55,
                    1.80,
                    0.30) == 0.55,
                "effective ceiling preserves the minimum SL floor against either undersized maximum");

            Assert(
                StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                    1.80,
                    1.00,
                    1.20) == 1.80,
                "effective ceiling remains at the normalized minimum when both maximums are below it");

            Assert(
                StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                    0.80,
                    1.70,
                    2.10) ==
                StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                    0.80,
                    2.10,
                    1.70),
                "effective ceiling is symmetric in the two maximum-cap parameters");

            Assert(
                StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                    0.55,
                    1.80,
                    2.40) == 1.80,
                "legacy default effective ceiling remains 1.80 ATR");
        }

        private static void VerifyLiquidityTargetCandidateSemantics()
        {
            Assert(
                LiquidityTargetCandidateRule.IsDirectionallyValid(
                    1,
                    100,
                    101) &&
                !LiquidityTargetCandidateRule.IsDirectionallyValid(
                    1,
                    100,
                    99) &&
                LiquidityTargetCandidateRule.IsDirectionallyValid(
                    -1,
                    100,
                    99) &&
                !LiquidityTargetCandidateRule.IsDirectionallyValid(
                    -1,
                    100,
                    101),
                "liquidity candidates preserve BUY/SELL direction");

            Assert(
                LiquidityTargetCandidateRule.IsDistinct(
                    104,
                    new List<double> { 100 },
                    2,
                    0.40) &&
                !LiquidityTargetCandidateRule.IsDistinct(
                    100.5,
                    new List<double> { 100 },
                    2,
                    0.40),
                "liquidity candidates apply ATR-bounded minimum separation");

            List<double> buyLevels =
                LiquidityTargetCandidateRule.OrderByDistance(
                    1,
                    100,
                    new List<double> { 108, 102, 105 });

            List<double> sellLevels =
                LiquidityTargetCandidateRule.OrderByDistance(
                    -1,
                    100,
                    new List<double> { 92, 98, 95 });

            Assert(
                buyLevels[0] == 102 &&
                buyLevels[1] == 105 &&
                buyLevels[2] == 108 &&
                sellLevels[0] == 98 &&
                sellLevels[1] == 95 &&
                sellLevels[2] == 92,
                "liquidity candidates are ordered by distance from entry");

            List<double> distinctBuyLevels =
                LiquidityTargetCandidateRule.OrderDistinctByDistance(
                    1,
                    100,
                    new List<double> { 102, 102.4, 105 },
                    2,
                    0.40);

            List<double> distinctSellLevels =
                LiquidityTargetCandidateRule.OrderDistinctByDistance(
                    -1,
                    100,
                    new List<double> { 98, 97.6, 95 },
                    2,
                    0.40);

            Assert(
                distinctBuyLevels.Count == 2 &&
                distinctBuyLevels[0] == 102 &&
                distinctBuyLevels[1] == 105 &&
                distinctSellLevels.Count == 2 &&
                distinctSellLevels[0] == 98 &&
                distinctSellLevels[1] == 95,
                "liquidity candidates are ordered by distance and deduplicated by ATR spacing");

            Assert(
                LiquiditySweepRule.IsActiveUnbrokenLevel(
                    -1,
                    1,
                    5,
                    105,
                    0.01,
                    i => new[] { 100.0, 103.0, 104.0, 104.5, 104.0, 103.0 }[i]),
                "high liquidity remains active while no later close breaks above it");

            Assert(
                !LiquiditySweepRule.IsActiveUnbrokenLevel(
                    -1,
                    1,
                    5,
                    105,
                    0.01,
                    i => new[] { 100.0, 103.0, 106.0, 104.5, 104.0, 103.0 }[i]),
                "high liquidity becomes invalid after a close breaks above it");

            Assert(
                LiquiditySweepRule.IsActiveUnbrokenLevel(
                    1,
                    1,
                    5,
                    95,
                    0.01,
                    i => new[] { 100.0, 97.0, 96.0, 95.5, 96.0, 97.0 }[i]),
                "low liquidity remains active while no later close breaks below it");

            Assert(
                !LiquiditySweepRule.IsActiveUnbrokenLevel(
                    1,
                    1,
                    5,
                    95,
                    0.01,
                    i => new[] { 100.0, 97.0, 94.0, 95.5, 96.0, 97.0 }[i]),
                "low liquidity becomes invalid after a close breaks below it");

            PlanRewardRiskQualityResult validReward =
                PlanRewardRiskQualityRule.Evaluate(
                    1,
                    100,
                    99,
                    104,
                    2,
                    0,
                    2,
                    0.75,
                    1.80,
                    12.0,
                    0.01);

            PlanRewardRiskQualityResult invalidReward =
                PlanRewardRiskQualityRule.Evaluate(
                    1,
                    100,
                    99,
                    101,
                    2,
                    0,
                    2,
                    0.75,
                    1.80,
                    12.0,
                    0.01);

            Assert(
                validReward.Allowed &&
                validReward.NominalRR == 4,
                "valid liquidity target survives the canonical reward-risk gate");

            Assert(
                !invalidReward.Allowed &&
                invalidReward.Reason ==
                    "REWARD TOO LOW FOR STOP • RR 1.00 < 2.00",
                "insufficient liquidity target reward is rejected by the existing RR contract");

            Assert(
                TargetRewardEnvelopeRule.CanReachStage(
                    4,
                    1,
                    2,
                    4,
                    12) &&
                !TargetRewardEnvelopeRule.CanReachStage(
                    5,
                    2,
                    2,
                    4,
                    12),
                "liquidity target stages remain bounded by the existing reward envelope");
        }

        private static void VerifyFrameScoringConstants()
        {
            Assert(
                FrameScoringConstants.StructureContribution == 16 &&
                FrameScoringConstants.MssContribution == 12 &&
                FrameScoringConstants.ChochContribution == 9 &&
                FrameScoringConstants.DisplacementContribution == 10 &&
                FrameScoringConstants.LiquidityContribution == 10 &&
                FrameScoringConstants.EqualLevelContribution == 5,
                "frame-scoring event contributions preserve the established values");

            Assert(
                FrameScoringConstants.DirectionMinimumScore == 35 &&
                FrameScoringConstants.DirectionMinimumLead == 8 &&
                FrameScoringConstants.ConflictPenaltyThreshold == 45 &&
                FrameScoringConstants.ConflictPenaltyBaseline == 40 &&
                FrameScoringConstants.ConflictPenaltyCap == 6 &&
                FrameScoringConstants.ConflictPenaltyDivisor == 10,
                "frame-scoring direction and conflict constants preserve the established values");

            Assert(
                Math.Abs(FrameScoringConstants.RsiBullExhaustionThreshold - 75.0) < 1e-12 &&
                Math.Abs(FrameScoringConstants.RsiBearExhaustionThreshold - 25.0) < 1e-12 &&
                FrameScoringConstants.RsiExhaustionPenalty == 5,
                "RSI exhaustion constants preserve the established values");

            Assert(
                Math.Abs(FrameScoringConstants.ChoppyRegimeBase - 8.0) < 1e-12 &&
                Math.Abs(FrameScoringConstants.ChoppyRegimeSlope - 0.30) < 1e-12 &&
                Math.Abs(FrameScoringConstants.NonChoppyRegimeCap - 14.0) < 1e-12 &&
                Math.Abs(FrameScoringConstants.NonChoppyAdxSlope - 0.35) < 1e-12 &&
                Math.Abs(FrameScoringConstants.RangeEfficiencyWeight - 8.0) < 1e-12 &&
                Math.Abs(FrameScoringConstants.EmaSpreadCap - 4.0) < 1e-12 &&
                Math.Abs(FrameScoringConstants.EmaSpreadWeight - 2.0) < 1e-12,
                "regime contribution constants preserve the established values");

            Assert(
                Math.Abs(FrameScoringConstants.StrongestQualityWeight - 0.40) < 1e-12 &&
                Math.Abs(FrameScoringConstants.AdxQualityScale - 1.45) < 1e-12 &&
                Math.Abs(FrameScoringConstants.AdxQualityWeight - 0.13) < 1e-12 &&
                Math.Abs(FrameScoringConstants.EvidenceQualityScale - 5.0) < 1e-12 &&
                Math.Abs(FrameScoringConstants.EvidenceQualityCap - 100.0) < 1e-12 &&
                Math.Abs(FrameScoringConstants.EvidenceQualityWeight - 0.20) < 1e-12 &&
                Math.Abs(FrameScoringConstants.RegimeQualityWeight - 0.15) < 1e-12 &&
                Math.Abs(FrameScoringConstants.IndicatorQualityWeight - 0.12) < 1e-12 &&
                FrameScoringConstants.QualityConflictPenaltyCap == 10 &&
                FrameScoringConstants.QualityConflictPenaltyBaseline == 35 &&
                FrameScoringConstants.QualityConflictPenaltyDivisor == 6 &&
                Math.Abs(FrameScoringConstants.DirectionalTotalMinimum - 1.0) < 1e-12 &&
                Math.Abs(FrameScoringConstants.PercentageScale - 100.0) < 1e-12 &&
                FrameScoringConstants.QualityMinimum == 0 &&
                FrameScoringConstants.QualityMaximum == 100,
                "frame quality composition constants preserve the established values");
        }


        private static void VerifyOssIndicatorParameters()
        {
            if (OssIndicatorParameters.RollingQuoteWindowSize != 161 ||
                OssIndicatorSettings.Default.MacdSignalPeriod != 9 ||
                OssIndicatorSettings.Default.BollingerPeriod != 20 ||
                Math.Abs(
                    OssIndicatorSettings.Default.BollingerStandardDeviations -
                    2.0) > 1e-12 ||
                OssIndicatorSettings.Default.MfiPeriod != 14 ||
                OssIndicatorSettings.Default.StochLookbackPeriod != 14 ||
                OssIndicatorSettings.Default.StochSignalPeriod != 3 ||
                OssIndicatorSettings.Default.StochSmoothPeriod != 3 ||
                OssIndicatorSettings.Default.SuperTrendPeriod != 10 ||
                Math.Abs(
                    OssIndicatorSettings.Default.SuperTrendMultiplier -
                    3.0) > 1e-12 ||
                OssIndicatorSettings.Default.AroonPeriod != 25 ||
                OssIndicatorSettings.Default.CciPeriod != 20 ||
                Math.Abs(
                    OssIndicatorSettings.Default.ParabolicSarAccelerationFactor -
                    0.02) > 1e-12 ||
                Math.Abs(
                    OssIndicatorSettings.Default.ParabolicSarMaximumAccelerationFactor -
                    0.20) > 1e-12 ||
                OssIndicatorParameters.RsiMinimumHistory != 20 ||
                OssIndicatorParameters.BollingerMinimumHistory != 40 ||
                OssIndicatorParameters.MfiMinimumHistory != 40 ||
                OssIndicatorParameters.StochMinimumHistory != 40 ||
                OssIndicatorParameters.SuperTrendMinimumHistory != 60 ||
                OssIndicatorParameters.AroonMinimumHistory != 40 ||
                OssIndicatorParameters.CciMinimumHistory != 40 ||
                OssIndicatorParameters.ObvMinimumHistory != 3 ||
                OssIndicatorParameters.ParabolicSarMinimumHistory != 60 ||
                OssIndicatorParameters.MacdWarmupMargin != 20)
            {
                throw new InvalidOperationException(
                    "OSS indicator constants or warm-up contracts changed unexpectedly.");
            }

            if (OssIndicatorParameters.SafeRsiPeriod(1) != 2 ||
                OssIndicatorParameters.SafeMacdFastPeriod(1) != 2 ||
                OssIndicatorParameters.SafeMacdSlowPeriod(2, 2) != 3 ||
                OssIndicatorParameters.RsiHistoryRequired(50) != 55 ||
                OssIndicatorParameters.MacdHistoryRequired(12, 26) != 46)
            {
                throw new InvalidOperationException(
                    "OSS configured-period safety semantics changed unexpectedly.");
            }
        }


        private static void VerifyFrameRegimeSemantics()
        {
            MarketRegimeSnapshot bullTrend =
                new MarketRegimeSnapshot
                {
                    Regime = "TREND",
                    Direction = 1
                };

            MarketRegimeSnapshot bearTrend =
                new MarketRegimeSnapshot
                {
                    Regime = "TREND",
                    Direction = -1
                };

            if (FrameRegimeResolutionRule.ResolveFrameSnapshotRegime(bullTrend) != "TREND" ||
                FrameRegimeResolutionRule.ResolveFrameSnapshotRegime(bearTrend) != "TREND")
            {
                throw new InvalidOperationException(
                    "Known timeframe regime resolution must not depend on trade direction.");
            }

            if (FrameRegimeResolutionRule.NormalizeFrameRegimeValue(null) !=
                    FrameRegimeResolutionRule.Unknown ||
                FrameRegimeResolutionRule.NormalizeFrameRegimeValue("unknown") !=
                    FrameRegimeResolutionRule.Unknown ||
                !FrameRegimeResolutionRule.IsNeutral(null) ||
                !FrameRegimeResolutionRule.IsNeutral("bad-regime") ||
                FrameRegimeResolutionRule.IsNeutral("RANGE"))
            {
                throw new InvalidOperationException(
                    "UNKNOWN regime semantics are not neutral or normalization is unstable.");
            }

            IndicatorEvidenceFusionInput baseInput =
                new IndicatorEvidenceFusionInput(
                    "TREND",
                    true,
                    false,
                    true,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    4,
                    24,
                    16,
                    60,
                    0.8,
                    0.30,
                    1,
                    70,
                            58,
                    0,
                    0,
                    false,
                    false,
                    false,
                    false,
                    3,
                    1,
                    4,
                    4,
                    3);

            IndicatorEvidenceFusionResult bullResult =
                IndicatorEvidenceFusionRule.Evaluate(
                    baseInput);

            IndicatorEvidenceFusionInput mirrorInput =
                new IndicatorEvidenceFusionInput(
                    "TREND",
                    false,
                    true,
                    false,
                    true,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    4,
                    24,
                    16,
                    40,
                    -0.8,
                    -0.30,
                    -1,
                    70,
                            58,
                    0,
                    0,
                    false,
                    false,
                    false,
                    false,
                    1,
                    3,
                    4,
                    4,
                    3);

            IndicatorEvidenceFusionResult bearResult =
                IndicatorEvidenceFusionRule.Evaluate(
                    mirrorInput);

            if (bullResult.BullBonus != bearResult.BearBonus ||
                bullResult.BearBonus != bearResult.BullBonus ||
                bullResult.Conflict != bearResult.Conflict ||
                bullResult.Quality != bearResult.Quality)
            {
                throw new InvalidOperationException(
                    "BUY/SELL regime-weighted fusion is not symmetric.");
            }

            IndicatorEvidenceFusionResult unknownResult =
                IndicatorEvidenceFusionRule.Evaluate(
                    new IndicatorEvidenceFusionInput(
                        null,
                        true,
                        false,
                        true,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        4,
                        24,
                        16,
                        60,
                        0.8,
                        0.30,
                        1,
                        70,
                            58,
                        0,
                        0,
                        false,
                        false,
                        false,
                        false,
                        3,
                        1,
                        4,
                        4,
                        3));

            IndicatorEvidenceFusionResult explicitNeutralResult =
                IndicatorEvidenceFusionRule.Evaluate(
                    new IndicatorEvidenceFusionInput(
                        "UNKNOWN",
                        true,
                        false,
                        true,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        4,
                        24,
                        16,
                        60,
                        0.8,
                        0.30,
                        1,
                        70,
                            58,
                        0,
                        0,
                        false,
                        false,
                        false,
                        false,
                        3,
                        1,
                        4,
                        4,
                        3));

            if (unknownResult.BullBonus != explicitNeutralResult.BullBonus ||
                unknownResult.BearBonus != explicitNeutralResult.BearBonus ||
                unknownResult.Conflict != explicitNeutralResult.Conflict ||
                unknownResult.Quality != explicitNeutralResult.Quality)
            {
                throw new InvalidOperationException(
                    "UNKNOWN must remain explicitly neutral in regime weighting.");
            }
        }

        private static void VerifyCi03IndicatorEvidenceIndependence()
        {
            IndicatorEvidenceFusionInput Build(
                bool trend = false, bool momentum = false, bool macd = false,
                bool vwap = false, bool volume = false, bool volatility = false,
                bool useMacd = false, bool useVwap = false, bool useVolume = false,
                bool useHealthyVolatility = false, double adx = 0,
                double adxMinimum = 20, double rsi = 50, double dmiBias = 0,
                double emaSlopeAtr = 0, int waveTrendDirection = 0,
                int waveTrendQuality = 0, int divergenceDirection = 0,
                int divergenceQuality = 0, int ossBullVotes = 0,
                int ossIndicatorCount = 0)
            {
                return new IndicatorEvidenceFusionInput(
                    "UNKNOWN", trend, false, momentum, false, macd, false,
                    vwap, false, volume, false, volatility, false,
                    useVolume, useMacd, useVwap, useHealthyVolatility, 4,
                    adx, adxMinimum, rsi, dmiBias, emaSlopeAtr,
                    waveTrendDirection, waveTrendQuality,
                            58,
                    divergenceDirection, divergenceQuality,
                    false, false, false, false,
                    ossBullVotes, 0, ossIndicatorCount, 4, 3);
            }

            Assert(
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(
                    Build(trend: true, momentum: true, macd: true, vwap: true,
                          volume: true, volatility: true, useMacd: true,
                          useVwap: true, useVolume: true,
                          useHealthyVolatility: true, adx: 30, rsi: 60,
                          dmiBias: 1, emaSlopeAtr: 0.20,
                          waveTrendDirection: 1, waveTrendQuality: 80)) == 3,
                "CI-03 correlated trend/momentum/context measurements collapse to three indicator groups");

            Assert(
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(Build(macd: true)) == 0 &&
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(Build(macd: true, useMacd: true)) == 1,
                "CI-03 disabled/enabled MACD affects only its momentum-group presence");

            Assert(
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(Build(vwap: true)) == 0 &&
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(Build(vwap: true, useVwap: true)) == 1,
                "CI-03 disabled/enabled VWAP affects only its context-group presence");

            Assert(
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(Build(volume: true)) == 0 &&
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(Build(volume: true, useVolume: true)) == 1,
                "CI-03 disabled/enabled volume affects only its context-group presence");

            Assert(
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(Build(volatility: true)) == 0 &&
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(Build(volatility: true, useHealthyVolatility: true)) == 1,
                "CI-03 disabled/enabled healthy-volatility affects only its context-group presence");

            Assert(
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(
                    Build(divergenceDirection: 1, divergenceQuality: 90,
                          ossBullVotes: 4, ossIndicatorCount: 4)) == 0,
                "CI-03 divergence and aggregate OSS measurements remain non-independent");

            IndicatorEvidenceFusionResult disabledMacd =
                IndicatorEvidenceFusionRule.Evaluate(Build(macd: true));
            IndicatorEvidenceFusionResult enabledMacd =
                IndicatorEvidenceFusionRule.Evaluate(Build(macd: true, useMacd: true));
            Assert(
                disabledMacd.BullBonus == 0 && disabledMacd.BearBonus == 0 &&
                enabledMacd.BullBonus > 0,
                "CI-03 disabling MACD removes only MACD directional contribution");

            IndicatorEvidenceFusionResult disabledVwap =
                IndicatorEvidenceFusionRule.Evaluate(Build(vwap: true));
            IndicatorEvidenceFusionResult enabledVwap =
                IndicatorEvidenceFusionRule.Evaluate(Build(vwap: true, useVwap: true));
            Assert(
                disabledVwap.BullBonus == 0 && disabledVwap.BearBonus == 0 &&
                enabledVwap.BullBonus > 0,
                "CI-03 disabling VWAP removes only VWAP directional contribution");

            IndicatorEvidenceFusionResult disabledVolume =
                IndicatorEvidenceFusionRule.Evaluate(Build(volume: true));
            IndicatorEvidenceFusionResult enabledVolume =
                IndicatorEvidenceFusionRule.Evaluate(Build(volume: true, useVolume: true));
            Assert(
                disabledVolume.BullBonus == 0 && disabledVolume.BearBonus == 0 &&
                enabledVolume.BullBonus > 0,
                "CI-03 disabling volume removes only volume directional contribution");

            IndicatorEvidenceFusionResult disabledVolatility =
                IndicatorEvidenceFusionRule.Evaluate(Build(volatility: true));
            IndicatorEvidenceFusionResult enabledVolatility =
                IndicatorEvidenceFusionRule.Evaluate(Build(volatility: true, useHealthyVolatility: true));
            Assert(
                disabledVolatility.BullBonus == 0 && disabledVolatility.BearBonus == 0 &&
                enabledVolatility.BullBonus > 0,
                "CI-03 disabling healthy volatility removes only volatility directional contribution");
        }

        private static void VerifyOutcomeMemoryIdentitySemantics()
        {
            List<OutcomeMemoryParameter> parameters =
                new List<OutcomeMemoryParameter>
                {
                    new OutcomeMemoryParameter(
                        "PanelWidth",
                        "14 · DISPLAY — CORE",
                        "430"),
                    new OutcomeMemoryParameter(
                        "EnableSoundAlerts",
                        "12 · ALERTS — CORE",
                        "true"),
                    new OutcomeMemoryParameter(
                        "Tp1MinimumRR",
                        "09 · Risk & Targets",
                        "2.00"),
                    new OutcomeMemoryParameter(
                        "MinimumConfidence",
                        "01 · Decision",
                        "72")
                };

            string baseFingerprint =
                OutcomeMemoryIdentityRule.BuildFingerprint(
                    parameters,
                    true);

            parameters[0] =
                new OutcomeMemoryParameter(
                    "PanelWidth",
                    "14 · DISPLAY — CORE",
                    "700");

            parameters[1] =
                new OutcomeMemoryParameter(
                    "EnableSoundAlerts",
                    "12 · ALERTS — CORE",
                    "false");

            string presentationChangedFingerprint =
                OutcomeMemoryIdentityRule.BuildFingerprint(
                    parameters,
                    true);

            Assert(
                string.Equals(
                    baseFingerprint,
                    presentationChangedFingerprint,
                    StringComparison.Ordinal),
                "presentation-only parameter changes do not change learning fingerprint");

            parameters[2] =
                new OutcomeMemoryParameter(
                    "Tp1MinimumRR",
                    "09 · Risk & Targets",
                    "2.10");

            string decisionChangedFingerprint =
                OutcomeMemoryIdentityRule.BuildFingerprint(
                    parameters,
                    true);

            Assert(
                !string.Equals(
                    baseFingerprint,
                    decisionChangedFingerprint,
                    StringComparison.Ordinal),
                "decision-affecting Tp1MinimumRR changes learning fingerprint");

            string legacyFingerprint =
                OutcomeMemoryIdentityRule.BuildFingerprint(
                    parameters,
                    false);

            Assert(
                !string.Equals(
                    legacyFingerprint,
                    decisionChangedFingerprint,
                    StringComparison.Ordinal),
                "legacy all-parameter fingerprint remains distinguishable for migration");

            string accountA =
                OutcomeMemoryIdentityRule.BuildAccountScopeToken(
                    "Broker-A",
                    123456,
                    "Hedged",
                    true);

            string accountB =
                OutcomeMemoryIdentityRule.BuildAccountScopeToken(
                    "Broker-A",
                    123457,
                    "Hedged",
                    true);

            string accountC =
                OutcomeMemoryIdentityRule.BuildAccountScopeToken(
                    "Broker-A",
                    123456,
                    "Netted",
                    true);

            string accountD =
                OutcomeMemoryIdentityRule.BuildAccountScopeToken(
                    "Broker-A",
                    123456,
                    "Hedged",
                    false);

            Assert(
                !string.Equals(accountA, accountB, StringComparison.Ordinal) &&
                !string.Equals(accountA, accountC, StringComparison.Ordinal) &&
                !string.Equals(accountA, accountD, StringComparison.Ordinal),
                "account number, account type and live/demo state are isolated in the account scope");

            string key =
                OutcomeMemoryIdentityRule.BuildMemoryKey(
                    "XAUUSD",
                    "Minute5",
                    accountA,
                    decisionChangedFingerprint);

            bool keyIsAlphaNumeric = true;
            for (int i = 0; i < key.Length; i++)
            {
                if (!char.IsLetterOrDigit(key[i]))
                {
                    keyIsAlphaNumeric = false;
                    break;
                }
            }

            Assert(
                key.Length <= 50 &&
                keyIsAlphaNumeric &&
                key.StartsWith(
                    OutcomeMemoryIdentityRule.CurrentKeyPrefix,
                    StringComparison.Ordinal),
                "scoped LocalStorage key stays within cTrader key constraints");
        }


        private static void VerifySignalTraceLineageSemantics()
        {
            long barOpenTicks = 638949600000000000;

            string first =
                SignalTraceIdentityRule.CreateTraceId(
                    "XAUUSD",
                    "Minute5",
                    "BROKER-123456-HEDGED-LIVE",
                    "ABCDEF",
                    barOpenTicks);

            string same =
                SignalTraceIdentityRule.CreateTraceId(
                    "XAUUSD",
                    "Minute5",
                    "BROKER-123456-HEDGED-LIVE",
                    "ABCDEF",
                    barOpenTicks);

            Assert(
                !string.IsNullOrWhiteSpace(first) &&
                string.Equals(first, same, StringComparison.Ordinal),
                "signal trace identity is deterministic for the same scope");

            Assert(
                first != SignalTraceIdentityRule.CreateTraceId(
                    "XAUUSD",
                    "Minute5",
                    "BROKER-123456-HEDGED-LIVE",
                    "ABCDEF",
                    barOpenTicks + 1) &&
                first != SignalTraceIdentityRule.CreateTraceId(
                    "XAUUSD",
                    "Minute5",
                    "BROKER-123457-HEDGED-LIVE",
                    "ABCDEF",
                    barOpenTicks) &&
                first != SignalTraceIdentityRule.CreateTraceId(
                    "XAUUSD",
                    "Minute5",
                    "BROKER-123456-HEDGED-LIVE",
                    "ABCDEF2",
                    barOpenTicks) &&
                first != SignalTraceIdentityRule.CreateTraceId(
                    "EURUSD",
                    "Minute5",
                    "BROKER-123456-HEDGED-LIVE",
                    "ABCDEF",
                    barOpenTicks),
                "trace identity isolates bar, account, configuration and symbol");

            Assert(
                string.IsNullOrEmpty(
                    SignalTraceIdentityRule.CreateTraceId(
                        "XAUUSD",
                        "Minute5",
                        "BROKER",
                        "CONFIG",
                        0)),
                "invalid trace bar fails closed");

            Assert(
                SignalTraceLineageRule.MatchesClosedBar(
                    25, barOpenTicks, 25, barOpenTicks) &&
                !SignalTraceLineageRule.MatchesClosedBar(
                    25, barOpenTicks, 26, barOpenTicks) &&
                !SignalTraceLineageRule.MatchesClosedBar(
                    25, barOpenTicks, 25, barOpenTicks + 1),
                "trace geometry requires the exact closed-M5 lineage");

            Assert(
                SignalTraceLineageRule.CanJoinOutcome(first, first) &&
                !SignalTraceLineageRule.CanJoinOutcome(first, first + "|other") &&
                !SignalTraceLineageRule.CanJoinOutcome("", first),
                "research outcome join requires exact non-empty trace identity");
        }

        private static void VerifyPersistenceHealthSemantics()
        {
            Assert(
                PersistenceHealthRule.IsRelativeHistoryPath(
                    "History\\CFIP_Runtime.csv") &&
                PersistenceHealthRule.IsRelativeHistoryPath(
                    "History/sub/file.csv") &&
                !PersistenceHealthRule.IsRelativeHistoryPath(
                    "C:\\Users\\User\\History\\file.csv") &&
                !PersistenceHealthRule.IsRelativeHistoryPath(
                    "../History/file.csv") &&
                !PersistenceHealthRule.IsRelativeHistoryPath(
                    "/History/file.csv"),
                "History persistence paths remain sandbox-relative and reject rooted or parent-escape paths");

            Assert(
                PersistenceHealthRule.Resolve(
                    true,
                    0,
                    0,
                    0) ==
                PersistenceHealthState.Healthy,
                "successful History probe with no I/O failures is healthy");

            Assert(
                PersistenceHealthRule.Resolve(
                    true,
                    1,
                    0,
                    0) ==
                PersistenceHealthState.Degraded &&
                PersistenceHealthRule.Resolve(
                    true,
                    0,
                    1,
                    0) ==
                PersistenceHealthState.Degraded,
                "read or write failures downgrade persistence health");

            Assert(
                PersistenceHealthRule.Resolve(
                    true,
                    0,
                    0,
                    3) ==
                PersistenceHealthState.Healthy,
                "pending buffered lines do not falsely report persistence failure after a successful probe");

            Assert(
                PersistenceHealthRule.Resolve(
                    false,
                    0,
                    0,
                    0) ==
                PersistenceHealthState.Unknown,
                "unverified History access is distinguishable from confirmed failure");
        }

        private static void VerifySwingPlateauSemantics()
        {
            double[] highs = { 1, 2, 5, 5, 5, 3, 2, 4 };
            int start, end;
            double level;
            Assert(
                SwingPlateauRule.TryGetHighPlateau(highs.Length, 2, 1, 6, 0.001, i => highs[i], out start, out end, out level) &&
                start == 2 && end == 4 && level == 5,
                "flat swing-high plateau resolves once at its leftmost canonical bar");
            Assert(
                !SwingPlateauRule.TryGetHighPlateau(highs.Length, 3, 1, 6, 0.001, i => highs[i], out start, out end, out level),
                "interior plateau bars cannot create duplicate swing identities");
            Assert(
                !SwingPlateauRule.TryGetHighPlateau(highs.Length, 2, 1, 4, 0.001, i => highs[i], out start, out end, out level),
                "unconfirmed plateau cannot use bars beyond closed index");

            double[] lows = { 9, 8, 5, 5, 5, 7, 8, 6 };
            Assert(
                SwingPlateauRule.TryGetLowPlateau(lows.Length, 2, 1, 6, 0.001, i => lows[i], out start, out end, out level) &&
                start == 2 && end == 4 && level == 5,
                "flat swing-low plateau is directionally symmetric");

            Assert(
                SwingPlateauRule.IsWithinAnchor(100, 100.08, 0.1) &&
                !SwingPlateauRule.IsWithinAnchor(100, 100.19, 0.1),
                "equal-level membership is bounded by fixed anchor, not chained neighbors");
            Assert(
                SwingPlateauRule.IsWithinAnchor(100, 100.08, 0.1) &&
                SwingPlateauRule.IsWithinAnchor(100.08, 100.16, 0.1) &&
                !SwingPlateauRule.IsWithinAnchor(100, 100.16, 0.1),
                "transitive tolerance chain cannot manufacture one equal-level cluster");
            Assert(
                SwingPlateauRule.BreakIdentity(1, 12, 20) ==
                SwingPlateauRule.BreakIdentity(1, 12, 20) &&
                SwingPlateauRule.BreakIdentity(1, 12, 20) !=
                SwingPlateauRule.BreakIdentity(-1, 12, 20) &&
                SwingPlateauRule.BreakIdentity(1, 12, 20) !=
                SwingPlateauRule.BreakIdentity(1, 12, 21),
                "structural break identity distinguishes direction and closed-bar event");

            Assert(
                StructuralEvidenceRule.CanonicalEventCount(true, true, true) == 1 &&
                StructuralEvidenceRule.CanonicalEventCount(false, true, true) == 1 &&
                StructuralEvidenceRule.CanonicalEventCount(false, false, false) == 0,
                "structure, MSS and CHOCH collapse to one canonical event");
            Assert(
                !StructuralEvidenceRule.IsIndependentTransition(true, true, false) &&
                StructuralEvidenceRule.IsIndependentTransition(false, true, false) &&
                StructuralEvidenceRule.IsIndependentTransition(false, false, true),
                "transition is independent only when structure is absent");
        }

        private static void VerifyLiveInvalidationSemantics()
        {
            Assert(
                LiveInvalidationRule.ShouldEvaluateClosedBar(20, -1) &&
                !LiveInvalidationRule.ShouldEvaluateClosedBar(20, 20) &&
                LiveInvalidationRule.ShouldEvaluateClosedBar(21, 20),
                "live invalidation evaluates at most once per canonical closed M5 bar");

            double move;
            Assert(
                LiveInvalidationRule.TryCalculateDirectionalMove(
                    1,
                    100,
                    99.25,
                    out move) &&
                Math.Abs(move + 0.75) < 1e-12,
                "BUY adverse move is directional and deterministic");

            Assert(
                LiveInvalidationRule.TryCalculateDirectionalMove(
                    -1,
                    100,
                    100.75,
                    out move) &&
                Math.Abs(move + 0.75) < 1e-12,
                "SELL adverse move is directionally symmetric");

            Assert(
                !LiveInvalidationRule.TryCalculateDirectionalMove(
                    0,
                    100,
                    99,
                    out move) &&
                !LiveInvalidationRule.TryCalculateDirectionalMove(
                    1,
                    0,
                    99,
                    out move) &&
                !LiveInvalidationRule.TryCalculateDirectionalMove(
                    -1,
                    100,
                    double.NaN,
                    out move),
                "live invalidation rejects invalid direction, price and non-finite input");

            Assert(
                LiveInvalidationRule.RecordExitM5(
                    5,
                    10,
                    true) == 10 &&
                LiveInvalidationRule.RecordExitM5(
                    5,
                    10,
                    false) == 5 &&
                LiveInvalidationRule.RecordExitM5(
                    5,
                    -1,
                    true) == 5,
                "rejected or invalid exits never advance successful-exit bookkeeping");
        }

        private static void VerifyFalseSignalAdverseRSemantics()
        {
            double soft;
            double hard;
            double stopR;

            Assert(
                FalseSignalAdverseRRule.TryResolveThresholds(
                    1,
                    100,
                    1,
                    1.10,
                    99,
                    out soft,
                    out hard,
                    out stopR) &&
                Math.Abs(soft - 1.0) < 1e-12 &&
                Math.Abs(hard - 1.0) < 1e-12 &&
                Math.Abs(stopR - 1.0) < 1e-12,
                "BUY false-signal thresholds cannot sit behind a one-R protective stop");

            Assert(
                FalseSignalAdverseRRule.TryResolveThresholds(
                    -1,
                    100,
                    2,
                    0.60,
                    101.5,
                    out soft,
                    out hard,
                    out stopR) &&
                Math.Abs(soft - 0.60) < 1e-12 &&
                Math.Abs(hard - 0.75) < 1e-12 &&
                Math.Abs(stopR - 0.75) < 1e-12,
                "SELL false-signal thresholds remain symmetric when protected risk is tighter");

            Assert(
                FalseSignalAdverseRRule.TryResolveThresholds(
                    1,
                    100,
                    1,
                    1.10,
                    100,
                    out soft,
                    out hard,
                    out stopR) &&
                Math.Abs(soft - 1.10) < 1e-12 &&
                Math.Abs(hard - 1.10) < 1e-12 &&
                stopR == 0,
                "break-even-or-better protection leaves configured adverse threshold meaningful but outside adverse-stop envelope");

            Assert(
                !FalseSignalAdverseRRule.TryResolveThresholds(
                    1,
                    100,
                    1,
                    double.NaN,
                    99,
                    out soft,
                    out hard,
                    out stopR) &&
                !FalseSignalAdverseRRule.TryResolveThresholds(
                    1,
                    100,
                    1,
                    5.10,
                    99,
                    out soft,
                    out hard,
                    out stopR),
                "false-signal adverse-R validation fails closed for non-finite and out-of-contract inputs");
        }

        private static void VerifySessionWindowSemantics()
        {
            Assert(
                SessionWindowRule.SessionResolutionMinutes == 60,
                "session parameters use explicit 60-minute UTC resolution");

            DateTime sameDay = Utc(12, 0);

            Assert(
                SessionWindowRule.IsInside(
                    sameDay,
                    6,
                    20),
                "same-day session accepts an in-session time");

            Assert(
                !SessionWindowRule.IsInside(
                    Utc(20, 0),
                    6,
                    20),
                "same-day session excludes its end boundary");

            Assert(
                SessionWindowRule.IsInside(
                    Utc(23, 0),
                    22,
                    6) &&
                SessionWindowRule.IsInside(
                    Utc(5, 59),
                    22,
                    6) &&
                !SessionWindowRule.IsInside(
                    Utc(6, 0),
                    22,
                    6),
                "overnight session is directionally correct at both boundaries");

            Assert(
                SessionWindowRule.IsInside(
                    Utc(5, 0),
                    6,
                    6),
                "start equals end represents a full-day session");

            DateTime sessionStart;
            DateTime sessionEnd;

            Assert(
                SessionWindowRule.TryResolveSessionWindow(
                    Utc(5, 0),
                    6,
                    20,
                    out sessionStart,
                    out sessionEnd) &&
                sessionStart == Utc(6, 0).AddDays(-1) &&
                sessionEnd == Utc(20, 0).AddDays(-1),
                "pre-session reference resolves the previous completed same-day window");

            Assert(
                SessionWindowRule.TryResolveSessionWindow(
                    Utc(3, 0),
                    22,
                    6,
                    out sessionStart,
                    out sessionEnd) &&
                sessionStart == Utc(22, 0).AddDays(-1) &&
                sessionEnd == Utc(6, 0),
                "overnight session range crosses midnight correctly");

            DateTime boundary;

            Assert(
                SessionWindowRule.TryResolveEndOfDayBoundary(
                    Utc(19, 45),
                    6,
                    20,
                    5,
                    out boundary) &&
                boundary == Utc(20, 0) &&
                SessionWindowRule.IsWithinPreBoundaryWindow(
                    Utc(19, 45),
                    boundary,
                    30),
                "same-day EOD pre-warning resolves the current session boundary");

            Assert(
                SessionWindowRule.IsWithinPostBoundaryWindow(
                    Utc(20, 3),
                    boundary,
                    5) &&
                !SessionWindowRule.IsWithinPostBoundaryWindow(
                    Utc(20, 6),
                    boundary,
                    5),
                "EOD cleanup is bounded to the five-minute post-boundary window");

            Assert(
                SessionWindowRule.TryResolveEndOfDayBoundary(
                    new DateTime(
                        2026, 1, 2, 5, 40, 0, DateTimeKind.Utc),
                    22,
                    6,
                    5,
                    out boundary) &&
                boundary ==
                    new DateTime(
                        2026, 1, 2, 6, 0, 0, DateTimeKind.Utc) &&
                SessionWindowRule.IsWithinPreBoundaryWindow(
                    new DateTime(
                        2026, 1, 2, 5, 40, 0, DateTimeKind.Utc),
                    boundary,
                    30),
                "overnight EOD warning uses the active session's next-day boundary");

            Assert(
                SessionWindowRule.TryResolveEndOfDayBoundary(
                    new DateTime(
                        2026, 1, 2, 6, 3, 0, DateTimeKind.Utc),
                    22,
                    6,
                    5,
                    out boundary) &&
                SessionWindowRule.IsWithinPostBoundaryWindow(
                    new DateTime(
                        2026, 1, 2, 6, 3, 0, DateTimeKind.Utc),
                    boundary,
                    5),
                "overnight EOD cleanup recognizes the completed session immediately after its boundary");

            int[] openMinutes = { 0, 60, 120, 180, 240 };

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    openMinutes.Length,
                    Utc(3, 0),
                    index =>
                        Utc(
                            openMinutes[index] / 60,
                            openMinutes[index] % 60)) == 2,
                "closed-bar reference resolves the latest fully closed period without a second-period offset");

            Assert(
                ClosedBarReferenceRule.IsFullyClosed(
                    openMinutes.Length,
                    2,
                    Utc(3, 0),
                    index =>
                        Utc(
                            openMinutes[index] / 60,
                            openMinutes[index] % 60)),
                "latest closed period is explicitly fully closed at its next open");

            Assert(
                !ClosedBarReferenceRule.IsFullyClosed(
                    openMinutes.Length,
                    3,
                    Utc(3, 0),
                    index =>
                        Utc(
                            openMinutes[index] / 60,
                            openMinutes[index] % 60)),
                "the current open period cannot be consumed as a closed reference");
        }

        private static void VerifyFvgMathematics()
        {
            double low;
            double high;
            double gap;

            Assert(
                FvgRule.TryGetThreeBarGap(
                    1, 100, 98, 102, 101,
                    out low, out high, out gap) &&
                low == 100 && high == 101 && gap == 1,
                "bullish 3-bar FVG uses older high to current low");

            Assert(
                !FvgRule.TryGetThreeBarGap(
                    1, 100, 98, 101, 100,
                    out low, out high, out gap) &&
                !FvgRule.TryGetThreeBarGap(
                    -1, 102, 100, 100, 99,
                    out low, out high, out gap),
                "FVG equality/touch is not a gap");

            Assert(
                FvgRule.TryGetThreeBarGap(
                    -1, 105, 104, 103, 102,
                    out low, out high, out gap) &&
                low == 103 && high == 104 && gap == 1,
                "bearish 3-bar FVG is directionally symmetric");

            Assert(
                FvgRule.TryGetTwoBarGap(
                    1, 100, 98, 102, 101,
                    out low, out high, out gap) &&
                low == 100 && high == 101 && gap == 1 &&
                FvgRule.TryGetTwoBarGap(
                    -1, 105, 104, 103, 102,
                    out low, out high, out gap) &&
                low == 103 && high == 104 && gap == 1,
                "two-bar imbalance geometry is explicit and symmetric");

            Assert(
                FvgRule.MeetsMinimumGap(0.80, 2.0, 0.30) &&
                !FvgRule.MeetsMinimumGap(0.80, 3.0, 0.30),
                "minimum FVG threshold is anchored to creation-bar ATR");

            Assert(
                FvgRule.IsOverlapInclusive(100, 101, 101, 102) &&
                !FvgRule.IsOverlapInclusive(100, 101, 101.01, 102) &&
                !FvgRule.IsOverlapInclusive(101, 100, 99, 102),
                "FVG overlap uses valid geometry and explicit boundary semantics");

            Assert(
                FvgRule.IsFullyFilled(1, 100, 101, 100) &&
                !FvgRule.IsFullyFilled(1, 100, 101, 100.01) &&
                FvgRule.IsFullyFilled(-1, 100, 101, 101) &&
                !FvgRule.IsFullyFilled(-1, 100, 101, 100.99),
                "bullish and bearish FVG full-fill boundaries are symmetric");

            Assert(
                FvgRule.TryApplyPartialMitigation(
                    1, 100, 101, 100.50, 0.01,
                    out low, out high) &&
                low == 100 && high == 100.50,
                "bullish partial FVG fill moves only the upper boundary");

            Assert(
                FvgRule.TryApplyPartialMitigation(
                    -1, 100, 101, 100.50, 0.01,
                    out low, out high) &&
                low == 100.50 && high == 101,
                "bearish partial FVG fill moves only the lower boundary");

            Assert(
                !FvgRule.TryApplyPartialMitigation(
                    1, 100, 101, 100, 0.01,
                    out low, out high) &&
                !FvgRule.TryApplyPartialMitigation(
                    -1, 100, 101, 101, 0.01,
                    out low, out high),
                "full fill cannot survive as an active partial FVG geometry");

            Assert(
                FvgRule.Identity(1, 42, false) ==
                FvgRule.Identity(1, 42, false) &&
                FvgRule.Identity(1, 42, false) !=
                FvgRule.Identity(1, 42, true) &&
                FvgRule.Identity(1, 42, false) !=
                FvgRule.Identity(-1, 42, false),
                "FVG identity separates direction and 3-bar/2-bar source variants");
        }

        private static void VerifyFvgLifecycleSemantics()
        {
            Assert(
                FvgLifecycleRule.IsAgeValid(
                    10,
                    10,
                    0) &&
                FvgLifecycleRule.IsAgeValid(
                    10,
                    50,
                    40) &&
                !FvgLifecycleRule.IsAgeValid(
                    10,
                    51,
                    40) &&
                !FvgLifecycleRule.IsAgeValid(
                    10,
                    9,
                    40),
                "FVG source age is bounded and never accepts a future current index");

            Assert(
                FvgLifecycleRule.ResolveFvgMitigationProbe(
                    1,
                    100.8,
                    100.4,
                    99.5,
                    101.0,
                    false) == 100.4 &&
                FvgLifecycleRule.ResolveFvgMitigationProbe(
                    -1,
                    100.4,
                    100.8,
                    99.5,
                    101.0,
                    false) == 100.8 &&
                FvgLifecycleRule.ResolveFvgMitigationProbe(
                    1,
                    100.8,
                    100.4,
                    99.5,
                    101.0,
                    true) == 99.5 &&
                FvgLifecycleRule.ResolveFvgMitigationProbe(
                    -1,
                    100.4,
                    100.8,
                    99.5,
                    101.0,
                    true) == 101.0,
                "FVG mitigation probe is directionally symmetric for body and wick policies");

            double low;
            double high;
            bool full;

            Assert(
                FvgLifecycleRule.TryApplyMitigationStep(
                    1,
                    100,
                    101,
                    100.50,
                    0.01,
                    true,
                    out low,
                    out high,
                    out full) &&
                !full &&
                low == 100 &&
                high == 100.50,
                "bullish FVG partial mitigation moves only the upper boundary");

            Assert(
                FvgLifecycleRule.TryApplyMitigationStep(
                    -1,
                    100,
                    101,
                    100.50,
                    0.01,
                    true,
                    out low,
                    out high,
                    out full) &&
                !full &&
                low == 100.50 &&
                high == 101,
                "bearish FVG partial mitigation moves only the lower boundary");

            Assert(
                !FvgLifecycleRule.TryApplyMitigationStep(
                    1,
                    100,
                    101,
                    99.99,
                    0.01,
                    true,
                    out low,
                    out high,
                    out full) &&
                full &&
                !FvgLifecycleRule.TryApplyMitigationStep(
                    -1,
                    100,
                    101,
                    101.01,
                    0.01,
                    true,
                    out low,
                    out high,
                    out full) &&
                full,
                "full FVG fill invalidates both directions when safety invalidation is enabled");

            Assert(
                FvgLifecycleRule.TryApplyMitigationStep(
                    1,
                    100,
                    101,
                    99.99,
                    0.01,
                    false,
                    out low,
                    out high,
                    out full) &&
                full &&
                low == 100 &&
                high == 101 &&
                FvgLifecycleRule.TryApplyMitigationStep(
                    -1,
                    100,
                    101,
                    101.01,
                    0.01,
                    false,
                    out low,
                    out high,
                    out full) &&
                full &&
                low == 100 &&
                high == 101,
                "disabled full-fill invalidation retains canonical source geometry");

            Assert(
                double.IsNaN(
                    FvgLifecycleRule.ResolveFvgMitigationProbe(
                        0,
                        100,
                        100,
                        99,
                        101,
                        false)),
                "invalid FVG mitigation direction fails closed");

            Console.WriteLine(
                "CI-05 FVG lifecycle semantics contracts PASS");
        }

        private static void VerifyFvgQualitySemantics()
        {
            int weak =
                FvgQualityRule.Calculate(
                    0.08, 0.20, 30, 40, 0.05,
                    false, false, false);

            int moderate =
                FvgQualityRule.Calculate(
                    0.40, 0.80, 20, 40, 0.80,
                    true, false, false);

            int strong =
                FvgQualityRule.Calculate(
                    0.80, 1.00, 0, 40, 1.50,
                    true, true, false);

            Assert(
                weak < 60 &&
                moderate > weak &&
                strong > moderate &&
                strong >= 90,
                "FVG quality separates weak, contextual and strong candidates");

            Assert(
                FvgQualityRule.Calculate(
                    0.40, 0.80, 20, 40, 0.80,
                    true, true, false) >
                FvgQualityRule.Calculate(
                    0.40, 0.80, 20, 40, 0.80,
                    false, false, false),
                "structural and higher-timeframe context increase quality");

            Assert(
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 1.0,
                    true, true, false) >
                FvgQualityRule.Calculate(
                    0.40, 1.0, 20, 40, 1.0,
                    true, true, false),
                "fresher FVGs receive higher quality");

            Assert(
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 1.0,
                    true, true, false) >
                FvgQualityRule.Calculate(
                    0.40, 0.50, 0, 40, 1.0,
                    true, true, false),
                "remaining unmitigated geometry increases quality");

            Assert(
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 1.5,
                    true, true, false) >
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 0.2,
                    true, true, false),
                "directional displacement increases quality");

            Assert(
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 1.0,
                    true, true, false) -
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 1.0,
                    true, true, true) ==
                3,
                "two-bar imbalance retains an explicit quality penalty");

            Assert(
                FvgQualityRule.Calculate(
                    double.NaN,
                    1.0,
                    0,
                    40,
                    1.0,
                    true,
                    true,
                    false) == 0,
                "non-finite FVG quality input fails closed");
        }
        private static void VerifyExecutionThresholdSemantics()
        {
            Assert(
                IndicatorExecutionQualityRule.AutomaticMarketQualityMinimum == 60 &&
                IndicatorExecutionQualityRule.AutomaticMarketConflictMaximum == 52,
                "automatic-market indicator thresholds retain current defaults");

            Assert(
                IndicatorExecutionQualityRule.PendingSetupQualityMinimum == 62 &&
                IndicatorExecutionQualityRule.PendingContinuationConflictMaximum == 48 &&
                IndicatorExecutionQualityRule.PendingReversalConflictMaximum == 50 &&
                IndicatorExecutionQualityRule.PendingSubmissionQualityMinimum == 58 &&
                IndicatorExecutionQualityRule.PendingSubmissionConflictMaximum == 55,
                "pending-path indicator thresholds are explicitly centralized");

            Assert(
                ExecutionThresholdPolicy.NormalizeDirectionShare(49) == 50 &&
                ExecutionThresholdPolicy.NormalizeDirectionShare(57) == 57 &&
                ExecutionThresholdPolicy.NormalizeDirectionShare(96) == 95,
                "direction-share bounds match the public parameter range");

            Assert(
                ExecutionThresholdPolicy.NormalizeReversalEvidence(1) == 2 &&
                ExecutionThresholdPolicy.NormalizeReversalEvidence(5) == 5 &&
                ExecutionThresholdPolicy.NormalizeReversalMtf(49) == 50 &&
                ExecutionThresholdPolicy.NormalizeReversalMtf(72) == 72,
                "reversal evidence and MTF defensive bounds are explicit");

            Assert(
                ExecutionThresholdPolicy.NormalizeEndOfDayAlertMinutesBefore(4) == 5 &&
                ExecutionThresholdPolicy.NormalizeEndOfDayAlertMinutesBefore(30) == 30,
                "EOD alert defensive bounds match parameter limits");

            Assert(
                Math.Abs(ExecutionThresholdPolicy.NormalizeMaximumSpreadToStopRiskRatio(0.01) - 0.02) < 0.0000001 &&
                Math.Abs(ExecutionThresholdPolicy.NormalizeMaximumSpreadToStopRiskRatio(0.18) - 0.18) < 0.0000001 &&
                Math.Abs(ExecutionThresholdPolicy.NormalizeMaximumSpreadToStopRiskRatio(0.60) - 0.50) < 0.0000001 &&
                Math.Abs(ExecutionThresholdPolicy.NormalizeMaximumSpreadToStopRiskRatio(double.NaN) - 0.02) < 0.0000001,
                "spread/stop ratio is finite and parameter-aligned");
        }

        private static void VerifyVolumeSizingSemantics()
        {
            Assert(
                VolumeSizingRule.IsValidStopPips(0.1) &&
                !VolumeSizingRule.IsValidStopPips(0) &&
                !VolumeSizingRule.IsValidStopPips(-1) &&
                !VolumeSizingRule.IsValidStopPips(double.NaN) &&
                !VolumeSizingRule.IsValidStopPips(double.PositiveInfinity),
                "volume sizing rejects zero, negative and non-finite stop risk");

            Assert(
                VolumeSizingRule.IsValidRiskInput(1000, 0.5) &&
                !VolumeSizingRule.IsValidRiskInput(0, 0.5) &&
                !VolumeSizingRule.IsValidRiskInput(1000, double.NaN),
                "risk sizing accepts only finite positive account inputs");

            Assert(
                Math.Abs(RiskAmountCalculator.Calculate(1000, 0.5) - 5.0) < 0.0000001 &&
                RiskAmountCalculator.Calculate(1000, 0) == 0 &&
                RiskAmountCalculator.Calculate(double.NaN, 0.5) == 0 &&
                RiskAmountCalculator.Calculate(1000, double.PositiveInfinity) == 0,
                "risk amount calculation is finite and fail-closed");

            Assert(
                VolumeSizingRule.IsValidNormalizedVolume(1000, 1000, 10000) &&
                !VolumeSizingRule.IsValidNormalizedVolume(999, 1000, 10000) &&
                !VolumeSizingRule.IsValidNormalizedVolume(10001, 1000, 10000) &&
                !VolumeSizingRule.IsValidNormalizedVolume(double.NaN, 1000, 10000),
                "normalized volume must remain inside broker bounds");

            Assert(
                Math.Abs(RiskPercentPolicy.Calculate(7, false, 1.0) - 5.0) < 0.0000001 &&
                Math.Abs(RiskPercentPolicy.Calculate(0.5, false, 1.0) - 0.5) < 0.0000001 &&
                Math.Abs(RiskPercentPolicy.Calculate(1.0, true, 0.5) - 0.5) < 0.0000001,
                "risk percent policy respects the five-percent parameter ceiling");
        }

        private static void VerifyDailyLossBaselineSemantics()
        {
            double startEquity;
            double baselineFloating;

            Assert(
                DailyLossBaselineRule.TryReconstruct(
                    975,
                    0,
                    -25,
                    0,
                    false,
                    out startEquity,
                    out baselineFloating) &&
                Math.Abs(startEquity - 1000) < 0.0001 &&
                Math.Abs(baselineFloating) < 0.0001,
                "midday restart reconstructs start-of-day equity from realized facts");

            Assert(
                DailyLossBaselineRule.TryReconstruct(
                    1480,
                    0,
                    -20,
                    500,
                    false,
                    out startEquity,
                    out baselineFloating) &&
                Math.Abs(startEquity - 1000) < 0.0001,
                "midday deposit is excluded from reconstructed trading loss");

            Assert(
                !DailyLossBaselineRule.TryReconstruct(
                    1000,
                    -12,
                    0,
                    0,
                    true,
                    out startEquity,
                    out baselineFloating),
                "restart baseline reconstruction refuses unknown prior floating P/L");

            Assert(
                !DailyLossBaselineRule.TryReconstruct(
                    0,
                    0,
                    0,
                    0,
                    false,
                    out startEquity,
                    out baselineFloating),
                "invalid equity cannot produce a synthetic baseline");
        }


        private static void VerifyNativeIndicatorReadinessSemantics()
        {
            Assert(
                !NativeIndicatorReadinessRule.IsIndexedSeriesReady(
                    13,
                    20,
                    14) &&
                NativeIndicatorReadinessRule.IsIndexedSeriesReady(
                    14,
                    20,
                    14),
                "native series readiness requires the configured warm-up boundary");

            Assert(
                !NativeIndicatorReadinessRule.IsIndexedSeriesReady(
                    14,
                    14,
                    14) &&
                !NativeIndicatorReadinessRule.IsIndexedSeriesReady(
                    -1,
                    20,
                    14),
                "native series readiness rejects missing and out-of-range samples");

            Assert(
                NativeIndicatorReadinessRule.IsIndexedWindowReady(
                    16,
                    2,
                    20,
                    14) &&
                !NativeIndicatorReadinessRule.IsIndexedWindowReady(
                    15,
                    2,
                    20,
                    14),
                "native indicator window readiness protects prior-sample comparisons");

            Assert(
                NativeIndicatorReadinessRule.IsFinitePositiveNative(1.0) &&
                !NativeIndicatorReadinessRule.IsFinitePositiveNative(0) &&
                !NativeIndicatorReadinessRule.IsFinitePositiveNative(double.NaN) &&
                !NativeIndicatorReadinessRule.IsFinitePositiveNative(
                    double.PositiveInfinity),
                "positive volatility and moving-average inputs reject zero/non-finite values");

            Assert(
                NativeIndicatorReadinessRule.IsFiniteBounded(50, 0, 100) &&
                NativeIndicatorReadinessRule.IsFiniteBounded(0, 0, 100) &&
                !NativeIndicatorReadinessRule.IsFiniteBounded(
                    double.NaN,
                    0,
                    100) &&
                !NativeIndicatorReadinessRule.IsFiniteBounded(
                    101,
                    0,
                    100),
                "bounded oscillator readiness accepts valid RSI/ADX ranges only");

            Assert(
                NativeIndicatorReadinessRule.IsFrameReady(
                    60,
                    80,
                    80,
                    80,
                    80,
                    80,
                    14,
                    14,
                    14,
                    20,
                    50,
                    1.5,
                    50,
                    20,
                    100,
                    101),
                "native frame readiness accepts fully warmed finite inputs");

            Assert(
                !NativeIndicatorReadinessRule.IsFrameReady(
                    60,
                    80,
                    80,
                    80,
                    80,
                    80,
                    14,
                    14,
                    14,
                    20,
                    50,
                    1.5,
                    50,
                    20,
                    0,
                    101),
                "native frame readiness rejects an unusable EMA/volatility value");

            IndicatorEvidenceFusionResult neutralRsi =
                IndicatorEvidenceFusionRule.Evaluate(
                    new IndicatorEvidenceFusionInput(
                        "UNKNOWN",
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        4,
                        20,
                        20,
                        50,
                        0,
                        0,
                        0,
                        0,
                            58,
                        0,
                        0,
                        false,
                        false,
                        false,
                        false,
                        0,
                        0,
                        0,
                        4,
                        1));

            Assert(
                neutralRsi.BullBonus == 0 &&
                neutralRsi.BearBonus == 0,
                "fabricated RSI-neutral value 50 cannot create directional evidence");

            IndicatorEvidenceFusionResult directionalRsi =
                IndicatorEvidenceFusionRule.Evaluate(
                    new IndicatorEvidenceFusionInput(
                        "UNKNOWN",
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        4,
                        20,
                        20,
                        55,
                        0,
                        0,
                        0,
                        0,
                            58,
                        0,
                        0,
                        false,
                        false,
                        false,
                        false,
                        0,
                        0,
                        0,
                        4,
                        1));

            Assert(
                directionalRsi.BullBonus > 0,
                "RSI above the existing directional threshold remains an intentional evidence input");
        }

        private static void VerifyPendingFillExitResolutionSemantics()
        {
            PendingFillExitResolution buyAbsolute =
                PendingFillExitResolutionRule.ResolveProgressiveTarget(
                    1,
                    102,
                    103,
                    120,
                    112,
                    1);

            Assert(
                buyAbsolute.Allowed &&
                buyAbsolute.Price == 120 &&
                buyAbsolute.Source == "ABSOLUTE PLAN TARGET",
                "BUY positive-slippage fill restores the absolute planned TP when it is more progressive");

            PendingFillExitResolution buyNegative =
                PendingFillExitResolutionRule.ResolveProgressiveTarget(
                    1,
                    98,
                    99,
                    120,
                    108,
                    1);

            Assert(
                buyNegative.Allowed &&
                buyNegative.Price == 120,
                "BUY negative-slippage fill preserves the same absolute planned TP");

            PendingFillExitResolution brokerProgressive =
                PendingFillExitResolutionRule.ResolveProgressiveTarget(
                    1,
                    102,
                    103,
                    120,
                    125,
                    1);

            Assert(
                brokerProgressive.Allowed &&
                brokerProgressive.Price == 125 &&
                brokerProgressive.Source == "BROKER MORE PROGRESSIVE",
                "BUY broker target remains authoritative when it is more progressive");

            PendingFillExitResolution sellAbsolute =
                PendingFillExitResolutionRule.ResolveProgressiveTarget(
                    -1,
                    98,
                    97,
                    80,
                    88,
                    1);

            Assert(
                sellAbsolute.Allowed &&
                sellAbsolute.Price == 80 &&
                sellAbsolute.Source == "ABSOLUTE PLAN TARGET",
                "SELL positive-side fill preserves the absolute planned TP");

            PendingFillExitResolution behindMarket =
                PendingFillExitResolutionRule.ResolveProgressiveTarget(
                    1,
                    102,
                    106,
                    105,
                    112,
                    1);

            Assert(
                behindMarket.Allowed &&
                behindMarket.Price == 112 &&
                behindMarket.Source == "BROKER CONFIRMED TARGET",
                "a planned BUY TP already behind market falls back to the safe broker target");

            PendingFillExitResolution buyStop =
                PendingFillExitResolutionRule.ResolveProtectiveStop(
                    1,
                    102,
                    103,
                    95,
                    97,
                    1);

            Assert(
                buyStop.Allowed &&
                buyStop.Price == 97 &&
                buyStop.Source == "BROKER MORE PROTECTIVE",
                "BUY broker stop is retained when more protective after fill");

            PendingFillExitResolution sellStop =
                PendingFillExitResolutionRule.ResolveProtectiveStop(
                    -1,
                    98,
                    97,
                    105,
                    103,
                    1);

            Assert(
                sellStop.Allowed &&
                sellStop.Price == 103 &&
                sellStop.Source == "BROKER MORE PROTECTIVE",
                "SELL broker stop is retained when more protective after fill");

            PendingFillExitResolution brokerOnlyStop =
                PendingFillExitResolutionRule.ResolveProtectiveStop(
                    1,
                    102,
                    103,
                    0,
                    97,
                    1);

            Assert(
                brokerOnlyStop.Allowed &&
                brokerOnlyStop.Price == 97 &&
                brokerOnlyStop.Source == "BROKER CONFIRMED STOP",
                "broker-confirmed stop can close the protection gap when the plan stop is invalid");

            PendingFillExitResolution unsafeTarget =
                PendingFillExitResolutionRule.ResolveProgressiveTarget(
                    1,
                    102,
                    116,
                    110,
                    112,
                    1);

            Assert(
                !unsafeTarget.Allowed,
                "no forward-safe absolute or broker TP fails closed");

            PendingFillExitResolution repeat =
                PendingFillExitResolutionRule.ResolveProgressiveTarget(
                    1,
                    102,
                    103,
                    120,
                    112,
                    1);

            Assert(
                repeat.Allowed &&
                repeat.Price == buyAbsolute.Price &&
                repeat.Source == buyAbsolute.Source,
                "pending-fill exit resolution is deterministic and idempotent");

            LifecycleEventIdempotencyGuard guard =
                new LifecycleEventIdempotencyGuard();

            Assert(
                guard.TryBegin("PENDING_FILLED", 501) &&
                !guard.TryBegin("PENDING_FILLED", 501) &&
                guard.TryBegin("PENDING_FILLED", 502),
                "pending-fill lifecycle event is idempotent per broker pending-order identity");
        }

        private static void VerifyBrokerStateRefreshSemantics()
        {
            DateTime now =
                new DateTime(
                    2026,
                    9,
                    30,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                BrokerStateRefreshRule.IsRefreshDue(
                    true,
                    now,
                    now,
                    1000),
                "dirty broker state always requires reconciliation");

            Assert(
                BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    DateTime.MinValue,
                    now,
                    1000),
                "broker state without a prior refresh requires reconciliation");

            Assert(
                !BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    now,
                    now.AddMilliseconds(999),
                    1000) &&
                BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    now,
                    now.AddMilliseconds(1000),
                    1000),
                "broker refresh cadence is bounded by the configured interval");

            Assert(
                BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    now,
                    now.AddMilliseconds(-1),
                    1000),
                "broker refresh becomes due after clock rollback");
        }

        private static void VerifyBufferedArchivePersistence()
        {
            string directory =
                Path.Combine(
                    Path.GetTempPath(),
                    "CFIP.Runtime.Contracts",
                    "buffered-archive");

            Directory.CreateDirectory(
                directory);

            string path =
                Path.Combine(
                    directory,
                    "buffered.csv");

            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                BufferedArchivePersistence store =
                    new BufferedArchivePersistence();

                Assert(
                    store.Enqueue(
                        path,
                        "CFIP-TEST,1",
                        "1,alpha",
                        "1"),
                    "first keyed archive row is queued");

                Assert(
                    !store.Enqueue(
                        path,
                        "CFIP-TEST,1",
                        "1,duplicate",
                        "1"),
                    "duplicate queued archive key is rejected");

                Assert(
                    store.Enqueue(
                        path,
                        "CFIP-TEST,1",
                        "2,beta",
                        "2"),
                    "second keyed archive row is queued");

                Assert(
                    store.PendingLineCount == 2,
                    "buffer contains exactly unique pending rows");

                int firstFlush =
                    store.Flush(1);

                Assert(
                    firstFlush == 1 &&
                    store.PendingLineCount == 1,
                    "flush budget writes only one queued row");

                int secondFlush =
                    store.Flush(1);

                Assert(
                    secondFlush == 1 &&
                    store.PendingLineCount == 0,
                    "second bounded flush drains remaining row");

                string[] lines =
                    File.ReadAllLines(path);

                Assert(
                    lines.Length == 3 &&
                    lines[0] == "CFIP-TEST,1" &&
                    lines[1] == "1,alpha" &&
                    lines[2] == "2,beta",
                    "buffered archive writes header and each unique row once");

                string[] roundTripLines;
                Assert(
                    store.FileExists(path) &&
                    store.TryReadAllLines(
                        path,
                        out roundTripLines) &&
                    roundTripLines != null &&
                    roundTripLines.Length == 3 &&
                    store.WriteFailureCount == 0 &&
                    store.ReadFailureCount == 0 &&
                    store.LastSuccessUtc != DateTime.MinValue,
                    "central persistence owner can verify on-disk content without reported I/O failures");

                Assert(
                    !store.Enqueue(
                        path,
                        "CFIP-TEST,1",
                        "2,duplicate-after-flush",
                        "2"),
                    "existing on-disk keyed row remains idempotent");

                Assert(
                    store.PendingLineCount == 0,
                    "existing archive key creates no pending duplicate");
            }
            finally
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);

                    if (Directory.Exists(directory))
                        Directory.Delete(
                            directory,
                            true);
                }
                catch
                {
                    // Test cleanup must not mask the contract result.
                }
            }
        }

        private static void VerifyCalculationReadinessSemantics()
        {
            DateTime now =
                new DateTime(
                    2026,
                    9,
                    30,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                CalculationReadinessRule.ResolveState(
                    false,
                    false,
                    false,
                    -1) ==
                CalculationReadinessState.BuildingHistory,
                "missing bars remain in BUILDING_HISTORY");

            Assert(
                CalculationReadinessRule.ResolveState(
                    true,
                    false,
                    false,
                    -1) ==
                CalculationReadinessState.BuildingHistory,
                "insufficient history remains in BUILDING_HISTORY");

            Assert(
                CalculationReadinessRule.ResolveState(
                    true,
                    true,
                    true,
                    -1) ==
                CalculationReadinessState.WaitingForClosedM5,
                "no stable closed M5 is explicit WAITING_FOR_CLOSED_M5");

            Assert(
                CalculationReadinessRule.ResolveState(
                    true,
                    true,
                    false,
                    30) ==
                CalculationReadinessState.WaitingForMtfData,
                "primary MTF data gap is explicit WAITING_FOR_MTF_DATA");

            Assert(
                CalculationReadinessRule.ResolveState(
                    true,
                    true,
                    true,
                    30) ==
                CalculationReadinessState.Ready,
                "complete closed MTF context becomes READY");

            Assert(
                CalculationReadinessRule.IsProbeDue(
                    DateTime.MinValue,
                    now,
                    500),
                "first readiness probe is due immediately");

            Assert(
                !CalculationReadinessRule.IsProbeDue(
                    now,
                    now.AddMilliseconds(499),
                    500) &&
                CalculationReadinessRule.IsProbeDue(
                    now,
                    now.AddMilliseconds(500),
                    500),
                "waiting readiness probe is bounded by an explicit interval");

            Assert(
                CalculationReadinessRule.IsProbeDue(
                    now,
                    now.AddMilliseconds(-1),
                    500),
                "clock rollback reopens readiness probing");

            Assert(
                CalculationReadinessRule.ProbeIntervalMilliseconds(
                    CalculationReadinessState.WaitingForMtfData) == 500 &&
                CalculationReadinessRule.ProbeIntervalMilliseconds(
                    CalculationReadinessState.BuildingHistory) == 250 &&
                CalculationReadinessRule.ProbeIntervalMilliseconds(
                    CalculationReadinessState.Ready) == 0,
                "readiness backoff intervals are deterministic");

            Assert(
                CalculationReadinessRule.StatusText(
                    CalculationReadinessState.WaitingForMtfData) ==
                    "WAITING FOR MTF DATA" &&
                CalculationReadinessRule.StatusText(
                    CalculationReadinessState.Ready) ==
                    "READY",
                "readiness status text is deterministic");
        }

        private static void VerifyDailyLossSemantics()
        {
            DailyLossEvaluation evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    970,
                    0,
                    -30,
                    0,
                    3.0,
                    true,
                    true,
                    false);

            Assert(
                evaluation.DataReady &&
                evaluation.LimitHit &&
                evaluation.Locked &&
                Math.Abs(evaluation.LossAmount - 30) < 0.0001 &&
                Math.Abs(evaluation.LossPercent - 3.0) < 0.0001,
                "realized daily loss is measured from a stable equity baseline");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    -20,
                    970,
                    -50,
                    0,
                    0,
                    3.0,
                    true,
                    true,
                    false);

            Assert(
                evaluation.LimitHit &&
                Math.Abs(evaluation.LossAmount - 30) < 0.0001,
                "change in floating P/L from the day baseline is included");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    1500,
                    0,
                    0,
                    500,
                    3.0,
                    false,
                    true,
                    false);

            Assert(
                !evaluation.LimitHit &&
                evaluation.UsedEquityFallback &&
                Math.Abs(evaluation.DailyNetPnl) < 0.0001,
                "deposit does not become daily trading loss in equity fallback");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    700,
                    0,
                    0,
                    -300,
                    3.0,
                    false,
                    true,
                    false);

            Assert(
                !evaluation.LimitHit &&
                evaluation.UsedEquityFallback &&
                Math.Abs(evaluation.DailyNetPnl) < 0.0001,
                "withdrawal does not become daily trading loss in equity fallback");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    970,
                    0,
                    0,
                    0,
                    3.0,
                    true,
                    true,
                    true);

            Assert(
                evaluation.LimitHit &&
                evaluation.Locked &&
                evaluation.Reason ==
                    "DAILY LOSS LIMIT ALREADY LOCKED",
                "daily loss lock remains latched after equity recovery");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    969.9,
                    0,
                    -30.1,
                    0,
                    3.0,
                    true,
                    true,
                    false);

            Assert(
                evaluation.LimitHit &&
                Math.Abs(
                    evaluation.LossPercent -
                    3.01) < 0.02,
                "threshold comparison is inclusive at the configured percentage");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    970,
                    double.NaN,
                    -30,
                    0,
                    3.0,
                    true,
                    true,
                    false);

            Assert(
                !evaluation.DataReady,
                "non-finite floating P/L cannot produce a false-safe result");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    970,
                    0,
                    -30,
                    0,
                    double.NaN,
                    true,
                    true,
                    false);

            Assert(
                !evaluation.DataReady,
                "non-finite daily-loss threshold fails closed");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    970,
                    0,
                    -30,
                    0,
                    3.0,
                    true,
                    false,
                    false);

            Assert(
                !evaluation.DataReady &&
                evaluation.Reason ==
                    "DAILY LOSS TRANSACTION DATA UNAVAILABLE",
                "missing transaction facts fail closed");

            evaluation =
                DailyLossRule.Evaluate(
                    0,
                    0,
                    1000,
                    0,
                    0,
                    0,
                    3.0,
                    true,
                    true,
                    false);

            Assert(
                !evaluation.DataReady &&
                !evaluation.LimitHit,
                "invalid baseline is reported as unavailable rather than as a false limit hit");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    975,
                    0,
                    -25,
                    0,
                    0,
                    true,
                    true,
                    false);

            Assert(
                !evaluation.LimitHit,
                "zero configured threshold does not create an implicit hidden floor");
        }

        private static void VerifyEconomicNewsFeedStateSemantics()
        {
            DateTime now =
                new DateTime(
                    2026,
                    9,
                    30,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                EconomicNewsFeedStateRule.Evaluate(
                    false,
                    DateTime.MinValue,
                    now,
                    90,
                    false) ==
                EconomicNewsFeedState.Disabled,
                "disabled news feed reports DISABLED state");

            Assert(
                EconomicNewsFeedStateRule.Evaluate(
                    true,
                    DateTime.MinValue,
                    now,
                    90,
                    false) ==
                EconomicNewsFeedState.NeverLoaded,
                "missing successful refresh reports NEVER_LOADED");

            Assert(
                EconomicNewsFeedStateRule.Evaluate(
                    true,
                    now.AddMinutes(-30),
                    now,
                    90,
                    false) ==
                EconomicNewsFeedState.Healthy,
                "fresh successful refresh reports HEALTHY");

            Assert(
                EconomicNewsFeedStateRule.Evaluate(
                    true,
                    now.AddMinutes(-91),
                    now,
                    90,
                    false) ==
                EconomicNewsFeedState.Stale,
                "expired successful refresh reports STALE");

            Assert(
                EconomicNewsFeedStateRule.Evaluate(
                    true,
                    now.AddMinutes(-1),
                    now,
                    90,
                    true) ==
                EconomicNewsFeedState.BlockingEvent,
                "active event overrides feed health with BLOCKING_EVENT");
        }

        private static void VerifyEconomicNewsCurrencyMappingSemantics()
        {
            string[] eurUsd =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "EURUSD.m",
                    "",
                    "");

            Assert(
                eurUsd.Length == 2 &&
                eurUsd[0] == "EUR" &&
                eurUsd[1] == "USD",
                "FX symbol resolves both traded currencies");

            string[] indexUsd =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "US30.cash",
                    "",
                    "US30=USD;GER40=EUR");

            Assert(
                indexUsd.Length == 1 &&
                indexUsd[0] == "USD",
                "index mapping resolves configured USD news relevance");

            string[] indexEur =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "GER40",
                    "",
                    "US30=USD;GER40=EUR");

            Assert(
                indexEur.Length == 1 &&
                indexEur[0] == "EUR",
                "index mapping resolves configured EUR news relevance");

            string[] crypto =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "BTCUSDT",
                    "",
                    "BTC=USD;ETH=USD");

            Assert(
                crypto.Length == 1 &&
                crypto[0] == "USD",
                "crypto mapping resolves USD news relevance");

            string[] extras =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "EURUSD",
                    "JPY, CHF",
                    "");

            Assert(
                extras.Length == 4 &&
                extras[0] == "CHF" &&
                extras[1] == "EUR" &&
                extras[2] == "JPY" &&
                extras[3] == "USD",
                "additional currencies merge deterministically without duplicates");

            string[] normalized =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "US_500.cash",
                    "",
                    "US500=USD");

            Assert(
                normalized.Length == 1 &&
                normalized[0] == "USD",
                "broker symbol punctuation does not break configured mapping");
        }


        private static void VerifyCi10TriggerLifecycle()
        {
            Assert(
                TriggerThresholdRule.ResolveRequiredScore(
                    false,
                    4,
                    5) == 4 &&
                TriggerThresholdRule.ResolveRequiredScore(
                    true,
                    4,
                    5) == 5 &&
                TriggerThresholdRule.ResolveRequiredScore(
                    true,
                    5,
                    4) == 5,
                "CI-10 trigger threshold owner preserves the existing live/precision max policy");

            Assert(
                TriggerThresholdRule.ResolveRequiredScore(
                    false,
                    0,
                    5) == 0 &&
                TriggerThresholdRule.ResolveRequiredScore(
                    true,
                    4,
                    1) == 0 &&
                !TriggerThresholdRule.IsScoreReady(
                    7,
                    4) &&
                !TriggerThresholdRule.IsScoreReady(
                    4,
                    0),
                "CI-10 invalid trigger threshold inputs fail closed");

            DateTime m5Open = Utc(12, 0);
            DateTime m5NextOpen = Utc(12, 5);
            DateTime m1Open = Utc(12, 4);
            DateTime m1NextOpen = Utc(12, 5);

            Assert(
                TriggerLifecycleRule.CanEvaluateLiveM1Confirmation(
                    10,
                    11,
                    11,
                    m1Open,
                    m1NextOpen,
                    m5Open,
                    m5NextOpen,
                    Utc(12, 5)),
                "CI-10 newly closed M1 confirmation is evaluated only inside the active next M5 window");

            Assert(
                !TriggerLifecycleRule.CanEvaluateLiveM1Confirmation(
                    10,
                    11,
                    10,
                    m1Open,
                    m1NextOpen,
                    m5Open,
                    m5NextOpen,
                    Utc(12, 5)) &&
                !TriggerLifecycleRule.CanEvaluateLiveM1Confirmation(
                    10,
                    10,
                    10,
                    m1Open,
                    m1NextOpen,
                    m5Open,
                    m5NextOpen,
                    Utc(12, 5)),
                "CI-10 stale/expired M1 confirmations cannot be accepted after the causal M5 window");

            Assert(
                !TriggerLifecycleRule.CanEvaluateLiveM1Confirmation(
                    10,
                    11,
                    11,
                    m1Open,
                    m1NextOpen,
                    m5Open,
                    m5NextOpen,
                    new DateTime(2026, 1, 1, 12, 4, 30, DateTimeKind.Utc)),
                "CI-10 an unclosed M1 bar cannot become a trigger");

            Assert(
                TriggerLifecycleRule.ShouldReset(
                    10,
                    1,
                    11,
                    1) &&
                TriggerLifecycleRule.ShouldReset(
                    10,
                    1,
                    10,
                    -1) &&
                !TriggerLifecycleRule.ShouldReset(
                    10,
                    1,
                    10,
                    1),
                "CI-10 trigger runtime resets on M5-window or direction identity change");

            Assert(
                TriggerLifecycleRule.ShouldRecordNewConfirmation(
                    -1,
                    11,
                    true) &&
                !TriggerLifecycleRule.ShouldRecordNewConfirmation(
                    11,
                    11,
                    true) &&
                !TriggerLifecycleRule.ShouldRecordNewConfirmation(
                    -1,
                    11,
                    false),
                "CI-10 M1 confirmation revision advances only for a new ready causal bar");

            Assert(
                TriggerLifecycleRule.IsConfirmed(
                    true,
                    false,
                    false) &&
                TriggerLifecycleRule.IsConfirmed(
                    true,
                    true,
                    true) &&
                !TriggerLifecycleRule.IsConfirmed(
                    true,
                    true,
                    false) &&
                !TriggerLifecycleRule.IsConfirmed(
                    false,
                    true,
                    true),
                "CI-10 confirmed TriggerReady propagation preserves M5 and optional M1 gates");

            TriggerRuntimeState runtime = new TriggerRuntimeState();
            runtime.ConfirmationRevision = 7;
            runtime.ConfirmedM1 = 11;
            runtime.Reset(
                12,
                1);

            Assert(
                runtime.ConfirmationRevision == 7 &&
                runtime.ConfirmedM1 == -1 &&
                runtime.DecisionM5 == 12 &&
                runtime.Direction == 1,
                "CI-10 trigger reset expires the prior M1 confirmation without rewinding the monotonic revision");
            
            Console.WriteLine("CI-10 trigger lifecycle contracts PASS");
        }

        private static void VerifyM1TriggerSemantics()
        {
            DateTime m5Open = Utc(12, 0);
            DateTime m5NextOpen = Utc(12, 5);
            DateTime m1Open = Utc(12, 4);
            DateTime m1NextOpen = Utc(12, 5);

            Assert(
                M1TriggerRule.IsClosedInsideM5Window(
                    m1Open,
                    m1NextOpen,
                    m5Open,
                    m5NextOpen,
                    Utc(12, 5)),
                "closed M1 bar belongs to the closed M5 window");

            Assert(
                !M1TriggerRule.IsClosedInsideM5Window(
                    Utc(12, 5),
                    Utc(12, 6),
                    m5Open,
                    m5NextOpen,
                    Utc(12, 5)),
                "M1 bar outside the M5 window is rejected");

            Assert(
                !M1TriggerRule.IsClosedInsideM5Window(
                    m1Open,
                    m1NextOpen,
                    m5Open,
                    m5NextOpen,
                    Utc(12, 4)),
                "M1 bar still open at the reference is rejected");

            Assert(
                M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    104,
                    99,
                    103,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    4,
                    4,
                    102,
                    98,
                    0.05,
                    false,
                    0),
                "bullish M1 trigger requires and accepts a causal micro-structure break");

            Assert(
                M1TriggerRule.IsReady(
                    -1,
                    -1,
                    100,
                    101,
                    96,
                    97,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    4,
                    4,
                    102,
                    98,
                    0.05,
                    false,
                    0),
                "bearish M1 trigger is directionally symmetric");

            Assert(
                M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    104,
                    99,
                    103,
                    2,
                    0.12,
                    0.70,
                    2.5,
                    6,
                    4,
                    104,
                    98,
                    0.05,
                    true,
                    0.80),
                "configured displacement can provide the causal confirmation when micro-break is absent");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    104,
                    99,
                    103,
                    2,
                    0.12,
                    0.70,
                    2.5,
                    6,
                    4,
                    104,
                    98,
                    0.05,
                    false,
                    0),
                "high technical trigger score cannot replace structural/displacement evidence");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    103,
                    99,
                    102,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    4,
                    4,
                    102.5,
                    98,
                    0.05,
                    false,
                    0),
                "a close that does not clear the buffered prior micro-high cannot confirm");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    -1,
                    100,
                    104,
                    99,
                    103,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    4,
                    4,
                    102,
                    98,
                    0.05,
                    true,
                    0.80),
                "opposite M1 direction cannot confirm the selected decision");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    101,
                    99,
                    100.5,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    4,
                    4,
                    100,
                    98,
                    0.05,
                    false,
                    0),
                "weak M1 body cannot confirm");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    104,
                    99,
                    100.5,
                    2,
                    0.12,
                    0.70,
                    2.5,
                    4,
                    4,
                    100,
                    98,
                    0.05,
                    false,
                    0),
                "poor M1 close location cannot confirm");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    103,
                    99,
                    102,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    3,
                    4,
                    101,
                    98,
                    0.05,
                    false,
                    0),
                "insufficient M1 trigger score cannot confirm");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    107,
                    99,
                    106,
                    2,
                    0.50,
                    0.70,
                    3.0,
                    4,
                    4,
                    102,
                    98,
                    0.05,
                    false,
                    0),
                "abnormally large M1 range cannot confirm");
        }

        private static void VerifyZoneConfluenceSymmetry()
        {
            DateTime liveM5Open = Utc(12, 5);
            DateTime liveM5NextOpen = Utc(12, 10);
            DateTime closedM1Open = Utc(12, 6);
            DateTime closedM1NextOpen = Utc(12, 7);

            Assert(
                M1TriggerRule.IsClosedM1InsideM5Window(
                    closedM1Open,
                    closedM1NextOpen,
                    liveM5Open,
                    liveM5NextOpen,
                    Utc(12, 7)),
                "closed M1 can confirm inside the currently forming M5 window");

            Assert(
                !M1TriggerRule.IsClosedM1InsideM5Window(
                    closedM1Open,
                    Utc(12, 8),
                    liveM5Open,
                    liveM5NextOpen,
                    Utc(12, 7)),
                "M1 trigger runtime rejects an M1 bar that is not closed yet");

            Assert(
                ZoneConfluenceRule.HasOverlap(
                    100,
                    105,
                    104,
                    110,
                    0) &&
                ZoneConfluenceRule.HasOverlap(
                    104,
                    110,
                    100,
                    105,
                    0),
                "zone overlap is BUY/SELL independent and argument-order symmetric");

            Assert(
                !ZoneConfluenceRule.HasOverlap(
                    100,
                    102,
                    102,
                    104,
                    0),
                "touch-only zone boundaries are not treated as positive-width confluence");

            Assert(
                ZoneConfluenceRule.HasOverlap(
                    100,
                    102,
                    102.05,
                    104,
                    0.10),
                "explicit confluence tolerance is symmetric");

            Assert(
                ZoneConfluenceRule.IsDirectionalMatch(1, 1) &&
                ZoneConfluenceRule.IsDirectionalMatch(-1, -1) &&
                !ZoneConfluenceRule.IsDirectionalMatch(1, -1) &&
                !ZoneConfluenceRule.IsDirectionalMatch(-1, 1),
                "zone direction matching is explicitly mirrored");
        }

        private static void VerifyTopDownCalibration()
        {
            TopDownCalibrationSnapshot strongBuy =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1, 0 },
                    new[] { 90, 84, 80, 0 },
                    new[] { 3.0, 2.0, 2.0, 0.0 },
                    new[] { 1, 1 },
                    new[] { 82, 78 },
                    new[] { 5.0, 8.0 },
                    1,
                    85,
                    72,
                    1);

            Assert(
                strongBuy.HtfStrong &&
                strongBuy.HtfDirection == 1 &&
                strongBuy.Eligible &&
                strongBuy.Stage == "ENTRY CALIBRATED",
                "strong H1+ anchor plus aligned mid/entry frames calibrates an actionable setup");

            TopDownCalibrationSnapshot entryConflict =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1, 0 },
                    new[] { 90, 84, 80, 0 },
                    new[] { 3.0, 2.0, 2.0, 0.0 },
                    new[] { 1, 1 },
                    new[] { 82, 78 },
                    new[] { 5.0, 8.0 },
                    -1,
                    85,
                    72,
                    -1);

            Assert(
                !entryConflict.Eligible &&
                entryConflict.Stage == "ENTRY CONFLICT",
                "M5 entry direction cannot override a strong H1+ anchor");

            TopDownCalibrationSnapshot midConflict =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1, 0 },
                    new[] { 90, 84, 80, 0 },
                    new[] { 3.0, 2.0, 2.0, 0.0 },
                    new[] { -1, -1 },
                    new[] { 82, 78 },
                    new[] { 5.0, 8.0 },
                    1,
                    85,
                    72,
                    1);

            Assert(
                !midConflict.Eligible &&
                midConflict.Stage == "MIDFRAME CONFLICT",
                "strong middle-timeframe conflict blocks lower-frame override");

            TopDownCalibrationSnapshot mixedHtf =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, -1, 0, 0 },
                    new[] { 90, 90, 0, 0 },
                    new[] { 3.0, 2.0, 2.0, 0.0 },
                    new[] { 1, 0 },
                    new[] { 85, 0 },
                    new[] { 5.0, 8.0 },
                    1,
                    85,
                    72,
                    1);

            Assert(
                mixedHtf.Stage == "HTF MIXED",
                "mixed H1+ context never pretends to be a calibrated anchor");

            TopDownCalibrationSnapshot weakMiddleAgreement =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1, 0 },
                    new[] { 90, 84, 80, 0 },
                    new double[] { 3.0, 2.0, 2.0, 0.0 },
                    new[] { 1, -1 },
                    new[] { 100, 100 },
                    new double[] { 5.0, 5.0 },
                    1,
                    85,
                    72,
                    1);

            Assert(
                weakMiddleAgreement.MidDirection == 0 &&
                weakMiddleAgreement.Stage == "MIDFRAME CALIBRATION",
                "directionally mixed middle frames cannot be promoted to calibrated entry");

        }

        private static void VerifyStructuralStopScoringSemantics()
        {
            Assert(
                StructuralStopScoringRule.CalculateRewardPathBonus(
                    2.0,
                    1.50) == 6.0 &&
                StructuralStopScoringRule.CalculateRewardPathBonus(
                    10.0,
                    1.50) == StructuralStopScoringRule.RewardPathBonusCap,
                "structural-stop reward-path bonus is bounded and preserves the existing slope");

            Assert(
                StructuralStopScoringRule.CalculateRewardPathBonus(
                    1.40,
                    1.50) == 0 &&
                StructuralStopScoringRule.CalculateRewardPathBonus(
                    double.NaN,
                    1.50) == 0,
                "structural-stop reward bonus fails closed below required RR or on non-finite input");

            double atPreferred =
                StructuralStopScoringRule.CalculateRiskBalance(
                    1.0,
                    1.0,
                    18);

            double tight =
                StructuralStopScoringRule.CalculateRiskBalance(
                    0.5,
                    1.0,
                    18);

            double wide =
                StructuralStopScoringRule.CalculateRiskBalance(
                    1.5,
                    1.0,
                    18);

            Assert(
                atPreferred > tight &&
                Math.Abs(tight - wide) < 1e-9,
                "structural-stop risk balance peaks at preferred risk and is symmetric around it");

            Assert(
                StructuralStopScoringRule.CalculateRiskBalance(
                    double.NaN,
                    1.0,
                    18) == 0 &&
                StructuralStopScoringRule.CalculateRiskBalance(
                    1.0,
                    1.0,
                    -1) == 0,
                "structural-stop risk scoring is finite and non-negative");
        }

        private static void VerifyDivergenceThresholdSemantics()
        {
            Assert(
                DivergenceThresholdRule.MinimumQuality == 55 &&
                DivergenceThresholdRule.ConflictQualityMargin == 10 &&
                DivergenceThresholdRule.RegularRsiDelta == 2.0 &&
                DivergenceThresholdRule.HiddenRsiDelta == 2.0 &&
                DivergenceThresholdRule.RegularWaveDelta == 1.50 &&
                DivergenceThresholdRule.HiddenWaveDelta == 1.50,
                "divergence quality/conflict and oscillator thresholds have one semantic owner");

            Assert(
                DivergenceThresholdRule.MinimumRegularPriceExcursion(1.0) == 0.10 &&
                DivergenceThresholdRule.MinimumHiddenPriceExcursion(1.0) == 0.08 &&
                DivergenceThresholdRule.MinimumRegularPriceExcursion(2.0) == 0.20 &&
                DivergenceThresholdRule.MinimumHiddenPriceExcursion(2.0) == 0.16,
                "divergence price thresholds scale with ATR while retaining absolute floors");

            Assert(
                DivergenceThresholdRule.ResolveRecentBoost(100, 88) ==
                    DivergenceThresholdRule.RecentBoostStrong &&
                DivergenceThresholdRule.ResolveRecentBoost(100, 80) ==
                    DivergenceThresholdRule.RecentBoostModerate &&
                DivergenceThresholdRule.ResolveRecentBoost(100, 79) == 0,
                "divergence recency boost boundaries are deterministic");

            Assert(
                DivergenceThresholdRule.CalculateQuality(0.5, 2, false, 5) >
                DivergenceThresholdRule.CalculateQuality(0.5, 1, false, 5) &&
                DivergenceThresholdRule.CalculateQuality(
                    double.NaN, 2, false, 5) == 0 &&
                DivergenceThresholdRule.CalculateQuality(
                    double.PositiveInfinity, 2, false, 5) == 0,
                "divergence quality is bounded and fails closed on invalid input");
        }

        private static void VerifyRejectionThresholdSemantics()
        {
            Assert(
                RejectionRule.MinimumBodyPips == 0.10 &&
                RejectionRule.MinimumBodyRangeFraction == 0.05 &&
                RejectionRule.WickToBodyRatio == 1.25 &&
                RejectionRule.MinimumWickRangeFraction == 0.20,
                "rejection/doji geometry has one explicit threshold owner");

            Assert(
                RejectionRule.ResolveMinimumMeaningfulBody(
                    2.0,
                    0.10) == 0.10 &&
                RejectionRule.ResolveMinimumMeaningfulBody(
                    4.0,
                    0.10) == 0.20,
                "minimum meaningful body uses the larger pip/range floor");

            Assert(
                RejectionRule.IsDoji(
                    100.0,
                    100.11,
                    102.0,
                    100.0,
                    0.10) == false &&
                RejectionRule.IsDoji(
                    100.0,
                    100.09,
                    102.0,
                    100.0,
                    0.10),
                "doji boundary is deterministic at the minimum meaningful body");

            Assert(
                RejectionRule.IsRejection(
                    100.0,
                    100.5,
                    101.0,
                    98.0,
                    1,
                    0.10) &&
                RejectionRule.IsRejection(
                    100.5,
                    100.0,
                    102.0,
                    99.0,
                    -1,
                    0.10),
                "bullish and bearish rejection geometry remains symmetric");
        }

        private static void VerifyHistoricalRenderingSemantics()
        {
            Assert(
                HistoricalRenderingRule.MaximumScanBars == 500 &&
                HistoricalRenderingRule.FirstAnalyzableIndex == 40 &&
                HistoricalRenderingRule.MinimumBarsRequired == 60,
                "historical presentation work has an explicit fixed scan budget");

            Assert(
                HistoricalRenderingRule.ResolveLastClosedIndex(100) == 98 &&
                HistoricalRenderingRule.ResolveOldestScannedIndex(100) == 40,
                "short histories use only fully closed analyzable bars");

            Assert(
                HistoricalRenderingRule.ResolveLastClosedIndex(10000) == 9998 &&
                HistoricalRenderingRule.ResolveOldestScannedIndex(10000) == 9499,
                "long histories are bounded to exactly 500 scanned bars");

            DateTime openTime =
                new DateTime(
                    2026,
                    9,
                    30,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                HistoricalRenderingRule.ObjectIdentity(openTime) ==
                HistoricalRenderingRule.ObjectIdentity(openTime) &&
                HistoricalRenderingRule.ObjectIdentity(openTime) !=
                HistoricalRenderingRule.ObjectIdentity(
                    openTime.AddMinutes(5)) &&
                HistoricalRenderingRule.ObjectIdentity(
                    openTime).Contains("PRESENTATION_"),
                "historical object identity is deterministic and timestamp based");

            Assert(
                HistoricalRenderingRule.IsClosedAnalyzableIndex(
                    40,
                    100) &&
                !HistoricalRenderingRule.IsClosedAnalyzableIndex(
                    99,
                    100) &&
                !HistoricalRenderingRule.IsClosedAnalyzableIndex(
                    39,
                    100),
                "historical renderer never treats the open host bar as a historical signal");
        }

        private static void VerifyWaveTrendMathematics()
        {
            Assert(
                WaveTrendMovingAverageCalculator.RequiredSourceBars(
                    WaveTrendMovingAverageCalculator.Simple,
                    3) == 3 &&
                WaveTrendMovingAverageCalculator.RequiredSourceBars(
                    WaveTrendMovingAverageCalculator.DoubleExponential,
                    3) == 5 &&
                WaveTrendMovingAverageCalculator.RequiredSourceBars(
                    WaveTrendMovingAverageCalculator.TripleExponential,
                    3) == 7 &&
                WaveTrendMovingAverageCalculator.RequiredSourceBars(
                    WaveTrendMovingAverageCalculator.Hull,
                    4) == 5,
                "WaveTrend MA dependency depth is explicit for simple, DEMA, TEMA and HMA");

            double[] source =
            {
                1, 2, 3, 4, 5, 6, 7
            };

            WaveTrendMovingAverageCalculator simple =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Simple,
                    3,
                    0);

            double value;
            Assert(
                !simple.TryCalculateWaveTrendAverage(source, 1, out value) &&
                simple.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2,
                "WaveTrend SMA uses a closed source window");

            WaveTrendMovingAverageCalculator exponential =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Exponential,
                    3,
                    0);

            Assert(
                exponential.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2 &&
                exponential.TryCalculateWaveTrendAverage(source, 3, out value) &&
                value == 3,
                "WaveTrend EMA seeds from an SMA and then uses alpha 2/(L+1)");

            WaveTrendMovingAverageCalculator wilder =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.WilderSmoothing,
                    3,
                    0);

            Assert(
                wilder.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2 &&
                wilder.TryCalculateWaveTrendAverage(source, 3, out value) &&
                Math.Abs(value - 2.6666666667) < 1e-9,
                "WaveTrend Wilder smoothing uses alpha 1/L");

            WaveTrendMovingAverageCalculator weighted =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Weighted,
                    3,
                    0);

            Assert(
                weighted.TryCalculateWaveTrendAverage(source, 2, out value) &&
                Math.Abs(value - 14.0 / 6.0) < 1e-9,
                "WaveTrend WMA uses linearly increasing weights");

            WaveTrendMovingAverageCalculator timeSeries =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.TimeSeries,
                    3,
                    0);

            Assert(
                timeSeries.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 3,
                "WaveTrend TimeSeries MA uses the regression endpoint");

            WaveTrendMovingAverageCalculator triangular =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Triangular,
                    3,
                    0);

            Assert(
                triangular.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2,
                "WaveTrend triangular smoothing composes two canonical SMA windows");

            WaveTrendMovingAverageCalculator vidya =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Vidya,
                    3,
                    0);

            Assert(
                vidya.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2 &&
                vidya.TryCalculateWaveTrendAverage(source, 3, out value) &&
                Math.Abs(value - 3.0) < 1e-9,
                "WaveTrend VIDYA adapts EMA alpha from source momentum efficiency");

            WaveTrendMovingAverageCalculator hull =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Hull,
                    4,
                    0);

            Assert(
                hull.TryCalculateWaveTrendAverage(source, 4, out value) &&
                Math.Abs(value - 5.0) < 1e-9,
                "WaveTrend HMA composes half-length WMA, full-length WMA and sqrt-length WMA");

            WaveTrendMovingAverageCalculator dema =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.DoubleExponential,
                    3,
                    0);

            Assert(
                dema.TryCalculateWaveTrendAverage(source, 4, out value) &&
                Math.Abs(value - 5.0) < 1e-9,
                "WaveTrend DEMA is 2*EMA1-EMA2 with explicit warm-up");

            WaveTrendMovingAverageCalculator tema =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.TripleExponential,
                    3,
                    0);

            Assert(
                tema.TryCalculateWaveTrendAverage(source, 6, out value) &&
                Math.Abs(value - 7.0) < 1e-9,
                "WaveTrend TEMA is 3*EMA1-3*EMA2+EMA3 with explicit warm-up");

            WaveTrendMovingAverageCalculator kama =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.KaufmanAdaptive,
                    3,
                    0);

            Assert(
                kama.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2 &&
                kama.TryCalculateWaveTrendAverage(source, 3, out value) &&
                Math.Abs(value - 2.8888888889) < 1e-9,
                "WaveTrend Kaufman Adaptive MA uses efficiency-ratio smoothing");

            int componentReady =
                WaveTrendReadinessRule.ResolveWaveTrendComponentReadyIndex(
                    10,
                    5);

            int smoothReady =
                WaveTrendReadinessRule.ResolveWaveTrendSmoothReadyIndex(
                    componentReady,
                    WaveTrendMovingAverageCalculator.Exponential,
                    4);

            int signalReady =
                WaveTrendReadinessRule.ResolveWaveTrendSignalReadyIndex(
                    smoothReady,
                    WaveTrendMovingAverageCalculator.Simple,
                    5);

            Assert(
                componentReady == 14 &&
                smoothReady == 17 &&
                signalReady == 21 &&
                !WaveTrendReadinessRule.IsWaveTrendSnapshotReady(
                    21,
                    signalReady) &&
                WaveTrendReadinessRule.IsWaveTrendSnapshotReady(
                    22,
                    signalReady),
                "WaveTrend readiness includes RSI/MFI/RMI dependencies, both smoothing layers and previous-bar stability");

            WaveTrendMovingAverageCalculator stateCalculator =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Exponential,
                    3,
                    0);

            Assert(
                double.IsNaN(
                    stateCalculator.GetWaveTrendAverageValue(0)),
                "WaveTrend calculator keeps pre-warm-up state non-ready");
        }

        private static void VerifyWaveTrendEvidence()
        {
            WaveTrendSnapshot bullCross =
                new WaveTrendSnapshot(
                    true,
                    8,
                    3,
                    5,
                    1,
                    4,
                    7,
                    true,
                    false,
                    true,
                    false,
                    false,
                    false,
                    true,
                    false);

            WaveTrendEvidenceResult bull =
                WaveTrendEvidenceRule.Evaluate(
                    bullCross,
                    58);

            Assert(
                bull.Bull &&
                bull.Direction == 1 &&
                bull.Quality >= 58,
                "WaveTrend bullish cross/rising state becomes bounded bullish evidence");

            WaveTrendSnapshot bearCross =
                new WaveTrendSnapshot(
                    true,
                    -8,
                    -3,
                    -5,
                    -1,
                    -4,
                    -7,
                    false,
                    true,
                    false,
                    true,
                    false,
                    false,
                    false,
                    true);

            WaveTrendEvidenceResult bear =
                WaveTrendEvidenceRule.Evaluate(
                    bearCross,
                    58);

            Assert(
                bear.Bear &&
                bear.Direction == -1 &&
                bear.Quality >= 58,
                "WaveTrend bearish cross/falling state becomes bounded bearish evidence");

            WaveTrendSnapshot weak =
                new WaveTrendSnapshot(
                    true,
                    1,
                    0.5,
                    0.5,
                    0.9,
                    0.5,
                    0.1,
                    false,
                    false,
                    true,
                    false,
                    false,
                    false,
                    true,
                    false);

            WaveTrendEvidenceResult weakResult =
                WaveTrendEvidenceRule.Evaluate(
                    weak,
                    70);

            Assert(
                !weakResult.Bull &&
                !weakResult.Bear,
                "weak WaveTrend movement cannot manufacture a strong evidence direction");
        }

        private static void VerifyParallelOpportunityRule()
        {
            TacticalOpportunityResult regular =
                TacticalOpportunityRule.Evaluate(
                    1,
                    92,
                    78,
                    4,
                    4,
                    1,
                    1,
                    55,
                    72,
                    1.90,
                    70,
                    1.75,
                    82,
                    2.20);

            Assert(
                regular.Allowed &&
                regular.Lane == OpportunityLane.Tactical &&
                regular.RiskReward >= 1.75,
                "aligned LTF opportunity can qualify without requiring strategic HTF calibration");

            TacticalOpportunityResult counterWeak =
                TacticalOpportunityRule.Evaluate(
                    1,
                    80,
                    75,
                    4,
                    4,
                    1,
                    -1,
                    84,
                    72,
                    2.25,
                    70,
                    1.75,
                    82,
                    2.20);

            Assert(
                !counterWeak.Allowed &&
                counterWeak.Lane == OpportunityLane.CounterHtfTactical,
                "counter-HTF opportunity needs the stricter tactical quality gate");

            TacticalOpportunityResult counterStrong =
                TacticalOpportunityRule.Evaluate(
                    1,
                    95,
                    86,
                    5,
                    5,
                    1,
                    -1,
                    84,
                    72,
                    2.35,
                    70,
                    1.75,
                    82,
                    2.20);

            Assert(
                counterStrong.Allowed &&
                counterStrong.Lane == OpportunityLane.CounterHtfTactical,
                "very strong LTF RR opportunity can survive a strong HTF conflict");
        }

        private static void VerifyIndependentEvidenceGroupSemantics()
        {
            IndependentEvidenceFusionInput structuralOnly =
                new IndependentEvidenceFusionInput(
                    true,
                    true,
                    true,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false);

            Assert(
                IndependentEvidenceFusionRule.CountGroups(structuralOnly) == 1,
                "correlated Structure/MSS/CHOCH/Displacement evidence belongs to one structural group");

            Assert(
                IndependentEvidenceFusionRule.CalculateScore(structuralOnly) == 2,
                "legacy structural evidence score remains numerically unchanged");

            IndependentEvidenceFusionInput locationOnly =
                new IndependentEvidenceFusionInput(
                    false,
                    false,
                    false,
                    true,
                    true,
                    true,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false);

            Assert(
                IndependentEvidenceFusionRule.CountGroups(locationOnly) == 1,
                "Liquidity/FVG/OrderBlock evidence belongs to one location group");

            IndependentEvidenceFusionInput allGroups =
                new IndependentEvidenceFusionInput(
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true);

            Assert(
                IndependentEvidenceFusionRule.CountGroups(allGroups) == 4,
                "all independent evidence families resolve to exactly four groups");

            IndependentEvidenceFusionInput buyProjection =
                new IndependentEvidenceFusionInput(
                    true,
                    false,
                    false,
                    true,
                    false,
                    false,
                    true,
                    false,
                    false,
                    false,
                    true,
                    false,
                    false,
                    false);

            IndependentEvidenceFusionInput sellProjection =
                new IndependentEvidenceFusionInput(
                    false,
                    true,
                    true,
                    false,
                    true,
                    false,
                    false,
                    true,
                    false,
                    true,
                    false,
                    true,
                    true,
                    false);

            Assert(
                IndependentEvidenceFusionRule.CountGroups(buyProjection) == 4 &&
                IndependentEvidenceFusionRule.CountGroups(sellProjection) == 4,
                "BUY/SELL directional projections preserve identical independent-family semantics");

            Assert(
                IndependentEvidenceFusionRule.CountGroups(
                    new IndependentEvidenceFusionInput(
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false)) == 0,
                "no evidence produces zero independent groups");
        }

        private static void VerifyProtectionProgressionSemantics()
        {
            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    100,
                    101) &&
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    101,
                    100),
                "BUY stop progression never moves backward");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    100,
                    99) &&
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    99,
                    100),
                "SELL stop progression never moves backward");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    110,
                    120,
                    true) &&
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    120,
                    110,
                    true) &&
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    110,
                    100,
                    true) &&
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    100,
                    110,
                    true),
                "target progression preserves directional monotonicity");
        }

        private static void VerifyOrderBlockMathematics()
        {
            double low;
            double high;
            double remainingRatio;
            bool partial;
            OrderBlockLifecycleState lifecycleState;

            Assert(
                OrderBlockRule.IsOppositeSourceCandle(1, 101, 100) &&
                OrderBlockRule.IsOppositeSourceCandle(-1, 100, 101) &&
                !OrderBlockRule.IsOppositeSourceCandle(1, 100, 101),
                "Order Block source candle must be opposite to intended direction");

            Assert(
                OrderBlockRule.TryGetZone(
                    1, false, 101, 100, 103, 99,
                    out low, out high) &&
                low == 99 && high == 103,
                "wick-based Order Block zone preserves full source range");

            Assert(
                OrderBlockRule.TryGetZone(
                    1, true, 101, 100, 103, 99,
                    out low, out high) &&
                low == 100 && high == 101,
                "body-based Order Block zone uses source body");

            Assert(
                OrderBlockRule.MeetsDisplacement(
                    1, 100, 101, 2.0, 0.50) &&
                !OrderBlockRule.MeetsDisplacement(
                    1, 100, 100.99, 2.0, 0.50) &&
                OrderBlockRule.MeetsDisplacement(
                    -1, 101, 100, 2.0, 0.50),
                "displacement is directional and anchored to creation-bar ATR");

            Assert(
                OrderBlockRule.BreaksStructure(
                    1, 101.1, 100, 2.0, 0.50) &&
                !OrderBlockRule.BreaksStructure(
                    1, 101.0, 100, 2.0, 0.50) &&
                OrderBlockRule.BreaksStructure(
                    -1, 98.9, 100, 2.0, 0.50),
                "Order Block structure break uses explicit creation-ATR threshold");

            Assert(
                OrderBlockLifecycleRule.ResolveOrderBlockMitigationProbe(
                    1, 100, 101, 103, 99, false) == 100 &&
                OrderBlockLifecycleRule.ResolveOrderBlockMitigationProbe(
                    -1, 100, 101, 103, 99, true) == 103,
                "Order Block wick/body probe semantics are directional");

            Assert(
                OrderBlockLifecycleRule.TryApplyOrderBlockPartialMitigation(
                    1, 99, 103, 101, 0.01,
                    out low, out high, out partial,
                    out remainingRatio,
                    out lifecycleState) &&
                partial &&
                lifecycleState ==
                    OrderBlockLifecycleState.Mitigated &&
                low == 99 && high == 101,
                "bullish Order Block partial mitigation moves upper boundary and marks the zone mitigated");

            Assert(
                OrderBlockLifecycleRule.TryApplyOrderBlockPartialMitigation(
                    -1, 99, 103, 101, 0.01,
                    out low, out high, out partial,
                    out remainingRatio,
                    out lifecycleState) &&
                partial &&
                lifecycleState ==
                    OrderBlockLifecycleState.Mitigated &&
                low == 101 && high == 103,
                "bearish Order Block partial mitigation moves lower boundary and remains directionally symmetric");

            Assert(
                OrderBlockLifecycleRule.ClassifyOrderBlockLifecycle(false, 1.0) ==
                    OrderBlockLifecycleState.Fresh &&
                OrderBlockLifecycleRule.ClassifyOrderBlockLifecycle(true, 0.5) ==
                    OrderBlockLifecycleState.Mitigated &&
                OrderBlockLifecycleRule.ClassifyOrderBlockLifecycle(
                    true,
                    OrderBlockLifecycleRule.MinimumRetainedRatio) ==
                    OrderBlockLifecycleState.Broken,
                "Order Block lifecycle distinguishes fresh, mitigated and broken zones");

            Assert(
                OrderBlockRule.IsOnCorrectMarketSide(
                    1, 100, 95, 99, 0.01) &&
                !OrderBlockRule.IsOnCorrectMarketSide(
                    1, 100, 99, 101, 0.01) &&
                OrderBlockRule.IsOnCorrectMarketSide(
                    -1, 100, 101, 105, 0.01) &&
                !OrderBlockRule.IsOnCorrectMarketSide(
                    -1, 100, 99, 101, 0.01),
                "Order Block selection accepts only the intended side of market");

            Assert(
                OrderBlockLifecycleRule.IsOrderBlockFullyMitigated(1, 99, 103, 99) &&
                OrderBlockLifecycleRule.IsOrderBlockFullyMitigated(-1, 99, 103, 103) &&
                !OrderBlockLifecycleRule.IsOrderBlockFullyMitigated(
                    1, 99, 103, 99.01),
                "Order Block full-fill boundary is symmetric");

            Assert(
                !OrderBlockLifecycleRule.TryApplyOrderBlockPartialMitigation(
                    1, 99, 103, 99, 0.01,
                    out low, out high, out partial,
                    out remainingRatio,
                    out lifecycleState) &&
                lifecycleState ==
                    OrderBlockLifecycleState.Broken &&
                !OrderBlockLifecycleRule.TryApplyOrderBlockPartialMitigation(
                    -1, 99, 103, 103, 0.01,
                    out low, out high, out partial,
                    out remainingRatio,
                    out lifecycleState) &&
                lifecycleState ==
                    OrderBlockLifecycleState.Broken,
                "fully mitigated Order Blocks are explicitly broken and cannot remain active");

            Assert(
                OrderBlockLifecycleRule.IsOrderBlockAgeValid(10, 20, 10) &&
                !OrderBlockLifecycleRule.IsOrderBlockAgeValid(10, 21, 10) &&
                !OrderBlockLifecycleRule.IsOrderBlockAgeValid(21, 20, 10) &&
                !OrderBlockLifecycleRule.IsOrderBlockAgeValid(-1, 20, 10),
                "Order Block age is bounded, non-future and fail-closed");

            Assert(
                OrderBlockRule.OrderBlockIdentity(1, 42, false) ==
                OrderBlockRule.OrderBlockIdentity(1, 42, false) &&
                OrderBlockRule.OrderBlockIdentity(1, 42, false) !=
                OrderBlockRule.OrderBlockIdentity(1, 42, true) &&
                OrderBlockRule.OrderBlockIdentity(1, 42, false) !=
                OrderBlockRule.OrderBlockIdentity(-1, 42, false),
                "Order Block identity separates direction and zone geometry variant");

            Console.WriteLine(
                "CI-06 Order Block lifecycle contracts PASS");
        }

        private static void VerifyOrderBlockQualitySemantics()
        {
            int baseQuality =
                OrderBlockQualityRule.Calculate(
                    0.50,
                    0.00,
                    0.00,
                    0,
                    false,
                    false,
                    false,
                    false,
                    false);

            Assert(
                baseQuality == 54,
                "Order Block quality has one explicit base score");

            Assert(
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0, 0,
                    true, false, false, false, false) -
                baseQuality == 14 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0, 0,
                    false, true, false, false, false) -
                baseQuality == 13 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0, 0,
                    false, false, true, false, false) -
                baseQuality == 8 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0, 0,
                    false, false, false, true, false) -
                baseQuality == 8,
                "displacement, structure break, liquidity sweep and FVG confluence are independently traceable");

            Assert(
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 1.0, 0,
                    false, false, false, false, false) == 62 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 1.0, 0, 0,
                    false, false, false, false, false) == 56 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 1.50, 0, 0,
                    false, false, false, false, false) == 58,
                "remaining-width and impulse thresholds keep their explicit contributions");

            Assert(
                OrderBlockQualityRule.Calculate(
                    0.20, 0, 0, 0,
                    false, false, false, false, false) == 50 &&
                OrderBlockQualityRule.Calculate(
                    0.70, 0, 0, 0,
                    false, false, false, false, false) == 57 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0, 6,
                    false, false, false, false, false) == 53 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0.50, 0,
                    false, false, false, false, true) == 53,
                "body shape, age and partial-mitigation penalties are deterministic");

            Assert(
                OrderBlockQualityRule.Calculate(
                    0.70,
                    1.50,
                    1.0,
                    0,
                    true,
                    true,
                    true,
                    true,
                    false) == 100,
                "Order Block quality is explicitly clamped at the upper bound");

            Assert(
                OrderBlockQualityRule.Calculate(
                    double.NaN,
                    1.0,
                    1.0,
                    0,
                    false,
                    false,
                    false,
                    false,
                    false) == 0,
                "non-finite Order Block quality inputs fail closed");
        }

        private static void VerifyParallelScenarioSelectionSemantics()
        {
            TradeOpportunityCandidate m5Buy =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-M5-BUY",
                    SourceTimeframe = "M5",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 82,
                    Tp1RR = 2.1,
                    ActionableNow = true,
                    Entry = 100,
                    Risk = 2
                };

            TradeOpportunityCandidate h1Buy =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-H1-BUY",
                    SourceTimeframe = "H1",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 90,
                    Tp1RR = 2.5,
                    ActionableNow = true,
                    Entry = 100.5,
                    Risk = 2
                };

            Assert(
                ParallelScenarioSelectionRule.GetScenarioIdentity(m5Buy) ==
                "TF-M5-BUY" &&
                ParallelScenarioSelectionRule.GetScenarioIdentity(h1Buy) ==
                "TF-H1-BUY",
                "parallel scenario identity is explicit and timeframe-scoped");

            Assert(
                ParallelScenarioSelectionRule.CoverageKey(m5Buy) !=
                ParallelScenarioSelectionRule.CoverageKey(h1Buy),
                "different source timeframes remain independent coverage");

            TradeOpportunityCandidate stronger =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-M5-BUY",
                    SourceTimeframe = "M5",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 88,
                    Tp1RR = 2.1,
                    ActionableNow = true,
                    Entry = 100.01,
                    Risk = 2
                };

            Assert(
                ParallelScenarioSelectionRule.ShouldReplace(
                    m5Buy,
                    stronger,
                    0.10),
                "same-identity close-geometry candidate replaces only on higher display priority");

            TradeOpportunityCandidate olderGeometry =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-M5-BUY",
                    SourceTimeframe = "M5",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 99,
                    Quality = 95,
                    Tp1RR = 3.0,
                    ActionableNow = true,
                    Entry = 106,
                    Risk = 2
                };

            Assert(
                !ParallelScenarioSelectionRule.ShouldReplace(
                    stronger,
                    olderGeometry,
                    0.10),
                "older materially different geometry cannot replace the current scenario");

            TradeOpportunityCandidate newerGeometry =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-M5-BUY",
                    SourceTimeframe = "M5",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 101,
                    Quality = 75,
                    Tp1RR = 1.9,
                    ActionableNow = false,
                    Entry = 106,
                    Risk = 2
                };

            Assert(
                ParallelScenarioSelectionRule.ShouldReplace(
                    stronger,
                    newerGeometry,
                    0.10),
                "newer materially different geometry replaces stale same-identity state");

            IReadOnlyList<TradeOpportunityCandidate> selected =
                ParallelScenarioSelectionRule.SelectForDisplay(
                    new[]
                    {
                        m5Buy,
                        h1Buy,
                        new TradeOpportunityCandidate
                        {
                            ScenarioId = "TF-M5-SELL",
                            SourceTimeframe = "M5",
                            Lane = OpportunityLane.Tactical,
                            Direction = -1,
                            CreatedM5 = 100,
                            Quality = 80,
                            Tp1RR = 2.2,
                            ActionableNow = true,
                            Entry = 100,
                            Risk = 2
                        }
                    },
                    2);

            Assert(
                selected.Count == 2 &&
                ParallelScenarioSelectionRule.CoverageKey(selected[0]) !=
                ParallelScenarioSelectionRule.CoverageKey(selected[1]),
                "display selection preserves distinct scenario coverage under a visible-count limit");

            Assert(
                ParallelScenarioSelectionRule.SelectForDisplay(
                    new[]
                    {
                        m5Buy,
                        h1Buy
                    },
                    8).Count == 2,
                "display selection retains all independent scenarios below the limit");
        }

        private static void VerifyMicroReactionClosedBarSemantics()
        {
            Assert(
                MicroReactionSafetyRule.IsClosedBarSafe(
                    100,
                    100,
                    1,
                    1,
                    82,
                    true,
                    80),
                "MicroReaction accepts an exact matching confirmed closed bar");

            Assert(
                !MicroReactionSafetyRule.IsClosedBarSafe(
                    100,
                    99,
                    1,
                    1,
                    95,
                    true,
                    80) &&
                !MicroReactionSafetyRule.IsClosedBarSafe(
                    100,
                    100,
                    1,
                    -1,
                    95,
                    true,
                    80) &&
                !MicroReactionSafetyRule.IsClosedBarSafe(
                    100,
                    100,
                    1,
                    1,
                    79,
                    true,
                    80) &&
                !MicroReactionSafetyRule.IsClosedBarSafe(
                    100,
                    100,
                    1,
                    1,
                    95,
                    false,
                    80),
                "MicroReaction rejects stale bars, direction mismatch, weak confirmed quality and unconfirmed state");

            Assert(
                !MicroReactionSafetyRule.IsClosedBarSafe(
                    100,
                    100,
                    0,
                    0,
                    100,
                    true,
                    80),
                "neutral reaction direction fails closed");
        }

        private static void VerifyMtfContextIntegrity()
        {
            DateTime reference =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    5,
                    0,
                    DateTimeKind.Utc);

            MtfClosedContext context =
                new MtfClosedContext(
                    reference,
                    101,
                    605,
                    41,
                    31,
                    31,
                    30,
                    3,
                    1);

            Assert(
                context.Reference == reference,
                "MTF reference");

            Assert(
                context.M5 == 101 &&
                context.M1 == 605 &&
                context.M15 == 41 &&
                context.M30 == 31 &&
                context.H1 == 31 &&
                context.H4 == 30 &&
                context.D1 == 3 &&
                context.W1 == 1,
                "MTF indices preserved");

            Assert(
                context.HasPrimaryDecisionHistory,
                "primary MTF history");

            MtfClosedContext incomplete =
                new MtfClosedContext(
                    reference,
                    29,
                    605,
                    41,
                    21,
                    11,
                    8,
                    3,
                    1);

            Assert(
                !incomplete.HasPrimaryDecisionHistory,
                "insufficient closed history blocked");
        }

        private static void VerifyCi08DivergenceWaveTrendReactionEarlySignal()
        {
            Assert(
                DivergenceThresholdRule.StrongConflictQuality == 70 &&
                DivergenceThresholdRule.MeetsStrongConflictQuality(70) &&
                !DivergenceThresholdRule.MeetsStrongConflictQuality(69),
                "CI-08 divergence strong-conflict threshold has one canonical owner");

            Assert(
                WaveTrendEvidenceRule.NormalizeMinimumQuality(30) == 40 &&
                WaveTrendEvidenceRule.NormalizeMinimumQuality(58) == 58 &&
                WaveTrendEvidenceRule.NormalizeMinimumQuality(120) == 100,
                "CI-08 WaveTrend minimum evidence quality normalizes deterministically");

            double positiveFlow;
            double negativeFlow;

            Assert(
                WaveTrendMoneyFlowRule.TryCalculateContribution(
                    105,
                    100,
                    10,
                    out positiveFlow,
                    out negativeFlow) &&
                positiveFlow == 1050 &&
                negativeFlow == 0,
                "CI-08 WaveTrend MFI assigns rising-price flow to the positive bucket");

            Assert(
                WaveTrendMoneyFlowRule.TryCalculateContribution(
                    95,
                    100,
                    10,
                    out positiveFlow,
                    out negativeFlow) &&
                positiveFlow == 0 &&
                negativeFlow == 950,
                "CI-08 WaveTrend MFI assigns falling-price flow to the negative bucket");

            Assert(
                WaveTrendMoneyFlowRule.TryCalculateContribution(
                    105,
                    100,
                    0,
                    out positiveFlow,
                    out negativeFlow) &&
                positiveFlow == 0 &&
                negativeFlow == 0,
                "CI-08 zero tick volume contributes no synthetic money flow");

            Assert(
                !WaveTrendMoneyFlowRule.TryCalculateContribution(
                    105,
                    100,
                    -1,
                    out positiveFlow,
                    out negativeFlow),
                "CI-08 negative tick volume fails closed");
