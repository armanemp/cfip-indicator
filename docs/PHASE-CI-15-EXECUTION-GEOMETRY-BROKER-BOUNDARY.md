# CI-15 — End-to-end execution-geometry and broker-boundary audit

Date: 2026-10-02

Status: **IMPLEMENTED — awaiting repository verification before merge.**

## Scope

CI-15 closes the boundary between the validated execution intent and the broker
submission path.

The phase verifies one causal chain:

`Decision → Trigger → Entry → SL → TP → Intent → Validation → Broker submission → Broker-confirmed state`

The correction is intentionally narrow. It does not retune strategy thresholds,
change public parameters or move broker authority to the cBot yet. The purpose is
to prevent a validated Entry/SL/TP snapshot from being discarded and then
reconstructed differently before broker mutation.

## Canonical execution-intent geometry

`ExecutionIntentGeometryRule` is the platform-neutral owner for the final
intent geometry projection.

It owns:

- directional stop/target-side validation;
- exact normalized Entry/SL/TP values passed into the intent;
- physical StopPips and TargetPips derived from those exact prices;
- deterministic BUY/SELL mirror behavior;
- fail-closed invalid/non-finite geometry.

`ExecutionIntentBuilder` now consumes this owner rather than calculating its
own pip distances.

`ExecutionIntentValidation` re-checks the same geometry and rejects a mutated
or stale pip projection.

## Broker handoff hardening

### Automatic Market

The final submission stage now retains the validated `ExecutionIntent` and
uses its exact Entry, StopPips, TargetPips, Target and Volume for broker
submission and server-side TP-ladder preparation.

The broker path no longer relies on the independently passed raw values after
the final validation boundary.

### Aggressive Market

The existing intent validation remains intact, but the broker boundary now
consumes the exact validated intent projection for Entry, StopPips, TargetPips,
Target and Volume.

### Pending Stop / Limit

Pending execution already preserved an absolute `ExecutionIntent`; CI-15 makes
the server-side protection ladder consume that same intent snapshot as well,
avoiding a second source for Entry/Target/Volume during submission.

### Broker-confirmed fill

The existing direction-aware `ExecutionFillAcceptanceRule` remains the sole
fill-envelope authority. CI-15 does not weaken favorable/adverse fill policy.

Pending fills retain the absolute pending snapshot as the reconciliation anchor;
broker state remains authoritative after confirmation.

## Submission telemetry

Execution submission telemetry now carries a bounded exact-intent trace containing:

- Entry;
- Trigger;
- SL;
- TP;
- StopPips;
- TargetPips;
- Volume;
- source M5.

This makes the submitted intent directly comparable with the later broker
confirmation/reconciliation records without creating a second execution authority.

## Deterministic contracts

Planning contracts cover:

- BUY/SELL intent geometry symmetry;
- exact pip projection;
- wrong-side fail-closed behavior;
- invalid pip-size behavior;
- physical sub-pip distance preservation.

## Static audit

`tools/audit_phase_ci_15.py` verifies:

- one canonical execution-intent geometry owner;
- intent-builder and intent-validation ownership;
- exact intent handoff for Market/Aggressive/Pending Stop/Pending Limit;
- exact-intent telemetry;
- preservation of the single fill-envelope owner;
- accumulated Source/Architecture workflow wiring;
- continuity documentation.

## Safety / non-goals

- No public parameter name/type/default changed.
- No confidence, decision, entry, SL, TP, RR, risk or execution threshold was tuned.
- No new broker mutation authority was introduced.
- No cBot separation was started in this phase.
- Recovery paths that intentionally reconcile against broker-confirmed state remain
  broker-authoritative.

## Verification boundary

Required:

- Source/Architecture accumulated gate including CI-15;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- Planning Contracts.

Still manual:

- exact target-terminal cTrader quote timing;
- broker-specific fill/slippage behavior;
- pending-order fill timing;
- reconnect/reload behavior;
- empirical signal quality and profitability.

## Next phase

**CI-16 — Deterministic replay, latency and counterexample suite.**
