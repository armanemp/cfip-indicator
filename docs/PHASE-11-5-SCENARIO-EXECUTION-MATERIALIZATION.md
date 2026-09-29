# Phase 11.5 — Scenario-Aware Execution Materialization & Submission Isolation

Date: 2026-09-30

Status: VERIFIED and MERGED to main.

Merge commit: `fd29b955c557df680e40d8fe4151955600160408`.
Final code commit verified before merge: `cef178da5f0359ef2c0800376b5b9beddf295424`.

## Scope

This phase turns the existing multi-scenario opportunity registry into an explicit, deterministic execution-selection layer without creating a second decision engine or silently changing the certified single-position broker capacity.

The objective is:

- keep multiple independent scenarios visible and measurable;
- identify which scenario, if any, corresponds to the canonical executable plan;
- keep independent timeframe scenarios observe-only until a separately certified per-timeframe broker policy exists;
- isolate automatic-submission retry/backoff/circuit state by scenario identity;
- expose the selected execution scenario in runtime diagnostics;
- preserve the existing market/aggressive/pending safety and broker-confirmation owners.

## Why this phase is necessary

Phase 11.4 strengthened reward/risk quality and scenario identity, but scenario objects were still primarily display/opportunity information. The broker paths used the canonical plan or reaction directly and their submission identity did not carry first-class scenario provenance.

A multi-scenario system therefore needs an explicit materialization contract before any future phase can safely decide that more than one scenario may be executed.

## Implementation

### 1. ScenarioExecutionPolicy

Added one policy owner with two layers:

- candidate eligibility: direction, lane, decision permission, trigger and current actionability;
- execution authorization: the explicit ExecutionPolicyAllowed state must also be true.

The policy resolves a canonical plan to an exact scenario only when closed-M5 identity, lane, direction and Entry/SL/TP1 geometry agree within a bounded price tolerance.

No scenario match is itself permission to trade; all existing execution gates still run before broker mutation.

### 2. Independent timeframe boundary

M5/M15/M30/H1/H4/D1/W1 scenario candidates remain independent opportunities.

They are explicitly marked:

INDEPENDENT TIMEFRAME • OBSERVE ONLY

This prevents a strong-looking H1 or D1 candidate from becoming an untested second broker-execution authority.

### 3. Scenario-scoped submission identity

SubmissionAttemptIdentity now carries ScenarioId and includes it in its canonical retry key.

Automatic market, aggressive market, pending Stop and pending Limit paths resolve and bind an execution scenario identity before entering the shared SubmissionGate.

This means retry/backoff/circuit state is scoped to the actual scenario/path instead of being implicitly shared by unrelated scenario identities.

### 4. Execution telemetry

Submission telemetry now preserves scenario identity in the panel-facing reason and history telemetry. Runtime state also exposes the currently selected execution scenario.

### 5. Runtime contracts

Added deterministic contract coverage for:

- canonical scenario eligibility;
- observe-only independent timeframe scenarios;
- exact plan-to-scenario materialization;
- rejection of non-actionable scenarios;
- scenario isolation of submission identity.

### 6. Capacity boundary

The certified broker capacity remains:

- one managed position;
- one active execution authority;
- one canonical plan authority.

Phase 11.5 deliberately does not promote multi-position execution. Such a promotion requires a separate lifecycle, capacity, duplicate-prevention, reconciliation, partial-close and broker-state certification.

## Whole-chain routine review

The mandatory review covered:

Analysis -> Decision -> Signal -> Alert -> Execution -> Broker confirmation -> Protection/Lifecycle -> Outcome -> Learning

The existing ownership was preserved for:

- top-down MTF and lower-timeframe opportunities;
- structure/liquidity;
- Order Block, FVG and OB+FVG confluence;
- WaveTrend/divergence/indicator fusion;
- Entry/Ideal Entry/Trigger;
- structural SL and TP1..TP4;
- reward path and spread-aware RR;
- automatic market, aggressive and pending order gates;
- broker confirmation, lifecycle and protection;
- runtime history and telemetry;
- persistent learning/calibration;
- panel presentation.

No threshold was tuned from source inspection alone.

## Verification

Automated verification required for merge:

- Phase 11.5 source audit: PASS;
- accumulated Source/Architecture audits: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile/Build: PASS;
- phase-specific regression coverage.

Target-terminal validation remains required for:

- actual broker rejection/acceptance semantics;
- timing and quote behavior;
- visual panel rendering;
- multi-scenario display behavior;
- realized SL/TP and lifecycle outcomes;
- empirical false-signal, missed-opportunity and execution-quality measurement.

No profitability, prediction-accuracy or realized-R improvement claim is made from source/CI verification alone.

## Continuation

The next phase can use the measured runtime scenario telemetry to design a formal scenario execution policy, but it should not change the one-position capacity or allow independent timeframe broker mutation without a separate certification pass.
