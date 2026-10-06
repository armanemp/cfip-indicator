# CFIP-ind — جامع فهرست و ممیزی زنجیره تحلیل تا ترید

> سند مرجع زنده پروژه CFIP برای inventory، مالکیت اجزا، ممیزی ریاضی/معماری و ثبت یافته‌های تحقیقاتی. در هر دور ممیزی این فایل باید تکمیل شود؛ هیچ behavior جدیدی بدون تعیین owner اضافه نشود.

## 0. معماری مرجع
- **Pre-Analysis:** Bars/Ticks/MarketDepth/MarketHours، symbol/price/time/spread، session/news، MTF context، readiness و data-quality.
- **Analysis:** technical indicators، regime، structure، liquidity، FVG/OB، reaction، volume/flow، divergence، VWAP/WaveTrend و independent evidence.
- **Decision:** evidence normalization، timeframe agreement، scoring، confidence، consensus، market/structure/lifecycle gates و actionability. **M15 مرجع تصمیم؛ M5 trigger/precision؛ M1 اختیاری؛ H1+ context. M2 ممنوع.**
- **Planning:** scenario، entry/trigger، structural SL، target ladder، reward path، obstacle scan، invalidation و execution intent.
- **Risk:** account risk، symbol-native sizing، margin، capacity، daily-loss، suitability، spread/slippage و protection.
- **Execution:** Indicator تحلیل/intent/contract را تولید می‌کند؛ **cBot تنها مالک broker mutation** است.
- **Management:** broker-confirmed lifecycle، protection، BE/trailing، partial TP، reversal/invalidation، reconciliation.
- **Post-Trade:** outcome، telemetry، trace/archive، calibration و feedback؛ بدون آلوده‌کردن تصمیم جاری.

## 1. یافته‌های معتبر وب — مبنای فنی
- cTrader MarketDepth، Bid/Ask entries، قیمت و `VolumeInUnits` و رویداد `Updated` را ارائه می‌کند؛ بنابراین DOM باید **depth/liquidity** نامیده شود، نه executed trade volume. citeturn0search0turn0search3
- cTrader MarketData دسترسی به Bars، Ticks و MarketDepth دارد و Bars را می‌توان برای timeframe و symbol مشخص گرفت؛ این مبنای MTF runtime است. citeturn0search4turn0search6
- Spot FX عمدتاً OTC، غیرمتمرکز و fragmented است؛ بنابراین «real volume جهانی» برای spot XAUUSD/FX وجود ندارد. broker tick count، broker DOM و exchange futures volume باید از هم تفکیک شوند. citeturn0search2turn0search36
- cTrader برای sizing، مشخصات symbol-native مانند `NormalizeVolumeInUnits`، min/max/step volume و `GetEstimatedMargin` دارد؛ XAU نباید با multiplier ثابت و حدسی سایزبندی شود. citeturn0search8
- cTrader صراحتاً cBot را محل اجرای عملیات trading، position/order management و risk controls معرفی می‌کند؛ این با مرزبندی Indicator/cBot CFIP هم‌راستاست. citeturn0search11

## 2. فهرست واقعی سورس
### Primitive / Technical Indicators (23)
- `src/CFIP.Indicator/Analysis/Indicators/AverageDirectionalIndex.cs`
- `src/CFIP.Indicator/Analysis/Indicators/AverageTrueRange.cs`
- `src/CFIP.Indicator/Analysis/Indicators/DirectionalMovementIndex.cs`
- `src/CFIP.Indicator/Analysis/Indicators/ExponentialMovingAverage.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorConfluenceAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorParameters.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorSnapshotCache.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteCacheEntry.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteSeriesCache.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/SkenderAroon.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/SkenderBollingerBands.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/SkenderCci.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/SkenderMacd.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/SkenderMfi.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/SkenderObv.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/SkenderParabolicSar.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/SkenderRsi.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/SkenderStoch.cs`
- `src/CFIP.Indicator/Analysis/Indicators/External/SkenderSuperTrend.cs`
- `src/CFIP.Indicator/Analysis/Indicators/MacdIndicator.cs`
- `src/CFIP.Indicator/Analysis/Indicators/Native/Native.cs`
- `src/CFIP.Indicator/Analysis/Indicators/NativeIndicatorRegistry.cs`
- `src/CFIP.Indicator/Analysis/Indicators/RelativeStrengthIndex.cs`

### Market Analysis (31)
- `src/CFIP.Indicator/Analysis/Market/ChoppinessIndexAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/DivergenceAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/FuturePendingOpportunityRuntime.cs`
- `src/CFIP.Indicator/Analysis/Market/HealthyVolatilityAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/LiveBiasAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/M5RegimeCoreCache.cs`
- `src/CFIP.Indicator/Analysis/Market/MacdBiasAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/MarketFrameAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/MarketFrameEvidence.cs`
- `src/CFIP.Indicator/Analysis/Market/MarketFrameScoring.cs`
- `src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs`
- `src/CFIP.Indicator/Analysis/Market/MarketRegimeAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/MarketRegimeBarFingerprint.cs`
- `src/CFIP.Indicator/Analysis/Market/MarketRegimeFrameCache.cs`
- `src/CFIP.Indicator/Analysis/Market/MarketRegimeFrameCacheEntry.cs`
- `src/CFIP.Indicator/Analysis/Market/MarketStateSnapshotBuilder.cs`
- `src/CFIP.Indicator/Analysis/Market/Math/IndexMath.cs`
- `src/CFIP.Indicator/Analysis/Market/Models/Frame.cs`
- `src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs`
- `src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs`
- `src/CFIP.Indicator/Analysis/Market/ParallelScenarioComputation.cs`
- `src/CFIP.Indicator/Analysis/Market/PremiumDiscountAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/RangeEfficiencyAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/RangeSignalQualityEvaluator.cs`
- `src/CFIP.Indicator/Analysis/Market/ScenarioEvidenceEnrichment.cs`
- `src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs`
- `src/CFIP.Indicator/Analysis/Market/VolumeExpansionAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/VolumeProfileAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/VwapBiasAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/WaveTrendEngine.cs`
- `src/CFIP.Indicator/Analysis/Market/WaveTrendEvidenceAnalyzer.cs`

### Decision / Consensus (39)
- `src/CFIP.Indicator/Analysis/Market/Decision/ConfidenceCalibrationCollector.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionConfidenceCalculator.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionConfirmationGates.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionConsensusCalculator.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionConsensusSnapshot.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvaluator.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvidenceSnapshot.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionFilterResult.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionFilters.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionFrameContribution.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionFrameContributionCalculator.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputBuildRequest.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshot.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionLifecycleGates.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionMarketGates.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionQualityCalculator.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionReasonBuilder.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionReasonFormatter.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionRestrictionAlertPolicy.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreCalculator.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreInput.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreSnapshot.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartConsensusFilterEvaluator.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartConsensusFilterInput.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartGates.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionStructureGates.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionTacticalOpportunityAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionThresholdFilterEvaluator.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DecisionThresholdFilterInput.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/DirectionAcceptanceGate.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/EmpiricalConfidenceCalibrator.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/FrameDecisionContributionAdapter.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/HigherTimeframePenaltyCalculator.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/IndependentEvidenceAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/StructuralConfirmationAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/TimeframeAgreementAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Market/Decision/TopDownCalibrationAnalyzer.cs`

### Structure / Zones (16)
- `src/CFIP.Indicator/Analysis/Structure/EqualLevelAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Structure/LiquiditySweepAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Structure/StructureAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Structure/SwingPointAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/FvgDetectionAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/FvgLifecycleAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/FvgMitigationEvaluator.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/FvgZoneQualityCalculator.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockCandidateBuilder.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockConfluenceAnalyzer.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockEvidenceBuilder.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockMitigationGuard.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockQualityCalculator.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/ZoneLookup.cs`
- `src/CFIP.Indicator/Analysis/Structure/Zones/ZoneLookupHotCache.cs`

### Reaction (1)
- `src/CFIP.Indicator/Analysis/Reaction/ReactionAnalyzer.cs`

