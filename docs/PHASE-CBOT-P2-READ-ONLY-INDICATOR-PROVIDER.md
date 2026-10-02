# CBOT-P2 — Read-Only Indicator Provider

Date: 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending.**

## Goal

Create the first real local Indicator → cBot handoff without moving broker authority yet.

The Indicator remains the analysis/decision/plan owner. The cBot receives a structured, immutable snapshot only.

## cTrader integration rule

The current target mechanism is cTrader's supported custom-indicator reference model:

- cBot references the compiled custom Indicator.
- cBot creates it with `Indicators.GetIndicator<CFIPIndicator>()`.
- The provider exposes a read-only public contract surface.
- An invisible `IndicatorDataSeries` heartbeat is consumed before the provider property so lazy evaluation cannot leave the cBot reading an uncalculated instance.

This follows cTrader's documented custom-indicator consumption pattern (`Indicators.GetIndicator<T>()`) and the documented/observed lazy-evaluation behavior around public custom-indicator properties. See cTrader Algo: `https://help.ctrader.com/ctrader-algo/how-tos/indicators/use-custom-indicators-in-cbots/`.

## Implemented

### Indicator

Added a focused partial-owner set so the provider remains consistent with the repository's production-module size/ownership audit:

- `CFIPReadOnlyProvider.cs` — state/output surface
- `CFIPReadOnlyProviderRefresh.cs` — snapshot refresh/fingerprint
- `CFIPReadOnlyProviderPlan.cs` — plan/intent projection
- `CFIPReadOnlyProviderIdentity.cs` — identity/action/lifecycle mapping

Added:

`src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProvider.cs`

The provider exposes:

- `LatestSignalEnvelope`
- `ProviderRevision`
- `ProviderReady`
- `ProviderState`
- `ProviderUpdatedUtc`
- invisible `CFIP Provider Heartbeat` output

The snapshot is built from current Indicator-owned decision/plan/execution intent state and converted to the canonical `CFIP.Contracts` records.

Identity preserves:

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

### Transitional intent capture

The existing Indicator-side `BuildExecutionIntent` remains the source of execution intent data for now.

P2 captures the successfully validated internal intent into the provider boundary. It does not call or expose any broker API.

This is deliberately transitional. P4+ will move the real execution owner after parity gates.

### cBot

`src/CFIP.cBot/CFIPExecutionBot.cs` now:

- references `CFIPIndicator`;
- initializes it through `Indicators.GetIndicator<CFIPIndicator>()`;
- explicitly disables Auto Trading, Automatic Orders, Aggressive Auto Entry, Broker Position Protection and Live Exit Management for the P2 instance;
- consumes `ProviderHeartbeat.LastValue` before reading the provider snapshot;
- logs provider revision, lifecycle stage, signal/scenario/plan identity, action and source timeframe;
- contains zero broker mutation calls.

The cBot is therefore a read-only/shadow consumer in P2.

### Build dependencies

`CFIP.Indicator` references `CFIP.Contracts`.

`CFIP.cBot` references both `CFIP.Contracts` and `CFIP.Indicator`.

This is the source/build representation of the supported cTrader custom-indicator reference; the cBot is not allowed to access Indicator private state.

## Lifecycle placement

The provider refresh runs after the existing presentation stage so the cBot observes the latest state after the existing calculation/execution pipeline has settled.

The startup calculation seed also refreshes the provider.

The normal `Calculate` lifecycle updates the heartbeat on every invocation.

## Important non-changes

P2 does not:

- move broker mutation;
- create a second executor;
- create a second decision engine;
- change analytical thresholds;
- move OB/FVG/WaveTrend/divergence/MTF logic;
- change chart/panel authority;
- introduce HTTP/IPC/cloud transport;
- activate broker execution in the cBot.

The existing Indicator execution authority remains temporarily intact until P4/P5/P6 replacement, deterministic parity and deletion gates are passed.

## Boundary audit

Added:

`tools/audit_cbot_provider_boundary.py`

The audit verifies:

- Indicator → Contracts reference exists;
- cBot → Indicator/Contracts references exist;
- canonical read-only provider surface exists;
- invisible heartbeat exists;
- canonical identity/revision is preserved;
- all four execution intent kinds are represented;
- provider contains no broker mutation;
- provider publication happens after the existing calculation/presentation flow;
- cBot uses `GetIndicator`;
- cBot consumes the heartbeat before the snapshot;
- cBot contains no broker mutation;
- execution-sensitive Indicator instance parameters are explicitly disarmed;
- no reflection, chart scraping, filesystem or network transport is used.

## Verification

Required gates:

- Source / Architecture
- Runtime Acceptance
- cTrader Compile

P2 is accepted only after all three gates are green on the final phase SHA.

## Next phase

**CBOT-P3 — cBot Host / Shadow**

P3 will add deterministic receive → validate → expiry/revision → deduplicate → broker-safety → shadow/telemetry state, still with broker mutation disarmed.
