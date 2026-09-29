# Phase 9.11 — Signal Lifecycle, Quality Recovery & Alert Execution Coherence

Date: 2026-09-29

## Status

VERIFIED COMPLETE. PR #53 merged into `main` as `43101635e24ad15b77374472fc676a8c6fe591d6`.

Verified phase head: `20ba01d4daa145f1118d3795277ed4d6f6a3bed3`.

Automated verification: Runtime Acceptance PASS; cTrader Compile/Build PASS; Source/Architecture + accumulated audit PASS.

## User-facing problems addressed

- stale/expired signal level lines could remain visible after their useful lifecycle ended;
- a legacy chart alert mirror could display `ALERT BUY/SELL` beside signal levels;
- blocked/restricted candidates could still travel through the unified sound/popup path;
- popup defaults were not aligned with the requested bottom-left presentation;
- final actionability quality could reject otherwise strong setups because one near-threshold presentation dimension was slightly deficient;
- signal/plan lines could become visually heavy through the public thickness setting.

## Implementation

### Signal lifecycle

Added `SignalVisualLifecycleRule` as the deterministic presentation-lifecycle owner.

Pre-trade plan visuals are retained only when:
- there is no live position or managed pending order;
- the plan direction still matches the current decision;
- the decision remains EntryAllowed + ActionableNow + TriggerReady;
- the plan age is no more than 2 closed M5 bars.

Setup-preview visuals use the same two-bar bounded lifecycle and current-direction/EntryAllowed checks.

The canonical visual-direction resolver no longer uses an arbitrary stale pre-trade `_plan` merely because a direction is present.

### Signal quality

Added a narrowly bounded quality-recovery path to `ActionableSignalQualityRule`.

A setup may recover one deficient location/timing/price-position dimension only when:
- the deficiency is within 8 quality points;
- confidence, Smart Quality, MTF agreement, independent evidence and structural confirmations all exceed strengthened floors;
- price-position quality is already acceptable;
- TP1 RR exceeds the normal minimum by 0.35R.

Multiple deficient dimensions, hard execution blockers, weak evidence, poor RR, late extension, opposite divergence, momentum conflict, and trap-risk blocks remain hard blockers.

The live actionability staging floor was lowered to 64 for location/timing so strong recovery candidates can reach the final deterministic recovery gate.

### Alerts

The legacy alert-mirror chart surface no longer renders a second signal marker or the `ALERT BUY/SELL` label. Its render call remains only as a cleanup hook for legacy objects.

Unified alert delivery now silently discards `RESTRICT|` / `CFIP ENTRY BLOCKED` events before any sound, popup, email or visual-alert side effect.

### Popup

Default alert popup presentation is now:
- enabled;
- bottom-left;
- not critical-only;
- bold;
- 400px wide;
- 10px padding;
- 10px corner radius.

Existing close button, duration and background-free chart-label contract remain unchanged.

### Signal line presentation

All plan and prediction signal levels are fixed to thickness 1 while retaining the existing public thickness input for compatibility. Line style remains Solid-only.

Level text remains white and background-free.

### Automatic execution & protection

Submission-gate telemetry now records the latest automatic market/aggressive/pending submission state, broker confirmation/rejection/null-result reason and failed-submission reason.

The auto-trade panel exposes compact execution trace state.

The server-side TP/BE ladder now refuses to become broker-owned protection when the structural stop is on the wrong side of the actual execution price, while retaining strict progressive TP geometry. This extends Phase 9.10 protection ownership deeper into the pre-adoption geometry check.

## Verification intent

Required gates:
- Decision Contracts;
- Runtime Acceptance;
- cTrader Compile/Build;
- Source/Architecture;
- accumulated whole-project audit.

Target-terminal cTrader replay remains required for empirical signal timing, false-signal rate, visual cleanup, popup behavior, broker protection timing, realized SL/TP and execution outcomes.

## Continuation

Next phase: Phase 9.12 — deepen broker outcome/recovery telemetry and historical signal lifecycle calibration without introducing a second decision authority.