### Planning / Trade Plan (63)
- `src/CFIP.Indicator/Planning/Entry/BearTriggerScoreAnalyzer.cs`
- `src/CFIP.Indicator/Planning/Entry/BullTriggerScoreAnalyzer.cs`
- `src/CFIP.Indicator/Planning/Entry/ClosedBarTriggerReadyEvaluator.cs`
- `src/CFIP.Indicator/Planning/Entry/M1TriggerReadyEvaluator.cs`
- `src/CFIP.Indicator/Planning/Entry/M1TriggerRuntimeUpdater.cs`
- `src/CFIP.Indicator/Planning/Entry/TriggerRuntimeState.cs`
- `src/CFIP.Indicator/Planning/Execution/ExecutionIntentBuilder.cs`
- `src/CFIP.Indicator/Planning/Execution/ExecutionIntentValidation.cs`
- `src/CFIP.Indicator/Planning/Execution/ExecutionModeResolver.cs`
- `src/CFIP.Indicator/Planning/Execution/ExecutionModelBuilder.cs`
- `src/CFIP.Indicator/Planning/Execution/ExecutionZoneBuilder.cs`
- `src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCandidates.cs`
- `src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCore.cs`
- `src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelector.cs`
- `src/CFIP.Indicator/Planning/Execution/ExecutionZoneQualityEvaluator.cs`
- `src/CFIP.Indicator/Planning/Execution/ExecutionZoneSelectionCandidate.cs`
- `src/CFIP.Indicator/Planning/Execution/MarketEntryValidation.cs`
- `src/CFIP.Indicator/Planning/Execution/PredictivePendingCandidateScorer.cs`
- `src/CFIP.Indicator/Planning/Execution/PredictivePendingLevelSelector.cs`
- `src/CFIP.Indicator/Planning/Execution/PredictivePendingZoneCollector.cs`
- `src/CFIP.Indicator/Planning/Execution/TriggerGate.cs`
- `src/CFIP.Indicator/Planning/Filters/RegimeFilter.cs`
- `src/CFIP.Indicator/Planning/Filters/TradingSessionFilter.cs`
- `src/CFIP.Indicator/Planning/TradePlan/HtfRewardSourcePolicy.cs`
- `src/CFIP.Indicator/Planning/TradePlan/HtfSourceClassifier.cs`
- `src/CFIP.Indicator/Planning/TradePlan/HtfTargetCounter.cs`
- `src/CFIP.Indicator/Planning/TradePlan/HtfTimeframeClassifier.cs`
- `src/CFIP.Indicator/Planning/TradePlan/MinimumRequiredRiskRewardCalculator.cs`
- `src/CFIP.Indicator/Planning/TradePlan/PlanBuilder.cs`
- `src/CFIP.Indicator/Planning/TradePlan/PlanInputPreparation.cs`
- `src/CFIP.Indicator/Planning/TradePlan/PlanIntegrityValidator.cs`
- `src/CFIP.Indicator/Planning/TradePlan/PlanMarketConstraintValidator.cs`
- `src/CFIP.Indicator/Planning/TradePlan/PlanMaterialization.cs`
- `src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs`
- `src/CFIP.Indicator/Planning/TradePlan/PlanProtectionIntegrityValidator.cs`
- `src/CFIP.Indicator/Planning/TradePlan/PlanRewardIntegrityValidator.cs`
- `src/CFIP.Indicator/Planning/TradePlan/PlanTargetPreparation.cs`
- `src/CFIP.Indicator/Planning/TradePlan/Sources/DailyPivotTargetSource.cs`
- `src/CFIP.Indicator/Planning/TradePlan/Sources/HtfTargetSource.cs`
- `src/CFIP.Indicator/Planning/TradePlan/Sources/LiquidityAboveTargetSource.cs`
- `src/CFIP.Indicator/Planning/TradePlan/Sources/LiquidityBelowTargetSource.cs`
- `src/CFIP.Indicator/Planning/TradePlan/Sources/M1MicroTargetSource.cs`
- `src/CFIP.Indicator/Planning/TradePlan/Sources/PreviousPeriodTargetSource.cs`
- `src/CFIP.Indicator/Planning/TradePlan/Sources/SessionTargetSource.cs`
- `src/CFIP.Indicator/Planning/TradePlan/Sources/SmartExtraTargetSource.cs`
- `src/CFIP.Indicator/Planning/TradePlan/Sources/SupplyDemandLiquidityTargetSource.cs`
- `src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs`
- `src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs`
- `src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateSelector.cs`
- `src/CFIP.Indicator/Planning/TradePlan/StructuralStopPlanner.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetCandidateEvaluator.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetLadderStageCandidateBuilder.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetLevelCandidateMerger.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetLevelMerger.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetMetadataEnricher.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetProgressionRule.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetProgressionValidator.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetSelectionPolicy.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetSelector.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetStageFeasibilityGate.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetStageRejectionTelemetry.cs`
- `src/CFIP.Indicator/Planning/TradePlan/TargetStageSelector.cs`

### Trading Risk (22)
- `src/CFIP.Indicator/Trading/Risk/AdaptiveOutcomeRiskPolicy.cs`
- `src/CFIP.Indicator/Trading/Risk/AggressiveRiskPolicy.cs`
- `src/CFIP.Indicator/Trading/Risk/AggressiveVolumeSizer.cs`
- `src/CFIP.Indicator/Trading/Risk/AutoPlanRiskValidator.cs`
- `src/CFIP.Indicator/Trading/Risk/AutoRiskPolicy.cs`
- `src/CFIP.Indicator/Trading/Risk/AutoTradeSafetyGuard.cs`
- `src/CFIP.Indicator/Trading/Risk/AverageAtrCalculator.cs`
- `src/CFIP.Indicator/Trading/Risk/DailyLossAccounting.cs`
- `src/CFIP.Indicator/Trading/Risk/DailyLossGuard.cs`
- `src/CFIP.Indicator/Trading/Risk/DailyLossPersistence.cs`
- `src/CFIP.Indicator/Trading/Risk/ExecutionCapacityGuard.cs`
- `src/CFIP.Indicator/Trading/Risk/ManagedPositionCounter.cs`
- `src/CFIP.Indicator/Trading/Risk/MarginSafetyCalculator.cs`
- `src/CFIP.Indicator/Trading/Risk/MarginUsagePolicy.cs`
- `src/CFIP.Indicator/Trading/Risk/MarketSuitabilityGuard.cs`
- `src/CFIP.Indicator/Trading/Risk/MarketSuitabilityRefreshCoordinator.cs`
- `src/CFIP.Indicator/Trading/Risk/RiskAmountCalculator.cs`
- `src/CFIP.Indicator/Trading/Risk/RiskPercentPolicy.cs`
- `src/CFIP.Indicator/Trading/Risk/SessionWindowEvaluator.cs`
- `src/CFIP.Indicator/Trading/Risk/SuitabilityCalculator.cs`
- `src/CFIP.Indicator/Trading/Risk/SuitabilityRiskMultiplierCalculator.cs`
- `src/CFIP.Indicator/Trading/Risk/VolumeSizer.cs`

### Trading Validation (18)
- `src/CFIP.Indicator/Trading/Validation/ActionableSignalQualityGate.cs`
- `src/CFIP.Indicator/Trading/Validation/DecisionBlockReasonPolicy.cs`
- `src/CFIP.Indicator/Trading/Validation/HigherTfRewardPathValidator.cs`
- `src/CFIP.Indicator/Trading/Validation/HtfTargetPresenceValidator.cs`
- `src/CFIP.Indicator/Trading/Validation/LiveExecutionGateReasonPolicy.cs`
- `src/CFIP.Indicator/Trading/Validation/LiveProtectionDistanceResolver.cs`
- `src/CFIP.Indicator/Trading/Validation/PlanCreationEligibility.cs`
- `src/CFIP.Indicator/Trading/Validation/PreTradePlanSynchronizer.cs`
- `src/CFIP.Indicator/Trading/Validation/PriceProtectionValidation.cs`
- `src/CFIP.Indicator/Trading/Validation/RewardPathZoneObstacleScanner.cs`
- `src/CFIP.Indicator/Trading/Validation/SignalPlanCoordinator.cs`
- `src/CFIP.Indicator/Trading/Validation/SmartThresholdPolicy.cs`
- `src/CFIP.Indicator/Trading/Validation/TargetObstacleScanCache.cs`
- `src/CFIP.Indicator/Trading/Validation/TargetObstacleScanSnapshotBuilder.cs`
- `src/CFIP.Indicator/Trading/Validation/TargetObstacleValidator.cs`
- `src/CFIP.Indicator/Trading/Validation/TradeActionabilityDecisionGate.cs`
- `src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs`
- `src/CFIP.Indicator/Trading/Validation/TradeActionabilityRetestContext.cs`

