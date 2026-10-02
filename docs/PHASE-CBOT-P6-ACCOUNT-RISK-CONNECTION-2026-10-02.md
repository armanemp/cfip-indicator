# CBOT-P6 — Account / Execution Risk + Connection Truth — 2026-10-02

Status: IMPLEMENTATION COMPLETE — verification pending on branch phase/cbot-p6-account-risk-and-connection.

Canonical architecture:
- M15 = trade-decision / execution reference timeframe.
- M5 = trigger, entry-tuning and entry-precision layer.
- M1 = optional confirmation.
- H1+ = higher-timeframe context/reward support.
- Indicator = analysis/planning/presentation.
- cBot = broker execution/lifecycle/risk authority.

Connection truth:
- Indicator now checks direct same-chart cBot presence through ChartRobots.
- cBot runtime State is checked separately from heartbeat.
- Exact Indicator-instance heartbeat freshness is still required for execution capability.
- Missing/stopped/pending/stale/connected states are distinguished.

Account/execution risk:
- cBot final gate checks demo/live boundary, trading permission, symbol/direction/geometry, quote, single-plan capacity, free margin, margin level, session window, spread/plan-risk and daily-loss.
- final broker volume/margin normalization remains in the existing cBot BrokerExecutionSafety owner.
- execution configuration is read from the actual attached Indicator instance instead of duplicating public parameter sets.

Daily loss:
- cBot uses account History + Transactions + current unrealized P&L for its final account-level daily-loss block and persists the UTC-day lock by broker/user/asset scope.
- Indicator-side daily-loss logic remains for current analysis/panel continuity until later physical cutover; cBot is the final execution block.

Verification required:
- Source / Architecture
- Runtime Acceptance
- cTrader Compile/Build
- dedicated P6 audit
- accumulated audits
- target-terminal attachment/state/heartbeat and restart-reconnect validation remains manual.

Operator action after merge: git pull --ff-only
