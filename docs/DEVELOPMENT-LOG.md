## 2026-10-02 — cBot Attachment / Alert Audio Hardening

- Root-caused the false CBOT NOT ATTACHED state: chart discovery compared Type.Name with the display label instead of the actual stable class type.
- Added one shared stable type identity for the cBot (CFIPExecutionBot) and one for the Indicator (CFIPIndicator).
- Kept exact InstanceId and fresh heartbeat as execution-liveness requirements; chart attachment and heartbeat are no longer conflated.
- Changed empty LocalStorage heartbeat from CBOT NOT ATTACHED to CBOT HEARTBEAT PENDING.
- Hardened alert audio delivery so an invalid custom sound path falls back to the semantic cTrader sound cue.
- Added explicit CFIP ALERT QUEUED, CFIP ALERT QUEUE REJECTED and CFIP ALERT SOUND DELIVERED diagnostics.
- Bounded each alert pump to four deliveries to avoid UI/audio bursts while preventing queued alerts from waiting unnecessarily.
- Added startup logging for the effective alert-audio configuration.
- Added a dedicated regression audit and accumulated it into .github/workflows/source-check.yml.
- Full pre-analysis -> M15 -> M5 -> M1(optional) -> entry/SL/TP -> signal -> alert -> contract -> cBot -> broker/protection -> outcome chain re-audited.
- No strategy threshold, RR rule, risk setting or broker execution capacity was weakened.
- Commercial-readiness note: this hardening improves reliability/observability but is not evidence of profitability; replay/OOS/forward-demo measurement remains mandatory.

## CBOT-P9 — Unified Alert Rail / Visual Coherence / cBot Signal Preflight — 2026-10-02

Status: **VERIFIED COMPLETE — automated gates PASS; target-terminal visual acceptance remains manual.**

Root causes addressed:
- the Popup surface had become a separate presentation path while the user-facing requirement was to keep messages in the panel;
- popup-specific parameters and lifecycle hooks were creating dead configuration/ownership surface;
- alert presentation and optional sound needed one canonical queued event boundary;
- execution-side signal validity checks were embedded in the cBot host instead of one explicit preflight owner.

Implemented:
- removed PopupRenderer, PopupRemover, PopupExpirationCleaner and the Popup-scoped AlertDeliveryProcessor;
- removed 20 obsolete popup configuration parameters;
- added PanelAlertMessageRenderer with bounded five-message history and semantic colors;
- added UI/Panel/AlertDeliveryProcessor.cs as the single alert delivery boundary;
- made panel presentation invalidate on a new alert revision instead of relying on a continuously forced full redraw;
- added CbotSignalPreflight and routed CFIPExecutionBot SignalEnvelope validation through it;
- added P9 source/static gate and accumulated full-project audit requirements.

The cBot execution architecture remains:
Indicator analysis → canonical SignalEnvelope → Device LocalStorage / exact InstanceId → cBot signal preflight → execution environment gate → broker coordinator → broker confirmation → lifecycle/protection.

Verification status:
- implementation commits are on phase/cbot-p9-unified-alert-rail-visual-coherence-2026-10-02;
- Source/Architecture CI: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader compile/build: PASS;
- dedicated P9 audit: PASS;
- terminal visual acceptance remains manual.

Next phase: CBOT-6M concurrent multi-scenario execution.

Operator action after verified merge: git pull --ff-only.

## CBOT-P8 — Progressive Protection / Broker-Confirmed State Sync — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

- Added one canonical `BrokerProtectionStateSynchronizer` for broker-confirmed Entry/SL/active-TP adoption.
- Confirmed management reports, broker refresh and position-modified events now share the same state-adoption owner.
- Accepted/unconfirmed management commands cannot manufacture new protected plan state.
- BUY/SELL stop progression remains protective-only; broker-confirmed stop regression enters explicit recovery.
- Active broker target must remain forward from entry and cannot regress from the previously confirmed target.
- Existing `IntelligentProtectionRule` remains the sole live trailing/protection policy; no duplicate trailing engine was introduced.
- Full-chain audit remains mandatory and preserves M15 as the execution/trade-decision reference, M5 as trigger/tuning/entry precision, M1 optional confirmation, H1+ context/reward and chart timeframe as presentation only.
- Performance path remains bounded by broker dirty-state / refresh cadence; repeated unconfirmed management writes are not introduced.

Phase record: `docs/PHASE-CBOT-P8-PROGRESSIVE-PROTECTION-STATE-SYNC-2026-10-02.md`.

Next: complete P8 verification, then execute CBOT-6M multi-scenario capacity/reconciliation before removing the single-plan gate.

## CBOT-P7R — Attachment Truth / Alert Visibility / Parallel Scenario Presentation — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

Root causes confirmed:
- cBot discovery was too dependent on mutable chart instance naming;
- analysis alerting was coupled to broker reconciliation even though alert emission itself is not broker mutation;
- the parallel opportunity architecture already supported multiple scenarios, but user-visible ordinal identity was missing;
- directional WATCH guidance was tied to the closed M5 anchor and, for early display, to EntryAllowed.

Canonical corrections:
- match cBot/Indicator chart objects by both instance name and stable type name;
- keep exact InstanceId + heartbeat freshness as execution liveness proof;
- emit scenario alerts from the canonical parallel registry using ScenarioId and deterministic #N numbering;
- keep alert delivery on the existing unified AlertDeliveryQueue;
- anchor directional guidance to the current chart bar so the arrow continues to track the active market state;
- keep arrow hidden when there is no valid direction;
- keep existing 40-candle solid level-line geometry;
- update the calculation-cycle audit owner to reflect the new analysis-alert boundary;
- add a dedicated accumulated P7R regression audit.

Important:
- this phase does not lower confidence/RR/risk filters;
- it does not create a second execution owner;
- it does not falsely claim multi-position/multi-pending execution, which remains blocked by the existing single-plan safety contract;
- intelligent trailing/progressive protection remains owned by the existing canonical protection policy and cBot mutation boundary.

Full-chain routine audit:
`Pre-analysis → M15 → M5 → M1 optional → entry geometry → signal/alert → contract → cBot → broker → lifecycle/protection → chart/panel`.

### Visual consistency follow-up — 2026-10-02

- BUY and SELL level presentation uses the same canonical line geometry, thickness mapping, font size, weight, and label placement; direction does not alter the visual layout.
- Entry / Trigger / SL / TP / Pending labels share the same compact renderer.
- Compact level text is now exactly the same semantic color as its corresponding Solid line; labels remain background-free.
- All compact plan lines remain finite and Solid with the existing default 40-bar span.

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

P6 root-cause note: LocalStorage heartbeat alone was insufficient to prove physical cBot attachment; the supported chart-robot API is now the first connection signal, with heartbeat freshness as the execution liveness proof.

## CBOT-P5 — Protection / Lifecycle / Recovery — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending on branch `phase/cbot-p5-reconciliation-protection`.**

Deep audit finding:
- cBot already published broker lifecycle events, but startup/reconnect did not have a dedicated reconciliation gate that classified ambiguous managed broker state or protection defects before new execution.

Canonical corrections:
- one cBot reconciliation owner now classifies managed broker position/pending state;
- unresolved duplicate/mixed/invalid protection state blocks new execution;
- existing ManagementExecutionCoordinator is the only broker mutation owner used for missing-protection recovery;
- cBot state publication now carries lifecycle/protection/recovery information;
- Indicator panel consumes that state as observation only.

The full signal chain remains:
`Pre-analysis → M15 decision → M5 trigger/tuning → entry plan → contract → cBot safety → broker → broker-confirmed lifecycle`.

No duplicate execution engine and no public strategy/RR/risk threshold change was introduced.

## CI-21 — Primary M15 Signal Visibility / M5 Entry Tuning — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #211 as `49e71227e830c9de39f35bb4f3bd3b9d0cd2d498`.**

Deep audit finding:
- a qualified M15/H1 source could disappear before presentation because the primary candidate path was coupled to downstream geometry/plan/target construction;
- the generic parallel quality margin could also suppress valid primary source visibility.

Canonical correction:
- source validation first;
- plan construction second;
- M5 remains the entry trigger/tuning layer rather than the existence test for the M15 setup;
- plan-less primary state is presentation-only and fail-closed at execution;
- primary display uses its dedicated source-quality floor without the extra generic parallel margin.

Verification:
- Source/Architecture: **PASS**;
- Runtime Acceptance: **PASS**;
- cTrader Compile: **PASS**;
- CI-21 audit: **PASS**.

No public strategy/RR/risk threshold was lowered.

Phase record: `docs/PHASE-CI-21-PRIMARY-M15-SIGNAL-VISIBILITY.md`.

## CI-20B — Protection / cBot / Signal Hardening — 2026-10-02

Status: **VERIFIED COMPLETE — merged via PR #204.**

Canonical changes:
- one pure intelligent protection owner now combines smart BE, structural trailing, pressure/momentum tightening and monotonic progression;
- ProtectionManager is an input collector/reconciler, not a second protection-policy owner;
- cBot management command freshness is bounded and stale commands become terminal Expired reports;
- neutral-M5 primary pullback handling is explicitly restricted to aligned, quality-qualified M15/H1 contexts;
- smart decision gates reuse the same active M5 regime snapshot within one cycle;
- regression contracts cover BUY/SELL protection symmetry, server BE ownership, monotonic SL, invalid numeric fail-closed, and primary pullback cases.

Verification:
- Source / Architecture #3141: PASS;
- Runtime Acceptance #2950: PASS;
- cTrader Compile #3134: PASS;
- CI20B dedicated audit: PASS.

No public confidence/RR/strategy threshold was lowered.

## CI-20B — Protection / cBot / Signal Hardening — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending on branch `phase/ci20b-protection-cbot-signal-hardening-2026-10-02`.**

Canonical corrections:
- live BE/SL/trailing calculation is consolidated in `IntelligentProtectionRule`;
- `ProtectionManager` no longer owns a second copy of BE/trailing arithmetic;
- cBot management commands now expire after a bounded age and publish a terminal Expired report;
- Indicator retires Expired management commands from its local queue;
- only qualified M15/H1 aligned pullbacks can pass neutral-M5 confirmation; opposite M5 remains blocked;
- smart-gate M5 regime lookup is reused within the same cycle.

Deterministic regression coverage:
- intelligent protection BUY/SELL symmetry;
- server-side BE competition prevention;
- monotonic stop protection;
- fail-closed non-finite protection;
- qualified neutral-M5 primary pullback acceptance and rejection cases.

No public confidence/RR/strategy threshold was lowered.

Phase record: `docs/PHASE-CI-20B-PROTECTION-CBOT-SIGNAL-HARDENING-2026-10-02.md`.
## CI-19 — Signal / Target Quality Coherence — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending on branch.**

Deep audit findings:
- DecisionStructureGates still globally blocked on TriggerReady after the CI-18 mode-specific trigger refactor;
- a qualified TacticalOpportunity could be rejected by TREND M5 evidence before canonical actionability;
- TargetLevelBuilder truncated candidates before confluence merging;
- TargetCandidateEvaluator favored the required/minimum RR instead of treating it as a floor;
- LiveTargetCandidateEvaluator had a separate target score and could not reason about reward expansion relative to the previous confirmed target.

Corrections are implemented in the canonical owners and covered by deterministic planning tests plus the CI-19 audit. No public signal-quality or safety threshold was lowered merely to increase signal count.

Next focus is intelligent progressive protection/trailing using the same canonical reward and structure semantics.

## Build Warning / Panel Height Integrity — 2026-10-02

Status: IMPLEMENTATION COMPLETE — verification pending.

Corrections:
- initialized BrokerExecutionReport.CommandIdempotencyKey and ManagementCommand.ExecutionLabel explicitly;
- made management JSON deserializers model nullable parsed arrays explicitly while preserving failure semantics;
- removed dead _lastPendingSignalM5 state;
- removed the panel geometry baseline that depended on Chart.Height;
- made the panel maximum-height resolver configuration-owned;
- set the overlay Indicator to AutoRescale = false;
- retained the transparent Provider Heartbeat solely as a read-only provider signal while preventing it from affecting chart scale;
- added tools/audit_phase_build_warning_panel_height.py and wired it into Source/Architecture checks.

Root cause confirmed:
the Provider Heartbeat output writes a provider revision rather than a price value, while cTrader overlay Indicators default to automatic chart rescaling. The panel renderer also retained a Chart.Height feedback input. Both paths are now removed from chart geometry/scaling authority.

Manual cTrader check remains required for chart plot height, panel attachment/reload, hide/show and provider-heartbeat scale behavior.
## CBOT-P4D — Pending Limit Authority + Signal/Popup Continuity — 2026-10-02

Status: **VERIFIED COMPLETE — merged to main via PR #196 as 352e6229adcff8a4ebb6ee6e5c71a0e0397dc70b.**

Completed:
- Pending Limit broker mutation moved completely to the existing cBot Pending Stop/Limit coordinator;
- obsolete Indicator BrokerLimitOrderPlacement owner removed;
- Indicator Pending Limit path is intent-only and preserves the absolute lifecycle snapshot;
- cBot Pending Limit arm added with fail-closed identity, margin, capacity, geometry and idempotency protections;
- M15 remains the internal execution clock; host Chart TF remains presentation-only;
- signal direction is decoupled from execution actionability for visibility; markers are arrows only;
- M1 trigger is UpArrow/DownArrow and arrow intensity is score-derived;
- BUY/SELL each have Strong, Confirmed and Caution visual colors;
- popup defaults to BottomRight, persistent until next alert/manual close, with important-alert filtering through the canonical alert engine;
- stale audits were reconciled to the single surviving owners;
- no duplicate execution engine, identity formatter or compatibility wrapper was retained.

Verification on implementation head 22d7af189d7037237b27a3df09a42a9cdda7f252: Source/Architecture PASS; Runtime Acceptance PASS; cTrader Compile PASS; P4D audit PASS; dependent execution/UI/identity audits PASS. Target-terminal acceptance remains manual.

## CBOT-P4C — Pending Stop Authority + Host-Timeframe Independence — 2026-10-02

Status: **VERIFIED COMPLETE — merged to main via PR #195 as 7b8648091bde26753b1e0fcff3d75984a1f1b9eb.**

Completed:
- deleted the migrated Indicator Pending Stop broker owner;
- created a single cBot Pending Stop mutation coordinator;
- centralized broker capacity and margin volume capping in BrokerExecutionSafety;
- made Pending Stop trigger calculation spread-aware on the executable side;
- removed Pending Stop broker submission, permission and submission-gate ownership from Indicator;
- made provider and memory/archive identity use internal M15 rather than host Chart TF;
- updated active audit scripts and operational documentation;
- added Pending Entry spread, Pending Stop shadow, and M15 host-independence checks;
- preserved the absolute Pending Stop lifecycle snapshot across the Indicator to cBot boundary;
- moved broker label ownership to the existing canonical instance-scoped identity carried by ExecutionIntent; no second label formatter was added.

No new strategy engine or duplicate execution engine was added.

Verification: Source/Architecture #3025 PASS; Runtime #2834 PASS; cTrader Compile #3018 PASS; P4C audit PASS; CR5.4, CI-15, M15/Risk/Spread and CR1.8/A11 audits PASS. Target-terminal acceptance remains manual.

## MTF-EXECUTION-M15 + Smart Margin/Spread Risk — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` in implementation commit `0a45bb251d28a5542fa7580d886d3af8b25184b7`.**

Completed:
- M15 is the canonical execution timeframe;
- cBot launch default is M15, but runtime host timeframe is ignored for execution;
- M15 agreement is required by canonical actionability;
- M5/M1 remain defensive tuning layers;
- H1+ remains higher-timeframe context/reward-path support;
- canonical effective reward-risk math now accounts for spread on the reward side as well as the risk side;
- synthetic TP generation includes spread cost when spread-aware sizing is enabled;
- cBot applies a final broker margin budget cap to requested volume;
- deterministic timeframe, spread and margin tests/audits were added.

No independent decision engine, second execution owner, or numeric signal-confidence tuning was added.

Verification: Source/Architecture #2997 PASS; Runtime #2806 PASS; cTrader Compile #2990 PASS; M15/Risk/Spread audit PASS. Target-terminal acceptance remains manual.


## CBOT-P4B + Panel Geometry Integrity — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` as `33dd37f225fb9e2677070f122b5068b0c2df2e92`.**

Panel:
- final outer panel height is now propagated to the internal panel StackPanel;
- panel sizing no longer depends solely on the transient live Chart.Height;
- a finite pre-control viewport baseline is captured before Chart.AddControl;
- long content remains bounded by the ScrollViewer.

cBot:
- ExecutionAction.Aggressive is accepted by the single demo mutation coordinator;
- an explicit Enable Demo Aggressive Execution switch was added;
- Aggressive broker reports use SubmitAggressive;
- Indicator Aggressive broker mutation owners were removed in the migrated path.

No analytical threshold, confidence, RR, Entry, SL, TP or risk tuning was performed.

Verification: Source/Architecture `36983568671` PASS; Runtime `36983568612` PASS; cTrader Compile/Build `36983568641` PASS; P4B audit PASS. Manual target-terminal acceptance remains required for real cTrader panel geometry/responsiveness and demo execution.

## CBOT-P4A — Market / Market Range Authority Cutover — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` on 2026-10-02 as `dee53a3dfa1cbfab7f4b7ec4826298739559d19c`.**

Final implementation HEAD: `e51d1bb82db2fec25b91917873ab19ccacbd9859`.

Verification:
- Source / Architecture run `36981439556`: **PASS**;
- Runtime Acceptance run `36981439561`: **PASS**;
- cTrader Compile/Build run `36981439570`: **PASS**.

Completed:
- Market / Market-Range broker mutation is owned by `src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs`;
- Indicator Market broker owner and legacy Market/Aggressive execution stages are physically removed from the active Indicator path;
- Indicator plan materialization is independent of Auto Trading, with an explicit analysis-only same-bar retry owner;
- panel broker-action UI is removed; finite bootstrap/live-height geometry and MTF frame-aware refresh are restored;
- M15/H1 primary source direction is displayed from exact resolved frame state with separate observe-only source markers;
- stale CI/audit ownership contracts were reconciled to the current architecture without changing public parameter count or trading thresholds.

No numeric signal threshold, RR, Entry, SL or TP policy was tuned in this phase.

Target-terminal validation remains manual and is not represented as repository-pass evidence.

Next: **CBOT-P4B / next staged execution-migration phase**, followed by the remaining Aggressive, Pending, Close, protection, lifecycle, account-risk and recovery migrations.

## CBOT-P3 — cBot Host / Shadow — 2026-10-02

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification: Source / Architecture #2801 PASS; Runtime Acceptance #2610 PASS; cTrader Compile #2794 PASS.

Implemented a deterministic read-only shadow host around the canonical Indicator provider. The cBot now validates contract identity/version/scope, provider/envelope revision agreement, expiry, intent lineage, basic BUY/SELL geometry integrity, duplicate/conflicting revisions, single-plan capacity and live quote validity; trading permission remains a P4 broker-submission concern because the Robot host does not expose the documented Plugin-style Permissions object. A bounded 128-entry idempotency cache suppresses repeated intents without introducing hot-path persistence.

Added deterministic behavioral fixtures under `tools/CFIP.cBot.Shadow.Tests` and wired them into the cTrader build workflow. Added `tools/audit_cbot_shadow_host.py` to Source/Architecture CI.

No broker mutation was added or moved. The cBot remains shadow-only and execution-disarmed.

Next: **CBOT-P4 — Broker Mutation Extraction**.


## CBOT-P2 — Read-Only Indicator Provider — 2026-10-02

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification: Source / Architecture #2790 PASS; Runtime Acceptance #2599 PASS; cTrader Compile #2783 PASS.

Added the canonical read-only Indicator provider backed by `CFIP.Contracts`, including immutable `SignalEnvelope`, semantic revisioning, deterministic identity/idempotency, and an invisible `ProviderHeartbeat` output to force lazy evaluation from cBot. The cBot now references `CFIPIndicator` through `Indicators.GetIndicator<CFIPIndicator>()`, explicitly disables all Indicator-side execution/protection switches for the P2 instance, and logs the provider snapshot without broker mutation.

Added `tools/audit_cbot_provider_boundary.py` and wired it into Source/Architecture CI. No broker execution owner was removed in P2; this phase establishes the handoff required for P3 shadow and later P4–P8 migration.

Phase report: `docs/PHASE-CBOT-P2-READ-ONLY-INDICATOR-PROVIDER.md`.

Next: **CBOT-P3 — cBot Host / Shadow**.

## CBOT-P1 — Platform-Neutral Contracts — 2026-10-02

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification: Source / Architecture #2778 PASS; Runtime Acceptance #2587 PASS; cTrader Compile #2771 PASS.

Created the canonical platform-neutral Indicator ↔ cBot contract layer under `src/CFIP.Contracts` with immutable Signal/Plan/Execution/Management/Broker/Lifecycle records and identity/revision/idempotency fields.

Added `tools/audit_cbot_contract_schema.py` and wired it into Source/Architecture. Contracts contain no cTrader dependency, no setters and no behavioral methods. Existing Indicator-side `Plan`/`ExecutionIntent` remain temporary source models; no duplicate cBot model was created.

Added detailed phase report: `docs/PHASE-CBOT-P1-PLATFORM-NEUTRAL-CONTRACTS.md`.

Next: **CBOT-P2 — Read-Only Indicator Provider**.

## CBOT-P0 — Activation / Boundary Lock — 2026-10-02

Status: **VERIFIED COMPLETE — 2026-10-02.**

Activated the cBot separation as a mandatory parallel track beginning immediately after M1.

Implemented:
- master roadmap now carries CBOT-P0→P8 as the active parallel execution-separation track;
- detailed cBot roadmap explicitly states that M29–M38 are historical/reference sequencing, not the start gate;
- created src/CFIP.Contracts and src/CFIP.cBot;
- registered both projects in CFIP.Indicator.sln;
- added a fail-closed cBot host with no broker mutation in P0;
- permanent CI now builds Contracts and cBot;
- created docs/CBOT-P0-EXECUTION-DEPENDENCY-CLOSURE.md with exact mutation owners and callers;
- froze the Indicator against adding new broker mutation authority.

No production trading behavior was moved or deleted in P0. The existing Indicator executor remains only as a temporary compatibility owner until the corresponding cBot replacement passes parity and deletion gates.

Verification: Source/Architecture #2771 PASS; Runtime Acceptance #2580 PASS; cTrader Compile #2764 PASS, including independent Contracts/cBot builds.

Next: CBOT-P1 Contracts. Execution migration then proceeds in controlled owner batches while M2+ continues in parallel.

## M1 — Full Forensic Audit — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — repository verification pending.**

Report: `docs/PHASE-M1-FULL-FORENSIC-AUDIT.md`

Baseline:
- 633 production Indicator C# files;
- 568 public parameters;
- 15 direct broker-mutation call-sites inside the frozen owner set.

