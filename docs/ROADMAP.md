# CFIP Indicator — Roadmap

## Mission

Build the complete CFIP cTrader indicator as a clean, modular, testable system without losing validated behavior from the complete reference implementation.

## Stages

1. Repository and architecture foundation
2. Complete parameter and behavior inventory
3. Market data, time and MTF
4. Market model and indicators
5. Structure, FVG, Order Blocks and liquidity
6. Decision and confluence
7. Entry and trigger
8. Risk, structural SL and target ladder
9. Unified execution
10. Pending-order lifecycle
11. Position lifecycle and broker reconciliation
12. Live position management
13. Outcome telemetry and calibration
14. Chart, terminal/panel, alerts and prediction presentation
15. Cleanup, performance and resource control
16. Full verification and release

## Mandatory rules

- Automatic trading is supported.
- Automatic pending orders are supported.
- Manual BUY/SELL/STOP/LIMIT entry controls are not part of the product.
- Smart SL/TP are strategy outputs, not broker defaults.
- Visual entry/trigger/SL/TP state comes from the same authoritative trade plan used by execution.
- Broker-confirmed state is authoritative after side effects.
- Analytical decisions use closed-bar data.
- BUY/SELL behavior remains symmetric.
- Fallbacks and important decisions have provenance.
- Tick/event paths are idempotent.
- A stage is complete only after its acceptance gates pass.

## Current position

The initial modular structural migration is in place. The next work is behavioral completion: recover every capability present in the complete reference implementation, then harden the resulting modules through contracts, tests and runtime validation.