### Trading Intelligence (32)
- `src/CFIP.Indicator/Trading/Intelligence/BufferedArchivePersistence.cs`
- `src/CFIP.Indicator/Trading/Intelligence/BufferedArchivePersistenceModels.cs`
- `src/CFIP.Indicator/Trading/Intelligence/BufferedPersistenceCoordinator.cs`
- `src/CFIP.Indicator/Trading/Intelligence/EconomicNewsCalendarClient.cs`
- `src/CFIP.Indicator/Trading/Intelligence/EconomicNewsCalendarParser.cs`
- `src/CFIP.Indicator/Trading/Intelligence/EconomicNewsCalendarSharedState.cs`
- `src/CFIP.Indicator/Trading/Intelligence/EconomicNewsFeedCoordinator.cs`
- `src/CFIP.Indicator/Trading/Intelligence/EconomicNewsProtection.cs`
- `src/CFIP.Indicator/Trading/Intelligence/EconomicNewsRiskEvaluator.cs`
- `src/CFIP.Indicator/Trading/Intelligence/EntryLocationQualityAnalyzer.cs`
- `src/CFIP.Indicator/Trading/Intelligence/EntrySignalTimingRuntime.cs`
- `src/CFIP.Indicator/Trading/Intelligence/ExecutionTelemetryRecord.cs`
- `src/CFIP.Indicator/Trading/Intelligence/FreshTriggerEvidenceAnalyzer.cs`
- `src/CFIP.Indicator/Trading/Intelligence/HistoricalOutcomeReader.cs`
- `src/CFIP.Indicator/Trading/Intelligence/NoTradeRegimeAnalyzer.cs`
- `src/CFIP.Indicator/Trading/Intelligence/OutcomeHistoryArchiveStore.cs`
- `src/CFIP.Indicator/Trading/Intelligence/OutcomeMemoryAccountSwitch.cs`
- `src/CFIP.Indicator/Trading/Intelligence/OutcomeMemoryStore.cs`
- `src/CFIP.Indicator/Trading/Intelligence/OutcomeObservation.cs`
- `src/CFIP.Indicator/Trading/Intelligence/OutcomePanelTelemetry.cs`
- `src/CFIP.Indicator/Trading/Intelligence/OutcomeRegistrationResult.cs`
- `src/CFIP.Indicator/Trading/Intelligence/OutcomeTelemetryEngine.cs`
- `src/CFIP.Indicator/Trading/Intelligence/OutcomeWindowMonitor.cs`
- `src/CFIP.Indicator/Trading/Intelligence/PortableMemorySnapshotStore.cs`
- `src/CFIP.Indicator/Trading/Intelligence/Prediction/EarlyPredictionEngine.cs`
- `src/CFIP.Indicator/Trading/Intelligence/Prediction/LiveReversalAnalyzer.cs`
- `src/CFIP.Indicator/Trading/Intelligence/RuntimeLogPersistence.cs`
- `src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchivePersistence.cs`
- `src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchiveStore.cs`
- `src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceRecorder.cs`
- `src/CFIP.Indicator/Trading/Intelligence/StructuralSequenceAnalyzer.cs`
- `src/CFIP.Indicator/Trading/Intelligence/TradePlanRegistry.cs`

### Execution (25)
- `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveConfirmationAlert.cs`
- `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveExecutionPreparation.cs`
- `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveFinalExecutionGuard.cs`
- `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressivePreTradeEligibility.cs`
- `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressivePreTradePreparation.cs`
- `src/CFIP.Indicator/Trading/Execution/Aggressive/BoundPlanProtection.cs`
- `src/CFIP.Indicator/Trading/Execution/Aggressive/BrokerProtectionExecution.cs`
- `src/CFIP.Indicator/Trading/Execution/Aggressive/OrphanManagedProtection.cs`
- `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketExecutionPreparation.cs`
- `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPostFillTargetResolver.cs`
- `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPreTrade.cs`
- `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPreTradeEligibility.cs`
- `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketSubmissionValidator.cs`
- `src/CFIP.Indicator/Trading/Execution/BrokerConfirmationPolicy.cs`
- `src/CFIP.Indicator/Trading/Execution/BrokerProtectionCoordinator.cs`
- `src/CFIP.Indicator/Trading/Execution/ExecutionPlanPreparation.cs`
- `src/CFIP.Indicator/Trading/Execution/ManagementCommandRequestCoordinator.cs`
- `src/CFIP.Indicator/Trading/Execution/PriceMath.cs`
- `src/CFIP.Indicator/Trading/Execution/ServerSideTakeProfitLadder.cs`
- `src/CFIP.Indicator/Trading/Execution/ServerSideTakeProfitLadderProgression.cs`
- `src/CFIP.Indicator/Trading/Execution/State/AutoTradingStateStore.cs`
- `src/CFIP.Indicator/Trading/Execution/State/LifecycleStateStore.cs`
- `src/CFIP.Indicator/Trading/Execution/State/TargetStageState.cs`
- `src/CFIP.Indicator/Trading/Execution/State/TradeLabelFormatter.cs`
- `src/CFIP.Indicator/Trading/Execution/SubmissionGateCoordinator.cs`

### Lifecycle / Management (57)
- `src/CFIP.Indicator/Trading/Lifecycle/ActiveBrokerStopAccessor.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/ActiveBrokerTargetAccessor.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/BrokerProtectionStateEvaluator.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/BrokerProtectionStateSynchronizer.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/BrokerStateSnapshot.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/LifecycleEventIdempotencyGuard.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/LifecycleEventState.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/LifecycleTransitionPolicy.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/LiveFillExitReconciler.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/LiveFillReconciliation.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/LivePlanExitCoordinator.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/LivePlanFactory.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/LivePlanFurtherTargetSelector.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/LivePlanTargetEnrichment.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/ManagedLivePlanRecovery.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/ManagedPositionLookup.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PendingCancelledHandler.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PendingCreatedHandler.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PendingFillPlanBuilder.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PendingFillProtectionCoordinator.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PendingFilledHandler.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PendingModifiedHandler.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PendingOrderCircuitBreaker.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PendingOrderPlanSnapshot.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PositionCircuitBreaker.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PositionClosedHandler.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PositionModifiedHandler.cs`
- `src/CFIP.Indicator/Trading/Lifecycle/PositionOpenedHandler.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanEvaluation.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanFalseSignalGuard.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanIntegrityHandler.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanLevelExitHandler.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanLiveManagement.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanMarketState.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanReactionExitHandler.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/LiveReversalEpisodeState.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/LiveStructuralPulse.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/LiveTargetCandidateEvaluator.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/PartialTakeProfitExecutor.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/PlanActivation.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/PlanRiskRewardRecalculator.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/ProtectionManager.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/ReversalCloseGuard.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/ReversalProtection.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/SmartExitModeResolver.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/SmartExitPressureCalculator.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/StructuralSetupInvalidationExit.cs`
- `src/CFIP.Indicator/Trading/LiveManagement/TargetProgression.cs`
- `src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPlacement.cs`
- `src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPreparation.cs`
- `src/CFIP.Indicator/Trading/Pending/Placement/PendingOrderCleanup.cs`
- `src/CFIP.Indicator/Trading/Pending/Placement/PendingOrderConfirmationReporter.cs`
- `src/CFIP.Indicator/Trading/Pending/Placement/PendingSubmissionValidator.cs`
- `src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPlacement.cs`
- `src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPreparation.cs`
- `src/CFIP.Indicator/Trading/Pending/Placement/SmartPendingOrderOrchestrator.cs`
- `src/CFIP.Indicator/Trading/Pending/Policy/PendingOrderPolicy.cs`

