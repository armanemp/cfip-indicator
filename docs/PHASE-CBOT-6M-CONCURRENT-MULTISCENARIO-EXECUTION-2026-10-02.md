# CBOT-6M — Concurrent Multi-Scenario Execution — 2026-10-02

Status: **IMPLEMENTATION IN PROGRESS — verification pending.**

## Purpose

This phase upgrades the Indicator → cBot handoff from one canonical execution envelope to a bounded batch of independently identified scenarios.

## Implemented in this phase

- SignalScenarioBatch contract with shared revision, Indicator instance identity, symbol and SignalEnvelope[] payload.
- Device-scoped scenario-batch publisher and cBot transport.
- Stable per-scenario broker label identity using ScenarioId.
- Candidate execution mode and requested volume are preserved into scenario materialization.
- M15 scenarios are executable when canonical decision, trigger and actionability permit; H1 remains context/observe-only.
- Scenario action materialization supports Market, Pending Stop and Pending Limit.
- cBot supports a bounded Max Concurrent Scenarios safety parameter.
- Broker capacity checks are scenario-aware while preserving same-scenario duplicate protection.
- Per-ScenarioId envelope/reconciliation caches are maintained in the cBot.
- Protection reconciliation sweep runs independently across cached active scenarios.
- Indicator remains broker-mutation-free.

## Full-chain audit

Every scenario follows the same chain:

Pre-analysis → canonical M15 decision → M5 trigger/tuning/entry precision → optional M1 confirmation → entry/SL/TP geometry → ScenarioId/PlanId → alert/message → SignalEnvelope → scenario batch → Device LocalStorage / InstanceId → cBot preflight → scenario capacity/risk → broker mutation → broker confirmation → per-scenario reconciliation/protection → outcome/history.

M15 remains the canonical trade-decision/execution reference. M5 remains trigger/tuning/entry precision. M1 remains optional confirmation. H1 remains context/reward.

## Safety constraints

- ScenarioId is part of the stable contract identity and broker label.
- Duplicate idempotency keys are rejected independently per scenario.
- An already-active scenario does not block a different scenario until the bounded overall concurrent capacity is reached.
- The global session execution cap remains in place.
- Live accounts remain blocked by the existing cBot safety boundary.
- The Indicator never places broker orders or modifies positions.
- Final removal of any remaining single-plan assumptions requires this phase to pass automated and terminal verification.

## Remaining verification

- Source/Architecture CI including the dedicated 6M audit.
- Runtime Contracts and cTrader compilation.
- Deterministic scenario-batch regression cases.
- Target-terminal coexistence test for multiple Market and Pending scenarios.
- Restart/reconnect adoption test for every active scenario.
- Protection/trailing independence test with at least two active scenarios.

## Next

After 6M verification, continue the remaining execution/lifecycle hardening without weakening the M15/M5 timeframe contract.

## Operator

After merge, run:
git pull --ff-only
