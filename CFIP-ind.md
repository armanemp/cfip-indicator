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
