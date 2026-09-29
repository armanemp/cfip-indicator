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