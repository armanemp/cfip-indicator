# M2 — Repository Hygiene / Dead Code / Ownership / Source-of-Truth Audit

Date: 2026-10-04
Branch: `phase/M2-repository-hygiene-ownership-current`
Status: **IN PROGRESS**

## 1. Objective

This phase is the first executable phase after the master baseline. Its purpose is to establish repository truth before touching trading mathematics:

- every production artifact has a reason to exist;
- every semantic behavior has exactly one owner;
- duplicate calculations/renderers/executors are identified;
- dead/disabled/unreachable production code is identified;
- compatibility aliases and historical remnants are classified;
- documentation/audit drift is separated from production behavior;
- no new parallel implementation is introduced to solve an old ownership problem.

## 2. Frozen architecture law

**One concept → one owner → one source of truth → read-only consumers.**

Forbidden:

- duplicate calculations for the same semantic value;
- duplicate signal-direction/strength derivation;
- duplicate chart rendering;
- duplicate alert/audio delivery;
- duplicate execution authority;
- duplicate lifecycle truth;
- duplicate Entry/SL/TP/RR calculations;
- panel-side reinterpretation of analytical state;
- cBot reconstruction of Indicator decisions;
- hidden fallback executors;
- "temporary" compatibility owners that become permanent.

## 3. Repository baseline

At the master audit baseline:

- 1113 repository files;
- 82 directory entries;
- 731 C# files;
- 667 Indicator C# files;
- 22 cBot C# files;
- 22 Contracts C# files;
- 204 Markdown documents;
- 155 Python audit/tool files;
- current parameter baseline: 548.

The master inventory is in:
`docs/MASTER-FULL-FORENSIC-AUDIT-2026-10-04.md`

## 4. Already evidenced ownership risks

### 4.1 Large mixed owners

M1 already identified:

- `SignalVisualSnapshotBuilder.BuildSignalVisualSnapshot` (~401 lines)
- `TradeActionabilityEvaluator.EvaluateTradeActionability` (~533 lines)

These are not automatically defects. They are ownership/decomposition risk points and must be reviewed before splitting so behavior does not migrate into duplicate helpers.

### 4.2 Broker boundary

M1 identified 15 direct broker-mutation call sites concentrated in the existing broker owners.

Rule for M2:

- do not create new mutation sites;
- do not copy Indicator execution classes wholesale into cBot;
- do not create a second executor while extracting;
- preserve broker-confirmed state as authoritative.

### 4.3 Historical audit drift

M1 records a historical public-parameter baseline of 568. The current repository baseline is 548 and the current parameter-semantic tooling/README use 548.

Disposition:

**HISTORICAL DOCUMENT — NOT A PRODUCTION DEFECT.**

It must not be rewritten to pretend historical work used today's count. Current audits must use 548.

## 5. M2 complete audit matrix

### A — File and directory truth

- [ ] M2.1 enumerate every production source file
- [ ] M2.2 enumerate every production test/preflight file
- [ ] M2.3 enumerate every audit/tool script
- [ ] M2.4 classify every documentation directory
- [ ] M2.5 identify generated/build artifacts accidentally tracked
- [ ] M2.6 identify temporary/debug files
- [ ] M2.7 identify backup/copy/old-version files
- [ ] M2.8 identify duplicate filenames with different namespaces
- [ ] M2.9 identify duplicate type names
- [ ] M2.10 identify duplicate semantic service names
- [ ] M2.11 identify files referenced by project files but absent from intended production scope
- [ ] M2.12 identify files present but unreachable from the build graph
- [ ] M2.13 identify source files excluded by conditions that still contain production logic
- [ ] M2.14 identify generated code incorrectly treated as hand-owned source

### B — Dead code

- [ ] M2.15 identify unused classes
- [ ] M2.16 identify unused methods
- [ ] M2.17 identify unused properties/fields
- [ ] M2.18 identify unused constructors
- [ ] M2.19 identify unreachable branches
- [ ] M2.20 identify permanently false feature flags
- [ ] M2.21 identify permanently true compatibility switches
- [ ] M2.22 identify dead enum values
- [ ] M2.23 identify dead event subscriptions
- [ ] M2.24 identify dead timer callbacks
- [ ] M2.25 identify dead chart object owners
- [ ] M2.26 identify dead alert/audio paths
- [ ] M2.27 identify dead persistence paths
- [ ] M2.28 identify dead cBot execution paths
- [ ] M2.29 identify stale recovery paths
- [ ] M2.30 identify commented-out production code that has no active purpose
- [ ] M2.31 identify exception swallowing that makes dead paths appear alive
- [ ] M2.32 identify reflection/string-based entry points that defeat ordinary reachability analysis

### C — Duplicate semantic ownership

- [ ] M2.33 direction
- [ ] M2.34 strength
- [ ] M2.35 confidence
- [ ] M2.36 quality
- [ ] M2.37 regime
- [ ] M2.38 Entry
- [ ] M2.39 Trigger
- [ ] M2.40 SL
- [ ] M2.41 TP
- [ ] M2.42 RR
- [ ] M2.43 Actionability
- [ ] M2.44 Scenario identity
- [ ] M2.45 execution intent
- [ ] M2.46 broker identity
- [ ] M2.47 lifecycle state
- [ ] M2.48 broker-confirmed state
- [ ] M2.49 chart signal
- [ ] M2.50 chart plan line
- [ ] M2.51 chart label
- [ ] M2.52 panel timeframe state
- [ ] M2.53 popup alert
- [ ] M2.54 sound selection
- [ ] M2.55 sound playback
- [ ] M2.56 alert cooldown
- [ ] M2.57 history outcome
- [ ] M2.58 calibration input
- [ ] M2.59 risk amount
- [ ] M2.60 volume
- [ ] M2.61 capacity
- [ ] M2.62 spread/session suitability
- [ ] M2.63 persistence ownership

