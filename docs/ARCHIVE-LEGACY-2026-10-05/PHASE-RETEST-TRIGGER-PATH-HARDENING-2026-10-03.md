# Retest Trigger-Path Hardening — 2026-10-03

Status: IMPLEMENTATION COMPLETE — automated verification pending.

## Root cause

A valid in-zone RetestMarket opportunity was allowed by the canonical actionability path, but two downstream owners still treated Decision.TriggerReady as a global prerequisite:

- Trading/Validation/PlanCreationEligibility.cs
- Core/Math/ScenarioExecutionPolicyRule.cs

This contradicts the existing mode-specific trigger contract in Planning/Execution/TriggerGate.cs, where RetestMarket is zone-driven and breakout/predictive pending modes remain trigger-dependent.

The resulting failure path was:

M15 decision valid -> M5 zone valid -> RetestMarket geometry valid -> live actionability valid -> global TriggerReady veto -> no Plan / no executable Scenario -> no position proposal

## Corrected architecture

Core/Math/EntryActionabilityPolicy.RequiresConfirmedTrigger(...) is now the single owner of the semantic:

- RetestMarket: trigger is not a prerequisite when M5OnlyConfirmedTrigger=true.
- BreakoutMarket: trigger remains required.
- ContinuationStop / ReversalLimit: remain trigger-dependent.
- when M5OnlyConfirmedTrigger=false, the trigger prerequisite is disabled for all modes.

Plan creation, scenario authorization and the trigger gate all consume this same owner.

## Signal-quality safety

No public confidence, smart-quality, MTF, evidence, structure, entry-quality, RR or risk threshold was lowered.

Retest eligibility still passes through current-quote actionability, M15 canonical execution direction, entry location/timing/position quality, RR and stop-risk geometry, trap-risk/divergence controls, indicator-fusion quality, and lifecycle/session/spread/news constraints.

This change restores a missing valid path; it does not intentionally turn weak setups into trades.

## Full-chain audit

Re-audit scope:
Pre-analysis -> M15 decision -> M5 tuning/zone -> M1 optional -> Entry/SL/TP/RR -> Actionability -> Scenario/Plan -> Alert -> Indicator/cBot contract -> cBot preflight -> Broker execution -> Protection -> Outcome/history

## Verification

Required:
- Retest trigger-path static audit
- accumulated Source/Architecture
- Runtime Acceptance
- cTrader Compile
- target cTrader validation that a valid in-zone Retest can produce a position proposal while Breakout/Pending retain trigger requirements.

## Operator action

After the verified merge, run: git pull --ff-only

No profitability claim is made by this phase; empirical signal/outcome validation remains a separate requirement.
