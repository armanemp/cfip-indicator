## 2026-10-04 — Native cTrader Signal-Line Presentation Closure

Status: IMPLEMENTED — terminal visual acceptance is the remaining boundary.

The previous implementation had a structural visual mismatch even though the canonical line renderer itself was correct: labels were positioned from the right endpoint and a large rectangle spanned chart bars instead of behaving as a compact cTrader-style price tag.

Root cause closed at the existing single owners:
- PlanLineRenderer remains the only signal/plan line owner: Solid, 1px, finite, exactly 40 bars ending at the latest candle.
- PlanLabelAnchorCalculator now resolves the LEFT endpoint of that same line.
- PlanLabelRenderCoordinator, pending labels and parallel opportunity labels converge on that anchor.
- PlanLabelRenderer now renders a compact filled tag immediately before the line start, rather than a large box across the signal line.
- The label uses the exact normalized signal price, white text and the same semantic color as the line.
- No second line renderer, label renderer, calculation path or alternate visual owner was introduced.

This is a presentation correction only; signal calculation, TP/SL/RR, MTF roles and cBot execution ownership are unchanged.

Verification:
- Branch source changes committed and static single-owner audit contract updated.
- Local cTrader compile and target-terminal visual acceptance remain required on the final branch head; they are not claimed from repository-side source inspection.

Operator action: git pull --ff-only
Then run the Release build and inspect the live cTrader chart once; the acceptance target is one clean 40-bar native-style line with one compact colored price tag attached immediately at its left start, with no floating right-side label.

---

## 2026-10-04 — Master Full Forensic Audit Baseline

Status: **BASELINE CREATED — audit execution pending.**

Created `docs/MASTER-FULL-FORENSIC-AUDIT-2026-10-04.md` as the master restart index.

Baseline inventory:
- 1113 repository files / 82 directories;
- 731 C# files;
- 667 Indicator C# files;
- 22 cBot C# files;
- 22 Contracts C# files;
- 204 Markdown documents;
- 155 Python audit/tool files;
- current machine-enforced public-parameter baseline: 548.

The document contains:
- complete recursive repository file inventory;
- canonical architecture and end-to-end data/execution flow;
- current ownership/source-of-truth map;
- evidenced findings from prior forensic audits;
- full numbered potential-problem/defect checklist;
- line-by-line/code-by-code review method;
- severity and closure evidence rules;
- restart order for one-complete-phase-at-a-time remediation.

This is a documentation/audit baseline only; no strategy threshold or production behavior was changed.

Operator action:
`git pull --ff-only`

## 2026-10-04 — Smart Separated Signal Arrows

Status: VERIFIED COMPLETE — PR #252 merged to main; target-terminal visual acceptance remains the final manual boundary.
Merge: PR #252, commit b095710eb83e7f93017edb1050b20079b27b4371.

Completed for this user-requested item:
- Removed the duplicate HTF arrow-strength owner. The canonical strength source is now MtfTrendStrengthRule.
- The nine-level model is one ladder: 1–3 = Weak 1/2/3, 4–6 = Medium 1/2/3, 7–9 = Strong 1/2/3.
- Strength is derived from the existing multi-timeframe market evidence stack: frame quality, directional score, trend/momentum, ADX, EMA spread/slope, structure, OB/FVG location, independent indicator evidence, live pressure and conflict penalty.
- The canonical directional arrow renderer consumes SignalVisualSnapshot.MtfTrendStrengthLevel only; it no longer recalculates a second strength value.
- The authoritative direction is resolved first and then passed into the strength evaluator, so displayed strength cannot silently belong to another direction.
- Arrow glyphs have deterministic vertical separation using both ATR-relative and minimum-pip clearance.
- M1 trigger is now a Circle precision marker rather than a second directional arrow, eliminating a major overlap/ambiguity path.
- Removed the unused duplicate MtfTrendArrowRenderer production path and its superseded HtfTrendArrowStrengthRule.
- Removed the legacy fallback arrow-state owner from canonical call-sites.
- Added a dedicated deep audit and accumulated it in Source/Architecture CI.
- Routine project audit remains mandatory: Analysis → Decision → Signal → Alert → cBot execution → Broker confirmation → Protection/Lifecycle → Outcome/History, plus performance/code-cleanliness review.

Verification:
- Branch/source consistency: PASS.
- Automated Source/Architecture + Runtime Acceptance + cTrader compile/build: PASS on final PR head.
- Target cTrader terminal visual validation remains the only manual boundary for actual glyph appearance and live chart behavior; code-side spacing/stale-object contracts are verified.

Operator action after merge: git pull --ff-only.

---

## 2026-10-03 — Single-Owner / No-Duality Repair

Status: VERIFIED COMPLETE — merged to `main` via PR #250, merge commit `2e670514e90deeb46a3f140d1383446f0292c64d`.

Root causes closed:
- cBot startup had two audio cues: canonical Start plus an immediate Live-disarmed cue.
- Active Plan and WATCH/Reaction had separate directional-arrow ownership.
- Several chart label paths retained obsolete box/vertical-offset semantics instead of one exact-price presentation.
- Historical audits still encoded superseded line/label contracts.

Corrections:
- One startup cue is owned by `CbotLifecycleAudioService.PlayStarted`; Live DISARMED remains state/panel information, not a second cue.
- Active signal arrows converge on the canonical stacked-arrow renderer.
- Signal/plan lines converge on `PlanLineRenderer`: Solid, fixed 1px, exactly 40 bars from the latest candle.
- Pending/parallel lines delegate to that owner.
- All compact labels converge on one renderer/formatter: exact signal price, white/no-background text, source timeframe once, pip distance where applicable, left-of-line with minimum horizontal gap.
- Added a dedicated Single-Owner / No-Duality source audit and accumulated it in CI.
- Resolved contradictory legacy audits without creating alternate production behavior.

Verification on final implementation head:
Source/Architecture PASS; Runtime Acceptance PASS; cTrader Compile PASS.

Manual target-terminal validation remains required.
Operator action: `git pull --ff-only`.

Next gated work: Signal Quality + TP/SL/RR + OB/FVG + full MTF chain audit.

---

## 2026-10-03 — Canonical Signal-Line Visual Repair

Status: VERIFIED COMPLETE — merged to `main` via PR #249, merge commit `fb7dee65275ddb1d8e81d0090eb2a0472eadf633`.

Completed:
- Signal/plan lines are fixed to Solid + 1px.
- The canonical span is exactly 40 chart bars from the latest candle; legacy `FullWidthLevelLines` cannot expand signal geometry.
- Compact level labels sit at the exact line price, use white text with no background, and remain left of the line start with a deterministic minimum three-bar horizontal gap.
- Active Plan and WATCH/Reaction directional markers share the canonical stacked-arrow lifecycle.
- The user's Release-build CS0219 warning from the dead `PanelMainRenderer.buttonMargin` local was removed.
- Dedicated Drawing audit, Source/Architecture, Runtime Acceptance and cTrader Compile all passed on the final implementation head before merge.

Manual acceptance boundary:
Target cTrader terminal must still be used to visually confirm exact arrow shape/position, 40-bar line span, label spacing/readability and stale-object cleanup.

Operator action:
`git pull --ff-only`, then rerun:
`dotnet build src/CFIP.Indicator/CFIP.Indicator.csproj --configuration Release`

Next gated phase:
Signal Quality + TP/SL/RR + OB/FVG + full MTF chain audit.

---

## 2026-10-03 — Canonical Signal Drawing Hardening

Status: VERIFIED COMPLETE — implemented directly on `main`. Code head `a3cd97f715ed6b6b91599b79fe3fa42c82690e1c` passed Source/Architecture, Runtime Acceptance and cTrader Compile.

Completed:
- Active Plan directional arrows now share the canonical `RenderStackedSignalArrows` renderer with WATCH/Reaction.
- Legacy `P + "ARROW"` active-plan ownership was removed from rendering/cleanup.
- BUY/SELL remains `UpArrow` / `DownArrow`; M1 trigger remains a separate Circle marker.
- Signal/plan level lines are Solid, fixed at 1px, finite 40-bar geometry.
- Compact labels are white text inside compact boxes matching their line color and attached to the exact line endpoint.
- Expired/invalid marker/line objects continue to be removed through the canonical lifecycle.
- The dead `buttonMargin` local that caused the user's Release-build CS0219 warning was removed from `PanelMainRenderer`.
- Legacy drawing audits/contracts were updated to enforce the new visual contract.

Verification:
- Source / Architecture: PASS.
- Runtime Acceptance Contracts: PASS.
- cTrader Compile: PASS.
- Dedicated signal-drawing audit: PASS.
- Manual target-terminal visual acceptance remains required.

Phase record: `docs/PHASE-SIGNAL-DRAWING-CANONICAL-2026-10-03.md`.

Operator action: `git pull --ff-only`.

---

## 2026-10-03 — Footer + Alert/Popup Lifecycle Hardening

Status: VERIFIED COMPLETE — merged to `main` via PR #248, merge commit `94edfb5f52f98bea21f0e25d0a74b3cce49fa700`.

Completed:
- Footer minimum reduced from 40px to 34px without reusing outer PanelPadding as internal button margin.
- Two-row alert rail reduced to 37px using 18px alert rows plus 1px gap, while preserving five retained events and readable compact presentation.
- Alert transport dedup now uses canonical ContractIdentity fields plus AlertKey, so envelope revision/generated AlertId changes cannot re-queue the same pending causal event.
- Email delivery is now gated by canonical queue acceptance, preventing a rejected duplicate from producing a second email side effect.
- Alert rail content updates are revision-driven; panel lifecycle visibility remains synchronized independently.
- Deterministic runtime coverage was added for revision-independent dedup and post-delivery re-arm.
- Existing blocked-alert silence, canonical panel/sound delivery owner, and Indicator/cBot execution boundaries remain intact.

Verification:
- Source / Architecture: PASS.
- Runtime Acceptance Contracts: PASS.
- cTrader Compile: PASS.
- Target-terminal visual/audio acceptance remains a manual cTrader boundary and was not performed in this environment.

Full-chain routine audit:
Analysis -> Decision -> Signal -> Alert -> cBot execution -> Broker confirmation -> Protection/Lifecycle -> Outcome/History.

Operator action:
`git pull --ff-only`

---

## 2026-10-03 — Cross-Layer Semantic & Visual Consistency Hardening

Status: VERIFIED COMPLETE — merged to `main` via PR #247, merge commit `5933386c26a787ee3297fc6af825d1d85b74a0c3`.

Completed:
- Market Bias, primary M15/H1 alignment and realtime header no longer re-encode canonical timeframe direction into a competing BUY/SELL vocabulary.
- MTF labels remain sourced from PanelTimeframePresentationState.DirectionLabel.
- Top-Down HTF/MID direction text no longer exposes raw numeric direction values; ENTRY retains alignment/strength without inventing a second EntryFrameDirection owner.
- Decision, Entry Gate and readiness rows no longer use overlapping READY/CONFIRMED/BLOCKED wording; setup confirmation, trigger waiting, entry waiting and current actionability are distinguished.
- Canonical SignalVisualSnapshot stage CONFIRMED is preserved as CONFIRMED in authoritative panel state instead of being relabeled READY.
- WaveTrend evidence text and color now use WaveTrend direction, with explicit CONFLICT when it opposes the trade direction.
- No strategy, risk, RR, execution, M15/M5/M1 role or cBot broker-ownership rule was changed.
- Obsolete duplicate realtime-header formatting/color helpers were removed after the dedicated header owner became canonical.
- Added a dedicated semantic/visual consistency audit and wired it into Source/Architecture CI.

Full-chain routine audit:
Analysis -> MTF -> Decision -> Signal -> Alert -> cBot execution -> Broker confirmation -> Protection/Lifecycle -> Outcome/History.

Verification:
- Source/Architecture, Runtime Acceptance and cTrader Compile/Build passed on the exact final branch head before merge.
- Branch-local semantic consistency audit passed.
- Target-terminal visual/runtime acceptance was not performed in this environment and remains a manual cTrader acceptance boundary; it must not be inferred from CI success.

Operator action:
`git pull --ff-only`

---

## 2026-10-03 — Panel Timeframe Visual Parity + cBot Local/Cloud Lifecycle

Status: IMPLEMENTED ON MAIN — automated verification pending; target-terminal Local/Cloud prompt acceptance remains manual.

Completed:
- M1/M5/M15/M30/H1/H4/D1/W1 panel text rows now consume the same canonical `PanelTimeframePresentationState` as the lamp rail.
- Direction, readiness, strength and semantic color are resolved once per timeframe; the lamp and written status no longer have independent color mapping.
- Unready frames fail closed to WAIT/secondary presentation instead of displaying a directional color with zero strength.
- The cBot no longer calls `ChartIndicators.Add("CFIP Smart Indicator")` during restart/reload/binding recovery.
- Missing Indicator is now fail-closed with an explicit local-attachment diagnostic; existing IndicatorAdded/Removed/Modified rebinding remains.
- Stable Indicator/cBot type names, assembly names and AlgoName values remain deterministic.

Root-cause records:
- `docs/PHASE-PANEL-TIMEFRAME-SINGLE-SOURCE-2026-10-03.md` + dedicated parity audit.
- `docs/PHASE-CBOT-LOCAL-CLOUD-LIFECYCLE-2026-10-03.md` + dedicated lifecycle audit.

Verification:
- Source/Architecture, Runtime Acceptance and cTrader Compile must pass on the exact main head.
- Target terminal must confirm lamp/text parity and that repeated local cBot restart/reload does not re-enter a Local/Cloud selection flow.

