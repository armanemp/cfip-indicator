# CBOT-P4C — Pending Stop Authority + Host-Timeframe Independence — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — final CI verification rerun.**

## Scope

Move Pending Stop broker mutation out of the Indicator without copying the analytical engine, while making the entire Indicator/cBot execution identity independent of the Chart timeframe.

## Final architecture

- M15 is the **internal execution clock**.
- M5 and M1 are defensive tuning layers for entry timing, adverse microstructure and loss reduction.
- H1/H4/D1/W1 are higher-timeframe context and reward-path layers.
- Indicator and cBot may be attached to any Chart timeframe. Chart timeframe is host/presentation context only and must not change signal identity, history identity, execution action or broker behavior.
- Pending Stop mutation is cBot-owned; Indicator only prepares and publishes the canonical immutable intent.

## Pending Stop flow

`M15 primary decision → M5/M1 tuning → structural trigger → spread-aware executable trigger → SL/TP → risk sizing → canonical ExecutionIntent → cBot validation → shared margin/capacity safety → broker PlaceStopOrder → confirmed pending state`

The Indicator no longer calls the broker Pending Stop API.

## Spread contract

Pending Stop trigger is converted once through `PendingEntryPriceRule.ForExecutableStop`:

- BUY: structural trigger + current spread;
- SELL: structural trigger - current spread;
- tick-normalized before validation.

This prevents an entry trigger from being interpreted using chart/structural price while the broker executes against the opposite side of the spread.

Market execution continues to use the canonical executable quote (Ask for BUY / Bid for SELL).

## Risk contract

- Indicator stop-risk sizing keeps the existing spread-aware `IncludeSpreadInRiskSizing` behavior.
- cBot owns final account/margin enforcement.
- `BrokerExecutionSafety` is the single shared cBot owner for capacity and margin-volume capping; Market and Pending Stop do not duplicate these helpers.
- Margin capping can only reduce exposure and fails closed if the minimum broker volume cannot fit the available risk budget.

## Cleanup performed

- deleted `src/CFIP.Indicator/Trading/Execution/BrokerPendingOrderPlacement.cs`;
- removed the Indicator Pending Stop broker caller and submission mutation path;
- removed duplicate cBot margin/capacity helpers from Pending Stop;
- migrated active audits/scripts to the new cBot owner;
- updated active docs to distinguish internal M15 execution from Chart timeframe;
- normalized cBot session-cap property naming;
- added deterministic Pending Entry spread tests;
- added Pending Stop shadow contract coverage;
- added dedicated `tools/audit_phase_cbot_p4c.py`.

## Verification

Required gates:
- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- CBOT-P4C audit.

Target-terminal manual checks remain required for:
- Indicator/cBot attached separately to M1, M5, M15, H1 and higher charts;
- identical execution identity across those host charts;
- M15-driven position intent;
- M5/M1 defensive refinement;
- H1+ reward-path behavior;
- Pending Stop broker placement and confirmation;
- spread-aware trigger under wider spreads;
- margin cap behavior and fail-closed minimum-volume behavior;
- panel responsiveness.

No profitability claim is made from this structural phase alone.

Next staged execution migration: **CBOT-P4D — Pending Limit authority extraction**.


Final verification rerun is intentionally tied to the latest branch head after all audit-owner reconciliation commits.
