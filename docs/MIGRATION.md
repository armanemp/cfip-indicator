# CFIP Indicator — v89 Migration Plan

## Reference
Source: CFIP-PRO integrations/ctrader/calude-edit-v89.cs

The v89 file is preserved and treated as a behavioral/reference source. It is not copied as the final architecture.

## Migration method
For each subsystem:
1. inventory types, methods, parameters and side effects;
2. assign an authoritative destination;
3. create the domain contract;
4. implement the pure/domain behavior;
5. implement the cTrader adapter only where platform access is required;
6. migrate callers;
7. add parity and invariant tests;
8. compare against v89 behavior;
9. remove the duplicate path;
10. document the migration.

## Initial mapping

v89 canonical models -> Core/Domain
Runtime and MTF snapshots -> Market
Market frame/structure/zones/liquidity -> Analysis
Decision snapshot -> Decision
Entry model -> Planning
TradePlan/targets -> Planning + Risk
Execution intent/policy -> Execution
Broker gateway -> Infrastructure/CTrader
Pending/position lifecycle -> Lifecycle
Live position manager -> LiveManagement
Outcome/telemetry -> Outcomes
Chart/panel/rendering -> Presentation

## Safety rule
No functionality is considered migrated merely because its code compiles. It must have an owner, contract, caller migration, parity coverage and acceptance evidence.

## Final migration gate
The monolithic reference must no longer be required by the production implementation. The final solution must build independently, pass architecture/dependency checks and pass controlled cTrader runtime acceptance.
