# CBOT-P9 — Unified Alert Rail / Visual Coherence / cBot Signal Preflight — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

## Scope

This phase completes the requested presentation cutover and hardens the Indicator → cBot execution boundary without introducing a second execution engine.

## Implemented

### Alert and message presentation

- Removed the production Popup renderer, Popup remover, Popup expiry cleaner and the old Popup-scoped delivery processor.
- Removed the obsolete Popup parameter surface from the Indicator.
- All eligible canonical alerts now enter the existing bounded `AlertDeliveryQueue(16)`.
- `UI/Panel/AlertDeliveryProcessor.cs` is the single delivery boundary:
  - updates the panel alert rail first;
  - then emits the optional sound cue;
  - preserves the existing semantic sound resolution and failure handling.
- Added `PanelAlertMessageRenderer` with a bounded five-message history.
- Alert message colors are semantic:
  - BUY direction → BUY semantic color;
  - SELL direction → SELL semantic color;
  - critical/protection/restriction/invalidation/reversal → warning semantic color;
  - neutral messages → panel secondary text.
- Messages are presented in the panel footer beside the Hide/Show control, without a popup layer.
- New alert revisions invalidate the panel presentation key so new messages appear without a permanent timer-driven full-render loop.

### Chart visual coherence

- Preserved the canonical compact level geometry:
  - 40 chart bars;
  - latest chart candle as the right edge;
  - `LineStyle.Solid`;
  - canonical thickness resolver;
  - background-free level labels;
  - label text uses the same semantic color as the corresponding line.
- BUY and SELL therefore share one renderer/geometry contract instead of direction-specific visual branches.
- Popup lifecycle hooks were removed from the calculation/runtime paths.

### cBot execution boundary

- Added `CbotSignalPreflight` as one cBot-owned pre-mutation validation owner for:
  - envelope presence and identity;
  - future observed-time rejection;
  - provider staleness;
  - symbol scope.
- `CFIPExecutionBot` now routes the canonical `SignalEnvelope` through this preflight before execution branching.
- Existing Indicator → Device LocalStorage → exact InstanceId → cBot transport remains unchanged.
- Existing cBot environment gate, broker-capacity guard, margin safety, execution coordinators and broker-confirmed lifecycle remain authoritative.
- The single-plan broker safety gate remains intentionally unchanged until the dedicated CBOT-6M multi-scenario execution phase is completed.

## Full-chain audit

Every phase continues to audit:

`Pre-analysis → M15 decision → M5 trigger/tuning/entry precision → M1 optional confirmation → entry/SL/TP geometry → signal → alert/message → SignalEnvelope → cBot binding/transport → cBot preflight → execution safety → broker mutation → broker confirmation → protection/lifecycle → outcome/history`.

Timeframe contract remains:

`M15 = canonical trade-decision/execution reference; M5 = trigger/tuning/entry precision; M1 = optional confirmation; H1+ = context/reward; chart timeframe = presentation only.`

## Verification gates

Pending on this phase branch:

- Source / Architecture CI;
- parameter inventory and count;
- runtime UI/static audits;
- CBOT-P9 dedicated audit;
- cTrader compile/build;
- target-terminal visual acceptance for message placement, BUY/SELL symmetry, arrow/line lifetime and cBot attachment/runtime state.

No claim of runtime-terminal completion is made until those gates pass.

## Next phase

After P9 verification, continue with **CBOT-6M concurrent multi-scenario execution**. That phase must add per-`ScenarioId` execution/reconciliation/idempotency/risk/protection ownership before the existing single-plan broker gate can be removed.

## Operator

After this phase is merged and verified, run:

`git pull --ff-only`

on the local `main` checkout.