### Runtime / MTF / Provider (34)
- `src/CFIP.Indicator/Core/Runtime/AlertDelivery.cs`
- `src/CFIP.Indicator/Core/Runtime/AlertDeliveryQueue.cs`
- `src/CFIP.Indicator/Core/Runtime/AlertEventDedupCoordinator.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CalculationBrokerBoundary.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CalculationLiveCycle.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CalculationMarketContext.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CalculationPreparation.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CalculationReadinessStateStore.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CalculationStartupSeed.cs`
- `src/CFIP.Indicator/Runtime/Calculation/CanonicalMarketContextBuilder.cs`
- `src/CFIP.Indicator/Runtime/Calculation/RuntimeFaultBoundary.cs`
- `src/CFIP.Indicator/Runtime/Calculation/RuntimeFaultState.cs`
- `src/CFIP.Indicator/Runtime/Calculation/RuntimeFaultStateMachine.cs`
- `src/CFIP.Indicator/Runtime/Cbot/CbotChartLifecycleEvents.cs`
- `src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs`
- `src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs`
- `src/CFIP.Indicator/Runtime/Initialization/StartupDataHelpers.cs`
- `src/CFIP.Indicator/Runtime/Mtf/MtfClosedContext.cs`
- `src/CFIP.Indicator/Runtime/Mtf/MtfClosedContextCache.cs`
- `src/CFIP.Indicator/Runtime/Mtf/MtfContextBuilder.cs`
- `src/CFIP.Indicator/Runtime/Provider/CFIPDeviceScenarioBatchPublisher.cs`
- `src/CFIP.Indicator/Runtime/Provider/CFIPDeviceSignalPublisher.cs`
- `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProvider.cs`
- `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderIdentity.cs`
- `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs`
- `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderRefresh.cs`
- `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderScenarioBatch.cs`
- `src/CFIP.Indicator/Runtime/Supervision/PanelHeartbeatLiveState.cs`
- `src/CFIP.Indicator/Runtime/Supervision/RuntimePanelHeartbeat.cs`
- `src/CFIP.Indicator/Runtime/Supervision/RuntimeSafetySupervisor.cs`

### Contracts (22)
- `src/CFIP.Contracts/AlertEnvelope.cs`
- `src/CFIP.Contracts/BrokerExecutionReport.cs`
- `src/CFIP.Contracts/CbotExecutionStateBus.cs`
- `src/CFIP.Contracts/CbotExecutionStateSnapshot.cs`
- `src/CFIP.Contracts/CbotIdentity.cs`
- `src/CFIP.Contracts/ContractBusKeyHash.cs`
- `src/CFIP.Contracts/ContractEnums.cs`
- `src/CFIP.Contracts/ContractVersion.cs`
- `src/CFIP.Contracts/ExecutionIntent.cs`
- `src/CFIP.Contracts/IdentityContracts.cs`
- `src/CFIP.Contracts/IndicatorIdentity.cs`
- `src/CFIP.Contracts/LifecycleEvent.cs`
- `src/CFIP.Contracts/ManagementBusKey.cs`
- `src/CFIP.Contracts/ManagementCommand.cs`
- `src/CFIP.Contracts/MarketExecutionProfile.cs`
- `src/CFIP.Contracts/PlanSnapshot.cs`
- `src/CFIP.Contracts/ScenarioExecutionIdentityRule.cs`
- `src/CFIP.Contracts/SignalBusKey.cs`
- `src/CFIP.Contracts/SignalEnvelope.cs`
- `src/CFIP.Contracts/SignalEnvelopeCodec.cs`
- `src/CFIP.Contracts/SignalScenarioBatch.cs`
- `src/CFIP.Contracts/SignalScenarioBatchCodec.cs`

### cBot Execution (22)
- `src/CFIP.cBot/Binding/CfipDeviceSignalTransport.cs`
- `src/CFIP.cBot/Binding/CfipIndicatorChartBinding.cs`
- `src/CFIP.cBot/CFIPExecutionBot.cs`
- `src/CFIP.cBot/Execution/BrokerExecutionSafety.cs`
- `src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs`
- `src/CFIP.cBot/Execution/CbotExecutionIdempotencyStore.cs`
- `src/CFIP.cBot/Execution/CbotExecutionLifecycleRule.cs`
- `src/CFIP.cBot/Execution/CbotExecutionSettings.cs`
- `src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs`
- `src/CFIP.cBot/Execution/CbotLifecycleAudioService.cs`
- `src/CFIP.cBot/Execution/CbotManagedObjectIdentityRule.cs`
- `src/CFIP.cBot/Execution/CbotManagementPolicyRule.cs`
- `src/CFIP.cBot/Execution/CbotSignalPreflight.cs`
- `src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs`
- `src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs`
- `src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs`
- `src/CFIP.cBot/Recovery/CbotBrokerReconciliation.cs`
- `src/CFIP.cBot/Risk/CbotDailyLossGuard.cs`
- `src/CFIP.cBot/Risk/ExecutionMarginBudgetRule.cs`
- `src/CFIP.cBot/Shadow/ShadowHostContracts.cs`
- `src/CFIP.cBot/Shadow/ShadowHostCoordinator.cs`
- `src/CFIP.cBot/Shadow/ShadowHostValidator.cs`

### UI / Presentation (80)
- `src/CFIP.Indicator/UI/Chart/AlertSignalRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/ChartObjectCleanup.cs`
- `src/CFIP.Indicator/UI/Chart/MtfTrendStrengthSnapshotBuilder.cs`
- `src/CFIP.Indicator/UI/Chart/OutcomeMarkerRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/ParallelOpportunityRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/PendingOrderRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs`
- `src/CFIP.Indicator/UI/Chart/PlanLabelFormatting.cs`
- `src/CFIP.Indicator/UI/Chart/PlanLabelRemover.cs`
- `src/CFIP.Indicator/UI/Chart/PlanLabelRenderCoordinator.cs`
- `src/CFIP.Indicator/UI/Chart/PlanLabelRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/PlanLevelVisualState.cs`
- `src/CFIP.Indicator/UI/Chart/PlanLineRemover.cs`
- `src/CFIP.Indicator/UI/Chart/PlanLineRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/PlanObjectClearer.cs`
- `src/CFIP.Indicator/UI/Chart/PlanObjectRemover.cs`
- `src/CFIP.Indicator/UI/Chart/PlanRenderCoordinator.cs`
- `src/CFIP.Indicator/UI/Chart/PredictionLabelsRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/PredictionLineRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/PredictionObjectCleanup.cs`
- `src/CFIP.Indicator/UI/Chart/PredictionRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/SignalPresentationColorRule.cs`
- `src/CFIP.Indicator/UI/Chart/SignalPresentationRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/SignalRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs`
- `src/CFIP.Indicator/UI/Chart/SignalVisualDirectionResolver.cs`
- `src/CFIP.Indicator/UI/Chart/SignalVisualIdentityBuilder.cs`
- `src/CFIP.Indicator/UI/Chart/SignalVisualSnapshot.cs`
- `src/CFIP.Indicator/UI/Chart/SignalVisualSnapshotBuilder.cs`
- `src/CFIP.Indicator/UI/Chart/SignalVisualSynchronizer.cs`
- `src/CFIP.Indicator/UI/Controls/ExecutionControlsFactory.cs`
- `src/CFIP.Indicator/UI/Controls/ExecutionControlsSynchronizer.cs`
- `src/CFIP.Indicator/UI/Controls/SessionPresentation.cs`
- `src/CFIP.Indicator/UI/Historical/HistoricalRenderer.cs`
- `src/CFIP.Indicator/UI/Historical/HistoricalSignalPresentation.cs`
- `src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs`
- `src/CFIP.Indicator/UI/Panel/PanelAlertMessageRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/PanelCanonicalSignalStatus.cs`
- `src/CFIP.Indicator/UI/Panel/PanelConstants.cs`
- `src/CFIP.Indicator/UI/Panel/PanelContentRefresh.cs`
- `src/CFIP.Indicator/UI/Panel/PanelExecutionSemantics.cs`
- `src/CFIP.Indicator/UI/Panel/PanelExecutionState.cs`
- `src/CFIP.Indicator/UI/Panel/PanelFactory.cs`
- `src/CFIP.Indicator/UI/Panel/PanelFooterFactory.cs`
- `src/CFIP.Indicator/UI/Panel/PanelHeaderLiveState.cs`
- `src/CFIP.Indicator/UI/Panel/PanelHeaderRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/PanelLayoutManager.cs`
- `src/CFIP.Indicator/UI/Panel/PanelMainRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/PanelPredictionState.cs`
- `src/CFIP.Indicator/UI/Panel/PanelRenderOptimization.cs`
- `src/CFIP.Indicator/UI/Panel/PanelRestoreButtonFactory.cs`
- `src/CFIP.Indicator/UI/Panel/PanelRowWriter.cs`
- `src/CFIP.Indicator/UI/Panel/PanelRowsFactory.cs`
- `src/CFIP.Indicator/UI/Panel/PanelRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/PanelSignalState.cs`
- `src/CFIP.Indicator/UI/Panel/PanelTextFormatting.cs`
- `src/CFIP.Indicator/UI/Panel/PanelTimeframePresentationState.cs`
- `src/CFIP.Indicator/UI/Panel/PanelToggleButtonFactory.cs`
- `src/CFIP.Indicator/UI/Panel/PanelTrendTimeframeLampRow.cs`
- `src/CFIP.Indicator/UI/Panel/PanelVisibility.cs`
- `src/CFIP.Indicator/UI/Panel/ProcessingHeartbeatLamp.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelCalibrationRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelContextRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelDecisionRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelExecutionRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewDiagnosticRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewStateRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelSignalPipelineRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelTradePlanLevelRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelTradePlanLiveRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelTradePlanRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Rows/PanelWaveTrendAndOpportunityRowsRenderer.cs`
- `src/CFIP.Indicator/UI/Panel/Theme/PanelActionButtonsLayout.cs`
- `src/CFIP.Indicator/UI/Panel/Theme/PanelQuickExecutionLayout.cs`
- `src/CFIP.Indicator/UI/Panel/Theme/PanelRestoreButtonLayout.cs`
- `src/CFIP.Indicator/UI/Panel/Theme/PanelRowsLayout.cs`
- `src/CFIP.Indicator/UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs`
- `src/CFIP.Indicator/UI/Panel/Theme/PanelVisualSettings.cs`

