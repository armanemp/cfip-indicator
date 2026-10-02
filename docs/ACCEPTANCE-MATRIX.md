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
| Alerts / panel alert rail / chart / panel | PASS | PASS | PASS | Required | Required |

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


## Runtime UI hotfix — Instant panel Hide/Show — 2026-09-29

| Contract | Automated controlled check | Status |
|---|---:|---:|
| Hide/Show mutation does not synchronously invoke full RenderPanel() | Source gate / review | PASS |
| Hidden panel visibility changes without rebuilding rows | Source gate | PASS |
| Normal heartbeat remains responsible for panel refresh | Existing runtime contract | PASS |
| No trading/decision state mutation is introduced | Source review | PASS |
| 535-parameter contract remains unchanged | Source gate | PASS |
| cTrader Compile | Required | Pending |
| Hands-on Hide/Show responsiveness | Required | Pending |

## Phase 7.1 — Hidden-clamp audit

| Contract | Automated controlled check | cTrader |
|---|---:|---:|
| Live Trigger Score user range is not hidden behind a hard floor of 4 | PASS | Required |
| Precision Trigger Score contributes through the effective user-facing threshold | PASS | Required |
| Target Update Step ATR declared 0.02 minimum is behaviorally reachable | PASS | Required |
| Remaining inspected bounds are either parameter-aligned or internal safety bounds | PASS | Required |
| No new public parameter introduced | PASS | Required |
| Source / Architecture | PASS | Required |
| Runtime Acceptance Contracts | PASS | Required |
| cTrader Compile | PASS | Required |

The phase restores parameter semantics only. It does not claim that signal quality,
Entry/SL/TP intelligence or live trailing has been fully improved; those remain
separate implementation owners.
## Runtime UI and smart protection hotfix — 2026-09-29

| Contract | Automated controlled check | cTrader |
|---|---:|---:|
| AUTO TRADE quick control uses a direct operator click action | PASS | Required |
| AUTO ORDERS quick control uses a direct operator click action | PASS | Required |
| Programmatic toggle synchronization cannot execute operator actions | PASS | Required |
| Setup preview renders Entry/Trigger/SL/TP compact labels | PASS | Required |
| Compact level label is background-free and text exactly matches the semantic line color | PASS | Required |
| Structural trailing cannot chase raw market price through final distance clamping | PASS | Required |
| Structural trailing progression is closed-M5 gated | PASS | Required |
| No new public parameter introduced | PASS | Required |
| Source / Architecture | Required | Required |
| Runtime Acceptance Contracts | Required | Required |
| cTrader Compile | Required | Required |

Hands-on cTrader validation remains required for actual toggle behavior, visible labels and observed live trailing stability.


## Corrective hotfix — execution priority / toggle state — 2026-09-29

| Requirement | Source contract | Runtime contract | cTrader compile |
| --- | --- | --- | --- |
| AUTO TRADE uses Checked/Unchecked operator events | Required | Required | Required |
| AUTO ORDERS uses Checked/Unchecked operator events | Required | Required | Required |
| Programmatic control synchronization cannot act as operator input | Required | Required | Required |
| Predictive pending execution precedes market-plan creation | Required | Required | Required |
| Aggressive AUTO TRADE gets priority before normal market-plan creation | Required | Required | Required |
| Market plan creation defers while a managed pending order exists | Required | Required | Required |
| Structural trailing is not a raw market-price chase | Required | Required | Required |

Manual cTrader validation remains required for actual button interaction, observed order submission, visible label/box appearance and live protection behavior.


## Corrective Hotfix — Chart Lines and Execution Status UI — 2026-09-29