Operator action:
`git pull --ff-only`

---
## 2026-10-03 — Panel Footer/Lamp Geometry + Deep Alert Coherence Hardening

Status: IMPLEMENTED ON MAIN — pending Source/Runtime/Build verification and manual cTrader visual acceptance.

Completed:
- Footer content minimum is 40px; outer PanelPadding is counted exactly once.
- The two-line MTF lamp rail is 38px and remains outside the scrolling content.
- Eight timeframe lamps divide the real panel content width into equal cells with no narrow-panel overflow.
- Lamp/label spacing is compact; lamp labels inherit the exact semantic lamp color.
- Shared panel status lamps are 30x30 with a 20px base font; MTF lamp tiers are 20/19/18px.
- Alert rail keeps five events in memory but renders the two latest rows, with no-wrap + ellipsis for guaranteed readable bounds.
- Canonical primary ScenarioId is suppressed from the parallel user-facing alert loop, preventing canonical ACTION/WATCH duplication while preserving independent simultaneous scenarios.
- SendUnifiedAlert now acknowledges delivery only after successful queue acceptance; cooldown and alert-state commits, plus local WATCH/REACTION/ACTION/RESTRICTION retry guards, follow that acknowledgement.
- Existing blocked-alert silence, bounded queue and single production sound owner remain intact.
- The existing panel/footer audit now includes the second-pass geometry and alert-coherence checks.

Manual acceptance must confirm: taller footer with every bottom item visible; equal left/right MTF spacing; compact lamp-to-label spacing; enlarged header lamp; one canonical primary alert; visible independent scenario alerts; blocked-alert silence; and audible delivery.

Operator action:
git pull --ff-only
then run the Release build locally.

---

## 2026-10-03 — Compiler Warning Cleanup / Candidate Reward-Distance Integrity

Status: IMPLEMENTED ON MAIN — commit `18c793ef320cbb2d087d17190b6c427197c5ec9d`.

Findings:
- Release build reported CS0649 for `TradeOpportunityCandidate.RewardDistanceAtr` and `MinimumRequiredRewardDistanceAtr`.
- The fields were present in the canonical candidate contract and consumed by `ScenarioExecutionPolicyRule`, but the main materialized parallel-candidate builder did not assign them.

Completed:
- `RewardDistanceAtr` is now populated from the canonical reward distance returned by `PlanRewardRiskQualityRule`, normalized by the candidate M5 ATR.
- `MinimumRequiredRewardDistanceAtr` is now populated from the existing `RegimeAdaptiveRewardFloorRule` using the existing `MinimumTpSpacingAtr` and `MinimumSlAtr` inputs.
- No new public parameter, hidden threshold, decision rule or execution authority was introduced.
- The dedicated panel/footer audit now also verifies these assignments.

Verification:
- User-provided Release build before this fix: succeeded with 2 CS0649 warnings, 0 errors.
- Runtime/Source verification after the previous panel fix was successful for Runtime; Build/Verify workflows were still running at the time of the last status check.
- Final local Release build after this commit must be re-run by the operator to confirm the warning count is 0.

Operator action:
`git pull --ff-only`, then `dotnet build src/CFIP.Indicator/CFIP.Indicator.csproj --configuration Release`.

---

## 2026-10-03 — FINAL REALTIME / LIVE / SMART SYSTEM INTEGRATION

Status: MERGED TO MAIN — PR #243 — merge commit `7af17f4fa65142468c76501399ee5082bf0f0f42`.

Completed in this continuation unit:
- realtime current-quote intelligence and historical/forecast evidence remain unified without introducing a second execution authority;
- all MTF engines remain active across M1/M5/M15/M30/H1/H4/D1/W1, with M15 canonical, M5 entry precision/tuning and M1 optional confirmation;
- current scenarios execute immediately while future scenarios are broker Pending Stop/Limit orders;
- multiple ScenarioIds remain bounded, idempotent and independently reconciled;
- stagnant-market reward/magnitude protection remains mandatory;
- LIVE Market/Pending/Aggressive/Management arms remain explicit and fail-closed by default;
- indicator broker-mutation-free boundary remains intact;
- signal sound delivery is separated from panel rendering and executed on the realtime Calculate/last-bar path;
- cBot lifecycle/block/execution audio has one owner;
- HTF trend presentation is a nine-level weak/medium/strong 1/2/3 arrow ladder;
- panel header has a dedicated realtime owner;
- fixed M1/M5/M15/M30/H1/H4/D1/W1 trend-lamp row lives outside the ScrollViewer;
- final integration audit and documentation added.

Verification:
- Pre-merge cTrader Compile/Build passed;
- Pre-merge Runtime Acceptance passed;
- Pre-merge Source/Architecture passed;
- target-terminal acceptance remains required;
- Runtime Acceptance Contracts;
- accumulated Source/Architecture audits;
- final realtime/live/smart-system integration audit;
- target-terminal acceptance for sound, attachment truth, same-tick handoff, concurrent scenarios, pending lifecycle, restart/reconnect and live broker mutation.

Operator action after verified merge: git pull --ff-only.

---

## 2026-10-03 — Lifecycle Audio + Realtime Panel Header

Status: IMPLEMENTED — verification pending.

Completed:
- cBot lifecycle/execution audio has a dedicated owner for Start, Stop, broker-confirmed execution, rejection and recovery.
- Indicator signal audio remains exclusively owned by the Indicator alert-delivery boundary.
- Panel header has a dedicated presentation owner and is refreshed from the current canonical signal state plus fresh cBot heartbeat/presence.
- Header updates are change-aware to avoid unnecessary chart-control writes.
- Existing M15/M5/M1/HTF timeframe-role contract, realtime current-vs-future scenarios, bounded multi-scenario execution, live-account explicit arming and stagnant-market reward protection remain unchanged.

Verification:
- cTrader Compile/Runtime/Source CI pending after this phase.
- Target-terminal validation remains required for actual sound audibility and panel freshness.

Operator action after verified merge: `git pull --ff-only` on local `main`.

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
- all aligned M1/M5/M15/M30/H1/H4/D1/W1 frames contribute simultaneous analysis;
- M15 is the canonical signal-tuning/reference layer, not the only analysis source;
- M5 provides lower-timeframe entry precision and M1 provides optional micro-entry/confirmation refinement;
- SL/TP candidates are drawn from lower and higher timeframe structure and filtered by common risk/reward and obstacle constraints;
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
- canonical BUY/SELL level rendering remains symmetric: Solid, fixed 1px, finite 40-bar geometry, background-free white labels aligned to the exact signal price and positioned left of the line with the canonical horizontal gap;
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
- deterministic Retest regression coverage and accumulated CI-18 audit were added.

Verification on final CI-18 head `1d33146d8fd3ffb4f17e8b580c3fc9321f74b4b5`:
- Source / Architecture #3087: **PASS**;
- Runtime Acceptance #2896: **PASS**;
- cTrader Compile #3080: **PASS**;
- CI-18 signal/panel coherence audit: **PASS**;
- accumulated MTF/UI/identity audits: **PASS**.

No public strategy threshold was lowered. Directional WATCH presentation remains presentation-only; final ActionableNow quality/RR, regime, divergence, trap-risk, indicator-fusion and market gates remain active.

Phase record: `docs/PHASE-CI-18-SIGNAL-PANEL-COHERENCE-2026-10-02.md`.

Next: **evidence-driven Signal/Target Quality Audit**, beginning with the 2R fallback clustering and Decision → Plan → Alert coherence audit, then intelligent progressive target/trailing behavior.



For every future phase, modify the existing canonical production owner directly. Do not create parallel hotfix files, duplicate executors, compatibility wrappers, alternate calculation paths, alternate identity formatters, or detached patch subsystems when the existing owner can be corrected. Any obsolete owner created by an extraction must be deleted in the same phase, and all audits/docs must point to the single surviving owner.

## CBOT-P4E — Management Command + Remaining Broker Mutation Authority — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #197 as `996cd9cb4872608674269c205b7a03b33ab9e812`.**

The Indicator's remaining broker mutation paths are command-only; `ManagementExecutionCoordinator` in the cBot is the single owner for cancellation, full/partial close, SL, absolute TP, TP-by-pips and server TP ladder mutation.

Verification on final merged implementation:
- Source / Architecture workflow #3064: **PASS**;
- Runtime Acceptance workflow #2873: **PASS**;
- cTrader Compile/Build workflow #3057: **PASS**;
- CBOT-P4E audit: **PASS**;
- accumulated architecture/execution/UI/identity audits: **PASS**.

Target-terminal broker execution verification remains manual. No profitability claim is made from this structural migration.

Phase record: `docs/PHASE-CBOT-P4E-MANAGEMENT-AUTHORITY-2026-10-02.md`.

Next staged phase: **CBOT-P5 — Protection / Lifecycle / Recovery completion**.

## CBOT-P4D — Pending Limit Authority + Signal/Popup Continuity — 2026-10-02

Status: **VERIFIED COMPLETE — merged to main via PR #196 as 352e6229adcff8a4ebb6ee6e5c71a0e0397dc70b.**

Scope:
- move Pending Limit broker mutation completely to the existing cBot pending execution owner;
- remove the obsolete Indicator Pending Limit broker owner;
- keep Indicator responsible for analysis, scenario, intent and absolute lifecycle snapshot only;
- expose a dedicated cBot Pending Limit arm while preserving demo-only/fail-closed execution;
- restore arrow-only direction visibility for early/watch/confirmed/strong states using three directional intensity colors;
- keep popup at BottomRight, persistent until next alert/manual close, and restrict popup delivery to important canonical alert families;
- preserve M15 internal execution independence from host Chart TF.

No new strategy engine, duplicate signal engine, duplicate broker owner, or alternate label formatter is introduced.

Repository verification on final P4D implementation head 22d7af189d7037237b27a3df09a42a9cdda7f252:
- Source / Architecture: **PASS**;
- Runtime Acceptance Contracts: **PASS**;
- cTrader Compile/Build: **PASS**;
- CBOT-P4D acceptance audit: **PASS**;
- accumulated execution/UI/identity audits: **PASS**.

Target-terminal acceptance remains manual. No profitability claim is made from this structural/UI phase alone.

Next staged execution phase: **CBOT-P4E — full remaining broker execution authority consolidation (cancellation, protection, partial TP, break-even, trail, close/recovery) in the cBot, with no duplicate Indicator mutation owners.**

## CBOT-P4C — Pending Stop Authority + Host-Timeframe Independence — 2026-10-02

Status: **VERIFIED COMPLETE — merged to main via PR #195 as 7b8648091bde26753b1e0fcff3d75984a1f1b9eb.**

Completed:
- M15 is the internal execution clock; Chart timeframe is host/presentation-only;
- Indicator/cBot runtime no longer rejects or branches on host Chart TF for execution;
- Pending Stop broker mutation moved completely to src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs;
- shared cBot margin/capacity safety owner prevents duplicate helper implementations;
- Pending Stop trigger uses current spread through canonical PendingEntryPriceRule;
- Indicator Pending Stop path is intent-only;
- active audits and documentation were reconciled;
- Pending Stop lifecycle snapshot is preserved across Indicator to cBot handoff;
- the canonical instance-scoped execution label is transported in CFIP.Contracts.ExecutionIntent and consumed by cBot broker execution/state ownership.

Verification on final implementation head c700e3adbc3557bf1278024df870a15a6e308b69:
- Source / Architecture 36991157739 / workflow #3025: PASS;
- Runtime Acceptance Contracts 36991157724 / workflow #2834: PASS;
- cTrader Compile/Build 36991157712 / workflow #3018: PASS;
- CBOT-P4C acceptance audit: PASS;
- CR5.4 absolute pending-fill reconciliation audit: PASS;
- CI-15 execution-geometry/broker-boundary audit: PASS;
- M15 / Risk / Spread audit: PASS;
- CR1.8 / A11 identity audit: PASS.

Target-terminal acceptance remains manual. No profitability claim is made from this structural migration alone.

Next staged execution migration: **CBOT-P4D — Pending Limit authority extraction.**

## MTF-EXECUTION-M15 — Primary Execution + Smart Margin/Spread Risk — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` in implementation commit `0a45bb251d28a5542fa7580d886d3af8b25184b7`.**

Decision for the active execution architecture:
- **M15 = internal primary execution clock**. The Indicator and cBot may be attached to any Chart TF; Chart TF is host/presentation-only and must not alter execution identity or logic.
- **M5 + M1 = defensive tuning layers** for entry timing/location and adverse microstructure; they refine or block an M15 setup but do not become a competing execution clock.
- **H1 + H4 + D1 + W1 = higher-timeframe context/reward-path layers** used for context and extended target selection.
- cBot runtime is host-timeframe independent.

Risk/price contract:
- requested volume remains risk-based from the Indicator's canonical stop geometry;
- Indicator sizing already includes spread when `Include Spread In Risk Sizing=true`;
- cBot now performs a final margin-budget cap after broker margin estimation and normalization, and only reduces exposure;
- canonical effective RR now subtracts spread from both risk and realized reward distance;
- synthetic targets account for the same spread cost;
- no new signal score, confidence threshold, RR policy tuning or target selection tuning is introduced beyond the requested spread/margin correctness.

Phase report: `docs/PHASE-MTF-EXECUTION-M15-RISK-SPREAD-2026-10-02.md`.

Verification on the final implementation head:
- Source / Architecture `36985594477` / workflow #2997: **PASS**;
- Runtime Acceptance `36985594449` / workflow #2806: **PASS**;
- cTrader Compile/Build `36985594530` / workflow #2990: **PASS**;
- M15 / Risk / Spread audit: **PASS**.

