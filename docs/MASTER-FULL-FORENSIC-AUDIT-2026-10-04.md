# CFIP — MASTER FULL FORENSIC AUDIT & COMPLETE PROJECT INVENTORY
## 2026-10-04 — Zero-to-Full Repository Baseline

> Purpose: master inventory/checklist for restarting the project audit from the root. This is broader than a feature-phase document.
>
> Status rule: checklist items are audit targets/potential failure modes, not claims that the defect already exists. Confirmed findings are separated below.

## 1. Audit baseline

- Repository: armanemp/cfip-indicator
- Branch: main
- Baseline commit: 8e061bfb70fcac356de190d8057d59246e22313a
- Audit date: 2026-10-04
- Total repository files: 1113
- Directory entries: 82
- Markdown documents: 204
- C# files: 731
- Python audit/tool files: 155
- Project files: 12
- Production Indicator C# files: 667
- Production cBot C# files: 22
- Contracts C# files: 22
- Active production boundary: CFIP.Indicator + CFIP.Contracts + CFIP.cBot
- Current public-parameter semantic audit baseline: 548
- Current README parameter count: 548
- M15: canonical decision/execution reference; M5: trigger/entry precision; M1: optional confirmation
- Smart arrows: one nine-level canonical strength ladder; M1 is a Circle precision marker
- Final execution target: zero broker mutation in Indicator; cBot is the sole broker execution authority

## 2. Audit scope

1. Repository/project/build structure
2. Dependency direction and project boundaries
3. Core primitives and mathematical ownership
4. Market data, price, time and MTF semantics
5. Native and OSS indicators
6. Structure, swing, liquidity, FVG, OB, divergence, WaveTrend and volume profile
7. Regime and market intelligence
8. Decision, scoring, confidence, quality and gates
9. Entry, trigger, plan, SL, TP, RR and actionability
10. Scenario and execution-intent contracts
11. Signal visual state and chart rendering
12. Alert/event/queue/audio
13. Panel/UI/layout/responsiveness
14. cBot binding, preflight, execution and broker confirmation
15. Protection, lifecycle, recovery, risk and capacity
16. History, persistence, outcome and calibration
17. Performance, allocation, caching and hot-path behavior
18. Tests, static audits, CI, build and release reproducibility
19. Documentation/roadmap/continuity consistency
20. Target-terminal manual acceptance

## 3. Non-negotiable project law

**One concept → one owner → one source of truth → many read-only consumers.**

Never repair a duplicated consumer by adding a third implementation. Locate the canonical owner, correct it, then remove or redirect competing paths.

## 4. Canonical end-to-end flow

Market Data → Canonical Price/Time Context → MTF Closed Context → Primitive Indicators/OSS → Market/Structure/Zones/Liquidity/Reaction/WaveTrend/Divergence → Evidence Fusion/Independence → Regime/Top-Down Calibration → Decision → Entry/Trigger/Actionability → Trade Plan Entry/SL/TP/RR → Scenario/Intent/Contracts → SignalVisualSnapshot/Alert → Indicator→Contracts→cBot → Preflight/Risk/Capacity/Idempotency → Broker Mutation → Broker Confirmation → Protection/Management/Lifecycle/Recovery → Outcome/History/Calibration → Panel/Chart/Alert

## 5. Primary ownership map

| Domain | Canonical owner/family | Rule |
|---|---|---|
| Price/quote | CanonicalPriceSnapshot / CalculationMarketContext | Consumers do not reconstruct quote semantics |
| MTF | MtfClosedContext family | No competing timeframe clock |
| Primitive math | Core/Math + native cTrader boundary | No second formula engine |
| OSS math | Skender adapters + cache/window owners | No duplicate numerical path |
| Structure | Structure/Swing/StructuralEvent owners | No renderer-side structure logic |
| FVG | FvgRule + lifecycle owners | One geometry/lifecycle definition |
| OB | OB analyzer/quality/confluence owners | No duplicate detector |
| Decision | DecisionInput/Evidence + Decision calculators/gates | UI/execution never decides |
| Entry/SL/TP/RR | Planning + canonical math/validation | No consumer recomputation |
| Scenario | Scenario builders/materialization | cBot consumes, does not invent |
| Visual state | SignalVisualSnapshot | Chart/panel consume |
| Arrow strength | MtfTrendStrengthRule | Renderer does not recalculate |
| Chart line | PlanLineRenderer | Secondary surfaces delegate |
| Chart label | PlanLabelRenderer + formatter/anchor | No competing text renderer |
| Alert | AlertEngine + AlertDeliveryQueue/Processor | One event boundary |
| Cross-project data | CFIP.Contracts | Immutable/platform-neutral |
| Broker mutation | CFIP.cBot | Sole future broker mutation authority |
| Broker truth | Broker reconciliation/report/state | Desired state never masquerades as confirmed |
| Risk | Canonical risk/capacity/suitability owners | No duplicate safety gates |
| Persistence | Buffered persistence/archive owners | No synchronous hot-path I/O |
| Panel timeframe | PanelTimeframePresentationState | Lamp/text use same state |

## 6. Confirmed/evidenced repository findings

1. Historical documents contain different parameter baselines (562/563/568), while the current machine-enforced semantic audit and README use 548. Historical values must not be mixed with the current baseline.
2. M1 recorded partial/manual areas in exhaustive numeric invalid-input coverage, hidden-constant inventory, time/session/DST behavior, persistence/recovery, visual responsiveness, outcome attribution and cBot separation readiness.
3. M1 identified large ownership pressure in SignalVisualSnapshotBuilder.BuildSignalVisualSnapshot and TradeActionabilityEvaluator.EvaluateTradeActionability.
4. M1 identified mixed ownership risk around broker protection/live-management boundaries; final zero-Indicator-mutation cutover remains a blocking architectural gate.
5. Target-terminal validation remains required for panel freshness, chart glyph appearance, sound audibility, attachment/startup order, broker lifecycle, restart/reconnect and live execution behavior.
6. Smart arrow architecture is explicitly single-owner: MtfTrendStrengthRule → SignalVisualSnapshot → SignalStackedArrowRenderer; M1 is a Circle precision marker.
7. Canonical plan line/label ownership is enforced by the current single-owner audit.
8. Alert delivery is unified through the canonical event/queue/delivery boundary.
9. Panel timeframe lamp/text parity is enforced through PanelTimeframePresentationState.
10. The active execution architecture is three-project: Indicator, Contracts, cBot.
11. M15/M5/M1 roles are frozen as canonical decision/execution reference, trigger/precision, optional confirmation.
12. OB/FVG, WaveTrend, structure, divergence, regime and MTF remain analytical and must not be recreated in cBot.
13. Broker-confirmed state is authoritative after mutation.
14. Signal/plan/scenario/execution identities must remain distinct through the full chain.
15. The calculation-integrity program forbids threshold tuning as a substitute for correctness proof.

## 7. M2 current-main correction and newly confirmed findings

The first M2 branch was found to be 41 commits behind current `main` and contained a truncated `ROADMAP.md` (96 lines versus 3522 on current `main`). It is not a valid merge candidate. M2 was recreated from current `main` as `phase/M2-repository-hygiene-ownership-current`; the stale branch must not be merged.

New source-based findings requiring M2 disposition:

16. `ManagementCommandRequestCoordinator.RequestManagementCommand` returns a single `bool` that conflates invalid/rejected, queued, and confirmed semantics. A queued request can therefore be interpreted as a rejection by one caller, while changing the bool to mean acceptance would make other callers treat queued state as broker-confirmed. A temporary boolean change was intentionally reverted. The root fix requires one explicit Request/Confirmed/Rejected status contract with one owner.
17. The management-command publication path performs synchronous `LocalStorage.SetString + Flush`; this is a confirmed hot-path persistence/latency risk.
18. `ProcessManagementReports` synchronously reads LocalStorage from the management request path, coupling command/reconciliation behavior to storage latency.
19. Indicator management helper names such as `TryClosePosition` and `TryModifyStopLoss` still imply direct broker mutation although the current implementation is command publication. This is a semantic maintenance hazard and should be normalized at the canonical owner.
20. Historical execution-boundary documents still mention removed Indicator broker owner files such as `BrokerMarketOrderMutation.cs` and `BrokerLimitOrderPlacement.cs`; this is documentation/audit drift, not evidence that those production files currently exist.
21. No parallel management bus may be introduced to solve the submission-state problem; the status contract must remain on the existing canonical management command path.

## 8. Complete repository structure — machine inventory

The following list is generated from the recursive Git tree of the audited main commit.

### .editorconfig

- .editorconfig

### .github

- .github/workflows/ci-build.yml
- .github/workflows/oss-benchmark.yml
- .github/workflows/runtime-acceptance.yml
- .github/workflows/source-check.yml

### .gitignore

- .gitignore

### .vscode

- .vscode/extensions.json

### CFIP.Indicator.sln

- CFIP.Indicator.sln

### Directory.Build.props

- Directory.Build.props

### README.md

- README.md

### ROUTINE.md

- ROUTINE.md

### docs

