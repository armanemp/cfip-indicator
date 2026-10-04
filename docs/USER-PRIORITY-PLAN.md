# CFIP — User Priority Work Plan

This file is the persistent implementation program for the user's requirements.
It is a continuity artifact and must be read together with ROADMAP, ARCHITECTURE
and DEVELOPMENT-LOG before future implementation work.

## A. Permanent execution rules
- One self-contained implementation phase per response.
- Exact progress and exact verification status are reported; completed work is
  not claimed until the relevant gates pass.
- Repository documentation records important decisions, findings and the next
  continuation point.
- Refactor the correct owner; do not add patches, duplicate business logic, a
  second decision authority, a second execution engine or redundant gates.
- Broker-confirmed state remains authoritative.
- Accuracy/performance improvements are not claimed without appropriate evidence.
- Tell the operator exactly when a local pull is required; never assume it happened.

## B. Immediate UI/runtime priorities
1. Panel startup: useful state should appear quickly; initialization must remain
   truthful and must not block the UI with unnecessary work.
2. Panel freshness: panel refresh must not depend on a successful full analysis
   pass. Calculation staleness and render staleness must be distinguishable.
3. Panel performance: bounded timer work, lazy UI allocation, cached/unchanged
   visual state and no redundant property writes.
4. Panel readability: compact, readable ordering of the most operationally useful
   state; avoid redundant rows.
5. Auto Trading / Auto Orders: visible state and runtime state must stay synchronized
   through one authoritative runtime owner; safety/recovery blocks must remain
   authoritative; no manual BUY/SELL controls.

Phase 5.5 completed the visual-level and toggle foundation.
Phase 5.6 completed responsive panel refresh, lazy rows, render optimization and
calculation-freshness diagnostics. Full target-terminal interaction validation
remains part of runtime certification.

## C.1 User strategy-quality overlay — 2026-09-29

These requirements are permanent inputs to future strategy-quality phases:

- important market levels must remain a first-class part of analysis and execution planning;
- Order Block analysis should be developed as deeply as the existing architecture safely allows, including geometry, displacement, structure, mitigation/retest, freshness, liquidity context, MTF alignment and confluence rather than a simplistic candle label;
- signal quality should be improved through better evidence quality, independence, regime relevance and structural confirmation, not by indiscriminately raising thresholds;
- indicator/analyzer coordination should become more coherent and "smart" through one shared decision/relevance framework;
- Entry, SL and TP should continue to be derived from meaningful structure, liquidity and reward-path geometry, with important levels considered before execution;
- improvements must preserve BUY/SELL symmetry, closed-bar safety, broker authority and the single decision/execution ownership model.

This overlay is persistent. Future phases should explicitly check whether important
levels, Order Block quality, signal quality and cross-analyzer coordination are
being improved without creating duplicate authorities.

## C. Signal-quality priority
Goal: stronger, cleaner signals with weak setups rejected, without blindly making
filters so strict that good setups disappear.

Required approach:
- do not solve quality by simply raising one threshold;
- separate direction, setup quality, confidence-like score and executable readiness;
- control correlated/evidence duplication;
- use regime-conditioned evidence relevance;
- validate BUY/SELL symmetry;
- keep prediction/watch distinct from confirmed decision;
- use genuinely distinct information before adding new indicators;
- make no-trade reasons explicit and deterministic.

Roadmap owners: Track 6 (bar semantics), Track 7 (parameter semantics), Track 8
(analytical correctness), Track 9 (decision intelligence/calibration).
Outcome validation must use replay/backtest/observed outcomes appropriate to the claim.

## D. Strong Entry / SL / TP levels
Goal: use meaningful structure, liquidity, invalidation and reward-path levels.

Rules:
- reuse existing planning authorities;
- never duplicate level formulas in UI or execution code;
- structural stop and reward-path validation remain authoritative;
- broker-confirmed live SL/TP stays distinct from intended levels;
- BUY/SELL geometry must remain symmetric unless an explicit rule says otherwise.

Roadmap owners: Track 8 (FVG/OB/structure/liquidity correctness), Track 16
(reward path), Track 17/22 (live progression and structural management), Track 24
(anti-lookahead/accuracy certification).

