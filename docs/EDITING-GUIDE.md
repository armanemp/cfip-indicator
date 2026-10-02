# CFIP Indicator — Editing Guide

Edit the smallest authoritative module that owns the behavior.

| Concern | Owner |
|---|---|
| Public cTrader parameters | `Indicator/Parameters/*.cs` — one file per parameter Group |
| Domain models | `Core/Models/*.cs` — one type per file |
| Enums | `Core/Enums/*.cs` — one enum per file |
| Native indicators | `Analysis/Indicators/*.cs` |
| Production OSS numerical adapters | `Analysis/Indicators/External/Skender*.cs`, `OssIndicatorConfluenceAnalyzer.cs`, `OssQuoteSeriesCache.cs`, `OssIndicatorSnapshotCache.cs` |
| Market-frame orchestration | `Analysis/Market/MarketFrameAnalyzer.cs` |
| Market-frame evidence | `Analysis/Market/MarketFrameEvidence.cs` |
| Market-frame scoring / quality | `Analysis/Market/MarketFrameScoringService.cs`, `MarketFrameScoring.cs` |
| Market-frame scoring | `Analysis/Market/MarketFrameScoring.cs` |
| Market context: volume | `Analysis/Market/VolumeExpansionAnalyzer.cs` |
| Market context: MACD | `Analysis/Market/MacdBiasAnalyzer.cs` |
| Market context: VWAP | `Analysis/Market/VwapBiasAnalyzer.cs` |
| Market context: volatility | `Analysis/Market/HealthyVolatilityAnalyzer.cs` |
| Market context: premium/discount | `Analysis/Market/PremiumDiscountAnalyzer.cs` |
| Market context: live bias | `Analysis/Market/LiveBiasAnalyzer.cs` |
| Decision | `Analysis/Market/Decision/*.cs` |
| Signal-plan coordination | `Trading/Validation/SignalPlanCoordinator.cs` |
| Plan creation eligibility | `Trading/Validation/PlanCreationEligibility.cs` |
| Decision block-reason policy | `Trading/Validation/DecisionBlockReasonPolicy.cs` |
| Live execution-gate policy | `Trading/Validation/LiveExecutionGateReasonPolicy.cs` |
| Adaptive smart thresholds | `Trading/Validation/SmartThresholdPolicy.cs` |
| Decision reason formatting | `Analysis/Market/Decision/DecisionReasonFormatter.cs` |
| Decision score normalization | `Analysis/Market/Decision/DecisionFrameContributionCalculator.cs` |
| Correlation-aware evidence fusion | `Analysis/Market/Decision/IndependentEvidenceFusionCalculator.cs` |
| Reaction | `Analysis/Reaction/*.cs` |
| Liquidity sweep | `Analysis/Structure/LiquiditySweepAnalyzer.cs` |
| Swing points | `Analysis/Structure/SwingPointAnalyzer.cs` |
| Equal highs/lows | `Analysis/Structure/EqualLevelAnalyzer.cs` |
| FVG detection / selection | `Analysis/Structure/Zones/FvgDetectionAnalyzer.cs` |
| FVG lifecycle / mitigation | `Analysis/Structure/Zones/FvgLifecycleAnalyzer.cs` |
| FVG lifecycle orchestration | `Analysis/Structure/Zones/FvgLifecycleAnalyzer.cs` |
| FVG mitigation state | `Analysis/Structure/Zones/FvgMitigationEvaluator.cs` |
| FVG quality | `Analysis/Structure/Zones/FvgZoneQualityCalculator.cs` |
| Order Block analysis | `Analysis/Structure/Zones/OrderBlockAnalyzer.cs` |
| Order Block confluence | `Analysis/Structure/Zones/OrderBlockConfluenceAnalyzer.cs` |
| Entry trigger readiness | `Planning/Entry/ClosedBarTriggerReadyEvaluator.cs` |
| Bullish trigger scoring | `Planning/Entry/BullTriggerScoreAnalyzer.cs` |
| Bearish trigger scoring | `Planning/Entry/BearTriggerScoreAnalyzer.cs` |
| Execution model / trigger validation | `Planning/Execution/*.cs` |
| Execution-zone orchestration | `Planning/Execution/ExecutionZoneBuilder.cs` |
| Execution-zone candidate selection | `Planning/Execution/ExecutionZoneCandidateSelector.cs` |
| Execution-zone quality | `Planning/Execution/ExecutionZoneQualityEvaluator.cs` |
| Plan construction orchestration | `Planning/TradePlan/PlanBuilder.cs` |
| Plan entry/stop/risk preparation | `Planning/TradePlan/PlanInputPreparation.cs` |
| Plan target preparation/validation | `Planning/TradePlan/PlanTargetPreparation.cs` |
| Target stage orchestration | `Planning/TradePlan/TargetSelector.cs` |
| Target selection policy | `Planning/TradePlan/TargetSelectionPolicy.cs` |
| Target candidate filtering/scoring | `Planning/TradePlan/TargetCandidateEvaluator.cs` |
| Order-block candidate orchestration | `Analysis/Structure/Zones/OrderBlockCandidateBuilder.cs` |
| Order-block impulse/structure evidence | `Analysis/Structure/Zones/OrderBlockEvidenceBuilder.cs` |
| Order-block mitigation | `Analysis/Structure/Zones/OrderBlockMitigationGuard.cs` |
| Order-block quality | `Analysis/Structure/Zones/OrderBlockQualityCalculator.cs` |
| Plan materialization/target metadata | `Planning/TradePlan/PlanMaterialization.cs` |
| Plan integrity orchestration | `Planning/TradePlan/PlanIntegrityValidator.cs` |
| Plan protection/entry integrity | `Planning/TradePlan/PlanProtectionIntegrityValidator.cs` |
| Plan reward integrity | `Planning/TradePlan/PlanRewardIntegrityValidator.cs` |
| Plan market constraints | `Planning/TradePlan/PlanMarketConstraintValidator.cs` |
| Structural stop | `Planning/TradePlan/StructuralStopPlanner.cs` |
| Structural stop selection orchestration | `Planning/TradePlan/StructuralStopCandidateSelector.cs` |
| Structural stop candidate evaluation | `Planning/TradePlan/StructuralStopCandidateEvaluator.cs` |
| Structural stop finalization | `Planning/TradePlan/StructuralStopFinalizer.cs` |
| Minimum required RR | `Planning/TradePlan/MinimumRequiredRiskRewardCalculator.cs` |
| Target levels | `Planning/TradePlan/TargetLevelBuilder.cs` |
| Target candidate merging | `Planning/TradePlan/TargetLevelCandidateMerger.cs`, `TargetLevelMerger.cs` |
| Target selection | `Planning/TradePlan/TargetSelector.cs`, `TargetStageSelector.cs` |
| Target metadata | `Planning/TradePlan/TargetMetadataEnricher.cs` |
| Target progression | `Planning/TradePlan/TargetProgressionValidator.cs`, `TargetProgressionRule.cs` |
| Executable plan preparation | `Trading/Execution/ExecutionPlanPreparation.cs` |
| Runtime initialization | `Runtime/Initialization/*.cs` |
| MTF context / closed-bar mapping | `Runtime/Mtf/*.cs` |
| Calculation entrypoint | `Runtime/Calculation/CalculationCycle.cs` — orchestration only |
| Calculation preparation | `Runtime/Calculation/CalculationPreparation.cs` |
| Newly-closed-bar calculation | `Runtime/Calculation/CalculationClosedBar.cs` |
| Decision alerts in calculation cycle | `Runtime/Calculation/CalculationDecisionAlerts.cs` |
| Live calculation cycle | `Runtime/Calculation/CalculationLiveCycle.cs` |
| Runtime / MTF / calculation support | `Runtime/**/*.cs` |
| Automatic market execution | `Trading/Execution/AutomaticMarket/*.cs` |
| Automatic market pre-trade eligibility | `Trading/Execution/AutomaticMarket/AutomaticMarketPreTradeEligibility.cs` |
| Automatic market execution preparation | `Trading/Execution/AutomaticMarket/AutomaticMarketExecutionPreparation.cs` |
| Automatic market submission validation | `Trading/Execution/AutomaticMarket/AutomaticMarketSubmissionValidator.cs` |
| Automatic market fill reconciliation | `Trading/Execution/AutomaticMarket/AutomaticMarketFillReconciliation.cs` |
| Automatic market post-fill target resolution | `Trading/Execution/AutomaticMarket/AutomaticMarketPostFillTargetResolver.cs` |
| Cross-path execution invariants | `tools/verify_architecture.py` — Market / Aggressive / Pending execution contract checks |
| Aggressive execution | `Trading/Execution/Aggressive/*.cs` |
| Aggressive pre-trade eligibility | `Trading/Execution/Aggressive/AggressivePreTradeEligibility.cs` |
| Aggressive execution preparation | `Trading/Execution/Aggressive/AggressiveExecutionPreparation.cs` |
| Aggressive accepted-fill handling | `Trading/Execution/Aggressive/AggressiveAcceptedFillHandler.cs` |
| Pending orders | `Trading/Pending/**/*.cs` |
| Pending submission validation | `Trading/Pending/Placement/PendingSubmissionValidator.cs` |
| Continuation stop orchestration | `Trading/Pending/Placement/ContinuationStopPlacement.cs` |
| Continuation stop preparation | `Trading/Pending/Placement/ContinuationStopPreparation.cs` |
| Reversal limit orchestration | `Trading/Pending/Placement/ReversalLimitPlacement.cs` |
| Reversal limit preparation | `Trading/Pending/Placement/ReversalLimitPreparation.cs` |
| Market broker mutation | `Trading/Execution/BrokerMarketOrderMutation.cs` |
| Pending stop-order mutation | `src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs` |
| Pending limit-order mutation | `Trading/Execution/BrokerLimitOrderPlacement.cs` |
| Pending cancellation mutation | `Trading/Execution/BrokerPendingOrderCancellation.cs` |
| Stop-loss mutation | `Trading/Execution/BrokerStopLossMutation.cs` |
| Take-profit mutation | `Trading/Execution/BrokerTakeProfitMutation.cs` |
| Position-close mutation | `Trading/Execution/BrokerPositionCloseMutation.cs` |
| Broker protection coordination | `Trading/Execution/BrokerProtectionCoordinator.cs` |
| Broker mutation confirmation policy | `Trading/Execution/BrokerConfirmationPolicy.cs` |
| Broker identity | `Trading/Identity/*.cs` |
| Risk / suitability | `Trading/Risk/*.cs` |
| Daily loss guard | `Trading/Risk/DailyLossGuard.cs` |
| Risk percent policy | `Trading/Risk/RiskPercentPolicy.cs` |
| Risk amount | `Trading/Risk/RiskAmountCalculator.cs` |
| Margin safety | `Trading/Risk/MarginSafetyCalculator.cs`, `MarginUsagePolicy.cs` |
| Volume sizing | `Trading/Risk/VolumeSizer.cs`, `AggressiveVolumeSizer.cs` |
| Market suitability | `Trading/Risk/SuitabilityCalculator.cs`, `MarketSuitabilityGuard.cs` |
| Session window | `Trading/Risk/SessionWindowEvaluator.cs` |
| Auto-trade safety | `Trading/Risk/AutoTradeSafetyGuard.cs` |
| Lifecycle events | `Trading/Lifecycle/*.cs` |
| Pending-fill plan | `Trading/Lifecycle/PendingFillPlanBuilder.cs` |
| Pending-fill protection | `Trading/Lifecycle/PendingFillProtectionCoordinator.cs` |
| Broker fill reconciliation | `Trading/Lifecycle/LiveFillReconciliation.cs` |
| Broker state snapshot | `Trading/Lifecycle/BrokerStateSnapshot.cs` |
| Broker protection state evaluation | `Trading/Lifecycle/BrokerProtectionStateEvaluator.cs` |
| Live-plan recovery | `Trading/Lifecycle/ManagedLivePlanRecovery.cs`, `LivePlanFactory.cs` |
| Live-plan target recovery | `Trading/Lifecycle/LivePlanTargetEnrichment.cs`, `LivePlanFurtherTargetSelector.cs` |
| Position/pending circuit breakers | `Trading/Lifecycle/PositionCircuitBreaker.cs`, `PendingOrderCircuitBreaker.cs` |
| Live-plan exit / managed lookup | `Trading/Lifecycle/LivePlanExitCoordinator.cs`, `ManagedPositionLookup.cs` |
| Execution runtime state | `Trading/Execution/State/*.cs` — auto-trading state, lifecycle state, target stage, labels |
| Live management | `Trading/LiveManagement/*.cs` |
| Live target progression orchestration | `Trading/LiveManagement/TargetProgression.cs` |
| Live target candidate evaluation | `Trading/LiveManagement/LiveTargetCandidateEvaluator.cs` |
| Plan TP risk/reward recalculation | `Trading/LiveManagement/PlanRiskRewardRecalculator.cs` |
| Prediction / intelligence | `Trading/Intelligence/**/*.cs` |
| Fresh trigger evidence | `Trading/Intelligence/FreshTriggerEvidenceAnalyzer.cs` |
| Structural sequence | `Trading/Intelligence/StructuralSequenceAnalyzer.cs` |
| Entry location quality | `Trading/Intelligence/EntryLocationQualityAnalyzer.cs` |
| Proxy expected value | `Trading/Intelligence/ProxyExpectedValueCalculator.cs` |
| No-trade regime | `Trading/Intelligence/NoTradeRegimeAnalyzer.cs` |
| Alerts | `Trading/Alerts/*.cs` |
| Validation | `Trading/Validation/*.cs` |
| Chart | `UI/Chart/**/*.cs` |
| Panel | `UI/Panel/**/*.cs` |
| Panel overview composition | `UI/Panel/Rows/PanelOverviewRowsRenderer.cs` |
| Panel overview state rows | `UI/Panel/Rows/PanelOverviewStateRowsRenderer.cs` |
| Panel overview execution rows | `UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs` |
| Panel overview diagnostics | `UI/Panel/Rows/PanelOverviewDiagnosticRowsRenderer.cs` |
| Panel trade-plan composition | `UI/Panel/Rows/PanelTradePlanRowsRenderer.cs` |
| Panel trade-plan level rows | `UI/Panel/Rows/PanelTradePlanLevelRowsRenderer.cs` |
| Panel trade-plan live rows | `UI/Panel/Rows/PanelTradePlanLiveRowsRenderer.cs` |
| Popup | `UI/Popup/**/*.cs` |
| Historical rendering | `UI/Historical/**/*.cs` |
| Shared math/text/time utilities | `Core/{Math,Text,Time}/*.cs` |

