# CFIP — Canonical Gate & Master Defect Register
## CFIP_GATE.md
### Acceptance, Defect, Evidence and Phase-Closeout Contract
### Canonical edition: 2026-10-04

> **STATUS: ACTIVE / CANONICAL**
>
> This is the single acceptance and defect register for CFIP.
> A phase/work package cannot be PASS because the code merely compiles. All applicable gates, scoped files and evidence must be closed.

# Current execution state

**Macro phase:** P2 — Ownership / Single-Source / Dead-Code Closure  
**Atomic package:** WP-05 — Preflight  
**Status:** PASS + TERMINAL PENDING

# 1. Gate hierarchy

- **G0 Repository Truth** — actual branch, commit, files, project graph, CI.
- **G1 Contract Truth** — intended behavior is explicit.
- **G2 Ownership Truth** — one canonical owner exists.
- **G3 Implementation Truth** — owner is correct and consumers migrated.
- **G4 Static/Automated Truth** — audits/contracts pass.
- **G5 Build Truth** — Release build is clean.
- **G6 Runtime Contract Truth** — deterministic runtime scenarios pass.
- **G7 Target-Terminal Truth** — cTrader manual acceptance passes where required.
- **G8 Continuity Truth** — roadmap, gate and repository agree.

A higher gate never excuses a lower-gate failure.

# 2. STOP conditions

Mark BLOCKED when any occurs:
- broker mutation outside cBot;
- duplicate execution/decision/critical visual/audio authority;
- M2 reintroduced;
- look-ahead/future-bar leakage;
- broker-confirmed state replaced by plan/request state;
- protective logic can widen risk;
- duplicate identity can cause duplicate broker action;
- any tracked file/artifact is unclassified, orphaned, accidentally generated, redundant, or retained only through an obsolete dependency;
- an artifact scheduled for deletion still has an active build, CI, audit, test, runtime, release, deployment, or continuity consumer;
- unresolved P0 safety/architecture defect;
- mandatory machine gate fails;
- Release build fails;
- evidence is missing;
- fix requires a parallel workaround;
- code and canonical documentation materially disagree without re-baseline.

# 3. Evidence rules

Every PASS identifies:
- exact baseline commit;
- evidence type;
- exact command/test/audit/scenario;
- result;
- date;
- relevant files/scenario IDs;
- remaining terminal requirement where applicable.

Allowed evidence:
**SOURCE / TEST / AUDIT / BUILD / RUNTIME / TERMINAL / BROKER / PERFORMANCE / DOC**

Never use "looks fine", screenshot-only code proof, CI as proof of audio, or historical evidence for changed code.

# 4. Current baseline record

This record describes the current canonical `main` state. Earlier P0/P1/P2/P3 baseline values remain historical evidence inside their dated closeout sections and are not current execution state.

| Field | Value |
|---|---|
| Repository | armanemp/cfip-indicator |
| Canonical branch | main |
| Baseline commit | `5b16b92a7b272e43c71a3bea14c94cc978e7065e` |
| Baseline date/time | `2026-10-04` — current control-plane verification |
| Indicator | CFIP.Indicator |
| Contracts | CFIP.Contracts |
| cBot | CFIP.cBot |
| Canonical MTF | M1/M5/M15/M30/H1/H4/D1/W1 |
| M15 | decision/reference |
| M5 | trigger/precision |
| M1 | optional confirmation |
| M2 | FORBIDDEN |
| Broker mutation | CFIP.cBot only |
| Macro phase | P2 — Ownership / Single-Source / Dead-Code Closure |
| Atomic package | WP-05 — Preflight |
| Atomic package status | PASS + TERMINAL PENDING |
| Blocking next package | WP-06 — Contracts, blocked pending target-terminal evidence |

# 5. Master defect register

One row per distinct root cause; do not duplicate rows for symptoms of the same cause.

| ID | Domain | Severity | Owner | Symptom | Root cause | Status | Phase | Evidence | Regression guard |
|---|---|---|---|---|---|---|---|---|---|
| DEF-P0-001 | Repository inventory | P1 | CFIP-LIST.md | Canonical inventory lagged actual tree by four files | Inventory snapshot predates the current canonical prompt/control-plane additions and `.vscode/extensions.json` indexing | VERIFIED | P0/WP-00 | Git tree `207e43b7...`: 1131 files; CFIP-LIST enumerated 1127; reconciled in WP-00 closeout | CFIP-LIST exact-tree comparison |
| DEF-P0-002 | Documentation governance | P1 | WP-04 | Active tooling depended on superseded `docs/ROADMAP.md` | Historical audit tooling and documentation used the superseded roadmap as an active dependency; active references were migrated to the canonical control plane and the superseded roadmap was isolated under `docs/archive/ROADMAP-LEGACY-2026-10-04.md` | VERIFIED | WP-04 | PR #284 merged as `2e98a4b4cd27dd8b083ee8aac1437cf1a3c6271e`; Source/Architecture #4365, Runtime #4174, cTrader Compile #4358 all PASS | Global legacy-reference scan + canonical control-plane CI |
| DEF-P0-003 | Architecture boundary | P1 | WP-08 | Indicator still exposes execution/auto-trading parameters and runtime execution state | cBot execution settings are still read from `CFIP.Indicator` parameters, while the canonical architecture requires Indicator = analysis/signal and cBot = broker-mutation/execution authority | OPEN | WP-08 | Source search on `main` confirms `src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs`, `Runtime/Initialization/RuntimeInitialization.cs`, `Trading/Execution/State/AutoTradingStateStore.cs` and cBot `CbotIndicatorExecutionSettings.cs` still form an execution-settings dependency | Boundary extraction audit + CI guard requiring execution authority to originate in cBot/Contracts |

Severity:
- P0 safety/architecture/data-integrity;
- P1 material behavior;
- P2 quality/maintainability;
- P3 optimization/documentation/ergonomics.

Status:
**OPEN / IN_PROGRESS / BLOCKED / FIXED_UNVERIFIED / VERIFIED / ACCEPTED / REOPENED / WONT_FIX_WITH_REASON**

# 6. Repository / Build / Dependency gates — RB

- RB-001 actual main commit captured.
- RB-002 repository/branch assumptions documented.
- RB-003 solution contains only intentional projects.
- RB-004 every required project builds Release.
- RB-005 ProjectReference direction is correct.
- RB-006 PackageReference versions are intentional.
- RB-007 TargetFrameworks match target cTrader/runtime constraints.
- RB-008 production source inclusion is complete.
- RB-009 orphan production files are identified.
- RB-010 generated artifacts are not accidental dependencies.
- RB-011 Debug/Release semantic behavior is equivalent.
- RB-012 CI commands match intended local commands.
- RB-013 compiler warnings are zero or explicitly justified.
- RB-014 assembly/algo names are intentional.
- RB-015 production runtime identifiers contain no historical version names.
- RB-016 no hidden environment dependency.
- RB-017 OSS provenance/license is recorded.
- RB-018 Release artifact is reproducible.
- RB-019 project tree and source tree have no silent mismatch.
- RB-020 latest CI status recorded at phase boundary.
- RB-021 every tracked repository artifact has an explicit classification/status;
- RB-022 orphan, dead, duplicate, temporary, and generated artifacts are identified repository-wide;
- RB-023 DELETE/ARCHIVE candidates have active-consumer dependency traces;
- RB-024 deleted artifacts are proven absent from build/CI/audit/test/runtime/release/deployment/continuity paths;
- RB-025 duplicate documentation/tool/configuration/evidence paths are consolidated to one canonical owner;
- RB-026 preserved historical evidence is moved to an explicit archive/reference boundary;
- RB-027 final repository cleanup leaves zero unexplained tracked artifacts.