M1 completed a repository-wide forensic review across BUY/SELL symmetry, closed/live boundaries, numeric safety, constants/clamps, collection bounds, hot-path I/O, time/session, persistence, idempotency, identity, authority boundaries, visual/alert ownership, outcome accounting and large-method ownership.

Key findings were explicitly dispositioned. The concrete tooling drift found was `tools/audit_cbot_boundary.py` expecting 564 instead of the current authoritative 568 parameter baseline; this was corrected on the M1 branch. No production trading behavior changed.

Design-risk/manual findings remain intentionally routed to M2, M4–M7, M8–M16 and M29–M39 rather than silently expanded into M1.

Next phase after gate verification: **M2 — Repository Hygiene / Dead Code / Ownership**.

## M0 — Adoption / Freeze / Baseline closeout — 2026-10-02

Status: **VERIFIED COMPLETE — Master Roadmap consolidation and repository baseline freeze completed.**

Branch: `phase/master-roadmap-single-source-2026-10-02`  
Verified implementation/continuity HEAD before closeout: `6ceab05142fab7f2ac2bf9bbfd6f6346bd1023bc6`.

Verification:
- Source / Architecture #2756: **PASS**
- Runtime Acceptance Contracts #2565: **PASS**
- cTrader Compile #2749: **PASS**

Completed:
- established `docs/ROADMAP.md` as the single active development roadmap;
- reconciled historical roadmap/audit continuity requirements after consolidation;
- verified the 568-parameter baseline and repository structure;
- preserved the frozen broker-mutation boundary and current single execution authority;
- recorded the target Indicator → Contracts → cBot architecture without creating the cBot in M0;
- made no production C# trading-behavior change.

Findings resolved during M0:
- missing Track 19 continuity link;
- missing Phase 7.4 continuity marker and misleading `Mxx` phase-closeout template;
- missing CR4.4 / D10 / CR-FINAL continuity anchors;
- missing CR7.5 / G5 and CR7.6a continuity anchors;
- accumulated CI/CR historical string anchors required by repository audits.

Manual boundary remains open:
- target cTrader terminal/broker behavior;
- panel/chart live responsiveness;
- broker fill/reconnect/lifecycle evidence.

Operator action after merge: `git pull --ff-only`.

Next phase: **M1 — Full Forensic Audit**.

## CI-17A closeout — Panel live-content refresh correction — 2026-10-02

Status: **VERIFIED COMPLETE — repository implementation gate passed; target-terminal visual acceptance remains pending.**

PR #175 merged to `main` as `6ffff643ad5c24782ca7035355e31ee4a04465c2`.

Verified implementation HEAD: `9641bfc02c7604d6432202459fa198866d4a5f53`.

Verification:
- Source/Architecture #2742: PASS;
- Runtime Acceptance Contracts #2551: PASS;
- cTrader Compile #2735: PASS;
- accumulated Source/Architecture historical audits through CI-17A: PASS.

Root cause was an incomplete refresh split: the runtime heartbeat updated only volatile rows while full panel rendering could short-circuit on an unchanged/incomplete presentation key. The fix adds a bounded 500 ms content-only row refresh, reuses one visual snapshot, preserves full-layout optimization, forces immediate refresh after restore/reset, and uses current Bid/Ask locally for live RR display.

The live quote is presentation-only and does not mutate trading state. No public parameters, strategy thresholds, decision rules, execution policy or broker mutation authority changed.

Target-terminal visual responsiveness remains a manual CI-17 acceptance boundary.

# CFIP Indicator — Development and Continuity Log

This file records implementation history so development can resume safely in a new chat without reconstructing prior work from conversation history.

## 2026-10-02 — CI-07 MTF / Regime / Market Context

Status: **VERIFIED COMPLETE — merged to `main` via PR #162 (merge commit `73511c84ff3072cdbdab8487b0d4331b52c789b1`). Runtime Acceptance #2333, cTrader compile #2517, and Source/Architecture #2524 passed on final CI-07 head.**

Implemented:
- reference-aware `MtfClosedContextCache` with next-bar boundary checks and exact-reference re-materialization;
- one platform-neutral `MarketStateSnapshot` for all eight MTF frame states, premium/discount and session context;
- decision-boundary validation of market-state reference and all eight closed indices;
- explicit regime-transition ownership and propagation;
- canonical snapshot consumption by decision and suitability paths;
- deterministic CI-07 Runtime Acceptance and Source/Architecture static audit wiring.

No public parameter/default or trading threshold was changed. No new decision, execution or broker-mutation authority was introduced.

Phase record: `docs/PHASE-CI-07-MTF-REGIME-CONTEXT.md`.

## Continuity rules

- `docs/ROADMAP.md` is authoritative for the next phase.
- `docs/ARCHITECTURE.md` is authoritative for ownership and dependency boundaries.
- This file records what was actually implemented and verified.
- A phase is not marked complete until its required verification gates pass.
- The operator normally pulls once at the completed phase boundary, after the final verified commit.
- Intermediate commits are implementation history; they do not require a local pull unless needed for review.

## Phase 0.1 — Repository truth synchronization

Status: complete.

Work recorded:
- read the repository roadmap, architecture and workflow;
- reconciled repository facts against the machine verifier;
- identified that the verifier enforced 535 parameters while older documentation stated 513;
- identified the source-check continuity failure caused by the missing Track 19 benchmark link;
- synchronized README, ROADMAP, ACCEPTANCE-MATRIX and WORKFLOW;
- corrected the Track 19 link so the verifier checks the actual repository path.

Important commits:
- `5d4b6a00fa61dda4c927800b8bd27f6dd496f3cd`
- `c589d427b1cbba2fc3541f50e56bf0cb3d6c7020`

Verification:
- source and architecture checks: PASS;
- runtime acceptance contracts: PASS;
- cTrader compile: PASS.

Result:
- baseline documented as 398 production C# files and 535 parameters;
- next phase became Phase 0.2.

## Phase 0.2 — Production-source hygiene

Status: complete.

Work recorded:
- added verifier enforcement for version/historical residue, obsolete/compatibility identifiers, compatibility aliases, empty catches, generated artifacts and mixed line endings;
- changed repository line-ending policy to LF to match production source;
- found and fixed six empty catch blocks across four production files;
- replaced silent cleanup/render catches with diagnostic logging while keeping fault containment;
- changed the hygiene verifier to aggregate all hygiene findings in one run;
- fixed a verifier regression where hygiene regex definitions were accidentally removed;
- fixed the calculation-era verifier contract so it no longer required obsolete direct `Calculate()` calls;
- corrected the documented finding count from three to six.

Important commits:
- `485e9de7cefda635d8680ba41c313aab39a49dac`
- `73cf272ffe1c9c0bb0c554b0427736f519b24275`
- `4fc0289491399e1d25439710a0ab30713352cd88`
- `cfc9de53e9fd7d0c1a8d9fd7d7e3919481cac4d7`
- `8dec99214ba2d986f90361855fde4c66171d6b0b`
- `f866ae3087c8e674ee46ec38e7c03aac2834d9b3`
- `10dca580b641fb5e58af3821966615abbfbbbdf3`
- `091e9defe0ce6becda10f342a5ae71002b53b346`

Verification at final phase state:
- source and architecture checks: PASS;
- runtime acceptance contracts: PASS;
- cTrader compile: PASS.

Result:
- next phase became Phase 1.1.

## Phase 1.1 — Calculate stage isolation

Status: complete.

Goal:
- prevent one recoverable calculation-stage failure from suppressing unrelated live stages;
- preserve existing strategy order and business semantics.

Implementation:
- introduced `Runtime/Calculation/CalculationStageIsolation.cs`;
- converted `Calculate()` into a thin orchestration boundary;
- separated preparation, closed-bar analysis and live-cycle stages;
- added independent recoverable fault boundaries around live analysis, plan synchronization, recovery, reminder/control synchronization, planning, broker-state synchronization, active management, execution, broker protection, telemetry, reversal management and presentation;
- recoverable closed-bar analysis faults fail closed for automatic order creation via the existing runtime state authority but allow live management/protection/reconciliation to continue;
- recoverable optional live-analysis faults no longer suppress downstream management stages;
- fatal memory/stack exceptions remain fatal;
- retained exact existing operation order inside the stage orchestration;
- added runtime contract coverage and architecture verifier checks for stage isolation;
- updated verifier expectations to recognize the new authoritative calculation-stage owner.

Important commits:
- `1b10248871983daf01ab51244f438bd0dcacbc2e`
- `dce27a4f8d85a438b7419ed3d5ab5c9a838e2251`
- `515058ed5183f2f79841e952958e3ad3c1adff7f`

Verification on the final implementation commit:
- source and architecture checks: PASS (workflow run 741);
- runtime acceptance contracts: PASS (workflow run 550);
- cTrader compile: PASS (workflow run 734).

Result:
- Phase 1.1 complete;
- next phase: **Phase 1.2 — Management-first runtime**;
- operator pull: **required at this phase boundary**, after the final documentation commit for Phase 1.1.

## Phase 1.2 — Management-first runtime

Status: complete.

Goal:
- move safety-critical broker reconciliation, recovery, active-plan management and broker protection ahead of optional live intelligence;
- preserve one broker mutation authority and broker-confirmed state authority.

Implementation:
- reordered the live calculation stages in `Runtime/Calculation/CalculationStageIsolation.cs`;
- added explicit pre-analysis broker reconciliation, recovery, active-plan management and protection stages;
- retained `UpdateLiveReaction()` as the live-analysis boundary after safety-critical management;
- added post-execution broker reconciliation and broker protection before telemetry/presentation;
- extended static and runtime source contracts to enforce the required stage order;
- kept later retry/backoff and supervisor redesign work out of this phase.

Important finding:
- active reversal protection still remains after the analysis stage because its current contract consumes `_decision`, `_m5Frame` and `_m15Frame`; moving that logic earlier would change its analytical prerequisites and exceed Phase 1.2.

Implementation commit:
- `77a1b1bc9912c7e1d78bd6428077a416a636f1a7`

Verification:
- source and architecture checks: PASS (workflow run 36497702461);
- runtime acceptance contracts: PASS (workflow run 36497702367);
- cTrader compile: PASS (workflow run 36497702418).

Result:
- Phase 1.2 complete;
- next phase: **Phase 1.3 — Runtime fault state machine**;
- operator pull: **required at this phase boundary**, after the final documentation commit.

## Current continuation point

Implementation queue:
- Phase 1.3 — Runtime fault state machine is next.

Current intent:
- introduce explicit HEALTHY / DEGRADED / ENTRY_BLOCKED / RECOVERING states around the existing runtime fault authority;
- preserve management, protection and reconciliation availability during recoverable analysis faults;
- do not combine Phase 1.3 with retry/backoff, timer supervisor or broader execution-safety redesign from later phases.

Before starting the next phase:
- pull the latest `main`;
- confirm the current HEAD;
- read the latest ROADMAP, ARCHITECTURE, WORKFLOW and this log;
- implement only Phase 1.3;
- re-run all applicable verification gates;
- update this log and roadmap at phase completion.


## Phase 1.3 — Runtime fault state machine

Status: complete.

Implementation:
- added `Runtime/Calculation/RuntimeFaultState.cs` with `HEALTHY`, `DEGRADED`, `ENTRY_BLOCKED` and `RECOVERING`;
- added deterministic `RuntimeFaultStateMachine` cycle/fault/recovery/re-arm semantics;
- connected the existing `RuntimeFaultBoundary` to the new state machine;
- added explicit fault-state cycle boundaries around `Calculate()`;
- entered recovery only after pre-analysis reconciliation, lifecycle recovery, active-plan management and protection complete without a current-cycle recoverable fault;
- kept broker automatic entry disarmed through recovery and after return to `HEALTHY` until an explicit `AutoTradingEnabled` false-to-true transition is observed;
- guarded market, aggressive and pending broker-entry owners without suppressing pending safety cleanup;
- added runtime contract and static architecture checks.

Important finding:
- `HEALTHY` is deliberately not an execution re-arm signal. Broker-confirmed state remains authoritative and recovery cannot silently reopen automatic execution.

Verification:
- source and architecture checks: PASS (workflow run 758);
- runtime acceptance contracts: PASS (workflow run 567);
- cTrader compile: PASS (workflow run 751).

Result:
- next phase after CI confirmation: **Phase 1.4 — Runtime recovery semantics**.

Certification record: implementation snapshot was verified by Source and architecture workflow 758, Runtime acceptance workflow 567, and cTrader compile workflow 751 before this documentation-only continuity update.

## Phase 1.4 — Closed-bar retry semantics and signal/execution synchronization

Status: complete.

Goal:
- bound repeated closed-bar analysis faults;
- prevent analysis failure from starving safety-critical management;
- make visual and execution reporting agree with the authoritative plan/broker state.

Implementation:
- added bounded closed-bar retry state to `RuntimeFaultStateMachine`: failure key, timestamp, exponential backoff, retry circuit and stale-failure detection;
- changed `Calculate()` so recoverable preparation/closed-bar analysis faults no longer cause an early return before management, protection and reconciliation;
- kept ordinary non-ready preparation exits unchanged when there is no runtime fault to recover;
- synchronized pre-trade SL rendering to `Plan.Stop` while preserving broker-confirmed SL as the live source;
- changed automatic market and aggressive success alerts to report broker-confirmed broker SL/TP values;
- changed pending Stop/Limit placement reporting to use the broker-confirmed pending order fields;
- guarded pending setup paths against missing decision/reaction dependencies;
- changed a silent reversal pending execution-model catch into an explicit failure state;
- added deterministic runtime contract coverage for closed-bar retry/backoff/circuit behavior;
- extended source/architecture verification for retry policy and signal/execution visual synchronization.

Deep audit findings:
- `MaximumOpenPositions` is publicly configurable above 1 while the current execution-capacity owner intentionally supports only a single active plan; this remains queued for Track 7.4 rather than being silently changed;
- M1 trigger configuration currently influences scoring but confirmed trigger readiness remains M5-based; Track 8.1 is required before changing the semantics;
- confidence is a deterministic composite score, not a calibrated probability; Track 9.2/9.3 must establish calibration before any probability wording is introduced;
- submission-gate failure history is shared per execution path rather than partitioned by signal identity; Track 2.1 will address coherent retry keys;
- daily-loss/trading-day/EOD semantics and cross-instance identity remain explicit later-track work;
- the UI already separates prediction/watch, confirmed signal and active plan conceptually, but a single immutable visual-state snapshot remains the planned Track 5.4 refinement.

Verification:
- Source and architecture checks: PASS (job 109187144452);
- Runtime acceptance contracts: PASS (job 109187144646);
- cTrader compile: PASS (job 109187144983).

Result:
- Phase 1.4 complete;
- next phase: **Phase 1.5 — Safety supervisor**;
- operator pull: required at the completed phase boundary after final verified documentation merge.


## Phase 1.5 — Async startup, performance optimization and safety supervision

Status: complete.

Implemented asynchronous Bars loading, bounded startup finalization/timeout, MTF context caching, closed-M1 frame reuse, recent M5 regime-core caching, age-bounded FVG/Order Block traversal, single-ATR reuse in protection, and a timer-driven safety supervisor that avoids full analysis on the heartbeat.

Uploaded indicator archive audit is recorded in docs/PERFORMANCE-ARCHITECTURE-2026-09-29.md.

Verification:
- Source / Architecture: PASS (verify job 109191030847);
- Runtime Acceptance Contracts: PASS (runtime job 109191030611);
- cTrader compile: PASS (build job 109191030662).

## Phase 5.4 — Canonical signal visual state

Status: complete.

Created SignalVisualSnapshot and centralized its builder/resolver. Chart plan, plan labels, pending rendering, signal arrows and panel signal state now consume the canonical snapshot. The renderer layer no longer reads decision/reaction/plan objects directly.

Verification:
- Source / Architecture: PASS (job 109194019581);
- Runtime Acceptance Contracts: PASS (job 109194019509);
- cTrader Compile: PASS (job 109194019411).
## Phase 2.1 — Unified execution submission retry policy

Status: complete.

Refactored the prior three submission-gate instances into one canonical keyed SubmissionGate. The policy now distinguishes signal key, attempt key and execution path, keeps failure/backoff/circuit state per identity, bounds retained failure state, and exposes one coordinator API to market, aggressive and pending execution paths.

Verification:
- Source / Architecture: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS.

Next: Phase 2.2 — Automatic market rejection matrix.

## Phase 2.1 — Unified submission retry policy

Status: complete.

Replaced three duplicate submission gates with one keyed policy using SubmissionAttemptIdentity and ExecutionSubmissionPath. Added runtime coverage for failure isolation, circuit behavior and reset. Uploaded ZIP audit: WaveTrend retained only as a future composite momentum candidate; TPO Profile file is empty.


## Phase 5.5 — Visual setup levels and execution controls

Status: complete.

Implemented:
- added the read-only TradeSetupPreview projection and builder, reusing the existing structural-stop and target-selection authorities;
- extended SignalVisualSnapshot with setup-preview levels;
- rendered Entry / Ideal Entry / Trigger / SL / TP1..TP4 through one shared level-line renderer before executable Plan activation;
- made the chart-level anchor explicit instead of reading the live Plan from the line renderer;
- replaced ToggleButton Click mutation with explicit Checked/Unchecked handlers using the existing runtime state owner;
- added architecture and runtime acceptance contracts for the visual/control boundaries.

Invariant: the setup preview is presentation-only and cannot authorize broker submission or create an executable Plan.

Strategy signal thresholds, RR policy, risk sizing, trailing and broker execution rules are intentionally unchanged in this phase.

Verification for final phase head `a7e8c2cf25256cfd5de098ed7f7660f409aa446d`:
- Source / Architecture: PASS (run 810).
- Runtime Acceptance Contracts: PASS (run 619).
- cTrader Compile: PASS (run 803).


Phase 5.5 merge record:
- PR #13 merged to main as `a357cca1920362975204069af405abf372a3b901` (squash merge).
- Final phase head before merge: `81bf6811519d29da0d42b78df64d88d76d37c530`.
- Final gates: Source / Architecture PASS (run 811), Runtime Acceptance PASS (run 620), cTrader Compile PASS (run 804).
- Main continuation point after the merge: **Phase 6.1 — Decision closed-bar contract**.


## Phase 5.6 — Responsive panel runtime

Status: complete on branch phase-5-6-responsive-panel-runtime.

Root cause and implementation are recorded in docs/PHASE-5-6-RESPONSIVE-PANEL.md. The phase decouples responsive panel refresh from safety supervision, makes panel refresh independent of Calculate, lazy-creates panel rows, eliminates redundant UI writes, centralizes one visual snapshot per panel refresh, refreshes initialization status, and exposes calculation freshness. No trading decision/risk/RR/trailing/broker semantics were changed.

Final verification on head `696f68513dbeb2b8ef83646201f7ce8734209fab`: Source/Architecture PASS (819), Runtime Acceptance PASS (628), cTrader Compile PASS (812).


## Phase 6.1 — Decision closed-bar contract

Status: complete.

Goal:
- make the temporal boundary of confirmed decisions explicit and deterministic;
- guarantee that decision frames are aligned to one UTC closed-bar reference;
- prevent future-bar leakage at the decision-input boundary.

Implementation:
- added Core/Math/ClosedBarReferenceRule.cs, which resolves the latest fully closed index from actual next-bar open times;
- replaced the prior ambiguous GetIndexByTime(...) dependency in IndexMath;
- passed one MtfClosedContext from closed-bar calculation into decision construction;
- added decision-input validation so required M5/M15/M30/H1/H4 frames and any present M1/D1/W1 frames must match the same canonical closed indices;
- changed timeframe agreement to consume those exact context indices;
- added deterministic runtime contract cases for exact boundaries, between-boundary behavior, real time gaps, future-bar rejection and end-of-series bounding;
- added source/architecture gates enforcing the Phase 6.1 ownership and alignment contract;
- recorded the phase design in docs/PHASE-6-1-DECISION-CLOSED-BAR.md.

Important finding:
- the first runtime acceptance test caught an incorrect expectation for a between-boundary reference (12:14): before the actual 12:15 open, the 12:00 bar is the latest fully closed bar. The production resolver was correct; the test expectation was corrected and the final contract passed.

Scope boundary:
- no score threshold, weight, RR, risk-sizing, trailing or broker-execution policy was changed by this phase.

Verification on final implementation head before documentation commits bed8dddc860083e76c193d4eb0d1dd4ba93962f0:
- Source / Architecture: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS.

Result:
- Phase 6.1 complete;
- next phase: Phase 6.2 — Reaction intrabar contract;
- operator pull: required at the completed phase boundary after the final verified documentation merge.


## Phase 6.2 — Reaction intrabar and chart observability

Status: complete.

User-observed runtime issue addressed:
- the chart could appear completely empty because the existing signal/plan/prediction renderers intentionally clear their objects when no qualifying visual state exists;
- the panel could not distinguish no calculation from other initialization/UI conditions;
- a persistent independent chart guide is now used as the operator-facing runtime observation surface.

Implementation:
- retained live reaction semantics on the current open M5 bar;
- added explicit `ReactionIntrabar` and `ReactionM5Index` fields to the canonical visual snapshot;
- routed reaction arrow anchoring through explicit snapshot identity;
- added `AnalysisGuideRenderer.cs`, which renders engine/data/calculation/decision/reaction/visual/plan/MTF status directly on the chart;
- ensured the guide is available during startup and does not depend on successful panel construction;
- removed the guide during full chart cleanup;
- added architecture gates for intrabar reaction semantics and chart observability;
- recorded the phase in `docs/PHASE-6-2-REACTION-INTRABAR.md`.

Important hardening:
- an intermediate CI failure was caused by a newly added display parameter increasing the parameter count from 535 to 536; the parameter was removed and the guide was made a base observability layer, preserving the established 535-parameter contract;
- an intermediate compile failure was caused by incorrect C# newline escaping in the guide text; this was corrected before final verification.

Verification on final implementation head `a2a14b7a806c1432cbd174d9bb06b7c443cc1d35`:
- Source / Architecture: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS.

Result:
- Phase 6.2 complete;
- next phase: **Phase 6.3 — Aggressive entry policy**;
- operator pull: **required at the completed phase boundary after the final verified documentation merge**.


## Post-Phase 6.2 runtime stabilization — 2026-09-29

User runtime validation after the Phase 6.2 merge reported that the indicator could remain visually blank for roughly 30 seconds, with no reliable indication of whether initialization or calculation had actually progressed.

Re-audit found two concrete issues:

1. AnalysisGuideRenderer used Chart.DrawText anchored above the visible high. A valid object could therefore be outside the visible price range.
2. Async market-data initialization can finish after the host's initial calculation callback sequence. The indicator then becomes _initializationReady without guaranteeing that one completed calculation cycle has executed.

Stabilization changes:

