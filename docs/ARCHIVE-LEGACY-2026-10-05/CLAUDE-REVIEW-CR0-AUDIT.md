# CFIP — CR-0 Review Audit Closure

Date: 2026-09-30

## 1. Scope

CR-0 closes the audit-only gate for the three supplied review prompts:

- A1–A12
- B1–B12
- C1–C9

This phase changes documentation only. No production `.cs`, `.csproj`, `.sln`, workflow or runtime behavior is changed.

Repository baseline audited:

- main: `c4928f86ba8a5c34be547e06a2e73c894708d7c2`
- Indicator remains the current production host.
- Local cBot extraction remains blocked until CR-FINAL.
- The review prompts are treated as evidence/risk inventory, not as automatic truth.

## 2. Repository parameter truth

The current parameter directory contains **29 source files** representing **27 logical parameter groups** (12 and 14 are split into multiple source files).

A fresh source-level count of `[Parameter(...)]` declarations across all 29 files is **566**. The older roadmap baseline of 534 is therefore stale and is corrected by this audit.

Per-file counts:

| File | Parameters |
|---|---:|
| 01_decision.cs | 11 |
| 02_mtf.cs | 9 |
| 03_structure.cs | 5 |
| 04_zones.cs | 16 |
| 05_liquidity.cs | 11 |
| 06_indicators.cs | 8 |
| 07_entry_precision.cs | 11 |
| 08_smart_weights.cs | 11 |
| 09_risk_targets.cs | 14 |
| 10_live_management.cs | 19 |
| 11_filters.cs | 15 |
| 12_alerts_advanced.cs | 25 |
| 12_alerts_core.cs | 21 |
| 13_auto_trading.cs | 57 |
| 14_display_advanced.cs | 10 |
| 14_display_core.cs | 48 |
| 14_display_panel.cs | 3 |
| 15_control_advanced.cs | 59 |
| 15_intelligence_early.cs | 18 |
| 16_accuracy.cs | 31 |
| 17_smart_engine.cs | 27 |
| 20_confluence_extensions.cs | 10 |
| 21_complete_intelligence.cs | 53 |
| 22_safety_precision.cs | 8 |
| 23_structural_execution.cs | 19 |
| 24_smart_execution.cs | 10 |
| 25_oss_analytics.cs | 3 |
| 26_wave_trend.cs | 13 |
| 27_parallel_opportunities.cs | 7 |
| 28_news_guard.cs | 14 |
| **Total** | **566** |

### Parameter ownership rule

The audit confirms the required migration rule:

- **Indicator-owned:** parameters that change evidence, MTF interpretation, structure/liquidity, FVG/OB quality, reaction, confidence, smart quality, regime, entry proposal, structural stop proposal, target discovery, analytical RR/reward path, scenario selection, news intelligence, or presentation.
- **cBot-owned:** parameters that only control broker execution, account-enforced risk, broker identity, broker protection, order submission/cancellation, live broker lifecycle, broker mutation throttling, or automatic close/partial-close behavior.
- **Split mixed groups:** the source file is not moved wholesale. Each parameter keeps a single owner based on its actual responsibility. No parameter is duplicated with two independent values.
- **Shared signal data is a contract, not shared mutable state.**

### Explicit cBot-sensitive parameter set

The following current parameters are classified as cBot-owned at the separation boundary unless an implementation audit proves that a named parameter is actually analytical:

**13 · AUTO TRADING — broker/execution set**

