# Phase — Modern Signal Lines / Single Presentation State — 2026-10-04

## Objective

Modernize the signal-line presentation without creating any second drawing path or duplicate interpretation logic.

## Canonical ownership

- PlanLineRenderer remains the only production owner that creates/updates signal/plan chart lines.
- PlanLinePresentationRule remains the presentation contract for fixed line thickness/style.
- PlanLevelVisualState is the single owner for which canonical plan levels are visually distinct and eligible for presentation.
- RenderLevelLines consumes that state for lines.
- RenderPlanLabels consumes the same state for labels.
- Pending and parallel opportunity paths continue to delegate line geometry to PlanLineRenderer.

## Root cause removed

Previously, line and label coordinators independently recalculated distinctness and visibility for ENTRY/IDEAL/TRIGGER/SL/TP1..TP4/ACTIVE_TP. That allowed the same signal level to be considered visible by one path and hidden by another.

The phase removes that dual interpretation at the shared presentation-state boundary.

## Visual contract

- Solid lines only.
- Fixed 1px thickness.
- Exactly 40 chart bars, ending at the latest chart candle.
- No infinite extension.
- Existing semantic line colors are preserved.
- Labels remain white, background-free and horizontally separated from the line start.
- Expired/invalid objects continue to be removed by the existing lifecycle owners.

## Verification status

Implementation is complete on the phase branch.

Not yet claimed:
- local cTrader compile, because the repository clone/build environment is not available here;
- GitHub CI success until the PR head produces actual workflow/status evidence;
- target-terminal visual acceptance.

No new renderer, alternate geometry engine, or parallel signal-line logic was introduced.