- replaced the chart guide with fixed-position Chart.DrawStaticText at the top-right;
- added per-timeframe dataset counts and initialization age to the guide;
- centralized the full calculation body under RunCalculationCycle(int index);
- kept Calculate(int index) as the host entry point delegating to that owner;
- added a one-shot startup catch-up from OnTimer immediately after readiness;
- kept the catch-up bounded by _lastCalculationCompletedUtc, so normal 500 ms heartbeat supervision does not become a full analysis loop;
- preserved the existing broker reconciliation, protection, execution, decision, risk, and closed-bar contracts;
- introduced no parameter, preserving the 535-parameter gate.

Verification gates required before merging:

- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile.


## Runtime regression correction — 2026-09-29

The post-Phase-6.2 observability hotfix was reverted after user validation
reported delayed startup, a sluggish/blank panel, and no reliable live panel
updates.

The concrete regression sources were:

- a persistent chart guide invoked from every panel refresh and rebuilding a
  full visual snapshot;
- a full startup calculation executed from the timer thread immediately after
  async initialization.

Both additions were outside the previously smooth panel/calculate cadence.

Restoration:

- deleted the persistent chart guide;
- removed all guide rendering/cleanup paths;
- removed the startup calculation catch-up;
- restored the established `Calculate(int index)` orchestration;
- preserved the Phase 6.1 closed-bar decision contract;
- preserved the Phase 6.2 intrabar reaction identity and current-open-M5
  semantics;
- kept the 535-parameter contract unchanged.

The existing responsive-panel runtime remains the production UI path. The
next implementation phase is **Phase 6.3 — Aggressive entry policy**, to start
only after this correction is verified on the local cTrader instance.


## Startup/core-readiness correction — 2026-09-29

User validation after the previous runtime rollback still reported slow/partial startup and cases where calculation did not reach the panel.

Root cause isolated from source inspection:

- StartAsyncBarsInitialization requested primary M5/M15/M30/H1/H4 data together with M1 and optional D1/W1 data;
- _initializationDataReady only became true after every requested async callback completed;
- FinalizeAsyncInitialization previously waited on _initializationDataReady, even though HasEnoughData() only requires M5/M15/M30/H1/H4;
- after async initialization became ready, cTrader can have already completed its historical Calculate callback sequence, so _initializationReady could become true without any subsequent completed calculation cycle;
- the result was a valid-looking panel startup state without guaranteed decision / reaction / presentation state.

Correction implemented:

- initialization finalization now keys off HasEnoughData() for the required decision frames instead of waiting for optional M1/D1/W1 callbacks;
- optional frames continue to arrive asynchronously and remain available to the existing lazy/native/context paths;
- added CalculationStartupSeed.cs with a single lightweight post-readiness seed;
- the seed builds the canonical MTF closed context, runs closed-bar analysis, updates live reaction and execution-model presentation state, then renders the normal visual/panel path;
- the seed explicitly does not call market execution, aggressive execution, pending execution or the full live management pipeline;
- normal Calculate(int index) remains the recurring live calculation authority;
- no public parameter was added or changed; production parameter count remains 535;
- no decision threshold, score, weight, RR, risk, trailing or broker-confirmation rule was changed.

Validation target:

- source / architecture gate;
- runtime acceptance contracts;
- cTrader compile.

This is a corrective hotfix only. Phase 6.3 remains the next strategy phase and is not mixed into this correction response.


## Phase 6.3 — Controlled intrabar aggressive entry policy — 2026-09-29

Status: complete.

User clarified that the pre-Phase-6 baseline calculated and rendered normally; therefore this phase preserves the established decision, planning, visual and execution owners and only resolves the temporal ambiguity of the already-existing aggressive reaction path.

Finding:
- the aggressive path consumed `_reaction`, which is explicitly evaluated on the current open M5 bar;
- the same path also received `closedM5` for structural SL/TP and execution context;
- without an explicit bridge, aggressive entry semantics were mixed-bar and could not state exactly how a transient reaction became executable.

Implementation:
- added `AggressiveEntryPolicy` as the single qualification owner;
- requires two distinct qualifying reaction observations on the same open M5 bar;
- repeated observations with the same reaction timestamp do not increment qualification;
- direction changes, `EntryAllowed` loss and M5 rollover invalidate the latch immediately;
- a confirmed aggressive fill consumes the latch so one qualifying reaction cannot be reused indefinitely;
- existing risk, structural SL/TP, market execution, submission-gate and broker-confirmation paths remain unchanged;
- no public parameter was added; the production parameter contract remains 535.

Validation:
- deterministic runtime contracts cover first sample, duplicate sample, second sample arming, direction invalidation, qualification loss and new-bar reset;
- source/architecture gate enforces the dedicated policy owner and execution boundaries;
- cTrader compile remains required before merge.

Scope boundary:
- no decision thresholds, general weights, RR, risk formulas, trailing rules or broker mutation semantics were changed.


## Startup responsiveness correction — 2026-09-29

User validation still reported global sluggishness after the core-readiness correction.

Baseline comparison against the known-good `696f685...` runtime showed that the persistent calculation/panel hot path is materially unchanged by Phases 6.1 and 6.2 after the chart-guide rollback. The remaining newly introduced heavy operation was the startup calculation seed being executed synchronously from `FinalizeAsyncInitialization()`.

Correction:
- the first READY panel is rendered first;
- the one-shot startup calculation seed is queued with cTrader's `BeginInvokeOnMainThread()` rather than executed inline inside initialization finalization;
- duplicate queueing is prevented with `_startupCalculationSeedQueued`;
- the seed remains one-shot and normal `Calculate()` remains the recurring calculation authority;
- no decision, risk, RR, trailing, broker-confirmation or signal-rendering semantics are changed;
- no public parameter is added; 535 remains unchanged.

This correction is deliberately limited to startup responsiveness and must pass all three project gates before merge.


## Phase 6.4 — Compact 40-Bar Plan-Level Visuals — 2026-09-29

User requested a cleaner chart presentation: Trigger, Entry, SL and TP levels
must no longer span the full chart; they should cover 40 candles back from the
latest candle, with a small left-attached name/price box in the same semantic
color as the level. The user also reiterated that responsiveness and
calculation performance are permanent priorities.

Implementation and optimization:

- `PlanLineRenderer` now fixes the plan-level span to 40 bars ending at the
  latest chart candle;
- visible-chart boundaries are no longer used to stretch plan levels;
- line styles are differentiated by level while preserving the existing color
  owners;
- `PlanLabelRenderer` adds a compact left-side chart-bound tag using reusable
  `ChartText` plus a lightweight outlined `ChartRectangle`;
- labels use a small fixed 9 px presentation size, bold text, left/center
  alignment and the existing level color;
- `PlanLabelRenderCoordinator` no longer deletes every visible label before
  each refresh; visible objects are updated in place and only stale labels are
  removed;
- teardown still removes both text and box objects;
- no public parameter was added, so the production parameter contract remains
  535;
- decision, reaction, risk, RR, SL/TP construction, broker confirmation,
  execution and lifecycle semantics are unchanged.

Performance reasoning:

The plan-label path was previously doing unconditional remove/recreate work
during refresh. Reusing existing chart objects removes avoidable chart-object
churn. The level renderer also keeps the chart footprint bounded to the
requested 40-bar span.

Verification targets:

- Source / Architecture: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS.

Repository gates validate source/contract/compile behavior. Actual visual
appearance and terminal responsiveness still require hands-on cTrader
validation after the merge.


## Runtime UI hotfix — instant panel Hide/Show — 2026-09-29

User validation reported that pressing the panel Hide/Show control itself could take a long time to visibly respond.

Source inspection isolated the direct cause in UI/Panel/PanelVisibility.cs: TogglePanel() changed the panel visibility and then synchronously called RenderPanel() whenever the panel was shown again. RenderPanel() is intentionally responsible for full panel layout/row synchronization and is therefore much heavier than a visibility toggle.

Correction:

- TogglePanel() now performs only the visibility state mutation and restore-button visibility change;
- the synchronous RenderPanel() call was removed from the click path;
- the normal ready-state panel heartbeat remains responsible for ordinary refresh/reconciliation after the panel becomes visible;
- no calculation, signal, entry, SL/TP, execution or broker state is changed;
- no public parameter is added; the 535-parameter contract remains unchanged.

This hotfix is strictly a responsiveness correction. It does not claim that the full calculation pipeline has been optimized; that remains a separate measurement/strategy-quality concern and is tracked by the performance roadmap.


### Hotfix merge record — 2026-09-29

PR #24 (instant panel Hide/Show) merged to main as `f57866a441e5e5babd83c268baa7bf91f9ff3f1b` after Source / Architecture, Runtime Acceptance and cTrader Compile all passed on the verified hotfix head. The hotfix changes only panel visibility interaction; hands-on cTrader responsiveness remains pending.

## Phase 7.1 — Hidden-clamp audit — 2026-09-29

Status: complete.

Objective:
- audit user-facing parameters for hidden hard-coded floors/ceilings and silent overrides.

Findings and corrections:
- `ClosedBarTriggerReadyEvaluator` previously enforced `trigger >= Math.Max(4, requiredTrigger)`, making Live Trigger Score values 1–3 ineffective and potentially making Precision Trigger Score values below 4 ineffective;
- the trigger gate now consumes `requiredTrigger` directly, preserving the existing precision-policy combination of the user-facing trigger parameters;
- `LiveTargetCandidateEvaluator` previously enforced `Math.Max(0.05, TargetUpdateStepAtr)`, making the declared 0.02–0.05 ATR parameter range partly ineffective;
- the live target update step now consumes `TargetUpdateStepAtr` directly.

Reviewed intentional bounds:
- index clamps protect valid historical indexing;
- EntryBufferAtr floor matches its declared minimum;
- StructuralTpRrStep floor matches its declared minimum;
- MinimumTpSpacingAtr floor matches its declared minimum;
- MaximumRewardRR clamp is below its declared minimum and therefore does not override user-facing values;
- DirectDisplacementOverrideScore clamp stays within its declared parameter range;
- level-hit contribution bounds an internal score component and is not a user-facing setting.

Verification hardening:
- `tools/verify_architecture.py` now checks the declared parameter ranges and rejects reintroduction of the two material hidden clamps.

Scope:
- no new parameters;
- 535-parameter contract preserved;
- no changes to decision authority, broker mutation, RR/risk policy or trailing policy;
- Smart Entry/SL/TP and trailing improvements remain in their dedicated roadmap phases.

Acceptance:
- Source / Architecture: required;
- Runtime Acceptance Contracts: required;
- cTrader Compile: required;
- hands-on cTrader validation remains required for actual runtime behavior.
## Runtime UI and smart protection hotfix — 2026-09-29

User validation reported three concrete live/runtime issues remaining after the Phase 7.1 merge:
- panel AUTO TRADE and AUTO ORDERS quick toggles did not visibly/operationally act as reliable operator controls;
- compact name/price boxes were still absent from setup-preview levels;
- SL behavior could follow raw market price instead of waiting for meaningful structural progression.

Corrections:
- switched the two quick execution toggles to direct ToggleButton Click actions;
- click handlers now update the canonical private runtime flags and immediately resynchronize the controls;
- preserved the execution-toggle synchronization guard so programmatic refresh cannot act like an operator click;
- setup previews now render compact plan labels instead of deleting them;
- compact label rectangles are created before text, use the level semantic color with translucent fill, and are reused;
- removed the final market-distance stop clamp from ProtectionManager so SL is no longer derived from raw price movement;
- further trailing progression is structurally gated to closed-M5 events and swing-derived candidates;
- momentum alignment only changes structural breathing distance; it does not create a price-chasing stop;
- pressure-based tightening is also closed-M5 gated;
- existing break-even/protection validation and monotonic broker progression remain unchanged.

Auto Orders note:
- existing pending execution already uses future Stop/Limit prices and validates that pending prices are away from market;
- deeper reversal-point forecasting across MTF structure/FVG/OB/liquidity/indicator confluence remains a dedicated smart-pending improvement and is not mixed into this UI/protection correction.

No public parameter was added. Production parameter count remains 535.
### Hotfix CI contract correction — 2026-09-29

The first hotfix CI attempt exposed stale test contracts that still required ToggleButton Checked/Unchecked handlers. Since cTrader's current ToggleButton API explicitly supports Click events, the UI action boundary was intentionally moved to direct Click handling. The source verifier and Runtime Acceptance contract were updated to enforce the new single action boundary and reject duplicate Checked/Unchecked owners.

The first failed Source/Runtime runs were therefore contract mismatches in the test harness; cTrader compile for that hotfix head already passed.


## Corrective hotfix — execution priority, toggle state events and structural lock — 2026-09-29

User runtime feedback identified that the chart AUTO TRADE / AUTO ORDERS controls still required a stronger cTrader event binding, while predictive pending execution could be starved by early market-plan creation. The same feedback also required aggressive entry to receive priority when its intrabar qualification is satisfied.

- switched the two quick execution toggles to the official cTrader `Checked` / `Unchecked` state events as their single operator-action owner;
- kept `_executionToggleSyncing` as the guard against programmatic synchronization becoming an operator action;
- explicitly kept both execution toggles enabled;
- reordered the live execution path to predictive pending -> aggressive auto -> plan creation -> normal market execution;
- deferred automatic plan creation while a managed pending order exists, preventing a confirmed predictive order from being hidden by a recreated market plan;
- preserved the existing structural pending entry, SL/TP, submission-gate and broker-confirmation policies;
- preserved structural-only stop progression and removed raw market-price chasing from the protection manager;
- retained compact chart label rendering, with semantic level color used by each label box;
- added `docs/HOTFIX-EXECUTION-PRIORITY-STRUCTURAL-LOCK.md` and updated source/runtime acceptance contracts;
- no public parameters were added or removed; production parameter count remains 535.

This hotfix is corrective and does not close the planned Track 7.2 parameter audit. Broader signal-strengthening, level intelligence, target/stop refinement and full trailing certification remain owned by their roadmap phases.


## Phase 7.2 — Dead/Unused Public Parameter Audit — 2026-09-29

Status: complete.

Implementation:
- Added `tools/audit_parameters.py` and connected it to Source / Architecture CI.
- The first machine audit of 535 parameters found four unread candidates: FullWidthLevelLines, LabelLeftOffsetBars, ShowEarlyArrow and SmartUseClosedBarDecision.
- FullWidthLevelLines was retained and activated in PlanLineRenderer; compact 40-bar presentation remains the default by changing its default to false.
- LabelLeftOffsetBars was activated in PlanLabelAnchorCalculator; label-box width now accommodates the configured offset.
- ShowEarlyArrow was activated as an independent early/watch arrow visibility control; confirmed/reaction arrow visibility remains governed by ShowSignalArrow.
- SmartUseClosedBarDecision was removed because confirmed decision logic is safety-enforced closed-bar behavior and must not be user-disableable.
- Current public parameter surface is 534: 531 baseline + 3 OSS extension parameters.

Findings:
- Final audit: 534 parameter declarations, 534 read-by-code candidates, 0 unused/unread candidates.
- No trading authority, risk authority, broker mutation path or second decision/execution engine was introduced.

Verification:
- Source / Architecture: PASS
- Runtime Acceptance Contracts: PASS
- cTrader Compile: PASS

Continuity:
- Phase 7.2 continuity document: `docs/PHASE-7-2-DEAD-PARAMETER-AUDIT.md`.
- Persistent user strategy priorities updated: important levels, deeper Order Block analysis, higher-quality signals and smarter cross-analyzer coordination.
- Next phase: Phase 7.3 — Semantic duplicate audit.
- Operator pull requirement: required after Phase 7.2 merge; intermediate branch commits do not require a local pull.

## Phase 7.2 merge record — 2026-09-29

PR #29 was merged into main as `861f15dda5af4599c92acb64bb6793ed2dfc296e`.

Final verified state:
- Source / Architecture: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS;
- 534 public parameters remain, with 0 declared-but-unread candidates;
- next implementation phase: Phase 7.3 — Semantic duplicate audit.

The earlier corrective-hotfix notes in this log describe intermediate states; the
final Phase 7.2 implementation and parameter audit supersede those interim
descriptions where they differ.

Operator action:
- local pull is required after the Phase 7.2 merge.


## Corrective UI and chart hotfix — 2026-09-29

User validation found two remaining problems: chart level lines could stop before the latest chart candle, and the Auto Trade / Auto Orders controls were not guaranteed to represent the configured cTrader settings.

Chart correction: PlanLineRenderer now owns one canonical chart span. Compact mode uses 40 chart bars ending at Bars.Count - 1. FullWidthLevelLines remains supported. M5 event-time mapping is no longer used to choose the line endpoint. Pending level rendering no longer depends on a temporary M5 anchor. Label placement reuses the same canonical left-edge calculation.

Control correction: Auto Trade and Auto Orders are now display-only switch-style status cards. They show ON/OFF from the normal settings/runtime synchronization path. They do not register click or toggle events and they cannot create an independent UI override.

Added the runtime UI audit tool and wired it into source CI. README current parameter count is 534. Roadmap and acceptance documentation were updated. A dedicated hotfix note was added in docs/HOTFIX-CHART-LINES-EXECUTION-STATUS-2026-09-29.md.

No strategy, risk, reward, protection or broker-order ownership was intentionally changed. Hands-on cTrader validation remains required.

PR #30 is the implementation vehicle. The next planned strategy phase remains Phase 7.3 — Semantic Duplicate Audit.


## Hotfix merge record — Chart Lines and Execution Status UI — 2026-09-29

PR #30 was merged into main as 3c5afe6f37c8852fb6975bbb5d4004bc1ec95ffe.

Final automated gates on the hotfix head were all PASS:
- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile.

The implementation corrected the compact level geometry so lines terminate at the latest chart candle, removed M5 event-time dependence from line endpoints, removed pending visual dependence on a disposable M5 anchor, and unified label placement with the canonical line geometry.

AUTO TRADE and AUTO ORDERS are now display-only modern switch-style status cards. Their state is synchronized from the canonical configuration/runtime path and the UI no longer owns an independent action or override path.

The dedicated hotfix document records the root causes and validation boundary. Hands-on cTrader visual/control validation remains required and is not claimed by automated CI.

Continuity: the next planned implementation phase remains Phase 7.3 — Semantic Duplicate Audit. Local pull is required after this merge before the next phase is started.


## Phase 7.3 — Semantic duplicate audit and visual synchronization — 2026-09-29

Status: implementation complete; automated verification is being finalized.

Findings and corrections:
- removed the proven redundant structural-stop parameter EnableDynamicSlTrail; EnableStructuralSlRepricing remains the single owner;
- found and removed the redundant DecisionEngine facade; DecisionOrchestration now calls DecisionEvaluator directly, leaving one concrete Evaluate(DecisionInputSnapshot) implementation;
- preserved stage-specific confidence, quality, RR, cooldown and trail parameters where their runtime roles are distinct;
- fixed the visual synchronization defect where setup levels could remain hidden until TriggerReady, causing the chart to show levels only after price crossed the trigger;
- setup preview now appears for an aligned directional structural forecast before trigger confirmation, while actual plan creation remains TriggerReady-gated;
- added permanent semantic-parameter and whole-project integrity audits to CI;
- updated workflow documentation so whole-project auditing is mandatory for every future phase.

Current parameter surface: 533 = 530 baseline + 3 OSS extension.

Next planned phase after green verification: Phase 7.4 — MaximumOpenPositions semantics.
Track 8 remains the analytical correctness track, including the dedicated Order Block mathematical audit.


## Phase 7.3 verification closure — 2026-09-29

Final branch implementation was verified before merge:
- Source / Architecture: PASS;
- dead/unused parameter audit: PASS, 533/533 read with 0 unread;
- runtime UI audit: PASS;
- semantic parameter audit: PASS;
- full project integrity audit: PASS across 419 production C# files with 0 exact duplicate method signatures;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS.

The full-project audit is now a permanent CI gate. It explicitly scans source ownership, parameter declarations, duplicate methods, visual synchronization, execution UI authority, safety boundaries and documentation continuity on every phase.

Historical roadmap/log coverage gaps are reported by the audit as informational because the roadmap contains older umbrella/subphase records created before the current log discipline. Current development phases must have explicit implementation, findings, verification and pull-state records.

Phase 7.3 can be considered complete on the implementation branch; merge is the next boundary. Next planned phase: Phase 7.4 — MaximumOpenPositions semantics.


## Phase 7.3 merge record — 2026-09-29

PR #31 was merged into main as a968550cae91e0dc2c54662f6dca2c5411654289.

Phase 7.3 is complete. The implementation removed the proven duplicate EnableDynamicSlTrail and the redundant DecisionEngine facade, restored pre-trigger structural level visibility, and installed permanent semantic/project-wide audits in CI.

Final pre-merge verification: Source / Architecture PASS; dead/unused parameter audit PASS; runtime UI audit PASS; semantic parameter audit PASS; full project integrity audit PASS; Runtime Acceptance Contracts PASS; cTrader Compile PASS.

Current project surface: 533 public parameters (530 baseline + 3 OSS extension). The standing project audit scans 419 production C# files for duplicate method signatures, parameter uniqueness, visual synchronization, execution authority and continuity rules.

Next planned implementation phase: Phase 7.4 — MaximumOpenPositions semantics.

Operator action: local pull is required after this merge/documentation boundary.


## Phase 7.4 — MaximumOpenPositions semantics — 2026-09-29

Status: implementation complete; verification/merge boundary follows.

Implementation:
- Kept `MaximumOpenPositions` as a public parameter for preset/API compatibility, but constrained it to the only supported architecture: DefaultValue=1, MinValue=1, MaxValue=1.
- Removed `BlockNewSignalWhileActive` because the single-active-plan boundary is mandatory and cannot be disabled without introducing unsupported multi-plan semantics.
- Reworked `ExecutionCapacityRule` into the platform-neutral owner for single-plan capacity, with separate semantics for new plan creation and new broker execution.
- Centralized cTrader state-to-rule translation in `ExecutionCapacityGuard`.
- Migrated plan creation, automatic market execution, aggressive execution and predictive-pending placement to the canonical capacity guard.
- Removed duplicate late-path position-capacity formulas.
- Added deterministic runtime contracts for empty capacity, existing plan, managed position, managed pending order and unsupported capacity cases.
- Extended architecture/integrity/semantic parameter audits so this contract cannot silently regress.

Deep signal/trade coordination audit:
- The decision path is correctly bound to the canonical closed-M5 context and trigger readiness remains distinct from directional state.
- The user-reported false-signal problem is not solved by this capacity phase and is not being hidden behind higher thresholds.
- Concrete remaining analytical risks recorded for Track 8/9: M1 direction currently contributes to decision score while canonical TriggerReady is still M5 closed-bar based; structural/liquidity evidence can be consumed in both score and downstream gates; and structural confirmations can overlap across M5/M15/H1/H4.
- These are now explicitly queued for mathematical/causal correction beginning in Phase 8.1, with Phase 8.4 reserved for the deep Order Block audit.