`EnableAutoTrading`,
`EnableAutomaticOrders`,
`PendingOrderMode`,
`PendingOrderExpiryMinutes`,
`PendingEntryBufferAtr`,
`PendingMinimumConfidence`,
`PendingMinimumSmartQuality`,
`PendingMinimumTrendQuality`,
`ReversalCloseMinimumEvidence`,
`ReversalCloseMinimumMtf`,
`ConfirmedSignalsOnly`,
`SizingMode`,
`RiskPercentEquity`,
`FixedLots`,
`MinimumAutoConfidence`,
`MinimumAutoSmartQuality`,
`MinimumAutoLevelQuality`,
`AutoTpStage`,
`EnableDynamicTpAdvance`,
`TpAdvanceProximityPercent`,
`EnablePartialTakeProfit`,
`PartialCloseTp1Percent`,
`PartialCloseTp2Percent`,
`MoveToBreakEvenAfterPartial`,
`EnableReversalProtectionClose`,
`ReversalProtectionMinimumQuality`,
`MaximumOpenPositions`,
`EnableDailyLossLimit`,
`MaximumDailyLossPercent`,
`UseMarketHoursGuard`,
`UseAutoMarginGuard`,
`MaxAutoMarginUsagePercent`,
`MarginBufferPercent`,
`AutoTradeLabel`,
`AutoBrokerProtection`,
`OneOrderPerSignal`,
`IncludeSpreadInRiskSizing`,
`ManagedActionsOnly`,
`AutoProtectBrokerPositions`,
`ManagedPositionLabel`,
`SyncBrokerTakeProfit`,
`PreventBrokerTpBackwardMove`,
`BrokerModifyCooldownMs`,
`EnableAggressiveAutoEntry`,
`AggressiveMinimumConfidence`,
`AggressiveMinimumEvidence`,
`AggressiveMinimumSmartQuality`,
`AggressiveRiskPercentEquity`,
`AggressiveTpStage`,
`AggressiveRequireSmartAgreement`.

Presentation-only parameters in this same file remain Indicator-owned:
`AutoTradingReminder`, `ShowTradeActionButtons`, `AlwaysShowSafetyButtons`, `ActionButtonMargin`, `ActionButtonWidth`, `ActionButtonHeight`.

**10 · LIVE MANAGEMENT — cBot live broker lifecycle set**

The 19 parameters in `10_live_management.cs` are cBot-sensitive because they govern live protection/lifecycle timing or broker mutation behavior:

`EnableLiveExitManagement`,
`MoveSlToBreakEven`,
`BreakEvenTriggerRR`,
`BreakEvenBufferPips`,
`UseSpreadAwareBreakEven`,
`RiskFreeLockPips`,
`SmartTrailMomentumBonusAtr`,
`SmartTrailTightenAtRR`,
`EnableStructuralSlRepricing`,
`SlRepriceStartRR`,
`SlRepriceBreathingAtr`,
`SlRepriceStepAtr`,
`UpdateUnhitTargets`,
`TargetUpdateTriggerRR`,
`EnableProfitExhaustionProtection`,
`ExhaustionMinimumPeakRR`,
`ExhaustionRetracementPercent`,
`ExhaustionPressureThreshold`,
`ExhaustionMinimumOppositeEvidence`.

The future split is by authority: Indicator computes/requests analytical exit or target intent; cBot validates and performs the broker mutation.

**11 · FILTERS — mixed boundary**

Indicator-owned: session filter, session times, EOD alert timing, spread/volatility/Friday/news analytical filters.

cBot-owned:
`EnableEndOfDayAutoClose`.

**28 · NEWS GUARD — mixed boundary**

Indicator-owned: news feed enablement, feed URI, refresh interval, impact windows, currency mapping, stale policy/status presentation.

cBot-owned:
`CancelPendingBeforeHighImpactNews`,
`CloseActiveBeforeHighImpactNews`.

**09 · RISK & TARGETS / 15 · CONTROL / 24 · SMART EXECUTION**

These groups are mixed in implementation dependency but are primarily Indicator-owned because they change the analytical plan, target/stop proposal, reward-path quality or suitability/risk interpretation. Account-normalized final volume, account capacity and broker enforcement move to cBot. No whole-file move is authorized.

## 3. Exact finding inventory

### Prompt 1

