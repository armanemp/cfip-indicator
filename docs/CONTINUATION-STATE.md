# CFIP — Cross-Chat Continuation State

Last updated: 2026-09-30

## Phase closeout

CR2.7 is verified complete and merged to main via PR #92, merge commit b021f56c4b75331fb52127547e006f2f8bb287c4. Source/Architecture, Runtime Acceptance and cTrader Compile all passed on head 181b238a548280fc01a67fb1e3ba8a617f63e42a.

## Authoritative order

1. Claude review remediation: `docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md`
2. Master roadmap: `docs/ROADMAP.md`
3. Only after `CR-FINAL`: local cBot separation Track 12A.

## Active phase

**Next: CR2.8 — Historical rendering semantics and cost (Prompt 2 / B9).**

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

## Rules for every continuation

- Complete exactly one CR phase per implementation response.
- Perform the project-wide routine audit: Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning.
- Run the performance/code-cleanliness audit in the same phase.
- Keep public parameter name/type/DefaultValue unchanged unless a new safety parameter is explicitly required.
- Every fix gets its own `fix(<ID>): ...` commit.
- Every logical change gets real deterministic behavior tests.
- Do not start or advance cBot separation until `CR-FINAL` passes.
- Never claim target-terminal verification from CI alone.

## Next transition

CR2.7 is now verified and merged. The next implementation response must execute **CR2.8** and only CR2.8. Track 12A remains blocked until CR-FINAL.
