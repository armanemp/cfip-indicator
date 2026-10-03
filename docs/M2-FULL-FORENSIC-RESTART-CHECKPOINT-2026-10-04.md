# M2 Full Forensic Restart Checkpoint — 2026-10-04

Status: **ACTIVE / IN PROGRESS / NOT CLOSED**

## Purpose

M2 is intentionally restarted from the beginning of the project rather than continuing from a partially consumed checklist. The objective is a gap-free forensic audit from the repository's earliest relevant architecture through the current implementation HEAD.

This is a **re-audit**, not permission to discard already-corrected work. Existing verified corrections remain authoritative unless the fresh audit proves that their current implementation is incomplete, duplicated, contradictory, unreachable, or owned by the wrong layer.

## Non-negotiable audit law

For every behavior, feature, state, calculation, presentation, event, persistence action, contract and execution action:

1. exactly one semantic owner;
2. exactly one source of truth;
3. exactly one production decision path;
4. consumers may read/transform according to the contract but may not silently recreate the owner's semantics;
5. no parallel fallback, compatibility implementation, duplicate calculation, duplicate rendering, duplicate audio, duplicate execution or duplicate persistence for the same behavior;
6. historical documentation/audits may describe old architecture only when explicitly marked historical and cannot masquerade as current authority.

Fix the existing root owner. Do not add a second implementation to hide a conflict.

## Complete audit order

The restart proceeds in these gates, in order, one complete gate/phase at a time:

### M2-A — Repository and build truth
- recursive file inventory;
- duplicate/near-duplicate files;
- partial classes and split ownership;
- project references and compile includes;
- conditional compilation;
- generated/obsolete/unreachable files;
- dependency direction;
- Contracts platform neutrality;
- scripts/audits/baselines versus production truth;
- stale docs and contradictory current-owner claims.

### M2-B — Architecture and ownership
- Indicator / Contracts / cBot boundary;
- analysis, decision, signal, plan, execution and broker-confirmation ownership;
- M15/M5/M1 role contract;
- ScenarioId / PlanId / Signal identity ownership;
- every mutation authority;
- every cross-layer observer;
- duplicate state machines and hidden global state.

### M2-C — Full calculation chain
Trace every production calculation from market data through:
Market Data → MTF Context → Structure → OB → FVG → WaveTrend → divergence/evidence → regime → trend strength → Decision → Entry → SL → TP → RR → normalization → actionability.
For each calculation: owner, inputs, units, timeframe, spread treatment, lifecycle, caching, invalidation, consumers, duplicate formulas and conflicting thresholds.

### M2-D — Signal and visual semantics
- SignalVisualSnapshot;
- nine-level arrow ladder;
- direction/strength separation;
- M1 precision marker;
- spacing/overlap/stale-object lifecycle;
- PlanLevelVisualState;
- PlanLineRenderer;
- PlanLabelRenderer;
- all chart object namespaces;
- panel/chart semantic parity.

### M2-E — Alert/audio/event transport
- canonical event identity;
- queue/dedup/re-arm;
- popup rendering;
- sound owner;
- blocked-event silence;
- email/other delivery;
- lifecycle subscription duplication;
- stale async callbacks.

### M2-F — cBot runtime/execution
- realtime clock ownership;
- OnTimer/OnTick duplication;
- binding/settings/store reload;
- request → broker mutation → report → confirmation;
- pending orders;
- protection;
- recovery;
- simultaneous scenarios;
- account/risk/margin/spread;
- broker-confirmed truth.

### M2-G — Persistence/history/calibration
- LocalStorage ownership;
- buffered persistence;
- command/report lifecycle;
- runtime logs;
- outcome/history;
- 90-day history/calibration;
- import/export/portability;
- synchronous I/O in hot paths;
- retention ordering and terminal-state identity.

### M2-H — Performance/lifecycle
- startup;
- initialization;
- timer cadence;
- chart/UI responsiveness;
- O(N) scans;
- allocation pressure;
- repeated serialization/deserialization;
- event hookup/teardown;
- shutdown/quiescence;
- stale callbacks;
- unnecessary work at the source.

### M2-I — Documentation/test/audit closure
- every current architecture statement;
- every verifier assertion;
- every negative/absence assertion;
- every stale baseline;
- test ownership;
- CI coverage;
- deterministic runtime contracts;
- build graph;
- target-terminal acceptance matrix.

## Finding policy

Every finding receives:
- unique M2 finding ID;
- exact owner/path;
- root cause;
- impact;
- canonical owner;
- remediation;
- evidence commit;
- verification state;
- whether it is production, documentation/audit, historical-only, or intentionally retained.

No finding is marked closed merely because code was edited.

## Closure evidence

M2 cannot be called complete until:
- Source/Architecture passes on the exact final HEAD;
- Runtime Acceptance passes on the exact final HEAD;
- cTrader Compile/Build passes on the exact final HEAD;
- build graph/Contracts boundary is proven;
- full audit checklist has a disposition for every item;
- current docs and machine audits agree;
- target-terminal acceptance requirements are explicitly listed and either evidenced or clearly marked manual;
- no unresolved duplicate/parallel production owner remains.

CI/runtime/terminal PASS must never be inferred from static inspection.

## Current restart position

Current branch:
`phase/M2-repository-hygiene-ownership-current`

Current HEAD at checkpoint creation:
`42e9566dd2ed2ce1e564de99ff55d16c85e73186`

PR:
`#257`

PR state:
- open;
- non-draft;
- mergeable=false;
- one commit behind main at the latest inspected state;
- no usable workflow/status result for the current HEAD, therefore no CI PASS is claimed.

Already root-corrected during the current M2 effort and must be re-verified, not blindly repeated:
- broker lifecycle subscription/unsubscription;
- transactional Indicator event hookup;
- startup panel rendering removal from polling;
- shutdown/quiescence ordering;
- duplicate realtime consumption clock;
- centralized scenario protection sweep;
- deterministic scenario revision tracking;
- management request-status contract;
- management terminal Expired versus Confirmed distinction;
- deferred management persistence/report processing;
- Request* management naming;
- stale current-owner documentation;
- stale audit exception removal;
- targeted duplicate-owner sweep;
- canonical panel refresh ownership.

## Exact next continuation point

**Do not jump to a new feature.**

Restart the audit at **M2-A.1: repository/build truth**, then progress sequentially through M2-A → M2-I.

The next chat must read this checkpoint and continue from the first not-yet-closed M2 gate. It must not assume that a prior summary means a gate is complete.