- docs/ACCEPTANCE-MATRIX.md
- docs/ARCHITECTURE.md
- docs/CBOT-0-BOUNDARY-INVENTORY.md
- docs/CBOT-P0-EXECUTION-DEPENDENCY-CLOSURE.md
- docs/CBOT-PREFLIGHT.md
- docs/CBOT-SEPARATION-ROADMAP.md
- docs/CLAUDE-REVIEW-CR0-AUDIT.md
- docs/CLAUDE-REVIEW-CR1.7.md
- docs/CLAUDE-REVIEW-CR1.8.md
- docs/CLAUDE-REVIEW-CR3.2-DECISION-PREDICTION.md
- docs/CLAUDE-REVIEW-CR3.3-PARTIAL-TP-BE-TRAILING.md
- docs/CLAUDE-REVIEW-CR3.5-CALIBRATION-OUTCOME-REJECTION.md
- docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md
- docs/CONTINUATION-STATE.md
- docs/CR-FINAL-2026-09-30.md
- docs/DEEP-AUDIT-2026-09-29.md
- docs/DEVELOPMENT-LOG.md
- docs/EDITING-GUIDE.md
- docs/ENGINEERING-PRINCIPLES.md
- docs/HOTFIX-CHART-LINES-EXECUTION-STATUS-2026-09-29.md
- docs/HOTFIX-EXECUTION-PRIORITY-STRUCTURAL-LOCK.md
- docs/HOTFIX-PANEL-LABELS-SMART-PROTECTION-2026-09-29.md
- docs/LOCAL-RELEASE-GATE.md
- docs/MARKET-REGIME-SIGNAL-SYNCHRONIZATION.md
- docs/OPERATOR-MAINTENANCE-GUIDE.md
- docs/OSS-COMPONENT-REGISTER.md
- docs/PARAMETER-INVENTORY.md
- docs/PERFORMANCE-ARCHITECTURE-2026-09-29.md
- docs/PHASE-10-MTF-SCENARIOS-ROUTINE-LOGGING.md
- docs/PHASE-11-1-AUTO-EXECUTION-HISTORY-PRESENTATION-HARDENING.md
- docs/PHASE-11-2-ANALYSIS-SIGNAL-EXECUTION-HEARTBEAT.md
- docs/PHASE-11-3-EXECUTION-FORENSICS-THRESHOLD-EVIDENCE.md
- docs/PHASE-11-4-PLAN-QUALITY-MULTISCENARIO-AUTO-EXECUTION.md
- docs/PHASE-11-5-SCENARIO-EXECUTION-MATERIALIZATION.md
- docs/PHASE-11-NEWS-FVG-CALIBRATION.md
- docs/PHASE-5-6-RESPONSIVE-PANEL.md
- docs/PHASE-6-1-DECISION-CLOSED-BAR.md
- docs/PHASE-6-2-REACTION-INTRABAR.md
- docs/PHASE-6-3-AGGRESSIVE-ENTRY.md
- docs/PHASE-6-4-COMPACT-PLAN-VISUALS.md
- docs/PHASE-7-1-HIDDEN-CLAMP-AUDIT.md
- docs/PHASE-7-2-DEAD-PARAMETER-AUDIT.md
- docs/PHASE-7-3-SEMANTIC-DUPLICATE-AUDIT.md
- docs/PHASE-7-4-MAXIMUM-OPEN-POSITIONS-SEMANTICS.md
- docs/PHASE-8-1-M1-TRIGGER-CORRECTNESS.md
- docs/PHASE-8-2-SWING-PLATEAU-CORRECTNESS.md
- docs/PHASE-8-3-FVG-MATHEMATICAL-AUDIT.md
- docs/PHASE-8-4-ORDER-BLOCK-MATHEMATICAL-AUDIT.md
- docs/PHASE-8-5-ZONE-CONFLUENCE-TRIGGER-SYNCHRONIZATION.md
- docs/PHASE-9-1-TOPDOWN-EVIDENCE-RUNTIME-RESPONSIVENESS.md
- docs/PHASE-9-10-SMART-AUTO-TRADE-PROTECTION-AUDIT.md
- docs/PHASE-9-11-SIGNAL-LIFECYCLE-QUALITY-ALERTS.md
- docs/PHASE-9-12-OUTCOME-RECOVERY-TELEMETRY-CALIBRATION.md
- docs/PHASE-9-13-PERSISTENT-MEMORY-SAFE-OPTIMIZATION.md
- docs/PHASE-9-14-SIGNAL-EVIDENCE-CALIBRATION.md
- docs/PHASE-9-15-STARTUP-PERSISTENT-HISTORY.md
- docs/PHASE-9-16-SIGNAL-MEASUREMENT-LOCATION-FUSION.md
- docs/PHASE-9-17-EXIT-GEOMETRY-PROGRESSION.md
- docs/PHASE-9-2-PARALLEL-OPPORTUNITIES-WAVETREND-VISUAL-LANES.md
- docs/PHASE-9-3-EMPIRICAL-CALIBRATION.md
- docs/PHASE-9-4-ACTIONABILITY-DIVERGENCE-PLAN-REGISTRY.md
- docs/PHASE-9-6-SIGNAL-QUALITY-VISUAL-COHERENCE.md
- docs/PHASE-9-7-REGIME-AUTO-EXECUTION-HARDENING.md
- docs/PHASE-9-8-INDICATOR-FUSION-TRADE-QUALITY.md
- docs/PHASE-9-9-SIGNAL-PROTECTION-COHERENCE.md
- docs/PHASE-BUILD-WARNING-PANEL-HEIGHT-INTEGRITY-2026-10-02.md
- docs/PHASE-CBOT-6M-TRADE-QUALITY-HARDENING-2026-10-02.md
- docs/PHASE-CBOT-ATTACHMENT-AUDIO-HARDENING-2026-10-02.md
- docs/PHASE-CBOT-BROKER-CONFIRMED-FACTS-2026-10-03.md
- docs/PHASE-CBOT-DEMO-LIVE-MARKET-2026-10-02.md
- docs/PHASE-CBOT-EFFECTIVE-LIFECYCLE-STATE-2026-10-03.md
- docs/PHASE-CBOT-LIFECYCLE-AUDIO-PANEL-HEADER-2026-10-03.md
- docs/PHASE-CBOT-LOCAL-CLOUD-LIFECYCLE-2026-10-03.md
- docs/PHASE-CBOT-MANAGEMENT-POLICY-HARDENING-2026-10-03.md
- docs/PHASE-CBOT-P0-ACTIVATION.md
- docs/PHASE-CBOT-P1-PLATFORM-NEUTRAL-CONTRACTS.md
- docs/PHASE-CBOT-P2-READ-ONLY-INDICATOR-PROVIDER.md
- docs/PHASE-CBOT-P3-CBOT-HOST-SHADOW.md
- docs/PHASE-CBOT-P4A-MARKET-RANGE-AUTHORITY-CUTOVER-2026-10-02.md
- docs/PHASE-CBOT-P4B-AGGRESSIVE-PANEL-GEOMETRY-2026-10-02.md
- docs/PHASE-CBOT-P4C-PENDING-STOP-HOST-INDEPENDENCE-2026-10-02.md
- docs/PHASE-CBOT-P4D-PENDING-LIMIT-SIGNAL-POPUP-2026-10-02.md
- docs/PHASE-CBOT-P4E-MANAGEMENT-AUTHORITY-2026-10-02.md
- docs/PHASE-CBOT-P5-RECONCILIATION-PROTECTION-2026-10-02.md
- docs/PHASE-CBOT-P6-ACCOUNT-RISK-CONNECTION-2026-10-02.md
- docs/PHASE-CBOT-P7-UI-STATE-CUTOVER-2026-10-02.md
- docs/PHASE-CBOT-P7R-ATTACHMENT-ALERT-VISIBILITY-2026-10-02.md
- docs/PHASE-CBOT-P8-PROGRESSIVE-PROTECTION-STATE-SYNC-2026-10-02.md
- docs/PHASE-CBOT-P9-UNIFIED-ALERT-RAIL-VISUAL-COHERENCE-2026-10-02.md
- docs/PHASE-CBOT-POSITION-TRUTH-HARDENING-2026-10-03.md
- docs/PHASE-CBOT-SHADOW-MULTISCENARIO-TRUTH-2026-10-03.md
- docs/PHASE-CI-00-CANONICAL-DATA-PRICE-TIME.md
- docs/PHASE-CI-01-PRIMITIVE-INDICATOR-INTEGRITY.md
- docs/PHASE-CI-02-OSS-PARITY-WARMUP-CACHE.md
- docs/PHASE-CI-03-INDICATOR-FUSION-EVIDENCE-INDEPENDENCE.md
- docs/PHASE-CI-04-STRUCTURE-SWING-LIQUIDITY.md
- docs/PHASE-CI-05-FVG-LIFECYCLE.md
- docs/PHASE-CI-06-ORDER-BLOCK-LIFECYCLE.md
- docs/PHASE-CI-07-MTF-REGIME-CONTEXT.md
- docs/PHASE-CI-08-DIVERGENCE-WAVETREND-REACTION-EARLY.md
- docs/PHASE-CI-09-DECISION-MATHEMATICS.md
- docs/PHASE-CI-10-TRIGGER-LIFECYCLE.md
- docs/PHASE-CI-11-ENTRY-GEOMETRY-TIMING.md
- docs/PHASE-CI-12-STRUCTURAL-SL.md
- docs/PHASE-CI-13-TP-SOURCE-OBSTACLE-LADDER.md
- docs/PHASE-CI-14-CANONICAL-RR-PROTECTION.md
- docs/PHASE-CI-15-EXECUTION-GEOMETRY-BROKER-BOUNDARY.md
- docs/PHASE-CI-16-DETERMINISTIC-REPLAY-LATENCY.md
- docs/PHASE-CI-17-TARGET-TERMINAL-VALIDATION.md
- docs/PHASE-CI-17A-PANEL-LIVE-CONTENT-REFRESH.md
- docs/PHASE-CI-18-SIGNAL-PANEL-COHERENCE-2026-10-02.md
- docs/PHASE-CI-19-SIGNAL-TARGET-QUALITY-COHERENCE-2026-10-02.md
- docs/PHASE-CI-20-PANEL-CBOT-ENGINE-COHERENCE-2026-10-02.md
- docs/PHASE-CI-20B-PROTECTION-CBOT-SIGNAL-HARDENING-2026-10-02.md
- docs/PHASE-CI-20C-CBOT-CONNECTION-LIFECYCLE-2026-10-02.md
- docs/PHASE-CI-21-PRIMARY-M15-SIGNAL-VISIBILITY.md
- docs/PHASE-CI-FULL-STACK-CALCULATION-ANALYTICAL-INTEGRITY.md
- docs/PHASE-CR1-3-NEWS-GUARD-CORRECTNESS.md
- docs/PHASE-CR1-9-MINOR-CLEANUP.md
- docs/PHASE-CR2-1-STRUCTURE-SEMANTICS.md
- docs/PHASE-CR2-2-REACTION-INTEGRITY.md
- docs/PHASE-CR2-3-INDICATOR-THRESHOLDS.md
- docs/PHASE-CR2-4-PENDING-ARBITER.md
- docs/PHASE-CR2-5-LIFECYCLE-OUTCOME.md
- docs/PHASE-CR2-6-ORDERBLOCK-QUALITY-CACHE.md
- docs/PHASE-CR2-7-WAVETREND-MATHEMATICAL-CORRECTNESS.md
- docs/PHASE-CR2-8-HISTORICAL-RENDERING.md
- docs/PHASE-CR2-9-STRUCTURAL-DIVERGENCE-REJECTION.md
- docs/PHASE-CR3-1-LIVE-INVALIDATION.md
- docs/PHASE-CR4-1-MEMORY-IDENTITY.md
- docs/PHASE-CR4-10-NATIVE-INDICATOR-SAFETY.md
- docs/PHASE-CR4-2-PERSISTENCE.md
- docs/PHASE-CR4-4-OSS-NUMERICAL-CACHING.md
- docs/PHASE-CR4-5-PER-TIMEFRAME-REGIME.md
- docs/PHASE-CR4-6-FRAME-SCORING-CONSTANTS.md
- docs/PHASE-CR4-7-TP-PIPELINE.md
- docs/PHASE-CR4-8-TP1-DIRECTIONAL-DEFENCE.md
- docs/PHASE-CR4-9-LIVE-REVERSAL-SEMANTICS.md
- docs/PHASE-CR5-1-STRUCTURAL-STOP-CEILING.md
- docs/PHASE-CR5-2-LIQUIDITY-TARGET-SOURCES.md
- docs/PHASE-CR5-3-INDEPENDENT-EVIDENCE-GROUPS.md
- docs/PHASE-CR5-4-PENDING-FILL-ABSOLUTE-RECONCILIATION.md
- docs/PHASE-CR5-5-PARALLEL-SCENARIO-MICROREACTION.md
- docs/PHASE-CR5-6-DIRECTIONAL-BIAS-TIMEFRAME.md
- docs/PHASE-CR5-7-WATCH-REACTION-ALERTS.md
- docs/PHASE-CR5-8-TARGET-SELECTION-CONSISTENCY.md
- docs/PHASE-CR6-1-F1-OPPOSING-ZONE-TARGET-PATH.md
- docs/PHASE-CR6-2-F2-AGGRESSIVE-RISK-FILL.md
- docs/PHASE-CR6-3-F4-THRESHOLD-TRANSPARENCY.md
- docs/PHASE-CR6-4-SMART-THRESHOLD-REGIME.md
- docs/PHASE-CR6-5-F6-TRAP-ACTIONABILITY.md
- docs/PHASE-CR6-6-F7-TIMEFRAME-SCENARIO-SEMANTICS.md
- docs/PHASE-CR6-7-F8-TARGET-OBSTACLE-TELEMETRY.md
- docs/PHASE-CR6-8-F9-TARGET-OBSTACLE-CACHE.md
- docs/PHASE-CR6-9-F3-ORPHAN-MANAGED-POSITION-PROTECTION.md
- docs/PHASE-CR7-1-G1-BROKER-PROTECTION-NO-RISK-EXPANSION.md
- docs/PHASE-CR7-1-G1-BROKER-PROTECTION-RISK-NON-EXPANSION.md
- docs/PHASE-CR7-2-G2-RETEST-ADVERSE-MOMENTUM.md
- docs/PHASE-CR7-3-G3-PLAN-LINE-THICKNESS.md
- docs/PHASE-CR7-4-G4-PANEL-EXECUTION-PROTECTION-STATE.md
- docs/PHASE-CR7-5-G5-PANEL-STATE-FRESHNESS.md
- docs/PHASE-CR7-6A-G6A-EXECUTION-PANEL-PRESENTATION-FRESHNESS.md
- docs/PHASE-CR7-6B-G6B-EXECUTION-CONTROL-TRUTH.md
- docs/PHASE-CR8-1-H1-DIRECTIONAL-FILL-ACCEPTANCE.md
- docs/PHASE-CR8-2-H2-TOPDOWN-ABSOLUTE-STRENGTH.md
- docs/PHASE-CR8-3A-H3-A-SKENDER-SETTINGS.md
- docs/PHASE-CR8-3B-H3-B-SKENDER-WARMUP-CACHE-PARITY.md
- docs/PHASE-FINAL-REALTIME-LIVE-SMART-SYSTEM-2026-10-03.md
- docs/PHASE-FOOTER-ALERT-POPUP-HARDENING-2026-10-03.md
- docs/PHASE-INDICATOR-NAME-CBOT-LAUNCH-MTF-PANEL.md
- docs/PHASE-M1-FULL-FORENSIC-AUDIT.md
- docs/PHASE-M3-TRADE-TRUTH-ALERT-CHART-COHERENCE-2026-10-02.md
- docs/PHASE-M4-TIME-SESSION-HISTORY-PERSISTENCE-2026-10-02.md
- docs/PHASE-M5-PANEL-LIVE-RESPONSIVENESS-2026-10-02.md
- docs/PHASE-MTF-EXECUTION-M15-RISK-SPREAD-2026-10-02.md
- docs/PHASE-MTF-P1-PRIMARY-M15-H1-PANEL.md
- docs/PHASE-MTF-P2-PRIMARY-LOCATION-OBFVG.md
- docs/PHASE-MTF-P3-PRIMARY-PROVIDER-IDENTITY.md
- docs/PHASE-OPPORTUNITY-MINING-ZONE-SELECTION-2026-10-02.md
- docs/PHASE-PANEL-CLEARANCE-RESTORE-POSITION.md
- docs/PHASE-PANEL-FOOTER-ALERT-DEDUP-2026-10-03.md
- docs/PHASE-PANEL-TIMEFRAME-SINGLE-SOURCE-2026-10-03.md
- docs/PHASE-POSITION-ENGINE-CBOT-TRUTH-HARDENING-2026-10-02.md
- docs/PHASE-REALTIME-LIVE-SIGNAL-UNIFICATION-2026-10-03.md
- docs/PHASE-REALTIME-MULTISCENARIO-OPPORTUNITY-ENGINE-2026-10-03.md
- docs/PHASE-RETEST-TRIGGER-PATH-HARDENING-2026-10-03.md
- docs/PHASE-SEMANTIC-CONSISTENCY-HARDENING-2026-10-03.md
- docs/PHASE-SIGNAL-DRAWING-CANONICAL-2026-10-03.md
- docs/PHASE-SMART-SEPARATED-SIGNAL-ARROWS-2026-10-04.md
- docs/PHASE-VOLUME-PROFILE-EVIDENCE-2026-10-03.md
- docs/REFERENCE-COVERAGE.md
- docs/REFERENCE-INDICATOR-AUDIT-2026-09-29.md
- docs/ROADMAP.md
- docs/RUNTIME-LOG-GUIDE.md
- docs/RUNTIME-STARTUP-OBSERVABILITY-HOTFIX.md
- docs/SMART-INTELLIGENCE.md
- docs/TRACK-19-OSS-NUMERICAL-BENCHMARK.md
- docs/TRADING-SAFETY-MATRIX.md
- docs/USER-PRIORITY-PLAN.md
- docs/WORKFLOW.md

### global.json

- global.json

### oss

- oss/README.md

### preflight

- preflight/CFIP.Preflight.Probe.csproj
- preflight/CFIP.Preflight.csproj
- preflight/CFIPPreflightBot.cs
- preflight/CFIPPreflightProbeIndicator.cs

### src