### D — Duplicate calculation patterns

- [ ] M2.64 repeated ATR calculations
- [ ] M2.65 repeated pip/price conversion
- [ ] M2.66 repeated spread calculation
- [ ] M2.67 repeated distance/RR calculation
- [ ] M2.68 repeated direction normalization
- [ ] M2.69 repeated score normalization
- [ ] M2.70 repeated confidence normalization
- [ ] M2.71 repeated regime classification
- [ ] M2.72 repeated zone selection
- [ ] M2.73 repeated FVG validity
- [ ] M2.74 repeated OB validity
- [ ] M2.75 repeated structural stop selection
- [ ] M2.76 repeated target selection
- [ ] M2.77 repeated readiness calculation
- [ ] M2.78 repeated closed-bar detection
- [ ] M2.79 repeated MTF index mapping
- [ ] M2.80 repeated stale/expiry detection
- [ ] M2.81 repeated idempotency keys
- [ ] M2.82 repeated broker-state inference

### E — Duplicate presentation

- [ ] M2.83 competing arrow renderers
- [ ] M2.84 competing arrow-strength calculations
- [ ] M2.85 competing signal labels
- [ ] M2.86 competing plan labels
- [ ] M2.87 competing plan-line renderers
- [ ] M2.88 competing panel signal rows
- [ ] M2.89 competing MTF lamp state
- [ ] M2.90 competing popup paths
- [ ] M2.91 competing sound paths
- [ ] M2.92 competing chart cleanup
- [ ] M2.93 competing object naming schemes
- [ ] M2.94 competing visual lifecycle/expiry logic

### F — Duplicate execution

- [ ] M2.95 market execution
- [ ] M2.96 aggressive execution
- [ ] M2.97 pending Stop execution
- [ ] M2.98 pending Limit execution
- [ ] M2.99 cancellation
- [ ] M2.100 close
- [ ] M2.101 partial close
- [ ] M2.102 SL mutation
- [ ] M2.103 TP mutation
- [ ] M2.104 recovery execution
- [ ] M2.105 emergency execution
- [ ] M2.106 reconciliation mutation
- [ ] M2.107 hidden/disabled legacy execution

### G — Contract/documentation drift

- [ ] M2.108 parameter-count drift
- [ ] M2.109 stale phase status
- [ ] M2.110 stale branch names
- [ ] M2.111 stale commit references
- [ ] M2.112 stale file counts
- [ ] M2.113 stale owner names
- [ ] M2.114 contradictory architecture claims
- [ ] M2.115 contradictory M15/M5/M1 roles
- [ ] M2.116 contradictory arrow semantics
- [ ] M2.117 contradictory alert semantics
- [ ] M2.118 contradictory cBot boundary
- [ ] M2.119 stale audit baselines
- [ ] M2.120 audit scripts whose assertions no longer match canonical contracts
- [ ] M2.121 historical documentation incorrectly presented as current truth

### H — Dependency direction

- [ ] M2.122 Contracts remains platform-neutral
- [ ] M2.123 Indicator does not depend on cBot implementation
- [ ] M2.124 cBot does not reach into Indicator private implementation
- [ ] M2.125 UI does not own domain decisions
- [ ] M2.126 renderers do not own calculations
- [ ] M2.127 alerts do not own decisions
- [ ] M2.128 persistence does not own live decisions
- [ ] M2.129 history does not feed unapproved live authority
- [ ] M2.130 calibration does not silently modify live thresholds
- [ ] M2.131 execution does not reconstruct analytical intent
- [ ] M2.132 no circular dependencies
- [ ] M2.133 no dependency inversion violation hidden by utility classes
- [ ] M2.134 no shared mutable singleton crosses semantic boundaries without ownership

### I — Runtime/lifecycle hygiene

- [ ] M2.135 duplicate event subscription
- [ ] M2.136 duplicate timer registration
- [ ] M2.137 missing unsubscribe
- [ ] M2.138 duplicate initialization
- [ ] M2.139 stale state surviving reload
- [ ] M2.140 stale state surviving symbol/timeframe change
- [ ] M2.141 stale chart objects surviving stop
- [ ] M2.142 stale panel controls surviving stop
- [ ] M2.143 stale alert queue surviving restart
- [ ] M2.144 stale execution identity surviving restart
- [ ] M2.145 stale caches surviving history replacement
- [ ] M2.146 async callback after disposal
- [ ] M2.147 timer/tick reentrancy
- [ ] M2.148 double processing of one bar/event
- [ ] M2.149 cleanup ordering defect

### K — Newly confirmed M2 findings