Verification target:
- 532 public parameters = 529 baseline + 3 OSS extension;
- MaximumOpenPositions is constrained to 1;
- BlockNewSignalWhileActive is absent;
- one platform-neutral capacity rule and one cTrader capacity guard;
- plan creation plus all automatic execution paths use the same capacity owner;
- full-project, semantic-parameter, runtime UI, runtime acceptance and cTrader compile gates all green before merge.

Next phase: Phase 8.1 — M1 trigger correctness.
Operator pull requirement: required after the final verified Phase 7.4 merge; intermediate branch commits do not require a local pull.


## Phase 7.4 merge record — 2026-09-29

PR #32 was merged into main as 29881e1a89eb0c8d92f2465234a6c3a44671154a.

Final pre-merge verification:
- Source / Architecture: PASS;
- dead/unused parameter audit: PASS, 532/532 read with 0 unread;
- runtime UI audit: PASS;
- semantic parameter audit: PASS;
- full project integrity audit: PASS across 419 production C# files with 0 exact duplicate method signatures;
- execution-capacity semantics audit: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS.

Phase 7.4 leaves MaximumOpenPositions exposed only at the supported value 1, removes the optional BlockNewSignalWhileActive control, and centralizes single-plan semantics across plan creation, automatic market, aggressive and predictive-pending execution.

Next planned phase: Phase 8.1 — M1 trigger correctness, with the operator-reported false-signal problem as a primary analytical focus.

Operator action: local pull is required after this merge/documentation boundary.


## Phase 8.1 — M1 trigger correctness — 2026-09-29

Status: implementation verified and ready to merge.

Verification closeout on verified branch head `929700e154d4b84a5a0b9efeae345b92017834b6`:
- Runtime Acceptance Contracts: PASS — workflow run 801;
- cTrader Compile: PASS — workflow run 985;
- Source / Architecture: PASS — workflow run 992.

Findings and corrections:
- Audited the full Decision → Trigger → Plan → Execution chain and confirmed that UseM1Trigger previously added a fixed +3 directional vote from M1Frame.Direction while DecisionEvaluator still derived TriggerReady from M5-only ClosedBarTriggerReady.
- Removed the fixed M1 score vote so M1 no longer creates an independent directional authority.
- Added Core/Math/M1TriggerRule.cs as the single platform-neutral owner of deterministic M1 confirmation semantics.
- Added Planning/Entry/M1TriggerReadyEvaluator.cs to consume the real closed M1 bar, verify exact containment inside the selected closed M5 candle, and evaluate M1 OHLC/ATR/candle direction/close location/trigger score.
- Extended DecisionEvidenceSnapshot and DecisionEvaluator so M1 is an optional confirmation gate after M5 TriggerReady, preserving the existing MTF decision direction.
- Added runtime acceptance scenarios for exact M1/M5 boundaries, future-bar rejection, direction symmetry, weak-body rejection, close-location rejection and insufficient trigger score.
- Extended Source / Architecture verification to enforce one M1 rule owner, real M1 evidence consumption, absence of the fixed +3/-3 vote and downstream M1 trigger gating.

Coordination invariant after Phase 8.1:
- UseM1Trigger=false → TriggerReady = ClosedM5TriggerReady.
- UseM1Trigger=true → TriggerReady = ClosedM5TriggerReady AND selected-direction M1TriggerReady.
- Decision direction remains owned by higher-timeframe consensus; M1 cannot vote the direction.
- Plan creation and all execution paths remain downstream of TriggerReady/capacity/risk ownership.

Remaining signal-quality work is intentionally not hidden in this phase: structural/liquidity evidence de-duplication, swing plateau semantics, FVG mathematics, Order Block mathematics and confidence calibration remain subsequent Track 8 work.

Verification target:
- deterministic runtime M1 trigger contracts;
- source/architecture M1 ownership gate;
- Runtime Acceptance Contracts;
- cTrader Compile;
- whole-project integrity remains green.

Next phase: Phase 8.2 — Swing plateau correctness.
Operator pull requirement: after final verified Phase 8.1 merge.


## Phase 8.2 kickoff — 2026-09-29

Phase 8.1 post-merge gates on main `3dbbeed5b703f0e5be13f5a1aabc6c87fd13f3b7`: Source/Architecture PASS, Build PASS, Runtime PASS. Created `phase-8-2-swing-plateau-correctness` from verified main. Initial audit: strict-neighbor swing extrema, raw-price equal-level bucketing, and rolling-extreme sweep semantics are independently represented. Scope: canonical plateau/level identity, bounded non-chaining equal-level clusters, causal sweep establishment, and de-duplication of one structural break across BOS/MSS/CHOCH. No win-rate/false-signal improvement claim; no public parameter or execution/risk changes without explicit justification.


## Phase 8.2 implementation closeout — 2026-09-29

The production structural chain now consumes the canonical plateau semantics introduced by `SwingPlateauRule`. Swing highs/lows are represented once per contiguous plateau; Equal High/Low compares canonical swing levels instead of arbitrary raw-bar pairs and uses fixed-anchor tolerance; liquidity sweeps require a previously confirmed structural swing level. `StructuralEvidenceRule` prevents Structure plus MSS/CHOCH from stacking as multiple independent events on the same timeframe. Runtime contracts cover plateau symmetry, closed-index confirmation, non-chaining tolerance and structural-event de-duplication. The pre-documentation head `5df5931828719fb635ec67fa59d57b519d4e70e7` passed Runtime, Build and Source/Architecture; documentation closeout changes are now on the latest branch head and require their own final CI pass.

## Phase 8.2 merge closeout — 2026-09-29

PR #36, `Phase 8.2 — Swing Plateau Correctness`, merged to `main` as `a9c63bb3e563126753206c49b070763108919b74`. Post-merge verification on that merge commit passed Runtime Acceptance, cTrader Compile/Build and Source/Architecture. The subsequent documentation continuity commits record the merge and advance the roadmap to Phase 8.3 — FVG mathematical audit. Target-terminal replay remains required for empirical signal-quality measurement; no win-rate or false-signal reduction claim is made from CI alone.

## Phase 8.3 kickoff — 2026-09-29

Started branch `phase-8-3-fvg-mathematical-audit` from verified main `9ffeeb22fe1da41ab7d760a3dd3a60608a543a35`. Audit findings: production FVG thresholding used current-bar ATR for historical gaps; retest acceptance used close-only proximity; full-fill handling and wick/body semantics were duplicated between mitigation paths; FVG geometry and source identity were not centrally owned. The phase introduces `FvgRule` as the deterministic mathematical owner, adds stable Zone identity, and keeps all public parameter counts unchanged. No empirical signal-quality claim is made until target cTrader replay.

## Phase 8.3 verification closeout — 2026-09-29

Automated gates on implementation head `89919e7363d374e2cf3a362ec553b1fdac464919` all passed: Runtime Acceptance, Build and Source/Architecture. Production and predictive-pending FVG paths now consume `FvgRule`; historical FVG sizing uses creation-bar ATR; retest requires a later bar; mitigation/full-fill semantics are centralized; managed zones retain source identity. Public parameters, risk, execution and lifecycle contracts were not expanded. No empirical false-signal or win-rate improvement is claimed without target cTrader replay.

## Phase 8.4 kickoff — 2026-09-29

Started branch `phase-8-4-order-block-mathematical-audit` from main `2c916a023f46940ff9df40c5f96066a493d0ddf9`. Audit target: centralize Order Block source-candle geometry, displacement/structure evidence using source-bar ATR, mitigation and identity; reuse canonical Phase 8.3 FVG mathematics for OB/FVG confluence. User-supplied WaveTrend source is not currently accessible in repository or searchable conversation/Library content, so no exact reproduction claim is made and no guessed WaveTrend formula is introduced.

## Phase 8.4 verification closeout — 2026-09-29

Head `41a578ba72fec2219447ddc1ceff12b96ee353e7` passed Runtime Acceptance, Build and Source/Architecture. Production Order Block logic now uses a single mathematical owner for source candle direction/geometry, source-bar ATR displacement and structure thresholds, directional mitigation and identity. OB/FVG confluence consumes canonical Phase 8.3 FVG mathematics. The user-provided FVG and custom WaveTrend reference sources were inspected from Library archives; WaveTrend remains intentionally outside the decision path until a closed-bar exact adapter is tested. No new public parameters were added and no empirical signal-quality improvement is claimed without cTrader replay.


## Phase 8.5 — Zone Confluence Symmetry and Timely M1 Trigger Runtime — 2026-09-29

Status: implementation complete; final verification/merge boundary pending.

Root-cause audit:
- TriggerReady was updated only during the closed-M5 decision cycle, creating a same-M5 delay after a valid M1 trigger appeared.
- the automatic plan path was keyed only to the M5 attempt index, so a newly valid M1 trigger could not reliably cause a same-M5 retry;
- a transient M1-direction mismatch could freeze into the M5 decision through a separate veto;
- Order Block liquidity-sweep evidence had a parallel rolling-extreme definition instead of the canonical structural swing/reclaim rule;
- neutral RSI=50 / DMI=0 could contribute directional trigger points;
- generic zone-overlap semantics were duplicated.

Corrections:
- added TriggerRuntimeState and M1TriggerRuntimeUpdater;
- added IsClosedM1InsideM5Window to the platform-neutral M1 trigger rule;
- inserted M1 trigger runtime evaluation into the live calculation stage before plan synchronization;
- automatic plan retry now keys on confirmed-M1 revision as well as M5;
- SignalVisualSnapshot carries trigger runtime status and SignalRenderer draws the exact confirming M1 marker;
- trigger marker visibility is owned by ShowTrigger rather than ShowSignalArrow;
- DecisionConfirmationGates no longer converts temporary M1 direction into a frozen decision veto;
- OB liquidity sweep reuses BullLiquiditySweep/BearLiquiditySweep;
- Bull/Bear trigger score analyzers require RSI/DMI to be directionally strict rather than neutral-inclusive;
- ZoneConfluenceRule provides symmetric positive-width overlap semantics for generic execution-zone geometry;
- FvgRule remains the canonical OB/FVG confluence owner.

CI feedback and fixes:
- Runtime compile initially failed because ZoneConfluenceRule was not linked into CFIP.Runtime.Contracts; fixed in the contract project;
- Source/Architecture initially failed because OB/FVG confluence temporarily bypassed FvgRule.IsOverlapInclusive; restored canonical FVG ownership;
- Runtime contract then caught an overly-inclusive touch-boundary assumption; overlap was changed to positive-width semantics with explicit tolerance.

WaveTrend:
The exact custom WaveTrend source is not available in the current searchable repository/Library continuation. No guessed formula was introduced. It remains an exact closed-bar confluence integration item.

Verification boundary:
- automated Runtime Acceptance, Source/Architecture and cTrader Compile/Build must all pass on the final branch head;
- no empirical performance claim is made from CI;
- target cTrader replay is required for visual and signal-timing validation.

Continuity:
- current branch: phase-8-5-zone-confluence-trigger-synchronization;
- PR #39;
- operator pull is required only after final merge to main.


## Phase 8.5 verification closeout — 2026-09-29

PR #39 was merged to `main` as `cbda7910ad3b30fbd74cc526676e6372cb098cd7`. Pre-merge head `3e77b974cc00c82e9f134bfd0057493ea47c29e7`: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture PASS. Post-merge verification on `cbda7910ad3b30fbd74cc526676e6372cb098cd7`: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture PASS.

Current engineering result: closed M1 confirmation can unlock a valid M5 direction inside the current forming M5 window without using unclosed M5 OHLC as trigger evidence; same-M5 plan retry occurs on new M1 confirmation; trigger readiness and exact M1 confirmation are represented in the canonical visual snapshot; FVG remains owned by FvgRule; OB/FVG confluence retains canonical FVG semantics; OB liquidity sweep searches earlier closed canonical sweep events; neutral RSI=50/DMI=0 are not directional votes.

WaveTrend exact-parity integration remains explicitly pending because the exact supplied source is not accessible in the current repo/Library continuation; no formula was guessed. The stale Phase 8.1 PR #35 was closed as superseded.

Operator pull requirement: pull `main` now before local continuation.


## Phase 9.1 kickoff — 2026-09-29

User-requested requirements: use H1+ as the primary opportunity-discovery layer, then calibrate through M30/M15/M5/M1; keep panel responsive; clarify the chart alert label; and make TP/trailing progression react promptly.

Deep audit findings:
- DecisionScoreCalculator allowed M5/M15/M30 lower-timeframe contribution to dominate the raw directional sum even when H1/H4/D1 disagreed, because HTF weights were comparatively small.
- HigherTimeframeConfidencePenalty only lowered confidence; it did not provide directional ownership.
- TimeframeAgreement was descriptive, not a top-down permission model.
- Panel heartbeat ran safety supervision plus full RenderPanel(), so the 500 ms heartbeat could repeatedly pay the full panel layout cost.
- RenderPanel also hid every existing row before re-rendering, creating unnecessary UI property churn.
- Live structural SL repricing and unhit-target updates were limited by newly-closed-M5 cadence when structural-only settings were enabled.
- TP1/TP2 hit handling did not force a same-cycle reward-path re-evaluation.
- AlertSignalRenderer generated visible text SIGNAL BUY/SELL plus alert kind as a presentation mirror of the last unified alert; it was not a second decision authority, but the label was semantically ambiguous.

Implementation decisions:
- H1/H4/D1/W1 are modeled as a directional anchor group.
- M30/M15 form the middle calibration group.
- M5 is the entry frame and M1 remains confirmation-only.
- A pure Core/Math TopDownCalibrationRule computes anchor direction/alignment, middle alignment, entry alignment and eligibility.
- Existing RequireHigherTfAgreement is reused as the activation boundary; no new public parameter is introduced.
- Actionable decisions require the explicit ENTRY CALIBRATED top-down stage while watch/prediction state can remain visible.
- Full panel renders are keyed by presentation state; the 500 ms heartbeat updates clock/live rows directly and no longer forces full layout work.
- A bounded 1-second structural pulse re-evaluates closed-M5 structure for protective SL/target progression.
- Successful TP1/TP2 partial-close confirmations force same-cycle target re-evaluation.
- Alert mirror label changes from SIGNAL BUY/SELL to ALERT BUY/SELL.

Evidence boundary:
- no guessed WaveTrend formula is introduced; exact source parity remains required before production integration.
- no empirical win-rate, false-signal or realized-RR improvement is claimed without target-terminal replay/outcome evidence.

Continuity:
- branch phase-9-1-topdown-evidence-runtime-responsiveness;
- PR #40;
- main baseline before phase 60749410917a4888e2a4c2ad90ce98c8f0916cfa;
- local pull is required only after merge.


## Phase 9.1 verification closeout — 2026-09-29

PR #40 merged to main as `b0e17e93551b3760d50bb38bb4725b9c523afddd`. Final pre-merge head `4530f4043195378f58727db8a395121c43abcd52` passed Runtime Acceptance, cTrader Compile/Build and Source/Architecture.

Implementation closeout:
- top-down directional anchor is now explicit in the Decision model and actionable gate;
- middle-frame quantitative alignment is required for ENTRY CALIBRATED;
- HTF-backed reward is required for TP1 on calibrated setups;
- heartbeat no longer calls full RenderPanel; live rows refresh directly;
- full panel render is keyed to canonical presentation state;
- bounded live structural pulse improves same-M5 protection/target responsiveness without using unclosed M5 OHLC;
- TP1/TP2 events force same-cycle next-target evaluation;
- ALERT BUY/SELL clarifies the presentation-only alert mirror.

Known boundary: no exact custom WaveTrend source was available in the current searchable repo/Library continuation, so no guessed formula was enabled. Empirical cTrader replay remains required.

Operator pull requirement: pull main before next local continuation.


## Phase 9.2 kickoff — 2026-09-29

User clarification: top-down must discover/calibrate strong higher-timeframe setups without discarding worthwhile lower-timeframe opportunities. The system should consider both paths simultaneously and retain low-risk/high-reward setups. The chart must support multiple visible opportunities without visual collisions. Price/position-style labels must have solid same-color backgrounds and readable contrast text, while existing line length geometry must remain unchanged.

Source recovery:
- the latest library ZIP was located as /indicator.zip;
- it contains wave-trend.txt with the user's CUSTOMWAVETREND source;
- the source uses RSI/MFI/RMI length 10, RMI momentum 5, exponential RMI up/down smoothing, average of RSI/MFI/RMI, EMA(4) smoothing, SMA(5) signal, -30/-40 OS and +30/+40 OB levels, then plots WAVE/SIGNAL shifted by -50.

Architecture decisions:
- OpportunityLane.Strategic = Phase 9.1 top-down calibrated opportunity.
- OpportunityLane.Tactical = LTF opportunity that passes its own quality/RR path.
- OpportunityLane.CounterHtfTactical = LTF opportunity against a strong HTF anchor, allowed only with stricter quality/RR.
- OpportunityLane.MicroReaction = reserved for strong live reaction opportunities.
- BUY and SELL tactical directions are assessed independently from M5 directional scores.
- Strategic candidate precedence is preserved when same-direction candidates are effectively colocated.
- Parallel visual objects use P + OPP + candidate ID namespaces and are removed deterministically when stale.

WaveTrend:
- WaveTrendEngine reimplements the recovered source cascade as a stateful per-Bars engine.
- WaveTrend evidence is attached to Frame and given bounded score influence.
- Exact source parity is treated as replay-validation until cTrader's built-in MFI numerical behavior is compared on the target terminal.

Execution boundary:
- Existing ExecutionCapacityRule still supports only one live executable plan.
- This phase intentionally supports parallel detection/presentation, not unsafe multi-position execution through the existing singleton _plan state.
- Phase 9.3 should create isolated plan contexts/registry, per-position protection state and independent broker mutation identities before allowing simultaneous auto execution of multiple lanes.

Panel/labels:
- parallel lane summary and WaveTrend state are visible in panel;
- compact chart labels now use opaque line-color boxes and automatic black/white text contrast;
- level line left/right calculation is untouched.

Evidence boundary:
- no win-rate/false-signal/realized-RR claim is made from code or CI;
- no guessed WaveTrend formula is introduced; the recovered source is the reference.

Continuity:
- branch phase-9-2-parallel-opportunities-wavetrend-visual-lanes;
- main base 02e0bb81b12af079935804a61d9d5ee3cb076e4e;
- local pull only after merge.


## Phase 9.2 verification closeout — 2026-09-29

PR #41 merged to main as `aedceba3f6d9e3791328f41c9e1fb01e2a474067`. Final PR head `c29608de510ebeb675c10c0438ede0cfcfbf59e5` passed Runtime Acceptance, cTrader Compile/Build and Source/Architecture.

Closeout:
- top-down Strategic lane preserved;
- Tactical BUY/SELL lanes independently evaluated;
- Counter-HTF Tactical lane requires stricter quality/RR;
- WaveTrend source recovered from latest ZIP and integrated as bounded evidence;
- line geometry preserved;
- parallel visual object IDs isolated;
- solid same-color label backgrounds and luminance-based text contrast implemented;
- singleton execution capacity deliberately unchanged to avoid unsafe multi-plan state corruption.

Next engineering phase:
- Phase 9.3 should create a true multi-plan registry with per-opportunity lifecycle, broker identity, protection state and event idempotency before any simultaneous auto-execution is enabled.

Empirical replay remains required for numerical WaveTrend parity and actual chart/UI behavior.

Operator pull requirement: pull main before next phase.


## 2026-09-29 — Phase 9.3 implementation

Branch: `phase-9-3-empirical-calibration`  
Pull request: #42

Implemented contextual empirical confidence calibration:
- removed the legacy direction-only adjustment from pre-context decision evidence;
- added hierarchical exact → lane/regime → directional fallback;
- added 50% shrinkage prior to reduce small-sample overreaction;
- applied calibration after Strategic/Tactical lane resolution;
- bound calibration metadata to executable plans and broker-closed outcomes;
- preserved pending-fill context where a pre-existing plan carried it;
- surfaced base/calibrated confidence and observed historical outcome diagnostics through the canonical visual snapshot and panel;
- added decision-contract coverage.

No new public parameter was introduced. No simultaneous execution path was added. Live cTrader replay remains required for empirical performance measurement.


## 2026-09-29 — Phase 9.5 corrective strategy-quality pass

Branch: `phase/9-5-actionable-signal-execution-coherence-v2`

Implementation is based on current `main` baseline `d245eec40749813a20b642fbd6e1ff0154fdaeca`.

Findings addressed:
- an independent Plan-activation alert path could diverge from the final actionable entry state;
- the chart had a directional M1 trigger marker in addition to the canonical entry arrow;
- ActionableNow protected plan creation but was not an explicit final market pre-trade gate;
- actionability did not sufficiently penalize range-extreme entries and materially adverse M5/M1 pressure;
- the multi-plan registry exposed candidates but its snapshot was not deterministically ranked.

Implemented:
- canonical `ACTION|` alert emitted once per closed M5 when the authoritative Decision is actionable; source priority is SMART → HIGH → CONFIRMED;
- Plan activation is lifecycle-only and no longer emits a second independent entry alert;
- directional entry arrow requires `ActionableNow` and is anchored to the current actionable M5 bar;
- M1 trigger visualization is a non-directional diamond;
- added pure `EntryTrapRiskRule` for range-extreme, M5/M1 adverse-momentum and opposing-divergence risk;
- automatic market pre-trade explicitly requires the same `ActionableNow` state used by plan creation and alerts;
- `TradePlanRegistry` now ranks snapshots by actionability, quality, RR, lane and recency and exposes `TryGetBest`;
- panel exposes final Entry Gate and Divergence state, while Early Analysis is explicitly labeled WATCH;
- Decision/Planning contract coverage added for entry-trap risk and registry ordering.

Safety boundary:
- parallel opportunity detection/presentation is supported;
- simultaneous broker execution remains single-plan/single-managed-identity until plan-scoped lifecycle/protection/broker mutation identities are isolated.

Verification:
- PR #45 was opened against the current `main`;
- Runtime Acceptance, cTrader Compile/Build and Source/Architecture workflows were triggered for the implementation head;
- merge is held until all required gates pass;
- target cTrader replay remains required for empirical signal timing, false-signal behavior and realized RR.

Operator pull:
- pull local `main` immediately after PR #45 is merged; the phase is ready for local replay.


## Phase 9.5 verification closeout — 2026-09-29

PR #45 merged into `main` as `46d7f40f685b96405eae8309141d5ef2ab557f76`.

Final verified head before merge: `347adabb0ae383c1e918d67996a858533625ef6c`.

Required gates on the final Phase 9.5 head:
- Runtime Acceptance PASS;
- cTrader Compile/Build PASS;
- Source/Architecture PASS.

The final Phase 9.5 implementation includes the canonical `ACTION|` alert path, ActionableNow-gated directional arrow, non-directional M1 trigger marker, turning-point/adverse-momentum/divergence protection, deterministic opportunity registry ordering, and shared market pre-trade actionability gate.

