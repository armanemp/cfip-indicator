# CBOT-P3 — cBot Host / Shadow

Date: 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

## Goal

Turn the cBot from a passive provider reader into a deterministic shadow execution host.

P3 performs:

`receive → contract validation → revision ordering → deduplication → broker/account safety check → shadow eligibility → telemetry`

It does not submit, modify, close or cancel any broker order/position.

## Implemented

Production shadow layer:

- `src/CFIP.cBot/Shadow/ShadowHostContracts.cs`
- `src/CFIP.cBot/Shadow/ShadowHostValidator.cs`
- `src/CFIP.cBot/Shadow/ShadowHostCoordinator.cs`
- `src/CFIP.cBot/CFIPExecutionBot.cs`

The validator checks:

- ContractVersion;
- required identity fields;
- symbol scope;
- provider revision == envelope revision;
- monotonic revision;
- same-revision idempotency conflicts;
- execution-intent identity lineage;
- finite/positive execution geometry;
- BUY/SELL stop and target-side invariants;
- signal and intent expiry;
- semantic SignalStage.

These are execution-boundary integrity checks. The cBot does not recalculate Indicator evidence, confidence, RR, OB/FVG, WaveTrend, divergence or MTF decisions.

## Revision / deduplication

The coordinator keeps a bounded 128-key idempotency cache.

Rules:

- lower revision → stale;
- same revision + different key → conflict;
- same revision + same key → duplicate/current state is suppressed;
- higher revision → new validation;
- transient broker blocks do not consume the revision and are rechecked against current broker state.

No per-tick persistence or unbounded memory was introduced.

## Broker safety

The cBot reads only:

- managed position/pending capacity;
- managed positions with `CFIP-SMART`;
- managed pending orders with `CFIP-SMART-PENDING`;
- Bid / Ask;
- PipSize.

Single-plan capacity is enforced as:

`managed positions + managed pending orders < 1`.

No broker object is mutated.

## Shadow states

The cBot reports:

`Waiting / Observing / Validating / Ready / Blocked / Expired / Duplicate`.

`Ready` means only that the current Indicator intent passes the shadow boundary. It does not mean submitted, accepted or filled.

## Behavioral test suite

Added:

`tools/CFIP.cBot.Shadow.Tests/`

Coverage:

- valid BUY;
- valid SELL;
- WATCH without execution intent;
- expiry;
- incompatible contract version;
- intent identity mismatch;
- symbol mismatch;
- trading permission block;
- single-plan capacity block;
- invalid quote;
- BUY/SELL wrong-side geometry;
- stale revision;
- revision conflict;
- duplicate idempotency;
- provider/envelope revision mismatch;
- transient capacity block followed by successful same-revision recheck.

The executable is wired into `.github/workflows/ci-build.yml`.

## Source audit

Added:

`tools/audit_cbot_shadow_host.py`

The audit enforces:

- shadow owner presence;
- provider/envelope revision integrity;
- revision/idempotency semantics;
- bounded cache;
- broker-safety seam;
- BUY/SELL safety symmetry;
- explicit execution disarm;
- zero broker mutation;
- no reflection/chart scraping/network transport;
- behavioral fixture presence.

## Architecture preserved

P3 does not:

- move/delete legacy Indicator broker owners;
- activate live broker execution;
- add a second executor;
- duplicate the decision engine;
- tune thresholds or weights;
- move analytical Entry/SL/TP/RR selection;
- add Cloud/HTTP/IPC;
- add a database or hot-path persistence.

The Indicator remains the temporary execution compatibility owner until P4/P5/P6 replacement, parity and physical-removal gates.

## Verification

Required gates:

- Source / Architecture;
- Runtime Acceptance;
- cTrader Compile.

P3 is accepted only on a final phase SHA where all required gates and the deterministic shadow behavioral executable are green.

## Next

**CBOT-P4 — Broker Mutation Extraction**

Start with the canonical Market / Market Range mutation owner, then proceed through Aggressive, Pending Stop, Pending Limit, Cancel, Close/Partial Close, SL and TP mutation. Each extraction requires replacement, caller migration, parity, and physical removal from Indicator.
