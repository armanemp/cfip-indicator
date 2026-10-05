# Phase 11.2 — Analysis / Signal / Execution Freshness & Panel Heartbeat

Date: 2026-09-29

## Findings from the operator build

Local Release build completed with zero errors but two warnings.

1. `IndicatorAttribute(string)` was obsolete in the installed cTrader API package. The host now uses the supported parameterless `[Indicator(...)]` form while keeping the class identity `CFIPIndicator`.
2. `_lastAutoPlanTriggerM1` was dead after the same-M5 automatic-plan latch was removed and has been deleted.

## Implementation

### Level-label separation

The historical implementation described below is superseded by the current canonical label contract: native right-aligned ChartText, exact line color, no background/box, and exactly one chart-bar gap from the visible right edge of the complete label text to the line start. Current behavior is owned by PlanLineRenderer + PlanLabelAnchorCalculator + PlanLabelRenderer; no price-space pip/ATR offset or alternate label renderer is permitted.

### Signal pipeline diagnostics

The overview panel now exposes:
`PIPELINE direction • D • T • A • P`

where D = Decision allowed, T = Trigger ready, A = Actionable now and P = Plan active.

A quality row also shows Confidence, Smart Quality, MTF agreement, independent evidence, structural confirmations and location quality.

These rows are presentation-only and do not create another decision authority.

### Automatic execution freshness

Before automatic market broker mutation, `RefreshLiveDecisionActionability()` is called again.

Before aggressive preparation, live actionability is refreshed again.

Before pending-order evaluation, the current decision/actionability state is refreshed. Future Stop/Limit eligibility remains governed by the pending-specific policy and does not incorrectly require market-entry actionability.

Existing safety gates remain intact: permission, single-capacity, daily loss, suitability, spread/risk, volume, SL/TP geometry, execution intent, submission identity, broker confirmation and lifecycle authority.

### Processing heartbeat lamp

A small pulsing `●/○` lamp is placed at the top-right of the panel header.

It is updated from the lightweight runtime Timer heartbeat and therefore does not require a full analysis/render cycle for each pulse.

The lamp is presentation-only and has no effect on decision or execution state.

## Permanent routine update

`ROUTINE.md` now explicitly requires every future phase to perform a deep whole-chain review:

Analysis -> Decision -> Signal -> Alert -> Execution -> Broker confirmation -> Protection/Lifecycle -> Outcome -> Learning

The routine specifically calls out MTF/top-down, OB/FVG and OB+FVG, WaveTrend, divergence, indicator fusion, regime, false-positive/false-negative cohorts, missed-actionable cases, stale state, Entry/SL/TP/reward-path integrity, Auto Trading, Auto Orders, broker geometry, history and verification.

Compile warnings are treated as phase-level defects and must be fixed or explicitly gated.

## Verification

Added `tools/audit_phase_11_2.py` and wired it into Source/Architecture CI.

The audit covers:
- non-obsolete indicator attribute contract;
- dead automatic-plan field removal;
- horizontal and vertical level-label separation;
- heartbeat lamp ownership and timer wiring;
- signal pipeline panel wiring;
- final actionability refresh points for market, aggressive and pending paths.

Whole-project accumulated audits remain mandatory.

## Evidence boundary

Automated verification proves source/contract/compile correctness. Target-terminal replay is still required for final visual spacing at different zoom levels, actual lamp rendering, broker permission state, order rejection/acceptance/fill behavior and empirical missed-opportunity measurement.

## Next phase

Next: Phase 11.3 — execution rejection forensics, missed-actionable cohorts and evidence-driven threshold refinement using accumulated runtime logs and outcomes.