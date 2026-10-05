# CFIP Indicator — CI-00 Canonical Data / Price / Time Closeout

Date: 2026-10-01

Status: **COMPLETE — verified on 2026-10-01; ready for merge**

## Objective

CI-00 establishes one calculation-time market context so downstream planning and
execution code does not independently reconstruct Bid/Ask, executable price,
spread, pip/tick scale or MTF reference state.

The existing `MtfClosedContext` remains the single owner of closed MTF indices.
CI-00 composes that context with one canonical quote snapshot instead of creating
a second MTF model.

## Authoritative owners

- `Core/Models/CanonicalPriceSnapshot.cs`
  - Bid / Ask;
  - executable BUY = Ask;
  - executable SELL = Bid;
  - midpoint;
  - spread and spread-in-pips;
  - pip/tick size and digits;
  - broker minimum-distance metadata and explicit distance unit.
- `Runtime/Calculation/CalculationMarketContext.cs`
  - binds host index, signal-reference UTC, quote-observation UTC, canonical
    price snapshot and the existing closed MTF context.
- `Runtime/Calculation/CanonicalMarketContextBuilder.cs`
  - is the only cTrader adapter that constructs the canonical price snapshot
    from `Symbol` and terminal time.
- `Runtime/Calculation/CalculationPreparation.cs`
  - refreshes the quote snapshot on every eligible live calculation even when
    readiness probing is throttled, preventing stale quote data from being
    retained merely because the closed-bar context did not change.
- `Trading/Execution/PriceMath.cs`
  - normalizes price using the canonical tick/digits scale when the canonical
    calculation context exists.

## Migrated consumers

The following high-risk planning/execution consumers now read the canonical
snapshot instead of independently reconstructing quote geometry:

- execution-zone market price;
- market-entry retest tolerance;
- automatic-market spread and executable entry;
- aggressive executable entry, pip conversion and spread-risk sizing;
- plan market spread constraint;
- automatic-market final live-entry plan-geometry check.

No public parameter name, type or default value was changed. No strategy
threshold, confidence, RR, SL or TP target was tuned by CI-00.

## Deterministic contracts

Runtime contracts cover:

- BUY/SELL executable-price directionality;
- midpoint and spread derivation;
- pip-distance conversion;
- percentage broker-distance conversion;
- invalid/crossed quote rejection;
- joint quote/time/MTF context identity and ATR source indices.

## Static ownership gate

`tools/audit_phase_ci_00.py` verifies:

- one canonical price-snapshot owner;
- explicit broker distance-unit semantics;
- one runtime market-context slot;
- migrated high-risk consumers;
- absence of direct Bid/Ask reads in those consumers;
- canonical normalization usage;
- runtime contract inclusion;
- audit accumulation in Source/Architecture CI.

## Important boundary

The terminal's `Server.TimeInUtc` is used as the **quote observation timestamp**.
cTrader's Symbol object provides the current quote values but does not expose a
separate exchange tick timestamp through this contract. Therefore the timestamp
is deliberately named observation time, not market-event time.

Broker mutation code remains broker-facing by design. CI-00 centralizes the
calculation input boundary; it does not turn the canonical snapshot into a
second broker state authority.

## Verification

Repository gate verification on implementation head `420b2390413df99f7ac14c16e73f0f456edc9665`:

- Source / Architecture: PASS — workflow run 2391;
- Runtime Acceptance Contracts: PASS — workflow run 2200;
- cTrader Compile / Build: PASS — workflow run 2384.
- CI-00 phase audit: PASS as step 73 of Source / Architecture run 2391.

Target-terminal and live-broker validation remain later acceptance layers in the
CI track, especially CI-15 through CI-17.

## Next phase

After CI-00 is merged and the local working copy is synchronized, the next
blocking phase is **CI-01 — Primitive Indicator Mathematical Audit**.

Prompt 8 remains paused at CR8.4/H4 until CI-FINAL.