- [ ] M2.166 Management-command request acceptance is conflated with broker confirmation: `RequestManagementCommand` returns `false` both for a newly queued request and for rejection/storage failure, while callers use the boolean as if it were a broker-mutation result. A temporary boolean fix was intentionally reverted because it would make queued requests look broker-confirmed. The correct root fix requires one explicit Request/Confirmed/Rejected status contract consumed consistently by all management callers.
- [ ] M2.167 Synchronous `LocalStorage.SetString + Flush` is still used in the management-command publication path. This is a confirmed hot-path persistence/performance risk and must be redesigned around buffered/asynchronous ownership before the persistence/performance phase can close it.
- [ ] M2.168 `ProcessManagementReports` synchronously reads LocalStorage from the management-request path. This couples broker-state reconciliation to storage latency and needs the same buffered ownership treatment as M2.167.
- [ ] M2.169 Several Indicator helper names (`TryClosePosition`, `TryModifyStopLoss`, `TryModifyTakeProfit`, `TryCancelPendingOrder`) still read like broker mutations although they now publish commands. This is a semantic/maintenance hazard; naming should be normalized without introducing a second API owner.
- [ ] M2.170 Historical execution-boundary documents still reference removed Indicator broker owners such as `BrokerMarketOrderMutation.cs` / `BrokerLimitOrderPlacement.cs`. These are documentation/audit drift findings and must be classified explicitly rather than treated as current production files.
- [ ] M2.171 The management-command API has no explicit submission-state type, forcing callers to infer semantic meaning from `bool`; this is the root contract defect behind M2.166 and must be eliminated without adding a parallel management bus.

### J — Build/test hygiene

- [ ] M2.150 production-only files accidentally excluded from build
- [ ] M2.151 tests excluded from solution
- [ ] M2.152 audit scripts not run by CI where required
- [ ] M2.153 CI checks stale baselines
- [ ] M2.154 local and CI manifests differ
- [ ] M2.155 Release and Debug compilation differ semantically
- [ ] M2.156 warnings hidden
- [ ] M2.157 analyzers disabled
- [ ] M2.158 conditional compilation hides defects
- [ ] M2.159 test names no longer match assertions
- [ ] M2.160 tests assert implementation details instead of contracts
- [ ] M2.161 golden fixtures stale
- [ ] M2.162 missing negative fixtures
- [ ] M2.163 missing BUY/SELL mirrored fixtures
- [ ] M2.164 missing multi-scenario fixtures
- [ ] M2.165 missing restart/reconnect fixtures

## 6. New cross-cutting issues added to the master audit

The following classes of risk are now mandatory in the master document in addition to the original checklist:

1. Build-graph reachability vs file existence.
2. Conditional-compilation dead code.
3. Reflection/string-based reachability.
4. Duplicate initialization/event subscription.
5. Stale state after symbol/timeframe/history replacement.
6. Object-name collision and chart cleanup ownership.
7. Audit-script assertion drift.
8. Local-vs-CI test-manifest drift.
9. Release-vs-Debug semantic divergence.
10. Golden-fixture staleness.
11. Negative-fixture absence.
12. Multi-scenario isolation regression.
13. Restart/reconnect identity leakage.
14. Hidden mutable singleton ownership.
15. Historical documentation being mistaken for current authority.
16. Calibration/history silently becoming live decision authority.
17. Consumer-side semantic reinterpretation of canonical values.
18. Contract-version compatibility accidentally becoming a second semantic contract.
19. Exception swallowing hiding unreachable/dead paths.
20. Async callbacks executing after owner disposal.

## 7. Phase completion rule

M2 cannot be marked complete merely because the repository compiles.

It closes only when:

- every applicable M2 item has a disposition;
- duplicate owners are removed or explicitly proven to be different semantics;
- dead production code is removed or explicitly proven reachable/required;
- documentation/audit drift is classified;
- dependency direction is verified;
- lifecycle ownership is verified;
- no new broker mutation owner appears;
- Source/Architecture gate passes;
- Runtime Acceptance gate passes;
- cTrader Compile gate passes;
- target-terminal checks are explicitly separated and assigned where M2 cannot statically prove behavior.

## 8. Current disposition

**NOT CLOSED YET.**

The repository inventory and master baseline are established. M2 execution must continue through the actual production source graph and accumulated audit suite before any claim of completion.



## 2026-10-04 — Current-main rebase correction

The original M2 branch was discovered to be 41 commits behind current `main` and carried a truncated 96-line ROADMAP that would have deleted current project history if merged. That branch is not a valid merge base. M2 was recreated from current `main` as `phase/M2-repository-hygiene-ownership-current`.

No production changes from the stale branch are being carried forward automatically. Findings are being re-verified against current-main source before implementation.


## 2026-10-04 — Additional deep lifecycle findings

- [ ] M2.172 **cBot broker event subscriptions are anonymous and are not unsubscribed in OnStop().** SubscribeBrokerLifecycleEvents() attaches seven lambda handlers to Positions/PendingOrders, while OnStop() only stops the timer and calls UnsubscribeIndicatorLifecycleEvents(). This creates an explicit lifecycle-ownership gap and can retain the cBot instance/closures or cause callbacks against a stopped owner. The canonical fix must store named delegates/handlers and unsubscribe exactly once; do not add another event layer.
- [ ] M2.173 **Indicator trading-event hookup can partially succeed.** FinalizeAsyncInitialization() performs multiple event subscriptions inside one try/catch; if one subscription throws, earlier subscriptions remain active while initialization continues. This requires transactional hookup semantics or deterministic rollback of the already-attached subset.
- [ ] M2.174 **Initialization polling renders the panel synchronously on every timer cycle while data is not ready.** OnTimer() can call RenderPanel() during the initialization wait path, while the timer is also responsible for lifecycle/startup work. This is a confirmed candidate for startup/UI latency amplification and must be measured against the panel-performance contract rather than patched with arbitrary throttles.
- [ ] M2.175 **Indicator registers broker Positions/PendingOrders lifecycle listeners despite the final architecture making the cBot the sole broker mutation authority.** Reading broker state is allowed where needed for reconciliation, but the exact purpose and downstream consumers of each Indicator broker event must be proven necessary; otherwise this is architectural residue from the former execution owner. Trace every handler to its final consumer before allowing it to remain.
- [ ] M2.176 **OnDestroy() persistence ordering is mixed with lifecycle teardown.** Buffered persistence is flushed before timer stop/unsubscription, while outcome/history persistence occurs after unsubscription. This needs an explicit shutdown contract covering callback quiescence, final state capture, flush, and disposal so late callbacks cannot race with persisted terminal state.


