# CI-05 — FVG Lifecycle Audit

Date: 2026-10-01

Status: **IN IMPLEMENTATION — repository verification pending final CI gates.**

## Objective

Audit and harden the complete Fair Value Gap lifecycle:

`detection → geometry → source age → mitigation → partial fill → full fill →
invalidation/retention → lookup → confluence → predictive pending → target-path usage`

The phase preserves the existing public parameter contract and does not retune
trading thresholds.

## Findings corrected

### 1. Full-fill retention bug

The existing mitigation path recognized the `FvgInvalidateOnFullFill = false`
case, but collapsed the managed zone to zero width and then returned it as
inactive. The parameter therefore behaved as though full-fill invalidation were
always enabled.

The canonical `FvgLifecycleRule` now makes the policy explicit:

- full fill + invalidation enabled → the zone is rejected;
- full fill + invalidation disabled → the original source geometry is retained;
- partial mitigation moves only the affected boundary;
- the lifecycle never fabricates zero-width active geometry.

### 2. Stale FVG leakage past the maximum age

The managed-zone boundary previously carried the age value and downstream
detectors filtered some candidates, but reward-path obstacle construction could
materialize an older FVG before any local age rejection.

`FvgLifecycleAnalyzer.BuildManagedFvgZone` now applies the canonical
`FvgLifecycleRule.IsAgeValid` guard before materialization. Every downstream
consumer that calls the managed-zone owner therefore receives only age-valid FVG
geometry.

## Canonical ownership

- `FvgRule`: 3-bar and 2-bar geometry, gap threshold, overlap, full-fill geometry,
  partial boundary transition and stable identity.
- `FvgLifecycleRule`: source-age validity, body/wick mitigation probe and the
  policy outcome for full-fill retention versus invalidation.
- `FvgLifecycleAnalyzer`: managed `Zone` orchestration/materialization.
- `FvgMitigationEvaluator`: bounded closed-bar iteration using the canonical
  lifecycle rule.
- `FvgDetectionAnalyzer`: candidate discovery/nearest selection and current-bar
  retest semantics.
- `FvgZoneQualityCalculator`: quality presentation from remaining geometry,
  age, displacement and context.

## Consumer audit

Rechecked all material FVG consumers:

- M5/MTF market-frame evidence;
- predictive pending zones;
- OB/FVG confluence;
- reward-path opposing-zone obstacles;
- target candidate evaluation;
- signal trace/presentation consumers.

No consumer is permitted to create a second FVG definition or bypass managed
mitigation when it requires an active zone.

## Deterministic acceptance coverage

Added `VerifyFvgLifecycleSemantics()` covering:

- age boundary at and beyond the configured ceiling;
- no future/current-index inversion;
- direction-symmetric wick/body mitigation probes;
- bullish/bearish partial mitigation;
- bullish/bearish full-fill invalidation;
- explicit full-fill retention when invalidation is disabled;
- invalid-direction fail-closed behavior.

The existing `VerifyFvgMathematics()` remains the canonical geometry contract.

## Static audit

Added:

`tools/audit_phase_ci_05.py`

The accumulated Source/Architecture workflow executes CI-05 immediately after
CI-04 and verifies:

- one lifecycle owner;
- canonical geometry/lifecycle delegation;
- source-age enforcement;
- consumer usage;
- contract project wiring;
- no duplicate lifecycle implementation;
- CI-05 continuation state.

## Performance / cleanup

- no additional historical scan was introduced;
- lifecycle age rejection occurs before managed-zone construction;
- mitigation remains bounded by `MaximumZoneAgeBars`;
- body/wick probe arithmetic is no longer duplicated inside the mitigation evaluator;
- no new I/O, network access or unbounded cache was introduced.

## Safety / invariants

No change was made to:

- public parameter names/types/defaults;
- confidence/score weights;
- RR/Entry/SL/TP numerical thresholds;
- risk limits;
- broker mutation permissions;
- execution authority.

The only behavior corrections are FVG lifecycle semantics that were already
described by existing parameters and the bounded zone-age contract.

## Verification boundary

Repository Source/Architecture, Runtime Acceptance Contracts and cTrader
Compile/Build must pass before merge.

Target-terminal/replay remains required for:

- real multi-timeframe FVG timing;
- FVG discovery/mitigation under live ticks;
- actual panel/chart representation;
- predictive pending return-point behavior;
- reward-path impact in target-terminal replay.
