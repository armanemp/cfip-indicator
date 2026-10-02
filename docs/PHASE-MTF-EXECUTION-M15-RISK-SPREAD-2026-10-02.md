# MTF-EXECUTION-M15 — Primary Execution + Smart Margin/Spread Risk — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — repository verification pending on branch.**

## Decision

- M15 is the canonical execution timeframe.
- M5 and M1 are defensive tuning layers: they refine timing/location and can block adverse microstructure but do not replace M15.
- H1, H4, D1 and W1 are higher-timeframe context/reward-path layers.
- The cBot defaults to M15 and fails closed when hosted on another timeframe.

## Risk / price contract

- Indicator requested volume remains derived from stop risk and existing smart-risk policy.
- Existing `Include Spread In Risk Sizing=true` adds current spread to the stop-risk distance used for sizing.
- The canonical RR math now exposes `NetReward` and subtracts spread from reward as well as adding spread to effective risk.
- Synthetic RR targets include the spread cost so their effective reward is not overstated.
- The cBot performs a final broker margin-budget cap after margin estimation and volume normalization. It can only reduce requested exposure and blocks when the result would fall below broker minimum.

## cBot execution

- `CFIP Smart Execution Bot` default host timeframe is M15.
- Non-M15 startup is fail-closed.
- Market/Aggressive demo execution remains bounded and demo-only.
- Existing live-account hard stop remains unchanged.

## Verification

Required: Source/Architecture, Runtime Acceptance Contracts, cTrader Compile/Build, and the new M15/risk/spread audit.

## Manual target-terminal boundary

Verify on actual cTrader: M15 chart launch, M15-only cBot startup, full panel/responsiveness, M15 signal alignment, M5/M1 defensive blocking/refinement, H1+ target/context visibility, broker margin cap behavior and spread-aware SL/TP geometry.

Signal quality and profitability claims remain open until replay/OOS/walk-forward evidence is available.