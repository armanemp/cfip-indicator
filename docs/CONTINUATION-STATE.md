# CFIP — Cross-Chat Continuation State

Last updated: 2026-09-30

## Phase closeout

CR3.2 is verified complete and merged to main via PR #98, merge commit `af7eb503dd661d508afcc7619c72f74cea1b3a0f`. The final implementation revision `c787c88538e82fbe795f09b7632061575ff023db` passed Source/Architecture (run 1820), Runtime Acceptance (run 1629) and cTrader Compile (run 1813).

CR3.1 remains part of the completed chain: PR #95, merge commit `f482d2f76cdf37cca87fabc5b11b3d8c0a7edac7`, with Source/Architecture run 1808, Runtime Acceptance run 1617 and cTrader Compile run 1801.

## Authoritative order

1. Claude review remediation: docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md
2. Master roadmap: docs/ROADMAP.md
3. Only after CR-FINAL: local cBot separation Track 12A.

## Active phase

CR-FINAL repository integration is PASS. Remaining active gate: **target-terminal acceptance** only.

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

CR3.5 is closed and repository-side CR-FINAL is PASS. The next required work is target-terminal CR-FINAL acceptance; Track 12A remains blocked until that evidence is accepted.


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
