# CI-20 — Panel / cBot / Analysis Engine Coherence — 2026-10-02

## Status

VERIFIED COMPLETE — repository gates PASS on final implementation head `60a0bad5cbcf300521db93aa275b4c00df5b1903`.

## Scope

This implementation closes three connected seams before continuing deeper protection behavior:

1. Panel options are mapped to canonical consumers and continuously audited.
2. The Indicator panel receives broker-authoritative cBot runtime state through a read-only Device-scope contract.
3. Analysis performance and evidence quality are improved without introducing public threshold parameters.

## Panel corrections

- `PanelWidth` runtime clamps now honor the public 220..700 range.
- `PanelMaxHeight` runtime clamps now honor the public 260..1200 range.
- `PanelRowGap` and `PanelRowPadding` now affect actual row controls, not only height estimation.
- `ShowTrigger` is honored by the unified trade-plan panel for a distinct trigger level.
- AUTO EXEC, AUTO ORDERS and PROTECTION rows now expose the actual cBot connection/capability state.
- cBot stale/not-attached state is rendered as a warning rather than being mistaken for executable readiness.
- A one-by-one panel option audit is accumulated in Source/Architecture CI.

## cBot continuation

- Added `CbotExecutionStateSnapshot` and Device-scope transport.
- cBot publishes a bounded heartbeat plus immediate broker lifecycle transitions.
- Broker lifecycle events observed: Positions Opened/Modified/Closed and PendingOrders Created/Modified/Filled/Cancelled.
- Indicator only reads this state; broker mutation authority remains in the cBot.
- Pending execution expiry validation was consolidated into one canonical check.
- cBot host launch default is restored to M5; execution logic remains independent of the host chart timeframe.

## Analysis / signal engine

- Reuse an aligned Frame cache for M15/M30/H1/H4/D1/W1 so unchanged higher-timeframe bars are not recomputed on every closed M5 cycle.
- Independent evidence-family coverage is now carried through the decision evidence snapshot.
- Bounded evidence-diversity quality bonus is applied by the canonical decision-quality owner.
- No new public threshold parameter was introduced.

## Verification boundary

Required repository checks:
- accumulated Source/Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- CI-20 panel option audit;
- CI-20 cBot state continuity audit;
- CI-20 analysis quality/performance audit.

Repository verification on the final head:
- Source / Architecture #3131: **PASS**;
- Runtime Acceptance Contracts #2940: **PASS**;
- cTrader Compile/Build #3124: **PASS**;
- accumulated CI-20 panel option, cBot state-continuity and analysis-quality/performance audits: **PASS**.

Manual target-terminal validation remains required for:
- visible panel controls and spacing on the real cTrader terminal;
- actual cBot attach/detach/stale transitions;
- real broker lifecycle and management behavior;
- signal frequency/quality and empirical outcomes.

## Next

Next implementation: **CI-20 protection/trailing and lifecycle/recovery completion**, using one canonical protection owner for monotonic SL tightening, profit-locking and structure/reward-driven target progression without constant TP movement.
