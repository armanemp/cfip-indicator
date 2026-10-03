# Phase 6.4 — Compact 40-Bar Plan-Level Visuals

Date: 2026-09-29

## Objective

Make the chart-level trade-plan presentation compact, readable and visually
consistent with cTrader-style price labels without adding public parameters or
touching decision/execution semantics.

Requested operator presentation:

- Trigger / Entry / SL / TP levels must not span the entire chart.
- Each level spans exactly the latest candle back by 40 bars when sufficient
  history exists.
- The level name and normalized price are shown in a small tag at the left end
  of the level.
- The tag uses the same semantic color as its level.
- Level styles are intentionally differentiated so Entry is visually primary,
  Trigger/SL are secondary guides, and TP stages remain readable.
- The renderer must reuse chart objects instead of removing/recreating every
  label on every refresh.

## Implementation

### 1. Fixed compact line span

`PlanLineRenderer` now owns the fixed compact span:

- `CompactPlanLineLengthBars = 40`;
- right anchor = `Bars.Count - 1` (latest chart candle);
- left anchor = latest bar - 40;
- no use of `Chart.FirstVisibleBarIndex` or
  `Chart.LastVisibleBarIndex` for plan-level span;
- `ExtendToInfinity = false`.

The existing `FullWidthLevelLines`, `LineLengthBars` and
`LineForwardBars` settings are no longer used to expand the production plan
level beyond the requested compact presentation.

### 2. Level styling

The existing color authorities remain unchanged.

Style differentiation:

- Entry: Solid, at least 2 px;
- Trigger: Dots;
- SL: DotsRare;
- TP1: Solid;
- TP2: Lines;
- TP3: Lines;
- TP4: LinesDots.

No trading rule, threshold, risk, RR or broker rule is changed.

### 3. Compact left-side tags

`PlanLabelRenderer` now renders:

- a reusable `ChartText` for the name + price;
- a small outline `ChartRectangle` behind the tag;
- tag placement at the compact line's left anchor;
- small fixed chart-label font (9 px);
- bold text;
- left/center text alignment;
- same full-strength color as the corresponding level;
- non-interactive chart objects.

cTrader's official API exposes `Chart.DrawText` for chart-bound text and
`Chart.DrawRectangle` with fill/interactivity controls, which are the two
objects used for the compact tag presentation.

### 4. Rendering hot-path optimization

The old plan-label coordinator cleared all labels before rebuilding them.
Phase 6.4 changes this to:

- update existing `ChartText` / `ChartRectangle` instances in place;
- remove only labels that are no longer visible;
- keep full cleanup available for plan teardown and state transitions.

This reduces unnecessary chart-object churn during live refreshes.

## Safety boundaries

Unchanged:

- broker-confirmed state authority;
- decision authority;
- automatic execution authority;
- aggressive controlled-intrabar qualification;
- market/pending execution semantics;
- structural SL/TP calculation;
- risk sizing;
- RR/target policy;
- protection and lifecycle semantics;
- 535 public-parameter contract.

## Verification

Required gates:

1. Source / Architecture
2. Runtime Acceptance Contracts
3. cTrader Compile

Live cTrader visual/runtime acceptance remains a terminal-side responsibility;
repository gates do not prove actual chart rendering or device performance.

## Operator acceptance checklist

After pulling the merged commit and rebuilding in cTrader:

- plan levels occupy at most the latest 40-bar span;
- tags sit at the left edge of the level;
- tags remain legible and do not use the right side of the chart as a permanent
  text column;
- Trigger / Entry / SL / TP colors match their level colors;
- level style differences are visible without overpowering candles;
- labels update without visible flicker or repeated object buildup;
- startup and live refresh remain responsive.

## Next phase

After live verification of this presentation phase, continue with
**Phase 7.1 — Hidden-clamp audit**.


---

## Superseded visual contract — 2026-10-04

The implementation described in the original 6.4 phase note is historical. Its label box/background, semantic-color text and earlier horizontal-alignment wording are superseded by the later canonical presentation contract.

Current authoritative contract:
- solid, finite, one-pixel signal/plan lines;
- exactly 40 chart bars ending at the latest candle;
- label anchor before the line start with deterministic horizontal gap;
- ChartText.HorizontalAlignment = Right so the visible label renders to the left of the line;
- exact level price, white text, no background;
- one ChartText mutation owner: PlanLabelRenderer.UpsertPlanLabel.

Do not reintroduce the historical box/second-renderer/alignment behavior.
