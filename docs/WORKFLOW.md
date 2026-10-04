# CFIP Indicator — Workflow

## Canonical continuation rule

One assistant implementation response = one complete atomic work package.

**Canonical control plane is only:**
1. `docs/CFIP-ROADMAP.md` — macro order/objectives
2. `docs/CFIP-LIST.md` — exact files and atomic work-package scope
3. `docs/CFIP_GATE.md` — acceptance/evidence/defect closure

When resuming in another chat/account:
1. Read all three canonical files first.
2. Inspect the actual current `main` branch and latest CI.
3. Find the first executable work package marked `NEXT` in `docs/CFIP-LIST.md`.
4. Use `docs/CFIP-ROADMAP.md` for macro order and `docs/CFIP_GATE.md` for acceptance/evidence.
5. Other documents are implementation/reference material only and cannot override the three canonical files.
6. Do not repeat a completed package unless a regression is proven.
7. One response must close exactly one complete atomic work package.

## Implementation order inside every phase

Contract -> authoritative implementation -> caller migration -> tests/static checks -> duplicate-path removal -> runtime validation -> documentation.

## Ownership rule

Edit the smallest authoritative module that owns the behavior. Do not introduce compatibility aliases or parallel business rules.

## Source hygiene rule

Production source must be version-neutral.

Versioned/historical source identifiers may appear only in roadmap/workflow material when they are necessary to identify the behavioral baseline. They must not appear in production C#, configuration, broker comments, class names, file names, namespaces, labels, or runtime identities.

## Permanent repository artifact hygiene rule

Every atomic work package and every global checkpoint must inspect the entire tracked repository for unnecessary artifacts, not only source code and `docs/`.

Every tracked file must be classified as **KEEP / MIGRATE / ARCHIVE / DELETE**. This applies to source, tests, tools, audits, configuration, documentation, logs, screenshots, generated files, and temporary/debug artifacts.

A DELETE/ARCHIVE decision requires active consumer/dependency tracing plus project-inclusion, build, CI, audit, test, runtime, release, deployment, and continuity checks. Valid knowledge/dependencies must be migrated to the canonical owner before deletion.

No artifact may remain solely because it is old, familiar, or referenced by another obsolete artifact. The final repository must contain no unexplained tracked artifact and no duplicate artifact representing the same behavior, authority, or contract.

## Modularity rule

Every indicator, analyzer, detector, model, enum, execution policy, lifecycle handler and renderer gets one clear source-file owner.

A file may contain multiple small declarations only when they are inseparable implementation details of the same primary artifact and the architecture verifier explicitly permits them. Nested helper/model types inside the cTrader host are not permitted.

## Trading safety rule

The strategy plan and broker lifecycle are different states.

- Accepted submission is not fill confirmation.
- A rejected broker action is not successful state.
- Broker-confirmed state is authoritative.
- SL changes are protective-only.
- Partial close and close state are consumed only after confirmation.
- Reconnect/restart reconciliation runs before assuming new lifecycle state.
- Automatic market and pending execution share one managed identity and one strategy authority.
- Manual entry controls remain absent.

## OSS rule

OSS is isolated under `oss/` or an explicit adapter/benchmark boundary.

Before production adoption, record upstream source, license, compatibility with target cTrader/.NET, benchmark results, numerical fixture results and the exact adapter owner.

Do not embed a second trading engine.

## Required verification for every phase

Check affected source ownership, references/callers, public parameter compatibility, dependency direction, BUY/SELL symmetry, side effects, failure paths, idempotency, and runtime authority.

Update roadmap status only after the phase is actually complete.

## Current execution policy

Work directly against the GitHub repository state. Use versioned/historical information only for continuity documentation, never as production identity.


## Baseline synchronization rule

Phase 0.1 must keep release-critical documentation aligned with machine-enforced facts. A verifier failure caused by missing continuity documentation blocks phase completion even when the verifier's architecture summary itself is valid.

## Cross-chat project continuity

The repository itself is the continuity source. The three canonical control-plane files are authoritative for planning, inventory and acceptance.

At the start of a new chat, read:
- `docs/CFIP-PROMPT.md`;
- `docs/CFIP-ROADMAP.md`;
- `docs/CFIP-LIST.md`;
- `docs/CFIP_GATE.md`;
then inspect actual `main`, latest CI and the first `NEXT` atomic package. `docs/WORKFLOW.md`, `README.md`, `docs/ARCHITECTURE.md` and `docs/DEVELOPMENT-LOG.md` are supporting/reference documents and cannot override the canonical control plane.

The first executable work package marked `NEXT` in `docs/CFIP-LIST.md` is the only implementation unit to execute in that response; macro objectives come from `docs/CFIP-ROADMAP.md` and acceptance from `docs/CFIP_GATE.md`.

Record every completed atomic work package in `docs/DEVELOPMENT-LOG.md`, including:
- phase and status;
- implementation summary;
- important findings/fixes;
- verification results;
- relevant commit SHA(s);
- next phase;
- operator pull requirement.

The operator should normally pull once at a completed phase boundary, after the final verified commit for that phase. Intermediate implementation commits do not require a pull unless the operator needs them locally.


## Permanent full-project audit rule

Every implementation phase must be accompanied by a whole-project audit, not only a local change review. The standing audit covers the production source tree, public parameter declarations and consumers, exact duplicate method/parameter signatures, module ownership, decision/execution authority boundaries, signal/level/presentation synchronization, BUY/SELL symmetry, safety invariants, and roadmap/development-log continuity.

