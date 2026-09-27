# CFIP Indicator — Architecture

## Authority chain
Market Input -> RuntimeSnapshot -> MTF -> MarketModel -> DecisionSnapshot -> EntrySnapshot -> TradePlan -> Risk -> ExecutionPolicy -> ExecutionIntent -> BrokerGateway -> BrokerState -> Lifecycle -> LiveManagement -> Outcome -> PresentationState

No stage may bypass the next authoritative boundary.

## Dependency rule
Dependencies point inward toward domain contracts. Core must not reference cAlgo.API, chart drawing, broker collections or UI controls.

CTrader infrastructure translates platform objects into domain snapshots and translates domain intents into broker mutations.

## Ownership
Core: pure domain types, enums, contracts, value objects and invariants.

Market: runtime snapshot, time, timeframe and MTF data.

Analysis: trend, momentum, structure, FVG, OB, liquidity and confluence. Side-effect free.

Decision: direction, confidence, evidence, regime, eligibility and block reasons.

Planning: entry, trigger, invalidation, TradePlan and target ladder.

Risk: risk budget, sizing, exposure, leverage, SL/TP validity and broker constraints.

Execution: execution policy, intent, idempotency and eligibility. No direct platform API.

Infrastructure/CTrader: platform adapters, broker gateway and event translation.

Lifecycle: pending/position state, reconciliation, confirmation, retry and recovery.

LiveManagement: protection maintenance, BE, structural repricing, partial close, reversal, exhaustion, invalidation and EOD actions.

Outcomes: realized outcomes, telemetry, calibration and drift. No broker authority.

Presentation: chart, terminal, alerts and localization. Rendering only.

## Key invariants
- BUY=+1, SELL=-1, WAIT=0.
- Analysis is side-effect free.
- Decision != execution.
- TradePlan != broker state.
- Broker is authoritative after mutation.
- Lifecycle follows broker reality.
- Execution is idempotent.
- Pending fills cannot become unmanaged.
- Partial TP requires broker confirmation.
- SL moves only protectively.
- Target ladder has one authority.
- No manual entry/order controls.
- Safety controls are separate from entry controls.
- Closed-bar data is required for decisions.
- Fallbacks carry provenance.
- All parameters have disposition.
- BUY/SELL logic is symmetric.
- Presentation is downstream.

## Entry terminology
IdealEntry = preferred structural price.
EntryZone = allowed structural retest region.
Trigger = activation threshold.
RequestedEntry = exact broker-requested price.
ActualFill = broker-confirmed price.
Invalidation = structural/risk boundary.

These concepts must remain separate in code and presentation.

## State separation
Strategy state and broker state are separate.

Strategy: WAITING, SIGNAL, PLAN_READY, EXECUTION_READY.
Broker lifecycle: FLAT, PENDING, LIVE, EXIT_REQUESTED, RECOVERY, CLOSED.

A strategy transition never implies a broker transition.

## Error taxonomy
Analysis: DATA_INCOMPLETE, MODEL_INVALID.
Decision: DECISION_BLOCKED, DECISION_UNCERTAIN.
Execution: EXECUTION_INVALID, EXECUTION_REJECTED, BROKER_CONSTRAINT.
Lifecycle: STATE_CONFLICT, ORPHAN_POSITION, ORPHAN_ORDER, RECOVERY_REQUIRED.
Presentation: PRESENTATION_DEGRADED.

Presentation errors never alter trading authority.