- [ ] M2.177 **cBot has two independent realtime signal-consumption clocks.** OnTick() reads scenario/signal transport and calls ProcessSignalEnvelope(), while OnTimer() independently reloads the signal store and reads the same scenario/signal transport and also calls ProcessSignalEnvelope(). A revision guard exists only for the timer path, so the architecture must prove that duplicate reads, ordering, state races, repeated preflight/logging, and scenario-protection sweeps cannot diverge. One canonical consumption clock/queue is required; the timer should not become a second execution path merely to improve responsiveness.
- [ ] M2.178 **cBot performs multiple high-frequency reconciliation/settings/binding operations on OnTick plus a 100ms timer.** RefreshIndicatorBinding, RefreshExecutionSettings, signal-store reload and broker reconciliation are invoked from both runtime paths. Their cache/IO cost and side effects must be traced and ownership centralized; otherwise tick rate can amplify execution overhead independently of the 100ms timer.

- [ ] M2.179 **Scenario protection sweep has O(N) broker reconciliation per stored scenario on every signal-consumption cycle.** `SweepScenarioProtectionStates()` iterates the retained scenario dictionary and calls broker reconciliation for each scenario. Because it is reached from both OnTick and OnTimer, the duplicate-clock problem multiplies this cost and can make protection behavior dependent on scenario count/runtime frequency.
- [ ] M2.180 **Scenario retention is capped by dictionary count, not explicit lifecycle/expiry semantics.** `TrackScenarioEnvelope()` keeps up to 32 scenarios and removes the first enumerated key when the cap is exceeded. Dictionary enumeration order is not a documented temporal eviction contract; therefore the retained set and protection sweep can discard an arbitrary scenario unless insertion-order semantics are guaranteed elsewhere.
- [ ] M2.181 **Indicator binding refresh and execution-settings refresh have different cache semantics.** `RefreshIndicatorBinding()` can detect a changed instance and clear/reload state, while `RefreshExecutionSettings(false)` simply returns whenever settings are non-null. The audit must prove that settings cannot become stale when the same Indicator instance's parameters/configuration change, especially because ChartIndicator Modified events can occur between the two clocks.
- [ ] M2.182 **`ReloadSignalStore(true)` is forced on every cBot timer cycle.** The 100ms timer bypasses the 100ms next-reload cache by design, while OnTick uses the non-forced path. This makes signal-store access frequency dependent on timer scheduling and duplicates the transport polling path; one canonical transport refresh cadence is required.
- [ ] M2.183 **Scenario protection can mutate shared execution context while iterating scenarios.** `SweepScenarioProtectionStates()` changes `_activeManagedExecutionLabel`, `_lastSignalEnvelope`, and `_reconciliation` when recovering a scenario. Those are also global cBot fields used by general execution/state publishing, so one scenario's recovery can temporarily become the global active context of another scenario. Scenario-local recovery state must remain scenario-scoped until explicitly promoted by a canonical policy.

### M2 implementation progress — realtime/cross-scenario ownership correction

- M2.177/M2.178 root correction implemented: realtime signal/management consumption is now owned by the 100ms OnTimer() path. OnTick() no longer reads signal transport, processes envelopes, processes management commands, or sweeps scenario protection. This removes the duplicate realtime execution-consumption clock rather than adding another guard.
- ReloadSignalStore(false) now uses the existing canonical reload cadence from the timer; startup remains the only forced initial load.
- M2.183 root correction implemented: scenario protection recovery no longer promotes a scenario's reconciliation/envelope/label into global execution context during the scenario sweep. Recovery receives the scenario-local reconciliation result explicitly.
- M2.184 confirmed/fixed: realtime revision tracking was previously global to one scenario. It is now keyed by ScenarioId, so a multi-scenario batch cannot cause scenario A to be reprocessed merely because scenario B advanced the global revision tracker. Binding/reset paths clear the per-scenario revision map.
- ShouldProcessRealtimeTimerEnvelope() remains the single canonical duplicate-consumption guard for the timer-owned transport consumer.
- These corrections are architectural, not threshold/latency patches: one realtime consumer owner, one transport refresh policy, and scenario-local recovery truth.

### Verification state for this implementation
- Source-level review completed for the modified cBot paths.
- GitHub combined status for the documentation-only preceding commit was empty; no passing build gate was claimed.
- cTrader compile/runtime acceptance remains open until the branch can pass the repository's authoritative build/audit workflow and target-terminal checks.


## 2026-10-04 — Mandatory Modern / Advanced / Optimized Engineering Gate

M2 now requires every touched production area to be evaluated for the most modern, advanced and efficient practical design compatible with CFIP constraints.

This covers architecture, logic, performance, runtime/lifecycle, UI/UX, presentation, observability, persistence, testing/CI, dependencies, safety and documentation. Modern does not mean adopting fashionable technology blindly: the single-owner architecture, deterministic contracts, measured performance, cTrader constraints and maintainability remain authoritative.

