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

The two cTrader `ToggleButton` controls now use the official `Checked` / `Unchecked` events as the single operator-action boundary.

Programmatic synchronization remains guarded by `_executionToggleSyncing`, so a refresh cannot become an operator command.

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

Source/architecture, runtime acceptance and cTrader compile gates must all pass on the hotfix PR head before merge.

Hands-on cTrader validation is still required for actual touch behavior of chart controls, visual label placement and observed broker/runtime execution behavior.
