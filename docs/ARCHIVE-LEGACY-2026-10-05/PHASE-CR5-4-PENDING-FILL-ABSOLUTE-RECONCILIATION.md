# CR5.4 / E4 — Pending-order post-fill absolute SL/TP reconciliation

Status: **VERIFIED COMPLETE — repository acceptance closed 2026-10-01.**

## Scope

Close the pending Stop/Limit fill gap where cTrader relative protection is based on
the eventual broker fill price while the indicator had already planned structural
absolute Entry/SL/TP levels.

The pending-fill lifecycle now preserves the original executable intent as an
absolute snapshot, reconciles it against the actual broker fill, and only then
commits managed-plan state and broker protection.

## Root cause verified

Before E4, successful pending placement cleared `_plan`, while the pending
preparation path carried Entry/SL/TP only in `ExecutionIntent`. The subsequent
`PendingFilledHandler` rebuilt a live plan from the broker-filled position,
which could make relative protection the de-facto plan geometry after slippage
or gap.

This was not treated as a parameter-tuning problem.

## Implementation

- `PendingOrderPlanSnapshot` preserves the absolute pending intent across
  successful Stop/Limit placement.
- The snapshot is built from `ExecutionIntent`, not from `_plan`, because
  pending preparation does not maintain a persistent market plan.
- Snapshot creation is fail-closed: a pending order is not adopted unless its
  absolute reference can be constructed.
- The snapshot preserves lane, entry context, structural SL and the canonical
  TP ladder derived from the same target-selection pipeline.
- `ReconcileLivePlanToActualFill` accepts the preserved absolute reference plan.
- `LiveFillExitReconciler` resolves absolute plan exits against broker-confirmed
  protection and the actual fill price before committing the live plan.
- More-protective broker SL and more-progressive broker TP remain authoritative
  when they are safer/progressive than the preserved plan.
- Advanced server-side TP protection is rebuilt in relative form from the
  reconciled absolute TP ladder and actual fill price, then broker-confirmed
  again.
- `PositionOpened` no longer reconstructs a managed position when an outstanding
  pending snapshot is waiting for the matching fill event.
- Pending cancellation clears the preserved snapshot so stale geometry cannot
  leak into a later lifecycle.
- Failed reconciliation keeps lifecycle/execution fail-closed in
  `RecoveryRequired`.

## Deterministic evidence

The runtime contract suite covers:

- positive pending-fill price divergence;
- negative pending-fill price divergence;
- BUY/SELL absolute TP preservation;
- broker-more-progressive TP adoption;
- broker-more-protective SL adoption;
- broker-only protection recovery;
- unsafe target fail-closed behavior;
- deterministic repeated resolution;
- pending-fill lifecycle idempotency by broker pending-order identity.

## Safety boundary

- no public parameter name, type or `DefaultValue` changed;
- no RR, confidence, stop, target or execution threshold tuned;
- no new decision authority;
- no new broker execution authority;
- broker-confirmed state remains authoritative;
- target-terminal broker-fill and protection timing remain manual acceptance items.

## Routine audit and performance

The full flow was reviewed:

Analysis → Decision → Signal → Alert → Execution → Broker confirmation →
Protection/Lifecycle → Outcome → Learning.

No second execution path was introduced. Pending snapshot construction is bounded
by the existing fixed-size target pipeline and occurs once per successful pending
submission, not on every calculation/tick.

## Verification boundary

Repository verification completed on PR #120 final head `6d89f010fa6d292d201ef79e37af5b162438f854`:

- Source/Architecture: PASS — run `36795710379` / workflow #2096, including `audit_phase_5_4.py` and the accumulated routine/optimization audits;
- Runtime Acceptance Contracts: PASS — run `36795710374` / workflow #1905;
- cTrader Compile: PASS — run `36795710377` / workflow #2089.

PR #120 was merged to `main` as merge commit
`782bca41cd37071c79f2cdfa12f712cefc045c8c`.

Target-terminal manual checks remain required for:

- actual Stop/Limit fill-price divergence;
- broker-side final absolute SL/TP after fill;
- Advanced Protection ladder behavior;
- rejection timing and restart/reconnect lifecycle.

The existing CR2.5 lifecycle-ordering audit was reconciled with the new
pending-snapshot invariant: `PositionOpened` may not reconstruct a managed
position while an outstanding pending-fill snapshot exists.

## Transition

After repository verification, the next phase is **CR5.5 / E5 —
Parallel-scenario computation/candidate ownership and MicroReaction safety**.