When a non-trivial decision can benefit from current research, use current official/vendor/primary technical sources and strong engineering references as design input. Do not import a pattern merely because it is new or popular.

Optimization must remove unnecessary work at its owner/source. Extra throttles, duplicate caches, parallel implementations and workaround gates are not accepted as substitutes for architectural correction.

M2 closure now additionally requires a modernization disposition for each touched area: what is current/optimal, what was improved, what was deliberately retained, and why no materially safer or more efficient practical design was ignored.

### M2.172 remediation — broker lifecycle ownership

**ROOT-CORRECTED.** The cBot broker/PendingOrder lifecycle subscriptions now have one explicit owner with named handlers, an idempotent subscription guard, and deterministic unsubscribe in OnStop(). This follows the current cTrader event contract and removes the anonymous-handler lifecycle leak without introducing another event layer.

Evidence commit: e09b07f79a718f24dc72733d5bd91184393946ec.

### M2.173/M2.174/M2.176 remediation — initialization and shutdown lifecycle

**ROOT-CORRECTED.** Trading lifecycle event hookup is now transactional with rollback of every successfully attached handler if a later subscription fails, preventing partial initialization and duplicate subscriptions on retry. Initialization polling no longer performs a full RenderPanel() layout on every 250ms data-wait cycle; it updates the existing lightweight live-state/header owner and leaves full rendering to the canonical render lifecycle. Shutdown now enters an explicit quiescing state, stops the timer and removes lifecycle subscriptions before final state persistence, then flushes buffered persistence after the final writes. Queued startup calculation callbacks also refuse to execute after shutdown begins.

Evidence commits: 5639cbb15a4490d14ed8ec30d88255de57b8e83a and 41d1f565f0c816d2720e1879806f99bcc55ecfdb.

### M2.180 remediation — deterministic confirmation retention

Management confirmation retention no longer removes an arbitrary first-enumerated `HashSet` item. A dedicated FIFO order tracks confirmed idempotency keys and trimming removes the oldest confirmation first. This keeps the bounded-memory policy deterministic without creating a second semantic authority.

Evidence commit: 7aa44cf88fd0fe1ed9a2fe54d178a7a2f99079b8.

### M2 verification-contract maintenance

The architecture verifier was updated to understand the intentional `RuntimeInitializationLifecycle.cs` owner split and the canonical modern plan-label contract: adaptive filled box, semantic line color, contrast-aware text, and left-of-line placement. These are verifier alignment changes only; they do not weaken production assertions.

Evidence commits: 61358e556defb3bbd84aa9a5fe2542154dd8a298 and 450cc7a8b2a4f2492886fdae4ebf792483054f68.


### M2.166/M2.171 remediation — explicit management request status

**ROOT-CORRECTED.** RequestManagementCommand no longer returns a boolean that conflates rejection, queued submission, pending idempotency and broker confirmation. The platform-neutral ManagementCommandRequestStatus contract now distinguishes Rejected, Queued, AlreadyPending, AlreadyConfirmed and WriteFailed, with one contract policy for accepted/confirmed interpretation. Protection and lifecycle consumers now use the explicit status; broker-confirmed protection is not adopted from a merely queued request.

Evidence commits: 8d6c59857734fc055ea364410cf1473ecfef3406, 47a4a88d1f321f88c391395069f0976160343bb8, bc7156e22949df7e0b0e7e3f44a51b71bef5281a.

### M2.167/M2.168 remediation — remove management persistence/report I/O from request hot path

**ROOT-CORRECTED.** Management commands are now kept in the canonical in-memory command owner and marked dirty; serialization to LocalStorage and Device flush are deferred to the existing BufferedPersistenceCoordinator heartbeat/shutdown owner. ProcessManagementReports() is no longer called by request submission; it runs once per canonical runtime heartbeat, updates the same in-memory command state, and lets the existing persistence owner flush any resulting compaction. Failed device flushes retain the dirty state for retry.

Evidence commits: 5d8467c486cb5fe321fa2cdbbad44831f2a7fc8b, 6c49940cb84cba9fff30345d9c2afaaa523d1262, 34db7435586ed12d197cc8e8257a792a2277ce77, d4e7990546bebab87234ee6156db83f1dfecca12, 1497a3c0c11bf33e87e9a93a3e96fafa0979c3d7.

### M2.169 semantic naming — management request owner

**ROOT-CORRECTED.** The management coordinator and its consumers now use explicit Request* helper names (RequestClosePosition, RequestCancelPendingOrder, RequestModifyStopLoss, RequestModifyTakeProfit, RequestModifyTakeProfitLadder, RequestModifyTakeProfitPips). The naming expresses request/coordination semantics without implying broker confirmation, and no compatibility wrapper was added.

### M2.175 broker lifecycle listeners — necessity audit

**ROOT-CORRECTED / RETAINED BY PROOF.** All seven Indicator broker lifecycle listeners were traced to concrete read-only consumers: position-open recovery/protection binding, position-close outcome and lifecycle finalization, position-modification broker-confirmed protection state, and pending-order lifecycle/fill/cancel reconciliation. They do not mutate broker state. Removing them would break broker-confirmed state ownership, so they remain under the single Indicator lifecycle observer owner. No second broker observation layer was added.

### Modernization disposition — current M2 work

