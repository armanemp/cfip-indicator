# CFIP Indicator — Architecture

## Authority chain

Market input
-> Runtime snapshot
-> MTF snapshot
-> Market model
-> Decision
-> Entry/Trigger
-> Trade Plan
-> Risk
-> Execution Policy
-> Execution Intent
-> Broker Gateway
-> Broker State
-> Lifecycle
-> Live Management
-> Outcome
-> Presentation

## Boundaries

Core: pure domain types and invariants.

Market: runtime, time, MTF and market-frame construction.

Analysis: structure, FVG, Order Block, liquidity and confluence. No broker mutation.

Decision: direction, evidence, confidence, regime and decision eligibility.

Planning: ideal entry, entry zone, trigger, invalidation, identity and trade plan.

Risk: risk budget, sizing, exposure, protection and target validation.

Execution: unified execution policy, intent, eligibility and idempotency.

Infrastructure: cTrader/platform translation and broker mutation gateway.

Lifecycle: pending/position state, reconciliation, confirmation, retry and recovery.

Live Management: protection maintenance, break-even, structural repricing, dynamic targets, partial exits and exit precedence.

Outcomes: realized outcomes, telemetry and calibration. No execution authority.

Presentation: chart, panel, alerts and prediction rendering only.

## Invariants

BUY=+1, SELL=-1, WAIT=0.
Decision is not execution.
Trade plan is not broker state.
Broker state is authoritative after mutation.
Lifecycle follows broker reality.
Partial close requires broker volume confirmation.
Close requires broker confirmation.
SL only moves protectively.
Presentation never creates trading authority.