| Contract | Automated controlled check | cTrader |
|---|---:|---:|
| Plan lines use a canonical 40-bar compact chart span | PASS | Required |
| Plan lines terminate at the latest chart candle | PASS | Required |
| Plan-line geometry is independent of M5 event-time mapping | PASS | Required |
| Pending level rendering has no disposable M5 anchor dependency | PASS | Required |
| Label placement reuses the canonical line left edge | PASS | Required |
| AUTO TRADE is a non-interactive switch-style status indicator | PASS | Required |
| AUTO ORDERS is a non-interactive switch-style status indicator | PASS | Required |
| Execution status is synchronized from EnableAutoTrading / EnableAutomaticOrders | PASS | Required |
| UI cannot mutate execution runtime authority | PASS | Required |
| Legacy interactive execution-control symbols are absent from source | PASS | Required |
| Source / Architecture | Required | Required |
| Runtime Acceptance Contracts | Required | Required |
| cTrader Compile | Required | Required |

Hands-on cTrader validation remains required for exact visual line endpoints, 40-bar appearance on target chart timeframes, status-switch rendering, settings synchronization and non-interactivity.


## Phase 8.1 — M1 trigger correctness

| Contract | Automated controlled check | cTrader / replay |
|---|---:|---:|
| M1 is consumed from the canonical closed M1 context | PASS | Required |
| M1 bar is inside the exact closed M5 window | PASS | Required |
| Future / still-open M1 bar is rejected | PASS | Required |
| BUY/SELL M1 trigger symmetry | PASS | Required |
| Opposite M1 direction is rejected | PASS | Required |
| Weak M1 body is rejected | PASS | Required |
| Poor M1 close location is rejected | PASS | Required |
| Abnormally large M1 range is rejected | PASS | Required |
| Insufficient M1 trigger score is rejected | PASS | Required |
| M1 cannot vote directly in directional consensus | PASS | Required |
| M1 confirmation is combined with canonical M5 TriggerReady | PASS | Required |
| Runtime Acceptance Contracts | PASS | Required |
| cTrader Compile | PASS | Required |
| Source / Architecture | PASS | Required |

CI evidence on branch head `929700e154d4b84a5a0b9efeae345b92017834b6`: Runtime Acceptance run 801 PASS; cTrader Compile run 985 PASS; Source / Architecture run 992 PASS.

The automated phase boundary is closed. Hands-on cTrader replay/live validation remains required to measure actual false-signal reduction and terminal behavior; CI does not establish an empirical win-rate or false-signal improvement.


## Phase 8.2 — Swing Plateau Correctness and Structural Evidence Identity

| Contract | Automated controlled check | cTrader / replay |
|---|---:|---:|
| Deterministic symmetric plateau high/low canonicalization | PASS | Required |
| Closed-bar-only pivot confirmation; no future bars | PASS | Required |
| One canonical swing identity per plateau | PASS | Required |
| Equal-level cluster uses fixed anchor; no tolerance chaining | PASS | Required |
| Liquidity sweep references an established prior level | PASS | Required |
| One causal break cannot be counted independently as BOS/MSS/CHOCH multiple times | PASS | Required |
| Runtime Acceptance Contracts | PASS | Required |
| Source / Architecture | PASS | Required |
| cTrader Compile | PASS | Required |

CI evidence on verified head `5df5931828719fb635ec67fa59d57b519d4e70e7`. No empirical false-signal or win-rate improvement is claimed until target-platform replay/historical evaluation is completed.
## Phase 8.3 — FVG Mathematical Audit

| Contract | Automated controlled check | cTrader / replay |
|---|---:|---:|
| Canonical bullish/bearish 3-bar gap formula | PASS | Required |
| Explicit 2-bar imbalance formula | PASS | Required |
| Minimum gap uses creation-bar ATR | PASS | Required |
| Valid zone geometry and overlap semantics | PASS | Required |
| Symmetric partial mitigation boundary movement | PASS | Required |
| Full-fill invalidation semantics | PASS | Required |
| Stable FVG identity by source/direction/variant | PASS | Required |
| Current-bar retest requires post-creation range interaction/proximity | PASS | Required |
| Predictive pending FVG consumer uses canonical rule | PASS | Required |
| Runtime Acceptance Contracts | PASS | Required |
| Source / Architecture | PASS | Required |
| cTrader Build | PASS | Required |