Target-terminal acceptance remains manual. No profitability claim is made from this architecture change alone.

Next execution-migration phase: **CBOT-P4C — Pending Stop authority extraction**.


# CFIP Indicator — Master Roadmap 2026-10-02
## Single Source of Truth — Correctness → Certification → cBot → Data-Driven Intelligence → Release

> این فایل **تنها مرجع اجرای پروژه** است. ترتیب فازها، شرایط پذیرش، وضعیت فعلی، معماری نهایی، قوانین تغییر، تست، پاکسازی و معیار پایان همگی در همین فایل تعریف می‌شوند.
>
> قانون اصلی: در هر پاسخ/واحد کاری فقط **یک فاز کامل** اجرا می‌شود. فاز بعدی فقط پس از ثبت نتیجه و تأیید ادامه داده می‌شود.
>
> این roadmap تضمین سود نمی‌دهد؛ هدف آن این است که هیچ نقص شناخته‌شده بدون مالک، تست، وضعیت و تصمیم نهایی باقی نماند و هیچ ادعای «حل شده» بدون شواهد معتبر پذیرفته نشود.

---

# 1. هدف نهایی

CFIP باید به یک سیستم واحد، قابل‌اعتماد، سریع، قابل‌آزمون و قابل‌نگهداری تبدیل شود که در آن:

1. تحلیل بازار، تصمیم، سناریو و Plan یک حقیقت واحد داشته باشند.
2. Chart، Panel و Alert دقیقاً همان حقیقت را نمایش دهند.
3. Signal → Plan → Alert → Chart → Execution → Broker State → Outcome از یک identity chain واحد تبعیت کند.
4. هیچ مسیر موازی نتواند Buy/Sell، Entry، SL، TP، RR یا وضعیت معامله را خلاف مسیر اصلی تولید کند.
5. SL فقط در جهت محافظتی حرکت کند و TP هرگز به عقب برنگردد.
6. Partial TP، Break-even، Trail، Dynamic TP، Close، Cancel و Recovery بر اساس broker-confirmed state باشند.
7. پنل واقعاً live، کامل، سریع و بدون stale content باشد.
8. Alert صوتی/نمایشی دقیقاً با event واقعی هم‌زمان و idempotent باشد.
9. تحلیل از execution جدا شود.
10. Indicator فقط Analysis / Decision / Scenario / Plan / Presentation باشد.
11. cBot تنها Broker Execution / Account Risk / Protection / Lifecycle Authority باشد.
12. کیفیت سیگنال فقط با Replay، OOS و Walk-forward سنجیده شود.
13. OB/FVG/WaveTrend/Divergence/VWAP/Liquidity/Volume فقط بر اساس evidence تقویت یا حذف شوند.
14. Clampهای پنهان، parameterهای دروغین، dead code، duplicate owner و فایل‌های زائد حذف شوند.
15. Repository نهایی از clean checkout قابل ساخت و تست باشد.

---

# 2. وضعیت مبنای واقعی — 2026-10-02

## 2.1 baseline

مبنای audit حاضر:
- حدود 631 فایل C#
- حدود 77 هزار خط
- 568 public parameters
- معماری ماژولار Analysis / Planning / Risk / Lifecycle / UI
- broker mutation هنوز داخل Indicator host
- current production OSS boundary validated separately
- Target Terminal acceptance هنوز gate نهایی است

## 2.2 وضعیت فعلی ثبت‌شده

- CI-17A مربوط به اصلاح live-content refresh پنل در repository پیاده و merge شده.
- repository gates آن PASS شده‌اند:
  - Source/Architecture
  - Runtime Acceptance Contracts
  - cTrader Compile
- با این حال stale/incomplete panel content هنوز در محیط واقعی کاربر گزارش شده؛ بنابراین repository PASS به‌تنهایی acceptance نهایی پنل نیست.
- Current terminal/broker boundary همچنان باید دستی اثبات شود.
- مسیر معماری نهایی: Indicator = analysis/signal/plan/presentation؛ cBot = broker execution/account risk/live protection/lifecycle.
- Cloud در این roadmap پیاده‌سازی نمی‌شود و فقط contractها transport-neutral می‌مانند.

## 2.3 اصل مهم status

هر موردی که فقط source-reviewed باشد، «حل‌شده قطعی» محسوب نمی‌شود.
برای رفتار cTrader/broker، evidence target-terminal لازم است.

---

# 3. قوانین غیرقابل‌مذاکره

## 3.1 One phase per response

هر پاسخ اجرایی دقیقاً یک فاز را کامل می‌کند.
نیمه‌کاره ماندن migration یا owner switch مجاز نیست.

## 3.2 ترتیب داخل هر فاز

1. Contract / invariant
2. Root-cause verification
3. Canonical implementation
4. Caller migration
5. Behavioral tests
6. Duplicate/dead-path cleanup
7. Runtime verification
8. Documentation/status
9. Performance/optimization audit
10. Phase closeout

## 3.3 Parameter contract

- نام، نوع و DefaultValue هیچ public Parameter بدون تصمیم صریح تغییر نمی‌کند.
- پارامتر جدید فقط با defaultی که behavior فعلی را حفظ کند.
- هر behavior change جداگانه علامت‌گذاری می‌شود.
- قبل از Replay هیچ tuning برای confidence/RR/risk/weights/thresholds مجاز نیست.

## 3.4 Evidence contract

- grep-only و text-only به‌تنهایی test نیست.
- هر bug fix باید behavioral test داشته باشد.
- ترجیح: test قبل از اصلاح fail و بعد از اصلاح pass.
- pure logic باید در Core یا معادل تست‌پذیر خود باشد.
- clock/time باید injectable باشد.
- ادعای قبلی در صورت تعارض با کد/تست باید رد شود و علت ثبت شود.

## 3.5 Single owner

ممنوع:
- duplicate decision engine
- duplicate execution engine
- alternate broker mutation
- second trading authority
- compatibility alias دارای business logic
- static mutable global برای bridge
- chart-object scraping
- reflection برای state خصوصی
- hidden fallback executor

## 3.6 Broker truth

- broker-confirmed state authoritative
- submitted != accepted
- accepted != filled
- pending != position
- rejection != success
- desired Plan state != broker state
- restart/reconnect: reconcile first, act second

## 3.7 Protective mutation

- Tighten SL فقط protective.
- Repair SL فقط برای missing/invalid/wrong-side.
- TP progression فقط forward.
- Partial/Close/Cancel فقط پس از broker confirmation.
- Plan live state فقط بعد از mutation success/reconciliation advance می‌کند.

## 3.8 Permanent optimization audit

در هر فاز بررسی کن:
startup، hot path، allocations، repeated computation، redraw، broker reads، cache growth، I/O، duplicate helpers، dead code، panel responsiveness.

Optimization نباید correctness را قربانی کند.

## 3.9 Mandatory closeout

هر فاز باید ثبت کند:
- Phase/IDs
- root cause
- files
- behavior changes
- before/after tests
- static checks
- runtime checks
- manual terminal checks
- new bugs
- unresolved risks
- CI results
- exact progress
- git pull requirement after merge

---

# 4. Definition of Done سراسری

پروژه فقط زمانی «کامل» تلقی می‌شود که:

1. هیچ Critical/High known defect بدون disposition باقی نماند.
2. Indicator هیچ direct broker mutation نداشته باشد.
3. یک decision authority و یک execution authority وجود داشته باشد.
4. Signal → Plan → Alert → Chart → Execution → Outcome identity-continuous باشد.
5. BUY/SELL symmetry و lifecycle invariants تست شوند.
6. Replay deterministic و بدون information leak باشد.
7. OOS evidence برای هر تغییر عددی وجود داشته باشد.
8. Target Terminal acceptance کامل باشد.
9. restart/reconnect/rejection/slippage/delayed-confirmation تست شده باشد.
10. Panel live content واقعی و سریع باشد.
11. Alert sound/popup/mark synchronized و idempotent باشد.
12. History durable و recoverable باشد.
13. source/tools/tests/build audit کامل شده باشد.
14. dead/obsolete files و duplicate docs پاک شده باشند.
15. clean checkout reproducible باشد.
16. Indicator بدون cBot قابل استفاده برای analysis/display باشد.
17. cBot به‌تنهایی broker execution را کامل انجام دهد.
18. نبود/stale بودن cBot یا signal source fail-closed باشد.
19. هیچ Cloud dependency برای local product وجود نداشته باشد.
20. بعد از آن feature expansion آزاد شود.

---

# 5. نقشه راه اصلی

# گروه A — تثبیت و اثبات واقعیت

## M0 — Adoption / Freeze / Baseline

Status: **VERIFIED COMPLETE — 2026-10-02.**

Repository baseline and continuity freeze were completed on branch `phase/master-roadmap-single-source-2026-10-02`.

Verified implementation/continuity HEAD before closeout: `6ceab05142fab7f2ac2bf9bbfd6f6346bd1023bc6`.

Verification:
- Source / Architecture #2756: **PASS**;
- Runtime Acceptance Contracts #2565: **PASS**;
- cTrader Compile #2749: **PASS**.

Completed:
- established this file as the single active development roadmap;
- reconciled accumulated historical audit continuity anchors without creating a second roadmap;
- verified repository baseline, parameter surface and execution-boundary inventory;
- preserved the existing Indicator execution architecture without production behavior change;
- documented the local Indicator → Contracts → cBot target boundary;
- documented that target-terminal/manual broker acceptance remains a later certification boundary.

Operator action after merge: `git pull --ff-only`.

### پذیرش
Baseline reproducible and all M0 finding/continuity blockers have a disposition.

### کارها
- ثبت branch/commit/working-tree.
- اجرای سه gate repository و ثبت runها.
- شمارش واقعی parameterها.
- ثبت project tree و dependency graph.
- inventory direct broker calls.
- inventory public parameters و ownership.
- جذب تمام known findings این roadmap.
- هیچ source behavior change.

### پذیرش
Baseline reproducible و همه findingها status داخلی دارند.

---

## M1 — Full Forensic Audit

Status: **IMPLEMENTATION COMPLETE — gate verification pending on M1 branch.**

Audit report: `docs/PHASE-M1-FULL-FORENSIC-AUDIT.md`.

### هدف
هیچ بخش قدیمی یا جدید بر اساس «گفته شده سالم است» فرض نشود.

### دامنه
Core, Analysis, Planning, Trading, Lifecycle, Runtime, UI, Tools, Tests, Build.

### checks
- BUY/SELL symmetry
- closed/live boundary
- NaN/Infinity/zero/negative
- hidden constants
- parameter clamps
- bounded collections
- hot-path I/O
- time/session
- persistence
- event idempotency
- identity propagation
- decision/execution authority
- visual authority
- alert authority
- outcome accounting
- large-method ownership

### پذیرش
هر finding: VERIFIED / PARTIAL / OPEN / FALSE / MANUAL-ONLY / DESIGN-RISK.
هیچ finding بدون disposition.

M1 disposition record: all F1–F18 are classified with an explicit owner/next-phase disposition. The only audit-tool correction made in M1 aligns `audit_cbot_boundary.py` with the current 568-parameter baseline; no production trading behavior changed.

---

## M2 — Repository Hygiene / Dead Code / Ownership

### هدف
قبل از توسعه جدید، repo از فایل و owner زائد پاک شود.

### کارها
- unreachable files
- dead classes/methods
- duplicate constants
- duplicate helpers
- empty methods
- duplicated visual suffix arrays
- unused scripts
- unused tests
- obsolete parameters بعد از migration
- stale references
- docs classification: WORKING / EVIDENCE / ARCHIVE

### rule
این roadmap تنها WORKING planning authority است.
بعد از جذب کامل محتوا، اسناد برنامه‌ریزی پراکنده و promptهای مصرف‌شده در cleanup نهایی حذف می‌شوند؛ Git history محفوظ می‌ماند.

---

# گروه A — Parallel cBot Separation (فعال از 2026-10-02)

این track به درخواست مالک پروژه از همین نقطه همزمان با M2 به بعد اجرا می‌شود و ترتیب قبلی M29–M38 را برای شروع migration عوض می‌کند. هدف آن این است که execution/account/lifecycle قبل از تکمیل بخش‌های تحلیلی آینده از Indicator خارج شود تا هیچ دوباره‌کاری یا دو owner باقی نماند.

## CBOT-P0 — Activation / Boundary Lock

Status: VERIFIED COMPLETE — 2026-10-02

Verification: Source/Architecture #2771 PASS; Runtime Acceptance #2580 PASS; cTrader Compile #2764 PASS.

- Indicator owner = analysis / decision / scenario / plan / presentation.
- cBot owner = broker execution / account risk / live protection / lifecycle / recovery.
- Contracts owner = platform-neutral immutable data-only boundary.
- No new broker mutation may be added to Indicator.
- Existing Indicator mutation remains temporarily only until its cBot replacement passes parity and terminal-safe verification.
- A migration step is not complete until: replacement exists → callers migrate → old owner removed → source audit proves zero duplicate authority for that migrated path.
- No project/file is copied wholesale when it mixes analytical and broker responsibilities.
- Exact extraction inventory: docs/CBOT-P0-EXECUTION-DEPENDENCY-CLOSURE.md.

## CBOT-P1 — Platform-Neutral Contracts

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification: Source / Architecture #2778 PASS; Runtime Acceptance #2587 PASS; cTrader Compile #2771 PASS.

