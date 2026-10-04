# CFIP Indicator — Operator and Maintenance Guide

## Scope

This guide describes the supported repository build workflow, cTrader acceptance boundary, trading-state safety model and maintenance rules.

The production target is a .NET 6 cTrader indicator. The repository preserves 513 baseline behavioral parameters and 3 optional OSS analytics parameters, for 516 total parameters.

## Repository build

The production project is src/CFIP.Indicator/CFIP.Indicator.csproj and targets net6.0.

The build requires:

- .NET SDK 6.0.428 or a compatible patch release allowed by global.json;
- the installed cTrader Automate API assembly, supplied through CFIP_CTRADER_API or the default cTrader API location;
- the pinned production Skender.Stock.Indicators 2.7.3 dependency.

The repository CI is the authoritative automated compile/static gate. Local validation should use the same source tree and target API assembly rather than introducing an alternate host or second trading engine.

## Runtime authority

The runtime has one strategy decision authority and one automatic execution authority.

The broker is authoritative after a successful mutation. A trade request, plan value or UI value is not broker state.

Live broker SL is exposed to exit-management logic only when the actual broker SL is directionally valid and satisfies the current market-distance rule. Missing or invalid broker SL does not fall back to the desired structural plan SL.

Broker TP is treated similarly for directionality when smart broker TP synchronization is enabled.

Missing or invalid protection enters explicit recovery. Protection reconciliation retries unresolved mutations and avoids repeating materially identical broker mutations.

## Automatic trading

Automatic market execution and automatic pending orders use the managed symbol/identity boundary.

The production indicator intentionally does not provide manual BUY/SELL/order-entry controls.

A rejected execution remains rejected. It is never converted into a synthetic position, pending order or fill.

Pending orders become live positions only after broker-confirmed pending-fill state.

Partial closes remain pending until broker confirmation. Break-even protection after a confirmed partial close has its own rejection/recovery path.

An automatic market or aggressive entry whose broker protection mutation fails is shown as `RECOVERY` in the auto-trading state; execution success is not presented as fully protected execution.

The current live execution state is single-plan: `Maximum Open Positions` must remain `1`. Values above `1` are blocked by the execution-capacity guard until multi-plan lifecycle state is implemented.

Aggressive fills outside the accepted execution envelope request closure. A close rejection leaves the position in explicit recovery rather than silently accepting the mismatched fill.

## Lifecycle and recovery

The lifecycle model explicitly separates:

Flat -> Signal -> PlanReady -> ExecutionReady -> PendingOrder/LivePosition -> ExitRequested/RecoveryRequired -> Closed

Same-state transitions are idempotent. One-shot position/pending lifecycle events are guarded against duplicate processing. Position/pending modification events remain repeatable because broker modifications can legitimately occur more than once.

Restart/reconnect recovery reconstructs managed live state from broker-confirmed positions and orders before new lifecycle assumptions are made.

## UI boundary

Chart, panel and popup modules render authoritative state only.

UI code must not call broker mutation APIs, own decision gates, invent a second trade plan, or replace broker-confirmed state with a presentation-only state.

Execution controls remain under the UI boundary, but broker mutations remain under the approved Trading/Execution owners.

## OSS boundary

Production OSS is limited to the pinned numerical indicator dependency and its explicit adapters.

Skender.Stock.Indicators 2.7.3 is the production package. The newer FacioQuo.Stock.Indicators 3.0.1 line is benchmark/research only because it targets newer .NET runtimes than the cTrader production target.

No OSS trading engine is permitted to become a second decision, risk or execution authority.

## Mandatory acceptance before live use

Repository CI passing is necessary but does not replace hands-on cTrader validation.

1. Indicator load and panel/chart rendering.
2. M1, M5, M15, M30, H1, H4, D1 and W1 behavior.
3. Closed-bar MTF synchronization around timeframe boundaries.
4. Automatic market entry confirmation and rejection handling.
5. Automatic stop/limit pending order creation, fill and cancellation.
6. Initial SL/TP broker protection.
7. Missing-protection recovery.
8. Break-even, profit-lock and structural trailing behavior.
9. Partial take-profit and post-partial break-even.
10. Reversal, invalidation, exhaustion and end-of-day exits.
11. Restart/reconnect adoption of managed broker state.
12. Broker slippage, rejection and order-timing behavior.
13. Runtime CPU/memory profile over a representative session.

No repository-only test can reproduce the live cTrader chart renderer, broker server timing, slippage, reconnection sequence or terminal resource profile.

## Maintenance rules

Changes must follow:

contract -> authoritative owner -> caller migration -> tests/static checks -> duplicate-path removal -> runtime validation -> documentation

Every indicator, analyzer, policy, model, enum, lifecycle handler and renderer keeps one clear file owner.

Do not add versioned production class/file names, compatibility aliases for obsolete implementations, duplicate decision authorities, direct broker mutations outside approved owners, platform dependencies into Core, chart mutations outside UI, or a second live trading engine.

The architecture verifier and CI workflows are regression gates, not substitutes for reasoning about behavioral parity.

## Current release boundary

The repository is hardened and CI-clean. The remaining validation boundary is hands-on cTrader acceptance against the actual target terminal and broker session, as documented in docs/CFIP-ROADMAP.md and docs/ACCEPTANCE-MATRIX.md.


## Local-first release policy

Local cTrader completion is the release prerequisite. Follow `docs/LOCAL-RELEASE-GATE.md` for the exact hands-on matrix and evidence requirements.

Adaptive Learning is intentionally not part of the current local production scope. The local product must first prove execution, broker confirmation, protection, lifecycle, MTF integrity, runtime stability and outcome instrumentation.

Cloud is a separate post-local milestone. Since cTrader Cloud execution is available for cBots rather than custom indicators, the Cloud path will use a cBot host around the CFIP engine while preserving the same decision, risk, execution and protection contracts.
