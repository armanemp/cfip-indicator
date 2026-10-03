# Native No-Box Signal Label Refinement — 2026-10-04

## Decision
Do not use ChartRectangle for signal labels. The rectangle is a chart-bound X/Y shape and is therefore a poor fit for compact text tagging: its width/height are price/time geometry and can visually drift or become oversized with chart scaling. cTrader provides ChartText specifically for text anchored to chart coordinates, including HorizontalAlignment and VerticalAlignment. It also provides ChartIcon for small native markers.

## Final presentation
- PlanLineRenderer remains the only line owner.
- Line remains Solid, 1px, finite, exactly 40 bars.
- PlanLabelRenderer is the only label owner.
- Label is ChartText; no filled rectangle is created.
- Text is anchored one bar before the line-left point and right-aligned, so the complete string remains on the label side and cannot extend across the line start.
- A single small Circle ChartIcon marks the exact junction.
- Legacy _BOX objects are actively removed so old installations cannot retain the rectangle.
- No chart controls, panels, or screen-space layout were introduced, preserving performance and zoom/time-price binding.

## Why this is preferable
The implementation uses the chart-native drawing primitives instead of a screen-space control or a second geometry system. That keeps object count and update work minimal while using the API's intended anchoring semantics.

## Verification
Source contract updated. cTrader Release compilation and live terminal visual acceptance are intentionally not claimed until executed locally.