## 3. زنجیره کامل از پیش‌تحلیل تا ترید
1. **Acquisition** — market data, ticks, DOM, session/calendar.
2. **Canonical context** — price/spread/time/symbol + closed/live MTF snapshots.
3. **Readiness** — warmup, freshness, numeric guards, data integrity, news/regime/suitability.
4. **Parallel analysis** — technical + structure + zones + liquidity + reaction + flow + divergence + MTF.
5. **Evidence normalization** — direction, strength, confidence, freshness, independence, location.
6. **Decision fusion** — frame contribution → score → quality/confidence → consensus → gates → actionability.
7. **Scenario generation** — current opportunity / future pending opportunity / competing scenarios.
8. **Trade plan** — entry geometry, trigger, structural SL, target ladder, reward path, invalidation.
9. **Risk** — risk amount → symbol-native volume → normalization → margin/capacity → final executable size.
10. **Contract publication** — SignalEnvelope / ScenarioBatch / ExecutionIntent / identity and lineage.
11. **cBot preflight** — environment, attachment, idempotency, managed identity, broker safety, margin.
12. **Broker execution** — market/pending order mutation; cBot only.
13. **Broker-confirmed lifecycle** — open/fill/modify/close/reconcile.
14. **Protection & progression** — SL/BE/trailing/partial TP/target progression/reversal protection.
15. **Outcome & calibration** — outcome observation, telemetry, trace archive, empirical calibration.

## 4. تحلیلگرها و خانواده‌های اصلی
- **Trend/Momentum:** EMA, MACD, RSI, DMI/ADX, Aroon, Stochastic, CCI, MFI, OBV, Parabolic SAR, SuperTrend, Bollinger.
- **Regime/Market Quality:** MarketRegime, Choppiness, HealthyVolatility, RangeEfficiency, RangeSignalQuality.
- **Structure:** SwingPoint, Structure, EqualLevel, LiquiditySweep.
- **Zones:** FVG detection/lifecycle/mitigation/quality/confluence؛ OrderBlock candidate/quality/lifecycle/mitigation/confluence.
- **Flow/Volume:** VolumeExpansion, VolumeProfile, VWAP, AggressiveFlow. Semantics must remain explicit: **DOM ≠ executed volume ≠ tick count ≠ aggressive-flow proxy**.
- **Reaction/Timing:** Reaction, FreshTriggerEvidence, EntrySignalTiming, LiveReversal.
- **Evidence quality:** Divergence, IndependentEvidenceAnalyzer, evidence-independence/diversity rules.
- **Prediction:** EarlyPredictionEngine, MTF early-prediction fusion, live reversal prediction.

## 5. Decision authority
- Canonical path: `DecisionInputSnapshotFactory` → `DecisionEvaluator` → score/quality/confidence → consensus → confirmation/structure/market/lifecycle gates → actionability.
- Panel، chart، alerts و execution contract باید از یک canonical decision/plan lineage مصرف کنند؛ محاسبه موازی برای نمایش یا اجرا ممنوع.
- Consensus باید correlation و double-counting را کنترل کند؛ پنج indicator هم‌خانواده نباید پنج رأی مستقل محسوب شوند.

## 6. Plan / Entry / Target authority
- Entry: M15 decision context، M5 precision/trigger، M1 optional confirmation.
- SL: structural + volatility-aware + broker-valid؛ نه فاصله ثابت.
- Targets: liquidity/HTF/session/previous-period/pivot/micro targets + reward-path obstacle validation.
- RR صرفاً constraint/quality signal است، نه فرمول اجباری «1:2»؛ reward path و adverse-risk باید تعیین‌کننده باشند.
- Pending setups باید lifecycle و identity مستقل و پایدار داشته باشند.

## 7. Risk / XAU authority
- مسیر مرجع: risk amount → SL geometry → symbol-native sizing → normalization → margin/capacity.
- برای XAU هیچ ضریب دستی ثابت اضافه نشود؛ TickSize/TickValue، volume limits/step و broker margin مرجع باشند. citeturn0search8
- Spread و slippage باید در actionability/execution validation لحاظ شوند.
- Daily loss و maximum exposure باید قبل از broker mutation enforce شوند.

## 8. Execution / cBot boundary
- Indicator باید read-only نسبت به broker mutation بماند و intent/plan/management request تولید کند.
- cBot مسئول order placement، pending placement، modify/cancel/close، broker confirmation، idempotency و reconciliation است.
- News Guard در Indicator فقط intelligence/safety intent تولید کند؛ global cTrader Quick Trade/trading permission را نباید خاموش کند.
- هیچ startup/lifetime timeout برای Indicator یا cBot مجاز نیست؛ freshness/provider staleness با safety gate کنترل شود، نه shutdown timer.

## 9. ممیزی‌های باز که از همین سند دنبال می‌شوند
- [ ] تک‌تک analyzerها: input/output، warmup، lookback، closed/live semantics، NaN/Infinity، performance، correlation و duplicate evidence.
- [ ] Decision: جلوگیری از double-counting و flip سریع؛ hysteresis/confirmation برای live signal.
- [ ] MTF: حذف کامل هر M2 reference؛ تثبیت M15/M5/M1/H1+ semantics.
- [ ] Flow: تفکیک دقیق broker DOM، tick activity، aggressive proxy و exchange futures volume.
- [ ] Indicator execution boundary: حذف تمام broker mutationهای باقی‌مانده.
- [ ] cBot: activation واقعی، attachment، live/demo، market/pending، margin، idempotency و broker-confirmed lifecycle.
- [ ] XAU sizing و target geometry با symbol-native broker specs.
- [ ] UI/alerts: یک source of truth برای signal, arrows, lines, labels, messages, sounds.
- [ ] Post-trade: outcome/calibration نباید future decision را با leakage آلوده کند.

## 10. روش کار این سند در ادامه
در هر پیام چند جزء/خانواده بررسی می‌شوند و نتیجه در همین فایل ثبت می‌شود: **وضعیت فعلی → ایراد/ریسک → مبنای ریاضی/API → پیشنهاد معماری → owner → تست/گیت → وضعیت (PASS/FAIL/OPEN)**. تحقیق اینترنتی فقط با منابع معتبر و سازگار با API واقعی cTrader وارد می‌شود.

## 11. وضعیت سند
- Base: `main` / `afedd73dad140bab3654cbaedc36b25cb8d8da2a`
- Inventory فعلی: 715 فایل C# در زیرساخت src که در دسته‌های این سند ثبت شده‌اند.
- تاریخ آخرین ممیزی وب: 2026-10-07.


## 12. ممیزی عمیق — Batch 01 (Technical + Regime + Consensus)

