## Current focus — REALTIME LIVE EXECUTION + SIGNAL TRUTH UNIFICATION — 2026-10-03

Status: IMPLEMENTATION COMPLETE — automated verification pending.

Closed in this phase:
- explicit account-scoped Live execution arms for Market, Pending Stop, Pending Limit, Aggressive and Management;
- live account no longer hard-stops the cBot; live mutation remains default-OFF and fail-closed until the corresponding live arm is explicitly enabled;
- bounded multi-scenario capacity now uses the effective minimum of cBot Max Concurrent Scenarios and Indicator Maximum Open Positions, with the Indicator default raised to 3 and maximum 10;
- cBot signal-store reload cadence reduced from 500 ms to 100 ms while preserving per-tick ScenarioBatch processing;
- volatility-relative minimum TP1 opportunity magnitude gate added for current and future opportunities;
- RANGE low-RR structural floor strengthened to 2.00;
- reaction/actionable alert direction now consumes the canonical SignalVisualSnapshot direction;
- cBot execution account mode is published to the Indicator panel as LIVE/DEMO;
- phase record: docs/PHASE-REALTIME-LIVE-SIGNAL-UNIFICATION-2026-10-03.md.

Full-chain audit:
History/context -> MTF -> M15 canonical decision -> M5 trigger/tuning -> M1 optional -> current quote -> current/future scenario -> ScenarioBatch -> cBot preflight -> account-mode gate -> scenario capacity -> broker mutation -> broker confirmation -> protection/management -> outcome/history -> panel/alert/chart truth.

Safety:
- live arms default OFF;
- Demo and Live flags are account-scoped;
- Indicator remains broker-mutation-free;
- M15/M5/M1 role separation unchanged;
- risk percentage was not increased;
- broker geometry/volume/margin/daily-loss/spread/market-hours protections remain active.

Verification:
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- target-terminal live execution, multi-position, same-tick handoff, attachment, audio and UI consistency validation.

Operator action after merge: git pull --ff-only.

---

## Current focus — REALTIME MULTI-SCENARIO OPPORTUNITY ENGINE — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

Purpose:
- continuously refresh current-quote opportunity state without rebuilding structural M5 geometry on every tick;
- support simultaneous current-market execution scenarios and future pending-order scenarios;
- keep M15 as canonical trade-decision/execution reference, M5 as trigger/tuning/entry precision, M1 optional, H1+ context/reward;
- keep historical calibration and forward prediction as separate evidence layers.

Completed:
- intrabar candidate state refresh on a bounded 200 ms cadence;
- live market actionability refresh from the current quote;
- future Continuation Stop and Reversal Limit scenario construction before price reaches the planned level;
- provider batch support for current actionable and future pending scenarios in the same revision;
- isolation of future-order preparation from the canonical live provider intent;
- scenario-scoped execution remains bounded by cBot Max Concurrent Scenarios;
- dedicated realtime static regression audit added to Source/Architecture CI.

Full-chain audit:
Past/history -> pre-analysis -> M15 decision -> M5 tuning/trigger -> M1 optional -> current quote actionability -> current Market/Aggressive OR future Stop/Limit -> ScenarioBatch -> cBot preflight -> broker execution/placement -> broker confirmation -> protection/management -> outcome/history.

Safety:
- no quality/RR/risk threshold was lowered;
- prior demo-only live-account blocking was superseded by the explicit default-OFF Live execution arms in the 2026-10-03 unification phase;
- Indicator remains broker-mutation-free;
- same ScenarioId remains idempotent;
- future pending scenarios are not presented as current market entries.

Verification:
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- dedicated realtime multi-scenario audit;
- target-terminal latency, same-tick handoff, concurrent positions, future pending placement, invalidation and restart/reconnect.

---
## Current focus — CBOT MANAGEMENT POLICY HARDENING — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

Closed:
- cBot now reads the execution-sensitive live-management controls required to authorize protection, partial close and TP progression;
- ManagementExecutionCoordinator applies one canonical CbotManagementPolicyRule before broker mutation;
- partial close is blocked when partial TP is disabled;
- SL/BreakEven protection mutation is blocked when broker protection is disabled, while startup safety recovery remains fail-closed;
- TP advance requires both live-exit management and broker TP sync;
- broker protection/TP mutations are throttled by a position-scoped BrokerModifyCooldownMs;
- cooldown is a transient deferment only: deferred management commands remain queued and retryable, with no synthetic broker acknowledgment;
- full close and pending cancellation remain available for safety lifecycle operations;
- deterministic cBot behavioral coverage and Source/Architecture audit are added.