The management command path is being modernized as a deterministic local state owner plus deferred persistence, rather than by adding another queue/cache or an arbitrary timer gate. The existing runtime heartbeat and buffered-persistence owner are reused. The request/confirmation boundary is explicit and platform-neutral, and broker confirmation remains the only authority for adopted protection state. Remaining modernization work is contract/build-graph validation and authoritative CI/runtime verification.


Evidence commits for M2.169/M2.175: daf856bc98f249f5208c37f16c1b235fa028f353, 1bd942e8f544957b3903b414f02c1cb3406b1e2f, 74f80be779dedb02512798f36f7c123549f57272.


### M2.170 remediation — stale execution-owner documentation

**ROOT-CORRECTED / CLASSIFIED.** The current editing guide had one stale Indicator market-mutation owner path and now points to the cBot DemoMarketExecutionCoordinator. Older audit/roadmap documents that intentionally describe the pre-cutover Indicator ownership are retained as historical evidence and are not current architecture authority. Current operational documentation must point to the cBot mutation owners and platform-neutral contracts.


### Management terminal-state hardening

**ROOT-CORRECTED.** Broker `Expired` reports are no longer stored in the confirmation set. `AlreadyExpired` is a distinct request status with bounded FIFO retention, so an expired command can never be reported to a consumer as broker-confirmed. Confirmed and expired terminal identities remain deterministic and separately owned.

Evidence commits: b00e744a1bf055bf4d6248124323a8cfd5b6751a, 5a8cbb627b8e3bebb2dc2a10059912ccf885b653.


### M2.185 — stale current-owner documentation

**ROOT-CORRECTED.** `docs/CBOT-0-BOUNDARY-INVENTORY.md` and `docs/ARCHITECTURE.md` contained pre-cutover Indicator broker-owner paths in sections presented as current architecture. They now identify the cBot owners as current authority. `docs/CBOT-P0-EXECUTION-DEPENDENCY-CLOSURE.md` is explicitly classified as historical extraction evidence, not a current owner registry.

### M2.186 — stale audit exception for deleted broker owner

**ROOT-CORRECTED.** `tools/audit_exit_geometry.py` had an exception allowing the deleted `BrokerTakeProfitMutation.cs` owner to be treated specially. The exception is removed; any management TP request path is now checked uniformly against the current request-owner contract.

### M2.187 — duplicate/parallel-owner sweep

**AUDIT RESULT.** Targeted executable-owner sweep found one canonical production owner for market broker mutation, pending broker mutation, management request coordination, plan line rendering, plan label rendering, and realtime signal envelope consumption. No second production executor/renderer was introduced by M2. Remaining same-name matches are either canonical consumers/builders or historical/audit guards, not competing owners. Full repository-wide semantic duplicate scan remains part of the M2 closure gate and is not being declared complete from this targeted pass.


### M2.188 — canonical panel refresh ownership

**ROOT-CORRECTED.** The normal ready-state panel path is already owned by `RefreshPanelContentIfDue()` under the runtime heartbeat. Four secondary paths still synchronously invoked the heavy `RenderPanel()` renderer: calculation-readiness updates, runtime-fault handling, closed-bar MTF-wait handling, and cBot chart-lifecycle events. These calls were replaced with the canonical `RequestPanelContentRefresh()` invalidation owned by `PanelContentRefresh.cs`; the heartbeat remains responsible for applying the content refresh. This removes competing panel refresh clocks and prevents lifecycle/error events from rebuilding panel layout outside the canonical UI owner.

Evidence commits: c782be3f610f201ab377bc4bc1a3add4ef472c66, fa9e966239e9a11410c405fd320ffb6023a36dba, e58b113b020579acd81b500a242eb47631ec949b, 1cbff2996dfafb82dc6dd717458e0539d8a5f370, 9d1f404b37a30397fb1e3cd4d200f407a76e5b1a, b99172233f626a0c8017c1d578f9d7d864a6bbdd.

**Modernization disposition:** retained `RenderPanel()` as the sole full-layout renderer; reused the existing heartbeat/content owner; introduced only a canonical invalidation primitive, not a second renderer, cache, timer, or workaround. This also respects cTrader's main-thread UI model and keeps UI work on the indicator lifecycle owner.


## 2026-10-04 — M2-A.1 repository/build truth — forensic execution

### Current repository snapshot at exact M2 HEAD

HEAD: `e8809f8bfcd0db7687c76201e8b7ed3b6613976e`

Recursive Git tree truth:
- 1122 tracked files;
- 82 directories;
- 734 C# files;
- 669 Indicator C# files;
- 22 cBot C# files;
- 23 Contracts C# files;
- 210 Markdown files;
- 155 Python audit/tool files.

The earlier 1113/731/667/22/22/204 baseline remains historical baseline evidence. It is not the current tree count. The difference is now explicitly classified as repository evolution, not silently treated as an audit failure.

### M2.189 — M2 branch is structurally divergent from current main

**STATUS: OPEN / BLOCKING FOR FINAL-MAIN CLAIM**

Repository refs show:
- M2 HEAD: `e8809f8bfcd0db7687c76201e8b7ed3b6613976e`
- `main`: `ae1d4c84ccbeb15dc269bcf681b50481998a0a74`
- merge base: `a0f5ab1e6d7711d980320f8bb83ecbd4997a148b`
- M2 branch: 138 commits ahead of main and 67 commits behind main.

This is not a cosmetic branch-name issue. It means the current M2 source graph and current main source graph are not the same project state. A final M2 PASS or merge-readiness claim cannot be made until the canonical integration base is explicitly reconciled and the resulting exact HEAD is re-audited.

