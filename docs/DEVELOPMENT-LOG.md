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