| ID | Exact current owner(s) inspected | Relevant methods / symbols | Verdict | Next phase |
|---|---|---|---|---|
| A1 | `Trading/Alerts/EndOfDayAlert.cs`; `Trading/Pending/Placement/PendingOrderCleanup.cs` | `CheckEndOfDayAlert`; `CleanupPendingOrdersIfNeeded` | CONFIRMED | CR1.1 |
| A2 | `Trading/Risk/DailyLossGuard.cs`; auto-entry callers | `DailyLossLimitHit` | CONFIRMED | CR1.2 |
| A3 | `Planning/TradePlan/Sources/PreviousPeriodTargetSource.cs`; suitability | `AddPreviousPeriodLevels`; `CalculateMarketSuitability` | CONFIRMED | CR1.1 |
| A4 | `Analysis/Structure/SwingPointAnalyzer.cs`; `LiquiditySweepAnalyzer.cs`; `StructureAnalyzer.cs`; `EqualLevelAnalyzer.cs` | `FindSwingHigh/Low`, `FindSwingHighAbove/LowBelow`, `BullLiquiditySweep`, `BearLiquiditySweep`, structure break helpers | PARTIAL — newest-swing claim is too broad; active/unbroken semantics and ATR-relative penetration are valid audit targets | CR2.1 |
| A5 | `Trading/Intelligence/EconomicNewsCalendarClient.cs`; news protection/evaluation | `InferNewsCurrencies`; `IsNewsEventRelevant`; `TryParseEconomicEventTime`; `RefreshEconomicNewsIfNeeded` | CONFIRMED core. AccessRights.None is not itself a defect; target-terminal verification remains required. | CR1.3 |
| A6 | `Runtime/Calculation/CalculationCycle.cs`; `CalculationClosedBar.cs`; `CalculationStageIsolation.cs`; `CalculationLiveCycle.cs` | `Calculate`; `ProcessNewClosedBar`; preparation/closed/live stage runners; `RefreshLiveDecisionActionability` | CONFIRMED | CR1.4 |
| A7 | runtime broker reads; FVG/OB calculations; history/archive persistence | broker synchronization/reconciliation stages; zone calculators; `ArchiveOutcomeObservation` family | CONFIRMED risk | CR1.5 |
| A8 | FVG zone lifecycle/quality | `BuildManagedFvgZone`; `CalculateFvgQuality`; `TryApplyFvgMitigation` | CONFIRMED | CR1.6 |
| A9 | Decision smart/threshold filters | `EvaluateDecisionSmartGates`; threshold/consensus evaluators | CONFIRMED | CR1.7 |
| A10 | `Trading/Risk/VolumeSizer.cs`; `AggressiveVolumeSizer.cs`; `RiskAmountCalculator.cs`; risk policies | `CalculateVolume`; `CalculateAggressiveVolume`; `Calculate`; effective-risk policy methods | AUDIT CLOSED — exact live volume path located; behavior fix deferred to CR1.7 only where a deterministic defect is proven | CR1.7 |
| A11 | `Trading/Identity/BrokerIdentity.cs`; `ManagedPositionGuards.cs`; label formatter | `IsManagedPosition`; `IsManagedPendingOrder`; `GetManagedPosition`; `GetManagedPendingOrder`; `HasManagedOpenPosition`; `NormalizeLabel` | CONFIRMED | CR1.8 |
| A12 | parameter/docs/session semantics | parameter groups + `SessionWindowEvaluator.IsInsideSessionWindow` | MIXED — documentation/counting corrections are now captured; behavior items go to owning phases | CR1.9 |

### Prompt 2

| ID | Exact current owner(s) inspected | Relevant methods / symbols | Verdict | Next phase |
|---|---|---|---|---|
| B1 | `Analysis/Structure/StructureAnalyzer.cs` | `BullStructure`, `BearStructure`, `BullMss`, `BearMss`, `BullChoch`, `BearChoch` | CONFIRMED | CR2.1 |
| B2 | structure + decision orchestration | structure event methods; decision structure/evidence gates | PARTIAL — event freshness/duplication needs explicit contract, not wholesale rewrite | CR2.1 |
| B3 | `Analysis/Reaction/ReactionAnalyzer.cs` | `BuildReaction` | CONFIRMED | CR2.2 |
| B4 | decision filters/gates | `Evaluate` in threshold and consensus evaluators; `EvaluateDecisionSmartGates`; confirmation/structure gates | CONFIRMED | CR2.3 |
| B5 | pending policy/preparation/orchestration | `TrendContinuationStrong`; `ReversalSetupStrong`; `PendingModeAllowsStop`; `PendingModeAllowsLimit`; `TrySmartPendingOrders`; `TryPrepareContinuationStop`; `TryPrepareReversalLimit` | CONFIRMED | CR2.4 |
| B6 | lifecycle handlers, idempotency and outcome telemetry | `OnPositionOpened`; `OnPositionClosed`; `OnPendingOrderFilled`; `OnPendingOrderCancelled`; `TryBegin`; `RecordManagedOutcome`; `ReconcileLivePlanToActualFill` | CONFIRMED core; broker-history/deal details require manual verification | CR2.5 |
| B7 | Order Block analysis | `FindNearestOrderBlock`; `BuildOrderBlockCandidate`; `CalculateOrderBlockQuality`; confluence/evidence builders | CONFIRMED performance risk; quality tuning requires deterministic evidence | CR2.6 |
| B8 | WaveTrend | `GetSnapshot`; `CalculateIndex`; `UpdateEma`; `UpdateMovingAverage`; RSI/MFI helpers | CONFIRMED core correctness/audit target | CR2.7 |
| B9 | historical presentation | `RenderHistoricalSignals`; `RemoveHistoricalObjects`; new-closed-bar presentation trigger | CONFIRMED performance/semantic risk, but not every `Calculate` | CR2.8 |
| B10 | plan/stop management | structural stop fallback; `CalculateProtectedStop`; `BetterStop` | CONFIRMED fallback. Tight-stop scoring remains design/evidence issue. | CR2.9 |
| B11 | divergence | `AnalyzeDivergence`; `EvaluateLowDivergence`; `EvaluateHighDivergence` | CONFIRMED | CR2.9 |
| B12 | reaction/candlestick rejection semantics | `BuildReaction`; structure/rejection evidence | CONFIRMED | CR2.9 |