Full-chain audit:
Pre-analysis -> M15 decision -> M5 trigger/tuning -> M1 optional -> Entry/SL/TP/RR -> Actionability -> Scenario/Plan -> Signal/Alert -> ScenarioBatch -> cBot preflight -> per-ScenarioId broker truth -> management policy -> broker mutation -> confirmation -> protection -> outcome/history.

Safety:
- live accounts were blocked during the historical phase recorded here; current live behavior is governed by the explicit default-OFF Live execution arm;
- cBot remains sole broker mutation owner;
- M15/M5/M1 role separation unchanged;
- no signal-quality, RR, risk, margin, spread, daily-loss or concurrency threshold is lowered;
- emergency full-close/pending-cancel lifecycle commands are not disabled by ordinary TP/protection toggles.

Verification:
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- dedicated management-policy audit;
- target-terminal management/protection/cooldown validation.

Phase record: docs/PHASE-CBOT-MANAGEMENT-POLICY-HARDENING-2026-10-03.md.

Operator action after merge: git pull --ff-only.

---

## Current focus — CBOT BROKER-CONFIRMED EXECUTION FACTS — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

Closed:
- successful market execution now publishes actual broker Entry/SL/TP through the existing BrokerExecutionReport fields;
- a broker-created Position without valid SL/TP is surfaced as RecoveryRequired rather than a clean execution confirmation;
- market/pending submission results immediately trigger broker reconciliation and cBot state republish;
- pending-order requests are not mislabeled as broker-confirmed fill facts;
- pending fill lifecycle events continue to derive active state from the real broker Position.

Full-chain audit:
Pre-analysis -> M15 decision -> M5 trigger/tuning -> M1 optional -> Entry/SL/TP/RR -> Actionability -> Scenario/Plan -> Signal/Alert -> cBot preflight -> per-ScenarioId truth -> broker submission -> broker-confirmed facts -> protection -> management -> outcome/history.

Safety:
- broker mutation remains cBot-owned;
- missing protection remains fail-closed;
- M15/M5/M1 role separation remains intact;
- all existing quality/RR/risk/margin/spread/daily-loss/concurrency gates remain unchanged.

Verification:
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- dedicated broker-confirmed-facts audit;
- target-terminal fill/pending/panel-latency validation.

Phase record: docs/PHASE-CBOT-BROKER-CONFIRMED-FACTS-2026-10-03.md.

Operator action after merge: git pull --ff-only.

---

# Current focus — CBOT EFFECTIVE LIFECYCLE STATE — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

Finding closed:
- reconciliation states use descriptive values such as ACTIVE / RECONCILED and ACTIVE / MULTI-SCENARIO;
- cBot state publication previously required exact READY/ACTIVE/PENDING strings;
- this could make an armed healthy cBot appear effectively OFF in the Indicator panel.

Canonical correction:
- one CbotExecutionLifecycleRule now defines READY/ACTIVE/PENDING state families;
- recoveryRequired always fails closed;
- CbotExecutionStatePublisher consumes that owner;
- behavioral and static regression coverage added;
- previous ScenarioId-scoped reconciliation and restart idempotency contracts remain intact.

Full-chain audit:
Pre-analysis -> M15 decision -> M5 trigger/tuning -> M1 optional -> Entry/SL/TP/RR -> Actionability -> Scenario/Plan -> Signal/Alert -> ScenarioBatch -> cBot preflight -> per-ScenarioId broker truth -> effective lifecycle state -> broker execution -> confirmation -> protection -> outcome/history.

Verification:
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- lifecycle effective-state audit;
- target-terminal panel/armed/recovery validation.

Phase record: docs/PHASE-CBOT-EFFECTIVE-LIFECYCLE-STATE-2026-10-03.md.

Operator action after merge: git pull --ff-only.

---

# Current focus — CBOT SHADOW MULTI-SCENARIO TRUTH — 2026-10-03

Status: IMPLEMENTATION COMPLETE — automated verification PASS; target-terminal acceptance pending.

Finding closed:

- SignalScenarioBatch permits independent ScenarioIds to share a provider revision;

- ShadowHostCoordinator previously used one global last revision/idempotency pair;

- a valid second ScenarioId in the same batch could therefore hit REVISION CONFLICT before broker execution.

Additional execution-chain seam closed:
- ScenarioBatch materialization was explicitly bound to the current closed M5 so stale registry candidates cannot enter the cBot handoff.

Canonical correction:

- last revision/idempotency state is now keyed by ScenarioId;

- same-scenario replay/conflict semantics remain strict;

- cached shadow result/recheck timing is also keyed by ScenarioId;
- cached shadow result/recheck timing is also keyed by ScenarioId;
- global last-accepted values remain telemetry only;
- ScenarioBatch materialization is restricted to the current closed M5 so stale registry candidates cannot enter the execution handoff;