Create and freeze one shared, immutable, platform-neutral contract model for Indicator ↔ cBot. Contracts must contain no cTrader dependency, no decision logic and no broker mutation. Required families: SignalEnvelope, PlanSnapshot, ExecutionIntent, ManagementCommand, BrokerExecutionReport, LifecycleEvent, ContractIdentity, ContractVersion, revision/sequence identity, CorrelationId and IdempotencyKey.

Authoritative schema audit: `tools/audit_cbot_contract_schema.py`.
Phase report: `docs/PHASE-CBOT-P1-PLATFORM-NEUTRAL-CONTRACTS.md`.

No parallel DTO/model set may be introduced in the cBot. Existing Indicator internal models remain temporary source models until P2+ provider migration replaces them with these contracts.

## CBOT-P2 — Read-Only Indicator Provider

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification: Source / Architecture #2790 PASS; Runtime Acceptance #2599 PASS; cTrader Compile #2783 PASS.

Expose one structured, immutable, read-only provider from the Indicator to the cBot using the supported cTrader custom-indicator reference mechanism. The provider uses the canonical `CFIP.Contracts` envelope and an invisible output heartbeat to make lazy evaluation deterministic. No chart scraping, reflection, private-field access or static mutable bridge is allowed.

Implementation: `src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProvider.cs`.
Boundary audit: `tools/audit_cbot_provider_boundary.py`.
Phase report: `docs/PHASE-CBOT-P2-READ-ONLY-INDICATOR-PROVIDER.md`.

## CBOT-P3 — cBot Host / Shadow

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification: Source / Architecture #2801 PASS; Runtime Acceptance #2610 PASS; cTrader Compile #2794 PASS.

The cBot now runs a deterministic shadow host over the canonical Indicator provider:

`receive → validate → expiry/revision → deduplicate → broker-safety → shadow state → telemetry`.

Implemented:
- `src/CFIP.cBot/Shadow/ShadowHostContracts.cs`
- `src/CFIP.cBot/Shadow/ShadowHostValidator.cs`
- `src/CFIP.cBot/Shadow/ShadowHostCoordinator.cs`
- cBot broker-state read adapter with single-plan capacity;
- bounded 128-key idempotency cache;
- providerRevision/envelopeRevision integrity check;
- BUY/SELL contract-geometry safety invariants;
- deterministic behavioral test project;
- Source/Architecture audit `tools/audit_cbot_shadow_host.py`.

No broker mutation is added in P3.

Phase report: `docs/PHASE-CBOT-P3-CBOT-HOST-SHADOW.md`.

## MTF-P1 — Primary M15/H1 Signal Layer + Panel Separation

Status: **VERIFIED COMPLETE — repository gates passed on implementation HEAD `c811967d8462229e8efc5f14af1f48cc3e3e72b2`; target-terminal evidence remains manual.**

Decision:
- M15 and H1 are the primary visible signal sources.
- Closed M5 tunes local structure/location/actionability.
- Closed M1 supplies timing confirmation through the existing trigger path.
- M15 and H1 may coexist simultaneously and independently.
- H1+/M30 context remains part of canonical top-down reasoning.
- Non-M5 primary candidates remain observe-only for execution until the cBot provider/execution track explicitly adopts them.
- M15/H1 OB/FVG evidence is preserved on the candidate and participates in display priority.
- Candidate Entry/SL/TP remains the canonical M5 planning projection in this phase; independent source-timeframe execution geometry is a later, separately verified change.

Panel:
- chart-panel AUTO TRADE / AUTO ORDERS quick controls are removed;
- canonical status/diagnostic rows remain;
- BottomLeft/BottomRight Indicator panel positions keep a fixed 100px bottom clearance for the separate cBot surface.

No public parameter, trading threshold, broker mutation authority or second decision authority is introduced.

Phase report: `docs/PHASE-MTF-P1-PRIMARY-M15-H1-PANEL.md`.

## MTF-P2 — Primary M15/H1 Location Evidence: OB/FVG Provenance

Status: **IMPLEMENTED — repository verification pending on this branch; target-terminal evidence remains manual.**

M15/H1 primary candidates now retain source-frame FVG quality, Order Block quality, OB+FVG confluence and canonical location quality. The existing LocationEvidenceRule remains the sole location-scoring owner; no new threshold or competing score was introduced.

Presentation priority uses primary source location evidence, and the chart panel exposes OB/FVG source evidence alongside the existing M5/M1 tuning state. This phase does not change broker authority, execution policy, RR, Entry, SL, TP, confidence, risk or public parameters.

Phase report: `docs/PHASE-MTF-P2-PRIMARY-LOCATION-OBFVG.md`.


## CBOT-DEMO-MARKET — Demo-only Market Execution Bridge — 2026-10-02

Status: **VERIFIED COMPLETE — 2026-10-02.**

Repository merge commit: `8ea385e2aa782ae24aa4f4a7941aee64bf4b685b`.

Verification on final implementation head `ba981da85e9ee06216f6c5ac3d7b4f2b4f4c6453`:
- Runtime Acceptance: PASS (run `36976202827`);
- cTrader Compile/Build: PASS (run `36976202817`);
- Source/Architecture: PASS (run `36976202805`).

Target-terminal evidence remains manual: attach exactly one `CFIP Smart Indicator` and this cBot to the same M5 chart, keep Indicator execution switches OFF, enable only the cBot demo-market switch on a demo account, and verify one bounded Market execution with broker-confirmed SL/TP.

The SDK `CS0612` warning for `IndicatorAttribute(string)` is suppressed only at the stable display-name attribute boundary; the equivalent cBot name warning is handled the same way. No strategy threshold, RR, Entry, SL, TP, risk or analytical score was retuned.

This phase is a validation bridge. Final production migration still requires the reissued CBOT-P4A Market / Market Range extraction from the current `main`, followed by Aggressive, Pending, Close, protection, lifecycle, account-risk and recovery migration.

Purpose: provide a bounded local/demo execution path while the remaining broker-mutation migration continues phase-by-phase.

Current boundary:
- Indicator = analysis / decision / scenario / plan / presentation and canonical SignalEnvelope publication.
- Contracts = immutable transport schema.
- cBot = demo Market broker mutation only in this phase, guarded against live accounts.

Safeguards:
- Enable Demo Market Execution defaults to false;
- live accounts are rejected by Account.IsLive;
- maximum demo market executions per cBot session defaults to 1;
- missing/duplicate Indicator, stale envelope, wrong symbol, invalid geometry, broker capacity or invalid volume fail closed;
- one idempotency key can never submit twice in one cBot session;
- only ExecuteMarketOrder is present in the cBot mutation owner; no pending, aggressive, close, SL mutation or TP mutation is activated by this phase;
- the existing Indicator market mutation remains dormant when Indicator automatic trading is OFF and is scheduled for the current-main P4A extraction immediately after this demo validation bridge.

Warning cleanup:
- the cTrader SDK obsolete IndicatorAttribute(string) warning is intentionally suppressed only around the stable display-name attribute so CFIP Smart Indicator remains unchanged.

Target-terminal acceptance:
- attach exactly one CFIP Smart Indicator and this cBot to the same M5 chart;
- keep Indicator Auto Trading / Automatic Orders / Aggressive Auto Entry OFF;
- arm only the cBot demo-market switch on a demo account;
- verify bind -> envelope -> SHADOW READY -> one demo Market execution -> broker position with initial SL/TP;
- stop/disable the cBot after the bounded test.

Phase report: docs/PHASE-CBOT-DEMO-LIVE-MARKET-2026-10-02.md.

## CBOT-P4B — Aggressive Authority + Panel Geometry Integrity — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` on 2026-10-02 as `33dd37f225fb9e2677070f122b5068b0c2df2e92`.**

Scope:
- fix panel content clipping by synchronizing the inner panel-stack height with the final outer panel height;
- harden panel sizing against transient/collapsed Chart.Height during control attachment;
- capture a finite pre-control viewport baseline before Chart.AddControl;
- complete the next staged cBot mutation batch for ExecutionAction.Aggressive;
- keep one demo broker mutation owner and preserve live-account fail-closed behavior.

No analytical threshold, confidence, RR, Entry, SL, TP or risk tuning is included.

Phase report: `docs/PHASE-CBOT-P4B-AGGRESSIVE-PANEL-GEOMETRY-2026-10-02.md`.

Verification:
- Source / Architecture run `36983568671`: **PASS**;
- Runtime Acceptance run `36983568612`: **PASS**;
- cTrader Compile/Build run `36983568641`: **PASS**;
- P4B deterministic panel/Aggressive audit: **PASS**.

Target-terminal acceptance remains manual.

Next: **CBOT-P4C — Pending Stop authority extraction**.

## CBOT-P4A — Market / Market Range Authority Cutover — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` on 2026-10-02 as `dee53a3dfa1cbfab7f4b7ec4826298739559d19c`.**

Repository verification on final implementation HEAD `e51d1bb82db2fec25b91917873ab19ccacbd9859`:
- Source / Architecture: PASS (run `36981439556`);
- Runtime Acceptance: PASS (run `36981439561`);
- cTrader Compile/Build: PASS (run `36981439570`).

This phase performs the physical Market / Market Range owner switch from Indicator to cBot:
- Indicator no longer contains the Market broker-mutation owner;
- the legacy Indicator automatic-market and aggressive-market broker execution stages are removed from the live calculation path;
- cBot consumes the canonical plan-derived MarketExecutionProfile and owns Market / Market-Range submission;
- Indicator plan materialization is no longer tied to Indicator Auto Trading;
- Indicator panel no longer exposes Close/Cancel broker-action buttons or the old Auto Trading / Auto Orders execution-status block;
- panel bootstrap geometry is finite before Chart.AddControl to prevent chart-area collapse during first measure;
- canonical signal presentation is separated from live quote actionability.

Remaining CBOT-P4 migrations are deliberately staged: Aggressive, Pending Stop, Pending Limit, Cancel, Close/Partial, SL, and TP/server-ladder mutations remain with their current Indicator owners until their replacement passes parity and owner-removal verification.

See docs/PHASE-CBOT-P4A-MARKET-RANGE-AUTHORITY-CUTOVER-2026-10-02.md.

## CBOT-P4 — Broker Mutation Extraction

Extract and remove, in controlled batches:
1. Market / Market Range
2. Aggressive
3. Pending Stop
4. Pending Limit
5. Cancel
6. Close / Partial Close
7. SL mutation
8. TP / server TP ladder mutation

Each batch requires caller migration and zero direct mutation remaining in Indicator for that path.

## CBOT-P5 — Protection / Lifecycle / Recovery

Move broker-confirmed state, protection mutation, BE, trailing/profit-lock mutation, partial/full close, pending lifecycle, fill reconciliation, restart/reconnect and orphan recovery to cBot. Analytical reasoning remains in Indicator.

## CBOT-P6 — Account / Execution Risk Authority

Move trading permission, managed identity, capacity, final volume normalization, margin, live spread/session checks, daily-loss enforcement and execution retry/circuit ownership to cBot. Analytical quality/RR/suitability stay in Indicator.

## CBOT-P7 — UI / State Cutover

Indicator panel/chart becomes a read-only reflection of execution state. No UI action in Indicator can mutate broker state. AUTO TRADE / AUTO ORDERS status reflects actual cBot state.

## CBOT-P8 — Physical Removal / Terminal Certification

Delete migrated Indicator execution owners, hidden fallbacks, duplicate lifecycle state, execution-only parameters, and stale references only after replacement parity and target-terminal verification. This phase feeds M39/M40 certification.

### Cross-track close rule

M2–M28 may continue in parallel, but any phase touching execution/protection/panel execution status must consume the current cBot contracts/authority. No later phase may reintroduce execution logic into Indicator.


# گروه B — Canonical Truth

## M3 — Single Trade Truth Chain

Status: **VERIFIED COMPLETE — merged through PR #207 after all required repository gates passed.**

2026-10-02 implementation:
- added immutable CFIP.Contracts.AlertEnvelope carrying the same ContractIdentity used by the Indicator→cBot provider boundary;
- routed popup and sound through the same AlertDelivery envelope;
- blocked/restriction candidates no longer produce the normal signal sound or chart signal marker;
- removed the obsolete visual-alert side-channel state so chart presentation remains snapshot-owned;
- extended SignalVisualSnapshot with SignalId/ScenarioId/PlanId/SourceTimeframe/Revision;
- made main plan labels resolve the real canonical source timeframe instead of a hard-coded (MTF) tag;
- added an accumulated tools/audit_phase_m3_trade_truth.py whole-chain audit and wired it into Source/Architecture CI.


### هدف
حل ریشه‌ای mismatch بین pre-analysis، signal، scenario، plan، panel، chart، alert، execution و outcome.

### immutable opportunity identity
- SignalId
- ScenarioId
- PlanId
- SourceTimeframe
- Lane
- Symbol
- Direction
- CreatedUtc
- ClosedBarReference
- Revision
- Expiry
- State

### same source
همه این‌ها باید از همان snapshot/contract بخوانند:
- panel
- chart
- popup
- sound
- market intent
- pending intent
- telemetry
- outcome attribution

### پذیرش
برای یک fixture، همه consumers همان direction/price/RR/identity/state را بدهند.

---

## M4 — Time / Session / History / Persistence Truth

Status: **VERIFIED COMPLETE — merged to `main` via PR #207.**

