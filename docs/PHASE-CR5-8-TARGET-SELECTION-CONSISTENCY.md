# CR5.8 / E8 — Small Constant Ownership and TargetSelection Consistency

## Status

VERIFIED COMPLETE — PR #124 merged to `main`; merge commit `03a569d6a18f1b6cbc3524dabc24ea713439c325`.

Branch: `phase/cr5-8-target-selection-consistency`

## Scope

E8 reconciles two static-review findings without changing public parameter
identity or silently retuning the established safety values:

1. internal reward-risk floors/margins in `PlanRewardRiskQualityRule`;
2. target-stage RR ordering and lane propagation across all current target
   selection contexts.

## Implementation

### 1. Internal constant ownership

The existing fixed values in `PlanRewardRiskQualityRule` are now named once:

- `BaseMinimumRrFloor = 0.50`
- `PreferredStopRiskAtrFloor = 0.25`
- `MaximumStopRiskAtrFloor = 0.50`
- `AdaptiveStopExcessRrCap = 0.50`
- `AdaptiveStopExcessRrMultiplier = 0.25`
- `EffectiveRrBaseFactor = 0.90`
- `EffectiveRrAbsoluteReduction = 0.15`

The calculations continue to use the same values.

### 2. Canonical required-RR ladder

`Core/Math/TargetSelectionRequiredRrRule.cs` is now the platform-neutral
owner for the TP1..TP4 required-RR ladder.

The lane-specific TP1 rule remains unchanged:

- Strategic: `max(TP1 minimum, MinimumRequiredRR, MinimumTradeRR)`
- Tactical / CounterHtfTactical / MicroReaction:
  `max(TP1 minimum, MinimumRequiredRR, 1.0, TacticalOpportunityMinimumRR)`

TP2..TP4 now build cumulatively from the previously resolved stage:

`TP2 = max(TP2 minimum, TP1 + step)`

`TP3 = max(TP3 minimum, TP2 + step)`

`TP4 = max(TP4 minimum, TP3 + step)`

This removes the confirmed inversion path where a large TP2 requirement could
be followed by a smaller TP3/TP4 requirement.

### 3. Lane propagation

`SelectTargets` and `SelectTarget` no longer default their lane silently.

Current contexts now pass a lane explicitly, including PlanTargetPreparation and PendingOrderPlanSnapshot:

- PlanBuilder: canonical plan-lane resolver;
- ExecutionPlanPreparation: one resolved lane reused for selection and fallback;
- LivePlanTargetEnrichment: active plan lane;
- LiveFillExitReconciler: active plan lane;
- EarlyPredictionEngine: explicit historical Strategic lane;
- PlanPreviewBuilder: preview lane.

Execution fallback RR also consumes the canonical required-RR ladder as a floor
while preserving the existing fallback parameters.

### 4. Verification

Added:

- deterministic four-lane RR/property fixtures;
- Tactical ordering regression fixture;
- BUY/SELL target-geometry symmetry checks;
- named-constant value checks;
- accumulated static audit `tools/audit_phase_5_8.py`;
- runtime-contract project inclusion for the new Core rule.

## Verification

Final implementation head: `51e1f2bc9ecdd12bc8a366630fb225a4fa2c5593` (PR #124 merge commit `03a569d6a18f1b6cbc3524dabc24ea713439c325`).

- Source / Architecture: PASS — run `36844545898` / workflow #2160.
- Runtime Acceptance Contracts: PASS — run `36844545976` / workflow #1969.
- cTrader Compile: PASS — run `36844546002` / workflow #2153.
- Accumulated Phase 11.4 and E6 continuity audits were reconciled with the
  current E8 ownership model; the complete Source/Architecture chain passed.

## Safety boundary

No public parameter name, type or `DefaultValue` was changed.

No default confidence, RR, stop, target-age or execution threshold was retuned.
No second decision or execution authority was introduced.

Target-terminal timing, panel/chart behavior, broker lifecycle/restart,
intrabar behavior and empirical signal-quality/profitability remain manual
acceptance boundaries.
