# CFIP — Cross-Chat Continuation State

Last updated: 2026-10-01

## Phase closeout

CR3.2 is verified complete and merged to main via PR #98, merge commit `af7eb503dd661d508afcc7619c72f74cea1b3a0f`. The final implementation revision `c787c88538e82fbe795f09b7632061575ff023db` passed Source/Architecture (run 1820), Runtime Acceptance (run 1629) and cTrader Compile (run 1813).

CR3.1 remains part of the completed chain: PR #95, merge commit `f482d2f76cdf37cca87fabc5b11b3d8c0a7edac7`, with Source/Architecture run 1808, Runtime Acceptance run 1617 and cTrader Compile run 1801.

## Phase-sequence integrity

Prompt 4 is explicitly D1–D10 in the canonical remediation roadmap. A
repository search on 2026-10-01 found no `CR4.11`, `D11`, `Phase 4.11` or
`Prompt 4.11` entry. Therefore the implemented sequence is
`CR4.10 → CR5.1 → CR5.2 → CR5.3`, with no authoritative CR4.11 phase omitted.

## Authoritative order

1. Claude review remediation: docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md
2. Master roadmap: docs/ROADMAP.md
3. Only after CR-FINAL: local cBot separation Track 12A.

## Active phase
### CR7.1 / G1 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — Source/Architecture #2262, Runtime Acceptance #2071 and cTrader Compile #2255 passed on final G1 code HEAD 2f1cb933a2c2407e1fe33302cf72f538f090ba91; merged via PR #138 as b8144c2f1edc62730b7a0723be3746afe6353851.**

**تأیید می‌کنم** — G1 prevents live risk expansion by separating existing broker-stop health from new-stop acceptability.

Implementation:
- TargetObstacleCacheKey.GetHashCode() now covers all equality fields, removing CS0659;
- existing-stop directionality is owned by ManagedStopProtectionRule.IsExistingStopHealthy;
- new SL candidates still use live market/minimum-distance validation;
- broker reconciliation, bound-plan protection and broker-state evaluation consume the correct existing-stop health semantics;
- ProtectionProgressionRule remains the only authority allowed to replace an already healthy stop;
- deterministic G1/hash runtime contracts and accumulated static audit are wired;
- audit_project_integrity.py now distinguishes methods by containing type.

Verification:
- Source/Architecture PASS — run 36868297463 / workflow #2262;
- Runtime Acceptance PASS — run 36868297578 / workflow #2071;
- cTrader Compile PASS — run 36868297556 / workflow #2255.

Manual boundary:
- target-terminal broker stop modification and broker minimum-distance behavior;
- restart/reconnect and live panel/runtime behavior;
- empirical signal-quality/profitability remain manual.

**Historical transition after G1: CR7.2 / G2 — Retest adverse-momentum semantics and rejection telemetry.**


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

Prompt 4, Prompt 5 and Prompt 6 are mandatory remediation tracks before CR-FINAL.
**Current: CR6.9 / F3 — Orphaned managed-position protection.**

CR5.1 through CR5.8 are verified complete at repository level. CR5.8 was merged
to `main` via PR #124 with merge commit
`03a569d6a18f1b6cbc3524dabc24ea713439c325`.
CR-FINAL remains paused until CR6.1–CR6.9 are completed or explicitly
documented as verified/deferred with evidence. Target-terminal acceptance
remains required afterward.
## Completed before this checkpoint

- CR-0 audit gate
- CR1.1 session/EOD + closed-period references
- CR1.2 daily-loss accounting/lock
- CR1.3 news guard refresh/state handling
- CR1.4 closed-bar cycle ordering/readiness state
- CR1.5 hot-path/cache/logging optimization
- CR1.6 FVG quality discrimination
- CR1.7 threshold truth + volume audit
- CR1.8 managed identity boundary
- CR1.9 minor cleanup and documentation
- CR2.1 structure/CHoCH/MSS/sweep/divergence/rejection semantics
- CR2.2 reaction/reversal integrity
- CR2.3 unified indicator-quality thresholds
- CR2.4 pending-order decision arbiter
- CR2.5 lifecycle ordering and outcome aggregation
- CR2.6 OrderBlock quality and cache discipline
- CR2.7 WaveTrend mathematical correctness
- CR2.8 historical rendering semantics and cost
- CR2.9 structural stop, divergence and rejection guardrails
- CR3.1 live invalidation and false-signal semantics

## CR3.1 implementation record

