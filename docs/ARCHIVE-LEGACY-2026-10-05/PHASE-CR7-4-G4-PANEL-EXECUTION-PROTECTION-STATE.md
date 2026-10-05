# CR7.4 / G4 — Panel execution/protection state semantics

Date: 2026-10-01

Status: **VERIFIED COMPLETE — PR #144; final code HEAD verified before documentation closeout.**

## Finding

The panel had two semantic leaks:

1. Auto Trade color/state could become green/ready because a decision, reaction, or plan looked executable, even when the runtime execution state was not ready.
2. Auto Protection primarily reported configuration scope (NEW TRADES / MANAGED POSITIONS) instead of the broker-confirmed protection state of the managed position.

That made presentation capable of suggesting operational readiness from analysis evidence rather than the canonical runtime/broker state.

## Implementation

Added Core/Math/ExecutionProtectionPanelStateRule.cs as the platform-neutral state owner.

Execution states:
- Disabled;
- Armed;
- Ready;
- Active;
- Blocked;
- RecoveryRequired.

Protection states:
- Off;
- NoLivePosition;
- Protected;
- RecoveryRequired.

Updated UI/Panel/PanelExecutionState.cs so:
- Auto Trade state uses runtime enablement, live-position presence, explicit submission readiness/block state, lifecycle/recovery state;
- Auto Orders state uses runtime enablement, managed pending-order presence, explicit placement readiness/block state, lifecycle/recovery state;
- Broker Protection uses the managed broker position, directionally-valid existing SL, target validity, server-side TP-ladder ownership and recovery state;
- Auto Trade presentation no longer reads decision/reaction readiness to decide operational color/state.

Updated panel rows so the overview and Auto Trading sections consume the same canonical execution/protection state helpers.

Updated panel presentation-key invalidation so changes to execution/protection state cannot leave a stale full-panel presentation cached.

## Deterministic runtime contracts

VerifyExecutionProtectionPanelStateG4() covers:
- disabled/armed/ready/blocked/active execution states;
- recovery overriding active state;
- pending-order active state;
- unconfigured protection;
- configured protection with no live position;
- valid broker SL/TP;
- target-not-required configuration;
- server-owned TP ladder;
- missing required TP;
- invalid broker SL;
- explicit recovery state.

## Safety boundary

No public parameter name/type/DefaultValue was changed.
No confidence, RR, entry, SL, TP, risk, trap, or execution threshold was retuned.
No broker mutation path was added.
No second decision or execution authority was introduced.
The change is a presentation/semantic ownership hardening over existing runtime and broker facts.

## Verification

- Source/Architecture: PASS — run #2313.
- Runtime Acceptance Contracts: PASS — run #2122.
- cTrader Compile: PASS — run #2306.
- Verified code HEAD: 2465444593afca6b566b17be2234336154fd6f2e.

## Manual cTrader boundary

Hands-on target-terminal validation remains required for:
- visible panel state transitions during startup/recovery/reconnect;
- live broker SL/TP synchronization timing;
- server-side TP ladder observation;
- actual cTrader panel rendering/responsiveness.

Empirical trading/signal-quality conclusions are outside this phase.
