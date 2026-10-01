# CFIP — Claude Review Defect Remediation Roadmap

## 0. Purpose

This document is the remediation gate for the three external Claude code-review prompts supplied on 2026-09-30.

The prompts contain useful code-reading findings, but they were not compiled or runtime-tested. This roadmap therefore separates:

- findings independently confirmed against the current repository;
- findings that are only partially confirmed and require a narrower implementation interpretation;
- findings that conflict with the current source and must not be implemented as stated;
- design risks that require evidence before changing behavior;
- execution/lifecycle defects whose deterministic logic can be hardened now, while broker mutation ownership remains governed by the later local cBot separation track.

The goal is to correct verified defects without blindly adopting speculative changes, while preserving the project's existing parameter names/types/defaults unless a separately documented safety change is explicitly required.

This track is blocking: complete it before resuming the remaining development roadmap or starting the local cBot extraction.

---

## 1. Review source and repository baseline

Review inputs:

- Prompt 1: A1–A12;
- Prompt 2: B1–B12;
- Prompt 3: C1–C9.

Repository baseline used for this remediation roadmap:

- main commit: e94c0adc10a9b17093bc7a8c3c2b0ab98c6f6320;
- current Indicator remains the production host;
- Track 12A local cBot separation is already documented, but execution extraction has not started.

---

## 2. Review verdict matrix

Legend:

- CONFIRMED — current source supports the defect substantially.
- PARTIAL — the underlying risk exists, but the prompt's exact causal claim or proposed fix is too broad.
- DISAGREE / STALE — current source does not support the claim as written.
- DESIGN RISK — not a bug proven by source alone; change only with evidence.
- AUDIT FIRST — exact owner/source path still needs precise inventory before coding.

### Prompt 1

| ID | Verdict | Remediation owner |
|---|---|---|
| A1 EOD/session | CONFIRMED | CR1.1 |
| A2 Daily loss | CONFIRMED | CR1.2; account authority later cBot |
| A3 previous day/week period | CONFIRMED | CR1.1 |
| A4 sweep latest swing claim | PARTIAL | CR2.1 shared structure audit |
| A5 news | CONFIRMED core; AccessRights subclaim requires no source-based fallback | CR1.3 |
| A6 calculation order / waiting state | CONFIRMED | CR1.4 |
| A7 repeated work / synchronous persistence | CONFIRMED risk | CR1.5 |
| A8 FVG quality | CONFIRMED | CR1.6 |
| A9 threshold truth / hidden clamps | CONFIRMED | CR1.7 |
| A10 volume | AUDIT FIRST | CR1.7 |
| A11 identity / broad unmanaged closing | CONFIRMED | CR1.8; execution authority later cBot |
| A12 minor issues | MIXED | CR1.9 |

### Prompt 2

| ID | Verdict | Remediation owner |
|---|---|---|
| B1 CHoCH semantics | CONFIRMED | CR2.1 |
| B2 MSS/Structure duplication | PARTIAL → requires event-freshness audit | CR2.1 |
| B3 Reaction repaint / missing reversal context | CONFIRMED | CR2.2 |
| B4 inconsistent quality/conflict thresholds | CONFIRMED | CR2.3 |
| B5 pending Stop/Limit conflict | CONFIRMED | CR2.4 |
| B6 lifecycle/outcome aggregation | CONFIRMED core; broker-event portions manual | CR2.5 |
| B7 OrderBlock quality/performance | CONFIRMED performance risk; quality details require exact calculator audit | CR2.6 |
| B8 WaveTrend MA/warm-up/cache | CONFIRMED core | CR2.7 |
| B9 historical renderer | CONFIRMED performance/semantic risk, but not literally every Calculate call | CR2.8 |
| B10 structural stop fallback / tight-stop bias | CONFIRMED fallback; DESIGN RISK on scoring bias | CR2.9 |
| B11 divergence thresholds/conflict quality | CONFIRMED | CR2.9 |
| B12 doji rejection | CONFIRMED | CR2.9 |

### Prompt 3

| ID | Verdict | Remediation owner |
|---|---|---|
| C1 soft adverse-R invalidation | CONFIRMED | CR3.1 |
| C2 false-signal threshold behind broker SL | CONFIRMED | CR3.1 |
| C3 proxy-EV semantics | CONFIRMED | CR3.2 |
| C4 early prediction = relative share | CONFIRMED | CR3.2 |
| C5 partial TP/server ladder | MIXED: several items confirmed, one stale | CR3.3 |
| C6 BE/trailing / peak recovery | DESIGN RISK + AUDIT | CR3.3 |
| C7 execution quick-toggle / popup overwrite | CONFIRMED | CR3.4 |
| C8 calibration/outcome statistics | CONFIRMED core | CR3.5 |
| C9 RR rejection transparency | DESIGN RISK / observability gap | CR3.5 |

---

## 3. Findings that must not be copied verbatim

### 3.1 A4 — latest sweep claim is too broad

The current swing helpers require right-side confirmation. Therefore the exact claim that the sweep candle necessarily invalidates the newest possible swing simply because it is inside the right-side window is not established.

What is valid:

- a swing can still be eligible after confirmation;
- the current sweep/structure model needs an explicit definition of an active/unbroken liquidity level;
- sweep penetration should be normalized to volatility rather than a universal fixed pip tolerance.

CR2.1 therefore changes only the proven semantics, not the assumption that every recent swing must be immediately sweepable.

### 3.2 A5 — AccessRights.None is not evidence of a mandatory permission failure

Current official cTrader documentation states that AccessRights.None is sufficient for network functions and demonstrates Http.Get under that access-right mode. The repository still needs target-terminal verification, but the remediation must not invent a permissions workaround merely from the prompt's warning.

### 3.3 B9 — historical rendering is not literally every Calculate

Current source only calls historical rendering when the host bar changes. The real problem is that each such rebuild can scan a large historical range and re-run expensive analysis. The fix must target the actual trigger frequency and repeated work, not an incorrect every-tick claim.

### 3.4 C5 item 4 — stale BE-plan claim is not supported by current source

Current PartialTakeProfitExecutor updates the plan stop to entry only after the broker stop modification succeeds. On rejection it enters recovery without overwriting the plan stop. Therefore this subclaim must not be implemented as written.

---

## 4. Global implementation rules for this remediation track

1. Preserve every existing public Parameter name, type and DefaultValue unless a new parameter is required for a safety correction. New parameters must default to current/safe behavior.
2. Every implementation fix receives its own commit using fix(<ID>): ...
3. Every logical fix gets a real behavior test, not source-text grep.
4. Pure deterministic logic belongs in Core/ and must not depend on Bars, Chart, broker objects or cTrader UI.
5. Time-dependent logic receives an injectable clock abstraction.
6. No threshold is silently moved into another module as a second decision authority.
7. Shared threshold truth is fixed once and reused by all callers.
8. For execution-related fixes that will later move to cBot, extract the deterministic rule now and keep broker mutation ownership explicit; do not create a second executor.
9. No profitability/accuracy claim is made from these source fixes alone.
10. Existing closed-bar and single-position invariants remain mandatory.
11. Every phase includes the permanent project-wide routine audit: Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning.
12. Every phase includes a performance/code-cleanliness audit so fixes do not trade correctness for panel latency or hot-path cost.

---

## 5. Prompt 1 remediation track

## CR1.1 — Session/EOD and period-reference correctness

Status: **COMPLETE — 2026-09-30**

Implemented:
- added pure `Core/Math/SessionWindowRule.cs` as the single semantic owner for session membership, session ranges and EOD boundaries;
- unified `SessionAllowed()`, `IsInsideSessionWindow()` and session presentation against the same rule;
- unified session-range resolution for session target sources, including overnight and start==end semantics;
- corrected the latest fully closed D1/W1 period references from `index - 1` to the canonical closed index;
- corrected the D1 pivot suitability consumer to the same canonical closed index;
- bounded EOD automatic cleanup to the explicit five-minute post-boundary safety window;
- included managed pending orders in EOD cleanup;
- prevented positions whose broker EntryTime is after the session boundary from being immediately closed by the previous session's cleanup;
- made EOD completion/reported closure contingent on a broker-state reread showing no remaining pre-boundary managed position and no managed pending order;
- added deterministic runtime contracts covering standard/overnight/start==end sessions, pre/post EOD windows and closed-bar reference semantics.

Explicit safety behavior change:
- the previous unbounded `now >= dayClose` auto-close behavior is replaced by a bounded post-boundary window;
- successful mutation request is no longer treated as final EOD completion without broker-state confirmation.

Verification:
- Runtime acceptance contracts: **PASS**, run 1339;
- cTrader compile: **PASS**, run 1523;
- Source and architecture checks: **PASS**, run 1530.

Permanent routine audit for this phase:
- Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning reviewed for session-boundary coupling;
- performance/code-cleanliness checked; no new hot-path I/O or duplicate session authority introduced.

Next implementation phase: **CR1.2 — Daily-loss lock and stable accounting basis**.

## CR1.2 — Daily-loss lock and stable accounting basis

Covers: A2.

Status: COMPLETE — implementation closed 2026-09-30; target-terminal verification remains required.

Implemented:
- separated deterministic daily-loss evaluation from broker/account fact acquisition;
- persisted the account-scoped UTC-day baseline and latched lock state through LocalStorageScope.Type;
- included same-day realized P/L plus the change in floating P/L from the baseline when complete history and transaction facts are available;
- added an equity-change fallback when realized history is unavailable, while subtracting verified same-day deposits/withdrawals;
- changed incomplete transaction facts to fail closed rather than silently ignoring cash-flow effects;
- hardened all daily-loss numeric inputs against NaN/Infinity and invalid thresholds;
- kept Market, Aggressive and Pending automatic-entry paths on the same daily-loss gate;
- preserved the existing rule that daily-loss lock blocks new automatic entries and does not close open positions;
- added account-switch reset/recovery handling;
- added deterministic contracts for realized loss, floating drawdown, deposits, withdrawals, persistence lock semantics, inclusive threshold behavior, invalid baseline, missing transaction facts and non-finite inputs;
- kept the implementation isolated into baseline rule, evaluation, accounting, guard and persistence owners to avoid a monolithic risk helper.

Accounting contract:
- preferred path: realizedNetProfit + (currentUnrealizedNetProfit - baselineUnrealizedNetProfit);
- fallback path: currentEquity - baselineEquity - netCashFlow;
- transaction facts are mandatory because cash-flow adjustments must be explicit;
- the baseline is persisted per account and UTC calendar day; broker/session-day equivalence remains a target-terminal verification item.

Verification:
- deterministic runtime-contract coverage updated for the complete accounting contract;
- current cTrader API surface verified against official documentation for Account equity/unrealized P/L, Transactions, HistoricalTrade and LocalStorage persistence;
- full repository cTrader compile/runtime acceptance and broker-day/restart behavior remain target-environment gates.

Safety:
- no new decision authority;
- no new execution authority;
- no increase in position capacity;
- no automatic close-on-daily-loss behavior introduced;
- broker mutation remains in the current Indicator until the approved cBot migration.

Manual verification:
- broker trading-day boundary;
- realized/deal history semantics;
- deposits/withdrawals;
- restart with an active daily-loss lock;
- separate Indicator instances observing the same account-scoped lock.

## CR1.3 — News guard correctness and non-blocking refresh

Covers: A5.

Status: COMPLETE — implementation closed 2026-09-30; target-terminal verification remains required.

Implemented:
- removed synchronous news HTTP from initialization and replaced calendar transport with cTrader Http.GetAsync;
- limited refresh to the existing Timer heartbeat so decision/hot paths never initiate network I/O;
- added explicit in-flight state, monotonically increasing request generation and a bounded 30-second transport timeout;
- ignored late responses from abandoned/timed-out requests so stale callbacks cannot overwrite newer calendar state;
- retained the last validated calendar cache across refresh failures instead of clearing valid events;
- added deterministic DISABLED / NEVER_LOADED / HEALTHY / STALE / BLOCKING_EVENT feed states with REFRESHING and bounded last-error diagnostics;
- preserved fail-closed automatic execution behavior for NEVER_LOADED/STALE across Auto Trading and Auto Orders when NewsFailClosedWhenStale is enabled;
- added a pure symbol/index/crypto currency-mapping rule and the configurable the existing AdditionalNewsCurrencies parameter using SYMBOL=CUR mapping entries parameter;
- normalized broker-specific symbol punctuation and deterministically merged detected, additional and mapped currencies;
- kept Market/decision and automatic execution news gates on the same cached NewsBlocked consumer;
- preserved existing pending cancellation, optional active-position pre-news close and manual UTC blackout behavior.

Acceptance:
- no decision path waits on network I/O;
- refresh failures are visible without destroying the last valid cache;
- never-loaded and stale feed behavior is deterministic and independently testable;
- relevant currencies for FX/index/commodity/crypto symbols are resolved deterministically;
- synchronous Http.Get/Http.Send usage is forbidden by the News Guard source audit;
- deterministic runtime contracts cover feed state and currency mapping.

Verification:
- cTrader documentation rechecked for Http.GetAsync and AccessRights.None network support;
- deterministic runtime-contract coverage added;
- News Guard source audit strengthened to detect synchronous HTTP and missing async/state contracts;
- target-terminal verification remains mandatory for actual feed availability, timestamp semantics, installed cTrader behavior, startup latency and live news-block/cancellation behavior.

## CR1.4 — Closed-bar cycle ordering and waiting-for-data state

Covers: A6.

Status: **COMPLETE — implementation closed 2026-09-30; target-terminal verification remains required.**

Implemented:
- added an explicit same-cycle pre-decision broker reconciliation boundary for each newly closed M5 decision cycle;
- gated decision/restriction/actionable alerts on successful same-cycle broker reconciliation so stale lifecycle snapshots cannot be consumed as fresh execution context;
- applied the same boundary to the startup calculation seed;
- introduced deterministic BUILDING DATA / WAITING FOR CLOSED M5 / WAITING FOR MTF DATA / READY states;
- throttled readiness probes while history or primary MTF context is incomplete, preventing expensive readiness/panel work on every tick;
- preserved broker lifecycle recovery, news protection, active-plan management, broker position protection and outcome/EOD/reversal supervision while analysis readiness waits;
- explicitly kept plan creation, market execution, aggressive execution and predictive pending execution out of the waiting-state path;
- limited pre-decision broker reconciliation to newly closed-bar decision cycles for hot-path efficiency; non-new-bar live cycles retain the existing management reconciliation;
- added deterministic runtime contracts for readiness state, probe cadence and rollback-safe clock behavior;
- added a dedicated cycle audit to enforce ordering and to reject execution calls from the waiting-state path.