- C1: invalidation is evaluated once per canonical closed M5 result; structural invalidation uses confirmed swing candidates; rejected broker closes remain RecoveryRequired and do not advance successful-exit bookkeeping.
- C2: soft adverse-R has an explicit safety flag; FalseSignalAdverseR is centrally validated and capped to a known adverse broker-stop R envelope; BUY/SELL symmetry and invalid inputs are runtime-tested.
- Public parameter inventory is now 568 because of the explicitly required safety flag; all parameter-count, semantic, integrity and optimization audits were reconciled.

## Rules for every continuation

- Complete exactly one CR phase per implementation response.
- Perform the project-wide routine audit: Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning.
- Run the performance/code-cleanliness audit in the same phase.
- Keep public parameter name/type/DefaultValue unchanged unless a new safety parameter is explicitly required.
- Every fix gets its own fix(<ID>): ... commit.
- Every logical change gets real deterministic behavior tests.
- Do not start or advance cBot separation until CR-FINAL passes.
- Never claim target-terminal verification from CI alone.

## Next transition

The next implementation response must execute **CR6.9 / F3 only**.
Track 12A and CR-FINAL remain blocked until the full Prompt 6 chain is closed.
## CR3.2 implementation record

- C3: the former proxy expected-value heuristic now has an explicit Reward Quality Floor semantic owner in Core math; numerical behavior for valid inputs and public parameter defaults were preserved.
- C4: early prediction scoring is centrally owned; directional share and absolute evidence strength are explicit; tiny-total/high-ratio evidence cannot qualify as a strong early prediction.
- Runtime contracts and the CR3.2 static gate cover reward-quality semantics, BUY/SELL symmetry, centralized weights and low-total/high-ratio behavior.
- Manual target-terminal validation remains required for empirical signal-quality conclusions; Track 12A remains blocked until CR-FINAL.

## Next transition

CR3.2 is closed. CR3.3 has been implemented and merged to main. The next implementation response must execute CR3.4 only. Track 12A remains blocked until CR-FINAL.


## CR3.3 implementation record

CR3.3 was merged to main in PR #99, merge commit `1e8681f57cb48bc51367b3df2ca129803d93d0c4`.

- partial TP retry is bounded by stage and canonical closed-M5 identity;
- server TP ladder stage evidence is broker-confirmed and remains authoritative through ladder collapse;
- post-partial and server-side break-even use the canonical spread-aware rule;
- peak-RR recovery uses broker EntryTime plus closed-bar historical extremes;
- target progression is monotonic and cannot move backward.

Target-terminal broker timing, restart/reconnect behavior and empirical outcome validation remain manual acceptance boundaries.

## Next transition

CR3.3 is closed. CR3.4 is verified complete on PR #100. The next implementation response must execute CR3.5 only. Track 12A remains blocked until CR-FINAL.


## CR3.4 implementation record

CR3.4 was implemented on branch `phase/cr3-4-execution-ui-popup-reliability` and all three required CI gates passed on commit `d8bb61083a043ce46c11b92dc8068b029c0c5add`.

- explicit Auto Trade runtime re-arm is owned by RuntimeFaultStateMachine and is independent of the public Indicator configuration parameter;
- non-Healthy runtime states remain fail-closed;
- popup alerts use a bounded critical-first queue;
- popup rendering is processed from the timer boundary rather than directly from alert emission;
- direct popup overwrite call sites were removed from lifecycle reminders;
- deterministic runtime and static phase contracts are wired into CI.

Manual target-terminal checks remain required for hosted UI timing and real cTrader runtime interaction.

## Next transition

CR3.4 is closed. The next implementation response must execute CR3.5 only. Track 12A remains blocked until CR-FINAL.


## CR3.5 implementation record

CR3.5 is verified complete and merged to main via PR #101, merge commit `2c31232b7d1f16e88e89e693d7d74fd8a5eedab6`.

- `ConfidenceCalibrationKey` now has deterministic structural equality and consistent hashing;
- managed outcome calibration retains broker-history aggregated realized R and exposes average realized R beside observed win rate and sample count;
- Decision/panel calibration diagnostics expose AVG R without changing thresholds or probability semantics;
- plan reward-integrity rejections emit explicit bounded `PLAN_REWARD / REJECTED` telemetry reasons;
- repeated identical reward rejection telemetry is deduplicated per M5/reason pair;
- Decision Contracts and a dedicated CR3.5 static audit cover the new semantics.

Manual target-terminal broker-history, restart/reconnect and empirical outcome validation remain required.

## Next transition

CR3.5 is closed. The next implementation response must execute **CR-FINAL** only. Track 12A remains blocked until CR-FINAL.


## Prompt 4 remediation insertion — 2026-09-30

Prompt 4 D1–D10 has been added to `docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md` and `docs/ROADMAP.md`.