- src/CFIP.Contracts/AlertEnvelope.cs
- src/CFIP.Contracts/BrokerExecutionReport.cs
- src/CFIP.Contracts/CFIP.Contracts.csproj
- src/CFIP.Contracts/CbotExecutionStateBus.cs
- src/CFIP.Contracts/CbotExecutionStateSnapshot.cs
- src/CFIP.Contracts/CbotIdentity.cs
- src/CFIP.Contracts/ContractBusKeyHash.cs
- src/CFIP.Contracts/ContractEnums.cs
- src/CFIP.Contracts/ContractVersion.cs
- src/CFIP.Contracts/ExecutionIntent.cs
- src/CFIP.Contracts/IdentityContracts.cs
- src/CFIP.Contracts/IndicatorIdentity.cs
- src/CFIP.Contracts/LifecycleEvent.cs
- src/CFIP.Contracts/ManagementBusKey.cs
- src/CFIP.Contracts/ManagementCommand.cs
- src/CFIP.Contracts/MarketExecutionProfile.cs
- src/CFIP.Contracts/PlanSnapshot.cs
- src/CFIP.Contracts/ScenarioExecutionIdentityRule.cs
- src/CFIP.Contracts/SignalBusKey.cs
- src/CFIP.Contracts/SignalEnvelope.cs
- src/CFIP.Contracts/SignalEnvelopeCodec.cs
- src/CFIP.Contracts/SignalScenarioBatch.cs
- src/CFIP.Contracts/SignalScenarioBatchCodec.cs
- src/CFIP.Indicator/Analysis/Indicators/AverageDirectionalIndex.cs
- src/CFIP.Indicator/Analysis/Indicators/AverageTrueRange.cs
- src/CFIP.Indicator/Analysis/Indicators/DirectionalMovementIndex.cs
- src/CFIP.Indicator/Analysis/Indicators/ExponentialMovingAverage.cs
- src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorConfluenceAnalyzer.cs
- src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorParameters.cs
- src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorSnapshotCache.cs
- src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteCacheEntry.cs
- src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteSeriesCache.cs
- src/CFIP.Indicator/Analysis/Indicators/External/SkenderAroon.cs
- src/CFIP.Indicator/Analysis/Indicators/External/SkenderBollingerBands.cs
- src/CFIP.Indicator/Analysis/Indicators/External/SkenderCci.cs
- src/CFIP.Indicator/Analysis/Indicators/External/SkenderMacd.cs
- src/CFIP.Indicator/Analysis/Indicators/External/SkenderMfi.cs
- src/CFIP.Indicator/Analysis/Indicators/External/SkenderObv.cs
- src/CFIP.Indicator/Analysis/Indicators/External/SkenderParabolicSar.cs
- src/CFIP.Indicator/Analysis/Indicators/External/SkenderRsi.cs
- src/CFIP.Indicator/Analysis/Indicators/External/SkenderStoch.cs
- src/CFIP.Indicator/Analysis/Indicators/External/SkenderSuperTrend.cs
- src/CFIP.Indicator/Analysis/Indicators/MacdIndicator.cs
- src/CFIP.Indicator/Analysis/Indicators/Native/Native.cs
- src/CFIP.Indicator/Analysis/Indicators/NativeIndicatorRegistry.cs
- src/CFIP.Indicator/Analysis/Indicators/RelativeStrengthIndex.cs
- src/CFIP.Indicator/Analysis/Market/ChoppinessIndexAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/Decision/ConfidenceCalibrationCollector.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionConfidenceCalculator.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionConfirmationGates.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionConsensusCalculator.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionConsensusSnapshot.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvaluator.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvidenceSnapshot.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionFilterResult.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionFilters.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionFrameContribution.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionFrameContributionCalculator.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputBuildRequest.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshot.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionLifecycleGates.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionMarketGates.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionQualityCalculator.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionReasonBuilder.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionReasonFormatter.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionRestrictionAlertPolicy.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreCalculator.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreInput.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreSnapshot.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartConsensusFilterEvaluator.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartConsensusFilterInput.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartGates.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionStructureGates.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionTacticalOpportunityAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionThresholdFilterEvaluator.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DecisionThresholdFilterInput.cs
- src/CFIP.Indicator/Analysis/Market/Decision/DirectionAcceptanceGate.cs
- src/CFIP.Indicator/Analysis/Market/Decision/EmpiricalConfidenceCalibrator.cs
- src/CFIP.Indicator/Analysis/Market/Decision/FrameDecisionContributionAdapter.cs
- src/CFIP.Indicator/Analysis/Market/Decision/HigherTimeframePenaltyCalculator.cs
- src/CFIP.Indicator/Analysis/Market/Decision/IndependentEvidenceAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/Decision/StructuralConfirmationAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/Decision/TimeframeAgreementAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/Decision/TopDownCalibrationAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/DivergenceAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/FuturePendingOpportunityRuntime.cs
- src/CFIP.Indicator/Analysis/Market/HealthyVolatilityAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/LiveBiasAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/M5RegimeCoreCache.cs
- src/CFIP.Indicator/Analysis/Market/MacdBiasAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/MarketFrameAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/MarketFrameEvidence.cs
- src/CFIP.Indicator/Analysis/Market/MarketFrameScoring.cs
- src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs
- src/CFIP.Indicator/Analysis/Market/MarketRegimeAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/MarketRegimeBarFingerprint.cs
- src/CFIP.Indicator/Analysis/Market/MarketRegimeFrameCache.cs
- src/CFIP.Indicator/Analysis/Market/MarketRegimeFrameCacheEntry.cs
- src/CFIP.Indicator/Analysis/Market/MarketStateSnapshotBuilder.cs
- src/CFIP.Indicator/Analysis/Market/Math/IndexMath.cs
- src/CFIP.Indicator/Analysis/Market/Models/Frame.cs
- src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs
- src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs
- src/CFIP.Indicator/Analysis/Market/ParallelScenarioComputation.cs
- src/CFIP.Indicator/Analysis/Market/PremiumDiscountAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/RangeEfficiencyAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/RangeSignalQualityEvaluator.cs
- src/CFIP.Indicator/Analysis/Market/ScenarioEvidenceEnrichment.cs
- src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs
- src/CFIP.Indicator/Analysis/Market/VolumeExpansionAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/VolumeProfileAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/VwapBiasAnalyzer.cs
- src/CFIP.Indicator/Analysis/Market/WaveTrendEngine.cs
- src/CFIP.Indicator/Analysis/Market/WaveTrendEvidenceAnalyzer.cs
- src/CFIP.Indicator/Analysis/Reaction/ReactionAnalyzer.cs
- src/CFIP.Indicator/Analysis/Structure/EqualLevelAnalyzer.cs
- src/CFIP.Indicator/Analysis/Structure/LiquiditySweepAnalyzer.cs
- src/CFIP.Indicator/Analysis/Structure/StructureAnalyzer.cs
- src/CFIP.Indicator/Analysis/Structure/SwingPointAnalyzer.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/FvgDetectionAnalyzer.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/FvgLifecycleAnalyzer.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/FvgMitigationEvaluator.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/FvgZoneQualityCalculator.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockAnalyzer.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockCandidateBuilder.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockConfluenceAnalyzer.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockEvidenceBuilder.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockMitigationGuard.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockQualityCalculator.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/ZoneLookup.cs
- src/CFIP.Indicator/Analysis/Structure/Zones/ZoneLookupHotCache.cs
- src/CFIP.Indicator/CFIP.Indicator.csproj
- src/CFIP.Indicator/Core/Enums/DecisionPolicyMode.cs
- src/CFIP.Indicator/Core/Enums/ExecutionIntentKind.cs
- src/CFIP.Indicator/Core/Enums/ExecutionMode.cs
- src/CFIP.Indicator/Core/Enums/ExecutionSubmissionPath.cs
- src/CFIP.Indicator/Core/Enums/LifecycleState.cs
- src/CFIP.Indicator/Core/Enums/OpportunityLane.cs
- src/CFIP.Indicator/Core/Enums/PanelCorner.cs
- src/CFIP.Indicator/Core/Enums/PendingOrderMode.cs
- src/CFIP.Indicator/Core/Enums/SizingMode.cs
- src/CFIP.Indicator/Core/Enums/TargetStage.cs
- src/CFIP.Indicator/Core/Execution/AggressiveEntryPolicy.cs
- src/CFIP.Indicator/Core/Execution/SubmissionAttemptIdentity.cs
- src/CFIP.Indicator/Core/Execution/SubmissionGate.cs
- src/CFIP.Indicator/Core/Execution/SubmissionGateState.cs
- src/CFIP.Indicator/Core/GlobalUsings.cs
- src/CFIP.Indicator/Core/Math/ActionabilityThresholdPolicy.cs
- src/CFIP.Indicator/Core/Math/ActionableSignalQualityRule.cs
- src/CFIP.Indicator/Core/Math/BrokerStateRefreshRule.cs
- src/CFIP.Indicator/Core/Math/CalculationReadinessRule.cs
- src/CFIP.Indicator/Core/Math/CanonicalTimeRule.cs
- src/CFIP.Indicator/Core/Math/ChoppinessIndexRule.cs
- src/CFIP.Indicator/Core/Math/ClosedBarReferenceRule.cs
- src/CFIP.Indicator/Core/Math/DailyLossBaselineRule.cs
- src/CFIP.Indicator/Core/Math/DailyLossEvaluation.cs
- src/CFIP.Indicator/Core/Math/DailyLossRule.cs
- src/CFIP.Indicator/Core/Math/DivergenceThresholdRule.cs
- src/CFIP.Indicator/Core/Math/DmiBiasRule.cs
- src/CFIP.Indicator/Core/Math/EarlyPredictionScoreRule.cs
- src/CFIP.Indicator/Core/Math/EconomicNewsCurrencyRule.cs
- src/CFIP.Indicator/Core/Math/EconomicNewsFeedStateRule.cs
- src/CFIP.Indicator/Core/Math/EntryActionabilityPolicy.cs
- src/CFIP.Indicator/Core/Math/EntryGeometryRule.cs
- src/CFIP.Indicator/Core/Math/EntrySignalTimingRule.cs
- src/CFIP.Indicator/Core/Math/EntryTrapRiskPolicy.cs
- src/CFIP.Indicator/Core/Math/EntryTrapRiskRule.cs
- src/CFIP.Indicator/Core/Math/ExecutionCapacityRule.cs
- src/CFIP.Indicator/Core/Math/ExecutionControlPresentationRule.cs
- src/CFIP.Indicator/Core/Math/ExecutionFillAcceptanceRule.cs
- src/CFIP.Indicator/Core/Math/ExecutionIntentGeometryRule.cs
- src/CFIP.Indicator/Core/Math/ExecutionPanelPresentationIdentityRule.cs
- src/CFIP.Indicator/Core/Math/ExecutionPlanGeometryRule.cs
- src/CFIP.Indicator/Core/Math/ExecutionProtectionPanelStateRule.cs
- src/CFIP.Indicator/Core/Math/ExecutionThresholdPolicy.cs
- src/CFIP.Indicator/Core/Math/ExecutionTimeframePolicy.cs
- src/CFIP.Indicator/Core/Math/ExecutionZoneSelectionRule.cs
- src/CFIP.Indicator/Core/Math/FalseSignalAdverseRRule.cs
- src/CFIP.Indicator/Core/Math/FrameRegimeResolutionRule.cs
- src/CFIP.Indicator/Core/Math/FrameScoringConstants.cs
- src/CFIP.Indicator/Core/Math/FvgLifecycleRule.cs
- src/CFIP.Indicator/Core/Math/FvgQualityRule.cs
- src/CFIP.Indicator/Core/Math/FvgRule.cs
- src/CFIP.Indicator/Core/Math/HealthyVolatilityRule.cs
- src/CFIP.Indicator/Core/Math/HistoricalOutcomeAggregationRule.cs
- src/CFIP.Indicator/Core/Math/HistoricalRenderingRule.cs
- src/CFIP.Indicator/Core/Math/IndependentEvidenceDiversityRule.cs
- src/CFIP.Indicator/Core/Math/IndependentEvidenceFusionRule.cs
- src/CFIP.Indicator/Core/Math/IndicatorActionabilityRule.cs
- src/CFIP.Indicator/Core/Math/IndicatorEvidenceFusionRule.cs
- src/CFIP.Indicator/Core/Math/IndicatorEvidenceIndependenceRule.cs
- src/CFIP.Indicator/Core/Math/IndicatorExecutionQualityRule.cs
- src/CFIP.Indicator/Core/Math/IntelligentProtectionRule.cs
- src/CFIP.Indicator/Core/Math/LiquiditySweepRule.cs
- src/CFIP.Indicator/Core/Math/LiquidityTargetCandidateRule.cs
- src/CFIP.Indicator/Core/Math/LiveExitGeometryRule.cs
- src/CFIP.Indicator/Core/Math/LiveInvalidationRule.cs
- src/CFIP.Indicator/Core/Math/LiveM5BiasRule.cs
- src/CFIP.Indicator/Core/Math/LivePlanRecoveryRule.cs
- src/CFIP.Indicator/Core/Math/LiveReversalDecisionRule.cs
- src/CFIP.Indicator/Core/Math/LiveReversalEpisodeRule.cs
- src/CFIP.Indicator/Core/Math/LocationEvidenceRule.cs
- src/CFIP.Indicator/Core/Math/M1TriggerRule.cs
- src/CFIP.Indicator/Core/Math/MacdBiasRule.cs
- src/CFIP.Indicator/Core/Math/ManagedIdentityRule.cs
- src/CFIP.Indicator/Core/Math/ManagedStopProtectionRule.cs
- src/CFIP.Indicator/Core/Math/MarketRegimeClassifier.cs
- src/CFIP.Indicator/Core/Math/MarketRegimeIdentity.cs
- src/CFIP.Indicator/Core/Math/MarketRegimeTransitionRule.cs
- src/CFIP.Indicator/Core/Math/MicroReactionSafetyRule.cs
- src/CFIP.Indicator/Core/Math/MtfEarlyPredictionFusionRule.cs
- src/CFIP.Indicator/Core/Math/MtfTrendStrengthRule.cs
- src/CFIP.Indicator/Core/Math/NativeIndicatorReadinessRule.cs
- src/CFIP.Indicator/Core/Math/NumericGuards.cs
- src/CFIP.Indicator/Core/Math/OpportunityMagnitudeRule.cs
- src/CFIP.Indicator/Core/Math/OrderBlockLifecycleRule.cs
- src/CFIP.Indicator/Core/Math/OrderBlockQualityRule.cs
- src/CFIP.Indicator/Core/Math/OrderBlockRule.cs
- src/CFIP.Indicator/Core/Math/OrphanManagedProtectionRule.cs
- src/CFIP.Indicator/Core/Math/OssIndicatorSettings.cs
- src/CFIP.Indicator/Core/Math/OssIndicatorWarmupPolicy.cs
- src/CFIP.Indicator/Core/Math/OssQuoteProjectionRule.cs
- src/CFIP.Indicator/Core/Math/OssQuoteWindowRule.cs
- src/CFIP.Indicator/Core/Math/OutcomeMemoryIdentityRule.cs
- src/CFIP.Indicator/Core/Math/PanelDimensionRule.cs
- src/CFIP.Indicator/Core/Math/PanelFrameDirectionRule.cs
- src/CFIP.Indicator/Core/Math/ParallelScenarioGeometry.cs
- src/CFIP.Indicator/Core/Math/ParallelScenarioSelectionRule.cs
- src/CFIP.Indicator/Core/Math/PartialTakeProfitRetryRule.cs
- src/CFIP.Indicator/Core/Math/PeakPriceReconstructionRule.cs
- src/CFIP.Indicator/Core/Math/PendingDecisionArbiterRule.cs
- src/CFIP.Indicator/Core/Math/PendingEntryPriceRule.cs
- src/CFIP.Indicator/Core/Math/PendingFillExitResolutionRule.cs
- src/CFIP.Indicator/Core/Math/PersistenceHealthRule.cs
- src/CFIP.Indicator/Core/Math/PlanLinePresentationRule.cs
- src/CFIP.Indicator/Core/Math/PlanRewardRiskQualityRule.cs
- src/CFIP.Indicator/Core/Math/PremiumDiscountBiasRule.cs
- src/CFIP.Indicator/Core/Math/PriceProtectionRule.cs
- src/CFIP.Indicator/Core/Math/PrimaryPullbackTuningRule.cs
- src/CFIP.Indicator/Core/Math/PrimaryTimeframeSignalRule.cs
- src/CFIP.Indicator/Core/Math/ProtectionProgressionRule.cs
- src/CFIP.Indicator/Core/Math/ProviderScenarioIdentityRule.cs
- src/CFIP.Indicator/Core/Math/RangeEfficiencyRule.cs
- src/CFIP.Indicator/Core/Math/RangeSignalQualityRule.cs
- src/CFIP.Indicator/Core/Math/ReactionQualificationRule.cs
- src/CFIP.Indicator/Core/Math/ReactionTimingRule.cs
- src/CFIP.Indicator/Core/Math/RegimeAdaptiveRewardFloorRule.cs
- src/CFIP.Indicator/Core/Math/RejectionRule.cs
- src/CFIP.Indicator/Core/Math/ReversalProfitThresholdRule.cs
- src/CFIP.Indicator/Core/Math/RewardPathGeometryRule.cs
- src/CFIP.Indicator/Core/Math/RewardQualityFloorRule.cs
- src/CFIP.Indicator/Core/Math/RiskRewardMathRule.cs
- src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs
- src/CFIP.Indicator/Core/Math/ServerPartialTakeProfitEvidenceRule.cs
- src/CFIP.Indicator/Core/Math/SessionWindowRule.cs
- src/CFIP.Indicator/Core/Math/SignalTraceIdentityRule.cs
- src/CFIP.Indicator/Core/Math/SignalTraceLineageRule.cs
- src/CFIP.Indicator/Core/Math/SignalVisualLifecycleRule.cs
- src/CFIP.Indicator/Core/Math/SmartBreakEvenRule.cs
- src/CFIP.Indicator/Core/Math/SmartThresholdPolicyRule.cs
- src/CFIP.Indicator/Core/Math/StructuralEventRule.cs
- src/CFIP.Indicator/Core/Math/StructuralEvidenceRule.cs
- src/CFIP.Indicator/Core/Math/StructuralStopGeometryRule.cs
- src/CFIP.Indicator/Core/Math/StructuralStopRiskRule.cs
- src/CFIP.Indicator/Core/Math/StructuralStopScoringRule.cs
- src/CFIP.Indicator/Core/Math/StructuralTimeframeRule.cs
- src/CFIP.Indicator/Core/Math/SwingPlateauRule.cs
- src/CFIP.Indicator/Core/Math/TacticalOpportunityRule.cs
- src/CFIP.Indicator/Core/Math/TargetAgeSemanticsRule.cs
- src/CFIP.Indicator/Core/Math/TargetCandidateConstraintRule.cs
- src/CFIP.Indicator/Core/Math/TargetCandidateRejectionReasons.cs
- src/CFIP.Indicator/Core/Math/TargetCandidateRewardScoreRule.cs
- src/CFIP.Indicator/Core/Math/TargetLadderOption.cs
- src/CFIP.Indicator/Core/Math/TargetLadderSelectionRule.cs
- src/CFIP.Indicator/Core/Math/TargetObstacleCachePolicy.cs
- src/CFIP.Indicator/Core/Math/TargetObstacleTelemetryAccumulator.cs
- src/CFIP.Indicator/Core/Math/TargetRewardEnvelopeRule.cs
- src/CFIP.Indicator/Core/Math/TargetSelectionRequiredRrRule.cs
- src/CFIP.Indicator/Core/Math/TopDownCalibrationGroupResult.cs
- src/CFIP.Indicator/Core/Math/TopDownCalibrationRule.cs
- src/CFIP.Indicator/Core/Math/TradeOpportunityQualityRule.cs
- src/CFIP.Indicator/Core/Math/TriggerLifecycleRule.cs
- src/CFIP.Indicator/Core/Math/TriggerThresholdRule.cs
- src/CFIP.Indicator/Core/Math/VolumeExpansionRule.cs
- src/CFIP.Indicator/Core/Math/VolumeProfileEvidenceRule.cs
- src/CFIP.Indicator/Core/Math/VolumeSizingRule.cs
- src/CFIP.Indicator/Core/Math/VwapBiasRule.cs
- src/CFIP.Indicator/Core/Math/WatchReactionAlertRule.cs
- src/CFIP.Indicator/Core/Math/WaveTrendEvidenceRule.cs
- src/CFIP.Indicator/Core/Math/WaveTrendMoneyFlowRule.cs
- src/CFIP.Indicator/Core/Math/WaveTrendMovingAverageCalculator.Advanced.cs
- src/CFIP.Indicator/Core/Math/WaveTrendMovingAverageCalculator.BaseKernels.cs
- src/CFIP.Indicator/Core/Math/WaveTrendMovingAverageCalculator.cs
- src/CFIP.Indicator/Core/Math/WaveTrendReadinessRule.cs
- src/CFIP.Indicator/Core/Math/ZoneConfluenceRule.cs
- src/CFIP.Indicator/Core/Models/CanonicalPriceSnapshot.cs
- src/CFIP.Indicator/Core/Models/Decision.cs
- src/CFIP.Indicator/Core/Models/DivergenceCandidate.cs
- src/CFIP.Indicator/Core/Models/DivergenceResult.cs
- src/CFIP.Indicator/Core/Models/EntryGeometrySnapshot.cs
- src/CFIP.Indicator/Core/Models/EntrySignalTiming.cs
- src/CFIP.Indicator/Core/Models/ExecutionIntent.cs
- src/CFIP.Indicator/Core/Models/ExecutionModel.cs
- src/CFIP.Indicator/Core/Models/Level.cs
- src/CFIP.Indicator/Core/Models/MarketRegimeClassificationInput.cs
- src/CFIP.Indicator/Core/Models/MarketRegimeSnapshot.cs
- src/CFIP.Indicator/Core/Models/MarketStateFrameSnapshot.cs
- src/CFIP.Indicator/Core/Models/MarketStateSnapshot.cs
- src/CFIP.Indicator/Core/Models/OssIndicatorSnapshot.cs
- src/CFIP.Indicator/Core/Models/Plan.cs
- src/CFIP.Indicator/Core/Models/Prediction.cs
- src/CFIP.Indicator/Core/Models/PredictivePendingCandidate.cs
- src/CFIP.Indicator/Core/Models/SignalEvaluationTrace.cs
- src/CFIP.Indicator/Core/Models/StructuralStopGeometrySnapshot.cs
- src/CFIP.Indicator/Core/Models/TradeActionabilityResult.cs
- src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs
- src/CFIP.Indicator/Core/Models/TradeSetupPreview.cs
- src/CFIP.Indicator/Core/Models/VolumeProfileSnapshot.cs
- src/CFIP.Indicator/Core/Models/WaveTrendSnapshot.cs
- src/CFIP.Indicator/Core/Models/Zone.cs
- src/CFIP.Indicator/Core/Runtime/AlertDelivery.cs
- src/CFIP.Indicator/Core/Runtime/AlertDeliveryQueue.cs
- src/CFIP.Indicator/Core/Text/TextUtilities.cs
- src/CFIP.Indicator/Core/Time/TimeWindowParser.cs
- src/CFIP.Indicator/Indicator/CFIPIndicator.cs
- src/CFIP.Indicator/Indicator/Parameters/01_decision.cs
- src/CFIP.Indicator/Indicator/Parameters/02_mtf.cs
- src/CFIP.Indicator/Indicator/Parameters/03_structure.cs
- src/CFIP.Indicator/Indicator/Parameters/04_zones.cs
- src/CFIP.Indicator/Indicator/Parameters/05_liquidity.cs
- src/CFIP.Indicator/Indicator/Parameters/06_indicators.cs
- src/CFIP.Indicator/Indicator/Parameters/07_entry_precision.cs
- src/CFIP.Indicator/Indicator/Parameters/08_smart_weights.cs
- src/CFIP.Indicator/Indicator/Parameters/09_risk_targets.cs
- src/CFIP.Indicator/Indicator/Parameters/10_live_management.cs
- src/CFIP.Indicator/Indicator/Parameters/11_filters.cs
- src/CFIP.Indicator/Indicator/Parameters/12_alerts_advanced.cs
- src/CFIP.Indicator/Indicator/Parameters/12_alerts_core.cs
- src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs
- src/CFIP.Indicator/Indicator/Parameters/14_display_advanced.cs
- src/CFIP.Indicator/Indicator/Parameters/14_display_core.cs
- src/CFIP.Indicator/Indicator/Parameters/14_display_panel.cs
- src/CFIP.Indicator/Indicator/Parameters/15_control_advanced.cs
- src/CFIP.Indicator/Indicator/Parameters/15_intelligence_early.cs
- src/CFIP.Indicator/Indicator/Parameters/16_accuracy.cs
- src/CFIP.Indicator/Indicator/Parameters/17_smart_engine.cs
- src/CFIP.Indicator/Indicator/Parameters/20_confluence_extensions.cs
- src/CFIP.Indicator/Indicator/Parameters/21_complete_intelligence.cs
- src/CFIP.Indicator/Indicator/Parameters/22_safety_precision.cs
- src/CFIP.Indicator/Indicator/Parameters/23_structural_execution.cs
- src/CFIP.Indicator/Indicator/Parameters/24_smart_execution.cs
- src/CFIP.Indicator/Indicator/Parameters/25_oss_analytics.cs
- src/CFIP.Indicator/Indicator/Parameters/26_wave_trend.cs
- src/CFIP.Indicator/Indicator/Parameters/27_parallel_opportunities.cs
- src/CFIP.Indicator/Indicator/Parameters/28_news_guard.cs
- src/CFIP.Indicator/Indicator/State.cs
- src/CFIP.Indicator/Planning/Entry/BearTriggerScoreAnalyzer.cs
- src/CFIP.Indicator/Planning/Entry/BullTriggerScoreAnalyzer.cs
- src/CFIP.Indicator/Planning/Entry/ClosedBarTriggerReadyEvaluator.cs
- src/CFIP.Indicator/Planning/Entry/M1TriggerReadyEvaluator.cs
- src/CFIP.Indicator/Planning/Entry/M1TriggerRuntimeUpdater.cs
- src/CFIP.Indicator/Planning/Entry/TriggerRuntimeState.cs
- src/CFIP.Indicator/Planning/Execution/ExecutionIntentBuilder.cs
- src/CFIP.Indicator/Planning/Execution/ExecutionIntentValidation.cs
- src/CFIP.Indicator/Planning/Execution/ExecutionModeResolver.cs
- src/CFIP.Indicator/Planning/Execution/ExecutionModelBuilder.cs
- src/CFIP.Indicator/Planning/Execution/ExecutionZoneBuilder.cs
- src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCandidates.cs
- src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCore.cs
- src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelector.cs
- src/CFIP.Indicator/Planning/Execution/ExecutionZoneQualityEvaluator.cs
- src/CFIP.Indicator/Planning/Execution/ExecutionZoneSelectionCandidate.cs
- src/CFIP.Indicator/Planning/Execution/MarketEntryValidation.cs
- src/CFIP.Indicator/Planning/Execution/PredictivePendingCandidateScorer.cs
- src/CFIP.Indicator/Planning/Execution/PredictivePendingLevelSelector.cs
- src/CFIP.Indicator/Planning/Execution/PredictivePendingZoneCollector.cs
- src/CFIP.Indicator/Planning/Execution/TriggerGate.cs
- src/CFIP.Indicator/Planning/Filters/RegimeFilter.cs
- src/CFIP.Indicator/Planning/Filters/TradingSessionFilter.cs
- src/CFIP.Indicator/Planning/TradePlan/HtfRewardSourcePolicy.cs
- src/CFIP.Indicator/Planning/TradePlan/HtfSourceClassifier.cs
- src/CFIP.Indicator/Planning/TradePlan/HtfTargetCounter.cs
- src/CFIP.Indicator/Planning/TradePlan/HtfTimeframeClassifier.cs
- src/CFIP.Indicator/Planning/TradePlan/MinimumRequiredRiskRewardCalculator.cs
- src/CFIP.Indicator/Planning/TradePlan/PlanBuilder.cs
- src/CFIP.Indicator/Planning/TradePlan/PlanInputPreparation.cs
- src/CFIP.Indicator/Planning/TradePlan/PlanIntegrityValidator.cs
- src/CFIP.Indicator/Planning/TradePlan/PlanMarketConstraintValidator.cs
- src/CFIP.Indicator/Planning/TradePlan/PlanMaterialization.cs
- src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs
- src/CFIP.Indicator/Planning/TradePlan/PlanProtectionIntegrityValidator.cs
- src/CFIP.Indicator/Planning/TradePlan/PlanRewardIntegrityValidator.cs
- src/CFIP.Indicator/Planning/TradePlan/PlanTargetPreparation.cs
- src/CFIP.Indicator/Planning/TradePlan/Sources/DailyPivotTargetSource.cs
- src/CFIP.Indicator/Planning/TradePlan/Sources/HtfTargetSource.cs
- src/CFIP.Indicator/Planning/TradePlan/Sources/LiquidityAboveTargetSource.cs
- src/CFIP.Indicator/Planning/TradePlan/Sources/LiquidityBelowTargetSource.cs
- src/CFIP.Indicator/Planning/TradePlan/Sources/M1MicroTargetSource.cs
- src/CFIP.Indicator/Planning/TradePlan/Sources/PreviousPeriodTargetSource.cs
- src/CFIP.Indicator/Planning/TradePlan/Sources/SessionTargetSource.cs
- src/CFIP.Indicator/Planning/TradePlan/Sources/SmartExtraTargetSource.cs
- src/CFIP.Indicator/Planning/TradePlan/Sources/SupplyDemandLiquidityTargetSource.cs
- src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs
- src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs
- src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateSelector.cs
- src/CFIP.Indicator/Planning/TradePlan/StructuralStopPlanner.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetCandidateEvaluator.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetLadderStageCandidateBuilder.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetLevelCandidateMerger.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetLevelMerger.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetMetadataEnricher.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetProgressionRule.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetProgressionValidator.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetSelectionPolicy.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetSelector.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetStageFeasibilityGate.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetStageRejectionTelemetry.cs
- src/CFIP.Indicator/Planning/TradePlan/TargetStageSelector.cs
- src/CFIP.Indicator/Runtime/Calculation/CalculationBrokerBoundary.cs
- src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs
- src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs
- src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs
- src/CFIP.Indicator/Runtime/Calculation/CalculationLiveCycle.cs
- src/CFIP.Indicator/Runtime/Calculation/CalculationMarketContext.cs
- src/CFIP.Indicator/Runtime/Calculation/CalculationPreparation.cs
- src/CFIP.Indicator/Runtime/Calculation/CalculationReadinessStateStore.cs
- src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs
- src/CFIP.Indicator/Runtime/Calculation/CalculationStartupSeed.cs
- src/CFIP.Indicator/Runtime/Calculation/CanonicalMarketContextBuilder.cs
- src/CFIP.Indicator/Runtime/Calculation/RuntimeFaultBoundary.cs
- src/CFIP.Indicator/Runtime/Calculation/RuntimeFaultState.cs
- src/CFIP.Indicator/Runtime/Calculation/RuntimeFaultStateMachine.cs
- src/CFIP.Indicator/Runtime/Cbot/CbotChartLifecycleEvents.cs
- src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs
- src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs
- src/CFIP.Indicator/Runtime/Initialization/StartupDataHelpers.cs
- src/CFIP.Indicator/Runtime/Mtf/MtfClosedContext.cs
- src/CFIP.Indicator/Runtime/Mtf/MtfClosedContextCache.cs
- src/CFIP.Indicator/Runtime/Mtf/MtfContextBuilder.cs
- src/CFIP.Indicator/Runtime/Provider/CFIPDeviceScenarioBatchPublisher.cs
- src/CFIP.Indicator/Runtime/Provider/CFIPDeviceSignalPublisher.cs
- src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProvider.cs
- src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderIdentity.cs
- src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs
- src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderRefresh.cs
- src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderScenarioBatch.cs
- src/CFIP.Indicator/Runtime/Supervision/PanelHeartbeatLiveState.cs
- src/CFIP.Indicator/Runtime/Supervision/RuntimePanelHeartbeat.cs
- src/CFIP.Indicator/Runtime/Supervision/RuntimeSafetySupervisor.cs
- src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs
- src/CFIP.Indicator/Trading/Alerts/CanonicalAlertEnvelopeBuilder.cs
- src/CFIP.Indicator/Trading/Alerts/ContextAlertEmitter.cs
- src/CFIP.Indicator/Trading/Alerts/EndOfDayAlert.cs
- src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveAcceptedFillHandler.cs
- src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveConfirmationAlert.cs
- src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveExecutionPreparation.cs
- src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveFinalExecutionGuard.cs
- src/CFIP.Indicator/Trading/Execution/Aggressive/AggressivePreTradeEligibility.cs
- src/CFIP.Indicator/Trading/Execution/Aggressive/AggressivePreTradePreparation.cs
- src/CFIP.Indicator/Trading/Execution/Aggressive/BoundPlanProtection.cs
- src/CFIP.Indicator/Trading/Execution/Aggressive/BrokerProtectionExecution.cs
- src/CFIP.Indicator/Trading/Execution/Aggressive/OrphanManagedProtection.cs
- src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketExecutionPreparation.cs
- src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketFillReconciliation.cs
- src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPostFillTargetResolver.cs
- src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPreTrade.cs
- src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPreTradeEligibility.cs
- src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketRangeCalculator.cs
- src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketSubmissionValidator.cs
- src/CFIP.Indicator/Trading/Execution/BrokerConfirmationPolicy.cs
- src/CFIP.Indicator/Trading/Execution/BrokerProtectionCoordinator.cs
- src/CFIP.Indicator/Trading/Execution/ExecutionPlanPreparation.cs
- src/CFIP.Indicator/Trading/Execution/ManagementCommandRequestCoordinator.cs
- src/CFIP.Indicator/Trading/Execution/PriceMath.cs
- src/CFIP.Indicator/Trading/Execution/ServerSideTakeProfitLadder.cs
- src/CFIP.Indicator/Trading/Execution/ServerSideTakeProfitLadderProgression.cs
- src/CFIP.Indicator/Trading/Execution/State/AutoTradingStateStore.cs
- src/CFIP.Indicator/Trading/Execution/State/LifecycleStateStore.cs
- src/CFIP.Indicator/Trading/Execution/State/TargetStageState.cs
- src/CFIP.Indicator/Trading/Execution/State/TradeLabelFormatter.cs
- src/CFIP.Indicator/Trading/Execution/SubmissionGateCoordinator.cs
- src/CFIP.Indicator/Trading/Identity/BrokerIdentity.cs
- src/CFIP.Indicator/Trading/Identity/ManagedPositionGuards.cs
- src/CFIP.Indicator/Trading/Identity/TradeExecutionMetadata.cs
- src/CFIP.Indicator/Trading/Intelligence/BufferedArchivePersistence.cs
- src/CFIP.Indicator/Trading/Intelligence/BufferedArchivePersistenceModels.cs
- src/CFIP.Indicator/Trading/Intelligence/BufferedPersistenceCoordinator.cs
- src/CFIP.Indicator/Trading/Intelligence/EconomicNewsCalendarClient.cs
- src/CFIP.Indicator/Trading/Intelligence/EconomicNewsProtection.cs
- src/CFIP.Indicator/Trading/Intelligence/EconomicNewsRiskEvaluator.cs
- src/CFIP.Indicator/Trading/Intelligence/EntryLocationQualityAnalyzer.cs
- src/CFIP.Indicator/Trading/Intelligence/EntrySignalTimingRuntime.cs
- src/CFIP.Indicator/Trading/Intelligence/ExecutionTelemetryRecord.cs
- src/CFIP.Indicator/Trading/Intelligence/FreshTriggerEvidenceAnalyzer.cs
- src/CFIP.Indicator/Trading/Intelligence/HistoricalOutcomeReader.cs
- src/CFIP.Indicator/Trading/Intelligence/NoTradeRegimeAnalyzer.cs
- src/CFIP.Indicator/Trading/Intelligence/OutcomeHistoryArchiveStore.cs
- src/CFIP.Indicator/Trading/Intelligence/OutcomeMemoryAccountSwitch.cs
- src/CFIP.Indicator/Trading/Intelligence/OutcomeMemoryStore.cs
- src/CFIP.Indicator/Trading/Intelligence/OutcomeObservation.cs
- src/CFIP.Indicator/Trading/Intelligence/OutcomePanelTelemetry.cs
- src/CFIP.Indicator/Trading/Intelligence/OutcomeRegistrationResult.cs
- src/CFIP.Indicator/Trading/Intelligence/OutcomeTelemetryEngine.cs
- src/CFIP.Indicator/Trading/Intelligence/OutcomeWindowMonitor.cs
- src/CFIP.Indicator/Trading/Intelligence/PortableMemorySnapshotStore.cs
- src/CFIP.Indicator/Trading/Intelligence/Prediction/EarlyPredictionEngine.cs
- src/CFIP.Indicator/Trading/Intelligence/Prediction/LiveReversalAnalyzer.cs
- src/CFIP.Indicator/Trading/Intelligence/RuntimeLogPersistence.cs
- src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchivePersistence.cs
- src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchiveStore.cs
- src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceRecorder.cs
- src/CFIP.Indicator/Trading/Intelligence/StructuralSequenceAnalyzer.cs
- src/CFIP.Indicator/Trading/Intelligence/TradePlanRegistry.cs
- src/CFIP.Indicator/Trading/Lifecycle/ActiveBrokerStopAccessor.cs
- src/CFIP.Indicator/Trading/Lifecycle/ActiveBrokerTargetAccessor.cs
- src/CFIP.Indicator/Trading/Lifecycle/AutoTradingDisableReminder.cs
- src/CFIP.Indicator/Trading/Lifecycle/BrokerProtectionStateEvaluator.cs
- src/CFIP.Indicator/Trading/Lifecycle/BrokerProtectionStateSynchronizer.cs
- src/CFIP.Indicator/Trading/Lifecycle/BrokerStateSnapshot.cs
- src/CFIP.Indicator/Trading/Lifecycle/LifecycleEventIdempotencyGuard.cs
- src/CFIP.Indicator/Trading/Lifecycle/LifecycleEventState.cs
- src/CFIP.Indicator/Trading/Lifecycle/LifecycleTransitionPolicy.cs
- src/CFIP.Indicator/Trading/Lifecycle/LiveFillExitReconciler.cs
- src/CFIP.Indicator/Trading/Lifecycle/LiveFillReconciliation.cs
- src/CFIP.Indicator/Trading/Lifecycle/LivePlanExitCoordinator.cs
- src/CFIP.Indicator/Trading/Lifecycle/LivePlanFactory.cs
- src/CFIP.Indicator/Trading/Lifecycle/LivePlanFurtherTargetSelector.cs
- src/CFIP.Indicator/Trading/Lifecycle/LivePlanTargetEnrichment.cs
- src/CFIP.Indicator/Trading/Lifecycle/ManagedLivePlanRecovery.cs
- src/CFIP.Indicator/Trading/Lifecycle/ManagedPositionLookup.cs
- src/CFIP.Indicator/Trading/Lifecycle/PendingCancelledHandler.cs
- src/CFIP.Indicator/Trading/Lifecycle/PendingCreatedHandler.cs
- src/CFIP.Indicator/Trading/Lifecycle/PendingFillPlanBuilder.cs
- src/CFIP.Indicator/Trading/Lifecycle/PendingFillProtectionCoordinator.cs
- src/CFIP.Indicator/Trading/Lifecycle/PendingFilledHandler.cs
- src/CFIP.Indicator/Trading/Lifecycle/PendingModifiedHandler.cs
- src/CFIP.Indicator/Trading/Lifecycle/PendingOrderCircuitBreaker.cs
- src/CFIP.Indicator/Trading/Lifecycle/PendingOrderPlanSnapshot.cs
- src/CFIP.Indicator/Trading/Lifecycle/PositionCircuitBreaker.cs
- src/CFIP.Indicator/Trading/Lifecycle/PositionClosedHandler.cs
- src/CFIP.Indicator/Trading/Lifecycle/PositionModifiedHandler.cs
- src/CFIP.Indicator/Trading/Lifecycle/PositionOpenedHandler.cs
- src/CFIP.Indicator/Trading/LiveManagement/ActivePlanEvaluation.cs
- src/CFIP.Indicator/Trading/LiveManagement/ActivePlanFalseSignalGuard.cs
- src/CFIP.Indicator/Trading/LiveManagement/ActivePlanIntegrityHandler.cs
- src/CFIP.Indicator/Trading/LiveManagement/ActivePlanLevelExitHandler.cs
- src/CFIP.Indicator/Trading/LiveManagement/ActivePlanLiveManagement.cs
- src/CFIP.Indicator/Trading/LiveManagement/ActivePlanMarketState.cs
- src/CFIP.Indicator/Trading/LiveManagement/ActivePlanReactionExitHandler.cs
- src/CFIP.Indicator/Trading/LiveManagement/LiveReversalEpisodeState.cs
- src/CFIP.Indicator/Trading/LiveManagement/LiveStructuralPulse.cs
- src/CFIP.Indicator/Trading/LiveManagement/LiveTargetCandidateEvaluator.cs
- src/CFIP.Indicator/Trading/LiveManagement/PartialTakeProfitExecutor.cs
- src/CFIP.Indicator/Trading/LiveManagement/PlanActivation.cs
- src/CFIP.Indicator/Trading/LiveManagement/PlanRiskRewardRecalculator.cs
- src/CFIP.Indicator/Trading/LiveManagement/ProtectionManager.cs
- src/CFIP.Indicator/Trading/LiveManagement/ReversalCloseGuard.cs
- src/CFIP.Indicator/Trading/LiveManagement/ReversalProtection.cs
- src/CFIP.Indicator/Trading/LiveManagement/SmartExitModeResolver.cs
- src/CFIP.Indicator/Trading/LiveManagement/SmartExitPressureCalculator.cs
- src/CFIP.Indicator/Trading/LiveManagement/StructuralSetupInvalidationExit.cs
- src/CFIP.Indicator/Trading/LiveManagement/TargetProgression.cs
- src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPlacement.cs
- src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPreparation.cs
- src/CFIP.Indicator/Trading/Pending/Placement/PendingOrderCleanup.cs
- src/CFIP.Indicator/Trading/Pending/Placement/PendingOrderConfirmationReporter.cs
- src/CFIP.Indicator/Trading/Pending/Placement/PendingSubmissionValidator.cs
- src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPlacement.cs
- src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPreparation.cs
- src/CFIP.Indicator/Trading/Pending/Placement/SmartPendingOrderOrchestrator.cs
- src/CFIP.Indicator/Trading/Pending/Policy/PendingOrderPolicy.cs
- src/CFIP.Indicator/Trading/Risk/AdaptiveOutcomeRiskPolicy.cs
- src/CFIP.Indicator/Trading/Risk/AggressiveRiskPolicy.cs
- src/CFIP.Indicator/Trading/Risk/AggressiveVolumeSizer.cs
- src/CFIP.Indicator/Trading/Risk/AutoPlanRiskValidator.cs
- src/CFIP.Indicator/Trading/Risk/AutoRiskPolicy.cs
- src/CFIP.Indicator/Trading/Risk/AutoTradeSafetyGuard.cs
- src/CFIP.Indicator/Trading/Risk/AverageAtrCalculator.cs
- src/CFIP.Indicator/Trading/Risk/DailyLossAccounting.cs
- src/CFIP.Indicator/Trading/Risk/DailyLossGuard.cs
- src/CFIP.Indicator/Trading/Risk/DailyLossPersistence.cs
- src/CFIP.Indicator/Trading/Risk/ExecutionCapacityGuard.cs
- src/CFIP.Indicator/Trading/Risk/ManagedPositionCounter.cs
- src/CFIP.Indicator/Trading/Risk/MarginSafetyCalculator.cs
- src/CFIP.Indicator/Trading/Risk/MarginUsagePolicy.cs
- src/CFIP.Indicator/Trading/Risk/MarketSuitabilityGuard.cs
- src/CFIP.Indicator/Trading/Risk/MarketSuitabilityRefreshCoordinator.cs
- src/CFIP.Indicator/Trading/Risk/RiskAmountCalculator.cs
- src/CFIP.Indicator/Trading/Risk/RiskPercentPolicy.cs
- src/CFIP.Indicator/Trading/Risk/SessionWindowEvaluator.cs
- src/CFIP.Indicator/Trading/Risk/SuitabilityCalculator.cs
- src/CFIP.Indicator/Trading/Risk/SuitabilityRiskMultiplierCalculator.cs
- src/CFIP.Indicator/Trading/Risk/VolumeSizer.cs
- src/CFIP.Indicator/Trading/Validation/ActionableSignalQualityGate.cs
- src/CFIP.Indicator/Trading/Validation/DecisionBlockReasonPolicy.cs
- src/CFIP.Indicator/Trading/Validation/HigherTfRewardPathValidator.cs
- src/CFIP.Indicator/Trading/Validation/HtfTargetPresenceValidator.cs
- src/CFIP.Indicator/Trading/Validation/LiveExecutionGateReasonPolicy.cs
- src/CFIP.Indicator/Trading/Validation/LiveProtectionDistanceResolver.cs
- src/CFIP.Indicator/Trading/Validation/PlanCreationEligibility.cs
- src/CFIP.Indicator/Trading/Validation/PreTradePlanSynchronizer.cs
- src/CFIP.Indicator/Trading/Validation/PriceProtectionValidation.cs
- src/CFIP.Indicator/Trading/Validation/RewardPathZoneObstacleScanner.cs
- src/CFIP.Indicator/Trading/Validation/SignalPlanCoordinator.cs
- src/CFIP.Indicator/Trading/Validation/SmartThresholdPolicy.cs
- src/CFIP.Indicator/Trading/Validation/TargetObstacleScanCache.cs
- src/CFIP.Indicator/Trading/Validation/TargetObstacleScanSnapshotBuilder.cs
- src/CFIP.Indicator/Trading/Validation/TargetObstacleValidator.cs
- src/CFIP.Indicator/Trading/Validation/TradeActionabilityDecisionGate.cs
- src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs
- src/CFIP.Indicator/Trading/Validation/TradeActionabilityRetestContext.cs
- src/CFIP.Indicator/UI/Chart/AlertSignalRenderer.cs
- src/CFIP.Indicator/UI/Chart/ChartObjectCleanup.cs
- src/CFIP.Indicator/UI/Chart/MtfTrendStrengthSnapshotBuilder.cs
- src/CFIP.Indicator/UI/Chart/OutcomeMarkerRenderer.cs
- src/CFIP.Indicator/UI/Chart/ParallelOpportunityRenderer.cs
- src/CFIP.Indicator/UI/Chart/PendingOrderRenderer.cs
- src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs
- src/CFIP.Indicator/UI/Chart/PlanLabelFormatting.cs
- src/CFIP.Indicator/UI/Chart/PlanLabelRemover.cs
- src/CFIP.Indicator/UI/Chart/PlanLabelRenderCoordinator.cs
- src/CFIP.Indicator/UI/Chart/PlanLabelRenderer.cs
- src/CFIP.Indicator/UI/Chart/PlanLineRemover.cs
- src/CFIP.Indicator/UI/Chart/PlanLineRenderer.cs
- src/CFIP.Indicator/UI/Chart/PlanObjectClearer.cs
- src/CFIP.Indicator/UI/Chart/PlanObjectRemover.cs
- src/CFIP.Indicator/UI/Chart/PlanRenderCoordinator.cs
- src/CFIP.Indicator/UI/Chart/PredictionLabelsRenderer.cs
- src/CFIP.Indicator/UI/Chart/PredictionLineRenderer.cs
- src/CFIP.Indicator/UI/Chart/PredictionObjectCleanup.cs
- src/CFIP.Indicator/UI/Chart/PredictionRenderer.cs
- src/CFIP.Indicator/UI/Chart/SignalPresentationRenderer.cs
- src/CFIP.Indicator/UI/Chart/SignalRenderer.cs
- src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs
- src/CFIP.Indicator/UI/Chart/SignalVisualDirectionResolver.cs
- src/CFIP.Indicator/UI/Chart/SignalVisualIdentityBuilder.cs
- src/CFIP.Indicator/UI/Chart/SignalVisualSnapshot.cs
- src/CFIP.Indicator/UI/Chart/SignalVisualSnapshotBuilder.cs
- src/CFIP.Indicator/UI/Chart/SignalVisualSynchronizer.cs
- src/CFIP.Indicator/UI/Controls/ExecutionControlsFactory.cs
- src/CFIP.Indicator/UI/Controls/ExecutionControlsSynchronizer.cs
- src/CFIP.Indicator/UI/Controls/SessionPresentation.cs
- src/CFIP.Indicator/UI/Historical/HistoricalRenderer.cs
- src/CFIP.Indicator/UI/Historical/HistoricalSignalPresentation.cs
- src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs
- src/CFIP.Indicator/UI/Panel/PanelAlertMessageRenderer.cs
- src/CFIP.Indicator/UI/Panel/PanelCanonicalSignalStatus.cs
- src/CFIP.Indicator/UI/Panel/PanelConstants.cs
- src/CFIP.Indicator/UI/Panel/PanelContentRefresh.cs
- src/CFIP.Indicator/UI/Panel/PanelExecutionSemantics.cs
- src/CFIP.Indicator/UI/Panel/PanelExecutionState.cs
- src/CFIP.Indicator/UI/Panel/PanelFactory.cs
- src/CFIP.Indicator/UI/Panel/PanelHeaderLiveState.cs
- src/CFIP.Indicator/UI/Panel/PanelHeaderRenderer.cs
- src/CFIP.Indicator/UI/Panel/PanelLayoutManager.cs
- src/CFIP.Indicator/UI/Panel/PanelMainRenderer.cs
- src/CFIP.Indicator/UI/Panel/PanelPredictionState.cs
- src/CFIP.Indicator/UI/Panel/PanelRenderOptimization.cs
- src/CFIP.Indicator/UI/Panel/PanelRestoreButtonFactory.cs
- src/CFIP.Indicator/UI/Panel/PanelRowWriter.cs
- src/CFIP.Indicator/UI/Panel/PanelRowsFactory.cs
- src/CFIP.Indicator/UI/Panel/PanelRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/PanelSignalState.cs
- src/CFIP.Indicator/UI/Panel/PanelTextFormatting.cs
- src/CFIP.Indicator/UI/Panel/PanelTimeframePresentationState.cs
- src/CFIP.Indicator/UI/Panel/PanelToggleButtonFactory.cs
- src/CFIP.Indicator/UI/Panel/PanelTrendTimeframeLampRow.cs
- src/CFIP.Indicator/UI/Panel/PanelVisibility.cs
- src/CFIP.Indicator/UI/Panel/ProcessingHeartbeatLamp.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelCalibrationRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelContextRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelDecisionRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelExecutionRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewDiagnosticRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewStateRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelSignalPipelineRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelTradePlanLevelRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelTradePlanLiveRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelTradePlanRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Rows/PanelWaveTrendAndOpportunityRowsRenderer.cs
- src/CFIP.Indicator/UI/Panel/Theme/PanelActionButtonsLayout.cs
- src/CFIP.Indicator/UI/Panel/Theme/PanelQuickExecutionLayout.cs
- src/CFIP.Indicator/UI/Panel/Theme/PanelRestoreButtonLayout.cs
- src/CFIP.Indicator/UI/Panel/Theme/PanelRowsLayout.cs
- src/CFIP.Indicator/UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs
- src/CFIP.Indicator/UI/Panel/Theme/PanelVisualSettings.cs
- src/CFIP.cBot/Binding/CfipDeviceSignalTransport.cs
- src/CFIP.cBot/Binding/CfipIndicatorChartBinding.cs
- src/CFIP.cBot/CFIP.cBot.csproj
- src/CFIP.cBot/CFIPExecutionBot.cs
- src/CFIP.cBot/Execution/BrokerExecutionSafety.cs
- src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs
- src/CFIP.cBot/Execution/CbotExecutionIdempotencyStore.cs
- src/CFIP.cBot/Execution/CbotExecutionLifecycleRule.cs
- src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs
- src/CFIP.cBot/Execution/CbotIndicatorExecutionSettings.cs
- src/CFIP.cBot/Execution/CbotLifecycleAudioService.cs
- src/CFIP.cBot/Execution/CbotManagedObjectIdentityRule.cs
- src/CFIP.cBot/Execution/CbotManagementPolicyRule.cs
- src/CFIP.cBot/Execution/CbotSignalPreflight.cs
- src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs
- src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs
- src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs
- src/CFIP.cBot/Recovery/CbotBrokerReconciliation.cs
- src/CFIP.cBot/Risk/CbotDailyLossGuard.cs
- src/CFIP.cBot/Risk/ExecutionMarginBudgetRule.cs
- src/CFIP.cBot/Shadow/ShadowHostContracts.cs
- src/CFIP.cBot/Shadow/ShadowHostCoordinator.cs
- src/CFIP.cBot/Shadow/ShadowHostValidator.cs