**Root cause:** M2 work evolved on a divergent branch while main continued to receive independent work.

**Impact:** A defect fixed only on one side can reappear during integration; file counts, owners, contracts and audit assertions can differ between the two histories.

**Required remediation:** reconcile M2 with the canonical project base before closure; do not overwrite either history and do not assume textual conflict resolution is semantic reconciliation. After reconciliation, rerun M2-A through the affected gates on the exact resulting HEAD.

**Verification:** GitHub compare API; current branch ref and main ref inspected directly.

### M2.190 — repository baseline counters are stale relative to exact HEAD

**STATUS: CLASSIFIED / NOT A PRODUCTION DEFECT**

The master baseline records 1113 files / 731 C# / 667 Indicator C# / 22 Contracts C# / 204 Markdown. Exact M2 HEAD contains 1122 / 734 / 669 / 23 / 210 respectively.

The discrepancy is explained by subsequent repository evolution and added audit/documentation/contract files. The baseline must remain immutable as historical evidence, while current phase documents must always report the exact-HEAD snapshot separately.

**Remediation:** current snapshot added above; future audits must never overwrite historical baseline counts.

### M2.191 — production source is explicitly recompiled into multiple contract-test assemblies

**STATUS: OPEN / BUILD-GRAPH REVIEW REQUIRED**

Current project files explicitly compile the same Indicator production source files into multiple tool assemblies. The current scan found **52 exact source-path overlaps** across the Decision, Planning, Runtime and Execution contract harness projects. Examples include:
- `NumericGuards.cs`;
- `MarketRegimeClassifier.cs`;
- `Decision.cs`;
- `RiskRewardMathRule.cs`;
- `TargetProgressionRule.cs`;
- `BrokerConfirmationPolicy.cs`;
- `LifecycleEventIdempotencyGuard.cs`.

The Runtime harness alone explicitly compiles 168 Indicator source files.

This is not yet classified as a production duplicate owner: the harnesses are deterministic acceptance executables that intentionally compile selected production rules in isolation. However, it is a **build-graph duplication risk** because one source file can exist in several independently compiled assemblies. A future change can make one harness's transitive context differ from another's, creating false confidence or inconsistent semantics.

**Root cause:** contract/acceptance harnesses use explicit source linking instead of a single shared test/contract assembly boundary.

**Impact:** maintenance drift, duplicate compile contexts, namespace/type-resolution differences, and possible test results that do not represent the exact production build graph.

**Required remediation:** before changing architecture, map each linked source to its reason for isolation. Where the same production rule is required by multiple harnesses, prefer one canonical reusable testable assembly or a shared test fixture boundary that preserves the exact production implementation, without copying or rewriting the rule. Do not solve this by creating another source copy.

**Verification:** all current `*.csproj` files inspected; 52 exact source-path overlaps calculated from explicit `Compile Include` entries.

### M2.192 — nested SDK pinning for the OSS benchmark is intentional and valid

**STATUS: VERIFIED / RETAIN**

Root `global.json` pins .NET 6 for the cTrader-oriented repository. The independent benchmark project targets .NET 8 and contains its own `tools/CFIP.StockIndicators.Benchmark/global.json` pinning SDK 8.0.425. Its workflow also explicitly installs .NET 8.

This is a deliberate toolchain boundary, not an accidental duplicate configuration. Removing the nested pin would make the benchmark depend on the repository-wide .NET 6 SDK and could break its target framework.

### M2.193 — four Python tools are not invoked by Source/Architecture CI

**STATUS: VERIFIED OBSERVATIONAL / RETAIN**

The source-check workflow invokes 151 Python scripts. Four current tools are not in that manifest:
- `tools/analyze_phase_11_3.py`
- `tools/analyze_runtime_log.py`
- `tools/analyze_signal_trace.py`
- `tools/benchmark_target_obstacle_cache.py`

Inspection shows these are observational/manual analyzers or a structural benchmark; they do not mutate live trading logic. They are intentionally retained as operator/research tools and are not dead production paths.

### M2.194 — two ExecutionIntent types are distinct semantics, not a duplicate execution owner

**STATUS: VERIFIED / RETAIN**

There are two files with the same basename:
- `src/CFIP.Contracts/ExecutionIntent.cs` — immutable cross-boundary Indicator↔cBot contract;
- `src/CFIP.Indicator/Core/Models/ExecutionIntent.cs` — internal Indicator-side source model with legacy/internal planning fields.

They live in different namespaces and serve different layers. Current architecture documentation explicitly maps the internal Indicator model into the canonical `CFIP.Contracts.ExecutionIntent` boundary.

This is therefore not classified as a second execution authority. It remains a **naming-risk observation**: future contributors can confuse the two types. Renaming is deferred until the full dependency graph is mapped; no speculative rename is introduced during M2-A.

### M2.195 — Contracts platform-neutrality verified at source level

**STATUS: PASS FOR M2-A.1 SUBCHECK**

The 23 current files under `src/CFIP.Contracts` were inspected at the source/project boundary. They contain no cTrader/cAlgo broker/chart dependency, and `CFIP.Contracts.csproj` has no cTrader package reference. The production Indicator and cBot each reference Contracts rather than the reverse.

The codecs use standard .NET serialization only. This satisfies the platform-neutral cross-boundary direction for the inspected Contracts layer.

### M2.196 — CI build graph does not directly build the solution file

**STATUS: OPEN / VERIFY INTENT**

