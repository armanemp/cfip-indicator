# CBOT Management Policy Hardening — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

## Purpose

Make the cBot the canonical policy owner for broker-facing management commands. The Indicator continues to decide and publish management intent; the cBot validates the command against current execution settings and broker state before mutation.

## Implemented

- CbotIndicatorExecutionSettings now reads the live-management/protection controls required by the cBot:
  - EnableLiveExitManagement
  - EnablePartialTakeProfit
  - AutoBrokerProtection
  - AutoProtectBrokerPositions
  - SyncBrokerTakeProfit
  - ManagedActionsOnly
  - BrokerModifyCooldownMs
- CbotManagementPolicyRule centralizes command eligibility.
- PartialClose is blocked when partial TP is disabled.
- ModifyProtection and BreakEven are blocked when broker protection is disabled.
- AdvanceTarget requires both live-exit management and broker TP sync.
- FullClose and CancelPending remain available as safety lifecycle operations.
- Protection/target broker mutations are throttled with a position-scoped cooldown.
- Indicator management commands remain immutable intent messages over Device LocalStorage; broker mutation remains cBot-only.
- Deterministic behavioral coverage and a dedicated Source/Architecture audit were added.

## Safety

- live accounts remain fail-closed;
- cBot remains the sole broker mutation owner;
- M15 remains canonical trade-decision/execution reference;
- M5 remains trigger/tuning/entry precision;
- M1 remains optional confirmation;
- existing signal-quality, RR, risk, margin, spread, daily-loss and concurrent-scenario gates remain unchanged;
- startup protection recovery remains fail-closed and can repair missing broker protection.

## Full-chain audit

Pre-analysis -> M15 decision -> M5 trigger/tuning -> M1 optional confirmation -> Entry/SL/TP/RR -> Actionability -> Scenario/Plan -> Signal/Alert -> ScenarioBatch -> cBot preflight -> per-ScenarioId truth -> management policy -> broker mutation -> broker confirmation -> protection -> management -> outcome/history/panel.

## Verification

Automated:
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- dedicated management-policy audit;
- behavioral cBot tests.

Target terminal:
- protection enable/disable behavior;
- partial TP enable/disable behavior;
- target advance enable/disable behavior;
- cooldown behavior under repeated management intents;
- emergency full close / pending cancel behavior;
- panel state after confirmed broker mutations.

Operator action after merge: git pull --ff-only on local main.