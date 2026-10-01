# CR7.5 / G5 — Panel execution/protection state freshness and broker-read minimization

Date: 2026-10-01

Status: **VERIFIED COMPLETE — PR #145 merged to `main` as `e2674b9800159ba1266639ad96a374f622aff555`.**

## Scope definition

G5 closes the freshness/performance gap exposed by the verified G4 panel-state owner.

The G4 snapshot was invalidated unconditionally inside `BuildPanelPresentationKey`. Because
the presentation key is built whenever the panel evaluates whether a full render is needed,
that placement could force repeated `GetManagedPosition()`, `GetManagedPendingOrder()` and
protection evaluation even when no authoritative execution or broker state had changed.

G5 therefore establishes one bounded freshness contract:

- presentation-key construction is read-only and does not force cache invalidation;
- execution/runtime state changes invalidate immediately;
- broker lifecycle events invalidate immediately;
- block, recovery and server-TP-ladder state changes invalidate through guarded state owners;
- the existing broker refresh cadence remains the backstop when a terminal event is not surfaced;
- no new broker enumeration, decision authority, execution path or public parameter is added.

## Root cause

G4 introduced a reusable panel execution/protection snapshot but placed invalidation at the
start of the presentation-key builder. The cache was therefore reusable only during one key
build instead of across unchanged refresh attempts.

The defect is an ownership/freshness issue in the presentation path, not a strategy-rule
change: presentation was driving freshness instead of the authoritative runtime/broker inputs.

## Implementation

### Presentation layer

`PanelRenderOptimization.BuildPanelPresentationKey` no longer calls
`InvalidatePanelExecutionProtectionStateCache()`.

The key builder still consumes the canonical G4 state helpers, but it no longer causes broker
object enumeration merely to decide whether the visible panel changed.

### Broker lifecycle

`MarkBrokerStateDirty()` now invalidates the panel snapshot immediately.

`SynchronizeLiveBrokerState()` invalidates on each due refresh before broker facts are
read again. The existing one-second broker-state cadence is retained as the bounded stale-state
backstop.

### Runtime/lifecycle inputs

The canonical runtime and lifecycle state setters invalidate when their values change.

The direct block-reason, broker-protection-recovery and server-side TP-ladder-active fields
are now guarded properties. Existing assignments therefore cannot silently bypass the panel
snapshot invalidation rule.

### Execution authority

No broker mutation or execution validation logic was changed. Automatic market, aggressive
and pending paths continue to use their existing authorities.

## Deterministic verification

The G5 Runtime Acceptance contract verifies the bounded broker refresh semantics.

Repository verification on final implementation head `1a2572e2f72e8842640e9c1cbea88e6868f05354`:
- Source/Architecture: **PASS** — run #2321;
- Runtime Acceptance Contracts: **PASS** — run #2130;
- cTrader Compile: **PASS** — run #2314.

PR #145 was then merged to `main` as `e2674b9800159ba1266639ad96a374f622aff555`.

- dirty state refreshes immediately;
- never-refreshed state refreshes immediately;
- unchanged state stays cacheable before the one-second interval;
- the one-second boundary refreshes;
- clock regression refreshes instead of serving stale state.

The dedicated `tools/audit_phase_7_5.py` checks the source ownership boundary and ensures:

- the presentation key does not force invalidation;
- broker dirty and due-refresh paths invalidate;
- runtime/lifecycle state owners participate;
- direct block/recovery/server-ladder inputs cannot bypass invalidation;
- broker enumeration remains inside the canonical G4 snapshot owner.

## Safety boundary

- no public parameter name/type/`DefaultValue` changed;
- no confidence, RR, entry, SL, TP, risk or execution threshold changed;
- no new decision/execution/broker-mutation authority;
- broker-confirmed state remains authoritative;
- single-plan/single-managed-identity semantics remain unchanged.

## Performance contract

For unchanged state, panel key construction no longer forces a fresh managed-position or
pending-order lookup on every rebuild attempt.

Freshness is event-driven for known changes and bounded by the existing one-second broker
refresh backstop for missing/late lifecycle events.

No claim is made here about exact cTrader frame latency; target-terminal measurement remains
required.

## Whole-project audit

The permanent analysis → decision → signal → alert → execution → broker confirmation →
protection/lifecycle → outcome → learning audit remains in force. G5 changes only panel-state
freshness/ownership and does not alter analytical or execution thresholds.

## Manual cTrader boundary

Hands-on validation remains required for:

- panel startup transitions;
- runtime toggle changes;
- broker position/protection changes;
- pending-order changes;
- reconnect/reload timing;
- visible panel responsiveness and stale-state duration.

## Next phase

**CR7.6a** remains the next Prompt 7 slice after G5 repository closeout.
