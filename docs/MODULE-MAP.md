# CFIP Indicator — Module Map

The migration uses domain-oriented modules plus a partial cTrader host. The physical split is intentional; dependency enforcement is the next hardening step.

| Module | Responsibility |
|---|---|
| Core | canonical scalars, value objects, targets, entry primitives |
| Market | runtime/time/MTF snapshots and market model |
| Analysis/Structure | structure ledger, FVG, Order Blocks, liquidity, premium/discount |
| Decision | authoritative DecisionSnapshot and decision engine |
| Planning | Entry/Trigger and TradePlan |
| Risk | sizing, protection and execution envelope |
| Execution | policy, intent, planner, result and idempotency |
| Infrastructure/CTrader | platform/broker adapter and broker state reader |
| Lifecycle | pending/position state, reconciliation, retry and recovery |
| LiveManagement | live protection, BE, target progression, partial TP and exits |
| Outcomes | outcome/telemetry/calibration boundary |
| Presentation | chart/panel state |
| Configuration | configuration snapshots and state |
| Indicator | host orchestration split by runtime/execution/lifecycle/events |

### Authority chain

Runtime → MTF → Market Model → Decision → Entry → TradePlan/Risk → Execution → Broker Gateway → Broker State → Lifecycle → Live Management → Outcomes → Presentation.
