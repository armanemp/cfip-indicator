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

## Phase-sequence integrity note

The canonical Prompt 4 remediation track contains **D1 through D10** only.
A repository-wide search on 2026-10-01 found no authoritative `CR4.11`,
`D11`, `Phase 4.11` or `Prompt 4.11` entry. The authoritative sequence
therefore transitions from **CR4.10 / D10** to **CR5.1 / E1**, and then to
**CR5.2 / E2**. No CR4.11 step is being skipped.

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

# Track CI — Full-Stack Calculation & Analytical Integrity (BLOCKING)

Status: **active — CI-01, CI-02 and CI-03 verified complete; CI-04 is in implementation. The track continues to block continuation of ordinary refinement phases until CI-FINAL closes.**

This track is introduced after the 2026-10-01 deep review of the Trigger →
Entry → SL → TP chain. It intentionally expands the audit upstream so
correctness is established from raw market data and indicator calculations
through analysis, decision, trigger, trade-plan geometry and broker execution.

The authoritative detailed specification is:
`docs/PHASE-CI-FULL-STACK-CALCULATION-ANALYTICAL-INTEGRITY.md`

The track does **not** renumber or invalidate Prompt 4/5/6/7/8 phases. It is a
blocking correctness gate inserted before the next unfinished refinement phase.

Sequence:

```
CI-00  Canonical data / price / time semantics
  ↓
CI-01  Native/primitives indicator mathematics
  ↓
CI-02  OSS numerical parity / warm-up / cache
  ↓
CI-03  Indicator fusion / correlation / evidence independence
  ↓
CI-04  Structure / swing / liquidity semantics
  ↓
CI-05  FVG lifecycle
  ↓
CI-06  Order Block lifecycle
  ↓
CI-07  MTF / regime / market context
  ↓
CI-08  Divergence / WaveTrend / reaction / early signal
  ↓
CI-09  Decision engine mathematics
  ↓
CI-10  Trigger + M1 lifecycle
  ↓
CI-11  Entry geometry + signal latency
  ↓
CI-12  Structural SL
  ↓
CI-13  TP sources + obstacle path + TP ladder
  ↓
CI-14  Canonical Risk / Reward / RR
  ↓
CI-15  End-to-end execution geometry / broker boundary
  ↓
CI-16  Deterministic replay + counterexamples + latency
  ↓
CI-17  Target-terminal cTrader validation
  ↓
CI-FINAL  Full-stack certification
```

Mandatory principle:

`No threshold/weight tuning is accepted as a substitute for correcting a
mathematical, semantic, provenance or timing defect.`

### CI-00 closeout — Canonical data / price / time — 2026-10-01

Status: **COMPLETE — PR #154 verified and merged.**

Completed:

- one canonical `CanonicalPriceSnapshot` owns Bid, Ask, executable BUY/SELL
  prices, midpoint, spread, pip/tick scale and broker-distance metadata;
- existing `MtfClosedContext` remains the sole closed-MTF index owner;
- `CalculationMarketContext` composes quote/time/MTF state without duplicating
  ownership;
- high-risk planning/execution consumers use the canonical market context;
- deterministic Runtime Acceptance and Source/Architecture coverage were added.

Verification:

- Source / Architecture: PASS — run 2391;
- Runtime Acceptance Contracts: PASS — run 2200;
- cTrader Compile / Build: PASS — run 2384;
- CI-00 phase audit: PASS — Source/Architecture step 73.

No public parameter, trading threshold, confidence, RR, SL/TP or execution policy
was tuned.

### CI-01 closeout — Primitive indicator mathematical audit — 2026-10-01

Status: **VERIFIED COMPLETE — PR #155 merged to `main` as `c52d7c7b5cafbd354b63432c03174156f76c611c`.**

Completed:

- retained cTrader-native ATR/ADX-DMI/EMA/RSI as the standard-indicator
  authorities;
- aligned DMI warm-up/readiness with the canonical native readiness rule;
- made the existing two-EMA feature explicit as MACD-line bias;
- centralized DMI, MACD-line bias, RangeEfficiency, Choppiness, VWAP and
  Volume Expansion arithmetic in dedicated pure owners;
- corrected RangeEfficiency interval semantics and removed silent period
  shortening;
- corrected Choppiness full-window semantics;
- corrected VWAP exact-window and zero-volume behavior;
- removed pip-scale distortion from Volume Expansion range geometry;
- added deterministic Runtime Acceptance contracts and an accumulated static audit;
- corrected one CI-01 audit false-positive and cleaned the roadmap continuity
  record without changing production trading behavior.

Verification boundary:

- implementation was merged after the repository merge operation;
- the available GitHub connector did not expose a workflow/check result for the
  corrected CI-01 head, so this record does not invent a fresh PASS claim;
- target-terminal cTrader timing, replay and empirical signal-quality remain
  manual acceptance items.

### CI-02 — OSS numerical parity / warm-up / cache — 2026-10-01

Status: **VERIFIED COMPLETE — final implementation head `c3720853edbcf5c04bb1f5cbf1e9533f39e87a4e`.**

Completed implementation:

- added pure `OssQuoteWindowRule` for canonical stable/rolling window geometry
  and rebuild decisions;
- added pure `OssQuoteProjectionRule` for finite, non-negative OSS quote-volume
  normalization;
- preserved fixed Skender settings under `OssIndicatorSettings.Default`;
- kept the stable/path-dependent adapter window bounded at 768 bars;
- kept window-local adapters bounded by the existing 161-bar rolling window;
- centralized stable/rolling first-index and rebuild logic in the new window owner;
- retained first/last stable-window boundary fingerprints;
- retained HistoryLoaded/Reloaded invalidation;
- removed artificial conversion of zero volume to unit volume;
- added runtime contracts for initial/append/rebuild window semantics, bounds and
  quote-volume normalization;
- consolidated the existing H3-B OSS benchmark under one Track 19 benchmark
  owner, covering all production Skender families, stable/rolling parity, OBV
  direction and deterministic zero-volume variants;
- removed the duplicate CI-02 benchmark module and added explicit terminal-date
  alignment to the parity benchmark;
- kept the existing FacioQuo research-only boundary unchanged;
- accumulated the CI-02 static audit after CI-01.

Acceptance implemented:

- stable recursive adapters require finite outputs and zero directional-classification
  mismatches against full-prefix references while publishing max/mean/RMS error;
- rolling adapters require exact output agreement within 1e-12;
- benchmark records full-prefix versus bounded runtime/allocation;
- no public parameter or trading-policy tuning is introduced.

Repository verification boundary:

- Source / Architecture: **PASS** — workflow run 36909965454 / Source step 75 (`audit_phase_ci_02.py`).
- Runtime Acceptance Contracts: **PASS** — workflow run 36909965513.
- cTrader Compile/Build: **PASS** — workflow run 36909965368.
- OSS benchmark: **PASS** — workflow run 36909965470 / run #92.
- Target-terminal cache/history behavior and live performance remain manual.

Next specified phase: **CI-03 — Indicator fusion / correlation / evidence independence.**

Manual boundary remains: target-terminal cache/history replacement behavior,
live CPU/memory characteristics and empirical signal-quality remain manual acceptance items.

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

**NEXT: CR8.3b/H3-B**

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

## Claude Review Remediation — CR1.9 Closeout — 2026-09-30

Status: IMPLEMENTED

Completed:
- machine-enforced parameter count/documentation audit added and wired into Source/Architecture CI;
- README parameter count aligned to the current machine-derived 567 public parameters;
- MaximumOpenPositions explicitly documented as intentional single-plan capacity (MinValue=1, MaxValue=1);
- session parameter resolution explicitly fixed at 60 minutes through the canonical SessionWindowRule invariant and runtime contract;
- phase record added at docs/PHASE-CR1-9-MINOR-CLEANUP.md;
- no public parameter defaults, trading thresholds, RR floors, position capacity or execution authority changed.

Next phase: **CR2.1 — Structure/CHoCH/MSS/Sweep/Divergence/Rejection semantics**.

Track 12A remains blocked until CR-FINAL passes.

## Claude Review Remediation — CR2.8 Closeout — 2026-09-30

Status: **VERIFIED COMPLETE — PR #93 merged to main as cab5a5e2e9a4fbccaf3ffe10d114c4ff54e6a243.**

Covers: B9.

Completed:
- fixed historical scan budget at 500 closed bars;
- cached per closed-bar presentation results by timestamp;
- changed chart object identity to timestamp-based presentation identity;
- invalidated historical rendering state on Bars HistoryLoaded/Reloaded and detected history-shape changes;
- avoided full historical object deletion/recreation on ordinary refreshes;
- kept historical arrows strictly presentation-only;
- preserved the existing same-host-bar rebuild guard;
- added deterministic runtime contracts and the CR2.8 static audit;
- preserved public parameters and all analysis/decision/execution authority.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS, run 1585;
- cTrader Compile: PASS, run 1769;
- CR2.8 static audit: PASS;
- accumulated CR2.1–CR2.7 audits: PASS.

Manual target-terminal verification remains required for history prepend/load-more/reload, visual stability/responsiveness, and presentation-only behavior.

Next phase: **CR2.9 — Structural stop, divergence and rejection guardrail refinement (B10/B11/B12).**

## Claude Review Remediation — CR2.9 Closeout — 2026-09-30

Status: **VERIFIED COMPLETE — PR #94 merged to main as 6786af20d7b63d371c57890fb1adabb666f830bc.**

Covers: B10, B11, B12.

Completed:
- preserved and verified fail-closed unknown structural timeframe behavior;
- centralized structural-stop reward-path and preferred-risk score components without unverified tuning;
- centralized divergence thresholds/conflict margin/oscillator deltas/recency and quality-score components;
- centralized doji/rejection body and wick thresholds under one canonical meaningful-body rule;
- added deterministic runtime contracts and CR2.9 static audit;
- preserved public parameters and all decision/execution/capacity authority.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS, run 1597;
- cTrader Compile: PASS, run 1781;
- CR2.9 static audit: PASS;
- accumulated CR2.1–CR2.8 audits: PASS.

Manual target-terminal replay/outcome evidence remains required before any structural-stop score tuning or empirical signal-quality claim.

Next phase: **CR3.1 — Live invalidation and false-signal semantics (C1/C2).**


## Claude Review Remediation — CR3.1 Closeout — 2026-09-30

Status: VERIFIED COMPLETE — PR #95 merged to main as f482d2f76cdf37cca87fabc5b11b3d8c0a7edac7.

Covers: C1, C2.

Completed:
- closed-bar-stable live invalidation and false-signal evaluation;
- confirmed structural swing candidates for structural invalidation;
- explicit broker-close success handling and rejected-exit recovery;
- centralized successful _lastExitM5 bookkeeping;
- explicit soft-adverse-R safety flag with current default preserved;
- canonical FalseSignalAdverseR protected-stop semantics and BUY/SELL symmetry;
- deterministic CR3.1 runtime contracts and static audit;
- parameter inventory/audit reconciliation to the intentional 568 public parameters.

Verification:
- Source/Architecture: PASS, run 1808;
- Runtime Acceptance: PASS, run 1617;
- cTrader Compile: PASS, run 1801;
- CR3.1 static audit: PASS;
- project-wide routine/optimization/integrity audits: PASS on the final implementation revision.

Manual target-terminal acceptance remains required for broker-event timing, rejected-close recovery, restart/reconnect behavior, and live quote/bar synchronization. No accuracy, RR improvement or profitability claim is made from CI.

## Claude Review Remediation — CR3.2 Closeout — 2026-09-30

Status: **VERIFIED COMPLETE — PR #98 merged to main as af7eb503dd661d508afcc7619c72f74cea1b3a0f.**