The solution contains the three production projects, but `.github/workflows/ci-build.yml` builds the Contracts project, cBot, Indicator CI harness, contract harnesses, shadow tests and preflight projects individually rather than invoking `dotnet build CFIP.Indicator.sln`.

This is not automatically wrong because the Indicator CI project compiles the Indicator source graph and preflight references the production Indicator project. However, the exact relationship between solution membership, CI coverage and the cTrader target artifact must be proven rather than inferred.

**Required remediation:** prove that every solution production project and every required compile item is covered by an authoritative CI target, and document why the solution itself is not the canonical CI build command. If a project is intentionally outside CI, classify it explicitly.

### M2-A.1 disposition

M2-A.1 is **NOT CLOSED**.

Verified:
- exact repository inventory;
- current project/solution manifests;
- current Contracts project boundary;
- current workflow manifest existence;
- intentional observational tools;
- intentional nested SDK boundary;
- distinct ExecutionIntent semantics.

Open:
- M2.189 branch/base reconciliation;
- M2.191 shared-source build-harness architecture;
- M2.196 solution-vs-CI build coverage;
- full conditional-compilation, generated-file, unreachable-source and semantic duplicate analysis.

**Next exact work item:** continue M2-A.2/A.3 from the reconciled repository truth; do not move to M2-B until the remaining M2-A build/reachability checks are dispositioned.


## 2026-10-04 — M2-A.2/A.3 deep repository scan

### Verified

- Recursive tree: **1122 tracked files** at the audited M2 HEAD.
- No backup/copy/temp/bak/old/generated-looking source artifacts were found by filename heuristic.
- No tracked build-output artifacts (`bin/`, `obj/`, DLL/PDB, VS user state) were found.
- No `#if/#elif/#else/#endif/#define/#undef` conditional-compilation directives were found by repository search in the audited source scopes. Conditional-compilation dead-code risk therefore remains low, but not yet globally closed because reflection/string reachability still requires source-graph analysis.
- Only four duplicate basenames exist:
  1. `Program.cs` across independent executable harnesses;
  2. `README.md` at root/OSS/benchmark scopes;
  3. `global.json` at root and nested .NET 8 benchmark scope;
  4. `ExecutionIntent.cs` in `CFIP.Contracts` and Indicator internal models.
- The two `ExecutionIntent` types are currently proven to have different semantic roles:
  - `CFIP.Contracts.ExecutionIntent` is the cross-boundary immutable execution contract.
  - `cAlgo.ExecutionIntent` is an internal Indicator planning/model object carrying trigger/zone/M5 creation metadata.
  They are therefore **not currently classified as duplicate owners**, but the identical type name is a maintainability/architecture ambiguity.

### Findings

**M2-A.2-F01 — Identical `ExecutionIntent` type name across boundary and internal model.**  
Status: **OPEN — naming/architecture clarity**.  
No behavior change should be made until all references are traced. If renamed,
the internal type must be renamed only once and all consumers migrated together;
no alias/parallel compatibility type should be introduced.

**M2-A.2-F02 — Nested .NET SDK manifest is intentional but needs explicit build-boundary documentation.**  
`tools/CFIP.StockIndicators.Benchmark/global.json` pins .NET 8 while the
repository root pins .NET 6. This is currently coherent because the benchmark
has its own SDK boundary and CI explicitly uses .NET 8.  
Status: **VERIFIED INTENTIONAL**, documentation normalization still desirable.

**M2-A.2-F03 — Manual-source contract harnesses remain the main build-graph drift risk.**  
The selected `Compile Include="../../src/..."` lists in Decision/Planning/
Execution/Runtime contract harnesses are test coverage manifests, not production
owners. Their exact source selection must still be reconciled against their
contract scope and the full Indicator CI build.  
Status: **OPEN**.

### Dead-code scan disposition

Filename-level backup/generated scan: **PASS**.  
Conditional-compilation scan: **PASS for discovered directives**.  
Full symbol reachability, reflection/string entry points, unused members,
dead event/timer/chart/persistence paths: **NOT CLOSED** and remain mandatory
for M2-A/B.



## 2026-10-04 — M2-A.1 repository/build truth audit

**M2-A.1 remains OPEN.** The exact findings and evidence are recorded in `docs/M2-A1-REPOSITORY-BUILD-TRUTH-2026-10-04.md`.

Confirmed at HEAD `3123030fa058200d1847203efe28454cf2172ada`:

- recursive tree: 1122 tracked files; previous master baseline: 1113, so +9 files require reconciliation;
- primary solution contains only Indicator, Contracts and cBot production projects;
- contract/preflight/benchmark/shadow verification projects are outside the primary solution graph;
- 52 platform-neutral source files are explicitly compiled into more than one contract harness assembly;
- Runtime Contracts has 168 explicit Compile entries;
- Indicator CI separately compiles the full Indicator source tree;
- Contracts project has no targeted `cAlgo.API` dependency evidence.

### Architecture disposition

The current source is physically single-owner but the verification build graph is overly coupled to `src/CFIP.Indicator`. The target architecture is to place genuinely platform-neutral shared rules/models behind explicit project references, keeping Indicator and cBot as platform adapters/owners and Contracts as the cross-boundary transport layer. No blind extraction is being performed until the dependency audit establishes the exact shared surface.

No runtime/build PASS is claimed from this static audit.

Next: **M2-A.2 — duplicate/near-duplicate files, partial classes, conditional compilation, generated/obsolete/unreachable artifacts, and build reachability reconciliation.**
