# Sub-phase 2 — Canonical Signal Drawing Hardening (2026-10-03)

Status: VERIFIED COMPLETE — code head `a3cd97f715ed6b6b91599b79fe3fa42c82690e1c` passed Source/Architecture, Runtime Acceptance and cTrader Compile.

## Scope

This sub-phase closes the Arrow / Signal Marker / Signal Line / Label portion of the larger cross-layer consistency phase.

## Root causes

1. The active-plan path rendered a separate `P + "ARROW"` object while WATCH/reaction states used `WATCH_ARROW*`. That created two directional-marker lifecycles and made stale-object cleanup depend on state transitions.
2. Plan-level line thickness still accepted a configurable 1–3px range, while the required visual contract is a single 1px solid line.
3. Compact signal-level labels reused semantic line colors. The current project presentation contract requires readable white text with no label background.
4. Label anchoring was left of the line start, but the label was also vertically offset from the price level. That made the annotation look detached from its line. The contract requires the label to share the exact line price and use horizontal separation only.
5. The legacy alert mirror correctly did not draw a second marker, but its ownership needs to remain explicit as the canonical signal renderer evolves.

## Corrections

- Active-plan directional arrows now use the same `RenderStackedSignalArrows` owner as WATCH/reaction arrows.
- Legacy `P + "ARROW"` objects are removed as part of canonical arrow cleanup.
- M1 trigger remains a separate `Circle` marker and is not treated as a directional signal arrow.
- BUY/SELL direction remains explicit through `ChartIconType.UpArrow` / `DownArrow`.
- Signal/plan level lines are now one-pixel and solid through `PlanLinePresentationRule`.
- Signal/plan level lines are fixed to exactly 40 chart bars from the latest candle; the legacy `FullWidthLevelLines` switch cannot expand them.
- Compact labels are background-free and use canonical white text.
- Compact labels sit at the exact level price and are separated from the line only horizontally, with a deterministic minimum three-bar gap.
- Expired/invalid drawing lifecycle continues to remove all plan, watch and legacy marker objects before rendering the current canonical snapshot.
- The legacy alert mirror remains prohibited from drawing a second signal marker.

## What was deliberately not changed

- The existing HTF smart-arrow strength ladder remains intact. It still determines whether the canonical arrow renderer shows one, two or three stacked directional arrows based on the existing strength contract.
- M15 remains the canonical decision/reference timeframe; M5 remains trigger/entry precision; M1 remains optional confirmation.
- Signal quality, TP/SL/RR and OB/FVG weighting are reserved for the next sub-phase.
- cBot broker mutation ownership is unchanged.

## Routine full-chain audit

Analysis -> Decision -> SignalVisualSnapshot -> Drawing -> Alert -> cBot execution -> Broker confirmation -> Protection/Lifecycle -> Outcome/History.

No new realtime calculation loop was introduced.

## Verification

- Source / Architecture: PASS (155 successful steps; canonical drawing audit PASS).
- Runtime Acceptance Contracts: PASS.
- cTrader Compile: PASS.
- User local Release build exposed one CS0219 warning from an unused `buttonMargin` local in `PanelMainRenderer`; that declaration has been removed at the root.
- Target-terminal visual acceptance remains manual for exact arrow rendering/placement, 40-bar line length, label gap/readability and stale-object removal.

Operator action:
`git pull --ff-only`
Then rerun `dotnet build src/CFIP.Indicator/CFIP.Indicator.csproj --configuration Release` locally; the `buttonMargin` CS0219 warning should be absent.
