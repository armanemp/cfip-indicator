# CI-06 — Order Block Full Lifecycle Integrity

Status: implementation complete; repository verification pending.

## Scope

CI-06 audits the complete production Order Block path:

- source candle selection and directional validity;
- zone geometry;
- creation-bar ATR displacement and structure-break evidence;
- quality;
- mitigation and retest;
- freshness/staleness;
- FVG/liquidity confluence;
- MTF consumers;
- Entry / Stop / Target / reward-path geometry reuse.

The phase does not retune public trading thresholds or add a second analytical authority.

## Root causes confirmed

Two structural defects were found in the existing path:

1. `OrderBlockAnalyzer` repeated the opposite-source-candle definition that was already owned by `OrderBlockRule`, creating a second definition point.
2. `BuildOrderBlockCandidate` did not reject an over-age source before materialization. Some downstream consumers performed their own age checks, while reward-path obstacle construction could receive an over-age OB candidate before later filtering.

A lifecycle-specific ownership split was also required so source geometry and lifecycle state were not mixed in one mathematical class.

## Implementation

### Canonical ownership

`OrderBlockRule` now owns:

- opposite-source-candle direction;
- body/wick zone geometry;
- creation-ATR displacement;
- creation-ATR structure break;
- market-side validation;
- deterministic source identity.

`OrderBlockLifecycleRule` now owns:

- `Fresh / Mitigated / Broken`;
- source-age validity;
- mitigation probe semantics;
- full mitigation;
- monotonic partial mitigation;
- retained-width classification.

### Candidate boundary

`OrderBlockCandidateBuilder` now rejects stale/future source objects at the canonical materialization boundary using `MaximumZoneAgeBars` and the lifecycle age rule.

This makes every caller consume the same freshness decision rather than depending on caller-specific age filters.

### Provenance / geometry reuse

Managed OBs continue to carry a deterministic `Zone.Id` derived from:

- direction;
- source candle index;
- body-versus-wick geometry mode.

Entry-zone, stop, target, predictive-pending and reward-path consumers use the resulting managed `Zone` geometry directly. No alternate raw-OHLC OB reconstruction was introduced.

### Duplicate-path cleanup

The duplicated source-candle test was removed from `OrderBlockAnalyzer`. Candidate selection now reaches the canonical source-direction rule through `BuildOrderBlockCandidate`.

## Deterministic contracts

Runtime contracts cover:

- bullish/bearish source-candle symmetry;
- body/wick geometry;
- weak/strong displacement boundaries;
- creation-ATR structure-break boundaries;
- directional mitigation probes;
- bullish/bearish partial mitigation;
- Fresh / Mitigated / Broken lifecycle;
- full-fill invalidation;
- bounded age and future-index rejection;
- deterministic identity.

## Related system integrity

The phase preserves the existing single audio delivery boundary: `AlertEngine` does not own direct sound playback and `AlertDeliveryProcessor` remains the production playback owner. No alert architecture or sound threshold was changed in CI-06.

The broader routine audit remains:

`Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning`

CI-06 changes only the Order Block correctness portion of that chain.

## Safety boundary

No public parameter name, type or default changed.

No confidence, RR, Entry, SL, TP, risk or execution threshold was tuned.

No second decision authority, broker mutation owner or trading engine was introduced.

## Verification

Required repository gates:

- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile / Build.

Manual boundary:

- target-terminal MTF timing;
- live mitigation/retest timing;
- broker/quote timing;
- empirical OB/FVG signal-quality outcomes;
- target-terminal panel/audio latency.

These manual items are not inferred from repository CI.

## Continuation

After CI-06 is verified and merged:

**Next implementation phase: CI-07 — Market regime, MTF and context audit.**
