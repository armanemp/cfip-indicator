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

## Phase 2.1 — Unified submission retry

| Contract | Automated controlled check | Status |
|---|---:|---:|
| One submission gate owner | Yes | PASS |
| Signal/attempt/path identity | Yes | PASS |
| Per-identity backoff/circuit | Yes | PASS |
| Failure isolation across signals | Yes | PASS |
| Failure isolation across execution paths | Yes | PASS |
| Successful retry-state reset | Yes | PASS |

## Phase 6.1 — Decision closed-bar contract

| Contract | Automated controlled check | Status |
|---|---:|---:|
| Canonical closed-bar index resolution uses actual next-bar open time | Yes | PASS |
| Exact bar-open boundary selects only the preceding fully closed bar | Yes | PASS |
| Between-boundary and gap semantics are deterministic | Yes | PASS |
| Future bar cannot be accepted as closed | Yes | PASS |
| Decision request carries one canonical MTF closed context | Yes | PASS |
| Required decision frames match canonical closed indices | Yes | PASS |
| Present optional M1/D1/W1 frames match canonical closed indices | Yes | PASS |
| Timeframe agreement reuses canonical closed indices | Yes | PASS |
| Source / architecture gates | Yes | PASS |
| Runtime acceptance contracts | Yes | PASS |
| cTrader compile | Yes | PASS |

The phase changes temporal and ownership guarantees only; decision thresholds, weights, RR, risk, trailing and broker execution semantics are intentionally unchanged.


## Phase 6.2 — Reaction intrabar

| Contract | Automated controlled check | Status |
|---|---:|---:|
| Live reaction evaluates current open M5 bar | Source gate | PASS |
| Confirmed decision remains closed-bar | Existing Phase 6.1 contract + source gate | PASS |
| Visual snapshot carries explicit intrabar reaction identity | Source gate | PASS |
| Reaction arrow uses explicit live reaction M5 index | Source gate | PASS |
| Unified panel remains the operator-facing presentation surface | Existing panel/runtime contract | PASS |
| Source / architecture gates | Yes | PASS |
| Runtime acceptance contracts | Yes | PASS |
| cTrader compile | Yes | PASS |


## Post-Phase 6.2 startup/runtime correction — 2026-09-29

| Contract | Automated controlled check | Status |
|---|---:|---:|
| Initialization may finalize as soon as required M5/M15/M30/H1/H4 data is sufficient | Source gate | PASS |
| Optional M1/D1/W1 loading cannot block runtime readiness once core data is sufficient | Source gate | PASS |
| Startup requests one lightweight calculation seed after readiness | Source gate | PASS |
| Startup seed does not own market/pending execution or full live-cycle management | Source gate | PASS |
| Normal Calculate remains the sole recurring live calculation owner | Source/runtime contract | PASS |
| 535-parameter production contract remains unchanged | Source gate | PASS |


## Phase 6.3 — Controlled intrabar aggressive entry

| Contract | Automated controlled check | Status |
|---|---:|---:|
| Aggressive trigger source is the explicit current-open-M5 reaction | Source gate | PASS |
| Two distinct qualifying reaction observations are required to arm | Runtime contract | PASS |
| Duplicate reaction observation timestamps cannot double-count qualification | Runtime contract | PASS |
| Direction change invalidates prior intrabar qualification | Runtime contract | PASS |
| Loss of reaction qualification invalidates immediately | Runtime contract | PASS |
| New M5 bar starts a fresh qualification window | Runtime contract | PASS |
| Confirmed aggressive fill consumes the qualification latch | Source gate | PASS |
| Structural SL/TP and existing execution authority remain unchanged | Source gate | PASS |
| Broker confirmation remains authoritative for aggressive market entry | Source gate | PASS |
| 535-parameter contract remains unchanged | Source gate | PASS |
| Source / architecture gates | Yes | PASS |
| Runtime acceptance contracts | Yes | PASS |
| cTrader compile | Yes | PASS |

The phase chooses controlled intrabar entry explicitly. The closed-bar decision remains the structural context; the live reaction is the trigger source. No new public parameter is introduced.


## Startup responsiveness correction — 2026-09-29

| Contract | Automated controlled check | Status |
|---|---:|---:|
| Initial panel render is completed before startup calculation seed execution | Source gate | PASS |
| Startup calculation seed is queued asynchronously on the indicator main thread | Source gate | PASS |
| Synchronous full calculation is not executed inside async initialization finalization | Source gate | PASS |
| Startup seed remains one-shot and cannot recur | Source/runtime contract | PASS |
| Normal Calculate remains the recurring live calculation owner | Source/runtime contract | PASS |
| cTrader main-thread dispatch uses the documented BeginInvokeOnMainThread API | cTrader API contract | PASS |


## Phase 6.4 — Compact 40-Bar Plan-Level Visuals — 2026-09-29

| Contract | Automated controlled check | Status |
|---|---:|---:|
| Plan levels use a fixed 40-bar compact span | Source gate | PASS |
| Plan levels terminate at the latest chart candle | Source gate | PASS |
| Full-width visible-chart boundaries are not used for plan levels | Source gate | PASS |
| Trigger / SL / TP styles are visually differentiated | Source gate | PASS |
| Name + price tag is bound to the left end of the compact level | Source gate | PASS |
| Existing ChartText objects are reused during refresh | Source gate | PASS |
| Compact label boxes are cleaned with their text objects | Source gate | PASS |
| No public parameter is added | Source / architecture | PASS |
| Runtime acceptance contracts | Yes | PASS |
| cTrader Compile | Yes | PASS |

Live cTrader acceptance remains required for actual chart appearance, zoom/layout
behavior, absence of flicker, startup latency and device-specific performance.
