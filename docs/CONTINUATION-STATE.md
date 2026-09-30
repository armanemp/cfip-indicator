# CFIP — Cross-Chat Continuation State

Last updated: 2026-09-30

## Phase closeout

CR2.4 is verified complete and merged to main. PR #89 merge commit: ac8c526f7ed0887ba990dc9a091c8299e96a6de5. Source/Architecture, Runtime Acceptance and cTrader Compile all passed.

## Authoritative order

1. Claude review remediation: `docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md`
2. Master roadmap: `docs/ROADMAP.md`
3. Only after `CR-FINAL`: local cBot separation Track 12A.

## Active phase

**CR2.5 — Lifecycle ordering and outcome aggregation (Prompt 2 / B6 + shared C8).**

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

CR2.4 is verified and merged (PR #89, merge commit ac8c526f7ed0887ba990dc9a091c8299e96a6de5). The next implementation response must execute **CR2.5** and only CR2.5. Track 12A remains blocked until CR-FINAL.