## E. Higher RR
Goal: improve reward/risk structure only when the reward path is structurally valid.

Rules:
- prefer HTF structure/liquidity/reward-path-supported targets;
- regime-aware minimum RR may adapt when explicitly justified;
- do not manufacture high RR by moving TP through strong obstacles;
- do not weaken SL quality merely to increase displayed RR;
- planned maximum reward and broker-confirmed active target remain separate.

Roadmap owners: Track 9.4, Track 16, Track 17 and Track 22.
Any claim of improved realized RR requires outcome evidence.

## F. Safer automatic trading / order placement
Goal: reduce rejected/duplicate submissions and preserve deterministic broker state.

Permanent invariants:
- one managed identity;
- one automatic execution authority;
- one keyed submission retry policy;
- no synthetic fills or synthetic pending states;
- fail closed after recoverable runtime faults;
- reconcile after restart/reconnect;
- no duplicate broker mutation owners.

Roadmap owners: Track 2.2+, Track 3, Track 4, Track 10, Track 12, Track 13 and
Track 23.

Acceptance includes deterministic rejection handling, duplicate-entry prevention,
risk authority preservation and broker-state convergence.

## G. Smart trailing / maximum profit capture
Goal: protect profit while allowing valid continuation and avoiding premature exits.

Rules:
- trailing is protective-only;
- never worsen existing protection;
- use structural progression, target stage, live RR and broker constraints;
- keep continuation room when structure supports it;
- confirm every broker mutation;
- rejected modifications use bounded retry/reconciliation rather than every-tick
  hammering;
- partial TP, break-even, trailing and target progression must converge to one
  broker-confirmed lifecycle state.

Roadmap owners: Track 12, Track 17, Track 22.2/22.3 and Track 23.

## H. Whole-system optimization
Optimize the complete indicator rather than one isolated function:
- asynchronous startup;
- bounded scans and caches;
- reuse of stable closed-state calculations;
- lightweight timer supervision;
- minimal UI work when no state changed;
- no full analysis from the safety heartbeat;
- hot-path cost proportional to new information, not full history;
- separate measurement of startup, tick path, timer and UI cost.

Performance improvements must preserve trading semantics.

## I. Uploaded archive continuity
Previously audited archive findings remain part of the plan:
- WaveTrend: candidate for future composite momentum evidence only; not an
  independent duplicate vote because its RSI/MFI/RMI components overlap existing evidence.
- TPO Profile file: empty; no usable logic.
- Existing production FVG engine remains authoritative.
- Direct external network/economic-data coupling does not enter the live execution core.

Future OSS adoption requires explicit adapter boundary, source/version/license/
compatibility evidence, deterministic fixtures and benchmark evidence.

## J. Certification sequence
Current next dependency after Phase 5.6: Phase 6.1 — Decision closed-bar contract.

Then:
6.2 reaction intrabar contract → 6.3 aggressive entry policy → Track 7 parameter
semantics → Track 8 analytical correctness → Track 9 signal-quality/decision
intelligence → Tracks 10–13 risk/execution/runtime certification → Tracks 16–17
reward/live progression → Track 22 protection/trailing → Tracks 23–25 stress,
replay and accuracy certification → release/cloud/learning.

## K. Cross-chat continuation
Read this file, ROADMAP, ARCHITECTURE and DEVELOPMENT-LOG; inspect main; confirm
the exact HEAD; start from the first incomplete dependency; complete one phase;
run the relevant gates; update continuity docs; state whether pull is required.

## L. New persistent top-down requirement — 2026-09-29

The user's requested signal architecture is explicitly top-down:

- search for the primary opportunity from H1 and higher (H4/D1/W1 when available);
- establish directional HTF anchor before treating lower-timeframe movement as a candidate;
- calibrate through M30/M15;
- use M5 for setup/location and M1 only for closed-bar trigger confirmation;
- actionable signal state should require the lower layers to agree with a strong HTF anchor;
- lower timeframes must not override a strong HTF directional conflict;
- reward planning should prefer HTF structure/liquidity when the reward path is valid;
- risk/protection quality must not be weakened merely to increase displayed RR.

This is a persistent engineering requirement for future phases.

## M. Persistent panel/live-management requirement — 2026-09-29