- cBot still processes every scenario from the batch and broker mutation ownership remains unchanged.

Full-chain audit:

Pre-analysis -> M15 decision -> M5 trigger/tuning -> M1 optional -> Entry/SL/TP/RR -> Actionability -> Scenario/Plan -> Signal/Alert -> ScenarioBatch -> cBot preflight -> ShadowHost per-ScenarioId truth -> broker reconciliation -> execution -> confirmation -> protection -> outcome/history.

Verification:

- Source/Architecture: PASS;

- Runtime Acceptance: PASS;

- cTrader Compile/Build: PASS;

- dedicated shadow multi-scenario audit: PASS;

- stale-candidate/current-closed-M5 materialization audit: wired and covered;

- target-terminal two-independent-ScenarioIds/same-revision, duplicate replay and independent protection/reconciliation validation.

Phase record: docs/PHASE-CBOT-SHADOW-MULTISCENARIO-TRUTH-2026-10-03.md.

Operator action after merge: git pull --ff-only.

---
# Current focus — CBOT POSITION TRUTH / RESTART IDEMPOTENCY HARDENING — 2026-10-03

Status: IMPLEMENTATION COMPLETE — automated verification PASS; target-terminal acceptance pending.

This work unit closes the remaining execution-correctness seam after CBOT-6M:

- per-ScenarioId reconciliation is the authority for an individual scenario before broker mutation;
- aggregate instance reconciliation no longer treats independent ScenarioIds as a false global ambiguity;
- same-scenario duplicate position/pending state remains fail-closed recovery;
- aggregate cBot state publishing discovers managed broker objects from exact Indicator-instance scope even before a scenario is selected;
- management commands are not disabled by an unrelated scenario recovery state;
- cBot SignalEnvelope preflight now enforces contract-version, Plan/Intent identity and stable execution identity;
- Market/Pending execution idempotency persists across cBot restart with bounded retry semantics for failed attempts;
- existing CBOT-6M capacity, margin, spread, daily-loss, RR and quality gates remain intact.

Full-chain audit:
Pre-analysis -> M15 decision -> M5 trigger/tuning -> M1 optional -> Entry/SL/TP/RR -> Actionability -> Scenario/Plan -> Signal/Alert -> cBot preflight -> per-ScenarioId broker truth -> broker execution -> confirmation -> protection -> outcome/history.

Safety:
- live accounts remain blocked;
- Indicator remains broker-mutation-free;
- M15 remains canonical execution/trade decision reference;
- M5 remains trigger/tuning/entry precision;
- no public quality/RR/risk threshold is lowered;
- concurrent capacity remains explicitly bounded.

Verification:
- automated Source/Architecture, Runtime Acceptance and cTrader Compile/Build: PASS for PR #227;
- follow-up recovery-truth regression is routed through the same audit path;
- dedicated cBot position-truth audit;
- Source/Architecture accumulated checks;
- Runtime Acceptance;
- cTrader Compile/Build;
- target-terminal multi-scenario, restart/rebind, duplicate-replay and independent-management/protection validation.

Phase record: docs/PHASE-CBOT-POSITION-TRUTH-HARDENING-2026-10-03.md.

Operator action after merge: git pull --ff-only.

---

# Current focus — RETEST TRIGGER-PATH HARDENING — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

Root cause found:
- in-zone RetestMarket actionability was mode-aware, but PlanCreationEligibility and ScenarioExecutionPolicyRule still applied Decision.TriggerReady as a global veto;
- this could remove a valid zone-driven Retest before Plan/Scenario/cBot handoff.

Completed:
- added canonical EntryActionabilityPolicy.RequiresConfirmedTrigger(...);
- plan creation now requires TriggerReady only for trigger-dependent modes;
- scenario authorization consumes the same mode-aware rule;
- TriggerGate delegates to the same canonical owner;
- added deterministic Runtime Acceptance coverage and a dedicated accumulated static audit.

Quality/safety:
- no public entry-quality, confidence, MTF, evidence, RR or risk threshold was lowered;
- M15 remains the canonical execution timeframe;
- M5 remains tuning/entry-precision;
- M1 remains optional confirmation;
- Breakout and predictive pending modes remain trigger-dependent.

Full-chain audit:
Pre-analysis -> M15 decision -> M5 tuning/zone -> M1 optional -> Entry/SL/TP/RR -> Actionability -> Scenario/Plan -> Alert -> cBot -> Broker -> Protection -> Outcome/history.

