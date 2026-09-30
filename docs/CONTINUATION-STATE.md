# CFIP — Cross-Chat Continuation State

Last updated: 2026-09-30

## Phase closeout

CR2.2 is verified complete and merged to main. PR #87 merge commit: bdb9b72d021972db4b3638ff5eb4d7078ab2cb2a. Source/Architecture, Runtime Acceptance and cTrader Compile all passed.

## Authoritative order

1. Claude review remediation: `docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md`
2. Master roadmap: `docs/ROADMAP.md`
3. Only after `CR-FINAL`: local cBot separation Track 12A.

## Active phase

**CR2.4 — Pending-order decision arbiter (Prompt 2 / B5).**

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

CR2.3 is verified and merged (PR #88, merge commit 98da2030d8e36312ee0c073c1a58889bb405f893). The next implementation response must execute **CR2.4** and only CR2.4. Track 12A remains blocked until CR-FINAL.
