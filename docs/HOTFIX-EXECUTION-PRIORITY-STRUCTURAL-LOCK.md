# Hotfix — Execution Priority, Toggle State Events and Structural Lock

Date: 2026-09-29

## Reason

This corrective hotfix addresses runtime behavior reported after the previous UI/protection hotfix:

- chart AUTO TRADE / AUTO ORDERS controls must mutate the runtime execution flags reliably;
- predictive pending orders must get an opportunity before a market plan suppresses them;
- aggressive market entry must get an opportunity before a normal market plan is materialized;
- an existing managed pending order must not be followed by a newly recreated market plan;
- chart plan labels must retain their compact semantic-color box presentation;
- structural protection must not degrade into raw market-price chasing.

## Implementation

### 1. Execution controls

The two cTrader `ToggleButton` controls now use one direct `Click` action as the single operator-action boundary. The handler toggles the canonical runtime flag and then synchronizes the visual control state.

Programmatic synchronization remains guarded by `_executionToggleSyncing`, so a refresh cannot become an operator command. The runtime properties remain the sole execution authority.

The controls are explicitly enabled.

### 2. Execution priority

The live calculation execution sequence is now:

1. execution-model preflight;
2. predictive pending-order execution;
3. aggressive AUTO TRADE execution;
4. automatic plan creation;
5. normal automatic market execution.

This prevents plan creation from starving predictive orders or qualified aggressive reaction entry.

### 3. Pending-order / market-plan arbitration

Automatic plan creation now defers while a managed pending order exists.

Predictive pending orders continue to require broker-confirmed submission and their own existing structural entry/SL/TP validation.

### 4. Structural protection

The previous raw-market final distance clamp remains removed.

Further trailing progression is still accepted only from structurally-derived candidates and closed-M5 structural updates, while broker state remains authoritative.

### 5. Predictive reversal pending levels

Reversal LIMIT preparation no longer derives its entry from the current quote or
from the execution-model ideal entry. It now asks a dedicated predictive selector
for a future structural level.

The selector combines:

- M5 and M15 FVG / two-bar imbalance zones;
- M5 order blocks;
- swing structure;
- equal highs/lows used as liquidity references;
- MTF direction/structure;
- MSS / CHOCH, liquidity, volume, MACD, VWAP, volatility and OSS votes;
- bounded price-distance and source-deduplicated confluence scoring.

A candidate must remain materially beyond the current market price in the intended
entry direction, must satisfy the configured smart-quality floor, and is then
passed through the existing structural SL/TP and broker-confirmation path.

### 6. Visual signal parity

When an audible signal alert is accepted by the alert engine, the same event updates
a non-authoritative visual alert state. The chart presentation path renders an
arrow at the alerted closed bar and can also render a compact semantic signal box.

Prediction objects are explicitly rendered in the live presentation path whenever
there is no plan/pending/setup-preview state hiding them.

### 7. Setup geometry and trailing stability

Setup execution geometry is no longer rebuilt merely because the live quote moved.
It is rebuilt on a newly closed M5 bar or a direction change, so preview Entry/SL/TP
levels do not chase the market tick-by-tick.

Smart trailing remains structurally derived. Exit-pressure now tightens the
structural trail room rather than constructing a new stop from the live market price.
## Parameters

No public parameter was added or removed.

Production parameter count remains 535.

## Safety invariants preserved

- broker-confirmed position/order state remains authoritative;
- accepted submission is not treated as a fill until confirmation;
- rejected broker mutations are not treated as successful;
- managed identity remains single-source;
- no manual BUY/SELL entry control was introduced;
- no backward SL movement was introduced;
- no synthetic broker position/order state was introduced.

## Validation

The corrective PR (#28) passed all three repository gates before merge:

- Source / architecture: PASS
- Runtime acceptance contracts: PASS
- cTrader compile: PASS

Merged into `main` as commit `c8fd3db7761d45b094986105e585a49b72645ba5`.

Hands-on cTrader validation is still required for actual chart-control touch behavior, visual label placement and observed broker/runtime execution behavior.