Panel behavior must remain responsive independently from full analysis:

- heartbeat should update only lightweight live rows/clock;
- full panel layout/render should be state-change driven;
- unchanged UI properties must not be rewritten;
- panel calculation staleness and render staleness should remain distinguishable.

Live target/protection management:

- closed-bar structural safety remains mandatory;
- structural progression must not wait for a later M5 boundary when a bounded live pulse can safely re-evaluate already-closed M5 structure;
- successful TP1/TP2 events should trigger immediate same-cycle re-evaluation of the next reward path;
- broker target/stop changes remain broker-confirmed and monotonic.

Chart semantics:

- persistent alert-mirror labels must identify themselves as ALERT rather than looking like a second independent SIGNAL engine.


## N. Phase 9.1 completion note — 2026-09-29

The top-down architecture requirement is now persistent and implemented: H1+ opportunity anchor -> M30/M15 calibration -> M5 setup/location -> M1 closed trigger.

Panel responsiveness and live-management requirements are also persistent and implemented in Phase 9.1. Future phases must preserve lightweight heartbeat behavior, state-change-driven full panel rendering, prompt same-cycle TP target refresh, and monotonic broker-confirmed progression.


## O. Phase 9.2 persistent requirement — 2026-09-29

The engine must maintain two simultaneous opportunity concepts:
- Strategic top-down opportunities using H1+ anchor -> M30/M15 calibration -> M5 setup -> M1 confirmation.
- Tactical lower-timeframe opportunities evaluated independently when their own risk/reward and evidence justify them.

A strong HTF conflict must not erase a potentially worthwhile LTF setup automatically. It should instead place the candidate in a stricter Counter-HTF lane.

The chart must be able to display several candidate setups simultaneously with isolated visual IDs and separated label anchors. Existing line length geometry must remain unchanged.

Price-level labels must use solid backgrounds matching the associated line color and automatically choose readable text contrast.

WaveTrend from the user's latest source ZIP is part of the evidence stack, but it remains confirmation evidence and not an independent signal authority.


## P. Phase 9.2 completion note — 2026-09-29

The persistent project contract now includes parallel opportunity preservation:
Strategic top-down opportunities and worthwhile Tactical LTF opportunities must coexist.

A strong HTF conflict should tighten the LTF candidate's quality/RR requirements rather than automatically erase it.

The chart must support multiple isolated candidate setups without changing established line lengths/geometry. Compact level labels must use one shared native `ChartText` presentation with no background, 10px regular typography, exact line color, and identical geometry/typography for BUY and SELL. The visible end of the text must remain exactly one chart bar before the canonical line start.

The user's CUSTOMWAVETREND source from the latest ZIP is part of the evidence stack. Its numerical parity still requires target-terminal replay.


## Phase 9.3 completion record — 2026-09-29

The next accuracy refinement is now implemented as contextual empirical calibration rather than another static threshold layer. Historical outcomes are keyed by direction, opportunity lane, M5 regime and confidence bucket, with exact/context/directional fallback and shrinkage toward a 50% prior. Calibration is applied only after lane resolution, so Strategic calibration cannot erase independently qualified Tactical or Counter-HTF opportunities.

Only broker-closed managed plans carrying a valid calibration context contribute observations. The canonical visual snapshot and panel expose the adjustment and observed historical win rate without presenting the rate as a probability.

PR #42 contains the implementation and deterministic contract coverage. cTrader/live replay remains the authority for measuring whether false-signal rate and realized risk/reward improve.


## Q. Permanent phase discipline — 2026-09-29

Every subsequent implementation phase must explicitly include and document all
of the following as one continuous system, not isolated feature work:

- complete source/architecture audit and accumulated regression audit;
- whole-system performance optimization where safe;
- analytical quality review across indicators, structure, liquidity, FVG,
  Order Block, WaveTrend, MTF and regime coordination;
- signal-quality review covering false signals and missed valid opportunities;
- Decision -> Signal -> Alert coherence;
- Entry/SL/TP and reward-path integrity;
- automatic execution/order-placement safety, broker confirmation and lifecycle
  protection;
- outcome/history learning and deterministic verification;
- explicit evidence of what is automated versus what still requires target-terminal
  replay or live validation.

