# CFIP — Cross-Chat Continuation State

Last updated: 2026-09-30

## Phase closeout

CR3.1 is verified complete and merged to main via PR #95, merge commit f482d2f76cdf37cca87fabc5b11b3d8c0a7edac7. The final implementation revision f9ab0aeb6802c78f4bb97adbde140fff1ed1967e passed Source/Architecture (run 1808), Runtime Acceptance (run 1617) and cTrader Compile (run 1801).

## Authoritative order

1. Claude review remediation: docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md
2. Master roadmap: docs/ROADMAP.md
3. Only after CR-FINAL: local cBot separation Track 12A.

## Active phase

Next: CR3.2 — Decision gate and early prediction semantics (Prompt 3 / C3-C4).

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

CR3.1 is closed. The next implementation response must execute CR3.2 only. Track 12A remains blocked until CR-FINAL.