CI evidence on head `89919e7363d374e2cf3a362ec553b1fdac464919`: Runtime PASS; Build PASS; Source/Architecture PASS. No empirical false-signal or win-rate improvement is claimed until target-platform replay/historical evaluation is completed.
## Phase 8.4 — Order Block Mathematical Audit

| Contract | Automated controlled check | cTrader / replay |
|---|---:|---:|
| Opposite source candle and zone geometry | PASS | Required |
| Body vs wick zone semantics | PASS | Required |
| Displacement uses creation-bar ATR | PASS | Required |
| Structure-break threshold uses creation-bar ATR | PASS | Required |
| Wick/body mitigation probe symmetry | PASS | Required |
| Partial mitigation and retained-width rule | PASS | Required |
| Full-fill invalidation | PASS | Required |
| Stable Order Block identity | PASS | Required |
| OB/FVG confluence consumes canonical FVG rule | PASS | Required |
| Runtime Acceptance Contracts | PASS | Required |
| Source / Architecture | PASS | Required |
| cTrader Build | PASS | Required |

CI evidence on head `41a578ba72fec2219447ddc1ceff12b96ee353e7`: Runtime PASS; Build PASS; Source/Architecture PASS. No empirical false-signal or win-rate improvement is claimed until target-platform replay/historical evaluation is completed.


## Phase 9.3 — Contextual empirical confidence calibration

| Contract | Automated controlled check | cTrader / replay |
| --- | --- | --- |
| Calibration uses direction + lane + regime + confidence bucket context | PASS | Required |
| Exact context requires a minimum sample population | PASS | Required |
| Sparse exact bucket falls back to lane/regime aggregate | PASS | Required |
| Sparse context falls back to calibration-eligible directional history | PASS | Required |
| Small samples are shrunk toward a 50% prior | PASS | Required |
| Adjustment is bounded by the existing calibration cap | PASS | Required |
| Calibration is applied only after opportunity-lane resolution | Source contract | Required |
| Tactical / Counter-HTF opportunity discovery remains independent | Source/runtime contract | Required |
| Closed broker outcomes bind to the original plan context | Source contract | Required |
| Recovery-only plans are excluded from calibration | Source contract | Required |
| Canonical visual snapshot carries calibration diagnostics | Source contract | Required |
| Panel uses observed-win wording rather than probability wording | Source contract | Required |
| No new public parameter added | Source / architecture | Required |
| Decision contracts | PASS | Required |
| cTrader compile | Required | Required |
| Runtime acceptance | Required | Required |
| Source / architecture | Required | Required |


## Phase 9.9 — Signal / Execution / Protection Coherence

| Contract | Automated controlled check | cTrader / replay |
|---|---:|---:|
| Live ActionableNow consumes the current closed-M5 indicator-fusion snapshot | Source | Required |
| Present-but-unavailable M5 indicator fusion fails closed | Decision contract / Source | Required |
| Pending/live execution suppresses lower-priority WATCH/REACTION presentation and alerts | Source / Runtime | Required |
| Stale M5 indicator-fusion state cannot reopen actionability | Decision/runtime contract | Required |
| Compression remains a hard indicator-actionability block | Decision contract | Required |
| Server TP ladder uses relative SL + partial TP1 + partial TP2 + final TP | Source / compile | Required |
| Server TP ladder requires valid progressive geometry and broker-valid normalized volumes | Source / compile | Required |
| Local TP1/TP2 close mutation is suppressed while server ladder is active | Source | Required |
| Live target progression yields while server ladder is active | Source | Required |
| Protection sync does not overwrite a server-owned TP ladder | Source | Required |
| Pending fills adopt the confirmed server TP ladder | Source | Required |
| Level-label renderer uses white text | Source | Required |
| Level-label renderer/coordinator does not create text-background rectangles | Source | Required |
| 535-parameter production contract remains unchanged | Source gate | Required |
| No second decision/execution authority introduced | Source / architecture | Required |
| Decision contracts | PASS | Required |
| Runtime Acceptance | Required | Required |
| cTrader Compile/Build | Required | Required |
| Source / Architecture | Required | Required |