No phase may optimize one isolated metric while allowing another stage of the
Analysis -> Decision -> Signal -> Alert -> Execution -> Broker -> Protection ->
Outcome -> Learning chain to regress.


## R. Phase 9.16 persistent signal-quality requirement — 2026-09-29

Signal quality refinement must be measurement-first:

- capture the closed-M5 stage where a directional candidate is stopped;
- distinguish consensus failure, decision filtering, trigger failure and actionability failure;
- compare rejection cohorts with forward MFE/MAE before changing thresholds;
- use OB/FVG/structure/liquidity/indicator/WaveTrend evidence as separate causal categories;
- treat OB+FVG as the strongest single location feature but never as a global override;
- keep measurement observational and out of the live decision authority;
- use target-terminal/replay evidence before changing default thresholds or claiming quality gains.

This requirement is persistent across future chats and phases.


## S. Phase 9.17 exit/protection integrity requirement — 2026-09-29

Exit management is part of the same signal-to-trade chain. Every future refinement must
preserve these invariants:

- BUY TP progression is strictly upward; SELL TP progression is strictly downward.
- A live TP must remain beyond the current executable market price.
- A live TP may advance only; it must never regress to an already-passed target.
- SL progression may only become more protective.
- Actual broker fill must become the new geometric reference for Entry/SL/TP.
- Server-side partial TP state must be reconciled to broker-confirmed position volume.
- After partial realization, remaining targets may be restructured only in the trade's
  favorable direction.
- Broker minimum stop/TP distance must be honored.
- No global exit threshold should be changed without replay evidence.


## T. Primary M15/H1 signal hierarchy — 2026-10-02

The preferred visible signal hierarchy is now:

M15 + H1 = primary signal sources;
M5 = local structure/location/actionability tuning;
M1 = optional closed-trigger timing confirmation.

M15 and H1 signals must be able to coexist at the same time, including opposite directions. Lower-timeframe tuning may refine readiness but must not silently erase a valid primary source signal.

Source-timeframe OB/FVG evidence must remain attributable to the M15/H1 frame. No new global OB/FVG override or hard threshold is introduced without replay evidence.

The Indicator chart-panel AUTO TRADE and AUTO ORDERS quick controls are not part of the presentation surface. Their canonical execution status/diagnostic information may remain available as text. The Indicator panel must retain explicit bottom clearance so the separate cBot panel can occupy the lower chart area without overlap.


## U. Primary M15/H1 location evidence — 2026-10-02

For primary M15/H1 signals, the selected FVG and Order Block evidence must remain attributable to the same closed source frame. OB+FVG confluence remains the strongest single location feature under the existing canonical location owner. M5 and M1 refine timing/readiness but must not replace or hide the source-frame location evidence.

The panel should expose source OB/FVG evidence for each primary M15/H1 candidate. Any future threshold or weight change must be supported by deterministic replay/OOS evidence rather than visual preference.


## V. Primary M15/H1 provider identity cohesion — 2026-10-02

The primary signal hierarchy is not complete until its identity survives the provider
boundary. For a canonical matched scenario, ScenarioId and SourceTimeframe must remain
the same from the Indicator registry through the read-only provider and into cBot shadow
telemetry.

The provider must not infer SourceTimeframe from the chart attachment when an exact
scenario candidate is available. Pending Stop/Limit identity must reuse the same canonical
scenario resolver rather than introducing parallel identifiers.

This is traceability only. No threshold, signal quality rule, Entry/SL/TP geometry, risk
rule or broker authority may change under this requirement. Empirical M15/H1 quality
changes remain a separate replay/OOS concern.


## W. Indicator/cBot names + MTF panel directional display — 2026-10-02

Operator visibility requirements:
- Indicator must appear in cTrader as **CFIP Smart Indicator**;
- cBot must appear as **CFIP Smart Execution Bot** and default to M5 for the initial host chart;
- MTF context rows must not call a directionally biased but unresolved frame NEUTRAL;
- use **BULL BIAS / BEAR BIAS** for directional context and reserve BUY/SELL for resolved
  direction;
- this presentation correction must not change signal authority or introduce a second
  strategy score.

The separate cBot remains the execution host; this phase only improves initial usability,
naming and diagnostic correctness.
