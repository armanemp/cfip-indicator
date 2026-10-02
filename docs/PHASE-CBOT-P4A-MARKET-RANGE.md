# CBOT-P4A — Market / Market Range Broker Mutation Extraction

Date: 2026-10-02

Status: **IMPLEMENTATION COMPLETE — repository/CI verification pending.**

## Scope

This controlled batch moves broker mutation authority for Market and Market Range into the cBot while retaining analytical ownership in the Indicator.

## What moved

Contracts now carry the exact execution label and MarketExecutionProfile needed by the cBot.

The Indicator now validates/calculates/prepares and publishes the Market intent; it no longer calls ExecuteMarketOrder or ExecuteMarketRangeOrder.

The former Indicator owner `src/CFIP.Indicator/Trading/Execution/BrokerMarketOrderMutation.cs` was physically removed.

New cBot owners:
- `src/CFIP.cBot/Execution/MarketExecutionIntentRule.cs`
- `src/CFIP.cBot/Execution/MarketBrokerMutation.cs`
- `src/CFIP.cBot/Execution/MarketExecutionCoordinator.cs`

The cBot revalidates the intent, rechecks live quote and single-plan capacity, uses the exact managed label, owns Market/Market Range broker submission, and keeps a bounded idempotency ledger.

`Enable Market Execution` defaults to false; `Use Market Range` defaults to true.

## Protection boundary

The Indicator remains the canonical owner of analytical Stop/Target and server-ladder calculation. P4A transports the exact resulting ladder payload only when required for Market submission.

The broader TP/SL lifecycle, trailing, close, pending and recovery mutation owners remain later extraction work.

## Safety

- No decision, confidence, RR, Entry, SL or TP selection thresholds were tuned.
- Unsupported or incomplete Market intents fail closed.
- Missing execution identity fails closed.
- Invalid geometry or non-progressive ladder fails closed.
- Duplicate idempotency keys do not resubmit.
- cBot Market mutation is disabled by default.

## Verification

Required repository gates: Source/Architecture, Runtime Acceptance Contracts, cTrader Compile/Build, the dedicated P4A audit, and accumulated project audits.

Target-terminal evidence is still required for actual broker acceptance/fill/rejection, slippage, Market Range behavior and runtime timing.

## Next

**CBOT-P4B — Aggressive Market broker mutation extraction.**
## TRADE-SYNC-01 integration

The cBot now binds its hosted CFIP analysis instance to the exact `CFIP Smart Indicator` configuration attached to the same chart. Parameter values are copied from `ChartIndicator.Parameters`; the cBot rechecks a compact fingerprint every 500 ms and rebuilds its hosted analysis instance only when the visible configuration changes.

The previous hard-coded hidden Indicator configuration was removed. This prevents an execution-side instance from silently using a different Auto Trading, risk, news, target or confluence configuration than the Indicator visible to the user.

Analysis plan/decision generation is also decoupled from broker arming. `EnableAutoTrading` remains an execution gate but no longer changes decision policy or whether the analytical plan can be built.

The cBot remains fail-closed when the named Indicator is missing or duplicated.