# 7. Ownership / No-Duality gates — OW

- OW-001 one market-context owner.
- OW-002 one closed-MTF owner.
- OW-003 one owner per primitive formula.
- OW-004 one owner per analyzer/detector.
- OW-005 one structure owner.
- OW-006 one FVG geometry owner.
- OW-007 one FVG lifecycle owner.
- OW-008 one OB owner/lifecycle.
- OW-009 one liquidity-sweep semantics owner.
- OW-010 one evidence-fusion owner.
- OW-011 one evidence-independence owner.
- OW-012 one regime owner.
- OW-013 one decision owner.
- OW-014 one actionability owner.
- OW-015 one trigger-qualification owner.
- OW-016 one plan/SL/TP/RR owner.
- OW-017 one scenario materialization/identity owner.
- OW-018 one SignalVisualSnapshot owner.
- OW-019 one arrow-strength owner.
- OW-020 one arrow-render owner.
- OW-021 one plan-line owner.
- OW-022 one plan-label owner.
- OW-023 one alert-event owner.
- OW-024 one alert-delivery owner.
- OW-025 one panel-presentation-state owner.
- OW-026 one submission-gate owner.
- OW-027 one broker-mutation owner.
- OW-028 one broker-confirmed-state mapping owner.
- OW-029 one protection owner.
- OW-030 one lifecycle/recovery owner.
- OW-031 one outcome owner.
- OW-032 one persistence owner.
- OW-033 duplicate helpers are removed.
- OW-034 consumer recomputation is removed.
- OW-035 dead fallbacks are removed.
- OW-036 partial-class files contain no alternate engine.
- OW-037 one owner must be traceable from each critical concept to every consumer.

# 8. MTF / Time / Price gates — TP

- TP-001 MTF set exactly M1/M5/M15/M30/H1/H4/D1/W1.
- TP-002 zero production M2 references.
- TP-003 no M2 enum/provider/request/cache.
- TP-004 no M2 parameter/panel/contract/source.
- TP-005 M15 = canonical decision/reference.
- TP-006 M5 = trigger/precision, not competing execution clock.
- TP-007 M1 = optional confirmation only.
- TP-008 D1/W1 = optional context only.
- TP-009 one canonical closed-index resolver.
- TP-010 exact next-bar-open boundary is deterministic.
- TP-011 gap semantics deterministic.
- TP-012 future bar cannot be classified closed.
- TP-013 open/closed references cannot silently mix.
- TP-014 signal reference time distinct from quote observation time.
- TP-015 quote freshness independent from closed-context cache.
- TP-016 UTC/timezone semantics explicit.
- TP-017 DST behavior deterministic.
- TP-018 Bid/Ask/executable side canonical.
- TP-019 spread semantics canonical.
- TP-020 pip/tick/digits semantics canonical.
- TP-021 broker minimum distance canonical.
- TP-022 stale data detectable and safe.
- TP-023 index out-of-range deterministic.
- TP-024 missing timeframe data fails safely.
- TP-025 MTF fixtures deterministic.
- TP-026 no timeframe is an implicit execution clock.
- TP-027 bar/time index is never reconstructed independently by a consumer.

# 9. Numerical / Math gates — NM

- NM-001 ATR ownership canonical.
- NM-002 ADX/DMI ownership canonical.
- NM-003 EMA ownership canonical.
- NM-004 RSI ownership canonical.
- NM-005 MACD ownership canonical.
- NM-006 CFIP-specific math has explicit owners.
- NM-007 OSS numerical boundary explicit.
- NM-008 window selection separated from arithmetic ownership.
- NM-009 zero-volume source observations remain zero-volume.
- NM-010 division-by-zero handled.
- NM-011 NaN handled by contract.
- NM-012 Infinity handled by contract.
- NM-013 illegal negative values rejected.
- NM-014 empty windows defined.
- NM-015 insufficient history defined.
- NM-016 rounding occurs once at canonical boundary.
- NM-017 pip-price conversion not duplicated.
- NM-018 volume normalization finite/non-negative.
- NM-019 hidden clamps eliminated or justified.
- NM-020 threshold constants have explicit owners.
- NM-021 BUY/SELL numerical symmetry checked.
- NM-022 extreme-value fixtures exist.
- NM-023 deterministic numerical tolerances explicit.
- NM-024 optimization preserves reference outputs.

# 10. Analytical / Structure gates — AN

- AN-001 every detector has one clear owner.
- AN-002 structure consumes confirmed swing state.
- AN-003 MSS semantics canonical.
- AN-004 CHOCH semantics canonical.
- AN-005 same-causal structural labels do not duplicate evidence/events.
- AN-006 structural break freshness deterministic.
- AN-007 liquidity level expiry explicit.
- AN-008 sweep requires active/unbroken level.
- AN-009 FVG geometry canonical.
- AN-010 FVG lifecycle canonical.
- AN-011 mitigation/full-fill semantics deterministic.
- AN-012 FVG age bound explicit.
- AN-013 OB geometry canonical.
- AN-014 OB invalidation canonical.
- AN-015 retest semantics canonical.
- AN-016 divergence modifier semantics explicit.
- AN-017 WaveTrend owner/provenance explicit.
- AN-018 volume-profile/VWAP owner explicit.
- AN-019 higher-timeframe context cannot create second decision engine.
- AN-020 analyzers cannot mutate broker.
- AN-021 missing analytical inputs are explicit, not fabricated.
- AN-022 analytical provenance reaches the evidence layer.

# 11. Evidence / Confluence / Regime gates — EV

- EV-001 evidence has provenance.
- EV-002 correlated evidence does not double-vote.
- EV-003 Trend grouping explicit.
- EV-004 Momentum grouping explicit.
- EV-005 Context grouping explicit.
- EV-006 divergence modifier explicit.
- EV-007 aggregate OSS consensus is not an unintended extra vote.
- EV-008 OB/FVG confluence calculated once.
- EV-009 structure/liquidity interaction deterministic.
- EV-010 live pressure separated from closed evidence.
- EV-011 conflict penalty has one owner.
- EV-012 regime transitions explicit.
- EV-013 range-market handling explicit.
- EV-014 missing evidence cannot become positive evidence.
- EV-015 evidence snapshot reaches decision without recomputation.
- EV-016 evidence weights/thresholds are intentional and documented.

# 12. Decision / Actionability gates — DC

- DC-001 one direction resolver.
- DC-002 one score owner.
- DC-003 one quality owner.
- DC-004 one confidence owner.
- DC-005 one actionability owner.
- DC-006 WATCH deterministic.
- DC-007 CONFIRMED deterministic.
- DC-008 READY deterministic.
- DC-009 BLOCKED deterministic.
- DC-010 RESTRICTED deterministic.
- DC-011 block reason canonical.
- DC-012 range handling cannot be bypassed downstream.
- DC-013 stale decisions cannot execute.
- DC-014 decision/actionability refreshed before auto submission.
- DC-015 panel does not decide.
- DC-016 cBot does not rebuild decision.
- DC-017 alert path does not invent decision.
- DC-018 direction cannot change between displayed authority and execution intent without explicit revision semantics.