Acceptance:
- decision/alert consumption follows same-cycle broker reconciliation;
- WAITING FOR MTF DATA is represented as an explicit bounded state and panel refresh is throttled to 500 ms while waiting;
- BUILDING DATA and WAITING FOR CLOSED M5 use a 250 ms probe cadence;
- waiting-state cycles continue protection/lifecycle work but never create or submit a new execution plan;
- runtime fault containment remains intact and a failed pre-decision reconciliation blocks automatic entry while lifecycle supervision continues.

Verification:
- Runtime acceptance contracts: PASS;
- cTrader compile: PASS;
- Source/architecture checks: PASS is required on the final head and includes the dedicated CR1.4 cycle audit;
- cTrader documentation rechecked for Calculate/IsLastBar semantics and Bars BarClosed/closed-bar behavior;
- target-terminal verification remains required for broker event timing, asynchronous data availability and startup/readiness behavior.

## CR1.5 — Hot-path/cache/logging performance

Covers: A7.

Status: COMPLETE — implementation closed 2026-09-30; target-terminal verification remains required.

Implemented:
- shared buffered archive writer with bounded one-second Timer flush cadence and 512-line budget;
- Runtime Log, Outcome archive and Signal Evaluation Trace file appends moved out of Calculate;
- keyed archive idempotency retained while existing-file scans are limited to the timer flush path;
- explicit LocalStorage.Flush for Outcome Memory and Daily Loss deferred to the runtime heartbeat, with priority persistence for a newly reached daily-loss lock;
- cross-instance Daily Loss LocalStorage.Reload moved off Calculate and onto the heartbeat;
- shutdown flushing for buffered persistence;
- 90-day archive prefixes and the parameter fingerprint cached to remove repeated reflection/string work from logging;
- closed-bar-context FVG and Order Block candidate caches with deterministic Bars/index keys while live/open-bar selection remains quote-sensitive;
- event/mutation-driven broker-state invalidation with a one-second safety refresh interval;
- deterministic Runtime Contracts for buffered archive dedupe/budget and broker refresh dirty/TTL semantics;
- dedicated hot-path persistence/cache audit wired into Source/Architecture CI;
- existing historical archive partitioning preserved and historical files are not deleted.

Acceptance:
- Runtime Acceptance: PASS on the final CR1.5 head;
- cTrader compile: PASS on the final CR1.5 head;
- Source/Architecture final-head verification remains required before merge;
- archive/event file writes are owned by BufferedArchivePersistence; the startup history-location marker remains an explicit startup-only file write;
- no synchronous LocalStorage.Reload/Flush remains in Calculate-facing persistence owners;
- broker-state refresh is deterministic and event/mutation invalidated;
- FVG/OB cache invalidation is tied to explicit Bars/index context and excludes open bars.

Safety:
- no public parameter added or changed;
- no signal/RR threshold tuned;
- no position capacity changed;
- no execution authority changed;
- no existing 90-day history files are deleted.

Verification boundary:
- target-terminal verification remains required for actual cTrader LocalStorage/file timing, restart persistence, cross-instance behavior and runtime performance;
- container-local clone/build was unavailable because this execution environment could not resolve github.com; repository CI is the compile/runtime contract authority for this phase.

## CR1.6 — FVG quality discrimination

Covers: A8.

Status: **COMPLETE — implementation closed 2026-09-30; target-terminal/replay validation remains required.**

Implementation:
- preserved canonical FVG geometry and mitigation rules unchanged;
- replaced the uniformly high fixed-base quality formula with the pure Core/Math/FvgQualityRule;
- scored six dimensions: gap/ATR magnitude, directional displacement, freshness, remaining unmitigated geometry, current structural alignment and higher-timeframe alignment;
- retained an explicit three-point penalty for the two-bar imbalance extension;
- used the M5→M15→M30→H1→H4→D1→W1 higher-timeframe chain;
- made non-finite quality input fail closed;
- added deterministic runtime-contract fixtures for weak/moderate/strong separation and monotonic component effects.

Behavior:
- weak, aged, mitigated and context-free FVGs no longer start from an artificial 70+ baseline;
- strong fresh FVGs with directional displacement and aligned structure/HTF context can reach the upper quality range;
- no public parameter name/type/DefaultValue changed.

Verification:
- deterministic FVG quality contract added to CFIP.Runtime.Contracts;
- Build/Runtime/Source checks are required on the final commit;
- target cTrader replay remains required before claiming empirical signal-quality improvement.

Permanent routine audit:
- Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning reviewed for duplicate FVG-quality ownership;
- performance/code-cleanliness reviewed; no network/file I/O or per-tick scan was introduced by this fix.

Next implementation phase: **CR1.7 — Threshold truth + volume audit**.

## CR1.7 — Threshold truth + volume audit

Covers: A9, A10 and overlap with B4.

Status: **COMPLETE — implementation closed 2026-09-30; target-terminal verification remains required.**

A9 — threshold truth:
- moved fixed execution-path IndicatorConfluenceQuality/IndicatorConflict thresholds into `Core/Math/ExecutionThresholdPolicy.cs` as named semantic constants;
- migrated Automatic Market, Pending Submission and Pending Continuation/Reversal callers to the centralized definitions;
- centralized defensive bounds for DirectionShare, reversal evidence/MTF, EOD alert minutes and MaximumSpreadToStopRiskRatio;
- aligned DirectionShare's defensive upper bound with its public `MaxValue=95` instead of the prior hard 90 ceiling;
- reduced the generic `RiskPercentPolicy` defensive upper clamp from 100 to the public parameter ceiling 5 because the parameter itself is constrained to 5%;
- did not add public parameters or change any public parameter name/type/DefaultValue;
- retained the four existing path-specific indicator-quality policies because semantic unification is deliberately deferred to CR2.3, preventing an accidental default-behavior change.

A10 — volume/sizing:
- added pure `Core/Math/VolumeSizingRule.cs` for stop-risk, risk-input and normalized-volume invariants;
- risk-percent volume sizing now rejects non-positive/non-finite stop risk before invoking `VolumeForFixedRisk`;
- both normal and Aggressive volume paths require finite normalized volume within broker min/max bounds;
- Aggressive volume failure now emits bounded diagnostic logging instead of silently swallowing exceptions;
- `RiskAmountCalculator` now fails closed on non-finite equity/risk percentage and computed amount;
- no position capacity or public risk default was increased.

Tests:
- added deterministic threshold-boundary contracts;
- added deterministic volume/risk-input and normalized-volume contracts;
- verified the five-percent risk ceiling and smart-risk scaling behavior.

Permanent routine audit:
- Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning reviewed for threshold/volume ownership drift;
- performance/code-cleanliness reviewed; no network/file I/O or unbounded scan was introduced.

Manual verification:
- target cTrader volume normalization/step semantics for the actual symbol;
- broker rejection behavior for min/max/step-normalized volume;
- actual account equity and risk conversion on representative symbols.

Next implementation phase: **CR1.9 — Minor cleanup and documentation**.
## CR1.8 — Managed identity boundary

Covers: A11.

Status: VERIFIED COMPLETE — implementation closed 2026-09-30.

Confirmed findings:
- BrokerIdentity.cs previously accepted same-symbol/same-label positions without instance isolation.
- ManagedActionsOnly=false previously broadened ownership to every same-symbol position.
- ReversalCloseGuard.cs previously required position.NetProfit > 0 with no configurable minimum.

Implementation:
- stable cTrader InstanceId is bound into one canonical managed execution label: <base>|CFIP-I:<instance>;
- Automatic Market, Aggressive Market, Pending Stop and Pending Limit submissions all use that instance-scoped label;
- position and pending-order ownership require the exact instance-scoped identity, so ManagedActionsOnly=false can no longer broaden broker mutation scope;
- ReversalCloseMinimumNetProfit was added as the explicit minimum net-profit safety parameter, default 0.0, preserving the prior strict-positive behavior while removing the hidden constant;
- deterministic runtime contracts and a dedicated CR1.8 source audit cover identity stability, cross-instance isolation and reversal-profit boundary behavior.

Architecture boundary: Pure identity matching is hardened now. Final broker-position authority still moves to cBot under Track 12A; CR1.8 does not start or advance cBot separation.

Acceptance:
- same-label instances are distinguishable by InstanceId;
- unmanaged/manual/foreign positions are not adopted merely because symbol/label match;
- all current broker submission paths carry the canonical instance identity;
- reversal closing consumes one explicit profit-threshold rule;
- identity remains authoritative for recovery/exit ownership.

Verification:
- Source/Architecture + CR1.8 source audit: required via CI;
- Runtime Acceptance: required via CI;
- cTrader Compile/Build: required via CI;
- target-terminal replay remains required for actual broker identity/restart/reconciliation behavior.

## CR1.9 — Minor cleanup and documentation

Covers: A12.

Status: COMPLETE — implementation closed 2026-09-30.

Implemented:
- added machine-enforced `tools/audit_parameter_count.py` and wired it into Source/Architecture CI;
- corrected README parameter reporting to the current machine-derived count of 567;
- documented `MaximumOpenPositions` as intentionally single-valued (`1/1`) under the current certified single-plan capacity;
- made the public session-parameter resolution explicit as 60 minutes via the canonical `SessionWindowRule.SessionResolutionMinutes` invariant;
- extended runtime contracts to assert the session-resolution invariant;
- documented the cleanup and exact acceptance boundary in `docs/PHASE-CR1-9-MINOR-CLEANUP.md`;
- preserved all public parameter names/types/defaults and introduced no new trading threshold or execution authority.

Verification:
- Source/Architecture CI must pass the new parameter-count/documentation audit;
- Runtime Acceptance must pass the extended session contract;
- cTrader Compile/Build must pass with unchanged host behavior.

Target-terminal runtime behavior is not claimed from CI alone.

Next implementation phase: **CR2.1 — Structure/CHoCH/MSS/Sweep/Divergence/Rejection semantics**.

---

## 6. Prompt 2 remediation track

## CR2.1 — Structure/CHoCH/MSS/Sweep/Divergence/Rejection semantics

Covers: B1, B2, A4 overlap, B10, B11, B12.