Covers: C3, C4.

Completed:
- renamed the former proxy expected-value concept to a deterministic Reward Quality Floor semantic without changing its established numerical behavior on valid inputs;
- moved the reward-quality calculation into platform-neutral Core math and removed the obsolete `ProxyExpectedValue` executable owner;
- preserved the public parameter property names, types and DefaultValue values for compatibility while correcting their visible cTrader labels;
- centralized early-prediction evidence weights/bonuses in `EarlyPredictionScoreRule`;
- separated directional share from absolute evidence strength and required both conditions for an early prediction;
- prevented a high directional ratio on tiny absolute evidence from being presented as a strong early prediction;
- added deterministic runtime contracts and a dedicated CR3.2 static audit;
- updated panel/readiness/early-alert wording to describe the actual measured semantics;
- completed the project-wide routine, optimization, integrity and accumulated phase audits without introducing a second decision authority.

Verification:
- Source/Architecture: PASS, run 1820;
- Runtime Acceptance: PASS, run 1629;
- cTrader Compile: PASS, run 1813;
- CR3.2 static audit: PASS;
- accumulated CR2.1–CR2.9 and CR3.1 audits: PASS;
- main merge commit: `af7eb503dd661d508afcc7619c72f74cea1b3a0f`.

Manual target-terminal replay/outcome evidence remains required for empirical signal-quality or profitability claims. No accuracy, RR improvement or profitability claim is made from CI.

Next phase: **CR3.5 — Calibration, outcome and rejection transparency.**


## Claude Review Remediation — CR3.3 Closeout — 2026-09-30

Status: **MERGED TO MAIN — PR #99, merge commit `1e8681f57cb48bc51367b3df2ca129803d93d0c4`.**

Completed the partial-TP, server-side ladder, break-even and trailing hardening recorded in `docs/CLAUDE-REVIEW-CR3.3-PARTIAL-TP-BE-TRAILING.md`. The phase added bounded retry semantics, broker-deal evidence, restart peak reconstruction, spread-aware BE diagnostics and monotonic target progression.

Target-terminal broker timing, restart/reconnect behavior and empirical outcome validation remain manual acceptance boundaries.

Next phase: **CR3.4 — Execution UI control and popup reliability.**

## Claude Review Remediation — CR2.1 Closeout — 2026-09-30

Status: VERIFIED COMPLETE — PR #86, merge commit 20835cbf1e541b51b9ad56af46cf0c5d13ff5350.

Completed:
- true structural break freshness and prior-opposite-structure CHOCH semantics;
- canonical structure/MSS/CHOCH event identity and duplicate evidence suppression;
- active/unbroken liquidity sweep state;
- fail-closed unknown structural timeframe handling;
- divergence conflict neutrality;
- minimum-meaningful-body rejection/doji semantics;
- deterministic runtime contracts, CR2.1 static audit and phase documentation.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

## Claude Review Remediation — CR2.2 Closeout — 2026-09-30

Status: VERIFIED COMPLETE — PR #87, merge commit bdb9b72d021972db4b3638ff5eb4d7078ab2cb2a.

Completed:
- explicit intrabar observation versus closed-bar reversal confirmation;
- actual reversal context based on prior counter-move, qualifying zone or canonical swing interaction;
- explicit no-zone and weak-zone behavior;
- equal bull/bear reaction scores resolve to neutral Direction=0;
- one canonical ReactionQualificationRule shared by Pending reversal and Aggressive intrabar qualification;
- deterministic runtime contracts, CR2.2 static audit and phase documentation.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

## Claude Review Remediation — CR2.3 Closeout — 2026-09-30

Status: VERIFIED COMPLETE — PR #88, merge commit 98da2030d8e36312ee0c073c1a58889bb405f893.

Completed:
- centralized IndicatorConfluenceQuality and IndicatorConflict execution gates in IndicatorExecutionQualityRule;
- preserved existing 60/52, 58/55 and 62/48/50 values while documenting their semantic stage differences;
- shared one 62 quality floor for pending continuation/reversal setup qualification;
- migrated Automatic Market, Pending Submission, Pending Continuation and Pending Reversal callers;
- added deterministic threshold contracts and CR2.3 static ownership audit;
- added phase documentation.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

## Claude Review Remediation — CR2.4 Closeout — 2026-09-30

Status: VERIFIED COMPLETE — PR #89, merge commit ac8c526f7ed0887ba990dc9a091c8299e96a6de5.

Completed:
- centralized Continuation Stop vs Reversal Limit selection in PendingDecisionArbiterRule;
- defined explicit quality-based winner selection with deterministic continuation tie-break;
- validated continuation and reversal candidates independently for their own direction, range and market suitability;
- removed same-cycle silent fallback from one pending strategy to the other after placement/preparation failure;
- added two-consecutive-closed-M5-bar cancellation hysteresis;
- corrected predictive Limit quote reference to Ask for Buy Limit and Bid for Sell Limit;
- hardened continuation fallback lookback boundaries;
- added deterministic runtime contracts, CR2.4 static audit and phase documentation.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

## Claude Review Remediation — CR2.5 Closeout — 2026-09-30

Status: VERIFIED COMPLETE — PR #90, merge commit 45d6d21e050db2c519a57a0cdeccfff5d2d88fac.

Completed:
- lifecycle idempotency memory bounded to a fixed 512-event window;
- PositionOpened recovery made tolerant to broker event ordering while preserving managed-identity boundaries;
- final close outcome aggregated from all available History.FindByPositionId trade legs, with deterministic position-level fallback only when history is unavailable;
- realized R calculated from final aggregated monetary outcome against the plan's original volume/initial risk;
- win/loss lifecycle counters aligned with the canonical recorded outcome;
- dedicated lifecycle/outcome runtime contracts, static audit and phase documentation added;
- outcome telemetry/presentation and history-reading responsibilities split into bounded modules to preserve architecture limits and hot-path cleanliness.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

Target-terminal History/Deal semantics remain a manual acceptance boundary.

Next phase: **CR2.6 — OrderBlock quality and cache discipline**.

Track 12A remains blocked until CR-FINAL passes.
## Claude Review Remediation — CR2.7 Closeout — 2026-09-30

Status: VERIFIED COMPLETE — PR #92 merged 2026-09-30, merge commit b021f56c4b75331fb52127547e006f2f8bb287c4.

Completed:
- deterministic WaveTrend MovingAverageType semantics for all currently exposed cTrader MA enum values;
- corrected DEMA/TEMA dependency-state warm-up;
- corrected HMA raw-window weighting;
- corrected WaveTrend component and signal readiness to include all dependent windows;
- added HistoryLoaded/Reloaded and history-prefix/count invalidation;
- documented TickVolume-based MFI semantics;
- added deterministic runtime contracts and CR2.7 static audit;
- preserved public parameters and execution authority.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

Verified head: `181b238a548280fc01a67fb1e3ba8a617f63e42a`.

Target-terminal replay remains required for numerical platform parity and empirical signal-quality measurement.

Next phase: **CR2.8 — Historical rendering semantics and cost**.

Track 12A remains blocked until CR-FINAL passes.




## Claude Review Remediation — CR3.4 Closeout — 2026-09-30

Status: **VERIFIED COMPLETE — PR #100, all required CI gates passed on commit `d8bb61083a043ce46c11b92dc8068b029c0c5add`.**

Completed:
- canonical explicit runtime re-arm contract independent of the public Indicator parameter;
- fail-closed runtime state-machine behavior for non-Healthy states;
- bounded critical-first popup queue with normal-message back-pressure;
- timer-boundary popup processing and removal of direct lifecycle popup overwrites;
- deterministic runtime contracts and CR3.4 static audit wired into CI.

Next phase: **CR3.5 — Calibration, outcome and rejection transparency.**

Track 12A remains blocked until CR-FINAL passes.


## Claude Review Remediation — CR3.5 Implementation — 2026-09-30

Status: **VERIFIED COMPLETE — PR #101 merged to main as `2c31232b7d1f16e88e89e693d7d74fd8a5eedab6`.**

Covers: C8, C9 and B6.

Completed:
- implemented `IEquatable<ConfidenceCalibrationKey>` with deterministic structural equality and hash consistency;
- retained broker-history aggregation as the source of realized outcome net profit and realized R;
- added recent-context average realized R to calibration diagnostics alongside observed win rate and sample count;
- exposed realized-R calibration evidence in Decision and the panel without changing thresholds, defaults or probability semantics;
- added explicit `PLAN_REWARD / REJECTED` telemetry reasons for reward-integrity failures;
- bounded repeated identical reward-rejection telemetry to one record per M5/reason pair;
- added deterministic Decision Contracts and a dedicated CR3.5 static audit wired into Source/Architecture CI.

Safety:
- no public parameter/default changes;
- no RR, confidence, stop or capacity tuning;
- no second decision authority or new broker authority;
- Track 12A remains blocked until CR-FINAL.

Verification boundary:
- CI does not prove target-terminal broker-history semantics, restart/reconnect behavior or empirical profitability/accuracy improvement.

Next phase: **CR-FINAL — final integration audit and target-terminal acceptance.**


## Prompt 4 Remediation Gate — D1–D10 — 2026-09-30

### CR4.1 closeout — 2026-09-30

Status: **COMPLETE — PR #102 merged to main; merge commit `071baf7ab0bda6df8f1d06c9ecbf9d28810e33d1`.**

Completed:
- decision/result-only outcome-learning fingerprint;
- account-scoped LocalStorage identity using broker/account/type/live-demo identity;
- account-scoped outcome archive/runtime-log prefixes;
- account-scoped, versioned portable memory snapshot;
- conservative legacy memory/snapshot migration with current-account broker History ownership validation;
- explicit Account.Switched memory reload;
- deterministic CR4.1 runtime contract and static gate.

Behavioral changes are limited to learning-memory identity and safe legacy migration; no public parameter name/type/DefaultValue or trading threshold was changed.

Repository status limitation:
- the available GitHub status interface returned no check/status records for PR #102/merge commit at closeout, so no CI PASS is claimed.

Target-terminal account switch/restart/persistence verification remains required.

Repository verification note: GitHub status records for this merge were not retrievable through the available interface at closeout; no CI PASS is claimed. Target-terminal manual evidence remains required.

Next phase: **CR4.3 — Signal-trace temporal lineage and future-outcome linkage (D3)**.



Status: **ACTIVE — CR4.3 merged; CR4.4 next**

The fourth Claude review prompt is now mandatory input to the remediation sequence. Its D1–D10 findings are treated as review hypotheses until independently reconciled against current main source.

Authoritative order is now:

`CR4.1 → CR4.2 → CR4.3 → CR4.4 → CR4.5 → CR4.6 → CR4.7 → CR4.8 → CR4.9 → CR4.10 → CR5.1 → CR5.2 → CR5.3 → CR5.4 → CR5.5 → CR5.6 → CR5.7 → CR5.8 → CR6.1 → CR6.2 → CR6.3 → CR6.4 → CR6.5 → CR6.6 → CR6.7 → CR6.8 → CR6.9 → CR-FINAL`

Coverage:
- D1: learning-memory identity, decision-only fingerprinting, account scoping and schema migration;
- D2: absolute/predictable persistence paths, I/O observability, AccessRights target-terminal verification and hot-path writes;
- D3: signal-trace temporal lineage and research-only future-outcome linkage;
- D4: Skender path-dependent indicators, OBV independence, fixed constants and incremental caching;
- D5: per-timeframe regime semantics;
- D6: named FrameScoringConstants with current values preserved;
- D7: TP1–TP4 feasibility, stage rejection telemetry and HTF age semantics;
- D8: defensive TP1 direction validation;
- D9: live reversal alert/action/state semantics and hidden-clamp audit;
- D10: native-indicator readiness/zero safety and NativeIndicatorRegistry performance.