2026-10-02 implementation:
- introduced a single CanonicalTimeRule for UTC normalization, UTC trading-day boundaries, half-open intervals and deterministic 90-day archive periods;
- migrated SessionWindowRule, DailyLoss accounting/guard/persistence, EOD, Runtime Log and related timing/news/persistence owners to the canonical UTC semantics;
- made unspecified API timestamps timezone-invariant by treating them as algorithm UTC rather than machine-local time;
- scoped DailyLoss persistence by the canonical account identity; the prior account-number-only key is intentionally not auto-adopted because broker ownership cannot be proven safely;
- kept the existing bounded buffered archive/history architecture and startup History location marker;
- added deterministic M4 runtime contracts covering midnight, overnight sessions, EOD windows, 90-day rotation, restart/account scope and DST-calendar invariance;
- added the accumulated tools/audit_phase_m4_time_history.py full-chain audit.


### هدف
حل همه ناسازگاری‌های زمان، trading day، previous-period، EOD، daily loss و history.

### کارها
- canonical trading-day.
- previous D1/W1.
- overnight session.
- EOD warning/cancel/close.
- late-created position handling.
- realized/floating loss separation.
- daily-loss persistence.
- restart-mid-day.
- storage root.
- History marker.
- read/write recovery.
- 90-day rolling archive بدون پاک کردن تاریخچه قبلی.
- duplicate outcome prevention.
- transfer/import path.
- DST-aware session semantics.

### پذیرش
پس از restart/re-attach history و daily-loss state درست و قابل ممیزی باشد.

---

## M5 — Panel Live Content / Responsiveness

Status: **VERIFIED COMPLETE — merged to `main` via PR #209 as `50dac181e6561bf65fd30a6cef246c189f5c89f5`.**

### هدف
حل واقعی stale/incomplete/slow panel.

### کارها
- full-layout و content-refresh را جدا کن.
- reaction/prediction/context/plan/execution rows را در content refresh تضمین کن.
- یک snapshot per refresh.
- حداقل broker reads.
- EffectivePanelContentWidth واحد.
- overflow counter؛ silent row drop ممنوع.
- button width بر اساس PanelWidth.
- height measurement واقعی یا conservative deterministic.
- width tests: 220/430/700.
- font tests.
- reload/restore.
- latency budget.
- no flicker/no stale row.

### I2 safety
پارامتر جدید Confirm Panel Close Actions فقط با default false.
وقتی true:
کلیک اول = CONFIRM?، کلیک دوم حداکثر طی 3 ثانیه.
برای مهلت از clock تزریقی استفاده شود.
ManagedActionsOnly و هشدار حالت unrestricted باید verification شوند.

### پذیرش
در target terminal، panel content بدون full-layout rebuild به‌روز شود و هیچ row مهمی stale نشود.

---

## M6 — Alert Synchronization / External Watchdog

### هدف
sound، popup، chart mark، panel state و telemetry یک event باشند.

### کارها
- canonical AlertEnvelope.
- event timestamp.
- source Revision.
- duplicate suppression.
- stale suppression.
- blocked signal → no sound/no mark.
- watch/prediction/reaction/confirmed/active/expired جدا.
- sound after accepted alert event only.
- restart/reconnect semantics.
- race tests between Calculate/Timer/UI.
- critical external notification channel برای مواردی مثل lost protection، rejected close و daily-loss lock؛ provider اتصال‌پذیر باشد و notification source همان canonical event باشد.

### پذیرش
برای هر fixture دقیقاً expected alerts تولید شود؛ sound/popup/mark mismatch صفر.

---

## M7 — Chart / Label Truth

### هدف
تمام chart objects دقیقاً همان Plan را نشان دهند.

### کارها
- Entry/Trigger/SL/TP canonical geometry.
- LineLengthBars به plan lines وصل شود.
- default 40 حفظ شود.
- future extension با DateTime/timeframe.
- solid/thickness contract حفظ شود.
- label text readability.
- واقعی کردن Source Timeframe به‌جای tag ثابت.
- label font consistency.
- LabelLeftOffsetBars دامنه را بی‌اثر نکند.
- expired drawings حذف.
- multiple scenarios بدون overlap/ambiguity.
- old generic signal text حذف فقط اگر duplicate owner باشد.
- marker shape از contract تبعیت کند.
- label text: price/name + TP/SL distance in pips + source timeframe، بدون background در طراحی نهایی.

### پذیرش
Visual contract tests + manual chart matrix.

---

# گروه C — Execution Safety قبل از جداسازی

## M8 — Canonical Entry / SL / TP / RR

### هدف
هیچ path نتواند geometry Plan را خراب کند.

### کارها
- BUY/SELL side rules.
- entry validation.
- stop-side validation.
- target-side validation.
- normalized price.
- normalized volume.
- slippage envelope.
- pending/market distinction.
- final RR recalculation.
- invalid stop candidate must fail explicitly.
- invalid computed stop must not return false success.
- RecoveryRequired when protection cannot be established.
- G1 Tighten-only at the mutation boundary.
- TP monotonic progression.
- BE after Partial TP uses same protection owner.
- Dynamic TP advance never backwards.

### پذیرش
Mock broker + symmetric BUY/SELL fixtures.

---

## M9 — Execution Lifecycle / Event Ordering

### کارها
- explicit state machine.
- submission identity.
- scenario-scoped retry/circuit.
- broker rejection.
- delayed confirmation.
- opened-before-ID event.
- duplicate events.
- manual volume reduction.
- pending creation/fill/cancel.
- orphan/recovery.
- restart/reconnect.
- no duplicate execution.

### پذیرش
mock broker deterministic.

---

## M10 — Runtime / Startup / Hot Path

### هدف
رفع late start، 30–60 second waits، calculation starvation و unnecessary work.

### کارها
- cold/warm startup measurement.
- MTF readiness.
- staged initialization.
- timer ownership.
- Skender bounded warm-up/cache.
- repeated allocations.
- broker enumeration.
- chart redraw.
- panel refresh.
- file I/O.
- cache bounds.
- large-method decomposition with golden regression tests.
- signal latency measurement: bar close → decision/plan.

### پذیرش
startup and refresh budgets recorded; no regression.

---

# گروه D — Test / Replay Foundation

## M11 — Deterministic Replay

### inputs
M1/M5/M15/H1/H4/D1/W1 CSV with UTC.

### output per signal
- time
- direction
- Entry
- SL
- TP1..TP4
- Confidence
- Quality
- Regime
- Lane
- ScenarioId
- Stage
- first SL/TP outcome
- MFE
- MAE
- bars-to-event
- spread
- commission
- slippage

### attribution
در یک candle با هر دو touch، SL first.

### reports
- Expectancy R
- Profit Factor
- Win rate
- max drawdown
- Bootstrap CI
- by Regime
- by Lane
- Confidence bucket
- hour
- session
- weekday

### walk-forward
Train/validation/test جدا و بدون leak.

### پذیرش
دو اجرای پشت‌سرهم byte-equivalent.

---

## M12 — Replay vs Live Consistency

### کارها
- SignalTrace comparison.
- repaint detection.
- stage ordering.
- scenario identity.
- plan identity.
- closed-bar semantics.
- future leak detection.
- live vs replay latency.
- plan/visual attribution consistency.

### پذیرش
هر difference یا توضیح ریشه‌ای دارد یا fail است.

---

## M13 — Data Quality / Time Quality

### کارها
- gap detection
- incomplete candle detection
- market closure
- DST
- rollover
- tick-volume limitations
- insufficient bars
- missing news data
- symbol→currency mapping
- insufficient MTF data
- fail closed for unavailable critical data

### پذیرش
داده ناکافی = unavailable / no-trade، نه تحلیل ساختگی.

---

# گروه E — Signal Quality / Data Science

## M14 — Ablation

### module set
- WaveTrend
- Divergence
- OSS indicator families
- OB
- FVG
- OB+FVG
- Liquidity
- VWAP
- Volume

### method
one-at-a-time + controlled combinations + OOS.

### measure
- Expectancy
- PF
- drawdown
- CI
- trade count
- latency
- redundancy

### decision
KEEP / REDUCE / DISABLE / REMOVE.

هیچ ماژول جدیدی قبل از این gate اضافه نمی‌شود.

---

## M15 — Parameter Simplification

### هدف
از 568 public parameters به حدود 40–60 effective controls، فقط در صورت evidence.

### کارها
- sensitivity analysis.
- identify non-effective params.
- detect redundant controls.
- merge semantically duplicate controls only when behavior can be preserved.
- three operator profiles: Conservative / Balanced / Aggressive.
- hide/persist low-value implementation controls only after migration safety.
- no destructive rename/default change without explicit approval.

### پذیرش
هر کاهش parameter با mapping قدیم→جدید و regression tests.

---

## M16 — Confidence Calibration

### هدف
Confidence = calibrated probability, not raw score.

### کارها
- P(TP1 before SL).
- regime-aware mapping.
- Isotonic or Logistic.
- minimum sample rule.
- reliability curve.
- Bootstrap CI.
- calibration drift.
- panel: P=0.xx ± 0.xx (n=...)

### پذیرش
فقط OOS evidence معتبر است.

---

## M17 — Range / Regime Signal Quality

### هدف
ضعف signals در range بدون از دست دادن فرصت‌های low-risk/high-reward.

### کارها
- centralize Range thresholds.
- verify current 0.35 / 0.65 behavior.
- test 20/80 hypothesis only via Replay.
- validate Confidence/SmartQuality/IndependentEvidence/RR interactions.
- prevent structural double counting.
- range false-positive study.
- range reward-path study.

### fixed-to-owner cases
- Confidence
- SmartQuality
- IndependentEvidence
- RR
- WaveTrend threshold
- range edge threshold

### پذیرش
هیچ numeric tuning بدون OOS evidence.

---

## M18 — OB / FVG / Confluence

### هدف
OB و FVG وزن بالاتر فقط اگر data confirms edge.

### کارها
- geometry
- mitigation
- opposing-direction obstacle
- freshness
- age
- touch count
- distance
- OB+FVG confluence
- reaction after touch
- reward-path effect
- target effect

### پذیرش
هر new weight/threshold evidence-backed + OOS.

---

## M19 — Exit MAE/MFE Research

### کارها
اندازه‌گیری:
- stop width
- 0.75R soft invalidation
- BE trigger
- Partial TP
- TP1..TP4 ladder
- Dynamic TP
- trailing give-back

### rule
هر exit policy با MFE/MAE و walk-forward قضاوت می‌شود.

---

## M20 — Session / Spread / News

### کارها
- DST-aware real session calendar.
- hourly/day expectancy.
- rollover filter.
- spread moving median.
- spread z-score.
- dynamic liquidity guard.
- news importance windows.
- symbol-to-currency mapping.
- index/gold/crypto mapping.
- optional reduced-risk news mode.
- transparent block reason in panel.

### پذیرش
changes measured OOS.

---

# گروه F — Risk / Survival / Explainability

## M21 — Outcome Integrity / Adaptive Risk

### هدف
Adaptive Risk فقط از outcome صحیح استفاده کند.

### required
- aggregate R across all legs
- Partial TP accounting
- BE accounting
- close accounting
- NaN/Infinity rejection
- min sample
- named constants/parameters with current defaults
- reason logging
- clear Risk % Equity description
- clear Use Smart Risk Scaling description
- effective risk visible

### current adaptive logic to preserve until evidence
Current multiplier behavior is bounded and cannot exceed configured risk.
Before changing coefficients, correct outcome attribution.

### after evidence
- weekly loss limit
- peak drawdown de-risking
- volatility targeting
- correlated exposure cap
- fractional Kelly ≤ 0.25 Kelly only if statistically justified

### acceptance
effective risk <= configured risk and reason is auditable.

---

## M22 — Statistical Safety Switch

### هدف
وقتی live results statistically diverge from Replay expectation، new auto execution stop شود.

### candidates
CUSUM / SPRT / defensible equivalent.

### behavior
- stop new execution
- preserve existing broker SL/TP
- alert operator
- do not auto-close solely from detector

### acceptance
normal synthetic stream = no trigger؛ degraded stream = trigger.

---

## M23 — Why-No-Trade

### هدف
کاربر بداند چرا معامله انجام نشد.

### output
Top 3 reasons:
- panel
- daily CSV
- replay report

### rule
هر block canonical reason code و owner داشته باشد.

### acceptance
panel/log/replay counts برابر.

---

# گروه G — Advanced Intelligence مشروط

## M24 — Importance-based Swing

- ATR-ZigZag
- level age
- depth
- touch count
- equal-high/low ATR tolerance
- liquidity importance

فقط در صورت evidence.

## M25 — Anchored VWAP / Volume Profile

- session anchored VWAP
- swing anchored VWAP
- high tick-volume zones
- target/obstacle utility

فقط اگر ablation positive باشد.

## M26 — Lightweight Meta-Labeling (اختیاری)

- offline only
- logistic regression
- walk-forward
- take/skip existing signal only
- no runtime ML dependency
- no black-box trading engine

اگر overfit/unstable، حذف شود.

---

# گروه H — Engineering Quality

## M27 — Property-Based / Mutation Testing

### tests
- BUY/SELL mirror property
- random geometry
- random regime combinations
- random event order
- NaN/Infinity
- invalid transitions
- mutation testing on Core

### پذیرش
mutated business logic should be caught.

---

## M28 — Replace Text-Only Audits

### کارها
auditهایی که behavior را فقط با regex/text بررسی می‌کنند به behavioral/contract tests منتقل شوند.
فقط architecture checks واقعی در scripts باقی بمانند.

### خروجی
- faster CI
- lower false confidence
- deterministic regression coverage

---

# گروه I — cBot Separation

## M29 — CBOT-0 Boundary Inventory