Status: COMPLETE — merged 2026-09-30 (PR #86, merge commit 20835cbf1e541b51b9ad56af46cf0c5d13ff5350).

Implemented:
- true closed-bar structural break freshness and prior-opposite-structure CHOCH semantics;
- canonical structural event identity and duplicate-evidence collapse;
- active/unbroken liquidity validation tied to canonical swing plateaus;
- explicit W1 handling plus fail-closed unknown structural timeframes;
- divergence conflict neutrality;
- minimum meaningful-body rejection/doji semantics;
- deterministic runtime contracts and CR2.1 static audit.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.
- Verified head: `1c5dbf17140f3757ea4e3289183fb73ab8e65b9b`.

Target-terminal broker/chart runtime remains a separate manual acceptance boundary.

## CR2.2 — Reaction/reversal integrity

Covers: B3.

Status: COMPLETE — merged 2026-09-30 (PR #87, merge commit bdb9b72d021972db4b3638ff5eb4d7078ab2cb2a).

Implemented:
- explicit split between moving M5 intrabar observation and closed-bar reversal confirmation;
- canonical reversal context using prior counter-move, qualifying zone, or canonical swing interaction;
- explicit no-zone behavior and weak-zone non-bypass semantics;
- equal bull/bear reaction scores resolve to Direction=0;
- shared ReactionQualificationRule used by Pending reversal and Aggressive intrabar qualification;
- deterministic runtime contracts, static audit and phase documentation.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

Target-terminal intrabar behavior remains a manual acceptance boundary.

Next implementation phase: **CR2.3 — Unified indicator-quality thresholds**.

## CR2.3 — Unified indicator-quality thresholds

Covers: B4 plus A9.

Status: COMPLETE — merged 2026-09-30 (PR #88, merge commit 98da2030d8e36312ee0c073c1a58889bb405f893).

Implemented:
- one semantic owner for IndicatorConfluenceQuality and IndicatorConflict execution gates;
- preserved legacy threshold values while separating setup qualification from submission safety semantics;
- shared the 62 quality floor across pending continuation and reversal;
- retained explicit 48/50 Conflict differences because their setup semantics differ;
- migrated all covered execution callers to the canonical stage-aware rule;
- added deterministic contracts, static ownership audit and phase documentation.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

Target-terminal behavior remains a separate manual acceptance boundary.

## CR2.4 — Pending-order decision arbiter

Covers: B5.

Status: COMPLETE — merged 2026-09-30 (PR #89, merge commit ac8c526f7ed0887ba990dc9a091c8299e96a6de5).

Implemented:
- canonical one-winner arbitration between Continuation Stop and Reversal Limit;
- explicit quality comparison and deterministic continuation tie-break;
- independent range/suitability evaluation for each candidate direction;
- no silent fallback from a failed selected placement to the other strategy;
- two-bar closed-M5 cancellation hysteresis;
- executable-side predictive Limit pricing;
- explicit continuation fallback lookback bounds;
- deterministic runtime contracts, static audit and phase documentation.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

Target-terminal broker behavior remains a separate manual acceptance boundary.

## CR2.5 — Lifecycle ordering and outcome aggregation

Covers: B6 plus the shared outcome work required by C8.

Status: COMPLETE — merged 2026-09-30 (PR #90, merge commit 45d6d21e050db2c519a57a0cdeccfff5d2d88fac).

Implemented:
- bounded lifecycle event idempotency storage;
- broker-order-tolerant PositionOpened recovery with managed identity enforcement;
- complete historical closing-trade aggregation through the canonical History reader;
- initial-risk-based realized R from final aggregated monetary outcome;
- one canonical outcome source for telemetry and win/loss counters;
- separated history reading, outcome result typing and panel/window telemetry responsibilities;
- deterministic runtime contracts, static CR2.5 audit and phase documentation.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

Manual acceptance remains required for actual cTrader broker History/Deal ordering and deal-history semantics.

## CR2.6 — OrderBlock quality and cache discipline

Covers: B7.

Status: COMPLETE — PR #91 merged 2026-09-30, merge commit 8f89d75e9fed70a53b51a902b52c539d9c514f0b.

Implemented:
- canonical OrderBlockQualityRule owning the exact base score, independent evidence components, age/mitigation penalties and final clamp;
- unit-consistent creation-ATR impulse normalization;
- canonical side-of-market validation for bullish/bearish OB selection;
- explicit Fresh / Mitigated / Broken lifecycle semantics;
- closed-bar/frame/direction candidate caching with quote selection deliberately kept outside the cache key;
- deterministic runtime contracts and static CR2.6 audit.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

Manual acceptance remains required for target-terminal zone interaction and empirical signal-quality measurement.

## CR2.7 — WaveTrend mathematical correctness

Covers: B8.

Status: COMPLETE — PR #92 merged 2026-09-30, merge commit b021f56c4b75331fb52127547e006f2f8bb287c4.

Implemented:
- deterministic MA semantics for all currently declared cTrader MovingAverageType values;
- DEMA/TEMA dependency-chain initialization and warm-up correction;
- HMA raw-window weighting correction;
- full component/smoothing/signal readiness dependency;
- HistoryLoaded/Reloaded and history-prefix/count invalidation;
- explicit TickVolume-based MFI documentation;
- deterministic runtime contracts and CR2.7 static audit.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS;
- cTrader Compile: PASS.

Verified head: `181b238a548280fc01a67fb1e3ba8a617f63e42a`.

## CR2.8 — Historical rendering semantics and cost

Covers: B9.

Status: **COMPLETE — PR #93 merged 2026-09-30, merge commit cab5a5e2e9a4fbccaf3ffe10d114c4ff54e6a243.**

Implemented:
- fixed historical scan budget at 500 closed bars, independent of HistoricalSignalLimit;
- cached each historical presentation result by closed-bar timestamp;
- changed historical object identity from bar index to timestamp-based presentation identity;
- invalidated historical cache/object state on Bars HistoryLoaded/Reloaded and detected history-shape changes;
- stopped deleting/recreating all historical objects on every ordinary render;
- documented and statically enforced that historical arrows are presentation-only;
- preserved the existing host-bar rebuild gate so unchanged host-bar state does not rerun historical analysis;
- added deterministic runtime contracts and the CR2.8 static audit.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS, run 1585;
- cTrader Compile: PASS, run 1769;
- CR2.8 static audit: PASS;
- accumulated CR2.1–CR2.7 audits: PASS.

Manual target-terminal verification remains required for history prepend/load-more/reload visual behavior, chart responsiveness and confirmation that historical arrows never create alerts/plans/orders/outcomes.

Next implementation phase: **CR2.9 — Structural stop, divergence and rejection guardrail refinement.**

## CR2.9 — Structural stop, divergence and rejection guardrail refinement

Covers: B10, B11, B12.

Status: **COMPLETE — PR #94 merged 2026-09-30, merge commit 6786af20d7b63d371c57890fb1adabb666f830bc.**

Implemented:
- preserved the existing fail-closed unknown structural timeframe behavior and verified it in the CR2.9 fixtures;
- extracted structural-stop reward-path bonus and preferred-risk balance into a pure, testable owner without tuning live coefficients;
- centralized all divergence quality/conflict thresholds, price-excursion floors, oscillator deltas, recency boosts and quality-score components;
- centralized rejection/doji meaningful-body and wick thresholds and made both semantics share the same body rule;
- added deterministic CR2.9 runtime contracts and a dedicated static audit;
- preserved public parameters and execution/decision authority.

Verification:
- Source/Architecture: PASS;
- Runtime Acceptance: PASS, run 1597;
- cTrader Compile: PASS, run 1781;
- CR2.9 static audit: PASS;
- accumulated CR2.1–CR2.8 audits: PASS.

No empirical stop-bias, accuracy, win-rate or profitability claim is made. Any future score tuning requires replay/outcome evidence.

Next implementation phase: **CR3.1 — Live invalidation and false-signal semantics**.

---

## 7. Prompt 3 remediation track

## CR3.1 — Live invalidation and false-signal semantics

Covers: C1, C2.

Status: VERIFIED COMPLETE — PR #95 merged to main as f482d2f76cdf37cca87fabc5b11b3d8c0a7edac7.

Implemented:
- explicit Enable Soft Adverse-R Invalidation safety flag with DefaultValue=true;
- closed-bar-stable live invalidation on canonical closed M5 results;
- confirmed structural swing candidates instead of rolling min/max invalidation extrema;
- explicit broker-close mutation result and RecoveryRequired handling on rejection;
- centralized successful _lastExitM5 bookkeeping;
- canonical FalseSignalAdverseR validation and BUY/SELL symmetric protected-stop envelope handling;
- deterministic CR3.1 runtime contracts and dedicated static audit;
- preserved configured FalseSignalAdverseR default 1.10 and its 0.25–5 bounds; no empirical threshold tuning.

Acceptance:
- rejected broker exits do not advance successful-exit bookkeeping;
- soft/hard adverse-R semantics cannot silently contradict a known adverse broker SL envelope;
- BUY/SELL symmetry is covered by runtime contracts.

Verification:
- Source/Architecture: PASS, run 1808;
- Runtime Acceptance: PASS, run 1617;
- cTrader Compile: PASS, run 1801;
- CR3.1 static audit: PASS;
- accumulated CR2.1–CR2.9 audits: PASS;
- project-wide optimization/integrity audits: PASS after reconciling the intentional safety-parameter inventory to 568.

Evidence boundary:
- CI does not prove target-terminal timing, broker rejection under live network conditions, restart/reconnect reconciliation, or profitability/accuracy.
- Any future parameter/threshold tuning remains replay/outcome gated.

Next implementation phase: CR3.2 — Decision gate and early prediction semantics (C3/C4).
## CR3.2 — Decision gate and early prediction semantics

Status: **VERIFIED COMPLETE — PR #98 merged to main as af7eb503dd661d508afcc7619c72f74cea1b3a0f.**

Covers: C3, C4.

Repository verification evidence:
- Source/Architecture: PASS, workflow run #2021;
- cTrader Compile: PASS, workflow run #2014;
- Runtime Acceptance Contracts: PASS, workflow run #1830.

Completed:
- deterministic Reward Quality Floor semantic replaces the misleading proxy-EV naming;
- early prediction exposes and validates directional share plus absolute evidence strength;
- named evidence weights are centrally owned;
- deterministic runtime contracts and CR3.2 static audit are in place.

Verification:
- Source/Architecture: PASS, run 1820;
- Runtime Acceptance: PASS, run 1629;
- cTrader Compile: PASS, run 1813;
- CR3.2 static audit: PASS;
- accumulated routine/optimization/integrity audits: PASS.

Manual target-terminal replay/outcome evidence remains required for empirical signal-quality claims.

Work:
- stop calling a proxy calculation expected value unless its input is a calibrated probability and actual RR;
- either rename the concept to a quality floor or introduce the minimum deterministic inputs required for a true expected-value computation;
- add an absolute-strength condition to early prediction so a high ratio of two tiny scores cannot look highly confident;
- centralize named weights instead of scattered magic constants;
- do not change live defaults merely to make the metrics look better.

Acceptance:
- confidence labels describe what is actually measured;
- early prediction contains both directional share and absolute evidence/strength;
- tests cover low-total-score/high-ratio cases.

## CR3.3 — Partial TP, server ladder, BE and trailing

Covers: C5, C6.

Work:
- ensure server-side TP3 observation is either explicitly intentional or consistently broker-authoritative;
- verify repeated partial-close retries use bounded retry/idempotency semantics;
- unify post-partial BE with the same spread-aware protection rule where appropriate;
- keep plan protection state synchronized only after broker-confirmed mutation;
- distinguish broker-side volume reduction caused by TP ladder from arbitrary/manual/external partial closes using broker history/identity evidence;
- verify dynamic TP advance is monotonic and cannot move backward;
- audit peak-RR reconstruction across restart/recovery before deciding whether persistence is necessary;
- expose explicit diagnostics when BE is not currently applicable rather than silently waiting.

Acceptance:
- partial close rejection does not create rapid duplicate broker requests;
- broker-confirmed protection is what the UI/plan reports;
- TP ladder state is driven by confirmed broker evidence, not just volume coincidence;
- restart preserves required management state or explicitly re-establishes it from broker/history truth.

## CR3.4 — Execution UI control and popup reliability

Covers: C7.

Status: **VERIFIED COMPLETE — PR #100, all required CI gates passed on commit `d8bb61083a043ce46c11b92dc8068b029c0c5add`.**

Implemented:
- explicit Auto Trade quick-enable re-arm now uses a dedicated runtime state-machine operation and does not depend on the public Indicator configuration parameter;
- fail-closed re-arm behavior remains enforced for Degraded, EntryBlocked and Recovering runtime states;
- alert popups now enter a fixed-capacity queue instead of overwriting the active popup directly;
- critical popup alerts are prioritized over normal informational alerts, while normal overflow is bounded;
- popup queue processing is moved to the timer boundary so popup control mutation is outside the calculation hot path;
- runtime contracts and a dedicated CR3.4 static audit enforce these ownership rules.

Acceptance:
- runtime-fault re-arm cannot bypass a non-Healthy state;
- popup messages are not silently replaced by later normal alerts;
- critical messages can take priority over an active normal popup;
- no new trading decision or broker authority was introduced.

Next implementation phase: **CR3.5 — Calibration, outcome and rejection transparency.**

Work:
- make runtime fault/re-arm checks independent of the current Indicator parameter state when the UI requests a runtime enable;
- do not let the Indicator UI falsely imply broker execution is armed after cBot separation begins;
- replace single-message popup overwrite with bounded queue/priority handling;
- preserve important safety alerts over low-priority informational messages.

Acceptance:
- runtime fault state cannot be bypassed by a quick-toggle path;
- critical protection/EOD/rejection messages are not silently overwritten by a later popup.

## CR3.5 — Calibration, outcome and rejection transparency

Covers: C8, C9 plus B6.

Work:
- implement IEquatable<ConfidenceCalibrationKey> and deterministic Equals;
- store outcome statistics from realized R/deal aggregation, not only final boolean win/loss where the metric claims calibration quality;
- report sample count beside calibrated metrics;
- verify calibration eligibility across Market/Aggressive/Pending so selection bias is visible;
- add explicit rejection-reason telemetry for plan reward integrity;
- preserve existing RR defaults unless replay evidence justifies a future parameter change.

Acceptance:
- calibration keys are structurally correct and efficient;
- sample size is visible;
- realized R/outcome data is consistent with partial-close history;
- plan rejection reasons are inspectable without converting the validator into a second decision engine.

---

## 7.0 — Prompt 4 Remediation Track: Storage, Learning, TP, OSS, Regime and Live Reversal

Prompt 4 from the external Claude review is now incorporated as a **new verification/remediation track before CR-FINAL**.

Important review rule:
- D1–D10 are review findings supplied from static code reading.
- They are **not automatically accepted as proven defects**.
- Each item must first be reconciled against current main source and existing CR-0 → CR3.5 changes.
- No public parameter name/type/DefaultValue may change.
- No behavior-changing default adjustment may be made without explicit evidence and separate documentation.
- Every accepted implementation item receives its own `fix(Dx): ...` or `test(Dx): ...` commit as appropriate.
- Every phase includes the permanent Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning audit and the performance/code-cleanliness audit.

### CR4.1 — Learning-memory identity and account scoping (D1)

Status: **COMPLETE — PR #102 merged to main, 2026-09-30.**

Own verification:
- D1.1 presentation parameters contaminating the learning fingerprint: confirmed and fixed;
- D1.2 missing account scope: confirmed and fixed across LocalStorage, outcome archive, runtime log and portable snapshot identities;
- D1.3 PositionId collision risk across accounts: isolated by account-scoped identity; legacy migration additionally requires current-account broker History evidence;
- D1.4 missing schema migration: confirmed and fixed with current schema v2 and a legacy reader;
- D1.5/D1.6 archive/runtime/snapshot identity drift: confirmed and fixed.

Verification assets:
- deterministic runtime contract: `VerifyOutcomeMemoryIdentitySemantics()`;
- static phase gate: `tools/audit_phase_4_1.py`;
- PR: #102;
- merge commit: `071baf7ab0bda6df8f1d06c9ecbf9d28810e33d1`.

Repository CI status for the merge could not be retrieved through the available GitHub status interface at closeout; no CI PASS is claimed here. Target-terminal account-switch/persistence evidence remains mandatory.



Scope:
- `OutcomeMemoryStore.MemoryConfigurationFingerprint()`;
- `OutcomeMemoryKey()`;
- `OutcomeHistoryArchiveStore.OutcomeArchivePrefix()`;
- runtime/portable memory key construction;
- account identity and cross-account collision semantics;
- schema migration from legacy memory keys/files.

Required verification:
- separate decision-affecting parameters from presentation-only parameters;
- confirm whether the current fingerprint still includes all public parameters;
- confirm account scoping is absent/present in all learning stores;
- confirm legacy archives remain readable after the new identity is introduced;
- ensure PositionId deduplication cannot collide across accounts.

Possible implementation direction, subject to audit:
- explicit decision-affecting parameter identity manifest or attribute;
- account-scoped schema v2 key;
- migration reader for the legacy schema;
- deterministic test fixtures proving presentation-only changes preserve the learning identity while decision changes alter it.

Manual cTrader evidence:
- account number/type;
- LocalStorage persistence and migration behavior across restart/account switch.

### CR4.2 — File/archive path and persistence observability (D2)

Status: **COMPLETE — PR #103 merged to main, 2026-09-30; merge commit `05a91cb9764f7ee86fcaaba0d7dede540c4b6e1c`.**

Own verification:
- cTrader's current .NET 6 restricted-file contract confirms that relative paths are the intended mechanism inside the designated algo folder; no AccessRights change is required by this finding.
- direct snapshot/marker writes were consolidated into the bounded persistence owner;
- archive/runtime/signal-trace writes remain buffered and timer-flushed;
- persistence read/write failures now have bounded observable counters and startup probe state;
- Signal Trace archive identity is account-scoped and invalidated on account switch.

Repository artifacts:
- deterministic runtime contract: `VerifyPersistenceHealthSemantics()` plus extended `VerifyBufferedArchivePersistence()`;
- static gate: `tools/audit_phase_4_2.py`;
- phase record: `docs/PHASE-CR4-2-PERSISTENCE.md`.

Target-terminal evidence remains mandatory for exact cTrader build, actual History path, AccessRights.None execution, read/write probe and restart/account-switch persistence behavior.



Scope:
- outcome archive;
- signal trace archive;
- runtime log persistence;
- portable memory snapshot;
- path construction;
- write-test/read-test and error counters;
- hot-path synchronous I/O.

Required verification:
- resolve the actual cTrader writable storage contract available to this project;
- replace cwd-dependent paths only if the target cTrader-supported path is confirmed;
- make persistence failure observable in UI/telemetry without adding hot-path I/O;
- preserve AccessRights.None unless documentation and target-terminal testing prove another access level is required;
- ensure a single bounded persistence service owns file writes.

Manual cTrader evidence:
- exact terminal version/build;
- actual resolved path;
- write success/failure under `AccessRights.None`;
- restart persistence.

### CR4.3 — Signal-trace temporal lineage and future-outcome linkage (D3)

Initial review label: **PARTIAL — ordering concern must be reconciled with CR1.4 before implementation.**

Scope:
- `SignalEvaluationTraceRecorder`;
- `ArchiveSignalTrace`;
- `ArchiveRuntimeDecisionTrace`;
- relationship between decision, setup preview and execution model;
- research/backtest outcome linkage.

Required verification:
- prove which canonical closed-M5 state each trace row represents;
- prevent stale `_setupPreview` / `_executionModel` values from being paired with a new decision;
- define a deterministic trace identity for later outcome joining;
- do not add future outcome labels directly into live decision logic;
- research-only linkage may be implemented as an offline/archival join keyed by stable signal identity and closure timestamp.

Performance constraint:
- no per-bar doubling of synchronous file writes; use the existing bounded persistence mechanism.

Implementation closeout — 2026-09-30:
- trace capture was moved out of BuildDecision and bound to the finalized new-closed-M5 calculation boundary;
- a deterministic `CFIP-ST1` trace identity now includes symbol, timeframe, account scope, configuration fingerprint and canonical closed-M5 open time;
- Plan/Preview geometry is accepted only when the source `closedM5`, bar-open timestamp and direction match the current decision;
- SignalTraceId is carried through Plan, aggressive fill plan creation and realized OutcomeObservation/archive rows;
- signal trace archive schema is v3 and remains on the existing buffered persistence owner;
- Outcome archive schema is v2 and remains backward-readable for prior rows;
- the offline analyzer performs research-only exact SignalTraceId joins and does not feed future outcomes into live decision logic;
- deterministic runtime contracts and the dedicated CR4.3 static gate cover identity, lineage, duplicate suppression, archive compatibility and outcome joining;
- the project-wide source/architecture audit, cTrader compile and runtime acceptance all passed on implementation head `7defa4731ebd1e60d87d88a1659d4307607c46ab` (Source/Architecture run 36776384440; cTrader Compile run 36776384275; Runtime Acceptance run 36776384489).

Verification boundary:
- no target-terminal replay/performance/profitability claim is made from CI;
- restart/reconnect timing and empirical signal/outcome quality remain target-terminal acceptance work.

Performance/code-cleanliness:
- no synchronous per-bar archive writes were introduced;
- duplicate trace/helper paths were removed or centralized under one owner;
- public parameters and default thresholds were unchanged.

Status: **COMPLETE — PR #104 merged to main; merge commit `d24b26de3ddf3709c8ea5e94f97a9f533b9b33dc`.**

### CR4.4 — Skender/OSS numerical stability and incremental caching (D4)

Status: **COMPLETE — PR #105 merged to main; merge commit `1b1a1762fee960e65903880f2566b3355a9a7431`.**

Implementation record:
- path-dependent Skender adapters use stable-prefix history: RSI, MACD, SuperTrend and Parabolic SAR;
- fixed-window adapters use a bounded 161-bar incremental quote cache;
- cache invalidation covers Bars identity, cTrader HistoryLoaded/Reloaded and conservative OHLCV fingerprints;
- OSS constants, warm-up contracts and configured-period safety clamps have one authoritative owner;
- OBV remains diagnostic/research data but is no longer counted as independent OSS confluence evidence;
- runtime contracts and a dedicated static audit are green;
- benchmark coverage compares rebuild versus incremental quote materialization.

Verification:
- Source/Architecture PASS — run 36779376240;
- cTrader Compile PASS — run 36779376267;
- Runtime Acceptance PASS — run 36779376210;
- OSS indicator benchmark PASS — run 36779376203.

Safety boundary:
- no public parameter name/type/DefaultValue changed;
- no default trading threshold, RR, confidence or execution policy changed;
- FacioQuo remains research-only;
- target-terminal cTrader timing, memory behavior and empirical signal-quality validation remain manual acceptance boundaries.

### CR4.5 — Per-timeframe regime semantics (D5)

Status: **VERIFIED COMPLETE — PR #106 merged to `main`, merge commit `c529c38ecea09e4b1ad85bc465e1ba12739ff95b`.**

Verification: Source/Architecture run `36781736303`; cTrader Compile run `36781736379`; Runtime Acceptance run `36781736247`.

Implementation preserved the existing M5 regime path, added bounded non-M5 regime caching, exposed frame-owned normalized regime metadata, made `UNKNOWN` explicitly neutral, and kept all regime thresholds/weights unchanged.

Initial review label: **CONFIRMED / MEDIUM — current per-frame regime coverage must be audited.**

Scope:
- `MarketFrameScoringService.ResolveFrameRegime`;
- `IndicatorEvidenceFusionRule.ApplyRegimeWeights`;
- M5/M15/M30/H1/H4/D1/W1 frame semantics.

Required verification:
- determine whether each frame already has a regime source or intentionally inherits M5 context;
- if per-frame regimes are introduced, preserve M5 behavior and use bounded/cached calculations;
- if UNKNOWN is intentional, document the neutral semantics explicitly;
- runtime contract must cover frame-specific regime resolution and BUY/SELL symmetry.

No silent threshold/weight tuning.

### CR4.6 — Frame-scoring constant ownership (D6)

Initial review label: **CONFIRMED / LOW — structural cleanup unless source audit finds a deeper semantic issue.**

Scope:
- direction thresholds;
- conflict penalties;
- RSI exhaustion thresholds;
- event contribution constants.

Required implementation:
- create a named `FrameScoringConstants` owner in Core/;
- preserve current numerical values;
- replace scattered magic numbers with that single owner;
- add deterministic contract coverage.

### CR4.7 — TP pipeline feasibility, rejection telemetry and HTF-age semantics (D7)

Initial review label: **CONFIRMED-COMPUTATION / PARTIAL-BEHAVIOR — requires deterministic replay before any threshold change.**

Scope:
- `TargetCandidateEvaluator`;
- `PlanRewardIntegrityValidator`;
- `TargetSelector`;
- `SmartExtraTargetSource`;
- `HtfTargetSource`;
- risk/target parameter groups;
- server-side TP ladder dependencies.

Required verification:
- quantify how current RR floors and maximum target extension interact with SL ATR;
- measure accepted TP1/TP2/TP3/TP4 rates under current defaults using deterministic fixtures/replay;
- emit per-stage rejection reason counters using the CR3.5 bounded telemetry model;
- separate M5 setup age from HTF target age semantics;
- define HTF age in elapsed time or a dedicated equivalent while preserving current behavior by default;
- identify whether TP3/TP4 unavailability is geometric, HTF-source, age, obstacle or reward-integrity driven.

No default RR/SL/age tuning in this phase. Any behavior-changing correction must be separately documented and explicitly approved.

**CR4.7 closeout — COMPLETE.**  
PR #108 merged to `main` as `2a586ca353f79d6151b9b8375edf46cb94df7880`. Source/Architecture #1999, Runtime Acceptance #1808 and cTrader Compile #1992 all passed on implementation HEAD `441611a8d1ca513ee332f9a8a006a3f37eb9a727`. The phase added deterministic target constraints, bounded stage rejection telemetry, M5-vs-HTF age separation, source-age propagation, reward-envelope feasibility detection, and dedicated owners that keep TargetSelector orchestration-only. No public defaults or trading thresholds were tuned; replay and target-terminal evidence remain required.

### CR4.8 — TP1 directional defensive validation (D8)

Initial review label: **PARTIAL — upstream protection may already exist; defensive invariant should be verified.**

Scope:
- `PlanMaterialization`;
- `TargetSelector`;
- `PlanRewardIntegrityValidator`;
- `TargetProgressionRule`.

Required verification:
- prove TP1 cannot be materialized on the invalid side of Entry;
- if the invariant is not guaranteed at every boundary, add a defensive Core validation;
- add BUY/SELL symmetry tests for wrong-side TP1;
- rejection must be observable without creating a second decision authority.

### CR4.8 closeout — 2026-10-01

Status: **COMPLETE — TP1 directional defensive invariant verified at the relevant repository boundaries.**

Verification:
- the canonical target-side owner is `PriceProtectionRule.ValidateTarget`;
- candidate constraint evaluation delegates wrong-side rejection to that owner;
- plan materialization fails closed for invalid-direction TP1;
- plan protection and reward-integrity boundaries independently defend TP1 direction;
- deterministic BUY/SELL valid and wrong-side TP1 fixtures are present;
- `TargetProgressionRule` BUY/SELL symmetry remains explicitly covered;
- the accumulated Source/Architecture workflow now includes `audit_phase_4_8.py`.

Safety:
- no public parameter, RR floor, confidence threshold, stop policy or execution policy changed;
- no second decision/execution authority introduced;
- target-terminal and broker runtime behavior remain manual acceptance boundaries.

### CR4.9 — Live reversal action/alert semantics (D9)

Initial review label: **CONFIRMED / PARTIAL — alert ordering is likely valid as described; direction-score and clamp claims require exact-source verification.**

Scope:
- `LiveReversalAnalyzer`;
- reversal alert emission;
- Auto Trading state presentation;
- directional reversal scoring;
- hidden minimum clamp;
- outcome/lifecycle coupling.

Required verification:
- distinguish "reversal detected" from "reversal close executed";
- remove repeated no-op active-plan alerts or gate them by one reversal episode/cooldown;
- do not label Auto Trading globally BLOCKED when the actual action is "position retained";
- verify whether reversal quality must use opposite-direction evidence;
- identify and remove only hidden clamps that violate the public-parameter contract; never tune the public default silently;
- verify no-position outcome behavior against PositionClosed/OutcomeTelemetry paths.

### CR4.9 / D9 closeout — 2026-10-01

Status: **COMPLETE — repository verification PASS.**

Completed:
- centralized live-reversal direction, directional confidence and action semantics in Core;
- prevented same-direction reaction/frame evidence from inflating reversal confidence;
- bounded reversal detection alerts to a position/direction episode;
- reset reversal-episode state on confirmed `PositionClosed`;
- separated retained/rejected/requested/confirmed semantics in Auto Trading state;
- prevented the reversal detector from synthesizing a closed lifecycle or outcome when the managed position is temporarily absent;
- centralized existing live-reversal parameter bounds without changing public names, types or defaults;
- added deterministic D9 contracts and `audit_phase_4_9.py` to the accumulated Source/Architecture workflow.

Safety:
- no public parameter name/type/DefaultValue changed;
- no default RR/confidence/stop/target or execution threshold tuned;
- no second decision/execution authority introduced;
- final closure and outcome remain broker/lifecycle authoritative.

Repository verification:
- Source/Architecture, Runtime Acceptance and cTrader Compile are pending PR check evidence at phase closeout;
- target-terminal broker acknowledgement, restart/reconnect and empirical validation remain manual.

Next phase: **CR4.10 / D10 — Native-indicator defensive safety and registry performance.**

### CR4.10 — Native-indicator defensive safety and registry performance (D10)

Status: **VERIFIED COMPLETE — PR #111 merged to `main`; merge commit `3371b9790902c4d35e4e1e28522dee42f93af861`.**

Repository evidence:
- Source/Architecture PASS — workflow run `36790307897`;
- Runtime Acceptance Contracts PASS — workflow run `36790307911`;
- cTrader Compile PASS — workflow run `36790307893`;
- OSS/Registry Benchmark PASS — workflow run `36790307937`.


Reconciled findings:
- repository-wide consumer inventory isolates direct native wrapper/registry access and the expected analysis consumers;
- ATR/EMA/ADX/DMI already fail closed on unusable numerical input;
- RSI neutral fallback `50` is compatibility-preserved, but incomplete native frames are now blocked before scoring;
- MACD prior-sample access now has explicit warm-up readiness.

Implemented:
- centralized Core `NativeIndicatorReadinessRule`;
- wrapper, MarketFrame and MarketRegime defensive readiness boundaries;
- reference-identity `Dictionary<Bars, Native>` registry;
- deterministic runtime readiness contracts;
- platform-neutral registry lookup benchmark;
- `audit_phase_4_10.py` wired after CR4.9.

Safety:
- no public parameter identity or default trading threshold changed;
- no second decision/execution authority introduced;
- target-terminal/broker runtime and empirical signal-quality validation remain manual.

Next transition: **CR-FINAL repository integration gate.**

### Prompt 4 completion gate

CR4.1 → CR4.2 → CR4.3 → CR4.4 → CR4.5 → CR4.6 → CR4.7 → CR4.8 → CR4.9 → CR4.10 → **CR-FINAL**

CR-FINAL cannot be considered complete while any D-item remains unverified, deferred without an explicit reason, or blocked by a missing target-terminal test.


## 7.1 — Prompt 5 Remediation Track: Stop Caps, Target Sources, Evidence Independence, Pending Fills and Parallel Scenarios

Status: **ADDED TO REMEDIATION PROGRAM — IMPLEMENTATION PENDING**

Prompt 5 is now a mandatory follow-on review track after Prompt 4. Its findings remain static-review hypotheses until each item is independently reconciled against current main source, contracts, replay evidence and target-terminal behavior where required.

Authoritative order:

`CR5.1 → CR5.2 → CR5.3 → CR5.4 → CR5.5 → CR5.6 → CR5.7 → CR5.8 → CR-FINAL`

### CR5.1 — Effective maximum structural stop risk and duplicate ceiling removal (E1)

Status: **VERIFIED COMPLETE — PR #114 merged to `main`; merge commit `62a119bef79e5a978f1f2ed66a913fa11a1bb2d4`.**

Repository evidence:
- Source/Architecture PASS — PR head run `36791329486`; merge run `36791455294`;
- Runtime Acceptance PASS — PR head run `36791329385`; merge run `36791455156`;
- cTrader Compile PASS — PR head run `36791329369`; merge run `36791455288`.

The existing effective ceiling formula was preserved exactly; all identified dual-cap
consumers now use the canonical Core owner and the deterministic E1 truth table
covers cap ordering, minimum-floor behavior and symmetry.

Next transition: **CR5.2 / E2.**

Scope:
- `StructuralStopCandidateEvaluator`;
- `PlanInputPreparation`;
- `PlanIntegrityValidator`;
- `ParallelOpportunityBuilder`;
- `TradeActionabilityEvaluator`;
- `AutomaticMarketSubmissionValidator`;
- `AggressiveFinalExecutionGuard`;
- `PendingSubmissionValidator`;
- `SignalEvaluationTraceRecorder`;
- all other structural-stop ceiling consumers.

Required verification:
- prove the current effective maximum-stop formula and all duplicate call sites;
- create one canonical `EffectiveMaximumStopRiskAtr()` owner if the duplicate rule is confirmed;
- preserve current numerical behavior by default;
- clarify the distinct meaning of `MaximumSlAtr` and `MaximumStructuralStopAtr` without silently changing either parameter's default semantics;
- reject over-ceiling structural-stop candidates as early as safely possible.

Testing:
- deterministic truth table covering Min/Maximum SL ATR and Maximum Structural Stop ATR combinations;
- BUY/SELL symmetry;
- candidate early-rejection behavior.

### CR5.2 — Liquidity/session target-source semantics and multi-level target candidates (E2)

Status: **VERIFIED COMPLETE — PR #116 merged to `main`; merge commit `10e01bd2610ce0c42b6d365f55fae24c75a3edfb`.**

Repository evidence:
- Source/Architecture: PASS — PR #116 head run `36792555340` / workflow #2072, including `audit_phase_5_2.py` and the accumulated routine/optimization audits;
- Runtime Acceptance Contracts: PASS — PR #116 head run `36792555225` / workflow #1881;
- cTrader Compile: PASS — PR #116 head run `36792555189` / workflow #2065.

Implementation/safety:
- canonical swing highs/lows replace raw candle-extreme liquidity forecasts;
- active/unbroken liquidity semantics are enforced before target admission;
- multiple valid liquidity forecasts are distance-ordered and separated with the existing `MinimumTpSpacingAtr`;
- session forecasts preserve the canonical `SessionWindowRule` and existing UTC parameter semantics;
- no public parameter/default, RR/confidence/stop/target threshold or decision/execution authority changed.

Manual boundary remains:
- target-terminal timing/readiness/panel behavior;
- broker lifecycle and restart/reconnect;
- empirical signal-quality/profitability.

**Next transition: CR5.3 / E3 — Independent-evidence group counting for parallel opportunities.**

Scope:
- `LiquidityAboveTargetSource`;
- `LiquidityBelowTargetSource`;
- `SmartExtraTargetSource`;
- `SESSION_FORECAST`;
- target candidate scoring/reward filtering.

Required verification:
- determine whether current liquidity candidates are true swing/pool/unbroken-liquidity references or merely nearest candle extremes;
- ensure multiple valid farther liquidity levels can be represented when the source intends to provide a liquidity forecast;
- apply ATR-bounded minimum-distance discrimination without breaking existing valid targets;
- verify session forecast semantics and whether its current window intentionally represents the existing default behavior;
- do not replace the existing session window with named Asia/London/New York sessions without explicit evidence/approval for a default behavior change.

Testing:
- near/far High/Low fixtures;
- equal-high/equal-low and broken/unbroken level cases;
- ordering by distance;
- target-candidate survival through RR/reward checks.

### CR5.3 — Independent-evidence group counting for parallel opportunities (E3)

Status: **VERIFIED COMPLETE — PR #119 merged to `main`; merge commit `96530088a4216eb4a3f8caae9595987d98c0a27e`.**

Repository evidence:
- Source/Architecture: PASS — run `36793867203` / workflow #2083, including `audit_phase_5_3.py` and accumulated routine/optimization audits;
- Runtime Acceptance Contracts: PASS — run `36793867170` / workflow #1892;
- cTrader Compile: PASS — run `36793867168` / workflow #2076.

Implementation/safety:
- centralized established independent-evidence score and independent-family group counting in Core `IndependentEvidenceFusionRule`;
- four families are Structural, Location, Trend-Momentum and Context;
- correlated observations inside one family count as one independent group;
- `Decision` and `TradeOpportunityCandidate` expose group-count provenance separately from the unchanged 0–8 behavior-driving score;
- duplicate Analysis-layer `IndependentEvidenceFusionCalculator` removed;
- no public parameter/default, RR/confidence/stop/target/actionability/execution threshold or decision/execution authority changed.

Manual boundary remains:
- target-terminal timing/readiness/panel behavior;
- broker lifecycle and restart/reconnect;
- empirical signal-quality/profitability.

**Next transition: CR5.4 / E4 — Pending-order post-fill absolute SL/TP reconciliation.**

Scope:
- `ScenarioExecutionPolicy.EnrichScenarioEvidence`;
- `TradeOpportunityCandidate.IndependentEvidenceScore`;
- all presentation/decision consumers of the score.

Required verification:
- inventory all consumers before changing semantics or field names;
- group evidence into structurally meaningful families such as structure, momentum, volume and position/context;
- verify how many distinct groups are actually represented in current candidates;
- keep public parameter names/types/defaults unchanged;
- if the persisted field name remains misleading, rename only with complete caller/schema migration and compatibility evidence.

Testing:
- three correlated structure flags must contribute one structural group, not three independent confirmations;
- BUY/SELL symmetry;
- no loss of canonical evidence provenance.

### CR5.4 — Pending-order post-fill absolute SL/TP reconciliation (E4)

Status: **VERIFIED COMPLETE — PR #120 merged to `main`; merge commit `782bca41cd37071c79f2cdfa12f712cefc045c8c`.**

Repository evidence on final implementation head `6d89f010fa6d292d201ef79e37af5b162438f854`:
- Source/Architecture: PASS — run `36795710379` / workflow #2096, including `audit_phase_5_4.py` and accumulated routine/optimization audits;
- Runtime Acceptance Contracts: PASS — run `36795710374` / workflow #1905;
- cTrader Compile: PASS — run `36795710377` / workflow #2089.

Implementation/safety:
- pending Stop/Limit orders now preserve their absolute Entry/SL/TP intent across placement;
- snapshot construction is fail-closed and is based on `ExecutionIntent`;
- actual broker fill is reconciled against the preserved absolute plan before managed-plan adoption;
- more-protective broker SL and more-progressive broker TP are retained;
- Advanced/server-side TP protection is rebuilt from the reconciled absolute ladder and actual fill;
- `PositionOpened` event-order inversion cannot bypass an outstanding pending snapshot;
- cancellation clears stale snapshot state;
- failed reconciliation remains `RecoveryRequired`;
- the accumulated CR2.5 lifecycle-ordering audit was updated to match the new pending-snapshot invariant;
- no public parameter/default, RR/confidence/stop/target/actionability/execution threshold or decision/execution authority changed.

Manual boundary remains:
- actual Stop/Limit fill-price divergence;
- broker-side final SL/TP and Advanced Protection ladder behavior;
- rejection timing;
- restart/reconnect;
- empirical signal-quality/profitability.

**Next transition: CR5.5 / E5 — Parallel-scenario computation/candidate ownership and MicroReaction safety.**

Scope:
- `BrokerPendingOrderPlacement`;
- `BrokerLimitOrderPlacement`;
- pending Stop/Limit fill adoption;
- `LiveFillExitReconciler`;
- any pending-specific post-fill target/stop resolver;
- broker-confirmed managed-plan adoption.

Required verification:
- prove whether RelativeStopLossProtection / RelativeTakeProfitProtection remains authoritative after a pending order is filled away from the planned entry price;
- trace the complete pending fill lifecycle to final broker-confirmed SL/TP;
- if absolute plan levels are not restored after fill, add one canonical post-fill reconciliation path rather than a second execution authority;
- preserve broker-confirmed state as authoritative and never assume accepted pending placement equals a position.

Testing:
- deterministic mock fills with positive and negative slippage/gap;
- final absolute SL/TP must match the canonical adopted plan where the strategy contract requires structural levels;
- rejection/retry/idempotency behavior.

Manual cTrader:
- actual Stop/Limit fill-price divergence;
- broker-side final protection levels after fill.

### CR5.5 — Parallel-scenario computation/candidate ownership and MicroReaction safety (E5)

Initial review label: **CONFIRMED + PARTIAL — duplicated scenario computation is confirmed; MicroReaction and candidate-replacement semantics require exact-source verification.**

Scope:
- `ParallelOpportunityBuilder`;
- Strategic/Tactical/MicroReaction lanes;
- `BuildExecutionModel`;
- `BuildTradeSetupPreview`;
- shared target/stop candidate calculations;
- `AddTimeframeScenarioCandidates`;
- `ScenarioExecutionPolicy.IsCanonicalCandidateEligible`;
- scenario identity/merge selection.

Required verification:
- identify shared calculations that can be computed once and reused across scenario lanes without changing ownership;
- prevent MicroReaction from becoming an automatic countertrend engine on unfinished candles;
- require the appropriate closed-bar confirmation before execution eligibility;
- verify same-`ScenarioId` replacement rules and whether replacement is based on quality/priority as intended rather than recency alone;
- preserve simultaneous scenario support when candidates are genuinely distinct.

Testing:
- one shared fixture should prove cache reuse across lanes;
- unfinished-candle MicroReaction is not executable;
- closed-bar confirmation restores the intended eligibility;
- stronger existing candidate is not replaced merely because a weaker candidate is newer;
- simultaneous distinct scenarios remain visible.

Performance:
- benchmark scenario build cost before/after;
- avoid per-lane duplicate expensive target/stop discovery.

### CR5.6 — Directional-bias semantics and timeframe consistency (E6)

Initial review label: **DESIGN RISK / LOW–MEDIUM — semantic coupling must be verified before any weighting change.**

Scope:
- `PremiumDiscountAnalyzer`;
- `LiveBiasAnalyzer`;
- `HealthyVolatilityAnalyzer`;
- all consumers of these flags;
- M5 closed-bar authority.

Required verification:
- determine whether premium/discount is deliberately used as a mean-reversion/context signal or is unintentionally fighting trend regimes;
- verify the exact parameter/default controlling its participation before any regime weighting change;
- prove whether `LiveBias` is actually M5-based or chart-timeframe based;
- migrate the live-bias read to the canonical closed M5 source only if the current contract is contradicted;
- separate volatility-health from trigger-body confirmation so `MinimumTriggerBodyAtr` cannot silently redefine volatility health, unless source/contract evidence shows that coupling is intentional.

Testing:
- M5, M15 and H1 chart fixtures;
- open-bar vs closed-M5 behavior;
- trend/premium and mean-reversion cases;
- volatility-health independence from trigger-body threshold.

### CR5.6 / E6 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #122**

The E6 directional-bias/timeframe remediation is closed:
- Premium/Discount keeps its established midpoint mean-reversion/context semantics and +6 participation;
- LiveBias is now canonical closed-M5 based rather than host-chart based;
- Healthy Volatility uses ATR health only, with trigger-body confirmation remaining in entry/trigger semantics;
- public parameter/default values and thresholds are preserved;
- deterministic E6 runtime contracts and static audit are wired;
- Source/Architecture, Runtime Acceptance Contracts and cTrader Compile all passed on implementation head e930e30d9c82ad279316a41c81116d4ae1e19859.

Manual target-terminal timing, panel behavior and empirical signal-quality validation remain outside CI claims.

### CR5.7 — Decision-owned WATCH/REACTION alerts separated from chart rendering (E7)

Initial review label: **PARTIAL / MEDIUM — exact render/alert coupling and fixed thresholds must be source-audited.**

Scope:
- `UI/Chart/SignalRenderer.RenderWatchAndReaction`;
- canonical decision/alert emission boundary;
- popup/audio delivery path;
- `ShowSignalArrow` and `PanelRenderOptimization` interactions.

Required verification:
- prove whether WATCH/REACTION alerts can disappear solely because rendering is disabled or skipped;
- move alert qualification/emission to the decision/alert owner when confirmed, leaving the renderer presentation-only;
- centralize or name the current fixed threshold offsets (60 and 4) without changing their values by default;
- preserve blocked-signal silence for marks/audio where that is already an invariant.

Testing:
- rendering enabled/disabled produces the same qualifying alert decision;
- blocked signal produces no alert;
- repeated closed-bar alert identity remains deterministic and bounded;
- BUY/SELL symmetry.

### CR5.7 / E7 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #123 merged to `main`; merge commit `d37f6d575595bfacecdff5a5ffb8fc44ba96455a`.**

Source audit finding:
- the previous WATCH alert emission lived inside chart rendering and its intended early-WATCH condition was unreachable after the renderer returned on `!ActionableNow`;
- REACTION alert emission was likewise coupled to renderer execution and `ShowReactionArrow` presentation state.

Implementation:
- added Core `WatchReactionAlertRule` as the single owner for WATCH/REACTION qualification, the established early-WATCH confidence floor/gap (60/4), and deterministic alert identity;
- moved WATCH/REACTION alert qualification and emission into the runtime decision-alert boundary;
- preserved intrabar REACTION cadence by evaluating alerts after `UpdateLiveReaction` and before presentation;
- made `SignalRenderer` presentation-only for WATCH/REACTION alerts;
- reused the Core WATCH rule from presentation instead of duplicating its threshold logic;
- added deterministic Runtime Contract coverage for threshold semantics, blocked/plan/pending/live guards, BUY/SELL symmetry and alert identity;
- added `tools/audit_phase_5_7.py` immediately after E6 in the accumulated Source/Architecture gate;
- recorded the implementation and safety boundary in `docs/PHASE-CR5-7-WATCH-REACTION-ALERTS.md`.

Safety:
- no public parameter name/type/`DefaultValue` changed;
- no RR/confidence/SL/TP/execution threshold was tuned;
- blocked signals remain fail-closed at the canonical alert eligibility boundary;
- target-terminal alert timing, popup/audio delivery, chart/panel behavior, broker lifecycle and empirical signal quality remain manual boundaries.

Repository verification:
- Source/Architecture PASS — run `36841785497` / job `110302351624`;
- Runtime Acceptance Contracts PASS — run `36841785708` / job `110302353199`;
- cTrader Compile PASS — run `36841785507` / job `110302351749`;
- PR #123 merged to `main` with final implementation head `8d39ad3fb88c75092908121a3cbaa65128f47659`.
- target-terminal rendering-disabled versus enabled alert equivalence;
- repeated closed-bar identity/dedup behavior;
- target-terminal intrabar REACTION timing.

**Next phase: CR5.8 / E8 — Small constant ownership and TargetSelection consistency.**

### CR5.8 — Small constant ownership and TargetSelection consistency (E8)

Initial review label: **CONFIRMED/PARTIAL — structural cleanup plus targeted semantic tests.**

Status: **VERIFIED COMPLETE — PR #124 merged to `main` on 2026-10-01.**

Scope:
- `PlanRewardRiskQualityRule` internal adaptive-RR constants;
- `TargetSelectionPolicy.BuildTargetSelectionRequiredRR`;
- all `TargetSelector` callers (`PlanBuilder`, `ExecutionPlanPreparation`, `LivePlanTargetEnrichment`, `LiveFillExitReconciler`, `EarlyPredictionEngine`, `PlanPreviewBuilder`, and any current seventh caller);
- lane/required-RR propagation.

Required verification:
- name and centralize the internal constants without changing values;
- prove required RR for TP1..TP4 is monotonically non-decreasing for every lane and current/default parameter combination;
- verify Tactical minimum RR cannot invert TP2/TP3/TP4 ordering;
- prove all TargetSelector callers pass the same canonical lane and required-RR contract for the same plan context;
- reconcile any caller that intentionally uses a distinct lane as an explicit contract, not accidental divergence.

Testing:
- property-style/order fixtures for TP1..TP4;
- lane matrix;
- caller consistency contract;
- BUY/SELL symmetry.


Verification:
- Source/Architecture PASS — run 36844545898;
- Runtime Acceptance Contracts PASS — run 36844545976;
- cTrader Compile PASS — run 36844546002;
- merge commit: `03a569d6a18f1b6cbc3524dabc24ea713439c325`;
- final implementation head: `51e1f2bc9ecdd12bc8a366630fb225a4fa2c5593`.

### Prompt 5 completion gate

CR5.1 → CR5.2 → CR5.3 → CR5.4 → CR5.5 → CR5.6 → CR5.7 → CR5.8 → **CR-FINAL**

CR-FINAL cannot be considered complete while any E-item remains unverified, deferred without an explicit reason, or blocked by a missing target-terminal/replay test.


## 7.2 — Prompt 6 Remediation Track: Target-Path Obstacles, Aggressive Risk Guards, Hidden Thresholds, Timeframe Scenarios and Orphan Protection

Status: **ADDED TO REMEDIATION PROGRAM — IMPLEMENTATION PENDING**

Prompt 6 is now a mandatory follow-on review track after Prompt 5 and before CR-FINAL. All F1–F9 findings remain static-review hypotheses until independently reconciled against current main source, deterministic contracts/replay, and target-terminal behavior where required.

Authoritative order:

`CR6.1 → CR6.2 → CR6.3 → CR6.4 → CR6.5 → CR6.6 → CR6.7 → CR6.8 → CR6.9 → CR-FINAL`

### CR6.1 — Opposing FVG/OB path-obstacle direction, mitigation and caching (F1)

Initial review label: **LIKELY CONFIRMED / HIGH — exact source and existing FVG mitigation semantics must be re-verified.**

Scope:
- `RewardPathZoneObstacleScanner`;
- `RewardPathGeometryRule`;
- `HigherTfRewardPathValidator`;
- `TargetCandidateEvaluator`;
- `FvgMitigationEvaluator`;
- M15/M30/H1/H4 obstacle scans.

Required verification:
- prove that opposing-zone semantics use `-direction` consistently for both FVG and Order Block;
- verify bullish/bearish FVG orientation against canonical `FvgRule`;
- exclude mitigated FVGs using the existing canonical mitigation owner;
- cache obstacle-scan results per (timeframe, closed index, direction) so the same path is not rescanned for each target/stage;
- preserve obstacle-owner separation and BUY/SELL symmetry.

Testing:
- BUY with bearish FVG obstacle;
- BUY with bullish FVG that is not an opposing obstacle;
- mitigated FVG excluded;
- SELL mirror cases;
- repeated target evaluation reuses the same obstacle result.

### CR6.2 — Aggressive execution minimum-RR/risk guard and fill-plan consistency (F2)

Initial review label: **CONFIRMED / HIGH for the missing pre-trade RR/risk path; other fill/lifecycle claims require source verification.**

Scope:
- `AggressiveFinalExecutionGuard`;
- `AggressivePreTradeEligibility`;
- `AggressiveExecutionPreparation`;
- `AutoPlanRiskValidator`;
- accepted-fill reconciliation and managed-plan adoption.

Required verification:
- prove whether the final aggressive guard's current reward-quality check is unreachable because the pre-trade path requires `_plan == null`;
- add a pre-submission RR and stop-risk validation using the existing canonical `PlanRewardRiskQualityRule` only if confirmed;
- do not add/tune a new public minimum-RR parameter without explicit approval; report a proposed parameter/default separately;
- enforce reaction/decision direction consistency at the final guard unless the existing explicit policy parameter intentionally permits divergence;
- remove/fix any dead `_plan != null` path;
- verify actual-fill versus pre-fill entry/SL/TP reconciliation and lifecycle cleanup after close.

Testing:
- sub-minimum RR rejected;
- stop risk above effective ceiling rejected;
- opposite reaction/decision direction rejected under the applicable policy;
- accepted fill reconciles to the canonical managed plan;
- post-close cleanup is deterministic.

Manual cTrader:
- actual aggressive fill-price divergence and broker protection behavior.

### CR6.3 — Effective-threshold transparency and hidden additive margins (F4)

Initial review label: **CONFIRMED / HIGH — parameter truth is the primary invariant; exact consumer ordering must be audited.**

Scope:
- `ActionableSignalQualityGate.EvaluateFinalActionableSignalQuality`;
- `ParallelOpportunityBuilder.ShouldPresentOpportunityCandidate`;
- `TradeActionabilityEvaluator`;
- panel/diagnostic exposure of effective thresholds.

Required verification:
- inventory every hidden addition/clamp: `+4`, `+3`, `+1`, fixed 75/70 floors, 70 entry-location clamp and any related constants;
- determine whether `ActionableNow` shown in the panel/chart is before or after the final gate;
- preserve current effective defaults until evidence supports a behavior change;
- if the hidden margins are retained, give them explicit named ownership and expose effective thresholds to diagnostics/panel;
- only introduce new parameters when necessary, with current effective values as defaults;
- reconcile 64/64 upstream thresholds with 75/70/70 final thresholds and document which layer is authoritative.

Testing:
- deterministic input/output table for each threshold layer;
- exact effective threshold calculation;
- parameter-truth regression cases;
- BUY/SELL symmetry.

### CR6.4 — Smart-threshold regime identity and hidden REVERSAL dead path (F5)

Initial review label: **CONFIRMED / LOW — dead regime branch and literal drift should be verified.**

Scope:
- `SmartThresholdPolicy.GetAdaptiveSmartThresholds`;
- `MarketRegimeClassifier`;
- all regime string consumers.

Required verification:
- enumerate every regime value the classifier can emit;
- prove which adaptive-threshold branch each value takes;
- eliminate unreachable `REVERSAL` logic or connect it only if the canonical classifier actually gains that state;
- centralize regime identity via an enum or named constants without introducing a second regime authority;
- preserve current threshold behavior unless a separate evidence-backed tuning phase is approved.

Testing:
- every possible classifier output has a deterministic threshold-policy result;
- unknown/future values fail safely;
- BUY/SELL symmetry.

### CR6.4 / F5 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — repository implementation completed without threshold tuning.**

Reconciled:
- the canonical classifier has six market regimes plus UNKNOWN; REVERSAL is not a classifier state;
- MarketRegimeIdentity now centralizes regime identifiers and normalization;
- Smart Threshold adaptation is platform-neutral and consumes the same identity vocabulary;
- the dead REVERSAL Smart Threshold branch is removed;
- unknown/future regime values fail safely to the base threshold path;
- reviewed regime consumers were migrated to canonical identities;
- deterministic contracts cover all six regimes and current numeric behavior;
- the accumulated Source/Architecture gate includes F5.

Safety:
- no public parameter name/type/DefaultValue changed;
- no default smart/regime threshold, RR, confidence or execution policy changed;
- no second decision or regime authority introduced.

Verification boundary:
- repository CI supplies source/compile/contract evidence;
- target-terminal runtime and empirical signal-quality validation remain manual.

Next phase: **CR6.5 / F6 — Trap-risk/trigger exceptions and actionability constant ownership.**
### CR6.5 — Trap-risk/trigger exceptions and actionability constant ownership (F6)

Initial review label: **DESIGN RISK + PARTIAL SEMANTIC FINDING — must distinguish intentional breakout policy from accidental bypass.**

Scope:
- `TradeActionabilityEvaluator`;
- `EntryTrapRiskRule`;
- `IndicatorActionabilityRule`;
- Breakout/Retest/Pending/Aggressive eligibility consumers.

Required verification:
- determine whether Breakout intentionally bypasses trap blocking and whether the existing parameter set expresses that policy;
- verify whether Retest can become actionable before its required trigger and whether downstream plan/entry gates independently enforce the trigger;
- centralize/name hard-coded actionability, divergence and range constants without changing values;
- verify `actualEntry`, anchor selection and late-entry ATR semantics for Breakout versus Retest.

Behavior rule:
- any change that makes Breakout subject to trap blocking or changes trigger requirements is behavior-changing and must be separately reported/approved; do not silently apply it.

Testing:
- Breakout trap-risk case;
- Retest inside-zone without trigger;
- trigger-ready case;
- anchor/late-entry symmetry;
- parameter/default preservation.

### CR6.5 / F6 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #131 head `01db0f46f2a19597be5428a62a230ebf67a7f36c`.**

تأیید می‌کنم — CR6.5/F6 با ممیزی مستقل مسیر فعلی و بدون تغییر در قرارداد عمومی پارامترها یا tuning عددی تکمیل شد.

Completed:
- ایجاد مالک Core واحد `EntryActionabilityPolicy` برای ثابت‌ها و semantics مربوط به trap-risk، range-location، divergence، adverse momentum، micro-conflict، trigger tolerance، anchor و late-entry؛
- مهاجرت `EntryTrapRiskRule`, `TradeActionabilityEvaluator`, `ExecutionModeResolver` و `TriggerGate` به owner واحد، بدون تغییر مقادیر مؤثر؛
- انتقال آستانه‌های actionability مربوط به indicator fusion به `ActionabilityThresholdPolicy` و canonicalization هویت‌های regime؛
- تأیید اینکه Breakout به‌صورت آگاهانه trap-risk block را bypass می‌کند و این سیاست در F6 به رفتار جدید تبدیل نشده است؛
- تأیید اینکه Retest می‌تواند در resolver محلی، در-zone و پیش از trigger دیده شود، اما مسیر canonical plan همچنان `Decision.TriggerReady` را enforce می‌کند؛
- تأیید تفاوت anchor، actual-entry و late-entry بین Breakout و Retest و حفظ trigger tolerance موجود؛
- افزودن Runtime Acceptance Contract و `audit_phase_6_5.py` به زنجیره audit انباشته؛
- اصلاح مستقل چند خطای verification که در حین CI آشکار شد: duplicate helper names، missing Decision.Contracts include، و دو assertion/audit اشتباه در F6.

Verification:
- Source/Architecture: **PASS** — run `36856702821`، شامل `audit_phase_6_5.py` و auditهای انباشته؛
- Runtime Acceptance Contracts: **PASS** — run `36856702812`؛
- cTrader Compile/Build: **PASS** — run `36856702767`.

Safety/manual boundary:
- هیچ `[Parameter]` name/type/`DefaultValue` تغییر نکرد؛
- هیچ RR/confidence/SL/TP یا execution threshold برای tuning تغییر نکرد؛
- هیچ decision یا execution authority جدید ایجاد نشد؛
- Breakout trap bypass عمداً حفظ شد؛
- trigger contract و Retest semantics عمداً حفظ شد؛
- target-terminal timing، panel/chart presentation، broker lifecycle، restart/reconnect و empirical signal-quality/profitability همچنان manual acceptance هستند.

**Next phase: CR6.6 / F7 — Independent-timeframe scenario semantics and duplicate-policy owners.**

### CR6.6 — Independent-timeframe scenario semantics and duplicate-policy owners (F7)

Initial review label: **CONFIRMED / MEDIUM — current candidates appear to reuse the M5 plan; exact source must be audited before semantic reclassification.**

Scope:
- `TimeframeScenarioBuilder.AddTimeframeScenarioCandidates`;
- `ParallelOpportunityBuilder`;
- `ScenarioExecutionPolicy` in both Analysis and Trading namespaces;
- scenario execution reason/presentation.

Required verification:
- prove whether M15/M30/H1/H4/D1/W1 candidates use only M5 Entry/Stop/TP data plus a timeframe label/quality;
- either make them genuine timeframe-specific scenarios or explicitly reclassify them as timeframe annotations over the shared M5 plan, without silently changing behavior;
- identify and unify the two scenario execution-policy authorities;
- ensure display reason cannot contradict the actual execution eligibility policy;
- avoid repeated full plan construction when candidates are annotations of one canonical plan.

Testing:
- same-direction frames do not cause unnecessary duplicate full plan builds;
- genuinely distinct frame inputs produce genuinely distinct scenarios;
- execution policy and display reason remain consistent;
- simultaneous distinct scenarios remain supported.

### CR6.6 / F7 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — independent timeframe candidates are explicitly classified as observations over the canonical M5 plan geometry, and scenario execution policy now has one owner.**

Implementation:
- M15/M30/H1/H4/D1/W1 candidates retain their timeframe identity and evidence, but explicitly declare BasePlanTimeframe = "M5";
- Entry/Stop/TP geometry for timeframe annotations continues to come from the shared closed-M5 parallel scenario path; no false claim of independent timeframe plans was introduced;
- same closed-M5/lane/direction preview construction is cached and reused, avoiding repeated full target/plan-preview work for same-base timeframe annotations;
- the former Analysis and Trading ScenarioExecutionPolicy owners were removed;
- Core ScenarioExecutionPolicyRule now owns candidate eligibility and execution authorization together;
- independent timeframe scenarios remain structurally evaluable but are explicitly OBSERVE-ONLY TF SCENARIO;
- display stage/reason consumes the same policy result used for execution authorization;
- deterministic F7 runtime contracts and accumulated static audit are wired.

Safety:
- no public parameter name/type/DefaultValue changed;
- no RR/confidence/SL/TP/actionability/execution threshold was tuned;
- no second decision or execution authority introduced;
- no separate broker execution path introduced.

Verification boundary:
- repository source/architecture and runtime contract evidence is deterministic once CI runs;
- cTrader target-terminal timing, live scenario presentation, broker lifecycle, replay and empirical signal-quality remain manual.

**Next phase: CR6.7 / F8 — Target-obstacle rejection telemetry and distant-target semantics.**

### CR6.7 — Target-obstacle rejection telemetry and distant-target semantics (F8)

Initial review label: **PARTIAL / MEDIUM — likely interaction with the existing D7 target pipeline; quantify before tuning.**

Scope:
- `TargetObstacleValidator`;
- `TargetCandidateEvaluator`;
- CR3.5 bounded rejection telemetry;
- M5 swing/equality/zone/HTF obstacle categories.

Required verification:
- determine whether ordinary M5 swings over-reject distant targets under current defaults;
- add bounded rejection reasons such as `OBSTACLE_SWING`, `OBSTACLE_EQ`, `OBSTACLE_ZONE`, `OBSTACLE_HTF_ZONE` if the current telemetry model does not already distinguish them;
- for HTF targets, verify whether obstacle magnitude/depth is evaluated in an appropriate timeframe before any change;
- preserve current defaults until replay evidence supports target-survival changes.

Testing:
- near versus distant targets;
- ordinary versus deep/major swings;
- zone/equality/HTF obstacles;
- rejection telemetry counts and dedupe.

### CR6.7 / F8 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — target-obstacle rejection telemetry now distinguishes swing/equality/zone classes and quantifies target distance without changing target-selection defaults.**

Implementation:
- M5 target-obstacle evaluation now returns a structured observation while preserving the existing boolean rejection behavior;
- ordinary swing obstruction remains `OBSTACLE_SWING`;
- equal-high/low liquidity obstruction is now separated as `OBSTACLE_EQ`;
- opposing-zone and HTF-zone categories remain distinct as `OBSTACLE_OPPOSING_ZONE` and `OBSTACLE_HTF_ZONE`;
- obstacle rejection telemetry aggregates target distance in ATR, target position as a percentage of the existing Maximum Target Extension ATR envelope, and known obstacle distance in ATR;
- zone/HTF telemetry records target-distance evidence without fabricating a precise obstacle depth;
- telemetry storage is bounded per closed M5 and deduplicated per target stage/reason using the existing PLAN_TARGET channel;
- deterministic F8 runtime contracts and the accumulated static audit are wired.

Finding:
- the current defaults already reject any qualifying M5 swing/equality strictly between Entry and Target minus the existing clearance; therefore farther valid targets can accumulate more obstacle rejections simply because they expose more path, but this phase does **not** retune or exempt distant targets;
- the new telemetry provides the evidence needed to quantify that relationship during replay before any future behavioral change.

Safety:
- no public parameter name/type/DefaultValue changed;
- no MaximumTargetExtensionAtr, TargetClearanceAtr, RejectTargetObstacle, RR, SL/TP or scoring threshold changed;
- no alternate target-selection authority introduced;
- no broker mutation path introduced.

Verification boundary:
- repository source, compile and runtime contract evidence is deterministic once CI passes;
- real-market obstacle frequency, replay distribution and target-survival conclusions remain evidence/manual work.

**Next phase: CR6.8 / F9 — Target-obstacle scan performance and cache reuse.**

### CR6.8 — Target-obstacle scan performance and cache reuse (F9)

Initial review label: **CONFIRMED / MEDIUM — repeated obstacle work is a hot-path optimization target; benchmark before/after.**

Scope:
- `SelectTargets`;
- `HasTargetObstacle`;
- `HasOpposingZonePathObstacle`;
- `HasHigherTfZonePathObstacle`;
- all TargetSelector callers identified by Prompt 5/E8.

Required verification:
- inventory every TargetSelector/SelectTargets call in current main;
- create a bounded obstacle-cache keyed by timeframe, closed index, direction and any other input that materially changes the scan;
- ensure cache invalidation follows history reload/new closed bar boundaries;
- benchmark computation count, elapsed time and allocations before/after;
- do not create a duplicate target-selection authority.

Testing:
- identical target selection calls return identical results;
- cache reuse is observable in deterministic counters/benchmarks;
- history/index/direction changes invalidate appropriately;
- BUY/SELL symmetry.

### CR6.8 / F9 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — Source/Architecture #2246, Runtime Acceptance #2055 and cTrader Compile #2239 passed on the final F9 HEAD.**

Completed:
- `TargetObstacleCacheKey` in Core captures Bars/history/index/direction and materially relevant scan inputs.
- `TargetObstacleScanCache` is bounded to 16 entries and reuses one M5 swing/equality snapshot across repeated target candidates.
- history reload/new closed-bar invalidation is explicit through Bars events plus same-Bars context invalidation.
- target-specific Entry/Target clearance remains evaluated on every candidate.
- existing F1 opposing-zone cache remains the sole zone-path candidate cache for M5 and M15/M30/H1/H4.
- deterministic planning contracts, F9 static audit and reference benchmark were added.
- no public parameter/default, target-selection authority, RR/confidence/SL/TP threshold or broker mutation path changed.

Verification boundary:
- Source/Architecture #2246, Runtime Acceptance #2055 and cTrader Compile #2239 passed on the final F9 HEAD before merge.
- the reference benchmark is platform-neutral structural-work evidence, not a terminal latency measurement.
- target-terminal replay, history reload/new-bar behavior and empirical outcome/signal-quality evidence remain manual.

**Next phase: CR6.9 / F3 — Orphaned managed-position protection.**

### CR6.9 / F3 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — Source/Architecture #2255, Runtime Acceptance #2064 and cTrader Compile #2248 passed on F3 implementation head `9b408ff7bcea45b03015e0b81f2e65a76c995b63`; merged via PR #137.**

**تأیید می‌کنم** — the invalid-stop success path in `OrphanManagedProtection.ProtectOrphanManagedPosition` has been corrected fail-closed.

Completed:
- invalid orphan stop candidate now returns failure and emits an explicit diagnostic;
- Core `OrphanManagedProtectionRule` gates success on valid stop and broker-confirmed protection for a valid BUY/SELL direction;
- caller enters `RecoveryRequired` on failure without updating `_lastBrokerModifyUtc`;
- deterministic Runtime Contract and static audit are accumulated.

Safety/manual boundary:
- no public parameter/default/threshold tuning;
- no alternate broker mutation owner;
- target-terminal broker/restart behavior remains manual acceptance.

**Next phase: CR7.1 / G1 — Broker protection must never increase live position risk.**

### CR6.9 — Orphaned managed-position protection (F3)

Initial review label: **CONFIRMED / MEDIUM — invalid-stop success result is a concrete safety invariant violation.**

Scope:
- `OrphanManagedProtection.ProtectOrphanManagedPosition`;
- `ManagedStopProtectionRule`;
- recovery-state transitions;
- caller handling of protection failure.

Required verification:
- prove that an invalid computed stop can return success even though no SL was placed;
- return explicit failure/RecoveryRequired when no valid protective stop can be established;
- emit a clear diagnostic without adding an alternate broker-mutation owner;
- verify callers retry/reconcile rather than treating the position as protected.

Testing:
- all stop candidates invalid;
- protection result is failure;
- recovery state is entered;
- caller does not suppress subsequent protection attempts;
- BUY/SELL symmetry.

### CR7.1 / G1 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #138 merged to `main` as `b8144c2f1edc62730b7a0723be3746afe6353851`.**

تأیید می‌کنم — G1 was audited and completed on the current execution/protection call chain.

Completed:
- existing broker-stop health is directionally protective and independent of current-market minimum-distance requirements;
- new proposed stops retain the existing market-distance validation;
- healthy existing SLs cannot be replaced by a farther/less protective stop;
- broker state, bound-plan protection and reconciliation consume the same existing-stop health owner;
- deterministic G1 runtime contracts and static audit are accumulated;
- project-integrity duplicate-method detection was corrected to distinguish methods by containing type.

Repository evidence on final G1 head `2f1cb933a2c2407e1fe33302cf72f538f090ba91`:
- Source/Architecture PASS — #2262;
- Runtime Acceptance PASS — #2071;
- cTrader Compile PASS — #2255.

Safety/manual boundary:
- no public parameter/default or trading-threshold tuning;
- actual cTrader broker modification/minimum-distance behavior remains manual acceptance.

**Next phase: CR7.2 / G2 — Retest adverse-momentum semantics and rejection telemetry.**

### CR7.2 / G2 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #142 merged to `main` as `f983d2fd7eb0baced4b5ff40988e6294b3f5bd28`.**

تأیید می‌کنم — G2 was implemented as semantic/ownership hardening rather than threshold tuning.

Completed:
- canonical Core trap threshold owner for 0.30 / 0.45 / 0.40;
- deterministic TRAP rejection reason codes;
- bounded Retest pre-zone versus post-zone/reaction diagnostics;
- unchanged Retest block gate and defaults;
- existing ActionabilityReason path remains the presentation/decision diagnostic authority;
- deterministic Runtime Acceptance and accumulated Source/Architecture coverage.

Verification:
- Source/Architecture PASS — #2283;
- Runtime Acceptance Contracts PASS — #2092;
- cTrader Compile PASS — #2276.

Safety/manual boundary:
- no public parameter/default or trading threshold tuning;
- no new decision/execution authority;
- target-terminal Retest timing and empirical signal-quality evidence remain manual.

### CR7.3 / G3 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #143 merged to `main` as `6c572643cfc6b9a4ee1083e300ec13607dd2774c`.**

Completed:
- `Level Line Thickness` public parameter retained unchanged with its existing 1..3 contract;
- removed the hidden `Math.Min(1, ...)` behavior that made valid values 2/3 render as 1;
- added `PlanLinePresentationRule.ResolveThickness` as the single canonical presentation mapping owner;
- preserved `LineStyle.Solid`;
- added deterministic runtime contracts and accumulated static audit.

Safety/manual boundary:
- no public parameter name/type/DefaultValue changed;
- no trading threshold or decision/execution authority changed;
- visible target-terminal thickness/style still requires manual cTrader validation.

### CR7.4 / G4 closeout — Panel execution/protection state semantics

Status: **VERIFIED COMPLETE — PR #144.**

Completed:
- canonical Core panel execution/protection state owner;
- Auto Trade and Auto Orders operational state is based on runtime/lifecycle state and actual managed action state, not decision/reaction readiness;
- broker protection state is based on broker-confirmed SL/TP geometry, target requirement, server-side TP-ladder ownership and recovery state;
- shared panel snapshot removes repeated broker enumeration across the three new state surfaces;
- deterministic G4 runtime contract and accumulated static audit pass.

Verification:
- Source/Architecture PASS — #2313;
- Runtime Acceptance Contracts PASS — #2122;
- cTrader Compile PASS — #2306;
- verified implementation HEAD: 2465444593afca6b566b17be2234336154fd6f2e.

Safety:
- no public parameter/default or trading threshold changes;
- no new decision/execution/broker-mutation authority.

Manual boundary:
- target-terminal cTrader panel/protection/reconnect behavior remains manual.

Historical continuity marker retained for G3 audit:
> **Current active phase: CR7.4 / G4 — Panel execution/protection state semantics.**

### CR7.5 / G5 — Panel execution/protection state freshness and broker-read minimization

Initial state: **SCOPE REQUIRED** after verified G4.

Status: **VERIFIED COMPLETE — PR #145 merged to `main` as `e2674b9800159ba1266639ad96a374f622aff555`.**

Repository verification on implementation head `1a2572e2f72e8842640e9c1cbea88e6868f05354`:
- Source/Architecture PASS — #2321;
- Runtime Acceptance Contracts PASS — #2130;
- cTrader Compile PASS — #2314.

Finding confirmed:
- `BuildPanelPresentationKey` invalidated the G4 execution/protection snapshot on every
  key calculation, defeating cross-refresh cache reuse.

Implemented boundary:
- presentation is read-only with respect to cache freshness;
- broker dirty events and due broker refresh invalidate the snapshot;
- runtime/lifecycle state changes invalidate centrally;
- direct block/recovery/server-ladder assignments are guarded against stale derived panel state;
- the existing one-second broker-state refresh remains the bounded stale-state backstop.

Verification:
- deterministic G5 Runtime Acceptance contract;
- dedicated `audit_phase_7_5.py`;
- accumulated Source/Architecture, Runtime Acceptance and cTrader Compile;
- target-terminal panel/reconnect responsiveness remains manual.

Safety:
- no public parameter/default or trading threshold tuning;
- no second decision/execution/broker-mutation authority.

### CR7.6a / G6A closeout — Execution panel presentation freshness — 2026-10-01

Status: **VERIFIED COMPLETE — PR #146 functional implementation HEAD `b0b19c1a3e7e65110dc1b64d4f8a3bf555b7c54e`.**

Completed:
- identified the G5 presentation-identity gap without undoing G5 broker-read minimization;
- centralized execution-facing panel identity in `ExecutionPanelPresentationIdentityRule`;
- included Auto Trading state/reason, execution telemetry path/state, active scenario, market-suitability score/state/reason and break-even diagnostic in the panel identity;
- made canonical Auto Trading state/reason changes invalidate the G4 execution/protection snapshot only when they actually change;
- kept telemetry-only presentation changes from forcing broker-state snapshot invalidation;
- added deterministic Runtime Acceptance coverage and `audit_phase_7_6a.py`;
- reconciled accumulated G5 continuity audit so historical phase transitions remain valid as the roadmap advances.

Verification:
- Source/Architecture: **PASS** — #2327;
- Runtime Acceptance Contracts: **PASS** — #2136;
- cTrader Compile: **PASS** — #2320.

Safety:
- no public parameter name/type/DefaultValue changed;
- no RR/confidence/entry/SL/TP/risk/execution threshold changed;
- no decision/execution/broker-mutation authority changed;
- no new broker enumeration or unbounded cache introduced.

Manual boundary:
- cTrader target-terminal panel refresh timing, scenario/suitability/break-even visual refresh, reconnect/reload and empirical behavior remain manual.

Operator action:
- after PR #146 is merged, run `git pull --ff-only` on local `main`.

Next phase: **CR7.6c.**

## CR7.6b / G6B closeout — Execution-control truth and single UI authority — 2026-10-01

Status: **VERIFIED COMPLETE — implementation head `7832f47c05117f66686adf659fd92670dcb14ba8`.**

Completed:
- restored AUTO TRADE / AUTO ORDERS as status-only presentation surfaces;
- removed the legacy in-panel execution mutation handler;
- centralized status text and the read-only interaction policy in Core;
- preserved canonical settings → runtime synchronization via `EnsureExecutionRuntimeState()`;
- reconciled accumulated architecture/project-integrity/CR3.4 audits;
- added deterministic Runtime Acceptance and Source/Architecture coverage.

Verification:
- Source/Architecture PASS — #2339;
- Runtime Acceptance Contracts PASS — #2148;
- cTrader Compile PASS — #2332.

Safety/manual boundary:
- no public parameter/default, RR/confidence, entry/SL/TP, risk or execution threshold tuning;
- no new decision/execution/broker-mutation authority;
- target-terminal click behavior, startup/reload synchronization and responsiveness remain manual.

Operator action after merge:
- run `git pull --ff-only` on local `main`.

Next implementation phase: **CR7.6c.**


### Prompt 6 completion gate

CR6.1 → CR6.2 → CR6.3 → CR6.4 → CR6.5 → CR6.6 → CR6.7 → CR6.8 → CR6.9 → CR7.1 → CR7.2 → CR7.3 → CR7.4 → CR7.5 → CR7.6a → CR7.6b → CR7.6c → CR7.6d → **CR-FINAL**

CR-FINAL cannot be considered complete while any F-item remains unverified, deferred without an explicit reason, or blocked by missing deterministic/replay/target-terminal evidence.

## 7.3 Cross-chat continuation checkpoint


This file is the canonical implementation order for the Claude review-remediation track. It supersedes the local cBot track as the immediate next-work source until `CR-FINAL` is accepted.

At the start of every new chat, read this file and `docs/CONTINUATION-STATE.md` first. The active phase recorded there is the only phase to implement next; do not jump to CBOT work while this track is incomplete.

Current active phase: **CI-13 — TP source, target obstacle and TP ladder audit**. CI-FINAL remains the gate before resuming the paused Prompt 8 sequence.

### Prompt 8 continuity override — 2026-10-01

The Prompt 7 G6B implementation is closed on main. The historical CR7.6c/G6C marker has no authoritative implementation scope on the audited main tree. Prompt 8 is the next fully specified remediation sequence and is now being executed one phase at a time:

CR8.1/H1 → CR8.2/H2 → CR8.3a/H3-A → CR8.3b/H3-B → CR8.4/H4 → CR8.5a/H5-A → CR8.5b/H5-B → CR8.6/H6 → CR-FINAL

Current active implementation phase: **CI-13 — TP source, target obstacle and TP ladder audit**.

Prompt 8 / CR8.4 is intentionally paused until **CI-FINAL**; after CI-FINAL, resume the existing Prompt 8 sequence at CR8.4/H4.

CR8.1/H1 is verified complete on PR #149 with Source/Architecture #2348, Runtime Acceptance #2157 and cTrader Compile #2341.

## 7.4 CR6.3 / F4 completion checkpoint

CR6.3 / F4 is **verified complete** on 2026-10-01 via PR #129. The effective actionability thresholds and hidden additive margins are now owned by `ActionabilityThresholdPolicy`, the panel exposes effective thresholds, `ActionableNow` is documented as post-final-gate state, and the existing public parameter/default contract was preserved.

Verification on commit `5dd9a8a8bb0fb8172ac40b7336ce86e0bb5c3c2a`: Source/Architecture PASS, Runtime Acceptance Contracts PASS, cTrader Compile PASS.

Next implementation phase: **CR6.4 / F5 — Smart-threshold regime identity and hidden REVERSAL dead path**.

## 8. Completion order and dependencies

Mandatory order:

CR-0 → CR1.1 → CR1.2 → CR1.3 → CR1.4 → CR1.5 → CR1.6 → CR1.7 → CR1.8 → CR1.9 → CR2.1 → CR2.2 → CR2.3 → CR2.4 → CR2.5 → CR2.6 → CR2.7 → CR2.8 → CR2.9 → CR3.1 → CR3.2 → CR3.3 → CR3.4 → CR3.5 → CR4.1 → CR4.2 → CR4.3 → CR4.4 → CR4.5 → CR4.6 → CR4.7 → CR4.8 → CR4.9 → CR4.10 → CR5.1 → CR5.2 → CR5.3 → CR5.4 → CR5.5 → CR5.6 → CR5.7 → CR5.8 → CR6.1 → CR6.2 → CR6.3 → CR6.4 → CR6.5 → CR6.6 → CR6.7 → CR6.8 → CR6.9 → CR-FINAL

Why this order:

1. Session/time/reference rules establish correct inputs for many later calculations.
2. Risk/news/runtime execution gates must be correct before interpreting signal quality.
3. Structure/reaction/pending correctness depends on clean event semantics.
4. Lifecycle/outcome correctness must exist before calibration is trusted.
5. Live management/TP work depends on reliable broker-state accounting.
6. Final integration must happen before resuming the rest of the product roadmap.

---

## 9. CR-0 — Final audit closure

Status: **COMPLETE — audit-only gate closed 2026-09-30**

Detailed source inventory and parameter ownership record:

`docs/CLAUDE-REVIEW-CR0-AUDIT.md`

CR-0 completed:

- exact file/method inventory for all A1–A12/B1–B12/C1–C9;
- all prior AUDIT FIRST items resolved to a confirmed owner or an explicitly bounded/deferred phase;
- current parameter truth recounted from all parameter source files: 566 declarations across 29 source files / 27 logical groups;
- cBot-boundary-sensitive parameters and mixed ownership groups classified without authorizing whole-file moves;
- threshold semantics unified into explicit semantic families;
- session/time semantics unified around one UTC closed-bar reference and one canonical session meaning;
- outcome/accounting semantics separated into broker facts, managed outcome observations, calibration observations and persistent archive;
- mandatory manual cTrader verification matrix defined.

Important corrections retained:

- A4 remains a partial finding; the latest-swing causal claim was not adopted verbatim.
- A5 AccessRights.None is not treated as a network blocker; current official cTrader documentation states AccessRights.None is sufficient for network functions, while target-terminal verification remains mandatory.
- B9 historical rendering is evaluated at host-bar rebuild frequency, not every Calculate call.
- C5's stale BE-rejection subclaim is not implemented because current source updates the plan stop only after successful broker stop modification.

CR-0 production behavior changes: **NONE**.

Implementation order now advances to:

`CR1.1 → CR1.2 → CR1.3 → CR1.4 → CR1.5 → CR1.6 → CR1.7 → CR1.8 → CR1.9 → CR2.1 → CR2.2 → CR2.3 → CR2.4 → CR2.5 → CR2.6 → CR2.7 → CR2.8 → CR2.9 → CR3.1 → CR3.2 → CR3.3 → CR3.4 → CR3.5 → CR-FINAL`

Track 12A local cBot separation remains blocked until CR-FINAL passes.

## 10. Final integration gate — CR-FINAL

All are mandatory:

### Source
- no unintended duplicate business rule;
- no hidden parameter clamp;
- no direct broker mutation introduced in a new analytical helper;
- no second decision authority;
- no multi-position behavior.

### Tests
- pure Core rule tests pass;
- deterministic synthetic bars cover time/session/structure/reaction/FVG/OB/WaveTrend/decision/outcome cases;
- runtime contract suite passes;
- source/architecture audit passes;
- cTrader compile/build passes.

### Manual cTrader
- AccessRights/HTTP feed behavior and failure states;
- LocalStorage/file persistence;
- ExecuteMarketOrder / PlaceStopOrder / PlaceLimitOrder semantics;
- trading permission behavior;
- History/deal aggregation for partial closes;
- broker SL/TP modification and rejection;
- restart/reconnect lifecycle;
- target-terminal chart/panel behavior;
- no duplicate execution.

### Documentation
- each fix has its own root-cause commit;
- each phase has a completion record;
- user-visible behavior changes are listed;
- unverified items remain explicitly marked;
- Roadmap points to Track 12A only after CR-FINAL passes.

---

## 11. Relationship to local cBot separation

This remediation track does not cancel or replace Track 12A.

Instead:

- pure analysis/math/runtime defects are corrected before separation;
- deterministic execution rules are hardened so their logic can be carried into the cBot without semantic change;
- broker mutation remains subject to the existing one execution authority rule;
- after CR-FINAL, continue with the already-approved local separation roadmap.

The future architecture remains:

Indicator = Analysis / Decision / Scenario / Plan / Presentation

cBot = Broker Execution / Account Risk / Protection / Lifecycle

No Cloud implementation is added by this track.

---

## 12. Permanent evidence rule

No change to public trading thresholds, signal confidence, RR floors or risk parameters is accepted merely because a source review says they look too strict or look too loose.

Such changes require deterministic replay/outcome evidence.

This is especially important for:

- OB/FVG weighting;
- confidence floors;
- RR requirements;
- reversal thresholds;
- stop-width preferences;
- early-prediction weights;
- calibration adjustments.


### CR8.3a / H3-A closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #151, implementation head `b19e3366b0115d79a6e6a61b79af310ac64bbdd7`.**

Completed:
- one immutable Core `OssIndicatorSettings.Default` owns the fixed production Skender settings;
- fixed defaults were removed from `OssIndicatorParameters`, which retains cache/history/safety settings;
- all affected Skender adapters consume the canonical owner;
- RSI and MACD fast/slow remain parameter-driven;
- deterministic Planning/Runtime contracts and `audit_phase_8_3a.py` cover ownership and preserved values;
- the accumulated CR4.4 settings audit was reconciled;
- phase completion is recorded in `docs/PHASE-CR8-3A-H3-A-SKENDER-SETTINGS.md`.

Verification:
- Source / Architecture: **PASS**;
- Runtime Acceptance Contracts: **PASS**;
- cTrader Compile / Build: **PASS**.

Safety:
- public parameter names/types/DefaultValues unchanged;
- no trading/RR/confidence/entry/SL/TP/risk/execution tuning;
- no decision or broker-execution authority changed;
- H3-B warm-up/bounded-computation/cache/parity work remains separate.

**Next implementation phase: CR8.3b / H3-B.**


### CR8.3b / H3-B implementation record — 2026-10-01

Status: **VERIFIED COMPLETE — PR #152; final implementation head `b0ddaabed9723515d50ac183592f1eb7d56b5942`; merged as `db52531a5fe333d2645cdd5f63ac33844d01f8e0`.**

Completed in branch `phase/cr8-3b-h3-b-skender-warmup-cache-parity`:
- bounded stable Skender quote window at 768 bars;
- incremental stable-window cache with first/last boundary fingerprints;
- removal of stable-prefix copies;
- removal of per-call Skender result-list materialization;
- deterministic Runtime Contract coverage for the warm-up policy;
- deterministic 2048-bar full-prefix versus bounded-window parity/performance benchmark;
- new `audit_phase_8_3b.py` accumulated after H3-A;
- accumulated CR4.4 audit semantics reconciled to the bounded stable window.

Safety boundary:
- no public parameter name/type/DefaultValue changed;
- no trading/RR/confidence/entry/SL/TP/risk/execution threshold tuned;
- no decision or broker-mutation authority changed;
- FacioQuo remains research-only;
- target-terminal timing, live performance and empirical signal quality remain manual acceptance items.

Verification on final implementation head `b0ddaabed9723515d50ac183592f1eb7d56b5942`:
- Source / Architecture: **PASS**;
- Runtime Acceptance Contracts: **PASS**;
- cTrader Compile / Build: **PASS**;
- OSS indicator benchmark: **PASS**.

Operator action after merge: run `git pull --ff-only` on local `main` before continuing.

Next specified phase: **CR8.4 / H4**.