### 12.1 Primitive indicators — نتیجه ممیزی
| جزء | مالک فعلی | وضعیت | نتیجه |
|---|---|---|---|
| EMA | cTrader native EMA wrapper | PASS | منطق داخلی دوباره‌نویسی نشده؛ readiness کنترل می‌شود؛ برای slope/spread استفاده می‌شود. |
| RSI | cTrader native RSI | PASS / مراقبت | مقدار 50 در نبود داده برای compatibility نگه داشته شده، اما باید قبل از scoring از evidence-ready بودن native set مطمئن شد؛ 30/70 نباید به‌تنهایی trigger باشد. |
| ATR | cTrader native ATR + Wilder smoothing | PASS | مرجع volatility/geometry است؛ صفر/non-finite fail-closed. |
| ADX/DMI | cTrader DirectionalMovementSystem | PASS | ADX و DI+/DI- از یک محاسبه مشترک می‌آیند؛ DMI فقط bias می‌دهد و نباید با ADX دوباره رأی مستقل بسازد. |
| MACD-line bias | دو EMA canonical | PASS / INTENTIONAL | CFIP آن را عمداً MACD-line bias نگه داشته، نه crossover؛ برای trigger باید histogram/signal از OSS adapter جدا و بدون دوباره‌شماری مصرف شود. |
| Bollinger | Skender adapter | PASS / نیاز به مصرف دقیق | `%B` و `Width` دو metric متفاوت‌اند؛ Width باید volatility/context باشد و `%B` location/momentum، نه دو رأی مستقل یک خانواده. |

اصل اجرایی: هر primitive باید یک خروجی canonical، warm-up مشخص، closed/live semantics روشن، finite-value policy و مصرف‌کننده مشخص داشته باشد. Built-in cTrader منبع اصلی primitiveهای native باقی می‌ماند؛ این با مستندات رسمی cTrader درباره EMA، RSI، Bollinger و Bars سازگار است.

### 12.2 Market regime — نتیجه ممیزی
- `ChoppinessIndexAnalyzer` فرمول CHOP را با پنجره کامل محاسبه می‌کند و نزدیک ابتدای سری پنجره ناقص را وارد نتیجه نمی‌کند؛ این رفتار حفظ می‌شود.
- `RangeEfficiencyAnalyzer` نسبت حرکت خالص به مسیر قیمت را محاسبه می‌کند؛ این metric باید efficiency/context بماند و با ADX/EMA/MACD به‌عنوان چند رأی مستقل هم‌ارزش جمع نشود.
- `MarketRegimeAnalyzer` اکنون برای M5 مسیر cache مخصوص و برای سایر timeframeها مسیر per-frame دارد؛ این با مدل «همه timeframeها تحلیل شوند» سازگار است. M15 همچنان مرجع تصمیم است، نه تنها timeframe تحلیل.
- `HealthyVolatilityAnalyzer` در Batch 01 اصلاح شد: baseline دیگر یک ATR قدیمی منفرد نیست و از میانگین 20 ATR از کندل‌های بسته قبلی (`index - 1`) استفاده می‌کند. بنابراین یک spike/drop منفرد نمی‌تواند به‌تنهایی health را flip کند.
- `RangeSignalQualityEvaluator` در RANGE/COMPRESSION فقط زمانی اجازه عبور می‌دهد که contextهای ساختاری/مکان/دیسپلیسمنت/flow به‌صورت هم‌جهت جمع شوند؛ با این حال این خروجی باید gate کیفیت باشد، نه یک رأی دوم در Decision.

### 12.3 MACD / Bollinger — تصمیم معماری
1. MACD native line و MACD OSS signal/histogram دو implementation برای یک family هستند؛ نباید هر دو به‌صورت رأی مستقل کامل شمرده شوند.
2. Bollinger `%B` و `Width` باید در یک family مستقل بمانند: location و volatility-context.
3. RSI، MACD، WaveTrend، DMI و EMA همگی بخشی از trend/momentum family هستند و `IndicatorEvidenceIndependenceRule` باید وزن گروه را بر اساس family نگه دارد، نه تعداد indicatorها.
4. هیچ indicator جدیدی فقط برای افزایش vote count اضافه نشود.

### 12.4 ماژول تجمیع و نتیجه‌گیری آرا — ممیزی دقیق
زنجیره canonical فعلی:
`DecisionFrameContributionCalculator → DecisionScoreCalculator → DecisionConsensusCalculator → DecisionEvaluator`.

نقاط قوت موجود:
- contribution هر timeframe با quality و weight مقیاس می‌شود؛ frame ضعیف نمی‌تواند صرفاً با raw-score بزرگ غالب شود.
- M1 در score رأی مستقل ندارد و فقط بعد از directional consensus برای trigger confirmation مصرف می‌شود.
- `DecisionConsensusCalculator` از softmax دوطرفه با temperature محدود استفاده می‌کند و tie دقیق را neutral نگه می‌دارد.
- NaN/Infinity در score/consensus fail-closed است.
- conflict penalty و independent-evidence diversity جدا از direction authority نگه داشته شده‌اند.

اصلاحات الزامی برای ادامه:
- `DecisionConsensusCalculator` نباید confidence را به‌عنوان probability معرفی کند؛ share فعلی یک deterministic directional share است.
- هیچ family همبسته نباید از طریق تعداد indicatorها قدرت بیشتری بگیرد؛ group diversity باید فقط quality/coverage را بهبود دهد.
- M15 باید canonical anchor باقی بماند؛ M5 نباید با تعداد رأی بیشتر M15 را کنار بزند.
- H1/H4/D1 باید context/constraint واقعی باشند؛ alignment ضعیف HTF نباید با یک lower-TF vote stack بی‌اثر شود.
- regime/structure/location/flow باید در score lineage قابل ردیابی باشند و هیچ downstream module نباید رأی خام دوم تولید کند.
- exact BUY/SELL tie، missing evidence، insufficient warm-up و non-finite data باید همگی neutral/fail-closed باشند.

### 12.5 مدل پیشنهادی نهایی رأی‌گیری
`Evidence → Family Normalization → Timeframe Contribution → M15 Anchor → HTF Constraint → Consensus → Quality → Trigger Gate`

در این مدل:
- Evidence فقط واقعیت خام/metric را تولید می‌کند.
- Family Normalization indicatorهای همبسته را در یک family قرار می‌دهد.
- Timeframe Contribution کیفیت، وزن timeframe و evidence coverage را اعمال می‌کند.
- M15 Anchor مرجع directional decision است.
- HTF Constraint تضاد H1+ قوی را نمی‌گذارد lower-TF stack آن را پنهان کند.
- Consensus فقط نتیجه نهایی direction/share را می‌دهد.
- Quality confidence/actionability را ارزیابی می‌کند و direction جدید نمی‌سازد.
- Trigger Gate M5 و در صورت فعال بودن M1 را برای زمان ورود بررسی می‌کند.

### 12.6 قرارداد جلوگیری از signal flip
در ادامه باید برای Decision یک state transition contract اضافه/تکمیل شود:
- تغییر direction فقط با closed canonical evidence یا trigger event معتبر؛
- تغییرات intrabar صرفاً forming/watch باشند و authority تصمیم بسته را overwrite نکنند؛
- flip متوالی BUY↔SELL بدون عبور از hysteresis/confirmation threshold ممنوع؛
- هر flip باید reason/provenance داشته باشد؛
- M5 trigger نباید direction M15 را بازنویسی کند.

### 12.7 Batch 01 — وضعیت
- [x] EMA / RSI / ATR / ADX-DMI ownership audit
- [x] MACD / Bollinger semantic audit
- [x] CHOP / Range Efficiency audit
- [x] Market Regime / volatility baseline audit
- [x] Decision contribution → score → consensus chain audit
- [x] Healthy-volatility baseline stability fix
- [ ] M15-anchor/higher-timeframe hard constraint implementation
- [ ] consensus hysteresis / anti-flip contract
- [ ] family-level vote matrix test for all OSS + native indicators
- [ ] numerical benchmark for score/consensus across regimes

## 13. ممیزی عمیق — Batch 02 (M15 Anchor / HTF / Agreement)

### 13.1 یافته قطعی
ممیزی مسیر تصمیم نشان داد مشکل فقط «تعداد رأی» نیست؛ دو مشکل واقعی در لایه تجمیع وجود داشت:

1. `TimeframeAgreement` فریم neutral را از مخرج حذف می‌کرد. در نتیجه اگر فقط یک فریم جهت‌دار باقی می‌ماند، agreement می‌توانست 100% گزارش شود بدون اینکه واقعاً تمام فریم‌های معتبر هم‌جهت باشند.
2. `HigherTimeframeConfidencePenalty` برای هر مخالفت تقریباً ثابت بود و کیفیت/وزن H1/H4/D1 را لحاظ نمی‌کرد؛ بنابراین یک HTF ضعیف و چند HTF قوی می‌توانست اثر مشابهی داشته باشند.

