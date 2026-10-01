# CI-11 — Entry geometry and signal-timing audit

Status: implementation candidate on `phase/ci-11-entry-geometry-timing`.

## Objective

Make one mathematical entry-geometry contract authoritative for execution-zone
membership, ideal entry, trigger price, actual executable quote, entry distance,
extension, late state, retest state, breakout state and continuation state.

The phase also measures actionable-signal latency from the causal event instead
of treating the display/closed-bar observation time as the causal origin.

## Implemented boundary

- `EntryGeometryRule` owns the composed live entry geometry.
- `ExecutionModel` stores the structural zone tolerance produced by the same
  execution-zone calculation.
- live actionability consumes the same geometry snapshot as the execution-mode
  resolver.
- plan preparation and plan market-integrity validation consume the same
  geometry contract for market retest/breakout plans.
- market-entry validation uses the same canonical trigger and zone predicates.
- plan/pending-fill snapshots preserve the structural zone tolerance.
- presentation preview preserves the same zone geometry inputs.
- M1 trigger confirmation records its causal closed-bar boundary timestamp.
- first `ActionableNow` observation is measured from the causal event and
  persisted as `ACTIONABILITY_TIMING` runtime telemetry.
- deterministic Decision Contracts cover BUY/SELL symmetry, tolerant zones,
  breakout/retest late states, continuation waiting and negative-latency
  fail-closed behavior.

## Threshold policy

No numerical tuning was introduced. Existing thresholds remain owned by the
existing policy/constants; CI-11 only removes duplicated equations and makes
their composition canonical.

## Verification

Automated verification is intentionally split:

1. Source/architecture static audit: `tools/audit_phase_ci_11.py`.
2. Deterministic Core contracts:
   `tools/CFIP.Decision.Contracts/Program.cs`.
3. cTrader compile and runtime acceptance remain required CI gates.
4. Hands-on cTrader timing/chart synchronization and historical outcome
   validation remain runtime/manual acceptance boundaries after code verification.

## Next phase

After CI-11 is merged and verified, continue with CI-12 from the Full-Stack
Calculation & Analytical Integrity track.