# 13. Entry / Plan / Risk gates — PL

- PL-001 requested entry distinct from fill.
- PL-002 trigger distinct from decision.
- PL-003 M5 trigger canonical.
- PL-004 M1 confirmation cannot create consensus.
- PL-005 side correct.
- PL-006 executable quote side correct.
- PL-007 spread included where contract requires.
- PL-008 SL structurally derived.
- PL-009 SL cannot widen during management.
- PL-010 TP1 directionally correct.
- PL-011 TP2 directionally correct.
- PL-012 TP3 directionally correct where present.
- PL-013 TP4 directionally correct where present.
- PL-014 reward path checks obstacles/opposing zones.
- PL-015 no generic fixed-RR dependency.
- PL-016 RR computed before display rounding.
- PL-017 broker distance normalization canonical.
- PL-018 trailing cannot backtrack.
- PL-019 break-even/profit lock protective.
- PL-020 sizing respects account/margin/volume constraints.
- PL-021 invalidation explicit.
- PL-022 revision/version semantics explicit.
- PL-023 risk sizing never assumes theoretical volume is broker-accepted volume.
- PL-024 current quote and spread are those actually used at submission.

# 14. Scenario / Contract / Identity gates — CT

- CT-001 SignalId unique.
- CT-002 ScenarioId unique per opportunity.
- CT-003 ExecutionId identifies attempt/lifecycle.
- CT-004 retries preserve ScenarioId.
- CT-005 stale revision cannot mutate current scenario.
- CT-006 current/future semantics explicit.
- CT-007 market execution contract explicit.
- CT-008 pending-order contract explicit.
- CT-009 aggressive path explicit.
- CT-010 execution-critical values are present.
- CT-011 cBot does not guess missing critical values.
- CT-012 serialization deterministic.
- CT-013 schema/version policy explicit.
- CT-014 canonical idempotency key.
- CT-015 broker identity mapping explicit.
- CT-016 outcome identity mapping explicit.
- CT-017 duplicate consumer serialization paths are absent.
- CT-018 contract changes have compatibility/regression coverage.

# 15. Visual / Chart gates — VS

- VS-001 SignalVisualSnapshot is visual source of truth.
- VS-002 direction comes from canonical decision.
- VS-003 strength comes from one nine-level owner.
- VS-004 levels 1–3 = Weak 1/2/3.
- VS-005 levels 4–6 = Medium 1/2/3.
- VS-006 levels 7–9 = Strong 1/2/3.
- VS-007 renderer never recalculates strength.
- VS-008 arrows have deterministic separation.
- VS-009 M1 marker is non-directional.
- VS-010 arrows do not overlap by design.
- VS-011 lines Solid.
- VS-012 thickness 1.
- VS-013 line span finite 40 chart bars.
- VS-014 endpoint uses canonical chart-bar anchor.
- VS-015 label price equals canonical line price.
- VS-016 one label owner.
- VS-017 one formatter/anchor.
- VS-018 current label contract uses deterministic left-of-line placement.
- VS-019 stale objects removed.
- VS-020 expired signal visuals bounded.
- VS-021 renderer has no decision logic.
- VS-022 panel/chart direction parity.
- VS-023 BUY/SELL visual symmetry.
- VS-024 label text cannot overflow its intended display region.
- VS-025 no duplicate visual object created for the same identity.

# 16. Alert / Audio gates — AL

- AL-001 one causal event identity.
- AL-002 one event-creation owner.
- AL-003 one queue/delivery owner.
- AL-004 one sound delivery path.
- AL-005 one popup lifecycle.
- AL-006 panel mirror derives from same event.
- AL-007 email derives from same event where enabled.
- AL-008 cooldown deterministic.
- AL-009 dedup identity-based.
- AL-010 retry does not create duplicate causal event.
- AL-011 one canonical startup cue.
- AL-012 blocked candidate produces no actionable cue.
- AL-013 restricted candidate follows diagnostic contract.
- AL-014 alert failure cannot mutate broker.
- AL-015 alert delivery does not recompute decision.
- AL-016 event text/label semantics have one formatter/source.

# 17. Panel / UI runtime gates — UI

- UI-001 startup state machine explicit.
- UI-002 async initialization bounded.
- UI-003 handler registration idempotent.
- UI-004 teardown unregisters handlers.
- UI-005 readiness is not duplicated.
- UI-006 panel consumes authoritative state.
- UI-007 timeframe lamps/text use same source.
- UI-008 M2 absent.
- UI-009 header/footer respect layout bounds.
- UI-010 alert rail stable.
- UI-011 hide/show has one lifecycle owner.
- UI-012 narrow widths handled.
- UI-013 chart height does not collapse unexpectedly.
- UI-014 update frequency bounded.
- UI-015 no blocking I/O in UI hot path.
- UI-016 stale state detectable.
- UI-017 panel cannot execute trade logic.
- UI-018 panel status text and semantic state cannot diverge.
- UI-019 initialization cannot register duplicate handlers after restart/reload.

# 18. Indicator / Contracts / cBot gates — CB

- CB-001 Indicator broker mutation count = 0.
- CB-002 cBot broker mutation authority count = 1.
- CB-003 Contracts are platform-neutral.
- CB-004 contracts do not hide analytical computation.
- CB-005 cBot consumes canonical decision/plan.
- CB-006 cBot does not recreate indicators.
- CB-007 cBot does not recreate OB/FVG/structure/regime.
- CB-008 binding/rebinding deterministic.
- CB-009 missing provider fails closed.
- CB-010 stale provider fails closed.
- CB-011 restart/reconnect reconciliation explicit.
- CB-012 execution identity survives reconnect.
- CB-013 pre-submit state refresh is mandatory.
- CB-014 cBot UI does not invent alternate state.
- CB-015 execution capacity is explicit/canonical.
- CB-016 current managed strategy identity is shared across market/pending paths.
- CB-017 no hidden cloud dependency.

# 19. Preflight / Broker Execution gates — BR

- BR-001 connection validated.
- BR-002 account validated.
- BR-003 symbol validated.
- BR-004 market state validated.
- BR-005 spread gate canonical.
- BR-006 session gate canonical.
- BR-007 risk/safety gate canonical.
- BR-008 margin capacity checked.
- BR-009 volume min/max/step canonical.
- BR-010 stop/freeze distance validated.
- BR-011 duplicate opportunity rejected idempotently.
- BR-012 stale decision rejected.
- BR-013 direction conflict rejected.
- BR-014 request logged with ExecutionId.
- BR-015 accepted result is not treated as fill.
- BR-016 rejected result is not treated as success.
- BR-017 unknown result handled explicitly.
- BR-018 slippage/fill reconciled.
- BR-019 pending confirmation reconciled.
- BR-020 all broker mutations in one owner.
- BR-021 no broker request bypasses canonical preflight without explicit emergency contract.

# 20. Protection / Lifecycle gates — LC