## Phase 9.5.1 corrective live-actionability coherence — 2026-09-29

Branch: `phase-9-5-1-live-actionability-coherence`
PR: #46

Corrective findings addressed after Phase 9.5 merge:
- `ActionableNow` and entry-quality fields could remain stale when the live execution model became unavailable or the decision was blocked;
- a pre-trade plan needed to remain aligned with the current decision direction and execution mode;
- a pending stop/limit-style plan must not be presented as a current market ACTION BUY/SELL state;
- live divergence quality should be refreshed from the current actionability evaluation rather than retained from an older state;
- the `ACTION` alert kind needed confirmed presentation semantics.

Implementation:
- live actionability now resets explicitly on blocked/unavailable state;
- existing pre-trade plans are re-evaluated from their own immutable setup geometry when the structural execution model is intentionally cleared;
- plan direction and execution mode must agree with the current actionability state;
- current market actionability is refreshed immediately before automatic market pre-trade eligibility;
- pending-style plans cannot produce current-market directional alert/arrow state;
- `ACTION` is classified as confirmed in the presentation mirror.

Verification closeout:
- PR #46 merged into `main` as `573f12ac380a8beace61db780082d5071db2defb`;
- final verified PR head before merge: `91b2a128e39bffff145fb84b2371e37057cc95b9`;
- Runtime Acceptance PASS;
- cTrader Compile/Build PASS;
- Source/Architecture PASS.
- target cTrader replay remains required for empirical signal timing, false-signal behavior, terminal rendering and realized RR.

Operator pull:
- pull local `main` now; this is the current continuation baseline.


## Phase 9.6 — Signal Quality & Visual Coherence — 2026-09-29

Status: VERIFIED COMPLETE on `phase/9-6-signal-quality-visual-coherence`.