Hands-on target-terminal replay remains required for observed partial fills, target-server behavior, duplicate suppression, terminal rendering, broker protection state and realized trading outcomes.


## Phase 9.10 — Smart Auto-Trade / Auto-Order Protection & Accumulated Audit

| Contract | Automated controlled check | cTrader / replay |
|---|---:|---:|
| Automatic market path consumes smart server protection when eligible | PASS | Required |
| Aggressive market path consumes smart server protection when eligible | PASS | Required |
| Continuation stop / reversal limit consume the same server protection contract | PASS | Required |
| Smart break-even uses structural risk, TP1 geometry and spread-aware inputs | PASS | Required |
| Local break-even yields to confirmed broker-owned break-even | PASS | Required |
| Server-owned TP ladder remains protected from local TP mutation | PASS | Required |
| Structural SL remains monotonic/protective-only | PASS | Required |
| Accumulated auto-trade/protection audit runs in Source/Architecture CI | PASS | Required |
| All signal/plan level lines are Solid | PASS | Required |
| Level text is white and background-free | PASS | Required |
| Public parameter contract remains 552 | PASS | Required |

Final automated verification for Phase 9.10: Runtime Acceptance #1070 PASS; cTrader Compile/Build #1254 PASS; Source/Architecture #1261 PASS, including the accumulated audit.

Target-terminal replay remains required for actual broker/server timing, partial fills, protection activation, live rendering and realized outcomes.


## Phase 9.11 — Signal Lifecycle, Quality Recovery & Alert Execution Coherence

| Contract | Automated controlled check | cTrader / replay |
|---|---:|---:|
| Expired pre-trade Plan visuals stop rendering after bounded lifecycle | PASS | Required |
| Setup-preview visuals stop rendering after bounded lifecycle | PASS | Required |
| Stale pre-trade Plan direction cannot become canonical visual direction | PASS | Required |
| Strong one-dimension location/timing/price-position recovery is deterministic | PASS | Required |
| Multiple deficiencies and hard blockers remain rejected | PASS | Required |
| Legacy ALERT BUY/SELL chart label is absent | PASS | Required |
| Blocked/restricted candidates cause no sound, email or visual-alert side effect | PASS | Required |
| Alert messages appear in the panel footer beside the Hide/Show control with semantic colors | PASS | Required |
| Plan and prediction signal lines are fixed to thickness 1 and Solid | PASS | Required |
| Broker submission confirmation/rejection/null-result telemetry is recorded through one gate | PASS | Required |
| Server SL/TP ladder rejects wrong-side structural stop geometry | PASS | Required |
| Existing server TP/BE ownership and local-mutation yield remain intact | PASS | Required |
| Automatic market/aggressive/pending paths retain shared submission/protection gates | PASS | Required |
| Public parameter count remains unchanged at 552 | PASS | Required |
| Full-project / accumulated audit | PASS | Required |
| Decision Contracts | PASS | Required |
| Runtime Acceptance | PASS | Required |
| cTrader Compile/Build | PASS | Required |
| Source / Architecture | PASS | Required |

Target-terminal replay remains required for exact signal timing, stale-object removal, panel alert-rail rendering, broker submission timing, server protection activation, realized SL/TP and false-signal measurements.



## Phase 9.12 — Broker Outcome / Recovery Telemetry & Recent Lifecycle Calibration

