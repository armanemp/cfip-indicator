# CBOT-P4E — Management Command / Broker Mutation Authority — 2026-10-02

## Status

**VERIFIED COMPLETE — merged to `main` via PR #197 as `996cd9cb4872608674269c205b7a03b33ab9e812`.**

P4E finishes the remaining broker-mutation owner extraction after P4D. The Indicator is now command-only for management mutation requests; the cBot is the sole broker-mutation owner for the migrated paths.

### Migrated mutation paths

- Pending-order cancellation
- Full position close
- Partial position close
- Stop-loss mutation
- Absolute take-profit mutation
- TP-by-pips mutation
- Server-side TP ladder mutation

### Canonical transport

`ManagementCommand` is published by the Indicator into a Device-scoped LocalStorage queue.

`ManagementExecutionCoordinator` in the cBot consumes the command, checks broker state, performs the broker mutation, and publishes `BrokerExecutionReport`.

The Indicator removes a command from its queue only after a `Confirmed` report is observed. Requested state is never treated as broker truth.

### Safety invariants

Stop mutations are tighten-only: a requested stop must be protective relative to entry and the live executable quote. A broker stop that is already more protective is treated as satisfied.

Target mutations are forward-only: the requested target must remain beyond entry and the current executable quote. Backward target movement is rejected.

Partial closes carry expected remaining volume. Full close and pending cancellation treat broker-side absence as confirmation, making retries/reconnects safe.

### Physical cleanup

Deleted obsolete Indicator owners:

- `BrokerPendingOrderCancellation.cs`
- `BrokerPositionCloseMutation.cs`
- `BrokerStopLossMutation.cs`
- `BrokerTakeProfitMutation.cs`

No parallel compatibility broker executor was added.

### Runtime boundary

The cBot keeps the existing hard live-account guard. The new management arm is `false` by default. This phase does not authorize live execution.

### Verification

Final merged HEAD `996cd9cb4872608674269c205b7a03b33ab9e812` was accepted by:

- Source / Architecture workflow #3064: **PASS**;
- Runtime Acceptance workflow #2873: **PASS**;
- cTrader Compile/Build workflow #3057: **PASS**;
- CBOT-P4E audit inside Source / Architecture: **PASS**;
- accumulated architecture/parameter/UI/execution/identity audits: **PASS**.

Target-terminal broker execution evidence remains manual and is not claimed by repository CI.

### Operator action

The P4E merge is already on `main`. Local checkout must run:

`git pull --ff-only`

### Next phase

**CBOT-P5 — Protection / Lifecycle / Recovery completion**, focused on durable restart/reconnect/orphan reconciliation and lifecycle ownership without reintroducing broker mutation into the Indicator.