Order:
`CR4.1 → CR4.2 → CR4.3 → CR4.4 → CR4.5 → CR4.6 → CR4.7 → CR4.8 → CR4.9 → CR4.10 → CR-FINAL`

The findings remain static-review hypotheses until each item is independently verified. No default trading threshold, RR floor, confidence threshold or public parameter identity is to be changed implicitly.



### CR4.4 / D4 closeout — 2026-10-01

CR4.4 is complete and merged to `main` via PR #105, merge commit `1b1a1762fee960e65903880f2566b3355a9a7431`.

Implementation:
- stable-prefix caching for RSI, MACD, SuperTrend and Parabolic SAR;
- bounded incremental 161-bar rolling cache for fixed-window adapters;
- centralized OSS constants and warm-up contracts;
- conservative history/cache invalidation;
- OBV retained as diagnostic/research data but removed from independent confluence vote/count evidence;
- runtime/static verification and cache benchmark added;
- Source/Architecture PASS — run 36779376240;
- cTrader Compile PASS — run 36779376267;
- Runtime Acceptance PASS — run 36779376210;
- OSS indicator benchmark PASS — run 36779376203;
- CR4.4 static gate PASS across the accumulated audit chain.
- Source/Architecture workflow now includes the CR4.4 static gate.

Next phase: **CR4.5 / D5 — Per-timeframe regime semantics.**
Target-terminal acceptance remains a later manual boundary.


### CR4.5 / D5 closeout — 2026-10-01

CR4.5 / D5 was completed and merged to `main` via PR #106, merge commit `c529c38ecea09e4b1ad85bc465e1ba12739ff95b`.

Implementation record:
- preserved the dedicated M5 regime path;
- added bounded per-timeframe regime caching for non-M5 Bars series;
- exposed normalized regime, quality and stability on each Frame;
- made frame scoring consume each frame's own normalized regime;
- centralized regime normalization and kept UNKNOWN explicitly neutral;
- added deterministic BUY/SELL symmetry and UNKNOWN-neutrality runtime contracts;
- hardened the CR4.5 static audit so source formatting cannot create false failures while cache invalidation and per-timeframe ownership remain checked.

Verification:
- Source/Architecture PASS — run 36781736303, including CR4.5 static gate;
- cTrader Compile PASS — run 36781736379;
- Runtime Acceptance PASS — run 36781736247.

Boundary:
- no public parameter name/type/DefaultValue changed;
- no default regime threshold, RR, confidence or execution policy changed;
- no second decision or execution authority introduced;
- target-terminal timing, replay and empirical signal-quality validation remain manual acceptance items.

Next transition: **CR4.6 / D6 — Frame-scoring constant ownership.**

### CR4.6 / D6 closeout — 2026-10-01

CR4.6 was completed on `phase/cr4-6-frame-scoring-constants` and merged to `main` via PR #107, merge commit `edf1f50511082a4eb7359217d6ae80853348c71c`.

Implementation:
- centralized frame-scoring constants under `Core/Math/FrameScoringConstants.cs`;
- migrated MarketFrameScoringService to that single owner without changing existing numerical values;
- added deterministic runtime coverage for the constant owner;
- added CR4.6 to the accumulated Source/Architecture audit chain;
- separately corrected a formatting-dependent false positive in the CR4.5 cache-refresh static audit.

Verification on implementation HEAD `a134b72e863067291fb04eae7ac530c3fbcae999`:
- Source/Architecture PASS — run 36783220015 / #1978;
- Runtime Acceptance Contracts PASS — run 36783220007 / #1787;
- cTrader Compile PASS — run 36783220117 / #1971.

Safety boundary:
- no public parameter name/type/DefaultValue changed;
- no default RR/confidence/stop/threshold tuning;
- no second decision or execution authority introduced;
- target-terminal timing, replay and empirical signal-quality validation remain manual.

### CR4.7 / D7 closeout — 2026-10-01

CR4.7 was completed on `phase/cr4-7-target-pipeline` and merged to `main` via PR #108, merge commit `2a586ca353f79d6151b9b8375edf46cb94df7880`.

Implementation:
- one Core owner for target-side/RR/extension/spacing/progression/HTF candidate constraints;
- deterministic M5 setup-age versus HTF elapsed-age semantics;
- source-age propagation through HTF, D1/W1 prior-period, pivot and liquidity target sources;
- bounded `PLAN_TARGET` per-stage rejection telemetry and explicit rejection taxonomy;
- pre-scan target reward-envelope feasibility detection;
- dedicated telemetry/stage-feasibility owners keeping `TargetSelector` orchestration-only;
- deterministic planning fixtures covering TP1–TP4 acceptance, below-minimum RR rejection and BUY/SELL symmetry.