### tools

- tools/CFIP.Decision.Contracts/CFIP.Decision.Contracts.csproj
- tools/CFIP.Decision.Contracts/Program.cs
- tools/CFIP.Execution.Contracts/CFIP.Execution.Contracts.csproj
- tools/CFIP.Execution.Contracts/Program.cs
- tools/CFIP.Indicator.CI/CFIP.Indicator.CI.csproj
- tools/CFIP.Planning.Contracts/CFIP.Planning.Contracts.csproj
- tools/CFIP.Planning.Contracts/Program.cs
- tools/CFIP.Runtime.Contracts/AlertDeliveryQueueContracts.cs
- tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj
- tools/CFIP.Runtime.Contracts/Ci20BProtectionAndSignalContracts.cs
- tools/CFIP.Runtime.Contracts/DeterministicReplaySuite.cs
- tools/CFIP.Runtime.Contracts/M4TimeHistoryContracts.cs
- tools/CFIP.Runtime.Contracts/M5PanelContracts.cs
- tools/CFIP.Runtime.Contracts/Program.cs
- tools/CFIP.StockIndicators.Benchmark/Benchmark/BenchmarkFixtures.cs
- tools/CFIP.StockIndicators.Benchmark/Benchmark/BenchmarkModel.cs
- tools/CFIP.StockIndicators.Benchmark/Benchmark/BenchmarkReport.cs
- tools/CFIP.StockIndicators.Benchmark/Benchmark/IndicatorComparison.cs
- tools/CFIP.StockIndicators.Benchmark/Benchmark/QuoteCacheBenchmark.cs
- tools/CFIP.StockIndicators.Benchmark/Benchmark/SkenderWarmupParityBenchmark.cs
- tools/CFIP.StockIndicators.Benchmark/CFIP.StockIndicators.Benchmark.csproj
- tools/CFIP.StockIndicators.Benchmark/D10/NativeRegistryLookupBenchmark.cs
- tools/CFIP.StockIndicators.Benchmark/Program.cs
- tools/CFIP.StockIndicators.Benchmark/README.md
- tools/CFIP.StockIndicators.Benchmark/global.json
- tools/CFIP.cBot.Shadow.Tests/CFIP.cBot.Shadow.Tests.csproj
- tools/CFIP.cBot.Shadow.Tests/Program.cs
- tools/analyze_phase_11_3.py
- tools/analyze_runtime_log.py
- tools/analyze_signal_trace.py
- tools/audit_calculation_cycle.py
- tools/audit_cbot_boundary.py
- tools/audit_cbot_contract_schema.py
- tools/audit_cbot_demo_live_market.py
- tools/audit_cbot_preflight.py
- tools/audit_cbot_project_boundary.py
- tools/audit_cbot_provider_boundary.py
- tools/audit_cbot_shadow_host.py
- tools/audit_execution_capacity_semantics.py
- tools/audit_exit_geometry.py
- tools/audit_hotpath_persistence.py
- tools/audit_news_guard.py
- tools/audit_optimization_readiness.py
- tools/audit_parameter_count.py
- tools/audit_parameter_semantics.py
- tools/audit_parameters.py
- tools/audit_phase_11_2.py
- tools/audit_phase_11_3.py
- tools/audit_phase_11_4.py
- tools/audit_phase_11_5.py
- tools/audit_phase_11_8.py
- tools/audit_phase_2_1.py
- tools/audit_phase_2_2.py
- tools/audit_phase_2_3.py
- tools/audit_phase_2_4.py
- tools/audit_phase_2_5.py
- tools/audit_phase_2_6.py
- tools/audit_phase_2_7.py
- tools/audit_phase_2_8.py
- tools/audit_phase_2_9.py
- tools/audit_phase_3_1.py
- tools/audit_phase_3_2.py
- tools/audit_phase_3_3.py
- tools/audit_phase_3_4.py
- tools/audit_phase_3_5.py
- tools/audit_phase_4_1.py
- tools/audit_phase_4_10.py
- tools/audit_phase_4_2.py
- tools/audit_phase_4_3.py
- tools/audit_phase_4_4.py
- tools/audit_phase_4_5.py
- tools/audit_phase_4_6.py
- tools/audit_phase_4_7.py
- tools/audit_phase_4_8.py
- tools/audit_phase_4_9.py
- tools/audit_phase_5_1.py
- tools/audit_phase_5_2.py
- tools/audit_phase_5_3.py
- tools/audit_phase_5_4.py
- tools/audit_phase_5_5.py
- tools/audit_phase_5_6.py
- tools/audit_phase_5_7.py
- tools/audit_phase_5_8.py
- tools/audit_phase_6_1.py
- tools/audit_phase_6_2.py
- tools/audit_phase_6_3.py
- tools/audit_phase_6_4.py
- tools/audit_phase_6_5.py
- tools/audit_phase_6_6.py
- tools/audit_phase_6_7.py
- tools/audit_phase_6_8.py
- tools/audit_phase_6_9.py
- tools/audit_phase_7_1.py
- tools/audit_phase_7_2.py
- tools/audit_phase_7_3.py
- tools/audit_phase_7_4.py
- tools/audit_phase_7_5.py
- tools/audit_phase_7_6a.py
- tools/audit_phase_7_6b.py
- tools/audit_phase_8_1.py
- tools/audit_phase_8_2.py
- tools/audit_phase_8_3a.py
- tools/audit_phase_8_3b.py
- tools/audit_phase_accumulation.py
- tools/audit_phase_build_warning_panel_height.py
- tools/audit_phase_cbot_6m.py
- tools/audit_phase_cbot_attachment_audio_2026_10_02.py
- tools/audit_phase_cbot_broker_confirmed_facts_2026_10_03.py
- tools/audit_phase_cbot_lifecycle_audio_2026_10_03.py
- tools/audit_phase_cbot_lifecycle_effective_state_2026_10_03.py
- tools/audit_phase_cbot_local_cloud_lifecycle_2026_10_03.py
- tools/audit_phase_cbot_management_policy_hardening_2026_10_03.py
- tools/audit_phase_cbot_p4b.py
- tools/audit_phase_cbot_p4c.py
- tools/audit_phase_cbot_p4d.py
- tools/audit_phase_cbot_p4e.py
- tools/audit_phase_cbot_p5.py
- tools/audit_phase_cbot_p5_reconciliation.py
- tools/audit_phase_cbot_p6_account_risk_and_connection.py
- tools/audit_phase_cbot_p7_ui_state_cutover.py
- tools/audit_phase_cbot_p7_whole_chain.py
- tools/audit_phase_cbot_p7r_attachment_alert_visibility.py
- tools/audit_phase_cbot_p8_progressive_protection_state_sync.py
- tools/audit_phase_cbot_p9.py
- tools/audit_phase_cbot_position_truth_hardening_2026_10_03.py
- tools/audit_phase_cbot_shadow_multiscenario_truth_2026_10_03.py
- tools/audit_phase_ci20_engine_quality.py
- tools/audit_phase_ci20_panel_options.py
- tools/audit_phase_ci20b_protection_cbot_signal.py
- tools/audit_phase_ci20c_cbot_connection_lifecycle.py
- tools/audit_phase_ci21_primary_signal_visibility.py
- tools/audit_phase_ci_00.py
- tools/audit_phase_ci_01.py
- tools/audit_phase_ci_02.py
- tools/audit_phase_ci_03.py
- tools/audit_phase_ci_04.py
- tools/audit_phase_ci_05.py
- tools/audit_phase_ci_06.py
- tools/audit_phase_ci_07.py
- tools/audit_phase_ci_08.py
- tools/audit_phase_ci_09.py
- tools/audit_phase_ci_10.py
- tools/audit_phase_ci_11.py
- tools/audit_phase_ci_12.py
- tools/audit_phase_ci_13.py
- tools/audit_phase_ci_14.py
- tools/audit_phase_ci_15.py
- tools/audit_phase_ci_16.py
- tools/audit_phase_ci_17.py
- tools/audit_phase_ci_17a.py
- tools/audit_phase_ci_18_signal_panel.py
- tools/audit_phase_ci_19_signal_target_quality.py
- tools/audit_phase_final_realtime_live_smart_system_2026_10_03.py
- tools/audit_phase_indicator_name_cbot_launch_mtf_panel.py
- tools/audit_phase_m15_primary_execution_risk_spread.py
- tools/audit_phase_m3_trade_truth.py
- tools/audit_phase_m4_time_history.py
- tools/audit_phase_m5_panel_live.py
- tools/audit_phase_mtf_primary_location_obfvg.py
- tools/audit_phase_mtf_primary_panel.py
- tools/audit_phase_mtf_primary_provider_identity.py
- tools/audit_phase_opportunity_mining_zone_selection_2026_10_02.py
- tools/audit_phase_panel_clearance_restore_position.py
- tools/audit_phase_panel_footer_alert_dedup_2026_10_03.py
- tools/audit_phase_panel_header_realtime_2026_10_03.py
- tools/audit_phase_panel_semantic_consistency_2026_10_03.py
- tools/audit_phase_panel_timeframe_single_source_2026_10_03.py
- tools/audit_phase_position_engine_cbot_truth_hardening_2026_10_02.py
- tools/audit_phase_realtime_live_signal_unification_2026_10_03.py
- tools/audit_phase_realtime_multiscenario_opportunity_engine_2026_10_03.py
- tools/audit_phase_retest_trigger_path.py
- tools/audit_phase_signal_drawing_canonical_2026_10_03.py
- tools/audit_phase_single_owner_duality_2026_10_03.py
- tools/audit_phase_smart_separated_signal_arrows_2026_10_04.py
- tools/audit_phase_user_requirements_2026_10_03.py
- tools/audit_phase_volume_profile_evidence_2026_10_03.py
- tools/audit_project_integrity.py
- tools/audit_runtime_ui.py
- tools/audit_signal_measurement.py
- tools/audit_startup_persistence.py
- tools/benchmark_target_obstacle_cache.py
- tools/verify_architecture.py

