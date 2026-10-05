# CR7.3 / G3 — Display parameter truth for plan-line thickness/style

Status: **VERIFIED COMPLETE**

Date: 2026-10-01

Merge: PR #143 → `main` as `6c572643cfc6b9a4ee1083e300ec13607dd2774c`.
Verification: Source/Architecture #2295 PASS; Runtime Acceptance #2104 PASS; cTrader Compile #2288 PASS.

## Scope

- Preserve the existing public `Level Line Thickness` parameter.
- Preserve the public parameter for saved-setting compatibility, while routing all production signal/plan lines through the current canonical one-pixel visual contract.
- Superseding contract (2026-10-03): configured values 1, 2 and 3 all resolve to production thickness 1.
- Preserve the current `Solid` plan-line style.
- Avoid changing signal, risk, target or execution thresholds.

## Root cause

`PlanLineRenderer` previously exposed configurable 1..3px production geometry. The current user-facing visual contract supersedes that flexibility: all signal/plan lines use one-pixel Solid geometry.

## Implementation

- Added the canonical presentation boundary `PlanLinePresentationRule.ResolveThickness`.
- `PlanLineRenderer` now consumes that owner for chart-line thickness.
- New and existing `ChartTrendLine` instances receive the resolved value.
- `ResolvePlanLineStyle` remains explicitly `LineStyle.Solid`.
- Added deterministic Runtime Acceptance coverage for the superseding mapping 0/1/2/3/4 → 1.
- Added accumulated static audit `tools/audit_phase_7_3.py`.

## Verification contract

Current canonical mapping:
- 0 → 1
- 1 → 1
- 2 → 1
- 3 → 1
- 4 → 1

The older 1/2/3 production-thickness contract is historical and is superseded by the current one-pixel signal-line requirement.

## Safety / scope boundary

No public parameter name/type/DefaultValue changed.
- Audit contract wording: no public parameter name/type/DefaultValue changed.
No trading threshold or execution authority changed.
No second decision/execution authority was introduced.

Target-terminal visual rendering still requires hands-on cTrader validation.


## Superseding contract note — 2026-10-03

The public `Level Line Thickness` parameter remains only for saved-settings compatibility and parameter-count stability. It no longer creates a second signal-line visual mode. Production signal/plan geometry is always Solid + 1px.

This current contract is enforced by `PlanLinePresentationRule.ResolveThickness` and the accumulated source/runtime audits.
