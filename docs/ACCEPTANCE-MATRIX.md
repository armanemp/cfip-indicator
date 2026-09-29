# CFIP Indicator — Acceptance Matrix

| Area | Source parity | Static | Compile | Scenario | cTrader |
|---|---:|---:|---:|---:|---:|
| Parameters | PASS | PASS | PASS | — | Required |
| Native indicators | PASS | PASS | PASS | Required | Required |
| MTF | PASS | PASS | PASS | Required | Required |
| Structure / FVG / OB / liquidity | PASS | PASS | PASS | Required | Required |
| Decision / confluence | PASS | PASS | PASS | Required | Required |
| Entry / trigger | PASS | PASS | PASS | Required | Required |
| Structural SL / TP | PASS | PASS | PASS | Required | Required |
| Market execution | PASS | PASS | PASS | Required | Required |
| Pending orders | PASS | PASS | PASS | Required | Required |
| Lifecycle / reconciliation | PASS | PASS | PASS | Required | Required |
| Live management | PASS | PASS | PASS | Required | Required |
| Risk / suitability | PASS | PASS | PASS | Required | Required |
| Prediction / telemetry | PASS | PASS | PASS | Required | Required |
| Alerts / popup / chart / panel | PASS | PASS | PASS | Required | Required |

Static/source parity is not a substitute for live cTrader scenario acceptance.

## Phase 9 — Automated controlled acceptance

The repository now executes a dedicated runtime contract suite covering the state and invariant portions of the Phase 9 scenarios:

| Scenario | Automated controlled check | Live cTrader |
|---|---|---:|
| Closed-bar MTF context | PASS | Required |
| Market execution confirmation/rejection | PASS | Required |
| Pending-order confirmation/rejection | PASS | Required |
| Fill/slippage envelope symmetry | PASS | Required |
| Initial SL/TP protection | PASS | Required |
| Managed break-even/profit lock | PASS | Required |
| Target progression | PASS | Required |
| Restart/recovery lifecycle transitions | PASS | Required |
| Reversal/invalidation/exit lifecycle | PASS | Required |
| End-of-day exit lifecycle | PASS | Required |
| Duplicate lifecycle events | PASS | Required |

The automated suite validates deterministic state/invariant behavior only. It does not emulate the live cTrader terminal, broker server, chart rendering engine, event timing, reconnection behavior, or memory profile.

CI executes the automated runtime acceptance contract suite on every push to `main`.

## Current repository gate snapshot

For the baseline verification commit `98ad11fa709c8663c5cb4dc75e77312706bcba8e`:

| Gate | Result | Evidence |
|---|---:|---|
| cTrader compile | PASS | workflow run 719 |
| Runtime acceptance contracts | PASS | workflow run 535 |
| Source / architecture checks | FAIL | workflow run 726; verifier failed only on the missing Track 19 continuity link |

The source/architecture failure is therefore a documentation-continuity defect identified by the machine verifier, not evidence of a runtime failure. The phase remains incomplete until the verifier passes after the documentation fix.

## Phase 1.3 — Runtime fault state machine

| Contract | Automated controlled check | Live cTrader |
|---|---:|---:|
| Recoverable fault enters explicit runtime fault state | Required | Required |
| Entry remains blocked after recoverable fault | Required | Required |
| Management/protection continue during entry block | Required | Required |
| Entry remains disarmed through RECOVERING → HEALTHY | Required | Required |
| Explicit AutoTradingEnabled re-arm transition | Required | Required |

The automated state-machine contract proves deterministic transitions and the no-blind-re-arm invariant. It does not emulate live terminal event timing or broker-side behavior.


## Phase 1.4 — Closed-bar retry and signal/execution synchronization

| Contract | Automated controlled check | Live cTrader |
|---|---:|---:|
| Repeated closed-bar failures are bounded | PASS | Required |
| Closed-bar retry uses timestamp/backoff/circuit | PASS | Required |
| Recoverable analysis fault does not starve management | PASS | Required |
| Pre-trade SL uses authoritative plan value | PASS | Required |
| Live SL/TP reporting uses broker-confirmed values | PASS | Required |
| Pending order reporting uses broker-confirmed values | PASS | Required |
| Missing pending decision/reaction dependency is handled explicitly | PASS | Required |

The repository contracts cover deterministic retry state, stage continuation and source synchronization. Target-terminal tests remain required for live event timing, broker normalization and reconnect behavior.

## Phase 1.5 — Performance, startup and safety supervisor

| Contract | Automated controlled check | Status |
|---|---:|---:|
| Asynchronous bounded startup | Yes | PASS |
| Timer safety supervisor without full analysis | Yes | PASS |
| Closed MTF context reuse | Yes | PASS |
| Closed M1 frame reuse | Yes | PASS |
| M5 regime core reuse | Yes | PASS |
| FVG/OB scan and mitigation bounded by zone age | Yes | PASS |
| Single ATR reuse in active protection | Yes | PASS |
| ZIP audit decisions recorded | Documentation | PASS |
## Phase 5.4 — Canonical signal visual state

| Contract | Automated controlled check | Status |
|---|---:|---:|
| One SignalVisualSnapshot owner | Yes | PASS |
| Single direction resolver | Yes | PASS |
| Plan arrow/levels consume snapshot | Yes | PASS |
| Pending entry/SL/TP consume snapshot | Yes | PASS |
| Panel direction/stage consumes snapshot | Yes | PASS |
| Signal renderer has no direct decision/reaction reads | Yes | PASS |
## Phase 2.1 — Unified execution submission retry

| Contract | Automated controlled check | Status |
|---|---:|---:|
| One submission gate owner | Yes | PASS |
| Signal / attempt / path identity | Yes | PASS |
| Per-identity backoff | Yes | PASS |
| Circuit isolation | Yes | PASS |
| Successful reset | Yes | PASS |
| Pending/market/aggressive paths share one policy | Yes | PASS |