## Phase 7.4 — MaximumOpenPositions semantics — continuity record

The single-plan execution-capacity semantics are enforced by the canonical
capacity rule and guard. This continuity record preserves the historical
phase identifier required by the project-integrity audit.

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

Detailed Track 19 record:
`docs/TRACK-19-OSS-NUMERICAL-BENCHMARK.md`

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

## CR4.4 — Numerical stability and caching continuity

## CR4.5 / D5 — Per-timeframe regime semantics

CR4.5 is a completed historical continuity item: each canonical timeframe
carries its own normalized regime metadata, non-M5 regime snapshots are bounded
and independently cached, UNKNOWN remains neutral, and frame scoring consumes
the owning frame's regime. Target-terminal timing and replay validation remain
manual acceptance boundaries.

The production OSS indicator adapters use the canonical bounded quote-cache
owners and preserve deterministic warm-up semantics. Historical target-terminal
validation remains a manual acceptance boundary; this continuity marker keeps
the review issue mapped into the current roadmap without changing production
trading policy.

## CR4.6 / D6 — Frame-scoring constant ownership

CR4.6 implementation complete: frame-scoring literals remain under the single
Core constant owner without numerical tuning. Target-terminal timing, replay and
empirical signal-quality validation remain manual boundaries.

## CR4.7 / D7 — TP pipeline feasibility and telemetry

CR4.7 implementation complete: target-stage feasibility, age, geometry and
bounded rejection telemetry remain under their canonical planning owners. No
public parameter/default or RR tuning was introduced; target-terminal validation
remains manual.

## CR4.8 / D8 — TP1 directional defensive validation

CR4.8 implementation complete: TP1 direction and reward-integrity validation use
the canonical target protection rules. No trading threshold tuning or second
decision authority was introduced; target-terminal validation remains manual.

## CR4.9 / D9 — Live reversal action and alert semantics

CR4.9 implementation complete: live reversal action/alert ownership remains
canonical and directionally symmetric. No public parameter/default or RR tuning
was introduced; target-terminal replay remains a manual acceptance boundary.

## CR4.10 / D10 — Native indicator safety and registry performance

CR4.10 / D10 implementation complete: native indicator ownership/safety and
registry lookup boundaries are preserved, with benchmark coverage. No public
parameter/default or RR tuning was introduced; target-terminal validation remains
manual. The next certification transition is CR-FINAL.

## CR5.3 / E3 — Indicator evidence independence

CR5.3 continuity marker: independent evidence-group semantics remain centralized
without turning correlated indicators into duplicate decision authority.

## CR5.4 / E4 — [historical remediation continuity]

CR5.4 continuity marker preserved for the accumulated calculation-integrity
audit chain.

## CR5.5 / E5 — Active remediation continuity

CR5.5 continuity marker preserved for the accumulated calculation-integrity
audit chain.

## CR5.6 / E6 — Signal bias and context continuity

CR5.6 continuity marker preserved for the accumulated calculation-integrity
audit chain.

## CR5.7 / E7 — Watch/reaction alert continuity

CR5.7 continuity marker preserved for the accumulated calculation-integrity
audit chain.

## CR5.8 / E8 — Reward-risk continuity

CR5.8 continuity marker preserved for the accumulated calculation-integrity
audit chain.

## CR6.6 / F7 closeout

Historical continuity marker for CR6.6 / F7 is retained in the accumulated
roadmap audit chain.

## CR6.7 / F8

Historical continuity marker for CR6.7 / F8 is retained in the accumulated
roadmap audit chain.

## CR6.7 / F8 closeout

Historical continuity marker for CR6.7 / F8 closeout is retained in the
accumulated roadmap audit chain.

## CR6.8 / F9

Historical continuity marker for CR6.8 / F9 is retained in the accumulated
roadmap audit chain.

## CR6.8 / F9 closeout

Historical continuity marker for CR6.8 / F9 closeout is retained in the
accumulated roadmap audit chain.

## CR6.9 / F3

Historical continuity marker for CR6.9 / F3 is retained in the accumulated
roadmap audit chain.

## CR6.9 / F3 closeout

Historical continuity marker for CR6.9 / F3 closeout is retained in the
accumulated roadmap audit chain.

## CR7.1 / G1

Historical continuity marker for CR7.1 / G1 is retained in the accumulated
roadmap audit chain.

## CR7.3 / G3 closeout

Historical continuity marker for CR7.3 / G3 closeout is retained.

**Next phase: CR7.4 / G4**

## CR7.5 / G5

Historical continuity marker for CR7.5 / G5 is retained.

Next continuation: CR7.6a

## CR7.6a / G6A

Historical continuity marker for CR7.6a / G6A is retained in the accumulated
roadmap audit chain.

## CI-03 — Indicator fusion / evidence independence

Historical continuity marker: CI-03 is verified complete; later CI phases remain
the active remediation sequence.

