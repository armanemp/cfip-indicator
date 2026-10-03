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
