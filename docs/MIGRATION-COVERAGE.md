# v89 Migration Coverage

Reference source: CFIP-PRO `integrations/ctrader/calude-edit-v89.cs`

## Migrated top-level responsibilities

- Direction / policy / execution / lifecycle enums
- Provenance and price/zone primitives
- Runtime and MTF snapshot builders
- Market model and native indicators
- Structure events, FVG/OB zones and liquidity
- Decision aggregation and confidence/evidence policy
- Entry/Trigger engine
- Trade plan and target ladder
- Risk envelope
- Execution intent/result/readiness/policy/planner
- Broker snapshots, gateway and state reader
- Pending-order lifecycle
- Position lifecycle and reconciliation
- Live position management
- Outcome state
- Presentation state
- Configuration state
- cTrader Indicator host with parameter surface and runtime/event orchestration

## Host split

The former v89 Indicator class is now represented as partial files:
- Declarations / parameters
- Runtime Calculate/Initialize
- Execution
- Metrics / daily loss
- Broker event handlers
- Lifecycle / reconciliation
- Identity

## Not yet considered complete

Physical extraction is not the same as final clean architecture. The next gates must:
1. introduce explicit interfaces between modules;
2. enforce dependency direction;
3. remove cTrader API dependencies from domain services;
4. add architecture/unit tests;
5. prove parameter parity;
6. compile the complete migrated project in the target cTrader environment;
7. run controlled trading scenarios before release.
