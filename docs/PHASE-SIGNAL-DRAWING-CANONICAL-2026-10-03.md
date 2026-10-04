# Sub-phase 2 — Canonical Signal Drawing Hardening (2026-10-03)

Status: VERIFIED COMPLETE — canonical drawing implementation plus Single-Owner / No-Duality repair merged to `main` via PR #250, merge commit `2e670514e90deeb46a3f140d1383446f0292c64d`.

## Scope

This sub-phase closes the Arrow / Signal Marker / Signal Line / Label portion of the larger cross-layer consistency phase.

## Root causes

1. The active-plan path rendered a separate `P + "ARROW"` object while WATCH/reaction states used `WATCH_ARROW*`. That created two directional-marker lifecycles and made stale-object cleanup depend on state transitions.
2. Plan-level line thickness still accepted a configurable 1–3px range, while the required visual contract is a single 1px solid line.
3. The prior text-only label contract is superseded by the current cTrader-style label contract: white regular-weight text inside a filled box using the exact semantic line color, with the box attached to the exact right endpoint of the 40-bar line.
5. The legacy alert mirror correctly did not draw a second marker, but its ownership needs to remain explicit as the canonical signal renderer evolves.

## Corrections

- Active-plan directional arrows now use the same `RenderStackedSignalArrows` owner as WATCH/reaction arrows.
- Legacy `P + "ARROW"` objects are removed as part of canonical arrow cleanup.
- M1 trigger remains a separate `Circle` marker and is not treated as a directional signal arrow.
- BUY/SELL direction remains explicit through `ChartIconType.UpArrow` / `DownArrow`.
- Signal/plan level lines are now one-pixel and solid through `PlanLinePresentationRule`.
- Signal/plan level lines are fixed to exactly 40 chart bars from the latest candle; the legacy `FullWidthLevelLines` switch cannot expand them.
- Compact labels use native background-free ChartText with the canonical semantic line color and 10px regular typography.
- Compact labels sit at the exact level price and are positioned exactly one chart bar left of the canonical line start.
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
- The user's local Release-build CS0219 warning from `PanelMainRenderer.buttonMargin` was eliminated before this phase closeout; local confirmation after pulling the merged head remains required.

Operator action:
`git pull --ff-only`
Then rerun `dotnet build src/CFIP.Indicator/CFIP.Indicator.csproj --configuration Release` locally; the `buttonMargin` CS0219 warning should be absent.


## Final duality-repair closeout — 2026-10-03

- cBot startup now emits one canonical `PlayStarted` lifecycle cue; the redundant Live-disarmed startup cue was removed.
- Signal/plan line geometry has one owner: Solid, 1px, exactly 40 bars from the latest candle.
- Pending and parallel signal lines delegate to the same line owner.
- All compact chart labels use one renderer and one formatter: exact price, white text, no background, left-of-line anchor with a deterministic gap.
- Historical audit contracts that conflicted with the current visual contract were updated instead of reintroducing duplicate production behavior.
- Dedicated Single-Owner / No-Duality audit passed on the final implementation head.

Final repository verification on merge head `2e670514e90deeb46a3f140d1383446f0292c64d`:
Source/Architecture PASS; Runtime Acceptance PASS; cTrader Compile PASS.

Manual cTrader terminal acceptance remains required for the actual audible startup cue and exact visual line/label rendering.