Verification on implementation HEAD `441611a8d1ca513ee332f9a8a006a3f37eb9a727`:
- Source/Architecture PASS — run #1999;
- Runtime Acceptance Contracts PASS — run #1808;
- cTrader Compile PASS — run #1992.

Deterministic evidence:
- primary TP1–TP4 fixtures: 4/4 accepted;
- below-minimum-RR fixtures: 4/4 rejected;
- SELL mirror fixtures: 4/4 accepted;
- TP4 envelope reachability: 1/4 fixed risk/ATR fixtures.

Safety boundary:
- no public parameter name/type/DefaultValue changed;
- no default RR/confidence/SL/target-age tuning;
- no second decision or execution authority introduced;
- target-terminal timing, replay and empirical signal-quality/profitability validation remain manual.

### CR6.1 / F1 closeout — 2026-10-01

CR6.1 / F1 was completed and merged to `main` via PR #127, merge commit
`a7a03a4403a7c6681f95ac0b053344144e933b6e`.

Repository gates on the final F1 head:
- Source/Architecture PASS — run #2180;
- Runtime Acceptance Contracts PASS — run #1989;
- cTrader Compile PASS — run #2173.

### Current implementation phase

**CR6.7 / F8 — Target-obstacle rejection telemetry and distant-target semantics.**

F4 was completed and verified on branch
`phase/cr6-3-f4-threshold-transparency`; PR #129 passed Source/Architecture,
Runtime Acceptance Contracts, and cTrader Compile on commit
`5dd9a8a8bb0fb8172ac40b7336ce86e0bb5c3c2a`.
### Prompt 6 remediation insertion — 2026-09-30

Prompt 6 F1–F9 has been added to `docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md` and `docs/ROADMAP.md`.

Order:
`CR6.1 → CR6.2 → CR6.3 → CR6.4 → CR6.5 → CR6.6 → CR6.7 → CR6.8 → CR6.9 → CR-FINAL`

The F-series remains static-review hypotheses until each item is independently verified. Behavior-changing fixes, especially Breakout/trap, Aggressive RR/stop and timeframe-scenario semantics, require explicit approval and evidence.

### Prompt 5 remediation insertion — 2026-09-30

Prompt 5 E1–E8 has been added to `docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md` and `docs/ROADMAP.md`.

Order:
`CR5.1 → CR5.2 → CR5.3 → CR5.4 → CR5.5 → CR5.6 → CR5.7 → CR5.8 → CR-FINAL`

The E-series remains static-review hypotheses until each item is independently verified. No public parameter name/type/DefaultValue or default RR/confidence/stop threshold may be changed implicitly.

### CR4.1 continuity contract

D1 must be audited first. It covers learning-memory identity, decision-affecting parameter fingerprinting, account scoping, PositionId collision isolation and legacy schema migration.

Next implementation response: **CR4.1 only**.


### CR4.1 closeout — 2026-09-30

CR4.1 / D1 was completed and merged via PR #102, merge commit `071baf7ab0bda6df8f1d06c9ecbf9d28810e33d1`.

Implementation:
- canonical Core learning-memory identity rule;
- decision/result-only fingerprint;
- broker/account/type/live-demo scoping;
- v2 memory/snapshot identity and conservative legacy migration;
- account-switch memory reload;
- account-scoped archive/runtime-log prefixes;
- deterministic runtime contract + CR4.1 static audit.

Repository verification:
- Source/Architecture: PASS, workflow run #2021;
- cTrader Compile: PASS, workflow run #2014;
- Runtime Acceptance Contracts: PASS, workflow run #1830.

Verification boundary:
- GitHub exposed no retrievable CI status records for the merge at closeout; no CI PASS is claimed.
- target-terminal account switching, restart and broker History migration semantics remain manual.


### CR4.2 closeout — 2026-09-30

CR4.2 / D2 was completed and merged to main via PR #103, merge commit `05a91cb9764f7ee86fcaaba0d7dede540c4b6e1c`. with a centralized bounded file-write owner, startup History read/write probe, persistence health diagnostics, and account-scoped Signal Trace archive identity.

Target-terminal verification remains required for exact cTrader build, actual History path, AccessRights.None behavior and restart/account-switch persistence.

Next phase: CR4.3 / D3.

### CR4.3 closeout — 2026-09-30

CR4.3 / D3 was completed and merged to main via PR #104, merge commit `d24b26de3ddf3709c8ea5e94f97a9f533b9b33dc`. The implementation head before merge was `7defa4731ebd1e60d87d88a1659d4307607c46ab`.

