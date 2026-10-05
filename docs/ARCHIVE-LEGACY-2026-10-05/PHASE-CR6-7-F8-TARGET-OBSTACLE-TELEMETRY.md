# CR6.7 / F8 — Target-obstacle rejection telemetry and distant-target semantics

## Verified source finding

The current target pipeline already uses these defaults in its obstacle decision:

- `RejectTargetObstacle = true`;
- `TargetClearanceAtr = 0.10`;
- `MaximumTargetExtensionAtr = 4.0`.

The M5 obstacle check rejects a target when a qualifying swing/equality level lies strictly between Entry and Target after the existing clearance. Therefore a farther target exposes a longer path and can naturally encounter more eligible obstacle levels. This is a quantifiable interaction, not a reason to retune the default in F8.

## Implementation

- `TargetObstacleEvaluation` provides one structured M5 scan result.
- Swing obstacles remain `OBSTACLE_SWING`.
- Equal-high/low liquidity obstacles are separated as `OBSTACLE_EQ`.
- Opposing-zone obstacles remain `OBSTACLE_OPPOSING_ZONE`.
- HTF-zone obstacles remain `OBSTACLE_HTF_ZONE`.
- `TargetObstacleTelemetryAccumulator` aggregates target distance in ATR, percentage of the existing target-extension envelope, and known obstacle distance in ATR.
- The existing bounded/deduplicated `PLAN_TARGET` telemetry channel emits the aggregate summary once per M5/stage/reason.
- Zone and HTF observations retain target-distance evidence but use `-1` for unknown obstacle depth rather than inventing a level.

## No behavior tuning

This phase does not:
- lower/raise obstacle thresholds;
- exempt distant targets;
- alter target scoring;
- change MaximumTargetExtensionAtr or TargetClearanceAtr;
- add a public parameter.

The purpose is evidence: replay can now quantify whether distant targets are disproportionately rejected by ordinary M5 path obstacles before any later behavior change is considered.

## Verification

- deterministic runtime contracts cover aggregation, distance-envelope math, missing-depth handling, invalid-input safety and taxonomy;
- a permanent `audit_phase_6_7.py` is added immediately after F7;
- existing target-selection flow remains the sole authority.

## Next

**CR6.8 / F9 — Target-obstacle scan performance and cache reuse.**
