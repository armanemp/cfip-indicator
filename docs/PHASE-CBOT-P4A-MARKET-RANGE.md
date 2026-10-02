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