- LC-001 initial protection validated.
- LC-002 broker-confirmed SL/TP authoritative.
- LC-003 protection cannot widen risk.
- LC-004 break-even idempotent.
- LC-005 profit lock idempotent.
- LC-006 trailing monotonic for risk reduction.
- LC-007 partial TP requires broker confirmation.
- LC-008 partial close requires broker confirmation.
- LC-009 close requires broker confirmation.
- LC-010 reversal explicit.
- LC-011 invalidation explicit.
- LC-012 end-of-day explicit.
- LC-013 reconnect reconciliation precedes lifecycle assumptions.
- LC-014 restart adoption idempotent.
- LC-015 duplicate lifecycle events do not duplicate mutation.
- LC-016 telemetry failure cannot transfer lifecycle ownership.
- LC-017 emergency protection remains independent of non-safety telemetry.

# 21. History / Outcome / Calibration gates — OH

- OH-001 broker-confirmed close is authoritative outcome.
- OH-002 outcome attribution is identity-based.
- OH-003 duplicate outcome rejected.
- OH-004 retention bounded.
- OH-005 archive deterministic.
- OH-006 persistence buffered where required.
- OH-007 persistence failure observable/recoverable.
- OH-008 no synchronous hot-path file I/O where prohibited.
- OH-009 MAE/MFE uses correct identity/time.
- OH-010 recovery-only records marked non-calibratable where required.
- OH-011 calibration cannot silently mutate live policy.
- OH-012 memory bounded.
- OH-013 schema migration explicit.
- OH-014 historical data cannot masquerade as live broker state.
- OH-015 calibration dataset eligibility is separate from execution truth.

# 22. Performance gates — PF

- PF-001 startup measured.
- PF-002 Calculate measured.
- PF-003 closed-bar rebuild bounded.
- PF-004 MTF cache reuse measured.
- PF-005 zone scan bounded.
- PF-006 allocations reviewed.
- PF-007 chart churn bounded.
- PF-008 panel updates bounded.
- PF-009 persistence queue bounded.
- PF-010 timers bounded.
- PF-011 optimized path matches reference semantics.
- PF-012 no unbounded history scan on hot path.
- PF-013 no redundant indicator calculation.
- PF-014 no unnecessary repeated serialization.

# 23. Replay / Quality gates — QP

- QP-001 deterministic fixtures exist for critical semantics.
- QP-002 replay deterministic.
- QP-003 live/replay semantic parity checked.
- QP-004 in-sample/out-of-sample separated.
- QP-005 walk-forward evidence before tuning.
- QP-006 ablation available for material components.
- QP-007 confidence calibration measured.
- QP-008 false-signal rate measured.
- QP-009 missed opportunities measured.
- QP-010 MAE measured where possible.
- QP-011 MFE measured where possible.
- QP-012 range-market quality measured.
- QP-013 realized reward distribution measured.
- QP-014 execution quality separated from predictive quality.
- QP-015 raw win rate is never the only metric.
- QP-016 sample-size sufficiency is explicit before statistical conclusions.

# 24. Target-Terminal gates — TA

- TA-001 indicator attaches cleanly.
- TA-002 startup completes correctly.
- TA-003 panel size/layout correct.
- TA-004 panel updates.
- TA-005 MTF display correct.
- TA-006 M2 absent.
- TA-007 arrows display correctly.
- TA-008 arrows do not overlap.
- TA-009 M1 marker is non-directional.
- TA-010 lines correct.
- TA-011 labels readable/anchored.
- TA-012 stale visuals disappear.
- TA-013 panel direction matches chart.
- TA-014 alert text matches semantics.
- TA-015 one startup sound audible.
- TA-016 signal sound behavior correct.
- TA-017 email behavior correct where enabled.
- TA-018 cBot binds/rebinds.
- TA-019 current signal execution correct.
- TA-020 pending scenario correct.
- TA-021 rejection visible and safe.
- TA-022 fill state broker-confirmed.
- TA-023 SL/TP protection correct.
- TA-024 break-even/trailing correct.
- TA-025 close/end lifecycle correct.
- TA-026 restart recovery correct.
- TA-027 reconnect recovery correct.
- TA-028 no duplicate actions.
- TA-029 terminal resource usage acceptable.
- TA-030 post-close visual cleanup correct.

# 25. Security / Release / Governance gates — SR

