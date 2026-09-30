# CFIP Indicator — Master Implementation, Certification and Release Roadmap

## 0. Purpose

This file is the single master roadmap for the CFIP Indicator project.

It is intentionally self-contained so that development can resume from another
chat, account or assistant without reconstructing the plan from conversation
history.

This document combines:

- the historical modularization work already completed in the repository;
- the current runtime/cTrader acceptance state;
- the unresolved findings from the external/Claude review;
- the current OSS numerical benchmark work;
- the complete certification sequence;
- local production release;
- cloud cBot portability and validation;
- post-stability adaptive learning.

The roadmap is a **plan and continuity contract**. It does not authorize a
production behavior change by itself.

---

# 1. Non-negotiable working rules

## 1.1 One phase per implementation response

One implementation response completes exactly one phase.

A phase is complete only when the affected implementation, caller migration,
static/contract verification, duplicate-path cleanup, available runtime
validation and roadmap status are updated.

A phase must be small enough to finish without leaving a partially applied
architectural change.

## 1.2 Implementation order inside each phase

Always use:

1. Contract / invariant
2. Authoritative implementation
3. Caller migration
4. Tests / static checks
5. Duplicate-path removal
6. Runtime validation where available
7. Documentation

## 1.3 Source ownership

Every behavior has one authoritative owner.

Do not add:

- compatibility aliases;
- duplicate business rules;
- parallel decision authorities;
- alternate broker mutation paths;
- second trading engines;
- historical/versioned production identities.

## 1.4 Production source hygiene

Production C# remains version-neutral.

Historical versions, source snapshots and migration references may appear only
in documentation required for continuity.

Production source must not contain historical class names, numbered strategy
identifiers, release suffixes or versioned trade comments.

## 1.5 Trading safety

The following are permanent invariants:

- broker-confirmed state is authoritative;
- accepted submission is not fill confirmation;
- rejection is not success;
- pending is not a position before confirmed fill;
- SL changes are protective-only;
- partial close and close are consumed only after broker confirmation;
- restart/reconnect reconciliation precedes assumptions about live state;
- automatic market and pending execution share one managed identity;
- there is one decision authority and one automatic execution authority;
- manual BUY/SELL entry controls do not exist.

## 1.6 OSS boundary

OSS libraries are allowed only through an explicit adapter or an isolated
research/benchmark project.

Before production adoption, record:

- upstream source;
- exact version/tag/commit;
- license and attribution;
- target-runtime compatibility;
- dependency implications;
- deterministic fixture results;
- benchmark evidence;
- adapter owner;
- authority boundary.

No OSS trading engine may become a second live CFIP engine.

---

# 2. Current repository baseline

Current branch:

