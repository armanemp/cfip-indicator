# CFIP — Smart Separated Signal Arrows — 2026-10-04

Status: VERIFIED COMPLETE — PR #252 merged to main as b095710eb83e7f93017edb1050b20079b27b4371.

## Purpose

This phase closes the requirement that directional arrows be separated, semantically intelligent, and consistently divided into three real levels inside each of three strength tiers.

## Final contract

The canonical presentation ladder is:

- Weak 1 / level 1
- Weak 2 / level 2
- Weak 3 / level 3
- Medium 1 / level 4
- Medium 2 / level 5
- Medium 3 / level 6
- Strong 1 / level 7
- Strong 2 / level 8
- Strong 3 / level 9

The number of visible arrows is derived from the level: one, two, or three stacked arrows. Tier/color is derived from the same level. No second renderer may independently recalculate signal strength.

## Intelligence source

MtfTrendStrengthRule is the single strength owner. It evaluates the already-existing M1/M5/M15/M30/H1/H4/D1/W1 frame evidence, with M1 excluded from HTF trend authority and M15/M5 role separation preserved. Per-frame strength incorporates existing directional score, quality, trend/momentum, ADX, EMA spread/slope, structure, OB/FVG location, independent indicator evidence, live EMA pressure and indicator conflict.

The authoritative chart direction is resolved first by SignalVisualSnapshotBuilder. That direction is passed into ApplyMtfTrendStrength, so the displayed strength describes the exact direction actually being rendered.

## Separation

The canonical stacked-arrow renderer enforces a minimum vertical separation using ATR-relative clearance plus a hard minimum of three pips. It renders at most three glyphs and removes unused stale objects each update.

The M1 trigger is a Circle precision marker, not an UpArrow/DownArrow, so it cannot visually masquerade as a second directional signal.

## Single-owner cleanup

The superseded HtfTrendArrowStrengthRule and unused MtfTrendArrowRenderer were removed. The legacy fallbackState path was removed from canonical arrow call-sites so presentation state is not duplicated.

## Verification boundary

Completed automated verification:
- dedicated smart separated arrow audit;
- accumulated Source/Architecture audits;
- Runtime Acceptance contracts;
- cTrader compile/build.

Final manual boundary:
- actual target-terminal arrow spacing;
- glyph appearance and direction;
- realtime relocation;
- stale object removal;
- M1 Circle separation.

Operator action after merge: git pull --ff-only.

## Supersession correction — 2026-10-06

The original phase implementation was later hardened by the current UI signal-presentation contract. Directional signal glyphs no longer render as candle-anchored chart icons. The canonical renderer now places the authoritative 1–3 directional arrow glyphs inside one fixed 66×66 bottom-right chart-control box.

The 1–9 strength ladder remains unchanged and still determines whether one, two or three arrows are visible. Direction and strength continue to come from the single SignalVisualSnapshot / MtfTrendStrengthRule chain.

The M1 precision/confirmation layer no longer creates a separate Circle or other chart marker. This removes the competing-marker path entirely; M1 remains analytical/confirmation evidence only.

The fixed-box contract is now the active visual contract. The older pip/price-coordinate separation and M1 Circle wording in this historical phase description are superseded by this correction.


## 2026-10-07 — Canonical chart-arrow renderer correction
- The single arrow owner remains SignalStackedArrowRenderer.
- The presentation surface is now the chart-object API (Chart.DrawIcon) rather than a chart-control overlay coupled to panel lifecycle.
- Direction still comes from MtfTrendStrengthSnapshot/SignalVisualSnapshot; strength still resolves to 1–9 levels and 1–3 visible arrows.
- Arrow positions are anchored to the canonical M5-to-chart bar and spaced from the candle using ATR-aware distance.
- Panel creation/reordering no longer participates in arrow rendering.