Safety:
- no public parameter name/type/DefaultValue changes;
- no default threshold/RR/confidence tuning;
- no second decision or execution authority;
- every accepted fix receives deterministic tests and the permanent project-wide routine + optimization audit;
- target-terminal-only behaviors remain explicitly marked manual.

CR-FINAL is **paused as a final acceptance gate** until CR4.2–CR4.10, CR5.1–CR5.8 and CR6.1–CR6.9 are either completed or explicitly documented as verified/deferred with evidence.


### CR4.2 closeout — 2026-09-30

Status: **COMPLETE — PR #103 merged to main; merge commit `05a91cb9764f7ee86fcaaba0d7dede540c4b6e1c`.**

Completed:
- retained cTrader-supported relative `History` path under AccessRights.None;
- centralized production file writes under `BufferedArchivePersistence`;
- added bounded write/read health counters and startup History round-trip probe;
- added persistence health diagnostics to the panel;
- account-scoped Signal Trace archive identity with account-switch cache invalidation;
- added deterministic persistence path/health/readback contracts and CR4.2 static gate.

No public parameter name/type/DefaultValue or trading threshold changed.

Next phase: **CR4.3 — Signal-trace temporal lineage and future-outcome linkage (D3)**.

### CR4.3 closeout — 2026-09-30

Status: **COMPLETE — PR #104 merged to main; merge commit `d24b26de3ddf3709c8ea5e94f97a9f533b9b33dc`.**

Completed:
- moved SignalEvaluationTrace capture to the finalized new-closed-M5 boundary, removing the earlier BuildDecision capture point;
- added deterministic `CFIP-ST1` SignalTraceId scoped by symbol, timeframe, account, configuration fingerprint and canonical closed-M5 open time;
- enforced exact Plan/Preview source-bar and direction lineage so stale mutable execution/setup state cannot be paired with a new signal;
- propagated SignalTraceId into plans, aggressive-fill-created plans and realized outcomes;
- versioned Signal Trace archive to v3 and Outcome archive to v2 while preserving buffered append-only persistence and prior-row readability;
- added research-only offline SignalTraceId → outcome joins and geometry-lineage diagnostics;
- added deterministic runtime lineage contracts, CR4.3 static gate and project-wide audit reconciliation.

Verification:
- Source/Architecture: PASS — run 36776384440;
- cTrader Compile: PASS — run 36776384275;
- Runtime Acceptance: PASS — run 36776384489;
- verified implementation head: `7defa4731ebd1e60d87d88a1659d4307607c46ab`.

Boundary:
- target-terminal replay/restart/reconnect and empirical signal-quality/profitability validation remain manual acceptance items;
- no public parameter name/type/DefaultValue, default threshold or RR tuning changed.

Next phase: **CR4.4 — Skender/OSS numerical stability and incremental caching (D4)**.


### CR4.4 / D4 implementation checkpoint — 2026-10-01

Status: **COMPLETE — MERGED TO MAIN**

Implemented on `phase/cr4-4-skender-numerical-caching`:
- centralized fixed OSS indicator constants and warm-up contracts under `OssIndicatorParameters`;
- separated path-dependent Skender adapters (RSI, MACD, SuperTrend, Parabolic SAR) onto a stable-history-prefix cache;
- replaced per-bar rolling Quote rebuilding with a bounded 161-bar incremental append/remove cache for fixed-window adapters;
- added conservative cache invalidation using Bars identity, first-bar fingerprint and stable-last-bar fingerprint;
- retained OBV for diagnostics/research while removing its one-bar bias from OSS confluence vote/count evidence;
- added CR4.4 runtime contracts, static audit and isolated incremental-cache benchmark/reporting;
- wired `audit_phase_4_4.py` into Source/Architecture CI.

Safety:
- no public parameter name/type/DefaultValue changed;
- existing RsiPeriod/MacdFastPeriod/MacdSlowPeriod safety semantics preserved;
- no default trading threshold, RR, confidence or execution policy tuning;
- Track 12A and CR-FINAL ordering remain unchanged;
- target-terminal behavior remains unverified until the required cTrader acceptance stage.


### CR4.4 / D4 closeout — 2026-10-01

CR4.4 was completed and merged to `main` via PR #105, merge commit `1b1a1762fee960e65903880f2566b3355a9a7431`.

Verification on final head `2aef0f7474535131604374f066f535025fe4cb7a`:
- Source/Architecture PASS — run 36779376240;
- cTrader Compile PASS — run 36779376267;
- Runtime Acceptance PASS — run 36779376210;
- OSS indicator benchmark PASS — run 36779376203;
- CR4.4 static gate PASS, including the accumulated routine audits.

Final implementation:
- stable-prefix history for RSI, MACD, SuperTrend and Parabolic SAR;
- bounded incremental 161-bar quote cache for fixed-window adapters;
- conservative invalidation for Bars replacement, cTrader HistoryLoaded/Reloaded and cached-history fingerprints;
- centralized OSS numerical constants and warm-up contracts;
- OBV retained for diagnostics/research and removed from independent OSS confluence vote/count evidence;
- benchmark coverage for rebuild versus incremental quote materialization.

Boundary:
- no public parameter name/type/DefaultValue or default trading threshold was changed;
- no new decision/execution authority was introduced;
- target-terminal runtime timing, memory behavior and empirical signal-quality/profitability remain manual acceptance work.

**Next phase: CR4.5 / D5 — Per-timeframe regime semantics.**

### CR4.5 / D5 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #106 merged to `main`, merge commit `c529c38ecea09e4b1ad85bc465e1ba12739ff95b`.**

Completed:
- preserved the dedicated M5 regime path;
- added bounded per-timeframe regime caching for non-M5 Bars series;
- exposed normalized regime, quality and stability on each Frame;
- made MarketFrameScoringService consume each frame's own normalized regime instead of forcing non-M5 frames to UNKNOWN;
- centralized regime normalization and kept UNKNOWN explicitly neutral;
- added deterministic BUY/SELL symmetry and UNKNOWN-neutrality runtime contracts;
- added the CR4.5 static gate to the accumulated Source/Architecture workflow;
- hardened the CR4.5 static audit so source-formatting changes cannot create false failures, while still checking cache invalidation and per-timeframe analysis ownership.

Verification on implementation head `9353cf11b5473d2753d4e4b539677115765c4792`:
- Source/Architecture PASS — run 36781736303 (including CR4.5 static gate);
- Runtime Acceptance PASS — run 36781736247;
- cTrader Compile PASS — run 36781736379.

Boundary:
- no public parameter name/type/DefaultValue changed;
- no regime threshold, RR, confidence or execution policy was tuned;
- no second decision or execution authority introduced;
- target-terminal timing, replay and empirical signal-quality validation remain manual acceptance items.

**Next phase: CR4.6 / D6 — Frame-scoring constant ownership.**

### CR4.6 / D6 closeout — 2026-10-01

Status: **COMPLETE — PR #107 merged to main after final verification.**

Implemented:
- created the single Core `FrameScoringConstants` owner;
- migrated MarketFrameScoringService to the owner for event contributions, direction thresholds, conflict penalties, RSI exhaustion, regime contribution and frame-quality composition;
- preserved all established numerical values;
- added deterministic runtime contract coverage for the constant owner;
- wired `audit_phase_4_6.py` into the accumulated Source/Architecture workflow;
- corrected the CR4.5 cache-refresh audit's formatting-dependent false positive in a separate fix commit.

