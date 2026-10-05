# Phase — Native cTrader Signal-Line Presentation Closure — 2026-10-04

## Objective
Close the remaining visual mismatch in signal/plan lines and labels at the existing canonical owners. The result must read as one integrated cTrader-style chart object rather than a line plus a detached annotation.

## Final visual contract
- One canonical PlanLineRenderer owns every signal/plan line.
- Lines are horizontal, Solid, 1px, finite and exactly 40 chart bars long, ending at the latest candle.
- The label is owned only by PlanLabelRenderer.
- The label anchor is the visible RIGHT edge of the native ChartText and is owned one chart bar before the exact LEFT endpoint of the same line geometry.
- The label sits immediately before the line start with a deterministic 1-bar gap.
- The label remains compact native ChartText; no large rectangle/background is created.
- Label text uses the exact same materialized semantic color as its line, through the line renderer’s canonical color owner.
- Text is white, compact, centered inside the tag, and uses the canonical formatted level text.
- There is no floating label at the chart's right edge.
- Pending and parallel labels use the same anchor and renderer.
- Prediction labels remain under the same renderer owner and must not create a competing presentation path.
- Expired/invalid objects continue to be removed by the existing lifecycle.

## Root cause
The line renderer was already canonical and correct. The remaining mismatch came from presentation geometry: the coordinator selected the line's right endpoint as the label anchor, and the label renderer then constructed a multi-bar rectangle from that point. The result looked like a detached right-side annotation rather than a native-style level tag.

## Correction
The existing owners are corrected in place: PlanLineRenderer owns line geometry and final chart color; PlanLabelAnchorCalculator computes exactly one chart-bar gap before the line start; PlanLabelRenderer consumes that anchor as the visible end of its right-aligned ChartText and applies the exact line color. No duplicate renderer or parallel geometry engine was added.

## Single-owner invariant
For this behavior there is exactly one line owner (PlanLineRenderer), one line presentation rule (PlanLinePresentationRule), one label owner (PlanLabelRenderer), one label anchor (PlanLabelAnchorCalculator), one label formatting owner (PlanLabelFormatting), and one presentation state owner (PlanLevelVisualState).

## Verification
Repository-side source review: PASS for the intended owner flow.

Not claimed without the user's terminal: cTrader Release compile, exact target-terminal pixels/spacing, and runtime stale-object cleanup under live signal transitions.

## Operator action
Run git pull --ff-only, then: dotnet build src/CFIP.Indicator/CFIP.Indicator.csproj --configuration Release

Finally inspect one live signal on cTrader. The acceptance criterion is a clean, thin, solid 40-bar line with regular-weight same-color label text whose visible end is exactly one full candle left of the line start; no overlap, detached right-side text or oversized rectangle.
