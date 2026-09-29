# Phase 7.4 — MaximumOpenPositions Semantics

Date: 2026-09-29

## Objective

Make execution-capacity semantics truthful and coordinated across every automatic execution path.

The current architecture is a single managed-plan system. The former public `MaximumOpenPositions` setting exposed values above 1 even though the execution-capacity owner rejected them and the runtime did not provide true multi-plan lifecycle ownership.

## Decision

CFIP remains single-active-plan / single-managed-position.

The unsupported numeric parameter is removed instead of being clamped to 1 while still advertising configurability.

## Implementation

- `ExecutionCapacityRule.AllowsNewSinglePlan(bool hasManagedOpenPosition)` is now the single capacity rule.
- `ValidateSinglePlanCapacity(out reason)` is the shared guard.
- Automatic market pre-trade, aggressive pre-trade and predictive-pending execution all use that guard.
- Late numeric checks based on `MaximumOpenPositions` were removed.
- The managed-position existence test remains broker/identity based.
- No broker mutation ownership, plan identity, or risk ownership was changed.

## Signal → Trade coordination audit

This phase inspected the authoritative chain:

`closed MTF context → decision → filters → trigger readiness → plan → execution mode/intent → pre-trade eligibility → broker`.

The audit confirmed that the capacity defect was a semantics/authority mismatch, not a second decision engine.

Important observations retained for the next analytical phases:

1. Decision creation is bound to the canonical closed M5 context and required MTF frame indices.
2. Direction, confidence, Smart Quality, structural confirmations and TriggerReady remain separate decision fields.
3. Plan creation continues to require `EntryAllowed` and `TriggerReady`.
4. Execution-zone selection already considers M5/M15 FVG and Order Block candidates plus their overlap; this remains owned by planning and is not reimplemented in execution.
5. The reported false-signal problem is not solved by this capacity phase and is not hidden behind a threshold increase.
6. The next analytical audit must examine real M1 trigger semantics, then swing equality, FVG mathematics, Order Block source/displacement/BOS-MSS/liquidity/mitigation/freshness/confluence, and evidence/regime relevance before any empirical accuracy claim.

## Acceptance

- No production public parameter advertises unsupported multi-position capacity.
- One semantic capacity rule and one guard own the invariant.
- Automatic market, aggressive and predictive-pending paths use the same capacity guard.
- No `MaximumOpenPositions` references remain in production C#.
- BUY/SELL and broker-authority invariants are unchanged.
- Full-project audit and phase-specific capacity audit must pass.
- Runtime acceptance and cTrader compile must pass.

## Verification boundary

Automated CI can prove the source/semantic/compile contracts above. It cannot substitute for hands-on cTrader runtime behavior, broker permissions, order fills, reconnects or chart interaction. Those remain part of target-terminal validation.

## Continuity

Next phase: Phase 8.1 — M1 trigger correctness.

Operator pull: required only after the final verified Phase 7.4 merge.