## 8. Full potential-problem / defect checklist

### 1. A — Architecture / Single Owner / Source of Truth

- [ ] 1.1 Identify every concept that has more than one producer, calculator, formatter, renderer, state store or executor.
- [ ] 1.2 Prove one owner for direction, strength, confidence, quality, actionability and signal stage.
- [ ] 1.3 Prove one owner for Entry, Trigger, requested Entry and actual Fill; reject semantic conflation.
- [ ] 1.4 Prove one owner for SL geometry and one owner for broker-confirmed SL state.
- [ ] 1.5 Prove one owner for TP selection and one owner for broker-confirmed TP state.
- [ ] 1.6 Prove one owner for RR calculation and ensure renderers never recompute RR.
- [ ] 1.7 Prove one owner for execution identity and ScenarioId/PlanId/SignalId lineage.
- [ ] 1.8 Prove one owner for lifecycle state and prevent panel-only lifecycle inference.
- [ ] 1.9 Prove one owner for alert event creation, delivery queue, sound selection and sound playback.
- [ ] 1.10 Prove one owner for chart signal rendering and remove competing direct Chart.Draw calls.
- [ ] 1.11 Prove one owner for panel timeframe presentation so lamps and text consume the same state.
- [ ] 1.12 Prove one owner for cBot effective execution state and ensure Indicator is read-only about it.
- [ ] 1.13 Scan for compatibility aliases, dead legacy owners and historical class names in production.
- [ ] 1.14 Scan partial CFIPIndicator files for hidden nested business logic or duplicate platform callbacks.
- [ ] 1.15 Scan for duplicate calculations hidden behind differently named helpers.

