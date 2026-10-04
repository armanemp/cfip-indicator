# CFIP — Complete Forensic Inspection List
## CFIP-LIST.md
### File-by-File / Directory-by-Directory / Line-by-Line Audit Inventory
### Canonical edition: 2026-10-04

> **STATUS: ACTIVE / CANONICAL INSPECTION INVENTORY**
>
> Roles:
> - \`docs/CFIP-ROADMAP.md\` = execution order and phase objectives.
> - \`docs/CFIP_GATE.md\` = acceptance/evidence/defects/PASS-BLOCKED rules.
> - \`docs/CFIP-LIST.md\` = exhaustive inspection inventory and forensic coverage.
>
> These three files form the canonical planning/inspection/acceptance system. Historical documents cannot override them.

# 1. Mission

No critical behavior may escape review because the roadmap was too broad. Every relevant file is audited file-by-file; every logic-bearing file is inspected declaration-by-declaration, method-by-method, branch-by-branch and line-by-line where source-level proof is required.

A file is not "reviewed" merely because it was opened. It is reviewed only when its ownership, inputs, outputs, callers, consumers, state, side effects, failure paths, lifecycle and verification are understood.

# 2. Baseline inventory

| Metric | Baseline |
|---|---:|
| Inventory source tree commit | \`d79a0c5a67537f3c6c9806e5eb7c85c33f7f94ac\` |
| Repository files | 1130 |
| Repository directories | 82 |
| Markdown files | 211 |
| Production projects | CFIP.Indicator / CFIP.Contracts / CFIP.cBot |
| Supported MTF | M1/M5/M15/M30/H1/H4/D1/W1 |
| M2 | FORBIDDEN |
| Decision/reference | M15 |
| Trigger/precision | M5 |
| Optional confirmation | M1 |
| Broker mutation | cBot only |

P0 must refresh this inventory from the actual current Git tree.

# 3. Universal line-by-line inspection protocol

## 3.1 Compilation/API
Inspect usings, namespaces, nullable flow, accessibility, generics, overloads, async/task patterns, cTrader APIs, warnings, unreachable code and conditional compilation.

## 3.2 Ownership
For every class/member determine:
- the semantic concept it owns;
- whether another owner exists;
- whether it is recomputing an authoritative value;
- whether a helper/fallback has become a second owner;
- whether partial-class structure hides business logic.

## 3.3 Inputs/state
For every input trace:
**source → validation → normalization → use → output → lifetime**

Inspect fields, properties, collections, caches, static state, defaults, initialization, reset and invalidation.

## 3.4 Control flow
Inspect every condition, switch, early return, fallback, retry, exception branch, null path, timeout, state transition, loop bound and ordering assumption.

Happy path alone is never evidence.

## 3.5 Time/market
Where applicable verify bar index, open/closed semantics, Bid/Ask, executable side, spread, pip/tick/digits, UTC/DST, observation time, reference time, MTF role, history replacement, gaps and stale state.

## 3.6 Numerical
Inspect divide-by-zero, NaN, Infinity, zero/negative values, overflow/underflow, rounding, normalization, hidden constants, units and floating-point tolerances.

## 3.7 Communication/dependencies
For every public or cross-file member:
- callers;
- consumers;
- event publishers/subscribers;
- serialization/deserialization;
- UI bindings;
- alert bindings;
- execution bindings;
- persistence/recovery bindings.

Trace both:
**producer → transformer → consumer**
and
**consumer → prerequisite → owner**

## 3.8 Identity
Trace SignalId, ScenarioId, ExecutionId, broker identity, lifecycle identity, outcome identity, revision/version and idempotency.

## 3.9 Side effects
Locate broker calls, chart objects, UI mutations, sounds, popups, email, file I/O, persistence, queues and network/cloud calls. Each side effect needs one owner and one causal path.

## 3.10 Resource/performance
Inspect history loops, allocations, repeated calculations, cache churn, chart churn, timers, UI refresh, queue growth, memory retention, synchronous I/O and startup cost.

## 3.11 Error containment
For every failure identify recoverability, fault owner, retry, blocking behavior, safety impact, stale-state risk, duplicate risk and observability.

## 3.12 Security/configuration
Inspect secrets, paths, access rights, environment assumptions, config precedence, demo/live behavior, parameter migration, packages, licenses and sensitive logging.

## 3.13 Testability
Map each important owner to unit, deterministic contract, negative-path, idempotency, replay, integration, terminal-only or broker-confirmed proof.

# 4. Directory close rule

A directory closes only when:
1. all children are enumerated;
2. each child has domain/role;
3. sibling and cross-directory dependency direction is understood;
4. duplicate concepts are checked;
5. project inclusion is verified;
6. tests/audits are mapped;
7. defects are entered in CFIP_GATE.

# 5. File status

**UNSEEN → INDEXED → OWNER_TRACED → LINE_AUDITED → DEPENDENCIES_AUDITED → VERIFIED → REGRESSION_LOCKED**

Optional:
**BLOCKED / REOPENED / ARCHIVE_CANDIDATE / DELETE_CANDIDATE**

No file is VERIFIED without owner and dependency trace.

# 6. Atomic work-package rule

A work package must be small enough to complete in one response.

Required properties:
- one coherent owner/domain;
- bounded file scope;
- explicit gate IDs;
- focused verification;
- documentation closeout.

**One response = one complete atomic work package.**

If a proposed package cannot close in one response, split it into smaller packages before implementation.

# 7. Atomic work-package map

| WP | Scope | Primary check |
|---|---|---|
| WP-00 | Rebaseline | current HEAD/tree/CI/parameters/MTF/owners/defects |
| WP-01 | Root/build metadata | solution/props/global/.editorconfig/.gitignore |
| WP-02 | GitHub workflows | triggers/tools/commands/failure/artifacts/duplication |
| WP-03 | Canonical control plane | CFIP-ROADMAP/CFIP_GATE/CFIP-LIST/WORKFLOW/README |
| WP-04 | Historical isolation | old docs and active tooling references |
| WP-05 | Preflight | probe/host/compile/safety boundary |
| WP-06 | Contracts | all CFIP.Contracts models/enums/codecs/identity/bus |
| WP-07 | cBot host | main robot/project boundary |
| WP-08 | cBot binding | Indicator/chart/device binding |
| WP-09 | cBot execution | market/pending/management mutation paths |
| WP-10 | cBot risk | preflight/risk/margin/capacity |
| WP-11 | cBot recovery/shadow | recovery/reconciliation/shadow |
| WP-12 | Indicator host | host/state/project boundary |
| WP-13 | Parameters | declaration→owner→consumer→effect |
| WP-14 | Core enums/execution | enums/submission gate/execution primitives |
| WP-15 | Core math/time/runtime | pure formulas/time rules/runtime primitives |
| WP-16 | Core models | models/contracts between domains |
| WP-17 | Native indicators | native indicator ownership/registry |
| WP-18 | OSS indicators | adapters/cache/warmup/parity |
| WP-19 | Market context | frame/state/regime/cache/timeframe |
| WP-20 | Decision | direction/score/quality/confidence/actionability |
| WP-21 | Reaction | intrabar/reaction semantics |
| WP-22 | Structure | swing/structure/liquidity |
| WP-23 | FVG | detection/lifecycle/mitigation/quality |
| WP-24 | Order Blocks | detection/quality/confluence/mitigation |
| WP-25 | Opportunities | parallel scenarios/enrichment |
| WP-26 | Planning entry | entry source/timing |
| WP-27 | Planning execution | execution geometry |
| WP-28 | Planning filters | planning filters/gates |
| WP-29 | Trade plan | entry/SL/TP/RR/reward path |
| WP-30 | Runtime calculation | Calculate/stage isolation |
| WP-31 | Runtime cBot bridge | read-only provider/contract bridge |
| WP-32 | Initialization | startup/readiness |
| WP-33 | MTF runtime | frame loading/closed context |
| WP-34 | Provider | provider state/freshness |
| WP-35 | Supervision | heartbeat/fault/safety supervision |
| WP-36 | Alerts | events/queue/delivery |
| WP-37 | Trading execution | Indicator-side execution semantics; zero broker mutation |
| WP-38 | Identity | signal/scenario/execution identities |
| WP-39 | Intelligence | outcomes/prediction/calibration telemetry |
| WP-40 | Lifecycle | broker lifecycle handlers |
| WP-41 | Live management | protection/targets/exits |
| WP-42 | Pending | pending policies/preparation/submission |
| WP-43 | Risk | sizing/margin/daily/session/suitability |
| WP-44 | Validation | actionability/target/protection validators |
| WP-45 | Chart | visual snapshot/arrows/markers/lines/labels/cleanup |
| WP-46 | Controls | UI controls/synchronizers |
| WP-47 | Historical UI | historical render/state |
| WP-48 | Panel core | panel state/factory/layout/refresh |
| WP-49 | Panel rows | every panel row renderer |
| WP-50 | Panel theme | visual/layout theme |
| WP-51 | OSS/benchmarks | OSS registry/benchmark boundary |
| WP-52 | Contract-test projects | runtime/decision/planning/execution CI contracts |
| WP-53 | cBot shadow tests | shadow host tests |
| WP-54 | Audit foundation | global architecture/project/parameter/MTF audits |
| WP-55 | Phase audit tools | all audit_phase tools in bounded semantic batches |
| WP-56 | Analysis tools | analyze scripts and benchmark helpers |
| WP-57 | Cross-project graph | all references across projects/docs/tools |
| WP-58 | Global duplicate scan | duplicate owners/methods/state/renderers/audio/execution |
| WP-59 | Global M2 guard | all timeframe/M2 surfaces |
| WP-60 | Global broker scan | all broker mutation API usage |
| WP-61 | Parameter graph | parameter declaration/consumer/effect/docs |
| WP-62 | Signal parity | decision→visual→panel→alert→execution |
| WP-63 | Lifecycle parity | submission→confirmation→protection→close→cleanup |
| WP-64 | Outcome parity | close→outcome→history→calibration |
| WP-65 | Performance | CPU/allocation/cache/queue/UI/IO |
| WP-66 | Security/config | secrets/access/config/licenses/dependencies |
| WP-67 | Test-gap | owner→proof mapping |
| WP-68 | Documentation migration | remove active dependence on old roadmap |
| WP-69 | Certification inventory | zero UNSEEN, zero unclassified critical owner |

# 8. Work-package completion contract

A package closes only when:
- every scoped file is accounted for;
- logic-bearing files receive required line audit;
- caller/consumer graph is known;
- canonical owner is verified;
- competing paths are removed;
- focused verification passes;
- required global regression scans pass;
- CFIP_GATE is updated;
- CFIP-ROADMAP is updated;
- exactly one next package is identified.

No "small remaining task" may be silently deferred.

# 9. File-level audit record

Each file must be representable as:
**Path / Type / Project-Domain / Owner / Role / Callers / Consumers / Side Effects / Contract Dependencies / Test-Audit Coverage / Lines Inspected / Defects / Gate IDs / Status**

These records may live in machine-generated audit output; they do not need to be duplicated line-by-line inside this document.

# 10. Mandatory repository-wide searches

At global checkpoints search:
- \`[Parameter]\`;
- timeframe declarations/usages;
- M2 / 2-minute;
- broker mutation APIs;
- sound/play APIs;
- popup APIs;
- chart object creation/removal;
- serialization/codecs;
- event subscribe/unsubscribe;
- timers;
- file I/O;
- network/cloud access;
- static mutable state;
- identity construction;
- threshold literals;
- risk/volume;
- entry/SL/TP/RR;
- direction;
- confidence/quality;
- signal renderers;
- panel state;
- persistence;
- lifecycle transitions;
- swallowed exceptions.

# 11. Deep line-audit questions

For every material method:
1. What inputs can be null/missing/stale/wrong-timeframe?
2. What preconditions exist?
3. What calculations/transforms occur?
4. Why does every branch exist?
5. Which outputs are authoritative/derived/diagnostic/presentation?
6. What state mutates and who owns it?
7. What side effects happen?
8. What happens on each dependency failure?
9. What retries and with which identity?
10. Can execution overlap/re-enter?
11. When is the state refreshed/invalidated/destroyed?
12. Who consumes it?
13. What proves it?

# 12. No-unexplained-line rule

For an actively repaired file:
- every changed line has a reason;
- every suspicious retained line is classified;
- duplicated branches are removed or justified;
- semantic constants have owners;
- public members have intentional consumers;
- subscriptions have lifecycle symmetry;
- caches have invalidation semantics;
- mutations have one owner;
- fallbacks have explicit contracts.

# 13. Canonical architecture inspection tree

## Repository
→ root metadata
→ solution/project graph
→ CI
→ documentation/control plane
→ preflight
→ source projects
→ test/contract projects
→ tools/audits
→ OSS/benchmarks

## Source
→ CFIP.Contracts
→ CFIP.cBot
→ CFIP.Indicator

### CFIP.Contracts
→ models
→ enums
→ identity
→ signal
→ scenario
→ execution intent
→ broker reports/state
→ lifecycle
→ management
→ codec/version
→ bus keys/state

### CFIP.cBot
→ host
→ binding
→ execution
→ risk
→ recovery
→ shadow

### CFIP.Indicator
→ host/state
→ parameters
→ Core
→ Analysis
→ Planning
→ Runtime
→ Trading
→ UI

### Core
→ enums
→ execution
→ math
→ models
→ runtime
→ text/time

### Analysis
→ Indicators
→ External/OSS
→ Market
→ Reaction
→ Structure
→ Zones
→ evidence/decision/regime
→ opportunity/scenario enrichment

### Planning
→ Entry
→ Execution
→ Filters
→ TradePlan

### Runtime
→ Calculation
→ Cbot
→ Initialization
→ Mtf
→ Provider
→ Supervision

### Trading
→ Alerts
→ Execution
→ Identity
→ Intelligence
→ Lifecycle
→ LiveManagement
→ Pending
→ Risk
→ Validation

### UI
→ Chart
→ Controls
→ Historical
→ Panel
→ Rows
→ Theme

## Verification
→ runtime contracts
→ decision/planning/execution contracts
→ shadow tests
→ architecture audits
→ parameter audits
→ MTF audits
→ cBot boundary audits
→ performance audits
→ benchmark/replay tools

# 14. Historical artifacts

Old M0–M45, CR/CI numbering, former cBot sequencing, hotfix sequencing and superseded visual contracts are evidence/archive only. They do not define current work order.

# 15. Exact repository file inventory

## Root
- [ ] `.editorconfig`
- [ ] `.gitignore`
- [ ] `CFIP.Indicator.sln`
- [ ] `Directory.Build.props`
- [ ] `README.md`
- [ ] `ROUTINE.md`
- [ ] `global.json`

## .github
- [ ] `.github/workflows/ci-build.yml`
- [ ] `.github/workflows/oss-benchmark.yml`
- [ ] `.github/workflows/runtime-acceptance.yml`
- [ ] `.github/workflows/source-check.yml`

## docs
- [ ] `docs/ACCEPTANCE-MATRIX.md`
- [ ] `docs/ARCHITECTURE.md`
- [ ] `docs/CBOT-0-BOUNDARY-INVENTORY.md`
- [ ] `docs/CBOT-P0-EXECUTION-DEPENDENCY-CLOSURE.md`
- [ ] `docs/CBOT-PREFLIGHT.md`
- [ ] `docs/CBOT-SEPARATION-ROADMAP.md`
- [ ] `docs/CFIP-ROADMAP.md`
- [ ] `docs/CFIP_GATE.md`
- [ ] `docs/CLAUDE-REVIEW-CR0-AUDIT.md`
- [ ] `docs/CLAUDE-REVIEW-CR1.7.md`
- [ ] `docs/CLAUDE-REVIEW-CR1.8.md`
- [ ] `docs/CLAUDE-REVIEW-CR3.2-DECISION-PREDICTION.md`
- [ ] `docs/CLAUDE-REVIEW-CR3.3-PARTIAL-TP-BE-TRAILING.md`
- [ ] `docs/CLAUDE-REVIEW-CR3.5-CALIBRATION-OUTCOME-REJECTION.md`
- [ ] `docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md`
- [ ] `docs/CONTINUATION-STATE.md`
- [ ] `docs/CR-FINAL-2026-09-30.md`
- [ ] `docs/DEEP-AUDIT-2026-09-29.md`
- [ ] `docs/DEVELOPMENT-LOG.md`
- [ ] `docs/EDITING-GUIDE.md`
- [ ] `docs/ENGINEERING-PRINCIPLES.md`
- [ ] `docs/HOTFIX-CHART-LINES-EXECUTION-STATUS-2026-09-29.md`
- [ ] `docs/HOTFIX-EXECUTION-PRIORITY-STRUCTURAL-LOCK.md`
- [ ] `docs/HOTFIX-PANEL-LABELS-SMART-PROTECTION-2026-09-29.md`
- [ ] `docs/LOCAL-RELEASE-GATE.md`
- [ ] `docs/MARKET-REGIME-SIGNAL-SYNCHRONIZATION.md`
- [ ] `docs/MASTER-FULL-FORENSIC-AUDIT-2026-10-04.md`
- [ ] `docs/OPERATOR-MAINTENANCE-GUIDE.md`
- [ ] `docs/OSS-COMPONENT-REGISTER.md`
- [ ] `docs/PARAMETER-INVENTORY.md`
- [ ] `docs/PERFORMANCE-ARCHITECTURE-2026-09-29.md`
- [ ] `docs/PHASE-10-MTF-SCENARIOS-ROUTINE-LOGGING.md`
- [ ] `docs/PHASE-11-1-AUTO-EXECUTION-HISTORY-PRESENTATION-HARDENING.md`
- [ ] `docs/PHASE-11-2-ANALYSIS-SIGNAL-EXECUTION-HEARTBEAT.md`
- [ ] `docs/PHASE-11-3-EXECUTION-FORENSICS-THRESHOLD-EVIDENCE.md`
- [ ] `docs/PHASE-11-4-PLAN-QUALITY-MULTISCENARIO-AUTO-EXECUTION.md`
- [ ] `docs/PHASE-11-5-SCENARIO-EXECUTION-MATERIALIZATION.md`
- [ ] `docs/PHASE-11-NEWS-FVG-CALIBRATION.md`
- [ ] `docs/PHASE-5-6-RESPONSIVE-PANEL.md`
- [ ] `docs/PHASE-6-1-DECISION-CLOSED-BAR.md`
- [ ] `docs/PHASE-6-2-REACTION-INTRABAR.md`
- [ ] `docs/PHASE-6-3-AGGRESSIVE-ENTRY.md`
- [ ] `docs/PHASE-6-4-COMPACT-PLAN-VISUALS.md`
- [ ] `docs/PHASE-7-1-HIDDEN-CLAMP-AUDIT.md`
- [ ] `docs/PHASE-7-2-DEAD-PARAMETER-AUDIT.md`
- [ ] `docs/PHASE-7-3-SEMANTIC-DUPLICATE-AUDIT.md`
- [ ] `docs/PHASE-7-4-MAXIMUM-OPEN-POSITIONS-SEMANTICS.md`
- [ ] `docs/PHASE-8-1-M1-TRIGGER-CORRECTNESS.md`
- [ ] `docs/PHASE-8-2-SWING-PLATEAU-CORRECTNESS.md`
- [ ] `docs/PHASE-8-3-FVG-MATHEMATICAL-AUDIT.md`
- [ ] `docs/PHASE-8-4-ORDER-BLOCK-MATHEMATICAL-AUDIT.md`
- [ ] `docs/PHASE-8-5-ZONE-CONFLUENCE-TRIGGER-SYNCHRONIZATION.md`
- [ ] `docs/PHASE-9-1-TOPDOWN-EVIDENCE-RUNTIME-RESPONSIVENESS.md`
- [ ] `docs/PHASE-9-10-SMART-AUTO-TRADE-PROTECTION-AUDIT.md`
- [ ] `docs/PHASE-9-11-SIGNAL-LIFECYCLE-QUALITY-ALERTS.md`
- [ ] `docs/PHASE-9-12-OUTCOME-RECOVERY-TELEMETRY-CALIBRATION.md`
- [ ] `docs/PHASE-9-13-PERSISTENT-MEMORY-SAFE-OPTIMIZATION.md`
- [ ] `docs/PHASE-9-14-SIGNAL-EVIDENCE-CALIBRATION.md`
- [ ] `docs/PHASE-9-15-STARTUP-PERSISTENT-HISTORY.md`
- [ ] `docs/PHASE-9-16-SIGNAL-MEASUREMENT-LOCATION-FUSION.md`
- [ ] `docs/PHASE-9-17-EXIT-GEOMETRY-PROGRESSION.md`
- [ ] `docs/PHASE-9-2-PARALLEL-OPPORTUNITIES-WAVETREND-VISUAL-LANES.md`
- [ ] `docs/PHASE-9-3-EMPIRICAL-CALIBRATION.md`
- [ ] `docs/PHASE-9-4-ACTIONABILITY-DIVERGENCE-PLAN-REGISTRY.md`
- [ ] `docs/PHASE-9-6-SIGNAL-QUALITY-VISUAL-COHERENCE.md`
- [ ] `docs/PHASE-9-7-REGIME-AUTO-EXECUTION-HARDENING.md`
- [ ] `docs/PHASE-9-8-INDICATOR-FUSION-TRADE-QUALITY.md`
- [ ] `docs/PHASE-9-9-SIGNAL-PROTECTION-COHERENCE.md`
- [ ] `docs/PHASE-BUILD-WARNING-PANEL-HEIGHT-INTEGRITY-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-6M-TRADE-QUALITY-HARDENING-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-ATTACHMENT-AUDIO-HARDENING-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-BROKER-CONFIRMED-FACTS-2026-10-03.md`
- [ ] `docs/PHASE-CBOT-DEMO-LIVE-MARKET-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-EFFECTIVE-LIFECYCLE-STATE-2026-10-03.md`
- [ ] `docs/PHASE-CBOT-LIFECYCLE-AUDIO-PANEL-HEADER-2026-10-03.md`
- [ ] `docs/PHASE-CBOT-LOCAL-CLOUD-LIFECYCLE-2026-10-03.md`
- [ ] `docs/PHASE-CBOT-MANAGEMENT-POLICY-HARDENING-2026-10-03.md`
- [ ] `docs/PHASE-CBOT-P0-ACTIVATION.md`
- [ ] `docs/PHASE-CBOT-P1-PLATFORM-NEUTRAL-CONTRACTS.md`
- [ ] `docs/PHASE-CBOT-P2-READ-ONLY-INDICATOR-PROVIDER.md`
- [ ] `docs/PHASE-CBOT-P3-CBOT-HOST-SHADOW.md`
- [ ] `docs/PHASE-CBOT-P4A-MARKET-RANGE-AUTHORITY-CUTOVER-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-P4B-AGGRESSIVE-PANEL-GEOMETRY-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-P4C-PENDING-STOP-HOST-INDEPENDENCE-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-P4D-PENDING-LIMIT-SIGNAL-POPUP-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-P4E-MANAGEMENT-AUTHORITY-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-P5-RECONCILIATION-PROTECTION-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-P6-ACCOUNT-RISK-CONNECTION-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-P7-UI-STATE-CUTOVER-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-P7R-ATTACHMENT-ALERT-VISIBILITY-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-P8-PROGRESSIVE-PROTECTION-STATE-SYNC-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-P9-UNIFIED-ALERT-RAIL-VISUAL-COHERENCE-2026-10-02.md`
- [ ] `docs/PHASE-CBOT-POSITION-TRUTH-HARDENING-2026-10-03.md`
- [ ] `docs/PHASE-CBOT-SHADOW-MULTISCENARIO-TRUTH-2026-10-03.md`
- [ ] `docs/PHASE-CI-00-CANONICAL-DATA-PRICE-TIME.md`
- [ ] `docs/PHASE-CI-01-PRIMITIVE-INDICATOR-INTEGRITY.md`
- [ ] `docs/PHASE-CI-02-OSS-PARITY-WARMUP-CACHE.md`
- [ ] `docs/PHASE-CI-03-INDICATOR-FUSION-EVIDENCE-INDEPENDENCE.md`
- [ ] `docs/PHASE-CI-04-STRUCTURE-SWING-LIQUIDITY.md`
- [ ] `docs/PHASE-CI-05-FVG-LIFECYCLE.md`
- [ ] `docs/PHASE-CI-06-ORDER-BLOCK-LIFECYCLE.md`
- [ ] `docs/PHASE-CI-07-MTF-REGIME-CONTEXT.md`
- [ ] `docs/PHASE-CI-08-DIVERGENCE-WAVETREND-REACTION-EARLY.md`
- [ ] `docs/PHASE-CI-09-DECISION-MATHEMATICS.md`
- [ ] `docs/PHASE-CI-10-TRIGGER-LIFECYCLE.md`
- [ ] `docs/PHASE-CI-11-ENTRY-GEOMETRY-TIMING.md`
- [ ] `docs/PHASE-CI-12-STRUCTURAL-SL.md`
- [ ] `docs/PHASE-CI-13-TP-SOURCE-OBSTACLE-LADDER.md`
- [ ] `docs/PHASE-CI-14-CANONICAL-RR-PROTECTION.md`
- [ ] `docs/PHASE-CI-15-EXECUTION-GEOMETRY-BROKER-BOUNDARY.md`
- [ ] `docs/PHASE-CI-16-DETERMINISTIC-REPLAY-LATENCY.md`
- [ ] `docs/PHASE-CI-17-TARGET-TERMINAL-VALIDATION.md`
- [ ] `docs/PHASE-CI-17A-PANEL-LIVE-CONTENT-REFRESH.md`
- [ ] `docs/PHASE-CI-18-SIGNAL-PANEL-COHERENCE-2026-10-02.md`
- [ ] `docs/PHASE-CI-19-SIGNAL-TARGET-QUALITY-COHERENCE-2026-10-02.md`
- [ ] `docs/PHASE-CI-20-PANEL-CBOT-ENGINE-COHERENCE-2026-10-02.md`
- [ ] `docs/PHASE-CI-20B-PROTECTION-CBOT-SIGNAL-HARDENING-2026-10-02.md`
- [ ] `docs/PHASE-CI-20C-CBOT-CONNECTION-LIFECYCLE-2026-10-02.md`
- [ ] `docs/PHASE-CI-21-PRIMARY-M15-SIGNAL-VISIBILITY.md`
- [ ] `docs/PHASE-CI-FULL-STACK-CALCULATION-ANALYTICAL-INTEGRITY.md`
- [ ] `docs/PHASE-CR1-3-NEWS-GUARD-CORRECTNESS.md`
- [ ] `docs/PHASE-CR1-9-MINOR-CLEANUP.md`
- [ ] `docs/PHASE-CR2-1-STRUCTURE-SEMANTICS.md`
- [ ] `docs/PHASE-CR2-2-REACTION-INTEGRITY.md`
- [ ] `docs/PHASE-CR2-3-INDICATOR-THRESHOLDS.md`
- [ ] `docs/PHASE-CR2-4-PENDING-ARBITER.md`
- [ ] `docs/PHASE-CR2-5-LIFECYCLE-OUTCOME.md`
- [ ] `docs/PHASE-CR2-6-ORDERBLOCK-QUALITY-CACHE.md`
- [ ] `docs/PHASE-CR2-7-WAVETREND-MATHEMATICAL-CORRECTNESS.md`
- [ ] `docs/PHASE-CR2-8-HISTORICAL-RENDERING.md`
- [ ] `docs/PHASE-CR2-9-STRUCTURAL-DIVERGENCE-REJECTION.md`
- [ ] `docs/PHASE-CR3-1-LIVE-INVALIDATION.md`
- [ ] `docs/PHASE-CR4-1-MEMORY-IDENTITY.md`
- [ ] `docs/PHASE-CR4-10-NATIVE-INDICATOR-SAFETY.md`
- [ ] `docs/PHASE-CR4-2-PERSISTENCE.md`
- [ ] `docs/PHASE-CR4-4-OSS-NUMERICAL-CACHING.md`
- [ ] `docs/PHASE-CR4-5-PER-TIMEFRAME-REGIME.md`
- [ ] `docs/PHASE-CR4-6-FRAME-SCORING-CONSTANTS.md`
- [ ] `docs/PHASE-CR4-7-TP-PIPELINE.md`
- [ ] `docs/PHASE-CR4-8-TP1-DIRECTIONAL-DEFENCE.md`
- [ ] `docs/PHASE-CR4-9-LIVE-REVERSAL-SEMANTICS.md`
- [ ] `docs/PHASE-CR5-1-STRUCTURAL-STOP-CEILING.md`
- [ ] `docs/PHASE-CR5-2-LIQUIDITY-TARGET-SOURCES.md`
- [ ] `docs/PHASE-CR5-3-INDEPENDENT-EVIDENCE-GROUPS.md`
- [ ] `docs/PHASE-CR5-4-PENDING-FILL-ABSOLUTE-RECONCILIATION.md`
- [ ] `docs/PHASE-CR5-5-PARALLEL-SCENARIO-MICROREACTION.md`
- [ ] `docs/PHASE-CR5-6-DIRECTIONAL-BIAS-TIMEFRAME.md`
- [ ] `docs/PHASE-CR5-7-WATCH-REACTION-ALERTS.md`
- [ ] `docs/PHASE-CR5-8-TARGET-SELECTION-CONSISTENCY.md`
- [ ] `docs/PHASE-CR6-1-F1-OPPOSING-ZONE-TARGET-PATH.md`
- [ ] `docs/PHASE-CR6-2-F2-AGGRESSIVE-RISK-FILL.md`
- [ ] `docs/PHASE-CR6-3-F4-THRESHOLD-TRANSPARENCY.md`
- [ ] `docs/PHASE-CR6-4-SMART-THRESHOLD-REGIME.md`
- [ ] `docs/PHASE-CR6-5-F6-TRAP-ACTIONABILITY.md`
- [ ] `docs/PHASE-CR6-6-F7-TIMEFRAME-SCENARIO-SEMANTICS.md`
- [ ] `docs/PHASE-CR6-7-F8-TARGET-OBSTACLE-TELEMETRY.md`
- [ ] `docs/PHASE-CR6-8-F9-TARGET-OBSTACLE-CACHE.md`
- [ ] `docs/PHASE-CR6-9-F3-ORPHAN-MANAGED-POSITION-PROTECTION.md`
- [ ] `docs/PHASE-CR7-1-G1-BROKER-PROTECTION-NO-RISK-EXPANSION.md`
- [ ] `docs/PHASE-CR7-1-G1-BROKER-PROTECTION-RISK-NON-EXPANSION.md`
- [ ] `docs/PHASE-CR7-2-G2-RETEST-ADVERSE-MOMENTUM.md`
- [ ] `docs/PHASE-CR7-3-G3-PLAN-LINE-THICKNESS.md`
- [ ] `docs/PHASE-CR7-4-G4-PANEL-EXECUTION-PROTECTION-STATE.md`
- [ ] `docs/PHASE-CR7-5-G5-PANEL-STATE-FRESHNESS.md`
- [ ] `docs/PHASE-CR7-6A-G6A-EXECUTION-PANEL-PRESENTATION-FRESHNESS.md`
- [ ] `docs/PHASE-CR7-6B-G6B-EXECUTION-CONTROL-TRUTH.md`
- [ ] `docs/PHASE-CR8-1-H1-DIRECTIONAL-FILL-ACCEPTANCE.md`
- [ ] `docs/PHASE-CR8-2-H2-TOPDOWN-ABSOLUTE-STRENGTH.md`
- [ ] `docs/PHASE-CR8-3A-H3-A-SKENDER-SETTINGS.md`
- [ ] `docs/PHASE-CR8-3B-H3-B-SKENDER-WARMUP-CACHE-PARITY.md`
- [ ] `docs/PHASE-F1-REPOSITORY-BUILD-DEPENDENCY-TRUTH-2026-10-04.md`
- [ ] `docs/PHASE-F2-SINGLE-OWNER-DEAD-CODE-CLOSURE-2026-10-04.md`
- [ ] `docs/PHASE-F3-PRICE-TIME-CLOSEDBAR-MTF-INTEGRITY-2026-10-04.md`
- [ ] `docs/PHASE-FINAL-REALTIME-LIVE-SMART-SYSTEM-2026-10-03.md`
- [ ] `docs/PHASE-FOOTER-ALERT-POPUP-HARDENING-2026-10-03.md`
- [ ] `docs/PHASE-INDICATOR-NAME-CBOT-LAUNCH-MTF-PANEL.md`
- [ ] `docs/PHASE-M1-FULL-FORENSIC-AUDIT.md`
- [ ] `docs/PHASE-M3-TRADE-TRUTH-ALERT-CHART-COHERENCE-2026-10-02.md`
- [ ] `docs/PHASE-M4-TIME-SESSION-HISTORY-PERSISTENCE-2026-10-02.md`
- [ ] `docs/PHASE-M5-PANEL-LIVE-RESPONSIVENESS-2026-10-02.md`
- [ ] `docs/PHASE-MODERN-SIGNAL-LINES-SINGLE-OWNER-2026-10-04.md`
- [ ] `docs/PHASE-MTF-EXECUTION-M15-RISK-SPREAD-2026-10-02.md`
- [ ] `docs/PHASE-MTF-P1-PRIMARY-M15-H1-PANEL.md`
- [ ] `docs/PHASE-MTF-P2-PRIMARY-LOCATION-OBFVG.md`
- [ ] `docs/PHASE-MTF-P3-PRIMARY-PROVIDER-IDENTITY.md`
- [ ] `docs/PHASE-NATIVE-CTRADER-SIGNAL-LINE-PRESENTATION-2026-10-04.md`
- [ ] `docs/PHASE-NATIVE-NO-BOX-SIGNAL-LABEL-2026-10-04.md`
- [ ] `docs/PHASE-OPPORTUNITY-MINING-ZONE-SELECTION-2026-10-02.md`
- [ ] `docs/PHASE-PANEL-CLEARANCE-RESTORE-POSITION.md`
- [ ] `docs/PHASE-PANEL-FOOTER-ALERT-DEDUP-2026-10-03.md`
- [ ] `docs/PHASE-PANEL-TIMEFRAME-SINGLE-SOURCE-2026-10-03.md`
- [ ] `docs/PHASE-POSITION-ENGINE-CBOT-TRUTH-HARDENING-2026-10-02.md`
- [ ] `docs/PHASE-REALTIME-LIVE-SIGNAL-UNIFICATION-2026-10-03.md`
- [ ] `docs/PHASE-REALTIME-MULTISCENARIO-OPPORTUNITY-ENGINE-2026-10-03.md`
- [ ] `docs/PHASE-RETEST-TRIGGER-PATH-HARDENING-2026-10-03.md`
- [ ] `docs/PHASE-SEMANTIC-CONSISTENCY-HARDENING-2026-10-03.md`
- [ ] `docs/PHASE-SIGNAL-DRAWING-CANONICAL-2026-10-03.md`
- [ ] `docs/PHASE-SIGNAL-LABEL-COLOR-2PIP-2026-10-04.md`
- [ ] `docs/PHASE-SMART-SEPARATED-SIGNAL-ARROWS-2026-10-04.md`
- [ ] `docs/PHASE-SMART-TREND-ARROWS-RECOVERY-2026-10-04.md`
- [ ] `docs/PHASE-VOLUME-PROFILE-EVIDENCE-2026-10-03.md`
- [ ] `docs/REFERENCE-COVERAGE.md`
- [ ] `docs/REFERENCE-INDICATOR-AUDIT-2026-09-29.md`
- [ ] `docs/ROADMAP.md`
- [ ] `docs/RUNTIME-LOG-GUIDE.md`
- [ ] `docs/RUNTIME-STARTUP-OBSERVABILITY-HOTFIX.md`
- [ ] `docs/SMART-INTELLIGENCE.md`
- [ ] `docs/TRACK-19-OSS-NUMERICAL-BENCHMARK.md`
- [ ] `docs/TRADING-SAFETY-MATRIX.md`
- [ ] `docs/USER-PRIORITY-PLAN.md`
- [ ] `docs/WORKFLOW.md`

## oss
- [ ] `oss/README.md`

## preflight
- [ ] `preflight/CFIP.Preflight.Probe.csproj`
- [ ] `preflight/CFIP.Preflight.csproj`
- [ ] `preflight/CFIPPreflightBot.cs`
- [ ] `preflight/CFIPPreflightProbeIndicator.cs`

## src
- [ ] `src/CFIP.Contracts/AlertEnvelope.cs`
- [ ] `src/CFIP.Contracts/BrokerExecutionReport.cs`
- [ ] `src/CFIP.Contracts/CFIP.Contracts.csproj`
- [ ] `src/CFIP.Contracts/CbotExecutionStateBus.cs`
- [ ] `src/CFIP.Contracts/CbotExecutionStateSnapshot.cs`
- [ ] `src/CFIP.Contracts/CbotIdentity.cs`
- [ ] `src/CFIP.Contracts/ContractBusKeyHash.cs`
- [ ] `src/CFIP.Contracts/ContractEnums.cs`
- [ ] `src/CFIP.Contracts/ContractVersion.cs`
- [ ] `src/CFIP.Contracts/ExecutionIntent.cs`
- [ ] `src/CFIP.Contracts/IdentityContracts.cs`
- [ ] `src/CFIP.Contracts/IndicatorIdentity.cs`
- [ ] `src/CFIP.Contracts/LifecycleEvent.cs`
- [ ] `src/CFIP.Contracts/ManagementBusKey.cs`
- [ ] `src/CFIP.Contracts/ManagementCommand.cs`
- [ ] `src/CFIP.Contracts/MarketExecutionProfile.cs`
- [ ] `src/CFIP.Contracts/PlanSnapshot.cs`
- [ ] `src/CFIP.Contracts/ScenarioExecutionIdentityRule.cs`
- [ ] `src/CFIP.Contracts/SignalBusKey.cs`
- [ ] `src/CFIP.Contracts/SignalEnvelope.cs`
- [ ] `src/CFIP.Contracts/SignalEnvelopeCodec.cs`
- [ ] `src/CFIP.Contracts/SignalScenarioBatch.cs`
- [ ] `src/CFIP.Contracts/SignalScenarioBatchCodec.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/AverageDirectionalIndex.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/AverageTrueRange.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/DirectionalMovementIndex.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/ExponentialMovingAverage.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorConfluenceAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorParameters.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorSnapshotCache.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteCacheEntry.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteSeriesCache.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/SkenderAroon.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/SkenderBollingerBands.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/SkenderCci.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/SkenderMacd.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/SkenderMfi.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/SkenderObv.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/SkenderParabolicSar.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/SkenderRsi.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/SkenderStoch.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/External/SkenderSuperTrend.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/MacdIndicator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/Native/Native.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/NativeIndicatorRegistry.cs`
- [ ] `src/CFIP.Indicator/Analysis/Indicators/RelativeStrengthIndex.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/ChoppinessIndexAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/ConfidenceCalibrationCollector.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionConfidenceCalculator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionConfirmationGates.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionConsensusCalculator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionConsensusSnapshot.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvaluator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvidenceSnapshot.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionFilterResult.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionFilters.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionFrameContribution.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionFrameContributionCalculator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputBuildRequest.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshot.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionLifecycleGates.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionMarketGates.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionQualityCalculator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionReasonBuilder.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionReasonFormatter.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionRestrictionAlertPolicy.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreCalculator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreInput.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreSnapshot.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartConsensusFilterEvaluator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartConsensusFilterInput.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartGates.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionStructureGates.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionTacticalOpportunityAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionThresholdFilterEvaluator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DecisionThresholdFilterInput.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/DirectionAcceptanceGate.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/EmpiricalConfidenceCalibrator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/FrameDecisionContributionAdapter.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/HigherTimeframePenaltyCalculator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/IndependentEvidenceAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/StructuralConfirmationAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/TimeframeAgreementAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Decision/TopDownCalibrationAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/DivergenceAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/FuturePendingOpportunityRuntime.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/HealthyVolatilityAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/LiveBiasAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/M5RegimeCoreCache.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/MacdBiasAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/MarketFrameAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/MarketFrameEvidence.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/MarketFrameScoring.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/MarketRegimeAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/MarketRegimeBarFingerprint.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/MarketRegimeFrameCache.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/MarketRegimeFrameCacheEntry.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/MarketStateSnapshotBuilder.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Math/IndexMath.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/Models/Frame.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/ParallelScenarioComputation.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/PremiumDiscountAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/RangeEfficiencyAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/RangeSignalQualityEvaluator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/ScenarioEvidenceEnrichment.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/VolumeExpansionAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/VolumeProfileAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/VwapBiasAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/WaveTrendEngine.cs`
- [ ] `src/CFIP.Indicator/Analysis/Market/WaveTrendEvidenceAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Reaction/ReactionAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/EqualLevelAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/LiquiditySweepAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/StructureAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/SwingPointAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/FvgDetectionAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/FvgLifecycleAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/FvgMitigationEvaluator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/FvgZoneQualityCalculator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockCandidateBuilder.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockConfluenceAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockEvidenceBuilder.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockMitigationGuard.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockQualityCalculator.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/ZoneLookup.cs`
- [ ] `src/CFIP.Indicator/Analysis/Structure/Zones/ZoneLookupHotCache.cs`
- [ ] `src/CFIP.Indicator/CFIP.Indicator.csproj`
- [ ] `src/CFIP.Indicator/Core/Enums/DecisionPolicyMode.cs`
- [ ] `src/CFIP.Indicator/Core/Enums/ExecutionIntentKind.cs`
- [ ] `src/CFIP.Indicator/Core/Enums/ExecutionMode.cs`
- [ ] `src/CFIP.Indicator/Core/Enums/ExecutionSubmissionPath.cs`
- [ ] `src/CFIP.Indicator/Core/Enums/LifecycleState.cs`
- [ ] `src/CFIP.Indicator/Core/Enums/OpportunityLane.cs`
- [ ] `src/CFIP.Indicator/Core/Enums/PanelCorner.cs`
- [ ] `src/CFIP.Indicator/Core/Enums/PendingOrderMode.cs`
- [ ] `src/CFIP.Indicator/Core/Enums/SizingMode.cs`
- [ ] `src/CFIP.Indicator/Core/Enums/TargetStage.cs`
- [ ] `src/CFIP.Indicator/Core/Execution/AggressiveEntryPolicy.cs`
- [ ] `src/CFIP.Indicator/Core/Execution/SubmissionAttemptIdentity.cs`
- [ ] `src/CFIP.Indicator/Core/Execution/SubmissionGate.cs`
- [ ] `src/CFIP.Indicator/Core/Execution/SubmissionGateState.cs`
- [ ] `src/CFIP.Indicator/Core/GlobalUsings.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ActionabilityThresholdPolicy.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ActionableSignalQualityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/BrokerStateRefreshRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/CalculationReadinessRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/CanonicalTimeRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ChoppinessIndexRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ClosedBarReferenceRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/DailyLossBaselineRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/DailyLossEvaluation.cs`
- [ ] `src/CFIP.Indicator/Core/Math/DailyLossRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/DivergenceThresholdRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/DmiBiasRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/EarlyPredictionScoreRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/EconomicNewsCurrencyRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/EconomicNewsFeedStateRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/EntryActionabilityPolicy.cs`
- [ ] `src/CFIP.Indicator/Core/Math/EntryGeometryRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/EntrySignalTimingRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/EntryTrapRiskPolicy.cs`
- [ ] `src/CFIP.Indicator/Core/Math/EntryTrapRiskRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ExecutionCapacityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ExecutionControlPresentationRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ExecutionFillAcceptanceRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ExecutionIntentGeometryRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ExecutionPanelPresentationIdentityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ExecutionPlanGeometryRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ExecutionProtectionPanelStateRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ExecutionThresholdPolicy.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ExecutionTimeframePolicy.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ExecutionZoneSelectionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/FalseSignalAdverseRRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/FrameRegimeResolutionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/FrameScoringConstants.cs`
- [ ] `src/CFIP.Indicator/Core/Math/FvgLifecycleRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/FvgQualityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/FvgRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/HealthyVolatilityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/HistoricalOutcomeAggregationRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/HistoricalRenderingRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/IndependentEvidenceDiversityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/IndependentEvidenceFusionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/IndicatorActionabilityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/IndicatorEvidenceFusionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/IndicatorEvidenceIndependenceRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/IndicatorExecutionQualityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/IntelligentProtectionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/LiquiditySweepRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/LiquidityTargetCandidateRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/LiveExitGeometryRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/LiveInvalidationRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/LiveM5BiasRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/LivePlanRecoveryRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/LiveReversalDecisionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/LiveReversalEpisodeRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/LocationEvidenceRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/M1TriggerRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/MacdBiasRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ManagedIdentityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ManagedStopProtectionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/MarketRegimeClassifier.cs`
- [ ] `src/CFIP.Indicator/Core/Math/MarketRegimeIdentity.cs`
- [ ] `src/CFIP.Indicator/Core/Math/MarketRegimeTransitionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/MicroReactionSafetyRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/MtfEarlyPredictionFusionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/MtfTrendStrengthRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/NativeIndicatorReadinessRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/NumericGuards.cs`
- [ ] `src/CFIP.Indicator/Core/Math/OpportunityMagnitudeRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/OrderBlockLifecycleRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/OrderBlockQualityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/OrderBlockRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/OrphanManagedProtectionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/OssIndicatorSettings.cs`
- [ ] `src/CFIP.Indicator/Core/Math/OssIndicatorWarmupPolicy.cs`
- [ ] `src/CFIP.Indicator/Core/Math/OssQuoteProjectionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/OssQuoteWindowRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/OutcomeMemoryIdentityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PanelDimensionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PanelFrameDirectionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ParallelScenarioGeometry.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ParallelScenarioSelectionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PartialTakeProfitRetryRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PeakPriceReconstructionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PendingDecisionArbiterRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PendingEntryPriceRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PendingFillExitResolutionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PersistenceHealthRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PlanLinePresentationRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PlanRewardRiskQualityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PremiumDiscountBiasRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PriceProtectionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PrimaryPullbackTuningRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/PrimaryTimeframeSignalRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ProtectionProgressionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ProviderScenarioIdentityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/RangeEfficiencyRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/RangeSignalQualityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ReactionQualificationRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ReactionTimingRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/RegimeAdaptiveRewardFloorRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/RejectionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ReversalProfitThresholdRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/RewardPathGeometryRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/RewardQualityFloorRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/RiskRewardMathRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ServerPartialTakeProfitEvidenceRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/SessionWindowRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/SignalTraceIdentityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/SignalTraceLineageRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/SignalVisualLifecycleRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/SmartBreakEvenRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/SmartThresholdPolicyRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/StructuralEventRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/StructuralEvidenceRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/StructuralStopGeometryRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/StructuralStopRiskRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/StructuralStopScoringRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/StructuralTimeframeRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/SwingPlateauRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TacticalOpportunityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TargetAgeSemanticsRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TargetCandidateConstraintRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TargetCandidateRejectionReasons.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TargetCandidateRewardScoreRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TargetLadderOption.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TargetLadderSelectionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TargetObstacleCachePolicy.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TargetObstacleTelemetryAccumulator.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TargetRewardEnvelopeRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TargetSelectionRequiredRrRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TopDownCalibrationGroupResult.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TopDownCalibrationRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TradeOpportunityQualityRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TriggerLifecycleRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/TriggerThresholdRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/VolumeExpansionRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/VolumeProfileEvidenceRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/VolumeSizingRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/VwapBiasRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/WatchReactionAlertRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/WaveTrendEvidenceRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/WaveTrendMoneyFlowRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/WaveTrendMovingAverageCalculator.Advanced.cs`
- [ ] `src/CFIP.Indicator/Core/Math/WaveTrendMovingAverageCalculator.BaseKernels.cs`
- [ ] `src/CFIP.Indicator/Core/Math/WaveTrendMovingAverageCalculator.cs`
- [ ] `src/CFIP.Indicator/Core/Math/WaveTrendReadinessRule.cs`
- [ ] `src/CFIP.Indicator/Core/Math/ZoneConfluenceRule.cs`
- [ ] `src/CFIP.Indicator/Core/Models/CanonicalPriceSnapshot.cs`
- [ ] `src/CFIP.Indicator/Core/Models/Decision.cs`
- [ ] `src/CFIP.Indicator/Core/Models/DivergenceCandidate.cs`
- [ ] `src/CFIP.Indicator/Core/Models/DivergenceResult.cs`
- [ ] `src/CFIP.Indicator/Core/Models/EntryGeometrySnapshot.cs`
- [ ] `src/CFIP.Indicator/Core/Models/EntrySignalTiming.cs`
- [ ] `src/CFIP.Indicator/Core/Models/ExecutionIntent.cs`
- [ ] `src/CFIP.Indicator/Core/Models/ExecutionModel.cs`
- [ ] `src/CFIP.Indicator/Core/Models/Level.cs`
- [ ] `src/CFIP.Indicator/Core/Models/MarketRegimeClassificationInput.cs`
- [ ] `src/CFIP.Indicator/Core/Models/MarketRegimeSnapshot.cs`
- [ ] `src/CFIP.Indicator/Core/Models/MarketStateFrameSnapshot.cs`
- [ ] `src/CFIP.Indicator/Core/Models/MarketStateSnapshot.cs`
- [ ] `src/CFIP.Indicator/Core/Models/OssIndicatorSnapshot.cs`
- [ ] `src/CFIP.Indicator/Core/Models/Plan.cs`
- [ ] `src/CFIP.Indicator/Core/Models/Prediction.cs`
- [ ] `src/CFIP.Indicator/Core/Models/PredictivePendingCandidate.cs`
- [ ] `src/CFIP.Indicator/Core/Models/SignalEvaluationTrace.cs`
- [ ] `src/CFIP.Indicator/Core/Models/StructuralStopGeometrySnapshot.cs`
- [ ] `src/CFIP.Indicator/Core/Models/TradeActionabilityResult.cs`
- [ ] `src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs`
- [ ] `src/CFIP.Indicator/Core/Models/TradeSetupPreview.cs`
- [ ] `src/CFIP.Indicator/Core/Models/VolumeProfileSnapshot.cs`
- [ ] `src/CFIP.Indicator/Core/Models/WaveTrendSnapshot.cs`
- [ ] `src/CFIP.Indicator/Core/Models/Zone.cs`
- [ ] `src/CFIP.Indicator/Core/Runtime/AlertDelivery.cs`
- [ ] `src/CFIP.Indicator/Core/Runtime/AlertDeliveryQueue.cs`
- [ ] `src/CFIP.Indicator/Core/Text/TextUtilities.cs`
- [ ] `src/CFIP.Indicator/Core/Time/TimeWindowParser.cs`
- [ ] `src/CFIP.Indicator/Indicator/CFIPIndicator.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/01_decision.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/02_mtf.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/03_structure.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/04_zones.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/05_liquidity.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/06_indicators.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/07_entry_precision.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/08_smart_weights.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/09_risk_targets.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/10_live_management.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/11_filters.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/12_alerts_advanced.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/12_alerts_core.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/14_display_advanced.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/14_display_core.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/14_display_panel.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/15_control_advanced.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/15_intelligence_early.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/16_accuracy.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/17_smart_engine.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/20_confluence_extensions.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/21_complete_intelligence.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/22_safety_precision.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/23_structural_execution.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/24_smart_execution.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/25_oss_analytics.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/26_wave_trend.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/27_parallel_opportunities.cs`
- [ ] `src/CFIP.Indicator/Indicator/Parameters/28_news_guard.cs`
- [ ] `src/CFIP.Indicator/Indicator/State.cs`
- [ ] `src/CFIP.Indicator/Planning/Entry/BearTriggerScoreAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Planning/Entry/BullTriggerScoreAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Planning/Entry/ClosedBarTriggerReadyEvaluator.cs`
- [ ] `src/CFIP.Indicator/Planning/Entry/M1TriggerReadyEvaluator.cs`
- [ ] `src/CFIP.Indicator/Planning/Entry/M1TriggerRuntimeUpdater.cs`
- [ ] `src/CFIP.Indicator/Planning/Entry/TriggerRuntimeState.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/ExecutionIntentBuilder.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/ExecutionIntentValidation.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/ExecutionModeResolver.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/ExecutionModelBuilder.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/ExecutionZoneBuilder.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCandidates.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCore.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelector.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/ExecutionZoneQualityEvaluator.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/ExecutionZoneSelectionCandidate.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/MarketEntryValidation.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/PredictivePendingCandidateScorer.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/PredictivePendingLevelSelector.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/PredictivePendingZoneCollector.cs`
- [ ] `src/CFIP.Indicator/Planning/Execution/TriggerGate.cs`
- [ ] `src/CFIP.Indicator/Planning/Filters/RegimeFilter.cs`
- [ ] `src/CFIP.Indicator/Planning/Filters/TradingSessionFilter.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/HtfRewardSourcePolicy.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/HtfSourceClassifier.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/HtfTargetCounter.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/HtfTimeframeClassifier.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/MinimumRequiredRiskRewardCalculator.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/PlanBuilder.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/PlanInputPreparation.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/PlanIntegrityValidator.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/PlanMarketConstraintValidator.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/PlanMaterialization.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/PlanProtectionIntegrityValidator.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/PlanRewardIntegrityValidator.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/PlanTargetPreparation.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/Sources/DailyPivotTargetSource.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/Sources/HtfTargetSource.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/Sources/LiquidityAboveTargetSource.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/Sources/LiquidityBelowTargetSource.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/Sources/M1MicroTargetSource.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/Sources/PreviousPeriodTargetSource.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/Sources/SessionTargetSource.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/Sources/SmartExtraTargetSource.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/Sources/SupplyDemandLiquidityTargetSource.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateSelector.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/StructuralStopPlanner.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetCandidateEvaluator.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetLadderStageCandidateBuilder.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetLevelCandidateMerger.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetLevelMerger.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetMetadataEnricher.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetProgressionRule.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetProgressionValidator.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetSelectionPolicy.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetSelector.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetStageFeasibilityGate.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetStageRejectionTelemetry.cs`
- [ ] `src/CFIP.Indicator/Planning/TradePlan/TargetStageSelector.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CalculationBrokerBoundary.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CalculationLiveCycle.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CalculationMarketContext.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CalculationPreparation.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CalculationReadinessStateStore.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CalculationStartupSeed.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/CanonicalMarketContextBuilder.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/RuntimeFaultBoundary.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/RuntimeFaultState.cs`
- [ ] `src/CFIP.Indicator/Runtime/Calculation/RuntimeFaultStateMachine.cs`
- [ ] `src/CFIP.Indicator/Runtime/Cbot/CbotChartLifecycleEvents.cs`
- [ ] `src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs`
- [ ] `src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs`
- [ ] `src/CFIP.Indicator/Runtime/Initialization/StartupDataHelpers.cs`
- [ ] `src/CFIP.Indicator/Runtime/Mtf/MtfClosedContext.cs`
- [ ] `src/CFIP.Indicator/Runtime/Mtf/MtfClosedContextCache.cs`
- [ ] `src/CFIP.Indicator/Runtime/Mtf/MtfContextBuilder.cs`
- [ ] `src/CFIP.Indicator/Runtime/Provider/CFIPDeviceScenarioBatchPublisher.cs`
- [ ] `src/CFIP.Indicator/Runtime/Provider/CFIPDeviceSignalPublisher.cs`
- [ ] `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProvider.cs`
- [ ] `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderIdentity.cs`
- [ ] `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs`
- [ ] `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderRefresh.cs`
- [ ] `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderScenarioBatch.cs`
- [ ] `src/CFIP.Indicator/Runtime/Supervision/PanelHeartbeatLiveState.cs`
- [ ] `src/CFIP.Indicator/Runtime/Supervision/RuntimePanelHeartbeat.cs`
- [ ] `src/CFIP.Indicator/Runtime/Supervision/RuntimeSafetySupervisor.cs`
- [ ] `src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs`
- [ ] `src/CFIP.Indicator/Trading/Alerts/CanonicalAlertEnvelopeBuilder.cs`
- [ ] `src/CFIP.Indicator/Trading/Alerts/ContextAlertEmitter.cs`
- [ ] `src/CFIP.Indicator/Trading/Alerts/EndOfDayAlert.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveAcceptedFillHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveConfirmationAlert.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveExecutionPreparation.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveFinalExecutionGuard.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressivePreTradeEligibility.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/Aggressive/AggressivePreTradePreparation.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/Aggressive/BoundPlanProtection.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/Aggressive/BrokerProtectionExecution.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/Aggressive/OrphanManagedProtection.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketExecutionPreparation.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketFillReconciliation.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPostFillTargetResolver.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPreTrade.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPreTradeEligibility.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketRangeCalculator.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketSubmissionValidator.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/BrokerConfirmationPolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/BrokerProtectionCoordinator.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/ExecutionPlanPreparation.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/ManagementCommandRequestCoordinator.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/PriceMath.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/ServerSideTakeProfitLadder.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/ServerSideTakeProfitLadderProgression.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/State/AutoTradingStateStore.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/State/LifecycleStateStore.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/State/TargetStageState.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/State/TradeLabelFormatter.cs`
- [ ] `src/CFIP.Indicator/Trading/Execution/SubmissionGateCoordinator.cs`
- [ ] `src/CFIP.Indicator/Trading/Identity/BrokerIdentity.cs`
- [ ] `src/CFIP.Indicator/Trading/Identity/ManagedPositionGuards.cs`
- [ ] `src/CFIP.Indicator/Trading/Identity/TradeExecutionMetadata.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/BufferedArchivePersistence.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/BufferedArchivePersistenceModels.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/BufferedPersistenceCoordinator.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/EconomicNewsCalendarClient.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/EconomicNewsProtection.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/EconomicNewsRiskEvaluator.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/EntryLocationQualityAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/EntrySignalTimingRuntime.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/ExecutionTelemetryRecord.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/FreshTriggerEvidenceAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/HistoricalOutcomeReader.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/NoTradeRegimeAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/OutcomeHistoryArchiveStore.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/OutcomeMemoryAccountSwitch.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/OutcomeMemoryStore.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/OutcomeObservation.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/OutcomePanelTelemetry.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/OutcomeRegistrationResult.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/OutcomeTelemetryEngine.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/OutcomeWindowMonitor.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/PortableMemorySnapshotStore.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/Prediction/EarlyPredictionEngine.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/Prediction/LiveReversalAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/RuntimeLogPersistence.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchivePersistence.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchiveStore.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceRecorder.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/StructuralSequenceAnalyzer.cs`
- [ ] `src/CFIP.Indicator/Trading/Intelligence/TradePlanRegistry.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/ActiveBrokerStopAccessor.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/ActiveBrokerTargetAccessor.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/AutoTradingDisableReminder.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/BrokerProtectionStateEvaluator.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/BrokerProtectionStateSynchronizer.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/BrokerStateSnapshot.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/LifecycleEventIdempotencyGuard.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/LifecycleEventState.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/LifecycleTransitionPolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/LiveFillExitReconciler.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/LiveFillReconciliation.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/LivePlanExitCoordinator.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/LivePlanFactory.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/LivePlanFurtherTargetSelector.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/LivePlanTargetEnrichment.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/ManagedLivePlanRecovery.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/ManagedPositionLookup.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PendingCancelledHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PendingCreatedHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PendingFillPlanBuilder.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PendingFillProtectionCoordinator.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PendingFilledHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PendingModifiedHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PendingOrderCircuitBreaker.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PendingOrderPlanSnapshot.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PositionCircuitBreaker.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PositionClosedHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PositionModifiedHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/Lifecycle/PositionOpenedHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanEvaluation.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanFalseSignalGuard.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanIntegrityHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanLevelExitHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanLiveManagement.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanMarketState.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/ActivePlanReactionExitHandler.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/LiveReversalEpisodeState.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/LiveStructuralPulse.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/LiveTargetCandidateEvaluator.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/PartialTakeProfitExecutor.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/PlanActivation.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/PlanRiskRewardRecalculator.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/ProtectionManager.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/ReversalCloseGuard.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/ReversalProtection.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/SmartExitModeResolver.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/SmartExitPressureCalculator.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/StructuralSetupInvalidationExit.cs`
- [ ] `src/CFIP.Indicator/Trading/LiveManagement/TargetProgression.cs`
- [ ] `src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPlacement.cs`
- [ ] `src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPreparation.cs`
- [ ] `src/CFIP.Indicator/Trading/Pending/Placement/PendingOrderCleanup.cs`
- [ ] `src/CFIP.Indicator/Trading/Pending/Placement/PendingOrderConfirmationReporter.cs`
- [ ] `src/CFIP.Indicator/Trading/Pending/Placement/PendingSubmissionValidator.cs`
- [ ] `src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPlacement.cs`
- [ ] `src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPreparation.cs`
- [ ] `src/CFIP.Indicator/Trading/Pending/Placement/SmartPendingOrderOrchestrator.cs`
- [ ] `src/CFIP.Indicator/Trading/Pending/Policy/PendingOrderPolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/AdaptiveOutcomeRiskPolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/AggressiveRiskPolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/AggressiveVolumeSizer.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/AutoPlanRiskValidator.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/AutoRiskPolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/AutoTradeSafetyGuard.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/AverageAtrCalculator.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/DailyLossAccounting.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/DailyLossGuard.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/DailyLossPersistence.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/ExecutionCapacityGuard.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/ManagedPositionCounter.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/MarginSafetyCalculator.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/MarginUsagePolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/MarketSuitabilityGuard.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/MarketSuitabilityRefreshCoordinator.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/RiskAmountCalculator.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/RiskPercentPolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/SessionWindowEvaluator.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/SuitabilityCalculator.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/SuitabilityRiskMultiplierCalculator.cs`
- [ ] `src/CFIP.Indicator/Trading/Risk/VolumeSizer.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/ActionableSignalQualityGate.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/DecisionBlockReasonPolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/HigherTfRewardPathValidator.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/HtfTargetPresenceValidator.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/LiveExecutionGateReasonPolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/LiveProtectionDistanceResolver.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/PlanCreationEligibility.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/PreTradePlanSynchronizer.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/PriceProtectionValidation.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/RewardPathZoneObstacleScanner.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/SignalPlanCoordinator.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/SmartThresholdPolicy.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/TargetObstacleScanCache.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/TargetObstacleScanSnapshotBuilder.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/TargetObstacleValidator.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/TradeActionabilityDecisionGate.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs`
- [ ] `src/CFIP.Indicator/Trading/Validation/TradeActionabilityRetestContext.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/AlertSignalRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/ChartObjectCleanup.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/MtfTrendStrengthSnapshotBuilder.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/OutcomeMarkerRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/ParallelOpportunityRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PendingOrderRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanLabelFormatting.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanLabelRemover.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanLabelRenderCoordinator.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanLabelRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanLevelVisualState.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanLineRemover.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanLineRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanObjectClearer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanObjectRemover.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PlanRenderCoordinator.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PredictionLabelsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PredictionLineRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PredictionObjectCleanup.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/PredictionRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/SignalPresentationRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/SignalRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/SignalVisualDirectionResolver.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/SignalVisualIdentityBuilder.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/SignalVisualSnapshot.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/SignalVisualSnapshotBuilder.cs`
- [ ] `src/CFIP.Indicator/UI/Chart/SignalVisualSynchronizer.cs`
- [ ] `src/CFIP.Indicator/UI/Controls/ExecutionControlsFactory.cs`
- [ ] `src/CFIP.Indicator/UI/Controls/ExecutionControlsSynchronizer.cs`
- [ ] `src/CFIP.Indicator/UI/Controls/SessionPresentation.cs`
- [ ] `src/CFIP.Indicator/UI/Historical/HistoricalRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Historical/HistoricalSignalPresentation.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelAlertMessageRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelCanonicalSignalStatus.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelConstants.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelContentRefresh.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelExecutionSemantics.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelExecutionState.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelFactory.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelHeaderLiveState.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelHeaderRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelLayoutManager.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelMainRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelPredictionState.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelRenderOptimization.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelRestoreButtonFactory.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelRowWriter.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelRowsFactory.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelSignalState.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelTextFormatting.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelTimeframePresentationState.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelToggleButtonFactory.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelTrendTimeframeLampRow.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/PanelVisibility.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/ProcessingHeartbeatLamp.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelCalibrationRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelContextRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelDecisionRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelExecutionRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewDiagnosticRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewStateRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelSignalPipelineRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelTradePlanLevelRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelTradePlanLiveRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelTradePlanRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Rows/PanelWaveTrendAndOpportunityRowsRenderer.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Theme/PanelActionButtonsLayout.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Theme/PanelQuickExecutionLayout.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Theme/PanelRestoreButtonLayout.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Theme/PanelRowsLayout.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs`
- [ ] `src/CFIP.Indicator/UI/Panel/Theme/PanelVisualSettings.cs`
- [ ] `src/CFIP.cBot/Binding/CfipDeviceSignalTransport.cs`
- [ ] `src/CFIP.cBot/Binding/CfipIndicatorChartBinding.cs`
- [ ] `src/CFIP.cBot/CFIP.cBot.csproj`
- [ ] `src/CFIP.cBot/CFIPExecutionBot.cs`
- [ ] `src/CFIP.cBot/Execution/BrokerExecutionSafety.cs`
- [ ] `src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs`
- [ ] `src/CFIP.cBot/Execution/CbotExecutionIdempotencyStore.cs`
- [ ] `src/CFIP.cBot/Execution/CbotExecutionLifecycleRule.cs`
- [ ] `src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs`
- [ ] `src/CFIP.cBot/Execution/CbotIndicatorExecutionSettings.cs`
- [ ] `src/CFIP.cBot/Execution/CbotLifecycleAudioService.cs`
- [ ] `src/CFIP.cBot/Execution/CbotManagedObjectIdentityRule.cs`
- [ ] `src/CFIP.cBot/Execution/CbotManagementPolicyRule.cs`
- [ ] `src/CFIP.cBot/Execution/CbotSignalPreflight.cs`
- [ ] `src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs`
- [ ] `src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs`
- [ ] `src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs`
- [ ] `src/CFIP.cBot/Recovery/CbotBrokerReconciliation.cs`
- [ ] `src/CFIP.cBot/Risk/CbotDailyLossGuard.cs`
- [ ] `src/CFIP.cBot/Risk/ExecutionMarginBudgetRule.cs`
- [ ] `src/CFIP.cBot/Shadow/ShadowHostContracts.cs`
- [ ] `src/CFIP.cBot/Shadow/ShadowHostCoordinator.cs`
- [ ] `src/CFIP.cBot/Shadow/ShadowHostValidator.cs`

## tools
- [ ] `tools/CFIP.Decision.Contracts/CFIP.Decision.Contracts.csproj`
- [ ] `tools/CFIP.Decision.Contracts/Program.cs`
- [ ] `tools/CFIP.Execution.Contracts/CFIP.Execution.Contracts.csproj`
- [ ] `tools/CFIP.Execution.Contracts/Program.cs`
- [ ] `tools/CFIP.Indicator.CI/CFIP.Indicator.CI.csproj`
- [ ] `tools/CFIP.Planning.Contracts/CFIP.Planning.Contracts.csproj`
- [ ] `tools/CFIP.Planning.Contracts/Program.cs`
- [ ] `tools/CFIP.Runtime.Contracts/AlertDeliveryQueueContracts.cs`
- [ ] `tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj`
- [ ] `tools/CFIP.Runtime.Contracts/Ci20BProtectionAndSignalContracts.cs`
- [ ] `tools/CFIP.Runtime.Contracts/DeterministicReplaySuite.cs`
- [ ] `tools/CFIP.Runtime.Contracts/M4TimeHistoryContracts.cs`
- [ ] `tools/CFIP.Runtime.Contracts/M5PanelContracts.cs`
- [ ] `tools/CFIP.Runtime.Contracts/Program.cs`
- [ ] `tools/CFIP.StockIndicators.Benchmark/Benchmark/BenchmarkFixtures.cs`
- [ ] `tools/CFIP.StockIndicators.Benchmark/Benchmark/BenchmarkModel.cs`
- [ ] `tools/CFIP.StockIndicators.Benchmark/Benchmark/BenchmarkReport.cs`
- [ ] `tools/CFIP.StockIndicators.Benchmark/Benchmark/IndicatorComparison.cs`
- [ ] `tools/CFIP.StockIndicators.Benchmark/Benchmark/QuoteCacheBenchmark.cs`
- [ ] `tools/CFIP.StockIndicators.Benchmark/Benchmark/SkenderWarmupParityBenchmark.cs`
- [ ] `tools/CFIP.StockIndicators.Benchmark/CFIP.StockIndicators.Benchmark.csproj`
- [ ] `tools/CFIP.StockIndicators.Benchmark/D10/NativeRegistryLookupBenchmark.cs`
- [ ] `tools/CFIP.StockIndicators.Benchmark/Program.cs`
- [ ] `tools/CFIP.StockIndicators.Benchmark/README.md`
- [ ] `tools/CFIP.StockIndicators.Benchmark/global.json`
- [ ] `tools/CFIP.cBot.Shadow.Tests/CFIP.cBot.Shadow.Tests.csproj`
- [ ] `tools/CFIP.cBot.Shadow.Tests/Program.cs`
- [ ] `tools/analyze_phase_11_3.py`
- [ ] `tools/analyze_runtime_log.py`
- [ ] `tools/analyze_signal_trace.py`
- [ ] `tools/audit_calculation_cycle.py`
- [ ] `tools/audit_cbot_boundary.py`
- [ ] `tools/audit_cbot_contract_schema.py`
- [ ] `tools/audit_cbot_demo_live_market.py`
- [ ] `tools/audit_cbot_preflight.py`
- [ ] `tools/audit_cbot_project_boundary.py`
- [ ] `tools/audit_cbot_provider_boundary.py`
- [ ] `tools/audit_cbot_shadow_host.py`
- [ ] `tools/audit_execution_capacity_semantics.py`
- [ ] `tools/audit_exit_geometry.py`
- [ ] `tools/audit_hotpath_persistence.py`
- [ ] `tools/audit_mtf_contract.py`
- [ ] `tools/audit_news_guard.py`
- [ ] `tools/audit_optimization_readiness.py`
- [ ] `tools/audit_parameter_count.py`
- [ ] `tools/audit_parameter_semantics.py`
- [ ] `tools/audit_parameters.py`
- [ ] `tools/audit_phase_11_2.py`
- [ ] `tools/audit_phase_11_3.py`
- [ ] `tools/audit_phase_11_4.py`
- [ ] `tools/audit_phase_11_5.py`
- [ ] `tools/audit_phase_11_8.py`
- [ ] `tools/audit_phase_2_1.py`
- [ ] `tools/audit_phase_2_2.py`
- [ ] `tools/audit_phase_2_3.py`
- [ ] `tools/audit_phase_2_4.py`
- [ ] `tools/audit_phase_2_5.py`
- [ ] `tools/audit_phase_2_6.py`
- [ ] `tools/audit_phase_2_7.py`
- [ ] `tools/audit_phase_2_8.py`
- [ ] `tools/audit_phase_2_9.py`
- [ ] `tools/audit_phase_3_1.py`
- [ ] `tools/audit_phase_3_2.py`
- [ ] `tools/audit_phase_3_3.py`
- [ ] `tools/audit_phase_3_4.py`
- [ ] `tools/audit_phase_3_5.py`
- [ ] `tools/audit_phase_4_1.py`
- [ ] `tools/audit_phase_4_10.py`
- [ ] `tools/audit_phase_4_2.py`
- [ ] `tools/audit_phase_4_3.py`
- [ ] `tools/audit_phase_4_4.py`
- [ ] `tools/audit_phase_4_5.py`
- [ ] `tools/audit_phase_4_6.py`
- [ ] `tools/audit_phase_4_7.py`
- [ ] `tools/audit_phase_4_8.py`
- [ ] `tools/audit_phase_4_9.py`
- [ ] `tools/audit_phase_5_1.py`
- [ ] `tools/audit_phase_5_2.py`
- [ ] `tools/audit_phase_5_3.py`
- [ ] `tools/audit_phase_5_4.py`
- [ ] `tools/audit_phase_5_5.py`
- [ ] `tools/audit_phase_5_6.py`
- [ ] `tools/audit_phase_5_7.py`
- [ ] `tools/audit_phase_5_8.py`
- [ ] `tools/audit_phase_6_1.py`
- [ ] `tools/audit_phase_6_2.py`
- [ ] `tools/audit_phase_6_3.py`
- [ ] `tools/audit_phase_6_4.py`
- [ ] `tools/audit_phase_6_5.py`
- [ ] `tools/audit_phase_6_6.py`
- [ ] `tools/audit_phase_6_7.py`
- [ ] `tools/audit_phase_6_8.py`
- [ ] `tools/audit_phase_6_9.py`
- [ ] `tools/audit_phase_7_1.py`
- [ ] `tools/audit_phase_7_2.py`
- [ ] `tools/audit_phase_7_3.py`
- [ ] `tools/audit_phase_7_4.py`
- [ ] `tools/audit_phase_7_5.py`
- [ ] `tools/audit_phase_7_6a.py`
- [ ] `tools/audit_phase_7_6b.py`
- [ ] `tools/audit_phase_8_1.py`
- [ ] `tools/audit_phase_8_2.py`
- [ ] `tools/audit_phase_8_3a.py`
- [ ] `tools/audit_phase_8_3b.py`
- [ ] `tools/audit_phase_accumulation.py`
- [ ] `tools/audit_phase_build_warning_panel_height.py`
- [ ] `tools/audit_phase_cbot_6m.py`
- [ ] `tools/audit_phase_cbot_attachment_audio_2026_10_02.py`
- [ ] `tools/audit_phase_cbot_broker_confirmed_facts_2026_10_03.py`
- [ ] `tools/audit_phase_cbot_lifecycle_audio_2026_10_03.py`
- [ ] `tools/audit_phase_cbot_lifecycle_effective_state_2026_10_03.py`
- [ ] `tools/audit_phase_cbot_local_cloud_lifecycle_2026_10_03.py`
- [ ] `tools/audit_phase_cbot_management_policy_hardening_2026_10_03.py`
- [ ] `tools/audit_phase_cbot_p4b.py`
- [ ] `tools/audit_phase_cbot_p4c.py`
- [ ] `tools/audit_phase_cbot_p4d.py`
- [ ] `tools/audit_phase_cbot_p4e.py`
- [ ] `tools/audit_phase_cbot_p5.py`
- [ ] `tools/audit_phase_cbot_p5_reconciliation.py`
- [ ] `tools/audit_phase_cbot_p6_account_risk_and_connection.py`
- [ ] `tools/audit_phase_cbot_p7_ui_state_cutover.py`
- [ ] `tools/audit_phase_cbot_p7_whole_chain.py`
- [ ] `tools/audit_phase_cbot_p7r_attachment_alert_visibility.py`
- [ ] `tools/audit_phase_cbot_p8_progressive_protection_state_sync.py`
- [ ] `tools/audit_phase_cbot_p9.py`
- [ ] `tools/audit_phase_cbot_position_truth_hardening_2026_10_03.py`
- [ ] `tools/audit_phase_cbot_shadow_multiscenario_truth_2026_10_03.py`
- [ ] `tools/audit_phase_ci20_engine_quality.py`
- [ ] `tools/audit_phase_ci20_panel_options.py`
- [ ] `tools/audit_phase_ci20b_protection_cbot_signal.py`
- [ ] `tools/audit_phase_ci20c_cbot_connection_lifecycle.py`
- [ ] `tools/audit_phase_ci21_primary_signal_visibility.py`
- [ ] `tools/audit_phase_ci_00.py`
- [ ] `tools/audit_phase_ci_01.py`
- [ ] `tools/audit_phase_ci_02.py`
- [ ] `tools/audit_phase_ci_03.py`
- [ ] `tools/audit_phase_ci_04.py`
- [ ] `tools/audit_phase_ci_05.py`
- [ ] `tools/audit_phase_ci_06.py`
- [ ] `tools/audit_phase_ci_07.py`
- [ ] `tools/audit_phase_ci_08.py`
- [ ] `tools/audit_phase_ci_09.py`
- [ ] `tools/audit_phase_ci_10.py`
- [ ] `tools/audit_phase_ci_11.py`
- [ ] `tools/audit_phase_ci_12.py`
- [ ] `tools/audit_phase_ci_13.py`
- [ ] `tools/audit_phase_ci_14.py`
- [ ] `tools/audit_phase_ci_15.py`
- [ ] `tools/audit_phase_ci_16.py`
- [ ] `tools/audit_phase_ci_17.py`
- [ ] `tools/audit_phase_ci_17a.py`
- [ ] `tools/audit_phase_ci_18_signal_panel.py`
- [ ] `tools/audit_phase_ci_19_signal_target_quality.py`
- [ ] `tools/audit_phase_f1_repository_build_dependency_truth.py`
- [ ] `tools/audit_phase_f3_price_time_closedbar_mtf_integrity.py`
- [ ] `tools/audit_phase_final_realtime_live_smart_system_2026_10_03.py`
- [ ] `tools/audit_phase_indicator_name_cbot_launch_mtf_panel.py`
- [ ] `tools/audit_phase_m15_primary_execution_risk_spread.py`
- [ ] `tools/audit_phase_m3_trade_truth.py`
- [ ] `tools/audit_phase_m4_time_history.py`
- [ ] `tools/audit_phase_m5_panel_live.py`
- [ ] `tools/audit_phase_mtf_primary_location_obfvg.py`
- [ ] `tools/audit_phase_mtf_primary_panel.py`
- [ ] `tools/audit_phase_mtf_primary_provider_identity.py`
- [ ] `tools/audit_phase_opportunity_mining_zone_selection_2026_10_02.py`
- [ ] `tools/audit_phase_panel_clearance_restore_position.py`
- [ ] `tools/audit_phase_panel_footer_alert_dedup_2026_10_03.py`
- [ ] `tools/audit_phase_panel_header_realtime_2026_10_03.py`
- [ ] `tools/audit_phase_panel_semantic_consistency_2026_10_03.py`
- [ ] `tools/audit_phase_panel_timeframe_single_source_2026_10_03.py`
- [ ] `tools/audit_phase_position_engine_cbot_truth_hardening_2026_10_02.py`
- [ ] `tools/audit_phase_realtime_live_signal_unification_2026_10_03.py`
- [ ] `tools/audit_phase_realtime_multiscenario_opportunity_engine_2026_10_03.py`
- [ ] `tools/audit_phase_retest_trigger_path.py`
- [ ] `tools/audit_phase_signal_drawing_canonical_2026_10_03.py`
- [ ] `tools/audit_phase_single_owner_duality_2026_10_03.py`
- [ ] `tools/audit_phase_smart_separated_signal_arrows_2026_10_04.py`
- [ ] `tools/audit_phase_user_requirements_2026_10_03.py`
- [ ] `tools/audit_phase_volume_profile_evidence_2026_10_03.py`
- [ ] `tools/audit_project_integrity.py`
- [ ] `tools/audit_runtime_ui.py`
- [ ] `tools/audit_signal_measurement.py`
- [ ] `tools/audit_startup_persistence.py`
- [ ] `tools/benchmark_target_obstacle_cache.py`
- [ ] `tools/verify_architecture.py`

# 16. Certification target

At certification:
- every file indexed;
- every production file owner-mapped;
- every logic-bearing production file line-audited as required;
- critical cross-file dependencies traced;
- critical paths have regression proof;
- all P0/P1 defects closed;
- all three canonical files agree;
- no historical document is active authority.

# 17. Current state

**Inventory source tree:** \`d79a0c5a67537f3c6c9806e5eb7c85c33f7f94ac\`

**Current executable package:** **WP-00 — NEXT**

**Canonical roadmap:** \`docs/CFIP-ROADMAP.md\`

**Canonical gate:** \`docs/CFIP_GATE.md\`

**Canonical inventory:** \`docs/CFIP-LIST.md\`

**Execution rule:** one complete atomic work package per response.


# 18. Inventory integrity

The exact file list above is generated from the repository Git tree. P0 must compare the current tree against this inventory. Any new/deleted/moved file creates an inventory delta that must be accounted for before related work is closed.

Inventory delta is not merely documentation: a new production file without an owner or gate mapping is a P0/P1 defect.

# 19. File-by-file completion rule

For every listed production file, the audit must eventually establish:
- owner;
- responsibility;
- inputs/outputs;
- callers/consumers;
- state;
- side effects;
- error paths;
- lifecycle;
- test/audit proof;
- line inspection status.

For non-production files, classify whether they are active build/test/audit infrastructure, documentation, tooling or archive evidence.

# 20. Current continuation lock

Do not start WP-01 or any later package until WP-00 has established the real current baseline and updated CFIP_GATE and CFIP-ROADMAP accordingly.