### 13.2 اصلاحات اعمال‌شده
- `TimeframeAgreementAnalyzer` اکنون هر فریم معتبر و هم‌تراز را در denominator نگه می‌دارد؛ neutral رأی مخالف نیست ولی alignment مثبت هم نیست.
- `HigherTimeframePenaltyCalculator` اکنون opposition را بر اساس `frame.Quality × timeframeWeight` محاسبه می‌کند و penalty متناسب با نسبت مخالفت اعمال می‌شود.
- W1 در صورت فعال بودن `SmartWeeklyContext` نیز در همین مدل لحاظ می‌شود.
- این penalty همچنان **direction owner نیست**؛ فقط confidence را کاهش می‌دهد. مالک direction همان consensus است و top-down/confirmation gates مالک permission نهایی هستند.
- CI-09 نیز برای این قراردادها به‌روزرسانی شد.

### 13.3 M15 Anchor
بررسی کل مسیر نشان داد M15 از قبل در چند owner نهایی حضور دارد:
- `RequireCoreAgreement`، M5 را با M15 هم‌راستا می‌کند.
- `TriggerGate` و `PendingOrderPolicy` نیز M15 compatibility/alignment را الزام می‌کنند.
- `DirectionAcceptanceGate` برای reversal مخالف، M15 direction + structural/force confirmation را بررسی می‌کند.

بنابراین در این batch یک رأی یا score دوم برای M15 اضافه نشد؛ چنین کاری با اصل single-owner تضاد ایجاد می‌کرد. اصلاح بعدی باید **M15 را به‌عنوان anchor permission** در همان top-down/confirmation owner تقویت کند، نه اینکه M15 را دوباره به score اضافه کند.

### 13.4 نتیجه معماری رأی
مدل canonical فعلی باید حفظ شود:
`Indicator Evidence → Family Normalization → Frame Contribution → Direction Consensus → Top-Down Permission → Confirmation → Actionability`

و نه:
`Indicator → Indicator → Indicator → raw vote count`.

### 13.5 Batch 02 وضعیت
- [x] Neutral timeframe denominator bug fixed
- [x] HTF penalty quality/weight-aware شد
- [x] W1 opposition در penalty لحاظ شد
- [x] M15 duplicate vote ایجاد نشد
- [x] CI-09 contract update
- [ ] Anti-flip hysteresis contract در decision lifecycle
- [ ] Family-level vote matrix برای تمام native/OSS indicators
- [ ] Structure/OB/FVG evidence audit
- [ ] Flow/DOM/aggressive-flow evidence audit

## 14. ممیزی عمیق — Batch 03 (Structure / Liquidity / FVG / OB / Reaction)

### 14.1 Structure
- `StructureAnalyzer` از `StructuralEventRule` برای شکست تازه و CHOCH استفاده می‌کند.
- Structure، MSS و CHOCH در یک فریم causal eventهای کاملاً مستقل فرض نمی‌شوند؛ `StructuralEvidenceRule` downstream آن‌ها را به یک canonical structural event فرو می‌ریزد.
- BullMss/BearMss از نظر هندسه به شکست ساختار نزدیک‌اند و بنابراین نباید به‌صورت رأی دوم وارد consensus شوند.
- BullChoch/BearChoch فقط زمانی معتبرند که ساختار قبلی مخالف و شکست تازه جهت جدید وجود داشته باشد؛ این transition است، نه رأی مستقل دوم.
- ریسک باقی‌مانده: قرارداد freshness/episode identity برای eventهای structure باید در anti-flip lifecycle صریح‌تر شود تا یک break در چند مسیر downstream دوباره event نشود.

### 14.2 Liquidity Sweep
- `LiquiditySweepAnalyzer` فقط swing تأییدشده و هنوز active/unbroken را مصرف می‌کند.
- `LiquiditySweepRule.IsActiveUnbrokenLevel` مالک اعتبار سطح است و شکست قبلی سطح را از reuse به‌عنوان sweep تازه جلوگیری می‌کند.
- penetration نسبت به ATR و حداقل pip/tick guard بررسی می‌شود.
- این نتیجه باید «reaction to liquidity» تلقی شود، نه volume/executed-flow؛ در FX/spot، liquidity observable از یک broker/venue کل بازار جهانی نیست، چون بازار OTC و fragmented است. citeturn0search0turn0search1
- ریسک باقی‌مانده: sweep، equal-level و structure break می‌توانند manifestations یک رویداد قیمتی باشند؛ در family/provenance matrix نهایی باید این هم‌پوشانی explicitly کنترل شود.

### 14.3 FVG
- `FvgRule` تنها مالک geometry است؛ 3-bar و optional 2-bar imbalance هر دو از همین owner استفاده می‌کنند.
- minimum gap نسبت به ATR زمان ایجاد سنجیده می‌شود؛ lifecycle/partial mitigation/full fill مالک جداگانه دارد.
- انتخاب FVG برای execution بین quality، distance و age توازن bounded دارد؛ FVG قدیمی یا دور صرفاً به‌خاطر quality بالا نباید برنده قطعی باشد.
- FVG و OB در `LocationEvidenceRule` یک location family هستند و confluence به‌صورت bounded synergy اضافه می‌شود؛ این از رأی‌سازی مستقل برای هر zone جلوگیری می‌کند.
- FVG opening-gap مفهومی نباید با structural FVG مخلوط شود؛ اگر بعداً داده gap اضافه شد، provenance جدا لازم است.

### 14.4 Order Block
- canonical ownership شامل `OrderBlockRule` برای geometry/qualification، `OrderBlockQualityRule` برای quality و `OrderBlockLifecycleRule` برای age/mitigation/invalidation است.
- OB+FVG confluence در location owner نگه داشته شده و نباید در decision جای دیگری دوباره امتیاز کامل بگیرد.
- ریسک باقی‌مانده: بعضی pending-candidate scorers هنوز context را به‌صورت feature-by-feature (FVG + OB + confluence + structure + MSS/CHOCH) امتیاز می‌دهند؛ این مسیر باید با canonical location/structural provenance تطبیق داده شود تا score مستقل موازی نسازد.

### 14.5 Reaction
- Reaction برای live M5 observation و closed-bar confirmation مسیر جدا و temporal-safe دارد.
- `ReactionQualificationRule` کیفیت/حداقل evidence/context را مالک است و `ReactionTimingRule` جداسازی observation و confirmation را کنترل می‌کند.
- Reaction direction نباید direction M15 را overwrite کند؛ در مسیر فعلی برای entry مخالف M15 gate وجود دارد.
- Reaction شامل displacement/break-micro/RSI/EMA/zone/swing است؛ این‌ها evidenceهای هم‌خانواده‌اند و نباید به‌عنوان چند رأی مستقل وارد canonical directional consensus شوند.

### 14.6 اصلاح واقعی Batch 03
در `MarketFrameScoringService` یک defect واقعی پیدا شد: شمارنده `Evidence` قبلاً evidence صعودی و نزولی را در یک متغیر مشترک جمع می‌کرد. بنابراین فریمی که همزمان location/structure conflict داشت می‌توانست evidence و در نتیجه quality بیشتری بگیرد، حتی وقتی direction قوی و پاک وجود نداشت.

اصلاح canonical:
- `bullEvidence` و `bearEvidence` جدا شدند.
- `Frame.Evidence` فقط evidence سمت غالب را نگه می‌دارد.
- در exact tie، `Frame.Evidence = 0`.
- این تغییر در همان owner انجام شد؛ هیچ score/gate موازی اضافه نشد.
- commit: `77b64a875f7aaa7b41f12973029e3c21f84a6296`
- static gate جدید: `tools/audit_phase_structure_zones_reaction_2026_10_07.py`
- gate commit: `540d02b39d41a21c7e8990249308f5a350146f37`

### 14.7 مبنای API/وب
مستندات cTrader تأکید می‌کند Indicator وظیفه پردازش market data و نمایش/تحلیل است؛ داده‌های MarketData شامل Bars/Ticks/Depth هستند. برای FX نیز BIS تأکید می‌کند spot FX عمدتاً OTC، غیرمتمرکز و fragmented است؛ بنابراین liquidity/sweep و broker DOM نباید به‌عنوان تصویر کامل global executed flow معرفی شوند. citeturn0search4turn0search8turn0search0