### 2. B — Market Data / Price / Time / MTF

- [ ] 2.1 Verify one CanonicalPriceSnapshot per calculation cycle.
- [ ] 2.2 Verify BUY uses executable Ask and SELL uses executable Bid at the correct execution boundary.
- [ ] 2.3 Verify spread, pip size, tick size, digits and broker minimum distances have one source.
- [ ] 2.4 Verify signal reference time and quote observation time are never conflated.
- [ ] 2.5 Verify all confirmed decisions use the intended closed-bar index.
- [ ] 2.6 Verify live/intrabar calculations are explicitly marked and cannot silently feed closed-bar consensus.
- [ ] 2.7 Verify M15 remains canonical trade-decision/execution reference.
- [ ] 2.8 Verify M5 remains trigger/entry-precision tuning and does not become a competing execution clock.
- [ ] 2.9 Verify M1 is confirmation-only and cannot independently create directional consensus.
- [ ] 2.10 Verify H1+ context/target evidence does not silently become a second execution decision.
- [ ] 2.11 Verify all MTF windows map to the same intended reference timestamp.
- [ ] 2.12 Verify startup warm-up cannot produce directional state from incomplete frames.
- [ ] 2.13 Verify new-bar detection cannot double-process a closed bar.
- [ ] 2.14 Verify missing bars/history gaps are handled deterministically.
- [ ] 2.15 Verify broker timezone/session/DST behavior at target terminal.

