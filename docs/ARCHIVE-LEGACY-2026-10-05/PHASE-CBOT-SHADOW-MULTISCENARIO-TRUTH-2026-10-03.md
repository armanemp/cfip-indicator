# CBOT Shadow Multi-Scenario Truth — 2026-10-03

Status: IMPLEMENTATION COMPLETE — automated verification PASS; target-terminal acceptance pending.

## Finding

SignalScenarioBatch can carry multiple independent ScenarioId envelopes under the same provider revision. The cBot processes those envelopes sequentially.

The shadow-host gate previously stored one global last accepted revision/idempotency pair. After the first scenario in a batch was accepted, the second independent scenario could therefore be rejected as a revision conflict even though its ScenarioId was distinct.

That created a false single-scenario bottleneck before the actual broker execution coordinators.

## Correction

ShadowHostCoordinator now stores the last accepted revision, idempotency key, cached result and broker-recheck timestamp per ScenarioId.

The provider-side ScenarioBatch materialization path is also bound explicitly to the current closed M5. A stale candidate from the opportunity registry cannot be promoted into the current execution batch.

The global LastAcceptedRevision and LastAcceptedIdempotencyKey properties remain available as telemetry for compatibility, but per-scenario validation is now the execution truth.

Replay semantics remain unchanged within the same ScenarioId:

- same revision + same idempotency key -> duplicate/revalidation path;

- lower revision -> stale;

- same revision + different key -> revision conflict;

- higher revision -> normal validation.

Independent ScenarioIds can now share the same provider revision without competing for a global revision slot.

## Preserved chain

Indicator candidate mining -> M15 decision -> M5 trigger/entry precision -> M1 optional confirmation -> Entry/SL/TP/RR -> Actionability -> Scenario/Plan -> SignalEnvelope/ScenarioBatch -> cBot preflight -> ShadowHost per-ScenarioId truth -> broker reconciliation -> execution -> confirmation -> protection -> outcome/history.

## Safety

- live accounts remain blocked;

- cBot remains the sole broker mutation owner;

- M15 remains canonical trade decision/execution reference;

- M5 remains trigger/tuning/entry precision;

- M1 remains optional confirmation;

- existing RR, quality, margin, spread, market-hours, daily-loss and concurrent-scenario limits are unchanged;

- no analytical threshold is lowered;

- no new execution authority is introduced.

## Verification

Automated:

- Source/Architecture: PASS;


- Runtime Acceptance: PASS;

- cTrader Compile/Build: PASS;

- dedicated shadow multi-scenario audit;
- stale-candidate/current-closed-M5 materialization audit.

Target terminal:

- batch containing two independent ScenarioIds with the same revision;

- same-scenario replay;

- independent broker reconciliation/protection;

- restart/rebind;

- panel aggregate state.

Operator action after merge: git pull --ff-only on local main.
