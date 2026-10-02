# Build Warning / Panel Height Integrity — 2026-10-02

## Status

**IMPLEMENTATION COMPLETE — verification pending.**

This phase closes the locally observed Release-build warnings and removes the remaining
panel/chart geometry feedback path that can distort or collapse the chart when the
Indicator is attached.

## Root causes closed

### Release build warnings

The shared Contracts project has nullable reference types enabled. Two deserialized/init-only
transport properties were not initialized, and the management JSON deserializers assigned
nullable values into non-nullable locals.

Corrections:
- BrokerExecutionReport.CommandIdempotencyKey now initializes to string.Empty;
- ManagementCommand.ExecutionLabel now initializes to string.Empty;
- management deserializers explicitly model nullable parsed JSON arrays;
- failure-state behavior remains unchanged: malformed/empty payloads still return false and leave the output null.

The Indicator _lastPendingSignalM5 field was dead state and is removed rather than artificially referenced.

### Chart collapsing / height regression

The Indicator is an overlay control host. Its Provider Heartbeat output is transparent,
but it writes the monotonically increasing provider revision into an output series. cTrader
documents AutoRescale as controlling whether an Indicator automatically rescales the chart;
its default is true. This phase therefore sets AutoRescale = false, because the provider
revision is metadata/heartbeat rather than a price series and must never influence the chart's scale.

The panel geometry path is also now fully configuration-owned:
- PanelMainRenderer continues to use the canonical maximum-height resolver;
- ResolvePanelMaximumHeight no longer reads Chart.Height;
- the obsolete chart-height baseline state/capture is removed;
- the panel remains an explicitly sized overlay control with bounded scrolling.

This eliminates the remaining layout feedback path where a transient chart viewport could be fed back into panel sizing.

## Safety / architecture invariants

- Indicator remains Analysis / Decision / Scenario / Plan / Presentation;
- cBot remains the broker mutation authority;
- no execution strategy, confidence, RR, Entry, SL, TP or signal threshold is changed;
- no parallel panel renderer or compatibility warning wrapper is added;
- panel height is not allowed to depend on a transient chart viewport.

## Verification

Required:
- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- new build-warning/panel-height audit.

Target-terminal manual check remains required for:
- chart plot area remains full-height after attach/reload;
- panel contents remain scrollable and complete;
- hide/show does not alter chart height;
- provider heartbeat remains invisible and does not change price scale.

## Operator action after merge

git pull --ff-only

## Next phase

After this phase, resume CBOT-P5 — Protection / Lifecycle / Recovery completion and then the
signal-quality/target-quality work. The next signal-quality phase must specifically audit why
fallback targets cluster at 2R and why weak setups survive the decision-to-actionability path;
it must use the canonical signal/plan pipeline and replay evidence before numeric retuning.
