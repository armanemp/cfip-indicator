# CFIP Indicator — Development Workflow

## Before every phase
Read ROADMAP, ARCHITECTURE, the current phase notes, previous acceptance notes, relevant v89 source sections and tests.

Produce a behavior inventory, dependency map, risk list and acceptance gates.

## Audit
For every behavior identify implementation, authoritative owner, inputs, outputs, side effects, broker interaction, UI interaction, failure modes, BUY/SELL symmetry, tests and migration destination.

## Implement
Create the target abstraction first. Move behavior behind it. Preserve behavior unless a defect is explicitly corrected. Remove old paths only after parity is demonstrated.

Never solve a shared-contract defect with a local patch. Analysis never calls broker APIs. UI never becomes trading authority. Broker acceptance is never treated as broker state.

## Validate
Use the strongest applicable gate: C# build, static checks, unit tests, architecture tests, scenario/replay tests, real cTrader compile and controlled broker runtime tests.

Source-level success is never reported as real cTrader runtime success.

## Commit discipline
Use focused commits:
chore: tooling/repository
docs: architecture/process
refactor: structural migration
feat: new behavior
fix: defect
test: validation
perf: performance
release: release preparation

## Runtime safety
Automated trading validation must use a controlled environment/account. Minimum scenarios include market entry, aggressive entry, pending stop/limit, rejection, slippage/fill reconciliation, missing protection, partial close, close confirmation, disconnect/reconnect, restart adoption, duplicate events, daily-loss circuit breaker, reversal, exhaustion, invalidation, EOD and multi-position pending fill.

## Completion record
Every phase records status, version, changed files, architectural decisions, defects, fixes, tests, runtime validation, known limitations, commit SHA and next phase.

## Progress
Every development update includes a compact phase progress chart.