### 14.8 وضعیت Batch 03
- [x] Structure ownership / causal event de-dup audit
- [x] Active/unbroken liquidity audit
- [x] FVG geometry/lifecycle/selection audit
- [x] OB ownership/confluence audit
- [x] Reaction temporal/quality audit
- [x] Directional Evidence inflation fix
- [x] Static acceptance gate added
- [ ] Unified structural/liquidity/zone provenance matrix
- [ ] Pending-candidate scorer migration to canonical evidence lineage
- [ ] Anti-flip episode identity and hysteresis
- [ ] Runtime/compile/architecture CI verification for this batch

## 15. وضعیت پیشرفت کل پروژه — 2026-10-07

این درصد، برآورد معماری/ممیزی است و به معنی درصد فایل‌های نوشته‌شده نیست.

- پایه معماری و inventory: **100%**
- Technical / Regime / primitive audit: **حدود 85%**
- M15 / MTF / HTF / agreement: **حدود 85%**
- Structure / Liquidity / FVG / OB / Reaction: **حدود 80%**
- Flow / DOM / Aggressive Flow / volume semantics: **حدود 55%**
- Decision anti-flip / hysteresis / family vote matrix: **حدود 60%**
- Trade Plan / Entry / SL / TP / reward path: **حدود 70%**
- Risk / XAU sizing / margin: **حدود 75%**
- Indicator → cBot boundary: **حدود 75%**
- cBot activation / real execution / reconciliation / lifecycle: **حدود 60%**
- UI / arrows / flow bars / labels / alerts: **حدود 75%**
- Outcome / calibration / leakage audit: **حدود 55%**

### برآورد کلی
**حدود 72% تکمیل معماری و ممیزی production.**

برای رسیدن به «کامل و آماده اتکای واقعی»، هنوز حدود **28%** کار باقی است؛ مهم‌ترین بخش‌های باقی‌مانده:
1. Flow/DOM/AggressiveFlow و نمایش حجم/میله‌ها با semantics دقیق.
2. anti-flip + episode identity + family-level vote matrix.
3. pending candidate scoring و حذف مسیرهای امتیازدهی موازی.
4. cBot اجرای واقعی market/pending + idempotency + broker confirmation/reconciliation.
5. protection/trailing/BE/partial lifecycle.
6. UI final hardening و single-source visual/alert contract.
7. end-to-end runtime/compile/architecture gates و تست واقعی روی cTrader/demo.
8. outcome/calibration بدون leakage.

**نکته مهم:** این 72% به معنی «72% کدنویسی» نیست؛ پروژه بخش زیادی از کد را دارد، اما درصد باقی‌مانده بر اساس owner integrity، correctness، runtime execution و acceptance gates محاسبه شده است.

## 16. ممیزی عمیق — Batch 04 (Flow / DOM / Aggressive Flow)

### 16.1 semantics قطعی
- cTrader `Tick` فقط Time/Bid/Ask دارد؛ executed trade size در Tick API موجود نیست.
- `MarketDepthEntry.VolumeInUnits` حجم سفارشات موجود در DOM است، نه حجم معاملات اجراشده.
- بنابراین CFIP دو مفهوم را جدا نگه می‌دارد:
  - **DOM BUY/SELL:** حجم قابل مشاهده سفارشات Bid/Ask از broker feed.
  - **FLOW BUY/SELL TICKS:** proxy جهت‌دار از تغییرات متوالی midpoint؛ این «executed volume» نیست.
- این تفکیک جلوی یکی از خطرناک‌ترین خطاهای معنایی پروژه یعنی نمایش tick/DOM به‌عنوان real traded volume را می‌گیرد.

### 16.2 مالک Flow
- `AggressiveFlowAnalyzer` تنها owner طبقه‌بندی realtime flow proxy است.
- پنجره rolling برابر 30 ثانیه و حداکثر 4096 نمونه دارد؛ بنابراین رشد حافظه نامحدود نیست.
- tickهای معتبر بر اساس حرکت midpoint به BUY/SELL/NEUTRAL طبقه‌بندی می‌شوند.
- history/reconnect semantics در مرحله بعد باید با invalidation/reseed صریح تکمیل شود تا snapshot بعد از reconnect stale نشود.

### 16.3 Runtime
- `AggressiveFlowRuntime.cs` تنها owner lifecycle اتصال به `MarketData.GetTicks(Symbol.Name)` است.
- subscription در initialization و unsubscribe در OnDestroy انجام می‌شود.
- مسیر دوم برای tick listener ایجاد نشده است.

### 16.4 Panel
Footer اکنون چهار ردیف canonical دارد:
1. DOM BUY — VolumeInUnits
2. DOM SELL — VolumeInUnits
3. FLOW BUY TICKS — directional tick proxy
4. FLOW SELL TICKS — directional tick proxy

ارتفاع rail از 42 به 84 و minimum footer از 86 به 146 افزایش یافت تا چهار ردیف واقعاً فضای مستقل داشته باشند و روی alert/actionها overlap نکنند.

### 16.5 اصلاح مهم runtime
در همین batch مشخص شد timeout 30 ثانیه‌ای data initialization هنوز در `RuntimeInitialization.cs` باقی مانده بود. حذف شد.
- indicator دیگر به‌خاطر کندی provider/broker بعد از 30 ثانیه shutdown/fault نمی‌شود.
- freshness/staleness باید در data-consumer و execution-safety gateها اعمال شود، نه به‌عنوان lifetime timeout خود Indicator.
- commit: `9c3e0bb4b3a86845c3adaffdbacc4b63ebdbf9c8`

### 16.6 commits این batch
- `463c0246d707d4a438d0029852ec379510997fc1` — AggressiveFlowAnalyzer
- `b741b1ba07e1ce96a7431f02ff5c28a64b21be68` — AggressiveFlowRuntime
- `7d8203740895ef187d9a98ac2506bef2902e294d` — lifecycle integration
- `ad71cbff7d92d38cb508211baa63856b471e32fa` — four-row DOM/flow panel
- `5b19dbb6745694d2c6fe453a58368647da63cef0` — footer geometry
- `d954f0703aaca73451f1fca0bb07519a396a5b16` — acceptance gate

### 16.7 وضعیت Batch 04
- [x] DOM semantics
- [x] Aggressive-flow owner
- [x] bounded rolling proxy
- [x] runtime subscription lifecycle
- [x] four-row UI
- [x] explicit volume semantics
- [x] startup lifetime timeout removed
- [x] static acceptance gate
- [ ] reconnect/history reseed contract
- [ ] flow normalization against symbol/session regime
- [ ] arrow strength consumer alignment
- [ ] full compile/runtime/architecture CI verification

## 17. به‌روزرسانی پیشرفت — Batch 04
با تکمیل owner اولیه Flow/DOM و حذف timeout lifecycle:
- Flow / DOM / Aggressive Flow: **حدود 70%**
- UI flow presentation: **حدود 85%**
- Runtime initialization lifecycle: **حدود 90%**
- پیشرفت کلی معماری/ممیزی production: **حدود 74%**
- باقی‌مانده کل: **حدود 26%**

این درصد همچنان درصد «آمادگی معماری و correctness» است، نه درصد تعداد خطوط کد.

### نزدیک‌ترین گلوگاه‌های تکمیل
1. Reconnect/history reseed برای aggressive flow.
2. اتصال flow به canonical decision evidence بدون vote inflation.
3. Arrow strength و trend alignment از همان flow/decision snapshot.
4. Pending candidate scoring migration به canonical evidence lineage.
5. Anti-flip/hysteresis و episode identity.
6. cBot live/pending execution + reconciliation + protection lifecycle.
7. End-to-end compile/runtime/architecture gates و terminal verification.
8. Outcome/calibration/leakage audit.


## 2026-10-07 — Batch 04.1 Flow reconnect/history reliability
- AggressiveFlowAnalyzer remains the sole owner of the bounded 30-second tick-direction proxy.
- AggressiveFlowRuntime now owns Tick + HistoryLoaded + Reloaded lifecycle and replays the newest available ticks chronologically after startup/history refresh/reconnect.
- Reconnect resets the previous-mid state before replay, preventing provider-generation boundary artifacts.
- AggressiveFlowSnapshot exposes an explicit freshness check; UI fails closed after 5 seconds without a fresh observation.
- DOM BUY/SELL and FLOW BUY/SELL TICKS remain distinct semantic surfaces.
- No decision vote was added in this package. Flow-to-decision confirmation/modulation remains the next controlled package to avoid vote inflation.
