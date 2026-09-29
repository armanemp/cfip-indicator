# Phase 9.15 — Startup Responsiveness & Portable Long-Term History

Date: 2026-09-29

## Objective

Address the reported startup delay and establish durable history archives that
survive operating-system migration without deleting prior learning evidence.

## Startup finding

The previous initialization path waited for all requested timeframe callbacks
and required deep minimum history: M5 100, M15 100, M30 80, H1 80, H4 60.
D1/W1 also contributed to the pending asynchronous load count when weekly
context was enabled, even though D1/W1 are not required by the primary closed
MTF decision contract.

## Startup implementation

1. Earlier core readiness

Primary readiness now uses internal minimum history of M5 60, M15 50, M30 45,
H1 40 and H4 36. These are internal readiness values, not public parameters.

2. D1/W1 no longer block critical startup

D1/W1 are still requested when SmartWeeklyContext is enabled, but are optional
asynchronous loads. Their callbacks adopt the bars, register native indicators
when appropriate and invalidate the MTF context cache.

3. Startup progress

While initialization is incomplete, the panel header receives explicit status
and current core timeframe bar counts so loading is distinguishable from a
stalled analytical state.

## Portable 90-day outcome archive

The existing cTrader LocalStorage remains the fast recent-memory layer.

A second durable layer is stored in the cTrader designated file folder:

Documents/cAlgo/Data/Indicators/CFIPIndicator/History/

Restricted .NET 6 cTrader algorithms can perform file operations inside their
designated algorithm folder while retaining AccessRights.None. The platform
protects other user directories. See current cTrader file-operations docs.

Each broker-confirmed outcome is appended to a CSV archive in a fixed 90-day
UTC bucket. File names include Symbol, Timeframe, configuration fingerprint
and the bucket start/end dates. Existing archive files are never deleted.

## Long-term learning

Archive import is deliberately deferred until after the first usable startup
calculation seed so archive scanning cannot block the initial signal engine.

The full archive for the current Symbol/Timeframe/configuration namespace is
aggregated into separate long-term calibration counters. The existing decision
calibration order is recent bounded history first, then persistent archive
baseline, then the existing in-memory aggregate fallback.

Thus old history remains useful without putting an unbounded list into the hot
path. Outcome memory never becomes a second BUY/SELL authority and never
submits or mutates broker orders.

## Migration / backup

To preserve the learning state during a Windows or machine migration, copy
both:

Documents/cAlgo/LocalStorage/Indicators/CFIPIndicator/
Documents/cAlgo/Data/Indicators/CFIPIndicator/History/

The first is cTrader LocalStorage recent memory; the second is the portable
long-term CSV archive.

## Safety and optimization

- AccessRights remains None.
- No unrestricted file access was introduced.
- Archive owner contains no delete operation.
- Recent adaptive risk remains bounded and can only reduce suitability-derived risk.
- No second decision, execution or broker-mutation authority was introduced.
- Whole-pipeline audit remains mandatory each phase: Analysis -> Decision ->
  Signal -> Alert -> Execution -> Broker confirmation -> Protection/Lifecycle
  -> Outcome -> Learning.

## Evidence boundary

This phase fixes a concrete startup bottleneck and adds portable memory. It does
not claim a measured profitability or win-rate improvement. Target-terminal
replay is still required to measure startup latency, signal timing, false
signals, missed opportunities, realized R and end-to-end execution quality.

## Next phase

Phase 9.16 should use the persistent archive and startup diagnostics to produce
a replay/measurement report by signal stage, block reason, lane and regime before
any production defaults are changed.

## Final verification

Phase 9.15 is verified and merged into main.

- Runtime Acceptance: PASS
- cTrader Compile/Build: PASS
- Source/Architecture + accumulated audits: PASS
- Startup/persistence audit: PASS
- Decision Contracts: PASS within the build workflow
- PR #57: merged
- Merge commit: 365abb790a49e85ad58c87aa5a92f49d37b7f77c

The source gates validate the implementation and architecture. Actual startup
latency and live/replay trading behavior still require observation on the target
cTrader terminal.