| Contract | Automated controlled check | cTrader / replay |
|---|---:|---:|
| Managed broker closes are recorded once by position id | PASS | Required |
| Outcome history is bounded to 128 observations | PASS | Required |
| Outcome observation retains plan lane/regime/confidence context when calibration-eligible | PASS | Required |
| Realized R is calculated from broker-close Pips and plan risk when finite | PASS | Required |
| Recovery-only reconstructed plans remain non-calibratable | PASS | Required |
| Recent 128 eligible outcomes are preferred when sample gates are satisfied | PASS | Required |
| Recent calibration preserves exact/context/directional fallback order | PASS | Required |
| Older observations are excluded when recent exact context is sufficiently populated | PASS | Required |
| Submission history retains confirmed/rejected/unconfirmed/null-result/failed events | PASS | Required |
| Execution telemetry history is bounded to 64 records | PASS | Required |
| RecoveryRequired and recovery-resolution transitions are observable | PASS | Required |
| Panel exposes recent outcome and broker-trace diagnostics | PASS | Required |
| Shared automatic market/aggressive/pending submission and protection gates remain intact | PASS | Required |
| Public parameter count remains unchanged at 552 | PASS | Required |
| Recent outcome calibration contract | PASS | Required |
| Runtime Acceptance | PASS | Required |
| cTrader Compile/Build | PASS | Required |
| Source / Architecture + accumulated audit | PASS | Required |

Target-terminal replay remains required for broker event ordering, realized R accuracy, lifecycle duration, chart timing, recent calibration behavior and actual signal-quality outcomes.


Verification evidence:
- Runtime Acceptance #1096 PASS
- cTrader Compile/Build #1280 PASS
- Source/Architecture #1287 PASS



## Phase 9.13 — Persistent Outcome Memory, Adaptive Risk & Optimization Routine

| Contract | Automated controlled check | cTrader / replay |
| --- | ---: | ---: |
| Outcome memory persists through cTrader LocalStorage without FullAccess | PASS | Required |
| Memory is scoped by symbol/timeframe/configuration fingerprint | PASS | Required |
| Persisted memory rejects malformed/invalid records | PASS | Required |
| Restored memory is bounded to 128 outcomes | PASS | Required |
| Restored outcomes older than 90 days are ignored | PASS | Required |
| Broker-confirmed close remains the only outcome source | PASS | Required |
| Recent outcome evidence feeds existing contextual calibration | PASS | Required |
| Adaptive outcome risk requires a minimum sample and uses latest-12 window | PASS | Required |
| Adaptive outcome risk can only reduce suitability-derived risk | PASS | Required |
| Adaptive risk cannot fall below the existing 0.25 hard multiplier floor | PASS | Required |
| Disabling outcome telemetry disables outcome-driven risk adaptation | PASS | Required |
| Optimization-readiness audit is part of Source/Architecture CI | PASS | Required |
| Public parameter count remains unchanged at 552 | PASS | Required |
| Shared automatic market/aggressive/pending gates remain intact | PASS | Required |
| Runtime Acceptance | PASS | Required |
| cTrader Compile/Build | PASS | Required |
| Source / Architecture + accumulated audit | PASS | Required |

Verification evidence: Runtime Acceptance #1110 PASS; cTrader Compile/Build #1294 PASS; Source/Architecture #1301 PASS.

Target-terminal replay remains required to measure restart persistence, observed signal timing, false-signal frequency, realized R and the empirical effect of recent calibration/risk scaling.

 
Verification evidence:
- Runtime Acceptance #1107 PASS
- cTrader Compile/Build #1291 PASS
- Source/Architecture #1298 PASS


## Phase 9.14 — Signal Evidence Integrity & Consensus Calibration

| Contract | Automated controlled check | cTrader / replay |
| --- | ---: | ---: |
| Relative frame dominance is modulated by absolute bull+bear strength | Decision Contract / Source | Required |
| Low-evidence frames cannot contribute as full-strength directional votes | Decision Contract / Source | Required |
| Evidence coverage is bounded and is not a hard gate | Decision Contract / Source | Required |
| Strong evidence retains high directional influence | Decision Contract | Required |
| BUY/SELL contribution symmetry is preserved | Decision Contract | Required |
| Weak balanced frames remain near neutral | Decision Contract | Required |
| Canonical adapter supplies the existing frame evidence count | Source | Required |
| No second decision/execution authority is introduced | Source / Architecture | Required |
| Public parameter count remains 552 | Source gate | Required |
| Runtime Acceptance | Required | Required |
| cTrader Compile/Build | Required | Required |
| Source / Architecture + accumulated audit | Required | Required |

