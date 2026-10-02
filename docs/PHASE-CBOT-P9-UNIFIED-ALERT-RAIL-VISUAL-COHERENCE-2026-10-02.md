# CBOT-P9 — Unified Alert Rail / Visual Coherence / cBot Signal Preflight — 2026-10-02

Status: **VERIFIED COMPLETE — automated gates PASS; target-terminal visual acceptance remains manual.**

## Implemented

- Removed production Popup renderer/remover/expiry and the obsolete Popup-scoped delivery processor.
- Removed the obsolete Popup parameter surface; current Indicator parameter count is 548.
- Kept one bounded AlertDeliveryQueue as the canonical alert transport.
- Added a bounded five-row panel alert rail beside the Hide/Show control.
- Alert rail colors follow canonical direction/priority semantics.
- Optional sound is delivered from the same queued alert event after panel presentation.
- BUY/SELL plan-level rendering remains one symmetric renderer contract: Solid, thickness 1, finite 40-bar geometry, background-free labels and semantic line/text color.
- Removed popup lifecycle hooks from calculation/runtime paths.
- Added a single cBot SignalEnvelope preflight owner for identity, future timestamp, staleness and symbol scope.
- Preserved cBot broker mutation ownership, environment gating, broker confirmation and existing single-plan safety.

## Full-chain audit

Pre-analysis → M15 decision → M5 trigger/tuning/entry precision → M1 optional confirmation → entry/SL/TP geometry → signal → alert/message → SignalEnvelope → cBot binding/transport → cBot preflight → execution safety → broker mutation → broker confirmation → protection/lifecycle → outcome/history.

Timeframe contract remains:
M15 = canonical trade-decision/execution reference; M5 = trigger/tuning/entry precision; M1 = optional confirmation; H1+ = context/reward; chart timeframe = presentation only.

## Automated verification

PASS:
- Source / Architecture checks
- parameter inventory/count and parameter semantics
- full-project integrity
- runtime UI audit
- execution-capacity and optimization audits
- accumulated historical/phase audits through CBOT-P8
- CBOT-P4D/P4E/P5/P6/P7/P7R/P8 regression audits
- CBOT-P9 dedicated audit
- Runtime Acceptance Contracts
- cTrader compile/build, including CFIP.Contracts, CFIP.cBot and Indicator/contract test projects

## Manual terminal validation

Still manual because a cTrader terminal is not available in this execution environment:
- panel alert rail location/appearance and message refresh;
- BUY/SELL line and label visual symmetry on live chart;
- arrow/line expiry and stale-object cleanup;
- cBot attach/start/stop/rename/reconnect truth in the actual cTrader chart;
- live execution/protection behavior against the target broker terminal.

## Safety

No broker mutation authority was returned to the Indicator. Multi-position/multi-pending capacity remains intentionally blocked until CBOT-6M adds per-ScenarioId lifecycle, reconciliation, idempotency, risk and protection ownership.

## Next phase

CBOT-6M — concurrent multi-scenario execution.

## Operator

After merging to main, run:
git pull --ff-only
