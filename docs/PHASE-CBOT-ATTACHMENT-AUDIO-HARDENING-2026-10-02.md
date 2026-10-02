# CBOT Attachment + Alert Audio Hardening — 2026-10-02

Status: IMPLEMENTATION COMPLETE — verification pending.

## Root causes addressed

### 1. cBot attachment identity was not actually stable

The previous implementation claimed to match stable cTrader object type names, but compared ChartRobot.Type.Name / ChartIndicator.Type.Name against the human display name. The actual code types are CFIPExecutionBot and CFIPIndicator.

The hardening introduces shared stable type identities:
- cBot display: CFIP Smart Execution Bot
- cBot type: CFIPExecutionBot
- Indicator display: CFIP Smart Indicator
- Indicator type: CFIPIndicator

Both chart discovery directions now use the correct type identity while retaining the visible instance-name match.

### 2. Two execution-panel rows could report attachment loss for a running cBot

The LocalStorage reader previously translated an empty heartbeat payload into CBOT NOT ATTACHED. That conflated physical chart attachment with runtime heartbeat freshness.

An empty payload is now reported as CBOT HEARTBEAT PENDING. Physical attachment remains determined by the chart cBot object, and exact InstanceId plus fresh heartbeat remain required before execution capability is considered live.

### 3. Alert sound delivery could fail completely on a bad custom sound path

When a custom SoundFilePath was configured but invalid, the previous delivery path caught the file-playback exception and stopped without trying the semantic SoundType.

Sound delivery is now: queued alert -> panel rail -> custom file (when configured) -> semantic sound fallback.

Each actual sound delivery is logged with the alert identity.

### 4. Alert delivery had insufficient runtime observability

The canonical alert path now logs CFIP ALERT QUEUED and CFIP ALERT SOUND DELIVERED. Queue rejection is explicitly logged.

## Preserved trading/signal contracts

No strategy threshold was lowered merely to increase signal frequency.

Canonical timeframe rule:
- M15 = trade decision / execution reference
- M5 = trigger, entry tuning and entry precision
- M1 = optional confirmation
- H1+ = context/reward support
- chart timeframe = presentation only

The single-plan broker capacity gate remains intact until the dedicated CBOT-6M multi-scenario execution contract is completed.

## Full-chain audit

The phase re-audits: Pre-analysis -> M15 decision -> M5 trigger/tuning -> M1 optional confirmation -> entry/SL/TP -> signal -> alert/message -> contract -> cBot binding -> cBot preflight -> broker execution/protection -> lifecycle -> outcome/history.

## Verification

Automated source/architecture gate: pending CI run.

cTrader compile/build: pending CI run.

Target-terminal validation remains required for cBot attachment, rename/restart/stop/reconnect, both AUTO rows, eligible signal -> panel alert, alert -> sound, and custom-file -> semantic-sound fallback.

No claim of profitability is made by this infrastructure hardening. Commercial readiness still requires deterministic replay, out-of-sample validation, forward demo validation and measured execution/outcome statistics.