Verification on final implementation HEAD `a134b72e863067291fb04eae7ac530c3fbcae999`:
- Source/Architecture PASS — run 36783220015 (#1978);
- Runtime Acceptance Contracts PASS — run 36783220007 (#1787);
- cTrader Compile PASS — run 36783220117 (#1971).

Safety boundary:
- no public parameter name/type/DefaultValue changed;
- no default RR/confidence/stop/threshold tuning;
- no second decision or execution authority introduced;
- target-terminal timing, replay and empirical signal-quality validation remain manual.

**Next phase: CR4.7 / D7 — TP pipeline feasibility, rejection telemetry and HTF-age semantics.**

### CR4.7 / D7 closeout — 2026-10-01

Status: **COMPLETE — PR #108 merged to main, merge commit `2a586ca353f79d6151b9b8375edf46cb94df7880`.**

Implemented:
- centralized deterministic target-candidate constraints in Core;
- separated M5 setup age from HTF source elapsed age;
- propagated source elapsed age through HTF, D1/W1 prior-period, pivot and liquidity target producers;
- added bounded per-stage `PLAN_TARGET` rejection telemetry with deterministic reason ordering and per-M5 deduplication;
- added explicit RR, extension, age, HTF and obstacle rejection reasons;
- added the target reward envelope check before candidate scanning so mathematically unreachable stages are observable;
- kept `TargetSelector` as a thin orchestration boundary by moving telemetry and stage feasibility into dedicated owners;
- added deterministic TP1–TP4 feasibility and BUY/SELL symmetry contracts;
- reused the canonical `StructuralTimeframeRule` for HTF classification.

Repository verification evidence:
- Source/Architecture: PASS, workflow run #2021;
- cTrader Compile: PASS, workflow run #2014;
- Runtime Acceptance Contracts: PASS, workflow run #1830.

Deterministic evidence:
- 4/4 valid TP1–TP4 BUY stage fixtures accepted;
- 4/4 below-minimum-RR fixtures rejected with the canonical reason;
- 4/4 SELL mirror fixtures accepted;
- TP4 was reachable in 1/4 fixed risk/ATR envelope fixtures under the existing 4 ATR extension and 12 RR cap.

Verification on implementation HEAD `441611a8d1ca513ee332f9a8a006a3f37eb9a727`:
- Source/Architecture PASS — run #1999;
- Runtime Acceptance Contracts PASS — run #1808;
- cTrader Compile PASS — run #1992.

Safety boundary:
- no public parameter name/type/DefaultValue changed;
- no default RR, SL or target-age tuning;
- no second decision or execution authority introduced;
- historical/live market TP acceptance rates, broker timing, replay and profitability remain target-terminal/manual evidence.

### CR4.8 / D8 closeout — 2026-10-01

Status: **COMPLETE — implementation ready for repository verification.**

Implemented:
- reused the canonical Core `PriceProtectionRule.ValidateTarget` owner for target-side validation inside target candidate constraints;
- added a fail-closed TP1 directional guard at plan materialization;
- propagated materialization rejection through `BuildPlan`;
- added independent TP1 directional validation at the plan protection boundary;
- added independent TP1 directional validation at the plan reward boundary with bounded `PLAN_REWARD` reason `TP1 DIRECTION INVALID`;
- preserved `TargetProgressionRule` as the monotonic progression owner without introducing a second direction/progression authority;
- added deterministic BUY/SELL valid and wrong-side TP1 contracts;
- added the CR4.8 static gate and wired it into the accumulated Source/Architecture workflow.

Deterministic evidence:
- valid BUY TP1: PASS;
- valid SELL TP1: PASS;
- wrong-side BUY TP1: PASS rejection;
- wrong-side SELL TP1: PASS rejection;
- canonical candidate-level wrong-side rejection: PASS for both directions;
- BUY/SELL target-progression symmetry: PASS.

Safety boundary:
- no public parameter name/type/DefaultValue changed;
- no default RR/confidence/SL/target-age or execution threshold changed;
- no second decision or execution authority introduced;
- no target-selection threshold was tuned;
- no target-terminal or broker-runtime claim is made from source changes alone.

**Next phase: CR4.9 / D9 — Live reversal action/alert semantics.**

### CR4.9 / D9 closeout — 2026-10-01

Status: **COMPLETE — repository verification PASS.**

Implemented:
- added one Core `LiveReversalDecisionRule` owner for opposite-direction mapping, directional reversal confidence and action semantics;
- ensured reversal confidence only consumes reaction/frame evidence when it is aligned with the opposite direction;
- added O(1) reversal-episode state so `REVERSAL` detection alerts are not re-emitted every qualifying M5 while the same episode remains active;
- reset reversal-episode state on broker-confirmed `PositionClosed`;
- changed retained-position presentation from `BLOCKED` to `WAIT • REVERSAL DETECTED • POSITION RETAINED`;
- changed accepted reversal close presentation from `EXECUTED` to `EXIT_REQUESTED`, leaving final closure/outcome authority with broker lifecycle events;
- changed missing-position reversal handling to broker-state reconciliation/recovery instead of directly clearing the plan or synthesizing a close/outcome;
- centralized the existing public live-reversal bounds under named Core threshold ownership without changing their values;
- added deterministic D9 contracts and `audit_phase_4_9.py`, wired after CR4.8 in the accumulated Source/Architecture workflow.

Deterministic evidence:
- BUY/SELL opposite-direction mapping: covered;
- same-direction reversal evidence rejection: covered;
- directional reaction/frame confidence: covered;
- retained vs exit-requested vs broker-confirmation states: covered;
- missing-position reconciliation: covered;
- live-reversal parameter-bound preservation: covered.

Safety boundary:
- no public parameter name/type/DefaultValue changed;
- no default RR/confidence/SL/target or execution threshold was tuned;
- no second decision or broker-execution authority introduced;
- no synthetic outcome is emitted by reversal detection;
- target-terminal timing, broker acknowledgement ordering, restart/reconnect behavior and empirical signal-quality/profitability remain manual acceptance boundaries.

**Next phase: CR4.10 / D10 — Native-indicator defensive safety and registry performance.**

### CR4.10 / D10 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #111 merged to `main`; merge commit `3371b9790902c4d35e4e1e28522dee42f93af861`.**

Implementation verified:
- centralized Core `NativeIndicatorReadinessRule` for warm-up, prior-window, finite-positive and bounded oscillator checks;
- hardened ATR, RSI, ADX and EMA native wrappers at their readiness boundaries;
- made MACD prior-sample comparison explicitly warm-up bounded;
- added `Frame.NativeIndicatorsReady` and fail-closed analysis/scoring before incomplete native inputs can become evidence;
- hardened market-regime normalization against unusable ATR/baseline/EMA values;
- converted the native registry from linear `List<Native>` lookup to reference-identity `Dictionary<Bars, Native>` without changing ownership semantics;
- added deterministic runtime contracts for readiness and neutral RSI safety;
- added a deterministic pre/post registry lookup benchmark;
- added `audit_phase_4_10.py` and wired it after CR4.9 in the accumulated Source/Architecture workflow.

Repository verification:
- Source and Architecture: **PASS** — workflow run `36790307897`; accumulated project audits, including CR4.10, completed successfully.
- Runtime Acceptance Contracts: **PASS** — workflow run `36790307911`.
- cTrader Compile: **PASS** — workflow run `36790307893`.
- OSS / Registry Benchmark: **PASS** — workflow run `36790307937`.

Consumer/cleanliness audit:
- direct native-holder access remains confined to the expected wrapper/registry and analysis boundaries;
- `Native.cs` remains storage-only;
- no duplicate decision/execution authority was introduced;
- no public parameter/default, RR, confidence, SL, target or execution threshold was tuned.

Verification boundary:
- target-terminal startup/readiness timing, panel responsiveness, broker acknowledgement ordering, restart/reconnect behavior and empirical signal-quality/profitability remain manual acceptance items;
- repository-level CR4.10 acceptance is closed and does not infer those manual results.

**Next phase: CR5.1 / E1 — Effective maximum structural-stop risk and duplicate ceiling removal.**



## Prompt 5 Remediation Gate — E1–E8 — 2026-09-30

Status: **IN PROGRESS — CR5.1 through CR5.7 VERIFIED COMPLETE; CR5.8 NEXT.**

Prompt 5 is now a mandatory remediation track after Prompt 4 and before CR-FINAL. The E1–E8 findings are review hypotheses until independently verified against current main source, deterministic contracts/replay, and target-terminal behavior where required.

Authoritative order:

`CR5.1 → CR5.2 → CR5.3 → CR5.4 → CR5.5 → CR5.6 → CR5.7 → CR5.8 → CR-FINAL`

Coverage:
- E1: one canonical effective maximum structural-stop ATR ceiling across all consumers;
- E2: real/multi-level liquidity target candidates and explicit session-forecast semantics;
- E3: independent-evidence group counting that does not treat correlated flags as independent confirmation;
- E4: pending Stop/Limit post-fill reconciliation of absolute plan SL/TP after fill-price divergence;
- E5: shared parallel-scenario computation, MicroReaction closed-bar safety and deterministic scenario replacement/selection;
- E6: PremiumDiscount/LiveBias/HealthyVolatility semantic separation and canonical M5 closed-bar consistency;
- E7: decision-owned WATCH/REACTION alerts separated from rendering;
- E8: named small constants plus TargetSelector lane/required-R consistency and TP-stage ordering.

Mandatory Prompt 5 rules:
- public `[Parameter]` name, type and `DefaultValue` remain unchanged;
- any new parameter must preserve current behavior by default;
- no default RR/confidence/stop/threshold tuning based on source review alone;
- each accepted code correction gets its own `fix(<ID>): ...` commit;
- each accepted correction receives deterministic behavior tests, not text/grep-only checks;
- logic that can be made platform-neutral belongs in testable Core ownership;
- project-wide routine audit and performance/code-cleanliness audit run in every phase;
- cTrader-dependent behavior is explicitly marked for hands-on verification;
- newly discovered bugs are documented separately and are not fixed outside the active scope.

### CR5.1 / E1 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #114 merged to `main`; merge commit `62a119bef79e5a978f1f2ed66a913fa11a1bb2d4`.**

Implementation:
- centralized the effective maximum structural-stop risk ceiling in Core `StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(...)`;
- preserved the existing numerical formula and the distinct roles of `MaximumSlAtr` and `MaximumStructuralStopAtr`;
- migrated all dual-cap consumers identified by the E1 inventory;
- retained early structural-stop rejection when risk exceeds the effective ceiling;
- added deterministic floor/cap ordering and symmetry contracts;
- added `audit_phase_5_1.py` to inventory dual-cap consumers and reject duplicate inline ceiling formulas.

Repository verification on PR #114 head `1f5b7e85c3c0f284431b38d56f17f833df24205b`:
- Source/Architecture: PASS — run `36791329486`;
- Runtime Acceptance Contracts: PASS — run `36791329385`;
- cTrader Compile: PASS — run `36791329369`.

The merge commit `62a119bef79e5a978f1f2ed66a913fa11a1bb2d4` also passed:
- Source/Architecture — PASS — run `36791455294`;
- Runtime Acceptance Contracts — PASS — run `36791455156`;
- cTrader Compile — PASS — run `36791455288`.

Safety/manual boundary:
- no public parameter name/type/default changed;
- no RR/confidence/SL/target/execution threshold tuning;
- no decision/execution authority changed;
- target-terminal, broker lifecycle, restart/reconnect and empirical outcome validation remain manual.

**Next phase: CR5.2 / E2 — Liquidity/session target-source semantics and multi-level target candidates.**

CR-FINAL is **paused** until Prompt 4, Prompt 5 and Prompt 6 are completed or explicitly documented as verified/deferred with evidence.


## Prompt 6 Remediation Gate — F1–F9 — 2026-09-30

Status: **ADDED TO REMEDIATION PROGRAM — IMPLEMENTATION PENDING**

Prompt 6 is now a mandatory remediation track after Prompt 5 and before CR-FINAL. The F1–F9 findings are review hypotheses until independently verified against current main source, deterministic contracts/replay, and target-terminal behavior where required.

Authoritative order:

`CR6.1 → CR6.2 → CR6.3 → CR6.4 → CR6.5 → CR6.6 → CR6.7 → CR6.8 → CR6.9 → CR-FINAL`

Coverage:
- F1: opposing FVG/OB target-path direction, mitigation and obstacle caching;
- F2: Aggressive pre-trade RR/risk guard, direction consistency and actual-fill plan reconciliation;
- F3: orphan managed-position protection failure must not report success;
- F4: hidden additive/clamped actionability thresholds and effective-threshold transparency;
- F5: regime identity and unreachable REVERSAL threshold branch;
- F6: Breakout trap-risk exception, Retest trigger semantics and actionability constant ownership;
- F7: independent-timeframe scenario semantics and duplicate scenario-policy authority;
- F8: target-obstacle rejection reasons and distant-target survival analysis;
- F9: target-obstacle scan performance and cache reuse.

Mandatory Prompt 6 rules:
- public `[Parameter]` name, type and `DefaultValue` remain unchanged;
- any new parameter must preserve current behavior by default;
- no behavior-changing Breakout, RR, stop, target, confidence or trigger-policy change without explicit approval and evidence;
- every accepted correction gets its own `fix(<ID>): ...` commit;
- every accepted correction receives deterministic behavioral tests;
- platform-neutral logic belongs in testable Core ownership where appropriate;
- project-wide routine audit and performance/code-cleanliness audit run in every phase;
- cTrader-dependent behavior is explicitly marked for hands-on verification;
- newly discovered bugs outside the active F-item remain documented and unfixed.

CR-FINAL is **paused** until Prompt 4, Prompt 5 and Prompt 6 are completed or explicitly documented as verified/deferred with evidence.

## CR-FINAL Repository Integration Gate — 2026-09-30

Status: **REPOSITORY GATE PASS — TARGET-TERMINAL ACCEPTANCE STILL BLOCKING**

Evidence record: `docs/CR-FINAL-2026-09-30.md`

Verified on main commit `c966e4e13271da65905341acb56f901ffabe2789`:
- Source / Architecture: PASS, run #1857;
- Runtime Acceptance Contracts: PASS, run #1666;
- cTrader Compile: PASS, run #1850.

The accumulated source, runtime, compile, optimization and CR3.5 gates pass. This does not certify the live cTrader/broker environment.

Remaining mandatory CR-FINAL evidence:
- target-terminal CBOT-Preflight P1–P8;
- broker History/Deal semantics;
- restart/reconnect behavior;
- live broker protection/rejection timing;
- target-terminal chart/panel responsiveness;
- duplicate-execution prevention under real terminal event ordering.

Therefore Track 12A remains blocked. The next operator action is to run the no-trade CBOT-Preflight procedure from `docs/CBOT-PREFLIGHT.md`.



### CR5.2 / E2 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #116 merged to `main`; merge commit `10e01bd2610ce0c42b6d365f55fae24c75a3edfb`.**

Completed:
- replaced raw candle-extreme liquidity forecasts with canonical swing-high/swing-low candidates;
- rejected broken liquidity through the canonical `LiquiditySweepRule.IsActiveUnbrokenLevel(...)`;
- exposed multiple valid liquidity forecasts in deterministic nearest-first distance order;
- applied the existing `MinimumTpSpacingAtr` for source-level separation;
- preserved `SessionWindowRule` and existing `SessionStartUtc` / `SessionEndUtc` semantics;
- kept the normal target selection/reward-risk pipeline authoritative;
- added deterministic E2 Core contracts and `audit_phase_5_2.py` to the accumulated Source/Architecture gate.

Repository verification on PR #116 head `9c7c2acdfe8575915ad1dc4129281bd429144a94`:
- Source/Architecture: PASS — run `36792555340` / workflow #2072;
- Runtime Acceptance Contracts: PASS — run `36792555225` / workflow #1881;
- cTrader Compile: PASS — run `36792555189` / workflow #2065.

Safety/manual boundary:
- no public parameter name/type/default changed;
- no RR/confidence/SL/target-extension threshold tuning;
- no decision/execution authority changed;
- target-terminal startup/readiness, broker lifecycle, restart/reconnect and empirical signal-quality/profitability remain manual.

**Next phase: CR5.3 / E3 — Independent-evidence group counting for parallel opportunities.**


### CR5.3 / E3 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #119 merged to `main`; merge commit `96530088a4216eb4a3f8caae9595987d98c0a27e`.**

Completed:
- centralized the established independent-evidence score and the new independent-group count under Core `IndependentEvidenceFusionRule`;
- defined four independent evidence families: Structural, Location, Trend-Momentum and Context;
- ensured correlated observations inside one family count as one independent group;
- exposed selected-direction group count on `Decision` and both score/group provenance on `TradeOpportunityCandidate`;
- migrated Decision Contracts and Runtime Contracts to the canonical Core owner;
- removed the duplicate Analysis-layer `IndependentEvidenceFusionCalculator`;
- added deterministic E3 contracts and `audit_phase_5_3.py` to the accumulated Source/Architecture gate.

Repository verification on PR #119 head `90207194fcb94768ade28d42f40d6e393b97fad2`:
- Source/Architecture: PASS — run `36793867203` / workflow #2083;
- Runtime Acceptance Contracts: PASS — run `36793867170` / workflow #1892;
- cTrader Compile: PASS — run `36793867168` / workflow #2076.

Safety/manual boundary:
- no public parameter name/type/default changed;
- no RR/confidence/SL/TP/actionability/execution threshold tuning;
- the established 0–8 independent-evidence score behavior was preserved;
- independent-group count is diagnostic/provenance state and is not a new trading filter;
- no decision or execution authority changed;
- target-terminal timing, broker lifecycle, restart/reconnect and empirical signal-quality/profitability remain manual.

**Next phase: CR5.4 / E4 — Pending-order post-fill absolute SL/TP reconciliation.**


### CR5.4 / E4 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #120 merged to `main`; merge commit `782bca41cd37071c79f2cdfa12f712cefc045c8c`.**

Completed:
- preserved the absolute pending Stop/Limit Entry/SL/TP intent across successful pending placement;
- made pending snapshot creation fail-closed when the executable absolute plan cannot be reconstructed;
- reconciled the broker-filled actual entry against the preserved absolute plan before final managed-plan adoption;
- retained a broker SL when it is more protective and a broker TP when it is more progressive;
- rebuilt Advanced/server-side TP protection from the reconciled absolute ladder and actual fill price, then re-confirmed broker ownership;
- prevented `PositionOpened` event-order inversion from bypassing an outstanding pending snapshot;
- cleared stale pending snapshot state on cancellation;
- kept failed reconciliation in `RecoveryRequired` so execution/protection cannot be reported as healthy;
- added deterministic positive/negative fill-divergence, BUY/SELL symmetry, broker-protection, rejection and lifecycle-idempotency contracts;
- added and wired `audit_phase_5_4.py`;
- reconciled the accumulated CR2.5 lifecycle-ordering audit with the new pending-snapshot invariant.

Repository verification on final implementation head `6d89f010fa6d292d201ef79e37af5b162438f854`:
- Source/Architecture: PASS — run `36795710379` / workflow #2096;
- Runtime Acceptance Contracts: PASS — run `36795710374` / workflow #1905;
- cTrader Compile: PASS — run `36795710377` / workflow #2089.

Safety/manual boundary:
- no public parameter name/type/default changed;
- no RR, confidence, stop, target or execution threshold was tuned;
- no second decision or broker execution authority introduced;
- broker-confirmed state remains authoritative;
- target-terminal verification is still required for actual Stop/Limit fill-price divergence, broker-side final SL/TP, Advanced Protection ladder behavior, rejection timing and restart/reconnect lifecycle.

**Next phase: CR5.6 / E6 — PremiumDiscount/LiveBias/HealthyVolatility semantic separation and canonical M5 closed-bar consistency.**


### CR5.5 / E5 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #121; implementation head `5b34afbdc7ed252a3fabc68cdb9859818cb911e6`.**

Completed:
- shared closed-M5 execution/entry/stop/risk geometry is cached once per active M5
  and direction; the platform-specific ExecutionModel remains in a separate
  bounded indicator cache while the Core geometry stays runtime-neutral;
- tactical parallel evaluation and candidate materialization consume the same
  geometry, removing duplicated execution-zone and structural-stop work for the
  same closed M5/direction;
- TradePlanRegistry owns parallel candidate replacement and the presentation
  list is now only a deterministic snapshot;
- Core `ParallelScenarioSelectionRule` owns scenario identity, coverage,
  replacement and visible-scenario selection;
- MicroReaction parallel presentation requires exact closed-M5 identity, matching
  direction, closed-bar confirmation and the existing `LiveReactionStrongThreshold`;
- Decision records the exact confirmed reaction M5/direction, resolved independently
  from the live intrabar reaction state;
- deterministic E5 runtime contracts and `audit_phase_5_5.py` are wired into CI;
- accumulated Phase 11.4 audit was reconciled to the new Core scenario-selection
  ownership without changing its reward-risk contract;
- no public parameter name/type/DefaultValue, default RR/confidence/stop/target
  threshold or decision/execution authority changed.

Verification on final implementation head:
- Source/Architecture: PASS — workflow run `36836762005`;
- Runtime Acceptance Contracts: PASS — workflow run `36836762039`;
- cTrader Compile: PASS — workflow run `36836762006`.

Safety/manual boundary:
- target-terminal intrabar/closed-bar timing, panel presentation, broker lifecycle,
  restart/reconnect and empirical signal-quality/profitability remain manual;
- CR-FINAL remains paused until CR5.6–CR5.8 and CR6.1–CR6.9 are reconciled and
  completed or explicitly documented.

### CR5.6 / E6 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #122**

Completed:
- Premium/Discount semantics are explicitly owned by a Core midpoint rule; its existing mean-reversion/context contribution remains unchanged at +6 when enabled;
- LiveBias no longer reads the host chart timeframe and now consumes the canonical closed M5 bar plus reference-time closed-bar validation;
- Healthy Volatility ATR health is separated from Minimum Trigger Body ATR; the existing defaults 0.85, 1.80 and 0.12 remain unchanged;
- deterministic contracts cover BUY/SELL symmetry, invalid-input safety, ATR-ratio boundaries, and M5/M15/H1 closed-bar boundaries;
- E6 static audit is wired immediately after E5;
- no public parameter name/type/default, RR/confidence/stop/target threshold, or execution authority changed.

Verification on implementation head e930e30d9c82ad279316a41c81116d4ae1e19859:
- Source/Architecture: PASS — run 36838144439;
- Runtime Acceptance Contracts: PASS — run 36838144403;
- cTrader Compile: PASS — run 36838144416.

Manual boundary:
- target-terminal M5/M15/H1 timing, panel/visual behavior, restart/reconnect and empirical signal quality/profitability remain manual.

### CR5.7 / E7 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #123 merged to `main`; merge commit `d37f6d575595bfacecdff5a5ffb8fc44ba96455a`.**

Completed:
- moved WATCH/REACTION alert qualification and emission out of `UI/Chart/SignalRenderer.RenderWatchAndReaction` into the runtime decision-alert boundary;
- added the platform-neutral Core `WatchReactionAlertRule` as the single owner of early-WATCH threshold semantics, WATCH eligibility, REACTION eligibility and deterministic alert identities;
- named the existing early-WATCH confidence floor `60` and allowance `4` without changing their values;
- preserved live REACTION intrabar evaluation cadence after `UpdateLiveReaction`, while removing its dependency on chart rendering;
- made `SignalRenderer` presentation-only for WATCH/REACTION alerts;
- made `SignalPresentationRenderer.IsStrongWatchSnapshot` reuse the canonical Core watch rule instead of duplicating qualification thresholds;
- added deterministic Runtime Contract coverage for threshold, blocked-state, plan/pending/live-state, BUY/SELL symmetry and deterministic alert identity semantics;
- added and wired `audit_phase_5_7.py` immediately after E6;
- recorded the E7 root cause and safety boundary in `docs/PHASE-CR5-7-WATCH-REACTION-ALERTS.md`.

Repository verification boundary:
- Source/Architecture: PASS — run `36841785497` / job `110302351624`;
- Runtime Acceptance Contracts: PASS — run `36841785708` / job `110302353199`;
- cTrader Compile: PASS — run `36841785507` / job `110302351749`;
- the final E7 implementation head was `8d39ad3fb88c75092908121a3cbaa65128f47659`; PR #123 merged successfully to `main`.
- target-terminal alert timing, popup/audio delivery, panel/chart behavior, broker lifecycle and empirical signal-quality behavior remain manual acceptance items;
- no public parameter name/type/default, RR/confidence/stop/target threshold or decision/execution authority was changed.

**Next phase: CR5.8 / E8 — Small constant ownership and TargetSelection consistency.**

### CR5.8 / E8 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #124 merged to `main`; merge commit `03a569d6a18f1b6cbc3524dabc24ea713439c325`.**

Completed:
- named the existing internal reward-risk floor/margin constants without changing their values;
- centralized TP1..TP4 required-RR construction in Core `TargetSelectionRequiredRrRule`;
- enforced cumulative non-decreasing TP2 → TP3 → TP4 required-RR ordering;
- removed silent Strategic lane defaults from target-selection entry points;
- propagated the explicit `OpportunityLane` through all current target-selection callers;
- reused the canonical ladder for execution fallback floors;
- added deterministic four-lane ordering and BUY/SELL symmetry contracts;
- reconciled accumulated Phase 11.4 tactical-RR and E6 continuity audits with the current ownership model;
- strengthened E8 static ownership checks so duplicate internal constants cannot return.

Verification on final implementation head `51e1f2bc9ecdd12bc8a366630fb225a4fa2c5593`:
- Source / Architecture: PASS — run `36844545898` / workflow #2160.
- Runtime Acceptance Contracts: PASS — run `36844545976` / workflow #1969.
- cTrader Compile: PASS — run `36844546002` / workflow #2153.

Safety/manual boundary:
- no public parameter name/type/DefaultValue changed;
- no default RR/confidence/stop/target threshold was retuned;
- no decision or execution authority changed;
- target-terminal timing, broker lifecycle, restart/reconnect and empirical signal-quality/profitability remain manual.

**Next phase: CR6.1 / F1 — Opposing FVG/OB target-path direction, mitigation and obstacle caching.**


### CR6.1 / F1 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #127 merged to `main`; merge commit `a7a03a4403a7c6681f95ac0b053344144e933b6e`.**

Completed:
- canonical Core `RewardPathGeometryRule` now owns opposing-zone direction and target-path geometry;
- obsolete Trading-layer reward-path geometry owner removed and the architecture gate migrated to the Core owner;
- opposing FVG and OB obstacle construction consistently uses `opposingDirection = -direction`;
- FVG obstacles are materialized through the canonical managed FVG lifecycle, so fully mitigated FVGs are excluded before entering the obstacle cache;
- broken Order Blocks remain excluded through the existing OB lifecycle state;
- opposing-zone candidates are cached per Bars/index/direction while entry/target-specific path geometry remains evaluated per target;
- M15/M30/H1/H4 higher-timeframe scans consume the same canonical F1 path;
- deterministic Runtime Contract coverage and `audit_phase_6_1.py` were added and wired into Source/Architecture CI;
- public parameter name/type/DefaultValue, RR/confidence/SL/TP thresholds and decision/execution authority were unchanged.

Behavior corrections:
- same-direction FVGs can no longer be treated as opposing reward-path obstacles;
- fully mitigated FVGs no longer block the reward path;
- FVG gap qualification on this obstacle path now follows the canonical creation-bar ATR semantics.

Repository verification on final implementation head `b37ebf00bbd835d7cab8a192842752be7238d703`:
- Source/Architecture: PASS — workflow run #2177;
- Runtime Acceptance Contracts: PASS — workflow run #1986;
- cTrader Compile: PASS — workflow run #2170;
- F1 static audit: PASS.

Manual boundary:
- target-terminal replay/live verification of M15/M30/H1/H4 zone-path behavior, mitigated-zone exclusion, warm-cache timing and unchanged chart/panel behavior remains required;
- empirical signal-quality/profitability impact remains unvalidated.

**Next phase: CR6.2 / F2 — Aggressive pre-trade RR/risk guard, direction consistency and actual-fill plan reconciliation.**

### CR6.2 / F2 closeout — 2026-10-01

CR6.2 / F2 is **VERIFIED COMPLETE — PR #128**, final implementation head
`3d7d819343c00135c4b0b9eb7ffa2dfa2b19a452`.

Completed:
- canonical `PlanRewardRiskQualityRule` is now evaluated on the actual
  aggressive pre-submission Entry/ATR/SL/TP geometry;
- the unreachable `_plan != null` dependency was removed from the final
  aggressive reward/risk guard;
- reaction/trade direction consistency is enforced at the final guard, while
  `AggressiveRequireSmartAgreement` remains the explicit decision/reaction
  policy;
- accepted aggressive fills seed the managed plan from actual-fill-derived
  exit geometry;
- post-fill reconciliation is fail-closed and mandatory before publishing
  `LivePosition`;
- final post-fill Entry/SL/TP geometry is revalidated;
- existing PositionClosed cleanup remains the lifecycle authority;
- deterministic F2 Runtime Contracts and `audit_phase_6_2.py` are wired into
  the accumulated CI audit chain.

Verification:
- Source/Architecture PASS — run #2183;
- Runtime Acceptance Contracts PASS — run #1992;
- cTrader Compile PASS — run #2176.

Safety/manual boundary:
- no public parameter name/type/DefaultValue changed;
- no RR/confidence/SL/TP/execution threshold was retuned;
- no decision or execution authority changed;
- actual cTrader fill divergence, broker protection/rejection timing,
  restart/reconnect and terminal UI remain manual acceptance boundaries.

**CR6.3 / F4 verified complete — Effective-threshold transparency and hidden additive margins.**

PR #129 was verified with Source/Architecture, Runtime Acceptance Contracts, and cTrader Compile all PASS on commit `5dd9a8a8bb0fb8172ac40b7336ce86e0bb5c3c2a`. No public parameter contract or default trading policy was changed.

**Next phase: CR6.4 / F5 — Smart-threshold regime identity and hidden REVERSAL dead path.**

### CR6.4 / F5 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — smart-threshold regime identity and the unreachable REVERSAL branch are reconciled.**

Completed:
- canonical MarketRegimeIdentity constants now own the six classifier states plus UNKNOWN;
- regime normalization is centralized in the same identity owner;
- MarketRegimeClassifier and reviewed regime consumers no longer duplicate the regime vocabulary;
- adaptive Smart Threshold adjustment is owned by platform-neutral SmartThresholdPolicyRule;
- the unreachable REVERSAL Smart Threshold branch is removed because the canonical classifier has no REVERSAL state;
- existing TREND/EXPANSION/RANGE/COMPRESSION numerical adjustments are preserved;
- HIGH_VOLATILITY, TRANSITION, UNKNOWN and future values retain the base threshold path;
- deterministic F5 Runtime Contracts and audit_phase_6_4.py are wired into the accumulated verification chain.

Safety:
- no public parameter name/type/DefaultValue changed;
- no default threshold, RR, confidence or execution policy changed;
- no second decision or execution authority introduced;
- REVERSAL remains an execution/reversal semantic elsewhere and is not a market-regime identifier.

Repository/manual boundary:
- Source/Architecture, Runtime Acceptance Contracts and cTrader Compile are repository gates;
- target-terminal regime timing/presentation, restart/reconnect and empirical signal-quality/profitability remain manual acceptance items.

**Next phase: CR6.5 / F6 — Trap-risk/trigger exceptions and actionability constant ownership.**


### CR6.5 / F6 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #131 head `01db0f46f2a19597be5428a62a230ebf67a7f36c`.**

تأیید می‌کنم — CR6.5/F6 با ممیزی مستقل مسیر فعلی و بدون تغییر در قرارداد عمومی پارامترها یا tuning عددی تکمیل شد.

Completed:
- ایجاد مالک Core واحد `EntryActionabilityPolicy` برای ثابت‌ها و semantics مربوط به trap-risk، range-location، divergence، adverse momentum، micro-conflict، trigger tolerance، anchor و late-entry؛
- مهاجرت `EntryTrapRiskRule`, `TradeActionabilityEvaluator`, `ExecutionModeResolver` و `TriggerGate` به owner واحد، بدون تغییر مقادیر مؤثر؛
- انتقال آستانه‌های actionability مربوط به indicator fusion به `ActionabilityThresholdPolicy` و canonicalization هویت‌های regime؛
- تأیید اینکه Breakout به‌صورت آگاهانه trap-risk block را bypass می‌کند و این سیاست در F6 به رفتار جدید تبدیل نشده است؛
- تأیید اینکه Retest می‌تواند در resolver محلی، در-zone و پیش از trigger دیده شود، اما مسیر canonical plan همچنان `Decision.TriggerReady` را enforce می‌کند؛
- تأیید تفاوت anchor، actual-entry و late-entry بین Breakout و Retest و حفظ trigger tolerance موجود؛
- افزودن Runtime Acceptance Contract و `audit_phase_6_5.py` به زنجیره audit انباشته؛
- اصلاح مستقل چند خطای verification که در حین CI آشکار شد: duplicate helper names، missing Decision.Contracts include، و دو assertion/audit اشتباه در F6.

Verification:
- Source/Architecture: **PASS** — run `36856702821`، شامل `audit_phase_6_5.py` و auditهای انباشته؛
- Runtime Acceptance Contracts: **PASS** — run `36856702812`؛
- cTrader Compile/Build: **PASS** — run `36856702767`.

Safety/manual boundary:
- هیچ `[Parameter]` name/type/`DefaultValue` تغییر نکرد؛
- هیچ RR/confidence/SL/TP یا execution threshold برای tuning تغییر نکرد؛
- هیچ decision یا execution authority جدید ایجاد نشد؛
- Breakout trap bypass عمداً حفظ شد؛
- trigger contract و Retest semantics عمداً حفظ شد؛
- target-terminal timing، panel/chart presentation، broker lifecycle، restart/reconnect و empirical signal-quality/profitability همچنان manual acceptance هستند.

**Next phase: CR6.7 / F8 — Target-obstacle rejection telemetry and distant-target semantics.**

### CR6.6 / F7 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — independent timeframe candidates are explicitly classified as observations over the canonical M5 plan geometry, and scenario execution policy now has one owner.**

Implementation:
- M15/M30/H1/H4/D1/W1 candidates retain their timeframe identity and evidence, but explicitly declare BasePlanTimeframe = "M5";
- Entry/Stop/TP geometry for timeframe annotations continues to come from the shared closed-M5 parallel scenario path; no false claim of independent timeframe plans was introduced;
- same closed-M5/lane/direction preview construction is cached and reused, avoiding repeated full target/plan-preview work for same-base timeframe annotations;
- the former Analysis and Trading ScenarioExecutionPolicy owners were removed;
- Core ScenarioExecutionPolicyRule now owns candidate eligibility and execution authorization together;
- independent timeframe scenarios remain structurally evaluable but are explicitly OBSERVE-ONLY TF SCENARIO;
- display stage/reason consumes the same policy result used for execution authorization;
- deterministic F7 runtime contracts and accumulated static audit are wired.

Safety:
- no public parameter name/type/DefaultValue changed;
- no RR/confidence/SL/TP/actionability/execution threshold was tuned;
- no second decision or execution authority introduced;
- no separate broker execution path introduced.

Verification boundary:
- repository source/architecture and runtime contract evidence is deterministic once CI runs;
- cTrader target-terminal timing, live scenario presentation, broker lifecycle, replay and empirical signal-quality remain manual.

**Next phase: CR6.7 / F8 — Target-obstacle rejection telemetry and distant-target semantics.**

### CR6.7 / F8 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — target-obstacle rejection telemetry now distinguishes swing/equality/zone classes and quantifies target distance without changing target-selection defaults.**

Implementation:
- M5 target-obstacle evaluation now returns a structured observation while preserving the existing boolean rejection behavior;
- ordinary swing obstruction remains `OBSTACLE_SWING`;
- equal-high/low liquidity obstruction is now separated as `OBSTACLE_EQ`;
- opposing-zone and HTF-zone categories remain distinct as `OBSTACLE_OPPOSING_ZONE` and `OBSTACLE_HTF_ZONE`;
- obstacle rejection telemetry aggregates target distance in ATR, target position as a percentage of the existing Maximum Target Extension ATR envelope, and known obstacle distance in ATR;
- zone/HTF telemetry records target-distance evidence without fabricating a precise obstacle depth;
- telemetry storage is bounded per closed M5 and deduplicated per target stage/reason using the existing PLAN_TARGET channel;
- deterministic F8 runtime contracts and the accumulated static audit are wired.

Finding:
- the current defaults already reject any qualifying M5 swing/equality strictly between Entry and Target minus the existing clearance; therefore farther valid targets can accumulate more obstacle rejections simply because they expose more path, but this phase does **not** retune or exempt distant targets;
- the new telemetry provides the evidence needed to quantify that relationship during replay before any future behavioral change.

Safety:
- no public parameter name/type/DefaultValue changed;
- no MaximumTargetExtensionAtr, TargetClearanceAtr, RejectTargetObstacle, RR, SL/TP or scoring threshold changed;
- no alternate target-selection authority introduced;
- no broker mutation path introduced.

Verification boundary:
- repository source, compile and runtime contract evidence is deterministic once CI passes;
- real-market obstacle frequency, replay distribution and target-survival conclusions remain evidence/manual work.

**Next phase: CR6.8 / F9 — Target-obstacle scan performance and cache reuse.**

### CR6.8 / F9 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — Source/Architecture #2246, Runtime Acceptance #2055 and cTrader Compile #2239 passed on the final F9 HEAD.**

Completed:
- Core `TargetObstacleCacheKey` captures Bars/history/index/direction and materially relevant scan inputs.
- bounded 16-entry `TargetObstacleScanCache` reuses M5 swing/equality structural extraction across repeated target candidates;
- same-Bars new-bar/history-size/open-time invalidation plus `HistoryLoaded`/`Reloaded` invalidation prevents stale snapshots;
- candidate-specific Entry/Target clearance remains live on every candidate;
- existing F1 opposing-zone cache remains the sole zone-path candidate cache;
- deterministic Planning Contracts, F9 static audit and reference benchmark were added;
- Source/Architecture #2246, Runtime Acceptance #2055 and cTrader Compile #2239 passed on the final F9 HEAD.

Safety:
- no public parameter name/type/DefaultValue changed;
- no obstacle/RR/confidence/SL/TP/execution threshold changed;
- no second target-selection or broker-execution authority introduced.

Verification boundary:
- Source/Architecture #2246 passed the accumulated repository gate through F9 before merge;
- reference benchmark is structural-work evidence, not a cTrader terminal latency claim;
- terminal replay/new-bar/history-reload and empirical signal-quality remain manual acceptance boundaries.

**Next phase: CR6.9 / F3 — Orphaned managed-position protection.**

### CR6.9 / F3 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — Source/Architecture #2255, Runtime Acceptance #2064 and cTrader Compile #2248 passed on F3 implementation head `9b408ff7bcea45b03015e0b81f2e65a76c995b63`; merged via PR #137.**

**تأیید می‌کنم** — F3 invalid-stop success reporting was corrected fail-closed.

Completed:
- invalid orphan stop candidate now fails and emits a clear diagnostic;
- Core `OrphanManagedProtectionRule` is the single success invariant;
- caller enters `RecoveryRequired` on failure;
- failed protection does not update `_lastBrokerModifyUtc`;
- deterministic Runtime Contract and accumulated static audit were added;
- no public parameter/default/trading threshold or broker mutation ownership changed.

Safety/manual boundary:
- only the explicit F3 safety correction is changed;
- target-terminal broker rejection timing and restart/reconnect behavior remain manual acceptance.

**Next phase: CR7.1 / G1 — Broker protection must never increase live position risk.**



## Prompt 7 Remediation Gate — G1–G6 — 2026-10-01

Status: **ADDED TO REMEDIATION PROGRAM — IMPLEMENTATION PENDING**

Prompt 7 is an additional remediation track after Prompt 6 and before CR-FINAL.
It does not reopen or renumber Prompt 4, Prompt 5 or Prompt 6. The findings are
review hypotheses until independently verified against the current `main`
source, deterministic behavioral contracts and target-terminal/cTrader behavior
where required.

Authoritative order:

`CR7.1 → CR7.2 → CR7.3 → CR7.4 → CR7.5 → CR7.6a → CR7.6b → CR7.6c → CR7.6d → CR-FINAL`

### CR7.1 / G1 — Broker protection must never increase live position risk
Priority: **SAFETY-CRITICAL**

Scope:
- separate existing broker-stop health from acceptability of a newly proposed stop;
- ensure an existing correctly-sided broker SL is not replaced with a farther stop merely because it becomes too close to current market price;
- require protective-only stop progression whenever a current SL exists;
- preserve/build protection when no SL exists;
- prevent false `MISSING OR INVALID` recovery state when an existing stop is present and correctly sided;
- verify the `BoundPlanProtection` path for the same risk-expansion possibility.

Required Core ownership:
- split `IsValidManagedStop` semantics into existing-stop health and new-stop acceptability;
- use `ProtectionProgressionRule` as the authoritative progression guard.

Deterministic tests:
- BUY/SELL with an existing near-market SL must not move farther away;
- BUY/SELL without SL must create protection;
- wrong-sided existing SL must be rejected/recovered;
- structural fallback farther from the current SL must not replace it.

Manual boundary:
- actual cTrader `ModifyStopLossPrice` behavior and broker minimum-distance semantics.

### CR7.1 / G1 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #138 merged to `main` as `b8144c2f1edc62730b7a0723be3746afe6353851`.**

تأیید می‌کنم — the existing broker-stop health contract was separated from new-stop market-distance acceptability.

Completed:
- Core `ManagedStopProtectionRule.IsExistingStopHealthy` now validates an existing SL only for finite price + correct side of Entry;
- new-stop validation continues to enforce current-market/minimum-distance constraints;
- broker reconciliation, bound-plan protection and protection-state evaluation now use existing-stop health;
- `ProtectionProgressionRule` remains the sole guard for replacing a healthy existing stop, preventing risk expansion;
- `TargetObstacleCacheKey.GetHashCode()` was completed for equality/hash correctness;
- deterministic BUY/SELL G1 contracts and `audit_phase_7_1.py` were added to the accumulated Source/Architecture gate;
- the project-integrity duplicate-method audit was corrected to include containing type, so independent `GetHashCode()` overrides are not false positives.

Verification on final G1 head `2f1cb933a2c2407e1fe33302cf72f538f090ba91`:
- Source/Architecture: **PASS** — run #2262;
- Runtime Acceptance Contracts: **PASS** — run #2071;
- cTrader Compile: **PASS** — run #2255.

Safety:
- no public parameter name/type/DefaultValue changed;
- no RR/confidence/SL/TP/execution threshold tuning;
- G1 is an explicit safety correction;
- no second broker-mutation or execution authority introduced.

Manual boundary:
- actual cTrader `ModifyStopLossPrice` behavior, broker minimum-distance edge cases and restart/reconnect timing remain target-terminal acceptance items.

**Next phase: CR7.2 / G2 — Retest adverse-momentum semantics and rejection telemetry.**

### CR7.2 / G2 — Retest adverse-momentum semantics and rejection telemetry

Scope:
- verify whether adverse-momentum thresholds incorrectly reject legitimate Retest entries;
- preserve current defaults unless a separately approved behavior change is made;
- expose rejection reasons `TRAP_ADVERSE_M5`, `TRAP_ADVERSE_M1`, `TRAP_EXTREME`,
  `TRAP_DIVERGENCE` through telemetry/panel ownership;
- distinguish pre-zone adverse movement from post-zone reaction behavior before
  changing Retest policy.

Required Core/test ownership:
- parameterize the existing 0.30 / 0.45 / 0.40 thresholds without changing
  their defaults;
- deterministic matrix covering range position, M5/M1 adverse momentum,
  divergence, risk and block reason.

Behavior-change gate:
- no Retest exemption or threshold retuning is authorized by the roadmap alone.

### CR7.2 / G2 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #142 merged to `main` as `f983d2fd7eb0baced4b5ff40988e6294b3f5bd28`.**

تأیید می‌کنم — the Retest/trap-risk chain was hardened without changing the existing block thresholds or introducing a Retest exemption.

Completed:
- canonical Core ownership for the existing 0.30 / 0.45 / 0.40 ATR adverse-momentum boundaries;
- deterministic rejection taxonomy: `TRAP_ADVERSE_M5`, `TRAP_ADVERSE_M1`, `TRAP_EXTREME`, `TRAP_DIVERGENCE`;
- bounded classification of recent adverse movement as pre-zone versus post-zone/reaction context;
- existing Retest block behavior preserved;
- Decision → ActionabilityReason → composed reason → panel diagnostic path preserved as the single authority;
- legacy six-argument trap-rule contract retained for accumulated F6 verification;
- architecture refactor moved Retest/adverse helper work out of oversized validation modules;
- accumulated F6 audit reconciled with the new canonical trap policy owner.

Verification on final G2 head `94e8154e3ec5107bb984be228aed874ba2e1e27c`:
- Source/Architecture: **PASS** — run #2283;
- Runtime Acceptance Contracts: **PASS** — run #2092;
- cTrader Compile: **PASS** — run #2276.

Safety:
- no public parameter name/type/DefaultValue changed;
- no RR/confidence/SL/TP/execution threshold retuned;
- no second decision or execution authority introduced;
- diagnostic context does not alter the `Block` result.

Manual boundary:
- target-terminal Retest intrabar/zone interaction timing;
- cTrader panel/chart rendering;
- empirical Retest signal-quality/profitability effects remain manual acceptance items.

### CR7.3 / G3 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #143 merged to `main` as `6c572643cfc6b9a4ee1083e300ec13607dd2774c`.**

Scope completed:
- preserved the public `Level Line Thickness` parameter unchanged;
- corrected the hidden display clamp that forced valid values 2 and 3 down to thickness 1;
- introduced canonical `PlanLinePresentationRule.ResolveThickness` as the single presentation-mapping owner;
- preserved `LineStyle.Solid` as the only plan-line style;
- added deterministic Runtime Acceptance coverage for thickness 1/2/3 plus bounded invalid-input behavior;
- added accumulated `audit_phase_7_3.py` after G2.

Deterministic contract:
- 1 → 1;
- 2 → 2;
- 3 → 3;
- out-of-contract 0 → 1 and 4 → 3.

Safety:
- no public parameter name/type/DefaultValue changed;
- no RR/confidence/SL/TP/execution threshold changed;
- no decision or execution authority changed;
- no second trading engine introduced.

Manual boundary:
- target-terminal cTrader rendering still requires hands-on validation of visible thickness 1/2/3 and preserved Solid style.

Verification:
- Source/Architecture: **PASS** — run #2295;
- Runtime Acceptance Contracts: **PASS** — run #2104;
- cTrader Compile: **PASS** — run #2288.

**Next phase: CR7.4 / G4 — Panel execution/protection state semantics.**
### CR7.4 / G4 — Panel execution/protection state semantics

Status: **VERIFIED COMPLETE — PR #144 merged after final gate verification.**

Scope completed:
- canonical Core execution/protection panel-state semantics;
- Auto Trade/Auto Orders state separated from decision/reaction readiness;
- broker protection state reflects broker-confirmed SL/TP, target requirement, server TP-ladder ownership and recovery;
- overview/detail rows consume one state owner;
- panel presentation cache invalidates on execution/protection state changes;
- deterministic G4 runtime contracts and static audit are accumulated.

Verification:
- Source/Architecture: **PASS** — run #2313;
- Runtime Acceptance Contracts: **PASS** — run #2122;
- cTrader Compile: **PASS** — run #2306;
- verified code HEAD: `2465444593afca6b566b17be2234336154fd6f2e`.

Safety/manual boundary:
- no public parameter/default or trading threshold changes;
- no new decision/execution/broker-mutation authority;
- target-terminal panel/protection timing remains manual.

### CR7.5 / G5 — Panel execution/protection state freshness and broker-read minimization

Status: **VERIFIED COMPLETE — PR #145 merged to `main` as `e2674b9800159ba1266639ad96a374f622aff555`.**

Scope:
- remove unconditional G4 cache invalidation from the presentation-key builder;
- drive invalidation from authoritative runtime, lifecycle and broker-state changes;
- guard direct block/recovery/server-TP-ladder mutations from bypassing snapshot invalidation;
- retain the existing one-second broker-state refresh as the stale-state backstop;
- preserve broker enumeration inside the canonical G4 panel-state snapshot owner;
- add deterministic runtime coverage and the accumulated G5 static audit.

Root cause confirmed from `main`:
- `BuildPanelPresentationKey` invalidated the G4 panel snapshot before every key calculation;
- the snapshot therefore did not persist across unchanged panel refresh attempts.

Safety/performance boundary:
- no public parameter/default changes;
- no RR/confidence/entry/SL/TP/risk/execution threshold tuning;
- no new decision/execution/broker-mutation authority;
- no new broker enumeration;
- unchanged broker refresh interval remains the freshness backstop.

Repository implementation was completed on branch `phase/cr7-5-g5-panel-state-freshness`.

Verification on final implementation head `1a2572e2f72e8842640e9c1cbea88e6868f05354`:
- Source/Architecture: **PASS** — #2321;
- Runtime Acceptance Contracts: **PASS** — #2130;
- cTrader Compile: **PASS** — #2314.

Merged via PR #145 as `e2674b9800159ba1266639ad96a374f622aff555`.

## CR7.6a / G6A — Execution panel presentation freshness

Status: **VERIFIED COMPLETE — functional implementation HEAD `b0b19c1a3e7e65110dc1b64d4f8a3bf555b7c54e` passed all three repository gates.**

Verification:
- Source/Architecture: **PASS** — #2327;
- Runtime Acceptance Contracts: **PASS** — #2136;
- cTrader Compile: **PASS** — #2320;
- accumulated G5 audit was reconciled so historical phase transitions remain valid as the roadmap advances.

Scope/root cause:
- G5 correctly made panel cache freshness event-driven and bounded, but the panel presentation
  key still omitted mutable execution-facing values already rendered by the panel;
- `SetAutoTradingState` changed canonical Auto Trade state/reason without invalidating the G4
  execution/protection snapshot.

Implemented:
- added Core `ExecutionPanelPresentationIdentityRule` as the single owner of the
  execution-facing presentation identity;
- the presentation key now includes Auto Trade state/reason, execution telemetry path/state,
  active execution scenario, market-suitability score/state/reason and break-even diagnostic;
- `SetAutoTradingState` invalidates the canonical G4 snapshot only when state/reason changes;
- telemetry remains presentation-only and does not force a broker-state refresh;
- added deterministic Runtime Acceptance coverage and `audit_phase_7_6a.py`;
- wired the G6A audit into Source/Architecture CI.

Safety/performance:
- no public parameter name/type/`DefaultValue` changed;
- no RR/confidence/entry/SL/TP/risk/execution threshold changed;
- no decision or broker-mutation authority changed;
- no new broker enumeration or unbounded cache introduced.

Manual cTrader boundary:
- target-terminal panel refresh timing, scenario/suitability/break-even visual updates,
  reconnect/reload and responsiveness remain manual.

## CR7.6b / G6B — Execution-control truth and single UI authority

Status: **VERIFIED COMPLETE — implementation head `7832f47c05117f66686adf659fd92670dcb14ba8` passed all three repository gates.**

Scope/root cause:
- the 2026-09-29 execution-status contract made AUTO TRADE / AUTO ORDERS status-only because
  public cTrader Indicator parameters are the execution configuration authority;
- later panel code had reintroduced click handlers that mutated private runtime flags, creating
  a second UI-owned execution state.

Implemented:
- removed `ExecutionToggleHandlers.cs` and its in-panel execution mutation path;
- kept the modern ToggleButton presentation while making the execution status surfaces explicitly
  non-interactive;
- added the platform-neutral Core `ExecutionControlPresentationRule` as the single owner of
  status text and the read-only interaction policy;
- made `ExecutionControlsSynchronizer` re-apply the read-only state while continuing to consume
  `EnsureExecutionRuntimeState()` as the settings → runtime synchronization boundary;
- reconciled accumulated architecture, project-integrity and CR3.4 audits with the status-only
  contract instead of weakening the underlying execution-state safety model;
- added deterministic Runtime Acceptance coverage and dedicated `audit_phase_7_6b.py`.

Safety/performance:
- no public parameter name/type/`DefaultValue` changed;
- no RR/confidence/entry/SL/TP/risk/execution threshold changed;
- no decision or broker-mutation authority changed;
- no new broker enumeration or unbounded cache introduced.

Verification:
- Source/Architecture: **PASS** — #2339;
- Runtime Acceptance Contracts: **PASS** — #2148;
- cTrader Compile: **PASS** — #2332.

Manual cTrader boundary:
- target-terminal click behavior, startup/reload synchronization, disabled-control readability,
  reconnect/reload and responsiveness remain manual.

**Next phase after G6B closeout: CR7.6c.**

## Prompt 8 Remediation Gate — H1–H6 — 2026-10-01

Status: **ACTIVE REMEDIATION TRACK — CR8.3a/H3-A VERIFIED COMPLETE; CR8.3b/H3-B NEXT.**

Prompt 8 is the next fully specified remediation sequence after Prompt 7.
It does not reopen or renumber Prompt 4, Prompt 5, Prompt 6 or Prompt 7.

Authoritative order:

`CR8.1/H1 → CR8.2/H2 → CR8.3a/H3-A → CR8.3b/H3-B → CR8.4/H4 → CR8.5a/H5-A → CR8.5b/H5-B → CR8.6/H6 → CR-FINAL`

### CR8.1 / H1 closeout — directional execution-fill acceptance

Status: **VERIFIED COMPLETE — PR #149 merged to `main`.**

Required phase confirmation:
- «تأیید می‌کنم» — verified the fill chain through
  `ValidateActualMarketFill`, `IsExecutableFillPrice`,
  `AggressiveAcceptedFillHandler` and
  `AutomaticMarketFillReconciliation`.

Completed:
- Core `ExecutionFillAcceptanceRule.IsAcceptable` is now direction-aware and
  owns the single execution-fill envelope;
- favorable BUY fills below requested entry and favorable SELL fills above
  requested entry are accepted when the caller enables the explicit
  `allowFavorable` policy;
- only adverse movement is bounded by ATR and
  `MaximumEntryExtensionAtr`;
- Automatic Market and intent validation consume the same Core rule;
- the duplicate Aggressive absolute-distance envelope was removed;
- Automatic Market validates the broker fill before actual-fill reconciliation
  and before `LivePosition` publication;
- post-fill reconciliation failure is fail-closed.

Behavior change:
- favorable fills that were previously rejected by the symmetric envelope are
  now accepted;
- no public parameter name/type/`DefaultValue` or numerical trading default
  was changed.

Verification:
- Source/Architecture PASS — run #2348;
- Runtime Acceptance Contracts PASS — run #2157;
- cTrader Compile PASS — run #2341;
- deterministic H1 Runtime Contracts PASS;
- `tools/audit_phase_8_1.py` PASS and accumulated in Source/Architecture CI.

Performance/code-cleanliness:
- one duplicated Aggressive fill-distance calculation was removed;
- no new broker enumeration, unbounded cache or second execution authority.

نیاز به تست دستی در cTrader:
- real fill timing/gaps, broker rejection and post-rejection close behavior;
- Automatic Market/Aggressive lifecycle ordering;
- startup/reload/reconnect and panel/chart state around execution.

### CR8.2 / H2 closeout — Top-Down absolute strength

Status: **VERIFIED COMPLETE — PR #150; final implementation HEAD 675283b82e345a86b1a6094e7b7ad66ec3632b84.**

Completed:
- canonical TopDownCalibrationGroupResult now exposes bounded AbsoluteStrength;
- absolute strength is the weighted average quality of the dominant-direction frames;
- HTF strong status requires both existing relative alignment and absolute strength;
- opposing mid-frame and entry evidence can block a strong anchor only when that opposing evidence is itself sufficiently strong;
- Decision and panel presentation expose the canonical HTF, mid-frame and entry absolute-strength diagnostics;
- the status-only execution-control synchronizer now reads its existing guard state, eliminating the CS0414 warning without removing the architecture-required guard;
- Decision Contracts cover weak aligned evidence, strong alignment, weak opposing evidence, strong opposing evidence, entry conflict and deterministic repeatability;
- audit_phase_8_2.py is accumulated in Source/Architecture CI.

Safety/performance:
- no public parameter name/type/DefaultValue changed;
- no RR/confidence/entry/SL/TP/risk/execution threshold was retuned;
- no broker mutation path or decision authority was added;
- no unbounded cache or additional hot-path broker enumeration was introduced;
- the user-reported CS0414 warning is removed by making the existing guard a real re-entrancy guard.

Verification on final implementation HEAD:
- Source/Architecture PASS — workflow run 36892859244;
- Runtime Acceptance Contracts PASS — workflow run 36892859151;
- cTrader Compile PASS — workflow run 36892859090.

Manual cTrader boundary:
- target-terminal top-down timing/presentation, replay distribution and empirical signal quality remain manual;
- the phase makes no profitability or win-rate claim.

Next specified phase: **CR8.3b / H3-B**.

Sequence continuity:
- Prompt 7 G6B is closed on `main`;
- the roadmap references CR7.6c/G6C, but no authoritative G6C scope or
  implementation branch was present on the 2026-10-01 `main` HEAD, so no
  speculative G6C behavior was invented;
- CR8.2/H2 is verified complete on PR #150 with all three repository gates passing on final implementation HEAD.
- next specified phase is **CR8.3a / H3-A**.


### CR8.3a / H3-A closeout — Skender settings ownership — 2026-10-01

Status: **VERIFIED COMPLETE — PR #151 implementation head `b19e3366b0115d79a6e6a61b79af310ac64bbdd7`.**

Completed:
- centralized all fixed production Skender settings under immutable Core `OssIndicatorSettings.Default`;
- removed those fixed defaults from `OssIndicatorParameters`, leaving cache/history and safety semantics there;
- migrated Aroon, Bollinger Bands, CCI, MACD signal, MFI, Parabolic SAR, Stochastic and SuperTrend adapters;
- preserved RSI and MACD fast/slow values as parameter-driven inputs exactly as before;
- added deterministic Planning/Runtime contract coverage for all preserved defaults;
- added `tools/audit_phase_8_3a.py` and accumulated it after H2 in Source/Architecture CI;
- reconciled the accumulated CR4.4 fixed-settings audit with the new owner;
- added the phase-specific completion record `docs/PHASE-CR8-3A-H3-A-SKENDER-SETTINGS.md`.

Verification on the H3-A implementation head:
- Source / Architecture: **PASS**;
- Runtime Acceptance Contracts: **PASS**;
- cTrader Compile / Build: **PASS**.

Safety/performance boundary:
- no public parameter name, type or DefaultValue changed;
- no RR/confidence/entry/SL/TP/risk/execution threshold tuned;
- no decision or broker-mutation authority changed;
- no unbounded cache, network/file I/O or new broker enumeration introduced;
- H3-B warm-up-window, bounded-computation, cache-design and numerical-parity optimization are explicitly deferred to the next phase;
- target-terminal timing/parity and empirical signal-quality remain manual acceptance items.

**Next phase: CR8.3b / H3-B — Skender warm-up, bounded computation, cache design and numerical-parity optimization.**


### CR8.3b / H3-B implementation record — 2026-10-01

Status: **VERIFIED COMPLETE — PR #152; final implementation head `b0ddaabed9723515d50ac183592f1eb7d56b5942`; merged as `db52531a5fe333d2645cdd5f63ac33844d01f8e0`.**

Completed in branch `phase/cr8-3b-h3-b-skender-warmup-cache-parity`:
- bounded stable Skender quote window at 768 bars;
- incremental stable-window cache with first/last boundary fingerprints;
- removal of stable-prefix copies;
- removal of per-call Skender result-list materialization;
- deterministic Runtime Contract coverage for the warm-up policy;
- deterministic 2048-bar full-prefix versus bounded-window parity/performance benchmark;
- new `audit_phase_8_3b.py` accumulated after H3-A;
- accumulated CR4.4 audit semantics reconciled to the bounded stable window.

Safety boundary:
- no public parameter name/type/DefaultValue changed;
- no trading/RR/confidence/entry/SL/TP/risk/execution threshold tuned;
- no decision or broker-mutation authority changed;
- FacioQuo remains research-only;
- target-terminal timing, live performance and empirical signal quality remain manual acceptance items.

Verification on final implementation head `b0ddaabed9723515d50ac183592f1eb7d56b5942`:
- Source / Architecture: **PASS**;
- Runtime Acceptance Contracts: **PASS**;
- cTrader Compile / Build: **PASS**;
- OSS indicator benchmark: **PASS**.

Operator action after merge: run `git pull --ff-only` on local `main` before continuing.

**Continuation gate:** CR8.4 / H4 is intentionally deferred until **CI-FINAL — Full-Stack Calculation Integrity Certification** is complete. Prompt 8 is paused, not renumbered; after CI-FINAL, resume the existing Prompt 8 sequence at CR8.4 / H4.