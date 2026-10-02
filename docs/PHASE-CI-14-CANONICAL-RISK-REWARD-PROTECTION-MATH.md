# CI-14 — Canonical Risk/Reward and Protection Mathematics

Date: 2026-10-02

Status: **IN PROGRESS**

## Scope

CI-14 closes formula drift across:

`Risk → Reward → Nominal RR → Spread-adjusted/effective RR → Minimum RR → Maximum RR`

The phase is an ownership correction. It does not tune trading thresholds, confidence,
signal weights or profitability targets.

## Findings carried from the full-stack audit

Several paths were independently calculating nominal RR, with different risk floors:

- target-candidate filtering used `Math.Max(pipSize, risk)`;
- parallel opportunity presentation used the same pip-size floor;
- pending/live plan snapshots could store a pip-size-inflated `Risk`;
- execution geometry divided reward by raw risk;
- plan validation also divided by raw risk;
- live target evaluation had another raw-ratio implementation.

That created a real semantic seam: the same Entry/SL/TP geometry could report different
RR depending on which subsystem evaluated it.

## Implementation

Added canonical platform-neutral owners:

- `RiskRewardGeometryRule`
  - validates directional Entry/SL/TP geometry;
  - computes raw Risk;
  - computes Reward;
  - computes Nominal RR;
  - computes spread-adjusted Effective Risk;
  - computes Effective RR;
  - fails closed on invalid/non-finite geometry and spread.

- `RiskRewardPolicyRule`
  - owns minimum/maximum comparison semantics;
  - preserves hard safety floors;
  - centralizes adaptive minimum RR calculation;
  - centralizes effective minimum RR calculation.

Migrated consumers:

- `PlanRewardRiskQualityRule`;
- `ExecutionPlanGeometryRule`;
- `TargetCandidateConstraintRule`;
- `PlanMaterialization`;
- `PlanTargetPreparation`;
- `PlanRewardIntegrityValidator`;
- `StructuralStopCandidateEvaluator`;
- `LiveExitGeometryRule`;
- `LiveTargetCandidateEvaluator`;
- `PlanRiskRewardRecalculator`;
- `LivePlanFactory`;
- `PendingOrderPlanSnapshot`;
- `ParallelOpportunityBuilder`.

Live/pending plan snapshots now preserve the real price-distance Risk instead of replacing
small risks with `Symbol.PipSize`.

## Preserved policy

No public parameter/default was changed.

Existing policy constants remain behaviorally identical:

- Base minimum RR floor = 0.50;
- preferred stop-risk ATR floor = 0.25;
- maximum stop-risk ATR floor = 0.50;
- adaptive stop excess RR cap = 0.50;
- adaptive stop excess RR multiplier = 0.25;
- effective RR base factor = 0.90;
- effective RR absolute reduction = 0.15;
- execution minimum RR floor = 0.10.

CI-14 changes ownership and mathematical consistency, not the policy values.

## Deterministic contracts

Planning contracts cover:

- mirrored BUY/SELL Risk and Reward;
- mirrored Nominal RR;
- mirrored spread-adjusted Effective RR;
- wrong-side Entry/SL/TP rejection;
- non-finite spread fail-closed behavior;
- minimum RR boundary;
- maximum RR boundary;
- adaptive RR policy;
- effective RR minimum policy;
- candidate/plan/execution consumers sharing the canonical result;
- a small-risk fixture proving that pip size no longer changes nominal RR.

## Required verification

- Source / Architecture with accumulated CI-00..CI-14 audits;
- Runtime Acceptance Contracts;
- cTrader Compile / Build;
- Planning Contracts.

Manual boundary remains:

- actual broker quote/fill behavior;
- target-terminal validation;
- replay against real feed data;
- empirical signal/TP outcome validation;
- panel/chart synchronization.

## Relation to the user's reported symptoms

CI-14 addresses one concrete class of cross-component mismatch: the same geometry being
assigned different RR values by different consumers.

It does not yet prove whether an apparently obvious chart setup is absent because analysis
failed to detect it, because decision/trigger/actionability gates rejected it, or because
presentation/execution state diverged. That distinction is intentionally reserved for
CI-15 and CI-16, where the exact authoritative Entry/SL/TP and causal timestamps will be
traced and replayed.

## Next phase

**CI-15 — End-to-end execution-geometry and broker-boundary audit.**
