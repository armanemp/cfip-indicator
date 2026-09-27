# CFIP Indicator — Modularization & Completion Roadmap

## Baseline

Behavioral source of truth: CFIP-PRO/integrations/ctrader/calude-edit-v73.cs.

## Design target

One cTrader host, one authority per behavior, one indicator/analyzer family per focused source file, explicit models/state, no duplicate engines.

### Target source layout

```text
src/CFIP.Indicator/
  Core/
  Configuration/
  Market/
  Analysis/
    Indicators/
    Structure/
    Zones/
    Liquidity/
    Context/
  Decision/
    Consensus/
    Filters/
    Quality/
    Prediction/
  Planning/
    Entry/
    ExecutionModel/
    Stops/
    Targets/
  Risk/
  Trading/
    Execution/
    PendingOrders/
    Broker/
    Validation/
    PositionManagement/
    Protection/
  Lifecycle/
  Outcomes/
  Presentation/
  UI/
  OSS/
```

## Phases

### Phase 1 — Modular foundation + native indicator isolation
- Establish the permanent roadmap and OSS boundary.
- Split native cTrader indicator families into focused files.
- Preserve v73 behavior and 513 configuration parameters.
- Normalize the baseline automatic-trade identity.
- Add source checks for modular boundaries.

### Phase 2 — Market data and context
- Isolate MTF data, closed-bar time mapping, sessions, volatility, momentum, VWAP and volume context.

### Phase 3 — Structure, zones and liquidity
- Separate swings, BOS, MSS, CHOCH, displacement, FVG, Order Block, supply/demand, mitigation, equal levels, sweeps, pivots and HTF liquidity.

### Phase 4 — Decision and intelligence
- Separate consensus, evidence, confidence, quality, regime gates, prediction and reaction into explicit owners.

### Phase 5 — Planning and risk
- Separate entry, trigger, execution model, structural stop, target ladder, RR, sizing, margin, daily-loss and exposure policy.

### Phase 6 — Trading execution and broker boundary
- Separate market execution, pending orders, broker mutations, validation, retries and idempotency.

### Phase 7 — Lifecycle, protection and outcomes
- Separate state machine, reconciliation, active position management, structural protection, partial TP/BE, reversal/invalidation and telemetry/calibration.

### Phase 8 — Presentation and UI
- Separate chart, labels, historical rendering, panel, popup and alert presentation. UI stays downstream of authoritative state.

### Phase 9 — OSS and advanced intelligence
- Benchmark OSS candidates before admitting them.
- Prefer dependency-light source-level components that compile in the cTrader .NET 6 target.
- Record exact commit/tag, license, provenance, dependencies and compatibility result for every adopted component.

### Phase 10 — Verification and release
- Compile against the installed cTrader Automate API.
- Execute controlled scenarios for market/pending/rejection/slippage/protection/reconnect/reversal/invalidation/EOD.
- Verify rendering and measure CPU/allocation before release.

## Completion rule

A phase is complete only when the intended modules exist, callers use the new authority, duplicates are removed, static checks pass, and the phase status is recorded.

## Progress

```text
P1  [██████████░░░░░░░░░░] Modular foundation + native indicators
P2  [░░░░░░░░░░░░░░░░░░░░] Market/context
P3  [░░░░░░░░░░░░░░░░░░░░] Structure/zones/liquidity
P4  [░░░░░░░░░░░░░░░░░░░░] Decision/intelligence
P5  [░░░░░░░░░░░░░░░░░░░░] Planning/risk
P6  [░░░░░░░░░░░░░░░░░░░░] Execution/broker
P7  [░░░░░░░░░░░░░░░░░░░░] Lifecycle/outcomes
P8  [░░░░░░░░░░░░░░░░░░░░] Presentation/UI
P9  [░░░░░░░░░░░░░░░░░░░░] OSS/advanced extensions
P10 [░░░░░░░░░░░░░░░░░░░░] Verification/release
```

## Phase 1 status

- Completed: native indicator registry and focused EMA, ATR, RSI, ADX, DMI and MACD modules.
- Completed: legacy Indicators.cs no longer contains indicator implementation logic.
- Completed: OSS boundary and register.
- Completed: v73 automatic-trade label default restoration is queued in the next cleanup commit.
- Next: market/context decomposition.