Implementation record:
- trace capture now occurs exactly once per canonical new closed M5 boundary, after decision/execution state is finalized;
- SignalTraceId is deterministic and account/configuration/symbol/timeframe scoped;
- Plan and Preview geometry require exact closed-M5 + bar-open timestamp + direction lineage;
- realized outcomes carry the originating SignalTraceId for research-only historical joins;
- trace archive v3 and outcome archive v2 remain buffered and prior-row readable;
- aggressive fill-created plans preserve source trace identity; recovery plans remain opt-in and are not attributed retroactively;
- deterministic runtime contracts and CR4.3 static audit are green.

Verification:
- Source/Architecture PASS — run 36776384440;
- cTrader Compile PASS — run 36776384275;
- Runtime Acceptance PASS — run 36776384489.

Boundary:
- target-terminal replay, restart/reconnect timing and empirical quality/profitability validation remain manual;
- no public parameters or default trading thresholds were changed.

Next phase: **CR4.4 / D4 — Skender/OSS numerical stability and incremental caching.**

### CR4.8 / D8 implementation record

CR4.8 / D8 is complete on branch `phase/cr4-8-tp1-directional-defence`.

Implementation:
- canonical target-side validation now flows through `PriceProtectionRule.ValidateTarget`;
- invalid TP1 is rejected before plan materialization can expose it;
- BuildPlan propagates materialization rejection fail-closed;
- protection and reward-integrity boundaries independently enforce TP1 direction;
- deterministic BUY/SELL valid and wrong-side fixtures cover both candidate and protection semantics;
- the CR4.8 static gate is wired into the accumulated Source/Architecture workflow.

Boundary:
- no public parameter/default or RR/SL/confidence tuning;
- no second decision/execution authority;
- target-terminal runtime, broker lifecycle ordering, replay and empirical signal-quality validation remain manual.

Next transition: **CR4.9 / D9 — Live reversal action/alert semantics.**


### CR4.9 / D9 implementation record

CR4.9 / D9 is implemented on branch `phase/cr4-9-live-reversal-semantics`.

Implementation:
- one Core owner for live-reversal direction/confidence/action semantics;
- opposite-direction-only reaction/frame evidence;
- bounded reversal episode alert state;
- confirmed-close episode reset;
- retained-position state is WAIT, not BLOCKED;
- accepted close is EXIT_REQUESTED, not EXECUTED;
- missing-position path requests broker reconciliation/recovery and does not synthesize closure/outcome;
- deterministic D9 contracts and static audit are wired into CI;
- no public parameter/default or trading threshold tuning.

Verification boundary:
- repository Source/Architecture, Runtime Acceptance and cTrader Compile remain to be read from the PR checks;
- target-terminal broker acknowledgement ordering, restart/reconnect timing, panel behavior and empirical outcome validation remain manual.

Next transition: **CR4-FINAL repository integration gate after D10 repository verification.**

### CR4.10 / D10 closeout — 2026-10-01

CR4.10 / D10 is **VERIFIED COMPLETE** and merged to `main` via PR #111, merge commit `3371b9790902c4d35e4e1e28522dee42f93af861`.

Repository verification:
- Source/Architecture: PASS — run `36790307897` (including the accumulated CR4.10 static gate and prior audits);
- Runtime Acceptance Contracts: PASS — run `36790307911`;
- cTrader Compile: PASS — run `36790307893`;
- OSS/Registry Benchmark: PASS — run `36790307937`.

Implementation and safety record:
- centralized native readiness and numeric safety in Core `NativeIndicatorReadinessRule`;
- hardened native wrappers, MACD prior-sample access and frame/scoring/regime boundaries;
- replaced linear native registry lookup with reference-identity dictionary lookup;
- added deterministic D10 runtime contracts, static audit and lookup benchmark;
- preserved public parameter identity/defaults and trading thresholds;
- no second decision/execution authority introduced.

Manual boundary remains:
- target-terminal readiness/panel timing;
- broker lifecycle ordering;
- restart/reconnect;
- empirical signal-quality/profitability.

### CR5.1 / E1 closeout — 2026-10-01

CR5.1 / E1 is **VERIFIED COMPLETE** and merged to `main` via PR #114, merge commit `62a119bef79e5a978f1f2ed66a913fa11a1bb2d4`.

Repository verification:
- Source/Architecture: PASS — PR #114 head run `36791329486`; merge run `36791455294`.
- Runtime Acceptance Contracts: PASS — PR #114 head run `36791329385`; merge run `36791455156`.
- cTrader Compile: PASS — PR #114 head run `36791329369`; merge run `36791455288`.

