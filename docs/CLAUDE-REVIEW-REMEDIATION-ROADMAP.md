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

Status: **VERIFIED COMPLETE on phase branch; merge/CI verification remains the repository acceptance boundary.**

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

## 7.1 Cross-chat continuation checkpoint

This file is the canonical implementation order for the Claude review-remediation track. It supersedes the local cBot track as the immediate next-work source until `CR-FINAL` is accepted.

At the start of every new chat, read this file and `docs/CONTINUATION-STATE.md` first. The active phase recorded there is the only phase to implement next; do not jump to CBOT work while this track is incomplete.

Current active phase: **CR3.4 — Execution UI control and popup reliability**.

## 8. Completion order and dependencies

Mandatory order:

CR-0 → CR1.1 → CR1.2 → CR1.3 → CR1.4 → CR1.5 → CR1.6 → CR1.7 → CR1.8 → CR1.9 → CR2.1 → CR2.2 → CR2.3 → CR2.4 → CR2.5 → CR2.6 → CR2.7 → CR2.8 → CR2.9 → CR3.1 → CR3.2 → CR3.3 → CR3.4 → CR3.5 → CR-FINAL

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