Verification:
- Retest trigger-path audit;
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile;
- target-terminal Retest/Breakout/Pending behavior.

Phase record: docs/PHASE-RETEST-TRIGGER-PATH-HARDENING-2026-10-03.md.

Operator action after merge: git pull --ff-only.

---
# Current focus — CBOT-6M + TRADE QUALITY HARDENING — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

Scope:
- enable bounded concurrent execution for independent ScenarioIds;
- preserve exact Indicator/cBot identity and per-scenario idempotency;
- reconcile/protect scenarios independently;
- improve opportunity ranking using independent evidence, OB/FVG confluence, WaveTrend, location quality, RR and entry-distance quality;
- keep M15 as canonical decision/execution reference, M5 as trigger/tuning/entry precision, M1 optional confirmation and H1 context/reward.

Completed in this work unit:
- SignalScenarioBatch transport and scenario identity contracts;
- scenario-aware Market / Pending Stop / Pending Limit execution;
- bounded `Max Concurrent Scenarios`;
- scenario-aware broker capacity counting;
- per-ScenarioId envelope/reconciliation caches and protection sweep;
- stable ScenarioId-based broker labels;
- composite trade-quality ranking;
- composite plan quality carried into the scenario PlanSnapshot;
- dedicated CBOT-6M audit and accumulated CI wiring.

Safety:
- Indicator remains broker-mutation-free;
- live accounts remain blocked;
- same ScenarioId is idempotent;
- broker capacity is bounded;
- no public signal threshold was lowered to increase frequency.

Verification:
- Source/Architecture;
- cTrader Compile;
- Runtime Acceptance;
- dedicated CBOT-6M audit;
- target terminal: multiple ScenarioIds, duplicate replay, restart/rebind and independent protection/reconciliation.

Phase record: docs/PHASE-CBOT-6M-TRADE-QUALITY-HARDENING-2026-10-02.md.

Operator action after merge: `git pull --ff-only` on local `main`.

---

# Current focus — POSITION ENGINE / cBot TRUTH HARDENING — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

This work unit is intentionally concentrated on the path from opportunity discovery to actual broker-object handoff.

Completed:
- canonical reward-aware execution-zone selection across M5/M15 FVG/OB, OB+FVG overlap, M5/M15 overlap and M15/H1 structural levels;
- downstream structural-stop quality and best attainable TP1 RR are included before an execution zone is selected;
- zero/invalid reward-path candidates cannot become the selected execution zone;
- structural-stop and forward-target FVG discovery can use valid historical/unretested zones at the discovery boundary;
- cBot publishes an independent symbol-scoped presence heartbeat during STARTING/RUNNING/STOPPED;
- exact fresh Indicator-instance heartbeat remains the only execution-capability truth;
- managed broker position/pending lookup uses stable instance scope as fallback to exact label, so base-label changes do not orphan existing managed objects;
- broker reconciliation immediately re-runs when the active execution label changes;
- panel alert rail is left-aligned with a larger readable font and 20 px rows.

Safety boundary:
- M15 remains the canonical trade-decision/execution reference timeframe;
- M5 remains trigger/tuning/entry precision;
- M1 remains optional confirmation;
- no public quality/RR/risk threshold was lowered;
- the broker remains single-plan until the dedicated CBOT-6M multi-scenario execution/reconciliation phase is verified.

Phase record: docs/PHASE-POSITION-ENGINE-CBOT-TRUTH-HARDENING-2026-10-02.md.

Verification:
- dedicated Position Engine / cBot Truth / Panel Readability source audit;
- Source/Architecture CI;
- cTrader Compile/Build;
- target-terminal attachment/restart and existing-position rediscovery validation.

Operator action after merge: run git pull --ff-only on local main.

---

## OPPORTUNITY-MINING / EXECUTION-ZONE SELECTION — 2026-10-02

Status: IMPLEMENTATION COMPLETE — verification pending.

Goal:
Increase the number of genuinely good setups captured by the Indicator without weakening final execution-quality, RR, risk, regime or broker-safety gates.

Implemented:
- replaced fixed-priority execution-zone selection with canonical scored selection;
- execution-only FVG lookup now compares valid FVG candidates by quality, distance and freshness instead of always selecting the nearest candidate;
- compare M5 FVG, M5 OB, M15 FVG and M15 OB candidates instead of stopping at the first available source;
- score actual zone quality together with distance-to-market and zone age;
- explicitly reward same-timeframe OB+FVG overlap;
- explicitly reward M5/M15 cross-timeframe overlap;
- retain M15 source priority while preserving M5 entry-precision responsibility;
- retain the M5 swing fallback when no qualifying zone candidate exists;
- keep all execution/actionability gates unchanged; better discovery does not automatically authorize a trade.