The permanent machine gate is `tools/audit_project_integrity.py`. It is wired into Source / Architecture CI and must remain green for every phase. Specialized audits such as parameter semantics and runtime UI audits remain layered on top of it.

A phase is not considered complete until:
- the implementation and callers are synchronized;
- the full-project audit is green;
- the phase-specific audits/contracts are green;
- cTrader compile is green;
- documentation records the findings, decisions, verification and next phase;
- the operator is explicitly told whether a local pull is required.

This rule is intentionally persistent across chats. The repository documentation is the continuity source; do not rely on conversational memory alone for phase state or audit obligations.


## Permanent auto-trade / auto-order hardening rule

Every implementation phase must make a concrete improvement to the automatic trading and/or automatic pending-order pipeline, even when the primary phase topic is analysis, UI or protection.

The standing checklist is:

- refresh the current decision/actionability state immediately before automatic market, aggressive and pending submission;
- preserve one submission identity/gate and one managed strategy authority;
- validate capacity, market suitability, spread/risk, volume and SL/TP geometry before broker mutation;
- prefer broker/server-owned advanced protection whenever the current cTrader API can express the intended protection safely;
- ensure smart structural SL, TP selection, break-even and target progression never create competing mutation authorities;
- keep server-confirmed state authoritative after submission/fill;
- audit duplicate orders, duplicate alerts, duplicate TP/SL mutations, stale decision state and direction conflicts;
- record the accumulated findings and fixes in the phase log and acceptance matrix.

## Permanent signal-line presentation rule

All signal/plan level lines must use `LineStyle.Solid`, fixed thickness 1, and finite 40-bar geometry ending at the latest chart candle. Compact level labels are native `ChartText`, background-free, white 10px regular-weight text, right-aligned and positioned exactly one chart bar to the left of the canonical line start. This is the current canonical UI contract and must be checked by phase audits.

Alert mirror text such as `ALERT BUY/SELL` is not a signal authority and must remain absent from chart level presentation. Expired/stale pre-trade visuals must be removed by a bounded lifecycle owner, and blocked/restricted candidates must produce no sound, email or visual-alert side effect.



## Permanent outcome / recovery telemetry rule

Broker-confirmed managed closes are the authoritative source for outcome observations. Outcome history must remain bounded, position-id idempotent, and separate from broker mutation authority.

Empirical calibration may prefer a bounded recent lifecycle window only after the existing sample gates are satisfied. Recovery-only reconstructed plans must remain non-calibratable because their original decision context is unavailable.

Submission and recovery telemetry is observational: it may retain bounded history and expose diagnostics, but it must not create a second execution authority or alter broker-confirmed state.



## Permanent optimization routine

Every implementation phase must include an optimization-readiness review.

The review must separate:
- predictive quality: false-signal rate, calibration error, realized R distribution;
- risk quality: drawdown, loss clusters, adverse excursion and risk-of-ruin proxies;
- execution quality: rejection/null-result rate, slippage, entry extension and broker protection integrity;
- computational quality: repeated heavy loops, unbounded history, unnecessary recalculation and state churn.

Parameter optimization must be multi-objective and must not be judged by raw win rate alone. Candidate parameter changes require replay/out-of-sample evidence before default values are changed.

Persistent memory may inform calibration and conservative risk scaling only through bounded, configuration-scoped, broker-confirmed outcomes. It must never become a second directional decision engine or a broker mutation authority.


## Permanent phase execution discipline

Every implementation phase must treat the complete trading chain as one system:

Analysis -> Decision -> Signal -> Alert -> Execution -> Broker confirmation ->
Protection/Lifecycle -> Outcome -> Learning.

Each phase must include a fresh source/architecture audit, accumulated regression
audit, safe performance optimization, analytical and signal-quality review,
Entry/SL/TP and reward-path review, automatic order/execution review, broker-state
verification, history/learning review, and explicit separation between automated
verification and target-terminal empirical validation.

A phase is not considered complete merely because one isolated indicator or
signal module changed successfully.


## Permanent signal-measurement rule

Signal-quality phases must add measurement before adding more global thresholds. The
canonical closed-M5 trace must identify the decision gate and exact rejection reason,
and offline forward-window analysis may be used to form potential-missed cohorts.

Trace data is observational and cannot become a second directional or execution authority.
Any change to live thresholds/defaults must be supported by replay/out-of-sample evidence.


## Permanent exit-geometry rule

Live exit management must use the actual executable market side as its geometric reference:
BUY uses Bid for TP/SL progression checks; SELL uses Ask. A target that has already been
passed by market cannot be restored, and a stop cannot become less protective. Server-side
advanced protection is broker-owned, but its ladder must be reconciled to the same monotonic
plan geometry after partial realization.


## Main branch canonicalization rule

All final project changes must land on `main` as the single canonical implementation state. Work branches/PRs are temporary delivery mechanisms only and must not become parallel long-lived project states. After verification and merge, the authoritative state is `main`; continuation, roadmap, gate, and development-log references must point to that merged state. Do not leave the same completed behavior implemented or maintained independently on multiple branches.

## Local execution handoff
When a work package needs user-local execution, the assistant must hand off the exact commands and their execution context, expected success signal, and required returned output. This is mandatory for every applicable package and must not be inferred from CI status.

## Permanent green-and-merge completion rule

A work package is not considered complete, and the assistant must not issue its completion response, until every required CI gate for the exact final commit is green and the verified work package has been merged into `main`. If any gate is pending or failed, continue inspecting, fixing the canonical owner when needed, rerunning verification, and waiting for green results. After green CI, verify the actual merged `main` state before declaring the package complete. This rule applies to every atomic work package and every phase, without exception.
