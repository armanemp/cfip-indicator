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
- 535 public configuration parameters (532 baseline + 3 OSS extension parameters);
- 27 parameter-group source files;
- 500 method declarations / 467 unique baseline methods.

Automated gate snapshot for the Phase 0.1 verification commit:

- cTrader compile: PASS (workflow run 720);
- runtime acceptance contracts: PASS (workflow run 536);
- source and architecture checks: PASS (workflow run 727).

The verifier reported 398 production C# files, 535 parameters, 500 method declarations and 467 unique baseline methods.

---

# 3. Acceptance hierarchy

CFIP certification is performed in this order:

\`\`\`
Repository truth
    ↓
Architecture / static gates
    ↓
Contract tests
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
| C1 | indicator acting as executor | Track 13, Track 27-29 |
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

Status: planned.

All confirmed decision inputs must reference closed bars at the defined UTC
alignment.

Acceptance:

- no future-bar leakage.

## Phase 6.2 — Reaction intrabar contract

Status: planned.

Explicitly define live reaction as intrabar if retained.

Rule:

\`\`\`
LIVE REACTION     = explicit intrabar feature
CONFIRMED DECISION = closed-bar feature
\`\`\`

Acceptance:

- the two semantics cannot be silently merged.

## Phase 6.3 — Aggressive entry policy

Status: planned.

Select one explicit policy:

- closed-bar only; or
- controlled intrabar with hysteresis and explicit invalidation.

Acceptance:

- no ambiguous mixed-bar policy.

---

# Track 7 — Parameter Semantics

## Phase 7.1 — Hidden-clamp audit

Status: planned.

Audit:

- Math.Max;
- Math.Min;
- Clamp;
- hard-coded floors/ceilings;
- silently overridden parameters.

Acceptance:

- user-facing parameters are behaviorally meaningful.

## Phase 7.2 — Dead/unused parameter audit

Status: planned.

Find:

- declared but unread parameters;
- values read but never consumed;
- settings rendered but not applied.

Acceptance:

- every retained parameter has an explicit runtime owner.

## Phase 7.3 — Semantic duplicate audit

Status: planned.

Compare similar settings such as:

- global confidence;
- pending confidence;
- aggressive confidence;
- quality floors.

Acceptance:

- every retained parameter represents one meaningful concept.

## Phase 7.4 — MaximumOpenPositions semantics

Status: planned.

Decide and encode real semantics:

- single active plan; or
- real multi-plan support.

Acceptance:

- parameter cannot advertise behavior that architecture does not provide.

---

# Track 8 — Analytical Correctness

## Phase 8.1 — M1 trigger correctness

Status: planned.

Verify that M1 trigger mode uses real M1 evidence and is not merely a reweighted
M5 signal.

Acceptance:

- real M1 data path;
- deterministic trigger semantics.

## Phase 8.2 — Swing plateau correctness

Status: planned.

Audit equality behavior for:

- equal highs;
- equal lows;
- plateau structures.

Acceptance:

- equality is handled intentionally.

## Phase 8.3 — FVG mathematical audit

Status: planned.

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

Status: planned.

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

**NEXT: Phase 1.5 — Safety supervisor**

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
