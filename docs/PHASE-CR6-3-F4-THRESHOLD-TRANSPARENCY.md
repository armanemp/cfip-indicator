# CR6.3 / F4 — Effective-threshold transparency and hidden additive margins

Date: 2026-10-01

Status: **VERIFIED — all three repository gates PASS on commit `5dd9a8a8bb0fb8172ac40b7336ce86e0bb5c3c2a`.**

## Scope

This phase reconciles the hidden actionability margins and hard-coded floors identified by the Prompt 6/F4 review without changing public parameters or default trading policy.

Affected owners:

- `Core/Math/ActionabilityThresholdPolicy.cs`
- `Core/Math/ActionableSignalQualityRule.cs`
- `Trading/Validation/ActionableSignalQualityGate.cs`
- `Trading/Validation/TradeActionabilityEvaluator.cs`
- `Analysis/Market/ParallelOpportunityBuilder.cs`
- `UI/Panel/Rows/PanelOverviewDiagnosticRowsRenderer.cs`

## Root causes confirmed

1. Final actionable-signal gating embedded additive margins and fixed floors directly in the consumer.
2. Staged actionability used literal 64/64 entry location/timing thresholds and a literal 40 precision-entry floor.
3. Parallel opportunity presentation applied a hidden `+3` quality margin directly in its consumer.
4. Quality-recovery tolerance/margins lived directly in the Core rule as numeric literals.
5. The panel exposed `Decision.ActionableNow` only after the final quality gate; pre-final and post-final semantics were not visibly distinguished by threshold diagnostics.
6. The final threshold calculation must preserve the existing precedence of both base and Smart minimums.

## Implementation

- Added one platform-neutral `ActionabilityThresholdPolicy` owner with a deterministic effective-threshold snapshot.
- Preserved current values:
  - upstream hard floors: 64/64; with the existing default `Minimum Entry Quality=72`, the effective staged defaults are 72/64;
  - precision-entry floor: 40;
  - final entry location/timing/position: 70/75/70;
  - final confidence margin: +4;
  - final Smart-quality/MTF margins: +3;
  - final independent-evidence/structural margins: +1;
  - parallel opportunity margin: +3;
  - quality-recovery deficit allowance: 8;
  - quality-recovery confidence/Smart/MTF/evidence/structure margins: +5/+5/+3/+1/+1;
  - quality-recovery TP1 RR margin: +0.35.
- Preserved the original `Math.Max(base, Smart minimum)` precedence for Smart quality, MTF agreement and independent evidence.
- Routed the final actionable-quality gate through the canonical threshold snapshot.
- Routed staged actionability floors through the same named owner.
- Routed parallel-opportunity's hidden margin through the same owner.
- Exposed effective thresholds in the panel diagnostics.
- Added deterministic Runtime Acceptance coverage and a dedicated static audit.
- Added the F4 audit to the accumulated Source/Architecture workflow.
- Removed the unrelated compiler warning by changing the unused archive catch variable from `catch (Exception ex)` to `catch (Exception)`; no persistence behavior changes.

## Actionability semantics

`Decision.ActionableNow` is assigned from the preliminary actionability evaluator first. When true, `EvaluateFinalActionableSignalQuality` executes next and may set `ActionableNow=false`. Therefore the decision/panel/chart actionable state is **post-final-gate**, not a pre-final prediction.

## Parameter boundary

No public `[Parameter]` name, type, `DefaultValue`, RR default, confidence threshold or execution policy was changed.

No second decision or execution authority was introduced.

## Verification

Required repository verification:

- Source/Architecture: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS;
- manual target-terminal validation remains a separate acceptance boundary.

Local Windows build of this branch was not runnable in this environment because the user's checkout is not mounted here. The provided build established the pre-change baseline of 1 warning / 0 errors; the warning's source line is corrected in this branch.
