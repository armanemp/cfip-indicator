# Phase — Signal Label Color, Marker Removal & Stable Left Gap — 2026-10-04

## Status
Implemented on branch `phase/fix-signal-label-spacing-color-2026-10-04`.

## User-visible contract
- Remove the small anchor circles completely.
- Keep exactly one label owner: `PlanLabelRenderer`.
- Label text uses the same canonical semantic color as its line.
- No rectangle/background/panel is created for the label.
- Text remains to the left of the canonical line start.
- The horizontal gap is owned by `CompactPlanLabelGapBars`.
- cTrader `ChartText` exposes time/bar coordinates on X and price on Y; it does not expose a pip-based horizontal X offset. Therefore the renderer does not fake a “2 pip” X offset by distorting price geometry. The stable chart-coordinate gap is one bar left of the line start.

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


## Corrective verification — 2026-10-04
- Removed the literal escaped-newline token that caused `PlanLabelRenderer.cs` compilation failure.
- Removed the obsolete one-bar label-gap owner; exact `2 * Symbol.PipSize` clearance is now the sole spacing contract.
- Updated architecture verification to reject rectangles/markers and white-only legacy label assumptions.
- cTrader indicator CI compile for commit `9b9c11b3...` passed; local cTrader build must be re-run after pulling the corrected branch/main commit.
- Runtime acceptance CI was already failing on `main` before this correction, so it is tracked separately and is not attributed to this visual-label change.