Target-terminal/replay remains required for actual false-signal frequency, missed valid opportunities, signal timing, realized R, lane/regime calibration and execution behavior.


Phase 9.14 automated verification evidence:
- Runtime Acceptance: PASS
- cTrader Compile/Build: PASS
- Source/Architecture + accumulated audits: PASS
- Decision Contracts: PASS within Build
- Target-terminal/replay: still required for empirical signal-quality and realized-R measurement.


## Phase 9.15 — Startup Responsiveness & Portable Long-Term History

| Contract | Automated controlled check | cTrader / replay |
| --- | ---: | ---: |
| Primary startup no longer waits for D1/W1 callbacks | Source / startup audit | Required |
| Core startup history thresholds remain sufficient for closed-bar analyzers | Source / startup audit | Required |
| D1/W1 optional callbacks are adopted after core startup | Source / compile | Required |
| MTF context cache is invalidated when optional bars arrive | Source / compile | Required |
| Startup panel reports meaningful loading progress | Source | Required |
| Persistent 90-day outcome archive uses append-only files | Source / startup audit | Required |
| Archive keeps prior 90-day files and does not delete them | Source / startup audit | Required |
| Archive remains scoped by symbol/timeframe/configuration | Source | Required |
| Archive import is deferred out of the critical startup path | Source | Required |
| Long-term archive aggregates participate only in existing calibration authority | Source | Required |
| LocalStorage recent memory remains available | Source | Required |
| AccessRights remains None | Source / architecture | Required |
| No second decision/execution authority | Source / Architecture | Required |
| Public parameter count remains 552 | Source gate | Required |
| Runtime Acceptance | Required | Required |
| cTrader Compile/Build | Required | Required |
| Source / Architecture + accumulated audits | Required | Required |

Target-terminal/replay remains required for actual startup latency, archive creation/rotation, migration/restore, signal timing and end-to-end trading outcomes.


## Phase 9.15 verification evidence

- Runtime Acceptance: PASS
- cTrader Compile/Build: PASS
- Source/Architecture + accumulated audits: PASS
- Startup/persistence audit: PASS
- Decision Contracts: PASS within Build
- Target-terminal validation: Required for observed startup latency, archive restore/rotation and end-to-end trading outcomes.


## Phase 9.16 — Signal Measurement, OB/FVG Location Fusion & Execution Safety

| Contract | Automated controlled check | cTrader / replay |
|---|---:|---:|
| Canonical OB/FVG location scoring has one source of truth | PASS | Required |
| Standalone OB has a higher base location contribution than standalone FVG | PASS | Required |
| High-quality OB+FVG is the strongest single bounded location feature | PASS | Required |
| Correlated FVG/OB confluence counts as one evidence unit | PASS | Required |
| Closed-M5 trace records the canonical decision gate | PASS | Required |
| Trace records exact decision/actionability reasons | PASS | Required |
| Trace is bounded in memory and append-only on disk | PASS | Required |
| Trace archive uses the same 90-day archive cadence | PASS | Required |
| Offline analyzer uses only forward bars for potential-missed diagnostics | PASS | Required |
| Potential-missed cohort is clearly observational, not a synthetic trade outcome | Source | Required |
| Final automatic-market plan geometry is revalidated immediately before broker mutation | PASS | Required |
| Wrong-side SL / TP1 cannot pass final automatic-market geometry gate | PASS | Required |
| No second decision or execution authority introduced | PASS | Required |
| No new public parameters | PASS | Required |
| Decision Contracts | PASS | Required |
| Runtime Acceptance | PASS | Required |
| cTrader Compile/Build | PASS | Required |
| Source / Architecture + accumulated audits | Required | Required |
| Target-terminal replay identifies actual missed setups and false signals | Required | Required |
| Target-terminal broker execution/protection behavior | Required | Required |


## Phase 9.16 verification evidence

