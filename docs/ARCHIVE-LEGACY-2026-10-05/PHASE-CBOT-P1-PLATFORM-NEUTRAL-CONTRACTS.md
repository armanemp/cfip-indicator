# CBOT-P1 — Platform-Neutral Contracts

Date: 2026-10-02

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification:
- Source / Architecture #2778: PASS
- Runtime Acceptance #2587: PASS
- cTrader Compile #2771: PASS

Branch: `phase/cbot-p1-platform-neutral-contracts`

## Goal

Establish one shared contract boundary for Indicator ↔ cBot without introducing a second decision engine, broker abstraction layer, or cloud dependency.

## Implemented

Created `src/CFIP.Contracts` as a standalone .NET 6 project with no cTrader dependency.

Canonical contract families:

- `ContractIdentity`
- `PlanSnapshot`
- `SignalEnvelope`
- `ExecutionIntent`
- `ManagementCommand`
- `BrokerExecutionReport`
- `LifecycleEvent`
- `ContractVersion`

Canonical enums cover trade direction, opportunity lane, signal stage, execution action, management command, broker action/status and lifecycle event type.

Identity carries:

- ContractVersion
- SignalId
- ScenarioId
- PlanId
- Symbol
- Direction
- Lane
- SourceTimeframe
- CreatedUtc
- CreatedClosedM5
- ExpiryUtc
- Revision
- CorrelationId
- IdempotencyKey

## Immutability

The contract records use immutable positional record state. No public/internal setters are allowed.

The dedicated source audit `tools/audit_cbot_contract_schema.py` verifies:

- required contract files exist;
- required immutable records exist;
- no cTrader/cAlgo platform dependency exists inside Contracts;
- no mutable setter exists;
- no behavioral methods are introduced into data contracts.

## Ownership rules

Indicator:
- creates analytical evidence, decision, scenario and plan;
- may request execution or management through the contract;
- does not own broker confirmation.

cBot:
- consumes the contract;
- performs broker/account validation;
- owns broker mutation and confirmation;
- reports broker truth back through BrokerExecutionReport/LifecycleEvent.

Contracts:
- contain data and identity only;
- do not calculate signals;
- do not validate broker state;
- do not call platform APIs.

## Migration rule

Existing `cAlgo.ExecutionIntent`, `Plan` and related internal models are not copied into cBot. They remain temporary Indicator-side source models until P2/P3 maps the live Indicator state to these canonical contracts.

No execution owner is deleted in P1.

## Verification

Required gates:
- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile.

cTrader Compile additionally proves standalone Contracts and cBot project builds.

## Acceptance

P1 is accepted when all three gates are green and the contract audit confirms one shared immutable boundary with zero platform dependency.

## Next phase

**CBOT-P2 — Read-Only Indicator Provider**

Expose these contracts from the Indicator through the supported custom-indicator reference path without private-state scraping, reflection or static mutable bridges.