\`main\`

Phase 0.1 verification commit:

\`5d4b6a00fa61dda4c927800b8bd27f6dd496f3cd\`

Current repository state includes:

- modular cTrader Indicator host;
- one managed strategy identity;
- one decision authority;
- one execution authority;
- explicit broker mutation owners;
- modular analysis, planning, risk, lifecycle and UI boundaries;
- deterministic decision/planning/execution/runtime contracts;
- cTrader compile and repository acceptance workflows;
- runtime fault containment;
- staged startup work;
- regime-aware analysis;
- pending Stop and Limit paths;
- automatic market and aggressive paths;
- OSS production/research separation;
- Track 19.1 numerical benchmark completed.

Current production numerical OSS package:

\`Skender.Stock.Indicators 2.7.3\`

Current research-only numerical package:

\`FacioQuo.Stock.Indicators 3.0.1\`

The FacioQuo v3 package remains outside the net6 cTrader production assembly.

Current benchmark milestone already completed:

- Track 19.1 FacioQuo comparison;
- four deterministic market-shape fixtures;
- 800 bars per scenario;
- all 10 production OSS indicator families;
- 12 numerical metrics;
- timestamp and output-count validation;
- warm-up-aware parity;
- max/mean/RMS error;
- finite-value coverage;
- batch timing;
- allocation measurement;
- CI report generation.

The benchmark completion does **not** constitute production package promotion.

Machine-enforced baseline facts for the Phase 0.1 verification commit:

- 398 production C# source files;
- 534 public configuration parameters (531 baseline + 3 OSS extension parameters);
- 27 parameter-group source files;
- 500 method declarations / 467 unique baseline methods.

Automated gate snapshot for the Phase 0.1 verification commit:

- cTrader compile: PASS (workflow run 720);
- runtime acceptance contracts: PASS (workflow run 536);
- source and architecture checks: PASS (workflow run 727).

The original Phase 0.1 baseline reported 398 production C# files, 535 parameters, 500 method declarations and 467 unique baseline methods. The current audited parameter surface is 534.

---

# 2.1 User-priority implementation overlay

Persistent user priorities are recorded in
`docs/USER-PRIORITY-PLAN.md` and are mandatory inputs to future phase planning.

Priority order:
1. responsive/accurate panel startup and live refresh;
2. clean, readable panel and reliable Auto Trading / Auto Orders controls;
3. stronger signal quality without indiscriminate over-filtering;
4. technically strong Entry / SL / TP levels;
5. structurally valid higher RR;
6. safer automatic market/pending execution and rejection handling;
7. smart trailing / profit-lock / target progression for better profit capture;
8. whole-system performance optimization and measurable validation.

The overlay never overrides architectural dependencies. Each item is implemented
under its correct existing owner and is validated through source, runtime, compile,
replay or outcome evidence appropriate to the claim.

# 3. Acceptance hierarchy

CFIP certification is performed in this order:

\`\`\`
Repository truth
    ↓
Architecture / static gates
    ↓
Contract tests
    ↓
Claude review-remediation gate (CR-0 → CR-FINAL)
    ↓
Local cBot separation gate
    ↓
cTrader compile
    ↓
Hands-on cTrader runtime
    ↓
Broker lifecycle acceptance
    ↓
Deterministic replay
    ↓
Historical outcome validation
    ↓
Local Release
    ↓
Cloud portability
    ↓
Cloud cBot validation
    ↓
Adaptive Learning
\`\`\`

A later stage cannot waive a failed earlier invariant.

---

# 4. External review issue map

The external review was treated as a risk inventory rather than as proof of
runtime failure. Its own limitation was that several findings were static
review findings and still require compile/runtime/replay evidence.

## A-series runtime/execution risks

| ID | Area | Roadmap coverage |
| --- | --- | --- |
| A1 | submission retry/backoff and rejection storms | Tracks 2, 16, 23 |
| A2 | spread/stop semantics | Track 4 |
| A3 | daily-loss persistence/day authority | Track 4, 16 |
| A4 | end-of-day behavior | Track 4, 16 |
| A5 | identity/label scope | Track 3 |
| A6 | fault isolation / management starvation | Track 1 |
| A7 | UI execution safety | Track 5 |
| A8 | cross-instance duplicate protection | Track 3 |
| A9 | intrabar policy | Track 6 |

## B-series analytical/runtime quality risks

| ID | Area | Roadmap coverage |
| --- | --- | --- |
| B1 | hidden clamps / ignored parameters | Track 7 |
| B2 | M1 trigger semantics | Track 8 |
| B3 | evidence duplication | Track 9 |
| B4 | confidence/calibration semantics | Track 9, Track 25 |
| B5 | MaximumOpenPositions semantics | Track 7 |
| B6 | swing plateau/equality semantics | Track 8 |
| B7 | margin duplication | Track 10 |
| B8 | alert key/delivery semantics | Track 11 |
| B9 | partial TP retry/event semantics | Track 12 |
| B10 | outcome persistence/idempotency | Track 11, Track 17 |

## C-series architectural/runtime quality risks

| ID | Area | Roadmap coverage |
| --- | --- | --- |
| C1 | indicator acting as executor | **Track 12A (mandatory local cBot separation)**, Track 27-29 |
| C2 | cosmetic-only modularization | Track 8, Track 14, Track 24 |
| C3 | timer/safety supervisor | Track 1, Track 14 |
| C4 | real performance profiling | Track 14 |
| C5 | broker trading-day model | Track 4 |
| C6 | code hygiene/dead paths | Track 0, Track 7, Track 26 |

---

# 5. Historical implementation milestones

These phases describe the work that created the current architecture. They are
retained for continuity and are not to be repeated unless a regression is found.

## Phase 1 — Canonical source hygiene and architecture contract

Status: complete.

Completed:

- version-neutral production identities;
- historical residue gate;
- managed trade-label authority;
- source/architecture gate;
- separate OSS boundary.

## Phase 2 — Atomic indicators and analysis modules

Status: complete.

Completed:

- atomic native indicators;
- isolated OSS adapters;
- market context decomposition;
- structure/liquidity decomposition;
- FVG lifecycle separation;
- Order Block confluence separation;
- modular editing ownership.

## Phase 3 — Decision and intelligence services

Status: complete.

Completed:

- immutable decision input snapshots;
- score/consensus/quality/confidence separation;
- independent evidence collection;
- ordered decision gates;
- intelligence helper ownership;
- deterministic decision contracts.

## Phase 4 — Planning and risk

Status: complete.

Completed:

- plan construction decomposition;
- target policy/evaluator separation;
- structural stop planning;
- target metadata and progression ownership;
- risk policy, sizing and margin separation;
- planning/risk contracts.

## Phase 5 — Automatic trading, pending orders and lifecycle

Status: complete.

Completed:

- market execution mutation boundary;
- pending Stop/Limit mutation boundaries;
- broker mutation ownership;
- broker reconciliation;
- lifecycle recovery;
- partial close;
- break-even;
- target progression;
- restart/adoption boundaries.

## Phase 6 — Presentation and UI

Status: complete.

Completed:

- chart render ownership;
- plan lines/labels;
- popup;
- panel sections;
- execution controls;
- UI authority gate.

## Phase 7 — OSS research, adapters and benchmarks

Status: complete.

Completed:

- Skender 2.7.3 production boundary;
- FacioQuo 3.0.1 research boundary;
- OSS snapshot caching;
- isolated benchmark project;
- package and license documentation;
- initial numerical parity and performance measurements.

## Phase 8 — Static verification and contract testing

Status: complete.

Completed:

- architecture contract gates;
- BUY/SELL symmetry;
- confidence repeatability;
- SL/TP directionality;
- lifecycle transition invariants;
- lifecycle event idempotency.

## Phase 9 — cTrader compile and runtime acceptance

Status: repository acceptance complete; hands-on terminal/broker acceptance remains
required.

Completed:

- runtime contract workflow;
- MTF integrity;
- broker confirmation;
- rejection handling;
- fill-envelope validation;
- protection recovery;
- partial-close/break-even contracts;
- lifecycle contracts.

Still requires hands-on cTrader/broker validation.

## Phase 10 — Final hardening

Status: complete.

Completed:

- broker-state authority hardening;
- missing-SL protection safeguards;
- live protection validation;
- aggressive fill-mismatch handling;
- explicit RECOVERY state.

## Phase 11 — Deep runtime decomposition and oversized-module audit

Status: complete.

Completed:

- Calculate decomposition;
- closed-bar/live-cycle extraction;
- alert-state extraction;
- production file-size ceiling;
- ownership gates.

## Phase 12 — Market-frame decomposition

Status: complete.

Completed:

- market-frame evidence construction;
- frame scoring;
- quality/direction ownership.

## Phase 13 — Trade-plan construction decomposition

Status: complete.

Completed:

- PlanInputPreparation;
- PlanTargetPreparation;
- PlanMaterialization;
- PlanBuilder orchestration.

## Phase 14 — Target selection decomposition

Status: complete.

Completed:

- stage policy;
- candidate evaluator;
- fixed TP1..TP4 semantics.

## Phase 15 — Order-block candidate decomposition

Status: complete.

Completed:

- geometry;
- structure/displacement evidence;
- mitigation;
- quality;
- confluence ownership.

## Phase 16 — Reward-path validation decomposition

Status: complete.

Completed:

- reward-path geometry;
- zone obstacle scanning;
- target obstacle validation;
- HTF reward-path traversal;
- HTF target presence.

## Phase 17 — Live target progression decomposition

Status: complete.

Completed:

- live target candidate evaluator;
- RR recalculation;
- live target stage orchestration.

## Phase 18 — Automatic market execution decomposition

Status: complete.

Completed:

- eligibility;
- execution preparation;
- submission validation;
- fill adoption;
- post-fill target resolution.

## Phase 19 — Aggressive execution decomposition

Status: complete.

Completed:

- aggressive eligibility;
- entry/SL/TP/volume preparation;
- accepted-fill validation;
- managed-plan adoption.

## Phase 20 — Pending placement decomposition

Status: complete.

Completed:

- Continuation Stop preparation;
- Reversal Limit preparation;
- shared pending submission validation;
- broker-confirmed pending adoption.

## Phase 21 — Cross-path automatic execution consistency

Status: complete.

Completed:

- common intent validation;
- common broker confirmation;
- common protection boundary;
- Stop/Limit distinction;
- Market/Aggressive distinction;
- cross-path invariant gate.

## Phase 22 — Broker protection state ownership

Status: complete.

Completed:

- BrokerProtectionStateEvaluator;
- unified SL/TP validity evaluation;
- lifecycle consumers migrated.

## Phase 23 — Panel renderer decomposition

Status: complete.

Completed:

- overview renderer decomposition;
- trade-plan renderer decomposition;
- pure presentation row ownership.

## Phase 24 — Execution-zone decomposition

Status: complete.

Completed:

- candidate discovery;
- overlap selection;
- retest/premium-discount quality;
- MTF quality adjustment.

## Phase 25 — Additional oversized-module and duplicate-owner cleanup

Status: complete.

Completed:

- remaining near-ceiling modules reduced where required;
- duplicate helper patterns removed;
- ownership documentation updated.

## Phase 26 — Regime-aware intelligence preparation

Status: complete.

Completed:

- regime classification;
- regime snapshots;
- regime-aware decision quality;
- no-trade gating;
- pending/plan synchronization;
- OSS cache improvements.

## Phase 27 — Smart fusion hardening

Status: complete.

Completed:

- evidence caps;
- BUY/SELL symmetry;
- quality-weighted frame contributions;
- deterministic smart-fusion contract;
- prevention of below-threshold directional retention.

## Phase 28 — Runtime resilience foundation

Status: complete.

Completed:

- calculation fault containment;
- fail-closed automatic execution after recoverable runtime failure;
- preservation of fatal process exceptions;
- diagnostic logging.

## Phase 29A — Startup and panel resilience

Status: superseded/iterated during host compatibility investigation.

The initial bootstrap implementation was removed when terminal behavior showed that
the added host API surface was not compatible with the installed environment.

## Phase 29B — Accepted cTrader host compatibility restoration

Status: complete.

Completed:

- minimal known-good Indicator host contract;
- removal of unverified host diagnostics;
- preservation of calculation-level containment.

## Phase 29C — Staged startup

Status: in progress.

Completed:

- early panel construction;
- staged MTF/native initialization;
- Calculate gating until initialization;
- runtime timer cleanup.

Remaining:

- measure target-terminal cold/warm startup;
- confirm final live rendering and resource behavior.

## Phase 29D — Regime-aware signal/execution synchronization

Status: in progress.

Completed:

- regime-aware gating;
- prediction/watch vs confirmed signal vs active plan separation;
- pre-trade plan reconciliation;
- pending-state visual authority;
- trigger rendering restriction;
- OSS per-bar caching.

Remaining:

- full runtime validation on the target terminal;
- replay validation;
- threshold calibration.

---

# 6. Forward master execution program

The following tracks are the current refinement/certification program.

The numerical order is the intended dependency order unless a track is explicitly
marked as a research milestone that may be completed early.

---

# Track 0 — Baseline and clean-state verification

## Phase 0.1 — Repository truth synchronization

Status: complete.

Completed:

- synchronized the documented parameter count to the machine-enforced 535 total (532 baseline + 3 OSS extension parameters);
- synchronized the documented production C# count to the verifier's 398;
- recorded the 27 parameter-group files and 500 method declarations / 467 unique baseline methods;
- synchronized ROADMAP, WORKFLOW and ACCEPTANCE wording;
- restored the required Track 19 continuity-document link;
- verified cTrader compile, runtime contracts and source/architecture checks on the post-fix commit.

Acceptance:

- one documented baseline;
- all counts agree with verifier;
- status claims are traceable to repository evidence.

## Phase 0.2 — Production-source hygiene

Status: complete.

Completed:

- audited production-source paths against the existing legacy/obsolete owner deny-list;
- added verifier checks for version/historical residue, obsolete compatibility identifiers, compatibility aliases, empty catch blocks, generated artifacts and mixed line endings;
- changed repository line-ending policy to LF to match the actual production source format;
- removed all three discovered empty catches from production code:
  - `Runtime/Initialization/RuntimeInitialization.cs`;
  - `UI/Chart/OutcomeMarkerRenderer.cs`;
  - `UI/Panel/PanelVisibility.cs`;
  - `UI/Popup/PopupRemover.cs`;
- replaced silent teardown/render cleanup catches with bounded diagnostic logging while preserving failure containment;
- changed the verifier to report all hygiene findings in one run so future cleanup is faster and less iterative;
- verified the production tree contains no generated binary/artifact paths;
- verified there are no production source paths named Legacy, Compatibility, Obsolete, Versioned, Alias or Deprecated;
- kept trading analysis, decision, risk and broker-mutation owners unchanged.

Phase 0.2 verification commit:

`f866ae3087c8e674ee46ec38e7c03aac2834d9b3`

Verification:

- source and architecture checks: PASS (workflow run 736);
- runtime acceptance contracts: PASS (workflow run 545);
- cTrader compile: PASS (workflow run 729).

Acceptance:

- no obsolete production identifiers;
- no dead compatibility owner;
- no generated production artifacts;
- no empty catch blocks;
- no mixed line endings;
- verifier remains green.

---

# Track 1 — Runtime Safety

## Phase 1.1 — Calculate stage isolation

Status: complete.

Goal:

Make runtime stages independently bounded so one failure cannot suppress
unrelated safety-critical stages.

Target stage model:

```
Broker state
Protection / management
Analysis
Planning
Execution
Telemetry
Presentation
```

Completed:

- replaced the monolithic live-cycle boundary with explicit calculation-stage orchestration;
- preserved the existing stage order and business behavior while giving each live stage an independent recoverable-fault boundary;
- added a dedicated `CalculationStageIsolation.cs` owner for stage orchestration;
- separated preparation, closed-bar analysis and live-cycle orchestration;
- recoverable closed-bar analysis faults fail closed for automatic entry but allow existing live management, protection and reconciliation stages to continue;
- recoverable optional live-analysis faults no longer suppress downstream management stages;
- fatal `OutOfMemoryException` and `StackOverflowException` behavior remains process-fatal;
- routed recoverable stage faults through the existing runtime fault authority;
- added runtime acceptance coverage for stage orchestration and ownership;
- added architecture verifier gates for the new calculation-stage contract;
- retained broker-confirmed state authority and avoided changes to trading strategy semantics.

Verification commit:

`515058ed5183f2f79841e952958e3ad3c1adff7f`

Verification:

- source and architecture checks: PASS (workflow run 741);
- runtime acceptance contracts: PASS (workflow run 550);
- cTrader compile: PASS (workflow run 734).

Acceptance:

- each extracted calculation stage has an independent recoverable-fault boundary;
- optional analysis faults do not prevent downstream live management;
- normal non-ready/skip behavior remains unchanged;
- verifier, runtime contracts and cTrader compile remain green.

## Phase 1.2 — Management-first runtime

Status: complete.

Move safety-critical live management before heavy intelligence.

Order:

```
Broker reconciliation / recovery
Active-plan management
Broker protection
Analysis
Planning
Entry execution
Post-execution reconciliation / protection
Telemetry / reversal / presentation
```

Completed:

- moved broker-state reconciliation ahead of optional live intelligence;
- moved managed-live-plan recovery ahead of optional live intelligence;
- moved active-plan exits and live management ahead of optional live intelligence;
- moved broker protection ahead of optional live intelligence;
- preserved the existing broker mutation owners and broker-confirmed-state authority;
- added post-execution broker reconciliation and protection so a newly created or changed position/order state is synchronized before telemetry and presentation;
- kept reversal analysis after the intelligence stage because its existing decision/frame evidence dependency is analytical rather than pre-analysis safety management;
- preserved independent recoverable stage boundaries so a live-analysis fault cannot suppress downstream planning, execution, telemetry or final presentation;
- did not introduce a second execution authority, duplicate broker mutation path or retry/backoff policy from later phases.

Verification commit:

`77a1b1bc9912c7e1d78bd6428077a416a636f1a7`

Verification:

- source and architecture checks: PASS (workflow run 36497702461);
- runtime acceptance contracts: PASS (workflow run 36497702367);
- cTrader compile: PASS (workflow run 36497702418).

Acceptance:

- broker reconciliation, live-plan recovery, active-plan management and broker protection execute before optional live analysis;
- post-execution broker reconciliation and protection execute before telemetry/presentation;
- management stages remain independently fault-contained;
- no trading owner or broker mutation authority was duplicated or replaced.

## Phase 1.3 — Runtime fault state machine

Status: complete.

Goal:

Make recoverable runtime faults explicit, entry-blocking and recoverable only through a clean management cycle plus an explicit automatic-trading re-arm transition.

States:

\`\`\`
HEALTHY
DEGRADED
ENTRY_BLOCKED
RECOVERING
HEALTHY
\`\`\`

Implementation:

- introduced the dedicated `RuntimeFaultState` enum and `RuntimeFaultStateMachine` under the calculation runtime boundary;
- started and completed one explicit fault-state cycle around each eligible `Calculate()` invocation;
- changed recoverable fault handling from the legacy `ERROR`-only flag mutation to an explicit `DEGRADED` → `ENTRY_BLOCKED` transition;
- preserved fatal `OutOfMemoryException` and `StackOverflowException` behavior;
- allowed management, reconciliation and protection to continue after a recoverable analysis-stage fault;
- transitioned `ENTRY_BLOCKED` → `RECOVERING` only after the pre-analysis management/protection stages complete without a recoverable fault in that cycle;
- transitioned `RECOVERING` → `HEALTHY` only after the complete cycle finishes without another recoverable fault;
- kept automatic entry disarmed across recovery; a `false` → `true` `AutoTradingEnabled` transition after reaching `HEALTHY` is required before broker entry can resume;
- added final entry guards to automatic market, aggressive market, continuation Stop and reversal Limit placement boundaries;
- kept pending daily-loss cancellation and other management checks available because the state gate is applied at the actual pending placement owner rather than suppressing the whole execution stage;
- added deterministic runtime contract coverage for all state transitions and the explicit re-arm rule;
- added source/architecture enforcement for the state machine, cycle lifecycle and automatic-entry guards.

Important design finding:

- `HEALTHY` is a runtime health state, not an automatic re-arm signal. Recovery therefore restores health without restoring broker-entry permission. This prevents a clean cycle from silently reopening automatic execution.

Verification:

- Source and architecture checks: PASS (workflow run 758);
- Runtime acceptance contracts: PASS (workflow run 567);
- cTrader compile: PASS (workflow run 751).

Acceptance:

- recoverable fault blocks entry: PASS by contract and broker-entry guards;
- management remains available: PASS by keeping management-first calculation stages active;
- no blind automatic re-arm: PASS by the latched entry gate and explicit enable transition.

## Phase 1.4 — Closed-bar retry semantics

Status: complete.

Work completed:

- bounded repeated closed-bar analysis failures with exponential retry backoff;
- recorded the failing closed-bar key, failure timestamp and next retry time;
- opened a retry circuit after repeated failures and kept newer closed bars independently eligible;
- prevented closed-bar analysis failure from aborting the management/protection/reconciliation path;
- preserved ordinary non-ready preparation behavior without forcing unnecessary live-cycle work;
- synchronized pre-trade SL rendering with the authoritative plan while retaining broker-confirmed SL for live positions;
- changed market/aggressive execution alerts to report broker-confirmed SL/TP values rather than desired plan values;
- changed pending-order placement reporting to use broker-confirmed order price/SL/TP;
- hardened pending setup against missing decision/reaction dependencies;
- removed a silent reversal pending execution-model catch and converted it to an explicit bounded failure reason;
- added runtime contract coverage and architecture gates for the retry policy and signal/execution synchronization.

Verification:

- Source and architecture checks: PASS (verify job 109187144452 on implementation commit `e43698f1...`);
- Runtime acceptance contracts: PASS (runtime job 109187144646);
- cTrader compile: PASS (build job 109187144983).

Acceptance:

- no CPU storm from repeated closed-bar faults: PASS;
- no repeated full-history rebuild loop for the same failing closed bar: PASS;
- management and protection continue during recoverable analysis faults: PASS;
- visual pre-trade levels come from the active plan and live levels come from broker-confirmed state: PASS;
- execution/pending reporting never presents intended protection as confirmed broker state: PASS.

## Phase 1.5 — Safety supervisor

Status: complete.

Work completed:

- replaced the serial 18-step market-data startup pipeline with asynchronous MarketData.GetBarsAsync(...) acquisition for required M1/M5/M15/M30/H1/H4 data and optional D1/W1 data;
- kept final native-indicator registration and event hookup behind a bounded startup finalization boundary;
- added a bounded initialization timeout so incomplete async loading cannot leave the instance silently waiting forever;
- added a closed MTF context cache keyed by participating Bars identity/count, avoiding repeated stable GetIndexByTime mapping on every tick while invalidating when a timeframe advances;
- removed duplicate closed-M1 frame analysis between preparation and closed-bar processing;
- added a three-entry M5 regime-core cache so stability checks reuse recent regime calculations;
- bounded FVG and Order Block candidate/mitigation traversal by effective zone age;
- reused the cached current closed-M5 ATR across the active protection calculation;
- added a timer-driven safety supervisor for broker reconciliation, managed-live recovery, broker protection and EOD supervision;
- kept the safety timer separate from full analysis and skipped the supervisor while the main calculation cycle is busy;
- recorded a detailed audit of the uploaded four-indicator archive and adopted only compatible optimization/architecture ideas.

ZIP audit result:

- fvg.txt: existing production FVG engine remains authoritative; only bounded-work ideas were retained;
- wavetrend.txt: retained as a future composite momentum candidate, not added as correlated independent votes;
- economic.txt: not imported into the execution core because of blocking external-network startup/runtime coupling;
- volume-profile.txt: empty.

Verification:

- Source / Architecture: PASS
- Runtime Acceptance Contracts: PASS
- cTrader compile: PASS.

Acceptance:

- startup data acquisition is asynchronous and bounded;
- timer supervision does not run the full analysis engine;
- stable closed-bar data is reused instead of recomputed;
- zone searches do not traverse history beyond the configured active lifetime;
- no strategy-score change is introduced solely for performance.

---

# Track 2 — Execution Safety

## Phase 2.1 — SubmissionGate refinement

Status: complete.

Implemented:

- one canonical SubmissionGate replaces the previous separate normal/aggressive/pending gate instances;
- explicit ExecutionSubmissionPath models AutomaticMarket, AggressiveMarket, PendingStop and PendingLimit;
- explicit SubmissionAttemptIdentity carries signal key, attempt key and execution path;
- retry/backoff/circuit state is isolated per canonical submission identity;
- successful submission clears only its own retry state;
- unrelated signals and execution paths remain independently eligible;
- retained retry-state memory is bounded with inactive-state pruning;
- runtime contracts cover first-attempt success, backoff, cross-signal isolation, cross-path isolation, circuit opening and reset;
- architecture verification enforces exactly one submission gate instance and rejects obsolete duplicate gate APIs.

Acceptance:

- one coherent retry policy across automatic paths;
- no failure state leaks from one signal/path into another;
- no duplicate SubmissionGate owner;
- no strategy score or entry-threshold changes introduced by the refactor.

Verification:

- Source / Architecture: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS;
## Phase 2.2 — Automatic market rejection matrix

Status: planned.

Cover:

- null result;
- broker rejection;
- permission failure;
- invalid volume;
- invalid protection;
- fill mismatch;
- exception.

Acceptance:

- each outcome has explicit internal/broker/retry/panel state.

## Phase 2.3 — Aggressive execution rejection matrix

Status: planned.

Same matrix as normal market execution, without merging distinct strategy
semantics.

Acceptance:

- aggressive rejection paths are bounded and traceable.

## Phase 2.4 — Pending Stop acceptance

Status: planned.

Cover:

- BUY STOP;
- SELL STOP;
- preparation;
- validation;
- submission;
- broker confirmation;
- rejection;
- expiration.

Acceptance:

- no synthetic pending state.

## Phase 2.5 — Pending Limit acceptance

Status: planned.

Cover:

- BUY LIMIT;
- SELL LIMIT;
- preparation;
- validation;
- submission;
- broker confirmation;
- rejection;
- expiration.

Acceptance:

- Stop and Limit remain semantically distinct.

---

# Track 3 — Identity and Lifecycle

## Phase 3.1 — Single managed identity

Status: planned.

Authority:

\`\`\`
AutoTradeLabel
    ↓
managed position
managed pending
managed plan
protection
lifecycle
\`\`\`

Acceptance:

- one identity contract;
- no label fallback that changes scope.

## Phase 3.2 — ManagedActionsOnly semantics

Status: planned.

Define exact behavior when off:

- unmanaged positions are ignored;
- managed positions remain bound to the authoritative identity;
- no symbol-wide ownership inference.

Acceptance:

- no accidental adoption of unrelated positions.

## Phase 3.3 — Cross-instance identity guard

Status: planned.

Cover multiple CFIP instances on the same account/symbol.

Acceptance:

- no duplicate automatic entry;
- no double management;
- no double protection.

## Phase 3.4 — Lifecycle event ordering

Status: planned.

Model exact ordering among:

- Execute return;
- Position.Opened;
- Pending.Created;
- Pending.Filled;
- Position.Modified;
- Position.Closed;
- plan binding;
- reconciliation.

Acceptance:

- state transitions remain deterministic under callback reordering.

---

# Track 4 — Risk and Session

## Phase 4.1 — Spread/stop semantic finalization

Status: planned.

Ensure the same spread/stop meaning is consumed by:

- validation;
- sizing;
- structural-stop logic;
- trade suitability.

Acceptance:

- one semantic definition;
- no divergent formulas.

## Phase 4.2 — Daily-loss authority

Status: planned.

Separate:

- trading-day baseline;
- realized P/L;
- floating P/L;
- combined loss;
- reset semantics.

Define whether the guard blocks:

- new market entries;
- new pending orders;
- existing-position management.

Acceptance:

- no ambiguity around what daily loss controls.

## Phase 4.3 — Broker trading-day model

Status: planned.

Create an explicit CFIP trading-day concept supporting broker/session rollover.

Acceptance:

- daily-loss reset matches the accepted broker/session model.

## Phase 4.4 — End-of-day boundary

Status: planned.

Define:

- session end;
- overnight handling;
- warning period;
- close/cancel boundary;
- late-created position handling.

Acceptance:

- one explicit EOD policy.

## Phase 4.5 — End-of-day execution

Status: planned.

Cover:

- position close;
- pending cancel;
- failure/retry;
- broker confirmation;
- recovery;
- operator notification.

Acceptance:

- EOD leaves no ambiguous managed state.

---

# Track 5 — UI Safety

## Phase 5.1 — Live-enable safety

Status: planned.

Automatic trading enablement becomes an explicit state transition.

Acceptance:

- enable/disable has authoritative state;
- UI cannot bypass safety gates.

## Phase 5.2 — Close/Cancel confirmation

Status: planned.

Define exact scope and confirmation semantics for:

- close positions;
- cancel pending orders.

Acceptance:

- operator action cannot accidentally target unrelated broker objects.

## Phase 5.3 — Panel authority cleanup

Status: planned.

Separate:

- clock;
- MTF header;
- M1;
- M5;
- M15;
- M30;
- H1;
- H4;
- D1;
- W1.

Acceptance:

- no presentation state doubles as trading state.

## Phase 5.4 — Signal presentation contract

Status: complete.

Three independent visual states:

```
Prediction / Watch
Confirmed Signal
Active Plan
```

Implemented:

- added one canonical SignalVisualSnapshot for chart/panel signal state;
- centralized authoritative direction resolution with explicit Live Plan → Pending → Pre-trade Plan → Confirmed → Reaction → Prediction precedence;
- routed plan entry, ideal entry, trigger, SL, TP1..TP4 and broker-confirmed live target through the same snapshot;
- routed pending entry/SL/TP display through the same snapshot;
- routed panel direction/stage and synchronization status through the same snapshot;
- removed direct decision/reaction/plan reads from the signal and plan visual renderers;
- kept planned levels and broker-confirmed live protection semantically distinct;
- constructed and released one snapshot per calculation presentation pass instead of letting each renderer recompute signal state.

Acceptance:

- prediction never implies active Entry/SL/TP;
- active live plan remains authoritative over pending display in anomalous overlap;
- pre-trade plan remains the source of intended levels;
- live SL/TP display uses broker-confirmed values;
- arrow, trigger, entry, TP, SL, pending and panel state share one visual-state contract;
- no visual renderer owns independent decision logic.

Verification:

- Source / Architecture: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS.
---

# Track 6 — Intrabar and Closed-Bar Semantics

## Phase 6.1 — Decision closed-bar contract

Status: complete.

Implemented the canonical UTC closed-bar reference contract. Closed-bar indices
are resolved from actual next-bar open times rather than ambiguous time-series
lookup semantics. One MTF closed context is created from the UTC runtime
reference and carried into the decision-input boundary.

The decision-input factory now rejects inconsistent reference/context identity,
missing required closed frames and any present optional frame whose index is not
the canonical closed index. Timeframe agreement consumes the same canonical
closed indices.

Acceptance:

- no future-bar leakage: PASS;
- exact-boundary, between-boundary and gap scenarios: PASS;
- future-bar rejection scenario: PASS;
- source / architecture gates: PASS;
- runtime acceptance contracts: PASS;
- cTrader compile: PASS.

Continuity record: docs/PHASE-6-1-DECISION-CLOSED-BAR.md.

## Phase 6.2 — Reaction intrabar contract

Status: complete.

Implemented and verified:

- live reaction is explicitly intrabar on the current open M5 bar;
- confirmed decision remains closed-bar under the canonical Phase 6.1 context;
- visual state carries explicit reaction identity;
- unified panel remains the operator-facing presentation surface;
- no thresholds, weights, RR, risk or broker execution semantics were changed.

Post-phase runtime correction is recorded in the development log and acceptance
matrix. The correction ensures optional M1/D1/W1 async loading cannot delay runtime
readiness after required M5/M15/M30/H1/H4 data is sufficient and guarantees one
lightweight analysis/presentation seed after readiness.

## Phase 6.3 — Aggressive entry policy

Status: complete.

Selected policy: **controlled intrabar**.

The live current-open-M5 reaction is the aggressive trigger source. It must
produce two distinct qualifying reaction observations on the same M5 bar before
arming. Duplicate observations do not count twice. Direction change, loss of
`EntryAllowed`, a new M5 bar, or confirmed fill invalidates the qualification.

The existing closed-bar decision remains the structural/execution-planning
context. Existing risk, SL/TP, submission, broker-confirmation and protection
owners remain authoritative.

Acceptance:

- no ambiguous mixed-bar policy: PASS;
- two-sample intrabar qualification: PASS;
- explicit invalidation: PASS;
- source / architecture gates: PASS;
- runtime acceptance contracts: PASS;
- cTrader compile: PASS.

Continuity record: `docs/PHASE-6-3-AGGRESSIVE-ENTRY.md`.

---

## Phase 6.4 — Compact 40-Bar Plan-Level Visuals

Status: complete.

The chart plan levels now use a fixed compact presentation:

- latest chart candle is the right edge;
- the level spans 40 bars to the left when sufficient history exists;
- full-width visible-chart boundaries are not used;
- level-specific line styles improve visual hierarchy;
- name + price is rendered in a small left-attached tag using the level's
  semantic color;
- visible label objects are reused during refresh instead of unconditional
  remove/recreate churn;
- cleanup removes both label text and tag boxes;
- no public parameter was added and the 535-parameter contract is preserved.

The implementation is presentation/performance scoped. Decision, reaction,
planning, risk, execution, broker confirmation, protection and lifecycle
semantics are unchanged.

Acceptance:

- source / architecture: PASS;
- runtime acceptance contracts: PASS;
- cTrader compile: PASS;
- hands-on cTrader visual and responsiveness validation: required.

Continuity record: `docs/PHASE-6-4-COMPACT-PLAN-VISUALS.md`.


Post-merge gate record for Phase 6.4 (2026-09-29): PR #23 merge commit `9cc35d7bef7e9bd80d6f1d54b3cbf92601e5d0a2`; Source / Architecture PASS, Runtime Acceptance PASS, cTrader Compile PASS on the verified PR head. Hands-on cTrader visual/performance acceptance remains required.
---

# Track 7 — Parameter Semantics

## Phase 7.1 — Hidden-clamp audit

Status: complete.

Completed:

- audited user-facing parameter consumers for hard-coded floors/ceilings and
  silent overrides;
- removed the hidden `Math.Max(4, requiredTrigger)` floor so Live Trigger Score
  values 1–3 and Precision Trigger Score values below 4 are no longer silently
  ignored;
- removed the hidden 0.05 ATR floor from `TargetUpdateStepAtr`, restoring the
  declared 0.02–2.0 parameter range;
- verified inspected remaining clamps are either aligned with their declared
  parameter minima/maxima or protect internal indexing, numeric validity or
  bounded internal score contribution;
- added source/architecture regression checks for the corrected parameter
  semantics;
- recorded the audit in `docs/PHASE-7-1-HIDDEN-CLAMP-AUDIT.md`;
- preserved the 535-parameter production contract and added no new parameters.

Acceptance:

- user-facing parameters audited for hidden overrides;
- materially hidden user-setting bounds corrected;
- regression checks enforce the corrected semantics;
- no decision authority, broker mutation path, RR/risk policy or trailing rule
  was introduced or duplicated.

## Corrective hotfix — execution priority / controls / structural lock

Status: corrective hotfix in progress on a dedicated branch; this does not change the roadmap owner order.

Completed in this correction:

- execution toggles moved to official Checked/Unchecked state events;
- predictive pending execution is prioritized before plan creation;
- qualified aggressive AUTO TRADE is prioritized before normal market-plan creation;
- market-plan creation defers while a managed pending order exists;
- structural stop progression remains protected from raw market-price chasing;
- no public parameter count change; baseline remains 535.

The corrective hotfix is merged and verified. Phase 7.2 — Dead/unused parameter audit is complete; the next roadmap phase is Phase 7.3 — Semantic duplicate audit.

## Phase 7.2 — Dead/unused parameter audit

Status: complete.

Completed:

- added a machine audit covering all public parameters;
- initial audit found four unread candidates;
- activated FullWidthLevelLines through the canonical line renderer and changed its default to compact mode;
- activated LabelLeftOffsetBars through the canonical label-anchor/box owner;
- activated ShowEarlyArrow as an independent early-watch arrow visibility control;
- removed SmartUseClosedBarDecision because confirmed decision closed-bar behavior is safety-enforced and must not be user-disableable;
- reduced the current public parameter surface from 535 to 534 while preserving 531 baseline + 3 OSS extension parameters;
- added the parameter audit to Source / Architecture CI;
- documented the phase in docs/PHASE-7-2-DEAD-PARAMETER-AUDIT.md.

Final audit:

- 534 declarations;
- 534 read-by-code candidates;
- 0 unused/unread candidates.

Acceptance:

- every retained public parameter has an explicit runtime consumer;
- no duplicate decision/execution authority was introduced;
- closed-bar safety remains enforced;
- all three repository gates passed.

## Phase 7.3 — Semantic duplicate audit

Status: complete.

This phase audits semantic overlap across the parameter surface and enforces one meaningful owner per retained concept. It also corrected the visual synchronization regression where setup levels were only activated after TriggerReady; execution eligibility remains separately guarded.

Confirmed removals: the redundant structural-stop alias `EnableDynamicSlTrail` and the redundant `DecisionEngine` pass-through facade. Current public parameter surface is 533 (530 baseline + 3 OSS extension).

Permanent audit coverage is now part of Source / Architecture CI through `tools/audit_project_integrity.py`, with semantic parameter and runtime UI audits layered on top.

Acceptance: 533 parameters and 0 unread candidates; 0 exact duplicate method signatures; canonical visual snapshot/level geometry PASS; execution UI single-owner boundary PASS; Runtime Acceptance PASS; cTrader Compile PASS.

Compare similar settings such as:

- global confidence;
- pending confidence;
- aggressive confidence;
- quality floors.

Acceptance:

- every retained parameter represents one meaningful concept.

## Phase 7.4 — MaximumOpenPositions semantics

Status: complete.

Decision:

- CFIP remains single-active-plan / single-managed-position by design in the current execution architecture.
- The public `MaximumOpenPositions` parameter is retained for preset/API compatibility but now advertises the only supported capacity: DefaultValue=1, MinValue=1, MaxValue=1.
- The unsupported `BlockNewSignalWhileActive` setting was removed because single-plan capacity is a mandatory safety invariant, not an optional execution mode.
- `ExecutionCapacityRule` is the platform-neutral semantic owner. It distinguishes new-plan capacity from new-broker-execution capacity.
- One shared execution-capacity guard is consumed by plan creation, automatic market execution, aggressive execution and predictive-pending placement.
- Duplicate late-path numeric capacity comparisons were removed.

Acceptance:

- the public configuration cannot advertise multi-position execution;
- execution capacity is defined by one semantic rule and one guard;
- automatic market, aggressive and predictive-pending paths consume the same broker-capacity boundary;
- deterministic runtime contracts cover empty, occupied, pending and unsupported-capacity cases;
- full-project and phase-specific audits enforce the invariant.

Signal-quality continuation:

- The phase performed a deeper Decision -> Trigger -> Plan -> Execution coherence audit specifically to prevent capacity state from producing stale or contradictory trade intent.
- The audit did not claim a win-rate improvement. It identified remaining analytical risks that directly affect false signals: M1 currently acts as a score contribution while canonical TriggerReady remains M5-closed-bar based; structural/liquidity facts are also consumed in multiple downstream gates; and structural confirmations may overlap across M5/M15/H1/H4.
- These are now explicitly owned by Track 8/9, beginning with Phase 8.1, rather than being masked by higher thresholds in the capacity phase.

Current surface: 532 parameters = 529 baseline + 3 OSS extension.

Next implementation phase: Phase 8.1 — M1 trigger correctness.

## Phase 8.1 — M1 trigger correctness

Status: implementation verified and ready to merge.

Verification closeout on branch head `929700e154d4b84a5a0b9efeae345b92017834b6`:
- Runtime Acceptance Contracts: PASS (workflow run 801);
- cTrader Compile: PASS (workflow run 985);
- Source / Architecture: PASS (workflow run 992).

The automated verification boundary is closed. Target-terminal replay/live validation remains required for empirical signal-quality measurement.

Implementation:

- removed the fixed M1 +3/-3 directional score vote;
- introduced one platform-neutral M1 trigger rule;
- bound the rule to the canonical closed M1 index and the exact closed M5 time window;
- required M1 direction, candle geometry, close location and trigger score to agree with the selected MTF direction;
- made M1 confirmation downstream of M5 closed-bar trigger readiness;
- kept direction ownership in the MTF decision and plan creation downstream of TriggerReady;
- added runtime deterministic contracts and permanent source/architecture ownership checks.

Acceptance:

- real M1 OHLC/ATR/trigger-score data path;
- deterministic BUY/SELL-symmetric M1 confirmation semantics;
- M1 cannot independently change decision direction;
- M1 confirmation cannot accept a future, stale, misaligned or weak bar;
- plan creation remains TriggerReady-gated.

Signal-quality continuation:

- This phase fixes the temporal/causal mismatch where M1 could affect direction while TriggerReady was based only on M5.
- It does not claim a measured win-rate improvement; that requires replay/historical outcome evidence.

## Phase 8.2 — Swing plateau correctness

Status: planned.

Audit equality behavior for:

- equal highs;
- equal lows;
- plateau structures.

Acceptance:

- equality is handled intentionally.

## Phase 8.3 — FVG mathematical audit

Status: IMPLEMENTATION VERIFIED — READY TO MERGE. Branch `phase-8-3-fvg-mathematical-audit`. Full scope and acceptance criteria: `docs/PHASE-8-3-FVG-MATHEMATICAL-AUDIT.md`. Production FVG detection and predictive pending collection now share canonical geometry, creation-bar ATR thresholding, post-creation retest semantics, centralized mitigation and stable identity. CI head `89919e7363d374e2cf3a362ec553b1fdac464919`: Runtime PASS; Build PASS; Source/Architecture PASS. Target cTrader replay remains required for empirical signal-quality measurement.

Audit:

- 3-bar definition;
- imbalance variants;
- gap size;
- mitigation;
- partial fill;
- full fill;
- age;
- quality.

Acceptance:

- one mathematical owner for each concept.

## Phase 8.4 — Order Block mathematical audit

Status: IMPLEMENTATION VERIFIED — READY TO MERGE. Branch `phase-8-4-order-block-mathematical-audit`. Production Order Block source geometry, creation-ATR qualification, mitigation, identity and canonical FVG confluence are centralized and verified. User reference FVG was audited; custom WaveTrend is documented as a future closed-bar confluence adapter rather than a standalone trigger. CI head `41a578ba72fec2219447ddc1ceff12b96ee353e7`: Runtime PASS; Build PASS; Source/Architecture PASS. Target cTrader replay remains required for empirical signal-quality measurement.

Audit:

- source candle;
- displacement;
- BOS/MSS;
- liquidity sweep;
- FVG confluence;
- mitigation;
- remaining width;
- quality.

Acceptance:

- OB evidence and quality remain CFIP-specific and deterministic.

## Phase 8.5 — Zone confluence symmetry

Status: planned.

BUY/SELL mirror validation across:

- FVG;
- OB;
- liquidity;
- structure;
- reward path.

Acceptance:

- no directional asymmetry without an explicit rule.

---

# Track 9 — Decision Intelligence

## Phase 9.1 — Evidence duplication

Status: planned.

Audit how many times one market fact contributes to score.

Acceptance:

- correlated evidence cannot receive uncontrolled duplicate influence.

## Phase 9.2 — Confidence semantics

Status: planned.

Distinguish:

\`\`\`
score
confidence-like score
probability
calibrated probability
\`\`\`

Acceptance:

- UI/documentation never presents an uncalibrated score as a probability.

## Phase 9.3 — Empirical calibration

Status: planned.

Connect calibration to real outcome data.

Acceptance:

- calibration is empirical;
- evaluation is out-of-sample/walk-forward where applicable.

## Phase 9.4 — Regime-conditioned intelligence

Status: planned.

Define evidence relevance by:

- TREND;
- EXPANSION;
- RANGE;
- COMPRESSION;
- HIGH_VOLATILITY;
- TRANSITION.

Acceptance:

- regime modifies relevance without becoming a second decision authority.

## Phase 9.5 — No-trade intelligence

Status: planned.

No-trade reasons must be:

- explicit;
- traceable;
- deterministic.

Acceptance:

- operator can identify why a setup did not become actionable.

---

# Track 10 — Risk Engine Quality

## Phase 10.1 — Margin logic consolidation

Status: planned.

Single owner for:

- required margin;
- margin usage;
- free margin;
- safety buffer;
- volume reduction.

Acceptance:

- no duplicated margin formula.

## Phase 10.2 — Volume semantics

Status: planned.

Validate:

- risk amount;
- stop distance;
- pip value;
- broker volume step;
- minimum/maximum volume;
- rounding.

Acceptance:

- deterministic sizing across supported symbols.

## Phase 10.3 — Market suitability cleanup

Status: planned.

Market suitability owns only:

- spread;
- session;
- volatility;
- event suitability;
- execution environment.

It must not become a hidden decision authority.

Acceptance:

- risk, decision and suitability responsibilities remain separate.

---

# Track 11 — Alert and Telemetry

## Phase 11.1 — Alert key semantics

Status: planned.

Normalize alert identities such as:

- AUTO;
- AUTO-OFF;
- AUTO-REACTION;
- execution/recovery events.

Acceptance:

- exact keys;
- no prefix collisions.

## Phase 11.2 — Async/throttled alert delivery

Status: planned.

Remove:

- email;
- sound;
- popup;
- heavy formatting

from the hot tick path.

Acceptance:

- notification latency cannot block trading management.

## Phase 11.3 — Outcome telemetry integrity

Status: planned.

Outcome event must be:

- idempotent;
- bounded;
- traceable;
- persistable;
- associated with decision and execution intent identity.

Acceptance:

- duplicate callbacks cannot duplicate outcomes.

---

# Track 12 — Partial TP and Exit Integrity

## Phase 12.1 — Partial TP retry policy

Status: planned.

A rejected partial close must not create uncontrolled every-tick retries.

Acceptance:

- explicit backoff/circuit state;
- no repeated broker hammering.

## Phase 12.2 — Partial TP event semantics

Status: planned.

Model:

- volume reduction;
- Position.Modified;
- Position.Closed;
- remaining position;
- target-stage state.

Acceptance:

- target stage derives from broker-confirmed remaining volume.

## Phase 12.3 — Break-even after partial

Status: planned.

Cover:

- partial success;
- BE success;
- BE rejection;
- invalid broker distance;
- recovery;
- retry.

Acceptance:

- no false rejection/recovery classification.

---

# Track 11.6 — Claude Review Defect Remediation — 2026-09-30

Status: CR1.8 COMPLETE; CR1.9 IS THE NEXT IMPLEMENTATION PHASE

Three external code-review prompts (A1–A12, B1–B12, C1–C9) are tracked in:
`docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md`

CR-0 audit/source inventory:
`docs/CLAUDE-REVIEW-CR0-AUDIT.md`

CR1.1 — Session/EOD and period-reference correctness is complete.
CR1.2 — Daily-loss lock and stable accounting basis is complete.
CR1.3 — News guard correctness and non-blocking refresh is complete.
CR1.4 — Closed-bar cycle ordering and waiting-for-data state is complete.
CR1.5 — Hot-path/cache/logging performance is complete.

CR1.6 — FVG quality discrimination is complete and merged.
CR1.7 — Threshold truth + volume audit is complete and merged.
CR1.8 — Managed identity boundary is complete and merged at the remediation level; target-terminal broker identity/restart/reconciliation verification remains required.
The next required implementation phase is **CR1.9 — Minor cleanup and documentation**.

Completed CR1.5 scope:
- buffered Runtime Log / Outcome / Signal Trace archive writes outside Calculate;
- deferred explicit LocalStorage flush/reload work to the Timer heartbeat;
- cached archive prefixes and parameter fingerprint used by logging;
- cached closed-bar FVG/OB candidate scans with quote-sensitive final selection;
- event/mutation-driven broker-state dirty invalidation with a bounded refresh rule;
- deterministic buffer/broker refresh runtime contracts and dedicated hot-path audit;
- preserved 90-day historical archive partitioning without deletion.

Verification boundary:
- Runtime Acceptance and cTrader Compile PASS on the final CR1.5 head;
- Source/Architecture final-head verification remains required before merge;
- target-terminal performance and persistence behavior remain mandatory runtime checks;
- container-local clone/build was unavailable because external DNS could not resolve github.com.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was rechecked. No signal threshold, RR floor, position capacity, decision authority or execution authority was changed.

The next required implementation phase is **CR1.6 — FVG quality discrimination**.

Track 12A local cBot separation remains blocked until Track 11.6 CR-FINAL passes.

# Track 12A — Mandatory Local cBot Separation

Status: **BLOCKING NEXT ARCHITECTURAL GATE**

Canonical document: `docs/CBOT-SEPARATION-ROADMAP.md`

Purpose:

Separate the current broker-execution authority from `CFIP.Indicator` into a
dedicated local cBot while preserving the Indicator as the sole analysis,
decision, scenario and trade-plan authority.

This track is intentionally local. It introduces no Cloud service, HTTP API,
socket, database, broker service, or multi-position capability.

## Mandatory preflight gate after CBOT-0

After CBOT-0 and before CBOT-1, the target cTrader environment must pass **CBOT-Preflight**:

- cBot can instantiate the compiled CFIP Indicator through the supported custom-indicator mechanism;
- cBot can read a structured public read-only signal surface without reflection or chart-object scraping;
- Indicator and cBot can start in either order without fabricated/stale signal acceptance;
- symbol/timeframe/strategy instance scope is deterministic;
- stale/uninitialized/unavailable signal state fails closed;
- the three-component packaging/output layout is accepted by the target terminal.

If this capability cannot be proven, stop the migration and redesign the local handoff. Do not create a second analysis engine or an unofficial transport workaround.

## Scope

Move/extract only the capabilities that must be owned by a broker-executing
cBot:

- broker market/aggressive/pending submission;
- pending cancellation;
- broker position close/partial close;
- broker SL/TP mutation and protection;
- broker confirmation/fill reconciliation;
- managed broker identity;
- account-dependent execution risk;
- execution capacity;
- restart/reconnect reconciliation;
- submission idempotency/retry/backoff/circuit state;
- broker execution telemetry;
- authoritative automatic-execution enable/disable state.

Keep the analytical brain inside the Indicator:

- Analysis;
- Decision;
- Entry/Trigger;
- Planning;
- analytical RR/reward-path;
- scenario generation/materialization;
- UI/presentation;
- alerts;
- learning/calibration.

Mixed modules are to be **split by method/responsibility**, not copied wholesale.
In particular, `BrokerProtectionCoordinator.cs`, server-side TP ladder modules,
`Trading/LiveManagement/*` and account-risk files may contain both analytical
and broker-facing responsibilities and therefore require explicit split inventories.

## Mandatory phases

`CBOT-0` Boundary inventory, dependency closure, parameter ownership and execution-authority freeze

`CBOT-Preflight` Target cTrader capability proof (blocking gate before CBOT-1)

`CBOT-1` Platform-neutral local contracts

`CBOT-2` Indicator read-only signal provider surface

`CBOT-3` cBot host + shadow execution

`CBOT-4` Broker execution extraction

`CBOT-5` Protection/lifecycle/recovery extraction

`CBOT-6` Account/execution risk + control ownership

`CBOT-7` Indicator execution-authority removal and final cutover

## Blocking rule

No new live broker-execution feature may be added to the Indicator after Phase
11.5 until Track 12A is complete.

The existing Phase 11.5 scenario policy remains valid, but any future promotion
from scenario/opportunity data to broker mutation must be implemented through the
cBot boundary.

## Hard migration rules

- A moved method carries its required fields/helpers/dependencies with it; no hidden dependency may remain in the Indicator.
- Semantic business rules have one authoritative owner. Pure serialization/DTO code may be duplicated only when it contains no business rule.
- The legacy Indicator executor may exist only as a repository snapshot/parity oracle during migration; it must never be shipped or run as a second live executor.
- Until final cutover, live trading uses exactly one explicitly designated executor at a time.
- After cutover, missing/stale/incompatible cBot state fails closed and never reactivates an Indicator fallback.
- The Indicator remains usable as an analysis/display component when the cBot is absent.

## Parameter-ownership rule

A parameter stays in Indicator when it changes analytical evidence, confidence/quality, MTF interpretation, scenario selection or proposed Entry/SL/TP geometry. A parameter moves to cBot when it controls account/broker permission, sizing normalization, capacity, daily-loss enforcement, broker protection mutation, execution throttling or the authoritative Auto Trading/Auto Orders state. Presentation-only parameters stay in Indicator. No execution behavior may have two independent parameter values.

## Required final acceptance

- `CFIP.Contracts` is platform-neutral.
- `CFIP.cBot` is the sole broker mutation authority.
- Indicator exposes read-only structured execution intent/plan data.
- Signal freshness, expiry, revision ordering and instance scope are deterministic.
- Scenario identity survives Indicator → cBot → broker telemetry.
- Broker-confirmed state remains authoritative.
- Single managed position/capacity remains unchanged.
- Indicator contains no direct broker mutation and no hidden/disabled duplicate broker executor.
- The authoritative Auto Trading/Auto Orders enable state is owned by cBot; Indicator presentation cannot disagree with broker behavior.
- Source/Architecture, Runtime Acceptance and cTrader Compile/Build are green.
- Target-terminal local execution replay passes the mandatory market, aggressive,
  pending, protection, close, recovery, restart/reconnect and duplicate-suppression matrix.

## Cloud boundary

Cloud portability remains a future track. Only the contracts are made
platform-neutral now; no Cloud implementation is created as part of Track 12A.

---

# Track 13 — cTrader Runtime Compatibility

## Phase 13.1 — Access/permission certification

Status: planned.

Verify the exact installed target runtime for:

- AccessRights;
- trading permission request;
- Indicator trading capability;
- live broker restrictions.

Acceptance:

- no assumption based only on a different cTrader build.

## Phase 13.2 — Indicator/package compatibility

Status: planned.

Verify:

- cTrader Automate API;
- external assembly loading;
- Skender package compatibility;
- startup behavior;
- unload/destroy behavior.

Acceptance:

- target terminal loads and runs the production assembly.

## Phase 13.3 — Native indicator parity

Status: planned.

Verify:

- DMI;
- ADX;
- DI+;
- DI-;
- native vs OSS references where applicable.

Acceptance:

- differences are documented rather than silently mixed.

## Phase 13.4 — Fill semantics

Status: planned.

Verify:

- actual fill;
- requested entry;
- execution envelope;
- SL/TP in pips/price;
- ProtectionType;
- slippage;
- broker normalization.

Acceptance:

- plan, intent and broker fill remain distinct.

## Phase 13.5 — Account model

Status: planned.

Verify:

- hedging/netting behavior;
- position volume semantics;
- pending behavior;
- label scope;
- position binding.

Acceptance:

- identity and lifecycle are valid for the actual account model.

## Phase 13.6 — Event ordering

Status: planned.

Record actual target-terminal callback timing.

Acceptance:

- lifecycle state machine matches observed cTrader behavior.

---

# Track 14 — Performance

## Phase 14.1 — Startup profiling

Status: planned.

Measure:

- panel appearance;
- M1 load;
- M5 load;
- MTF load;
- native registration;
- first-ready latency.

Acceptance:

- cold/warm startup baselines recorded.

## Phase 14.2 — Native registration optimization

Status: planned.

Avoid registering unused timeframes/indicators.

Acceptance:

- only required registrations are retained.

## Phase 14.3 — Tick hot-path audit

Status: planned.

Profile:

- reaction;
- execution model;
- FVG;
- OB;
- suitability;
- panel;
- chart;
- broker reconciliation.

Acceptance:

- hot path has bounded work.

## Phase 14.4 — Cache correctness

Status: planned.

Every cache must have:

- explicit key;
- invalidation rule;
- bounded lifetime/memory;
- no recursive construction;
- no stale-bar reuse.

Acceptance:

- cache correctness is proven by tests.

## Phase 14.5 — UI render throttling

Status: planned.

Heavy render only after relevant state change.

Acceptance:

- unchanged state does not trigger repeated heavy rendering.

---

# Track 15 — Real Testability

## Phase 15.1 — Clock abstraction

Status: planned.

Introduce an injectable clock for runtime/replay tests.

Acceptance:

- no test depends on wall-clock timing.

## Phase 15.2 — Market-data abstraction

Status: planned.

Expose a minimal interface for:

- bars;
- quotes;
- timeframes;
- symbol metadata.

Acceptance:

- analysis can be supplied deterministic test data.

## Phase 15.3 — Broker abstraction

Status: planned.

Define operations for:

- submit;
- modify;
- close;
- cancel;
- query;
- confirmation.

Acceptance:

- broker side effects are test-replaceable.

## Phase 15.4 — Fake broker

Status: planned.

Must simulate:

- success;
- rejection;
- delay;
- fill mismatch;
- missing protection;
- reconnect;
- stale state.

Acceptance:

- execution state machine is testable without cTrader.

---

# Track 16 — Automated Safety Tests

## Phase 16.1 — A1 rejection-storm tests

Status: planned.

Acceptance:

- bounded retries;
- exponential/backoff semantics;
- no duplicate broker calls.

## Phase 16.2 — A3 daily-loss restart tests

Status: planned.

Acceptance:

- reset/persistence semantics survive restart and broker-day rollover.

## Phase 16.3 — A4 EOD tests

Status: planned.

Acceptance:

- position/pending cleanup and confirmation are deterministic.

## Phase 16.4 — A5 identity tests

Status: planned.

Acceptance:

- only the intended managed identity is adopted/managed.

## Phase 16.5 — A6 fault-isolation tests

Status: planned.

Acceptance:

- broken analysis does not starve protection/exit/reconciliation.

## Phase 16.6 — A7 safety-control tests

Status: planned.

Acceptance:

- operator controls cannot bypass managed scope or safety gates.

## Phase 16.7 — A8 cross-instance tests

Status: planned.

Acceptance:

- duplicate entries and double management are prevented.

## Phase 16.8 — A9 intrabar tests

Status: planned.

Acceptance:

- closed-bar decision and intrabar reaction semantics remain distinct.

---

# Track 17 — Deterministic Replay

## Phase 17.1 — Replay data model

Status: planned.

Model:

- bars;
- timestamps;
- symbol metadata;
- session;
- spread;
- broker events;
- decision identity.

Acceptance:

- complete input is serializable and reproducible.

## Phase 17.2 — Bar replay engine

Status: planned.

Support:

\`\`\`
M1 → M5 → M15 → M30 → H1 → H4 → D1 → W1
\`\`\`

Acceptance:

- MTF closed-bar alignment is deterministic.

## Phase 17.3 — Decision replay

Status: planned.

Recreate:

- evidence;
- score;
- decision;
- no-trade reasons.

Acceptance:

- same input produces same decision.

## Phase 17.4 — Plan replay

Status: planned.

Recreate:

- entry;
- trigger;
- SL;
- TP1..TP4;
- reward path;
- risk sizing.

Acceptance:

- plan is traceable to the originating decision.

## Phase 17.5 — Execution-intent replay

Status: planned.

Recreate:

- submission intent;
- broker fill;
- mismatch;
- rejection;
- protection.

Acceptance:

- no synthetic fill.

## Phase 17.6 — Outcome replay

Status: planned.

Record:

- MFE;
- MAE;
- time-to-outcome;
- target stage;
- final outcome.

Acceptance:

- outcome is reproducible and traceable.

---

# Track 18 — Quant and Backtest Fabric

LEAN is reference/research infrastructure only.

## Phase 18.1 — LEAN research boundary

Status: planned.

Create only an adapter/reference boundary.

Acceptance:

- LEAN code never enters the production cTrader assembly.

## Phase 18.2 — Setup classification

Status: planned.

Classify:

- trend;
- reversal;
- continuation;
- breakout;
- retest;
- FVG;
- OB;
- FVG+OB.

Acceptance:

- classifications are deterministic and stored with outcomes.

## Phase 18.3 — Performance metrics

Status: planned.

Calculate:

- win rate;
- average R;
- expectancy;
- drawdown;
- MFE;
- MAE;
- duration.

Acceptance:

- metrics are calculated consistently from replay/outcome data.

## Phase 18.4 — Threshold validation

Status: planned.

Validate current threshold families empirically instead of by intuition.

Acceptance:

- threshold decisions are supported by replay/outcome evidence.

---

# Track 19 — OSS Numerical Benchmark

## Phase 19.1 — FacioQuo comparison

Status: complete.

Completed:

- production/research package boundary preserved;
- shared deterministic fixture source;
- 800-bar fixtures;
- TREND_UP;
- TREND_DOWN;
- RANGE;
- REGIME_SHIFT;
- output count checks;
- timestamp checks;
- warm-up-aware comparison;
- finite coverage;
- max/mean/RMS error;
- strict 1e-6 absolute tolerance;
- all 10 production OSS indicator families;
- Bollinger %B and Width;
- Stochastic %K and %D;
- batch timing;
- allocation measurement;
- Markdown report;
- GitHub Actions summary;
- static verifier gates.

Continuity document: [Track 19 OSS Numerical Benchmark](docs/TRACK-19-OSS-NUMERICAL-BENCHMARK.md).

Acceptance:

- v3 remains benchmark-only regardless of parity result.

## Phase 19.2 — TA-Lib.NETCore cross-check

Status: planned.

Scope:

- benchmark only;
- overlapping indicators;
- native/runtime dependency inventory;
- licensing/distribution review;
- same deterministic fixtures.

Acceptance:

- numerical evidence reproducible;
- license gate documented;
- no production boundary change.

## Phase 19.3 — QuantConnect LEAN numerical/reference comparison

Status: planned.

Scope:

- indicator/reference overlap;
- formula/output conventions;
- no trading-engine import.

Acceptance:

- reference results reproducible;
- semantic differences documented.

## Phase 19.4 — OSS benchmark consolidation

Status: planned.

Scope:

- package;
- version;
- metric;
- fixture;
- tolerance;
- timing;
- machine-readable result.

Acceptance:

- one canonical OSS numerical benchmark contract.

## Phase 19.5 — OSS admission decision

Status: planned.

Promotion requires all:

- runtime compatibility;
- license/distribution review;
- numerical parity;
- performance evidence;
- regression fixtures;
- explicit adapter owner.

Acceptance:

- each research package has documented admission/rejection status.

---

# Track 20 — Automatic market execution certification

## Phase 20.1 — Normal BUY/SELL market certification

Status: planned.

Acceptance:

- valid intent;
- valid quote;
- valid volume;
- confirmed fill;
- protection;
- lifecycle adoption.

## Phase 20.2 — Aggressive BUY/SELL certification

Status: planned.

Acceptance:

- original submission intent remains available for fill validation;
- mismatch follows close/recovery policy.

## Phase 20.3 — Broker-result classification

Status: planned.

Classify:

- accepted;
- rejected;
- exception;
- incomplete;
- mismatched.

Acceptance:

- no result is silently reclassified.

## Phase 20.4 — Market protection certification

Status: planned.

Acceptance:

- protection is validated from broker state;
- failed protection enters explicit RECOVERY.

---

# Track 21 — Pending-order certification

## Phase 21.1 — Stop order certification

Status: planned.

Acceptance:

- BUY STOP / SELL STOP;
- broker-confirmed pending adoption;
- expiration;
- rejection handling.

## Phase 21.2 — Limit order certification

Status: planned.

Acceptance:

- BUY LIMIT / SELL LIMIT;
- broker-confirmed pending adoption;
- expiration;
- rejection handling.

## Phase 21.3 — Pending-fill transition

Status: planned.

Acceptance:

- pending becomes position only after confirmed fill;
- fill identity binds to active plan.

## Phase 21.4 — Pending cancellation/idempotency

Status: planned.

Acceptance:

- cancellation is confirmation-driven;
- duplicate cancel/fill callbacks are safe.

---

# Track 22 — Protection, lifecycle and recovery certification

## Phase 22.1 — Initial SL/TP

Status: planned.

Acceptance:

- correct side;
- correct distance;
- broker-normalized;
- confirmed.

## Phase 22.2 — Break-even/profit lock

Status: planned.

Acceptance:

- post-entry protection may move into protected profit;
- current-market validation remains correct.

## Phase 22.3 — Trailing/structural management

Status: planned.

Acceptance:

- no unsafe stop movement;
- no synthetic broker state.

## Phase 22.4 — Partial/full close

Status: planned.

Acceptance:

- volume changes are broker-confirmed;
- lifecycle state matches broker state.

## Phase 22.5 — Missing protection recovery

Status: planned.

Acceptance:

- explicit recovery;
- bounded retry;
- no false healthy state.

## Phase 22.6 — Restart/reconnect reconciliation

Status: planned.

Acceptance:

- broker state is re-adopted deterministically;
- stale plans are cleared safely.

---

# Track 23 — Cross-path stress and reconciliation

## Phase 23.1 — Rapid-tick stress

Status: planned.

Acceptance:

- no duplicate entry;
- no broker-mutation storm;
- management remains responsive.

## Phase 23.2 — Duplicate callback stress

Status: planned.

Acceptance:

- lifecycle idempotency survives duplicate events.

## Phase 23.3 — Broker lag/reconnect stress

Status: planned.

Acceptance:

- state converges after reconciliation.

## Phase 23.4 — Fill mismatch stress

Status: planned.

Acceptance:

- invalid fills never become permanent synthetic state.

## Phase 23.5 — Stale-plan/invalidation stress

Status: planned.

Acceptance:

- stale plan cannot execute after invalidation.

## Phase 23.6 — Concurrent-looking event stress

Status: planned.

Acceptance:

- deterministic final state despite adversarial event ordering.

---

# Track 24 — Analytical accuracy and anti-lookahead certification

## Phase 24.1 — Closed-bar enforcement

Status: planned.

Acceptance:

- no future bar enters confirmed decision state.

## Phase 24.2 — MTF timestamp/index verification

Status: planned.

Acceptance:

- all MTF evidence maps to the intended closed reference.

## Phase 24.3 — Indicator parity fixtures

Status: planned.

Acceptance:

- native/OSS numerical differences are explicit.

## Phase 24.4 — Structure/FVG/OB/liquidity fixtures

Status: planned.

Acceptance:

- deterministic fixtures cover boundary cases and mirror symmetry.

## Phase 24.5 — Evidence deduplication

Status: planned.

Acceptance:

- independent evidence weight is traceable.

## Phase 24.6 — Numerical edge-case certification

Status: planned.

Cover:

- NaN;
- Infinity;
- zero-range;
- zero-volume;
- insufficient warmup;
- flat price;
- large spread.

Acceptance:

- no invalid numerical value becomes actionable state.

---

# Track 25 — Confluence calibration and no-trade intelligence

## Phase 25.1 — Evidence quality

Status: planned.

Acceptance:

- evidence quality is explicit and deterministic.

## Phase 25.2 — Regime relevance

Status: planned.

Acceptance:

- evidence weights can vary by regime without bypassing risk gates.

## Phase 25.3 — Confidence calibration

Status: planned.

Acceptance:

- calibrated confidence is evaluated against held-out outcomes.

## Phase 25.4 — No-trade model

Status: planned.

Acceptance:

- strong no-trade conditions are explicit and traceable.

## Phase 25.5 — Expected-value diagnostics

Status: planned.

Acceptance:

- diagnostics do not become hidden execution authority.

---

# Track 26 — Local hardening and release

## Phase 26.1 — Final performance/memory sweep

Status: planned.

Measure:

- startup;
- memory;
- allocations;
- tick CPU;
- panel render cost;
- cache growth.

Acceptance:

- no unresolved high-impact resource regression.

## Phase 26.2 — Error/recovery logging review

Status: planned.

Acceptance:

- execution/recovery failures are diagnosable;
- sensitive information is not logged unnecessarily.

## Phase 26.3 — Parameter/default audit

Status: planned.

Acceptance:

- defaults are intentional;
- hidden behavior is removed;
- parameter inventory is consistent.

## Phase 26.4 — Chart/panel/alert acceptance

Status: planned.

Acceptance:

- all presentation states render correctly;
- no stale visual authority.

## Phase 26.5 — Demo-to-controlled-live checklist

Status: planned.

Acceptance:

- explicit operational checklist exists;
- broker/account/session assumptions are recorded.

## Phase 26.6 — Final local release gate

Status: planned.

Acceptance:

- all required acceptance matrix scenarios PASS;
- no unresolved critical execution/lifecycle defect;
- reproducible production source;
- rollback procedure documented.

Milestone:

**LOCAL PRODUCT COMPLETE**

---

# Track 27 — Cloud portability architecture

Status: planned after Local Product Complete.

## Phase 27.1 — Host-neutral contracts

Separate:

- analysis;
- planning;
- risk;
- lifecycle contracts

from cTrader UI callbacks.

Acceptance:

- core strategy logic can run without Indicator UI.

## Phase 27.2 — cBot host adapter contract

Define:

- market data;
- clock;
- broker events;
- execution;
- lifecycle.

Acceptance:

- cBot host reuses existing authority chain.

## Phase 27.3 — Broker/event abstraction

Acceptance:

- no cloud adapter bypasses risk/protection/identity.

## Phase 27.4 — Local/cloud configuration parity

Acceptance:

- configuration semantics remain consistent.

---

# Track 28 — Cloud cBot host implementation

## Phase 28.1 — cBot host

Status: planned.

Implement:

- cBot callbacks;
- market data mapping;
- runtime lifecycle.

## Phase 28.2 — Broker confirmation/reconciliation

Status: planned.

Acceptance:

- broker-confirmed state remains authoritative.

## Phase 28.3 — Managed identity

Status: planned.

Acceptance:

- Indicator and cBot do not create separate managed identities.

## Phase 28.4 — Protection/recovery parity

Status: planned.

Acceptance:

- recovery semantics match local contracts.

## Phase 28.5 — Decision/execution parity

Status: planned.

Acceptance:

- identical inputs produce equivalent decision/execution intent.

---

# Track 29 — Cloud validation and operational hardening

## Phase 29.1 — Demo cloud execution

Status: planned.

Acceptance:

- normal market path validated in controlled environment.

## Phase 29.2 — Pending cloud execution

Status: planned.

Acceptance:

- Stop/Limit behavior matches local contract.

## Phase 29.3 — Restart/continuity

Status: planned.

Acceptance:

- cloud restart/reconciliation is deterministic.

## Phase 29.4 — Resource/runtime observation

Status: planned.

Acceptance:

- resource use remains controlled.

## Phase 29.5 — Broker compatibility

Status: planned.

Acceptance:

- no broker-specific divergence from local assumptions.

## Phase 29.6 — Secrets/configuration/operations

Status: planned.

Acceptance:

- secrets are not embedded in source;
- operational runbook is complete.

Milestone:

**CLOUD VALIDATION COMPLETE**

---

# Track 30 — Adaptive Learning, post-stability only

This track is intentionally last.

Learning cannot modify safety invariants, create a second execution authority or
silently mutate production behavior.

## Phase 30.1 — Outcome dataset quality

Status: deferred.

Acceptance:

- sufficient clean outcome data;
- provenance;
- replay traceability.

## Phase 30.2 — Rolling calibration

Status: deferred.

Acceptance:

- calibration evaluated on rolling/out-of-sample data.

## Phase 30.3 — Evidence reliability updates

Status: deferred.

Acceptance:

- adaptation is bounded and observable.

## Phase 30.4 — Bounded parameter adaptation

Status: deferred.

Acceptance:

- safety floors remain immutable;
- changes are bounded.

## Phase 30.5 — Drift detection

Status: deferred.

Acceptance:

- drift is measurable;
- adaptation can be frozen on degraded conditions.

## Phase 30.6 — Offline training/evaluation

Status: deferred.

Acceptance:

- training never runs as an uncontrolled live broker authority.

## Phase 30.7 — Approval and rollback

Status: deferred.

Acceptance:

- changes require approval;
- previous state is restorable.

## Phase 30.8 — Optional local persistence

Status: deferred.

Potential bounded storage:

- cTrader LocalStorage;
- explicit versioned model metadata;
- provenance.

Acceptance:

- learning state is durable but cannot bypass safety contracts.

Milestone:

**ADAPTIVE LEARNING ELIGIBLE**

---

# 7. Final project architecture target

The final production flow remains:

\`\`\`
Market Data
    ↓
MTF Closed Context
    ↓
Indicators
    ↓
Market / Regime / Structure / Zones / Liquidity
    ↓
Evidence
    ↓
Decision
    ↓
Entry / Trigger
    ↓
Trade Plan
    ↓
Risk
    ↓
Execution Intent
    ↓
Broker Mutation Boundary
    ↓
Broker Confirmation
    ↓
Lifecycle
    ↓
Live Management / Protection
    ↓
Outcome
    ↓
Presentation / Telemetry
\`\`\`

The following are never allowed to become parallel authorities:

\`\`\`
OSS indicator package
LEAN
TA-Lib
UI
Telemetry
Replay engine
Learning model
Prediction module
\`\`\`

They may provide evidence, reference output, simulation or presentation, but
the CFIP decision/risk/execution authority remains singular.

---

# 8. Final completion criteria

CFIP is not considered fully complete until all of the following are true:

## Architecture

- one decision authority;
- one risk authority;
- one automatic execution authority;
- one managed identity;
- one broker mutation boundary;
- no duplicate business rules;
- no obsolete monolithic owner;
- no production source above the enforced module ceiling.

## Runtime

- runtime faults are isolated;
- protection and lifecycle management cannot be starved by optional analysis;
- retries are bounded;
- reconnect/restart reconciliation is deterministic;
- EOD is explicit.

## Analytical correctness

- no look-ahead leakage;
- MTF timestamps/indexes are correct;
- BUY/SELL symmetry is tested;
- FVG/OB/liquidity mathematics are deterministic;
- evidence duplication is controlled.

## Execution

- no synthetic fills;
- no synthetic pending state;
- protection is broker-confirmed;
- partial close and close are confirmation-driven;
- rejection/recovery is explicit;
- fill mismatch is handled deterministically.

## Performance

- startup profile is accepted;
- tick hot path is bounded;
- caches are bounded and correct;
- UI rendering is throttled;
- memory/allocation profile is documented.

## Testability

- fake broker exists;
- deterministic replay exists;
- execution/lifecycle failure tests exist;
- outcome events are idempotent and traceable.

## OSS

- production OSS packages are minimal;
- research packages are isolated;
- numerical benchmark is reproducible;
- license/compatibility evidence exists;
- no OSS package is a second trading engine.

## Release

- Local Release Gate PASS;
- controlled cTrader validation PASS;
- broker compatibility evidence PASS;
- rollback procedure exists.

## Cloud

- cloud host reuses local contracts;
- cBot behavior matches local decision/execution intent;
- cloud restart/recovery is validated.

## Learning

- sufficient outcome data;
- calibrated confidence;
- drift detection;
- bounded adaptation;
- approval and rollback;
- learning cannot bypass safety.

---

# 9. Execution queue for continuation

The current research milestone Track 19.1 and the completed safety-first phases through Phase 1.4 are recorded above.

Deep project audit continuity record: `docs/DEEP-AUDIT-2026-09-29.md`. The certification sequence continues from the next dependency below.

**NEXT: Phase 8.1 — M1 trigger correctness**

Then proceed in dependency order:

\`\`\`
0.1
0.2
↓
1.1 → 1.2 → 1.3 → 1.4 → 1.5
↓
2.x
↓
3.x
↓
4.x
↓
5.x
↓
6.x
↓
7.x
↓
8.x
↓
9.x
↓
10.x
↓
11.x
↓
12.x
↓
13.x
↓
14.x
↓
15.x
↓
16.x
↓
17.x
↓
18.x
↓
19.2 → 19.3 → 19.4 → 19.5
↓
20.x → 21.x → 22.x → 23.x
↓
24.x → 25.x
↓
26.x
↓
27.x → 28.x → 29.x
↓
30.x
\`\`\`

Track 19.1 is intentionally recorded as already complete because it was a
research milestone executed ahead of the main certification queue.

Phases 0.1, 0.2, 1.1, 1.2, 1.3 and 1.4 are also intentionally recorded as complete and
must not be restarted unless a regression is demonstrated.

Do not restart completed historical phases unless a regression is demonstrated.

---

# 10. Cross-chat continuation contract

When a new chat starts:

1. Read this file.
2. Read \`docs/ARCHITECTURE.md\`.
3. Read \`docs/WORKFLOW.md\`.
4. Inspect \`main\`.
5. Confirm the current commit.
6. Find the first phase marked \`next\`.
7. Implement only that phase.
8. Run the relevant static/contract/CI checks.
9. Update this file only after the phase is actually complete.
10. Commit the completed phase directly to GitHub.

The roadmap is authoritative for **what comes next**.
The architecture is authoritative for **how ownership works**.
The source and broker-confirmed runtime state are authoritative for **what is
actually true**.


## Phase 5.5 — Visual setup levels and execution controls

Status: complete.

This phase closes the semantic gap where a signal/watch arrow can be visible while Entry/Trigger/SL/TP levels are absent, without turning presentation state into an executable Plan. It also fixes Auto Trading / Auto Orders quick controls through explicit ToggleButton Checked/Unchecked events and the existing runtime authority.

Acceptance:
- setup levels use one shared renderer with executable plan levels;
- setup preview cannot submit orders;
- quick execution controls mutate only the canonical runtime flags;
- no second gate, trading engine or decision authority is introduced;
- source, runtime and cTrader compile gates are green before merge.

The next strategy-quality phase remains isolated: stronger signal selection, stronger entry/TP/SL level selection, higher RR and smart trailing.


## Phase 5.6 — Responsive panel runtime

Status: complete.

The panel now refreshes independently from full analysis through a 500 ms ready-state heartbeat, while broker safety supervision remains one-second bounded. Startup panel construction is lighter through lazy row allocation, duplicate property writes are skipped, one canonical visual snapshot is reused per refresh, and initialization/calculation freshness is observable. This is a runtime/UI optimization only; strategy and execution semantics remain unchanged.

Verification: Source / Architecture PASS; Runtime Acceptance PASS; cTrader Compile PASS.

Next: Phase 6.1 — Decision closed-bar contract.

## Runtime UI / protection correction — 2026-09-29

Status: complete (corrective hotfix merged 2026-09-29).

User-reported runtime defects addressed:
- AUTO TRADE and AUTO ORDERS panel controls use a single direct-click operator boundary tied to canonical runtime flags;
- setup-preview Entry/Ideal/Trigger/SL/TP levels use compact semantic name/price boxes;
- setup execution geometry no longer reprices on raw quote movement;
- structural trailing no longer constructs protection as a raw market-price +/- distance;
- audible signal alerts are mirrored by non-authoritative on-chart signal markers;
- prediction objects are rendered from the live presentation path when not superseded by a plan/pending/setup-preview state;
- reversal LIMIT pending orders select materially future structural levels rather than the current market price, using M5/M15 FVG, order blocks, swing/equal-liquidity structure, MTF context and indicator/OSS confluence;
- predictive confluence is source-deduplicated and remains behind the existing smart-quality and broker-confirmation gates.

No new public parameters were introduced. The existing 535-parameter contract remains intact.

Acceptance:
- source/architecture verification: PASS;
- runtime acceptance contracts: PASS;
- cTrader compile: PASS;
- PR #28 merged into `main` as `c8fd3db7761d45b094986105e585a49b72645ba5`;
- hands-on cTrader chart/broker validation: still required;
- local pull is required now because `main` has advanced.



## Corrective Hotfix — Chart Lines and Execution Status UI — 2026-09-29

Status: complete; merged after all three automated gates passed.

User runtime validation identified two remaining presentation/control defects after earlier hotfixes:
- compact plan lines could stop before the latest chart candle because the right edge was tied to an M5-to-chart time mapping;
- AUTO TRADE / AUTO ORDERS quick controls could act as runtime overrides rather than guaranteed editors of the public cTrader settings.

Correction:
- PlanLineRenderer now owns a canonical 40-bar compact span ending at the latest chart candle; FullWidthLevelLines remains an explicit full-series mode;
- pending level rendering no longer depends on mapping a disposable M5 anchor;
- label coordinators reuse the canonical plan-line left edge;
- AUTO TRADE / AUTO ORDERS are now modern switch-style status indicators, not action buttons;
- status indicators consume the canonical EnableAutoTrading / EnableAutomaticOrders state through EnsureExecutionRuntimeState();
- legacy interactive execution-control fields and handlers are removed from the UI boundary;
- tools/audit_runtime_ui.py adds machine checks for these invariants.

No decision, signal, risk, RR, SL/TP, broker-mutation, predictive-pending or lifecycle authority changed.

Verification: Source / Architecture PASS; Runtime Acceptance Contracts PASS; cTrader Compile PASS. Hands-on cTrader validation remains required.

Next planned strategy phase after this corrective hotfix: Phase 7.3 — Semantic Duplicate Audit.


## Phase 8.2 — Swing Plateau Correctness and Structural Evidence Identity (2026-09-29)

**Status: MERGED AND VERIFIED.** Phase branch `phase-8-2-swing-plateau-correctness` was merged into `main` as `a9c63bb3e563126753206c49b070763108919b74`. Scope completed: canonical plateau identity, non-chaining equal-level clusters, causally established sweep levels, and one-event structural BOS/MSS/CHOCH evidence. Pre-merge gates on head `5df5931828719fb635ec67fa59d57b519d4e70e7`: Runtime PASS; cTrader Compile PASS; Source/Architecture PASS. Post-merge main gates on `a9c63bb3e563126753206c49b070763108919b74`: Runtime PASS; Build PASS; Source/Architecture PASS. Target cTrader replay remains required for empirical signal-quality measurement. Next phase: Phase 8.3 — FVG mathematical audit.


## Phase 8.5 — Zone Confluence Symmetry and Timely M1 Trigger Runtime — 2026-09-29

Status: implementation complete on branch; final automated verification pending.

Scope completed:
- introduced a live closed-M1 TriggerRuntimeState tied to the canonical closed-M5 decision;
- allowed a fully closed M1 confirmation to unlock the selected M5 decision inside the currently-forming M5 window without using unclosed M5 OHLC as evidence;
- removed the frozen transient M1-direction veto from DecisionConfirmationGates;
- allowed automatic plan creation to retry on a new M1 confirmation revision within the same M5;
- synchronized exact M1 trigger confirmation into the canonical SignalVisualSnapshot and chart marker;
- made Trigger marker visibility independent from the general signal arrow through ShowTrigger;
- centralized generic zone overlap semantics in ZoneConfluenceRule while preserving FvgRule as the canonical FVG/OB confluence owner;
- aligned OB liquidity-sweep evidence with the canonical confirmed-swing penetration/reclaim liquidity engine;
- removed neutral RSI=50 / DMI=0 from directional trigger scoring;
- added deterministic runtime contracts for live M1/M5 timing and symmetric zone confluence.

Reference-indicator boundary:
- audited FVG logic continues to use canonical FvgRule;
- exact custom WaveTrend source is not available in the current repo/Library continuation, so no guessed formula is enabled;
- future WaveTrend integration remains an exact closed-bar confluence adapter with parity tests.

Acceptance boundary:
- no public parameters intentionally added;
- no second decision authority;
- no new broker mutation ownership;
- target cTrader replay remains required for empirical signal-quality and visual-timing validation.

Next phase is not advanced until Phase 8.5 final CI and merge verification close.


## Phase 8.5 verification closeout — 2026-09-29

Phase 8.5 merged as PR #39 into `main` at merge commit `cbda7910ad3b30fbd74cc526676e6372cb098cd7`.

Pre-merge head `3e77b974cc00c82e9f134bfd0057493ea47c29e7` passed Runtime Acceptance, cTrader Compile/Build and Source/Architecture. Post-merge verification on `cbda7910ad3b30fbd74cc526676e6372cb098cd7` also passed all three gates.

The exact custom WaveTrend source remains unavailable in the current searchable repo/Library continuation, so no guessed WaveTrend formula was introduced. Target cTrader replay remains required for empirical signal-quality and chart-timing validation.

Next strategy-quality work must start from the verified Phase 8.5 main commit. Operator pull requirement: local `main` is now advanced to `cbda7910ad3b30fbd74cc526676e6372cb098cd7`; pull locally before continuing.


## Phase 9.1 — Top-down opportunity calibration and runtime responsiveness — 2026-09-29

Status: implementation in progress on branch phase-9-1-topdown-evidence-runtime-responsiveness.

Scope:
- H1/H4/D1/W1 directional anchor;
- M30/M15 calibration;
- M5 setup/entry alignment;
- M1 closed trigger confirmation;
- actionable EntryAllowed only after explicit top-down calibration under the existing higher-TF agreement setting;
- state-change-driven full panel rendering with lightweight heartbeat live rows;
- bounded live structural SL/target pulse using closed M5 structure;
- immediate same-cycle target refresh after successful TP1/TP2;
- explicit ALERT BUY/SELL label semantics.

Acceptance:
- strong HTF anchor cannot be overridden by lower frames;
- deterministic top-down contracts pass;
- panel heartbeat does not invoke full panel layout rendering;
- target/protection progression remains monotonic and broker-confirmed;
- Runtime Acceptance, cTrader Compile/Build and Source/Architecture all pass;
- no empirical performance claim without target cTrader replay.

Next continuation point remains this phase until final verification and merge close.


## Phase 9.1 verification closeout — 2026-09-29

Status: MERGED AND VERIFIED at implementation level.

PR #40 merged into main as `b0e17e93551b3760d50bb38bb4725b9c523afddd` from verified head `4530f4043195378f58727db8a395121c43abcd52`.

Pre-merge gates on the final implementation head: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture PASS.

Core result:
- H1/H4/D1/W1 provide the directional anchor;
- M30/M15 calibrate that anchor;
- M5 is the setup/entry frame;
- M1 remains closed-bar confirmation only;
- strong HTF conflict cannot be overridden by lower frames;
- calibrated setups require an HTF-backed TP1 candidate;
- panel full rendering is state-change driven while the heartbeat updates lightweight live rows;
- structural target/protection progression can pulse on already-closed M5 structure and TP1/TP2 hits trigger same-cycle target re-evaluation;
- the chart alert mirror is explicitly labeled ALERT BUY/SELL.

Empirical boundary: target cTrader replay is still required to measure actual terminal responsiveness, visual placement, signal latency and realized risk/reward behavior. No profitability, win-rate or false-signal reduction claim is inferred from CI.

Operator action: local main is now ahead of the previously verified baseline and must be pulled before the next phase.


## Phase 9.2 — Parallel opportunity lanes, WaveTrend evidence and non-overlapping chart labels — 2026-09-29

Status: implementation in progress on branch phase-9-2-parallel-opportunities-wavetrend-visual-lanes.

Scope:
- preserve the Phase 9.1 top-down Strategic lane;
- evaluate independent LTF Tactical candidates in both BUY and SELL directions;
- permit Counter-HTF Tactical candidates only with stricter quality/RR requirements;
- keep single-plan execution capacity unchanged until a dedicated multi-plan registry/position-isolation phase is implemented;
- integrate the user's exact CUSTOMWAVETREND source cascade as bounded market-frame evidence;
- render multiple opportunity candidates under isolated chart namespaces without changing existing level-line length geometry;
- use opaque line-color label backgrounds with luminance-based black/white text;
- expose WaveTrend and lane summary in the unified panel.

Acceptance:
- tactical LTF opportunities are not suppressed merely because the Strategic HTF lane is not calibrated;
- strong HTF conflict raises the tactical quality/RR requirements rather than deleting every LTF opportunity;
- WaveTrend is evidence, not a second signal authority;
- multiple candidates use unique visual object IDs and stale objects are removed deterministically;
- existing plan line geometry remains unchanged;
- Runtime Acceptance, cTrader Compile/Build and Source/Architecture must pass;
- numerical WaveTrend parity and empirical signal-quality improvement remain replay-validation items.

Next continuation point: finish verification and merge Phase 9.2, then pull main before Phase 9.3.


## Phase 9.2 verification closeout — 2026-09-29

Status: MERGED AND VERIFIED at implementation level.

PR #41 merged into main as `aedceba3f6d9e3791328f41c9e1fb01e2a474067`.

Final verified PR head before merge: `c29608de510ebeb675c10c0438ede0cfcfbf59e5`.

Pre-merge gates on the final code head:
- Runtime Acceptance PASS;
- cTrader Compile/Build PASS;
- Source/Architecture PASS.

Implemented:
- Strategic HTF opportunity lane remains intact;
- Tactical LTF opportunities are evaluated independently for BUY and SELL;
- strong HTF conflict does not erase every LTF setup; Counter-HTF Tactical uses stricter quality/RR;
- parallel chart candidates use isolated namespaces and separated label anchors;
- existing line-length geometry is reused unchanged;
- compact labels use solid semantic line-color backgrounds with automatic black/white contrast text;
- recovered CUSTOMWAVETREND source is implemented as bounded evidence;
- panel exposes WaveTrend/lane state while Phase 9.1 responsive heartbeat behavior remains intact.

Execution boundary:
- current engine still has a singleton executable plan/protection context;
- Phase 9.2 therefore supports parallel detection/presentation, not simultaneous multi-position auto execution;
- Phase 9.3 is reserved for isolated plan registry, position-scoped protection state, and independent broker mutation identities before multi-position automation.

Empirical boundary:
- numerical WaveTrend parity against the target cTrader terminal and real-world chart readability/overlap still require target-terminal replay;
- no empirical win-rate, false-signal or realized-RR improvement is claimed from CI.

Operator action:
- local main must be pulled before the next continuation.


## Current Track 9 status — 2026-09-29

| Phase | Status | Note |
| --- | --- | --- |
| 9.1 Evidence duplication | COMPLETE | Evidence ownership/correlation controls are in main. |
| 9.2 Confidence semantics + parallel opportunities + WaveTrend evidence | COMPLETE | Multi-lane detection/presentation and exact WaveTrend evidence are in main. |
| 9.3 Empirical calibration | COMPLETE | Contextual prequential calibration is merged in PR #42; CI passed on the final implementation head. |
| 9.4 Regime-conditioned intelligence | NEXT | Context-specific weighting remains a separate phase. |
| 9.5 No-trade intelligence | NEXT | Explainable no-trade state synthesis remains a separate phase. |

Phase 9.3 does not claim measured trading-performance improvement until target-terminal replay/historical evaluation is completed.


## Phase 9.5 — Actionable Entry Coherence and Turning-Point Risk — 2026-09-29

Status: VERIFIED COMPLETE on `phase/9-5-actionable-signal-execution-coherence-v2`.

This is a corrective strategy-quality continuation of the Phase 9.4 actionability/divergence work already present on `main`.

Scope:
- one canonical actionable entry alert for Confirmed/High/Smart thresholds;
- no independent entry alert when a Plan is merely activated;
- directional entry arrow only when `ActionableNow` is true;
- M1 trigger marker is non-directional so it cannot be mistaken for a second entry;
- final market entry gate explicitly consumes the same `ActionableNow` state used by Plan creation and alerts;
- turning-point risk from recent M5 range position and adverse M5/M1 momentum;
- strong opposing regular divergence remains an immediate entry blocker;
- deterministic ranked `TradePlanRegistry` snapshot and best-candidate selection;
- panel visibility for final entry-gate state, reason, quality, RR and divergence;
- regression contracts for turning-point risk and registry ordering.

Behavioral intent:
- BUY near a recent range high is treated as a potential late/chasing setup;
- SELL near a recent range low is treated symmetrically;
- a directional decision while lower-timeframe pressure is materially against the entry is blocked unless the setup is genuinely in the intended execution window;
- insufficient RR and late/extended price remain hard blockers;
- informational WATCH/REACTION states do not create directional entry arrows.

Safety boundary:
- `TradePlanRegistry` supports parallel analysis/presentation;
- broker execution remains single-plan/single-managed-identity until plan-scoped lifecycle, protection and broker mutation identities are isolated.

Verification:
- Runtime Acceptance, cTrader Compile/Build and Source/Architecture workflows are GREEN on the verified head;
- target cTrader replay remains required for empirical false-signal, timing and realized RR measurements;
- no profitability or win-rate claim is inferred from static/contract checks.

Next dependency after verified completion: continue Track 9 intelligence/no-trade work without reopening completed historical phases.


## Phase 9.5.1 — Live Actionability Coherence — 2026-09-29

Status: MERGED AND VERIFIED.

Scope:
- eliminate stale ActionableNow state when decision/execution inputs become unavailable;
- reuse the existing pre-trade plan geometry for live re-evaluation when the structural execution model is intentionally cleared;
- require plan direction and execution mode to agree with the current actionability state;
- prevent pending stop/limit-style plans from being exposed as current market ACTION BUY/SELL opportunities;
- refresh the same actionability state immediately before automatic market submission;
- classify ACTION alert presentation consistently as a confirmed event.

Acceptance:
- current quote changes can invalidate market actionability without waiting for a new M5 decision;
- a pending plan cannot coexist with a current-market actionable arrow/alert for the same pre-trade state;
- automatic market execution uses the refreshed actionability state;
- Runtime Acceptance, cTrader Compile/Build and Source/Architecture all pass.

Verification closeout:
- PR #46 merged into `main` as `573f12ac380a8beace61db780082d5071db2defb`;
- final verified PR head before merge: `91b2a128e39bffff145fb84b2371e37057cc95b9`;
- Runtime Acceptance PASS;
- cTrader Compile/Build PASS;
- Source/Architecture PASS.

Empirical boundary: target-terminal replay is still required for exact chart timing, visual behavior and realized risk/reward measurement.


## Phase 9.6 — Signal Quality & Visual Coherence

Status: VERIFIED COMPLETE on `phase/9-6-signal-quality-visual-coherence`.

Pre-merge verification on head `9e001449ee852289676aa29d1907174b96ce1b44`: Runtime Acceptance PASS, cTrader Compile/Build PASS, Source/Architecture PASS. Target cTrader replay remains required for empirical visual/signal-quality validation.

The phase hardens the final actionable state without creating a parallel signal authority. The same stricter quality rule is applied during closed-bar decision construction and during live quote re-evaluation.

Presentation changes:
- weak directional WATCH states are no longer rendered merely because a non-zero direction exists;
- reaction/prediction chart presentation requires stronger existing quality evidence;
- directional markers are UpArrow/DownArrow;
- the M1 trigger is a non-directional Circle;
- compact level-label backgrounds stay attached to their text and use bounded dimensions;
- parallel opportunity labels reuse the canonical compact anchor;
- low-quality parallel candidates are suppressed from presentation.

Verification requires Runtime Acceptance / Decision Contracts, cTrader Compile/Build, and Source/Architecture. Target-terminal replay is still required for empirical signal timing, chart readability, false-signal frequency and realized RR measurement.

Detailed record: `docs/PHASE-9-6-SIGNAL-QUALITY-VISUAL-COHERENCE.md`.


## Phase 9.7 — Regime-Aware No-Trade & Auto-Execution Hardening

Status: VERIFIED COMPLETE on `469f66bd2d461016c9283e0e4243ca429c694aa3`.

Pre-merge gates: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture PASS.

Phase 9.7 merge closeout: PR #49 merged into `main` as `954e5021648e43a11d0de08f35b7fc7aaa8d2125`. The next continuation starts from this verified main baseline. Operator must pull local `main` before continuing.

This continuation concentrates strategy quality and automatic execution on one shared pipeline. RANGE/COMPRESSION now receive a canonical specialist no-trade rule; automatic market orders use bounded Market Range submission; live suitability and spread-to-stop-risk are rechecked immediately before broker mutation; pending orders are revalidated/cleaned when the regime invalidates them; compact level labels now use explicitly opaque backgrounds.

No new public parameters, second decision authority, or multi-position broker authority were introduced.

Detailed record: `docs/PHASE-9-7-REGIME-AUTO-EXECUTION-HARDENING.md`.


## Phase 9.8 — Indicator Fusion & Trade Quality

Status: VERIFIED COMPLETE on `ae7abf3f169d0743e56be85f59f0d9ffa5081069`.

Final pre-merge gates: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture and project audits PASS.

Merge closeout pending.

Focus:
- regime-aware fusion rather than raw indicator vote stacking;
- conflict-aware Smart Quality;
- canonical propagation of indicator quality into Decision and automatic execution;
- stronger auto-entry rejection when indicator evidence is weak/conflicted;
- white, background-free level labels.

The next hardening opportunity after this phase is server-side advanced protection migration and deeper target-terminal replay/calibration, subject to the project's cTrader API version and empirical verification.

Detailed record: `docs/PHASE-9-8-INDICATOR-FUSION-TRADE-QUALITY.md`.


Phase 9.8 merge closeout: PR #50 merged into `main` as `37cfd761bbb439d6e154315995664721b6c31740`. Local `main` must be pulled before the next continuation.

## Phase 9.9 — Signal / Execution / Protection Coherence — 2026-09-29

Status: VERIFIED COMPLETE on `98ac6f0844abb1171f70d7d5eaa50e8fdf623134`.

Final automated gates: Runtime Acceptance #1048 PASS; cTrader Compile/Build #1232 PASS; Source/Architecture #1239 PASS.

PR #51 merge closeout remains pending. Target-terminal cTrader replay remains required for empirical behavior and realized trading outcomes.

Scope:
- canonical live indicator-fusion actionability gate with stale-frame rejection;
- server-side partial TP ladder for valid TP1/TP2/final configurations;
- suppression of duplicate local TP mutation/target progression when broker-side ladder is active;
- pending-fill adoption of broker-side TP ladder;
- preservation of the white, background-free level-label contract;
- no new public parameters and no second decision/execution authority.

Detailed record: `docs/PHASE-9-9-SIGNAL-PROTECTION-COHERENCE.md`.


## Phase 9.10 — Smart Auto-Trade / Auto-Order Protection & Accumulated Audit — 2026-09-29

Status: VERIFIED COMPLETE; PR #52 merged into `main` as `3f82fd9ad35ff33aafb9216c325524878e368a3a`.

Mandatory per-phase improvements:
- automatic market, aggressive and pending-order paths must receive a concrete hardening change;
- smart structural SL / TP and broker protection must be audited and kept under one authority;
- accumulated duplicate/stale/direction-conflict issues must be checked each phase;
- all signal/plan level lines use Solid;
- level text remains white and background-free.

Current implementation:
- deterministic `SmartBreakEvenRule`;
- broker/server break-even transport on eligible advanced protection;
- local break-even yields to broker-owned server break-even;
- accumulated auto-trade/protection audit added to CI;
- all signal/plan level lines forced to Solid.

Final documentation-inclusive verification is green: Runtime Acceptance #1070 PASS; cTrader Compile/Build #1254 PASS; Source/Architecture #1261 PASS.


## Phase 9.11 — Automatic Execution Telemetry & Deeper SL/TP Coherence — 2026-09-29

Status: VERIFIED COMPLETE; PR #53 merged into `main` as `43101635e24ad15b77374472fc676a8c6fe591d6`.

Final automated verification on verified phase head `20ba01d4daa145f1118d3795277ed4d6f6a3bed3`: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture PASS, including accumulated audit.

Completed scope:
- deterministic bounded visual lifecycle for pre-trade plans and setup previews;
- high-quality one-dimension actionability recovery without weakening hard conflict/RR/trap blockers;
- legacy `ALERT BUY/SELL` chart mirror removed;
- blocked/restricted signal delivery is silent before sound/popup/email/visual side effects;
- alert popup defaults moved to a readable bottom-left presentation;
- all plan/prediction signal lines fixed to thickness 1 and remain Solid;
- broker submission confirmation/rejection/null-result telemetry exposed in the auto-trade panel;
- server-side SL/TP protection ladder rejects structurally invalid stop-side geometry before adoption;
- accumulated audit extended across lifecycle, alert, popup, line-thickness and telemetry contracts;
- no new public parameters.

Detailed record: `docs/PHASE-9-11-SIGNAL-LIFECYCLE-QUALITY-ALERTS.md`.

Next phase after verification: Phase 9.12 — broker outcome/recovery telemetry and historical signal lifecycle calibration.



## Phase 9.12 — Broker Outcome / Recovery Telemetry & Recent Lifecycle Calibration — 2026-09-29

Status: VERIFIED COMPLETE; merged into `main` as PR #54.

Completed scope:
- bounded 128-observation broker-confirmed outcome history;
- centralized/idempotent close-outcome recording with lane/regime/confidence context;
- realized-R and lifecycle-duration telemetry;
- recent-first contextual empirical calibration with exact/context/directional fallback;
- bounded 64-record submission/recovery telemetry history;
- explicit RecoveryRequired/resolution telemetry transitions;
- compact panel diagnostics for recent outcomes and broker trace state;
- Decision Contract coverage and accumulated-audit enforcement;
- no public parameter changes and no second decision/execution authority.

Known boundary:
- recent history remains in-memory and is cleared on indicator restart;
- target cTrader replay is still required for empirical signal timing, lifecycle duration, realized R, broker event ordering and actual signal-quality validation.

Detailed record: `docs/PHASE-9-12-OUTCOME-RECOVERY-TELEMETRY-CALIBRATION.md`.

Next phase after verification: Phase 9.13 — target-terminal lifecycle replay and outcome calibration validation.
Operator action: local main should not be advanced from this branch until the phase is verified and merged.


Verification:
- Runtime Acceptance #1096 PASS
- cTrader Compile/Build #1280 PASS
- Source/Architecture + accumulated audit #1287 PASS
- Decision Contracts PASS within the compile workflow
- final verified branch head `626618e7100b2e2cecf8a172d67ed49aee43345b`; merge commit `033bad1555fc7e2e1780c126d501020ddb7c7737`

Next phase after merge: Phase 9.13 — target-terminal lifecycle replay and outcome calibration validation.
Operator action: pull local `main` after PR #54 merge.



## Phase 9.13 — Persistent Outcome Memory, Adaptive Risk & Optimization Routine — 2026-09-29

Status: VERIFIED COMPLETE; merged into `main` as PR #55.
Branch: `phase/9-13-persistent-memory-safe-optimization`

Completed scope:
- persistent 128-record outcome memory through cTrader LocalStorage;
- 90-day restore age boundary and malformed-record rejection;
- symbol/timeframe/configuration-scoped memory;
- recent-first empirical calibration continuity across restarts;
- conservative adaptive risk reduction from recent realized outcomes;
- optimization-readiness audit added to permanent Source/Architecture CI;
- adaptive risk Decision Contracts;
- unchanged 552-parameter public contract and single decision/execution authority.

Known boundary:
- LocalStorage persistence is available in real-time; cTrader documents that local storage is in-memory during backtesting/optimisation, so replay/optimization must still establish empirical behavior independently. citeturn380699search2
- target-terminal replay is required to validate actual restart persistence, signal timing, realized R and false-signal behavior.

Detailed record: `docs/PHASE-9-13-PERSISTENT-MEMORY-SAFE-OPTIMIZATION.md`.

Next phase after verification: Phase 9.14 — target-terminal replay, calibration/optimization measurement and evidence-driven parameter refinement.
Operator action: pull `main` now.

 
Verification:
- Runtime Acceptance #1107 PASS
- cTrader Compile/Build #1291 PASS
- Source/Architecture + accumulated audit #1298 PASS
- Decision Contracts PASS within build
- final verified code head `b742aae2dd1f94452f41743ad749374433dbc763`
 
Next phase after merge: Phase 9.14 — target-terminal replay, calibration/optimization measurement and evidence-driven parameter refinement.
Operator action: pull `main` after PR #55 merge.

Merge commit: `6246782125718af184568eb9339965e3f228c9a7`.


## Phase 9.14 — Signal Evidence Integrity & Consensus Calibration — 2026-09-29

Status: VERIFIED COMPLETE; merged into main as PR #56.
Verified merge commit: 16e788f6b196afcfe2580908cbdb4dabc46cb5b.

Completed scope:
- directional frame contribution now accounts for absolute bull+bear strength and canonical evidence coverage;
- weak concentrated frames are softened toward neutral instead of becoming full-strength votes;
- strong evidence retains high directional influence;
- BUY/SELL symmetry and deterministic behavior are covered by Decision Contracts;
- no global thresholds were blindly raised;
- no execution/risk/trade-plan authority was duplicated;
- public parameter count remains 552.

Key finding:
The prior contribution model could turn relative directional dominance from a low-evidence frame into an overly strong MTF vote. The correction preserves direction while making evidence strength part of directional influence.

Detailed record: docs/PHASE-9-14-SIGNAL-EVIDENCE-CALIBRATION.md.

Known boundary:
Target-terminal/replay measurement is still required to establish empirical changes in false-signal frequency, missed opportunities, realized R and execution behavior.

Verification: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture + accumulated audits PASS; Decision Contracts PASS within build.

Next phase: Phase 9.15 — target-terminal replay/measurement and evidence-driven parameter refinement.
Operator pull: required now; pull main to the latest closeout commit.


## Phase 9.15 — Startup Responsiveness & Portable Long-Term History — 2026-09-29

Status: VERIFIED COMPLETE; merged into main as PR #57.
Verified merge commit: 365abb790a49e85ad58c87aa5a92f49d37b7f77c.

Completed scope:
- reduced primary startup readiness thresholds to the actual minimum closed history required by the canonical analyzers;
- D1/W1 are no longer included in the blocking startup pending-load count;
- optional D1/W1 callbacks are adopted after core startup and invalidate MTF context cache;
- startup panel header reports loading status and core timeframe bar counts;
- added portable append-only CSV outcome archive in the cTrader designated indicator folder;
- archive rotation uses deterministic fixed 90-day UTC buckets;
- previous archive files are never deleted;
- long-term archive aggregates feed empirical calibration only after recent history is considered;
- archive import is deferred until after the first startup calculation seed;
- no public parameters or execution authorities were added.

Known boundary:
Target-terminal measurement remains required for actual startup latency and live signal timing. The archive learning layer is configuration-scoped for safety; moving across operating systems requires copying both cTrader LocalStorage and the indicator History folder.

Detailed record: docs/PHASE-9-15-STARTUP-PERSISTENT-HISTORY.md.

Verification: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture + accumulated audits PASS; startup/persistence audit PASS; Decision Contracts PASS within Build.

Next phase: Phase 9.17 — target-terminal replay of Phase 9.16 traces and evidence-driven gate refinement.
Operator pull: required now; pull main to the latest closeout commit.


## Phase 9.16 — Signal Measurement, OB/FVG Location Fusion & Execution Safety — 2026-09-29

Status: VERIFIED COMPLETE; merged into `main` as PR #58.

Phase 9.16 adds evidence attribution before further signal tuning:
- canonical closed-M5 gate tracing for CONSENSUS -> DECISION-FILTER -> TRIGGER -> ACTIONABILITY -> ACTIONABLE;
- bounded canonical OB/FVG location scoring, with OB+FVG as the strongest single location feature;
- correlated FVG/OB location evidence remains one evidence unit;
- append-only 90-day signal trace archives and an offline forward-window analyzer;
- latest trace gate/reason exposed in the panel;
- final automatic-market plan Entry/Stop/TP1 geometry revalidation before broker mutation.

No new public parameters and no second decision/execution authority were introduced.
Trace measurement is observational and never feeds live decisions.

Target-terminal replay remains required before empirical claims about missed opportunities,
false-signal rate, entry timing, realized R or trading outcomes are made.

Next phase: Phase 9.17 — target-terminal replay of Phase 9.16 traces and evidence-driven gate refinement.
Operator pull: required after the final verified Phase 9.16 main closeout.


## Phase 9.16 verification closeout — 2026-09-29

Verified merge commit: `fd6f43594ac1a50a67ccd3007c661a2ba059374f`.
CI: Runtime Acceptance #1172 PASS; cTrader Compile/Build #1356 PASS; Source/Architecture + accumulated audits #1363 PASS.

Next phase: Phase 9.17 — target-terminal replay of Phase 9.16 traces and evidence-driven gate refinement.
Operator action: pull `main` now.


## Phase 9.17 — Exit Geometry, TP Progression & Protection Integrity — 2026-09-29

Status: IMPLEMENTED; CI verification pending.

Completed scope:
- canonical live exit geometry with forward-only TP and protective SL validation;
- live TP progression prevented from regressing behind the current market;
- live target enrichment made monotonic;
- actual-fill exit reconciliation made live-aware and transactional;
- unsafe post-fill fallback removed;
- server-side TP progression restored before and after partial realization;
- post-TP2 final-target continuation enabled;
- broker TP mutation hardened against backwards movement;
- broker-distance-aware target spacing and final SL geometry validation added;
- deterministic exit Decision Contracts and dedicated exit audit added.

Detailed record: docs/PHASE-9-17-EXIT-GEOMETRY-PROGRESSION.md.

Target-terminal validation remains required for live timing, broker/server-side protection,
slippage, realized R and confirmation of the reported TP regression fix.

Next phase: Phase 9.18 — target/protection measurement and evidence-driven exit refinement.
Operator pull: required after final verified closeout.

## Phase 9.17 verification closeout — 2026-09-29

Status: VERIFIED COMPLETE; target-terminal replay still required.

Verification:
- Runtime Acceptance #1193 PASS
- cTrader Compile/Build #1377 PASS
- Source/Architecture + accumulated audits #1384 PASS
- Decision Contracts PASS within Build
- Phase 9.16 signal measurement audit PASS
- Phase 9.17 exit geometry audit PASS
- Verified head: `c027a983081102ace0353d064219a5aad739ed94`

Next phase: Phase 9.18 — target/protection measurement and evidence-driven exit refinement.
Operator action: merge PR #60, then pull local main to the verified merge commit.


## Phase 10 — MTF Scenarios, Clear Level Labels, Runtime Logs & Development Routine — 2026-09-29

Status: VERIFIED and MERGED to main.

Merge commit: `fd29b955c557df680e40d8fe4151955600160408`.
Final code commit verified before merge: `cef178da5f0359ef2c0800376b5b9beddf295424`.

Scope:
- give each simultaneous timeframe opportunity a stable ScenarioId and SourceTimeframe;
- allow M5/M15/M30/H1/H4/D1/W1 opportunities to coexist without cross-timeframe same-direction collapsing;
- identify each scenario consistently across ENTRY/SL/TP objects;
- show main plan as (MTF);
- show absolute SL/TP distance from Entry in pips;
- keep a visible horizontal gap between line endpoint and label;
- add unified runtime CSV logging for decisions, predictions, scenarios and execution/state transitions;
- add an offline runtime-log analyzer;
- add ROUTINE.md so recurring engineering and reporting tasks are not forgotten.

Design boundary:
The source timeframe controls scenario provenance and directional/quality evidence. Execution geometry continues to use the tested M5 execution model in this phase; this avoids introducing an untested second broker-execution authority. Independent scenarios remain non-authoritative for auto-trading.

Verification gates:
- source/architecture checks;
- cTrader compile/build and runtime acceptance;
- parameter usage audit;
- scenario coexistence/deduplication audit;
- UI object ownership/cleanup audit;
- runtime log schema smoke test;
- terminal replay for broker-side behavior;
- outcome/replay study before any empirical accuracy or expectancy claim.

Next phase after closeout:
Phase 10.1 — evidence-driven multi-timeframe scenario geometry, per-scenario execution policy design subject to audit, and deeper log/outcome analytics.


## Phase 11 — Economic News Guard, Reference Reassessment & Calibration — 2026-09-29

Status: VERIFIED COMPLETE; merged into main as `43f9bf606fedffa7f712734e00248e161bef42d7`.

Scope:
- promote the useful weekly economic XML feed from the user-provided reference indicator into CFIP;
- make news a first-class market gate for new decisions and final auto-trade safety;
- refresh feed on Timer/cache instead of tick-by-tick networking;
- cancel managed pending orders before high-impact events;
- optionally close active managed positions before high-impact releases;
- log news risk state and event identity for later forensic review;
- preserve the canonical internal WaveTrend and FVG engines after reference parity reassessment;
- automate News Guard source auditing.

Next after closeout:
Phase 11.1 — Auto Execution, Indicator Identity, Level Presentation & History Discoverability.


## Phase 11.1 — Auto Execution, Indicator Identity, Level Presentation & History Discoverability — 2026-09-29

Status: VERIFIED COMPLETE; merged into main as `8be58c289566711f8b80caa741012edc94b8a523`.

Completed:
- explicit CFIPIndicator cTrader identity and aligned assembly name;
- removed same-M5 automatic-plan actionability latch;
- history location marker and startup diagnostic;
- automatic trade/order block-reason visibility;
- initial plan label separation hardening.

Detailed record: `docs/PHASE-11-1-AUTO-EXECUTION-HISTORY-PRESENTATION-HARDENING.md`.

Next phase: Phase 11.2 — Analysis / Signal / Execution Freshness & Panel Heartbeat.

## Phase 11.2 — Analysis / Signal / Execution Freshness & Panel Heartbeat — 2026-09-29

Status: VERIFIED COMPLETE; merged into main as `8102cda74059da88e5be224a76a64dd0683ec366`.

Focus:
- remove obsolete/dead compile warnings;
- harden horizontal and vertical line/text separation;
- expose canonical Decision/Trigger/Actionability/Plan pipeline state;
- refresh quote-sensitive actionability immediately before automatic execution;
- add lightweight pulsing processing lamp to the panel;
- make deep analysis/signal/auto-trade/auto-order review permanent in ROUTINE.md.

Detailed record: `docs/PHASE-11-2-ANALYSIS-SIGNAL-EXECUTION-HEARTBEAT.md`.

Next after closeout: Phase 11.3 — execution rejection forensics, missed-actionable cohorts and evidence-driven threshold refinement.

## Phase 11.3 — Execution Rejection Forensics, Missed-Actionable Cohorts & Threshold Evidence — 2026-09-30

Status: VERIFIED COMPLETE; merged into main as `6eebf25f4e5604f92acdd51c73c92cfa1c83be73`.

Implementation:
- added offline `tools/analyze_phase_11_3.py` for unified signal-trace and runtime-log forensics;
- normalized execution rejection/failure cohorts by category, path, state and reason;
- correlated execution events with same-configuration closed-M5 signal gates;
- added potential missed-actionable and adverse-actionable forward cohorts using MFE/MAE diagnostics;
- surfaced OB+FVG, WaveTrend, indicator-fusion and MTF evidence inside missed-actionable cohorts;
- added near-threshold evidence around the existing confidence/quality/entry/RR boundaries;
- allowed an optional offline threshold snapshot JSON without changing live parameters;
- added `tools/audit_phase_11_3.py` and wired it into Source/Architecture CI;
- documented the uploaded Swing reference assessment and kept the canonical CFIP swing owner unchanged.

Threshold policy:
No automatic live threshold was changed in Phase 11.3. Real cTrader logs/replay/outcomes are required before any threshold refinement.

Verification:
Phase 11.3 audit, accumulated Source/Architecture, Runtime Acceptance and cTrader Compile/Build are required. Target-terminal replay remains required for broker rejection semantics and empirical signal-quality measurements.

Next phase: evidence-backed refinement of the specific owner/gate identified by measured cohorts.

## Phase 11.4 — Plan Reward/Risk Quality, Multi-Scenario Coverage & Execution Hardening — 2026-09-30

Status: VERIFIED COMPLETE; merged into main as `5886989c001ebeac58cde63486f519262b942559`.

Implementation:
- added one shared PlanRewardRiskQualityRule for nominal/effective TP1 RR, stop-risk ATR and spread-aware reward/risk validation;
- made structural stop selection reward-path aware so oversized SL candidates are rejected earlier or deprioritized when they cannot support adequate TP1 reward;
- removed the Tactical/Parallel plan-selection RR bypass by making lane RR respect the canonical Tp1MinimumRR/regime floor;
- applied the same reward-risk gate to live ActionableNow, parallel opportunity construction, automatic market, aggressive market and pending-order final submission validation;
- strengthened parallel scenario identity so distinct timeframe/lane/direction scenarios do not collapse just because their prices are close;
- made visible-scenario capping preserve distinct scenario coverage before filling remaining slots by priority;
- strengthened ROUTINE.md with reward-risk, scenario-identity and final execution-gate requirements;
- added Phase 11.4 source audit and wired it into Source/Architecture CI.

Important boundary:
No public trading threshold was blindly tuned and no second broker-execution authority was introduced. Independent timeframe scenarios remain signal/opportunity objects unless a separately tested scenario execution policy is promoted.

Next phase after verification: **Track 12A — Mandatory Local cBot Separation**, followed by
scenario-aware broker policy only through the dedicated cBot execution authority.


## Phase 11.5 — Scenario-Aware Execution Materialization & Submission Isolation — 2026-09-30

Status: VERIFIED and MERGED to main.

Merge commit: `fd29b955c557df680e40d8fe4151955600160408`.
Final code commit verified before merge: `cef178da5f0359ef2c0800376b5b9beddf295424`.

Scope:
- explicit scenario-to-canonical-plan materialization;
- observe-only boundary for independent timeframe scenarios;
- scenario-scoped SubmissionGate retry/backoff/circuit identity;
- scenario-aware execution telemetry and Auto Trading diagnostics;
- deterministic runtime contracts and Phase 11.5 source audit;
- no change to the certified single-position broker capacity.

Design boundary:
The phase does not introduce a second decision authority, a second execution engine,
or multi-position broker mutation. Independent timeframe scenarios remain opportunities
until a separately tested execution policy is certified.

Verification completed before merge:
- Phase 11.5 audit: PASS;
- accumulated Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile/Build: PASS;
- target-terminal replay for broker semantics and empirical outcomes.

Next phase:
**Track 12A — Mandatory Local cBot Separation**, followed by scenario-aware broker policy only
through the dedicated cBot execution authority.


---

## Track 12A Final-Readiness Audit — 2026-09-30

Status: ROADMAP READY FOR IMPLEMENTATION.

The master and detailed separation roadmap now agree on CBOT-Preflight, CBOT-0 dependency-closure and parameter ownership, Indicator/cBot authority boundaries, one-executor-at-a-time migration, final zero-broker-mutation/fallback gates, and target-terminal runtime/replay requirements.

No production C# behavior changed.


---

## Track 12A Final Phase Ordering — 2026-09-30

The authoritative order is now: `CBOT-0 → CBOT-Preflight → CBOT-1 → CBOT-2 → CBOT-3 → CBOT-4 → CBOT-5 → CBOT-6 → CBOT-7`. CBOT-0 is repository/source inventory; CBOT-Preflight is the no-trade target-terminal capability gate; CBOT-1 and later may not bypass either gate.

No production C# behavior changed.

## CBOT-0 — Boundary Inventory and Execution-Authority Freeze — 2026-09-30

Status: **VERIFIED COMPLETE**

Completed in this phase:
- created the machine-enforced CBOT-0 inventory gate at tools/audit_cbot_boundary.py;
- recorded the direct broker mutation owner matrix in docs/CBOT-0-BOUNDARY-INVENTORY.md;
- froze the current broker/account/lifecycle boundary before any cBot source is created;
- classified the separation as Indicator analytical authority vs cBot broker/account authority, with mixed modules explicitly split by responsibility;
- added execution-related parameter usage-domain classification so parameters are never moved wholesale by file/group;
- wired the CBOT-0 audit into Source/Architecture CI;
- made no production C# behavior change;
- did not create CFIP.Contracts or CFIP.cBot yet.
- final Source/Architecture verify `109870832998`: PASS;
- final Runtime Acceptance `109870832972`: PASS;
- final cTrader Build/Compile `109870832958`: PASS;

Acceptance boundary:
- direct broker calls must remain behind the current known mutation owners until extraction;
- all execution-related parameters must receive deterministic INDICATOR / CBOT / SPLIT classification from source usage;
- public parameter count remains 563 on the current CR1.8-audited main baseline;
- no duplicate public parameter declarations;
- no new broker executor is permitted during the separation track.

Next phase: CBOT-Preflight — target-terminal, no-trade proof of the supported local Indicator → cBot structured read-only handoff.

Operator action after merge: git pull --ff-only.
## CBOT-Preflight — Local cTrader Host Capability Gate — 2026-09-30

Status: IMPLEMENTED; target-terminal verification is the remaining blocking acceptance

Completed:
- validated the intended local integration path against current official cTrader documentation: Manage References + Indicators.GetIndicator<T>();
- added a no-trade preflight probe indicator and cBot under preflight/;
- added tools/audit_cbot_preflight.py and wired it into Source/Architecture CI;
- enforced no broker mutation, reflection, chart scraping, file/HTTP/WebSocket transport or static singleton in the preflight kit;
- documented the target-terminal startup-order matrix and evidence requirements in docs/CBOT-PREFLIGHT.md;
- kept production C# trading behavior unchanged;
- did not create CFIP.Contracts or CFIP.cBot yet.

Important boundary:
- the current CFIP Indicator does not yet expose the final structured public signal/provider surface; that is deliberately owned by CBOT-2;
- therefore this preflight proves the host/reference mechanism and safe no-trade instantiation, while the final CFIP structured handoff remains a CBOT-2 acceptance item;
- CBOT-1 remains blocked until the target terminal verifies the no-trade capability matrix.

Static verification:
- Source/Architecture includes the preflight static gate.
- Build/Runtime are unchanged by the preflight kit.

Next phase after target-terminal acceptance: CBOT-1 — platform-neutral contracts.

Operator action after merge: git pull --ff-only.