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