Implementation/safety:
- canonical `StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(...)`;
- all E1 dual-cap consumers migrated;
- deterministic truth-table contract and repository-wide E1 audit;
- public risk parameters/defaults and trading thresholds preserved;
- no decision/execution authority change.

Manual boundary remains:
- target-terminal startup/readiness/panel timing;
- broker lifecycle ordering;
- restart/reconnect;
- empirical signal-quality/profitability.

### CR5.2 / E2 closeout — 2026-10-01

CR5.2 / E2 is **VERIFIED COMPLETE** and merged to `main` via PR #116, merge commit `10e01bd2610ce0c42b6d365f55fae24c75a3edfb`.

Repository verification:
- Source/Architecture: PASS — run `36792555340` / workflow #2072, including `audit_phase_5_2.py` and the accumulated routine/optimization audits;
- Runtime Acceptance Contracts: PASS — run `36792555225` / workflow #1881;
- cTrader Compile: PASS — run `36792555189` / workflow #2065.

Implementation/safety:
- canonical swing highs/lows replace raw candle-extreme liquidity forecasts;
- broken liquidity is rejected through the canonical active-unbroken rule;
- multiple valid liquidity forecasts are ordered by distance and discriminated through the existing `MinimumTpSpacingAtr`;
- session targets retain the existing `SessionWindowRule` + `SessionStartUtc` / `SessionEndUtc` semantics;
- no public parameter/default, RR/confidence/stop/target threshold or decision/execution authority changed.

Manual boundary remains:
- target-terminal startup/readiness/panel timing;
- broker lifecycle ordering;
- restart/reconnect;
- empirical signal-quality/profitability.

### CR5.3 / E3 closeout — 2026-10-01

CR5.3 / E3 is **VERIFIED COMPLETE** and merged to `main` via PR #119, merge commit `96530088a4216eb4a3f8caae9595987d98c0a27e`.

Repository verification:
- Source/Architecture: PASS — run `36793867203` / workflow #2083, including `audit_phase_5_3.py` and the accumulated routine/optimization audits;
- Runtime Acceptance Contracts: PASS — run `36793867170` / workflow #1892;
- cTrader Compile: PASS — run `36793867168` / workflow #2076.

Implementation/safety:
- one Core owner for the established independent-evidence score and independent-family group counting;
- four families: Structural, Location, Trend-Momentum and Context;
- correlated observations inside a family count as one group;
- decision and parallel candidates expose group-count provenance separately from the unchanged 0–8 behavior-driving score;
- duplicate Analysis-layer calculator removed;
- no public parameter/default, RR/confidence/stop/target/actionability/execution threshold or decision/execution authority changed.

Manual boundary remains:
- target-terminal startup/readiness/panel timing;
- broker lifecycle ordering;
- restart/reconnect;
- empirical signal-quality/profitability.

### CR5.4 / E4 closeout — 2026-10-01

CR5.4 / E4 is **VERIFIED COMPLETE** and merged to `main` via PR #120, merge commit `782bca41cd37071c79f2cdfa12f712cefc045c8c`.

Repository verification on final implementation head `6d89f010fa6d292d201ef79e37af5b162438f854`:
- Source/Architecture: PASS — run `36795710379` / workflow #2096, including `audit_phase_5_4.py` and the accumulated routine/optimization audits;
- Runtime Acceptance Contracts: PASS — run `36795710374` / workflow #1905;
- cTrader Compile: PASS — run `36795710377` / workflow #2089.

Implementation/safety:
- preserved absolute pending Stop/Limit Entry/SL/TP intent across placement;
- fail-closed snapshot creation and post-fill reconciliation;
- reconciled actual broker fill against the original absolute plan;
- preserved safer broker SL and more progressive broker TP;
- resynced Advanced/server-side TP protection from the reconciled absolute ladder;
- protected against PositionOpened event-order inversion;
- cleared stale snapshot state on cancellation;
- failed reconciliation remains RecoveryRequired;
- no public parameter/default, RR/confidence/stop/target/actionability/execution threshold or decision/execution authority changed.

Manual boundary remains:
- actual Stop/Limit fill divergence;
- broker-side final protection and Advanced Protection ladder;
- rejection timing;
- restart/reconnect;
- empirical signal quality/profitability.

### CR5.5 / E5 closeout — 2026-10-01

CR5.5 / E5 is **VERIFIED COMPLETE on PR #121**; implementation head
`5b34afbdc7ed252a3fabc68cdb9859818cb911e6`.

Implementation record:
- shared closed-M5 execution/entry/stop/risk geometry is cached once per active M5
  and direction; platform-specific ExecutionModel state remains separate from
  the runtime-neutral Core geometry;