### 3. C — Primitive Indicators / Numerical Integrity

- [ ] 3.1 Audit ATR formula source, period, seed, warm-up and index alignment.
- [ ] 3.2 Audit ADX/DMI readiness and +DI/-DI symmetry.
- [ ] 3.3 Audit EMA source and slope/spread semantics.
- [ ] 3.4 Audit RSI readiness and boundary behavior.
- [ ] 3.5 Audit MACD terminology: line, signal and histogram must not be mislabeled.
- [ ] 3.6 Audit Choppiness full-window requirement.
- [ ] 3.7 Audit Range Efficiency interval count and period semantics.
- [ ] 3.8 Audit VWAP window and zero-volume semantics.
- [ ] 3.9 Audit Volume Expansion range semantics and zero/flat bars.
- [ ] 3.10 Audit all Skender adapters against bounded/full-prefix references.
- [ ] 3.11 Audit Skender warm-up windows and cache invalidation.
- [ ] 3.12 Audit all numerical outputs for NaN, Infinity, zero and negative invalid states.
- [ ] 3.13 Audit division-by-zero and near-zero denominator handling.
- [ ] 3.14 Audit BUY/SELL mirror symmetry wherever mathematically expected.
- [ ] 3.15 Audit every normalization/clamp for a named policy owner.
- [ ] 3.16 Inventory every hard-coded threshold and determine whether it is policy, geometry, safety or UI.
- [ ] 3.17 Detect hidden fallback values that silently alter analytical meaning.
- [ ] 3.18 Ensure no standard indicator has a competing production implementation.
- [ ] 3.19 Ensure numerical caches cannot return stale values after history replacement.
- [ ] 3.20 Measure numerical error rather than assuming parity.

### 4. D — Structure / Zones / Liquidity / Evidence

- [ ] 4.1 Audit swing-point confirmation and plateau semantics.
- [ ] 4.2 Audit MSS/CHOCH freshness and repeated-break suppression.
- [ ] 4.3 Audit structural event deduplication.
- [ ] 4.4 Audit equal-level detection and tolerance semantics.
- [ ] 4.5 Audit liquidity-pool creation, invalidation and sweep freshness.
- [ ] 4.6 Audit FVG geometry source and exact gap definition.
- [ ] 4.7 Audit FVG age, mitigation and full-fill invalidation semantics.
- [ ] 4.8 Audit FVG source timeframe propagation.
- [ ] 4.9 Audit Order Block detection, displacement evidence and invalidation.
- [ ] 4.10 Audit OB mitigation guard and re-use prevention.
- [ ] 4.11 Audit OB quality and confluence scoring.
- [ ] 4.12 Audit OB+FVG combined evidence for accidental double counting.
- [ ] 4.13 Audit zone overlap and nearest-zone selection.
- [ ] 4.14 Audit premium/discount boundaries and midpoint semantics.
- [ ] 4.15 Audit target obstacle detection and source provenance.
- [ ] 4.16 Audit structural stop source provenance.
- [ ] 4.17 Audit divergence regular/hidden semantics and direction symmetry.
- [ ] 4.18 Audit WaveTrend formula, readiness and evidence ownership.
- [ ] 4.19 Audit volume-profile evidence and ensure it is not duplicated elsewhere.
- [ ] 4.20 Audit correlated evidence groups so one phenomenon is not counted multiple times.

### 5. E — Decision / Scoring / Confidence / Regime

- [ ] 5.1 Trace every raw measurement to normalized evidence and final decision contribution.
- [ ] 5.2 Verify directional evidence is collected independently for BUY and SELL before consensus.
- [ ] 5.3 Audit score normalization and score bounds.
- [ ] 5.4 Audit confidence calculation and calibration inputs.
- [ ] 5.5 Audit quality calculation and calibration inputs.
- [ ] 5.6 Audit consensus calculation for correlated evidence.
- [ ] 5.7 Audit regime classification and transition stability.
- [ ] 5.8 Audit range-market handling for weak/false signals.
- [ ] 5.9 Audit trend/range/compression/expansion/high-volatility distinctions.
- [ ] 5.10 Audit higher-timeframe penalties and rewards.
- [ ] 5.11 Audit top-down calibration for direction, not duplicate execution authority.
- [ ] 5.12 Audit all decision gates for overlapping semantics.
- [ ] 5.13 Separate Setup/Watch, Confirmed, TriggerReady and ActionableNow semantics.
- [ ] 5.14 Verify blocked/restricted states cannot leak into actionable signals.
- [ ] 5.15 Verify no renderer/panel reinterprets raw score into a second strength.
- [ ] 5.16 Verify strong/medium/weak tiers are derived from canonical evidence, not arbitrary visual state.
- [ ] 5.17 Audit adaptive thresholds for hidden interaction and double application.
- [ ] 5.18 Audit all confidence floors against intended scenario lane.
- [ ] 5.19 Audit negative/neutral direction behavior.
- [ ] 5.20 Replay deterministic cases for BUY/SELL/range/flat/high-volatility.

### 6. F — Planning / Entry / Trigger / SL / TP / RR

- [ ] 6.1 Audit execution-zone selection.
- [ ] 6.2 Audit ideal-entry selection versus executable market price.
- [ ] 6.3 Audit trigger lifecycle and M5/M1 synchronization.
- [ ] 6.4 Audit late-entry/extension handling.
- [ ] 6.5 Audit entry invalidation and expiry.
- [ ] 6.6 Audit structural stop ceiling/floor and side correctness.
- [ ] 6.7 Audit TP1–TP4 source ladder and obstacle handling.
- [ ] 6.8 Audit TP directional correctness for BUY and SELL.
- [ ] 6.9 Audit TP progression so targets can never move backward.
- [ ] 6.10 Audit RR calculation from the same normalized Entry/SL/TP values used downstream.
- [ ] 6.11 Audit fallback targets and prove when fallback is allowed.
- [ ] 6.12 Audit reward-distance minimums and stagnant-market protection.
- [ ] 6.13 Audit target source timeframe/provenance.
- [ ] 6.14 Audit broker minimum-distance normalization without changing strategy meaning.
- [ ] 6.15 Audit plan materialization against canonical candidate geometry.
- [ ] 6.16 Audit Actionability and PlanCreationEligibility for duplicate gates.
- [ ] 6.17 Audit scenario materialization for stale candidates.
- [ ] 6.18 Audit scenario identity propagation.
- [ ] 6.19 Audit market/pending/aggressive intent semantics.
- [ ] 6.20 Verify the exact validated geometry reaches cBot without reconstruction.

### 7. G — Signal / Arrow / Chart Drawing

- [ ] 7.1 Verify SignalVisualSnapshot is the only visual-state source.
- [ ] 7.2 Verify authoritative direction is resolved before strength.
- [ ] 7.3 Verify nine-level strength is one canonical 1–9 ladder: Weak 1–3, Medium 1–3, Strong 1–3.
- [ ] 7.4 Verify arrow renderer consumes canonical strength without recalculation.
- [ ] 7.5 Verify arrows have deterministic ATR-relative and minimum-pip separation.
- [ ] 7.6 Verify M1 is a Circle precision marker, not a competing directional arrow.
- [ ] 7.7 Verify obsolete arrow renderer/strength owners remain absent.
- [ ] 7.8 Verify signal arrows cannot overlap due to identical coordinates.
- [ ] 7.9 Verify stale signal objects are removed when signal expires or direction changes.
- [ ] 7.10 Verify blocked signals produce no visible signal marks.
- [ ] 7.11 Verify plan lines are solid, finite and anchored to the latest candle.
- [ ] 7.12 Verify plan line span is exactly 40 bars where the canonical contract requires it.
- [ ] 7.13 Verify labels are background-free, readable and left of the line with deterministic gap.
- [ ] 7.14 Verify source timeframe appears exactly once in canonical label formatting.
- [ ] 7.15 Verify TP/SL distance text uses the canonical price/pip conversion.
- [ ] 7.16 Verify pending/parallel/prediction surfaces delegate to canonical line/label owners.
- [ ] 7.17 Verify no direct competing Chart.DrawTrendLine/DrawText path remains.
- [ ] 7.18 Verify historical and live rendering cannot create duplicate objects.

### 8. H — Alert / Audio

- [ ] 8.1 Verify one canonical event is created for each alertable semantic event.
- [ ] 8.2 Verify queue acceptance precedes delivery acknowledgement.
- [ ] 8.3 Verify popup and sound consume the same canonical event boundary.
- [ ] 8.4 Verify sound selection has one policy owner.
- [ ] 8.5 Verify sound playback has one production owner per application boundary.
- [ ] 8.6 Verify blocked/restricted signals remain silent.
- [ ] 8.7 Verify cooldown state commits only after successful queue acceptance.
- [ ] 8.8 Verify retry guards cannot duplicate sound/popup.
- [ ] 8.9 Verify primary canonical ScenarioId is not duplicated into the same user-facing alert loop.
- [ ] 8.10 Verify independent simultaneous scenarios remain independently alertable.
- [ ] 8.11 Verify cBot lifecycle sounds cannot overlap Indicator signal-sound ownership.
- [ ] 8.12 Verify sound type mapping cannot diverge between branches.
- [ ] 8.13 Verify audio failures do not create false broker/execution state.
- [ ] 8.14 Verify alert history is bounded and does not block calculation.
- [ ] 8.15 Verify target-terminal audibility remains a manual acceptance boundary.

### 9. I — Panel / UI / Responsiveness

- [ ] 9.1 Verify panel startup is asynchronous and does not block calculation.
- [ ] 9.2 Verify panel content refresh is change-aware.
- [ ] 9.3 Verify lamp and text consume exactly the same PanelTimeframePresentationState.
- [ ] 9.4 Verify unready frames cannot show directional color with zero strength.
- [ ] 9.5 Verify footer geometry does not clip bottom content.
- [ ] 9.6 Verify MTF lamp rail stays outside scrolling content.
- [ ] 9.7 Verify narrow panel widths cannot overflow.
- [ ] 9.8 Verify alert rail renders bounded recent events only.
- [ ] 9.9 Verify header has one realtime owner.
- [ ] 9.10 Verify panel never invents lifecycle/execution state from stale snapshots.
- [ ] 9.11 Verify execution controls are presentation/status-only unless explicitly authorized by the final architecture.
- [ ] 9.12 Verify cBot state freshness is shown separately from chart attachment.
- [ ] 9.13 Verify panel hide/show operations are low-cost and idempotent.
- [ ] 9.14 Verify repeated Calculate calls do not rebuild unchanged UI objects.
- [ ] 9.15 Verify no renderer performs full-history scans on every tick.
- [ ] 9.16 Verify panel row formatting has one semantic owner per concept.
- [ ] 9.17 Verify colors/text/lamp semantics cannot drift.
- [ ] 9.18 Verify UI exceptions cannot terminate the calculation engine.
- [ ] 9.19 Verify chart objects and panel controls are cleaned on stop/reload.
- [ ] 9.20 Verify target-terminal visual acceptance for latency, clipping and freshness.

### 10. J — cBot / Contracts / Execution

- [ ] 10.1 Verify CFIP.Contracts is platform-neutral.
- [ ] 10.2 Verify Indicator and cBot depend on the same canonical contracts.
- [ ] 10.3 Verify cBot cannot access Indicator private state.
- [ ] 10.4 Verify no chart-object scraping, reflection or label parsing is used as transport.
- [ ] 10.5 Verify signal/plan identity survives Indicator→cBot→broker.
- [ ] 10.6 Verify revision ordering is deterministic.
- [ ] 10.7 Verify ScenarioId-scoped idempotency prevents one scenario suppressing another.
- [ ] 10.8 Verify stale/missing/incompatible contracts fail closed.
- [ ] 10.9 Verify Market execution has one broker mutation owner.
- [ ] 10.10 Verify Aggressive execution has one broker mutation owner.
- [ ] 10.11 Verify Pending Stop has one broker mutation owner.
- [ ] 10.12 Verify Pending Limit has one broker mutation owner.
- [ ] 10.13 Verify close/partial-close has one broker mutation owner.
- [ ] 10.14 Verify SL mutation has one broker mutation owner.
- [ ] 10.15 Verify TP mutation/server ladder has one broker mutation owner.
- [ ] 10.16 Verify broker-confirmed facts are never fabricated from accepted requests.
- [ ] 10.17 Verify pending accepted != pending filled.
- [ ] 10.18 Verify broker-created position without valid protection enters recovery-required state.
- [ ] 10.19 Verify broker/account constraints are applied only at cBot execution boundary.
- [ ] 10.20 Verify no Indicator-side hidden/disabled fallback executor remains after final cutover.

