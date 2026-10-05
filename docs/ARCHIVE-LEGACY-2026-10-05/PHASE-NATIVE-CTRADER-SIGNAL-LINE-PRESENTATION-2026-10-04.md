# Phase — Native cTrader Signal-Line Presentation Closure — 2026-10-04

## Objective
Close the remaining visual mismatch in signal/plan lines and labels at the existing canonical owners. The result must read as one integrated cTrader-style chart object rather than a line plus a detached annotation.

## Final visual contract
- One canonical PlanLineRenderer owns every signal/plan line.
- Lines are horizontal, Solid, 1px, finite and exactly 40 chart bars long, ending at the latest candle.
- The label is owned only by PlanLabelRenderer.
- The label is native `ChartText` with no rectangle, box or marker.
- Text is 10px regular weight, right aligned, uses the exact canonical line color, and the visible end of the text sits exactly one chart bar before the canonical line start.
- The line renderer owns final line color and the anchor calculator owns the one-bar gap.
- There is no floating label at the chart's right edge.
- Pending and parallel labels use the same anchor and renderer.
- Prediction labels remain under the same renderer owner and must not create a competing presentation path.
- Expired/invalid objects continue to be removed by the existing lifecycle.

## Root cause
The line renderer was already canonical and correct. The remaining mismatch came from label presentation geometry: the text was visually too close to the line start and too small for reliable reading. The label owner already had the correct line-left anchor; the correction keeps that anchor and enforces exactly one chart bar of left clearance with a slightly larger native-text font.

## Correction
The existing owners were corrected in place: `GetCompactPlanLabelAnchorBar()` continues to return `GetPlanLineLeftBar()`; `RenderPlanLabels()` consumes that canonical anchor; `DrawCompactPlanLabel()` places one native `ChartText` object exactly one chart bar to the left of the line start and uses the canonical 10px presentation size. No duplicate renderer, geometry engine, box, marker or calculation path was added.

## Single-owner invariant
For this behavior there is exactly one line owner (PlanLineRenderer), one line presentation rule (PlanLinePresentationRule), one label owner (PlanLabelRenderer), one label anchor (PlanLabelAnchorCalculator), one label formatting owner (PlanLabelFormatting), and one presentation state owner (PlanLevelVisualState).

## Verification
Repository-side source review: PASS for the intended owner flow.

Not claimed without the user's terminal: cTrader Release compile, exact target-terminal pixels/spacing, and runtime stale-object cleanup under live signal transitions.

## Operator action
Run git pull --ff-only, then: dotnet build src/CFIP.Indicator/CFIP.Indicator.csproj --configuration Release

Finally inspect one live signal on cTrader. The acceptance criterion is a clean, thin, solid 40-bar line with native text positioned exactly one chart bar to its left; no text crossing the line, no detached right-side label, and no background rectangle.
