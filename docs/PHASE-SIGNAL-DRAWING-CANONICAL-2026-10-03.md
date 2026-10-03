# Sub-phase 2 — Canonical Signal Drawing Hardening (2026-10-03)

Status: IMPLEMENTATION COMPLETE — verification pending on the final main head.

## Scope

This sub-phase closes the Arrow / Signal Marker / Signal Line / Label portion of the larger cross-layer consistency phase.

## Root causes

1. The active-plan path rendered a separate `P + "ARROW"` object while WATCH/reaction states used `WATCH_ARROW*`. That created two directional-marker lifecycles and made stale-object cleanup depend on state transitions.
2. Plan-level line thickness still accepted a configurable 1–3px range, while the required visual contract is a single 1px solid line.
3. Compact signal-level labels reused semantic line colors. The current project presentation contract requires readable white text with no label background.
4. Label anchoring was already left of the line start, but it required an explicit deterministic gap contract so the line and label cannot visually touch.
5. The legacy alert mirror correctly did not draw a second marker, but its ownership needs to remain explicit as the canonical signal renderer evolves.

## Corrections

- Active-plan directional arrows now use the same `RenderStackedSignalArrows` owner as WATCH/reaction arrows.
- Legacy `P + "ARROW"` objects are removed as part of canonical arrow cleanup.
- M1 trigger remains a separate `Circle` marker and is not treated as a directional signal arrow.
- BUY/SELL direction remains explicit through `ChartIconType.UpArrow` / `DownArrow`.
- Signal/plan level lines are now one-pixel and solid through `PlanLinePresentationRule`.
- Compact labels are background-free and use canonical white text.
- Label anchoring remains at least three bars to the left of the line start.
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

Required on final head:
- Source / Architecture checks
- Runtime Acceptance Contracts
- cTrader Compile
- canonical drawing audit
- target-terminal visual acceptance for arrow shape/placement, 40-bar line length, left label gap, white text and stale-object removal.

Operator action after merge:
`git pull --ff-only`
