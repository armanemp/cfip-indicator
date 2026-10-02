# Opportunity Discovery + cBot Truth + Panel Readability — 2026-10-02

## Status

IMPLEMENTATION COMPLETE — verification is through the repository gates for this branch.

## Objective

Concentrate the next development unit on the trade-opportunity engine itself:

analysis → opportunity discovery → entry zone → structural SL → reward path / TP ladder → actionability → signal → cBot handoff

The cBot connection state and panel readability are treated as hard support boundaries because a correct analytical opportunity is not useful when execution capability is falsely reported or the panel cannot be read.

## Changes

### 1. Reward-aware entry-zone mining

`ExecutionZoneCandidateSelector` now adds higher-timeframe structure candidates in addition to the existing M5/M15 FVG, Order Block, OB+FVG overlap and M5↔M15 overlap candidates.

Each candidate is evaluated against the same structural-stop and target pipeline used by the broader planning path. The selector therefore prefers entry zones that not only have good source quality and proximity, but also leave a credible reward path.

The canonical `ExecutionZoneSelectionRule` now supplies the bounded reward-path preference. Candidate state retains `RewardPathRR` and `StopQuality` for deterministic diagnostics.

A candidate with no usable reward path cannot become the selected execution zone. Final plan/actionability/RR/risk/regime/broker gates remain unchanged.

### 2. Important-level bias

M15 and H1 structural swing levels can now participate as entry-zone candidates when available. This broadens discovery when a clean FVG/OB is absent while preserving the M15 execution role and H1 context role.

No blind threshold reduction was introduced.

### 3. cBot connection truth

The Indicator now treats a fresh, instance-scoped and symbol-scoped cBot heartbeat as sufficient liveness evidence.

`ChartRobots` remains a useful secondary physical-state diagnostic, but its presence is no longer the sole prerequisite for a valid fresh heartbeat state. This prevents a running cBot with a valid heartbeat from being incorrectly shown as `CBOT NOT ATTACHED` and prevents the flaky chart-object lookup from blocking the read-only status surface.

Stale or invalid heartbeat state remains fail-closed.

### 4. Panel message rail

The unified alert/message rail is now left-aligned and uses a larger minimum font plus slightly taller rows so signal, entry, SL and TP messages remain readable.

## Preserved contracts

- M15 remains the canonical trade-decision/execution reference.
- M5 remains trigger / entry-tuning / entry-precision.
- M1 remains optional confirmation.
- H1+ remains higher-timeframe context and reward support.
- Structural SL remains reward-path aware.
- TP selection continues to use the broad structural / OB / FVG / liquidity / HTF / previous-period candidate pipeline.
- Final ActionableNow, RR, risk, regime, divergence, market and cBot safety gates remain intact.
- Multiple analytical scenarios remain displayable; broker single-plan capacity is not removed in this phase.
- cBot remains the only broker execution authority.

## Verification boundary

Repository validation is required for Source/Architecture, Runtime Acceptance and cTrader Compile/Build.

Target-terminal validation remains required for actual cBot attachment recognition, heartbeat/start-stop/reconnect, panel rendering and empirical opportunity frequency/quality.

The implementation does not by itself establish profitability; replay/OOS/walk-forward evidence is still required.

## Next analytical focus

The next opportunity-focused phase should use persisted `SignalEvaluationTrace` and outcome history to classify why high-quality candidates are rejected or missed, by family and gate, and then expand candidate families based on those measured bottlenecks rather than lowering thresholds globally.


### Follow-up hardening — position coverage and cBot detection

Two additional coverage gaps were closed on the same phase branch:

- Target FVG discovery no longer requires the current M5 bar to retest the opposing FVG. The target pipeline now evaluates forward opposing FVGs by quality, distance to entry and age.
- Structural-stop FVG discovery on M5 and HTF frames now evaluates unretested valid FVG candidates by quality and entry proximity before the existing structural-stop, risk-envelope and reward-path gates.
- The cBot now publishes a separate symbol-scoped presence heartbeat before Indicator binding completes. The Indicator can therefore distinguish “cBot detected but Indicator bind unresolved” from “no cBot detected”.
- The presence heartbeat never grants execution capability; exact IndicatorInstanceId + fresh instance heartbeat remains mandatory for executable cBot state.
- The strong-HTF counter-M5 opportunity lane is now reachable when the selected direction is aligned with the strong HTF anchor; its stricter quality/RR gate remains active.