## CI-04 closeout — implementation record

CI-04 structure, swing, liquidity and alert-delivery integrity is retained as a verified historical continuity item in the accumulated calculation-integrity chain.

Completed:
- canonical swing plateau, confirmation and structural-break freshness;
- repeated re-break rejection for already-crossed confirmed levels;
- active/unbroken liquidity-sweep validation with BUY/SELL symmetry;
- canonical Structure/MSS/CHOCH event de-duplication;
- unified bounded alert delivery queue for popup and sound;
- popup-before-sound delivery ordering on the same queued alert event;
- direct sound ownership removed from AlertEngine;
- deterministic Runtime Acceptance and accumulated Source/Architecture audit coverage.

Repository verification was completed on the CI-04 implementation head before the later CI phases advanced.


## CI-05 implementation record

CI-05 FVG lifecycle semantics are retained as a completed historical
continuity item in the accumulated calculation-integrity chain.

## CI-06 — Order Block lifecycle

CI-06 Order Block lifecycle integrity is retained as a completed historical
continuity item.

## CI-07 — Market regime, MTF and context audit

CI-07 MTF/regime/context integrity is retained as a completed historical
continuity item.

## CI-08 implementation record

CI-08 divergence, WaveTrend, reaction and early-signal integrity are retained
as a completed historical continuity item.

## CI-09 implementation record

CI-09 decision-engine mathematical integrity is retained as a completed
historical continuity item.

## CI-10 — Trigger and trigger-lifecycle audit

CI-10 trigger/lifecycle integrity is retained as a completed historical
continuity item.

Phase document: `docs/PHASE-CI-10-TRIGGER-LIFECYCLE.md`

## CI-11 — Entry geometry and signal-timing audit

CI-11 entry-geometry and causal signal-timing integrity are retained as a
completed historical continuity item.

Current implementation phase: CI-12
## CI-12 — Structural SL

CI-12 structural-stop integrity is retained as a completed historical
continuity item.

Phase document: `docs/PHASE-CI-12-STRUCTURAL-SL.md`

# Track CI — Full-Stack Calculation & Analytical Integrity (BLOCKING)

Status: **active — CI-00 through CI-11 verified complete; CI-12 is the active blocking phase and CI-FINAL remains the final certification gate.**

This track is introduced after the 2026-10-01 deep review of the Trigger →
Entry → SL → TP chain. It intentionally expands the audit upstream so
correctness is established from raw market data and indicator calculations
through analysis, decision, trigger, trade-plan geometry and broker execution.

The authoritative detailed specification is:
`docs/PHASE-CI-FULL-STACK-CALCULATION-ANALYTICAL-INTEGRITY.md`

The track does **not** renumber or invalidate Prompt 4/5/6/7/8 phases. It is a
blocking correctness gate inserted before the next unfinished refinement phase.

**Current implementation phase: CI-13 — TP source, target obstacle and TP ladder audit.**

### CI-12 closeout — 2026-10-02

Status: **VERIFIED COMPLETE — PR #167 merged to `main`.**

Implementation head: `f452b3b8e2892f1773cef5873a051a2b542e72a6`

Merge commit: `cd10da89ddf8ba9cf1c9da517a3fdc74b6953851`.

Repository verification:
- Source/Architecture #2609: **PASS** (workflow 36939262476);
- Runtime Acceptance Contracts #2418: **PASS** (workflow 36939262418);
- cTrader Compile #2602: **PASS** (workflow 36939262429);
- accumulated audits through CI-12: **PASS**.

Completed:
- canonical structural-stop geometry and immutable geometry snapshot;
- evaluated structural stop is preserved through candidate selection without a second materialization pass;
- obsolete duplicate StructuralStopFinalizer removed;
- canonical M5/HTF buffer semantics and price normalization;
- canonical planning stop-risk envelope with configured maximum independent of spread-derived minimum;
- canonical ATR fallback geometry across planning, preview, parallel and lifecycle/recovery paths;
- fail-closed fallback risk validation;
- deterministic CI-12 Runtime Contracts and static audit;
- accumulated CR5.1/CR6.9/CI-04..CI-12 audits reconciled to the new ownership without removing safety assertions.

Safety:
- no public parameter name/type/DefaultValue changed;
- no RR, confidence, entry, SL, TP, risk or execution threshold was tuned;
- no second decision/plan/broker-mutation authority was introduced;
- broker-confirmed live protection remains authoritative.

Manual boundary:
- target-terminal broker-specific stop distance;
- startup/reconnect recovery;
- actual order/fill protection;
- chart/panel presentation and empirical historical signal/outcome validation remain manual acceptance items.

Phase record: `docs/PHASE-CI-12-STRUCTURAL-SL.md`.

**Next phase: CI-13 — TP source, target obstacle and TP ladder audit.**
**Prompt 8 / CR8.4 remains paused until CI-FINAL.**