- Runtime Acceptance #1172: PASS
- cTrader Compile/Build #1356: PASS
- Source/Architecture + accumulated audits #1363: PASS
- Phase 9.15 startup/persistence audit: PASS
- Phase 9.16 signal measurement audit: PASS
- Decision Contracts: PASS within Build
- Verified merge commit: `fd6f43594ac1a50a67ccd3007c661a2ba059374f`

Target-terminal replay remains required for empirical missed-opportunity/false-signal measurement, actual startup latency, chart lifecycle and broker execution/protection.


## Phase 9.17 corrective numerical hardening — 2026-09-29

| Contract | Automated controlled check | cTrader / replay |
| --- | ---: | ---: |
| Live TP uses one centralized broker/ATR minimum-forward-distance calculation | PASS | Required |
| BUY percentage distance uses Bid and SELL percentage distance uses Ask | PASS | Required |
| Server-ladder internal target adopts broker LastTakeProfit.Price | PASS | Required |
| Every server-side TP stage is revalidated against live market before mutation | PASS | Required |
| Post-fill structural SL compares prior stop versus new structural stop before ratchet | PASS | Required |
| Recovery fallback cannot exceed configured MaximumRewardRR | PASS | Required |
| RR progression is recomputed from current target/entry/risk geometry | PASS | Required |
| NaN/Infinity in risk, TP stages and Smart Break-Even fails closed | PASS | Required |
| Stale server ladder cannot alone mark broker protection synchronized | PASS | Required |
| No new public parameters or second authority introduced | PASS | Required |

Automated verification:
- Runtime Acceptance #1196: PASS
- cTrader Compile/Build #1380: PASS
- Source/Architecture + accumulated audits #1387: PASS
- Phase 9.17 exit geometry audit: PASS

## Phase 9.17 — Exit Geometry, TP Progression & Protection Integrity

| Contract | Automated controlled check | cTrader / replay |
|---|---:|---:|
| Live TP must remain beyond current market | PASS | Required |
| Live TP cannot regress from current target | PASS | Required |
| BUY/SELL target geometry symmetry | PASS | Required |
| BUY/SELL protective SL geometry symmetry | PASS | Required |
| Progressive TP1→TP2→TP3→TP4 ladder | PASS | Required |
| Actual-fill exit reconciliation is live-aware | PASS | Required |
| Failed fill reconciliation cannot fall back to stale ladder rebuild | PASS | Required |
| Server TP ladder can progress after TP1 | PASS | Required |
| Final target can continue after TP2 | PASS | Required |
| Broker TP mutations are monotonic independent of legacy tuning | PASS | Required |
| Broker minimum TP distance participates in live spacing | PASS | Required |
| Final live SL geometry is broker-distance aware | PASS | Required |
| No second decision/execution authority | PASS | Required |
| No new public parameters | PASS | Required |
| Decision Contracts | Required | Required |
| Runtime Acceptance | Required | Required |
| cTrader Compile/Build | Required | Required |
| Source / Architecture + accumulated audits | Required | Required |
| Target-terminal replay of TP rollback and exit behavior | Required | Required |

Phase 9.17 corrective hardening is merged into `main` via PR #61. Target-terminal replay
remains required for actual live timing, broker/server-side protection behavior, slippage,
realized exit R and confirmation that the reported TP rollback does not recur on the target symbol/timeframe.

## Phase 9.17 verification evidence

- Runtime Acceptance #1196: PASS
- cTrader Compile/Build #1380: PASS
- Source/Architecture + accumulated audits #1387: PASS
- Decision Contracts: PASS within Build
- Phase 9.16 signal measurement audit: PASS
- Phase 9.17 exit geometry audit: PASS
- Phase 9.17 corrective-hardened merge commit: `af4ef4edb5ce032c4a71feaf2cc0be1203f4992b`

Target-terminal replay remains required for empirical confirmation of the reported TP rollback fix,
live timing, broker/server-side protection behavior, slippage, realized exit R and continuation behavior.