- tactical parallel evaluation and candidate materialization share that geometry;
- TradePlanRegistry is the authoritative parallel candidate owner;
- Core `ParallelScenarioSelectionRule` owns identity, coverage, replacement and
  display selection;
- MicroReaction parallel presentation is fail-closed on exact closed-M5 identity,
  matching direction, closed confirmation and the existing strong-quality floor;
- Decision records `ReactionConfirmedM5` and `ReactionConfirmedDirection`;
- deterministic E5 runtime contracts and static audit are wired;
- accumulated Phase 11.4 audit was reconciled to the Core scenario-selection owner.

Verification:
- Source/Architecture PASS — workflow run `36836762005`;
- Runtime Acceptance Contracts PASS — workflow run `36836762039`;
- cTrader Compile PASS — workflow run `36836762006`.

Safety/manual boundary:
- no public parameter name/type/DefaultValue or default trading threshold changed;
- no second decision/execution authority introduced;
- target-terminal intrabar timing, panel presentation, broker lifecycle,
  restart/reconnect and empirical signal-quality/profitability remain manual.

### CR6.1 / F1 closeout — 2026-10-01

CR6.1 / F1 is **VERIFIED COMPLETE on PR #127**, implementation head
`b37ebf00bbd835d7cab8a192842752be7238d703`.

Implementation record:
- canonical Core `RewardPathGeometryRule` owns opposing-zone direction and target-path geometry;
- FVG/OB obstacle creation consistently uses `-direction`;
- managed FVG lifecycle/mitigation is applied before obstacle caching;
- fully mitigated FVGs and broken OBs are excluded;
- candidate obstacle snapshots are cached per Bars/index/direction while
  target-specific geometry remains live;
- higher-timeframe M15/M30/H1/H4 reward-path checks share the same owner;
- deterministic F1 runtime contracts and the accumulated static audit are wired.

Verification:
- Source/Architecture PASS — run #2177;
- Runtime Acceptance Contracts PASS — run #1986;
- cTrader Compile PASS — run #2170;
- F1 static audit PASS.

Safety/manual boundary:
- no public parameter name/type/DefaultValue or RR/confidence/SL/TP threshold changed;
- no decision/execution authority changed;
- target-terminal replay, zone mitigation timing, warm-cache behavior and empirical
  signal-quality/profitability remain manual.

Next phase: **CR6.4 / F5 — Smart-threshold regime identity and hidden REVERSAL dead path.**

## Current active phase

### CR6.4 / F5 closeout — 2026-10-01

CR6.4 / F5 is **VERIFIED COMPLETE on branch phase/cr6-4-f5-smart-regime-identity**, implementation commit 1182b45479881557b2f75d0aee5230cba8970753.

Implementation:
- MarketRegimeIdentity is the canonical regime identity/normalization owner;
- MarketRegimeClassifier emits only TREND, EXPANSION, RANGE, TRANSITION, HIGH_VOLATILITY, COMPRESSION or UNKNOWN;
- FrameRegimeResolutionRule delegates normalization to the canonical identity owner;
- SmartThresholdPolicyRule owns the existing adaptive threshold adjustments;
- the unreachable REVERSAL branch is removed without threshold retuning;
- deterministic runtime coverage verifies every canonical regime, UNKNOWN/future safety, legacy numeric adjustments, disabled adaptation and direction-neutral symmetry;
- audit_phase_6_4.py is accumulated in Source/Architecture CI.

Safety/manual boundary:
- no public parameter name/type/DefaultValue, threshold, RR, confidence or execution policy changed;
- no second decision/execution authority introduced;
- target-terminal timing/presentation, restart/reconnect and empirical signal-quality/profitability remain manual.

Next phase: **CR6.5 / F6 — Trap-risk/trigger exceptions and actionability constant ownership.**
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

**Next phase: CR6.7 / F8 — Target-obstacle rejection telemetry and distant-target semantics.**


Prompt 4, Prompt 5 and Prompt 6 are mandatory remediation tracks before
CR-FINAL. **Current: CR6.7 / F8 — Target-obstacle rejection telemetry and distant-target semantics.**

CR5.1 through CR5.8 are verified complete. CR5.8 / E8 was merged to main via
PR #124, merge commit `03a569d6a18f1b6cbc3524dabc24ea713439c325`. Prompt 5
E1–E8 is closed at repository level. CR6.1 / F1 and CR6.2 / F2 are verified
complete, and CR6.3 / F4 is verified complete on PR #129 with all three
repository gates PASS on commit
`5dd9a8a8bb0fb8172ac40b7336ce86e0bb5c3c2a`. CR-FINAL remains paused until
CR6.1–CR6.9 are completed or explicitly documented as verified/deferred with
evidence. Target-terminal acceptance remains required afterward.

