# CFIP — CR2.3 Unified Indicator-Quality Thresholds

## Status

IMPLEMENTED — CI acceptance pending.

## Scope

CR2.3 covers B4 plus A9 from the Claude review-remediation program:

- establish one semantic owner for IndicatorConfluenceQuality and IndicatorConflict execution gates;
- preserve current production threshold values initially;
- make path-specific policy differences explicit rather than distributing magic values through callers.

## Canonical owner

`src/CFIP.Indicator/Core/Math/IndicatorExecutionQualityRule.cs` is now the single owner for execution-stage indicator quality thresholds.

The caller supplies a semantic stage:

- AutomaticMarket
- PendingSubmission
- PendingContinuation
- PendingReversal

The rule returns both the pass/fail result and the canonical diagnostic reason.

## Preserved thresholds

| Stage | Minimum indicator quality | Maximum indicator conflict | Meaning |
| --- | ---: | ---: | --- |
| Automatic Market | 60 | 52 | final market-entry safety gate after decision/actionability checks |
| Pending Submission | 58 | 55 | defense-in-depth mutation gate after setup qualification |
| Pending Continuation | 62 | 48 | candidate qualification for continuation Stop |
| Pending Reversal | 62 | 50 | candidate qualification for reversal Limit |

The 62 quality floor is shared by the two pending setup types. The 58 submission floor is intentionally lower because it is a second, defense-in-depth check rather than the candidate-selection rule.

Continuation uses a tighter 48 conflict maximum because its semantics confirm continuation of an already aligned trend. Reversal uses 50 because its qualification is independently constrained by the CR2.2 reversal-context rule.

No public parameter, default, RR floor, execution authority, or trading threshold value was changed.

## Caller migration

Automatic market pre-trade, pending submission, pending continuation and pending reversal now consume the canonical stage-aware rule instead of owning separate numeric comparisons.

## Verification

Deterministic runtime contracts cover boundary values, path-specific conflict policy, stage diagnostics and unknown-stage fail-closed behavior.

A dedicated static audit verifies that legacy indicator-threshold constants were removed from `ExecutionThresholdPolicy` and that the execution callers route through the canonical owner.

Required CI gates:

- Source/Architecture
- Runtime Acceptance
- cTrader Compile

Target-terminal behavior remains a separate manual acceptance boundary.

## Routine project audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning

The change is confined to threshold ownership/semantics. No second decision authority or broker mutation path was introduced. Runtime cost is constant-time per existing execution gate.