### Prompt 3

| ID | Exact current owner(s) inspected | Relevant methods / symbols | Verdict | Next phase |
|---|---|---|---|---|
| C1 | live false-signal guard | `ProcessActivePlanFalseSignalRisk`; active plan evaluation | CONFIRMED | CR3.1 |
| C2 | live invalidation + broker SL reality | `ProcessActivePlanFalseSignalRisk`; broker stop accessor/reconciler | CONFIRMED | CR3.1 |
| C3 | expected-value helper | `ProxyExpectedValue` | CONFIRMED — synthetic proxy, not calibrated probability × realized RR | CR3.2 |
| C4 | early prediction | `BuildEarlyPrediction` | CONFIRMED | CR3.2 |
| C5 | partial TP/server ladder | `ExecutePartialClose`; `TryBuildServerSideTakeProfitLadder`; `TryAdvanceServerSideTakeProfitLadder`; target progression | MIXED — current BE rejection path does not overwrite `_plan.Stop`; remaining ladder/partial issues proceed | CR3.3 |
| C6 | live protection/recovery | `CalculateProtectedStop`; `UpdateActivePlanLiveManagement`; recovery/lifecycle readers | DESIGN RISK + AUDIT | CR3.3 |
| C7 | controls/popup | `ApplyAutoTradingQuickToggleClick`; `ApplyAutomaticOrdersQuickToggleClick`; `ShowPopup` | CONFIRMED | CR3.4 |
| C8 | calibration/outcome | `CalculateContextual`; `RegisterCalibratedOutcome`; `RecordManagedOutcome`; persistence/rebuild methods | CONFIRMED core | CR3.5 |
| C9 | RR observability | `ValidatePlanRewardStructure`; plan-reward caller paths | DESIGN RISK / observability gap | CR3.5 |

## 4. Unified threshold truth

Threshold classes are now explicitly separated into semantic families:

1. **Decision threshold:** base confidence, edge, smart quality, timeframe agreement, independent evidence, structural confirmations.
2. **Smart/regime threshold:** consensus share, regime quality, trend/range/compression/no-trade thresholds.
3. **Execution-eligibility threshold:** minimum automatic confidence/quality/level quality and broker/account safety gates.
4. **Plan reward threshold:** minimum RR, target spacing, target obstacle/clearance, maximum reward extension.
5. **Live-management threshold:** break-even, structural SL repricing, target advancement, exhaustion/reversal pressure.
6. **Zone quality threshold:** FVG/OB minimum quality, displacement and freshness/age semantics.
7. **Divergence/reaction threshold:** divergence candidate quality and reaction watch/confirmed/strong thresholds.

Rule: each threshold has one semantic owner; callers may consume it but may not create a second hidden clamp or magic floor.

## 5. Unified session/time semantics

Canonical audit model:

- all analytical closed-bar references use one UTC reference;
- a session has an explicit start/end interpretation;
- start==end must have one documented meaning across every consumer;
- overnight sessions (e.g. 22→06) are distinct from same-day sessions (06→20);
- EOD auto-close is a bounded window around the intended session boundary;
- completion is broker-confirmed, not inferred from submission attempt;
- pending cleanup and position cleanup use the same managed identity and boundary;
- prior D1/W1 target sources must consume the latest completed period, not one additional period backwards.

CR1.1 owns the implementation of these semantics.

## 6. Unified outcome/accounting semantics

