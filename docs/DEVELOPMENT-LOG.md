# CFIP Indicator — Development and Continuity Log

This file records implementation history so development can resume safely in a new chat without reconstructing prior work from conversation history.

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