### Indicator keeps
- Market data
- MTF analysis
- structure
- liquidity
- BOS/MSS/CHoCH
- FVG
- OB
- OB+FVG
- WaveTrend
- divergence
- regime
- no-trade intelligence
- confidence/quality
- trigger intelligence
- scenario construction
- Entry/SL/TP proposal
- analytical RR
- chart/panel
- signal alerts
- analytical outcome/calibration logic

### cBot owns
- trading permission
- managed identity
- balance/equity/margin
- final volume normalization
- capacity
- live daily-loss enforcement
- broker/account spread/session checks
- Market
- Aggressive
- Pending Stop
- Pending Limit
- Cancel
- Close/Partial Close
- SL/TP mutation
- BE
- Trail
- Profit Lock
- broker confirmation
- fill reconciliation
- lifecycle
- restart/reconnect recovery
- retry/backoff/circuit
- execution telemetry

### mixed classes
method-by-method split؛ whole-file copy ممنوع.

### acceptance
every direct broker mutation future-owned exactly once.

---

## M30 — CBOT-Preflight

### no-trade capability test
اثبات کن:
- cBot can instantiate the custom Indicator through supported cTrader mechanism.
- cBot can read structured read-only signal data.
- no reflection/static globals/chart scraping required.
- instance scope deterministic.
- stale/unavailable provider is detected.
- package/build layout works in target terminal.
- startup order variations do not fabricate signal.

### blocking rule
بدون موفقیت M29 + M30، cBot implementation شروع نمی‌شود.

---

## M31 — Platform-Neutral Contracts

### required data
- SignalEnvelope
- PlanSnapshot
- ExecutionIntent
- ManagementCommand
- BrokerExecutionReport
- LifecycleEvent
- ContractVersion
- Sequence/Revision
- CorrelationId
- IdempotencyKey

### identity
SignalId / ScenarioId / PlanId / SourceTimeframe / Lane / Symbol / Direction / CreatedUtc / ClosedBarReference / Expiry / Revision.

### rule
Contracts data-only است؛ decision engine جدید نیست.

---

## M32 — Indicator Read-Only Provider

### states
- unavailable
- initializing
- watch
- prediction
- confirmed
- active
- expired
- blocked

### forbidden
- broker mutation
- private state scraping
- static mutable global bridge
- hidden executor fallback

### acceptance
stale/uninitialized signal never actionable.

---

## M33 — cBot Shadow Execution

### flow
receive → validate → expiry/revision → duplicate → identity → account safety → simulate → telemetry

### acceptance
one-to-one plan/intent parity; no live broker mutation.

---

## M34 — Broker Execution Migration

### exact order
1. Market
2. Aggressive
3. Pending Stop
4. Pending Limit
5. Cancel
6. Close
7. Partial Close
8. SL
9. TP

### acceptance
zero direct broker mutation in Indicator for migrated paths.

---

## M35 — Protection / Lifecycle / Recovery Migration

### move
- broker-confirmed state
- protection reconciliation
- BE
- TP progression mutation
- trailing
- partial/full close
- pending lifecycle
- restart/reconnect
- orphan recovery

### invariant
Indicator supplies analytical intent/management request; cBot validates broker-level mutation and owns the mutation.

---

## M36 — Account Risk / Execution Control Migration

### move
- capacity
- volume normalization
- margin safety
- daily loss enforcement
- trading permission
- managed identity
- live spread
- live session restriction
- broker modification throttles

### stay Indicator
- analytical RR
- signal quality
- market suitability
- scenario reasoning

---

## M37 — UI Control Authority Cutover

### goal
UI never lies about execution.

### rules
- cBot authoritative execution state.
- Indicator shows status/diagnostics.
- no Indicator click can mutate broker.
- unavailable control/status channel = fail-closed.
- AUTO TRADE / AUTO ORDERS status must correspond to actual cBot state.

---

## M38 — Remove Indicator Execution Engine

### physical cleanup
- direct broker mutation code
- hidden fallback
- duplicate execution state
- duplicate managed identity
- dead execution handlers
- execution-only parameters no longer owned by Indicator

### hard acceptance
AST/source architecture proves zero direct broker mutation in Indicator.

---

# گروه J — Terminal Certification / Release

## M39 — Full Target-Terminal Acceptance

### demo only until complete
Mandatory:
- startup/reload
- M1/M5 timing
- MTF readiness
- panel cold/warm
- live panel updates
- chart drawing
- sound alerts
- news
- history write/read
- daily loss
- EOD
- overnight session
- Market/Aggressive/Stop/Limit
- slippage
- rejection
- delayed confirmation
- restart
- reconnect
- partial close
- BE
- trail
- TP advance
- protection recovery
- duplicate event
- missing cBot
- stale signal
- expired signal
- multiple simultaneous scenarios
- panel widths 220/430/700
- fonts
- chart reload

### acceptance
هر fail => defect ID و no-final.

---

## M40 — End-to-End Certification

### chain
Market
→ Analysis
→ Decision
→ Scenario
→ Plan
→ Signal
→ Panel
→ Chart
→ Alert
→ cBot
→ Broker
→ Confirmation
→ Protection
→ Outcome
→ History
→ Replay

### checks
identity, direction, prices, RR, risk, timeframe, revision, reason.

---

## M41 — Final Repository Cleanup

### کارها
- delete unreachable source
- delete dead classes
- remove obsolete execution code
- remove duplicate scripts
- remove obsolete planning files
- remove temporary artifacts
- remove stale references
- clean accidental binaries/logs
- remove historical production identifiers from source
- verify generated outputs
- verify package contents
- verify clean checkout

### acceptance
clean checkout builds/tests exactly.

---

## M42 — Release Gate

### required green
- Source/Architecture
- Runtime Acceptance
- cTrader Compile
- Replay
- deterministic replay
- property tests
- mutation evidence
- target terminal
- restart/reconnect
- repository hygiene
- documentation completeness

### rule
No real-account auto execution before all mandatory gates are green.

---

# 6. بعد از Release

## M43 — Production Observation

monitor:
- live/replay distribution
- drift
- reject reasons
- broker error rate
- protection failures
- panel latency
- execution latency

بدون automatic tuning.

## M44 — Controlled Calibration

- OOS only
- walk-forward
- versioned calibration
- rollback
- no hidden coefficient changes

## M45 — Future Cloud Analysis (اختیاری)

فقط در صورت نیاز:
- same Contracts
- cBot remains execution authority
- no duplicate analysis engine
- local mode remains functional
- no Cloud requirement for local operation

---

# 7. Exact Known-Issue Closure Matrix

## I-series
- I1 IndependentEvidence mismatch → M3
- I2 Close/Cancel confirmation → M5 / M37
- I3 score rounding → M1 / M28
- I4 WaveTrend OS/OB validation → M1
- I5 Quality Recovery reference → M17
- I6 Range hard-coded thresholds → M17
- I7 panel button/height sizing → M5
- I8 missing time/session behavioral tests → M4 / M28
- I9 source-review-only claims must be re-verified → M1

## J-series
- J1 LineLengthBars plan lines → M7
- J2 label color / real timeframe → M7
- J3 LabelLeftOffsetBars clamp → M2 / M7
- J4 effective panel width / overflow → M5

## K-series
- K1 adaptive outcome risk → M21
- K2 empty HashSet + duplicated suffix ownership → M2 / M7
- K3 MAX RR must show actual Plan RR → M7
- K4 Invariant UI formatting → M7

## G1 remainder
- Tighten-only at mutation boundary → M8
- Partial-TP BE path audit → M8/M9

## D2
- Relative History path → M4/M39

---

# 8. Hidden-Clamp Closure Contract

M1/M2 must detect not only simple Max/Min but also:

- nested Max(Min(...))
- Clamp / ClampInt
- multiline calls
- reversed argument order
- helper wrappers
- parameter aliases

Every clamp must be:
1. aligned with public Min/Max;
2. removed if it is accidental;
3. allowlisted with explicit reason;
4. or escalated as a behavior question.

Known high-risk examples that require explicit review:
- MinimumTriggerBodyAtr internal floor higher than its public minimum.
- SmartStrongSetupEdge internal floor higher than its public minimum.
- LabelLeftOffsetBars capped below its public maximum.
- panel width internal minimum above public minimum.
- WaveTrend minimum quality below internal floor.
- BreakEven / Trail hidden minimums.
- structural lookback and obstacle lookbacks.
- predictive quality/zone-age floors.

No default is silently changed merely to make the clamp disappear.

---

# 9. Permanent Signal-Quality Rules

1. Do not add indicators before ablation.
2. Do not raise/lower thresholds by intuition.
3. Do not optimize on the complete dataset.
4. Do not use black-box live entry/exit AI.
5. Do not infer profitability from a visual sample.
6. Do not promote independent scenarios to execution without one authoritative execution contract.
7. Preserve high-RR opportunities during tightening studies by measuring conditional expectancy, not trade count alone.
8. OB/FVG evidence must be measured against baselines.
9. Range filters must be evaluated on Range-only OOS subsets.
10. Confidence must be calibrated before treating it as probability.
11. Exit changes require MAE/MFE evidence.
12. Any change that improves in-sample but harms OOS is rejected.

---

# 10. Permanent Execution / Protection Rules

### Entry
exact plan identity and geometry.

### SL
never farther in Tighten mode.

### TP
never backwards.

### Partial
volume and realized R reconciled from broker facts.

### Close
broker-confirmed close only.

### Cancel
broker-confirmed cancellation only.

### Recovery
missing protection = explicit recovery state.

### Restart
broker state first.

### Reconnect
broker state first.

### Duplicate
idempotency key and scenario identity.

---

# 11. Permanent UI Truth Rules

Panel and chart are presentation surfaces, not authorities.

Every displayed:
- direction
- Entry
- SL
- TP
- RR
- confidence
- quality
- stage
- reason
- execution state
- protection state

must come from the canonical current snapshot.

No display-only value may become an execution input.

---

# 12. Permanent Parameter Ownership

## Indicator owns
Any parameter changing:
- market analysis
- evidence
- MTF interpretation
- confidence
- quality
- scenario selection
- analytical Entry/SL/TP
- analytical RR
- presentation

## cBot owns
Any parameter changing:
- broker execution
- account sizing
- margin
- capacity
- daily loss enforcement
- live spread/session broker guard
- trading permission
- managed identity
- retries
- broker protection
- lifecycle

## rule
No duplicated public controls.
One authority, other side read-only reflection.

---

# 13. Permanent cBot Architecture

### Indicator
Market → Evidence → Decision → Scenario → Plan → Presentation

### Contracts
data exchange only; platform-neutral; deterministic IDs/versioning.

### cBot
Intent → Broker Eligibility → Submission → Confirmation → Protection → Lifecycle → Recovery → Account Risk Enforcement

Exactly one live execution authority.

### forbidden
second engine, chart scraping, static globals, reflection, cloud dependency, multi-position expansion.

---

# 14. Permanent cBot Runtime State Machine

WAITING
→ RECEIVE
→ VALIDATE
→ EXPIRY
→ REVISION
→ DUPLICATE
→ ACCOUNT/BROKER SAFETY
→ PREPARE
→ SUBMIT
→ BROKER RESULT

BROKER RESULT:
- REJECTED → Retry/Circuit
- ACCEPTED/UNCONFIRMED → Reconcile
- CONFIRMED → Adopt

CONFIRMED
→ PROTECTION
→ LIVE MANAGEMENT
→ PARTIAL / TP / SL / CLOSE
→ BROKER CONFIRMATION
→ OUTCOME

RESTART/RECONNECT:
START
→ READ BROKER STATE
→ RECONCILE
→ ADOPT
→ REPAIR PROTECTION
→ RESUME

---

# 15. Permanent Test Matrix

## Analytical
- BUY/SELL mirror
- closed/open candle
- MTF
- Range/Trend/Transition
- OB/FVG
- WaveTrend
- Divergence
- liquidity
- NaN/Infinity
- insufficient data

## Planning
- Entry
- SL
- TP1..TP4
- RR
- obstacle
- source timeframe
- expiry
- scenario coexistence

## Execution
- Market
- Aggressive
- Stop
- Limit
- rejection
- delayed confirmation
- slippage
- duplicate
- partial fill
- manual change

## Lifecycle
- open
- modify
- close
- partial
- cancel
- restart
- reconnect
- orphan
- recovery

## UI
- panel 220/430/700
- fonts
- overflow
- live content
- reload
- chart objects
- label readability

## Persistence
- history write
- history read
- duplicate
- restart
- recovery
- 90-day archive

## Statistical
- deterministic replay
- walk-forward
- bootstrap
- calibration
- ablation
- drift detection

---

# 16. Permanent Phase-Close Record

## Phase-closeout template
Mxx — Title

## Result
VERIFIED COMPLETE / PARTIAL / BLOCKED / FAILED

## Root cause
...

## Changed files
...

## Behavioral changes
...

## Tests
before: FAIL
after: PASS

## CI
Source/Architecture: ...
Runtime Acceptance: ...
cTrader Compile: ...

## Manual Target Terminal
...

## New bugs
...

## Remaining risks
...

## Exact progress
...

## Operator action
git pull required after merge: YES / NO

## Next phase
Mxx+1 — Title

---

# 17. Final Product Target

## Accuracy
- consistent decision evidence
- no double-counted structure
- calibrated confidence
- measured OB/FVG value
- measured exit behavior
- data-backed Range handling

## Performance
- bounded startup
- fast panel content refresh
- bounded caches
- limited redraw
- limited broker reads
- no hot-path file I/O

## Safety
- broker truth
- protective SL
- monotonic TP
- deterministic retries
- restart/reconnect recovery
- fail-closed

## Architecture
- Indicator analysis only
- Contracts data only
- cBot execution only
- one decision authority
- one execution authority

