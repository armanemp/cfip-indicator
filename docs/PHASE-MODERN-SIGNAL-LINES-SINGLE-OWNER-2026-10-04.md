# Phase — Native cTrader Signal-Line Presentation Closure — 2026-10-04

## Objective
Close the remaining visual mismatch in signal/plan lines and labels at the existing canonical owners. The result must read as one integrated cTrader-style chart object rather than a line plus a detached annotation.

## Final visual contract
- One canonical PlanLineRenderer owns every signal/plan line.
- Lines are horizontal, Solid, 1px, finite and exactly 40 chart bars long, ending at the latest candle.
- The label is owned only by PlanLabelRenderer.
- The label anchor is the LEFT endpoint of the exact same line geometry.
- The label sits immediately before the line start with a deterministic 1-bar gap.
- The label is a compact filled price tag, not a large rectangle spanning the line.
- Tag fill uses the exact semantic color of its line.
- Text is white, compact, centered inside the tag, and uses the canonical formatted level text.
- There is no floating label at the chart's right edge.
- Pending and parallel labels use the same anchor and renderer.
- Prediction labels remain under the same renderer owner and must not create a competing presentation path.
- Expired/invalid objects continue to be removed by the existing lifecycle.

## Root cause
The line renderer was already canonical and correct. The remaining mismatch came from presentation geometry: the coordinator selected the line's right endpoint as the label anchor, and the label renderer then constructed a multi-bar rectangle from that point. The result looked like a detached right-side annotation rather than a native-style level tag.

## Correction
The existing owners were corrected in place: GetCompactPlanLabelAnchorBar() now returns GetPlanLineLeftBar(); RenderPlanLabels() consumes that canonical anchor; DrawCompactPlanLabel() treats the anchor as the line start and builds only a compact tag before it; the tag retains exact-price alignment and semantic color. No duplicate renderer or parallel geometry engine was added.

## Single-owner invariant
For this behavior there is exactly one line owner (PlanLineRenderer), one line presentation rule (PlanLinePresentationRule), one label owner (PlanLabelRenderer), one label anchor (PlanLabelAnchorCalculator), one label formatting owner (PlanLabelFormatting), and one presentation state owner (PlanLevelVisualState).

## Verification
Repository-side source review: PASS for the intended owner flow.

Not claimed without the user's terminal: cTrader Release compile, exact target-terminal pixels/spacing, and runtime stale-object cleanup under live signal transitions.

## Operator action
Run git pull --ff-only, then: dotnet build src/CFIP.Indicator/CFIP.Indicator.csproj --configuration Release

Finally inspect one live signal on cTrader. The acceptance criterion is a clean, thin, solid 40-bar line with a small same-color price tag immediately at its left start; no detached text on the right and no oversized rectangle.
