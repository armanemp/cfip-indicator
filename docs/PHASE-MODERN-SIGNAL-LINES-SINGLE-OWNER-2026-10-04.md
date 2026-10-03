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
- Semantic line colors remain intact, with one canonical full-opacity cTrader-like treatment (255) applied uniformly to every signal/plan line.
- Labels are white text inside a compact filled box.
- The box uses exactly the same semantic/full-opacity color as its line.
- The box's right edge is attached to the exact right endpoint of the 40-bar line.
- The text is padded inside the box; no separate label renderer or alternate anchor exists.
- Expired/invalid objects continue to be removed by the existing lifecycle owners.

## Verification status

Implementation is complete on the phase branch.

Not yet claimed:
- local cTrader compile, because the repository clone/build environment is not available here;
- GitHub CI success until the PR head produces actual workflow/status evidence;
- target-terminal visual acceptance.

No new renderer, alternate geometry engine, or parallel signal-line logic was introduced.


## cTrader visual reference audit — 2026-10-04

Official cTrader documentation and current chart examples were reviewed. The native horizontal-line visual language is intentionally minimal: horizontal, solid, thin and crisp, with price-axis context. CFIP adopts that visual language through the existing PlanLineRenderer and does not introduce a second renderer. The project-specific 40-bar span remains unchanged because it is an explicit CFIP presentation contract.

## Final label-box contract — 2026-10-04

The plan-level price label is a cTrader-style compact price tag: filled rectangle, same color as the associated level line, white bold text, compact geometry, and exact attachment to the line's right endpoint. `PlanLabelRenderer` is the sole owner; `PlanLabelRenderCoordinator` supplies the canonical presentation state and endpoint.


## Final geometry correction — 2026-10-04

The visual contract is now explicit and non-overlapping:

- canonical plan line: Solid, 1 px, finite 40-chart-bar span ending at the latest candle;
- canonical label: filled cTrader-style compact tag using the exact level color and white text;
- label position: outside the line, to the left of the line's left endpoint;
- fixed two-bar separation prevents the tag from touching or covering the signal line;
- line and label share the same canonical level state/color owner;
- no second line renderer or label renderer is introduced;
- stale label-box objects remain under the same canonical label name and are updated/removed by the existing owner.
