# Realtime Multi-Scenario Opportunity Engine — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

## Goal

Make the Indicator continuously evaluate the current quote while preserving closed-bar structural analysis, historical calibration and forward-looking pending opportunities.

Target chain:
Pre-analysis/history -> M15 decision -> M5 trigger/tuning -> M1 optional confirmation -> live quote actionability -> current market scenario OR future pending scenario -> SignalScenarioBatch -> cBot same-tick consumption -> broker execution/placement -> broker confirmation -> protection -> outcome/history.

## Implemented

- Structural parallel opportunity geometry remains cached to the current closed M5 instead of being rebuilt on every tick.
- A separate live opportunity-state refresh runs intrabar on a bounded 100 ms cadence, or immediately when the M5 changes.
- Live market candidates re-evaluate actionability from the current quote without rebuilding their structural Entry/SL/TP geometry.
- Future Continuation Stop and Reversal Limit candidates can be armed before the current trigger/entry is reached.
- Future pending discovery reuses the existing analytical pending preparation, structural SL/TP and RR validation rather than introducing a second geometry owner.
- Future pending discovery is isolated from the canonical current-market provider intent, so future-order discovery cannot replace the current market signal.
- Scenario batch publication accepts current actionable market scenarios and valid future pending scenarios together.
- The cBot processes every ScenarioId in the batch on incoming ticks with bounded concurrent capacity.
- Existing per-ScenarioId idempotency, broker reconciliation and protection ownership remain unchanged.

## Past / present / future intelligence

Historical context remains supplied by the existing empirical calibration path, which uses recent bounded outcomes and persistent archive aggregates before adjusting contextual confidence.

Current state is supplied by the live quote path and intrabar actionability refresh.

Forward view remains supplied by the existing EarlyPrediction layer plus predictive pending level selection and structural target analysis. No future candle or look-ahead data is introduced.

## Timeframe contract

- M15 = canonical trade-decision / execution reference.
- M5 = trigger, tuning and entry precision.
- M1 = optional confirmation.
- H1+ = context/reward support.
- Chart host timeframe = presentation only.

## Execution contract

- Current Market/Aggressive opportunities are executed by the cBot when the current live preflight passes.
- Future Stop/Limit opportunities are placed by the cBot as broker-managed pending orders and wait for their future trigger.
- Independent ScenarioIds may coexist up to the cBot Max Concurrent Scenarios bound.
- Same ScenarioId remains idempotent.
- Live accounts are supported by the same broker mutation owners, but EnableLiveExecution remains an explicit arm and is OFF by default; when unarmed, broker mutation is fail-closed.

## Performance

The design avoids full structural scenario regeneration on every tick. Only bounded intrabar candidate-state evaluation and future-level validation run between closed M5 structural rebuilds.

## Quality and safety

No public confidence, smart-quality, RR, risk, evidence or MTF threshold was lowered to increase frequency. The phase improves timeliness and separates present execution from future order planning; it does not claim profitability or prediction accuracy.

Legacy Indicator MaximumOpenPositions remains the certified single-plan analytical capacity contract. Multi-scenario broker concurrency is controlled independently by the cBot Max Concurrent Scenarios safety bound.

## Verification

Required:
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- dedicated realtime multi-scenario audit;
- target-terminal validation of current market reaction, same-tick cBot handoff, multiple independent ScenarioIds, future pending placement, invalidation, idempotency, restart/reconnect and panel/alert latency.

Operator action after verified merge: git pull --ff-only.
## 2026-10-03 live/attachment/audio hardening follow-up

- Corrected cBot presence identity to publish the real cBot InstanceId instead of a synthesized type/symbol token.
- The Indicator now treats a fresh cBot heartbeat as proof of exact attachment when the cBot explicitly reports the current Indicator InstanceId, even when direct chart-robot enumeration is temporarily unavailable.
- Ambiguous matching cBot instances can be resolved by the exact published cBot InstanceId.
- Critical queued alerts are no longer allowed to evict an already-buffered critical alert under queue pressure.
- Broker trade comments now distinguish CFIP LIVE from CFIP DEMO, removing misleading demo labels on live orders.
- The accumulated live hardening audit now covers these identity and queue invariants.

Repository CI remains the verification authority for source/runtime/compile gates. Target-terminal validation is still required for actual cTrader chart attachment, live arm, broker fill/placement, audio playback, restart/reconnect and observed realtime latency.