Full chain re-audited:
Pre-analysis -> M15 decision -> M5 trigger/tuning -> M1 optional confirmation -> zone/entry geometry -> SL/TP/RR -> signal/alert -> cBot contract -> broker execution -> protection -> outcome/history.

Next:
After verification, continue with candidate-family mining and empirical missed-opportunity analysis using the existing SignalEvaluationTrace/Outcome history rather than lowering thresholds blindly.

Operator action after merge: run git pull --ff-only on local main.

## URGENT STABILIZATION — cBot Attachment / Alert Audio — 2026-10-02

Status: IMPLEMENTATION COMPLETE — verification pending.

Scope completed on the hardening branch:
- corrected cTrader chart-object type matching to the actual stable class identities CFIPExecutionBot and CFIPIndicator while retaining display-name compatibility;
- separated physical cBot attachment truth from LocalStorage heartbeat truth so empty heartbeat state no longer masquerades as CBOT NOT ATTACHED;
- hardened the two AUTO TRADE / AUTO ORDERS panel states through the canonical cBot connection presentation path;
- hardened alert sound delivery with custom-file failure fallback to the canonical semantic sound cue;
- added bounded multi-event alert draining per delivery pump;
- added explicit alert queue / sound-delivery runtime diagnostics;
- added a dedicated source regression audit and accumulated it into Source/Architecture CI;
- preserved all strategy, M15/M5 role, RR and broker-capacity contracts; no threshold was lowered just to increase signal frequency.

Full chain audited for this phase:
Pre-analysis -> M15 decision -> M5 trigger/tuning -> M1 optional confirmation -> entry/SL/TP -> signal -> alert/message -> contract -> cBot binding -> cBot preflight -> broker execution/protection -> lifecycle -> outcome/history.

Verification pending:
- Source/Architecture CI;
- cTrader compile/build;
- target-terminal cBot attachment/rename/start/stop/reconnect;
- target-terminal eligible-signal -> panel alert -> sound delivery.

Next work unit after this stabilization: CBOT-6M concurrent multi-scenario execution with per-ScenarioId idempotency, reconciliation, risk and protection ownership.

Operator action after merge: run git pull --ff-only on local main.

## CBOT-P9 — Unified Alert Rail / Visual Coherence / cBot Signal Preflight — 2026-10-02

Status: **VERIFIED COMPLETE — automated gates PASS; target-terminal visual acceptance remains manual.**

Completed:
- production Popup UI and its obsolete parameter surface removed;
- all eligible alerts use the bounded canonical AlertDeliveryQueue and are shown in a five-row panel message rail beside the Hide/Show control;
- message color is derived from the canonical alert direction/priority semantics;
- sound remains on the same queued event boundary and no direct AlertEngine sound/popup side effect exists;
- canonical BUY/SELL level rendering remains symmetric: Solid, finite, 40-bar geometry, shared thickness, background-free labels and semantic line/text color;
- cBot now has one dedicated SignalEnvelope preflight owner for identity, timestamp, staleness and symbol scope before execution branching;
- full Pre-analysis → M15 → M5 → M1(optional) → entry/SL/TP → signal → alert → contract → cBot → broker/protection → outcome chain was re-audited;
- no multi-position capacity was enabled early and the single-plan safety contract remains intact until CBOT-6M is completed.

Phase record: `docs/PHASE-CBOT-P9-UNIFIED-ALERT-RAIL-VISUAL-COHERENCE-2026-10-02.md`.

Verification:
- Source / Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile/Build: PASS;
- P9 dedicated audit: PASS;
- target-terminal panel message placement, line/arrow symmetry and cBot attach/reconnect validation: manual.

Next required work: after verification, continue CBOT-6M concurrent multi-scenario execution with per-ScenarioId idempotency, reconciliation, risk and protection ownership before removing the single-plan broker gate.

Operator action after merge: run `git pull --ff-only` on local `main`.

## CBOT-P8 — Progressive Protection / Broker-Confirmed State Sync — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

Completed:
- one canonical broker-confirmed protection-state adoption owner;
- confirmed management reports and position events now update execution state only after broker confirmation;
- BUY/SELL stop progression remains protective-only;
- broker stop/target regressions fail into explicit recovery instead of rewriting the plan backward;
- active broker target remains separate from the future TP1→TP4 ladder;
- full-chain and M15/M5-role audit is accumulated in Source/Architecture;
- no new broker mutation authority and no new execution capacity were introduced.

Phase record: `docs/PHASE-CBOT-P8-PROGRESSIVE-PROTECTION-STATE-SYNC-2026-10-02.md`.

