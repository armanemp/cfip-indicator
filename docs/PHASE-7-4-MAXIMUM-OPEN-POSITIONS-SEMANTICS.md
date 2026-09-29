# Phase 7.4 — MaximumOpenPositions Semantics

Date: 2026-09-29

Status: verified and merged into main.

Merge: PR #32, commit 29881e1a89eb0c8d92f2465234a6c3a44671154a.

## Objective

Make execution-capacity semantics truthful and coordinated across every automatic execution path.

The current architecture is a single managed-plan system. It does not provide true multi-plan lifecycle ownership.

## Decision

CFIP remains single-active-plan / single-managed-position.

The public `MaximumOpenPositions` setting is retained for preset/API compatibility, but its parameter contract is constrained to:

- DefaultValue = 1
- MinValue = 1
- MaxValue = 1

A malformed/internal value above 1 is rejected fail-closed by the platform-neutral capacity rule.

The separate `BlockNewSignalWhileActive` setting was removed because blocking a second plan is a mandatory architecture invariant, not an optional mode.

## Implementation

- `ExecutionCapacityRule` owns the platform-neutral single-plan capacity semantics.
- `AllowsNewSinglePlan(...)` requires supported capacity, no local plan, no managed position and no managed pending order.
- `AllowsNewSingleExecution(...)` requires supported capacity, no managed position and no managed pending order.
- `ExecutionCapacityGuard` is the cTrader state-to-rule adapter.
- Plan creation consumes `ValidateSinglePlanCapacity(...)`.
- Automatic market, aggressive and predictive-pending execution consume `ValidateSingleExecutionCapacity(...)`.
- Duplicate late-path comparisons against `MaximumOpenPositions` were removed.
- Existing pending-order checks remain where they have a distinct post-cleanup/revalidation role; they are not a second numeric capacity rule.

## Signal → Trade coordination audit

This phase inspected the authoritative chain:

`closed MTF context → decision → filters → trigger readiness → plan → execution mode/intent → pre-trade eligibility → broker`.

The capacity change does not alter signal scoring or claim an accuracy improvement.

The audit confirmed:

1. Decision creation remains bound to the canonical closed-M5 context and required MTF frame indices.
2. Direction, confidence, Smart Quality, structural confirmations and TriggerReady remain separate decision fields.
3. Plan creation remains guarded by `EntryAllowed` and `TriggerReady`.
4. Capacity is now one shared invariant rather than a configurable value interpreted differently by multiple execution paths.
5. The reported false-signal problem remains an analytical issue for Track 8/9, not something to hide with higher thresholds.
6. Concrete analytical risks are now explicitly recorded for the next phases:
   - M1 direction contributes to decision score while canonical TriggerReady remains M5 closed-bar based;
   - structural/liquidity facts can influence both score and downstream gates;
   - structural confirmations can overlap across M5/M15/H1/H4;
   - Order Block quality needs deterministic source-candle, displacement, BOS/MSS, liquidity sweep, FVG confluence, mitigation/freshness and remaining-width semantics.

## Acceptance

- Public configuration cannot advertise multi-position execution.
- One semantic capacity rule and one cTrader guard own the invariant.
- Automatic market, aggressive and predictive-pending paths use the same broker-capacity boundary.
- No unsupported `BlockNewSignalWhileActive` parameter remains.
- Deterministic runtime contracts cover empty, occupied, pending and unsupported-capacity cases.
- Full-project integrity, semantic parameter, runtime UI and architecture audits remain green.
- Runtime Acceptance Contracts and cTrader Compile must pass.

## Verification boundary

Automated CI can prove source ownership, semantic contracts and compilation. It cannot substitute for hands-on cTrader runtime behavior, broker permissions, fills, reconnects or chart interaction.

## Continuity

Current public surface: 532 parameters = 529 baseline + 3 OSS extension parameters.

Next phase: Phase 8.1 — M1 trigger correctness.

Operator pull: required only after the final verified Phase 7.4 merge.