### 11. K — Lifecycle / Protection / Risk

- [ ] 11.1 Verify broker state is authoritative after mutation.
- [ ] 11.2 Verify desired plan SL cannot substitute for missing broker SL.
- [ ] 11.3 Verify SL can only move protectively.
- [ ] 11.4 Verify TP can only progress forward.
- [ ] 11.5 Verify protection mutation has correct direction and broker distance checks.
- [ ] 11.6 Verify partial close requires its enabling policy and broker truth.
- [ ] 11.7 Verify emergency close/cancel lifecycle remains available independently.
- [ ] 11.8 Verify management cooldown is deferment, not synthetic broker acknowledgement.
- [ ] 11.9 Verify deferred management commands remain retryable.
- [ ] 11.10 Verify restart/reconnect adopts real broker state.
- [ ] 11.11 Verify recovery does not duplicate execution.
- [ ] 11.12 Verify position/pending identity is deterministic and instance-scoped.
- [ ] 11.13 Verify account-mode gates are explicit and fail closed.
- [ ] 11.14 Verify live arms remain default-off unless deliberately armed by the final architecture.
- [ ] 11.15 Verify risk amount, volume, margin and daily-loss rules have one owner each.
- [ ] 11.16 Verify spread/market-hours/session guards have one owner each.
- [ ] 11.17 Verify execution capacity matches actual advertised semantics.
- [ ] 11.18 Verify capacity cannot silently regress to single-plan or multi-scenario behavior through a second gate.
- [ ] 11.19 Verify risk controls are not weakened by cBot migration.
- [ ] 11.20 Verify no safety-critical management path depends on telemetry timing.

### 12. L — History / Persistence / Calibration

- [ ] 12.1 Verify 90-day history semantics and retention.
- [ ] 12.2 Verify archive path is deterministic on target terminal.
- [ ] 12.3 Verify startup loads history without blocking the hot path.
- [ ] 12.4 Verify buffered persistence flushes safely.
- [ ] 12.5 Verify persistence failures are observable and fail appropriately.
- [ ] 12.6 Verify outcome deduplication uses canonical position identity.
- [ ] 12.7 Verify history records preserve SignalId/ScenarioId/PlanId lineage.
- [ ] 12.8 Verify timestamps distinguish signal/reference/observation/fill/outcome.
- [ ] 12.9 Verify calibration never treats incomplete outcomes as final.
- [ ] 12.10 Verify calibration is separated from live decision authority.
- [ ] 12.11 Verify history cannot silently change active strategy thresholds.
- [ ] 12.12 Verify portable/local memory stores have one ownership path.
- [ ] 12.13 Verify restart recovery does not duplicate history entries.
- [ ] 12.14 Verify archive compaction/retention is bounded.
- [ ] 12.15 Verify persistence has no synchronous hot-path file writes.

### 13. M — Performance / Memory / Hot Path

- [ ] 13.1 Measure Calculate latency by stage.
- [ ] 13.2 Measure startup/warm-up latency.
- [ ] 13.3 Measure first-panel render latency.
- [ ] 13.4 Measure per-tick allocation.
- [ ] 13.5 Measure closed-bar recalculation cost.
- [ ] 13.6 Measure intrabar/live refresh cost.
- [ ] 13.7 Measure MTF cache hit/miss behavior.
- [ ] 13.8 Measure zone lookup/cache behavior.
- [ ] 13.9 Measure Skender adapter cache behavior.
- [ ] 13.10 Measure chart-object mutation count.
- [ ] 13.11 Measure panel-control mutation count.
- [ ] 13.12 Measure alert queue pressure.
- [ ] 13.13 Measure persistence queue pressure.
- [ ] 13.14 Measure worst-case multi-scenario processing.
- [ ] 13.15 Detect full-history scans inside tick paths.
- [ ] 13.16 Detect repeated LINQ/materialization allocations in hot paths.
- [ ] 13.17 Detect duplicate calculations between analysis and presentation.
- [ ] 13.18 Detect unbounded collections or logs.
- [ ] 13.19 Detect timer/tick reentrancy.
- [ ] 13.20 Detect stale asynchronous work racing current state.
- [ ] 13.21 Verify cancellation/cleanup on stop/reload.
- [ ] 13.22 Keep optimization architectural: remove work rather than hide it behind gates.

### 14. N — Tests / CI / Build / Documentation

- [ ] 14.1 Run Source/Architecture gate on the exact final commit.
- [ ] 14.2 Run Runtime Acceptance Contracts on the exact final commit.
- [ ] 14.3 Run cTrader Compile/Build on the exact final commit.
- [ ] 14.4 Run all accumulated phase-specific audits.
- [ ] 14.5 Verify audit scripts do not contain stale historical baselines.
- [ ] 14.6 Verify documentation counts match machine-derived current values.
- [ ] 14.7 Verify every completed phase has continuity documentation.
- [ ] 14.8 Verify roadmap has one current authority and historical material is clearly historical.
- [ ] 14.9 Verify no duplicate audit script claims the same semantic owner.
- [ ] 14.10 Verify deterministic replay covers invalid numeric inputs.
- [ ] 14.11 Verify deterministic replay covers BUY/SELL symmetry.
- [ ] 14.12 Verify deterministic replay covers lifecycle/idempotency.
- [ ] 14.13 Verify contract tests cover version mismatch and revision ordering.
- [ ] 14.14 Verify cBot shadow tests cover multi-scenario isolation.
- [ ] 14.15 Verify target-terminal acceptance is explicitly separated from CI claims.
- [ ] 14.16 Verify clean checkout/package restore/build reproducibility.
- [ ] 14.17 Verify warnings are zero in Release build.
- [ ] 14.18 Verify no production C# file exceeds the project file-size rule.
- [ ] 14.19 Verify no production historical-version identifiers remain.
- [ ] 14.20 Verify no dead production source survives behind disabled flags.
- [ ] 14.21 Verify every audit finding has disposition: fixed, accepted invariant, manual test, or scheduled phase.

## 9. Mandatory line-by-line / code-by-code review method

1. Identity — what is the primary artifact in this file?
2. Owner — what single semantic behavior does it own?
3. Inputs — where do all inputs originate?
4. Time — which bar/timestamp/quote does every input represent?
5. Direction — is BUY/SELL behavior symmetric where expected?
6. Math — are units, denominators, normalization and boundaries correct?
7. State — where is state initialized and invalidated?
8. Concurrency — can tick/timer/event paths race or double-run?
9. Dependencies — does it call another owner that already computes the same concept?
10. Outputs — are outputs canonical and consumed without reinterpretation?
11. Side effects — does it mutate broker, chart, panel, sound, files or shared state?
12. Lifecycle — startup/reload/history/new-bar/disconnect/rejection/restart behavior?
13. Failure — do invalid/missing/stale inputs fail closed where required?
14. Performance — is work proportional to new information?
15. Cleanup — are objects, subscriptions, caches and temporary state cleaned?
16. Tests — what deterministic and target-terminal evidence proves it?
17. Documentation — is the final owner/contract recorded?

## 10. Severity model

- P0: safety/authority — duplicate executor, wrong broker state, unsafe protection, identity collision, wrong side.
- P1: correctness — wrong formula/time/bar/Entry/SL/TP/RR or signal/panel/chart contradiction.
- P2: reliability — restart/reconnect, stale state, duplicate events, missed invalidation, persistence corruption.
- P3: performance — hot-path work, allocation, UI churn, startup latency.
- P4: maintainability — duplicate helpers, dead code, stale docs, ownership confusion.

## 11. Evidence required to close an item

- Exact owner file and method.
- Root cause and old behavior.
- Corrected behavior.
- Removed duplicate paths, if any.
- Deterministic test/static audit.
- Source/Architecture result.
- Runtime Acceptance result.
- cTrader Compile/Build result.
- Target-terminal result where required.
- Exact commit/PR.
- Operator action: git pull --ff-only after a main merge.

## 12. Restart order

1. Repository truth + ownership inventory
2. Dead code / duplicate owner elimination
3. Canonical market/price/time verification
4. Full mathematical verification
5. Structure/zone/liquidity verification
6. Decision/evidence/scoring verification
7. Entry/SL/TP/RR verification
8. Signal/arrow/chart verification
9. Alert/audio verification
10. Panel/UI verification
11. Contracts + Indicator/cBot boundary verification
12. cBot execution/lifecycle/risk verification
13. History/persistence/calibration verification
14. Performance/hot-path verification
15. Full integration/replay
16. Target-terminal acceptance
17. Final architecture + build + runtime certification

Rule: one complete phase at a time. No later phase may create a second owner to work around an earlier phase.

## 13. Definition of Done

- Zero unresolved P0/P1 correctness defects.
- No unexplained duplicate semantic owner.
- No duplicate broker executor.
- No duplicate signal visual owner.
- No duplicate alert/audio owner.
- M15/M5/M1 semantics preserved.
- Exact Entry/SL/TP/RR geometry preserved through cBot.
- Broker-confirmed state authoritative.
- Deterministic restart/reconnect/idempotency coverage.
- Source/Architecture PASS.
- Runtime Acceptance PASS.
- cTrader Compile/Build PASS.
- Target-terminal mandatory matrix PASS.
- Current documentation and machine audits agree.

## 14. Starting point

The project already contains substantial hardening. We do not discard it. We re-audit it from the root and preserve verified canonical owners.

First implementation target after this baseline: repository hygiene/dead-code/ownership collision audit, then the calculation chain from Market Data → Decision → Entry/SL/TP/RR.

No tuning, new strategy feature or parallel implementation is authorized merely because an audit item is found.

## 15. Continuity

This file is the master restart index. Existing phase documents remain evidence/history records; they do not override a newer canonical owner.

After merge to main: git pull --ff-only.

Then begin at checklist item 1.1 and record every disposition in this document or in a linked phase record.

### Additional confirmed M2 lifecycle findings — historical baseline before remediation

> **Status:** This numbered list is the pre-remediation evidence snapshot. Items superseded by M2 are intentionally retained as audit history; the M2 remediation checkpoints below are the current disposition and authority.


22. cBot SubscribeBrokerLifecycleEvents() attaches seven anonymous broker/PendingOrder handlers, but OnStop() does not unsubscribe those handlers. This is a lifecycle ownership/leak risk and requires named handler ownership with deterministic unsubscribe.
23. Indicator FinalizeAsyncInitialization() performs multiple event subscriptions inside one try/catch; a failure in a later subscription can leave earlier subscriptions active while initialization continues. Partial-hook rollback is required.
24. Indicator initialization OnTimer() synchronously calls RenderPanel() during data-wait cycles. This can amplify startup latency and panel responsiveness problems; it must be measured and architecturally scheduled under the existing panel owner.
25. Indicator still listens to broker lifecycle events. Their necessity must be proven handler-by-handler because cBot is now the sole broker mutation authority; unnecessary listeners are execution-boundary residue, not an invitation to create another state owner.
26. Indicator OnDestroy() has mixed shutdown ordering: buffered persistence is flushed before timer/event teardown, while outcome/history persistence occurs later. A single explicit shutdown/quiescence contract is required to prevent late callbacks from racing with persisted terminal state.

27. cBot has two realtime signal-consumption clocks: OnTick() and a 100ms OnTimer() both read signal/scenario transport and call ProcessSignalEnvelope(). This is a duplicate execution-consumption path and requires one canonical consumer/queue.
28. cBot also repeats binding/settings/store/reconciliation work across OnTick and OnTimer. This can amplify CPU/IO and produce ordering differences; ownership and scheduling must be centralized rather than patched with more guards.

29. Scenario protection sweep is O(N) broker reconciliation per retained scenario and currently runs from both realtime clocks, multiplying cost and potentially making protection behavior depend on scenario count/frequency.
30. Scenario retention uses dictionary-count cap (32) and removes the first enumerated key without an explicit temporal/expiry contract; the retained scenario set is therefore not proven to be oldest-first.
31. Indicator binding refresh can reset instance state while execution settings have a separate cache rule that returns when settings are non-null. Same-instance configuration changes must be proven to invalidate settings, especially across ChartIndicator Modified events.
32. The cBot timer forces signal-store reload every cycle (~100ms), bypassing the non-forced reload cadence used by OnTick. This is a second transport refresh policy and must be centralized.
33. Scenario recovery in SweepScenarioProtectionStates writes scenario-local data into global fields (`_activeManagedExecutionLabel`, `_lastSignalEnvelope`, `_reconciliation`). This can let one scenario's recovery context leak into another scenario's global execution/presentation state; recovery must remain scenario-scoped until a canonical promotion rule exists.

### M2 remediation checkpoint — realtime and scenario ownership

M2.177/M2.178 were root-corrected by making OnTimer the sole realtime transport/management consumer and removing signal/management consumption plus protection sweeping from OnTick. The timer now uses the canonical non-forced transport reload cadence after startup. M2.183 was root-corrected by keeping scenario recovery reconciliation local rather than copying it into global execution context during scenario sweeping. M2.184 was corrected by replacing the single global realtime revision tracker with per-scenario revision tracking keyed by ScenarioId, with reset on indicator rebinding.

Acceptance remains open pending authoritative CI/build and target-terminal validation; no compile PASS is claimed from static source edits alone.


## M2 continuation evidence — 2026-10-04

The management command path has been root-corrected during M2: request submission now exposes an explicit status contract; queued/pending/confirmed states are distinct; broker-confirmed protection is adopted only from confirmation; request-time LocalStorage write/flush and report reads were removed from the hot path; deferred command persistence uses the existing BufferedPersistenceCoordinator; and management helper names use Request* semantics. The Indicator broker lifecycle observer set was audited and retained because each handler has a required read-only consumer for broker-confirmed lifecycle/protection reconciliation. These changes do not alter the single-owner law or cBot sole-mutation authority. CI/runtime closure remains pending.


### Management terminal-state hardening evidence — 2026-10-04

Expired management reports are now represented separately from broker-confirmed identities (`AlreadyExpired`), with bounded FIFO retention. This prevents terminal expiry from being reinterpreted as broker confirmation and preserves the single request/confirmation truth across the Indicator → Contracts → cBot boundary.


### M2.188 current disposition — panel refresh ownership

The master audit baseline identified synchronous panel rendering during readiness/startup and lifecycle paths as a responsiveness risk. The remaining secondary direct `RenderPanel()` calls were re-audited and removed from calculation-readiness, runtime-fault, closed-bar MTF-wait, and cBot chart-lifecycle paths. Those paths now invalidate the canonical `PanelContentRefresh` owner via `RequestPanelContentRefresh()`. `RenderPanel()` remains the single full-layout renderer, while the runtime heartbeat remains the single normal content-refresh clock. This is an ownership/performance correction, not a strategy or execution change.