Pre-merge gates passed on head `9e001449ee852289676aa29d1907174b96ce1b44`:
- Runtime Acceptance: PASS (run #988)
- cTrader Compile/Build: PASS (run #1172)
- Source/Architecture: PASS (run #1179)

Target cTrader replay remains required for empirical visual/signal-quality validation. Merge closeout follows after this verified documentation update.

Scope completed:
- added one final deterministic actionable-quality gate and applied it at both closed-bar decision creation and live-quote actionability refresh, preventing live refresh from reopening a marginal signal;
- tightened directional WATCH presentation to strong evidence only;
- tightened reaction/prediction presentation thresholds;
- tightened parallel opportunity presentation quality without introducing a second decision authority;
- changed the directional chart marker path to explicit UpArrow/DownArrow;
- changed the non-directional M1 trigger marker from Diamond to Circle;
- anchored compact label backgrounds to the label point, bounded their width/height, reduced compact font size to 8.5pt, and reused the canonical anchor for parallel opportunities;
- added deterministic decision-contract coverage for the final actionability quality rule.

Architecture/safety boundary:
- no new public parameters;
- no second decision/signal authority;
- no broker mutation ownership changes;
- singleton executable-plan model remains unchanged.

Acceptance boundary:
- Runtime Acceptance / Decision Contracts;
- cTrader Compile/Build;
- Source/Architecture;
- target cTrader replay remains required for empirical visual and signal-quality measurement.

Detailed phase record: `docs/PHASE-9-6-SIGNAL-QUALITY-VISUAL-COHERENCE.md`.


## Phase 9.7 — Regime-Aware No-Trade & Auto-Execution Hardening — 2026-09-29

Status: VERIFIED COMPLETE on `469f66bd2d461016c9283e0e4243ca429c694aa3`.

Pre-merge gates: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture PASS.

Phase 9.7 merge closeout: PR #49 merged into `main` as `954e5021648e43a11d0de08f35b7fc7aaa8d2125`.

Operator pull requirement: local `main` must be pulled before the next continuation.

Scope:
- introduced a single canonical RANGE/COMPRESSION quality rule;
- hard-blocked COMPRESSION;
- filtered weak RANGE signals and visual fallbacks;
- preserved only structurally strong range reversals or prior-range breakouts;
- required range quality for new automatic pending orders and regime-aware pending cleanup;
- forced fresh market suitability before automatic market and pending submission;
- enforced spread-to-stop-risk before automatic market entry;
- switched automatic market submission to bounded cTrader Market Range execution;
- kept broker-confirmed lifecycle and single execution capacity unchanged;
- forced opaque compact label backgrounds and kept the Phase 9.6 label geometry contract.

External execution research:
- cTrader supports market-range execution, where the allowed execution price range is supplied as pips;
- cTrader's order documentation notes that market orders are exposed to slippage and that market-range/limit approaches can reduce that exposure;
- server-side SL/TP remains part of the execution safety design.

Detailed phase record: `docs/PHASE-9-7-REGIME-AUTO-EXECUTION-HARDENING.md`.


## Phase 9.8 — Indicator Fusion & Trade Quality — 2026-09-29

Status: VERIFIED COMPLETE on `ae7abf3f169d0743e56be85f59f0d9ffa5081069`.

Final pre-merge gates: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture and project audits PASS.

Merge closeout pending.

Changes:
- added deterministic regime-aware indicator evidence fusion across trend, momentum and context groups;
- capped OSS consensus so raw indicator count does not become an artificial edge source;
- added directional conflict measurement and strong-divergence suppression inside indicator fusion;
- connected M5 indicator fusion quality/conflict to canonical Decision and Smart Quality;
- removed duplicate raw regime votes from DecisionScoreCalculator;
- added stronger auto-market and pending execution quality floors;
- exposed indicator fusion quality/conflict in the panel and decision reason;
- removed compact plan-label backgrounds completely and forced level-label text to white.

Research:
- current cTrader Algo documentation supports multi-timeframe bar retrieval through MarketData.GetBars, bounded Market Range execution through ExecuteMarketRangeOrder, volume normalization via Symbol.NormalizeVolumeInUnits, and server-side position protections.

Detailed phase record: `docs/PHASE-9-8-INDICATOR-FUSION-TRADE-QUALITY.md`.


Phase 9.8 merge closeout: PR #50 merged into `main` as `37cfd761bbb439d6e154315995664721b6c31740`. Local `main` must be pulled before the next continuation.

## 2026-09-29 — Phase 9.9 signal / execution / protection coherence

Branch: `phase/9-9-signal-protection-coherence`

Implementation scope:
- added a canonical `IndicatorActionabilityRule` and connected live ActionableNow evaluation to the current closed-M5 fusion snapshot;
- stale/weak/conflicted indicator fusion can no longer reopen a live actionable state;
- added server-side relative TP1/TP2/final protection for eligible automatic market, aggressive market, continuation-stop and reversal-limit entries;
- pending fills adopt the confirmed broker-side TP ladder;
- local partial-close and live target progression paths yield while the server-side ladder is active;
- broker protection sync no longer overwrites a confirmed advanced TP ladder with a simple single TP;
- chart level labels remain background-free and white through the common renderer.

Safety boundary:
- no new public parameter;
- no second decision authority;
- no second broker mutation authority;
- legacy execution path remains available when a valid server-side ladder cannot be constructed.

Detailed phase record: `docs/PHASE-9-9-SIGNAL-PROTECTION-COHERENCE.md`.

CI and target-terminal replay are required before the phase is considered verified complete.

### Phase 9.9 corrective hardening — 2026-09-29

Additional implementation pass on `phase/9-9-signal-protection-coherence`:

- corrected cTrader advanced-protection partial TP construction to the current documented `RelativeTakeProfitProtection(double volume, double distance)` API;
- changed live M5 indicator-fusion actionability to fail closed when the current fusion snapshot is present but unavailable/zero-quality;
- suppressed decision/reaction alert layers when a managed pending order or live position is authoritative;
- extended `tools/audit_runtime_ui.py` with regression checks for the server-ladder API shape, white/background-free labels, and execution-state signal-layer suppression.

Final verified code/documentation head: `98ac6f0844abb1171f70d7d5eaa50e8fdf623134`.

Final automated verification:
- Runtime Acceptance #1048 PASS;
- cTrader Compile/Build #1232 PASS;
- Source/Architecture #1239 PASS.

Target-terminal cTrader replay remains required for empirical signal timing, visual rendering, broker event behavior, partial-fill observation, duplicate-alert observation, protection recovery and realized trading outcomes.


## Phase 9.10 — Smart Auto-Trade / Auto-Order Protection & Accumulated Audit — 2026-09-29

Status: VERIFIED COMPLETE; PR #52 merged.

Branch: `phase/9-10-smart-auto-trade-protection-audit`
PR: #52
Verified phase head: `934d5aac56cfcd2769a743441d5260a2ad273997`

Implementation and accumulated findings:
- added deterministic `SmartBreakEvenRule` using structural risk, TP1 geometry, spread and existing break-even settings;
- propagated broker-owned server break-even through automatic market, aggressive market, continuation-stop and reversal-limit advanced protection paths;
- local polling break-even yields while broker-owned break-even is active;
- server-side TP ladder remains the mutation authority for eligible partial-TP configurations;
- all plan/signal level lines are Solid; Trigger/SL remain differentiated by thickness only;
- accumulated auto-trade/auto-order audit is wired into Source/Architecture CI and checks execution gates, broker protection ownership, local mutation de-duplication, chart-line style and parameter stability;
- Decision Contracts include deterministic smart break-even and TP1-collision coverage;
- two verifier regressions were found and corrected during this phase: legacy dotted-line expectation and an execution-boundary size edge case.

Verification evidence on the latest code-equivalent head:
- Runtime Acceptance #1066 PASS;
- cTrader Compile/Build #1250 PASS;
- Source/Architecture #1257 PASS, including all layered audits.

The latest audit-only refinement strengthens Solid-only checking across all chart renderers, so final CI must be green on the documentation-inclusive head before merge.

Next phase: Phase 9.11 — Automatic Execution Telemetry & Deeper SL/TP Coherence.
Operator pull: required after PR #52 is merged.


## Phase 9.11 — Signal Lifecycle, Quality Recovery & Alert Execution Coherence — 2026-09-29

Status: VERIFIED COMPLETE; merged into `main`.
Branch: `phase-9-11-signal-lifecycle`
PR: #53
Merge commit: `43101635e24ad15b77374472fc676a8c6fe591d6`
Verified phase head: `20ba01d4daa145f1118d3795277ed4d6f6a3bed3`

Implementation:
- bounded two-M5 lifecycle for stale pre-trade Plans and setup previews;
- stale pre-trade direction removed from canonical visual-direction resolution;
- deterministic one-dimension high-quality actionability recovery;
- legacy ALERT BUY/SELL chart mirror disabled and cleaned;
- blocked/restricted alerts made completely silent;
- popup defaults aligned to bottom-left, readable, bold presentation;
- all plan/prediction signal lines fixed to thickness 1 and Solid;
- broker submission confirmation/rejection/null-result telemetry added to the shared submission gate and exposed in auto-trade status;
- server SL/TP ladder now rejects wrong-side structural stops before broker-owned protection is created;
- accumulated audit extended for signal lifecycle, quality recovery, alerts, popup defaults, line thickness and execution telemetry.

Important finding:
The stale chart issue was caused by the pre-trade `_plan` branch in the visual snapshot builder treating any residual directional Plan as active. The blocked-alert sound was caused by `RESTRICT|` events entering the unified alert side-effect path.

Quality decision:
Recovery is intentionally narrow: only one near-threshold location/timing/price-position deficiency can recover, and only with stronger confidence, Smart Quality, MTF, evidence, structure and RR. Hard blockers remain unchanged.

Detailed record: `docs/PHASE-9-11-SIGNAL-LIFECYCLE-QUALITY-ALERTS.md`.

Verification:
- Runtime Acceptance: PASS;
- cTrader Compile/Build: PASS;
- Source/Architecture + accumulated audit: PASS;
- the Build gate includes the Decision Contracts execution.
- target-terminal cTrader replay: still required for empirical signal timing, false-signal behavior, popup rendering, and realized SL/TP outcomes.

Next phase: Phase 9.12 — broker outcome/recovery telemetry and historical signal lifecycle calibration.
Operator pull: required now from the final main documentation closeout commit.



## Phase 9.12 — Broker Outcome / Recovery Telemetry & Recent Lifecycle Calibration — 2026-09-29

Status: VERIFIED COMPLETE; merged into `main`.
Branch: `phase/9-12-outcome-recovery-calibration`

Implementation:
- added bounded `OutcomeObservation` history for broker-confirmed managed closes;
- centralized broker-close outcome registration and deduplicated history by position id;
- retained lane, regime, confidence and execution context for calibration-eligible plans;
- added realized-R and lifecycle-duration observations for post-trade diagnosis;
- added recent-first contextual empirical calibration over the latest 128 eligible observations, with existing dictionary fallback when recent history is sparse;
- added bounded submission telemetry history and retained the latest compatibility fields;
- recorded transitions into and out of `RecoveryRequired` through the same observational telemetry path;
- exposed recent outcome and broker-trace summaries in the panel;
- extended Decision Contracts and the accumulated whole-project audit.

Important findings:
- Phase 9.11's latest-only execution telemetry was diagnostically lossy because each new submission replaced the prior state;
- the Phase 9.3 in-memory calibration dataset had no recent-window preference, so older observations could dominate a changed market context;
- recovery plans created from reconstructed broker state must remain excluded from calibration because they do not carry the original decision context.

Safety:
- broker-confirmed position close remains the sole outcome source;
- no broker mutation path was added;
- no second decision or execution authority was introduced;
- public parameter count remains unchanged.

Verification:
- Decision Contracts within cTrader Compile/Build #1280: PASS;
- Runtime Acceptance #1096: PASS;
- cTrader Compile/Build #1280: PASS;
- Source/Architecture + accumulated audit #1287: PASS;
- Final verified branch head: `626618e7100b2e2cecf8a172d67ed49aee43345b`;
- Merge commit into `main`: `033bad1555fc7e2e1780c126d501020ddb7c7737`;
- target cTrader replay remains required for empirical validation.

Detailed record: `docs/PHASE-9-12-OUTCOME-RECOVERY-TELEMETRY-CALIBRATION.md`.

Next phase after verification: Phase 9.13 — target-terminal lifecycle replay and outcome calibration validation.
Operator pull: required now; pull `main` to the merge commit and subsequent documentation closeout.



## Phase 9.13 — Persistent Outcome Memory, Adaptive Risk & Optimization Routine — 2026-09-29

Status: VERIFIED COMPLETE; merged into `main`.
Branch: `phase/9-13-persistent-memory-safe-optimization`

Implementation:
- added persistent outcome memory using cTrader LocalStorage under `AccessRights.None`;
- scoped memory by symbol, chart timeframe and a deterministic fingerprint of key decision/risk configuration;
- retained at most 128 observations and rejected records older than 90 days on restore;
- hardened outcome serialization using invariant numerics and Base64 text fields;
- restored historical outcome aggregates before startup calculation;
- connected recent outcome history to the existing contextual calibration engine;
- added conservative recent-performance risk scaling using the latest 12 outcomes with an 8-observation minimum;
- ensured outcome-aware risk can only reduce the canonical suitability-derived risk and never increase it;
- exposed outcome risk scaling in the panel;
- added `tools/audit_optimization_readiness.py` to the permanent Source/Architecture routine;
- added Decision Contracts for adaptive-risk floor, minimum-sample, determinism and non-escalation behavior;
- no new public parameters and no second decision/execution authority.

Important design finding:
Persistent memory is useful only because the restored history is actually consumed. In this phase it directly affects recent contextual calibration and conservative risk scaling after restarts. Memory is evidence continuity, not predictive power by itself.

External-memory decision:
Current cTrader/.NET 6 supports safe local file operations in a designated algorithm folder and also provides LocalStorage for persistent cBot/indicator data. CFIP uses LocalStorage as the default safe persistence boundary, avoiding unrestricted FullAccess. A human-readable export file can be added later for offline analysis.

Optimization policy:
Every phase now includes an optimization-readiness audit. Actual parameter optimisation must be measured against out-of-sample/replay data and multi-objective risk metrics rather than a single win-rate target.

Safety:
- outcome history does not choose direction;
- outcome history does not submit/cancel/modify broker orders;
- recent wins never increase automatic risk above the suitability-derived baseline;
- telemetry off means outcome history cannot affect automatic risk;
- memory is configuration-scoped to avoid cross-version calibration contamination.

Verification:
- Decision Contracts within cTrader Compile/Build #1291: PASS;
- Runtime Acceptance #1107: PASS;
- cTrader Compile/Build #1291: PASS;
- Source/Architecture + accumulated audit #1298: PASS;
- final verified code head: `b742aae2dd1f94452f41743ad749374433dbc763`;
- Merge commit into `main`: `6246782125718af184568eb9339965e3f228c9a7`;
- target cTrader replay remains required for empirical validation.

Detailed record: `docs/PHASE-9-13-PERSISTENT-MEMORY-SAFE-OPTIMIZATION.md`.

Next phase after verification: Phase 9.14 — target-terminal replay, calibration/optimization measurement and evidence-driven parameter refinement.
Operator pull: required now; pull `main` to the current closeout commit.


## Phase 9.14 — Signal Evidence Integrity & Consensus Calibration — 2026-09-29

Status: VERIFIED COMPLETE; merged into main as PR #56.
Merge commit: 16e788f6b196afcfe2580908cbdb4dabc46cb5b.

Finding:
The canonical frame contribution path used relative bull/bear percentages without sufficiently preserving absolute frame strength and evidence coverage. This can inflate directional consensus from weak but dominant frames.

Implementation:
- added bounded absolute-strength modulation;
- added bounded evidence-coverage modulation;
- shrank weak directional shares toward 50/50;
- propagated the existing frame evidence count through the canonical adapter;
- added regression contracts for weak-frame suppression, strong-frame recovery, symmetry and determinism.

Safety:
- no direction authority moved;
- no broker mutation path changed;
- no hard trigger/RR/trap blocker was removed;
- no public parameters were added;
- public parameter count remains 552.

Empirical boundary:
This phase addresses a concrete consensus-calibration defect, but improved profitability, drawdown or win rate must not be claimed until target-terminal/replay evidence is collected.

Detailed record: docs/PHASE-9-14-SIGNAL-EVIDENCE-CALIBRATION.md.

Verification:
- Runtime Acceptance: PASS;
- cTrader Compile/Build: PASS;
- Source/Architecture + accumulated audit: PASS;
- Decision Contracts: PASS within build.

Next phase: Phase 9.15 — target-terminal replay/measurement and evidence-driven parameter refinement.
Operator pull: required now; pull main to the latest closeout commit.


## Phase 9.15 — Startup Responsiveness & Portable Long-Term History — 2026-09-29

Status: VERIFIED COMPLETE; merged into main as PR #57.
Merge commit: 365abb790a49e85ad58c87aa5a92f49d37b7f77c.

Finding:
Startup could remain in LOADING DATA because the critical readiness check required deep history and D1/W1 were allowed to contribute to the blocking async load count.

Implementation:
- lowered internal core readiness to M5 60, M15 50, M30 45, H1 40 and H4 36;
- made D1/W1 optional startup data;
- invalidated MTF context cache when optional data arrives;
- added explicit startup progress text;
- added append-only 90-day CSV outcome archives in the designated cTrader indicator folder;
- deferred archive import until after the first usable calculation seed;
- connected full archive aggregates to long-term contextual calibration;
- kept recent history as the first calibration source;
- added a dedicated startup/persistence audit to CI;
- recorded a permanent requirement that every future phase audit and optimize the complete Analysis -> Decision -> Signal -> Alert -> Execution -> Broker -> Protection -> Outcome -> Learning chain.

Safety:
- AccessRights remains None;
- no FullAccess;
- no history-delete path;
- no second signal or execution authority;
- no new public parameters.

Detailed record: docs/PHASE-9-15-STARTUP-PERSISTENT-HISTORY.md.

Verification:
- Runtime Acceptance: PASS;
- cTrader Compile/Build: PASS;
- Source/Architecture + accumulated audit: PASS;
- Startup/persistence audit: PASS;
- Decision Contracts: PASS within Build.

Next phase: Phase 9.16 — replay/measurement instrumentation and evidence-driven signal refinement.
Operator pull: required now; pull main to the latest closeout commit.


## Phase 9.16 — Signal Measurement, OB/FVG Location Fusion & Execution Safety — 2026-09-29

Status: VERIFIED COMPLETE; merged into main as PR #58.

Branch: `phase/9-16-signal-measurement-location-fusion`
PR: #58

Implementation:
- introduced a canonical location evidence rule;
- raised OB above standalone FVG for base location contribution;
- made high-quality OB+FVG overlap the strongest bounded single location feature;
- preserved evidence integrity by counting the correlated FVG/OB group as one evidence unit;
- added a closed-M5 signal evaluation trace with decision, MTF, indicator, WaveTrend,
  divergence, FVG/OB, actionability and Entry/SL/TP context;
- persisted traces to append-only 90-day `History/` CSV files and exposed the latest
  gate/reason in the panel;
- added `tools/analyze_signal_trace.py` for forward MFE/MAE and potential-missed
  opportunity diagnostics;
- added `ExecutionPlanGeometryRule` as a final automatic-market plan safety check
  immediately before broker mutation;
- extended Source/Architecture CI with a dedicated Phase 9.16 audit.

Important design findings:
- the remaining missed-signal problem should be diagnosed by rejection cohort rather
  than by blindly tightening/loosening global thresholds;
- OB+FVG is the strongest single location feature, but it must not bypass structure,
  liquidity, MTF conflict, trigger, RR, trap, spread, capacity or broker-protection gates;
- the long-term raw file archive remains canonical; LocalStorage remains bounded recent
  memory, avoiding unbounded duplication into the finite LocalStorage quota.

Verification boundary:
- Runtime Acceptance: passed on the initial PR head;
- cTrader compile and Source/Architecture exposed two implementation issues, both corrected:
  the new trace model was registered with domain-model isolation and the new location rule
  was linked into Decision Contracts;
- final rerun is required on the current documentation-inclusive head.

Target-terminal replay remains required for empirical validation of signal timing, false
signals, missed opportunities, broker behavior and realized outcomes.

Detailed record: `docs/PHASE-9-16-SIGNAL-MEASUREMENT-LOCATION-FUSION.md`.

Next phase: Phase 9.17 — target-terminal replay of Phase 9.16 traces and evidence-driven gate refinement.
Operator pull: required now; pull `main` to the verified Phase 9.16 closeout.


### Phase 9.16 closeout clarification — persistent memory path

The existing portable-memory bridge restores the bounded recent outcome cache from the matching History snapshot into Type-scoped LocalStorage when the LocalStorage key is missing. Long-term raw outcome/signal traces remain in History files and are intentionally not duplicated without bound into LocalStorage.


## Phase 9.17 — Exit Geometry, TP Progression & Protection Integrity — 2026-09-29

Status: VERIFIED COMPLETE; corrective hardening merged into main.

Finding:
The reported TP rollback had multiple interacting causes: live target selection did not require
the target to remain beyond the current market, target progression was disabled while the server
TP ladder was active, and actual-fill/recovery flows could reconstruct stale exit geometry.

Implementation:
- canonical LiveExitGeometryRule;
- forward-only live TP candidate selection;
- monotonic live TP2/TP3/TP4 enrichment;
- transactional actual-fill exit reconciliation;
- no unsafe legacy post-fill ladder rebuild fallback;
- server ladder progression before TP1, after TP1, and final-target continuation after TP2;
- mandatory monotonic broker TP mutation;
- canonical protective-stop geometry check;
- broker minimum TP-distance included in live target spacing;
- Decision Contracts and dedicated exit-geometry audit.

Safety:
- no new public parameters;
- no second decision authority;
- no second execution authority;
- broker-confirmed state remains authoritative;
- raw history archives remain non-destructive.

Empirical boundary:
Target-terminal/replay remains required to confirm the user's observed rollback is eliminated in
practice and to measure realized exits, slippage, protection rejection, and continuation behavior.

Detailed record: docs/PHASE-9-17-EXIT-GEOMETRY-PROGRESSION.md.

Next phase: Phase 9.18 — target/protection measurement and evidence-driven exit refinement.
Operator pull: required now; pull `main` at merge commit `af4ef4edb5ce032c4a71feaf2cc0be1203f4992b`.

## Phase 9.17 automated verification closeout — 2026-09-29

Status: VERIFIED COMPLETE at source/contract level; target-terminal replay still required.

Verification:
- Runtime Acceptance #1193: PASS
- cTrader Compile/Build #1377: PASS
- Source/Architecture + accumulated audits #1384: PASS
- Decision Contracts: PASS within Build
- Phase 9.15 startup/persistence audit: PASS
- Phase 9.16 signal measurement audit: PASS
- Phase 9.17 exit geometry audit: PASS
- Verified head: `c027a983081102ace0353d064219a5aad739ed94`

Key engineering result:
The TP rollback was traced to stale/non-live-aware target reuse plus a progression path that stopped
when server-side TP protection was active. The new live geometry layer requires TP to remain beyond
the executable quote and forbids target regression; the SL side has the matching protective geometry
rule.

Routine closeout:
- phase docs updated;
- acceptance matrix updated;
- user-priority and workflow invariants updated;
- no public parameter increase;
- no second decision/execution authority introduced.

Next phase: Phase 9.18 — target/protection measurement and evidence-driven exit refinement.
Operator pull: after PR #60 merge, pull main at the verified merge commit.


## Phase 9.17 corrective numerical hardening — 2026-09-29

Status: VERIFIED COMPLETE at source/contract level; PR #61 merged into main; target-terminal replay remains required.

Second-pass findings and fixes:
- centralized live TP minimum-forward distance across broker and lifecycle paths;
- made percentage broker distance quote-direction aware;
- made server-side ladder adoption trust broker LastTakeProfit.Price;
- enforced live quote geometry on every server-ladder target before mutation;
- fixed post-fill SL ratchet to compare prior stop against structural candidate;
- capped recovery target fallback by MaximumRewardRR;
- recomputed later-stage RR from current geometry instead of cached stage RR;
- fail-closed on NaN/Infinity for exit/risk calculations;
- prevented stale server-ladder state from alone satisfying protection synchronization.

Verification on the latest automated head:
- Runtime Acceptance #1196: PASS;
- cTrader Compile/Build #1380: PASS;
- Source/Architecture + accumulated audits #1387: PASS;
- Decision Contracts: PASS within cTrader Compile/Build;
- Phase 9.17 exit geometry audit: PASS.

The local container could not reach github.com for an independent local build, so verification is based on the repository's GitHub Actions runs above. Target-terminal replay is still required for actual broker/server-side behavior, timing, slippage and empirical confirmation of the reported TP rollback.



## Phase 10 — MTF Scenario Identity, Level Presentation, Runtime Logging & Routine — 2026-09-29

Status: IMPLEMENTED on phase branch; CI verification required before merge.

Findings:
- Phase 9.17.1 is already merged and its exit/risk hardening passed source/contract verification.
- The previous parallel-opportunity layer supported multiple lanes, but did not have first-class source-timeframe identity and its same-direction proximity deduplication could collapse distinct timeframe opportunities.
- Main plan labels did not explicitly communicate that the canonical signal is MTF, and SL/TP labels did not show pip distance from Entry.

Implemented:
- added ScenarioId and SourceTimeframe to TradeOpportunityCandidate;
- added independent closed-frame scenario generation for M5/M15/M30/H1/H4/D1/W1;
- preserved same-direction scenarios from different source timeframes;
- deterministic priority/capping for visible scenarios;
- added shared scenario label formatting and (MTF) main-plan tags;
- added Entry-relative SL/TP pip distances to labels;
- increased minimum horizontal line-to-label gap and aligned the parameter default/minimum to that contract;
- expanded visible opportunity capacity to default 6 / maximum 8;
- added unified CFIP_RuntimeLog_*.csv for decision, prediction, scenario, execution and meaningful auto-state events;
- added tools/analyze_runtime_log.py for basic forensic analysis and geometry anomaly detection;
- added persistent ROUTINE.md;
- documented the phase in docs/PHASE-10-MTF-SCENARIOS-ROUTINE-LOGGING.md.

Safety boundary:
Independent timeframe scenarios remain opportunity information. They do not independently authorize automatic orders. Automatic market and pending execution continue through the canonical decision/plan and existing safety gates.

Prediction boundary:
Prediction is now explicitly captured in runtime logs and covered by the recurring routine. No accuracy claim is made until replay/outcome data is available.

Verification boundary:
Source/architecture, runtime acceptance, parameter-use, UI cleanup, scenario coexistence and runtime-log smoke verification must pass on the phase branch. Terminal replay remains required for actual broker behavior and empirical prediction/exit measurements.

Operator action after merge: pull local main and run terminal-level scenario/label, market-order, pending-order and exit-management replay tests.


## Phase 11 — Economic News Guard & Reference-Indicator Reassessment — 2026-09-29

Status: IMPLEMENTED on phase branch; CI verification required before merge.

Reference review:
- `indicator.zip`: WaveTrend reference plus empty TPO placeholder.
- `indicators.zip`: economic calendar, WaveTrend, FVG and empty volume-profile placeholder.
- WaveTrend is already implemented internally as a deterministic closed-bar engine; no duplicate runtime copy was added.
- FVG geometry and mitigation in CFIP already cover the useful 2-bar/3-bar structural behaviors in the reference. The reference opening-gap family is kept separate to avoid correlated double-counting.
- The economic reference was useful and exposed the real gap: CFIP's old news block was manual `NewsBlackoutUtc` only.

Implemented:
- Internet access enabled for calendar retrieval;
- weekly XML economic feed parsed into a cache;
- relevant currency inference plus configurable extra currencies;
- high-impact pre/post blackout and optional medium-impact block;
- stale feed fail-closed for auto trading;
- pending cancellation before high-impact events;
- optional active-position pre-news close, default OFF;
- Timer-based refresh so decision/tick path uses cached data after initial load;
- news state surfaced in the execution panel;
- NEWS_RISK event telemetry added;
- automated News Guard audit added to source CI.

Incident relevance:
The old implementation could not automatically know that a scheduled economic release was imminent. Therefore a signal could be valid technically, open, and then be hit by release volatility without the news gate having blocked that entry. This phase closes that gap for future trades.

Important runtime boundary:
Even a news-aware system cannot guarantee an open position will not hit its stop during a fast release. Broker-side gap/slippage can cross the stop before any client-side reaction. Terminal replay is required to measure the actual behavior.


## Phase 11.1 — Auto Execution, Indicator Identity, Level Presentation & History Discoverability — 2026-09-29

Status: VERIFIED COMPLETE; merged into main as `8be58c289566711f8b80caa741012edc94b8a523`.

Implementation:
- explicit CFIPIndicator identity;
- same-M5 auto-plan actionability latch removal;
- History location marker and startup diagnostic;
- automatic execution block diagnostics;
- plan label separation hardening.

Verification:
- Source/Architecture: PASS
- Runtime Acceptance: PASS
- cTrader Compile: PASS

Next phase: Phase 11.2 — Analysis / Signal / Execution Freshness & Panel Heartbeat.
Operator pull: required at phase boundary.


## Phase 11.2 — Analysis / Signal / Execution Freshness & Panel Heartbeat — 2026-09-29

Status: VERIFIED COMPLETE; merged into main as `8102cda74059da88e5be224a76a64dd0683ec366`.

Implementation:
- obsolete IndicatorAttribute constructor removed;
- dead `_lastAutoPlanTriggerM1` removed;
- final market, aggressive and pending freshness refreshes added;
- horizontal and vertical plan-label separation hardened;
- canonical signal-pipeline panel diagnostics added;
- lightweight pulsing processing lamp added;
- permanent deep phase routine added to ROUTINE.md;
- phase-specific audit added to CI.

Detailed record: `docs/PHASE-11-2-ANALYSIS-SIGNAL-EXECUTION-HEARTBEAT.md`.

Next phase: Phase 11.3 — execution rejection forensics, missed-actionable cohorts and evidence-driven threshold refinement.
Operator pull: required after final verified phase merge.

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

Next phase after verification: scenario-aware execution materialization and deeper automatic-order/multi-scenario broker policy, driven by the Phase 11.3/11.4 telemetry rather than guesswork.


## Phase 11.5 — Scenario-Aware Execution Materialization & Submission Isolation — 2026-09-30

Status: VERIFIED and MERGED to main.

Merge commit: `fd29b955c557df680e40d8fe4151955600160408`.
Final code commit verified before merge: `cef178da5f0359ef2c0800376b5b9beddf295424`.

Implementation:
- added canonical ScenarioExecutionPolicy separating candidate eligibility from broker execution authorization;
- kept independent timeframe scenarios explicitly observe-only;
- materialized the canonical plan against an exact scenario identity and Entry/SL/TP1 geometry;
- extended SubmissionAttemptIdentity with scenario-scoped retry/circuit identity;
- propagated scenario identity into automatic market, aggressive, pending Stop and pending Limit submission paths;
- preserved scenario identity in execution telemetry and exposed the active execution scenario in Auto Trading diagnostics;
- added runtime contract coverage and tools/audit_phase_11_5.py;
- wired the Phase 11.5 audit into Source/Architecture CI;
- preserved the certified single-position broker capacity and existing execution/lifecycle authorities.

Whole-chain review:
Analysis -> Decision -> Signal -> Alert -> Execution -> Broker -> Protection/Lifecycle -> Outcome -> Learning
was rechecked for MTF, OB/FVG/OB+FVG, WaveTrend/divergence, indicator fusion, Entry/SL/TP,
RR, market/aggressive/pending execution, broker confirmation, lifecycle and telemetry.

Evidence boundary:
No live threshold tuning, profitability claim or accuracy claim was made. Target-terminal
replay remains required for broker behavior and empirical signal-quality/outcome measurement.

Next phase:
Formal scenario execution policy design driven by observed runtime telemetry, without
promoting independent timeframe broker mutation or multi-position capacity until separately certified.


## Architecture Gate — Local cBot Separation Roadmap — 2026-09-30

Status: **DOCUMENTED / IMPLEMENTATION NOT STARTED**

Repository baseline audited for this decision: `4eed8aa29759e372b02a0726b18ac7020bdb349a`.

Decision:

- The current Indicator remains the analysis/decision/scenario/trade-plan authority.
- A dedicated local cBot becomes the sole broker execution and live account/lifecycle authority.
- Only execution capabilities that require broker/account mutation are separated.
- Mixed source modules are split by responsibility instead of copied wholesale.
- No Cloud infrastructure is introduced now.
- A small platform-neutral contract boundary is introduced only where required for Indicator → cBot communication and later portability.
- The existing single-position capacity and one execution authority invariant remain unchanged.
- Independent timeframe scenarios remain observe-only until separately certified.

Audited execution owners include:

- `Trading/Execution/BrokerMarketOrderMutation.cs`;
- broker pending/order placement and cancellation owners;
- `BrokerPositionCloseMutation.cs`;
- `BrokerStopLossMutation.cs`;
- `BrokerTakeProfitMutation.cs`;
- `BrokerProtectionCoordinator.cs`;
- broker identity/managed-position ownership;
- `SubmissionAttemptIdentity` / `SubmissionGate`;
- broker-confirmed lifecycle/recovery paths;
- account-dependent execution risk and capacity.

Important non-transfer boundary:

- `Trading/Execution/ExecutionPlanPreparation.cs` remains analytical/executable-plan preparation and does not become a cBot analysis engine.
- `ScenarioExecutionPolicy.cs` remains the Indicator's scenario eligibility/materialization authority.
- Analysis, Planning, UI, alerts and learning remain Indicator-owned.

New authoritative document: `docs/CBOT-SEPARATION-ROADMAP.md`.

Implementation order:

`CBOT-0 → CBOT-1 → CBOT-2 → CBOT-3 → CBOT-4 → CBOT-5 → CBOT-6 → CBOT-7`

No source code behavior was changed in this roadmap-only planning step.

Verification for this documentation gate:

- repository source inventory reviewed from GitHub;
- execution/planning/identity/risk/lifecycle ownership reviewed;
- Phase 11.4 and 11.5 boundaries reconciled;
- no production C# modified by this step.

Next phase: **CBOT-0 — Boundary inventory and execution-authority freeze**.

Documentation merge: PR #69 was squash-merged to `main` as `161f1972513e83685ea2364070d081af823fa7c6`.

Roadmap continuation correction: `docs/ROADMAP.md` was aligned in follow-up commit
`b92cc367083b0830278d799f49e29d21b83d1779` so the master continuation points to CBOT-0.

Operator pull: **required** before beginning CBOT-0, so the local checkout contains the authoritative roadmap.


## Roadmap Hardening — Local cBot Separation Gate — 2026-09-30

Status: DOCUMENTATION-ONLY hardening; no production C# behavior changed.

Verified repository baseline: `3b0107ff0436a4f9aaa65cbefb6194312d52bf2f`.

Corrections and gates added to Track 12A:

- blocking CBOT-Preflight before contract implementation;
- corrected executable-plan preparation path to `Trading/Execution/ExecutionPlanPreparation.cs`;
- explicit method/field/helper dependency-closure rule;
- explicit split-only handling for mixed protection, live-management, server-side TP and account-risk modules;
- signal freshness, expiry, revision ordering and instance-scope requirements;
- authoritative cBot execution-control state with Indicator presentation unable to bypass the broker boundary;
- legacy executor allowed only as a repository parity oracle, never as a second live engine;
- fail-closed behavior for missing/stale/incompatible Indicator/cBot state;
- final zero direct-broker-mutation and zero-hidden-fallback acceptance gate.

Verification completed for the documentation revision:

- repository source paths and project structure rechecked against `main`;
- current main head verified as `3b0107ff0436a4f9aaa65cbefb6194312d52bf2f`;
- no production C# files changed;
- implementation remains blocked at CBOT-Preflight / CBOT-0.

Next implementation phase: **CBOT-Preflight**, then **CBOT-0 — Boundary inventory, dependency closure and execution-authority freeze**.
Operator pull: required after this documentation revision is merged.


## Final Roadmap Readiness Audit — 2026-09-30

Status: documentation-only; ready for CBOT-Preflight / CBOT-0.

Final hardening removed duplicate migration-rule sections, corrected numbering, synchronized master/detail ordering, made parameter-by-parameter ownership mandatory for mixed execution groups, made clean solution/project/package wiring part of acceptance, and clarified cBot ownership of final broker-normalized volume.

No production C# behavior changed.


## Final Ordering Correction — 2026-09-30

Confirmed authoritative order for local cBot separation: **CBOT-0 → CBOT-Preflight → CBOT-1 → CBOT-2 → CBOT-3 → CBOT-4 → CBOT-5 → CBOT-6 → CBOT-7**. CBOT-0 is source-only inventory and can start without terminal interaction; CBOT-Preflight is the no-trade target-terminal blocking gate before Contracts.

No production C# behavior changed.


## Roadmap Ordering Recheck — 2026-09-30

Detailed and master separation roadmaps are aligned to the authoritative order **CBOT-0 → CBOT-Preflight → CBOT-1 → CBOT-2 → CBOT-3 → CBOT-4 → CBOT-5 → CBOT-6 → CBOT-7**. Preflight is a no-trade blocking capability gate after repository inventory, not a prerequisite for starting CBOT-0.

## Claude Review Remediation Roadmap — 2026-09-30

Status: DOCUMENTATION-ONLY; blocking Track 11.6 established.

Three supplied review prompts were cross-checked against the current repository. The resulting remediation track is documented in `docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md`.

Important review corrections:
- A4 latest-sweep causal claim is only partial; active/unbroken swing semantics and ATR-relative penetration are the valid parts.
- A5 synchronous news I/O/stale mapping are real issues; the prompt's implication that AccessRights.None inherently blocks HTTP is not accepted as a source-based defect. Current official cTrader documentation states AccessRights.None is sufficient for network functions.
- B9 historical rendering is expensive on host-bar changes, not literally every Calculate call.
- C5 item 4 is stale against current source: `_plan.Stop` is updated after successful BE mutation, not on rejection.

No production C# behavior changed.
Next phase: CR-0 — final audit closure.

## CR-0 — Claude Review Audit Closure

Status: **complete — 2026-09-30**.

Scope:
- audited all 33 supplied findings: A1–A12, B1–B12 and C1–C9;
- mapped each finding to exact current source owners and relevant methods;
- resolved prior AUDIT FIRST items into confirmed or explicitly bounded/deferred work;
- established cBot-boundary-sensitive source ownership without moving mixed files wholesale;
- defined canonical threshold, session/time and outcome/accounting semantics;
- created the mandatory manual cTrader verification matrix.

Detailed evidence:
- `docs/CLAUDE-REVIEW-CR0-AUDIT.md`
- `docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md`
- `docs/CBOT-SEPARATION-ROADMAP.md`

Repository truth correction:
- current parameter source tree: 29 files / 27 logical parameter groups;
- current `[Parameter]` declarations: **566**;
- the older documented count of 534 is stale and has been corrected in the remediation/master roadmap.

Review corrections preserved:
- A4 latest-sweep causality narrowed;
- A5 AccessRights.None is not treated as a source-proven HTTP blocker; current official cTrader documentation confirms network access is available under None;
- B9 historical rendering frequency corrected to host-bar rebuilds;
- C5 BE-rejection overwrite claim rejected because current source only commits the plan-stop change after successful broker modification.

Production behavior changed by CR-0: **none**.

Next implementation phase: **CR1.1 — Session/EOD and period-reference correctness**.
Track 12A cBot separation remains blocked until CR-FINAL.

Operator pull required at this completed phase boundary:
`git pull --ff-only origin main`


## CR1.1 — Session/EOD and period-reference correctness

Status: **complete — 2026-09-30**.

Implemented:
- introduced canonical `SessionWindowRule` for session membership, session ranges and EOD boundaries;
- unified SessionAllowed, session suitability and session presentation semantics;
- fixed overnight and start==end session handling;
- corrected previous D1/W1 target references to use the latest canonical fully closed index;
- corrected the D1 pivot suitability reference;
- replaced unbounded EOD auto-close with a five-minute bounded post-boundary cleanup window;
- included managed pending orders in EOD cleanup;
- protected positions opened after the previous session boundary from immediate EOD closure;
- made EOD completion reporting depend on broker-state re-read rather than mutation-request success alone;
- added deterministic runtime contracts for the new session and closed-period rules.

Relevant commits:
- `067ee861ffc28f627b2de4b37915d8c94479a78b` — canonical session rule
- `a0e1b8f720594fc9947c0d8ce38f82977d53cabd` — unified session evaluator
- `4d4279ded3adf903778a8ef758481269e7ff5c97` — removed duplicate session semantics
- `91c216e648d4184536700e1f622d259bc40aa7ff` — unified session range
- `0311f3bcda1dd593691ef91a941ea908d42caf64` / `0f803eab65509b71e5035d5b6e9e5bb36188232a` / `617577244ddce0c9318bc911e9a8ef43494bfab3` — corrected closed D1/W1 consumers
- `d51dafbed42a1dc5cb3f081c811b683a67191a6a` — bounded EOD cleanup
- `fecbfec1507d6e3316b9791fe426e462653a4edc` / `1a08be083479ce62f6f0412d6473d32248a2ab26` — deterministic contracts
- `107fa474e4c6bb8d6b910b294a82b5c3a8d27d95` — canonical session presentation

Verification:
- Runtime acceptance contracts: PASS (run 1339)
- cTrader compile: PASS (run 1523)
- Source and architecture checks: PASS (run 1530)

Routine audit:
- Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning coupling reviewed.
- Performance/code cleanliness reviewed; no synchronous network/file path or duplicate session decision authority introduced.

Operator pull required at this completed phase boundary:
`git pull --ff-only origin main`

Next phase: **CR1.2 — Daily-loss lock and stable accounting basis**.


## CR1.2 — Daily-loss lock and stable accounting basis — 2026-09-30

Status: **IMPLEMENTATION COMPLETE; target-terminal verification remains required.**

Root-cause hardening:
- replaced the volatile daily equity baseline with persisted account-scoped UTC-day state;
- made daily loss a deterministic pure rule with explicit realized/floating/cash-flow accounting;
- corrected an integration gap where missing transaction facts could otherwise allow cash-flow effects to be ignored;
- made incomplete/Non-finite accounting inputs fail closed;
- preserved a single daily-loss gate across Market, Aggressive and Pending automatic-entry paths;
- preserved the no-auto-close behavior when the daily-loss lock is reached;
- added deterministic contracts for recovery, deposits, withdrawals, threshold boundary, incomplete facts and invalid numeric input.

Whole-chain routine audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed for this phase. The change introduces no second decision authority, no second execution authority, no position-capacity expansion and no additional broker mutation path.

Verification basis:
- official cTrader API documentation rechecked for Account equity/unrealized P/L, account Transactions, HistoricalTrade and LocalStorage persistence;
- pure daily-loss contract coverage updated in the Runtime Contracts tool;
- target-terminal cTrader Compile/Build, Runtime Acceptance, broker trading-day boundary, restart persistence and cross-instance behavior remain mandatory verification items.

Next phase: **CR1.3 — News guard correctness and non-blocking refresh**.


## CR1.3 — News guard correctness and non-blocking refresh — 2026-09-30

Status: **IMPLEMENTATION COMPLETE; target-terminal verification remains required.**

Root-cause hardening:
- removed the synchronous economic-news HTTP path from initialization;
- switched calendar transport to cTrader Http.GetAsync;
- restricted refresh initiation to the existing Timer heartbeat;
- added in-flight request gating, request-generation identity and a bounded 30-second transport timeout;
- ignored late responses from timed-out/abandoned generations;
- retained the last validated calendar cache after refresh failure;
- added explicit DISABLED / NEVER_LOADED / HEALTHY / STALE / BLOCKING_EVENT state semantics plus REFRESHING and bounded error diagnostics;
- added configurable symbol/index/commodity/crypto currency mapping with normalized broker-symbol matching;
- preserved one shared NewsBlocked consumer across decision, market, aggressive and pending safety paths.

Verification basis:
- cTrader HTTP documentation rechecked for Http.GetAsync and AccessRights.None network support;
- deterministic runtime contracts cover feed state transitions and currency mapping;
- News Guard source audit now forbids synchronous Http.Get/Http.Send in production source and checks async generation/in-flight contracts;
- target-terminal tests remain mandatory for real feed availability, timestamp interpretation, startup responsiveness and live news block/cancellation.

Whole-chain routine audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed. No trading threshold, RR floor, position capacity or execution authority was changed.

Next phase: **CR1.4 — Closed-bar cycle ordering and waiting-for-data state**.


## CR1.4 — Closed-bar cycle ordering and waiting-for-data state — 2026-09-30

Status: **IMPLEMENTATION COMPLETE; target-terminal verification remains required.**

Implemented:
- pre-decision broker reconciliation before each newly closed M5 decision cycle;
- same-cycle broker reconciliation gate for decision/restriction/actionable alerts;
- startup seed alignment with the same broker boundary;
- deterministic BUILDING DATA / WAITING FOR CLOSED M5 / WAITING FOR MTF DATA / READY states;
- bounded readiness probe cadence and throttled waiting-state panel refresh;
- lifecycle/recovery/news/protection/outcome supervision kept alive while analysis waits;
- waiting-state path explicitly excludes plan creation and every execution submission path;
- runtime acceptance contracts and a dedicated cycle source audit added.

Verification:
- Runtime acceptance: PASS;
- cTrader compile: PASS;
- Source/architecture checks: final head verification required before merge;
- official cTrader documentation rechecked for indicator Calculate/IsLastBar and closed-bar semantics;
- target-terminal verification remains mandatory for broker event timing and data-readiness behavior.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed. No threshold, RR floor, position-capacity or execution-authority change was introduced.

Next phase: **CR1.5 — repeated work / synchronous persistence**.


## CR1.5 — Hot-path/cache/logging performance — 2026-09-30

Status: IMPLEMENTATION COMPLETE; target-terminal verification remains required.

Implemented:
- buffered Runtime Log / Outcome / Signal Trace archives with bounded Timer flush; startup-only history marker file write remains outside the archive writer by design;
- deferred explicit LocalStorage flush/reload to the Timer heartbeat;
- cached archive prefixes and parameter fingerprint;
- closed-bar FVG/OB candidate cache with deterministic Bars/index invalidation;
- event/mutation-driven broker-state dirty invalidation with bounded one-second refresh;
- deterministic buffer and broker-refresh contracts plus dedicated source audit;
- preserved historical 90-day archive partitioning and did not delete prior history.

Verification:
- Runtime Acceptance: PASS on final CR1.5 head;
- cTrader compile: PASS on final CR1.5 head;
- Source/Architecture final-head verification remains required before merge;
- actual cTrader persistence/timing/performance behavior remains a target-terminal test item;
- container clone/build was unavailable because github.com DNS resolution was unavailable in this execution environment.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed. No threshold, RR floor, capacity or execution-authority change was introduced.

Next phase: CR1.6 — FVG quality discrimination.

## CBOT-0 — Boundary Inventory & Execution-Authority Freeze — 2026-09-30

Status: **VERIFIED COMPLETE**

Implemented:
- added tools/audit_cbot_boundary.py to inventory every direct broker mutation call in production and prevent new calls outside the frozen broker-mutation owners;
- recorded the current direct broker API owner matrix and Indicator/cBot responsibility split in docs/CBOT-0-BOUNDARY-INVENTORY.md;
- added source-usage-domain analysis for execution-related public parameters so mixed analytical/execution controls become explicit SPLIT refactors instead of duplicated controls;
- wired the new audit into Source/Architecture CI;
- kept current production execution behavior unchanged;
- did not create the Contracts/cBot projects and did not introduce a second executor.

Verification:
- Source/Architecture: PASS (`109870832998`)
- Runtime Acceptance: PASS (`109870832972`)
- cTrader Build/Compile: PASS (`109870832958`)

Verification boundary:
- CI proves the 562-parameter current baseline remains intact;
- direct broker mutations must stay inside the current seven broker-mutation owner files;
- lifecycle/account ownership markers must remain present;
- no duplicate public parameter declaration may exist.

Whole-chain routine audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning remains unchanged in this no-behavior-change phase.

Next phase: CBOT-Preflight.
Operator action after merge: git pull --ff-only.
## CBOT-Preflight — Local cTrader Host Capability Gate — 2026-09-30

Status: IMPLEMENTED; target-terminal verification remains required.

Implemented:
- verified the supported custom-indicator reference model against current cTrader documentation;
- added a no-trade probe Indicator and cBot under preflight/;
- added a static preflight audit and CI integration;
- explicitly banned broker mutation, reflection, chart scraping and alternative IPC transports in the preflight kit;
- documented the target-terminal startup-order and stale/unavailable fail-closed matrix;
- preserved the rule that the final CFIP structured signal/provider surface is created in CBOT-2, not silently duplicated here.

Verification:
- repository Source/Architecture preflight gate is implemented;
- actual target-terminal P1–P8 execution is still required and must not be simulated or claimed from repository CI.

Whole-chain routine audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning remains unchanged. No production broker behavior was changed.

Next phase: CBOT-1 after target-terminal preflight acceptance.

Operator action after merge: git pull --ff-only.

## CR2.7 — WaveTrend mathematical correctness — 2026-09-30

Status: **VERIFIED COMPLETE**

Root-cause hardening:
- replaced the previous partial MovingAverageType behavior with deterministic implementations for all currently exposed cTrader MA enum values used by WaveTrend;
- corrected DEMA/TEMA warm-up so dependent EMA state chains are populated from their first valid seed instead of consuming default array values;
- corrected HMA final weighting so the contiguous raw-HMA window is consumed exactly once with the final WMA weights;
- corrected WaveTrend readiness to include RSI/MFI/RMI dependencies, both smoothing layers and previous-signal stability;
- added WaveTrend state invalidation for Bars HistoryLoaded/Reloaded and history-prefix/count changes;
- documented that WaveTrend MFI uses cTrader TickVolumes and is not an assertion of exchange-level real volume;
- preserved all public WaveTrend parameter names, types and DefaultValues;
- kept Decision, Signal, Auto-Trade, Auto-Order, broker mutation and capacity authority unchanged.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS;
- CR2.7 static audit: PASS;
- existing CR2.6 and project-wide routine audits: PASS.

Verified implementation head: `181b238a548280fc01a67fb1e3ba8a617f63e42a`.
Merged via PR #92, merge commit `b021f56c4b75331fb52127547e006f2f8bb287c4`.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed. The phase changed only WaveTrend analytical math/readiness/state invalidation and did not bypass any execution safety gate.

Performance/code-cleanliness:
- MA kernels were split into bounded partial modules to satisfy the existing production module-size architecture limits;
- repeated calculations remain incrementally cached by WaveTrend engine state;
- history invalidation clears both WaveTrend arrays and MA calculator state to prevent stale reuse.

Manual boundary:
- target-terminal replay remains required to verify platform-specific numerical parity for every MA type and to measure any empirical effect on signal quality;
- no accuracy, win-rate, realized-RR or profitability claim is made from CI alone.

Next phase: **CR2.8 — Historical rendering semantics and cost**.


## CR2.8 — Historical rendering semantics and cost — 2026-09-30

Status: **VERIFIED COMPLETE**

Implemented:
- bounded historical scan to a fixed maximum of 500 closed bars;
- cached historical presentation results by closed-bar timestamp;
- replaced index-based historical chart identity with timestamp-based presentation identity;
- invalidated historical presentation state on Bars HistoryLoaded/Reloaded and detected history-shape changes;
- synchronized drawings by ownership instead of deleting/recreating the complete historical set on every ordinary render;
- explicitly kept historical arrows presentation-only, with no alert, plan, execution, lifecycle mutation or outcome authority;
- added deterministic runtime contracts and dedicated CR2.8 static architecture audit;
- moved the helper presentation model to a top-level file to preserve repository architecture limits;
- removed unused historical-renderer imports.

Root-cause boundary:
- B9 was confirmed as a bounded-cost/repeat-work issue at host-bar historical rebuild frequency, not an every-Calculate rendering path.

Verification:
- Source/Architecture: PASS on final code head 18a0d14035b30249bac69f0315c1ad143af33704;
- Runtime Acceptance: PASS, run 1585;
- cTrader Compile: PASS, run 1769;
- CR2.8 static audit: PASS;
- accumulated CR2.1–CR2.7 audits: PASS.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning reviewed. No signal threshold, RR/SL/TP, OB/FVG, WaveTrend, structure/divergence, Auto Trading, Auto Orders, capacity or broker mutation authority changed.

Performance/code cleanliness:
- maximum historical analytical work is bounded;
- per-timestamp cache prevents repeat analysis for unchanged closed bars;
- cache is trimmed to the bounded display window;
- history lifecycle invalidation prevents stale reuse;
- no network/file persistence or broker mutation is introduced into historical rendering.

Manual target-terminal boundary:
- history prepend/load-more, reconnect/reload, chart-index remapping and visual responsiveness still require real cTrader verification;
- historical arrows must be observed to remain presentation-only and never trigger alerts/plans/orders/outcomes;
- no empirical accuracy or profitability claim is made from CI.

Phase record: `docs/PHASE-CR2-8-HISTORICAL-RENDERING.md`.

Next phase: **CR2.9 — Structural stop, divergence and rejection guardrail refinement (B10/B11/B12).**


## CR2.9 — Structural stop, divergence and rejection guardrails — 2026-09-30

Status: **VERIFIED COMPLETE**

Implemented:
- preserved the existing fail-closed unknown structural-timeframe behavior;
- extracted structural-stop reward-path bonus and preferred-risk balance into a pure owner without tuning current live coefficients;
- centralized divergence minimum quality, conflict margin, ATR excursion floors, RSI/WaveTrend deltas, recency boosts and quality-score components;
- centralized rejection/doji body and wick thresholds and unified meaningful-body semantics;
- added deterministic runtime contracts for B10/B11/B12;
- added and wired tools/audit_phase_2_9.py;
- preserved public parameters, signal/RR policy, execution authority and single-position capacity.

Verification:
- Source/Architecture: PASS, final branch head 8e51396c89498b84ac569b29517d4ba2ba6c8f45;
- Runtime Acceptance: PASS, run 1597;
- cTrader Compile: PASS, run 1781;
- CR2.9 static audit: PASS;
- accumulated CR2.1–CR2.8 audits: PASS.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning reviewed.
No new execution authority, broker mutation path, capacity, RR floor or signal gate was introduced.

Performance/code-cleanliness:
Pure rules are isolated in Core/; no UI/network/broker I/O was added; duplicated divergence arithmetic was removed from the analyzer; rejection/doji body logic has one owner; structural-stop scoring arithmetic is centralized.

Important evidence boundary:
CR2.9 does not claim that structural-stop scoring is empirically superior, nor that divergence/rejection changes improve accuracy or profitability. Any future coefficient/threshold tuning requires replay/outcome evidence.

Phase record:
docs/PHASE-CR2-9-STRUCTURAL-DIVERGENCE-REJECTION.md

Next phase: **CR3.1 — Live invalidation and false-signal semantics (C1/C2).**

## CR3.1 — Live invalidation and false-signal semantics — 2026-09-30

Status: VERIFIED COMPLETE

Implemented:
- closed-bar-stable invalidation on canonical closed M5 results;
- confirmed structural swing candidate invalidation;
- explicit broker-close success handling with rejected-exit recovery;
- centralized successful _lastExitM5 bookkeeping;
- explicit soft adverse-R safety flag with DefaultValue=true;
- canonical FalseSignalAdverseR validation and protected-stop coherence;
- deterministic runtime contracts and tools/audit_phase_3_1.py;
- reconciled parameter inventories/audits to the intentional 568 public parameters.

Verification:
- Source/Architecture: PASS, run 1808;
- Runtime Acceptance: PASS, run 1617;
- cTrader Compile: PASS, run 1801;
- CR3.1 static audit: PASS;
- project-wide routine/optimization/integrity audits: PASS.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning reviewed. No new cBot code, execution authority, capacity or signal/RR tuning was introduced.

Performance/code cleanliness:
- repeated intra-bar invalidation work was removed from the invalidation path;
- canonical rules isolate C1/C2 arithmetic from the cTrader host;
- structural invalidation no longer performs a rolling min/max scan;
- no network/file I/O or new mutable business-state cache was introduced.

Evidence boundary:
CI does not prove target-terminal broker timing, live network rejection behavior, restart/reconnect reconciliation or profitability/accuracy. Any future threshold tuning remains replay/outcome gated.

Phase record: docs/PHASE-CR3-1-LIVE-INVALIDATION.md

Next phase: CR3.2 — Decision gate and early prediction semantics (C3/C4).



## CR5.4 / E4 — Pending-order post-fill absolute SL/TP reconciliation — 2026-10-01

Status: **VERIFIED COMPLETE**

Root cause:
- successful pending Stop/Limit placement cleared `_plan`, while the executable
  Entry/SL/TP geometry remained only in `ExecutionIntent`;
- `PendingFilledHandler` could therefore rebuild the live plan from the eventual
  broker fill and allow relative broker protection to become the effective
  geometry after slippage/gap;
- `PositionOpened` could also arrive before `PendingFilled` and reconstruct
  managed state too early.

Implemented:
- preserved an absolute pending-plan snapshot from `ExecutionIntent`;
- fail-closed pending placement when the absolute snapshot cannot be constructed;
- reconciled the actual broker fill against the preserved absolute plan before
  final managed-plan adoption;
- retained broker-confirmed SL when it is more protective and TP when it is more
  progressive;
- rebuilt Advanced/server-side TP protection from the reconciled absolute ladder
  and actual fill price;
- protected the pending snapshot from `PositionOpened` event-order inversion;
- cleared stale snapshot state on pending cancellation;
- kept failed reconciliation in `RecoveryRequired`;
- added deterministic positive/negative fill-divergence, BUY/SELL symmetry,
  broker-protection, rejection and idempotency contracts;
- added `audit_phase_5_4.py`;
- reconciled the accumulated CR2.5 lifecycle-ordering audit with the new pending
  snapshot invariant.

Verification on final implementation head
`6d89f010fa6d292d201ef79e37af5b162438f854`:
- Source/Architecture: PASS — run `36795710379` / workflow #2096;
- Runtime Acceptance Contracts: PASS — run `36795710374` / workflow #1905;
- cTrader Compile: PASS — run `36795710377` / workflow #2089.

Safety:
- no public parameter name/type/default changed;
- no RR/confidence/stop/target/execution threshold tuned;
- no second decision or broker-execution authority introduced;
- broker-confirmed state remains authoritative.

Manual cTrader boundary:
- actual Stop/Limit fill-price divergence;
- broker-side final SL/TP and Advanced Protection ladder behavior;
- rejection timing;
- restart/reconnect;
- empirical signal-quality/profitability.

Next phase: **CR5.5 / E5 — Parallel-scenario computation/candidate ownership and MicroReaction safety**.

Operator action after merge: `git pull --ff-only`.


## CR7.1 / G1 — Broker protection must never increase live position risk — 2026-10-01

Status: **VERIFIED COMPLETE**

تأیید می‌کنم — the G1 safety finding was confirmed against the live protection call chain and corrected without tuning trading thresholds.

Implemented:
- split existing-stop directional health from new-stop market/minimum-distance validation;
- preserved current protective SL when a near-market stop is still correctly sided;
- retained `ProtectionProgressionRule` as the only healthy-stop replacement guard;
- hardened BoundPlanProtection and BrokerProtectionStateEvaluator to the same Core owner;
- completed `TargetObstacleCacheKey.GetHashCode()` and corrected the project-integrity audit false-positive by scoping duplicate signatures to containing type;
- added deterministic G1 Runtime Acceptance coverage and `audit_phase_7_1.py`.

Verification:
- Source/Architecture PASS — #2262;
- Runtime Acceptance Contracts PASS — #2071;
- cTrader Compile PASS — #2255;
- merged PR #138 as `b8144c2f1edc62730b7a0723be3746afe6353851`.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed. No second decision/execution authority or unrelated behavior change was introduced.

Performance/code-cleanliness:
- the correction removes an architectural audit false-positive instead of weakening the duplicate-method gate;
- existing-stop checks are now a direct Core predicate rather than reusing the wrong market-distance semantics;
- no new hot-path loop or unbounded state was introduced.

Manual boundary:
Actual cTrader `ModifyStopLossPrice`, broker minimum-distance behavior and restart/reconnect remain manual acceptance.

Next phase: **CR7.2 / G2 — Retest adverse-momentum semantics and rejection telemetry.**


## CR7.2 / G2 — Retest adverse-momentum semantics and rejection telemetry — 2026-10-01

Status: **VERIFIED COMPLETE**

تأیید می‌کنم — G2 was completed without changing Retest thresholds, defaults or execution authority.

Implemented:
- canonical Core trap threshold ownership;
- deterministic TRAP_ADVERSE_M5 / TRAP_ADVERSE_M1 / TRAP_EXTREME / TRAP_DIVERGENCE reasons;
- bounded pre-zone/post-zone Retest diagnostics;
- preserved legacy F6 trap-rule compatibility contract;
- isolated Retest/adverse calculations from oversized validation modules;
- accumulated G2 static audit.

Verification:
- Source/Architecture PASS — #2283;
- Runtime Acceptance Contracts PASS — #2092;
- cTrader Compile PASS — #2276;
- PR #142 merged as `f983d2fd7eb0baced4b5ff40988e6294b3f5bd28`.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed. No second decision/execution authority or unrelated trading behavior was introduced.

Performance/code-cleanliness:
- validation module size was kept below the repository architecture ceiling by isolating Retest/adverse helpers;
- no new unbounded state or hot-path scan was introduced;
- canonical policy ownership reduced duplicated threshold literals.

Manual boundary:
Target-terminal Retest intrabar/zone timing and empirical signal-quality/profitability validation remain manual.

Next phase: **CR7.3 / G3 — Display parameter truth for plan-line thickness/style.**


## CR7.3 / G3 — Display parameter truth for plan-line thickness/style — 2026-10-01

Status: **VERIFIED COMPLETE — PR #143 merged to `main` as `6c572643cfc6b9a4ee1083e300ec13607dd2774c`**

Implemented:
- fixed the hidden forced-one clamp in `PlanLineRenderer.ResolvePlanLineThickness`;
- added canonical `PlanLinePresentationRule.ResolveThickness`;
- preserved the public `Level Line Thickness` parameter and its 1..3 contract;
- preserved `LineStyle.Solid`;
- added deterministic Runtime Acceptance coverage for 1/2/3 and safe bounds;
- added `audit_phase_7_3.py` to the accumulated source-check chain;
- added the phase-specific root-cause/verification document.

Verification:
- Source/Architecture: PASS — #2295;
- Runtime Acceptance Contracts: PASS — #2104;
- cTrader Compile: PASS — #2288.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed. G3 is presentation-only and introduces no new decision or execution authority.

Performance/code-cleanliness:
- removed the incorrect renderer-level forced-one clamp;
- kept thickness normalization as a bounded, single-owner presentation rule;
- no new hot-path loop, state store or market-data scan was introduced.

Safety/manual boundary:
- no public parameter name/type/DefaultValue changed;
- no RR/confidence/SL/TP/execution threshold changed;
- target-terminal cTrader visual verification of thickness 1/2/3 and Solid style remains manual.

Next phase: **CR7.4 / G4 — Panel execution/protection state semantics.**

## CR7.4 / G4 — Panel execution/protection state semantics

Status: **IMPLEMENTATION COMPLETE — repository verification pending.**

Implemented:
- canonical ExecutionProtectionPanelStateRule;
- explicit Auto Trade / Auto Orders Disabled / Armed / Ready / Active / Blocked / RecoveryRequired semantics;
- broker-protection Off / NoLivePosition / Protected / RecoveryRequired semantics;
- panel rows now consume the canonical state owner;
- Auto Trade panel color/state no longer infers operational readiness from decision/reaction;
- broker SL/TP and server TP-ladder state are represented explicitly;
- panel cache invalidation includes execution/protection state;
- deterministic G4 runtime contract and accumulated static audit are wired.

Safety:
- no public parameters/defaults changed;
- no strategy/RR/risk threshold tuning;
- no new decision, execution or broker-mutation authority.

Manual boundary:
- cTrader target-terminal panel/reconnect/protection timing remains manual.

## CR7.4 / G4 closeout — Panel execution/protection state semantics — 2026-10-01

Status: **VERIFIED COMPLETE — PR #144.**

Completed:
- canonical ExecutionProtectionPanelStateRule;
- explicit execution states Disabled / Armed / Ready / Active / Blocked / RecoveryRequired;
- broker-protection states Off / NoLivePosition / Protected / RecoveryRequired;
- overview/detail rows consume the same state owner;
- Auto Trade state/color no longer infers operational readiness from decision/reaction evidence;
- shared panel execution/protection snapshot avoids repeated broker enumeration;
- deterministic G4 runtime contract, dedicated audit and accumulated audit coverage.

Verification:
- Source/Architecture PASS — #2313;
- Runtime Acceptance Contracts PASS — #2122;
- cTrader Compile PASS — #2306.

Safety/manual boundary:
- no public parameter/default or trading threshold changes;
- no new decision/execution/broker-mutation authority;
- target-terminal panel/protection timing and restart/reconnect remain manual.

Next phase: **CR7.5 / G5 — scope definition required before implementation; no documented G5 scope is currently present.**



## CR7.5 / G5 — Panel execution/protection state freshness and broker-read minimization — 2026-10-01

Status: **VERIFIED COMPLETE — PR #145 merged to `main` as `e2674b9800159ba1266639ad96a374f622aff555`**

Scope/root cause:
- G4's canonical panel execution/protection snapshot was invalidated unconditionally by
  `BuildPanelPresentationKey`, which could repeat managed Position/Pending enumeration and
  protection evaluation across unchanged panel refresh attempts.

Implementation:
- removed presentation-key-driven invalidation;
- broker dirty events invalidate the panel snapshot immediately;
- due broker refresh invalidates it as the one-second freshness backstop;
- runtime/lifecycle state changes invalidate centrally;
- direct block-reason, recovery and server-TP-ladder state changes are guarded so stale
  derived panel state cannot survive those mutations;
- added deterministic G5 Runtime Acceptance coverage and `audit_phase_7_5.py`.

Safety/performance:
- no public parameter name/type/default changed;
- no RR/confidence/entry/SL/TP/risk/execution threshold changed;
- no decision/execution/broker-mutation authority changed;
- unchanged broker refresh semantics remain authoritative.

Verification:
- Source/Architecture: PASS — #2321;
- Runtime Acceptance Contracts: PASS — #2130;
- cTrader Compile: PASS — #2314;
- PR #145 merged to `main` as `e2674b9800159ba1266639ad96a374f622aff555`.

Target-terminal panel responsiveness, broker event timing and reconnect/reload remain manual.

Operator action after merge:
- run `git pull --ff-only` on local `main`.

## CR7.6a / G6A closeout — Execution panel presentation freshness — 2026-10-01

Status: **VERIFIED COMPLETE — PR #146 functional implementation HEAD `b0b19c1a3e7e65110dc1b64d4f8a3bf555b7c54e`.**

Completed:
- identified the G5 presentation-identity gap without undoing G5 broker-read minimization;
- centralized execution-facing panel identity in `ExecutionPanelPresentationIdentityRule`;
- included Auto Trading state/reason, execution telemetry path/state, active scenario, market-suitability score/state/reason and break-even diagnostic in the panel identity;
- made canonical Auto Trading state/reason changes invalidate the G4 execution/protection snapshot only when they actually change;
- kept telemetry-only presentation changes from forcing broker-state snapshot invalidation;
- added deterministic Runtime Acceptance coverage and `audit_phase_7_6a.py`;
- reconciled accumulated G5 continuity audit so historical phase transitions remain valid as the roadmap advances.

Verification:
- Source/Architecture: **PASS** — #2327;
- Runtime Acceptance Contracts: **PASS** — #2136;
- cTrader Compile: **PASS** — #2320.

Safety:
- no public parameter name/type/DefaultValue changed;
- no RR/confidence/entry/SL/TP/risk/execution threshold changed;
- no decision/execution/broker-mutation authority changed;
- no new broker enumeration or unbounded cache introduced.

Manual boundary:
- cTrader target-terminal panel refresh timing, scenario/suitability/break-even visual refresh, reconnect/reload and empirical behavior remain manual.

Operator action:
- after PR #146 is merged, run `git pull --ff-only` on local `main`.

Next phase: **CR7.6c.**

## CR7.6b / G6B closeout — Execution-control truth and single UI authority — 2026-10-01

Status: **VERIFIED COMPLETE — implementation head `7832f47c05117f66686adf659fd92670dcb14ba8`.**

Completed:
- restored AUTO TRADE / AUTO ORDERS as status-only panel presentation;
- removed the legacy in-panel execution mutation path;
- centralized execution-control presentation/read-only policy in Core;
- preserved `EnsureExecutionRuntimeState()` as the canonical settings → runtime synchronization boundary;
- reconciled accumulated architecture, project-integrity and CR3.4 audits;
- added deterministic G6B runtime and static audit coverage.

Verification:
- Source/Architecture PASS — #2339;
- Runtime Acceptance Contracts PASS — #2148;
- cTrader Compile PASS — #2332.

Safety/manual boundary:
- no public parameter/default or trading threshold changes;
- no RR/confidence/entry/SL/TP/risk tuning;
- no new decision/execution/broker-mutation authority;
- target-terminal click behavior, startup/reload synchronization, responsiveness and reconnect/reload remain manual.

Operator action after merge:
- run `git pull --ff-only` on local `main`.

Next phase: **CR7.6c.**



## CR8.2 / H2 closeout — Top-Down absolute strength — 2026-10-01

Status: **VERIFIED COMPLETE — PR #150; final implementation HEAD 675283b82e345a86b1a6094e7b7ad66ec3632b84.**

Root cause:
- relative top-down alignment could be high even when the aligned timeframe quality was weak;
- weak opposing lower-timeframe evidence could be treated as a conflict without sufficient absolute strength.

Completed:
- added canonical dominant-direction absolute strength to top-down group evaluation;
- required alignment plus absolute strength for a strong HTF anchor;
- required sufficient absolute strength for opposing mid/entry conflict blocking;
- exposed H2 diagnostics through Decision and panel;
- added deterministic Decision Contracts and accumulated static audit;
- resolved CS0414 by turning the existing execution-toggle flag into an actual re-entrancy guard.

Verification:
- Source/Architecture PASS — 36892859244;
- Runtime Acceptance Contracts PASS — 36892859151;
- cTrader Compile PASS — 36892859090.

Next phase: **CR8.3a / H3-A**.


## CR8.3a / H3-A — Skender settings ownership — 2026-10-01

Status: **VERIFIED COMPLETE — PR #151 implementation head `b19e3366b0115d79a6e6a61b79af310ac64bbdd7`.**

Root cause / audit finding:
- fixed production Skender settings were distributed through `OssIndicatorParameters`, mixing immutable indicator defaults with cache/history/safety policy;
- this made the settings ownership boundary less explicit and allowed accumulated OSS-audit rules to depend on the wrong owner.

Implemented:
- added immutable Core `OssIndicatorSettings.Default`;
- centralized MACD signal, Bollinger, MFI, Stochastic, SuperTrend, Aroon, CCI and Parabolic SAR fixed settings;
- migrated all affected Skender adapters;
- preserved parameter-driven RSI and MACD fast/slow semantics;
- added Planning/Runtime deterministic default-preservation contracts;
- added and accumulated `audit_phase_8_3a.py`;
- reconciled `audit_phase_4_4.py` with the new owner;
- added phase record `docs/PHASE-CR8-3A-H3-A-SKENDER-SETTINGS.md`.

Verification:
- Source / Architecture: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile / Build: PASS.

Safety/performance audit:
- public parameter name/type/DefaultValue unchanged;
- no trading threshold, RR, confidence, risk, entry, SL or TP tuning;
- no decision or execution authority changed;
- no new broker enumeration, runtime I/O or unbounded cache;
- H3-B is explicitly reserved for warm-up-window, bounded computation, cache and numerical-parity work.

Next phase: **CR8.3b / H3-B**.

## CI-09 — Decision engine mathematical audit — 2026-10-02

Branch: `phase/ci-09-decision-mathematical-audit`

Implemented a narrow mathematical-integrity correction without policy retuning:
- exact 50/50 consensus is neutral;
- non-finite consensus inputs fail closed;
- adaptive conflict penalties cannot create a side bias at equal score;
- canonical decision score components are exposed in `DecisionScoreSnapshot` for provenance;
- non-finite frame/advanced numeric contributions fail closed;
- deterministic Decision Contracts reconstruct the score and verify symmetric modifiers;
- `audit_phase_ci_09.py` is accumulated after CI-08.

No public parameter/default, confidence/RR/SL/TP/risk/execution threshold or broker authority was changed. Empirical signal-quality remains a target-terminal/replay concern.

## CI-09 closeout — 2026-10-02

PR #164 merged to `main` as `58d0ef85b2960ac9c706aad120d5f89ffd377946`.
Final verification: Source/Architecture #2560 PASS; Runtime Acceptance Contracts #2369 PASS; cTrader Compile #2553 PASS. The accumulated CI-09 static audit passed. Next phase: **CI-10 — Gate/threshold semantic audit**.

## 2026-10-02 — CI-10 Trigger / Trigger-Lifecycle Audit

CI-10 implementation moved trigger-threshold and trigger-lifecycle semantics behind canonical Core owners.

Completed:
- added `TriggerThresholdRule` for Live/Precision trigger-score resolution and fail-closed readiness;
- added `TriggerLifecycleRule` for active-M5 M1 confirmation eligibility, M5/direction reset identity, one-time confirmation recording and TriggerReady propagation;
- prevented live runtime from accepting an M1 bar belonging to the already-closed decision M5;
- added monotonic `TriggerRuntimeState.ConfirmationRevision` and explicit confirmation expiry/reset behavior;
- corrected M1 runtime frame-refresh state ordering so the previous closed-M1 index is compared before it is replaced;
- added deterministic CI-10 Runtime Acceptance coverage;
- added `tools/audit_phase_ci_10.py` and accumulated it immediately after CI-09 in Source/Architecture workflow;
- reconciled the accumulated architecture audit with the canonical trigger-threshold owner;
- updated ROADMAP.md, CONTINUATION-STATE.md and the CI-10 phase record.

Verification:
- implementation head `699cc1dd9c6e58a6df9bbd730b75ee37f7ce93f7`;
- Source/Architecture #2571: **PASS**;
- Runtime Acceptance Contracts #2380: **PASS**;
- cTrader Compile #2564: **PASS**;
- PR #165 merged to `main` as `ed8fadb2e8af2ca5e0250c72de955e5659d8bdaa`.

No public parameter/default, confidence/RR/SL/TP/risk/execution threshold or broker authority was changed by CI-10.

Next phase: **CI-11 — Entry geometry and signal-timing audit**.


## 2026-10-02 — CI-11 Entry Geometry / Signal-Timing Audit

CI-11 canonicalized entry geometry and signal timing across execution-mode resolution, live actionability, plan preparation, market validation, pending/presentation snapshots and M1 trigger timing.

Completed:
- added the canonical `EntryGeometryRule` / `EntryGeometrySnapshot` composition;
- removed duplicated late/anchor/zone equations from downstream consumers;
- preserved structural zone tolerance across plan and presentation state;
- restored canonical price-snapshot ownership in `MarketEntryValidation` after the accumulated CI-00 audit exposed the regression;
- added causal M1 confirmation timestamping and `ACTIONABILITY_TIMING` telemetry;
- added deterministic CI-11 contracts and accumulated static audit wiring.

Verification: implementation head `b7bbf5375a062d82c2883360d3f4e8b61de0ee45`; Source/Architecture #2589 PASS; Runtime Acceptance Contracts #2398 PASS; cTrader Compile #2582 PASS; PR #166 merged to `main` as `56554a7cacbdd17d75e2ab38dcad8692594d138b`.

No public parameter/default, confidence/RR/Entry/SL/TP/risk/execution threshold or broker authority was changed by CI-11. Target-terminal timing and empirical signal-quality validation remain manual boundaries.

Next phase: **CI-12 — Structural SL audit**.

## CI-15 closeout — 2026-10-02

Status: **VERIFIED COMPLETE — PR #172 merged to `main`.**

CI-15 removed the validated-intent-to-broker-submission seam across Automatic Market, Aggressive Market, Pending Stop and Pending Limit. A canonical `ExecutionIntentGeometryRule` now owns the final intent-side Entry/SL/TP and pip projection; `ExecutionIntentValidation` re-checks that projection; broker submission consumes the exact validated intent; and bounded execution telemetry records the exact intent geometry for downstream broker reconciliation.

Repository verification on final implementation HEAD `52679d319ecd03d5bbf0358cf319e0d4e96e9b2a`:
- Source/Architecture #2707: **PASS**;
- Runtime Acceptance Contracts #2516: **PASS**;
- cTrader Compile/Build #2700: **PASS**;
- Planning Contracts: **PASS** (`Planning contracts OK`).

Manual acceptance remains required for target-terminal timing, broker-specific fill/slippage behavior, reconnect/reload and empirical signal/outcome validation.

Next phase: **CI-16 — Deterministic replay, latency and counterexample suite.**


## CI-15 merge closeout — 2026-10-02

PR #172 was merged to `main` as `8aa4a7dc3fee97d6ce233c26b2114e844b36a669`. The CI-15 implementation head was `52679d319ecd03d5bbf0358cf319e0d4e96e9b2a`; Source/Architecture #2707, Runtime Acceptance #2516 and cTrader Compile #2700 passed on the final implementation head, with Planning Contracts reporting `Planning contracts OK`. The next phase is **CI-16 — Deterministic replay, latency and counterexample suite**.

## CI-16 deterministic replay / latency / counterexample suite — 2026-10-02

Implemented the blocking CI-16 replay layer as a verification-only extension of Runtime Acceptance Contracts. The suite runs the same 16 required counterexample fixtures twice, compares complete deterministic traces, records causal/reference/quote timing plus Decision → Trigger → Entry → SL → TP1..TP4 → intent → actionability → alert → execution-attempt → fill timestamps, and compares authoritative versus submission geometry fingerprints.

Canonical existing owners are reused for Entry geometry, Risk/Reward, final ExecutionIntent geometry, Trigger lifecycle and fill acceptance. No live calculation path, broker mutation path, public parameter, confidence/RR/Entry/SL/TP/risk threshold or execution policy was changed.

The dedicated audit_phase_ci_16.py is accumulated immediately after CI-15.

CI-16 verification results on implementation HEAD `1c292d3162f6085869238029fe599630ab3ca3a9`: Source/Architecture workflow #2715 (36950516204) PASS; Runtime Acceptance Contracts #2524 (36950516284) PASS; cTrader Compile/Build #2708 (36950516224) PASS; accumulated CI-16 audit PASS. Target-terminal timing, broker event ordering, actual slippage/fills, panel timing and empirical outcome quality remain CI-17/manual boundaries.

Next phase: **CI-17 — Target-terminal cTrader validation.**

## CI-17 closeout package — 2026-10-02

Status: **REPOSITORY PACKAGE COMPLETE AND MERGED — PR #174, merge commit `194ab90030f668ea3a42f0e709d42ca3238383ae`.**

Implementation head `9a71b1dbac09e41759458a404ce4675dc4972f92` passed Source/Architecture run `36951458606`, Runtime Acceptance Contracts run `36951458326`, and cTrader Compile run `36951458297`.

The target-terminal package strengthens the existing strict no-trade preflight path, records real terminal timing/quote/bar diagnostics, provides compilable preflight project separation, accumulates `audit_phase_ci_17.py`, and documents the full manual acceptance matrix.

Manual acceptance remains open for the real cTrader terminal/broker: M1/M5 initialization and timing, Bid/Ask execution geometry, market/pending fill and slippage, cancellation/expiration, restart/reconnect, panel/chart responsiveness and Decision → Plan → Execution synchronization. Repository CI is not used as a substitute for these facts.

Operator action: run `git pull --ff-only` on local `main`.

Next gated phase after manual evidence: **CI-FINAL — Full-stack Calculation Integrity Certification**.



## MTF-P1 — Primary M15/H1 Signal Layer + Panel Separation — 2026-10-02

Implemented the requested signal presentation hierarchy without creating a second decision or execution authority. M15 and H1 are now the primary visible signal sources; closed M5 provides local tuning and the existing closed-M1 trigger provides optional timing confirmation. M15 and H1 retain separate identities and can be presented simultaneously.

M15/H1 source-frame OB/FVG evidence remains attached to each candidate and participates in display priority. Existing non-M5 execution policy remains observe-only, and the candidate Entry/SL/TP projection remains canonical M5 geometry until a separately verified source-timeframe planning phase.

The Indicator chart-panel AUTO TRADE / AUTO ORDERS quick-control row was removed. Canonical execution status rows were retained. Bottom-left/bottom-right panel positions now keep a fixed 100px clearance so the separate cBot surface can occupy the lower chart area.

No public parameter or trading threshold changed. Target-terminal timing, panel responsiveness/spatial separation and empirical signal-quality effects remain manual acceptance boundaries.

Phase record: `docs/PHASE-MTF-P1-PRIMARY-M15-H1-PANEL.md`.


## MTF-P2 — Primary M15/H1 Location Evidence: OB/FVG Provenance — 2026-10-02

The primary M15/H1 signal layer now carries the source timeframe's actual FVG and Order Block quality plus explicit OB+FVG confluence. Existing LocationEvidenceRule remains the sole scoring owner, so no duplicate location score or hidden threshold was introduced.

Primary source location quality/confluence is used for deterministic presentation priority. The panel exposes the evidence for each primary candidate so M15/H1 source levels are auditable instead of being represented only by a generic scenario quality value.

No public parameter or trading threshold changed. Non-M5 execution remains observe-only for these independent primary candidates, and target-terminal/empirical validation remains required.

Phase record: `docs/PHASE-MTF-P2-PRIMARY-LOCATION-OBFVG.md`.


## MTF-P3 — Primary M15/H1 Provider Scenario Identity Cohesion — 2026-10-02

Started the provider identity cohesion phase. The existing provider envelope currently
uses a read-only single-snapshot bridge, while M15/H1 primary scenarios coexist in the
Indicator registry. The phase therefore corrects identity traceability without creating
a second decision or execution authority.

Completed implementation and repository verification. Source/Architecture, Runtime Acceptance and cTrader Compile/Build all passed on implementation HEAD `5f6a9873a27b3b1edfa139ab21d19c1b5faa6bdc`.

Completed changes:
- provider SourceTimeframe is mapped from the exact scenario candidate;
- canonical plan scenario resolution is reused without selecting an unrelated fallback;
- canonical source timeframe is carried into provider execution-intent identity;
- pending Stop/Limit paths no longer create independent hard-coded scenario identifiers;
- deterministic runtime/static acceptance coverage is being accumulated.

No public parameter, threshold, RR, Entry, SL, TP, confidence, risk or broker mutation
authority has changed.

Phase record: `docs/PHASE-MTF-P3-PRIMARY-PROVIDER-IDENTITY.md`.


## Panel Geometry Correction — 50px Bottom Clearance + Hidden Restore Position — 2026-10-02

Completed the requested panel geometry correction on a dedicated phase branch. Repository Source/Architecture, Runtime Acceptance and cTrader Compile/Build gates passed on implementation HEAD `b63f82e1d1fca9ef3af2d7fbbe34e779099ab7d7`. The previous
100px bottom clearance is reduced to 50px. The hidden restore button now uses a dedicated
50px bottom clearance for BottomLeft/BottomRight, keeping it aligned with the new panel/cBot
boundary instead of falling directly against the chart bottom edge.

The accumulated P1 panel audit was made value-agnostic so a deliberate geometry correction does
not look like a historical regression. No public parameter, strategy logic or broker authority
was changed.

Phase record: `docs/PHASE-PANEL-CLEARANCE-RESTORE-POSITION.md`.


## Indicator Naming + cBot Launch + MTF Panel Direction Correction — 2026-10-02

Completed the operator-facing correction phase. Repository Source/Architecture, Runtime Acceptance and cTrader Compile/Build all passed on implementation HEAD `b360761d13df94ac098e8bfc626ed2985499742d`. The Indicator was given the stable
display name **CFIP Smart Indicator** and the cBot **CFIP Smart Execution Bot** with M5 as the default host timeframe; the cBot Compile/Build gate is green.

The MTF context panel was corrected so an unresolved frame is not presented as
NEUTRAL when existing BullScore/BearScore or TrendBull/TrendBear evidence establishes a
directional bias. The panel will show BULL BIAS / BEAR BIAS while canonical Frame.Direction
and Decision semantics remain unchanged.

The live panel render path also drops the obsolete Quick Execution height reservation and
synchronization call.

No strategy threshold, Decision, Plan, risk or broker authority is changed.

Phase record: `docs/PHASE-INDICATOR-NAME-CBOT-LAUNCH-MTF-PANEL.md`.
## 2026-10-02 — M3 Single Trade Truth / Alert–Chart Coherence

Implemented on phase/m3-trade-truth-alert-chart-coherence-2026-10-02.

- Added immutable CFIP.Contracts.AlertEnvelope so popup/sound delivery retains SignalId/ScenarioId/PlanId/SourceTimeframe/Revision identity.
- Refactored AlertDelivery to carry the canonical envelope while preserving the single bounded delivery queue.
- Updated AlertEngine to use the existing provider identity resolvers and to suppress normal sound/visual signal side effects for blocked candidate alerts.
- Removed the obsolete _lastVisualAlert* presentation side-channel.
- Extended SignalVisualSnapshot with canonical opportunity identity.
- Prevented directional watch markers while EntryAllowed is false or a pending/live broker state owns the chart.
- Replaced the hard-coded main-plan (MTF) label with the canonical source timeframe from the current visual snapshot, with the existing deterministic fallback.
- Corrected compact signal-label anchoring so labels sit to the left of the 40-candle line start with a horizontal gap; labels are background-free and use the exact semantic color of their corresponding line.
- Added and accumulated tools/audit_phase_m3_trade_truth.py and wired it into Source/Architecture CI.

Verification status:
- Local source checkout/build was not available in this environment; repository CI is the authoritative compile/static verification boundary.
- GitHub API exposed no completed workflow runs for the new phase branch at implementation time.
- Target-terminal acceptance is intentionally still manual for sound playback, popup behavior and marker/line timing.

## 2026-10-02 — M5 Panel Live Content / Responsiveness

Implementation branch: `phase/m5-panel-live-responsiveness-2026-10-02`.

- Rebased panel execution presentation on the actual cBot heartbeat/capability state instead of retained Indicator execution flags.
- Removed the obsolete Indicator-owned AUTO TRADING presentation path that could surface a false `OFF` state after the cBot separation.
- Centralized effective panel width/content width through `PanelDimensionRule`.
- Made panel content refresh consume one current visual snapshot and fresh cBot state per refresh cadence.
- Added observable panel row-capacity overflow and stale live-row clearing when a plan disappears.
- Re-ran the full analysis -> signal -> plan -> presentation -> provider -> cBot -> broker -> outcome/history ownership checks through the accumulated M3/M4 audits and the new M5 panel audit.
- Added deterministic M5 panel geometry contracts.

Verification status: final M5 head `bb8238d8572880c785745ed763c51a6a8fd85e91` passed Source/Architecture, Runtime Acceptance Contracts and cTrader Compile/Build; accumulated M3/M4/M5 audits also PASS.

Next: complete M5 repository verification and target-terminal panel/cBot state validation, then proceed to M6.

## 2026-10-02 — M4 Time / Session / History / Persistence Truth

Implemented on phase/m4-time-session-history-persistence-2026-10-02.

- Added Core/Math/CanonicalTimeRule as the single owner for UTC normalization, UTC-day identity, exact day boundaries, half-open UTC intervals and deterministic 90-day archive periods.
- Migrated SessionWindowRule, DailyLoss accounting/guard/persistence and EOD lifecycle checks to the canonical time owner.
- Migrated signal timing, broker refresh, calculation readiness, economic-news timing, news protection/calendar and buffered-persistence timestamp normalization away from machine-local ToUniversalTime conversions.
- Made DailyLoss persistence account-scope aware using the existing canonical broker/account/live-state identity token. The previous account-number-only key is intentionally not auto-adopted because broker ownership cannot be proven; current-account facts reconstruct the state instead.
- Kept 90-day outcome/runtime archives append-oriented and preserved existing History marker/buffered persistence ownership.
- Added M4TimeHistoryContracts and an accumulated M4 source audit.
- The M4 audit explicitly rechecks the whole analysis -> signal -> plan -> presentation -> provider -> cBot -> broker lifecycle -> outcome/history chain.

Verification status:
- Final M4 head `2f8b447f70d7327232dfa5f118d00520017cef99` passed Source / Architecture, Runtime Acceptance Contracts and cTrader Compile/Build.
- Target-terminal verification remains required for exact broker session-time behavior, restart-mid-day persistence, History folder/readback and EOD/cBot reconnect behavior.
