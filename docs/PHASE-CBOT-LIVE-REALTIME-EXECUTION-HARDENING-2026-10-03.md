# cBot Live / Realtime Execution Hardening — 2026-10-03

Status: IMPLEMENTED — automated verification pending; target-terminal validation pending.

## Objective

Close the remaining seams between high-frequency Indicator opportunity publication and cBot broker execution while keeping the Indicator analysis-only boundary.

## Implemented

- The cBot remains attached and running on live accounts when Enable Live Execution is false; broker mutation is blocked and the execution state is published instead of stopping before the chart binding is established.
- The cBot reloads the shared Device LocalStorage signal bus on a bounded 100 ms cadence, aligned with the Indicator's existing 200 ms intrabar opportunity refresh.
- cBot market/pending execution modes are the broker-execution authority. Indicator Enable Auto Trading / Enable Automatic Orders no longer silently veto an explicitly armed cBot.
- Effective execution state published back to the Indicator follows cBot mode, lifecycle and recovery state, preventing misleading Indicator-setting OFF messages.
- CFIP chart binding scans the Custom indicator collection first, keeps aggregate compatibility, matches stable display/type identities case-insensitively, and reports the actual chart candidates when binding fails.
- Present and future opportunity candidates use one adaptive reward floor: base max(MinimumTpSpacingATR, 0.75 × MinimumSLATR), raised to 0.85 ATR in RANGE, 1.00 ATR in COMPRESSION and 0.75 ATR in TRANSITION.
- The adaptive reward floor is rechecked at the final ScenarioExecutionPolicyRule boundary so current and future execution cannot bypass it.
- Alert sound delivery is constrained to the indicator realtime last-bar path, matching the cTrader notification API guidance for indicators, while preserving the existing bounded sound-bearing queue.

## Present / future / history contract

Past/outcome memory -> pre-analysis -> M15 canonical decision -> M5 trigger/tuning -> optional M1 confirmation -> current-quote actionability -> current Market/Aggressive execution OR future Stop/Limit order -> broker confirmation -> protection/management -> outcome archive/calibration.

The existing prediction/forecast and empirical calibration subsystems remain evidence layers; they do not become a second execution authority.

## Safety

- Live execution is explicit and OFF by default.
- A live cBot with the arm OFF performs no broker mutation, including management mutation.
- Concurrent scenarios remain bounded by the cBot Max Concurrent Scenarios cap and broker-confirmed reconciliation.
- Same ScenarioId idempotency remains mandatory.
- No confidence/RR/risk gate was lowered.
- Stagnant-market improvement is achieved by rejecting undersized reward excursions rather than by creating extra trades.

## Verification

Required:
- Source/Architecture CI and the dedicated hardening audit;
- cTrader Compile/Build;
- live-account arm OFF attachment/state visibility;
- demo and explicitly armed live market/pending execution;
- simultaneous distinct ScenarioIds;
- future pending placement and invalidation;
- 100–200 ms signal publication/handoff observation;
- eligible-signal audio delivery;
- one/multiple/missing CFIP indicator attachment diagnostics;
- restart/reconnect/idempotency and broker protection;
- target-terminal realized quality review.

Operator action after verified merge: git pull --ff-only on local main.
