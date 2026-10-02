# Panel Geometry Correction — 50px Bottom Clearance + Hidden Restore Position

Date: 2026-10-02

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
