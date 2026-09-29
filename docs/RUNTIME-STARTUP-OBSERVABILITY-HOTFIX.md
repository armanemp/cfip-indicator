# SUPERSEDED — Runtime Startup and Chart Observability Hotfix

# CFIP — Runtime Startup and Chart Observability Hotfix

## Problem

After the Phase 6.2 merge, a live cTrader check showed a completely blank chart for about 30 seconds. The previous diagnostic guide was not a reliable diagnostic surface because it was anchored to a price coordinate above the visible chart.

A second startup race was also present: asynchronous MTF data loading could finish after the host's initial Calculate callbacks, making the runtime ready without guaranteeing that one completed calculation cycle had executed.

## Changes

### Fixed-position chart guide

AnalysisGuideRenderer.cs now uses cTrader's Chart.DrawStaticText API with fixed top-right positioning. The guide reports engine/data/calculation/decision/reaction/visual/plan/MTF state plus loaded dataset counts and initialization age.

The guide no longer depends on the chart price viewport.

### One-shot startup calculation catch-up

RuntimeInitialization.cs calls RunStartupCalculationCatchUp when async initialization reaches _initializationReady.

CalculationCycle.cs now owns the shared RunCalculationCycle(int index) method. Both the normal host callback and the startup catch-up use the same owner, so there is no duplicate calculation implementation.

The catch-up executes only while _lastCalculationCompletedUtc is still DateTime.MinValue. After the first cycle completes, the normal 500 ms runtime heartbeat continues without running full analysis repeatedly.

## Safety boundary

This hotfix does not change decision thresholds, evidence weights, closed-bar decision semantics, intrabar reaction policy, risk sizing, RR, trailing/protection rules, broker mutation ownership, broker-confirmed state authority, managed identity, or the automatic execution safety state machine.

No new parameter was introduced; the enforced production parameter count remains 535.

## Verification

The branch must pass Source / Architecture, Runtime Acceptance Contracts, and cTrader Compile. The fixed-position guide also follows the current cTrader Algo Chart.DrawStaticText API contract.


## Current status

This hotfix was deliberately reverted after live validation showed that its persistent chart guide and timer-driven startup catch-up reduced runtime responsiveness and duplicated the existing panel. It is retained only as historical continuity; the production runtime no longer contains these additions.
