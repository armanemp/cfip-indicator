# CR7.1 / G1 — Broker Protection Must Never Increase Live Position Risk

## Status

**تأیید می‌کنم** — CR7.1/G1 is verified complete at repository level and merged to main via PR #138.

Final G1 code HEAD:
2f1cb933a2c2407e1fe33302cf72f538f090ba91

Merge commit:
b8144c2f1edc62730b7a0723be3746afe6353851

## Findings fixed

- TargetObstacleCacheKey overrode Equals without GetHashCode, causing compiler warning CS0659 and violating the equality/hash contract.
- Existing broker SL health was coupled to live market/minimum-distance validation. A correctly sided existing SL near market could therefore be treated as invalid and replaced with a farther stop.
- Bound-plan protection and shared broker-state evaluation inherited the same semantic ambiguity.
- audit_project_integrity.py treated methods with the same name and signature in independent types as duplicates.

## Implementation

- Added TargetObstacleCacheKey.GetHashCode() using all fields that participate in equality.
- Added ManagedStopProtectionRule.IsExistingStopHealthy as the Core owner for existing broker-stop directionality.
- Kept ManagedStopProtectionRule.Validate as the owner for new-stop market/minimum-distance acceptance.
- Updated PriceProtectionValidation, BrokerProtectionCoordinator, BoundPlanProtection and BrokerProtectionStateEvaluator to use the correct semantic boundary.
- ProtectionProgressionRule remains the sole replacement guard for an already healthy SL.
- Added deterministic BUY/SELL G1 runtime contracts and cache-key hash contracts.
- Added tools/audit_phase_7_1.py to the accumulated Source/Architecture chain.
- Made audit_project_integrity.py type-aware so independent GetHashCode overrides are not false duplicate business methods.

## Safety

- No public [Parameter] name/type/DefaultValue changed.
- No RR/confidence/SL/TP/execution threshold tuning was introduced.
- No second decision authority or broker mutation owner was introduced.
- Wrong-sided or invalid protection remains fail-closed.
- A correctly sided existing SL cannot be replaced by a farther/less protective level merely because it is close to market.

## Verification

- Source/Architecture: PASS — run 36868297463 / workflow #2262
- Runtime Acceptance Contracts: PASS — run 36868297578 / workflow #2071
- cTrader Compile: PASS — run 36868297556 / workflow #2255

## Manual boundary

- Fresh local Windows Release build to confirm the CS0659 warning count is now zero.
- Actual cTrader ModifyStopLossPrice acceptance/rejection and broker-specific minimum-distance behavior.
- Restart/reconnect lifecycle ordering.
- Live panel/runtime behavior and empirical signal-quality/profitability.

## Project-wide routine audit

The accumulated Source/Architecture chain passed on the final G1 code HEAD. The G1 change adds no network/file persistence work, no unbounded cache, and no new allocation-heavy runtime loop. Platform-neutral stop-health logic remains in Core.

## Next phase

CR7.2 / G2 — Retest adverse-momentum semantics and rejection telemetry.

Operator action after the documentation PR is merged:
git pull --ff-only
