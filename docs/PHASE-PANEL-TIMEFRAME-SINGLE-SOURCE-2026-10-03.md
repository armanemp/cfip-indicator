# Phase 2 — Panel Timeframe Single Source of Truth (2026-10-03)

## Scope

This phase hardens the Chart Panel timeframe status path so the timeframe lamp, timeframe text, and live header consume the same presentation state derived from the canonical cached Frame objects.

## Root cause

The underlying MTF frames were already calculated through the canonical cached calculation path, but presentation consumers duplicated parts of the interpretation:

- the timeframe lamp called FrameDirection and had its own trend-strength calculation;
- FrameText independently resolved display direction/label;
- the live header independently resolved M15/H1 direction.

This could allow presentation elements to disagree when display-direction rules, readiness, or strength semantics changed.

## Architecture change

Added:

- src/CFIP.Indicator/UI/Panel/PanelTimeframePresentationState.cs

The new canonical resolver derives display direction, direction label, readiness, and display strength.

Updated consumers:

- PanelTrendTimeframeLampRow.cs
- PanelTextFormatting.cs
- PanelHeaderLiveState.cs

The calculation source remains the existing cached MTF frame pipeline; no new calculation loop or competing timeframe clock was introduced.

## Performance

The change does not introduce an additional market-data calculation. Presentation consumes the already-computed frame state. Existing panel render optimization remains keyed by the canonical frame presentation keys.

## Verification

Added:

- tools/audit_phase_panel_timeframe_single_source_2026_10_03.py

and registered it in:

- .github/workflows/source-check.yml

The audit explicitly rejects reintroduced local direction/strength logic in the lamp and verifies shared state consumption.

## Status

Implementation committed directly to main.

Final CI result is intentionally reported only after the new HEAD workflows complete.