Verification:
- Source / Architecture: pending;
- Runtime Acceptance: pending;
- cTrader Compile/Build: pending;
- P8 dedicated audit: pending;
- target-terminal trailing/SL/TP confirmation and reconnect validation: manual.

Next required work: complete verification, then implement **CBOT-6M concurrent multi-scenario execution** before removing the single-plan gate.

## CBOT-P7R — Attachment Truth / Alert Visibility / Parallel Scenario Presentation — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

Completed in canonical owners:
- cBot/Indicator same-chart discovery now matches both stable object type name and visible instance name; exact InstanceId + heartbeat freshness remain mandatory;
- analysis alerts no longer depend on same-cycle broker reconciliation, so a valid Indicator signal cannot be silenced by a missing/stale/restarting cBot;
- existing parallel opportunity registry now emits scenario-specific actionable/watch alerts with stable ScenarioId and deterministic `#N` numbering;
- directional WATCH/CONFIRMED arrow follows the current chart bar and remains visible while a valid direction exists, instead of being pinned to the last closed M5 bar or blocked solely by EntryAllowed;
- parallel chart labels now show the same scenario ordinal;
- existing compact signal-line geometry remains solid, finite and 40 candles by default;
- accumulated calculation-cycle audit was updated to treat broker-independent analysis alerting as canonical;
- dedicated P7R source regression audit is accumulated in CI.

Important boundary:
- this phase increases **multi-opportunity detection/presentation**, not broker capacity;
- cBot execution remains safely single-plan until a dedicated multi-scenario execution/reconciliation contract is completed;
- existing intelligent protection/trailing ownership is preserved; no second trailing policy is introduced.

Full chain re-audited:
`Pre-analysis → M15 decision → M5 trigger/tuning → M1 optional confirmation → entry geometry → signal/alert → contract → cBot → broker → lifecycle/protection → chart/panel`.

Canonical timeframe rule:
M15 = trade-decision/execution reference; M5 = trigger/tuning/entry precision; M1 optional confirmation; H1+ context/reward; Chart TF presentation only.

Phase record: `docs/PHASE-CBOT-P7R-ATTACHMENT-ALERT-VISIBILITY-2026-10-02.md`.

Verification:
- Source / Architecture: pending;
- Runtime Acceptance: pending;
- cTrader Compile/Build: pending;
- P7R dedicated audit: pending;
- target-terminal cBot attach/rename/start/stop/reconnect and live alert/chart validation: manual.

Next required work:
1. Complete **CI-20 intelligent progressive protection/trailing verification** with monotonic SL tightening, profit locking and non-regressive target progression.
2. Complete a dedicated **CBOT multi-scenario execution phase** before removing the current single-plan capacity gate; that phase must add per-ScenarioId execution/reconciliation/idempotency/risk/protection ownership for concurrent positions and pending orders.

Operator action after merge: run `git pull --ff-only` on local `main`.

## CBOT-P7 — UI / State Cutover — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

- AUTO TRADE/AUTO ORDERS are now status-only surfaces backed by the cBot effective-state snapshot.
- Effective state combines attached Indicator settings, cBot execution capability and cBot lifecycle/recovery state.
- Indicator uses direct ChartRobots presence/state plus exact-instance heartbeat freshness for connection/liveness.
- cBot attach/remove/modify/start/stop events trigger prompt panel refresh without forcing a LocalStorage read on every redraw.
- Full chain re-audited: pre-analysis → M15 → M5 → entry → signal/alert → contract → cBot → broker → lifecycle/protection → panel.

Phase record: docs/PHASE-CBOT-P7-UI-STATE-CUTOVER-2026-10-02.md.


## CBOT-P6 — Account / Execution Risk + Connection Truth — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

Completed:
- direct same-chart cBot presence/state is now part of Indicator connection truth;
- exact-instance heartbeat remains required for executable cBot capability;
- cBot final execution environment gate checks trading permission, account safety, single-plan capacity, session, live spread/plan-risk and daily loss;
- execution settings are read from the bound Indicator instance instead of duplicated public parameters;
- final broker volume/margin normalization remains in the cBot broker safety owner;
- the full pre-analysis → M15 → M5 → entry → signal/alert → contract → cBot → broker lifecycle was re-audited during the phase.

Canonical rule remains:
M15 = trade decision/execution reference; M5 = trigger/tuning/entry precision; M1 optional confirmation; H1+ context/reward; Chart TF presentation only.

Phase record: docs/PHASE-CBOT-P6-ACCOUNT-RISK-CONNECTION-2026-10-02.md.