## Maintenance
- one roadmap
- clean repo
- reproducible build
- behavioral tests
- property/mutation evidence
- target-terminal evidence

---

# 18. Current Starting Point

**Canonical implementation start: M2 — Repository Hygiene / Dead Code / Ownership, with the CBOT separation track active; current execution migration phase is CBOT-P4E.**

M0 — Adoption / Freeze / Baseline, M1 — Full Forensic Audit and CBOT-P0 — Activation / Boundary Lock are **VERIFIED COMPLETE**.

No other roadmap, prompt, continuation note or planning document may override this file.

Do not start advanced intelligence, parameter tuning or new indicators before the relevant gates. cBot separation is explicitly active now, but no permanent dual broker executor is allowed: migration is staged, parity-gated and fail-closed.

Once M42 is accepted, the project leaves the remediation/certification track and enters controlled production observation/calibration.


---

# 19. Historical baseline absorbed into this roadmap

این بخش فقط برای حفظ تداوم است. فازهای historical زیر «باز» نیستند مگر M1 خلافشان را ثابت کند.

## CR4.10 / D10 — Native indicator safety and registry performance

CR4.10 / D10 implementation complete. Native indicator readiness, safe warm-up/fail-closed behavior and deterministic registry lookup ownership were hardened without changing public parameter name/type/DefaultValue or trading thresholds. Target-terminal validation remains a separate manual acceptance boundary. The certification transition after the historical CR4.10/D10 remediation is **CR-FINAL**.

## Historical audit string anchors

## Accumulated audit string anchors

These exact historical strings are retained for accumulated repository-audit compatibility. They are continuity markers only and are not additional implementation phases or an alternate roadmap:

- CI-03
- CI-04
- CI-04 closeout
- ## 2.0.1 — Current certification state
- CI-05
- CI-05 implementation record
- CI-07
- CI-08 implementation record
- CI-10 — Trigger and trigger-lifecycle audit
- docs/PHASE-CI-10-TRIGGER-LIFECYCLE.md
- CI-11 — Entry geometry and signal-timing audit
- CI-12 — Structural SL
- docs/PHASE-CI-12-STRUCTURAL-SL.md
- ### CI-13 implementation record
- Status: **VERIFIED COMPLETE — PR #168 merged to `main`.
- CI-14
- CI-15
- CI-16
- CI-17
- CI-17A
- ### CI-17 target-terminal acceptance package — 2026-10-02
- CR-FINAL

- CR7.5 / G5
- CR7.6a

## Historical audit continuity anchors

The following identifiers are retained solely so accumulated repository audits can prove historical continuity after the roadmap consolidation. They are not additional implementation phases and do not override the M0–M42 order:

- CR4.4
- CR4.5
- CR4.6
- CR4.7
- CR4.8
- CR4.9
- CR4.10 / D10
- CR5.3 / E3
- CR5.4 / E4
- CR5.5 / E5
- CR5.7 / E7
- CR5.8 / E8
- CR6.6 / F7 closeout
- CR6.7 / F8
- CR6.7 / F8 closeout
- CR6.8 / F9
- CR6.8 / F9 closeout
- CR6.9 / F3
- CR6.9 / F3 closeout
- CR7.1 / G1
- CR7.3 / G3 closeout
- **Next phase: CR7.4 / G4**

Historical target-terminal validation remains a separate manual acceptance boundary; no anchor above claims terminal certification.

## Phase 7.4 — MaximumOpenPositions semantics

Historical continuity marker preserved: the single-plan `Maximum Open Positions` semantics remain an audited invariant. This is historical continuity only; it is not a new implementation phase in the M0–M42 execution sequence.

## CR4.4 — Numerical stability and caching continuity

CR4.4 is a completed historical numerical-stability/caching remediation. Its production work remains under the existing OSS adapter/cache owners. Target-terminal validation is a separate manual acceptance boundary and is **not claimed as completed** by this roadmap.

## Track 19 — OSS Numerical Benchmark

Track 19.1 numerical benchmark is a completed historical research/validation milestone. Its detailed continuity and benchmark evidence are retained in:
`docs/TRACK-19-OSS-NUMERICAL-BENCHMARK.md`

This track does not authorize production package promotion or numerical policy tuning.

## Foundation already established
- Canonical source/architecture boundary
- Modular indicator/analysis decomposition
- Decision and planning separation
- Risk/sizing separation
- Automatic market execution decomposition
- Aggressive execution decomposition
- Pending Stop/Limit decomposition
- Broker protection ownership work
- Panel decomposition
- Regime-aware intelligence
- Runtime fault containment
- staged startup foundation
- deterministic decision/planning/execution/runtime contracts
- OSS adapter/research separation
- numerical benchmark foundation
- scenario identity and multi-scenario observation semantics

## Previously completed review/correction families
- runtime fault containment
- closed-bar Reaction semantics
- CHoCH structural semantics
- daily loss foundation
- EOD boundary foundation
- asynchronous news retrieval
- calibration-key equality
- FVG/OB direction-aware obstacle scanning
- aggressive RR guard
- existing-stop health checks
- fill acceptance direction
- HTF absolute strength
- bounded Skender computation/cache work
- memory account scoping
- execution/presentation state decomposition

## Current inherited gate
Target-terminal validation remains the final evidence boundary for current repository behavior until M39/M40 closes it.

The historical work is not reimplemented merely because its old phase number disappears from the current roadmap. M1 can reopen a historical behavior only if code/tests/runtime evidence prove regression or missing coverage.

---

# 20. Mapping of the old planning vocabulary into the new master roadmap

این نگاشت فقط برای اثبات پوشش کامل است؛ برای اجرای آینده باید فقط M-phaseهای همین فایل استفاده شوند.

| Old group | Absorbed into |
|---|---|
| P0 | M0, M1 |
| P1 / CI-15 | M8, M9 |
| P2 / CI-16 Replay | M11, M12 |
| P3 / CI-17 Target Terminal | M39 |
| P4 / CI-FINAL | M40 |
| P5 / H4-H6 | M3, M7, M2 |
| P6 / Prompt 9 I2-I7 | M5, M1, M7 |
| P7 / Prompt 10 J1-J4 | M7, M5, M2 |
| P8 / Prompt 11 K1-K4 | M21, M2, M7 |
| P9 independent code review | M1 |
| P10 structural/testability | M10, M27, M28 |
| P11 final integration | M40, M41 |
| P12 cBot separation | M29-M38 |
| S1 Replay/Walk-forward | M11-M13 |
| S2 calibrated confidence | M16 |
| S3 ablation | M14 |
| S4 parameter reduction | M15 |
| S5 MAE/MFE exits | M19 |
| S6 time/session | M13, M20 |
| S7 dynamic risk | M21 |
| S8 statistical cut-off | M22 |
| S9 spread/liquidity guard | M20 |
| S10 news guard | M20 |
| S11 cBot/server execution | M29-M38 |
| S12 watchdog/external alerts | M6, M22 |
| S13 data quality | M13 |
| S14 swing importance | M24 |
| S15 anchored VWAP/volume | M25 |
| S16 meta-labeling | M26 |
| S17 property/mutation testing | M27 |
| S18 why-no-trade | M23 |
| I1-I9 | M1, M3, M4, M5, M28 |
| J1-J4 | M5, M7, M2 |
| K1-K4 | M2, M7, M21 |
| G1 remainder | M8, M9 |

No item above is allowed to create a second roadmap or an alternate implementation order.

---

# 21. Canonical known-risk inventory

M1 must explicitly scan and disposition at least these risk families:

### Analysis
- evidence double counting
- structure/MSS/CHoCH correlation
- closed/open candle contamination
- MTF source contamination
- Range center/edge behavior
- OB/FVG mitigation and direction
- WaveTrend OS/OB validity
- divergence/swing equality
- liquidity false positives
- hidden threshold changes

### Planning
- Entry/SL/TP direction
- reward-path obstacles
- target ladder ordering
- RR after normalization
- expiry
- simultaneous scenarios
- source timeframe attribution

### Execution
- market/aggressive distinction
- pending Stop/Limit distinction
- slippage
- final volume
- broker rejection
- delayed confirmation
- duplicate attempt
- retry storm
- capacity
- account permission
- protection mutation

### Lifecycle
- opened/modified/closed events
- pending events
- partial close
- BE
- trail
- TP progression
- recovery
- orphan adoption
- restart/reconnect

### Presentation
- panel stale content
- panel overflow
- panel sizing
- label readability
- timeframe labels
- line length
- line extension
- marker identity
- alert/sound synchronization
- stale/blocked marks

### Persistence
- history location
- write/read failure
- duplicate rows
- account scope
- session/day boundary
- 90-day retention
- recovery after restart

---

## Realtime / Live Execution + Volume Profile Intelligence — 2026-10-03

Status: implementation branch codex/realtime-live-signal-unification-2026-10-03; PR #240 remains the integration boundary. Repository CI must pass before merge; target-terminal acceptance remains manual.

This current unit establishes:
- current ActionableNow scenarios -> immediate Market/Aggressive cBot execution;
- future FutureOrderReady scenarios -> Pending Stop/Limit placement;
- multiple independent ScenarioIds with bounded cBot-owned concurrency;
- explicit LIVE/DEMO account routing with live arms default OFF;
- 100 ms signal-store handoff cadence;
- magnitude filtering for tiny stagnant-market setups and stronger RANGE RR protection;
- canonical arrow/popup direction synchronization;
- explicit cBot attachment/presence truth and queued alert-audio delivery;
- M15 closed-bar Volume Profile evidence (POC/VAL/VAH) cached by closed M15 index and used as bounded contextual confluence in opportunity quality/ranking.

The Indicator legacy Maximum Open Positions remains the certified single-plan analytical capacity contract. The cBot owns broker-side simultaneous ScenarioId capacity so the two concerns are not incorrectly coupled.

Phase records:
- docs/PHASE-REALTIME-LIVE-SIGNAL-UNIFICATION-2026-10-03.md
- docs/PHASE-VOLUME-PROFILE-EVIDENCE-2026-10-03.md

No claim of profitability, prediction accuracy or target-terminal certification is made by repository code/CI alone.

## 21A. 2026-10-03 — Realtime All-Timeframe Intelligence / Smart Arrow Hardening

Implementation branch: phase/mtf-realtime-all-engines-smart-arrows-2026-10-03.

Scope completed in this implementation unit:
- all aligned closed frames M1/M5/M15/M30/H1/H4/D1/W1 continue to feed the common analysis/decision stack;
- Early Prediction now has an explicit all-timeframe fusion owner over M5/M15/M30/H1/H4/D1/W1;
- live trend-strength presentation is recalculated from all active frames plus the current quote, with H1+ as higher-timeframe authority;
- the chart displays weak/medium/strong 1/2/3-arrow intensity tiers as a presentation state only;
- stagnant-market magnitude floors and RANGE RR floor are strengthened to reduce tiny low-value trade plans;
- cBot attachment binding is exact-instance aware and emits deterministic chart-indicator diagnostics when unresolved;
- current scenarios remain immediate Market/Aggressive actions, future scenarios remain Pending Stop/Limit orders;
- cBot-side concurrent ScenarioId capacity remains bounded independently from the Indicator legacy analytical single-plan contract;
- audio remains under the existing canonical queued delivery processor; actual audibility still requires target-terminal verification.

Non-goals:
- no claim of guaranteed profitability or predictive accuracy;
- no automatic live arming by default;
- no replacement of the M15 canonical decision/reference role;
- no bypass of shared RR, structural-risk, spread, margin, daily-loss or broker-confirmation gates.

Verification:
- PR/CI must pass on the latest branch head;
- cTrader target-terminal checks remain required for actual live broker mutation, sound audibility and attachment behavior.

Operator action after merge: git pull --ff-only.
# 22. Final cleanup rule for obsolete planning material

Once M41 is accepted:

1. This ROADMAP.md remains.
2. Evidence produced by actual acceptance/testing remains only when it has operational value.
3. Historical planning prompts, duplicate roadmaps and superseded execution plans are removable after dependency verification.
4. No source code, test or CI workflow may depend on a planning-only document.
5. No deleted document may be the hidden source of implementation order.
6. Git history remains the archival record.
7. The repository must contain exactly one active development roadmap: this file.

---

# 23. No-silent-scope-expansion rule

During M0-M42, a newly discovered issue is handled as follows:

- Critical safety defect: can interrupt the current phase and becomes part of the current phase only if necessary to leave the system safe.
- Direct regression blocking the current acceptance: current phase expands only enough to restore the invariant.
- Non-critical defect: register it here with owner and target phase; do not silently change scope.
- New feature idea: postponed until M42 unless required for an existing acceptance contract.
- New indicator: forbidden before M14.
- Threshold tuning: forbidden before OOS evidence.
- Production live auto-execution may be implemented through the cBot as an explicitly armed capability, but remains default-OFF and requires target-terminal evidence before operational adoption.

This prevents the roadmap from becoming an endlessly expanding patch queue.
## MTF-P3 — Primary M15/H1 Provider Scenario Identity Cohesion

Status: **VERIFIED COMPLETE — repository gates passed on implementation HEAD `5f6a9873a27b3b1edfa139ab21d19c1b5faa6bdc`; target-terminal identity verification remains manual.**

The provider bridge must preserve the exact ScenarioId/SourceTimeframe of the canonical
candidate used by the execution-facing plan. Provider SourceTimeframe may not be derived
from the Indicator chart attachment timeframe when a scenario candidate is available.

