# Build Warning / Panel Height Integrity — 2026-10-02

## Status

**VERIFIED COMPLETE — merged to `main` via PR #198 as `396c72513fc5043bd348e5ceb5c74894da72c395`.**

This phase closes the locally observed Release-build warnings and removes the remaining
panel/chart geometry feedback path that could distort or collapse the chart when the
Indicator was attached.

## Root causes closed

### Release build warnings

The shared Contracts project has nullable reference types enabled. Two init-only transport
properties were not initialized, management JSON deserializers assigned nullable values to
non-nullable outputs, and one Indicator field was dead state.

Corrections:
- `BrokerExecutionReport.CommandIdempotencyKey` initializes to `string.Empty`;
- `ManagementCommand.ExecutionLabel` initializes to `string.Empty`;
- management deserializers explicitly model nullable parsed arrays while preserving failure semantics;
- dead `_lastPendingSignalM5` state is removed.

### Chart collapsing / height regression

The Indicator is an overlay host and its Provider Heartbeat writes a monotonically increasing
provider revision into an invisible output series. Automatic chart rescaling was therefore
disabled because this series is metadata rather than price data.

The panel geometry path is fully configuration-owned:
- `ResolvePanelMaximumHeight` no longer reads `Chart.Height`;
- the obsolete chart-height baseline state/capture is removed;
- the panel remains explicitly sized and bounded by the ScrollViewer.

These changes remove both the chart-scale contamination path and the remaining panel/chart
height feedback path.

## Verification

- cTrader Compile/Build workflow #3064: **PASS** on the feature head;
- Runtime Acceptance workflow #2880: **PASS** on the feature head;
- the reported files produced no warning/error diagnostics in the compile log;
- P4B, P4C, P4D and P4E audits all passed on the feature head after audit reconciliation.

Note: the broader Planning Contracts build still emits pre-existing `CS0649` warnings in
`TradeOpportunityCandidate.cs`; those are separate from the 9 warnings reported locally here
and were not expanded into this phase.

Target-terminal manual check remains required for:
- full chart plotting area after Indicator attach/reload;
- panel contents and scrolling;
- hide/show behavior;
- provider heartbeat not changing the price scale.

## Operator action

Run:

`git pull --ff-only`

## Next phase

Resume **CBOT-P5 — Protection / Lifecycle / Recovery completion**. The signal-quality/target-quality
phase remains separate and must specifically investigate weak setups and the 2R fallback cluster
using the canonical signal/plan pipeline and replay evidence before numeric retuning.