## Permanent Development Rule — Canonical Owners / No Patches
## CBOT-P5 — Protection / Lifecycle / Recovery — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending on branch `phase/cbot-p5-reconciliation-protection`.**

Scope:
- add explicit broker-state reconciliation to the cBot startup/reconnect path;
- block new execution while managed broker state is ambiguous or protection is missing/invalid;
- recover missing broker protection only through the existing canonical cBot management mutation owner;
- publish lifecycle/protection/recovery truth to the Indicator panel;
- preserve M15 as the execution/trade-decision timeframe and M5 as trigger/tuning/entry-precision.

No second execution engine, no analytical-rule duplication, and no broker mutation has been reintroduced into the Indicator.

Phase record: `docs/PHASE-CBOT-P5-RECONCILIATION-PROTECTION-2026-10-02.md`.

Verification:
- Source / Architecture: pending;
- Runtime Acceptance: pending;
- cTrader Compile/Build: pending;
- CBOT-P5 reconciliation audit: pending;
- target-terminal restart/reconnect/protection validation remains manual.

Next: continue end-to-end chain verification and then CBOT-P6 account/execution risk authority, without weakening M15/M5 signal-role separation.

Operator action after merge: run `git pull --ff-only` on local `main`.

## CI-21 — Primary M15 Signal Visibility / M5 Entry Tuning — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #211 as `49e71227e830c9de39f35bb4f3bd3b9d0cd2d498`.**

Scope:
- preserve **M15 as the canonical execution/trade-decision timeframe**;
- preserve **M5 as trigger, entry-tuning and microstructure-defence timeframe**;
- validate M15/H1 primary source readiness before downstream entry-plan construction;
- keep a qualified primary setup visible when plan geometry/preview is temporarily unavailable;
- make presentation-only primary state explicitly non-executable;
- prevent the generic parallel-candidate quality margin from hiding a qualified primary source setup.

Root causes confirmed:
- primary source candidates could be discarded by geometry/execution/preview/reward-path construction before presentation;
- generic parallel quality margin could suppress a source-quality M15/H1 setup;
- execution boundary was not explicit for a plan-less presentation state.

No public confidence, RR, risk, spread or strategy threshold was lowered.

Phase record: `docs/PHASE-CI-21-PRIMARY-M15-SIGNAL-VISIBILITY.md`.

Verification:
- Source / Architecture: **PASS** — run 37030614471;
- Runtime Acceptance: **PASS** — run 37030614523;
- cTrader Compile/Build: **PASS** — run 37030614229;
- CI-21 dedicated audit: **PASS**;
- target-terminal M15/M5 validation and empirical signal-quality validation remain manual.

Next: continue the full pre-analysis → M15 decision → M5 trigger/tuning → entry → cBot execution chain audit, with every phase checking that the visual setup state and executable state remain consistent.

Operator action: run `git pull --ff-only` on local `main`.

## Build Warning / Panel Height Integrity — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #198 as `396c72513fc5043bd348e5ceb5c74894da72c395`.**

The locally reported nine Release-build warnings were resolved: Contracts nullability is explicit, dead Indicator state was removed, panel geometry no longer reads `Chart.Height`, and overlay `AutoRescale` is disabled so the invisible provider heartbeat cannot affect price scaling.

Verification on the implementation head:
- cTrader Compile/Build #3064: **PASS**;
- Runtime Acceptance #2880: **PASS**;
- the reported warning files produced no warning/error diagnostics;
- accumulated P4B/P4C/P4D/P4E and architecture/UI/identity audits were preserved.

A broader pre-existing Planning Contracts warning set remains in `TradeOpportunityCandidate.cs` (`CS0649`) and is outside this phase.

Phase record: `docs/PHASE-BUILD-WARNING-PANEL-HEIGHT-INTEGRITY-2026-10-02.md`.

## CI-20B — Protection / cBot / Signal Hardening — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #204.**

Scope:
- consolidate live BE/SL/trailing semantics under one canonical pure protection rule;
- harden cBot management command freshness with terminal expiry;
- preserve cBot-only broker mutation authority;
- allow only qualified M15/H1 primary pullbacks with neutral M5 to continue past M5 confirmation;
- reuse one M5 regime snapshot inside smart decision gates;
- add deterministic protection/signal regression contracts and an accumulated CI20B source audit.

No public confidence, RR or strategy threshold is lowered by this phase.

Phase record: `docs/PHASE-CI-20B-PROTECTION-CBOT-SIGNAL-HARDENING-2026-10-02.md`.

Verification:
- Source / Architecture #3141: **PASS**;
- Runtime Acceptance Contracts #2950: **PASS**;
- cTrader Compile/Build #3134: **PASS**;
- dedicated CI20B protection/cBot/signal audit: **PASS**;
- accumulated audits: **PASS**.

