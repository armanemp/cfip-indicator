# CR6.1 / F1 — Opposing FVG/OB target-path direction, mitigation and obstacle caching

Status: **IMPLEMENTED — repository verification pending**

تأیید می‌کنم — `RewardPathZoneObstacleScanner.HasOpposingZonePathObstacle` (base `main` line 9) and `ZoneLookup.FindNearestOpposingZone` (base `main` line 50) are the F1 owners reviewed before change.

## Root cause

The target-path obstacle scanner constructed FVG geometry using the trade direction, which inspects the same-direction imbalance family rather than the opposing zone family. The named `FindNearestOpposingZone` helper also passed the trade direction directly to FVG and OB lookup.

The scanner additionally duplicated raw FVG candle-gap construction, bypassing the managed FVG lifecycle/mitigation pipeline.

## Correction

- `RewardPathObstacleRule` is the single owner for resolving the opposite zone direction.
- FVG and OB obstacle scans now query the opposite trade direction.
- FVG obstacles are consumed from the existing cached managed candidates.
- OB obstacles are consumed from the existing cached managed candidates, preserving lifecycle state.
- Broken/null-lifecycle OB candidates fail closed.
- `FindNearestOpposingZone` now uses the same opposite-direction owner.

## Verification

Deterministic Runtime Contracts cover BUY/SELL symmetry, FVG mitigation, and OB mitigation/lifecycle. `audit_phase_6_1.py` is wired into the accumulated Source/Architecture workflow immediately after E8.

## Behavior boundary

No public `[Parameter]` name, type, or `DefaultValue` changed. No RR/confidence/SL/TP threshold was retuned. No decision or execution authority changed.

The intended behavior change is limited to target-obstacle evaluation using the correct opposing managed FVG/OB zones.

## Manual cTrader verification

Required for target-terminal closed-bar timing, partially/fully mitigated opposing zones, cache refresh behavior, and actual target suppression behavior.

## Out-of-scope discoveries

New unrelated findings remain documented only and are not fixed in CR6.1.