### CR5.7 / E7 closeout

CR5.7 / E7 is **VERIFIED COMPLETE on PR #123**; implementation head
`8d39ad3fb88c75092908121a3cbaa65128f47659`, merged to `main` via commit
`d37f6d575595bfacecdff5a5ffb8fc44ba96455a`.

Implementation record:
- WATCH/REACTION alert qualification and emission are decision-owned rather than renderer-owned;
- Core `WatchReactionAlertRule` is the sole owner of early-WATCH thresholds,
  eligibility semantics and deterministic alert identities;
- the existing confidence floor 60 and allowance 4 are preserved;
- live REACTION cadence remains intact after `UpdateLiveReaction`;
- chart rendering remains presentation-only for these alerts;
- deterministic Runtime Contract coverage and the E7 static audit are wired.

Repository verification at merge:
- Source/Architecture PASS — run `36841785497`;
- Runtime Acceptance Contracts PASS — run `36841785708`;
- cTrader Compile PASS — run `36841785507`.

Safety/manual boundary:
- no public parameter name/type/DefaultValue or RR/confidence/stop/target threshold changed;
- no decision or execution authority duplication introduced;
- target-terminal alert timing, popup/audio delivery, panel/chart behavior,
  broker lifecycle and empirical signal-quality validation remain manual.

### CR5.8 / E8 closeout

CR5.8 / E8 is verified complete. Final implementation head was
`51e1f2bc9ecdd12bc8a366630fb225a4fa2c5593`.

Verification:
- Source / Architecture: PASS — run `36844545898` / workflow #2160.
- Runtime Acceptance Contracts: PASS — run `36844545976` / workflow #1969.
- cTrader Compile: PASS — run `36844546002` / workflow #2153.

Completed hardening included canonical required-RR ownership, monotonic TP-stage
ordering, explicit lane propagation, internal constant ownership, and
reconciliation of accumulated Phase 11.4 and E6 continuity audits.

### Next transition

The next implementation response must execute **CR6.5 / F6 — Trap-risk/trigger
exceptions and actionability constant ownership** only. Track 12A and CR-FINAL
remain blocked until the full Prompt 6 chain is closed.


## CR7.1 / G1 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #138 merged to main as `b8144c2f1edc62730b7a0723be3746afe6353851`.**

تأیید می‌کنم — existing broker-stop health is now distinct from new-stop market-distance acceptability, and healthy broker SL progression is protective-only.

Verification:
- Source/Architecture PASS — run #2262;
- Runtime Acceptance Contracts PASS — run #2071;
- cTrader Compile PASS — run #2255.

The Source/Architecture failure discovered during G1 was an audit false-positive on independent `GetHashCode()` overrides; the duplicate-method audit now keys by containing type.

**Historical transition after G1: CR7.2 / G2 — Retest adverse-momentum semantics and rejection telemetry.**


## CR7.2 / G2 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #142 merged to main as `f983d2fd7eb0baced4b5ff40988e6294b3f5bd28`.**

Verification:
- Source/Architecture PASS — #2283;
- Runtime Acceptance Contracts PASS — #2092;
- cTrader Compile PASS — #2276.

## CR7.3 / G3 closeout — 2026-10-01

Status: **VERIFIED COMPLETE — PR #143 merged to `main` as `6c572643cfc6b9a4ee1083e300ec13607dd2774c`.**

Completed:
- valid `Level Line Thickness` values 1/2/3 now map to actual thickness 1/2/3;
- the former forced-one clamp was removed from the renderer;
- thickness mapping is owned by `PlanLinePresentationRule`;
- Solid line style remains unchanged;
- deterministic runtime and static acceptance coverage added.


Verification:
- Source/Architecture: PASS — #2295;
- Runtime Acceptance Contracts: PASS — #2104;
- cTrader Compile: PASS — #2288.

## CR7.4 / G4 — panel execution/protection state semantics

Status: **IMPLEMENTATION COMPLETE — repository verification pending.**

The panel now has one Core state owner for Auto Trade, Auto Orders and broker-protection presentation. Operational state is derived from runtime/lifecycle/broker facts rather than decision/reaction readiness. Deterministic G4 runtime contracts and a phase-specific static audit are wired.

Manual cTrader validation remains required for live panel transitions, broker SL/TP synchronization, TP-ladder observation and restart/reconnect timing.

**Current phase: CR7.4 / G4 — Panel execution/protection state semantics.**