During verification, pre-existing ownership-sensitive audits were updated to the canonical owners introduced by this phase; no production compatibility subsystem was added.

Operator action after merge: `git pull --ff-only` on local `main`.

Next: continue cBot lifecycle/recovery completion and broker-owned protection/target progression migration.

## CI-20 — Panel / cBot / Analysis Engine Coherence — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #203.**

Scope:
- audit every Display/Panel option against a canonical runtime consumer;
- make actual cBot broker state visible to the Indicator without moving broker authority back into the Indicator;
- align panel geometry clamps with public parameter ranges;
- make panel row spacing parameters affect actual controls;
- continue cBot protection/lifecycle groundwork with broker-event state publication;
- reduce repeated MTF analysis work through aligned frame caching;
- strengthen decision quality with bounded independent-evidence-family diversity;
- preserve the no-host-timeframe-coupling execution architecture.

No live-account execution authority or public strategy threshold is introduced by this phase.

Phase record: `docs/PHASE-CI-20-PANEL-CBOT-ENGINE-COHERENCE-2026-10-02.md`.

Verification:
- Source / Architecture #3131: **PASS**;
- Runtime Acceptance Contracts #2940: **PASS**;
- cTrader Compile/Build #3124: **PASS**;
- CI-20 panel option audit: **PASS**;
- CI-20 cBot P5 state-continuity audit: **PASS**;
- CI-20 analysis quality/performance audit: **PASS**;
- accumulated audits: **PASS**.

The Source Gate itself exposed two audit-owner drift issues during closeout: CR4.5 still expected the superseded `AnalyzeFrame(` call shape, and the CI-20 panel option audit assumed direct parameter-name presence inside downstream methods instead of following canonical argument flow. Both were corrected in the audit owners without introducing production compatibility code.

Operator action after merge: run `git pull --ff-only` on local `main`.

Next implementation: **CI-20 protection/trailing and cBot lifecycle/recovery completion**, with monotonic SL tightening, profit-locking and structure/reward-driven target progression without constant TP movement.

## CI-19 — Signal / Target Quality Coherence — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #200 as `623dd198885c684b85be01459b4e72faafa00b8a`; target-terminal and empirical outcome validation remain manual.**

Root causes under active correction:
- global TriggerReady blocking survived in DecisionStructureGates and could suppress Retest before actionability;
- TREND M5 evidence could reject already-qualified TacticalOpportunity paths;
- target candidates were capped before MergeLevels, potentially removing higher-reward structural/HTF/OB/FVG targets before confluence;
- target scoring treated minimum RR as an implicit preference via a centered efficiency bonus and nearest-level bias;
- live progressive target selection used a separate quality-only score from initial target selection.

Canonical CI-19 corrections:
- remove global trigger blocking from DecisionStructureGates;
- preserve high-quality Tactical Retest paths in TREND;
- merge all valid levels before applying SmartTargetMaxCandidates;
- use one TargetCandidateRewardScoreRule for initial and live target selection;
- treat minimum RR as a validity floor and reward additional valid RR only when source quality supports it;
- add panel BARRIER TRACE to show the actual blocker/actionability reason;
- preserve all final confidence, SmartQuality, RR, regime, divergence, trap-risk, indicator-fusion and market safety gates.

Phase record: `docs/PHASE-CI-19-SIGNAL-TARGET-QUALITY-COHERENCE-2026-10-02.md`.

Next: **CI-20 — Intelligent progressive protection and trailing**, with monotonic SL tightening, profit-locking and progressive target expansion driven by structure and reward potential without constant TP movement.

## CI-18 — Signal / Panel Coherence — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #199 as `8ba701ea288641bc1435ac8f94ea6890713ed63c`.**

Deep audit found two concrete coherence failures:
- directional Decision state could disappear from the chart/panel and become WAITING because visual direction was coupled to EntryAllowed/TriggerReady;
- continuation context was evaluated before an actual in-zone Retest, so a valid trend pullback could become WAITING FOR TRIGGER and never reach actionability.

Completed in canonical owners:
- directional Decision remains visible before trade actionability;
- MARKET BIAS is presented separately from SIGNAL readiness;
- M15/H1 panel direction uses the shared frame-display direction;
- trigger score, required score, M1 direction and trigger reason are visible in the panel;
- M5OnlyConfirmedTrigger is enforced at mode-specific actionability rather than globally blocking Decision EntryAllowed;
- in-zone Retest takes precedence over generic continuation waiting;
- ExecutionModel readiness is required before actionability can pass;
- strong M5 trigger override remains owned by the canonical TriggerGate;