using System;
using System.Collections.Generic;
using System.IO;
using CFIP.Contracts;

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
            VerifyWaveTrendMathematics();
            Ci20BProtectionAndSignalContracts.Run();
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
            VerifyBrokerConfirmedProtectionStateSync();
            VerifyTargetProgression();
            VerifyExecutionCapacity();
            VerifyStaleLivePlanRecovery();
            VerifyLifecycleFlows();
            VerifyLifecycleIdempotency();
            VerifyRuntimeStageIsolation();
            VerifyRuntimeFaultStateMachine();
            VerifyRuntimeExplicitRearmSemantics();
            VerifyAlertDeliveryQueueSemantics();
            AlertDeliveryQueueContracts.Run();
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
            M4TimeHistoryContracts.Run();
            M5PanelContracts.Run();
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
            VerifyRetestTriggerModeSemantics();
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

            IndicatorEvidenceFusionResult configuredWaveTrend =
                IndicatorEvidenceFusionRule.Evaluate(
                    new IndicatorEvidenceFusionInput(
                        "UNKNOWN",
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false, false, false,
                        6, 25, 20,
                        50, 0, 0,
                        1, 50, 40,
                        0, 0,
                        false, false,
                        false, false,
                        0, 0, 0, 4, 3));

            IndicatorEvidenceFusionResult rejectedWaveTrend =
                IndicatorEvidenceFusionRule.Evaluate(
                    new IndicatorEvidenceFusionInput(
                        "UNKNOWN",
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false, false, false,
                        6, 25, 20,
                        50, 0, 0,
                        1, 50, 58,
                        0, 0,
                        false, false,
                        false, false,
                        0, 0, 0, 4, 3));

            Assert(
                configuredWaveTrend.BullBonus > 0 &&
                rejectedWaveTrend.BullBonus == 0,
                "CI-08 WaveTrend fusion respects configured minimum quality instead of a hidden 58 floor");

            int configuredGroups =
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(
                    new IndicatorEvidenceFusionInput(
                        "UNKNOWN",
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false, false, false,
                        6, 25, 20,
                        50, 0, 0,
                        1, 50, 40,
                        0, 0,
                        false, false,
                        false, false,
                        0, 0, 0, 4, 3));

            int blockedGroups =
                IndicatorEvidenceIndependenceRule.CountIndicatorGroups(
                    new IndicatorEvidenceFusionInput(
                        "UNKNOWN",
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false, false, false,
                        6, 25, 20,
                        50, 0, 0,
                        1, 50, 58,
                        0, 0,
                        false, false,
                        false, false,
                        0, 0, 0, 4, 3));

            Assert(
                configuredGroups == 1 &&
                blockedGroups == 0,
                "CI-08 WaveTrend indicator-group provenance uses the same configured quality floor");

            Assert(
                ReactionTimingRule.IsSeparatedObservationAndConfirmation(100, 99) &&
                !ReactionTimingRule.IsSeparatedObservationAndConfirmation(100, 100) &&
                !ReactionTimingRule.IsSeparatedObservationAndConfirmation(99, 100),
                "CI-08 live reaction and closed confirmation remain temporally separated");
        }

        private static void VerifyCi07MarketStateSemantics()
        {
            DateTime reference =
                new DateTime(
                    2026,
                    10,
                    2,
                    12,
                    5,
                    30,
                    DateTimeKind.Utc);

            MarketStateFrameSnapshot m1 =
                new MarketStateFrameSnapshot(
                    "M1", 605, 1, 81, "TREND", 76, 3,
                    "TREND", "STABLE", 38, 1.05, 0.32, 0.10, 0.62);
            MarketStateFrameSnapshot m5 =
                new MarketStateFrameSnapshot(
                    "M5", 101, 1, 86, "EXPANSION", 82, 1,
                    "TREND", "CHANGED", 42, 1.31, 0.44, 0.18, 0.71);
            MarketStateFrameSnapshot m15 =
                new MarketStateFrameSnapshot(
                    "M15", 41, 1, 78, "TREND", 74, 3,
                    "TREND", "STABLE", 35, 1.08, 0.40, 0.12, 0.68);
            MarketStateFrameSnapshot m30 =
                new MarketStateFrameSnapshot(
                    "M30", 31, 1, 73, "TREND", 70, 3,
                    "EXPANSION", "CHANGED", 34, 1.16, 0.47, 0.14, 0.64);
            MarketStateFrameSnapshot h1 =
                new MarketStateFrameSnapshot(
                    "H1", 31, 1, 69, "TREND", 68, 3,
                    "TREND", "STABLE", 31, 1.02, 0.51, 0.09, 0.66);
            MarketStateFrameSnapshot h4 =
                new MarketStateFrameSnapshot(
                    "H4", 30, 1, 71, "TREND", 72, 3,
                    "TREND", "STABLE", 29, 0.98, 0.55, 0.08, 0.63);
            MarketStateFrameSnapshot d1 =
                new MarketStateFrameSnapshot(
                    "D1", 3, 0, 55, "RANGE", 51, 2,
                    "RANGE", "STABLE", 58, 0.93, 0.12, 0.01, 0.31);
            MarketStateFrameSnapshot w1 =
                new MarketStateFrameSnapshot(
                    "W1", 1, 0, 48, "UNKNOWN", 0, 0,
                    "UNKNOWN", "UNKNOWN", 0, 1.0, 0, 0, 0);

            MarketStateSnapshot state =
                new MarketStateSnapshot(
                    reference,
                    m1, m5, m15, m30, h1, h4, d1, w1,
                    1,
                    true);

            Assert(
                state.MatchesReference(reference) &&
                !state.MatchesReference(reference.AddSeconds(1)),
                "CI-07 snapshot preserves exact reference identity");

            Assert(
                state.IsAlignedWithClosedIndices(
                    605, 101, 41, 31, 31, 30, 3, 1),
                "CI-07 snapshot preserves all eight canonical MTF indices");

            Assert(
                !state.IsAlignedWithClosedIndices(
                    605, 100, 41, 31, 31, 30, 3, 1),
                "CI-07 snapshot rejects a future/misaligned M5 index");

            Assert(
                state.M5.Regime == "EXPANSION" &&
                state.M5.PreviousRegime == "TREND" &&
                state.M5.RegimeTransition == "CHANGED" &&
                state.PremiumDiscountBias == 1 &&
                state.SessionOpen,
                "CI-07 snapshot carries regime transition, premium/discount and session context");

            Assert(
                MarketRegimeTransitionRule.ClassifyTransition("TREND", "TREND") ==
                    MarketRegimeTransitionRule.Stable &&
                MarketRegimeTransitionRule.ClassifyTransition("TREND", "RANGE") ==
                    MarketRegimeTransitionRule.Changed &&
                MarketRegimeTransitionRule.ClassifyTransition(null, "TREND") ==
                    MarketRegimeTransitionRule.Initial &&
                MarketRegimeTransitionRule.ClassifyTransition("TREND", "bad") ==
                    MarketRegimeTransitionRule.Unknown,
                "CI-07 regime transition classification is deterministic and fail-closed");

            MarketStateFrameSnapshot normalized =
                new MarketStateFrameSnapshot(
                    "m5", 101, 9, 120, "bad", 120, 9,
                    "bad", null, double.NaN, double.PositiveInfinity,
                    double.NaN, double.NaN, double.NaN);

            Assert(
                normalized.Timeframe == "M5" &&
                normalized.Direction == 0 &&
                normalized.Quality == 100 &&
                normalized.Regime == MarketRegimeIdentity.Unknown &&
                normalized.RegimeQuality == 100 &&
                normalized.RegimeStability == 3 &&
                normalized.PreviousRegime == MarketRegimeIdentity.Unknown &&
                normalized.RegimeTransition == MarketRegimeTransitionRule.Unknown,
                "CI-07 frame snapshot normalizes invalid state without inventing direction");
        }

        private static void VerifyCanonicalMarketContext()
        {
            DateTime signalReference =
                Utc(12, 5);

            DateTime quoteObserved =
                Utc(12, 5);

            CanonicalPriceSnapshot pipPrice =
                CanonicalPriceSnapshot.CreateCanonicalSnapshot(
                    quoteObserved,
                    100.0000,
                    100.0002,
                    0.0001,
                    0.00001,
                    5,
                    BrokerDistanceUnit.Pips,
                    3,
                    4);

            Assert(
                pipPrice.IsQuoteValid &&
                pipPrice.Bid == 100.0000 &&
                pipPrice.Ask == 100.0002 &&
                pipPrice.ExecutableBuyPrice == 100.0002 &&
                pipPrice.ExecutableSellPrice == 100.0000,
                "canonical quote preserves BUY/SELL executable prices");

            Assert(
                Math.Abs(pipPrice.Midpoint - 100.0001) < 1e-10 &&
                Math.Abs(pipPrice.Spread - 0.0002) < 1e-10 &&
                Math.Abs(pipPrice.SpreadPips - 2.0) < 1e-10,
                "canonical quote derives midpoint and spread once");

            Assert(
                Math.Abs(
                    pipPrice.GetMinimumStopDistancePrice(1) -
                    0.0003) < 1e-12 &&
                Math.Abs(
                    pipPrice.GetMinimumTakeProfitDistancePrice(-1) -
                    0.0004) < 1e-12,
                "pip broker distances convert to price distance consistently");

            CanonicalPriceSnapshot percentagePrice =
                CanonicalPriceSnapshot.CreateCanonicalSnapshot(
                    quoteObserved,
                    100.0,
                    100.5,
                    0.1,
                    0.01,
                    2,
                    BrokerDistanceUnit.Percentage,
                    0.1,
                    0.2);

            Assert(
                percentagePrice.HasBrokerDistanceMetadata &&
                Math.Abs(
                    percentagePrice.GetMinimumStopDistancePrice(1) -
                    0.1005) < 1e-12 &&
                Math.Abs(
                    percentagePrice.GetMinimumTakeProfitDistancePrice(-1) -
                    0.2) < 1e-12,
                "percentage broker distances use the executable reference price");

            CanonicalPriceSnapshot invalidQuote =
                CanonicalPriceSnapshot.CreateCanonicalSnapshot(
                    quoteObserved,
                    100.5,
                    100.0,
                    0.1,
                    0.01,
                    2,
                    BrokerDistanceUnit.Pips,
                    1,
                    1);

            Assert(
                !invalidQuote.IsQuoteValid &&
                invalidQuote.GetExecutablePrice(1) == 0 &&
                invalidQuote.GetExecutablePrice(-1) == 0,
                "invalid crossed quote cannot become an executable price");

            MtfClosedContext closedContext =
                new MtfClosedContext(
                    signalReference,
                    101,
                    605,
                    41,
                    31,
                    31,
                    30,
                    3,
                    1);

            CalculationMarketContext calculationContext =
                new CalculationMarketContext(
                    999,
                    signalReference,
                    quoteObserved,
                    pipPrice,
                    closedContext);

            Assert(
                calculationContext.HostIndex == 999 &&
                calculationContext.SignalReferenceUtc == signalReference &&
                calculationContext.QuoteObservedUtc == quoteObserved &&
                calculationContext.AtrM5Index == 101 &&
                calculationContext.AtrM1Index == 605 &&
                ReferenceEquals(
                    calculationContext.Price,
                    pipPrice) &&
                ReferenceEquals(
                    calculationContext.ClosedBars,
                    closedContext),
                "calculation context keeps price, time and canonical MTF indices together");

            Console.WriteLine(
                "CI-00 canonical market context contract PASS");
        }

        private static void VerifyClosedBarReferenceContract()
        {
            DateTime[] opens =
            {
                Utc(12, 0),
                Utc(12, 5),
                Utc(12, 15),
                Utc(12, 20)
            };

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 4),
                    index => opens[index]) == -1,
                "no closed bar before first boundary");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 5),
                    index => opens[index]) == 0,
                "exact boundary closes prior bar");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 14),
                    index => opens[index]) == 0,
                "between boundaries keeps prior bar closed");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 15),
                    index => opens[index]) == 1,
                "gap boundary uses next actual open time");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 21),
                    index => opens[index]) == 2,
                "latest available fully closed bar");

            Assert(
                ClosedBarReferenceRule.IsFullyClosed(
                    opens.Length,
                    1,
                    Utc(12, 15),
                    index => opens[index]),
                "resolved bar is fully closed at reference");

            Assert(
                !ClosedBarReferenceRule.IsFullyClosed(
                    opens.Length,
                    2,
                    Utc(12, 19),
                    index => opens[index]),
                "future bar cannot be considered closed");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 30),
                    index => opens[index]) == 2,
                "reference after last open is bounded to last closed bar");
        }

        private static DateTime Utc(int hour, int minute)
        {
            return new DateTime(
                2026,
                1,
                1,
                hour,
                minute,
                0,
                DateTimeKind.Utc);
        }

        private static void VerifyMarketExecutionAcceptance()
        {
            Assert(
                BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    true,
                    true),
                "confirmed market position");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    false,
                    true),
                "rejected market position");
        }

        private static void VerifyPendingOrderAcceptance()
        {
            Assert(
                BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    true,
                    true),
                "confirmed pending order");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    false,
                    true),
                "rejected pending order");
        }

        private static void VerifyRejectedMutationHandling()
        {
            Assert(
                !BrokerConfirmationPolicy.IsSuccessfulMutation(
                    false,
                    true),
                "missing mutation result");

            Assert(
                !BrokerConfirmationPolicy.IsSuccessfulMutation(
                    true,
                    false),
                "unsuccessful mutation");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    true,
                    false),
                "missing confirmed position entity");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    true,
                    false),
                "missing confirmed pending entity");
        }

        private static void VerifyFillEnvelopeSymmetry()
        {
            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    1,
                    100,
                    102,
                    10,
                    0.25,
                    true),
                "BUY-side fill inside envelope");

            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    -1,
                    100,
                    98,
                    10,
                    0.25,
                    true),
                "SELL-side mirrored fill inside envelope");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    1,
                    100,
                    103,
                    10,
                    0.25,
                    true),
                "BUY-side fill outside envelope");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    -1,
                    100,
                    97,
                    10,
                    0.25,
                    true),
                "SELL-side mirrored fill outside envelope");
        }

        private static void VerifyInitialProtectionDirectionality()
        {
            Assert(
                PriceProtectionRule.ValidateStop(
                    1,
                    100,
                    98,
                    1),
                "BUY initial stop");

            Assert(
                PriceProtectionRule.ValidateTarget(
                    1,
                    100,
                    102,
                    1),
                "BUY initial target");

            Assert(
                PriceProtectionRule.ValidateStop(
                    -1,
                    100,
                    102,
                    1),
                "SELL initial stop");

            Assert(
                PriceProtectionRule.ValidateTarget(
                    -1,
                    100,
                    98,
                    1),
                "SELL initial target");
        }

        private static void VerifyManagedBreakEvenDirectionality()
        {
            Assert(
                ManagedStopProtectionRule.Validate(
                    1,
                    100,
                    104,
                    102,
                    1),
                "BUY profit-lock stop");

            Assert(
                !ManagedStopProtectionRule.Validate(
                    1,
                    100,
                    104,
                    106,
                    1),
                "BUY stop beyond market");

            Assert(
                ManagedStopProtectionRule.Validate(
                    -1,
                    100,
                    96,
                    98,
                    1),
                "SELL profit-lock stop");

            Assert(
                !ManagedStopProtectionRule.Validate(
                    -1,
                    100,
                    96,
                    94,
                    1),
                "SELL stop beyond market");
        }

        private static void VerifyProtectionProgression()
        {
            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    100,
                    101),
                "BUY SL advances upward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    101,
                    100),
                "BUY SL backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    100,
                    99),
                "SELL SL advances downward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    99,
                    100),
                "SELL SL backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    105,
                    106,
                    true),
                "BUY TP advances forward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    106,
                    105,
                    true),
                "BUY TP backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    95,
                    94,
                    true),
                "SELL TP advances forward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    94,
                    95,
                    true),
                "SELL TP backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    106,
                    105,
                    false),
                "TP policy can explicitly allow backward move");
        }

        private static void VerifyBrokerConfirmedProtectionStateSync()
        {
            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    100,
                    101) &&
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    101,
                    100) &&
                ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    100,
                    99) &&
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    99,
                    100),
                "broker-confirmed BUY/SELL stop adoption is monotonic");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    105,
                    106,
                    true) &&
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    106,
                    105,
                    true) &&
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    95,
                    94,
                    true) &&
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    94,
                    95,
                    true),
                "broker-confirmed BUY/SELL target state is monotonic");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    0,
                    100,
                    101) &&
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    0,
                    100,
                    101,
                    true),
                "invalid broker protection direction fails closed");

            Console.WriteLine(
                "CBOT-P8 broker-confirmed protection state sync contracts PASS");
        }

        private static void VerifyTargetProgression()
        {
            Assert(
                TargetProgressionRule.IsValid(
                    1,
                    102,
                    105),
                "BUY target progression");

            Assert(
                TargetProgressionRule.IsValid(
                    -1,
                    98,
                    95),
                "SELL target progression");

            Assert(
                !TargetProgressionRule.IsValid(
                    1,
                    105,
                    102),
                "BUY backward target blocked");

            Assert(
                !TargetProgressionRule.IsValid(
                    -1,
                    95,
                    98),
                "SELL backward target blocked");
        }

        private static void VerifyExecutionCapacity()
        {
            Assert(
                ExecutionCapacityRule.IsSupportedSinglePlanCapacity(1),
                "single-plan capacity 1 is supported");

            Assert(
                !ExecutionCapacityRule.IsSupportedSinglePlanCapacity(2),
                "multi-position capacity is not supported");

            Assert(
                ExecutionCapacityRule.AllowsNewSinglePlan(
                    1,
                    false,
                    0,
                    0),
                "new single plan is allowed with empty broker capacity");

            Assert(
                !ExecutionCapacityRule.AllowsNewSinglePlan(
                    1,
                    true,
                    0,
                    0),
                "new plan is blocked by an existing local plan");

            Assert(
                !ExecutionCapacityRule.AllowsNewSinglePlan(
                    1,
                    false,
                    1,
                    0),
                "new plan is blocked by an existing managed position");

            Assert(
                !ExecutionCapacityRule.AllowsNewSinglePlan(
                    1,
                    false,
                    0,
                    1),
                "new plan is blocked by an existing managed pending order");

            Assert(
                ExecutionCapacityRule.AllowsNewSingleExecution(
                    1,
                    0,
                    0),
                "new broker execution is allowed with empty capacity");

            Assert(
                !ExecutionCapacityRule.AllowsNewSingleExecution(
                    1,
                    1,
                    0),
                "new broker execution is blocked by a managed position");

            Assert(
                !ExecutionCapacityRule.AllowsNewSingleExecution(
                    1,
                    0,
                    1),
                "new broker execution is blocked by a managed pending order");

            Assert(
                !ExecutionCapacityRule.AllowsNewSingleExecution(
                    2,
                    0,
                    0),
                "unsupported configured capacity blocks broker execution");
        }

        private static void VerifyStaleLivePlanRecovery()
        {
            Assert(
                LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    true,
                    false),
                "stale live plan clears when broker position disappears");

            Assert(
                !LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    true,
                    true),
                "live plan remains when broker position exists");

            Assert(
                !LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    false,
                    false),
                "non-live plan is not cleared by broker absence");
        }

        private static void VerifyLifecycleFlows()
        {
            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Flat,
                    LifecycleState.Signal),
                "signal entry");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Signal,
                    LifecycleState.PlanReady),
                "plan readiness");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.PlanReady,
                    LifecycleState.ExecutionReady),
                "execution readiness");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExecutionReady,
                    LifecycleState.LivePosition),
                "market fill flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExecutionReady,
                    LifecycleState.PendingOrder),
                "pending placement flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.PendingOrder,
                    LifecycleState.LivePosition),
                "pending fill flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.LivePosition,
                    LifecycleState.RecoveryRequired),
                "live recovery flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.RecoveryRequired,
                    LifecycleState.LivePosition),
                "recovery completion flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.LivePosition,
                    LifecycleState.ExitRequested),
                "reversal/invalidation exit");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExitRequested,
                    LifecycleState.Closed),
                "end-of-day/exit close");

            Assert(
                !LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Closed,
                    LifecycleState.PendingOrder),
                "closed state cannot create pending order");
        }

        private static void VerifyRuntimeStageIsolation()
        {
            string cyclePath =
                Path.Combine(
                    "src",
                    "CFIP.Indicator",
                    "Runtime",
                    "Calculation",
                    "CalculationCycle.cs");

            string stagesPath =
                Path.Combine(
                    "src",
                    "CFIP.Indicator",
                    "Runtime",
                    "Calculation",
                    "CalculationStageIsolation.cs");

            Assert(
                File.Exists(cyclePath),
                "calculation cycle source exists");

            Assert(
                File.Exists(stagesPath),
                "calculation stage isolation source exists");

            string cycle =
                File.ReadAllText(
                    cyclePath);

            string stages =
                File.ReadAllText(
                    stagesPath);

            foreach (string requiredCall in new[]
            {
                "RunCalculationPreparationStage(",
                "RunClosedBarAnalysisStage(",
                "ProcessLiveCalculationStages("
            })
            {
                Assert(
                    cycle.Contains(requiredCall),
                    "Calculate uses " + requiredCall.Trim('(', ' '));
            }

            Assert(
                !cycle.Contains("ProcessNewClosedBar("),
                "Calculate does not directly own closed-bar analysis");

            Assert(
                !cycle.Contains("ProcessLiveCalculation("),
                "Calculate does not directly own live-cycle orchestration");

            foreach (string requiredStage in new[]
            {
                "BROKER RECONCILIATION • PREFLIGHT",
                "BROKER LIFECYCLE RECOVERY",
                "BROKER RECONCILIATION • POST-RECOVERY",
                "ACTIVE PLAN MANAGEMENT",
                "BROKER PROTECTION • PRE-ANALYSIS",
                "LIVE ANALYSIS",
                "PLAN SYNCHRONIZATION",
                "PLAN CREATION",
                "EXECUTION",
                "BROKER RECONCILIATION • POST-EXECUTION",
                "BROKER PROTECTION • POST-EXECUTION",
                "TELEMETRY",
                "REVERSAL MANAGEMENT",
                "BROKER STATE FINALIZATION",
                "PRESENTATION"
            })
            {
                Assert(
                    stages.Contains(requiredStage),
                    "isolated stage " + requiredStage);
            }

            Assert(
                stages.Contains("HandleRuntimeFault("),
                "stage fault containment");

            Assert(
                stages.Contains("return true;"),
                "stage fault continues to next stage");

            Assert(
                stages.IndexOf(
                    "BROKER RECONCILIATION • PREFLIGHT",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "broker reconciliation precedes live analysis");

            Assert(
                stages.IndexOf(
                    "BROKER LIFECYCLE RECOVERY",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "broker recovery precedes live analysis");

            Assert(
                stages.IndexOf(
                    "ACTIVE PLAN MANAGEMENT",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "active plan management precedes live analysis");

            Assert(
                stages.IndexOf(
                    "BROKER PROTECTION • PRE-ANALYSIS",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "pre-analysis protection precedes live analysis");

            Assert(
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "PLAN SYNCHRONIZATION",
                    StringComparison.Ordinal),
                "analysis stage precedes downstream planning");

            Assert(
                stages.IndexOf(
                    "PLAN SYNCHRONIZATION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "EXECUTION",
                    StringComparison.Ordinal),
                "planning synchronization precedes execution");

            Assert(
                stages.IndexOf(
                    "BROKER RECONCILIATION • POST-EXECUTION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "BROKER PROTECTION • POST-EXECUTION",
                    StringComparison.Ordinal),
                "post-execution reconciliation precedes protection");

            Assert(
                stages.IndexOf(
                    "BROKER PROTECTION • POST-EXECUTION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "TELEMETRY",
                    StringComparison.Ordinal),
                "post-execution protection precedes telemetry");

            Assert(
                stages.IndexOf(
                    "BROKER STATE FINALIZATION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "PRESENTATION",
                    StringComparison.Ordinal),
                "final broker state synchronization precedes presentation");
        }

        private static void VerifyLifecycleIdempotency()
        {
            LifecycleEventIdempotencyGuard guard =
                new LifecycleEventIdempotencyGuard();

            Assert(
                guard.TryBegin("POSITION_OPENED", 501),
                "first open event");

            Assert(
                !guard.TryBegin("POSITION_OPENED", 501),
                "duplicate open event");

            Assert(
                guard.TryBegin("PENDING_CREATED", 501),
                "different event type");

            Assert(
                guard.TryBegin("POSITION_OPENED", 502),
                "different entity");

            Assert(
                !guard.TryBegin("POSITION_OPENED", 0),
                "invalid entity");
        }

        private static void VerifyRuntimeFaultStateMachine()
        {
            RuntimeFaultStateMachine machine =
                new RuntimeFaultStateMachine();

            machine.BeginCycle();
            machine.ObserveAutoTradingSetting(true);

            Assert(
                machine.State == RuntimeFaultState.Healthy,
                "initial runtime state healthy");

            Assert(
                machine.CanAutomaticEntryProceed,
                "initial automatic entry armed");

            machine.RecordRecoverableFault();

            Assert(
                machine.State == RuntimeFaultState.Degraded,
                "recoverable fault enters degraded");

            machine.BlockAutomaticEntry();

            Assert(
                machine.State == RuntimeFaultState.EntryBlocked,
                "fault blocks automatic entry");

            Assert(
                !machine.CanAutomaticEntryProceed,
                "entry remains blocked after fault");

            machine.BeginCycle();
            machine.MarkManagementReadyForRecovery();

            Assert(
                machine.State == RuntimeFaultState.Recovering,
                "healthy management enters recovery");

            Assert(
                !machine.CanAutomaticEntryProceed,
                "recovery does not re-arm entry");

            machine.CompleteCycle();

            Assert(
                machine.State == RuntimeFaultState.Healthy,
                "clean recovery returns healthy");

            Assert(
                !machine.CanAutomaticEntryProceed,
                "healthy recovery remains disarmed");

            machine.ObserveAutoTradingSetting(false);
            machine.ObserveAutoTradingSetting(true);

            Assert(
                machine.CanAutomaticEntryProceed,
                "explicit enable transition re-arms entry");

            machine.BeginCycle();
            machine.RecordRecoverableFault();
            machine.BlockAutomaticEntry();

            machine.ObserveAutoTradingSetting(false);
            machine.ObserveAutoTradingSetting(true);

            Assert(
                !machine.CanAutomaticEntryProceed,
                "enable transition cannot bypass blocked state");
        }

        private static void VerifyClosedBarRetryPolicy()
        {
            RuntimeFaultStateMachine machine =
                new RuntimeFaultStateMachine();

            DateTime t =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            string reason;

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    101,
                    t,
                    out reason),
                "first closed-bar attempt allowed");

            machine.RecordClosedBarAnalysisFailure(
                101,
                t);

            Assert(
                machine.ClosedBarFailureCount == 1,
                "first closed-bar failure counted");

            Assert(
                machine.ClosedBarLastFailureUtc == t,
                "failure timestamp recorded");

            Assert(
                !machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddMilliseconds(500),
                    out reason),
                "first closed-bar retry is backed off");

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddSeconds(1),
                    out reason),
                "first closed-bar retry becomes eligible");

            machine.RecordClosedBarAnalysisFailure(
                101,
                t.AddSeconds(1));

            Assert(
                machine.ClosedBarFailureCount == 2,
                "second closed-bar failure counted");

            Assert(
                !machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddSeconds(2),
                    out reason),
                "second retry uses longer backoff");

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    102,
                    t.AddSeconds(2),
                    out reason),
                "new closed bar is not blocked by prior bar failure");

            Assert(
                machine.HasStaleClosedBarFailure(102),
                "prior closed-bar failure is detectable as stale");

            machine.RecordClosedBarAnalysisFailure(
                101,
                t.AddSeconds(3));

            machine.RecordClosedBarAnalysisFailure(
                101,
                t.AddSeconds(7));

            Assert(
                machine.ClosedBarFailureCount == 4,
                "fourth closed-bar failure counted");

            Assert(
                !machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddSeconds(8),
                    out reason),
                "repeated closed-bar failure opens retry circuit");

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    102,
                    t.AddSeconds(8),
                    out reason),
                "new closed bar bypasses stale retry circuit");

            machine.RecordClosedBarAnalysisSuccess(101);

            Assert(
                machine.ClosedBarFailureCount == 0,
                "successful retry clears failure state");

            Assert(
                machine.ClosedBarLastFailureUtc == DateTime.MinValue,
                "successful retry clears failure timestamp");
        }

        private static void VerifyUnifiedSubmissionGate()
        {
            SubmissionGate gate =
                new SubmissionGate();

            DateTime t =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            SubmissionAttemptIdentity firstSignal =
                new SubmissionAttemptIdentity(
                    "EURUSD|100|1",
                    "AutomaticMarket|100|1",
                    ExecutionSubmissionPath.AutomaticMarket);

            SubmissionAttemptIdentity secondSignal =
                new SubmissionAttemptIdentity(
                    "EURUSD|101|1",
                    "AutomaticMarket|101|1",
                    ExecutionSubmissionPath.AutomaticMarket);

            SubmissionAttemptIdentity otherPath =
                new SubmissionAttemptIdentity(
                    "EURUSD|100|1",
                    "PendingStop|100|1",
                    ExecutionSubmissionPath.PendingStop);

            string reason;

            Assert(
                gate.TryAcquire(
                    firstSignal,
                    t,
                    out reason),
                "first submission attempt allowed");

            gate.Record(
                firstSignal,
                t,
                false);

            Assert(
                !gate.TryAcquire(
                    firstSignal,
                    t.AddMilliseconds(500),
                    out reason),
                "failed signal enters backoff");

            Assert(
                gate.TryAcquire(
                    secondSignal,
                    t.AddMilliseconds(500),
                    out reason),
                "different signal remains independently eligible");

            Assert(
                gate.TryAcquire(
                    otherPath,
                    t.AddMilliseconds(500),
                    out reason),
                "different execution path remains independently eligible");

            Assert(
                gate.TryAcquire(
                    firstSignal,
                    t.AddSeconds(1),
                    out reason),
                "first retry becomes eligible after backoff");

            for (int i = 0; i < 3; i++)
            {
                gate.Record(
                    firstSignal,
                    t.AddSeconds(2 + i * 2),
                    false);
            }

            Assert(
                !gate.TryAcquire(
                    firstSignal,
                    t.AddSeconds(10),
                    out reason),
                "repeated same-attempt failures open circuit");

            Assert(
                gate.TryAcquire(
                    secondSignal,
                    t.AddSeconds(10),
                    out reason),
                "open circuit is isolated to the failing attempt");

            gate.Record(
                firstSignal,
                t.AddSeconds(70),
                true);

            Assert(
                gate.TryAcquire(
                    firstSignal,
                    t.AddSeconds(70),
                    out reason),
                "successful submission clears retry state");

            Assert(
                firstSignal.CanonicalKey !=
                secondSignal.CanonicalKey &&
                firstSignal.CanonicalKey !=
                otherPath.CanonicalKey,
                "submission identities remain distinct");
        }
        private static void VerifyVisualAndExecutionControls()
        {
            string snapshotPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "SignalVisualSnapshot.cs");
            string previewPath = Path.Combine("src", "CFIP.Indicator", "Core", "Models", "TradeSetupPreview.cs");
            string previewBuilderPath = Path.Combine("src", "CFIP.Indicator", "Planning", "TradePlan", "PlanPreviewBuilder.cs");
            string calculationPath = Path.Combine("src", "CFIP.Indicator", "Runtime", "Calculation", "CalculationLiveCycle.cs");
            string calculationStagePath = Path.Combine("src", "CFIP.Indicator", "Runtime", "Calculation", "CalculationStageIsolation.cs");
            string rendererPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "PlanRenderCoordinator.cs");
            string pendingRendererPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "PendingOrderRenderer.cs");
            string alertRendererPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "AlertSignalRenderer.cs");
            string alertEnginePath = Path.Combine("src", "CFIP.Indicator", "Trading", "Alerts", "AlertEngine.cs");
            string statePath = Path.Combine("src", "CFIP.Indicator", "Indicator", "State.cs");
            string predictiveSelectorPath = Path.Combine("src", "CFIP.Indicator", "Planning", "Execution", "PredictivePendingLevelSelector.cs");
            string predictiveCollectorPath = Path.Combine("src", "CFIP.Indicator", "Planning", "Execution", "PredictivePendingZoneCollector.cs");
            string predictiveScorerPath = Path.Combine("src", "CFIP.Indicator", "Planning", "Execution", "PredictivePendingCandidateScorer.cs");
            string reversalLimitPath = Path.Combine("src", "CFIP.Indicator", "Trading", "Pending", "Placement", "ReversalLimitPreparation.cs");
            string controlFactoryPath = Path.Combine("src", "CFIP.Indicator", "UI", "Controls", "ExecutionControlsFactory.cs");
            string controlPresentationRulePath = Path.Combine("src", "CFIP.Indicator", "Core", "Math", "ExecutionControlPresentationRule.cs");
            string controlSyncPath = Path.Combine("src", "CFIP.Indicator", "UI", "Controls", "ExecutionControlsSynchronizer.cs");

            Assert(
                File.Exists(snapshotPath) &&
                File.Exists(previewPath) &&
                File.Exists(previewBuilderPath) &&
                File.Exists(calculationPath) &&
                File.Exists(calculationStagePath) &&
                File.Exists(rendererPath) &&
                File.Exists(pendingRendererPath) &&
                File.Exists(alertRendererPath) &&
                File.Exists(alertEnginePath) &&
                File.Exists(statePath) &&
                File.Exists(predictiveSelectorPath) &&
                File.Exists(predictiveCollectorPath) &&
                File.Exists(predictiveScorerPath) &&
                File.Exists(reversalLimitPath) &&
                File.Exists(controlFactoryPath) &&
                File.Exists(controlPresentationRulePath) &&
                File.Exists(controlSyncPath),
                "visual/control sources exist");

            string snapshot = File.ReadAllText(snapshotPath);
            string preview = File.ReadAllText(previewPath);
            string previewBuilder = File.ReadAllText(previewBuilderPath);
            string calculation = File.ReadAllText(calculationPath);
            string calculationStage = File.ReadAllText(calculationStagePath);
            string renderer = File.ReadAllText(rendererPath);
            string pendingRenderer = File.ReadAllText(pendingRendererPath);
            string alertRenderer = File.ReadAllText(alertRendererPath);
            string alertEngine = File.ReadAllText(alertEnginePath);
            string state = File.ReadAllText(statePath);
            string predictiveSelector = File.ReadAllText(predictiveSelectorPath);
            string predictiveCollector = File.ReadAllText(predictiveCollectorPath);
            string predictiveScorer = File.ReadAllText(predictiveScorerPath);
            string reversalLimit = File.ReadAllText(reversalLimitPath);
            string controlFactory = File.ReadAllText(controlFactoryPath);
            string controlPresentationRule = File.ReadAllText(controlPresentationRulePath);
            string controlSync = File.ReadAllText(controlSyncPath);

            Assert(
                snapshot.Contains("SetupPreviewActive") &&
                snapshot.Contains("SetupEntry") &&
                snapshot.Contains("SetupStop") &&
                snapshot.Contains("SetupTp1"),
                "canonical snapshot carries setup levels");

            Assert(
                preview.Contains("never consumed by broker submission"),
                "visual preview is non-executable");

            Assert(
                previewBuilder.Contains("BuildStructuralStop(") &&
                previewBuilder.Contains("BuildTargetLevels(") &&
                previewBuilder.Contains("SelectTargets("),
                "visual preview reuses planning authorities");

            Assert(
                calculation.Contains("RenderSetupPreview("),
                "calculation renders setup preview");

            string visualSnapshotBuilderPath =
                Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "SignalVisualSnapshotBuilder.cs");
            string visualSnapshotBuilder =
                File.ReadAllText(visualSnapshotBuilderPath);

            Assert(
                visualSnapshotBuilder.Contains("_setupPreview.Direction != 0") &&
                visualSnapshotBuilder.Contains("The setup preview is the pre-trigger structural forecast") &&
                !visualSnapshotBuilder.Contains("_setupPreview.Direction == visualDirection"),
                "setup preview remains visible before TriggerReady and does not wait for post-trigger visual direction");

            string planEligibilityPath =
                Path.Combine("src", "CFIP.Indicator", "Trading", "Validation", "PlanCreationEligibility.cs");
            string planEligibility =
                File.ReadAllText(planEligibilityPath);

            Assert(
                planEligibility.Contains("EntryActionabilityPolicy.RequiresConfirmedTrigger(") &&
                planEligibility.Contains("M5OnlyConfirmedTrigger") &&
                !planEligibility.Contains("if (!_decision.TriggerReady)"),
                "execution plan creation uses mode-aware trigger gating while preview remains pre-trigger");

            Assert(
                !File.ReadAllText(
                    Path.Combine(
                        "src",
                        "CFIP.Indicator",
                        "Indicator",
                        "Parameters",
                        "15_control_advanced.cs"))
                    .Contains("EnableDynamicSlTrail"),
                "semantic duplicate structural-stop alias is removed");

            Assert(
                renderer.Contains("RenderLevelLines("),
                "plan renderer owns shared level rendering");

            string lineRendererPath =
                Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "PlanLineRenderer.cs");
            string labelCoordinatorPath =
                Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "PlanLabelRenderCoordinator.cs");
            string lineRenderer =
                File.ReadAllText(lineRendererPath);
            string labelCoordinator =
                File.ReadAllText(labelCoordinatorPath);

            Assert(
                lineRenderer.Contains("GetPlanLineRightBar()") &&
                lineRenderer.Contains("return Bars.Count - 1") &&
                lineRenderer.Contains("CompactPlanLineLengthBars = 40") &&
                !lineRenderer.Contains("MapM5ToChart(") &&
                !lineRenderer.Contains("anchorM5"),
                "plan levels terminate at the latest chart candle without stale M5 anchoring");

            Assert(
                labelCoordinator.Contains("GetCompactPlanLabelAnchorBar(") &&
                !labelCoordinator.Contains("GetCompactPlanLineLeftBar("),
                "plan labels reuse the canonical compact-label anchor");

            Assert(
                controlFactory.Contains("CreateExecutionToggle(") &&
                controlFactory.Contains("IsEnabled = false") &&
                !controlFactory.Contains("_autoTradingQuickToggle.Click +=") &&
                !controlFactory.Contains("_automaticOrdersQuickToggle.Click +=") &&
                controlFactory.Contains("ExecutionControlPresentationRule.ComposeStatusText("),
                "execution controls are status-only presentation surfaces");

            Assert(
                controlPresentationRule.Contains("public static bool IsInteractive => false;") &&
                controlPresentationRule.Contains("ComposeStatusText("),
                "execution-control interaction/presentation policy is canonical and read-only");

            Assert(
                controlSync.Contains("_executionToggleSyncing = true") &&
                controlSync.Contains("RefreshCbotExecutionStateIfDue();") &&
                controlSync.Contains("EffectiveAutoTradingEnabled") &&
                controlSync.Contains("EffectiveAutomaticOrdersEnabled") &&
                controlSync.Contains("ExecutionControlPresentationRule.IsInteractive"),
                "execution control synchronization is guarded and uses canonical cBot state");

            int pendingExecution =
                calculationStage.IndexOf(
                    "PENDING INTENT PREPARATION",
                    StringComparison.Ordinal);
            int aggressiveExecution =
                calculationStage.IndexOf(
                    "AGGRESSIVE AUTO EXECUTION",
                    StringComparison.Ordinal);
            int planCreation =
                calculationStage.IndexOf(
                    "PLAN CREATION",
                    StringComparison.Ordinal);
            int marketExecution =
                calculationStage.IndexOf(
                    "AUTOMATIC MARKET EXECUTION",
                    StringComparison.Ordinal);

            Assert(
                pendingExecution >= 0 &&
                planCreation >= pendingExecution &&
                aggressiveExecution < 0 &&
                marketExecution < 0,
                "execution priority is pending -> plan; Market/Aggressive broker execution is cBot-owned");

            Assert(
                calculationStage.Contains("RefreshPendingExecutionIntent(") &&
                calculationStage.Contains("EnsureCanonicalPlan(") &&
                !calculationStage.Contains("TryAggressiveAutoTrade(") &&
                !calculationStage.Contains("TryAutoTrade("),
                "Indicator calculation keeps analysis/plan stages and removes broker execution paths");

            Assert(
                calculation.Contains("EnsureCanonicalPlan(") &&
                calculation.Contains("RenderPredictionObjects(") &&
                calculation.Contains("RenderLatestAlertSignalMarker("),
                "live cycle retains canonical plan plus prediction/alert presentation orchestration");

            Assert(
                predictiveSelector.Contains("TrySelectPredictivePendingLevel(") &&
                predictiveSelector.Contains("CollectPredictiveZoneCandidates(") &&
                predictiveSelector.Contains("FindEqualLow(") &&
                predictiveSelector.Contains("FindEqualHigh(") &&
                predictiveCollector.Contains("BuildManagedFvgZone(") &&
                predictiveCollector.Contains("BuildOrderBlockCandidate(") &&
                predictiveScorer.Contains("PredictivePendingContextQuality(") &&
                predictiveScorer.Contains("PredictivePendingSourceKey("),
                "predictive pending selector combines structural level sources");

            Assert(
                reversalLimit.Contains("TrySelectPredictivePendingLevel(") &&
                reversalLimit.Contains("candidate.Source") &&
                reversalLimit.Contains("candidate.DistanceAtr"),
                "reversal limit consumes predictive candidate evidence");

            Assert(
                pendingRenderer.Contains("RenderCompactPlanLabel(") &&
                pendingRenderer.Contains("RemovePlanLabel("),
                "pending levels render through compact semantic boxes");

            string alertDeliveryPath = Path.Combine(
                "src", "CFIP.Indicator", "Core", "Runtime", "AlertDelivery.cs");
            string alertContractPath = Path.Combine(
                "src", "CFIP.Contracts", "AlertEnvelope.cs");

            Assert(
                !alertEngine.Contains("RememberVisualSignalAlert(") &&
                !state.Contains("_lastVisualAlertM5") &&
                !state.Contains("_lastVisualAlertDirection") &&
                alertRenderer.Contains("RenderLatestAlertSignalMarker(") &&
                File.Exists(alertDeliveryPath) &&
                File.Exists(alertContractPath) &&
                File.ReadAllText(alertDeliveryPath).Contains("AlertEnvelope Envelope") &&
                File.ReadAllText(alertContractPath).Contains("VisualMarkAllowed"),
                "audible signal alerts use canonical envelope delivery without a second visual authority");

            Assert(
                File.ReadAllText(alertEnginePath).Contains("BuildCanonicalAlertEnvelope(") &&
                File.ReadAllText(alertEnginePath).Contains("EnableSoundAlerts &&") &&
                File.ReadAllText(alertEnginePath).Contains("!blockedCandidateAlert"),
                "blocked signal alert side effects are suppressed at the alert authority");

            string protectionPath = Path.Combine(
                "src", "CFIP.Indicator", "Trading", "LiveManagement", "ProtectionManager.cs");
            string protection = File.ReadAllText(protectionPath);
            string protectionRulePath = Path.Combine(
                "src", "CFIP.Indicator", "Core", "Math", "IntelligentProtectionRule.cs");
            string protectionRule = File.ReadAllText(protectionRulePath);
            Assert(
                !protection.Contains("pressureStop") &&
                !protection.Contains("market - tightRoom") &&
                !protection.Contains("market + tightRoom") &&
                protection.Contains("IntelligentProtectionRule.Evaluate(") &&
                protectionRule.Contains("pressureTighten") &&
                protectionRule.Contains("IsStructuralFarEnough("),
                "smart trailing remains structural under exit pressure");
        }

        private static void VerifyResponsivePanelRuntime()
        {
            string heartbeatPath = Path.Combine(
                "src", "CFIP.Indicator", "Runtime", "Supervision", "RuntimePanelHeartbeat.cs");
            string initPath = Path.Combine(
                "src", "CFIP.Indicator", "Runtime", "Initialization", "RuntimeInitialization.cs");
            string panelPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelMainRenderer.cs");
            string rowsPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelRowsFactory.cs");
            string writerPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelRowWriter.cs");
            string optimizationPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelRenderOptimization.cs");
            string statePath = Path.Combine(
                "src", "CFIP.Indicator", "Indicator", "State.cs");

            Assert(
                File.Exists(heartbeatPath) &&
                File.Exists(initPath) &&
                File.Exists(panelPath) &&
                File.Exists(rowsPath) &&
                File.Exists(writerPath) &&
                File.Exists(optimizationPath) &&
                File.Exists(statePath),
                "responsive panel sources exist");

            string heartbeat = File.ReadAllText(heartbeatPath);
            string init = File.ReadAllText(initPath);
            string panel = File.ReadAllText(panelPath);
            string rows = File.ReadAllText(rowsPath);
            string writer = File.ReadAllText(writerPath);
            string optimization = File.ReadAllText(optimizationPath);
            string state = File.ReadAllText(statePath);

            Assert(
                !heartbeat.Contains("RenderPanel();") &&
                heartbeat.Contains("UpdatePanelHeartbeatRows(") &&
                heartbeat.Contains("UpdatePanelHeartbeatLiveRows("),
                "heartbeat stays lightweight and refreshes live rows without full panel layout");

            Assert(
                heartbeat.Contains("TotalMilliseconds >= 1000"),
                "safety supervisor remains one-second bounded");

            Assert(
                init.Contains("TimeSpan.FromMilliseconds(500)"),
                "ready timer uses responsive panel cadence");

            Assert(
                init.Contains("TimeSpan.FromMilliseconds(250)"),
                "initialization polling avoids 100ms timer churn");

            Assert(
                init.Contains("_status =") &&
                init.Contains("RenderPanel();"),
                "initialization status reaches the panel");

            Assert(
                panel.Contains("BuildSignalVisualSnapshot(") &&
                panel.Contains("ShouldRenderFullPanel(") &&
                optimization.Contains("BuildPanelPresentationKey(") &&
                state.Contains("_lastPanelPresentationKey"),
                "panel uses one canonical snapshot and state-change-driven full render");

            Assert(
                rows.Contains("EnsurePanelRow("),
                "panel row allocation is lazy");

            Assert(
                writer.Contains("if (row.Text != nextText)"),
                "panel writer skips duplicate text writes");
        }

        private static void VerifyPanelLiveContentRefresh()
        {
            string heartbeatPath = Path.Combine(
                "src", "CFIP.Indicator", "Runtime", "Supervision", "RuntimePanelHeartbeat.cs");
            string contentPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelContentRefresh.cs");
            string statePath = Path.Combine(
                "src", "CFIP.Indicator", "Indicator", "State.cs");
            string visibilityPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelVisibility.cs");
            string livePath = Path.Combine(
                "src", "CFIP.Indicator", "Runtime", "Supervision", "PanelHeartbeatLiveState.cs");

            string heartbeat = File.ReadAllText(heartbeatPath);
            string content = File.ReadAllText(contentPath);
            string state = File.ReadAllText(statePath);
            string visibility = File.ReadAllText(visibilityPath);
            string live = File.ReadAllText(livePath);

            Assert(
                content.Contains("PanelContentRefreshMilliseconds = 500") &&
                content.Contains("ShouldRefreshPanelContent(") &&
                content.Contains("RenderPanelRows(") &&
                !content.Contains("RenderPanel();"),
                "panel content refresh is a bounded row-only refresh");

            Assert(
                heartbeat.Contains("RefreshPanelContentIfDue(") &&
                heartbeat.Contains("UpdatePanelHeartbeatRows(") &&
                heartbeat.Contains("UpdatePanelHeartbeatLiveRows("),
                "runtime heartbeat drives full content refresh before volatile row overlays");

            Assert(
                state.Contains("_lastPanelContentRefreshUtc"),
                "panel content refresh has dedicated runtime cadence state");

            Assert(
                visibility.Contains("_lastPanelContentRefreshUtc =\n                                            DateTime.MinValue"),
                "panel restore/reset forces immediate content refresh");

            Assert(
                live.Contains("double liveMarket =") &&
                live.Contains("Symbol.Bid") &&
                live.Contains("Symbol.Ask") &&
                live.Contains("IsFinitePositive(liveMarket)") &&
                !live.Contains("_lastMarket = liveMarket"),
                "live panel RR uses the current executable-side quote");

            Console.WriteLine("Panel live content refresh contract PASS");
        }

        private static void VerifyTradePlanRegistry()
        {
            TradePlanRegistry registry =
                new TradePlanRegistry();

            TradeOpportunityCandidate buy =
                new TradeOpportunityCandidate
                {
                    Id = "TACTICAL_BUY",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    Quality = 82,
                    ActionableNow = true
                };

            TradeOpportunityCandidate sell =
                new TradeOpportunityCandidate
                {
                    Id = "TACTICAL_SELL",
                    Lane = OpportunityLane.Tactical,
                    Direction = -1,
                    Quality = 79,
                    ActionableNow = false,
                    ActionabilityReason = "RR BELOW ACTIONABLE FLOOR"
                };

            registry.Upsert(buy);
            registry.Upsert(sell);

            TradeOpportunityCandidate actual;
            Assert(registry.Count == 2, "multi-plan registry retains independent lanes");
            Assert(registry.Contains("TACTICAL_BUY"), "registry BUY identity");
            Assert(registry.Contains("TACTICAL_SELL"), "registry SELL identity");
            Assert(registry.TryGetCandidate("TACTICAL_BUY", out actual) &&
                   actual.ActionableNow,
                   "registry preserves candidate actionability");

            registry.Remove("TACTICAL_BUY");
            Assert(registry.Count == 1, "registry removal");
        }

        private static void VerifyScenarioExecutionPolicy()
        {
            Decision decision =
                new Decision
                {
                    Direction = 1,
                    EntryAllowed = true,
                    TriggerReady = true,
                    ActionableNow = true
                };

            TradeOpportunityCandidate strategic =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "CANONICAL-HTF-BUY",
                    Lane = OpportunityLane.Strategic,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 90,
                    Tp1RR = 3.0,
                    ActionableNow = true
                };

            ScenarioExecutionPolicyResult strategicPolicy =
                ScenarioExecutionPolicyRule.Evaluate(
                    strategic,
                    decision,
                    OpportunityLane.Strategic,
                    true);

            Assert(
                strategicPolicy.CandidateEligible &&
                strategicPolicy.ExecutionAuthorized &&
                strategicPolicy.ExecutionReason ==
                    "CANONICAL SCENARIO EXECUTION AUTHORIZED",
                "canonical scenario policy authorizes an aligned actionable candidate");

            TradeOpportunityCandidate independent =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-H1-BUY",
                    SourceTimeframe = "H1",
                    BasePlanTimeframe = "M5",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 96,
                    Tp1RR = 4.0,
                    ActionableNow = true
                };

            ScenarioExecutionPolicyResult independentPolicy =
                ScenarioExecutionPolicyRule.Evaluate(
                    independent,
                    decision,
                    OpportunityLane.Tactical,
                    true);

            Assert(
                independentPolicy.CandidateEligible &&
                !independentPolicy.ExecutionAuthorized &&
                independentPolicy.ExecutionReason ==
                    "OBSERVE-ONLY HTF SCENARIO",
                "H1 timeframe scenario remains structurally evaluable but observe-only for execution");

            TradeOpportunityCandidate m15 =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-M15-BUY",
                    SourceTimeframe = "M15",
                    BasePlanTimeframe = "M5",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 91,
                    Tp1RR = 3.2,
                    ActionableNow = true
                };

            ScenarioExecutionPolicyResult m15Policy =
                ScenarioExecutionPolicyRule.Evaluate(
                    m15,
                    decision,
                    OpportunityLane.Tactical,
                    true);

            Assert(
                m15Policy.CandidateEligible &&
                m15Policy.ExecutionAuthorized &&
                m15Policy.ExecutionReason ==
                    "M15 SCENARIO EXECUTION AUTHORIZED",
                "M15 timeframe scenario is execution-authorized when the canonical decision is actionable");

            Assert(
                independent.BasePlanTimeframe == "M5",
                "timeframe scenario explicitly declares M5 as its canonical plan geometry base");

            TradeOpportunityCandidate presentationOnly =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-M15-BUY",
                    SourceTimeframe = "M15",
                    BasePlanTimeframe = "M5",
                    IsPrimaryTimeframeSignal = true,
                    PresentationOnly = true,
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 78,
                    ActionableNow = true
                };

            ScenarioExecutionPolicyResult presentationPolicy =
                ScenarioExecutionPolicyRule.Evaluate(
                    presentationOnly,
                    decision,
                    OpportunityLane.Tactical,
                    true);

            Assert(
                !presentationPolicy.CandidateEligible &&
                !presentationPolicy.ExecutionAuthorized &&
                presentationPolicy.ExecutionReason ==
                    "PRIMARY PRESENTATION ONLY",
                "primary presentation-only setup can never cross the execution policy boundary");

            TradeOpportunityCandidate mismatch =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "CANONICAL-TACTICAL-SELL",
                    Lane = OpportunityLane.Tactical,
                    Direction = -1,
                    ActionableNow = true
                };

            ScenarioExecutionPolicyResult mismatchPolicy =
                ScenarioExecutionPolicyRule.Evaluate(
                    mismatch,
                    decision,
                    OpportunityLane.Tactical,
                    true);

            Assert(
                !mismatchPolicy.CandidateEligible &&
                !mismatchPolicy.ExecutionAuthorized &&
                mismatchPolicy.ExecutionReason ==
                    "SCENARIO / DECISION DIRECTION MISMATCH",
                "direction mismatch blocks both candidate eligibility and execution");

            Plan plan =
                new Plan
                {
                    Direction = 1,
                    Lane = OpportunityLane.Tactical,
                    CreatedM5 = 100,
                    Entry = 100,
                    Stop = 98,
                    Tp1 = 104
                };

            TradeOpportunityCandidate tactical =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "CANONICAL-TACTICAL-BUY",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 84,
                    Tp1RR = 2.5,
                    ActionableNow = true,
                    Entry = 100,
                    Stop = 98,
                    Tp1 = 104
                };

            Assert(
                ScenarioExecutionPolicyRule.TryResolvePlanScenario(
                    new[]
                    {
                        strategic,
                        tactical,
                        independent
                    },
                    plan,
                    decision,
                    0.001,
                    true,
                    out TradeOpportunityCandidate selected,
                    out string reason) &&
                selected == tactical &&
                reason ==
                    "SCENARIO MATCHED • CANONICAL-TACTICAL-BUY",
                "plan scenario resolution uses the single canonical execution policy owner");

            decision.ActionableNow = false;

            ScenarioExecutionPolicyResult blockedPolicy =
                ScenarioExecutionPolicyRule.Evaluate(
                    tactical,
                    decision,
                    OpportunityLane.Tactical,
                    true);

            Assert(
                !blockedPolicy.CandidateEligible &&
                !blockedPolicy.ExecutionAuthorized &&
                blockedPolicy.ExecutionReason ==
                    "CANONICAL DECISION NOT ACTIONABLE",
                "non-actionable canonical state cannot authorize a scenario");
        }

        private static void VerifyIndependentTimeframeScenarioSemanticsF7()
        {
            TradeOpportunityCandidate h1 =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-H1-BUY",
                    SourceTimeframe = "H1",
                    BasePlanTimeframe = "M5",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 200,
                    Quality = 90,
                    ActionableNow = true
                };

            TradeOpportunityCandidate h4 =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-H4-BUY",
                    SourceTimeframe = "H4",
                    BasePlanTimeframe = "M5",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 200,
                    Quality = 89,
                    ActionableNow = true
                };

            Assert(
                h1.SourceTimeframe != h4.SourceTimeframe &&
                h1.BasePlanTimeframe == "M5" &&
                h4.BasePlanTimeframe == "M5" &&
                h1.ScenarioId != h4.ScenarioId,
                "same-direction timeframe annotations remain distinct without claiming independent plan geometry");

            Assert(
                ParallelScenarioSelectionRule.GetScenarioIdentity(h1) !=
                ParallelScenarioSelectionRule.GetScenarioIdentity(h4) &&
                ParallelScenarioSelectionRule.CoverageKey(h1) !=
                ParallelScenarioSelectionRule.CoverageKey(h4),
                "timeframe identity and coverage preserve simultaneous distinct frame scenarios");

            TradeOpportunityCandidate h1Duplicate =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-H1-BUY",
                    SourceTimeframe = "H1",
                    BasePlanTimeframe = "M5",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 200,
                    Quality = 95,
                    ActionableNow = true
                };

            Assert(
                ParallelScenarioSelectionRule.SameIdentity(
                    h1,
                    h1Duplicate) &&
                ParallelScenarioSelectionRule.ShouldReplace(
                    h1,
                    h1Duplicate,
                    0.01),
                "duplicate timeframe annotation is replaced deterministically by the higher-priority observation");

            Assert(
                ParallelScenarioSelectionRule.SameIdentity(
                    h1,
                    h1Duplicate) &&
                !ParallelScenarioSelectionRule.SameIdentity(
                    h1,
                    h4),
                "selection identity prevents cross-timeframe accidental deduplication");
        }

        private static void VerifyActionabilityAndDivergenceState()
        {
            DivergenceResult divergence =
                new DivergenceResult(
                    1,
                    88,
                    "REGULAR_BULL",
                    true,
                    false,
                    false,
                    false);

            Assert(
                divergence.HasSignal &&
                divergence.Direction == 1 &&
                divergence.Quality == 88 &&
                divergence.RegularBull,
                "strong regular divergence state");

            TradeActionabilityResult blocked =
                TradeActionabilityResult.Blocked(
                    "LATE / PRICE EXTENDED",
                    81,
                    -1,
                    "REGULAR_BEAR");

            Assert(
                !blocked.Actionable &&
                blocked.Reason == "LATE / PRICE EXTENDED" &&
                blocked.DivergenceQuality == 81 &&
                blocked.DivergenceDirection == -1,
                "actionability block preserves divergence diagnostics");
        }

        private static void VerifyManagedIdentitySemantics()
        {
            string first;
            string same;
            string other;

            Assert(
                ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "INSTANCE-A",
                    out first) &&
                first == "CFIP-SMART|CFIP-I:INSTANCE-A",
                "managed identity label embeds the cTrader instance identity");

            Assert(
                ManagedIdentityRule.TryBuildLabel(
                    " CFIP|SMART ",
                    "INSTANCE|A",
                    out first) &&
                first == "CFIP/SMART|CFIP-I:INSTANCE/A",
                "managed identity label sanitizes separators deterministically");

            Assert(
                ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "INSTANCE-A",
                    out first) &&
                ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "INSTANCE-A",
                    out same) &&
                first == same,
                "same instance produces stable ownership identity");

            Assert(
                ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "INSTANCE-A",
                    out first) &&
                ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "INSTANCE-B",
                    out other) &&
                first != other,
                "different instances produce isolated ownership identities");

            Assert(
                !ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "",
                    out first) &&
                !ManagedIdentityRule.TryBuildLabel(
                    "",
                    "INSTANCE-A",
                    out first),
                "missing identity inputs fail closed");

            Assert(
                ManagedIdentityRule.InstanceMarker == "|CFIP-I:",
                "managed identity marker is centralized and stable");
        }

        private static void VerifyReversalProfitThresholdSemantics()
        {
            Assert(
                ReversalProfitThresholdRule.MeetsMinimumNetProfit(0.01, 0),
                "default reversal threshold retains strictly-positive-profit behavior");

            Assert(
                !ReversalProfitThresholdRule.MeetsMinimumNetProfit(0, 0),
                "zero net profit does not satisfy the strict reversal threshold");

            Assert(
                ReversalProfitThresholdRule.MeetsMinimumNetProfit(10, 5) &&
                !ReversalProfitThresholdRule.MeetsMinimumNetProfit(5, 5),
                "configured minimum net profit is a strict lower boundary");

            Assert(
                !ReversalProfitThresholdRule.MeetsMinimumNetProfit(
                    double.NaN,
                    0) &&
                !ReversalProfitThresholdRule.MeetsMinimumNetProfit(
                    10,
                    double.NaN),
                "non-finite reversal profit inputs fail closed");

            Assert(
                !ReversalProfitThresholdRule.MeetsMinimumNetProfit(10, -1),
                "negative configured reversal profit threshold is rejected");
        }

        private static void VerifyStructuralEventSemantics()
        {
            double[] bullishFresh =
            {
                100.10,
                100.40,
                100.60
            };

            double[] bearishFresh =
            {
                99.90,
                99.60,
                99.40
            };

            Assert(
                StructuralEventRule.IsFreshBreak(
                    1,
                    0,
                    2,
                    100.0,
                    1.0,
                    0.50,
                    i => bullishFresh[i]) &&
                StructuralEventRule.IsFreshBreak(
                    -1,
                    0,
                    2,
                    100.0,
                    1.0,
                    0.50,
                    i => bearishFresh[i]),
                "fresh bullish and bearish structural breaks are directionally symmetric");

            double[] bullishRebreak =
            {
                100.10,
                100.70,
                100.40,
                100.60
            };

            Assert(
                !StructuralEventRule.IsFreshBreak(
                    1,
                    0,
                    3,
                    100.0,
                    1.0,
                    0.50,
                    i => bullishRebreak[i]),
                "bullish re-break after an earlier close above the same structural threshold is not a new event");

            double[] bearishRebreak =
            {
                99.90,
                99.30,
                99.60,
                99.40
            };

            Assert(
                !StructuralEventRule.IsFreshBreak(
                    -1,
                    0,
                    3,
                    100.0,
                    1.0,
                    0.50,
                    i => bearishRebreak[i]),
                "bearish re-break after an earlier close below the same structural threshold is not a new event");

            Assert(
                !StructuralEventRule.IsFreshBreak(
                    0,
                    0,
                    2,
                    100.0,
                    1.0,
                    0.50,
                    i => bullishFresh[i]),
                "invalid structural direction fails closed");

            Assert(
                StructuralEventRule.IsChangeOfCharacter(
                    1,
                    true,
                    true) &&
                !StructuralEventRule.IsChangeOfCharacter(
                    1,
                    false,
                    true) &&
                !StructuralEventRule.IsChangeOfCharacter(
                    -1,
                    true,
                    false),
                "CHOCH requires prior opposite structure and a fresh break");

            Assert(
                StructuralEventRule.EventIdentity(
                    1,
                    "bos",
                    20,
                    25) ==
                StructuralEventRule.EventIdentity(
                    1,
                    "BOS",
                    20,
                    25) &&
                StructuralEventRule.EventIdentity(
                    1,
                    "BOS",
                    20,
                    25) !=
                StructuralEventRule.EventIdentity(
                    1,
                    "MSS",
                    20,
                    25) &&
                StructuralEventRule.EventIdentity(
                    1,
                    "BOS",
                    20,
                    25) !=
                StructuralEventRule.EventIdentity(
                    -1,
                    "BOS",
                    20,
                    25),
                "structural event identity separates type and direction");
        }
        private static void VerifyLiquiditySweepSemantics()
        {
            double[] intact =
            {
                100.10,
                99.95,
                100.05,
                100.20,
                100.30
            };

            Assert(
                LiquiditySweepRule.IsActiveUnbrokenLevel(
                    1,
                    0,
                    4,
                    100.0,
                    0.10,
                    i => intact[i]),
                "bullish liquidity remains active while prior closes stay above tolerance");

            double[] broken =
            {
                100.10,
                99.70,
                100.05,
                100.20,
                100.30
            };

            Assert(
                !LiquiditySweepRule.IsActiveUnbrokenLevel(
                    1,
                    0,
                    4,
                    100.0,
                    0.10,
                    i => broken[i]),
                "bullish sweep rejects a level already invalidated by a prior close");

            double[] bearIntact =
            {
                99.90,
                100.05,
                99.95,
                99.80,
                99.70
            };

            Assert(
                LiquiditySweepRule.IsActiveUnbrokenLevel(
                    -1,
                    0,
                    4,
                    100.0,
                    0.10,
                    i => bearIntact[i]),
                "bearish liquidity remains active while prior closes stay below tolerance");

            double[] bearBroken =
            {
                99.90,
                100.30,
                99.95,
                99.80,
                99.70
            };

            Assert(
                !LiquiditySweepRule.IsActiveUnbrokenLevel(
                    -1,
                    0,
                    4,
                    100.0,
                    0.10,
                    i => bearBroken[i]),
                "bearish sweep rejects a level already invalidated by a prior close");
        }

        private static void VerifyRejectionSemantics()
        {
            Assert(
                RejectionRule.IsRejection(
                    100.0,
                    100.5,
                    101.0,
                    98.0,
                    1,
                    0.10),
                "bullish rejection requires a meaningful body and lower wick");

            Assert(
                RejectionRule.IsRejection(
                    100.5,
                    100.0,
                    102.0,
                    99.0,
                    -1,
                    0.10),
                "bearish rejection is directionally symmetric");

            Assert(
                RejectionRule.IsDoji(
                    100.0,
                    100.01,
                    101.0,
                    99.0,
                    0.10) &&
                !RejectionRule.IsRejection(
                    100.0,
                    100.01,
                    101.0,
                    99.0,
                    1,
                    0.10),
                "doji-like body is not promoted to directional rejection evidence");
        }

        private static void VerifyDivergenceConflictSemantics()
        {
            DivergenceResult conflict =
                DivergenceResult.CreateConflict(
                    true,
                    true,
                    false,
                    false);

            Assert(
                conflict.Direction == 0 &&
                conflict.Quality == 0 &&
                conflict.Type == "CONFLICT" &&
                conflict.RegularBull &&
                conflict.RegularBear &&
                !conflict.HasSignal,
                "divergence conflict remains visible but cannot act as directional strength");
        }

        private static void VerifyStructuralTimeframeSemantics()
        {
            Assert(
                StructuralTimeframeRule.IsSupported("M15") &&
                StructuralTimeframeRule.IsSupported("H4") &&
                StructuralTimeframeRule.IsSupported("W1"),
                "known structural timeframes are accepted");

            Assert(
                !StructuralTimeframeRule.IsSupported("H2") &&
                !StructuralTimeframeRule.IsSupported("") &&
                !StructuralTimeframeRule.IsSupported("  "),
                "unknown structural timeframes fail explicitly");
        }

        private static void VerifyAggressiveRiskAndFillSemantics()
        {
            PlanRewardRiskQualityResult buyAccepted =
                PlanRewardRiskQualityRule.Evaluate(
                    1,
                    100.0,
                    99.0,
                    102.0,
                    1.0,
                    0.0,
                    1.0,
                    0.25,
                    1.80,
                    12.0,
                    0.01);

            PlanRewardRiskQualityResult sellAccepted =
                PlanRewardRiskQualityRule.Evaluate(
                    -1,
                    100.0,
                    101.0,
                    98.0,
                    1.0,
                    0.0,
                    1.0,
                    0.25,
                    1.80,
                    12.0,
                    0.01);

            Assert(
                buyAccepted.Allowed &&
                sellAccepted.Allowed,
                "aggressive pre-trade reward/risk guard accepts valid BUY/SELL geometry");

            PlanRewardRiskQualityResult buyLowRr =
                PlanRewardRiskQualityRule.Evaluate(
                    1,
                    100.0,
                    99.0,
                    100.8,
                    1.0,
                    0.0,
                    1.0,
                    0.25,
                    1.80,
                    12.0,
                    0.01);

            PlanRewardRiskQualityResult sellLowRr =
                PlanRewardRiskQualityRule.Evaluate(
                    -1,
                    100.0,
                    101.0,
                    99.2,
                    1.0,
                    0.0,
                    1.0,
                    0.25,
                    1.80,
                    12.0,
                    0.01);

            Assert(
                !buyLowRr.Allowed &&
                !sellLowRr.Allowed &&
                buyLowRr.Reason.StartsWith("REWARD TOO LOW") &&
                sellLowRr.Reason.StartsWith("REWARD TOO LOW"),
                "aggressive pre-trade guard rejects sub-minimum RR symmetrically");

            PlanRewardRiskQualityResult buyWideStop =
                PlanRewardRiskQualityRule.Evaluate(
                    1,
                    100.0,
                    98.0,
                    104.0,
                    1.0,
                    0.0,
                    1.0,
                    0.25,
                    1.80,
                    12.0,
                    0.01);

            PlanRewardRiskQualityResult sellWideStop =
                PlanRewardRiskQualityRule.Evaluate(
                    -1,
                    100.0,
                    102.0,
                    96.0,
                    1.0,
                    0.0,
                    1.0,
                    0.25,
                    1.80,
                    12.0,
                    0.01);

            Assert(
                !buyWideStop.Allowed &&
                !sellWideStop.Allowed &&
                buyWideStop.Reason.StartsWith("STOP RISK TOO HIGH") &&
                sellWideStop.Reason.StartsWith("STOP RISK TOO HIGH"),
                "aggressive pre-trade guard rejects stop risk above the canonical effective ceiling");

            Assert(
                PriceProtectionRule.ValidateStop(
                    1,
                    100.0,
                    99.0,
                    0.10) &&
                PriceProtectionRule.ValidateTarget(
                    1,
                    100.0,
                    102.0,
                    0.10) &&
                PriceProtectionRule.ValidateStop(
                    -1,
                    100.0,
                    101.0,
                    0.10) &&
                PriceProtectionRule.ValidateTarget(
                    -1,
                    100.0,
                    98.0,
                    0.10) &&
                !PriceProtectionRule.ValidateStop(
                    1,
                    100.0,
                    101.0,
                    0.10) &&
                !PriceProtectionRule.ValidateTarget(
                    -1,
                    100.0,
                    102.0,
                    0.10),
                "actual-fill managed-plan exit geometry remains directionally valid and symmetric");

            AggressiveEntryPolicy policy =
                new AggressiveEntryPolicy();

            DateTime t =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                !policy.ObserveReactionSample(
                    300,
                    1,
                    true,
                    t) &&
                policy.ObserveReactionSample(
                    300,
                    1,
                    true,
                    t.AddSeconds(1)),
                "aggressive qualification can arm before execution");

            policy.ResetQualification();

            Assert(
                !policy.IsQualified &&
                policy.QualifyingSamples == 0,
                "aggressive qualification cleanup resets the arm state deterministically");
        }

        private static void VerifyDirectionalExecutionFillAcceptance()
        {
            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    1,
                    100.0,
                    99.5,
                    1.0,
                    0.50,
                    true),
                "BUY favorable fill is accepted");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    1,
                    100.0,
                    100.5,
                    1.0,
                    0.25,
                    true),
                "BUY adverse fill beyond envelope is rejected");

            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    -1,
                    100.0,
                    100.5,
                    1.0,
                    0.50,
                    true),
                "SELL favorable fill is accepted");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    -1,
                    100.0,
                    99.5,
                    1.0,
                    0.25,
                    true),
                "SELL adverse fill beyond envelope is rejected");

            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    1,
                    100.0,
                    100.50,
                    1.0,
                    0.50,
                    true),
                "BUY adverse boundary is accepted");

            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    -1,
                    100.0,
                    99.50,
                    1.0,
                    0.50,
                    true),
                "SELL adverse boundary is accepted");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    0,
                    100.0,
                    100.0,
                    1.0,
                    0.50,
                    true),
                "invalid direction fails closed");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    1,
                    100.0,
                    100.0,
                    0.0,
                    0.50,
                    true),
                "invalid ATR fails closed");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    1,
                    100.0,
                    100.0,
                    1.0,
                    double.NaN,
                    true),
                "invalid envelope fails closed");

            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    1,
                    100.0,
                    99.5,
                    1.0,
                    0.50,
                    false) == false,
                "favorable fill requires explicit allowFavorable policy");
        }

        private static void VerifyAggressiveEntryPolicy()
        {
            AggressiveEntryPolicy policy =
                new AggressiveEntryPolicy();

            DateTime t =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                !policy.ObserveReactionSample(200, 1, true, t),
                "first intrabar qualification sample does not arm");

            Assert(
                policy.QualifyingSamples == 1 &&
                policy.Direction == 1 &&
                policy.ReactionM5 == 200,
                "first qualifying sample is tracked");

            Assert(
                !policy.ObserveReactionSample(200, 1, true, t),
                "duplicate reaction sample does not count twice");

            Assert(
                policy.QualifyingSamples == 1,
                "duplicate sample count remains stable");

            Assert(
                policy.ObserveReactionSample(
                    200,
                    1,
                    true,
                    t.AddMilliseconds(800)),
                "second qualifying intrabar sample arms");

            Assert(
                policy.IsQualified &&
                policy.GetQualificationStateText() == "ARMED",
                "policy reaches armed state");

            Assert(
                !policy.ObserveReactionSample(
                    200,
                    -1,
                    true,
                    t.AddMilliseconds(1600)) &&
                !policy.IsQualified &&
                policy.Direction == -1 &&
                policy.QualifyingSamples == 1,
                "direction change invalidates prior qualification");

            Assert(
                !policy.ObserveReactionSample(
                    200,
                    -1,
                    false,
                    t.AddMilliseconds(2400)) &&
                !policy.IsQualified &&
                policy.QualifyingSamples == 0,
                "lost reaction qualification invalidates immediately");

            Assert(
                !policy.ObserveReactionSample(
                    201,
                    1,
                    true,
                    t.AddMilliseconds(3200)) &&
                policy.ReactionM5 == 201 &&
                policy.Direction == 1 &&
                policy.QualifyingSamples == 1,
                "new M5 starts a fresh qualification window");
        }

        private static void VerifyReactionQualificationSemantics()
        {
            Assert(
                ReactionQualificationRule.ResolveDirection(80, 70) == 1 &&
                ReactionQualificationRule.ResolveDirection(70, 80) == -1 &&
                ReactionQualificationRule.ResolveDirection(80, 80) == 0,
                "reaction direction resolves symmetrically and ties to neutral");

            Assert(
                ReactionQualificationRule.HasPriorCounterMove(
                    1,
                    100.0,
                    99.0,
                    98.0) &&
                ReactionQualificationRule.HasPriorCounterMove(
                    -1,
                    100.0,
                    101.0,
                    102.0),
                "reversal requires a prior move against the candidate direction");

            Assert(
                !ReactionQualificationRule.HasPriorCounterMove(
                    1,
                    100.0,
                    101.0,
                    101.5) &&
                !ReactionQualificationRule.HasPriorCounterMove(
                    -1,
                    100.0,
                    99.0,
                    98.5),
                "same-direction continuation is not accepted as prior counter-move");

            Assert(
                ReactionQualificationRule.HasQualifyingContext(
                    true,
                    false,
                    false,
                    false) &&
                ReactionQualificationRule.HasQualifyingContext(
                    false,
                    false,
                    false,
                    true),
                "no-zone reversal requires a real non-zone structural context");

            Assert(
                ReactionQualificationRule.HasQualifyingContext(
                    false,
                    true,
                    true,
                    false) &&
                !ReactionQualificationRule.HasQualifyingContext(
                    true,
                    true,
                    false,
                    true),
                "present zones must be qualifying and cannot be bypassed by unrelated context");

            Assert(
                ReactionQualificationRule.IsQualified(
                    1,
                    75,
                    3,
                    true,
                    70,
                    2,
                    false,
                    false) &&
                !ReactionQualificationRule.IsQualified(
                    1,
                    75,
                    3,
                    true,
                    70,
                    2,
                    true,
                    false) &&
                ReactionQualificationRule.IsQualified(
                    1,
                    75,
                    3,
                    true,
                    70,
                    2,
                    true,
                    true),
                "intrabar qualification can arm immediately, while pending confirmation requires a closed bar");

            Assert(
                !ReactionQualificationRule.IsQualified(
                    1,
                    90,
                    3,
                    false,
                    70,
                    2,
                    false,
                    false) &&
                !ReactionQualificationRule.IsQualified(
                    0,
                    90,
                    3,
                    true,
                    70,
                    2,
                    false,
                    false),
                "missing context or direction always fails closed");

            Assert(
                ReactionQualificationRule.IsClosedBarConfirmed(
                    20,
                    20) &&
                ReactionQualificationRule.IsClosedBarConfirmed(
                    20,
                    21) &&
                !ReactionQualificationRule.IsClosedBarConfirmed(
                    20,
                    19),
                "closed-bar confirmation never accepts a candidate after the known closed index");
        }

        private static void VerifyIndicatorExecutionQualitySemantics()
        {
            IndicatorQualityGateResult marketPass =
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.AutomaticMarket,
                    60,
                    52);

            Assert(
                marketPass.Allowed &&
                marketPass.MinimumQuality == 60 &&
                marketPass.MaximumConflict == 52,
                "automatic market uses its documented final-entry indicator gate");

            Assert(
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.AutomaticMarket,
                    59,
                    40).Allowed &&
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.AutomaticMarket,
                    80,
                    53).Allowed,
                "automatic market rejects below quality or above conflict");

            Assert(
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingSubmission,
                    58,
                    55).Allowed &&
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingSubmission,
                    57,
                    10).Allowed &&
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingSubmission,
                    80,
                    56).Allowed,
                "pending submission keeps a separate lower defense-in-depth floor");

            IndicatorQualityGateResult continuation =
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingContinuation,
                    62,
                    48);

            IndicatorQualityGateResult reversal =
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingReversal,
                    62,
                    50);

            Assert(
                continuation.Allowed &&
                reversal.Allowed &&
                continuation.MinimumQuality ==
                    reversal.MinimumQuality &&
                continuation.MinimumQuality == 62,
                "continuation and reversal share one pending setup quality floor");

            Assert(
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingContinuation,
                    70,
                    49).Allowed &&
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingReversal,
                    70,
                    51).Allowed,
                "continuation and reversal retain explicitly different conflict tolerance");

            Assert(
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingContinuation,
                    61,
                    48).Reason.Contains(
                        "PENDING CONTINUATION") &&
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingSubmission,
                    57,
                    20).Reason.Contains(
                        "PENDING SUBMISSION"),
                "blocked diagnostics identify the canonical semantic gate");

            Assert(
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    (IndicatorQualityGateStage)999,
                    100,
                    0).Allowed == false,
                "unknown indicator gate stage fails closed");
        }

        private static void VerifyPendingDecisionArbiterSemantics()
        {
            Assert(
                PendingDecisionArbiterRule.ScoreCandidate(
                    90,
                    80,
                    70,
                    4,
                    4) >
                PendingDecisionArbiterRule.ScoreCandidate(
                    80,
                    70,
                    60,
                    3,
                    3),
                "pending candidate score rewards stronger decision evidence deterministically");

            PendingArbiterResult continuationOnly =
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    80,
                    false,
                    -1,
                    90,
                    PendingOrderMode.Adaptive);

            Assert(
                continuationOnly.Choice ==
                    PendingArbiterChoice.ContinuationStop &&
                continuationOnly.Direction == 1,
                "single eligible continuation is selected");

            PendingArbiterResult reversalOnly =
                PendingDecisionArbiterRule.SelectWinner(
                    false,
                    1,
                    80,
                    true,
                    -1,
                    90,
                    PendingOrderMode.Adaptive);

            Assert(
                reversalOnly.Choice ==
                    PendingArbiterChoice.ReversalLimit &&
                reversalOnly.Direction == -1,
                "single eligible reversal is selected");

            PendingArbiterResult bothContinuationWins =
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    92,
                    true,
                    -1,
                    84,
                    PendingOrderMode.Both);

            PendingArbiterResult bothReversalWins =
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    84,
                    true,
                    -1,
                    92,
                    PendingOrderMode.Both);

            Assert(
                bothContinuationWins.Choice ==
                    PendingArbiterChoice.ContinuationStop &&
                bothReversalWins.Choice ==
                    PendingArbiterChoice.ReversalLimit,
                "when both candidates are eligible, quality chooses exactly one winner");

            PendingArbiterResult tie =
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    88,
                    true,
                    -1,
                    88,
                    PendingOrderMode.Both);

            Assert(
                tie.Choice ==
                    PendingArbiterChoice.ContinuationStop &&
                tie.Reason.Contains(
                    "TIE"),
                "exact candidate ties use one explicit deterministic policy");

            Assert(
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    100,
                    true,
                    -1,
                    100,
                    PendingOrderMode.ContinuationStop).Choice ==
                    PendingArbiterChoice.ContinuationStop &&
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    100,
                    true,
                    -1,
                    100,
                    PendingOrderMode.ReversalLimit).Choice ==
                    PendingArbiterChoice.ReversalLimit,
                "pending mode constrains the arbiter before quality comparison");

            Assert(
                PendingDecisionArbiterRule.SelectWinner(
                    false,
                    1,
                    0,
                    false,
                    -1,
                    0,
                    PendingOrderMode.Both).Choice ==
                    PendingArbiterChoice.None,
                "no eligible candidate produces no pending decision");

            Assert(
                PendingDecisionArbiterRule.IsSameChoice(
                    PendingArbiterChoice.ContinuationStop,
                    true) &&
                !PendingDecisionArbiterRule.IsSameChoice(
                    PendingArbiterChoice.ContinuationStop,
                    false) &&
                PendingDecisionArbiterRule.IsSameChoice(
                    PendingArbiterChoice.ReversalLimit,
                    false) &&
                !PendingDecisionArbiterRule.IsSameChoice(
                    PendingArbiterChoice.ReversalLimit,
                    true),
                "existing pending order type maps unambiguously to its policy choice");

            Assert(
                !PendingDecisionArbiterRule.ShouldCancelAfterHysteresis(
                    true,
                    1,
                    2) &&
                PendingDecisionArbiterRule.ShouldCancelAfterHysteresis(
                    true,
                    2,
                    2) &&
                !PendingDecisionArbiterRule.ShouldCancelAfterHysteresis(
                    false,
                    9,
                    2),
                "pending cancellation needs two consecutive invalidation bars");

            Assert(
                PendingDecisionArbiterRule.ScoreCandidate(
                    90,
                    90,
                    90,
                    8,
                    8) <= 100 &&
                PendingDecisionArbiterRule.ScoreCandidate(
                    0,
                    0,
                    0,
                    0,
                    0) == 0,
                "candidate score remains bounded");
        }

        private static void VerifyLifecycleOutcomeSemantics()
        {
            LifecycleEventIdempotencyGuard guard =
                new LifecycleEventIdempotencyGuard();

            Assert(
                guard.TryBegin("POSITION_OPENED", 101) &&
                !guard.TryBegin("POSITION_OPENED", 101) &&
                guard.TryBegin("POSITION_CLOSED", 101),
                "lifecycle idempotency distinguishes event type and rejects duplicates");

            for (long id = 1; id <= 600; id++)
            {
                Assert(
                    guard.TryBegin("TEST_EVENT", id),
                    "bounded idempotency accepts fresh lifecycle identities");
            }

            Assert(
                guard.RememberedEventCount <= 512,
                "lifecycle idempotency memory is bounded");

            HistoricalOutcomeAggregate empty =
                HistoricalOutcomeAggregationRule.AggregateHistoricalOutcomeRecords(
                    null);

            Assert(
                !empty.Available &&
                empty.TradeCount == 0,
                "empty historical outcome set fails closed");

            List<HistoricalOutcomeRecord> trades =
                new List<HistoricalOutcomeRecord>
                {
                    new HistoricalOutcomeRecord
                    {
                        NetProfit = 40,
                        GrossProfit = 45,
                        Swap = -2,
                        Commissions = -3,
                        Pips = 10,
                        ClosingTime =
                            new DateTime(
                                2026,
                                9,
                                30,
                                10,
                                0,
                                0,
                                DateTimeKind.Utc)
                    },
                    new HistoricalOutcomeRecord
                    {
                        NetProfit = -10,
                        GrossProfit = -8,
                        Swap = -1,
                        Commissions = -1,
                        Pips = -2,
                        ClosingTime =
                            new DateTime(
                                2026,
                                9,
                                30,
                                10,
                                5,
                                0,
                                DateTimeKind.Utc)
                    }
                };

            HistoricalOutcomeAggregate aggregate =
                HistoricalOutcomeAggregationRule.AggregateHistoricalOutcomeRecords(
                    trades);

            Assert(
                aggregate.Available &&
                aggregate.TradeCount == 2 &&
                Math.Abs(aggregate.NetProfit - 30) < 0.000001 &&
                Math.Abs(aggregate.GrossProfit - 37) < 0.000001 &&
                Math.Abs(aggregate.Swap + 3) < 0.000001 &&
                Math.Abs(aggregate.Commissions + 4) < 0.000001 &&
                Math.Abs(aggregate.Pips - 8) < 0.000001 &&
                aggregate.ClosingTime ==
                    new DateTime(
                        2026,
                        9,
                        30,
                        10,
                        5,
                        0,
                        DateTimeKind.Utc),
                "multiple closing historical trades aggregate to one final realized outcome");

            HistoricalOutcomeAggregate invalid =
                HistoricalOutcomeAggregationRule.AggregateHistoricalOutcomeRecords(
                    new[]
                    {
                        new HistoricalOutcomeRecord
                        {
                            NetProfit = double.NaN,
                            GrossProfit = 1,
                            Swap = 0,
                            Commissions = 0,
                            Pips = 1,
                            ClosingTime = DateTime.UtcNow
                        }
                    });

            Assert(
                !invalid.Available &&
                invalid.TradeCount == 0,
                "non-finite historical outcome data is ignored");

            HistoricalOutcomeAggregate fallback =
                HistoricalOutcomeAggregationRule.FromPositionFallback(
                    25,
                    5);

            Assert(
                fallback.Available &&
                fallback.TradeCount == 1 &&
                Math.Abs(fallback.NetProfit - 25) < 0.000001 &&
                Math.Abs(fallback.Pips - 5) < 0.000001,
                "position outcome fallback remains deterministic when history is unavailable");
        }


        private static void VerifyRewardQualityFloorSemantics()
        {
            Assert(
                Math.Abs(
                    RewardQualityFloorRule.Calculate(
                        80,
                        1.50) -
                    1.00) < 0.000001,
                "reward quality floor preserves the legacy deterministic quality/RR balance");

            Assert(
                RewardQualityFloorRule.Calculate(80, 2.0) >
                RewardQualityFloorRule.Calculate(80, 1.0) &&
                RewardQualityFloorRule.Calculate(80, 1.0) >
                RewardQualityFloorRule.Calculate(70, 1.0),
                "reward quality floor is monotonic in both reward-risk and analytical quality");

            Assert(
                RewardQualityFloorRule.Calculate(100, 1.0) <=
                RewardQualityFloorRule.Calculate(100, 2.0),
                "reward quality floor never reverses reward-risk ordering");

            Assert(
                RewardQualityFloorRule.Calculate(80, double.NaN) ==
                RewardQualityFloorRule.Calculate(80, double.PositiveInfinity) ||
                RewardQualityFloorRule.Calculate(80, double.NaN) <
                RewardQualityFloorRule.Calculate(80, 1.0),
                "non-finite reward-risk input cannot create an inflated reward-quality result");
        }

        private static void VerifyEarlyPredictionScoreSemantics()
        {
            EarlyPredictionScoreResult lowTotal =
                EarlyPredictionScoreRule.Evaluate(
                    1.0,
                    0.1,
                    0,
                    0,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false);

            Assert(
                lowTotal.DirectionalShare >= 90 &&
                lowTotal.AbsoluteStrength < 1,
                "tiny absolute evidence cannot hide behind a highly asymmetric directional share");

            Assert(
                lowTotal.AbsoluteStrength < 56,
                "low-total early prediction evidence remains below the existing minimum confidence strength floor");

            EarlyPredictionScoreResult buy =
                EarlyPredictionScoreRule.Evaluate(
                    80,
                    20,
                    70,
                    10,
                    true,
                    true,
                    false,
                    true,
                    false,
                    true,
                    false);

            EarlyPredictionScoreResult sell =
                EarlyPredictionScoreRule.Evaluate(
                    20,
                    80,
                    10,
                    70,
                    true,
                    false,
                    true,
                    false,
                    true,
                    false,
                    true);

            Assert(
                buy.Direction == 1 &&
                sell.Direction == -1 &&
                buy.DirectionalShare == sell.DirectionalShare &&
                Math.Abs(
                    buy.AbsoluteStrength -
                    sell.AbsoluteStrength) < 0.000001,
                "early prediction evidence fusion is BUY/SELL symmetric");

            Assert(
                Math.Abs(
                    buy.BuyStrength -
                    (80 * EarlyPredictionScoreRule.M5Weight +
                     70 * EarlyPredictionScoreRule.M15Weight +
                     EarlyPredictionScoreRule.LiquidityForecastBonus +
                     EarlyPredictionScoreRule.VolumeBonus +
                     EarlyPredictionScoreRule.VwapBonus)) < 0.000001,
                "named early prediction weights and evidence bonuses own the scoring constants");
        }


        private static void VerifyPartialTakeProfitRetrySemantics()
        {
            Assert(
                PartialTakeProfitRetryRule.ShouldAttempt(20, -1) &&
                !PartialTakeProfitRetryRule.ShouldAttempt(20, 20) &&
                PartialTakeProfitRetryRule.ShouldAttempt(21, 20),
                "partial take-profit mutation retries at most once per canonical closed M5 bar");

            Assert(
                PartialTakeProfitRetryRule.ShouldAttemptStage(
                    20,
                    20,
                    "TP1",
                    "TP2") &&
                !PartialTakeProfitRetryRule.ShouldAttemptStage(
                    20,
                    20,
                    "TP1",
                    "TP1") &&
                PartialTakeProfitRetryRule.ShouldAttemptStage(
                    21,
                    20,
                    "TP1",
                    "TP1"),
                "partial take-profit retry identity is stage-specific and advances on the next closed bar");

            Assert(
                !PartialTakeProfitRetryRule.ShouldAttemptStage(
                    -1,
                    -1,
                    "",
                    "TP1") &&
                !PartialTakeProfitRetryRule.ShouldAttemptStage(
                    20,
                    20,
                    "",
                    ""),
                "invalid retry inputs fail closed");
        }

        private static void VerifyServerPartialTakeProfitEvidenceSemantics()
        {
            Assert(
                ServerPartialTakeProfitEvidenceRule.IsMatchingClosingDeal(
                    101,
                    1,
                    101,
                    -1,
                    true,
                    101.20,
                    101.20,
                    3300,
                    3300,
                    0.03,
                    1),
                "BUY server TP evidence requires the matching closing deal identity, price and volume");

            Assert(
                ServerPartialTakeProfitEvidenceRule.IsMatchingClosingDeal(
                    102,
                    -1,
                    102,
                    1,
                    true,
                    98.80,
                    98.80,
                    2200,
                    2200,
                    0.03,
                    1),
                "SELL server TP evidence is directionally symmetric");

            Assert(
                !ServerPartialTakeProfitEvidenceRule.IsMatchingClosingDeal(
                    103,
                    1,
                    103,
                    -1,
                    false,
                    101.20,
                    101.20,
                    3300,
                    3300,
                    0.03,
                    1) &&
                !ServerPartialTakeProfitEvidenceRule.IsMatchingClosingDeal(
                    103,
                    1,
                    999,
                    -1,
                    true,
                    101.20,
                    101.20,
                    3300,
                    3300,
                    0.03,
                    1) &&
                !ServerPartialTakeProfitEvidenceRule.IsMatchingClosingDeal(
                    103,
                    1,
                    103,
                    1,
                    true,
                    101.20,
                    101.20,
                    3300,
                    3300,
                    0.03,
                    1) &&
                !ServerPartialTakeProfitEvidenceRule.IsMatchingClosingDeal(
                    103,
                    1,
                    103,
                    -1,
                    true,
                    101.70,
                    101.20,
                    3300,
                    3300,
                    0.03,
                    1),
                "server TP evidence rejects non-closing, wrong-position, wrong-direction and materially wrong-price deals");
        }

        private static void VerifyPeakPriceReconstructionSemantics()
        {
            double[] highs = { 100, 101.5, 104, 103 };
            double[] lows = { 100, 99, 98, 97.5 };

            double peak;
            Assert(
                PeakPriceReconstructionRule.TryResolve(
                    1,
                    100,
                    102,
                    0,
                    3,
                    i => highs[i],
                    i => lows[i],
                    out peak) &&
                Math.Abs(peak - 104) < 0.000001,
                "BUY peak reconstruction includes the highest closed-bar extreme and current market");

            Assert(
                PeakPriceReconstructionRule.TryResolve(
                    -1,
                    100,
                    98.5,
                    0,
                    3,
                    i => highs[i],
                    i => lows[i],
                    out peak) &&
                Math.Abs(peak - 97.5) < 0.000001,
                "SELL peak reconstruction includes the lowest closed-bar extreme and current market");

            Assert(
                !PeakPriceReconstructionRule.TryResolve(
                    0,
                    100,
                    101,
                    0,
                    3,
                    i => highs[i],
                    i => lows[i],
                    out peak) &&
                !PeakPriceReconstructionRule.TryResolve(
                    1,
                    100,
                    101,
                    3,
                    2,
                    i => highs[i],
                    i => lows[i],
                    out peak),
                "peak reconstruction rejects invalid direction and reversed ranges");
        }


        private static void VerifySmartBreakEvenSemantics()
        {
            SmartBreakEvenResult buy =
                SmartBreakEvenRule.Evaluate(
                    20,
                    60,
                    2,
                    0.90,
                    0.5,
                    0.5,
                    true);

            SmartBreakEvenResult sell =
                SmartBreakEvenRule.Evaluate(
                    20,
                    60,
                    2,
                    0.90,
                    0.5,
                    0.5,
                    true);

            Assert(
                buy.Allowed &&
                sell.Allowed &&
                Math.Abs(buy.TriggerPips - sell.TriggerPips) < 0.000001 &&
                Math.Abs(buy.OffsetPips - sell.OffsetPips) < 0.000001,
                "spread-aware break-even parameters are direction-neutral");

            SmartBreakEvenResult blocked =
                SmartBreakEvenRule.Evaluate(
                    20,
                    5,
                    2,
                    0.90,
                    0.5,
                    0.5,
                    true);

            Assert(
                !blocked.Allowed &&
                blocked.Reason.Contains(
                    "TP1 TOO CLOSE"),
                "break-even reports an explicit non-applicable reason when TP1 leaves insufficient room");

            Assert(
                !SmartBreakEvenRule.Evaluate(
                    double.NaN,
                    60,
                    2,
                    0.90,
                    0.5,
                    0.5,
                    true).Allowed,
                "break-even fails closed for non-finite risk inputs");
        }

        private static void VerifyTargetProgressionMonotonicity()
        {
            Assert(
                TargetProgressionRule.IsValid(
                    1,
                    100,
                    105) &&
                !TargetProgressionRule.IsValid(
                    1,
                    105,
                    100),
                "BUY target progression is strictly forward");

            Assert(
                TargetProgressionRule.IsValid(
                    -1,
                    100,
                    95) &&
                !TargetProgressionRule.IsValid(
                    -1,
                    95,
                    100),
                "SELL target progression is strictly forward");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    100,
                    105,
                    true) &&
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    105,
                    100,
                    true) &&
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    100,
                    95,
                    true) &&
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    95,
                    100,
                    true),
                "broker TP progression never accepts a backward move");
        }

        private static void VerifyRuntimeExplicitRearmSemantics()
        {
            RuntimeFaultStateMachine machine =
                new RuntimeFaultStateMachine();

            Assert(
                machine.RequestExplicitRearm() &&
                machine.CanAutomaticEntryProceed,
                "healthy runtime accepts an explicit re-arm request");

            machine.BlockAutomaticEntry();

            Assert(
                !machine.RequestExplicitRearm() &&
                !machine.CanAutomaticEntryProceed,
                "entry-blocked runtime cannot be bypassed by explicit re-arm");

            machine = new RuntimeFaultStateMachine();
            machine.RecordRecoverableFault();

            Assert(
                !machine.RequestExplicitRearm() &&
                !machine.CanAutomaticEntryProceed,
                "degraded runtime cannot be re-armed before recovery");
        }

        private static AlertDelivery BuildTestAlertDelivery(
            string key,
            string message,
            bool critical,
            DateTime eventUtc,
            bool playSound,
            string soundTypeName,
            string soundFilePath,
            bool showPanelMessage)
        {
            ContractIdentity identity =
                new ContractIdentity(
                    ContractVersion.Current,
                    key,
                    "TEST-SCENARIO|" + key,
                    "TEST-PLAN|" + key,
                    "TEST",
                    CFIP.Contracts.TradeDirection.None,
                    CFIP.Contracts.OpportunityLane.Tactical,
                    "M5",
                    eventUtc,
                    0,
                    null,
                    1,
                    key,
                    "TEST|" + key);

            AlertEnvelope envelope =
                new AlertEnvelope(
                    identity,
                    critical
                        ? SignalStage.Confirmed
                        : SignalStage.Watch,
                    "TEST-ALERT|" + key,
                    key,
                    message,
                    critical,
                    true,
                    eventUtc);

            return new AlertDelivery(
                envelope,
                0,
                playSound,
                soundTypeName,
                soundFilePath);
        }

        private static void VerifyAlertDeliveryQueueSemantics()
        {
            DateTime now = Utc(12, 0);
            AlertDeliveryQueue queue =
                new AlertDeliveryQueue(3);

            AlertDelivery normal1 =
                BuildTestAlertDelivery(
                    "NORMAL|1",
                    "normal-1",
                    false,
                    now,
                    true,
                    "Announcement",
                    "",
                    true);

            AlertDelivery normal2 =
                BuildTestAlertDelivery(
                    "NORMAL|2",
                    "normal-2",
                    false,
                    now,
                    true,
                    "Announcement",
                    "",
                    true);

            AlertDelivery critical =
                BuildTestAlertDelivery(
                    "CRITICAL|1",
                    "critical-1",
                    true,
                    now,
                    true,
                    "Confirmation",
                    "",
                    true);

            Assert(
                queue.Enqueue(normal1) &&
                queue.Enqueue(normal2),
                "alert delivery queue accepts normal alerts");

            Assert(
                queue.Enqueue(critical) &&
                queue.Count == 3,
                "alert delivery queue remains bounded when a critical alert arrives");

            AlertDelivery alert;
            Assert(
                queue.TryDequeue(out alert) &&
                alert.Critical &&
                alert.Message == "critical-1" &&
                alert.PlaySound &&
                alert.Message == "critical-1",
                "critical alert delivery is prioritized with audio and canonical message intent intact");

            Assert(
                queue.TryDequeue(out alert) &&
                alert.Message == "normal-1" &&
                !alert.Critical,
                "normal alert FIFO order is preserved after critical priority");

            queue = new AlertDeliveryQueue(3);

            Assert(
                queue.Enqueue(normal1) &&
                !queue.Enqueue(normal1) &&
                queue.Count == 1,
                "identical canonical alert cannot be queued twice while pending");

            Assert(
                queue.TryDequeue(out alert) &&
                alert.Message == "normal-1",
                "deduplicated alert remains deliverable exactly once");

            Assert(
                queue.Enqueue(normal1) &&
                queue.Count == 1,
                "same alert may be re-armed after it is actually delivered");

            queue = new AlertDeliveryQueue(3);

            Assert(
                queue.Enqueue(normal1) &&
                queue.Enqueue(normal2) &&
                queue.Enqueue(
                    BuildTestAlertDelivery(
                        "NORMAL|3",
                        "normal-3",
                        false,
                        now,
                        true,
                        "Announcement",
                        "",
                        false)) &&
                !queue.Enqueue(
                    BuildTestAlertDelivery(
                        "NORMAL|4",
                        "normal-4",
                        false,
                        now,
                        true,
                        "Announcement",
                        "",
                        true)) &&
                queue.Count == 3,
                "normal alert overflow is bounded and drops new low-priority work");

            queue = new AlertDeliveryQueue(3);
            queue.Enqueue(normal1);
            queue.Enqueue(normal2);
            queue.Enqueue(
                BuildTestAlertDelivery(
                    "NORMAL|3",
                    "normal-3",
                    false,
                    now,
                    true,
                    "Announcement",
                    "",
                    true));

            Assert(
                queue.Enqueue(
                    BuildTestAlertDelivery(
                        "CRITICAL|2",
                        "critical-2",
                        true,
                        now,
                        true,
                        "Confirmation",
                        "",
                        true)) &&
                queue.Count == 3 &&
                queue.TryPeek(out alert) &&
                alert.Critical,
                "critical overflow evicts lower-priority work first");

            Assert(
                !queue.Enqueue(
                    BuildTestAlertDelivery(
                        "EMPTY",
                        "",
                        false,
                        now,
                        true,
                        "Announcement",
                        "",
                        true)) &&
                queue.Count == 3,
                "alert delivery queue rejects empty messages without changing bounded state");
        }
        private static void VerifyDirectionalBiasTimeframeSemantics()
        {
            Assert(
                PremiumDiscountBiasRule.Evaluate(99, 110, 90) == 1 &&
                PremiumDiscountBiasRule.Evaluate(101, 110, 90) == -1 &&
                PremiumDiscountBiasRule.Evaluate(100, 110, 90) == 0,
                "premium/discount bias remains a neutral midpoint context with mean-reversion direction");

            Assert(
                PremiumDiscountBiasRule.Evaluate(double.NaN, 110, 90) == 0 &&
                PremiumDiscountBiasRule.Evaluate(100, 90, 90) == 0,
                "premium/discount bias fails closed on invalid geometry");

            Assert(
                LiveM5BiasRule.Evaluate(105, 103, 101, 1) == 5 &&
                LiveM5BiasRule.Evaluate(97, 99, 101, -1) == 5 &&
                LiveM5BiasRule.Evaluate(105, 103, 101, -1) == 0 &&
                LiveM5BiasRule.Evaluate(97, 99, 101, 1) == 0,
                "closed-M5 live bias is BUY/SELL symmetric and direction-specific");

            Assert(
                LiveM5BiasRule.Evaluate(105, 103, 101, 0) == 0 &&
                LiveM5BiasRule.Evaluate(double.NaN, 103, 101, 1) == 0,
                "closed-M5 live bias fails closed on invalid inputs");

            Assert(
                HealthyVolatilityRule.IsHealthy(0.85, 1.00, 0.85, 1.80) &&
                HealthyVolatilityRule.IsHealthy(1.80, 1.00, 0.85, 1.80) &&
                !HealthyVolatilityRule.IsHealthy(0.84, 1.00, 0.85, 1.80) &&
                !HealthyVolatilityRule.IsHealthy(1.81, 1.00, 0.85, 1.80),
                "healthy-volatility uses the configured ATR ratio envelope only");

            DateTime[] m5 = new[]
            {
                Utc(10, 0),
                Utc(10, 5),
                Utc(10, 10),
                Utc(10, 15)
            };

            DateTime[] m15 = new[]
            {
                Utc(10, 0),
                Utc(10, 15),
                Utc(10, 30),
                Utc(10, 45)
            };

            DateTime[] h1 = new[]
            {
                Utc(10, 0),
                Utc(11, 0),
                Utc(12, 0),
                Utc(13, 0)
            };

            Assert(
                ClosedBarReferenceRule.IsFullyClosed(
                    m5.Length,
                    2,
                    Utc(10, 15),
                    i => m5[i]) &&
                ClosedBarReferenceRule.IsFullyClosed(
                    m15.Length,
                    2,
                    Utc(10, 45),
                    i => m15[i]) &&
                ClosedBarReferenceRule.IsFullyClosed(
                    h1.Length,
                    2,
                    Utc(13, 0),
                    i => h1[i]),
                "M5/M15/H1 closed-bar fixtures share the same fully-closed boundary");

            Assert(
                !ClosedBarReferenceRule.IsFullyClosed(
                    m5.Length,
                    2,
                    Utc(10, 14),
                    i => m5[i]),
                "unfinished M5 bar cannot be used as live-bias confirmation");
        }


        private static void VerifyOpposingZonePathF1()
        {
            double bearishLow;
            double bearishHigh;
            double bearishGap;

            Assert(
                FvgRule.TryGetThreeBarGap(
                    -1,
                    108,
                    106,
                    104,
                    102,
                    out bearishLow,
                    out bearishHigh,
                    out bearishGap) &&
                bearishLow == 104 &&
                bearishHigh == 106,
                "F1 BUY opposing zone uses canonical bearish FVG orientation");

            double bullishLow;
            double bullishHigh;
            double bullishGap;

            Assert(
                FvgRule.TryGetThreeBarGap(
                    1,
                    104,
                    100,
                    108,
                    106,
                    out bullishLow,
                    out bullishHigh,
                    out bullishGap) &&
                bullishLow == 104 &&
                bullishHigh == 106,
                "F1 bullish FVG orientation remains distinct from BUY opposing semantics");

            Assert(
                RewardPathGeometryRule.BlocksRewardPath(
                    bearishLow,
                    bearishHigh,
                    100,
                    110,
                    0.10),
                "F1 BUY bearish FVG blocks the reward path");

            Assert(
                RewardPathGeometryRule.IsOpposingZoneDirection(
                    1,
                    -1) &&
                !RewardPathGeometryRule.IsOpposingZoneDirection(
                    1,
                    1),
                "F1 BUY opposing-zone direction is explicit and symmetric");

            Assert(
                FvgRule.IsFullyFilled(
                    -1,
                    bearishLow,
                    bearishHigh,
                    bearishHigh),
                "F1 fully mitigated bearish FVG is detectable as filled");

            double managedLow;
            double managedHigh;

            Assert(
                !FvgRule.TryApplyPartialMitigation(
                    -1,
                    bearishLow,
                    bearishHigh,
                    bearishHigh,
                    0.01,
                    out managedLow,
                    out managedHigh),
                "F1 fully mitigated opposing FVG cannot survive canonical mitigation");

            Assert(
                RewardPathGeometryRule.BlocksRewardPath(
                    104,
                    106,
                    100,
                    110,
                    0.10) ==
                RewardPathGeometryRule.BlocksRewardPath(
                    94,
                    96,
                    100,
                    90,
                    0.10),
                "F1 reward-path obstacle geometry is BUY/SELL symmetric");

            Assert(
                RewardPathGeometryRule.BlocksRewardPath(
                    104,
                    106,
                    100,
                    110,
                    0.10) &&
                !RewardPathGeometryRule.BlocksRewardPath(
                    104,
                    106,
                    100,
                    104,
                    0.10),
                "F1 target-specific path check preserves entry/target clearance semantics");

            Console.WriteLine(
                "F1 opposing-zone path contracts: canonical direction, mitigation and path geometry passed");
        }

        private static void VerifyWatchReactionAlertSemantics()
        {
            Assert(
                WatchReactionAlertRule.ResolveEarlyWatchMinimumConfidence(82) == 78 &&
                WatchReactionAlertRule.ResolveEarlyWatchMinimumConfidence(60) == 60 &&
                WatchReactionAlertRule.ResolveEarlyWatchMinimumConfidence(63) == 60,
                "early WATCH keeps the established 60-floor and 4-point confidence gap");

            Assert(
                WatchReactionAlertRule.IsStrongWatch(
                    1,
                    78,
                    70,
                    6,
                    3,
                    2,
                    82,
                    65,
                    60,
                    5,
                    4,
                    3,
                    2) &&
                WatchReactionAlertRule.IsStrongWatch(
                    -1,
                    78,
                    70,
                    6,
                    3,
                    2,
                    82,
                    65,
                    60,
                    5,
                    4,
                    3,
                    2),
                "strong WATCH qualification is BUY/SELL symmetric");

            Assert(
                !WatchReactionAlertRule.IsStrongWatch(
                    1,
                    77,
                    70,
                    6,
                    3,
                    2,
                    82,
                    65,
                    60,
                    5,
                    4,
                    3,
                    2) &&
                !WatchReactionAlertRule.IsStrongWatch(
                    1,
                    78,
                    64,
                    6,
                    3,
                    2,
                    82,
                    65,
                    60,
                    5,
                    4,
                    3,
                    2),
                "WATCH rejects sub-threshold confidence and smart quality");

            Assert(
                WatchReactionAlertRule.IsWatchAlertEligible(
                    true,
                    true,
                    false,
                    false,
                    false,
                    false,
                    1,
                    78,
                    70,
                    6,
                    3,
                    2,
                    82,
                    65,
                    60,
                    5,
                    4,
                    3,
                    2) &&
                !WatchReactionAlertRule.IsWatchAlertEligible(
                    false,
                    true,
                    false,
                    false,
                    false,
                    false,
                    1,
                    78,
                    70,
                    6,
                    3,
                    2,
                    82,
                    65,
                    60,
                    5,
                    4,
                    3,
                    2) &&
                !WatchReactionAlertRule.IsWatchAlertEligible(
                    true,
                    false,
                    false,
                    false,
                    false,
                    false,
                    1,
                    78,
                    70,
                    6,
                    3,
                    2,
                    82,
                    65,
                    60,
                    5,
                    4,
                    3,
                    2),
                "WATCH alert requires an allowed, non-actionable decision and enabled alerting");

            Assert(
                !WatchReactionAlertRule.IsWatchAlertEligible(
                    true,
                    true,
                    false,
                    true,
                    false,
                    false,
                    1,
                    78,
                    70,
                    6,
                    3,
                    2,
                    82,
                    65,
                    60,
                    5,
                    4,
                    3,
                    2) &&
                !WatchReactionAlertRule.IsWatchAlertEligible(
                    true,
                    true,
                    false,
                    false,
                    true,
                    false,
                    1,
                    78,
                    70,
                    6,
                    3,
                    2,
                    82,
                    65,
                    60,
                    5,
                    4,
                    3,
                    2) &&
                !WatchReactionAlertRule.IsWatchAlertEligible(
                    true,
                    true,
                    false,
                    false,
                    false,
                    true,
                    1,
                    78,
                    70,
                    6,
                    3,
                    2,
                    82,
                    65,
                    60,
                    5,
                    4,
                    3,
                    2),
                "WATCH alert is suppressed by an existing plan, pending order or live position");

            Assert(
                WatchReactionAlertRule.IsReactionAlertEligible(
                    true,
                    true,
                    true,
                    false,
                    false,
                    false,
                    true,
                    1,
                    82,
                    3,
                    80,
                    2) &&
                WatchReactionAlertRule.IsReactionAlertEligible(
                    true,
                    true,
                    true,
                    false,
                    false,
                    false,
                    true,
                    -1,
                    82,
                    3,
                    80,
                    2),
                "REACTION alert qualification is BUY/SELL symmetric");

            Assert(
                !WatchReactionAlertRule.IsReactionAlertEligible(
                    false,
                    true,
                    true,
                    false,
                    false,
                    false,
                    true,
                    1,
                    82,
                    3,
                    80,
                    2) &&
                !WatchReactionAlertRule.IsReactionAlertEligible(
                    true,
                    false,
                    true,
                    false,
                    false,
                    false,
                    true,
                    1,
                    82,
                    3,
                    80,
                    2) &&
                !WatchReactionAlertRule.IsReactionAlertEligible(
                    true,
                    true,
                    false,
                    false,
                    false,
                    false,
                    true,
                    1,
                    82,
                    3,
                    80,
                    2) &&
                !WatchReactionAlertRule.IsReactionAlertEligible(
                    true,
                    true,
                    true,
                    true,
                    false,
                    false,
                    true,
                    1,
                    82,
                    3,
                    80,
                    2) &&
                !WatchReactionAlertRule.IsReactionAlertEligible(
                    true,
                    true,
                    true,
                    false,
                    false,
                    false,
                    false,
                    1,
                    82,
                    3,
                    80,
                    2),
                "REACTION alert is closed by disabled alerting, live-reaction disablement, blocked state, existing plan or invalid range state");

            Assert(
                WatchReactionAlertRule.BuildWatchAlertKey(100, 1) ==
                    WatchReactionAlertRule.BuildWatchAlertKey(100, 1) &&
                WatchReactionAlertRule.BuildReactionAlertKey(100, 1) ==
                    WatchReactionAlertRule.BuildReactionAlertKey(100, 1) &&
                WatchReactionAlertRule.BuildWatchAlertKey(100, 1) !=
                    WatchReactionAlertRule.BuildWatchAlertKey(100, -1) &&
                WatchReactionAlertRule.BuildReactionAlertKey(100, 1) !=
                    WatchReactionAlertRule.BuildReactionAlertKey(100, -1),
                "WATCH/REACTION alert identities are deterministic and direction-distinct");
        }

        private static void VerifyActionabilityThresholdTransparency()
        {
            ActionabilityThresholdSnapshot defaults =
                ActionabilityThresholdPolicy.ResolveFinal(
                    72,
                    70,
                    70,
                    72,
                    72,
                    4,
                    4,
                    4,
                    64,
                    72,
                    2.00);

            Assert(
                defaults.UpstreamEntryLocationQuality == 72 &&
                defaults.UpstreamEntryTimingQuality == 64,
                "actionability upstream effective defaults remain 72/64 above the named 64/64 hard floors");

            Assert(
                defaults.FinalMinimumConfidence == 76 &&
                defaults.FinalMinimumSmartQuality == 73 &&
                defaults.FinalMinimumTimeframeAgreement == 75 &&
                defaults.FinalMinimumIndependentEvidence == 5 &&
                defaults.FinalMinimumStructuralConfirmations == 5,
                "actionability final confidence/quality/evidence margins preserve current effective defaults");

            Assert(
                defaults.FinalMinimumEntryLocationQuality == 70 &&
                defaults.FinalMinimumEntryTimingQuality == 75 &&
                defaults.FinalMinimumEntryPositionQuality == 70 &&
                Math.Abs(defaults.FinalMinimumTp1RR - 2.00) < 1e-12,
                "actionability final entry and RR floors preserve current effective defaults");

            ActionabilityThresholdSnapshot smartOverrides =
                ActionabilityThresholdPolicy.ResolveFinal(
                    72,
                    70,
                    70,
                    60,
                    84,
                    4,
                    6,
                    4,
                    64,
                    72,
                    2.00);

            Assert(
                smartOverrides.FinalMinimumTimeframeAgreement == 87 &&
                smartOverrides.FinalMinimumIndependentEvidence == 7,
                "smart minimums retain precedence over lower base thresholds");

            ActionabilityThresholdSnapshot capped =
                ActionabilityThresholdPolicy.ResolveFinal(
                    99,
                    95,
                    95,
                    99,
                    99,
                    8,
                    8,
                    8,
                    95,
                    40,
                    2.00);

            Assert(
                capped.FinalMinimumConfidence == 99 &&
                capped.FinalMinimumSmartQuality == 95 &&
                capped.FinalMinimumTimeframeAgreement == 100 &&
                capped.FinalMinimumIndependentEvidence == 8 &&
                capped.FinalMinimumStructuralConfirmations == 8,
                "actionability threshold caps remain bounded");

            Assert(
                ActionabilityThresholdPolicy.EffectiveUpstreamEntryLocationQuality(40) == 64 &&
                ActionabilityThresholdPolicy.EffectivePrecisionEntryQualityFloor(35) == 40 &&
                ActionabilityThresholdPolicy.ApplyParallelCandidateQualityMargin(70, true) == 73 &&
                ActionabilityThresholdPolicy.ApplyParallelCandidateQualityMargin(70, false) == 70,
                "staged and parallel actionability threshold helpers are deterministic");

            ActionableSignalQualityResult accepted =
                ActionableSignalQualityRule.Evaluate(
                    new ActionableSignalQualityInput(
                        76,
                        73,
                        75,
                        5,
                        5,
                        70,
                        75,
                        70,
                        2.00,
                        defaults.FinalMinimumConfidence,
                        defaults.FinalMinimumSmartQuality,
                        defaults.FinalMinimumTimeframeAgreement,
                        defaults.FinalMinimumIndependentEvidence,
                        defaults.FinalMinimumStructuralConfirmations,
                        defaults.FinalMinimumEntryLocationQuality,
                        defaults.FinalMinimumEntryTimingQuality,
                        defaults.FinalMinimumEntryPositionQuality,
                        defaults.FinalMinimumTp1RR));

            ActionableSignalQualityResult belowFinalTiming =
                ActionableSignalQualityRule.Evaluate(
                    new ActionableSignalQualityInput(
                        76,
                        73,
                        75,
                        5,
                        5,
                        70,
                        74,
                        70,
                        2.00,
                        defaults.FinalMinimumConfidence,
                        defaults.FinalMinimumSmartQuality,
                        defaults.FinalMinimumTimeframeAgreement,
                        defaults.FinalMinimumIndependentEvidence,
                        defaults.FinalMinimumStructuralConfirmations,
                        defaults.FinalMinimumEntryLocationQuality,
                        defaults.FinalMinimumEntryTimingQuality,
                        defaults.FinalMinimumEntryPositionQuality,
                        defaults.FinalMinimumTp1RR));

            Assert(
                accepted.Allowed &&
                !belowFinalTiming.Allowed &&
                belowFinalTiming.Reason ==
                    "SIGNAL QUALITY • TIMING",
                "final actionability threshold application is deterministic at the acceptance boundary");

            Assert(
                defaults.FinalMinimumEntryLocationQuality ==
                    defaults.FinalMinimumEntryPositionQuality &&
                defaults.FinalMinimumEntryTimingQuality >=
                    defaults.FinalMinimumEntryLocationQuality,
                "entry quality floors remain direction-neutral and ordered");

            Assert(
                Math.Abs(
                    defaults.RecoveryTp1RRFloor -
                    2.35) < 1e-12,
                "quality-recovery RR margin remains 0.35 above the effective minimum");
        }

        private static void VerifySmartThresholdRegimeSemantics()
        {
            string[] expectedRegimes =
            {
                MarketRegimeIdentity.Trend,
                MarketRegimeIdentity.Expansion,
                MarketRegimeIdentity.Range,
                MarketRegimeIdentity.Compression,
                MarketRegimeIdentity.HighVolatility,
                MarketRegimeIdentity.Transition
            };

            foreach (string regime in expectedRegimes)
            {
                Assert(
                    MarketRegimeIdentity.IsKnown(regime),
                    "classifier regime is known: " + regime);

                SmartThresholdResolution adaptive =
                    SmartThresholdPolicyRule.ResolveSmartThresholds(
                        regime,
                        true,
                        70,
                        60,
                        10,
                        6);

                Assert(
                    adaptive.QualityThreshold >= 40 &&
                    adaptive.QualityThreshold <= 95 &&
                    adaptive.ShareThreshold >= 50 &&
                    adaptive.ShareThreshold <= 95 &&
                    adaptive.EdgeThreshold >= 4 &&
                    adaptive.EdgeThreshold <= 30,
                    "adaptive threshold result is bounded for regime: " + regime);
            }

            SmartThresholdResolution trend =
                SmartThresholdPolicyRule.ResolveSmartThresholds(
                    MarketRegimeIdentity.Trend,
                    true,
                    70,
                    60,
                    10,
                    6);

            SmartThresholdResolution expansion =
                SmartThresholdPolicyRule.ResolveSmartThresholds(
                    MarketRegimeIdentity.Expansion,
                    true,
                    70,
                    60,
                    10,
                    6);

            SmartThresholdResolution range =
                SmartThresholdPolicyRule.ResolveSmartThresholds(
                    MarketRegimeIdentity.Range,
                    true,
                    70,
                    60,
                    10,
                    6);

            SmartThresholdResolution compression =
                SmartThresholdPolicyRule.ResolveSmartThresholds(
                    MarketRegimeIdentity.Compression,
                    true,
                    70,
                    60,
                    10,
                    6);

            SmartThresholdResolution highVolatility =
                SmartThresholdPolicyRule.ResolveSmartThresholds(
                    MarketRegimeIdentity.HighVolatility,
                    true,
                    70,
                    60,
                    10,
                    6);

            SmartThresholdResolution transition =
                SmartThresholdPolicyRule.ResolveSmartThresholds(
                    MarketRegimeIdentity.Transition,
                    true,
                    70,
                    60,
                    10,
                    6);

            SmartThresholdResolution unknown =
                SmartThresholdPolicyRule.ResolveSmartThresholds(
                    MarketRegimeIdentity.Unknown,
                    true,
                    70,
                    60,
                    10,
                    6);

            SmartThresholdResolution future =
                SmartThresholdPolicyRule.ResolveSmartThresholds(
                    "FUTURE_REGIME",
                    true,
                    70,
                    60,
                    10,
                    6);

            Assert(
                trend.QualityThreshold == 64 &&
                trend.ShareThreshold == 58 &&
                trend.EdgeThreshold == 8 &&
                expansion.QualityThreshold == 64 &&
                expansion.ShareThreshold == 58 &&
                expansion.EdgeThreshold == 8,
                "TREND/EXPANSION adaptive branch preserves legacy -buffer defaults");

            Assert(
                range.QualityThreshold == 73 &&
                range.ShareThreshold == 62 &&
                range.EdgeThreshold == 12,
                "RANGE adaptive branch preserves legacy +buffer defaults");

            Assert(
                compression.QualityThreshold == 76 &&
                compression.ShareThreshold == 63 &&
                compression.EdgeThreshold == 13,
                "COMPRESSION adaptive branch preserves legacy +buffer defaults");

            Assert(
                highVolatility.QualityThreshold == 70 &&
                highVolatility.ShareThreshold == 60 &&
                highVolatility.EdgeThreshold == 10 &&
                transition.QualityThreshold == 70 &&
                transition.ShareThreshold == 60 &&
                transition.EdgeThreshold == 10,
                "HIGH_VOLATILITY/TRANSITION remain neutral adaptive branches");

            Assert(
                unknown.QualityThreshold == 70 &&
                unknown.ShareThreshold == 60 &&
                unknown.EdgeThreshold == 10 &&
                future.QualityThreshold == 70 &&
                future.ShareThreshold == 60 &&
                future.EdgeThreshold == 10,
                "UNKNOWN/future regime values fail safely to the base thresholds");

            Assert(
                MarketRegimeIdentity.NormalizeMarketRegime(" trend ") ==
                    MarketRegimeIdentity.Trend &&
                MarketRegimeIdentity.NormalizeMarketRegime("EXPANSION") ==
                    MarketRegimeIdentity.Expansion &&
                MarketRegimeIdentity.NormalizeMarketRegime("REVERSAL") ==
                    MarketRegimeIdentity.Unknown,
                "regime identity normalization excludes legacy REVERSAL");

            SmartThresholdResolution adaptiveOff =
                SmartThresholdPolicyRule.ResolveSmartThresholds(
                    MarketRegimeIdentity.Range,
                    false,
                    70,
                    60,
                    10,
                    6);

            Assert(
                adaptiveOff.QualityThreshold == 70 &&
                adaptiveOff.ShareThreshold == 60 &&
                adaptiveOff.EdgeThreshold == 10,
                "Adaptive Smart Thresholds=false preserves configured thresholds");

            Assert(
                trend.QualityThreshold == expansion.QualityThreshold &&
                trend.ShareThreshold == expansion.ShareThreshold &&
                trend.EdgeThreshold == expansion.EdgeThreshold,
                "regime threshold policy is direction-neutral and symmetric");
        }

        private static void VerifyEntryTrapRiskG2()
        {
            Assert(
                EntryTrapRiskPolicy.ResolveBlockReason(
                    false,
                    0,
                    78,
                    0.30,
                    0,
                    18) == EntryTrapRiskPolicy.AdverseM5Reason,
                "G2 M5 adverse threshold resolves to TRAP_ADVERSE_M5");

            Assert(
                EntryTrapRiskPolicy.ResolveBlockReason(
                    false,
                    0,
                    78,
                    0,
                    0.40,
                    75) == EntryTrapRiskPolicy.AdverseM1Reason,
                "G2 strong M1 adverse threshold resolves to TRAP_ADVERSE_M1");

            Assert(
                EntryTrapRiskPolicy.ResolveBlockReason(
                    true,
                    0,
                    78,
                    0,
                    0,
                    45) == EntryTrapRiskPolicy.ExtremeReason,
                "G2 extreme location resolves to TRAP_EXTREME");

            Assert(
                EntryTrapRiskPolicy.ResolveBlockReason(
                    false,
                    78,
                    78,
                    0,
                    0,
                    25) == EntryTrapRiskPolicy.DivergenceReason,
                "G2 opposing divergence resolves to TRAP_DIVERGENCE");

            Assert(
                EntryTrapRiskPolicy.ResolveRetestContext(
                    true,
                    true,
                    true,
                    false,
                    true) == "RETEST PRE-ZONE M5",
                "G2 Retest pre-zone M5 context is explicit");

            Assert(
                EntryTrapRiskPolicy.ResolveRetestContext(
                    true,
                    true,
                    false,
                    false,
                    true) == "RETEST POST-ZONE/REACTION",
                "G2 Retest post-zone reaction context is explicit");

            Assert(
                EntryTrapRiskPolicy.ResolveRetestContext(
                    false,
                    false,
                    true,
                    true,
                    true) == "NON_RETEST",
                "G2 non-Retest context never claims Retest semantics");

            EntryTrapRiskResult m5 =
                EntryTrapRiskRule.Evaluate(
                    1,
                    0.50,
                    0.30,
                    0,
                    0,
                    false,
                    true,
                    true,
                    true,
                    false,
                    true);

            Assert(
                m5.Block &&
                m5.Reason == EntryTrapRiskPolicy.AdverseM5Reason &&
                m5.Context == "RETEST PRE-ZONE M5",
                "G2 EntryTrapRiskRule preserves M5 block and pre-zone context");

            EntryTrapRiskResult m1 =
                EntryTrapRiskRule.Evaluate(
                    -1,
                    0.50,
                    0,
                    0.40,
                    0,
                    false,
                    true,
                    true,
                    false,
                    true,
                    true);

            Assert(
                m1.Block &&
                m1.Reason == EntryTrapRiskPolicy.AdverseM1Reason &&
                m1.Context == "RETEST PRE-ZONE M1",
                "G2 EntryTrapRiskRule preserves strong M1 block and context");
        }
        private static void VerifyRetestTriggerModeSemantics()
        {
            Assert(
                !EntryActionabilityPolicy.RequiresConfirmedTrigger(
                    ExecutionMode.RetestMarket,
                    true),
                "RetestMarket remains zone-driven when M5 confirmed-trigger mode is enabled");

            Assert(
                EntryActionabilityPolicy.RequiresConfirmedTrigger(
                    ExecutionMode.BreakoutMarket,
                    true) &&
                EntryActionabilityPolicy.RequiresConfirmedTrigger(
                    ExecutionMode.ContinuationStop,
                    true) &&
                EntryActionabilityPolicy.RequiresConfirmedTrigger(
                    ExecutionMode.ReversalLimit,
                    true),
                "breakout and predictive pending modes remain trigger-dependent");

            Assert(
                !EntryActionabilityPolicy.RequiresConfirmedTrigger(
                    ExecutionMode.BreakoutMarket,
                    false) &&
                !EntryActionabilityPolicy.RequiresConfirmedTrigger(
                    ExecutionMode.ContinuationStop,
                    false),
                "disabling M5 confirmed-trigger mode removes the trigger requirement");

            Decision retestDecision =
                new Decision
                {
                    Direction = 1,
                    EntryAllowed = true,
                    TriggerReady = false,
                    ActionableNow = true
                };

            TradeOpportunityCandidate retest =
                new TradeOpportunityCandidate
                {
                    Direction = 1,
                    Lane = OpportunityLane.Tactical,
                    ExecutionMode = ExecutionMode.RetestMarket,
                    ActionableNow = true,
                    PresentationOnly = false
                };

            ScenarioExecutionPolicyResult retestPolicy =
                ScenarioExecutionPolicyRule.Evaluate(
                    retest,
                    retestDecision,
                    OpportunityLane.Tactical,
                    true);

            Assert(
                retestPolicy.CandidateEligible &&
                retestPolicy.ExecutionAuthorized,
                "Retest scenario remains executable without generic TriggerReady");

            TradeOpportunityCandidate breakout =
                new TradeOpportunityCandidate
                {
                    Direction = 1,
                    Lane = OpportunityLane.Tactical,
                    ExecutionMode = ExecutionMode.BreakoutMarket,
                    ActionableNow = true,
                    PresentationOnly = false
                };

            ScenarioExecutionPolicyResult breakoutPolicy =
                ScenarioExecutionPolicyRule.Evaluate(
                    breakout,
                    retestDecision,
                    OpportunityLane.Tactical,
                    true);

            Assert(
                !breakoutPolicy.ExecutionAuthorized &&
                breakoutPolicy.ExecutionReason ==
                    "CANONICAL TRIGGER NOT READY",
                "Breakout scenario still requires confirmed trigger");

            Console.WriteLine(
                "Retest mode-aware trigger contracts PASS");
        }

        private static void VerifyEntryActionabilityF6()
        {
            Assert(
                EntryActionabilityPolicy.LongExtremeRangePosition == 0.85 &&
                EntryActionabilityPolicy.ShortExtremeRangePosition == 0.15 &&
                EntryActionabilityPolicy.LongNearExtremeRangePosition == 0.75 &&
                EntryActionabilityPolicy.ShortNearExtremeRangePosition == 0.25,
                "F6 range-location constants preserve established values");

            Assert(
                EntryActionabilityPolicy.AdverseM5BlockAtr == 0.30 &&
                EntryActionabilityPolicy.AdverseM1BlockAtr == 0.45 &&
                EntryActionabilityPolicy.StrongAdverseM5Atr == 0.45 &&
                EntryActionabilityPolicy.StrongAdverseM1Atr == 0.40 &&
                EntryActionabilityPolicy.MicroConflictAdverseM1Atr == 0.25 &&
                EntryActionabilityPolicy.MicroConflictEntryDistanceAtr == 0.10,
                "F6 adverse-momentum and micro-conflict constants preserve values");

            Assert(
                EntryActionabilityPolicy.DivergenceLowQuality == 70 &&
                EntryActionabilityPolicy.DivergenceMediumQuality == 78 &&
                EntryActionabilityPolicy.DivergenceHighQuality == 86 &&
                EntryActionabilityPolicy.DivergenceLowRisk == 16 &&
                EntryActionabilityPolicy.DivergenceMediumRisk == 25 &&
                EntryActionabilityPolicy.DivergenceHighRisk == 32 &&
                EntryActionabilityPolicy.SupportiveHiddenDivergenceRiskAdjustment == 10,
                "F6 divergence constants preserve established values");

            EntryTrapRiskResult buyExtreme =
                EntryTrapRiskRule.Evaluate(1, 0.85, 0, 0, 0, false);
            EntryTrapRiskResult sellExtreme =
                EntryTrapRiskRule.Evaluate(-1, 0.15, 0, 0, 0, false);
            Assert(
                buyExtreme.Block &&
                sellExtreme.Block &&
                buyExtreme.Risk == sellExtreme.Risk &&
                buyExtreme.Reason == sellExtreme.Reason,
                "F6 trap extreme-location semantics are BUY/SELL symmetric");

            EntryTrapRiskResult belowM5 =
                EntryTrapRiskRule.Evaluate(1, 0.50, 0.2999, 0, 0, false);
            EntryTrapRiskResult atM5 =
                EntryTrapRiskRule.Evaluate(1, 0.50, 0.30, 0, 0, false);
            Assert(
                !belowM5.Block &&
                atM5.Block &&
                atM5.Reason == "ADVERSE MOMENTUM",
                "F6 M5 trap threshold is inclusive at 0.30 ATR");

            EntryTrapRiskResult belowStrongM1 =
                EntryTrapRiskRule.Evaluate(1, 0.50, 0, 0.3999, 0, false);
            EntryTrapRiskResult atStrongM1 =
                EntryTrapRiskRule.Evaluate(1, 0.50, 0, 0.40, 0, false);
            EntryTrapRiskResult atM1BlockThreshold =
                EntryTrapRiskRule.Evaluate(1, 0.50, 0, 0.45, 0, false);
            Assert(
                !belowStrongM1.Block &&
                atStrongM1.Block &&
                atStrongM1.Reason == "ADVERSE MOMENTUM" &&
                atM1BlockThreshold.Block,
                "F6 M1 strong-adverse threshold is inclusive at 0.40 ATR and 0.45 remains blocked");

            EntryTrapRiskResult divergence70 =
                EntryTrapRiskRule.Evaluate(1, 0.50, 0, 0, 70, false);
            EntryTrapRiskResult divergence78 =
                EntryTrapRiskRule.Evaluate(1, 0.50, 0, 0, 78, false);
            Assert(
                !divergence70.Block &&
                divergence70.Risk == 16 &&
                divergence78.Block &&
                divergence78.Risk == 25,
                "F6 divergence actionability boundaries preserve values");

            EntryTrapRiskResult neutral =
                EntryTrapRiskRule.Evaluate(1, 0.50, 0, 0, 0, false);
            EntryTrapRiskResult noHiddenAtNearExtreme =
                EntryTrapRiskRule.Evaluate(1, 0.75, 0, 0, 0, false);
            EntryTrapRiskResult hiddenAtNearExtreme =
                EntryTrapRiskRule.Evaluate(1, 0.75, 0, 0, 0, true);
            Assert(
                neutral.Risk == 0 &&
                !neutral.Block &&
                noHiddenAtNearExtreme.Risk == 25 &&
                hiddenAtNearExtreme.Risk == 15 &&
                !hiddenAtNearExtreme.Block,
                "F6 supportive hidden divergence reduces trap risk by the established ten-point adjustment");

            EntryTrapRiskResult invalidDirection =
                EntryTrapRiskRule.Evaluate(0, 0.99, 1, 1, 100, false);
            Assert(
                !invalidDirection.Block &&
                invalidDirection.Risk == 0 &&
                invalidDirection.Reason == "NONE",
                "F6 invalid direction fails closed to no-trap state");

            Assert(
                !EntryActionabilityPolicy.ShouldBlockTrapRisk(
                    ExecutionMode.BreakoutMarket) &&
                EntryActionabilityPolicy.ShouldBlockTrapRisk(
                    ExecutionMode.RetestMarket) &&
                EntryActionabilityPolicy.ShouldBlockTrapRisk(
                    ExecutionMode.WaitingForTrigger),
                "F6 preserves intentional Breakout trap-risk bypass semantics");

            Assert(
                EntryActionabilityPolicy.IsRetestReady(true, false) &&
                !EntryActionabilityPolicy.IsRetestReady(true, true) &&
                !EntryActionabilityPolicy.IsRetestReady(false, false),
                "F6 Retest mode remains inside-zone and pre-trigger only");

            Assert(
                EntryActionabilityPolicy.ResolveAnchor(
                    ExecutionMode.BreakoutMarket,
                    110,
                    105,
                    100) == 110 &&
                EntryActionabilityPolicy.ResolveAnchor(
                    ExecutionMode.RetestMarket,
                    110,
                    105,
                    100) == 105 &&
                EntryActionabilityPolicy.ResolveAnchor(
                    ExecutionMode.WaitingForTrigger,
                    110,
                    105,
                    100) == 105 &&
                EntryActionabilityPolicy.ResolveAnchor(
                    ExecutionMode.RetestMarket,
                    0,
                    0,
                    100) == 100,
                "F6 anchor selection preserves Breakout/Retest semantics");

            Assert(
                EntryActionabilityPolicy.ResolveActualEntry(
                    ExecutionMode.BreakoutMarket,
                    120,
                    105) == 120 &&
                EntryActionabilityPolicy.ResolveActualEntry(
                    ExecutionMode.RetestMarket,
                    120,
                    105) == 120 &&
                EntryActionabilityPolicy.ResolveActualEntry(
                    ExecutionMode.WaitingForTrigger,
                    120,
                    105) == 105,
                "F6 actual-entry selection preserves market/waiting semantics");

            Assert(
                !EntryActionabilityPolicy.IsLate(
                    ExecutionMode.BreakoutMarket,
                    0.75,
                    0,
                    0.75,
                    0.45) &&
                EntryActionabilityPolicy.IsLate(
                    ExecutionMode.BreakoutMarket,
                    0.7501,
                    0,
                    0.75,
                    0.45) &&
                !EntryActionabilityPolicy.IsLate(
                    ExecutionMode.RetestMarket,
                    0,
                    0.45,
                    0.75,
                    0.45) &&
                EntryActionabilityPolicy.IsLate(
                    ExecutionMode.RetestMarket,
                    0,
                    0.4501,
                    0.75,
                    0.45),
                "F6 late-entry boundaries preserve strict-greater semantics");

            Assert(
                EntryActionabilityPolicy.IsMicroConflict(true, 0.25, 0) &&
                EntryActionabilityPolicy.IsMicroConflict(true, 0, 0.10) &&
                !EntryActionabilityPolicy.IsMicroConflict(true, 0.2499, 0.0999) &&
                !EntryActionabilityPolicy.IsMicroConflict(false, 1, 1),
                "F6 micro-conflict thresholds preserve existing boundaries");

            Assert(
                EntryActionabilityPolicy.ResolveTriggerTolerance(0.01, 0.02) == 0.01 &&
                EntryActionabilityPolicy.ResolveTriggerTolerance(0.001, 0.02) == 0.002,
                "F6 trigger tolerance preserves tick/pip precedence");

            Assert(
                EntryActionabilityPolicy.ResolveContinuationStructuralMinimum(2) == 3 &&
                EntryActionabilityPolicy.ResolveContinuationStructuralMinimum(4) == 4,
                "F6 continuation structural minimum remains bounded at three");

            Assert(
                IndicatorActionabilityRule.Evaluate(
                    MarketRegimeIdentity.Range,
                    58,
                    55).Allowed &&
                !IndicatorActionabilityRule.Evaluate(
                    MarketRegimeIdentity.Range,
                    57,
                    0).Allowed &&
                !IndicatorActionabilityRule.Evaluate(
                    MarketRegimeIdentity.Range,
                    58,
                    56).Allowed &&
                IndicatorActionabilityRule.Evaluate(
                    MarketRegimeIdentity.Trend,
                    60,
                    52).Allowed &&
                !IndicatorActionabilityRule.Evaluate(
                    MarketRegimeIdentity.Trend,
                    59,
                    0).Allowed &&
                !IndicatorActionabilityRule.Evaluate(
                    MarketRegimeIdentity.Trend,
                    60,
                    53).Allowed,
                "F6 indicator actionability thresholds preserve RANGE/TRANSITION and neutral values");
        }

        private static void VerifyExecutionPanelPresentationIdentityG6A()
        {
            string baseline =
                ExecutionPanelPresentationIdentityRule.Compose(
                    "ARMED",
                    "AWAITING EXECUTION",
                    "AUTO",
                    "ACCEPTED",
                    "SCENARIO-M5-BUY",
                    72,
                    "READY",
                    "MARKET SUITABLE",
                    "NOT EVALUATED");

            Assert(
                baseline ==
                ExecutionPanelPresentationIdentityRule.Compose(
                    "ARMED",
                    "AWAITING EXECUTION",
                    "AUTO",
                    "ACCEPTED",
                    "SCENARIO-M5-BUY",
                    72,
                    "READY",
                    "MARKET SUITABLE",
                    "NOT EVALUATED"),
                "G6A unchanged execution presentation identity is deterministic");

            Assert(
                baseline !=
                ExecutionPanelPresentationIdentityRule.Compose(
                    "EXECUTED",
                    "AWAITING EXECUTION",
                    "AUTO",
                    "ACCEPTED",
                    "SCENARIO-M5-BUY",
                    72,
                    "READY",
                    "MARKET SUITABLE",
                    "NOT EVALUATED") &&
                baseline !=
                ExecutionPanelPresentationIdentityRule.Compose(
                    "ARMED",
                    "BLOCKED • RISK",
                    "AUTO",
                    "ACCEPTED",
                    "SCENARIO-M5-BUY",
                    72,
                    "READY",
                    "MARKET SUITABLE",
                    "NOT EVALUATED"),
                "G6A auto-trading state/reason changes alter presentation identity");

            Assert(
                baseline !=
                ExecutionPanelPresentationIdentityRule.Compose(
                    "ARMED",
                    "AWAITING EXECUTION",
                    "RECOVERY",
                    "REQUIRED",
                    "SCENARIO-M5-BUY",
                    72,
                    "READY",
                    "MARKET SUITABLE",
                    "NOT EVALUATED") &&
                baseline !=
                ExecutionPanelPresentationIdentityRule.Compose(
                    "ARMED",
                    "AWAITING EXECUTION",
                    "AUTO",
                    "ACCEPTED",
                    "SCENARIO-M5-SELL",
                    72,
                    "READY",
                    "MARKET SUITABLE",
                    "NOT EVALUATED"),
                "G6A telemetry/scenario changes alter presentation identity");

            Assert(
                baseline !=
                ExecutionPanelPresentationIdentityRule.Compose(
                    "ARMED",
                    "AWAITING EXECUTION",
                    "AUTO",
                    "ACCEPTED",
                    "SCENARIO-M5-BUY",
                    73,
                    "READY",
                    "MARKET SUITABLE",
                    "NOT EVALUATED") &&
                baseline !=
                ExecutionPanelPresentationIdentityRule.Compose(
                    "ARMED",
                    "AWAITING EXECUTION",
                    "AUTO",
                    "ACCEPTED",
                    "SCENARIO-M5-BUY",
                    72,
                    "BLOCKED",
                    "SPREAD",
                    "APPLIED  •  LOCKED"),
                "G6A market-suitability/break-even changes alter presentation identity");

            Assert(
                ExecutionPanelPresentationIdentityRule.Compose(
                    null,
                    null,
                    null,
                    null,
                    null,
                    0,
                    null,
                    null,
                    null) ==
                "|||||0|||",
                "G6A null presentation fields normalize deterministically");

            Console.WriteLine(
                "CR7.6a / G6A execution panel presentation identity contract PASS");
        }

        private static void VerifyExecutionProtectionPanelStateFreshnessG5()
        {
            DateTime baseline =
                new DateTime(
                    2026,
                    10,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                BrokerStateRefreshRule.IsRefreshDue(
                    true,
                    baseline,
                    baseline.AddMilliseconds(1),
                    1000),
                "G5 dirty broker state forces immediate refresh");

            Assert(
                BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    DateTime.MinValue,
                    baseline,
                    1000),
                "G5 never-refreshed broker state is immediately due");

            Assert(
                !BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    baseline,
                    baseline.AddMilliseconds(999),
                    1000),
                "G5 unchanged broker state stays cacheable before refresh interval");

            Assert(
                BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    baseline,
                    baseline.AddMilliseconds(1000),
                    1000),
                "G5 broker refresh occurs at the bounded freshness interval");

            Assert(
                BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    baseline.AddSeconds(1),
                    baseline,
                    1000),
                "G5 clock regression refreshes instead of serving stale broker state");
        }

        private static void VerifyExecutionControlPresentationG6B()
        {
            Assert(
                !ExecutionControlPresentationRule.IsInteractive,
                "G6B execution controls are status-only and cannot own execution mutations");

            Assert(
                ExecutionControlPresentationRule.ComposeStatusText(
                    "AUTO TRADE",
                    true) ==
                "AUTO TRADE  •  ON" &&
                ExecutionControlPresentationRule.ComposeStatusText(
                    "AUTO ORDERS",
                    false) ==
                "AUTO ORDERS  •  OFF",
                "G6B execution-control status text is deterministic and reflects canonical state");

            Assert(
                ExecutionControlPresentationRule.ComposeStatusText(
                    null,
                    true) ==
                "EXECUTION  •  ON" &&
                ExecutionControlPresentationRule.ComposeStatusText(
                    "  ",
                    false) ==
                "EXECUTION  •  OFF",
                "G6B blank captions normalize deterministically");

            Console.WriteLine(
                "CR7.6b / G6B execution-control presentation contract PASS");
        }

        private static void VerifyExecutionProtectionPanelStateG4()
        {
            Assert(
                ExecutionProtectionPanelStateRule.ResolveAutoTrading(
                    false,
                    false,
                    false,
                    false,
                    false) ==
                    ExecutionPanelStateKind.Disabled,
                "G4 Auto Trade disabled state is OFF");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveAutoTrading(
                    true,
                    false,
                    false,
                    false,
                    false) ==
                    ExecutionPanelStateKind.Armed,
                "G4 Auto Trade enabled without a live action is ARMED");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveAutoTrading(
                    true,
                    false,
                    true,
                    false,
                    false) ==
                    ExecutionPanelStateKind.Ready,
                "G4 Auto Trade readiness is distinct from analysis readiness");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveAutoTrading(
                    true,
                    false,
                    false,
                    true,
                    false) ==
                    ExecutionPanelStateKind.Blocked,
                "G4 Auto Trade blocked state is explicit");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveAutoTrading(
                    true,
                    true,
                    true,
                    false,
                    false) ==
                    ExecutionPanelStateKind.Active,
                "G4 live Auto Trade state is ACTIVE");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveAutoTrading(
                    true,
                    true,
                    true,
                    false,
                    true) ==
                    ExecutionPanelStateKind.RecoveryRequired,
                "G4 recovery state overrides active Auto Trade state");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveAutoOrders(
                    false,
                    false,
                    false,
                    false,
                    false) ==
                    ExecutionPanelStateKind.Disabled,
                "G4 Auto Orders disabled state is OFF");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveAutoOrders(
                    true,
                    false,
                    false,
                    false,
                    false) ==
                    ExecutionPanelStateKind.Armed,
                "G4 Auto Orders enabled without a pending order is ARMED");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveAutoOrders(
                    true,
                    false,
                    true,
                    false,
                    false) ==
                    ExecutionPanelStateKind.Ready,
                "G4 Auto Orders READY state is explicit");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveAutoOrders(
                    true,
                    true,
                    false,
                    false,
                    false) ==
                    ExecutionPanelStateKind.Active,
                "G4 managed pending order makes Auto Orders ACTIVE");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false) ==
                    ProtectionPanelStateKind.Off,
                "G4 unconfigured protection with no live position is OFF");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    true,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false) ==
                    ProtectionPanelStateKind.NoLivePosition,
                "G4 configured protection without a live position is explicit");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    true,
                    true,
                    true,
                    true,
                    true,
                    false,
                    false) ==
                    ProtectionPanelStateKind.Protected,
                "G4 valid broker SL/TP is PROTECTED");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    true,
                    true,
                    true,
                    false,
                    false,
                    false,
                    false) ==
                    ProtectionPanelStateKind.Protected,
                "G4 TP is not required when target synchronization is disabled");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    true,
                    true,
                    true,
                    true,
                    false,
                    true,
                    false) ==
                    ProtectionPanelStateKind.Protected,
                "G4 server-owned TP ladder satisfies target protection");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    true,
                    true,
                    true,
                    true,
                    false,
                    false,
                    false) ==
                    ProtectionPanelStateKind.RecoveryRequired,
                "G4 missing required broker TP is RECOVERY REQUIRED");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    true,
                    true,
                    false,
                    true,
                    true,
                    false,
                    false) ==
                    ProtectionPanelStateKind.RecoveryRequired,
                "G4 invalid broker SL is RECOVERY REQUIRED");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    true,
                    true,
                    true,
                    true,
                    true,
                    false,
                    true) ==
                    ProtectionPanelStateKind.RecoveryRequired,
                "G4 explicit recovery state remains RECOVERY REQUIRED");

            Assert(
                ExecutionProtectionPanelStateRule.ResolveProtection(
                    true,
                    true,
                    true,
                    true,
                    true,
                    false,
                    false) ==
                    ProtectionPanelStateKind.Protected,
                "G4 broker-protection rule is independent of BUY/SELL analysis readiness");

            Console.WriteLine(
                "CR7.4 / G4 panel execution/protection state contract PASS");
        }

        private static void VerifyPlanLineThicknessG3()
        {
            Assert(
                PlanLinePresentationRule.ResolveThickness(1) == 1 &&
                PlanLinePresentationRule.ResolveThickness(2) == 1 &&
                PlanLinePresentationRule.ResolveThickness(3) == 1,
                "G3 signal/plan line presentation is always one pixel");

            Assert(
                PlanLinePresentationRule.ResolveThickness(0) == 1 &&
                PlanLinePresentationRule.ResolveThickness(4) == 1,
                "G3 resolver remains deterministic outside the legacy configurable range");
        }

        private static void VerifyTargetObstacleCacheKeyHashSemantics()
        {
            TargetObstacleCacheKey first =
                new TargetObstacleCacheKey(
                    800,
                    798,
                    638316000000000000L,
                    1,
                    3,
                    64,
                    32,
                    true,
                    0.10,
                    0.0001);

            TargetObstacleCacheKey equal =
                new TargetObstacleCacheKey(
                    800,
                    798,
                    638316000000000000L,
                    1,
                    3,
                    64,
                    32,
                    true,
                    0.10,
                    0.0001);

            TargetObstacleCacheKey differentDirection =
                new TargetObstacleCacheKey(
                    800,
                    798,
                    638316000000000000L,
                    -1,
                    3,
                    64,
                    32,
                    true,
                    0.10,
                    0.0001);

            Assert(
                first.Equals(equal) &&
                first.GetHashCode() == equal.GetHashCode(),
                "TargetObstacleCacheKey equal values must produce equal hashes");

            Assert(
                !first.Equals(differentDirection),
                "TargetObstacleCacheKey direction participates in equality");
        }

        private static void VerifyBrokerProtectionG1()
        {
            Assert(
                ManagedStopProtectionRule.IsExistingStopHealthy(
                    1,
                    100.0,
                    99.9999),
                "G1 BUY near-market existing stop remains directionally protective");

            Assert(
                ManagedStopProtectionRule.IsExistingStopHealthy(
                    -1,
                    100.0,
                    100.0001),
                "G1 SELL near-market existing stop remains directionally protective");

            Assert(
                !ManagedStopProtectionRule.IsExistingStopHealthy(
                    1,
                    100.0,
                    100.0001) &&
                !ManagedStopProtectionRule.IsExistingStopHealthy(
                    -1,
                    100.0,
                    99.9999),
                "G1 wrong-sided existing stops are not healthy");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    99.0,
                    99.5) &&
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    99.0,
                    98.5) &&
                ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    101.0,
                    100.5) &&
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    101.0,
                    101.5),
                "G1 existing-stop replacement remains protective-only for BUY/SELL");

            Assert(
                !ManagedStopProtectionRule.IsExistingStopHealthy(
                    0,
                    100.0,
                    99.0),
                "G1 invalid direction is fail-closed");
        }


        private static void VerifyPrimitiveIndicatorMathematics()
        {
            Assert(
                Math.Abs(
                    DmiBiasRule.Calculate(
                        30,
                        10) -
                    0.5) < 1e-12 &&
                Math.Abs(
                    DmiBiasRule.Calculate(
                        10,
                        30) +
                    0.5) < 1e-12 &&
                DmiBiasRule.Calculate(
                    double.NaN,
                    10) == 0,
                "DMI bias is normalized, finite and directionally symmetric");

            Assert(
                MacdBiasRule.IsMacdDirectional(
                    1,
                    0.80,
                    0.40) &&
                !MacdBiasRule.IsMacdDirectional(
                    1,
                    0.40,
                    0.80) &&
                MacdBiasRule.IsMacdDirectional(
                    -1,
                    -0.80,
                    -0.40) &&
                !MacdBiasRule.IsMacdDirectional(
                    -1,
                    -0.40,
                    -0.80),
                "MACD-line bias is directionally symmetric without histogram mislabeling");

            Assert(
                RangeEfficiencyRule.ResolveFirstCloseIndex(
                    50,
                    20) == 30 &&
                !RangeEfficiencyRule.HasRangeEnoughHistory(
                    19,
                    20) &&
                RangeEfficiencyRule.HasRangeEnoughHistory(
                    20,
                    20),
                "RangeEfficiency requires the full configured interval window");

            Assert(
                Math.Abs(
                    RangeEfficiencyRule.Evaluate(
                        4,
                        8) -
                    0.5) < 1e-12 &&
                RangeEfficiencyRule.Evaluate(
                    8,
                    4) == 1 &&
                RangeEfficiencyRule.Evaluate(
                    1,
                    0) == 0,
                "RangeEfficiency uses one coherent net-move/path ratio");

            Assert(
                ChoppinessIndexRule.ResolveFirstBarIndex(
                    50,
                    20) == 31 &&
                !ChoppinessIndexRule.HasChoppinessEnoughHistory(
                    18,
                    20) &&
                ChoppinessIndexRule.HasChoppinessEnoughHistory(
                    19,
                    20),
                "Choppiness requires the full configured bar window");

            double expectedChoppiness =
                100.0 *
                Math.Log10(10.0 / 5.0) /
                Math.Log10(10.0);

            Assert(
                Math.Abs(
                    ChoppinessIndexRule.Evaluate(
                        10,
                        5,
                        10) -
                    expectedChoppiness) < 1e-12,
                "Choppiness implements the canonical logarithmic ratio");

            Assert(
                VwapBiasRule.IsVwapDirectional(
                    101,
                    100,
                    1) &&
                !VwapBiasRule.IsVwapDirectional(
                    99,
                    100,
                    1) &&
                VwapBiasRule.IsVwapDirectional(
                    99,
                    100,
                    -1) &&
                !VwapBiasRule.IsVwapDirectional(
                    101,
                    100,
                    -1),
                "VWAP directional bias is mirrored");

            Assert(
                VwapBiasRule.AccumulateVolume(
                    5,
                    0) == 5 &&
                VwapBiasRule.AccumulatePriceVolume(
                    7,
                    100,
                    0) == 7,
                "VWAP zero-volume samples do not receive artificial unit weight");

            Assert(
                VolumeExpansionRule.IsExpanded(
                    true,
                    true,
                    120,
                    100,
                    1.10) &&
                !VolumeExpansionRule.IsExpanded(
                    true,
                    true,
                    109,
                    100,
                    1.10) &&
                !VolumeExpansionRule.IsExpanded(
                    true,
                    false,
                    120,
                    100,
                    1.10),
                "volume expansion requires both price structure and configured volume ratio");

            Console.WriteLine(
                "CI-01 primitive indicator mathematics contracts PASS");
        }

        private static void VerifyOssQuoteProjectionSemantics()
        {
            Assert(
                OssQuoteProjectionRule.NormalizeVolume(
                    0) == 0m &&
                OssQuoteProjectionRule.NormalizeVolume(
                    -1) == 0m &&
                OssQuoteProjectionRule.NormalizeVolume(
                    25.5) == 25.5m,
                "OSS quote volume normalization preserves positive values and zero");

            Assert(
                OssQuoteProjectionRule.NormalizeVolume(
                    double.NaN) == 0m &&
                OssQuoteProjectionRule.NormalizeVolume(
                    double.PositiveInfinity) == 0m &&
                OssQuoteProjectionRule.NormalizeVolume(
                    double.NegativeInfinity) == 0m,
                "OSS quote volume normalization is finite and fail-closed");

            Console.WriteLine(
                "CI-02 OSS quote projection semantics contracts PASS");
        }

        private static void VerifyOssQuoteWindowSemantics()
        {
            Assert(
                OssQuoteWindowRule.ResolveFirstIndex(
                    100,
                    161) == 0 &&
                OssQuoteWindowRule.ResolveWindowCount(
                    100,
                    161) == 101,
                "OSS rolling window keeps all available bars before the cap");

            Assert(
                OssQuoteWindowRule.ResolveFirstIndex(
                    160,
                    161) == 0 &&
                OssQuoteWindowRule.ResolveWindowCount(
                    160,
                    161) == 161 &&
                OssQuoteWindowRule.ResolveFirstIndex(
                    161,
                    161) == 1 &&
                OssQuoteWindowRule.ResolveWindowCount(
                    161,
                    161) == 161,
                "OSS rolling window becomes exactly bounded at the configured size");

            Assert(
                OssQuoteWindowRule.RequiresRebuild(
                    160,
                    0,
                    -1,
                    -1) &&
                !OssQuoteWindowRule.RequiresRebuild(
                    161,
                    1,
                    160,
                    0) &&
                !OssQuoteWindowRule.RequiresRebuild(
                    160,
                    0,
                    159,
                    0),
                "OSS cache allows initial construction and one-bar append without rebuild");

            Assert(
                OssQuoteWindowRule.RequiresRebuild(
                    162,
                    2,
                    160,
                    0) &&
                OssQuoteWindowRule.RequiresRebuild(
                    159,
                    0,
                    160,
                    0) &&
                OssQuoteWindowRule.RequiresRebuild(
                    162,
                    3,
                    161,
                    1),
                "OSS cache rebuilds on skipped, backward or non-contiguous window movement");

            Assert(
                OssQuoteWindowRule.IsBoundedCount(
                    768,
                    768) &&
                OssQuoteWindowRule.IsBoundedCount(
                    161,
                    161) &&
                !OssQuoteWindowRule.IsBoundedCount(
                    769,
                    768) &&
                !OssQuoteWindowRule.IsBoundedCount(
                    -1,
                    768),
                "OSS cached quote collections remain bounded");

            Console.WriteLine(
                "CI-02 OSS quote-window semantics contracts PASS");
        }

        private static void VerifyCi16DeterministicReplay()
        {
            DeterministicReplaySuite.Verify();
        }
        private static void VerifyOssWarmupPolicy()
        {
            Assert(
                OssIndicatorWarmupPolicy.StableQuoteWindowSize == 768,
                "H3-B stable quote window is fixed and bounded");

            Assert(
                OssIndicatorWarmupPolicy.RecommendedStableBars(
                    50,
                    100,
                    9,
                    10) == 500,
                "H3-B maximum current public envelope requires <= 500 recommended bars");

            Assert(
                OssIndicatorWarmupPolicy.FitsCurrentPublicParameterEnvelope(
                    50,
                    100,
                    9,
                    10),
                "H3-B current public parameter envelope fits inside bounded window");

            Assert(
                OssIndicatorWarmupPolicy.RecommendedStableBars(
                    14,
                    26,
                    9,
                    10) >= 260,
                "H3-B default warm-up includes SuperTrend convergence margin");

            Assert(
                OssIndicatorWarmupPolicy.RecommendedStableBars(
                    14,
                    26,
                    9,
                    10) == 285,
                "H3-B default warm-up includes MACD convergence margin");

            Console.WriteLine(
                "CR8.3b / H3-B warm-up policy contract PASS");
        }

        private static void VerifyMtfPrimaryTimeframeSignals()
        {
            PrimaryTimeframeSignalResult both =
                PrimaryTimeframeSignalRule.Evaluate(
                    "M15",
                    1,
                    80,
                    70,
                    1,
                    true,
                    1,
                    true);

            Assert(
                both.Allowed &&
                both.M5Aligned &&
                both.M1Confirmed &&
                both.State.Contains("PRIMARY M15") &&
                both.State.Contains("M5 ALIGNED") &&
                both.State.Contains("M1 CONFIRMED"),
                "M15 primary signal keeps its source direction and accepts aligned M5/M1 tuning");

            PrimaryTimeframeSignalResult h1Conflict =
                PrimaryTimeframeSignalRule.Evaluate(
                    "H1",
                    -1,
                    82,
                    70,
                    1,
                    true,
                    1,
                    false);

            Assert(
                h1Conflict.Allowed &&
                !h1Conflict.M5Aligned &&
                !h1Conflict.M1Confirmed &&
                h1Conflict.State.Contains("PRIMARY H1") &&
                h1Conflict.State.Contains("LTF TUNING PENDING"),
                "H1 primary signal remains observable when M5 is opposite and M1 is not confirmed");

            PrimaryTimeframeSignalResult wrongTimeframe =
                PrimaryTimeframeSignalRule.Evaluate(
                    "M30",
                    1,
                    90,
                    70,
                    1,
                    false,
                    0,
                    false);

            Assert(
                !wrongTimeframe.Allowed &&
                wrongTimeframe.Reason == "SOURCE TIMEFRAME IS NOT M15/H1",
                "only M15 and H1 may be primary timeframe signal sources");

            PrimaryTimeframeSignalResult weak =
                PrimaryTimeframeSignalRule.Evaluate(
                    "M15",
                    1,
                    69,
                    70,
                    1,
                    false,
                    0,
                    false);

            Assert(
                !weak.Allowed &&
                weak.Reason == "SOURCE QUALITY BELOW PRIMARY FLOOR",
                "primary source quality floor remains deterministic and explicit");

            Console.WriteLine(
                "MTF-P1 primary timeframe signal contracts PASS");
        }

        private static void VerifyMtfPrimaryLocationEvidence()
        {
            LocationEvidenceScore confluence =
                LocationEvidenceRule.Evaluate(
                    true,
                    90,
                    true,
                    85,
                    true);

            Assert(
                confluence.Confluence &&
                confluence.Score > 0 &&
                confluence.Evidence == 1,
                "canonical location evidence preserves explicit OB+FVG confluence");

            LocationEvidenceScore fvgOnly =
                LocationEvidenceRule.Evaluate(
                    true,
                    90,
                    false,
                    0,
                    false);

            LocationEvidenceScore obOnly =
                LocationEvidenceRule.Evaluate(
                    false,
                    0,
                    true,
                    85,
                    false);

            Assert(
                fvgOnly.Score > 0 &&
                obOnly.Score > 0 &&
                confluence.Score > fvgOnly.Score &&
                confluence.Score > obOnly.Score,
                "canonical OB+FVG location evidence remains stronger than single-zone evidence");

            Console.WriteLine(
                "MTF-P2 primary location OB/FVG contracts PASS");
        }


        private static void VerifyMtfPrimaryProviderIdentity()
        {
            TradeOpportunityCandidate m15 =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-M15-BUY",
                    SourceTimeframe = "M15",
                    BasePlanTimeframe = "M5",
                    IsPrimaryTimeframeSignal = true
                };

            TradeOpportunityCandidate h1 =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-H1-SELL",
                    SourceTimeframe = "H1",
                    BasePlanTimeframe = "M5",
                    IsPrimaryTimeframeSignal = true
                };

            TradeOpportunityCandidate m5 =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "CANONICAL-TACTICAL-BUY",
                    SourceTimeframe = "M5",
                    BasePlanTimeframe = "M5"
                };

            Assert(
                ProviderScenarioIdentityRule.ResolveScenarioId(
                    m15,
                    "FALLBACK") == "TF-M15-BUY" &&
                ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                    m15) == "M15",
                "provider identity preserves M15 scenario and source timeframe");

            Assert(
                ProviderScenarioIdentityRule.ResolveScenarioId(
                    h1,
                    "FALLBACK") == "TF-H1-SELL" &&
                ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                    h1) == "H1",
                "provider identity preserves H1 scenario and source timeframe");

            Assert(
                ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                    m5) == "M5" &&
                ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                    null,
                    "") == "M5",
                "provider identity falls back deterministically to canonical M5");

            Assert(
                ProviderScenarioIdentityRule.ResolveScenarioId(
                    null,
                    "CFIP-SC1|fallback") == "CFIP-SC1|fallback",
                "provider identity preserves explicit fallback scenario id");

            Console.WriteLine(
                "MTF-P3 provider scenario identity contracts PASS");
        }


        private static void VerifyPanelFrameDirectionPresentation()
        {
            Assert(
                PanelFrameDirectionRule.ResolveDisplayDirection(
                    1,
                    20,
                    10,
                    false,
                    false) == 1 &&
                PanelFrameDirectionRule.ResolveLabel(1, 1) == "BUY",
                "resolved BUY direction remains authoritative in panel");

            Assert(
                PanelFrameDirectionRule.ResolveDisplayDirection(
                    0,
                    28,
                    10,
                    true,
                    false) == 1 &&
                PanelFrameDirectionRule.ResolveLabel(0, 1) == "BULL BIAS",
                "unresolved bullish frame is shown as bullish bias instead of misleading neutral");

            Assert(
                PanelFrameDirectionRule.ResolveDisplayDirection(
                    0,
                    8,
                    24,
                    false,
                    true) == -1 &&
                PanelFrameDirectionRule.ResolveLabel(0, -1) == "BEAR BIAS",
                "unresolved bearish frame is shown as bearish bias");

            Assert(
                PanelFrameDirectionRule.ResolveDisplayDirection(
                    0,
                    12,
                    12,
                    false,
                    false) == 0 &&
                PanelFrameDirectionRule.ResolveLabel(0, 0) == "NEUTRAL",
                "only genuinely balanced unresolved frames remain neutral");

            Console.WriteLine(
                "Panel frame direction presentation contracts PASS");
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Runtime acceptance contract failed: " +
                    name);
        }
    }
}