- SR-001 no secrets in source.
- SR-002 access rights are intentional.
- SR-003 no unsafe hard-coded local production path.
- SR-004 external dependencies have provenance.
- SR-005 licenses recorded.
- SR-006 package versions intentional.
- SR-007 public parameter contract documented.
- SR-008 deployment artifact reproducible.
- SR-009 release naming stable.
- SR-010 exactly one active roadmap: \`docs/CFIP-ROADMAP.md\`.
- SR-011 exactly one active acceptance register: \`docs/CFIP_GATE.md\`.
- SR-012 historical documents cannot override canonical contracts.
- SR-013 operator instructions explicit.
- SR-014 repository alone is sufficient to resume a new chat/account.
- SR-015 current main baseline is always recorded.

# 26. Phase-to-gate mapping

| Phase | Mandatory groups |
|---|---|
| P0 | RB, OW, TP, SR |
| P1 | RB, SR |
| P2 | OW |
| P3 | TP |
| P4 | NM |
| P5 | AN |
| P6 | EV |
| P7 | DC |
| P8 | PL |
| P9 | CT |
| P10 | VS |
| P11 | AL |
| P12 | UI |
| P13 | CB |
| P14 | BR |
| P15 | BR, CT |
| P16 | LC |
| P17 | OH |
| P18 | PF |
| P19 | QP |
| P20 | TA |
| P21 | SR, RB |
| P22 | ALL |
| P23 | QP, PF, OH |
| P24 | CB, CT, RB, SR |

Cross-phase mandatory audits always apply.

# 27. Phase closeout template

## [P#] — [NAME]
**Status:**  
**Baseline commit:**  
**Implementation commit(s):**  
**PR:**  
**Date:**  

### Root cause
[Evidence]

### Canonical owner
[Exact owner]

### Callers/consumers audited
[Evidence]

### Changes
[Summary]

### Duplicate/legacy paths removed
[Evidence]

### Gate results
| Level | Result | Evidence |
|---|---|---|
| G0 | PASS/BLOCKED | |
| G1 | PASS/BLOCKED | |
| G2 | PASS/BLOCKED | |
| G3 | PASS/BLOCKED | |
| G4 | PASS/BLOCKED | |
| G5 | PASS/BLOCKED | |
| G6 | PASS/BLOCKED | |
| G7 | PASS/PENDING/BLOCKED | |
| G8 | PASS/BLOCKED | |

### Whole-project audit
[Result]

### Performance
[Measured result]

### Safety
[Result]

### Residual risk
[Only open items]

### Next phase
[Exactly one]

### Operator action
[\`git pull --ff-only\` when required]

# 28. Regression policy

A PASSed gate becomes **REOPENED** when:
- owner code changes;
- a consumer changes interpretation;
- contract changes;
- duplicate path appears;
- regression test fails;
- terminal evidence contradicts contract;
- broker evidence invalidates an assumption.

Do not weaken the gate to preserve PASS.

# 29. Defect lifecycle

**OPEN → ROOT_CAUSE_CONFIRMED → OWNER_IDENTIFIED → FIXED → STATIC_VERIFIED → BUILD_VERIFIED → RUNTIME_VERIFIED → TERMINAL_VERIFIED → CLOSED**

P0/P1 defects require explicit regression guards.

# 30. Duplicate-path audit method

For each critical concept:
1. where calculated?
2. where stored?
3. where transformed?
4. where rendered?
5. where alerted?
6. where executed?
7. where persisted?
8. where recovered?
9. which are owners vs consumers?
10. is any consumer recomputing it?
11. is any fallback returning a different answer?
12. can two paths fire for one identity?
13. can stale state survive after owner changes?

Two active semantic owners = FAIL.

# 31. Full-chain evidence trace

For the affected behavior, trace:

**Market observation → Time semantics → Evidence → Decision → Actionability → Signal → Alert → Execution intent → Preflight → Broker request → Broker confirmation → Protection → Lifecycle → Outcome → Presentation**

Every broken link is a gate failure.

# 32. Canonical contracts to guard

- MTF = M1/M5/M15/M30/H1/H4/D1/W1; M2 forbidden.
- M15 = decision/reference.
- M5 = trigger/entry precision.
- M1 = optional confirmation.
- cBot = only broker mutation authority.
- SignalVisualSnapshot = visual source.
- nine-level strength ladder = one owner.
- M1 marker = non-directional.
- plan lines = Solid, 1px, finite 40 bars.
- labels = one owner, canonical exact-price formatter/anchor.
- alerts = one event/delivery lifecycle.
- broker-confirmed state = authoritative.
- protection = risk-reducing only.

# 33. Required repository-level machine audit coverage

At minimum, every phase keeps semantic coverage for:
- project/build integrity;
- architecture/ownership;
- MTF/timeframe prohibition;
- parameter semantics;
- runtime state invariants;
- signal/plan synchronization;
- alert uniqueness;
- visual ownership;
- cBot/Indicator boundary;
- persistence/outcome;
- performance/hot-path constraints.

Tool names may evolve, but semantic coverage may not be silently removed.

# 34. Acceptance matrix

| Surface | Static | Runtime | Terminal | Broker |
|---|---:|---:|---:|---:|
| MTF/closed-bar | Required | Required | Required where visible | — |
| Decision | Required | Required | Required where visible | — |
| Entry/plan | Required | Required | Required | Required |
| Chart visuals | Required | Required | Required | — |
| Alerts | Required | Required | Required for audibility/timing | — |
| cBot binding | Required | Required | Required | — |
| Execution | Required | Required | Required | Required |
| Protection | Required | Required | Required | Required |
| Lifecycle | Required | Required | Required | Required |
| Outcome/history | Required | Required | Required where terminal-dependent | Required |

# 35. P0 baseline deliverables

P0 must produce:
1. actual main commit;
2. project/file inventory;
3. current CI snapshot;
4. M2 scan;
5. current parameter inventory;
6. project reference graph;
7. critical owner map;
8. duplicate/legacy report;
9. current warnings/build;
10. terminal-boundary list;
11. defects entered here;
12. stale roadmap-reference inventory;
13. this baseline table completed;
14. \`docs/CFIP-ROADMAP.md\` synchronized.

# 35.1 P0 / WP-00 closeout — 2026-10-04

**Status:** PASS  
**Baseline commit:** `207e43b7d5293db4445e3f00e9b2b008c95f8dea`  
**Baseline time:** `2026-10-04T11:36:56Z`  
**Atomic package:** WP-00 — Rebaseline

### Baseline truth
- Actual `main` HEAD: `207e43b7d5293db4445e3f00e9b2b008c95f8dea`.
- Repository tree: **1,131 files / 82 directories / 218 Markdown files**; tree is not truncated.
- Canonical inventory before closeout enumerated 1,127 files. The four-file delta was: `.vscode/extensions.json`, `docs/CFIP-LIST.md`, `docs/CFIP-PROMPT.md`, `docs/CFIP-PREPROMPT.md`. All four are now indexed.
- Production projects: `CFIP.Indicator`, `CFIP.Contracts`, `CFIP.cBot`.
- Production C# baseline from the machine gate: **668 files**.
- Public parameter baseline: **545 declarations / 545 unique**, across **30 parameter source files**; zero unread candidates and no duplicate public parameter names.
- Canonical MTF: **M1/M5/M15/M30/H1/H4/D1/W1**; M2 absent.

### CI / build evidence
- Source / Architecture: **PASS**, run `37199344197`, all 157 audit steps completed successfully.
- Runtime Acceptance Contracts: **PASS**, run `37199344236`.
- cTrader Compile: **PASS**, run `37199344261`; Release restore/build/run sequence completed for Contracts, cBot, CI, Decision/Planning/Execution contracts, Shadow tests and Preflight projects. Indicator compilation is covered through the Preflight project reference.
- Current CI logs report **0 failures** for the cBot boundary audit and **0 direct broker mutation calls in Indicator**.
- M2 audit: **PASS / ABSENT**.
- Current runtime/UI audit: **PASS**.
- Current single-owner/no-duality audit: **PASS**.
- The only runner-level warning observed in the Source job is the GitHub Actions Node.js 20 deprecation warning for `actions/checkout@v4` / `actions/setup-python@v5`; it is not a C# compiler warning.

### Project / dependency graph
- Solution contains exactly the three production projects: Indicator, Contracts and cBot.
- Production projects target `net6.0`.
- Indicator package pins: `cTrader.Automate 1.0.21`, `Skender.Stock.Indicators 2.7.3`; Contracts is dependency-free; cBot references Contracts and cTrader.Automate.
- CI/runtime harness projects intentionally compile selected canonical production owners directly; F1 audit reports **12 projects scanned**, `cTrader.Automate=[1.0.21]`, `Skender.Stock.Indicators=[2.7.3]`, **214 neutral Core/Contracts files scanned**, **0 Runtime.Contracts linked platform files**, and **0 cBot→Indicator ProjectReferences**.
- `Directory.Build.props`: deterministic build, nullable disabled globally, implicit usings disabled, warnings not treated as errors.
- `CFIP.Indicator.csproj`: `net6.0`, deterministic, default SDK source inclusion, Contracts reference.
- `CFIP.cBot.csproj`: `net6.0`, deterministic, Contracts reference, no Indicator reference.

### Canonical owner map
- Closed-bar boundary: `ClosedBarReferenceRule`; MTF closed context: `MtfClosedContext`.
- Decision/actionability/plan semantics remain in their existing canonical Core/Decision/Trading owners; presentation consumes canonical snapshots.
- Direction/strength: `MtfTrendStrengthRule` with the canonical nine-level ladder.
- Signal visual snapshot: `SignalVisualSnapshotBuilder` / `SignalVisualSnapshot`.
- Arrow rendering: canonical arrow renderer; stale legacy arrow renderers are absent by current dedicated audit.
- Plan line geometry: `PlanLineRenderer`; plan label rendering: `PlanLabelRenderer`; label formatting/anchor have dedicated canonical owners.
- Alert delivery: `AlertDeliveryProcessor`; cBot lifecycle audio: `CbotLifecycleAudioService`.
- Broker mutation: cBot execution owners only; Indicator direct mutation count is zero.
- Broker-confirmed lifecycle/recovery: cBot lifecycle/reconciliation owners.
- The current machine gates confirm these ownership boundaries without introducing a second owner.

### Duplicate / parallel-path scan
- Source/Architecture full accumulated audit: PASS.
- Single-owner/no-duality audit: PASS.
- Smart separated signal-arrow audit: PASS; obsolete arrow owners are absent.
- Signal drawing canonical audit: PASS; one line owner, one label owner, solid 1px/40-bar geometry.
- Realtime/live unification audit: PASS; Indicator remains broker-mutation-free and cBot remains execution authority.
- No duplicate execution/decision/critical visual/audio path was detected by the active accumulated gates.
- No M2 path was detected.

### Safety / performance / terminal boundary
- Safety: fail-closed execution and cBot-only broker mutation gates PASS; live account arm remains explicit/default-off per current CI evidence.
- Performance: accumulated optimization/hot-path audits PASS; no new hot-path code was introduced by WP-00.
- Terminal-only evidence remains required for actual cTrader chart geometry, sound audibility, attachment visibility, broker-server fills/slippage, restart/reconnect timing and live resource behavior. WP-00 does not claim terminal PASS.

### Historical/open-PR boundary
- Open PRs inspected at baseline include #279, #273, #272, #263, #258, #256, #254, #253, #251, #246, #244, #241, #239, #235 and #226.
- These PRs are not part of the `main` baseline unless their changes are actually present in HEAD; no open PR was treated as authoritative over current `main`.
- PR #272 is an older baseline-gate repair against an earlier base and is not merged into the current HEAD; it is not used as WP-00 evidence.

### Residual defects
- **DEF-P0-001** inventory drift is fixed in this closeout.
- **DEF-P0-002** historical `docs/ROADMAP.md` remains referenced by active audit tooling and must be migrated/isolated under WP-03/WP-04. This does not change current runtime authority, but it is a canonical-control-plane dependency and remains OPEN.

### Gate results
| Gate | Result | Evidence |
|---|---|---|
| G0 Repository Truth | PASS | HEAD/tree/CI/project graph captured |
| G1 Contract Truth | PASS | Canonical MTF, boundary and product contracts reconciled |
| G2 Ownership Truth | PASS | Accumulated single-owner/no-duality audits |
| G3 Implementation Truth | PASS | Current main source and cBot boundary audits |
| G4 Static/Automated Truth | PASS | Source/Architecture run 37199344197 |
| G5 Build Truth | PASS | cTrader compile run 37199344261 |
| G6 Runtime Contract Truth | PASS | Runtime Acceptance run 37199344236 |
| G7 Target-Terminal Truth | PASS + TERMINAL PENDING | WP-00 cannot prove host-only behavior statically |
| G8 Continuity Truth | PASS | Three canonical files synchronized by this closeout |

# 35.2 P1 / WP-01 closeout — 2026-10-04

**Status:** PASS  
**Implementation branch:** `phase/p1-wp01-root-build-metadata-2026-10-04`  
**Baseline HEAD:** `f81bfee89dbbc18324892007c26678cc4d5e8639`

### Root/build audit
- `CFIP.Indicator.sln` contains exactly the three canonical production projects: Indicator, Contracts, cBot.
- All 13 project files were inspected for target framework, project references, package references, source inclusion and shared build-property duplication.
- `Directory.Build.props` is the canonical owner for `Deterministic=true` and `ImplicitUsings=disable`.
- Duplicate per-project declarations of those shared defaults were removed from the 11 affected net6 projects plus the net8 benchmark's redundant Deterministic declaration.
- `CFIP.Contracts` retains its intentional `Nullable=enable`; other nullable-disabled projects retain their explicit semantic override.
- `CFIP.StockIndicators.Benchmark` intentionally retains `net8.0` and `ImplicitUsings=enable`; its dedicated workflow supplies the .NET 8 SDK.
- `.gitignore` now protects generated benchmark output and common temporary/build artifacts while preserving the intentionally tracked `.vscode/extensions.json`.

### Artifact classification
- No tracked `bin/`, `obj/`, TestResults, coverage, publish/dist, archive package, log, tmp, backup or generated benchmark-report artifact was present in the actual tree.
- Active production/build/test/tool files are **KEEP**.
- Benchmark project and benchmark workflow are **KEEP** because the active OSS benchmark workflow consumes them.
- `.vscode/extensions.json` is **KEEP** as the sole intentional editor recommendation; the rest of `.vscode/` is ignored.
- No DELETE/ARCHIVE candidate met the required proof threshold in WP-01; no artifact was removed merely by name.

### Owner / dependency / consumer analysis
- Shared build defaults: `Directory.Build.props` → every descendant SDK project.
- Production build authority: the three canonical projects in `CFIP.Indicator.sln`.
- Indicator source compile harness: `tools/CFIP.Indicator.CI` → canonical Indicator source.
- Runtime/contract harnesses and Preflight were traced through their active CI commands.
- OSS benchmark: `.github/workflows/oss-benchmark.yml` → `tools/CFIP.StockIndicators.Benchmark`; generated `benchmark-report.md` is now explicitly ignored.
- No duplicate production project owner or duplicate ProjectReference was introduced.

### Safety / performance
No runtime trading logic, thresholds, parameters, contracts, broker mutation, renderer or alert/audio path changed. Centralizing identical MSBuild defaults reduces configuration drift without changing evaluated values. Artifact ignore changes affect repository hygiene only.

### Verification boundary
The existing accumulated source, runtime, cTrader build, M2, cBot-boundary and single-owner gates remain mandatory. WP-01 additionally requires the F1 root/build metadata checks introduced in `audit_phase_f1_repository_build_dependency_truth.py`.

### Terminal
No target-terminal behavior changed. Manual terminal acceptance remains unchanged and is not claimed by this repository metadata package.

# 36. Certification

Certification requires:
- P0–P22 PASS;
- no OPEN P0/P1 defect;
- no unresolved duplicate critical owner;
- no M2;
- no Indicator broker mutation;
- no secondary decision or execution engine;
- no mandatory gate failure;
- Release clean;
- required static/runtime contracts green;
- terminal acceptance complete;
- continuity synchronized;
- all residual risks classified.

# 37. New-chat / new-account continuity

A fresh assistant reads only the two canonical files for planning and acceptance:
1. \`docs/CFIP-ROADMAP.md\`
2. \`docs/CFIP_GATE.md\`

Then it verifies actual \`main\` and latest CI.

It reports:
- current phase;
- baseline;
- blockers;
- exact next action;
- relevant gate IDs.

Then it executes one complete phase and updates both canonical files.

Historical documents are evidence/archive only.

# 38. Current state

**Canonical roadmap:** \`docs/CFIP-ROADMAP.md\`

**Canonical gate:** \`docs/CFIP_GATE.md\`

**Macro phase:** P2 — Ownership / Single-Source / Dead-Code Closure

**Current atomic package:** WP-05 — Preflight

**Current status:** PASS + TERMINAL PENDING

**Blocking next package:** WP-06 — Contracts, blocked pending target-terminal evidence.

**Execution unit:** one complete atomic work package per implementation response.

**Last instruction:** never continue from historical phase numbering when it conflicts with the canonical files or current code.


# 39. CFIP operator/product acceptance gates

These gates capture the intended CFIP behavior in addition to architectural invariants.

## Product / execution — UXE

- UXE-001 all valid timeframe analyzers can operate concurrently without creating competing decision clocks.
- UXE-002 M15 remains the canonical decision/reference frame.
- UXE-003 M5 is trigger/entry precision, not a second decision authority.
- UXE-004 M1 is optional confirmation only.
- UXE-005 higher-timeframe context is available to reward-path/structure logic without duplicate decision ownership.
- UXE-006 no M2 exists anywhere in production behavior.
- UXE-007 auto-trade/order placement is owned only by cBot.
- UXE-008 current actionable opportunity can reach immediate execution only after all canonical gates pass.
- UXE-009 future opportunity can map to a pending order only through canonical scenario/execution intent.
- UXE-010 concurrent opportunities are represented distinctly by identity and an explicit capacity policy.
- UXE-011 demo/live selection does not create a second execution engine.
- UXE-012 local deployment does not hard-code away future cloud-analysis compatibility.
- UXE-013 sizing respects account/margin/broker-volume/risk constraints.
- UXE-014 spread/executable quote is included where required by entry/SL/TP safety.
- UXE-015 no fixed generic 1:2 RR owner exists.
- UXE-016 weak/range conditions cannot silently become actionable through a downstream bypass.
- UXE-017 OB/FVG/structure/liquidity context contributes through canonical evidence/plan owners.
- UXE-018 displayed signal, alert and execution intent share identity and direction.

## Chart/panel — UXP

- UXP-001 one signal creates one visual set.
- UXP-002 directional arrows use one nine-level strength source.
- UXP-003 arrows have deterministic non-overlap separation.
- UXP-004 M1 precision is visually distinguishable from directional consensus.
- UXP-005 level lines remain through the defined active lifecycle.
- UXP-006 level lines are removed after the canonical lifecycle ends.
- UXP-007 labels are exact-price and readable.
- UXP-008 labels are positioned left of the line by the canonical anchor.
- UXP-009 label text does not overflow.
- UXP-010 panel/chart direction agrees.
- UXP-011 Indicator and cBot names shown to operator are meaningful, not source placeholders.
- UXP-012 cBot attachment/execution status is derived from one canonical state.
- UXP-013 no repeated local/cloud choice popup exists.
- UXP-014 operator messages stay inside the intended UI message region.
- UXP-015 timeframe lamps and text agree.
- UXP-016 UI density remains usable on reload/hide/show/narrow widths.
- UXP-017 UI handlers remain single-registration.

## Alert/audio — UXA

- UXA-001 one causal event creates one delivery lifecycle.
- UXA-002 one canonical startup event creates one startup cue.
- UXA-003 duplicate startup sounds are impossible through code paths.
- UXA-004 signal/warning/diagnostic cues are causally distinct.
- UXA-005 blocked/restricted candidates have no actionable side effect.
- UXA-006 panel/audio/email mirrors cannot drift in event identity.
- UXA-007 retry/dedup cannot replay the same causal event unintentionally.

## Broker/lifecycle — UXB

- UXB-001 broker-confirmed close drives visual lifecycle completion.
- UXB-002 reconnect cannot duplicate an order/position.
- UXB-003 restart cannot duplicate lifecycle mutation.
- UXB-004 protective changes never widen risk.
- UXB-005 execution identity survives the full submission/fill/lifecycle chain.
- UXB-006 current/future opportunity semantics remain distinct after recovery.


# 40. Cross-cutting gates — XG

## Concurrency / re-entrancy — XGC
- XGC-001 Calculate cannot execute conflicting state transitions concurrently.
- XGC-002 timer supervision cannot re-enter unsafe broker mutation.
- XGC-003 startup/readiness callbacks are idempotent.
- XGC-004 reload/restart cannot multiply event subscriptions.
- XGC-005 reconnect cannot race duplicate reconciliation.
- XGC-006 scenario identity/gating is atomic enough to prevent duplicate submission.
- XGC-007 shared mutable state has explicit ownership.
- XGC-008 cTrader UI/runtime calls respect required thread/event boundaries.

## Data acquisition / history — XGD
- XGD-001 history load boundaries are explicit.
- XGD-002 history replacement invalidates affected caches.
- XGD-003 insufficient history is explicit and safe.
- XGD-004 missing bars/gaps are deterministic.
- XGD-005 source observations have deterministic ordering.
- XGD-006 stale data can be detected.
- XGD-007 rebuild decisions have one owner.
- XGD-008 no historical replacement can silently preserve stale derived state.

## Fault containment / errors — XGF
- XGF-001 recoverable/fatal faults are distinct.
- XGF-002 fault transitions have one owner.
- XGF-003 retries use one policy per semantic domain.
- XGF-004 retry cannot duplicate causal event or execution identity.
- XGF-005 swallowed exceptions cannot silently leave stale state.
- XGF-006 automatic entry fails closed under unsafe faults.
- XGF-007 safety-critical management can continue when non-safety analysis fails, where architecture permits.
- XGF-008 fault reason is observable.

## Observability — XGO
- XGO-001 critical events have correlation identity.
- XGO-002 decision reason is observable.
- XGO-003 execution request/result is observable.
- XGO-004 broker confirmation is observable.
- XGO-005 protection/lifecycle transitions are observable.
- XGO-006 startup/reload/reconnect are observable.
- XGO-007 alert delivery/dedup behavior is observable.
- XGO-008 log volume is bounded.
- XGO-009 sensitive information is not emitted.
- XGO-010 diagnostics do not become a second state authority.

## Testing / verification architecture — XGT
- XGT-001 pure mathematical rules have focused tests.
- XGT-002 boundary/invalid-value cases are covered.
- XGT-003 contract boundaries have deterministic tests.
- XGT-004 negative/failure paths are tested.
- XGT-005 idempotency is tested.
- XGT-006 replay/regression fixtures cover critical historical defects.
- XGT-007 static audits remain layered, not duplicated.
- XGT-008 target-terminal cases are mapped separately from automation.
- XGT-009 broker-confirmed cases are not simulated as broker truth without evidence.
- XGT-010 tests assert semantic contracts, not merely implementation details.

## Configuration / migration — XGCN
- XGCN-001 every default change has an explicit reason.
- XGCN-002 obsolete parameters are removed rather than left as hidden compatibility paths.
- XGCN-003 serialized state migration is explicit.
- XGCN-004 contract/config schema changes have compatibility rules.
- XGCN-005 demo/live configuration is one architecture.
- XGCN-006 environment-specific behavior is explicit.
- XGCN-007 configuration cannot bypass safety gates.

## Resource / backpressure — XGR
- XGR-001 caches have bounded memory.
- XGR-002 queues have bounded capacity.
- XGR-003 chart object count is bounded.
- XGR-004 timer work is bounded.
- XGR-005 persistence backpressure is handled.
- XGR-006 memory growth is observable.
- XGR-007 graceful degradation is defined for unavailable resources.
- XGR-008 hot-path CPU/allocation budgets are measured.

## Deployment / rollback — XGP
- XGP-001 Release artifacts have stable identity.
- XGP-002 Indicator/cBot compatibility is explicit.
- XGP-003 deployment steps are reproducible.
- XGP-004 rollback artifact is known.
- XGP-005 partial deployment/restart recovery is defined.
- XGP-006 operator validation checklist exists.
- XGP-007 release cannot silently pair incompatible contracts.


# 40. Atomic work-package gate contract

The canonical executable unit is the work package defined in \`docs/CFIP-LIST.md\`.

For each work package, record:
- package ID;
- macro phase;
- exact scoped paths;
- files inspected;
- lines/regions inspected where applicable;
- canonical owner;
- caller/consumer graph;
- defect IDs;
- gate IDs;
- tests/audits;
- build result;
- runtime result;
- terminal requirement;
- performance/safety result;
- closeout commit;
- next package.

A package is **PASS** only when its entire declared scope is closed.

No partial package is carried silently into the next response.

# 41. Atomic package gate mapping

| Package range | Default gate focus |
|---|---|
| WP-00–05 | RB, SR, TP, OW, XGD, XGF, XGO |
| WP-06–11 | CB, CT, BR, LC, SR |
| WP-12–18 | RB, OW, TP, NM, AN |
| WP-19–25 | TP, AN, EV, DC |
| WP-26–29 | PL, CT, DC |
| WP-30–35 | XGC, XGF, UI, TP, CB |
| WP-36–44 | AL, LC, BR, PL, CT |
| WP-45–50 | VS, UI, AL |
| WP-51–56 | RB, SR, XGT, PF |
| WP-57–64 | OW, TP, CT, VS, AL, CB, LC, OH |
| WP-65–67 | PF, SR, XGT, QP |
| WP-68–69 | SR, RB, OW, ALL |

This is the minimum gate set. Cross-phase/global invariants still apply.

# 42. Inventory synchronization gate

- INV-001 current Git tree has been compared with CFIP-LIST.
- INV-002 every added/deleted/moved file has a classification.
- INV-003 every production file has an owner or an explicit ARCHIVE/DELETE decision.
- INV-004 every critical production path has relevant gate IDs.
- INV-005 no new production file is left UNSEEN.
- INV-006 inventory source commit is recorded.
- INV-007 CFIP-LIST and CFIP-ROADMAP identify the same current executable package.
- INV-008 CFIP-GATE and CFIP-LIST have matching package completion state.
- INV-009 every tracked file has a lifecycle/classification decision;
- INV-010 every DELETE/ARCHIVE candidate has a dependency trace;
- INV-011 repository cleanup deltas are reflected in the inventory;
- INV-012 no obsolete artifact remains merely because another obsolete artifact references it.

# 43. Repository artifact hygiene gate

A repository-cleanliness PASS requires:
- HYG-001 current Git tree is compared with CFIP-LIST;
- HYG-002 every tracked file has KEEP / MIGRATE / ARCHIVE / DELETE disposition;
- HYG-003 every DELETE candidate has no active consumer;
- HYG-004 every ARCHIVE candidate is outside active authority/tooling;
- HYG-005 generated/build/temp artifacts are ignored/untracked or intentionally retained with an owner;
- HYG-006 duplicate documentation/tool/configuration/evidence paths are consolidated;
- HYG-007 deletion is verified against project inclusion, CI, tests, and release packaging;
- HYG-008 canonical control-plane docs remain the only active roadmap/gate authority;
- HYG-009 final tree contains zero unexplained files.

A single unresolved HYG-* item blocks the applicable cleanup package.

# 44. Global line-audit gate

For any material code file being changed or flagged:
- LINE-001 all declarations inspected;
- LINE-002 all relevant methods inspected;
- LINE-003 all branches/fallbacks inspected;
- LINE-004 all side effects inspected;
- LINE-005 all caller/consumer links traced;
- LINE-006 all applicable error/boundary paths inspected;
- LINE-007 all changed lines have a reason;
- LINE-008 suspicious retained lines are classified;
- LINE-009 test/audit proof is mapped;
- LINE-010 no second semantic owner is introduced.

# 45. Current executable control state

**Macro phase:** P2 — Ownership / Single-Source / Dead-Code Closure

**Executable package:** WP-05 — PASS + TERMINAL PENDING; WP-06 Contracts is BLOCKED pending target-terminal evidence.

**Roadmap:** \`docs/CFIP-ROADMAP.md\`

**Inventory:** \`docs/CFIP-LIST.md\`

**Gate:** \`docs/CFIP_GATE.md\`

**Rule:** one complete atomic package per response.


# 35.3 P2 / WP-02 closeout — 2026-10-04

**Status:** PASS  
**Implementation branch:** `phase/p2-wp02-github-workflows-2026-10-04`  
**PR:** #282  
**Merge commit:** `f6ef574355a115393ef5279036724675ac18af4a`

### Root cause
The four active workflows lacked one enforced contract for permissions, bounded runtime and stale-run cancellation. The OSS benchmark pipeline could also mask a failing `dotnet run` through `tee`, and its report was not retained as an Actions artifact.

### Canonical owner
`tools/audit_github_workflows.py` is the sole static owner of workflow-contract invariants; workflow files remain owners of their domain-specific commands.

### Verification
- Workflow contract audit: PASS
- Source and Architecture: PASS — 158 audit steps
- Runtime Acceptance: PASS
- cTrader Compile: PASS
- OSS Indicator Benchmark: PASS
- Terminal: PENDING — no terminal behavior changed

### Safety / performance
No trading/runtime behavior changed. Read-only permissions reduce CI authority; concurrency cancellation prevents stale duplicate CI work; timeouts bound hung jobs; `pipefail` restores benchmark failure integrity.

### Residual risk
Target-terminal behavior remains outside this repository-only package. `DEF-P0-002` remains OPEN and is the explicit WP-03/WP-04 migration target.

### Historical next package
**WP-03 — Canonical control plane — NEXT.**

> Historical closeout snapshot only. It does not define current execution order.


# 35.4 P3 / WP-03 closeout — 2026-10-04

**Status:** PASS  
**PR:** #283  
**Merge commit:** `e42e57f7bd425c535ecef57c0835d9d22d84313b`

Canonical planning, inventory and acceptance authority was normalized across ROADMAP/LIST/GATE. WORKFLOW continuity was aligned to the canonical bootstrap sequence. README ownership wording was aligned to the current Indicator-analysis / cBot-execution boundary.

**Verification:** Source and Architecture PASS; Runtime Acceptance PASS; cTrader Compile PASS; no runtime/trading/broker logic changed.

**Residual risk:** `DEF-P0-002` remains OPEN and is the explicit WP-04 migration target. No blind deletion of legacy documents was performed.

**Historical package snapshot:** WP-05 — Preflight was NEXT when this historical closeout section was written.

## Permanent acceptance requirement — Local command handoff
If a gate requires user-local execution, the gate record must contain the exact command, environment, execution point, expected success signal, and required returned output. A green GitHub workflow does not close a local verification requirement.

### WP-04 closeout — Historical control-plane isolation — 2026-10-04

**Status:** PASS

**Evidence:** PR #284 merged to `main` as `2e98a4b4cd27dd8b083ee8aac1437cf1a3c6271e`. Exact implementation head `3853f3d9708a7b378e8f61edeefb800a52898ee6` passed Source/Architecture #4365, Runtime Acceptance #4174 and cTrader Compile #4358. DEF-P0-002 is VERIFIED. The active roadmap authority is now isolated to `docs/CFIP-ROADMAP.md`, with historical continuity preserved in the archive boundary.

**Target-terminal requirement:** None for WP-04; this package is repository/control-plane scope.

**Historical next:** WP-05 — Preflight.
