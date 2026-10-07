Status: **VERIFIED COMPLETE — repository gates passed on implementation HEAD `b63f82e1d1fca9ef3af2d7fbbe34e779099ab7d7`; target-terminal visual confirmation remains manual.**


Status: **IMPLEMENTATION IN PROGRESS — branch `phase/panel-clearance-restore-position-2026-10-02`.**

## Requested correction

The previous Indicator panel bottom clearance was 100px. It is now reduced to exactly 50px,
matching the requested half-distance.

When the Indicator panel is hidden, the separate restore "+" button must not remain attached
to the chart's bottom edge. For BottomLeft/BottomRight positions it now uses a dedicated 50px
bottom clearance, keeping the restore control at the same vertical boundary as the panel.

## Design

- `PanelBottomClearance` = 50px.
- `PanelRestoreBottomClearance` = 50px.
- TopLeft/TopRight behavior is unchanged.
- BottomLeft/BottomRight panel alignment remains bottom-aligned with 50px clearance.
- BottomLeft/BottomRight hidden restore button uses the same 50px vertical clearance.
- No public parameter is introduced.
- Existing panel/cBot separation remains intact.

The accumulated MTF-P1 audit is also corrected so it checks the existence and use of the
internal clearance contract rather than freezing the historical 100px value.

## Safety

No signal logic, OB/FVG logic, Decision, Plan, RR, Entry, SL, TP, confidence, risk,
execution authority or broker mutation behavior is changed.

Public parameter count remains 568.

## Verification

Repository gates:
- accumulated Source/Architecture audit;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- accumulated panel/MTF audits.

Manual target-terminal boundary:
- inspect BottomLeft and BottomRight panel placement;
- hide the panel and confirm the "+" restore control is approximately 50px above the bottom edge;
- verify cBot surface space remains available below the Indicator panel.

## Operator action

After merge to main, run: `git pull --ff-only`.


## 2026-10-07 — Hide/show footer restore regression

**Status:** IMPLEMENTED ON THIS WORKING BRANCH — automated source audit added; target-terminal visual confirmation remains required.

Root cause:
- the hide/show handler only changed control visibility;
- the canonical full-panel layout renderer was not guaranteed to run when the panel became visible again;
- the panel could therefore return using stale or small bootstrap geometry, leaving the fixed footer/pressure rail clipped below the timeframe-lamp row.

Correction:
- panel restoration invalidates the canonical presentation key and forces the existing RenderPanel() owner once after visibility is restored;
- no second height calculation, footer renderer, or alternate layout path is introduced;
- the existing footer, lamp rail, ScrollViewer budget, and panel-height equation remain the single geometry owners.

Acceptance:
- hidden state still exposes only the restore button;
- restoring the panel immediately rebuilds the complete panel geometry;
- timeframe lamps, scrollable rows, pressure/footer rail and footer actions are all visible within the same bounded panel;
- repeated hide/show cycles do not accumulate controls or handlers.