Implementation scope:
- reuse canonical scenario identity from the existing registry/resolution path;
- derive provider SourceTimeframe from the exact candidate;
- pass the same source timeframe into canonical execution-intent identity;
- remove hard-coded pending Stop/Limit scenario identifiers in favor of the canonical
  direction scenario identity;
- accumulate deterministic runtime/static verification.

This phase changes traceability only. It does not change strategy thresholds, RR, Entry,
SL, TP, confidence, risk, broker authority or the M15/H1 observe-only execution policy.

Phase report: `docs/PHASE-MTF-P3-PRIMARY-PROVIDER-IDENTITY.md`.

## Panel Geometry Correction — 50px Bottom Clearance + Hidden Restore Position

Status: **VERIFIED COMPLETE — repository gates passed on implementation HEAD `b63f82e1d1fca9ef3af2d7fbbe34e779099ab7d7`; target-terminal visual confirmation remains manual.**

The Indicator panel bottom clearance is corrected from 100px to exactly 50px. The hidden
restore button for BottomLeft/BottomRight positions receives the same 50px bottom clearance.
This preserves the reserved lower chart area for the separate cBot surface without the
previously excessive gap.

No strategy, signal, Decision, Plan, RR, Entry/SL/TP, risk or execution authority changes.

Phase report: `docs/PHASE-PANEL-CLEARANCE-RESTORE-POSITION.md`.

## Indicator Naming + cBot Launch + MTF Panel Direction Correction

Status: **VERIFIED COMPLETE — repository gates passed on implementation HEAD `b360761d13df94ac098e8bfc626ed2985499742d`; target-terminal name/visual confirmation remains manual.**

Operator-facing corrections:
- Indicator display name: **CFIP Smart Indicator**;
- cBot display name: **CFIP Smart Execution Bot**;
- cBot default host timeframe: **M5**;
- MTF panel distinguishes resolved BUY/SELL from BULL BIAS/BEAR BIAS and true NEUTRAL;
- obsolete Quick Execution height reservation is removed from the live panel render path.

No strategy/threshold or broker authority change is allowed in this phase.

Phase report: `docs/PHASE-INDICATOR-NAME-CBOT-LAUNCH-MTF-PANEL.md`.
    
### CI-20C closeout — 2026-10-02

Status: IMPLEMENTATION COMPLETE — verification pending for the CI20C branch.

CI20C establishes explicit same-chart exact-instance cBot binding and heartbeat freshness are separated, makes Indicator execution capability require a Running cBot plus fresh state, and adds event-driven cBot rebind on Indicator Added/Removed/Modified.

The panel now distinguishes **NOT ATTACHED**, **STOPPED/RESTARTING**, **CONNECTING**, and **CONNECTED** instead of conflating these states.

No strategy threshold, RR/Entry/SL/TP policy, position capacity or Cloud transport was changed. The cBot remains demo-only.

Target-terminal startup/restart/reconnect, panel latency and broker synchronization remain manual acceptance boundaries until evidenced.
### Realtime all-timeframe intelligence + smart arrows
This current hardening includes the all-timeframe realtime intelligence path, M15 canonical decision/reference, M5 entry refinement, optional M1 confirmation, future pending-order scenarios, nine-level HTF smart arrows, realtime panel truth and modular audio ownership.

### VOLUME PROFILE EVIDENCE
The Volume Profile evidence phase is part of the consolidated realtime/live architecture and remains contextual evidence only.


## 2026-10-03 — Panel footer / alert sound / popup visibility correction

The latest UI feedback resulted in a second hardening pass on the same panel/alert phase. The implementation keeps the full five-event alert history in memory, but renders a compact two-message rail; adds the MTF rail to the final panel-height calculation; enlarges the shared panel lamps and color-locks timeframe labels to the lamp semantic color; and adds a signal-event sound gate so parallel WATCH/REACTION/ACTION alerts do not produce duplicate or triplicate audio for the same M5 event. The indicator/cBot execution architecture is unchanged: analysis and panel presentation remain in the Indicator, while broker execution remains cBot-owned.


## 2026-10-03 — Live alert footer relayout hardening

The panel alert rail now performs an immediate lightweight footer geometry refresh on delivery, so timer-delivered alerts are visible without waiting for a later full panel render. Signal sound deduplication uses a bounded recent event set keyed by symbol/signal/bar/direction, preventing interleaved WATCH/REACTION/ACTION events from producing repeated audio while retaining the compact two-message visual rail.

## 2026-10-03 — Third-pass panel/alert/terminal correction

Current panel correction is now based on actual visible geometry rather than a large fixed footer reserve. The Footer content minimum is 40px, the two-line MTF rail is 38px, outer panel padding is counted only once, and alert text is no-wrap/ellipsis within the audit-required 20px row. Signal-family sound is deduplicated across semantic stages for one closed-M5 event, and MTF arrows have their own chart-object namespace.

cTrader Local/Cloud: the repository does not add Cloud execution. cTrader documentation states that cloud synchronisation makes created/installed algorithms and updates available across apps; Windows/Mac can adjust synchronisation, while custom indicators execute locally on Windows/Mac and cBot local/cloud execution is selectable. This makes repeated Local/Cloud reconciliation a terminal synchronization/instance-state issue rather than a CFIP source-code execution path.

## 2026-10-04 — Modern Signal Lines / Single Presentation State

Status: IMPLEMENTED — repository verification pending; target-terminal visual acceptance remains manual.

Completed in this phase:
- Preserved the existing canonical PlanLineRenderer as the only production owner for signal/plan line geometry; no second line renderer was introduced.
- Introduced PlanLevelVisualState as the single presentation-state owner for ENTRY, IDEAL_ENTRY, TRIGGER, SL, TP1..TP4 and ACTIVE_TP visibility/distinctness.
- RenderLevelLines and RenderPlanLabels now consume the same level state instead of independently recalculating visibility/distinctness.
- Trigger visibility now has one canonical rule for line and label presentation, preventing line/label disagreement.
- All signal/plan lines retain the established visual contract: Solid, 1px, finite 40-bar span ending at the latest candle, no infinite extension.
- Semantic colors remain intact with one canonical full-opacity cTrader-like treatment (255) applied uniformly to every signal/plan line. Plan labels are now white text inside compact filled boxes using the exact associated line color; the box is attached to the line's exact right endpoint. Modernization remains one consistent presentation path rather than a parallel visual language.
- Pending and parallel opportunity lines continue to delegate to PlanLineRenderer; no alternate geometry owner was added.
- Routine single-owner / no-duality audit remains part of the phase acceptance.

Verification:
- Source changes implemented on the phase branch.
- Local cTrader build is not available in this environment.
- GitHub CI/PR verification is required before claiming repository gates green.
- Target-terminal visual acceptance remains required for actual chart appearance, label spacing and stale-object behavior.

Operator action after merge: git pull --ff-only.


## 21B. 2026-10-04 — Smart Trend Arrow Recovery / Single-Owner Lifecycle

The canonical MTF trend-arrow path was recovered on main. Trend arrows are independent of trade actionability, use one nine-level strength owner, and render once per calculation cycle. Weak/medium/strong remain three levels each; M1 remains a Circle precision marker. See docs/PHASE-SMART-TREND-ARROWS-RECOVERY-2026-10-04.md.


## 2026-10-04 — Arrow + Signal-Line Deep Presentation Audit

Canonical presentation re-audited end-to-end. Final contract: one MTF trend-arrow owner with 9 strength levels (1–3 weak, 4–6 medium, 7–9 strong), M1 Circle precision marker only; signal/plan lines Solid + 1px + exactly 40 chart-bar geometry ending at latest candle; labels are regular-weight white text in a filled box matching the line color, with the box attached to the exact line endpoint. Historical audits that contradicted this current contract were reconciled so they cannot reintroduce duplicate/obsolete rendering paths.


### 2026-10-04 — Follow-up canonical arrow/line audit cleanup
- Re-verified the live MTF arrow path: `MtfTrendStrengthRule → SignalVisualSnapshot → SignalStackedArrowRenderer → CalculationLiveCycle`; BUY/SELL remain explicit UpArrow/DownArrow, M1 remains Circle, and 9-level strength maps to 1/2/3 glyphs within each tier.
- Re-verified plan-level geometry: Solid, fixed 1px, finite 40-bar span ending at the latest chart candle; compact labels use a filled semantic-color box with white regular text and the exact line endpoint as the right edge.
- Removed obsolete arrow calculations from the non-owner signal renderer and consolidated arrow-color resolution under the canonical stacked-arrow owner; no parallel renderer was introduced.
- Runtime/build verification remains pending on the user's local cTrader environment.


### 2026-10-04 — Unified trend-strength / signal-presentation consistency
- The MTF trend remains the single strength source: 9 levels, grouped into WEAK/MEDIUM/STRONG with 3 levels each; the arrow stack maps level 1/2/3 within each tier to 1/2/3 arrows.
- Canonical arrows are now decision-aware: when an active/actionable canonical trade direction conflicts with the MTF trend direction, contradictory directional arrows are suppressed rather than presenting BUY/SELL visually at the same time.
- Event, opportunity, and historical markers no longer use the canonical UpArrow/DownArrow glyphs; they use triangle markers so directional arrows remain uniquely owned by the live MTF trend presentation.
- Canonical watch/action alerts and Entry box text now expose the same MTF trend tier/level, keeping audio/event delivery, chart arrows, signal levels and box text tied to the same snapshot context.
- The signal-level geometry contract remains unchanged: Solid, 1px, finite 40-bar span ending at the latest candle; labels remain filled semantic-color boxes with white regular text.
- Local Release build/runtime verification is still required.


## 2026-10-04 — Semantic Alert Sound Diversity / Single Audio Owner

Status: **IMPLEMENTED — local Release build and target-terminal audio acceptance remain required.**

Completed:
- Kept `AlertDeliveryProcessor` as the single physical audio playback owner; no second sound engine or playback path was introduced.
- Expanded the canonical `AlertEngine.ResolveAlertSoundType` mapping across the five cTrader built-in notification cues: `PositiveNotification`, `NegativeNotification`, `Announcement`, `Confirmation`, and `Doorbell`.
- Semantic families now distinguish important events: WATCH/EARLY, REACTION/AUTO-REACTION, ACTION/HIGH/SMART, TP, SL/INVALID/RESTRICT, REVERSAL, pending/fill/execution outcomes and the remaining critical/general alerts.
- Sound deduplication no longer collapses WATCH/REACTION/ACTION stages into one audible cue for the same M5 event. The canonical `AlertKey` is now part of the bounded sound fingerprint, so distinct semantic stages can have distinct sounds while exact repeats remain suppressed.
- Existing custom `SoundFilePath` fallback behavior remains intact; it was not turned into a second per-event audio owner.
- Added the semantic sound-diversity checks to the existing alert/footer architecture audit.

**Source of the sounds:** these semantic cues are not downloaded from a third-party sound library. They are the five built-in `cAlgo.API.SoundType` notification sounds provided by cTrader Algo itself. The implementation calls cTrader's `Notifications.PlaySound(SoundType)` at the existing canonical delivery boundary. cTrader's official documentation lists these built-in types and the `PlaySound(SoundType)` API.

Verification boundary:
- Source changes are committed, but no local `dotnet build` was run in this environment.
- Actual audibility and whether the five cues are sufficiently distinct must be confirmed in the user's target cTrader terminal.
- If later we want more than these five built-in timbres, that must be designed as one canonical per-event custom-file mapping rather than adding another playback path.

Operator action: `git pull --ff-only`, then run the Release build and test each alert family in cTrader.


## 2026-10-04 — Signal Quality / Useful TP / Entry-Risk Contract Hardening

Status: **IMPLEMENTED — local Release build and target-terminal/replay verification remain required.**

Deep audit found a real contract split in the signal-quality chain:
- canonical TP1 minimum is 2.00 by default, but the regime RR calculator was lowering RANGE to 1.75;
- the RANGE classifier contained a duplicated 2.25 RR gate, but its evaluator supplied Tp1RR = 0, so that gate was effectively fail-open;
- candidate reward-distance policy for RANGE was only 1.00 ATR, allowing geometrically small targets to survive upstream despite the stronger intended range-quality contract.

Root-level correction:
- MinimumRequiredRiskRewardCalculator now keeps RANGE TP1 RR at at least 2.25 when adaptive structural RR is enabled; it can no longer lower the canonical floor to 1.75.
- RegimeAdaptiveRewardFloorRule now requires 2.25 ATR minimum candidate reward distance for RANGE candidates.
- RangeSignalQualityRule no longer owns a duplicate/stale TP1 RR gate. Actual TP1/RR validation remains at the canonical reward-risk/actionability boundary using the real materialized Entry/SL/TP geometry.
- Obsolete Tp1RR = 0 plumbing and the dead LOW-RR branch were removed from the range classifier contract and decision-contract fixtures.

This is a root-contract correction, not a new parallel signal filter. Entry quality, M15 canonical decision authority, M5 trigger precision, structural evidence, divergence/trap checks, spread/news/session safeguards and final reward-risk validation remain part of the existing chain.

cTrader supports server-side multi-level TP/protection plans, but no broker/execution architecture was changed in this phase. citeturn0search0turn0search1

Verification:
- Source changes committed.
- No local dotnet build was available in this environment.
- Decision-contract compile/test execution is still required locally.
- Target-terminal and historical replay verification are required to measure whether weak signals disappear without eliminating valid low-risk/high-reward setups. cTrader supports visual backtesting/Market Replay and custom optimisation criteria for this type of validation. citeturn3search0turn1search0

Operator action: git pull --ff-only, then run the Release build and the decision-contract test project before chart/runtime validation.