The project now distinguishes:

- **broker/account facts:** balance, equity, margin, realized deal history, floating P/L, position/pending state;
- **managed outcome observation:** one coherent logical outcome record keyed by managed identity/scenario;
- **calibration observation:** confidence/regime/direction/lane bucket + observed outcome;
- **archive:** persistent 90-day/quarterly records;
- **broker deal history:** authoritative evidence for partial-close and multiple-deal aggregation.

CR1.2 owns the daily-loss accounting contract; CR2.5 owns lifecycle/outcome aggregation; CR3.5 owns calibration semantics.

## 7. cBot boundary-sensitive source inventory

Definite broker mutation owners identified:

- `Trading/Execution/BrokerMarketOrderMutation.cs`
- `Trading/Execution/BrokerPendingOrderPlacement.cs`
- `Trading/Execution/BrokerLimitOrderPlacement.cs`
- `Trading/Execution/BrokerPendingOrderCancellation.cs`
- `Trading/Execution/BrokerPositionCloseMutation.cs`
- `Trading/Execution/BrokerStopLossMutation.cs`
- `Trading/Execution/BrokerTakeProfitMutation.cs`
- broker-facing mutation methods in `BrokerProtectionCoordinator.cs`
- broker-facing portions of AutomaticMarket/Aggressive/Pending placement
- broker-confirmed lifecycle/event/recovery paths
- broker mutation portions of LiveManagement

Indicator-only analytical owners that must not migrate wholesale:

- Analysis/**
- Planning/**
- UI/**
- `ScenarioExecutionPolicy.cs`
- `ExecutionPlanPreparation.cs`
- signal/decision/zone/reaction/WaveTrend/divergence intelligence

Mixed files must be split by dependency rather than copied.

## 8. Manual cTrader verification matrix

These are acceptance tests, not claims already proven by static source reading.

| Area | Test |
|---|---|
| HTTP/news | feed success, HTTP failure, malformed feed, stale feed, never-loaded feed, refresh throttling; verify target cTrader behavior with current API |
| Access rights | confirm network call works under current declared access rights; no permission workaround is introduced merely because of review text |
| LocalStorage/files | persistence, restart, scope, file/archive creation, 90-day rotation, failure containment |
| Indicator→cBot | referenced custom Indicator instance, structured read-only signal surface, instance identity, revision/sequence ordering, no chart scraping |
| Trading permission | disabled/enabled permission and recovery behavior |
| Market order | submitted vs accepted vs filled semantics; broker-confirmed state only |
| Pending Stop/Limit | exact order type, trigger/entry, expiry, cancellation and fill events |
| Broker SL/TP | valid modification, rejection, retry/recovery, forward-only protection |
| Partial close | full deal-history inspection, multiple deals, partial-close aggregation, BE success/rejection |
| Restart/reconnect | reconciliation before assumptions; no duplicate entry; managed pending/position adoption |
| EOD | same-day and overnight session, bounded close window, post-boundary open, pending cleanup |
| Daily loss | broker day boundary, realized + floating contract, deposit/withdrawal, restart while locked |
| Multi-instance identity | same symbol, different instance/signal identity; no cross-instance adoption |
| UI | quick toggle re-arm, read-only execution authority after cBot split, popup message queuing/retention |
| Target progression | TP cannot move backward after a forward target has been broker-confirmed |
| Panel performance | startup time, first render, new-bar refresh, hide/show controls, no hot-path network/file stall |

## 9. CR-0 disposition

Status: **COMPLETE**

Production behavior changed: **NO**

Audit-first items resolved/deferred:

- A10 exact volume source located; behavior change deferred to CR1.7 only where deterministic evidence exists.
- A4 narrowed to the proven active-swing/ATR-penetration semantics.
- A5 AccessRights.None subclaim rejected as a source-based defect; terminal verification retained.
- B2 event freshness remains a dedicated CR2.1 contract.
- B6 partial-close/deal aggregation retains a mandatory manual broker-history test.
- B9 corrected from “every Calculate” to host-bar-triggered historical rebuild cost.
- C5 stale BE-rejection subclaim rejected; current source only commits plan stop to entry after successful broker mutation.
- C6 retained as design/audit work, not treated as a proven bug.

Next implementation phase: **CR1.1 — Session/EOD and period-reference correctness**.

This document is the source inventory for CR-FINAL and for the later cBot separation track.
