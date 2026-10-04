# Phase — Signal Label Presentation & Stable Left Gap — 2026-10-04

> Superseded within the same existing label owner: the obsolete 2-pip/box-era wording below has been reconciled to the current one-bar, text-only contract.

## Status
Current corrective implementation in branch `fix/signal-line-label-spacing-color-2026-10-04`; terminal visual revalidation remains required.

## User-visible contract
- Remove the small anchor circles completely.
- Keep exactly one label owner: `PlanLabelRenderer`.
- Label text uses the canonical semantic color of its line.
- No rectangle/background/panel is created for the label.
- Text remains exactly one chart bar to the left of the canonical line start.
- The horizontal gap is owned only by `PlanLabelAnchorCalculator` through `CompactPlanLabelGapBars = 1`; renderers consume the resulting anchor without recomputing the gap.
- cTrader `ChartText` uses `HorizontalAlignment.Right`, so its anchor is the visible end of the text; the anchor calculator places that point one chart bar before the line start. No pip-space X approximation is used.

## Architecture
The existing `PlanLineRenderer` remains the only line owner and also owns the materialized chart line color consumed by `PlanLabelRenderer`. `PlanLabelAnchorCalculator` is the only horizontal-gap owner and `PlanLabelRenderer` remains the only label owner. No second rendering path, marker lifecycle, rectangle, or control was introduced.

## Verification boundary
Repository-side source/audit changes are complete. Actual visual acceptance still requires the user's target cTrader terminal because ChartText placement depends on the live chart's zoom/time scale.

## Operator action
After merge: `git pull --ff-only`.
Then run:
```bash
dotnet build src/CFIP.Indicator/CFIP.Indicator.csproj --configuration Release
```
