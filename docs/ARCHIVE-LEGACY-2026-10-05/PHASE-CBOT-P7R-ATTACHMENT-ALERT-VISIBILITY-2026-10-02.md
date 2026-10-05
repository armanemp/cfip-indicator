# CBOT-P7R — Attachment Truth / Alert Visibility / Parallel Scenario Presentation — 2026-10-02

## Status

**Implementation complete on branch `phase/cbot-p7r-attachment-alert-visibility-2026-10-02`; repository verification pending.**

## Why this phase was required

The target terminal was able to report **CBOT NOT ATTACHED** even when the cBot was present/running on the same chart, because connection/binding discovery depended primarily on the mutable chart-instance `Name`. The Indicator alert path also treated broker reconciliation as a prerequisite for analysis alerts, which could suppress alerts while the cBot was absent, reconnecting, or waiting for a heartbeat.

The parallel-scenario subsystem already supported multiple opportunities structurally, but its chart labels had no stable visible ordinal and the directional WATCH arrow could remain pinned to the last closed M5 bar or disappear when `EntryAllowed` was false.

## Canonical corrections

### 1. cBot attachment truth

Both sides of the same-chart boundary now accept the stable cTrader object type name in addition to the visible instance name:

- Indicator cBot reader: `ChartRobot.Name` OR `ChartRobot.Type.Name` matches `CbotIdentity.DisplayName`.
- cBot Indicator binding: `ChartIndicator.Name` OR `ChartIndicator.Type.Name` matches `CFIP Smart Indicator`.
- Existing exact-instance `InstanceId` and heartbeat checks remain in force.
- Multiple matches remain fail-closed as ambiguous.

This keeps a renamed chart instance from becoming a false "not attached" state without weakening instance identity/liveness requirements.

### 2. Alert delivery no longer waits for broker reconciliation

Analysis/presentation alerts are not broker mutations.

`ProcessDecisionAlerts()` now requires only an available canonical decision and includes the canonical parallel-opportunity alert stage. The old same-cycle broker-reconciliation prerequisite was removed from the analysis alert owner.

Actionability, risk, execution-capability and broker confirmation remain separate gates for actual execution. This is intentional: a missing/stale cBot can no longer silence an Indicator signal that is otherwise valid to display.

### 3. Multiple simultaneous opportunity announcements

The existing `TradePlanRegistry` / parallel-scenario architecture is retained and now feeds explicit scenario alerts.

For each visible actionable scenario, or qualified primary M15/H1 watch scenario:

- a stable `ScenarioId` is included in the alert identity;
- a deterministic `#N` ordinal is emitted;
- timeframe, direction, quality, Entry/SL/TP1 when available, RR and stage are included;
- actionable and watch events use distinct alert keys;
- existing alert cooldown/duplicate protection remains active.

This is **multi-opportunity detection/presentation**, not yet multi-position broker capacity.

### 4. Direction arrow follows live direction

The current-state direction arrow is now anchored to the current chart bar instead of the mapped last closed M5 bar.

A valid BUY/SELL direction remains visible while the signal state is non-actionable, subject to the existing display settings and confidence floor. The previous direct dependency on `DecisionEntryAllowed` was removed from the directional WATCH-arrow gate.

When no canonical direction exists, the renderer removes the arrow.

### 5. Numbered scenario labels

Parallel opportunity chart labels now accept the same deterministic display ordinal used by the opportunity render loop:

`#1`, `#2`, ... for the currently selected visible scenarios.

The existing compact line geometry remains unchanged: solid, finite, default 40-candle span.

## Important execution boundary

The cBot currently has working market-execution, pending-stop, pending-limit and broker-management owners, but the certified execution capacity is still **single-plan**. The current reconciliation layer intentionally blocks ambiguous multiple managed objects and simultaneous position+pending state.

Therefore this phase does **not** claim that multiple broker positions and pending orders can safely coexist. Implementing that requires a dedicated multi-scenario execution contract, per-scenario capacity/risk accounting, per-object reconciliation, idempotency and management/protection ownership.

## Trailing / profit expansion boundary

The existing canonical `IntelligentProtectionRule` / protection-progression path remains the source for monotonic SL tightening and progressive target handling, with the cBot as the broker mutation owner. This phase does not duplicate or replace that policy.

The next execution-focused work must verify, on the target terminal, that trailing only moves protection forward toward additional profit and does not regress already-protected state.

## Full-chain audit performed

The phase re-checks:

`Pre-analysis → M15 decision → M5 trigger/tuning → M1 optional confirmation → entry geometry → signal/alert → ScenarioId/PlanId contract → cBot attachment/liveness → cBot safety → broker → lifecycle/protection → chart/panel`.

Canonical timeframe rule remains:

- **M15 = trade-decision / execution reference**
- **M5 = trigger, entry tuning and entry precision**
- **M1 = optional confirmation**
- **H1+ = context/reward support**
- **host Chart TF = presentation only**

## Verification to close the phase

- Source / Architecture: pending
- Runtime Acceptance: pending
- cTrader Compile/Build: pending
- P7R dedicated audit: pending
- target-terminal cBot attach/rename/start/stop/reconnect verification: manual
- live alert sound/popup/chart synchronization: manual
- empirical signal-frequency / signal-quality validation: manual

## Operator action after merge

Run:

`git pull --ff-only`

on local `main`.