Do not add compatibility aliases, duplicate business rules or a second execution path. Update the authoritative owner, migrate callers, remove the old owner, then run static and runtime acceptance.


| Broker fill reconciliation | `Trading/Lifecycle/LiveFillReconciliation.cs` |
| Broker state snapshot | `Trading/Lifecycle/BrokerStateSnapshot.cs` |
| Pending fill plan | `Trading/Lifecycle/PendingFillPlanBuilder.cs` |
| Pending fill protection | `Trading/Lifecycle/PendingFillProtectionCoordinator.cs` |
| Outcome telemetry | Trading/Intelligence/OutcomeTelemetryEngine.cs |
| Prediction calculations | Trading/Intelligence/Prediction/ |
| Prediction rendering | UI/Chart/PredictionRenderer.cs |
| Pending-order chart rendering | UI/Chart/PendingOrderRenderer.cs |
| Outcome chart markers | UI/Chart/OutcomeMarkerRenderer.cs |

Presentation methods should be added under UI even when their callers are in Trading. Broker mutation methods must remain in their execution/lifecycle ownership directories.


| Market index/range helpers | Analysis/Market/Math/IndexMath.cs |
| Price normalization/protection math | Trading/Execution/PriceMath.cs |
| Platform-neutral numeric guards | Core/Math/NumericGuards.cs |
