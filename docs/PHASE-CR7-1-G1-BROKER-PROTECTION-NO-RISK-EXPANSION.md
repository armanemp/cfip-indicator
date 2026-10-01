# CR7.1 / G1 — Broker Protection Must Never Increase Live Position Risk

Date: 2026-10-01

Status: **VERIFIED COMPLETE**

## Phase acknowledgement

تأیید می‌کنم — the existing protection path was audited before implementation across ManagedStopProtectionRule, BrokerProtectionCoordinator, BoundPlanProtection and BrokerProtectionStateEvaluator.

## Root cause

The previous shared managed-stop validity predicate mixed two different concepts:

1. whether an existing broker SL is directionally protective relative to Entry;
2. whether a newly proposed SL is currently acceptable relative to market price and broker minimum-distance constraints.

That allowed a correctly-sided existing SL near current market to be treated as invalid and potentially replaced with a farther stop.

A separate audit defect also treated unrelated GetHashCode() overrides as duplicate production methods because it ignored the containing type.

## Implementation

- Added Core ManagedStopProtectionRule.IsExistingStopHealthy(direction, entry, stop).
- Kept ManagedStopProtectionRule.Validate(...) for new-stop market/minimum-distance acceptance.
- Migrated broker reconciliation, bound-plan protection and broker protection-state evaluation to existing-stop health.
- Preserved ProtectionProgressionRule as the sole guard for replacing an already healthy stop.
- Added deterministic TargetObstacleCacheKey.GetHashCode() consistent with equality.
- Corrected tools/audit_project_integrity.py so duplicate signatures are scoped to their containing type.
- Added deterministic BUY/SELL G1 Runtime Acceptance coverage.
- Added and accumulated tools/audit_phase_7_1.py.

## Verification

Final implementation head: 2f1cb933a2c2407e1fe33302cf72f538f090ba91

- Source/Architecture: PASS — run #2262.
- Runtime Acceptance Contracts: PASS — run #2071.
- cTrader Compile: PASS — run #2255.
- Merged via PR #138 — merge commit b8144c2f1edc62730b7a0723be3746afe6353851.

## Safety boundary

- No public parameter name/type/DefaultValue changed.
- No RR/confidence/SL/TP/execution threshold was tuned.
- No second decision or broker-mutation authority was introduced.
- This phase is an explicit safety correction.

## Routine audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed for ownership continuity.

## Performance/code cleanliness

- Existing-stop health is now a direct Core predicate instead of reusing the wrong market-distance semantics.
- No unbounded cache or new hot-path scanning loop was introduced.
- The duplicate-method audit was strengthened rather than weakened.

## Manual target-terminal boundary

Actual cTrader ModifyStopLossPrice behavior, broker minimum-distance edge cases, and restart/reconnect ordering remain target-terminal acceptance items.

## Next phase

**CR7.2 / G2 — Retest adverse-momentum semantics and rejection telemetry.**
