# CFIP Indicator — Acceptance Matrix

A phase is accepted only when all applicable gates are satisfied.

| Gate | Static/source | Unit | Scenario/replay | Real cTrader | Broker/runtime |
|---|---|---|---|---|---|
| Repository structure | Required | — | — | — | — |
| Declaration/parameter parity | Required | Required | — | — | — |
| Core invariants | Required | Required | Required | — | — |
| MTF closed-bar integrity | Required | Required | Required | Optional | — |
| Decision authority | Required | Required | Required | — | — |
| Entry/Trigger consistency | Required | Required | Required | Required | — |
| TradePlan / SL / TP | Required | Required | Required | Required | Required |
| Execution policy | Required | Required | Required | Required | Required |
| Pending lifecycle | Required | Required | Required | Required | Required |
| Position lifecycle | Required | Required | Required | Required | Required |
| Live management | Required | Required | Required | Required | Required |
| Outcomes/calibration | Required | Required | Required | Optional | Optional |
| Presentation consistency | Required | Required | Required | Required | Optional |
| Performance/resource usage | Required | Optional | Required | Required | Required |
| Release artifact | Required | Required | Required | Required | Required |

## Evidence

Record commit SHA, changed files, test results, runtime environment, broker/symbol/timeframe used, and known limitations.

A source-level pass is never promoted to runtime acceptance. Broker submission acceptance is never treated as fill or closure confirmation.

## Release blockers

- missing protection
- direction asymmetry
- duplicate execution
- orphan position/order
- unconfirmed close or partial close
- invalid live SL/TP mutation
- forming-bar leakage
- unowned pending fill
- UI/execution divergence
- silent fallback
- undocumented parameter behavior
