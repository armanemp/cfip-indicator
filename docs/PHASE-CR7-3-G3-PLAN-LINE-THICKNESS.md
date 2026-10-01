# CR7.3 / G3 — Display parameter truth for plan-line thickness/style

Status: **VERIFIED COMPLETE**

Date: 2026-10-01

## Scope

- Preserve the existing public `Level Line Thickness` parameter.
- Make configured values 1, 2 and 3 produce actual chart line thickness 1, 2 and 3.
- Contract statement: values 1/2/3 produce actual thickness 1/2/3.
- Preserve the current `Solid` plan-line style.
- Avoid changing signal, risk, target or execution thresholds.

## Root cause

`PlanLineRenderer.ResolvePlanLineThickness` previously normalized the configured value and then applied `Math.Min(1, configured)`, which made every valid configuration (1/2/3) render at thickness 1.

## Implementation

- Added the canonical presentation boundary `PlanLinePresentationRule.ResolveThickness`.
- `PlanLineRenderer` now consumes that owner for chart-line thickness.
- New and existing `ChartTrendLine` instances receive the resolved value.
- `ResolvePlanLineStyle` remains explicitly `LineStyle.Solid`.
- Added deterministic Runtime Acceptance coverage for 1/2/3 plus bounded invalid inputs.
- Added accumulated static audit `tools/audit_phase_7_3.py`.

## Verification contract

- 1 → 1
- 2 → 2
- 3 → 3
- 0 → 1 (safe lower bound)
- 4 → 3 (safe upper bound)

## Safety / scope boundary

No public parameter name/type/DefaultValue changed.
No trading threshold or execution authority changed.
No second decision/execution authority was introduced.

Target-terminal visual rendering still requires hands-on cTrader validation.
