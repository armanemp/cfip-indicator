# CFIP — Cross-Chat Continuation State

Last updated: 2026-10-01

## Phase closeout

CR3.2 is verified complete and merged to main via PR #98, merge commit `af7eb503dd661d508afcc7619c72f74cea1b3a0f`. The final implementation revision `c787c88538e82fbe795f09b7632061575ff023db` passed Source/Architecture (run 1820), Runtime Acceptance (run 1629) and cTrader Compile (run 1813).

CR3.1 remains part of the completed chain: PR #95, merge commit `f482d2f76cdf37cca87fabc5b11b3d8c0a7edac7`, with Source/Architecture run 1808, Runtime Acceptance run 1617 and cTrader Compile run 1801.

## Authoritative order

1. Claude review remediation: docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md
2. Master roadmap: docs/ROADMAP.md
3. Only after CR-FINAL: local cBot separation Track 12A.

## Active phase

Prompt 4, Prompt 5 and Prompt 6 are now mandatory remediation tracks before CR-FINAL. **Next: CR4.5 — Per-timeframe regime semantics (D5).**

CR4.1 and CR4.2 are complete on main. The repository-side CR-FINAL gate remains paused until CR4.2–CR4.10, CR5.1–CR5.8 and CR6.1–CR6.9 are reconciled and completed or explicitly documented as verified/deferred. Target-terminal acceptance remains required afterward.

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

CR3.5 is closed. Prompt 4 D1–D10 is now inserted before CR-FINAL. The next implementation response must execute **CR4.1 only**. Track 12A remains blocked.


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

### Next transition

The next implementation response must execute **CR4.8 / D8 — TP1 directional defensive validation** only.
Track 12A remains blocked until CR-FINAL.

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

## Current active phase

CR4.8 / D8 — TP1 directional defensive validation.
