# CFIP — cBot Live + Realtime Intelligence Hardening — 2026-10-03

Status: **IMPLEMENTATION COMPLETE — automated verification pending; target-terminal acceptance still required.**

## Scope

This phase turns the existing demo-only execution boundary into an explicitly armed demo/live execution boundary while preserving the Indicator → Contracts → cBot ownership split.

The intended runtime flow remains:

**history/outcomes → pre-analysis → M15 decision → M5 trigger/tuning/entry precision → optional M1 confirmation → current-quote actionability → current market execution OR future pending placement → cBot preflight → broker submission → broker-confirmed facts → protection/management → outcome/history**

## Realtime execution changes

- Live accounts are no longer intrinsically rejected. They remain **disarmed by default** and require the cBot's explicit **Enable Live Execution** arm.
- The same broker mutation owners are reused for demo and live; no second execution engine was introduced.
- Provider staleness default is tightened from 15s to 3s for the cBot handoff.
- Market/Aggressive execution now has a final cBot-side **Max Market Entry Drift Pips** check against the current Bid/Ask before broker mutation.
- Maximum concurrent scenario capacity is raised to 5 by default (hard parameter ceiling remains 10).
- Maximum execution count per cBot session is raised to 10 by default (hard ceiling remains 20).

## Opportunity-quality changes

- `TradeOpportunityCandidate` now carries explicit `RewardDistanceAtr` and `MinimumRequiredRewardDistanceAtr`.
- The minimum reward excursion is derived from the existing risk/target semantics: `max(MinimumTpSpacingAtr, MinimumSlAtr * 0.75)`.
- Executable current and future scenarios below that derived reward-excursion floor are rejected at the canonical `ScenarioExecutionPolicyRule` boundary.
- Larger valid reward excursions receive a bounded ranking bonus so the selection layer naturally prefers materially useful opportunities over tiny ones.
- No existing confidence, RR, risk, spread, margin, daily-loss or structural-quality gate was weakened.

## Future-order semantics

- A scenario marked `ActionableNow` is the only scenario eligible for immediate Market/Aggressive execution.
- `FutureOrderReady` scenarios remain future plans and are routed to Pending Stop/Limit placement.
- Multiple distinct ScenarioIds may be processed from the same realtime ScenarioBatch, subject to cBot concurrency/capacity and broker safety.

## Attachment and alert hardening

- Indicator/cBot chart discovery now scans the complete chart instance collection and accepts stable type-name representations (`Type.Name`, type text, and qualified type text).
- The attachment reader retains exact InstanceId + fresh heartbeat as execution-liveness truth.
- Attachment diagnostics now expose chart-indicator count when the expected CFIP indicator is not resolved.
- Sound-bearing alerts are preserved under bounded queue pressure by evicting an older normal diagnostic event before dropping the audio-bearing event.
- The existing queued alert delivery owner remains the sole `Notifications.PlaySound` owner.

## Safety and live-arm contract

- `Enable Live Execution` defaults to **false**.
- A live account with live execution disarmed is stopped before broker mutation.
- Demo accounts can continue to run through the same mutation path.
- Broker-side margin, capacity, geometry, spread/plan-risk, protection and reconciliation gates remain mandatory.
- Missing/invalid broker protection remains fail-closed.
- Indicator remains broker-mutation-free.

## Verification required

1. Source/Architecture CI.
2. cTrader compile/build.
3. Demo realtime current-market execution.
4. Demo future pending Stop/Limit placement and fill transition.
5. Live account with `Enable Live Execution = false` remains blocked.
6. Live account with explicit arm passes only when all account/broker safety gates pass.
7. Same-tick market handoff latency and current-entry drift rejection.
8. Multiple simultaneous ScenarioIds and capacity enforcement.
9. Alert queue burst test and audible signal confirmation.
10. Same-chart indicator/cBot attachment after add/remove/restart/reorder.
11. Restart/reconnect/idempotency/protection recovery.
12. 90-day/outcome-history continuity and post-trade calibration telemetry.

## Operator action after merge

Run:

    git pull --ff-only

For live, enable **Enable Live Execution** explicitly only after the target terminal has passed the live-arm acceptance checklist.