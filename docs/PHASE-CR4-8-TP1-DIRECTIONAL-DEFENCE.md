# CR4.8 / D8 — TP1 directional defensive validation

## Scope

Harden the TP1 direction invariant at every relevant planning boundary without
changing public parameters, RR defaults, stop policy or execution policy.

## Finding reconciliation

The repository already had one canonical directional protection rule:

`PriceProtectionRule.ValidateTarget(direction, entry, target, minimumDistance)`

The D8 correction therefore reuses that owner instead of introducing another
directional business-rule implementation.

## Implementation

- Target-candidate constraint evaluation now delegates target-side validation to
  `PriceProtectionRule`.
- Plan materialization rejects invalid-direction TP1 before a `Plan` can escape
  the materialization boundary.
- `BuildPlan` propagates materialization rejection fail-closed.
- Plan protection validation independently re-checks TP1 direction at the
  protection boundary.
- Plan reward validation independently re-checks TP1 direction and emits the
  existing bounded `PLAN_REWARD` rejection telemetry with reason
  `TP1 DIRECTION INVALID`.
- Existing `TargetProgressionRule` remains the canonical monotonic progression
  owner; no duplicate progression semantics were added.

## Deterministic evidence

The planning contracts cover:

- valid BUY TP1;
- valid SELL TP1;
- wrong-side BUY TP1 rejection;
- wrong-side SELL TP1 rejection;
- BUY/SELL mirrored stop/target semantics;
- canonical wrong-side rejection from `TargetCandidateConstraintRule`;
- continued BUY/SELL symmetry for `TargetProgressionRule`.

## Safety boundary

- No public parameter name/type/DefaultValue changed.
- No default RR, confidence, SL, target-age or execution threshold changed.
- No new decision or execution authority was introduced.
- No trading threshold was tuned.
- This is a defensive correctness correction only.

## Verification boundary

Repository CI must validate:

- accumulated Source/Architecture checks including `audit_phase_4_8.py`;
- Runtime Acceptance Contracts;
- cTrader Compile.

Target-terminal cTrader runtime timing, live panel behavior, broker lifecycle
ordering, replay and empirical signal-quality/profitability remain manual
acceptance work and are not claimed from repository CI.

## Performance and cleanliness audit

- No new per-bar I/O was introduced.
- No new repeated target scan was introduced.
- The target-side rule remains a small pure Core validation.
- No duplicate target-direction owner was added.
- Existing accumulated project-wide audits remain mandatory.
