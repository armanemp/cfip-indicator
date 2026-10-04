# Phase — Signal Label Presentation & Stable Left Gap — 2026-10-04

> Superseded within the same existing label owner: the obsolete 2-pip/box-era wording below has been reconciled to the current one-bar, text-only contract.

## Status
Implemented on branch `phase/fix-signal-label-spacing-color-2026-10-04`.

## User-visible contract
- Remove the small anchor circles completely.
- Keep exactly one label owner: `PlanLabelRenderer`.
- Label text uses the canonical semantic color of its line.
- No rectangle/background/panel is created for the label.
- Text remains exactly one chart bar to the left of the canonical line start.
- The horizontal gap is owned by `CompactPlanLabelGapBars = 1`.
- cTrader `ChartText` is positioned on the chart time axis; horizontal presentation is therefore expressed as a deterministic chart-bar gap, not a fabricated pip-space X offset.

## Architecture
The existing `PlanLineRenderer` remains the only line owner and `PlanLabelRenderer` remains the only label owner. No second rendering path, marker lifecycle, rectangle, or control was introduced.

## Verification boundary
Repository-side source/audit changes are complete. Actual visual acceptance still requires the user's target cTrader terminal because ChartText placement depends on the live chart's zoom/time scale.

## Operator action
After merge: `git pull --ff-only`.
Then run:
```bash
dotnet build src/CFIP.Indicator/CFIP.Indicator.csproj --configuration Release
```
