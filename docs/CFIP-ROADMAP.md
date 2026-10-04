# CFIP — Canonical Master Roadmap
## CFIP-ROADMAP.md
### Zero-to-Full Forensic Review, Hardening, Certification and Continuation Contract
### Canonical edition: 2026-10-04

> **STATUS: ACTIVE / CANONICAL**
>
> This file is the only active development roadmap for CFIP. Its executable work units are defined by the atomic package map in `docs/CFIP-LIST.md`.
> Historical roadmaps, old phase numbering, review plans, hotfix plans and previous sequencing are archival only.
> A new chat/account must read this file and \`docs/CFIP_GATE.md\`, inspect actual current \`main\`, and continue from the first phase marked **NEXT**.

---

# Current execution state

**Macro phase:** P2 — Ownership / Single-Source / Dead-Code Closure  
**Atomic package:** WP-05 — Preflight  
**Status:** PASS + TERMINAL PENDING
**Progress:** 5/70 atomic packages fully PASS (7.1%); WP-05 repository scope 100% complete, target-terminal acceptance 0/13 scenarios completed.

WP-05 repository implementation is complete and merged. Target-terminal acceptance remains the blocking external boundary. The macro phase remains P2 while atomic packages advance independently.

# 1. Mission

CFIP is reviewed as one complete system:

**Market Data → Canonical Time/Price Context → MTF → Indicators/OSS → Structure/Zones/Liquidity/Regime → Evidence → Decision → Entry/Trigger → Plan → Risk → Scenario/Contract → Signal → Alert → Indicator/Contracts/cBot → Preflight → Broker → Broker Truth → Protection/Lifecycle → Outcome/History → Calibration → Presentation**

This roadmap is a gated engineering program, not a feature wishlist. The objective is correctness first, then safety, consistency, robustness, predictive quality, performance and controlled evolution.

# 2. Non-negotiable laws

## 2.1 Single owner

**One concept → one owner → one source of truth → many read-only consumers.**

Repair the canonical owner. Do not add a second calculation, state machine, renderer, audio path, execution engine, contract or overlapping safety gate for the same semantic question.

## 2.2 Architecture before patch

For every defect:
1. identify the concept;
2. identify its canonical owner;
3. inspect all callers/consumers;
4. prove where the contradiction enters;
5. repair the owner;
6. migrate consumers;
7. remove the competing path;
8. verify the complete chain.

A compile-only workaround is not a valid closure.

## 2.3 Broker truth

Planned/requested state is never broker-confirmed state.

- accepted submission is not fill confirmation;
- rejection is not success;
- broker-confirmed state is authoritative;
- protective mutations may reduce risk but never silently widen it;
- restart/reconnect must reconcile broker truth before assuming lifecycle state.

## 2.4 Indicator / cBot boundary

\`CFIP.Indicator\` owns analysis and canonical signal/plan intent.

\`CFIP.Contracts\` owns the platform-neutral cross-boundary contract.

\`CFIP.cBot\` owns broker mutation and execution lifecycle.

The cBot must not recreate the Indicator's analytical engine. The Indicator must not mutate the broker.

## 2.5 Timeframe law

Production MTF is exactly:

**M1 / M5 / M15 / M30 / H1 / H4 / D1 / W1**

- M15 = canonical decision/reference/execution-planning frame.
- M5 = trigger/retest/breakout/entry-precision layer; not a competing execution clock.
- M1 = optional confirmation/precision evidence only; it cannot create directional consensus alone.
- M30/H1/H4 = higher-context support.
- D1/W1 = optional broader context only.
- **M2 / two-minute is forbidden.** It must not exist as provider, Bars request, enum, cache, panel item, parameter, contract field, signal source, fallback or execution clock.

## 2.6 Closed-bar law

Confirmed decision semantics use the canonical fully closed-bar contract. Intrabar/reaction semantics must be explicit and isolated.

No look-ahead, future-bar acceptance, open/closed index mixing, or use of stale closed-bar state as a substitute for current executable quote state.

## 2.7 Presentation law

Presentation consumes authoritative state. It does not decide, score, validate or rebuild trading semantics.

Current canonical contracts:
- one \`SignalVisualSnapshot\`;
- one direction resolver;
- one nine-level directional strength ladder;
- M1 precision marker is non-directional;
- one arrow renderer;
- one plan-line renderer;
- one plan-label renderer/formatter;
- bounded stale-object lifecycle;
- plan lines: Solid, 1px, finite 40 chart bars;
- exact-price compact labels anchored by the canonical label owner.

## 2.8 Alert law

One causal event produces one canonical event record and one delivery lifecycle.

Event creation, queue/delivery and presentation are distinct roles, but no duplicate startup sound, popup, panel alert, email event or retry-generated duplicate event may exist.

Blocked/restricted candidates cannot create trading side effects.

## 2.9 Safety law

Automatic entry is fail-closed. Recoverable faults may reduce availability but cannot silently authorize unsafe automatic entry. Safety-critical management must remain independent from telemetry/UI/persistence failures where feasible.

## 2.10 Performance law

Correctness precedes optimization. Optimize only measured hot paths and prove semantic equivalence before/after.

## 2.11 Repository hygiene law

The repository itself is production infrastructure and must remain minimal, intentional and dependency-closed.

During the forensic review, every tracked artifact must be classified as:
**ACTIVE / REFERENCE / ARCHIVE / DELETE-CANDIDATE**.

Identify and remove, when proven unused:
- orphan source files;
- dead or unreachable tooling/audits;
- obsolete tests/contracts;
- accidentally tracked generated/build artifacts;
- temporary/debug files;
- duplicate configuration;
- superseded scripts;
- stale screenshots/logs/reports;
- redundant documentation;
- abandoned compatibility shims;
- empty or meaningless artifacts.

A file is not kept merely because it is historical, familiar, or referenced by another obsolete artifact. Trace dependencies to actual active consumers. Migrate valid knowledge or active dependencies to the canonical owner before deletion. Deletion is allowed only after proving no active build, CI, audit, runtime, test, release, deployment, or continuity path depends on the artifact.

The final repository must have **zero unexplained tracked files** and **zero duplicate artifacts** representing the same behavior, authority, or contract.

# 3. Canonical terminology

| Term | Meaning |
|---|---|
| Observation | Raw market/platform observation with explicit observation time |
| Closed context | Canonical fully closed MTF references |
| Reaction | Explicit intrabar/open-bar observation under a defined contract |
| Evidence | Measured fact with provenance |
| Independent evidence | Evidence not merely another representation of the same information |
| Decision | Canonical direction/quality/confidence result |
| Actionability | Eligibility for a defined action now |
| Trigger | Entry-timing qualification, primarily M5 |
| Plan | Entry, SL, TP, reward-path and risk geometry |
| Scenario | One distinct execution opportunity |
| Execution intent | Contract crossing Indicator → cBot boundary |
| Submission | Broker request |
| Confirmation | Broker/server-confirmed result |
| Lifecycle | State progression of managed broker entities |
| Outcome | Broker-confirmed realized result |
| Calibration | Governed empirical evaluation of live-policy quality |

# 4. Identity model

These are distinct and must remain distinct:

**SignalId → ScenarioId → ExecutionId → Broker position/order identity → Outcome identity**

Retries preserve scenario identity. New opportunities create new scenario identity. Stale revisions must not mutate current opportunities. Missing identity evidence must fail closed rather than invent continuity.

# 5. Every-phase Definition of Done

A phase is complete only when all applicable items are true:

1. Root cause is evidence-based.
2. Canonical owner is identified.
3. Relevant callers/consumers/contracts are audited.
4. Competing/dead paths are removed or redirected.
5. Invalid, boundary and stale states are covered where applicable.
6. BUY/SELL symmetry is checked.
7. Time/index semantics are explicit.
8. Public parameters are unchanged unless intentionally governed.
9. No hidden threshold/constant or workaround was introduced.
10. Focused tests/contracts pass.
11. Whole-project integrity gate passes.
12. Relevant static audits pass.
13. cTrader Release compile passes.
14. Performance impact is assessed.
15. Safety impact is assessed.
16. \`CFIP_GATE.md\` is updated with evidence.
17. Exactly one next phase is marked.
18. Manual target-terminal requirements are explicitly recorded.

# 6. Phase sequence

## P0 — Baseline Truth & Roadmap Bootstrap
**Status: PASS**

Re-baseline from actual current \`main\`: exact HEAD, project graph, production inventory, CI, warnings, parameters, MTF surface, owner map, cBot boundary, known legacy/duplicate paths and target-terminal boundaries.

Mandatory first checks:
- prove M2 absence;
- fingerprint repository structure;
- identify current critical owners;
- identify current open defects;
- reconcile roadmap/gate with code, not historical documents.

Exit: reproducible baseline and filled P0 section in \`CFIP_GATE.md\`.

## P1 — Repository / Build / Dependency Integrity
**Status: PASS**

Audit solution/projects, ProjectReference/PackageReference, target frameworks, source inclusion, generated/temporary artifacts, orphan/dead files, duplicate configuration, Debug/Release parity, CI drift, warnings, assembly/algo names, dependency provenance and reproducible Release. Every repository artifact must also be classified for KEEP / MIGRATE / ARCHIVE / DELETE; active dependencies must be migrated before deletion.

Exit: build graph is intentional and clean.

## P2 — Ownership / Single-Source / Dead-Code Closure
**Status: NEXT**

Audit calculation, state, decision, actionability, trigger, plan, risk, scenario, contract, visual, label, arrow, alert, sound, execution, broker mutation, lifecycle, persistence and history ownership, together with dead production source, duplicate tools, obsolete tests, superseded scripts and redundant documentation.

Exit: one semantic owner per critical concept; competing/dead paths removed.

## P3 — Market Data / Time / Price / MTF / Closed-Bar Integrity

Audit bar indices, next-bar-open boundaries, gaps, UTC/DST, observation time, Bid/Ask, executable side, spread, pip/tick/digits, broker min distance, cache freshness and quote refresh.

Exit: deterministic temporal and executable-price semantics; no look-ahead.

## P4 — Numerical / Formula / Boundary Integrity

Audit ATR, ADX/DMI, EMA, RSI, MACD, volatility, range efficiency, choppiness, VWAP/volume transformations, rounding, conversions, finite values, hidden clamps and native-vs-CFIP-vs-OSS ownership.

Exit: deterministic numerical contracts and no second formula engine.

## P5 — Analytical Stack

Audit native indicators, OSS adapters, trend/momentum, swing/structure, MSS/CHOCH, liquidity sweeps, FVG, OB, retest, divergence, WaveTrend, volume profile/VWAP and higher-timeframe context.

Exit: each detector/lifecycle has one owner; provenance is preserved.

## P6 — Evidence Independence / Confluence / Regime

Audit correlation, double counting, Trend/Momentum/Context grouping, OB/FVG overlap, structure/liquidity interaction, divergence modifiers, live pressure and regime transitions.

Exit: independent evidence votes once and provenance reaches decision.

## P7 — Decision / Score / Confidence / Actionability

Audit direction, score, quality, confidence, WATCH/CONFIRMED/READY/BLOCKED/RESTRICTED, range handling, stale decisions and canonical block reasons.

Exit: one decision/actionability authority; consumers do not rebuild.

## P8 — Entry / Trigger / Plan / SL / TP / RR

Audit M5 trigger/retest/breakout, optional M1 confirmation, entry side, requested/executable/fill prices, structural SL, TP1..TP4 where contracted, obstacle/reward path, spread/slippage, RR, minimum distance, trailing and break-even.

Exit: no wrong-side target, SL widening, RR-by-rounding or trailing backtrack.

## P9 — Scenario / Identity / Contract Integrity

Audit SignalId/ScenarioId/ExecutionId, revision/staleness, current vs future semantics, market/aggressive/pending intents, codec/schema and idempotency.

Exit: no duplicate execution or stale mutation.

## P10 — Signal Visual State / Chart Rendering

Audit \`SignalVisualSnapshot\`, direction, nine-level strength, arrow stack, M1 marker, lines, exact-price labels, anchoring, colors and stale-object cleanup.

Exit: one signal = one visual set; renderer is presentation-only.

## P11 — Alerts / Popup / Sound / Email

Audit event identity, queue, delivery, cooldown, dedup, retry, startup cue, panel mirror, sound, email and blocked/restricted behavior.

Exit: one causal event = one delivery lifecycle.

## P12 — Panel / Startup / UI Runtime

Audit async startup, readiness, handlers, teardown, refresh, MTF lamp/text parity, header/footer, alert rail, hide/show, narrow widths, chart-height behavior and UI hot-path performance.

Exit: no duplicate handlers and no contradictory operator-facing state.

## P13 — Indicator ↔ Contracts ↔ cBot Boundary

Audit contracts, provider freshness, binding/rebind, restart, missing/stale provider, version compatibility and mutation permissions.

Exit: cBot is sole broker mutation authority; cBot does not invent analysis.

## P14 — cBot Preflight / Risk / Capacity / Broker Rules

Audit connection/account/symbol/market, spread/session/safety gates, margin, volume normalization, distance/freeze rules, capacity, idempotency, direction conflicts and current decision refresh immediately before submission.

Exit: unsafe requests never reach broker.

## P15 — Broker Execution / Submission / Retry / Confirmation

Audit market/pending/aggressive submissions, one submission gate, identity, backoff, retries, rejected/accepted/unknown states, fills, slippage and pending confirmations.

Exit: submission ≠ confirmation; one retry policy and one identity.

## P16 — Protection / Lifecycle / Recovery / Restart / Reconnect

Audit initial protection, break-even, profit lock, trailing, partial TP/close, reversal, invalidation, end-of-day, restart, reconnect, adoption and reconciliation.

Exit: broker-confirmed state is authoritative; recovery idempotent; risk never widens.

## P17 — History / Persistence / Outcome / Calibration

Audit identity, dedup, retention/archive, buffered persistence, failure recovery, outcome attribution, MAE/MFE, calibration eligibility and policy isolation.

Exit: no duplicate/lost outcome and no implicit live-policy mutation.

## P18 — Performance / Allocation / Cache / Hot Path

Measure startup, Calculate, closed-bar rebuild, MTF/zone cache, allocations, chart churn, panel updates, timers and persistence.

Exit: measured improvement with semantic equivalence.

## P19 — Replay / OOS / Ablation / Signal-Quality Proof

Build deterministic replay, live-vs-replay parity, walk-forward/OOS, ablation, confidence calibration, false-signal/missed-opportunity, MAE/MFE and reward-distribution measurement.

Exit: no tuning without controlled evidence; predictive/risk/execution/computational quality remain separate.

## P20 — Full Target-Terminal Acceptance

Verify attach/startup, panel, MTF, arrows, M1 marker, lines, labels, alerts, sound, email, cBot binding, market/pending execution, rejection, fills, protection, trailing, close, restart, reconnect and cleanup.

Exit: every terminal-required scenario has evidence.

## P21 — Security / Configuration / Release / OSS / Reproducibility

Audit secrets, access rights, local paths, config, parameter compatibility, dependency/license provenance, package versions and deployment artifact reproducibility.

Exit: no unsafe environment assumption or undocumented dependency.

## Repository-wide cleanup requirement

P22 performs the final zero-unclassified-artifact sweep across the entire repository, not only `docs/`. Remove every proven-unused source, test, tool, audit, configuration, generated artifact, temporary file, stale report, redundant document, and compatibility shim. Preserve valid evidence only in a clearly marked archive/reference boundary.

No tracked file may remain without a justified status, owner/category, and dependency decision.

## P22 — Final Repository Cleanup / Certification

Remove or clearly archive legacy roadmap references, dead docs/anchors, stale names, generated artifacts, contradictory contracts and temporary workarounds.

Certification requires:
- P0–P21 closed;
- no OPEN P0/P1 defect;
- M2 absent;
- Indicator broker mutation = zero;
- one critical owner per concept;
- clean Release build;
- static/runtime contracts green;
- terminal acceptance complete.

## P23 — Controlled Production Observation

Only after certification: observe drift, execution failures, resource health and safety behavior. No silent strategy changes.

## P24 — Future Cloud-Analysis Readiness

Prepare a future cloud analysis brain only after local certification. Contracts remain the boundary; cBot remains local broker/execution authority; cloud cannot become a hidden second decision engine.

# 7. Mandatory audit on every phase

Every phase also checks:
- repository artifact hygiene: new/deleted/moved files, orphan/dead/generated/temporary artifacts, stale docs, duplicate tools/configuration, unreferenced tests, and cleanup deltas;
- ownership/duplication;
- BUY/SELL symmetry;
- current/future semantics;
- open/closed semantics;
- quote/signal timestamps;
- numeric invalids;
- stale/boundary states;
- contract compatibility;
- panel/chart parity;
- alert/audio uniqueness;
- Indicator/cBot authority;
- broker-confirmed truth;
- lifecycle/recovery;
- persistence/outcome;
- performance;
- terminal implications;
- documentation continuity.

# 8. Evidence package

Every phase closeout records:
- phase ID/status/date;
- exact main baseline;
- implementation commit(s)/PR;
- changed files;
- canonical owner before/after;
- consumers/callers audited;
- competing paths removed;
- tests/static/runtime/build;
- terminal/broker evidence where required;
- performance;
- safety impact;
- residual risk;
- next phase;
- operator pull instruction.

# 9. Status protocol

Use exactly:
**BLOCKED / NEXT / IN PROGRESS / VERIFICATION / PASS / PASS + TERMINAL PENDING / REOPENED / N/A**

Only one phase may be NEXT.

# 10. Parameter governance

Every public parameter has an owner, type, default, valid range, consumer, semantic purpose, safety impact and relevant test coverage.

No parameter exists solely to hide a defect. Strategy-changing parameter changes require the relevant planning/evidence gates.

# 11. Quality governance

Order of concern:

**Correctness → Safety → Consistency → Robustness → Predictive Quality → Performance → Controlled Optimization**

Win rate alone is never sufficient evidence.

# 12. Manual terminal boundary

Static CI cannot prove actual chart appearance, audio audibility, terminal event ordering, broker-server behavior, fill/slippage, restart timing, reconnect timing or live resource profile. Those need target-terminal evidence.

# 13. Current canonical architecture snapshot

Production projects:
- \`CFIP.Indicator\`
- \`CFIP.Contracts\`
- \`CFIP.cBot\`

Authority:
**Indicator Analysis → Decision/Plan → Contracts → cBot Preflight/Execution → Broker → Confirmed Lifecycle → Outcome**

Visual:
**Decision/Plan → SignalVisualSnapshot → Chart/Panel**

Alerts:
**Canonical Event → Alert Queue/Delivery → contracted channels**

Current protected contracts:
- MTF = M1/M5/M15/M30/H1/H4/D1/W1;
- M15 decision/reference;
- M5 trigger/precision;
- M1 optional confirmation;
- cBot-only broker mutation;
- one nine-level directional strength ladder;
- M1 non-directional precision marker;
- Solid 1px finite 40-bar plan lines;
- single label owner;
- one causal alert delivery lifecycle.

# 14. New-chat / new-account boot sequence

A fresh assistant must:
1. read \`docs/CFIP-ROADMAP.md\`;
2. read \`docs/CFIP_GATE.md\`;
3. inspect actual \`main\` and latest CI;
4. compare code with the gate baseline;
5. find the first phase marked NEXT;
6. audit the affected chain and all global invariants;
7. execute one complete phase;
8. close it in \`CFIP_GATE.md\`;
9. mark exactly one next phase here.

Do not rely on chat memory, screenshots or old phase numbering when repository evidence differs.

# 15. Conflict rule

If any historical document, issue, review, screenshot or memory conflicts with these two canonical files, use:
**actual current code + verified evidence**, with this file defining execution order and \`CFIP_GATE.md\` defining acceptance.

# 16. Permanent P0 prohibitions

The following are P0 defects if reintroduced:
- M2;
- Indicator broker mutation;
- second execution engine;
- second decision engine;
- second critical visual owner;
- second startup sound owner;
- duplicate signal event path;
- consumer-side decision recomputation;
- hidden strategy thresholds;
- blind auto-rearm;
- plan state masquerading as broker state;
- protective SL widening;
- duplicate lifecycle ownership;
- unbounded hot-path scanning/I/O;
- silent calibration mutation.

# 17. Current state

**Current macro phase: P3**

**Historical snapshot:** WP-05 was the executable package at the time of this historical entry; the current executable state is recorded at the canonical control-plane header above.

**Canonical inspection inventory:** \`docs/CFIP-LIST.md\`

**Canonical gate:** \`docs/CFIP_GATE.md\`

**Execution unit:** one complete atomic work package per response.

**Roadmap authority:** this file for macro order; CFIP-LIST for atomic scope.



## 17.1 P0 / WP-00 closeout — 2026-10-04

**Status: PASS**

WP-00 established the reproducible baseline from actual `main` HEAD `207e43b7d5293db4445e3f00e9b2b008c95f8dea`.

Evidence:
- Git tree: 1,131 files, 82 directories, 218 Markdown files; tree not truncated.
- Canonical inventory drift was found and corrected: four omitted files were `.vscode/extensions.json`, `docs/CFIP-LIST.md`, `docs/CFIP-PROMPT.md`, and `docs/CFIP-PREPROMPT.md`.
- Production C# baseline: 668 files.
- Public parameters: 545 unique declarations across 30 parameter source files; machine audits report zero unread candidates.
- MTF contract: M1/M5/M15/M30/H1/H4/D1/W1; M2 absent.
- Indicator direct broker mutation: zero.
- cBot→Indicator ProjectReference: zero.
- Source/Architecture, Runtime Acceptance and cTrader Compile are PASS on the exact HEAD.
- Current accumulated ownership, UI, MTF, execution-boundary and realtime audits are PASS.
- Target-terminal evidence remains pending for host-only behavior.

Residual control-plane defect:
- Active tooling still contains references to historical `docs/ROADMAP.md`; this is registered as DEF-P0-002 and is queued for WP-03/WP-04 migration/isolation. Historical documents do not override the canonical control plane.

This historical section records the package that was current when this entry was written; it is not active execution authority.

# 18. CFIP operator/product contract

These requirements are treated as acceptance targets and must be verified against the actual implementation before being declared PASS.

## Analysis and timeframe behavior
- All supported analyzers may evaluate their own valid timeframe context concurrently.
- M15 remains the decision/reference center of the system.
- M5 is the trigger/entry precision layer and must not become a second decision clock.
- M1 is optional confirmation/precision only.
- Higher frames provide context and larger structural/reward-path information.
- M2 remains permanently forbidden.

## Execution behavior
- Automatic trading and automatic order placement belong to the cBot, not the Indicator.
- Current actionable market opportunities may be submitted immediately when all gates pass.
- Future opportunities may become pending Stop/Limit orders according to the canonical scenario contract.
- Multiple distinct opportunities are a product requirement; capacity policy must be explicit rather than accidentally single-plan. Any current capacity restriction must be a deliberate, visible risk policy and must never be hidden in UI or fallback logic.
- Demo and live broker behavior must share one execution architecture; environment selection must not create a second execution engine.
- Current deployment may be local on the same machine/terminal; the contract boundary must remain suitable for later cloud-hosted analysis without moving broker authority out of cBot.
- Position sizing must respect account risk, margin, broker volume limits, minimum executable risk and maximum viable reward-path constraints.
- Spread/slippage/executable quote effects must be included wherever entry, SL or TP safety requires them.
- No generic fixed 1:2 RR rule is the strategy authority.

## Signal quality
- The system must reject weak/range conditions when evidence does not justify an actionable signal.
- Direction, confidence, quality, actionability, trigger, plan and execution must never disagree without an explicit revision/invalid state.
- Stronger reward is desirable only when the structural path remains valid and risk is controlled.
- OB/FVG/structure/liquidity and higher-timeframe context must inform reward-path quality without becoming duplicated decision engines.

## Chart contract
- One signal produces one canonical visual set.
- Directional arrows are separated deterministically and use the canonical nine-level strength ladder.
- Signal/plan line labels are owned by `PlanLabelRenderer`; the label’s visible text end must sit exactly one chart-bar interval left of the canonical `PlanLineRenderer` line start in actual chart X space. `PlanLabelAnchorCalculator` is the sole owner: it resolves the canonical line-left bar and returns the immediately previous `Bars.OpenTimes` value as the label anchor. The line and label therefore use the same DateTime/OpenTime X system. `PlanLabelRenderer` consumes only that canonical anchor and never mutates `ChartText.Time`.
- `PlanLabelAnchorCalculator` is the sole owner of that one-bar gap, and `PlanLineRenderer` is the sole owner of the materialized line color consumed by the label.
- Label text must use the exact same materialized color as its corresponding line; `Color.White` is forbidden for canonical line labels.
- Pending, parallel and prediction labels must continue through the same renderer/anchor contract; no secondary label geometry is allowed. A parallel candidate matching the canonical plan is not a second opportunity and must not render a second line/label set.
- There is no secondary directional arrow path.
- M1 precision evidence must be distinguishable from directional consensus.
- Level lines remain active for the defined lifecycle and are removed when the related managed position/plan lifecycle ends.
- Labels must remain readable, non-overflowing, price-accurate and placed to the left of the line according to the canonical renderer/anchor contract; the exact visual implementation is accepted only through the canonical renderer.
- Chart geometry must not be recreated by panel, alert or cBot code.

## Panel/runtime contract
- Panel state, chart state, alert state and execution state must derive from the same authoritative semantic result.
- Indicator and cBot identity must be visible with meaningful names; source/assembly placeholders must not be presented as operator identity.
- The panel must clearly distinguish Indicator analysis state from cBot attachment/execution state.
- No repeated popup may ask the operator to choose local/cloud execution when that choice is already governed by the current architecture.
- Operator messages must remain readable and placed in the intended panel message/alert region rather than creating contradictory chart popups.
- Timeframe lamp and text must agree.
- Footer/header density must remain compact and usable after reload, hide/show and narrow-width conditions.
- UI event handlers must not multiply after reload/restart.

## Alert contract
- One causal event produces one canonical delivery lifecycle.
- Startup sound is emitted exactly once per canonical startup event.
- Signal sounds, warning sounds and diagnostic notifications must not duplicate each other for the same cause.
- Blocked/restricted candidates do not create actionable sound/email/trading side effects.
- Panel notification and audio/email, when enabled, are mirrors of the same causal event.

## Lifecycle contract
- Signal/plan visuals and state live only as long as their canonical lifecycle allows.
- Broker-confirmed close ends the corresponding managed visual lifecycle.
- Reconnect/restart cannot create duplicate positions, orders, alerts or protection changes.
- Risk-reducing protection is monotonic.


# 19. Cross-cutting domains that every phase must inspect

The phase titles are not exemptions. Every phase must inspect the following system-wide concerns when affected, and P0 must baseline all of them.

## Data acquisition and history
- history load boundaries and replacement events;
- insufficient history;
- missing bars and gaps;
- source observation ordering;
- cache invalidation after history replacement;
- stale data detection;
- deterministic rebuild rules.

## Concurrency and re-entrancy
- overlapping Calculate/timer/UI callbacks;
- duplicate subscriptions;
- shared mutable state;
- re-entrant execution requests;
- concurrent scenario updates;
- atomic identity/gate decisions;
- race conditions during startup/reload/reconnect;
- thread-affinity rules for cTrader UI/runtime APIs.

## Error handling and fault containment
- exception ownership;
- recoverable vs fatal fault classification;
- fault state transitions;
- retry/backoff/circuit semantics;
- fail-closed automatic entry;
- continuation of safety-critical management during analytical faults;
- no swallowed exceptions that conceal state divergence.

## Observability and auditability
- structured diagnostic events;
- correlation with SignalId/ScenarioId/ExecutionId;
- startup/shutdown/reconnect evidence;
- decision and execution reasons;
- broker request/result evidence;
- protection/lifecycle transitions;
- alert delivery evidence;
- bounded logs;
- no sensitive data leakage.

## Testing strategy
- unit/pure-rule tests;
- deterministic contract tests;
- integration tests across module boundaries;
- negative/failure-path tests;
- property/boundary tests where valuable;
- replay/regression fixtures;
- build/architecture/static audits;
- target-terminal manual scenarios;
- broker-confirmed acceptance where required.

## Configuration and migration
- default-value changes;
- parameter migration;
- serialized state/schema compatibility;
- environment-specific configuration;
- demo/live configuration;
- backward compatibility only where intentionally required;
- removal of obsolete configuration rather than indefinite compatibility.

## Resource governance
- memory bounds;
- cache bounds;
- queue bounds;
- chart-object bounds;
- timer frequency;
- CPU/allocation limits;
- persistence backpressure;
- graceful degradation when resources are unavailable.

## Deployment and rollback
- Release artifact identity;
- install/update procedure;
- cBot/Indicator version pairing;
- contract compatibility;
- rollback to the prior certified artifact;
- recovery after partial deployment;
- operator-facing validation checklist.

# 20. Canonical defect taxonomy

Every confirmed issue must be classified by root cause, not symptom. Minimum classes:

**DATA / TIME / NUMERICAL / ANALYSIS / EVIDENCE / DECISION / ACTIONABILITY / PLAN / RISK / CONTRACT / IDENTITY / VISUAL / ALERT / UI / EXECUTION / BROKER / LIFECYCLE / RECOVERY / PERSISTENCE / OUTCOME / PERFORMANCE / SECURITY / BUILD / DEPENDENCY / DOCUMENTATION**

A single root cause gets one defect identity even when it produces multiple symptoms.

# 21. Change-impact discipline

Before changing any owner, determine:
- direct callers;
- indirect consumers;
- contracts crossing the boundary;
- visual/panel consumers;
- alert/event consumers;
- execution consumers;
- persistence/outcome consumers;
- tests/static audits;
- performance-sensitive callers.

After the change, re-check the same graph. No phase is closed on file-local evidence only.

# 22. Current state

**Historical macro snapshot:** P0 was marked NEXT in this superseded historical section; the active atomic state is recorded in the canonical control-plane header above.

**Canonical companion:** \`docs/CFIP_GATE.md\`

**Execution unit:** one complete phase per implementation response.

**Roadmap authority:** this file only.


# 23. Atomic execution model

The P0–P24 items are macro domains. They are never assumed to be one-message implementation tasks.

The executable unit is an atomic work package in \`docs/CFIP-LIST.md\`.

Rules:
- one response = one complete atomic work package;
- every package has bounded paths, explicit gates and a closeout;
- a package cannot be closed with a hidden remainder;
- if a package is too large, split it into smaller packages before implementation;
- \`docs/CFIP_GATE.md\` is updated at package closeout;
- \`docs/CFIP-LIST.md\` records package/file inspection state;
- this roadmap records macro progress and the next package.

## Atomic package ordering

**WP-00 → WP-01 → WP-02 → … → WP-69**

Dependencies may block progression, but cannot silently reorder or skip a package. A package may be marked N/A only with an architecture-approved evidence record in CFIP_GATE.

## Package status

Only one atomic package may be \`NEXT\`.

The macro phase status may remain \`IN PROGRESS\` while its atomic packages are being completed.

# 24. File-level review depth

Roadmap phase closure requires the inspection depth defined by \`CFIP-LIST.md\`:

**directory → file → declaration → method → branch → dependency → side effect → lifecycle → verification**

For a file actively changed or found to contain a material defect, line-by-line source inspection is mandatory.

# 25. Control-plane separation

- \`CFIP-ROADMAP.md\` answers **what order and why**.
- \`CFIP-LIST.md\` answers **what exact files/work units**.
- \`CFIP_GATE.md\` answers **how we know it is complete**.

No fourth planning document may become an active authority.


### P3 / WP-03 closeout — 2026-10-04

**Status:** PASS  
**PR:** #283  
**Merge commit:** `e42e57f7bd425c535ecef57c0835d9d22d84313b`

Canonical planning, inventory and acceptance authority was normalized across ROADMAP/LIST/GATE. WORKFLOW continuity was aligned to the canonical bootstrap sequence and README ownership wording was aligned to the Indicator-analysis / cBot-execution boundary. Exact-head Source/Architecture, Runtime Acceptance and cTrader Compile gates passed.

**Historical next:** WP-04 — Legacy control-plane migration and stale-document isolation (completed; retained only as historical closeout evidence).

### Permanent workflow rule — local command handoff
For every Atomic Work Package, if local execution is required, the package MUST explicitly give the user the exact commands, working directory/environment, execution point, expected success signal, and whether output must be returned. CI success never substitutes for a required local verification. This requirement applies to Git sync, build/test/audit, cTrader/cBot compile/runtime probes, artifact generation, and final acceptance.


### Historical continuity registry — preserved canonical evidence

- **CBOT-P5 historical continuity:** broker reconciliation/protection recovery remains represented by the canonical cBot/Indicator boundary, with detailed implementation evidence retained in docs/CONTINUATION-STATE.md and docs/DEVELOPMENT-LOG.md. This registry preserves the historical audit marker without restoring obsolete roadmap authority.

- **CBOT-P6 historical continuity:** account/risk/connection truth remains recorded in canonical continuity evidence; this registry preserves the historical audit marker without reactivating the superseded roadmap.

- **CBOT-P8 historical continuity:** progressive protection state synchronization remains recorded in canonical continuity evidence, including the M15/M5 role boundary; the historical marker does not act as an independent roadmap.


### WP-04 closeout — Historical control-plane isolation — 2026-10-04

**Status: PASS**

PR #284 was merged to `main` as `2e98a4b4cd27dd8b083ee8aac1437cf1a3c6271e` after the final implementation head `3853f3d9708a7b378e8f61edeefb800a52898ee6` passed Source/Architecture #4365, Runtime Acceptance #4174 and cTrader Compile #4358. The obsolete `docs/ROADMAP.md` authority is isolated in the archive boundary, active tooling is aligned to the canonical control plane, and DEF-P0-002 is verified closed.

The completion protocol is now permanent: required CI must be green, the verified work must be merged to `main`, and merged `main` must be re-verified before the package is declared complete.

**Next:** WP-05 — Preflight.
