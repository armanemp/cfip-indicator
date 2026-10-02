# CI20C — cBot Connection, Lifecycle and Recovery Truth — 2026-10-02

## Status

IMPLEMENTATION COMPLETE — verification pending on the CI20C feature branch.

## Objective

Remove the ambiguity between:

- cBot absent from the chart;
- cBot present but stopped/restarting;
- cBot running but its state heartbeat unavailable/stale;
- cBot running with a fresh broker-runtime state.

The panel must not infer physical cBot attachment from LocalStorage alone.

## Completed

### 1. Canonical cBot identity

Added `CFIP.Contracts.CbotIdentity` and changed the cBot display attribute to consume the shared constant.

### 2. Same-chart presence detection

The Indicator now treats a fresh cBot heartbeat keyed by the exact `IndicatorInstanceId` as the connection proof. The cBot side keeps same-chart binding authoritative through `ChartIndicators` and event-driven rebind.

Panel states are explicit:

- `CBOT NOT ATTACHED • ATTACH TO THIS CHART`
- `CBOT AMBIGUOUS`
- `CBOT STOPPED` / `CBOT RESTARTING` / `CBOT STOPPING`
- `CBOT CONNECTING • HEARTBEAT PENDING`
- `CBOT CONNECTED • ...`

This keeps chart presence separate from heartbeat freshness.

### 3. Execution safety

Indicator-side execution capability checks now require both:

1. the cBot has successfully bound the exact Indicator instance on its chart;
2. its published execution state is fresh.

A stale storage snapshot therefore cannot be treated as a live cBot execution authority.

### 4. cBot lifecycle rebinding

The cBot now subscribes to chart Indicator Added/Removed/Modified events and immediately rechecks the canonical CFIP Indicator binding.

This closes the startup-order / late-attachment seam that previously depended on the next periodic tick.

The cBot also publishes a terminal stop-state before detaching its lifecycle handlers.

### 5. Existing execution architecture preserved

No second broker-mutation owner, no second decision engine, no new public strategy thresholds, and no Cloud transport were introduced.

The cBot remains demo-only and live accounts remain blocked.

## Verification

Required:

- Source/Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- accumulated static audits through CI20C.

Manual target-terminal acceptance remains required for:

- actual same-chart attachment;
- startup in either order;
- restart/reload;
- panel update latency;
- broker lifecycle;
- execution synchronization.

## Safety / strategy scope

No public parameter/default, confidence threshold, RR threshold, Entry/SL/TP threshold or position-capacity change is part of CI20C.

Empirical signal accuracy/performance is not claimed from repository tests.
